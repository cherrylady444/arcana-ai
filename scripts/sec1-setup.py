#!/usr/bin/env python3
"""Create local-only SEC-1 secrets once. Never print or overwrite credentials."""
from pathlib import Path
import os
import secrets

ROOT = Path(__file__).resolve().parents[1]
directory = ROOT / ".security"
directory.mkdir(mode=0o700, exist_ok=True)
directory.chmod(0o700)
target = directory / "sec1.env"
if target.exists():
    print("SEC-1 configuration already exists; preserved all credentials.")
else:
    values = {
        "KEYCLOAK_DB_PASSWORD": secrets.token_hex(32),
        "KEYCLOAK_ADMIN_PASSWORD": secrets.token_hex(32),
        "Security__ClientSecret": secrets.token_hex(32),
        "Security__Authority": "http://localhost:8081/realms/arcana",
        "Security__PublicOrigin": "http://localhost:3000",
        "Security__AllowLoopbackHttp": "true",
        "ASPNETCORE_ENVIRONMENT": "Development",
    }
    with os.fdopen(os.open(target, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600), "w") as stream:
        stream.write("".join(f"{key}={value}\n" for key, value in values.items()))
    print("Created local SEC-1 configuration. No secrets were printed.")

target.chmod(0o600)
print("Next: bash scripts/sec1-infra.sh")
