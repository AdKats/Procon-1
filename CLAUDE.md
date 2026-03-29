# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PRoCon is an open-source RCON tool for Frostbite game servers (BF3, BF4, BFBC2, BFH, MOHW). v2.0 is a full rewrite from .NET Framework 4.7/WinForms to .NET 8/Avalonia UI. The v1.x codebase is preserved on the `v1-legacy` branch.

**Context**: EZSCALE needed to upgrade MySQL infrastructure and legacy PRoCon was blocking that. This is an infrastructure update, not a project revival. The successor platform is [metabans.com](https://metabans.com).

## Build & Run

```bash
# Build
dotnet build src/PRoCon.UI/PRoCon.UI.csproj

# Run (Linux — use ~/.dotnet/dotnet if dotnet isn't in PATH)
DISPLAY=:0 dotnet run --project src/PRoCon.UI/PRoCon.UI.csproj

# Publish single-file self-contained executables
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r linux-x64 --self-contained -o publish/linux/
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r win-x64 --self-contained -o publish/win/

# Headless / Docker
dotnet run --project src/PRoCon.Console/PRoCon.Console.csproj -- -console 1
docker compose up -d
```

Linux .NET SDK path: `~/.dotnet/dotnet`

Build output: `builds/Release/` or `builds/Debug/`. Publish creates a single executable per platform (~77MB).

**No automated test suite.** Testing is manual against live game servers.

## Solution Structure (`src/PRoCon.sln`)

| Project | Purpose |
|---------|---------|
| `PRoCon.Core` | Core business logic, protocol, plugins, config |
| `PRoCon.UI` | Avalonia GUI (16 tabbed panels) |
| `PRoCon.Console` | Headless console app |
| `PRoCon.Service` | Windows Service / Linux systemd wrapper |
| `PRoCon.Themes` | Dark/Light theme ResourceDictionaries |

All executables depend on `PRoCon.Core`. `PRoCon.UI` depends on `PRoCon.Themes`.

## Data Directory (ProConPaths)

All user data lives in a centralized location, **not** next to the exe:

| Platform | Default Path |
|----------|-------------|
| Windows | `%APPDATA%\PRoCon\` |
| Linux | `~/.config/procon/` |
| macOS | `~/Library/Application Support/PRoCon/` |
| Docker/K8s | `/config/` (auto-detected) |
| Portable | Exe directory (if `Configs/` exists next to exe) |

Override: `PROCON_DATA_DIR` env var or `--datadir` CLI arg.

Structure: `Configs/`, `Plugins/`, `Logs/`, `Cache/`, `Localization/`, `Media/`

Implementation: `src/PRoCon.Core/ProConPaths.cs` — all code uses `ProConPaths.ConfigsDirectory`, `ProConPaths.PluginsDirectory`, etc. Assembly/binary paths use `ProConPaths.ApplicationDirectory`.

## Architecture

### Network/Protocol
`Remote/FrostbiteConnection.cs` — async TCP with Frostbite binary RCON protocol, optional TLS, packet sequence correlation. Game clients (`BF3Client.cs`, `BF4Client.cs`, etc.) extend base protocol. Packet cache with `Invalidate(Regex)`.

### Client Orchestration
`Remote/PRoConClient.cs` — manages auth, player lists, server state, fires 100+ events consumed by plugins and UI.

### Plugin System
- Plugins are `.cs` files compiled at runtime with Roslyn (`Microsoft.CodeAnalysis.CSharp 4.8.0`)
- Loaded via `PluginLoadContext` (collectible `AssemblyLoadContext`) with shared assembly blocklist to prevent type identity issues
- `PluginManager.cs` handles lifecycle: extract defaults → compile → load → wire events
- Multi-file plugins via partial classes (`MyPlugin.cs`, `MyPlugin.Commands.cs`) and `#include` directives
- 59-second execution timeout per invocation
- Plugin SDK: `pluginsdk/` directory with templates and guide
- Available to plugins: PRoCon.Core, Newtonsoft.Json, MySqlConnector, Dapper, Flurl.Http, Microsoft.Data.Sqlite

### Config System
- **v2 default**: `procon.json` (typed model in `Options/ProConConfig.cs`)
- **Legacy fallback**: `procon.cfg` (command-based format, still loaded for backward compat)
- Startup checks for `procon.json` first, falls back to `procon.cfg`
- Saves always write both formats

### Layer System
SignalR WebSocket at `/layer` endpoint (`Layer/LayerHostService.cs` + `Layer/LayerHub.cs`). JWT auth. Replaces old TCP binary layer protocol (still in `Remote/Layer/` for reference).

### IP Check Service
`Network/IPCheckService.cs` — ProxyCheck.io v3 integration with SQLite cache (Dapper ORM), memory cache, rate limiting, daily query budgeting. Database: `Cache/IPCheck/ipcache.db` with WAL mode.

### Theme Engine
`PRoCon.Themes` — Dark/Light Avalonia ResourceDictionaries. `ThemeManager` handles runtime switching.

## Key Directories in `src/PRoCon.Core/`

- `Remote/` — Game server connection, protocol, packet caching
- `Layer/` — SignalR layer (LayerHostService, LayerHub, JWT auth)
- `Remote/Layer/` — Legacy TCP layer (reference only)
- `Plugin/` — Plugin API, PluginLoadContext, Roslyn compilation, command system
- `Options/` — OptionsSettings, ProConConfig (JSON model), TrustedHosts, StatsLinks
- `Network/` — IPCheckService (ProxyCheck.io + SQLite)
- `Config/` — Legacy .cfg config parser
- `Consoles/` — Chat, connection, plugin, PunkBuster console handlers
- `Accounts/` — Account management and privilege system (`CPrivileges`)
- `Battlemap/` — Map geometry and zone system
- `Localization/` — i18n (extracted from embedded resources at startup)

## Key Directories in `src/PRoCon.UI/`

- `Views/` — 16 Avalonia panels (MainWindow, PluginsPanel, BanListPanel, etc.)
- `Models/` — ServerEntry, PlayerDisplayInfo, RconCommandDefs
- `Services/` — ConsoleFileLogger

## Naming Conventions

- **C-prefix**: Core data structures (`CBanInfo`, `CMap`, `CPlayerInfo`, `CServerInfo`)
- **I-prefix**: Interfaces (`IPRoConPluginInterface`)
- **usc-prefix**: Legacy control names still in some types
- Game protocol commands defined in `.def` files under `src/Resources/Configs/`

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Avalonia | 11.2.3 | Cross-platform UI |
| Newtonsoft.Json | 13.0.3 | JSON (also plugin API) |
| Microsoft.CodeAnalysis.CSharp | 4.8.0 | Roslyn plugin compilation |
| MySqlConnector | 2.4.0 | MySQL/MariaDB (also plugin API) |
| Dapper | 2.1.72 | Micro-ORM (also plugin API) |
| Flurl.Http | 4.0.2 | HTTP client (also plugin API) |
| Microsoft.Data.Sqlite | 8.0.11 | SQLite (also plugin API) |
| MaxMind.GeoIP2 | 5.2.0 | IP geolocation |
| SharpZipLib | 1.4.2 | ZIP handling |
| Microsoft.AspNetCore.SignalR | (framework) | Layer WebSocket |

## Plugin Development

- SDK template and guide: `pluginsdk/`
- Refactoring guide (v1→v2): `docs/PLUGIN-REFACTORING-GUIDE.md`
- Plugins use `namespace PRoConEvents` and extend `PRoConPluginAPI`
- Place `.cs` files in `Plugins/<GameType>/` — compiled automatically on connect
- `#include` directives supported for shared code (`.inc` files in parent `Plugins/`)
- HttpServer removed — no `OnHttpRequest`
- `MySql.Data.MySqlClient` replaced by `MySqlConnector`
- `System.Windows.Forms` not available — plugins must be cross-platform

## CI/CD

GitHub Actions (`.github/workflows/build-and-release.yml`):
- **PR to master**: Build verification
- **Tag push (`v*`)**: Build, create ZIP with checksums, publish GitHub Release
- Version stamped from git tag into `src/VersionInfo.cs`

## Publish Configuration

All projects use `DebugType=embedded` (no loose `.pdb` files). `PRoCon.UI.csproj` has:
- `PublishSingleFile=true`
- `SelfContained=true`
- `IncludeNativeLibrariesForSelfExtract=true`
- `EnableCompressionInSingleFile=true`

## Git Branches

- `master` — v2.0 (.NET 8, Avalonia UI)
- `v1-legacy` — v1.x stable (.NET Framework 4.7, WinForms) — frozen, no new development
