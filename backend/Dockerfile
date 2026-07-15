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

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "Tarot.Api.dll"]
