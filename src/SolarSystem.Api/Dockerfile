# Contexto de build: raiz do repositorio (ver docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore isolado das fontes: so refaz o restore quando um .csproj muda.
COPY nuget.config .
COPY src/SolarSystem.Domain/SolarSystem.Domain.csproj src/SolarSystem.Domain/
COPY src/SolarSystem.Application/SolarSystem.Application.csproj src/SolarSystem.Application/
COPY src/SolarSystem.Infrastructure/SolarSystem.Infrastructure.csproj src/SolarSystem.Infrastructure/
COPY src/SolarSystem.Api/SolarSystem.Api.csproj src/SolarSystem.Api/
RUN dotnet restore src/SolarSystem.Api/SolarSystem.Api.csproj

COPY src/ src/
RUN dotnet publish src/SolarSystem.Api/SolarSystem.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# A imagem aspnet nao traz wget nem curl; o healthcheck do compose precisa de um deles.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Roda sem privilegios: a imagem aspnet ja traz o usuario 'app'.
USER app

ENV ASPNETCORE_URLS=http://+:5000 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SolarSystem.Api.dll"]
