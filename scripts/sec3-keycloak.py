#!/usr/bin/env python3
"""Configure the existing local Keycloak realm. Run once from the development VM."""
import getpass
import json
import os
import re
import secrets
import tempfile
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import HTTPRedirectHandler, ProxyHandler, Request, build_opener

ROOT = Path(__file__).resolve().parents[1]
BASE = "http://localhost:8081"
REALM = "/admin/realms/arcana"
MARK = "Managed by Arcana SEC3"


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, *args):
        return None


http = build_opener(ProxyHandler({}), NoRedirect())


def call(path, *, method="GET", data=None, auth=None, form=False):
    hdr = {"Content-Type": "application/x-www-form-urlencoded" if form else "application/json"}
    if auth:
        hdr["Authorization"] = "Bearer " + auth
    body = None if data is None else (urlencode(data) if form else json.dumps(data)).encode()
    req = Request(BASE + path, data=body, headers=hdr, method=method)
    try:
        with http.open(req, timeout=10) as resp:
            raw = resp.read()
            return json.loads(raw) if raw else None
    except HTTPError as exc:
        raise SystemExit(f"Keycloak rejected {method} {path.split('?')[0]} (HTTP {exc.code}). No credentials printed.")
    except URLError:
        raise SystemExit("Cannot reach Keycloak on localhost:8081. Start SEC1 infrastructure first.")


def main():
    target = ROOT / "ai-service/.env"
    if target.is_symlink() or not target.is_file():
        raise SystemExit("Expected an existing regular ai-service/.env; keep your working AI settings.")
    # All access is local; admin credentials and the short admin token stay in memory.
    admin = input("master realm admin username [arcana-admin]: ").strip() or "arcana-admin"
    token = call("/realms/master/protocol/openid-connect/token", method="POST", form=True,
                 data={"grant_type": "password", "client_id": "admin-cli", "username": admin,
                       "password": getpass.getpass("Master admin password (hidden): ")})["access_token"]

    def api(path, method="GET", data=None):
        return call(REALM + path, method=method, data=data, auth=token)

    def client(name):
        return next((x for x in api("/clients?clientId=" + name) if x["clientId"] == name), None)

    web = client("arcana-web")
    if not web or web["publicClient"]:
        raise SystemExit("Expected SEC1 confidential client arcana-web.")
    ai = client("arcana-ai")
    if not ai:
        api("/clients", "POST", {"clientId": "arcana-ai", "description": MARK, "protocol": "openid-connect",
            "publicClient": False, "clientAuthenticatorType": "client-secret", "secret": secrets.token_hex(32),
            "standardFlowEnabled": False, "implicitFlowEnabled": False, "directAccessGrantsEnabled": False,
            "serviceAccountsEnabled": False, "fullScopeAllowed": False})
        ai = client("arcana-ai")
    if not ai or ai.get("description") != MARK or ai.get("publicClient"):
        raise SystemExit("An unrelated arcana-ai client exists; stopped without adopting its secret.")
    scopes = {s["name"]: s for s in api("/client-scopes")}
    if "basic" not in scopes:
        raise SystemExit("Expected the built-in Keycloak basic scope (user subject).")
    if "ai.invoke" not in scopes:
        api("/client-scopes", "POST", {"name": "ai.invoke", "description": MARK, "protocol": "openid-connect",
            "attributes": {"include.in.token.scope": "true", "display.on.consent.screen": "false"}})
        scopes = {s["name"]: s for s in api("/client-scopes")}
    scope = scopes["ai.invoke"]
    if scope.get("description") != MARK or scope.get("attributes", {}).get("include.in.token.scope") != "true":
        raise SystemExit("Unexpected ai.invoke scope configuration. Inspect it before continuing.")
    mpath = "/client-scopes/" + scope["id"] + "/protocol-mappers/models"
    mapper = {"name": "Arcana AI audience", "protocol": "openid-connect", "protocolMapper": "oidc-audience-mapper",
              "config": {"included.client.audience": "arcana-ai", "access.token.claim": "true",
                         "introspection.token.claim": "true", "id.token.claim": "false"}}
    mappings = api(mpath)
    if not mappings:
        api(mpath, "POST", mapper)
    elif len(mappings) != 1 or mappings[0]["protocolMapper"] != mapper["protocolMapper"] or any(
            mappings[0].get("config", {}).get(k) != v for k, v in mapper["config"].items()):
        raise SystemExit("Unexpected audience mapper; inspect ai.invoke before continuing.")
    clientpath = "/clients/" + web["id"]
    defaults = {s["name"] for s in api(clientpath + "/default-client-scopes")}
    if "ai.invoke" in defaults:
        raise SystemExit("ai.invoke must be optional, not default. Correct that assignment first.")
    if "basic" not in defaults:
        api(clientpath + "/default-client-scopes/" + scopes["basic"]["id"], "PUT")
    if "ai.invoke" not in {s["name"] for s in api(clientpath + "/optional-client-scopes")}:
        api(clientpath + "/optional-client-scopes/" + scope["id"], "PUT")
    attrs = api(clientpath).get("attributes", {})
    api(clientpath, "PUT", {"attributes": {**attrs, "standard.token.exchange.enabled": "true"}})
    secret = api("/clients/" + ai["id"] + "/client-secret")["value"]
    if not re.fullmatch(r"[A-Za-z0-9_-]{16,}", secret):
        raise SystemExit("Unexpected AI client secret format. Local file not updated.")
    # Replace only SEC3 keys; atomic rename with owner-only permissions.
    lines = target.read_text().splitlines()
    lines = [s for s in lines if not re.match(r"^\s*(?:export\s+)?(?:AI_INTROSPECTION_SECRET|AI_AUTH_ISSUER)\s*=", s)]
    with tempfile.NamedTemporaryFile(mode="w", dir=target.parent, delete=False) as f:
        os.fchmod(f.fileno(), 0o600)
        f.write("\n".join([*lines, "AI_AUTH_ISSUER=" + BASE + "/realms/arcana",
                           "AI_INTROSPECTION_SECRET=" + secret]) + "\n")
    os.replace(f.name, target)
    print("SEC3 configured. Existing users, readings, and arcana-web secret preserved. Restart app services.")


if __name__ == "__main__":
    main()
