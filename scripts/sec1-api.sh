#!/usr/bin/env bash
set -euo pipefail
task_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ ! -f "$task_root/.security/sec1.env" ]]; then
  echo "Run python3 scripts/sec1-setup.py first." >&2
  exit 1
fi
source "$task_root/.security/sec1.env"
export Security__ClientSecret Security__Authority Security__PublicOrigin Security__AllowLoopbackHttp
export ASPNETCORE_ENVIRONMENT
unset KEYCLOAK_ADMIN_PASSWORD KEYCLOAK_DB_PASSWORD
cd "$task_root/backend"
# Apply pending EF migrations before starting the API.
dotnet ef database update --project Tarot.Infrastructure --startup-project Tarot.Api
exec dotnet run --project Tarot.Api --no-launch-profile
