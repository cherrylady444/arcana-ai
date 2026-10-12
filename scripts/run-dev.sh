#!/usr/bin/env bash
# Arcana development launcher for Linux. Save as arcana-ai/scripts/run-dev.sh.
# Prerequisites: existing SEC1 configuration, installed dependencies, and uv.
# Run from any directory: bash /path/to/arcana-ai/scripts/run-dev.sh

# Stop on command failures, unset variables, or failures inside pipelines.
set -euo pipefail

# Find the project using this script's location, not the terminal's directory.
task_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Check the tools required for startup. setsid gives each service its own
# process group, allowing cleanup to stop npm/dotnet/uv and their children.
for tool in docker curl dotnet npm uv setsid python3; do
  command -v "$tool" >/dev/null || { echo "Missing command: $tool" >&2; exit 1; }
done

# Reuse working credentials. Do not generate replacements for an existing DB.
if [[ ! -f "$task_root/.security/sec1.env" ]]; then
  echo "Missing .security/sec1.env. Complete SEC1 setup before starting." >&2
  exit 1
fi

# Detect existing application listeners before launching duplicates.
# This checks the Linux server; VS Code's Windows forwarding is separate.
python3 - <<'PY'
import socket
import sys

busy = []
for port in (3000, 5000, 8000):
    with socket.socket() as listener:
        try:
            listener.bind(("127.0.0.1", port))
        except OSError:
            busy.append(str(port))
if busy:
    sys.exit("Stop the existing processes on server ports: " + ", ".join(busy))
PY

# Start the databases and Keycloak, and wait for Keycloak discovery.
# This reuses your updated sec1-infra.sh (Keycloak external port 8081).
bash "$task_root/scripts/sec1-infra.sh"

# Record only process groups started by THIS launcher.
task_groups=()

# On exit, give the application processes time to stop, then force-stop
# remaining members of their groups. Docker containers are left running.
cleanup() {
  trap '' INT TERM
  for task_pid in "${task_groups[@]}"; do
    kill -TERM -- "-$task_pid" 2>/dev/null || true
  done
  for task_attempt in {1..50}; do
    task_alive=false
    for task_pid in "${task_groups[@]}"; do
      if kill -0 -- "-$task_pid" 2>/dev/null; then task_alive=true; fi
    done
    if [[ "$task_alive" == false ]]; then break; fi
    sleep 0.1
  done
  for task_pid in "${task_groups[@]}"; do
    kill -KILL -- "-$task_pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true

  # Stop Keycloak and both databases without deleting containers or volumes.
  # Absolute paths make this work regardless of the terminal's directory.
  # Do not use "down -v": the volumes contain accounts and application data.
  echo "Stopping Keycloak and both PostgreSQL containers..."
  if ! docker compose --project-directory "$task_root/backend" \
    --env-file "$task_root/.security/sec1.env" \
    -f "$task_root/backend/docker-compose.yml" \
    -f "$task_root/backend/compose.security.yml" \
    stop keycloak keycloak-postgres postgres; then
    echo "Container shutdown failed. Check Docker and stop the containers manually." >&2
  fi
}

# Run cleanup on normal exit, errors, Ctrl+C, or a termination request.
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

# Each background command runs in a separate process group and directory.
# exec replaces the temporary shell with the service's startup command.
# & lets the launcher continue; $! records the background process ID.
setsid bash -c 'cd "$1/ai-service"; exec uv run fastapi dev --host 127.0.0.1 --port 8000' _ "$task_root" &
task_groups+=("$!")

# Load ASP.NET settings, apply pending migrations, then start the API.
setsid bash "$task_root/scripts/sec1-api.sh" &
task_groups+=("$!")

# Start Next.js on its fixed port. Requests may fail until the API is ready.
setsid bash "$task_root/scripts/sec1-frontend.sh" &
task_groups+=("$!")

# All logs appear in this terminal. These URLs are usable from your browser
# after forwarding server ports 3000 and 8081 in VS Code.
echo "Starting Arcana. Wait for each service's ready/listening message."
echo "Frontend: http://localhost:3000 | Keycloak: http://localhost:8081"
echo "Ctrl+C stops Python, ASP.NET and Next.js. Keycloak and both databases."

# Wait for any application to exit. If one fails, stop its siblings instead
# of leaving a partially running development stack behind.
task_status=0
wait -n "${task_groups[@]}" || task_status=$?
echo "An application process exited (status $task_status); stopping the others."
exit "$task_status"
