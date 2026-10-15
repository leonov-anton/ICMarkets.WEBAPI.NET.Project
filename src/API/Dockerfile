# Build context is the repository root: docker build -f src/API/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Project files first, so the restore layer is cached until dependencies change.
COPY src/Domain/ICMarkets.Domain.csproj src/Domain/
COPY src/Application/ICMarkets.Application.csproj src/Application/
COPY src/Infrastructure/ICMarkets.Infrastructure.csproj src/Infrastructure/
COPY src/API/ICMarkets.API.csproj src/API/
RUN dotnet restore src/API/ICMarkets.API.csproj

COPY src/ src/
RUN dotnet publish src/API/ICMarkets.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The base image listens on 8080 and has a non-root "app" user.
EXPOSE 8080
USER app

# Exec form: dotnet is PID 1 and receives SIGTERM on docker stop, so the job releases its Redis lock.
ENTRYPOINT ["dotnet", "ICMarkets.API.dll"]
