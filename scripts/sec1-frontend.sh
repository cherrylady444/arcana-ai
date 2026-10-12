#!/usr/bin/env bash
set -euo pipefail
task_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$task_root/frontend"
export API_INTERNAL_BASE_URL=http://127.0.0.1:5000
# A fixed port avoids silently switching to 3001 and breaking the OIDC callback.
exec npm run dev -- --hostname 127.0.0.1 --port 3000
