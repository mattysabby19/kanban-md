# syntax=docker/dockerfile:1.7

# ----------------------------------------------------------------------------
# Build stage
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy MSBuild props + csprojs first so `dotnet restore` benefits from layer
# cache when only source files change.
COPY Directory.Build.props ./
COPY src/Kanban.Md.App/Kanban.Md.App.csproj src/Kanban.Md.App/
COPY src/Kanban.Md.Cli/Kanban.Md.Cli.csproj src/Kanban.Md.Cli/
RUN dotnet restore src/Kanban.Md.App/Kanban.Md.App.csproj

# Copy the rest and publish. The App project uses Microsoft.NET.Sdk.Web,
# which materializes wwwroot static assets into the publish output (the CLI
# project alone does not — see CHANGELOG entry on KMD-0031-followup).
COPY src/ src/
RUN dotnet publish src/Kanban.Md.App/Kanban.Md.App.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

# ----------------------------------------------------------------------------
# Runtime stage
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl is needed for HEALTHCHECK; clear apt lists to keep the layer small.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# The aspnet:8.0 image already ships with a non-root `app` user
# (UID/GID configured by Microsoft). /data is the mount point for the
# consuming project's tasks directory and must be writable by it.
RUN mkdir -p /data && chown -R app:app /data

COPY --from=build --chown=app:app /app/publish /app

USER app

ENV ASPNETCORE_URLS=http://0.0.0.0:8090 \
    ASPNETCORE_ENVIRONMENT=Production \
    KanbanMd__TasksPath=/data \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false

EXPOSE 8090
VOLUME ["/data"]

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail --silent http://localhost:8090/healthz || exit 1

ENTRYPOINT ["dotnet", "Kanban.Md.App.dll"]
