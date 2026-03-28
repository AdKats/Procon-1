# PRoCon v2.0 Plugin SDK

## Quick Start

1. Copy `SdkTemplatePlugin.cs` to `Plugins/<GameType>/` (e.g., `Plugins/BF4/`)
2. Rename the file and class to your plugin name
3. Connect to a server — your plugin compiles and loads automatically
4. Open the Plugins tab to see it

That's it. Read on for details.

---

## Minimal Plugin

The smallest possible plugin:

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
            RegisterEvents(GetType().Name, "OnPlayerJoin", "OnGlobalChat");
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

Save as `MyPlugin.cs` in `Plugins/BF4/`, connect to a BF4 server, and it works.

---

## File Structure

### Single File (simple plugins)

```
Plugins/BF4/
  MyPlugin.cs      ← Everything in one file
```

### Multi-File with Partial Classes (large plugins)

```
Plugins/BF4/
  AdKats.cs                  ← Main file: metadata, lifecycle, variables
  AdKats.Commands.cs         ← Partial: command processing
  AdKats.Players.cs          ← Partial: player tracking
  AdKats.Bans.cs             ← Partial: ban management
  AdKats.Database.cs         ← Partial: MySQL operations
  AdKats.WebApi.cs           ← Partial: HTTP API calls
```

Rules:
- The **main file** name must match the class name exactly: `AdKats.cs` → `class AdKats`
- **Partial files** must be named `<ClassName>.<Anything>.cs`: `AdKats.Commands.cs`
- All files use `partial class` and the same `namespace PRoConEvents`
- All files share fields, methods, and properties — they're one class at compile time

**Main file:**
```csharp
namespace PRoConEvents
{
    public partial class AdKats : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool _isEnabled;

        public string GetPluginName() => "AdKats";
        // ... metadata, variables, lifecycle
    }
}
```

**Partial file (AdKats.Commands.cs):**
```csharp
namespace PRoConEvents
{
    public partial class AdKats
    {
        // Can access _isEnabled and all other fields from the main file
        private void HandleCommand(string speaker, string command)
        {
            if (!_isEnabled) return;
            // ...
        }
    }
}
```

### #include Directives (shared code across games)

```
Plugins/
  SharedUtils.inc           ← Shared across all game types
  BF4/
    MyPlugin.cs             ← Uses #include
  BF3/
    MyPlugin.cs             ← Can share the same .inc files
```

In your `.cs` file:
```csharp
// Include from parent Plugins/ directory
#include "../SharedUtils.inc"

// Include from current game directory
#include "MyPlugin.Helpers.inc"

// Game-type aware include
#include "%GameType%/GameSpecific.inc"
```

The `#include` directive pastes the file contents inline before compilation. Use `.inc` extension by convention. Nesting up to 5 levels deep.

---

## Plugin Variables

Variables let users configure your plugin from the UI.

```csharp
// Simple types
new CPluginVariable("Setting Name", typeof(string), "default value")
new CPluginVariable("Max Players", typeof(int), 32)
new CPluginVariable("Enabled", typeof(bool), true)

// Enum dropdown
new CPluginVariable("Mode", "enum.MyMode(Off|Warn|Kick|Ban)", "Warn")

// Categorized (Category|Name) — shown as groups in the UI
new CPluginVariable("Messages|Welcome Text", typeof(string), "Hello!")
new CPluginVariable("Messages|Goodbye Text", typeof(string), "Bye!")
new CPluginVariable("Limits|Max Warnings", typeof(int), 3)
```

Handle changes in `SetPluginVariable`:
```csharp
public void SetPluginVariable(string variable, string value)
{
    if (variable == "Max Players")
        int.TryParse(value, out _maxPlayers);
    else if (variable == "Enabled")
        bool.TryParse(value, out _isEnabled);
}
```

---

## Registering Events

You must register which events your plugin listens to. Unregistered events are never delivered.

```csharp
public void OnPluginLoaded(string host, string port, string version)
{
    RegisterEvents(GetType().Name,
        // Player events
        "OnPlayerJoin",
        "OnPlayerLeft",
        "OnPlayerKilled",
        "OnPlayerSpawned",
        "OnPlayerTeamChange",
        "OnPlayerSquadChange",

        // Chat
        "OnGlobalChat",
        "OnTeamChat",
        "OnSquadChat",

        // Server
        "OnServerInfo",
        "OnListPlayers",
        "OnRoundOver",
        "OnLevelLoaded",

        // Bans
        "OnBanAdded",
        "OnBanRemoved",
        "OnBanList",

        // PunkBuster
        "OnPunkbusterPlayerInfo",

        // Accounts
        "OnAccountLogin",
        "OnAccountLogout"
    );
}
```

---

## Common Event Overrides

