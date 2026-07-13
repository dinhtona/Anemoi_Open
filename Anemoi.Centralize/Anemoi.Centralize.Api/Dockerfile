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
COPY ["Anemoi.Centralize/Anemoi.Centralize.Api/Anemoi.Centralize.Api.csproj", "Anemoi.Centralize/Anemoi.Centralize.Api/"]
COPY ["Anemoi.Centralize/Anemoi.Centralize.Infrastructure/Anemoi.Centralize.Infrastructure.csproj", "Anemoi.Centralize/Anemoi.Centralize.Infrastructure/"]
COPY ["Anemoi.Centralize/Anemoi.Centralize.Application/Anemoi.Centralize.Application.csproj", "Anemoi.Centralize/Anemoi.Centralize.Application/"]
# COPY ["Anemoi.Contract/Anemoi.Contract.Notification/Anemoi.Contract.Notification.csproj", "Anemoi.Contract/Anemoi.Contract.Notification/"]
COPY ["Anemoi.Contract/Anemoi.Contract.CrossCuttingConcern/Anemoi.Contract.CrossCuttingConcern.csproj", "Anemoi.Contract/Anemoi.Contract.CrossCuttingConcern/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Infrastructure/Anemoi.BuildingBlock.Infrastructure.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Infrastructure/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Anemoi.BuildingBlock.Application.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/"]
COPY ["Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/Anemoi.BuildingBlock.Domain.csproj", "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/"]
COPY ["Anemoi.Contract/Anemoi.Contract.Identity/Anemoi.Contract.Identity.csproj", "Anemoi.Contract/Anemoi.Contract.Identity/"]
COPY ["Anemoi.Contract/Anemoi.Contract.MasterData/Anemoi.Contract.MasterData.csproj", "Anemoi.Contract/Anemoi.Contract.MasterData/"]
COPY ["Anemoi.Contract/Anemoi.Contract.Workspace/Anemoi.Contract.Workspace.csproj", "Anemoi.Contract/Anemoi.Contract.Workspace/"]
COPY ["Anemoi.Centralize/Anemoi.Centralize.Domain/Anemoi.Centralize.Domain.csproj", "Anemoi.Centralize/Anemoi.Centralize.Domain/"]
RUN dotnet restore "Anemoi.Centralize/Anemoi.Centralize.Api/Anemoi.Centralize.Api.csproj"
COPY . .
WORKDIR "/src/Anemoi.Centralize/Anemoi.Centralize.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Anemoi.Centralize.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /home/app/.anemoi/jwt
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Anemoi.Centralize.Api.dll"]
