FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/CoffeeAgent.Api/CoffeeAgent.Api.csproj", "src/CoffeeAgent.Api/"]
RUN dotnet restore "src/CoffeeAgent.Api/CoffeeAgent.Api.csproj"

COPY . .
WORKDIR "/src/src/BrewCoffee.Api"
RUN dotnet build "./BrewCoffee.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "./BrewCoffee.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewCoffee.Api.dll"]