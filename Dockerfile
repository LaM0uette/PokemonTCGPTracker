# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY PokemonTCGPTracker.sln ./
COPY PokemonTCGPTracker.Shared/PokemonTCGPTracker.Shared.csproj PokemonTCGPTracker.Shared/
COPY PokemonTCGPTracker/PokemonTCGPTracker.Client/PokemonTCGPTracker.Client.csproj PokemonTCGPTracker/PokemonTCGPTracker.Client/
COPY PokemonTCGPTracker/PokemonTCGPTracker/PokemonTCGPTracker.csproj PokemonTCGPTracker/PokemonTCGPTracker/

# Restore dependencies
RUN dotnet restore

# Copy the remaining source
COPY . .

# Publish the server project (references Client + Shared)
WORKDIR /src/PokemonTCGPTracker/PokemonTCGPTracker
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Optionally document the port
EXPOSE 8080

# Run the server
ENTRYPOINT ["dotnet", "PokemonTCGPTracker.dll"]
