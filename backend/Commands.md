## 6. Run the API locally on the server

Run:

```bash
dotnet run --project Tarot.Api/Tarot.Api.csproj --urls http://localhost:5000
or
dotnet run --project backend/Tarot.Api
```

**docker run --rm -p 5000:5000 arcana-backend:dev**

Run this now

From your backend folder:

dotnet clean Tarot.slnx
for fake repo
dotnet restore Tarot.slnx
dotnet build Tarot.slnx
dotnet test Tarot.slnx

Then run the API:

dotnet run --project Tarot.Api

Test:

curl -i -X POST http://localhost:5000/api/readings \
  -H "Content-Type: application/json" \
  -d '{"question":"What should I focus on today?"}'
  
6. Validate the Compose configuration

Before starting anything:
  docker compose config

  docker compose down

docker compose build --no-cache --progress=plain api

Merg impreuna
  docker compose up -d postgres pgadmin
  &
dotnet run --project Tarot.Api

```
dotnet watch --project Tarot.Api
```
->This starts the application and continuously monitors the relevant project files for changes.

When you edit a C# file and save it, dotnet watch tries to apply that change to the running application automatically.

docker compose up -d

or do both in one command
docker compose up --build -d

--------------------------

Run the integration tests

Docker must be running, but your normal arcana-ai-postgres container does not need to be started.

dotnet test Tarot.IntegrationTests/Tarot.IntegrationTests.csproj

Then run the entire solution:

dotnet test Tarot.slnx