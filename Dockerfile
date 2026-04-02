# PRoCon Frostbite v2.0 — Docker Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore
COPY src/Directory.Build.props .
COPY src/PRoCon.Core/PRoCon.Core.csproj PRoCon.Core/
COPY src/PRoCon.Console/PRoCon.Console.csproj PRoCon.Console/
RUN dotnet restore PRoCon.Console/PRoCon.Console.csproj

# Build
COPY src/ .
RUN dotnet publish PRoCon.Console/PRoCon.Console.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app/publish

# Runtime — slim image, no SDK
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish/PRoCon.Console /app/PRoCon.Console
RUN chmod +x /app/PRoCon.Console

# /config is the data directory (ProConPaths auto-detects containers)
# Mount a volume here for persistent configs, plugins, logs, cache
VOLUME /config

# Pre-create data subdirectories so volume mounts work on first run
RUN mkdir -p /config/Configs /config/Plugins /config/Logs /config/Cache

EXPOSE 27260

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD pgrep -f PRoCon.Console || exit 1

ENTRYPOINT ["/app/PRoCon.Console"]
