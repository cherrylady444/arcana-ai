**dotnet new sln -n Tarot** - Create an empty .NET solution named Tarot.

Creates: Tarot.sln

**dotnet new web -n Tarot.Api** - Create a minimal ASP.NET Core web project named Tarot.Api.

Creates:
```
Tarot.Api/
  Program.cs
  Tarot.Api.csproj
  appsettings.json
```

**dotnet new classlib -n Tarot.Domain**- dotnet new classlib -n Tarot.Domain

Creates:
```
Tarot.Domain/
  Tarot.Domain.csproj
  Class1.cs
```

**dotnet new xunit -n Tarot.Tests**- Create a test project using xUnit.

Creates:
```
Tarot.Tests/
  Tarot.Tests.csproj
  UnitTest1.cs
```

**dotnet sln add Tarot.Api/Tarot.Api.csproj** - Add the Tarot.Api project to the Tarot.sln solution.


**dotnet add Tarot.Application/Tarot.Application.csproj reference Tarot.Domain/Tarot.Domain.csproj** - Allow Tarot.Application to use code from Tarot.Domain.

## 6. Run the API locally on the server

Run:

```bash
dotnet run --project Tarot.Api/Tarot.Api.csproj --urls http://localhost:5000
or
dotnet run --project backend/Tarot.Api
```

**docker run --rm -p 5000:8080 arcana-backend:dev**