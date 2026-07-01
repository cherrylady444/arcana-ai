# Arcana AI Backend — Commands, Meaning, and Dockerfile Explanation


## 0. Important current fixes

Before running Docker, fix these two issues.

### 0.1 Rename `TarotCard` to `TarotCard.cs`

Run from the `backend/` folder:

```bash
mv Tarot.Domain/TarotCard Tarot.Domain/TarotCard.cs
```

What this does:

- Renames the file from `TarotCard` to `TarotCard.cs`.
- C# source files must normally end in `.cs`.
- Without `.cs`, the compiler will ignore the file.

Why this matters:

- If `TarotCard` contains C# code but has no `.cs` extension, your project may build without actually including that class.
- Later, when another project tries to use `TarotCard`, you may get confusing errors.

---

### 0.2 Fix the Dockerfile to use `Tarot.slnx`

Your project currently has:

```text
Tarot.slnx
```

but your Dockerfile currently says:

```dockerfile
COPY Tarot.sln ./
RUN dotnet restore Tarot.sln
```

That will fail because there is no `Tarot.sln` file.

Run from the `backend/` folder:

```bash
sed -i 's/Tarot\.sln/Tarot.slnx/g' Dockerfile
```

What this does:

- Finds every `Tarot.sln` reference inside `Dockerfile`.
- Replaces it with `Tarot.slnx`.

Why this matters:

- Docker builds happen inside a temporary container.
- If the Dockerfile tries to copy a file that does not exist, the Docker build stops immediately.

After the fix, the Dockerfile should contain:

```dockerfile
COPY Tarot.slnx ./
RUN dotnet restore Tarot.slnx
```

---

## 1. Go to the backend folder

Run this first whenever you want to work on the backend:

```bash
cd ~/projects/arcana-ai/backend
```

What this does:

- Moves your terminal into the backend project folder.

Why this matters:

- Commands like `dotnet restore`, `dotnet build`, and `docker build` must be run from the correct folder.
- Your `Tarot.slnx` and Dockerfile are located in this folder.

Check that you are in the right place:

```bash
ls
```

You should see something like:

```text
Dockerfile
Tarot.slnx
Tarot.Api
Tarot.Application
Tarot.Domain
Tarot.Infrastructure
Tarot.Tests
```

---

## 2. Check installed .NET version

Run:

```bash
dotnet --version
```

What this does:

- Shows which .NET SDK version is installed on the server.

Why this matters:

- Your projects target `.NET 10`.
- If the installed SDK is too old, the project will not build.

Expected for your current setup:

```text
10.0.109
```

---

## 3. Restore NuGet packages

Run:

```bash
dotnet restore Tarot.slnx
```

What this does:

- Reads the solution file.
- Finds all projects inside it.
- Downloads all NuGet packages required by those projects.

NuGet packages are the .NET equivalent of npm packages.

Examples from your backend:

```text
Serilog.AspNetCore
FluentValidation
FluentValidation.DependencyInjectionExtensions
```

Why this matters:

- The backend cannot build if its dependencies are missing.
- This is similar to running `npm install` in a Node project.

You usually run this:

- after cloning the repo;
- after adding/removing NuGet packages;
- when Docker build complains about missing dependencies.

---

## 4. Build the backend

Run from arcana-ai/backend$:

```bash
dotnet build Tarot.slnx
```

What this does:

- Compiles all C# projects in the solution.
- Checks that the code is valid.
- Checks that project references are correct.

Why this matters:

- This catches compile-time errors before you try to run the app.
- It checks all backend projects, not only the API.

Your solution includes:

```text
Tarot.Api
Tarot.Application
Tarot.Domain
Tarot.Infrastructure
Tarot.Tests
```

If this command succeeds, your backend code compiles.

---

## 5. Run tests

Run:

```bash
dotnet test Tarot.slnx
```

What this does:

- Builds the solution.
- Finds the test project.
- Runs all xUnit tests.

Why this matters:

- Tests will later verify important backend logic.
- For example:
  - card drawing returns unique cards;
  - a 3-card spread returns exactly 3 cards;
  - invalid questions are rejected;
  - the AI service client handles failed responses.

Right now your tests are still minimal, so this mainly checks that the test project is correctly wired.

---

## 6. Run the API locally on the server

Run:

```bash
dotnet run --project Tarot.Api/Tarot.Api.csproj --urls http://localhost:5000
or
dotnet run --project backend/Tarot.Api
```

What this does:

- Starts only the `Tarot.Api` project.
- Makes the API listen on port `5000`.

Why we specify `--project`:

- The backend folder contains multiple projects.
- Only `Tarot.Api` is the web server.
- `Tarot.Domain`, `Tarot.Application`, and `Tarot.Infrastructure` are class libraries and cannot be run directly.

Why we specify `--urls http://localhost:5000`:

- ASP.NET Core otherwise may choose a random development port from `launchSettings.json`.
- Using port `5000` makes testing simpler and predictable.

Leave this terminal running while testing the API.

---

## 7. Test the running API with curl

Open another terminal on the server and run:

```bash
curl http://localhost:5000/
```

What this does:

- Sends an HTTP GET request to the root endpoint.

Expected result:

```json
{
  "service": "tarot-backend",
  "status": "ok"
}
```

Now test the health endpoint:

```bash
curl http://localhost:5000/health
```

What this does:

- Sends an HTTP GET request to `/health`.
- This checks whether the backend service is alive.

Why this matters:

- Health endpoints are useful for Docker, Nginx, deployment, and debugging.
- Before adding real features, you always want to know that the service can start.

