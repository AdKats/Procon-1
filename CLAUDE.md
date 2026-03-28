# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Procon 1 (PRoCon Frostbite) is a free, open-source remote control (RCON) tool for managing game servers. It supports Battlefield (BFBC2, BF3, BF4, Hardline) and Medal of Honor: Warfighter through the Frostbite engine.

**Two branches:**
- `master` — v1.x stable, .NET Framework 4.7, WinForms
- `feature/modernization` — v2.0 in progress, .NET 8, Avalonia UI (worktree at `~/.config/superpowers/worktrees/Procon-1/modernize/`)

## Build Commands

### v1.x (master branch)

```bash
nuget restore src/PRoCon-VS2008E.sln
msbuild src/PRoCon-VS2008E.sln /p:Configuration=Release /p:Platform="Any CPU" /m
```

### v2.0 (feature/modernization branch)

```bash
# Build (from worktree root)
~/.dotnet/dotnet build src/PRoCon.UI/PRoCon.UI.csproj

# Run on Linux
DISPLAY=:0 ~/.dotnet/dotnet run --project src/PRoCon.UI/PRoCon.UI.csproj

# Publish self-contained
~/.dotnet/dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r linux-x64 --self-contained -o publish/linux-ui/
```

Build output goes to `builds/Release/` or `builds/Debug/`.

## Running

```bash
# v1.x GUI
./builds/Release/PRoCon.exe

# v1.x Headless/console mode (for servers/Docker)
./builds/Release/PRoCon.Console.exe -console 1

# v2.0 on Linux (run .exe directly, not via cmd.exe)
~/.dotnet/dotnet run --project src/PRoCon.UI/PRoCon.UI.csproj
```

Linux .NET SDK path: `~/.dotnet/dotnet`

## Testing

There is no automated test suite. Testing is manual against live game servers.

## CI/CD

GitHub Actions workflow (`.github/workflows/build-and-release.yml`):
- **PR to master**: Builds solution to verify compilation
- **Tag push (`v*`)**: Builds, creates ZIP archive with checksums, publishes GitHub Release
- Version is extracted from git tag and stamped into `src/VersionInfo.cs` during CI

## Architecture

### v2.0 Solution Structure

| Project | Purpose | Framework |
|---------|---------|-----------|
| `PRoCon.UI` | Avalonia GUI application | net8.0 |
| `PRoCon.Core` | Core business logic library | net8.0 |
| `PRoCon.Themes` | Dark/Light theme resources | net8.0 |
| `PRoCon.Console` | Headless console application | net8.0 |
| `PRoCon.Service` | Windows Service wrapper | net8.0 |

### Key Architectural Layers

**Network/Protocol**: `PRoCon.Core/Remote/FrostbiteConnection.cs` handles raw TCP communication using the Frostbite binary RCON protocol with packet sequence correlation. Game-specific clients (`BF3Client.cs`, `BF4Client.cs`, etc.) extend the base protocol with game-specific commands. Packet cache with `Invalidate(Regex)` for stale data.

**Client Orchestration**: `PRoCon.Core/Remote/PRoConClient.cs` is the main client class that manages authentication, player lists, server state, and fires 100+ event types consumed by plugins and the UI.

**Plugin System**: Plugins are C# source files compiled at runtime using Roslyn (`Microsoft.CodeAnalysis.CSharp 4.8.0`). The API is defined through `IPRoConPluginInterface`. `PluginManager` handles lifecycle with 59-second execution timeout. Plugins load into a collectible `AssemblyLoadContext`. Shared assemblies (PRoCon.Core, Newtonsoft.Json, MySqlConnector) must load from the host context to avoid type identity issues.

**Layer System**: v2.0 uses SignalR WebSocket (`LayerHostService` + `LayerHub`) replacing the old TCP binary layer protocol. Kestrel hosts at `/layer` endpoint. `LayerHostService` implements `ILayerInstance` as an adapter.

**UI**: Avalonia 11.2.3 with 16 tabbed panels. Models extracted to `PRoCon.UI/Models/` (ServerEntry, PlayerDisplayInfo, RconCommandDefs). Controls cached via `CacheControls()` to avoid repeated `FindControl` lookups.

### Naming Conventions

- **C-prefix**: Core data structures (`CBanInfo`, `CMap`, `CPlayerInfo`, `CServerInfo`)
- **I-prefix**: Interfaces (`IPRoConPluginInterface`)

### Key Directories in `src/PRoCon.Core/`

- `Remote/` - Game server connection, protocol handling, packet caching
- `Layer/` - SignalR layer system (LayerHostService, LayerHub, LayerHubClient)
- `Plugin/` - Plugin API, manager, PluginLoadContext, command system
- `Accounts/` - Account management and privilege system (`CPrivileges`)
- `Consoles/` - Chat, connection, plugin, and PunkBuster console handlers
- `Players/` - Player data structures and management
- `Network/` - IPCheckService (ProxyCheck.io v3 integration)
- `Battlemap/` - Map geometry and zone system
- `Localization/` - i18n support (extracted from embedded resources)

### Key Directories in `src/PRoCon.UI/`

- `Views/` - All Avalonia panels (MainWindow, PluginsPanel, BanListPanel, etc.)
- `Models/` - ServerEntry, PlayerDisplayInfo, RconCommandDefs
- `Services/` - ConsoleFileLogger

### Game Definitions

Game protocol commands are defined in `.def` files under `src/Resources/Configs/` (e.g., `BF4.def`, `BF3.def`). Default plugins are embedded resources under `src/Resources/DefaultPlugins/`.

## Key Dependencies (v2.0)

- **Avalonia 11.2.3** - Cross-platform UI framework
- **Newtonsoft.Json 13.0.3** - JSON serialization
- **Microsoft.CodeAnalysis.CSharp 4.8.0** - Roslyn compiler for runtime plugin compilation
- **MySqlConnector 2.4.0** - MySQL/MariaDB connectivity (replaced MySql.Data)
- **Microsoft.AspNetCore.SignalR** - Layer system WebSocket transport
- **MaxMind.GeoIP2 5.2.0** - IP geolocation
- **SharpZipLib 1.4.2** - ZIP handling (replaced Ionic.Zip)

## Plugin Development (v2.0)

- SDK template: `src/Resources/DefaultPlugins/BF4/SdkTemplatePlugin.cs`
- Refactoring guide: `docs/PLUGIN-REFACTORING-GUIDE.md`
- Plugins use `namespace PRoConEvents` and extend `PRoConPluginAPI`
- Place `.cs` files in `Plugins/<GameType>/` — compiled automatically on connect
- `#include` directives supported for shared code (`.inc` files)
- HttpServer removed — no `OnHttpRequest`, no `HttpWebServerRequestData`/`ResponseData`
- `MySql.Data.MySqlClient` replaced by `MySqlConnector` namespace
- `System.Windows.Forms` not available — plugins must be cross-platform
