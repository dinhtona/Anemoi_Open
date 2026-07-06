FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj", "Anemoi.Hr/Anemoi.Hr.Api/"]
COPY ["Anemoi.Hr/Anemoi.Hr.Infrastructure/Anemoi.Hr.Infrastructure.csproj", "Anemoi.Hr/Anemoi.Hr.Infrastructure/"]
COPY ["Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj", "Anemoi.Hr/Anemoi.Hr.Application/"]
COPY ["Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj", "Anemoi.Hr/Anemoi.Hr.Domain/"]
COPY ["Anemoi.Hr/Anemoi.Hr.ModelIds/Anemoi.Hr.ModelIds.csproj", "Anemoi.Hr/Anemoi.Hr.ModelIds/"]
COPY ["Anemoi.Contract/Anemoi.Contract.Identity/Anemoi.Contract.Identity.csproj", "Anemoi.Contract/Anemoi.Contract.Identity/"]
COPY ["Anemoi.Contract/Anemoi.Contract.MasterData/Anemoi.Contract.MasterData.csproj", "Anemoi.Contract/Anemoi.Contract.MasterData/"]
COPY ["Anemoi.Contract/Anemoi.Contract.CrossCuttingConcern/Anemoi.Contract.CrossCuttingConcern.csproj", "Anemoi.Contract/Anemoi.Contract.CrossCuttingConcern/"]
COPY ["Anemoi.Contract/Anemoi.Contract.Identity.ModelIds/Anemoi.Contract.Identity.ModelIds.csproj", "Anemoi.Contract/Anemoi.Contract.Identity.ModelIds/"]
COPY ["Anemoi.Contract/Anemoi.Contract.MasterData.ModelIds/Anemoi.Contract.MasterData.ModelIds.csproj", "Anemoi.Contract/Anemoi.Contract.MasterData.ModelIds/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Infrastructure/Anemoi.BuildingBlock.Infrastructure.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Infrastructure/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Anemoi.BuildingBlock.Application.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/Anemoi.BuildingBlock.Domain.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/"]
RUN dotnet restore "Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj"
COPY . .
WORKDIR "/src/Anemoi.Hr/Anemoi.Hr.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Anemoi.Hr.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /home/app/.anemoi/jwt
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Anemoi.Hr.Api.dll"]
