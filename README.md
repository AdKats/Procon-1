# PRoCon v2.0

> **⚠️ NOT READY FOR USE — This branch contains an in-progress rewrite of PRoCon. It is not yet stable or feature-complete. Do not use in production.**

PRoCon is a free, open-source remote control (RCON) tool for Frostbite game servers. It supports Battlefield Bad Company 2, Battlefield 3, Battlefield 4, Battlefield Hardline, and Medal of Honor: Warfighter.

## Migrating from v1.x

PRoCon v2.0 can import your existing v1.x configuration, servers, accounts, and plugins automatically.

### Step 1: Find your data directory

| Platform | v2.0 Data Directory |
|----------|-------------------|
| Windows | `%APPDATA%\PRoCon\` |
| Linux | `~/.config/procon/` |
| Docker | `/config/` |

Run PRoCon v2.0 once to create the directory structure, then close it.

### Step 2: Copy your v1 files

Create an `Import/` folder inside the data directory and copy your old PRoCon files into it:

```
~/.config/procon/Import/          (or %APPDATA%\PRoCon\Import\)
  Configs/
    procon.cfg                    <- your servers, options, layer settings
    accounts.cfg                  <- your layer accounts
    1.2.3.4_47200/                <- per-server plugin configs
    5.6.7.8_47300/
```

You can also just copy your entire old PRoCon `Configs/` folder into `Import/`.

**Note:** v1 plugin source files (`.cs`) are **not** imported — they need to be updated for v2. Download v2-compatible plugins from the release announcement.

### Step 3: Launch PRoCon v2.0

On startup, PRoCon will:
1. Detect the `Import/` folder
2. Import your servers, accounts, options, and per-server plugin configs
3. Save everything as encrypted `procon.json` (passwords are AES-256 encrypted)
4. Rename `Import/` to `Import.done/` so it won't re-import

Your old `procon.cfg` and `accounts.cfg` are archived as `.v1.bak` files.

### Breaking changes to check

- **Plugins using `System.Windows.Forms`** need refactoring (see [`docs/PLUGIN-REFACTORING-GUIDE.md`](docs/PLUGIN-REFACTORING-GUIDE.md))
- **`using MySql.Data.MySqlClient;`** is auto-rewritten to `using MySqlConnector;`
- **`OnHttpRequest`** handler must be removed from plugins (HTTP server is gone)
- **Layer protocol changed** — v1.x and v2.0 instances cannot cross-connect. Upgrade all at once.

---

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
| Config Format | `procon.cfg` (plaintext) | `procon.json` (AES-256 encrypted passwords) |
| IP Checking | None | ProxyCheck.io v3 (SQLite cache) |
| Distribution | 50+ loose DLLs | Single-file executable (~77MB) |

## Download

Download the latest release from the [Releases](https://github.com/AdKats/Procon-1/releases) page.

| Platform | File |
|----------|------|
| Windows GUI | `PRoCon.UI.exe` (self-contained, no .NET install needed) |
| Linux GUI | `PRoCon.UI` (self-contained) |
| Windows Headless | `PRoCon.Console.exe` (for servers, no GUI) |
| Linux Headless | `PRoCon.Console` (for servers/Docker) |

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
```

### Docker

```bash
docker compose up -d
# Data volume: ./data/ → /config/ (Configs, Plugins, Logs, Cache)
```

### Headless (CLI)

```bash
./PRoCon.Console                              # uses default data directory
./PRoCon.Console --datadir /opt/procon/data   # custom data path
```

## Data Directory

PRoCon stores all user data in a platform-appropriate location, separate from the executable:

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\PRoCon\` |
| Linux | `~/.config/procon/` |
| macOS | `~/Library/Application Support/PRoCon/` |
| Docker/K8s | `/config/` (auto-detected) |
| Portable | Exe directory (if `Configs/` exists next to exe) |

Override with `PROCON_DATA_DIR` environment variable or `--datadir` CLI argument.

Directory structure (created on first launch):
```
Configs/
  procon.json          <- servers, accounts, options (encrypted passwords)
  .procon-key          <- AES-256 encryption key (auto-generated)
Plugins/
  BF3/ BF4/ BFBC2/ BFHL/ MOH/ MOHW/
Logs/
Cache/
  IPCheck/ipcache.db   <- ProxyCheck.io cache (SQLite)
```

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

## Architecture

| Project | Purpose |
|---------|---------|
| `PRoCon.UI` | Avalonia GUI application |
| `PRoCon.Core` | Core business logic, plugin system, RCON protocol |
| `PRoCon.Themes` | Dark/light theme resources |
| `PRoCon.Console` | Headless console application |
| `PRoCon.Service` | Windows Service / Linux systemd wrapper |

Key technologies: .NET 8, Avalonia 11, SignalR (layer system), Roslyn (plugin compilation), Dapper + SQLite (caching), Kestrel (layer hosting).

See [`docs/CHANGELOG-v2.md`](docs/CHANGELOG-v2.md) for the full changelog and [`docs/PLUGIN-REFACTORING-GUIDE.md`](docs/PLUGIN-REFACTORING-GUIDE.md) for plugin migration steps.

## License

PRoCon is licensed under the [GPLv3](LICENSE).

## Credits

Originally developed by Phogue and the Myrcon community. v2.0 modernization by Prophet / EZSCALE.

The Battlefield franchise is a product of [DICE](https://dice.se).
