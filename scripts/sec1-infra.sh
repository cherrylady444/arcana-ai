#!/usr/bin/env bash
set -euo pipefail
task_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ ! -f "$task_root/.security/sec1.env" ]]; then
  echo "Run python3 scripts/sec1-setup.py first." >&2
  exit 1
fi
cd "$task_root/backend"
# Starting only these services preserves the existing host-run API workflow.
docker compose --env-file ../.security/sec1.env \
  -f docker-compose.yml -f compose.security.yml up -d postgres keycloak-postgres keycloak
echo "Waiting for Keycloak discovery at localhost:8081..."
for attempt in $(seq 1 90); do
  if curl --fail --silent --max-time 2 http://localhost:8081/realms/arcana/.well-known/openid-configuration >/dev/null; then
    echo "Keycloak is ready. Next: bash scripts/sec1-api.sh"
    exit 0
  fi
  sleep 2
done
echo "Keycloak did not become ready. Inspect docker compose logs for keycloak." >&2
exit 1
