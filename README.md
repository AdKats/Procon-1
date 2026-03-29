# PRoCon v2.0

PRoCon is a free, open-source remote control (RCON) tool for Frostbite game servers. It supports Battlefield Bad Company 2, Battlefield 3, Battlefield 4, Battlefield Hardline, and Medal of Honor: Warfighter.

## What's v2.0?

EZSCALE needed to upgrade their MySQL infrastructure and the legacy PRoCon codebase (.NET Framework 4.7) was blocking that. Prophet took the time to modernize the entire stack to .NET 8.

**This is an infrastructure update, not a project revival.** For the future of game server management across multiple titles, check out [metabans.com](https://metabans.com).

### What Changed

| Area | v1.x | v2.0 |
|------|------|------|
| Runtime | .NET Framework 4.7 (Windows only) | .NET 8 (Windows, Linux, macOS) |
| UI | WinForms | Avalonia UI (dark/light themes) |
| Layer Protocol | Custom TCP binary | SignalR WebSocket |
| MySQL Driver | MySql.Data | MySqlConnector |
| Plugin Compiler | CodeDom | Roslyn (C# latest) |
| Config Format | `procon.cfg` (command-based) | `procon.json` (with .cfg fallback) |
| IP Checking | None | ProxyCheck.io v3 (SQLite cache) |
| Distribution | 50+ loose DLLs | Single-file executable (~77MB) |

## Download

Download the latest release from the [Releases](https://github.com/AdKats/Procon-1/releases) page.

| Platform | File |
|----------|------|
| Windows | `PRoCon.UI.exe` (self-contained, no .NET install needed) |
| Linux | `PRoCon.UI` (self-contained) |

## Building from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Clone and setup
git clone https://github.com/AdKats/Procon-1.git
cd Procon-1
git config core.hooksPath .githooks    # enable code formatting hook

# Build
dotnet build src/PRoCon.UI/PRoCon.UI.csproj

# Run
dotnet run --project src/PRoCon.UI/PRoCon.UI.csproj

# Publish single-file executables
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r win-x64 --self-contained -o publish/win
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r linux-x64 --self-contained -o publish/linux
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r osx-x64 --self-contained -o publish/osx
```

### Docker

```bash
docker compose up -d
# Data: /config/ (Configs, Plugins, Logs, Cache)
```

## Data Directory

PRoCon stores all user data in a platform-appropriate location:

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\PRoCon\` |
| Linux | `~/.config/procon/` |
| macOS | `~/Library/Application Support/PRoCon/` |
| Docker/K8s | `/config/` (auto-detected) |

Override with `PROCON_DATA_DIR` environment variable or `--datadir` CLI argument. If a `Configs/` folder exists next to the executable, PRoCon uses portable mode (data stored next to the exe).

## Plugin Development

Plugins are `.cs` source files compiled at runtime with Roslyn. Place them in `Plugins/<GameType>/` and they load automatically when connecting to a server.

### Quick Start

```csharp
using System;
using System.Collections.Generic;
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Players;

namespace PRoConEvents
{
    public class MyPlugin : PRoConPluginAPI, IPRoConPluginInterface
    {
        public string GetPluginName() => "My Plugin";
        public string GetPluginVersion() => "1.0.0";
        public string GetPluginAuthor() => "YourName";
        public string GetPluginWebsite() => "";
        public string GetPluginDescription() => "Does something cool.";

        public List<CPluginVariable> GetDisplayPluginVariables() => new List<CPluginVariable>();
        public List<CPluginVariable> GetPluginVariables() => new List<CPluginVariable>();
        public void SetPluginVariable(string variable, string value) { }

        public void OnPluginLoaded(string host, string port, string version)
        {
            RegisterEvents(GetType().Name, "OnPlayerJoin");
        }

        public void OnPluginEnable() { }
        public void OnPluginDisable() { }

        public override void OnPlayerJoin(string soldierName)
        {
            ExecuteCommand("procon.protected.send", "admin.say",
                "Welcome " + soldierName + "!", "player", soldierName);
        }
    }
}
```

### Available Libraries

| Library | Import | Use |
|---------|--------|-----|
| **MySqlConnector** | `using MySqlConnector;` | MySQL/MariaDB database access |
| **Dapper** | `using Dapper;` | Micro-ORM (automatic object mapping) |
| **Flurl** | `using Flurl.Http;` | Fluent HTTP client |
| **Newtonsoft.Json** | `using Newtonsoft.Json;` | JSON serialization |
| **Microsoft.Data.Sqlite** | `using Microsoft.Data.Sqlite;` | SQLite local databases |

### Multi-File Plugins

Large plugins can be split using partial classes. Two layouts supported:

```
# Flat layout (files named ClassName.Part.cs)
Plugins/BF4/
  AdKats.cs                  <- Main file
  AdKats.Commands.cs         <- Partial class files
  AdKats.Database.cs

# Subfolder layout (recommended for large plugins)
Plugins/BF4/
  AdKats.cs                  <- Main file stays at top level
  AdKats/                    <- Subfolder with same name as class
    Commands.cs              <- All .cs files compiled together
    Database.cs
    Players.cs
```

### IP Reputation Checking

Plugins can check player IPs for VPN/proxy usage:

```csharp
// Request a lookup
ExecuteCommand("procon.protected.ipcheck", playerIP);

// Receive the result (register for "OnIPChecked" event)
public override void OnIPChecked(string ip, string countryName,
    string countryCode, string city, string provider,
    bool isVPN, bool isProxy, bool isTor, int risk) { }
```

See the full SDK template and developer guide in [`pluginsdk/`](pluginsdk/).

## Breaking Changes from v1.x

- **.NET 8 required** — .NET Framework 4.7 no longer supported
- **HTTP web server removed** — use the SignalR layer instead
- **Layer protocol changed** — v1.x and v2.0 cannot cross-connect
- **Plugin sandbox removed** — plugins run with full trust
- **Auto-updater removed** — download updates from GitHub Releases
- **Default plugins not bundled** — install plugins manually
- **MySql.Data replaced** — use `using MySqlConnector;` instead of `using MySql.Data.MySqlClient;`
- **System.Windows.Forms removed** — plugins must be cross-platform
- **Config format changed** — new installs use `procon.json`, legacy `procon.cfg` still loaded

See [`docs/CHANGELOG-v2.md`](docs/CHANGELOG-v2.md) for the full changelog and [`docs/PLUGIN-REFACTORING-GUIDE.md`](docs/PLUGIN-REFACTORING-GUIDE.md) for plugin migration steps.

## Architecture

| Project | Purpose |
|---------|---------|
| `PRoCon.UI` | Avalonia GUI application |
| `PRoCon.Core` | Core business logic, plugin system, RCON protocol |
| `PRoCon.Themes` | Dark/light theme resources |
| `PRoCon.Console` | Headless console application |
| `PRoCon.Service` | Windows Service / Linux systemd wrapper |

Key technologies: .NET 8, Avalonia 11, SignalR (layer system), Roslyn (plugin compilation), Dapper + SQLite (caching), Kestrel (layer hosting).

## License

PRoCon is licensed under the [GPLv3](LICENSE).

## Credits

Originally developed by Phogue and the Myrcon community. v2.0 modernization by Prophet / EZSCALE.

The Battlefield franchise is a product of [DICE](https://dice.se).
