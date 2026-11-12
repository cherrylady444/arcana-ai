#!/usr/bin/env bash
# Manage Arcana on its Linux development VM. Docker supervises the processes.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root/backend"

# Use the SAME Compose project directory and files as sec1-infra.sh so its
# database volumes and Keycloak realm are reused. No "down -v" action here.
export ARCANA_UID="$(id -u)" ARCANA_GID="$(id -g)"
compose=(docker compose --env-file ../.security/sec1.env -f docker-compose.yml
         -f compose.security.yml -f compose.local-app.yml)
services=(postgres keycloak-postgres keycloak local-ai local-api local-web)
action="${1:-up}"

case "$action" in
  up)
    # Validate without printing the expanded Compose file (it contains secrets).
    [[ -s ../ai-service/.env && -s ../.security/sec1.env ]] || {
      echo "Missing ai-service/.env or .security/sec1.env. Reuse your working development configuration." >&2; exit 1; }
    "${compose[@]}" config --quiet
    # Do not take a port away from an existing run-dev.sh instance.
    for pair in local-web:3000 local-api:5000 local-ai:8000; do
      service="${pair%:*}"; port="${pair#*:}"
      if [[ -z "$("${compose[@]}" ps --status running -q "$service")" ]] &&
         [[ -n "$(ss -H -ltn "sport = :$port")" ]]; then
        echo "Port $port is occupied. Stop the old launcher or competing container first." >&2; exit 1
      fi
    done
    # Run the API with the same Linux user and key directory as the host API.
    mkdir -p -m 700 Tarot.Api/.security-keys
    # Compose only starts the six named services (not the old 'api' or pgAdmin).
    "${compose[@]}" up -d --build "${services[@]}"
    # Probe all four HTTP entry points; a started container need not be ready.
    for attempt in {1..90}; do
      if curl -fsS --max-time 3 -o /dev/null http://localhost:8081/realms/arcana/.well-known/openid-configuration 2>/dev/null &&
         curl -fsS --max-time 3 -o /dev/null http://127.0.0.1:5000/health 2>/dev/null &&
         curl -fsS --max-time 3 -o /dev/null http://127.0.0.1:8000/health 2>/dev/null &&
         curl -fsS --max-time 4 -o /dev/null http://127.0.0.1:3000/account 2>/dev/null; then
        echo "Arcana is ready. Open http://localhost:3000 with ports 3000 and 8081 forwarded."
        exit 0
      fi
      sleep 2
    done
    echo "Arcana is not ready yet. Run: bash scripts/arcana.sh status (or logs)." >&2; exit 1 ;;
  stop) # Stop services but keep all database volumes and session keys.
    "${compose[@]}" stop "${services[@]}" ;;
  status) "${compose[@]}" ps --all "${services[@]}" ;;
  logs) # Ctrl+C exits the log viewer only.
    "${compose[@]}" logs --tail 40 --follow "${services[@]}" ;;
  *) echo "Usage: bash scripts/arcana.sh {up|stop|status|logs}" >&2; exit 2 ;;
esac
