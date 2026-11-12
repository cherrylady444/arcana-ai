"""All AI routes require a delegated user access token. Health has no user data."""
import time
from functools import lru_cache
from typing import Annotated
from urllib.parse import urlsplit

import httpx
from fastapi import Depends, HTTPException
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from pydantic import SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class AiAuthSettings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")
    ai_auth_issuer: str = "http://localhost:8081/realms/arcana"
    ai_introspection_secret: SecretStr | None = None


@lru_cache
def settings() -> AiAuthSettings:
    return AiAuthSettings()


def unauthorized() -> HTTPException:
    return HTTPException(401, "A valid AI access token is required.", headers={"WWW-Authenticate": "Bearer"})


def verify_claims(claims: dict, issuer: str) -> None:
    exp = claims.get("exp")
    sub = claims.get("sub")
    if (claims.get("active") is not True or claims.get("iss") != issuer
            or claims.get("typ") != "Bearer" or not isinstance(sub, str) or not sub.strip()
            or type(exp) is not int or exp <= time.time()):
        raise unauthorized()
    aud = claims.get("aud")
    if isinstance(aud, str):
        aud = [aud]
    if (not isinstance(aud, list) or "arcana-ai" not in aud
            or claims.get("azp") != "arcana-web" or not isinstance(claims.get("scope"), str)
            or "ai.invoke" not in claims["scope"].split()):
        raise HTTPException(403, "Token is not authorized for Arcana AI.")


async def require_ai_pass(
    credential: Annotated[HTTPAuthorizationCredentials | None, Depends(HTTPBearer(auto_error=False))],
) -> None:
    if credential is None:
        raise unauthorized()
    cfg = settings()
    issuer = cfg.ai_auth_issuer.rstrip("/")
    target = urlsplit(issuer)
    if (not cfg.ai_introspection_secret or not cfg.ai_introspection_secret.get_secret_value()
            or not target.hostname or target.username or target.password or target.query or target.fragment
            or not (target.scheme == "https" or (target.scheme == "http" and target.hostname in
                    {"localhost", "127.0.0.1", "::1"}))):
        raise HTTPException(503, "AI token verification is not configured.")
    try:
        async with httpx.AsyncClient(timeout=5, follow_redirects=False, trust_env=False) as client:
            reply = await client.post(
                issuer + "/protocol/openid-connect/token/introspect",
                auth=("arcana-ai", cfg.ai_introspection_secret.get_secret_value()),
                data={"token": credential.credentials, "token_type_hint": "access_token"},
            )
            reply.raise_for_status()
            claims = reply.json()
            if not isinstance(claims, dict):
                raise ValueError
    except (httpx.HTTPError, ValueError):
        raise HTTPException(503, "AI token verification unavailable.") from None
    verify_claims(claims, issuer)