---

## 8. Stop the running API

In the terminal where `dotnet run` is running, press:

```text
Ctrl + C
```

What this does:

- Stops the ASP.NET Core server.

Why this matters:

- You need to stop it before starting another process on the same port.
- If port `5000` is already in use, another run may fail.

---

## 9. Clean generated build files

Run from the `backend/` folder:

```bash
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
```

What this does:

- Deletes all `bin/` and `obj/` folders inside the backend.

What are these folders?

```text
bin/ = compiled build output
obj/ = temporary build and restore files
```

Why this matters:

- These files are generated automatically by .NET.
- They should not be committed to GitHub.
- Deleting them is safe because .NET recreates them when you build again.

Run this before committing if you accidentally generated or copied `bin/` and `obj/` folders into the repo.

---

## 10. Build the backend Docker image

Run from the `backend/` folder:

```bash
docker build -t arcana-backend:dev .
```

What this does:

- Reads the `Dockerfile` in the current folder.
- Builds a Docker image for the backend.
- Names the image `arcana-backend`.
- Tags it as `dev`.

Why this matters:

- Docker proves that your backend can run in an isolated production-like environment.
- Later, Docker Compose will run this backend together with PostgreSQL, Redis, frontend, and AI service.

The `.` at the end means:

```text
Use the current folder as the Docker build context.
```

The build context is the set of files Docker can copy into the image.

---

## 11. Run the backend Docker container

Run:

```bash
docker run --rm -p 5000:8080 arcana-backend:dev
```

What this does:

- Starts a container from the `arcana-backend:dev` image.
- Maps port `5000` on your server to port `8080` inside the container.
- Removes the container automatically when it stops because of `--rm`.

Port explanation:

```text
server port 5000  →  container port 8080
```

Why container port `8080`?

- The Dockerfile sets:

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
```

That makes ASP.NET Core listen on port `8080` inside the container.

Why server port `5000`?

- You access the API from outside the container using port `5000`.
- This keeps the same development URL as the local `dotnet run` version.

---

## 12. Test the Dockerized API

While the Docker container is running, open another terminal and run:

```bash
curl http://localhost:5000/
```

Then:

```bash
curl http://localhost:5000/health
```

What this does:

- Confirms that the backend works from inside Docker.

Why this matters:

- It proves your Dockerfile is correct.
- It proves the backend can start without relying on your local shell environment.

---

## 13. Stop the Docker container

In the terminal where `docker run` is running, press:

```text
Ctrl + C
```

Because you used `--rm`, Docker will remove the stopped container automatically.

---

## 14. Check Git status

Run:

```bash
git status
```

What this does:

- Shows which files are changed, new, deleted, or untracked.

Why this matters:

- You should check what you are about to commit.
- You should avoid committing generated folders like `bin/` and `obj/`.

---

## 15. Commit backend setup fixes

After fixing the filename and Dockerfile, run:

```bash
git add .
git commit -m "chore: fix backend bootstrap setup"
```

What this does:

- Stages all current changes.
- Creates a Git commit with a clear message.

Why this matters:

- Commits create checkpoints.
- If you break something later, you can return to a known working state.

---

## 16. Push to GitHub

Run:

```bash
git push
```

If this is a new branch, Git may ask you to run something like:

```bash
git push -u origin feat/bootstrap-skeleton
```

What this does:

- Uploads your local commits to GitHub.

Why this matters:

- Your work is backed up.
- Your project history becomes visible in your portfolio.
- You can later set up GitHub Actions CI/CD.

---

# Dockerfile explanation

Your Dockerfile builds and runs the ASP.NET Core backend in two stages.

This is called a multi-stage Docker build.

Why multi-stage?

- The first stage uses the full .NET SDK to build the app.
- The second stage uses the smaller ASP.NET runtime to run the app.
- The final image is smaller and cleaner.

---

## Full corrected Dockerfile

Because your project has `Tarot.slnx`, not `Tarot.sln`, the Dockerfile should look like this:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Tarot.slnx ./
COPY Tarot.Api/Tarot.Api.csproj Tarot.Api/
COPY Tarot.Application/Tarot.Application.csproj Tarot.Application/
COPY Tarot.Domain/Tarot.Domain.csproj Tarot.Domain/
COPY Tarot.Infrastructure/Tarot.Infrastructure.csproj Tarot.Infrastructure/
COPY Tarot.Tests/Tarot.Tests.csproj Tarot.Tests/

RUN dotnet restore Tarot.slnx

COPY . .

RUN dotnet publish Tarot.Api/Tarot.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Tarot.Api.dll"]
```

---

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

# Recommended next backend step

Before starting the frontend, implement one small backend endpoint:

```http
POST /api/readings
```

At first, this endpoint should not use PostgreSQL or AI.

It should only:

1. receive a question;
2. validate that the question is not empty;
3. draw 3 fake or hardcoded cards;
4. return them as JSON.

Why this is the best next step:

- It teaches you `app.MapPost`.
- It gives the future frontend a real endpoint to call.
- It keeps the learning curve small.
- Later, you can replace fake cards with secure random drawing.
- After that, you can add PostgreSQL persistence.
- After that, you can call the Python AI service.

Suggested learning order:

```text
1. Make backend run.
2. Understand Program.cs.
3. Add GET /health.
4. Add POST /api/readings with fake response.
5. Add domain classes.
6. Add secure card drawing service.
7. Add tests for card drawing.
8. Add database.
9. Add Python AI service call.
10. Only then build the frontend form.
```
