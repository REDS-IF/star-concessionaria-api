# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

# Copia solucoes e arquivos de projeto para otimizacao de cache
COPY Directory.Build.props ./
COPY StarConcessionaria.sln ./
COPY src/StarConcessionaria.Domain/*.csproj ./src/StarConcessionaria.Domain/
COPY src/StarConcessionaria.Application/*.csproj ./src/StarConcessionaria.Application/
COPY src/StarConcessionaria.Infrastructure/*.csproj ./src/StarConcessionaria.Infrastructure/
COPY src/StarConcessionaria.Api/*.csproj ./src/StarConcessionaria.Api/
COPY tests/StarConcessionaria.UnitTests/*.csproj ./tests/StarConcessionaria.UnitTests/
COPY tests/StarConcessionaria.IntegrationTests/*.csproj ./tests/StarConcessionaria.IntegrationTests/

RUN dotnet restore

# Copia codigo-fonte completo e publica em release
COPY . ./
RUN dotnet publish src/StarConcessionaria.Api/StarConcessionaria.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Adiciona usuario nao-root para seguranca de producao
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StarConcessionaria.Api.dll"]
