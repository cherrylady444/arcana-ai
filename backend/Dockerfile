# syntax=docker/dockerfile:1

# Restore dependencies separately so Docker can cache this layer.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY Tarot.Api/Tarot.Api.csproj Tarot.Api/
COPY Tarot.Application/Tarot.Application.csproj Tarot.Application/
COPY Tarot.Domain/Tarot.Domain.csproj Tarot.Domain/
COPY Tarot.Infrastructure/Tarot.Infrastructure.csproj Tarot.Infrastructure/

RUN dotnet restore Tarot.Api/Tarot.Api.csproj

# Compile the application.
FROM restore AS build
COPY . .
RUN dotnet build Tarot.Api/Tarot.Api.csproj \
    --configuration Release \
    --no-restore

# Publish only the files required by the runtime container.
FROM build AS publish
RUN dotnet publish Tarot.Api/Tarot.Api.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish \
    /p:UseAppHost=false

# Final image contains the ASP.NET Core runtime, not the full SDK.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Tarot.Api.dll"]
