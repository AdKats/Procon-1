# PRoCon Frostbite - Docker Build
# Multi-stage build: SDK for building, runtime for running

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for NuGet restore
COPY src/Directory.Build.props .
COPY src/PRoCon.Core/PRoCon.Core.csproj PRoCon.Core/
COPY src/PRoCon.Console/PRoCon.Console.csproj PRoCon.Console/
RUN dotnet restore PRoCon.Console/PRoCon.Console.csproj

# Copy all source files
COPY src/ .

# Build
RUN dotnet publish PRoCon.Console/PRoCon.Console.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage - Debian-based for plugin native library compatibility
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy resources
COPY src/Resources/ /app/Resources/

# Create directories for persistent data
RUN mkdir -p /app/Configs /app/Plugins /app/Logs

# Expose default RCON layer port and HTTP server port
EXPOSE 27260
EXPOSE 27360

# Set environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD pgrep -f PRoCon.Console || exit 1

ENTRYPOINT ["dotnet", "PRoCon.Console.dll", "-console", "1"]
