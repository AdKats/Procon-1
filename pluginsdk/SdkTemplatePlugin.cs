/*
 * PRoCon v2.0 SDK Template Plugin
 *
 * This is a starting point for developing PRoCon plugins on .NET 8.
 * It demonstrates the plugin API, available callbacks, variable system,
 * and best practices for cross-platform compatibility.
 *
 * ========================================================================
 * FILE STRUCTURE
 * ========================================================================
 *
 * PRoCon supports splitting plugins across multiple files using two methods:
 *
 * 1. PARTIAL CLASSES — Split a class across multiple .cs files:
 *      SdkTemplatePlugin.cs              ← Main file (class name = file name)
 *      SdkTemplatePlugin.Commands.cs     ← Partial: chat command handling
 *      SdkTemplatePlugin.Database.cs     ← Partial: database operations (MySqlConnector + Dapper)
 *      SdkTemplatePlugin.Http.cs         ← Partial: HTTP requests (HttpClient + Flurl)
 *
 *    Any file matching <ClassName>.*.cs is compiled as part of the plugin.
 *    Each file must use `partial class` and the same namespace.
 *
 * 2. #include DIRECTIVES — Inline file contents at compile time:
 *      #include "MySharedCode.inc"          ← From Plugins/<GameType>/
 *      #include "../SharedAcrossGames.inc"  ← From Plugins/ (parent dir)
 *      #include "%GameType%/Specific.inc"   ← %GameType% replaced at compile
 *
 *    Includes are processed before compilation (like C preprocessor).
 *    Use .inc extension by convention to distinguish from compilable .cs files.
 *    Nesting up to 5 levels deep is supported.
 *
 * FOR LARGE PLUGINS (like AdKats):
 *   Use partial classes to split into logical modules:
 *     AdKats.cs              ← Main: metadata, lifecycle, variables
 *     AdKats.Commands.cs     ← Chat/admin command processing
 *     AdKats.Players.cs      ← Player tracking, team management
 *     AdKats.Bans.cs         ← Ban enforcement logic
 *     AdKats.Database.cs     ← All MySQL operations
 *     AdKats.WebApi.cs       ← HTTP client calls
 *
 * ========================================================================
 * TO CREATE YOUR OWN PLUGIN
 * ========================================================================
 *
 *   1. Copy these files and rename the class to your plugin name
 *   2. The main filename MUST match the class name (MyPlugin.cs → class MyPlugin)
 *   3. Place all files in the Plugins/<GameType>/ directory
 *   4. PRoCon will compile and load them automatically on connect
 *
 * ========================================================================
 * AVAILABLE NAMESPACES
 * ========================================================================
 *
 *   PRoCon.Core           — CServerInfo, CPrivileges, etc.
 *   PRoCon.Core.Plugin    — PRoConPluginAPI, CPluginVariable, etc.
 *   PRoCon.Core.Plugin.Commands — MatchCommand, CapturedCommand
 *   PRoCon.Core.Players   — CPlayerInfo, Kill, Inventory, etc.
 *   PRoCon.Core.Players.Items   — Weapon, Specialization, Kits, etc.
 *   PRoCon.Core.Maps      — CMap, MaplistEntry, etc.
 *   PRoCon.Core.Battlemap  — MapZone, ZoneAction, Point3D
 *   PRoCon.Core.Accounts   — CPrivileges, AccountPrivilege
 *   PRoCon.Core.TextChatModeration — TextChatModerationEntry, etc.
 *   MySqlConnector         — MySqlConnection, MySqlCommand, etc.
 *   Newtonsoft.Json         — JsonConvert, JObject, etc.
 *   System.Net.Http         — HttpClient for web requests
 *
 * NOT available on .NET 8 (removed):
 *   System.Windows.Forms    — use plugin console for output
 *   PRoCon.Core.HttpServer  — removed, use SignalR layer
 *   System.Runtime.Remoting — AppDomain sandboxing gone
 *   System.Security.Permissions — CAS not available
 *   MySql.Data.MySqlClient  — replaced by MySqlConnector
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;

namespace PRoConEvents
{
    // The 'partial' keyword allows this class to span multiple files.
    // See SdkTemplatePlugin.Commands.cs and SdkTemplatePlugin.Database.cs
    public partial class SdkTemplatePlugin : PRoConPluginAPI, IPRoConPluginInterface
    {
        // =====================================================================
        // Plugin state
        // =====================================================================

        private string _hostName;
        private string _port;
        private string _proconVersion;
        private bool _isEnabled;

        // =====================================================================
        // Plugin variables — configurable in the UI
        // =====================================================================

        private string _welcomeMessage = "Welcome to the server, %player%!";
        private int _announcementInterval = 300;
        private bool _enableWelcomeMessages = true;
        private string _logLevel = "Info";

        // =====================================================================
        // IPRoConPluginInterface — Required metadata methods
        // =====================================================================

        public string GetPluginName() => "SDK Template Plugin";
        public string GetPluginVersion() => "1.0.0";
        public string GetPluginAuthor() => "PRoCon Team";
        public string GetPluginWebsite() => "https://github.com/your-repo";

        public string GetPluginDescription()
        {
            return @"
<h2>SDK Template Plugin</h2>
<p>A starting point for PRoCon v2.0 plugin development on .NET 8.</p>
<p>Demonstrates multi-file plugin structure using partial classes.</p>

<h3>File Structure</h3>
<ul>
  <li><b>SdkTemplatePlugin.cs</b> — Main: metadata, lifecycle, variables, player events</li>
  <li><b>SdkTemplatePlugin.Commands.cs</b> — Chat command handling</li>
  <li><b>SdkTemplatePlugin.Database.cs</b> — Database operations (raw SQL + Dapper ORM)</li>
  <li><b>SdkTemplatePlugin.Http.cs</b> — HTTP requests (HttpClient + Flurl)</li>
</ul>

<h3>Settings</h3>
<ul>
  <li><b>Welcome Message</b> — Message sent to players on join. Use %player% for their name.</li>
  <li><b>Announcement Interval</b> — Seconds between server announcements (0 = disabled).</li>
  <li><b>Log Level</b> — Off, Info, or Debug.</li>
</ul>
";
        }

        // =====================================================================
        // Plugin variables — UI configuration
        // =====================================================================

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            return new List<CPluginVariable>
            {
                new CPluginVariable("Messages|Welcome Message", typeof(string), _welcomeMessage),
                new CPluginVariable("Messages|Enable Welcome Messages", typeof(bool), _enableWelcomeMessages),
                new CPluginVariable("Timing|Announcement Interval (seconds)", typeof(int), _announcementInterval),
                new CPluginVariable("Database|DB Connection String", typeof(string), _dbConnectionString),
                new CPluginVariable("Debug|Log Level", "enum.LogLevel(Off|Info|Debug)", _logLevel),
            };
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return new List<CPluginVariable>
            {
                new CPluginVariable("Welcome Message", typeof(string), _welcomeMessage),
                new CPluginVariable("Enable Welcome Messages", typeof(bool), _enableWelcomeMessages),
                new CPluginVariable("Announcement Interval (seconds)", typeof(int), _announcementInterval),
                new CPluginVariable("DB Connection String", typeof(string), _dbConnectionString),
                new CPluginVariable("Log Level", typeof(string), _logLevel),
            };
        }

        public void SetPluginVariable(string variable, string value)
        {
            switch (variable)
            {
                case "Welcome Message":
                    _welcomeMessage = value;
                    break;
                case "Enable Welcome Messages":
                    bool.TryParse(value, out _enableWelcomeMessages);
                    break;
                case "Announcement Interval (seconds)":
                    int.TryParse(value, out _announcementInterval);
                    break;
                case "DB Connection String":
                    _dbConnectionString = value;
                    break;
                case "Log Level":
                    _logLevel = value;
                    break;
            }
        }

        // =====================================================================
        // Lifecycle events
        // =====================================================================

        public void OnPluginLoaded(string hostName, string port, string proconVersion)
        {
            _hostName = hostName;
            _port = port;
            _proconVersion = proconVersion;

            // Register events this plugin wants to receive.
            // Only events listed here will trigger the corresponding On* methods.
            RegisterEvents(GetType().Name,
                "OnPlayerJoin",
                "OnPlayerLeft",
                "OnPlayerKilled",
                "OnPlayerSpawned",
                "OnPlayerTeamChange",
                "OnGlobalChat",
                "OnTeamChat",
                "OnSquadChat",
                "OnRoundOver",
                "OnServerInfo",
                "OnListPlayers",
                "OnLevelLoaded"
            );
        }

        public void OnPluginEnable()
        {
            _isEnabled = true;
            Log("Info", "Plugin enabled on {0}:{1}", _hostName, _port);
        }

        public void OnPluginDisable()
        {
            _isEnabled = false;
            Log("Info", "Plugin disabled");
        }

        // =====================================================================
        // Player events
        // =====================================================================

        public override void OnPlayerJoin(string soldierName)
        {
            if (!_isEnabled || !_enableWelcomeMessages) return;

            string msg = _welcomeMessage.Replace("%player%", soldierName);
            ExecuteCommand("procon.protected.send", "admin.say", msg, "player", soldierName);

            Log("Info", "Player joined: {0}", soldierName);
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (!_isEnabled) return;
            Log("Debug", "Player left: {0}", playerInfo.SoldierName);
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            if (!_isEnabled) return;

            string killer = kKillerVictimDetails.Killer?.SoldierName ?? "Server";
            string victim = kKillerVictimDetails.Victim?.SoldierName ?? "Unknown";
            string weapon = kKillerVictimDetails.DamageType ?? "Unknown";

            Log("Debug", "{0} killed {1} with {2}", killer, victim, weapon);
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            if (!_isEnabled) return;
            Log("Debug", "Player spawned: {0}", soldierName);
        }

        // =====================================================================
        // Round & server events
        // =====================================================================

        public override void OnRoundOver(int winningTeamId)
        {
            if (!_isEnabled) return;
            Log("Info", "Round over. Winning team: {0}", winningTeamId);
        }

        public override void OnServerInfo(CServerInfo serverInfo)
        {
            if (!_isEnabled) return;
            Log("Debug", "Server: {0} | Players: {1}/{2} | Map: {3}",
                serverInfo.ServerName, serverInfo.PlayerCount,
                serverInfo.MaxPlayerCount, serverInfo.Map);
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            if (!_isEnabled) return;
            Log("Info", "Level loaded: {0} ({1}) — Round {2}/{3}",
                mapFileName, gamemode, roundsPlayed + 1, roundsTotal);
        }

        // =====================================================================
        // Logging helper (shared across all partial files)
        // =====================================================================

        private void Log(string level, string format, params object[] args)
        {
            if (_logLevel == "Off") return;
            if (_logLevel == "Info" && level == "Debug") return;

            string msg = string.Format("[SdkTemplatePlugin] [{0}] {1}", level, string.Format(format, args));
            ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }
    }
}
