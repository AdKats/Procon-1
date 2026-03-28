# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PRoCon 2 (PRoCon Frostbite) is a free, open-source remote control (RCON) tool for managing game servers. It supports Battlefield (BFBC2, BF3, BF4, Hardline) and Medal of Honor: Warfighter through the Frostbite engine. Written in C# targeting .NET 8, cross-platform (Windows, Linux, Mac). Modernized from the original Procon 1 with Avalonia UI, async networking, SignalR layer, and full plugin backward compatibility.

## Build Commands

```bash
# Restore and build
dotnet restore src/PRoCon.sln
dotnet build src/PRoCon.sln --configuration Release

# Build specific project
dotnet build src/PRoCon.Core/PRoCon.Core.csproj

# Publish cross-platform
dotnet publish src/PRoCon.Console/PRoCon.Console.csproj -c Release -r linux-x64 --self-contained
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r win-x64 --self-contained
```

Build output goes to `builds/Release/` or `builds/Debug/`.

## Running

```bash
# GUI application (Avalonia, cross-platform)
./builds/Release/PRoCon.UI

# Headless/console mode (for servers/Docker)
./builds/Release/PRoCon.Console -console 1

# Docker
docker compose up -d
```

## Testing

There is no automated test suite. Testing is manual against game servers.

## CI/CD

GitHub Actions workflow (`.github/workflows/build-and-release.yml`):
- **PR to master**: Builds on Linux, Windows, macOS
- **Tag push (`v*`)**: Multi-platform publish (linux-x64, linux-arm64, win-x64, osx-x64, osx-arm64), creates release archives with SHA-256 checksums
- Version is extracted from git tag and stamped into `src/Directory.Build.props`

## Architecture

### Solution Structure

The solution (`src/PRoCon.sln`) contains these projects:

| Project | Purpose | Platform |
|---------|---------|----------|
| `PRoCon.Core` | Core business logic library | net8.0 (cross-platform) |
| `PRoCon.UI` | Avalonia GUI application | net8.0 (cross-platform) |
| `PRoCon.Console` | Headless console application | net8.0 (cross-platform) |
| `PRoCon.Service` | Windows Service + Linux systemd | net8.0 |
| `PRoCon.Themes` | Dark/Light theme definitions | net8.0 |
| `PRoConUpdater` | Auto-updater utility | net8.0 |

All executables depend on `PRoCon.Core` for business logic. `PRoCon.UI` depends on `PRoCon.Themes`.

### Key Architectural Layers

**Network/Protocol**: `PRoCon.Core/Remote/FrostbiteConnection.cs` handles async TCP communication using the Frostbite binary RCON protocol. Supports optional TLS wrapping. Game-specific clients (`BF3Client.cs`, `BF4Client.cs`, etc.) extend the base protocol.

**Client Orchestration**: `PRoCon.Core/Remote/PRoConClient.cs` manages authentication, player lists, server state, and fires 100+ event types consumed by plugins and the UI.

**Plugin System**: Plugins are C# source files compiled at runtime using Roslyn (`Microsoft.CodeAnalysis`). Loaded via `AssemblyLoadContext` for isolation. Plugin references: `PRoCon.Core.dll`, `MySqlConnector.dll`, `Newtonsoft.Json.dll`. The API is defined through `IPRoConPluginInterface`. Optional async API via `IPRoConPluginInterfaceAsync`.

**Layer System**: SignalR-based (WebSocket) admin connection hub at `PRoCon.Core/Layer/`. Replaces the legacy TCP-based layer in `PRoCon.Core/Remote/Layer/` (kept for reference). Uses JWT authentication.

**Theme Engine**: `PRoCon.Themes` contains dark/light Avalonia ResourceDictionaries with Blue + Orange gaming-oriented palette. `ThemeManager` handles runtime theme switching.

### Naming Conventions

- **C-prefix**: Core data structures (`CBanInfo`, `CMap`, `CPlayerInfo`, `CServerInfo`)
- **I-prefix**: Interfaces (`IPRoConPluginInterface`)
- **ScheduledTask**: The task scheduler class (renamed from `Task` to avoid `System.Threading.Tasks.Task` conflict)

### Key Directories in `src/PRoCon.Core/`

- `Remote/` - Game server connection (async), protocol handling, packet caching
- `Layer/` - SignalR-based admin layer hub (new)
- `Remote/Layer/` - Legacy TCP layer (kept for reference)
- `Plugin/` - Plugin API, AssemblyLoadContext loader, command system
- `Config/` - Dual-format config manager (.cfg + JSON)
- `Logging/` - Centralized logging via Microsoft.Extensions.Logging
- `Accounts/` - Account management and privilege system
- `HttpServer/` - Embedded web server with HTTPS support
- `Battlemap/` - Map geometry and zone system (cross-platform point-in-polygon)

### Game Definitions

Game protocol commands are defined in `.def` files under `src/Resources/Configs/` (e.g., `BF4.def`, `BF3.def`).

## Key Dependencies

- **Newtonsoft.Json 13.0.3** - JSON serialization (also exposed to plugins)
- **Microsoft.CodeAnalysis.CSharp 4.8.0** - Roslyn compiler for runtime plugin compilation
- **MySqlConnector 2.4.0** - MySQL/MariaDB 5+ connectivity (replaces MySql.Data, exposed to plugins)
- **MaxMind.GeoIP2 5.2.0** - Country lookup by IP
- **Microsoft.Extensions.Logging** - Structured logging
- **Microsoft.Extensions.Configuration** - Dual-format config system
- **Microsoft.AspNetCore.SignalR** - WebSocket-based layer hub
- **Avalonia 11.2.3** - Cross-platform UI framework
- **SharpZipLib 1.4.2** - ZIP handling (replaces Ionic.Zip)

## Docker

```bash
# Build and run
docker compose up -d

# Volumes: ./data/Configs, ./data/Plugins, ./data/Logs
# Ports: 27260 (SignalR layer), 27360 (HTTP)
# Auto-updater is disabled in container mode
```