```csharp
// Player joins the server
public override void OnPlayerJoin(string soldierName) { }

// Player leaves
public override void OnPlayerLeft(CPlayerInfo playerInfo) { }

// Player killed another player (or was killed by server)
public override void OnPlayerKilled(Kill kKillerVictimDetails)
{
    string killer = kKillerVictimDetails.Killer?.SoldierName ?? "Server";
    string victim = kKillerVictimDetails.Victim?.SoldierName ?? "Unknown";
    string weapon = kKillerVictimDetails.DamageType ?? "Unknown";
    bool headshot = kKillerVictimDetails.Headshot;
}

// Chat messages
public override void OnGlobalChat(string speaker, string message) { }
public override void OnTeamChat(string speaker, string message, int teamId) { }
public override void OnSquadChat(string speaker, string message, int teamId, int squadId) { }

// Server info (sent periodically)
public override void OnServerInfo(CServerInfo serverInfo)
{
    string name = serverInfo.ServerName;
    int players = serverInfo.PlayerCount;
    int max = serverInfo.MaxPlayerCount;
    string map = serverInfo.Map;
    string mode = serverInfo.GameMode;
}

// Player list (sent periodically)
public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
{
    foreach (CPlayerInfo p in players)
    {
        string name = p.SoldierName;
        int kills = p.Kills;
        int deaths = p.Deaths;
        int score = p.Score;
        int team = p.TeamID;
        int squad = p.SquadID;
    }
}

// Round ended
public override void OnRoundOver(int winningTeamId) { }

// New map loaded
public override void OnLevelLoaded(string mapFileName, string gamemode,
    int roundsPlayed, int roundsTotal) { }

// Ban was added
public override void OnBanAdded(CBanInfo ban)
{
    string name = ban.SoldierName;
    string guid = ban.Guid;
    string ip = ban.IpAddress;
    string reason = ban.Reason;
}
```

---

## Sending Commands

```csharp
// Say to one player
ExecuteCommand("procon.protected.send", "admin.say", "Hello!", "player", soldierName);

// Say to all
ExecuteCommand("procon.protected.send", "admin.say", "Hello everyone!", "all");

// Yell to player (5 seconds)
ExecuteCommand("procon.protected.send", "admin.yell", "WARNING!", "5", "player", soldierName);

// Kick player
ExecuteCommand("procon.protected.send", "admin.kickPlayer", soldierName, "Reason here");

// Kill player
ExecuteCommand("procon.protected.send", "admin.killPlayer", soldierName);

// Move player to team 2, squad 0
ExecuteCommand("procon.protected.send", "admin.movePlayer", soldierName, "2", "0", "true");

// Ban by name (permanent)
ExecuteCommand("procon.protected.send", "banList.add", "name", soldierName, "perm", "Ban reason");

// Ban by GUID (temporary, 1 hour = 3600 seconds)
ExecuteCommand("procon.protected.send", "banList.add", "guid", playerGuid, "seconds", "3600", "Temp ban");

// Write to plugin console
ExecuteCommand("procon.protected.pluginconsole.write", "My log message here");
```

---

## Database Access (MySqlConnector)

```csharp
using MySqlConnector;

private void QueryDatabase()
{
    string connStr = "Server=localhost;Database=procon;User=root;Password=pass;";

    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();

        // INSERT
        using (var cmd = new MySqlCommand(
            "INSERT INTO kills (killer, victim, weapon) VALUES (@k, @v, @w)", conn))
        {
            cmd.Parameters.AddWithValue("@k", "PlayerA");
            cmd.Parameters.AddWithValue("@v", "PlayerB");
            cmd.Parameters.AddWithValue("@w", "M16A4");
            cmd.ExecuteNonQuery();
        }

        // SELECT
        using (var cmd = new MySqlCommand(
            "SELECT kills, deaths FROM stats WHERE name = @name", conn))
        {
            cmd.Parameters.AddWithValue("@name", "PlayerA");
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int kills = reader.GetInt32("kills");
                    int deaths = reader.GetInt32("deaths");
                }
            }
        }
    }
}
```

---

## HTTP Requests

```csharp
using System.Net.Http;

private void FetchFromApi()
{
    var client = new HttpClient();
    // Synchronous (use within plugin timeout limits)
    string json = client.GetStringAsync("https://api.example.com/data").Result;

    // Parse with Newtonsoft.Json
    var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
    string value = obj["key"]?.ToString();
}
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Plugin doesn't appear | Check `Plugins/<GameType>/` has your `.cs` file. Delete `PluginCache.xml` and reconnect. |
| Compile errors | Check Plugin Console tab for errors. Missing `using` directive? Missing assembly? |
| `BadImageFormatException` | Delete the `.dll` file in the plugin directory and reconnect to recompile. |
| `InvalidCastException` on load | Plugin was compiled against a different PRoCon.Core. Delete all `.dll` files and `PluginCache.xml`. |
| Variables not saving | Make sure `GetPluginVariables()` returns the same variable names as `SetPluginVariable()` handles. |
| Events not firing | Check `RegisterEvents()` includes the event name. Must be called in `OnPluginLoaded`. |
| Partial class not found | File must be named `<ClassName>.<Something>.cs` and use `partial class` in the same namespace. |
