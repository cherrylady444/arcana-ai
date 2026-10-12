# Run the run-dev.sh
```
cd ~/projects/arcana-ai
bash scripts/run-dev.sh
```

# Terminal 1: start infrastructure
**Compose paths below are relative to the backend directory.**
```
cd ~/projects/arcana-ai/backend

# Read the existing secrets and combine the two Compose files.
# -d means the containers keep running in the background.
# Start only these services; ASP.NET will run outside Docker.
docker compose --env-file ../.security/sec1.env \
  -f docker-compose.yml \
  -f compose.security.yml \
  up -d postgres keycloak-postgres keycloak
```

Then wait for Keycloak to respond, matching the infrastructure script’s purpose

# Terminal 2: start ASP.NET
```
cd ~/projects/arcana-ai

# Read the existing settings into this shell.
# "source" reads the file; it does not modify it.
source .security/sec1.env

# Make the settings available to the ASP.NET process.
export Security__ClientSecret
export Security__Authority
export Security__PublicOrigin
export Security__AllowLoopbackHttp
export ASPNETCORE_ENVIRONMENT

# ASP.NET does not need Keycloak's administrator or database passwords.
# This removes them from this shell, NOT from the configuration file.
unset KEYCLOAK_ADMIN_PASSWORD KEYCLOAK_DB_PASSWORD

cd backend

# Apply pending database migrations.
# If there are none, this reports that the database is up to date.
dotnet ef database update \
  --project Tarot.Infrastructure \
  --startup-project Tarot.Api

# Start ASP.NET using the configuration loaded above.
# --no-launch-profile avoids launch-profile overrides.
dotnet run --project Tarot.Api --no-launch-profile
```

# Terminal 3: start Next.js
```
cd ~/projects/arcana-ai/frontend

# Tell the Next.js server where ASP.NET is running.
export API_INTERNAL_BASE_URL=http://127.0.0.1:5000

# Start the frontend on the exact port registered for login callbacks.
# Binding to 127.0.0.1 makes it reachable through your SSH forwarding.
npm run dev -- --hostname 127.0.0.1 --port 3000
```

# Terminal 4: start Python
Your project’s ai-service/Commands.md uses uv run fastapi dev. Here it is with the address and port explicit:
```
cd ~/projects/arcana-ai/ai-service

# uv runs FastAPI in the project's Python environment.
# The entry point app.main:app is configured in pyproject.toml.
# Development mode reloads the service when source files change.
uv run fastapi dev --host 127.0.0.1 --port 8000
```
