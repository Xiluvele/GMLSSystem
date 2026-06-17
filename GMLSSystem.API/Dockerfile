# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY GMLSSystem.API/GMLSSystem.API.csproj GMLSSystem.API/
COPY GMLSSystem.Shared/GMLSSystem.Shared.csproj GMLSSystem.Shared/

# Restore dependencies
RUN dotnet restore GMLSSystem.API/GMLSSystem.API.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/GMLSSystem.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GMLSSystem.API.dll"]