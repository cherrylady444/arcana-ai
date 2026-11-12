import asyncio
import time

import httpx
import unittest
from unittest.mock import patch
from fastapi import Depends, FastAPI, HTTPException
from fastapi.security import HTTPAuthorizationCredentials

from app import ai_auth

ISS = "http://localhost:8081/realms/arcana"


def pass_claims():
    return {"active": True, "exp": int(time.time()) + 90, "typ": "Bearer", "sub": "ana",
            "iss": ISS, "aud": "arcana-ai", "scope": "ai.invoke", "azp": "arcana-web"}


class TestAiGuard(unittest.TestCase):
    def test_denied_claims(self):
        for change, status in [
            ({"active": False}, 401), ({"exp": 0}, 401), ({"iss": "wrong"}, 401),
            ({"typ": "Refresh"}, 401), ({"sub": ""}, 401), ({"aud": "arcana-web"}, 403),
            ({"scope": "other"}, 403), ({"azp": "elsewhere"}, 403),
        ]:
            with self.subTest(change=change), self.assertRaises(HTTPException) as err:
                ai_auth.verify_claims(pass_claims() | change, ISS)
            self.assertEqual(err.exception.status_code, status)

    def test_introspection_uses_ai_client_and_rejects_outage(self):
        cfg = ai_auth.AiAuthSettings(_env_file=None, ai_introspection_secret="fixture-secret", ai_auth_issuer=ISS)
        real_client = httpx.AsyncClient
        for outcome in (200, 503):
            def answer(req):
                self.assertEqual(req.url.path, "/realms/arcana/protocol/openid-connect/token/introspect")
                self.assertTrue(req.headers["Authorization"].startswith("Basic "))
                self.assertIn(b"token=delegated-fixture", req.content)
                return httpx.Response(outcome, json=pass_claims())
            with patch.object(ai_auth, "settings", return_value=cfg), patch.object(
                ai_auth.httpx, "AsyncClient", side_effect=lambda **kw: real_client(
                    transport=httpx.MockTransport(answer), **kw)
            ):
                run = ai_auth.require_ai_pass(HTTPAuthorizationCredentials(scheme="Bearer", credentials="delegated-fixture"))
                if outcome == 200:
                    asyncio.run(run)
                else:
                    with self.assertRaises(HTTPException) as err:
                        asyncio.run(run)
                    self.assertEqual(err.exception.status_code, 503)

    def test_missing_pass_stops_before_business_function(self):
        demo = FastAPI()
        calls = []
        @demo.post("/work", dependencies=[Depends(ai_auth.require_ai_pass)])
        def work():
            calls.append(1)
        async def go():
            async with httpx.AsyncClient(transport=httpx.ASGITransport(app=demo), base_url="http://test") as c:
                assert (await c.post("/work")).status_code == 401
        asyncio.run(go())
        assert not calls
