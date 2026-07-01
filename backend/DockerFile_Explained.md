
## Line-by-line explanation

### 1. Build stage

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

What it does:

- Starts from Microsoft’s official .NET SDK image.
- Names this stage `build`.

Why this matters:

- The SDK image contains tools like:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet publish`
- You need the SDK to compile the app.

---

```dockerfile
WORKDIR /src
```

What it does:

- Sets the working directory inside the Docker image to `/src`.

Why this matters:

- All following commands run from `/src` unless another directory is specified.
- It keeps the build organized.

---

```dockerfile
COPY Tarot.slnx ./
```

What it does:

- Copies `Tarot.slnx` from your server into `/src` inside the image.

Why this matters:

- Docker needs the solution file to restore all project dependencies.

---

```dockerfile
COPY Tarot.Api/Tarot.Api.csproj Tarot.Api/
COPY Tarot.Application/Tarot.Application.csproj Tarot.Application/
COPY Tarot.Domain/Tarot.Domain.csproj Tarot.Domain/
COPY Tarot.Infrastructure/Tarot.Infrastructure.csproj Tarot.Infrastructure/
COPY Tarot.Tests/Tarot.Tests.csproj Tarot.Tests/
```

What this does:

- Copies only the `.csproj` files first.

Why this matters:

- Docker caches layers.
- NuGet restore depends mostly on `.csproj` files.
- If your source code changes but your project files do not, Docker can reuse the restore layer.
- This makes future builds faster.

---

```dockerfile
RUN dotnet restore Tarot.slnx
```

What it does:

- Downloads NuGet packages for all projects in the solution.

Why this matters:

- The app cannot be built without its dependencies.
- Doing restore before copying all source code improves Docker cache efficiency.

---

```dockerfile
COPY . .
```

What it does:

- Copies the rest of the backend source code into the image.

Why this matters:

- Now Docker has the actual C# files, configuration files, and project folders needed to publish the app.

---

```dockerfile
RUN dotnet publish Tarot.Api/Tarot.Api.csproj -c Release -o /app/publish
```

What it does:

- Publishes the ASP.NET Core API project.
- Uses Release mode.
- Places the final publish output into `/app/publish`.

Why this matters:

- `dotnet publish` prepares the app for running in production-like environments.
- It collects compiled DLLs and required runtime files.
- We publish only `Tarot.Api` because it is the actual runnable web app.

---

## 2. Runtime stage

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

What it does:

- Starts a new image from the ASP.NET Core runtime image.

Why this matters:

- This image is smaller than the SDK image.
- It contains what is needed to run ASP.NET Core apps, but not the full compiler/toolchain.
- Smaller runtime images are better for deployment.

---

```dockerfile
WORKDIR /app
```

What it does:

- Sets `/app` as the working directory in the runtime container.

Why this matters:

- The published app files will live here.

---

```dockerfile
COPY --from=build /app/publish .
```

What it does:

- Copies the published output from the earlier `build` stage into the final runtime image.

Why this matters:

- The final image only receives the clean published app.
- It does not include source code, intermediate build files, or the full SDK.

---

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
```

What it does:

- Sets an environment variable telling ASP.NET Core to listen on port `8080` inside the container.

The `+` means:

```text
Listen on all network interfaces inside the container.
```

Why this matters:

- If the app only listened on `localhost` inside the container, it might not be reachable from outside the container.
- `http://+:8080` makes the containerized API accessible through Docker port mapping.

---

```dockerfile
EXPOSE 8080
```

What it does:

- Documents that the container is intended to use port `8080`.

Important:

- `EXPOSE` does not publish the port by itself.
- You still need `docker run -p 5000:8080 ...` or a Docker Compose ports mapping.

Why this matters:

- It makes the Dockerfile clearer.
- Tools and developers can see which port the app expects.

---

```dockerfile
ENTRYPOINT ["dotnet", "Tarot.Api.dll"]
```

What it does:

- Defines the command that runs when the container starts.

Meaning:

```bash
dotnet Tarot.Api.dll
```

Why this matters:

- `Tarot.Api.dll` is the compiled ASP.NET Core application.
- This starts your backend server inside the container.

---
