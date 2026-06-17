FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GMLSSystem.csproj", "./"]
COPY ["GMLSSystem.Shared/GMLSSystem.Shared.csproj", "GMLSSystem.Shared/"]

RUN dotnet restore "GMLSSystem.csproj"

COPY . .

RUN dotnet publish "GMLSSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "GMLSSystem.dll"]