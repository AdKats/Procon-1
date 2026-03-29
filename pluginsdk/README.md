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

Two layouts are supported — use whichever fits your plugin:

**Option A: Flat layout** (files named `ClassName.Part.cs`)

```
Plugins/BF4/
  AdKats.cs                  ← Main file: metadata, lifecycle, variables
  AdKats.Commands.cs         ← Partial: command processing
  AdKats.Players.cs          ← Partial: player tracking
  AdKats.Database.cs         ← Partial: MySQL operations
```

**Option B: Subfolder layout** (recommended for large plugins)

```
Plugins/BF4/
  AdKats.cs                  ← Main file stays at the top level
  AdKats/                    ← Subfolder matches the class name
    Commands.cs              ← Any .cs file here is compiled with the main file
    Players.cs
    Database.cs
    WebApi.cs
    Utils/                   ← Nested subdirectories work too
      Helpers.cs
```

Both layouts can coexist — you can even have `AdKats.Legacy.cs` in BF4/ alongside an `AdKats/` subfolder. All files are compiled together.

Rules:
- The **main file** name must match the class name exactly: `AdKats.cs` → `class AdKats`
- **Flat partials** must be named `<ClassName>.<Anything>.cs`: `AdKats.Commands.cs`
- **Subfolder files** can be named anything — just put them in a folder matching the class name
- All files use `partial class` and the same `namespace PRoConEvents`
- All files share fields, methods, and properties — they're one class at compile time

**Main file (AdKats.cs):**
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

**Subfolder file (AdKats/Commands.cs):**
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

## Database Access

Two options are available — use whichever fits your needs:

### Option 1: Raw SQL (MySqlConnector)

Full control, explicit parameter binding, manual result reading.

```csharp
using MySqlConnector;

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
```

### Option 2: Dapper (Micro-ORM)

Less boilerplate — maps query results to C# objects automatically.

```csharp
using MySqlConnector;
using Dapper;

// Define a class matching your table columns
public class PlayerStats
{
    public string Name { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
}

using (var conn = new MySqlConnection(connStr))
{
    conn.Open();

    // SELECT single row → object (or null)
    var stats = conn.QueryFirstOrDefault<PlayerStats>(
        "SELECT name, kills, deaths FROM player_stats WHERE name = @Name",
        new { Name = "PlayerA" });

    // SELECT multiple rows → List<T>
    var topPlayers = conn.Query<PlayerStats>(
        "SELECT name, kills, deaths FROM player_stats ORDER BY kills DESC LIMIT 10"
    ).ToList();

    // INSERT / UPDATE / DELETE
    conn.Execute(
        "INSERT INTO player_stats (name, kills, deaths) VALUES (@Name, @Kills, @Deaths)",
        new { Name = "PlayerA", Kills = 10, Deaths = 5 });

    conn.Execute(
        "UPDATE player_stats SET kills = kills + 1 WHERE name = @Name",
        new { Name = "PlayerA" });

    // Scalar value
    int count = conn.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM player_stats WHERE kills > @Min",
        new { Min = 100 });

    // Bulk insert (executes once per item)
    var newPlayers = new List<PlayerStats>
    {
        new PlayerStats { Name = "Player1", Kills = 0, Deaths = 0 },
        new PlayerStats { Name = "Player2", Kills = 0, Deaths = 0 },
    };
    conn.Execute(
        "INSERT INTO player_stats (name, kills, deaths) VALUES (@Name, @Kills, @Deaths)",
        newPlayers);
}
```

Dapper uses the same `MySqlConnection` — just add `using Dapper;` and call extension methods directly on the connection.

---

## HTTP Requests

Two options — raw HttpClient or Flurl (axios-style):

### Option 1: HttpClient (built-in)

```csharp
using System.Net.Http;
using Newtonsoft.Json.Linq;

var client = new HttpClient();

// GET
string json = client.GetStringAsync("https://api.example.com/data").Result;
var obj = JObject.Parse(json);

// POST JSON
var content = new StringContent(
    Newtonsoft.Json.JsonConvert.SerializeObject(new { player = "PlayerA" }),
    System.Text.Encoding.UTF8, "application/json");
var response = client.PostAsync("https://api.example.com/report", content).Result;
```

### Option 2: Flurl (axios-style, recommended)

```csharp
using Flurl;
using Flurl.Http;

// GET with query params
var data = "https://api.example.com/player"
    .SetQueryParam("name", "PlayerA")
    .GetJsonAsync<JObject>()
    .Result;

// GET with headers
var authed = "https://api.example.com/data"
    .WithHeader("Authorization", "Bearer my-token")
    .GetJsonAsync<JObject>()
    .Result;

// POST JSON body
var response = "https://api.example.com/report"
    .PostJsonAsync(new { player = "PlayerA", reason = "Cheating" })
    .Result;

// POST form data
var login = "https://api.example.com/login"
    .PostUrlEncodedAsync(new { username = "admin", password = "secret" })
    .Result;

// GET string
string html = "https://example.com/page"
    .GetStringAsync()
    .Result;

// With timeout
var slow = "https://api.example.com/slow"
    .WithTimeout(5)
    .GetStringAsync()
    .Result;

// Error handling
try
{
    var resp = "https://api.example.com/check".GetAsync().Result;
}
catch (FlurlHttpException ex)
{
    // ex.StatusCode, ex.Message
    Log("Info", "API error {0}: {1}", ex.StatusCode, ex.Message);
}
```

Flurl turns URLs into fluent request builders — chain `.WithHeader()`, `.SetQueryParam()`, `.WithTimeout()`, then call `.GetJsonAsync<T>()`, `.PostJsonAsync()`, etc.

---

## IP Reputation Checking (ProxyCheck.io)

Plugins can check player IPs for VPN/proxy usage via the built-in ProxyCheck.io integration:

```csharp
// Register for the callback event
public void OnPluginLoaded(string host, string port, string version)
{
    RegisterEvents(GetType().Name, "OnIPChecked", "OnPunkbusterPlayerInfo");
}

// Request an IP check when you get a player's IP (e.g., from PunkBuster)
public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
{
    if (!string.IsNullOrEmpty(playerInfo.Ip))
        ExecuteCommand("procon.protected.ipcheck", playerInfo.Ip);
}

// Receive the result asynchronously
public override void OnIPChecked(string ip, string countryName, string countryCode,
    string city, string provider, bool isVPN, bool isProxy, bool isTor, int risk)
{
    if (isVPN || isProxy)
    {
        Log("Warn", "Player from {0} ({1}) is using a {2}",
            ip, countryName, isVPN ? "VPN" : "Proxy");
    }
}
```

Results are cached in SQLite for 48 hours. Free tier: 1,000 lookups/day. Set an API key in Options for higher limits.

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
| Partial class not found | File must be named `<ClassName>.<Something>.cs` (flat) or placed in a `<ClassName>/` subfolder, and use `partial class` in the same namespace. |
| Subfolder files not found | The subfolder must be named exactly like the class (e.g., `AdKats/` for `AdKats.cs`). Files inside are scanned recursively. |
