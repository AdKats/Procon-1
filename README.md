# PRoCon v2.0

PRoCon is a free, open-source remote control (RCON) tool for Frostbite game servers. It supports Battlefield Bad Company 2, Battlefield 3, Battlefield 4, Battlefield Hardline, and Medal of Honor: Warfighter.

## What's v2.0?

EZSCALE needed to upgrade their MySQL infrastructure and the legacy PRoCon codebase (.NET Framework 4.7) was blocking that. Prophet took the time to modernize the entire stack to .NET 8.

**This is an infrastructure update, not a project revival.** For the future of game server management across multiple titles, check out [metabans.com](https://metabans.com).

### What Changed

- **.NET 8** — cross-platform runtime (Windows, Linux, macOS natively)
- **Avalonia UI** — replaces WinForms with a modern cross-platform UI (dark/light themes)
- **SignalR Layer** — replaces the custom TCP binary protocol with WebSocket
- **MySqlConnector** — replaces MySql.Data for modern MySQL/MariaDB support
- **Roslyn Compiler** — plugins compile with C# latest features
- **Single-file publish** — one executable per platform, no loose DLLs
- **Plugin SDK** — Dapper (ORM), Flurl (HTTP client), multi-file plugin support

## Download

Download the latest release from the [Releases](https://github.com/AdKats/Procon-1/releases) page.

| Platform | File |
|----------|------|
| Windows | `PRoCon.UI.exe` (self-contained, no .NET install needed) |
| Linux | `PRoCon.UI` (self-contained) |

## Building from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Clone
git clone https://github.com/AdKats/Procon-1.git
cd Procon-1
# Build
dotnet build src/PRoCon.UI/PRoCon.UI.csproj

# Run
dotnet run --project src/PRoCon.UI/PRoCon.UI.csproj

# Publish single-file executables
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r win-x64 -o publish/win
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r linux-x64 -o publish/linux
dotnet publish src/PRoCon.UI/PRoCon.UI.csproj -c Release -r osx-x64 -o publish/osx
```

## Plugin Development

PRoCon plugins are `.cs` source files compiled at runtime. Place them in `Plugins/<GameType>/` and they load automatically when connecting to a server.

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

Plugins have access to these libraries at compile time:

| Library | Import | Use |
|---------|--------|-----|
| **MySqlConnector** | `using MySqlConnector;` | Raw SQL database access |
| **Dapper** | `using Dapper;` | Micro-ORM (automatic object mapping) |
| **Flurl** | `using Flurl.Http;` | Fluent HTTP client (like axios) |
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

See the full SDK template and developer guide in the [`pluginsdk/`](pluginsdk/) directory.

## Breaking Changes from v1.x

- **.NET 8 required** — .NET Framework 4.7 no longer supported
- **HTTP web server removed** — use the SignalR layer instead
- **Layer protocol changed** — v1.x and v2.0 cannot cross-connect
- **Plugin sandbox removed** — plugins run with full trust
- **MySql.Data replaced** — use `using MySqlConnector;` instead of `using MySql.Data.MySqlClient;`
- **System.Windows.Forms removed** — plugins must be cross-platform

See [`docs/CHANGELOG-v2.md`](docs/CHANGELOG-v2.md) for the full changelog and [`docs/PLUGIN-REFACTORING-GUIDE.md`](docs/PLUGIN-REFACTORING-GUIDE.md) for plugin migration steps.

## Architecture

| Project | Purpose |
|---------|---------|
| `PRoCon.UI` | Avalonia GUI application |
| `PRoCon.Core` | Core business logic, plugin system, RCON protocol |
| `PRoCon.Themes` | Dark/light theme resources |
| `PRoCon.Console` | Headless console application |

Key technologies: Avalonia 11, SignalR (layer system), Roslyn (plugin compilation), Kestrel (layer hosting).

## License

PRoCon is licensed under the [GPLv3](LICENSE).

## Credits

Originally developed by Phogue and the Myrcon community. v2.0 modernization by Prophet / EZSCALE.

The Battlefield franchise is a product of [DICE](https://dice.se).
