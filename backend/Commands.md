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
dotnet build Tarot.slnx
dotnet test Tarot.slnx

Then run the API:

dotnet run --project Tarot.Api

Test:

curl -i -X POST http://localhost:5000/api/readings \
  -H "Content-Type: application/json" \
  -d '{"question":"What should I focus on today?"}'
  