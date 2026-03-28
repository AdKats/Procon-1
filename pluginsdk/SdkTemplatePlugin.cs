/*
 * PRoCon v2.0 SDK Template Plugin
 *
 * This is a starting point for developing PRoCon plugins on .NET 8.
 * It demonstrates the plugin API, available callbacks, variable system,
 * and best practices for cross-platform compatibility.
 *
 * To create your own plugin:
 *   1. Copy this file and rename the class to your plugin name
 *   2. The filename MUST match the class name (e.g., MyPlugin.cs → class MyPlugin)
 *   3. Place it in the Plugins/<GameType>/ directory
 *   4. PRoCon will compile and load it automatically on connect
 *
 * Available namespaces:
 *   - PRoCon.Core           (CServerInfo, CPrivileges, etc.)
 *   - PRoCon.Core.Plugin    (PRoConPluginAPI, CPluginVariable, etc.)
 *   - PRoCon.Core.Plugin.Commands  (MatchCommand, CapturedCommand)
 *   - PRoCon.Core.Players   (CPlayerInfo, Kill, Inventory, etc.)
 *   - PRoCon.Core.Players.Items    (Weapon, Specialization, Kits, etc.)
 *   - PRoCon.Core.Maps      (CMap, MaplistEntry, etc.)
 *   - PRoCon.Core.Battlemap  (MapZone, ZoneAction, Point3D)
 *   - PRoCon.Core.Accounts   (CPrivileges, AccountPrivilege)
 *   - PRoCon.Core.TextChatModeration (TextChatModerationEntry, etc.)
 *   - MySqlConnector         (MySqlConnection, MySqlCommand, etc.)
 *   - Newtonsoft.Json         (JsonConvert, JObject, etc.)
 *   - System.Net.Http         (HttpClient for web requests)
 *
 * NOT available on .NET 8 (removed):
 *   - System.Windows.Forms    (use Console.WriteLine for output)
 *   - PRoCon.Core.HttpServer  (removed — use SignalR layer)
 *   - System.Runtime.Remoting (AppDomain sandboxing gone)
 *   - System.Security.Permissions (CAS not available)
 *   - MySql.Data.MySqlClient  (replaced by MySqlConnector)
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
    public class SdkTemplatePlugin : PRoConPluginAPI, IPRoConPluginInterface
    {
        // =====================================================================
        // Plugin metadata — shown in the Plugins panel
        // =====================================================================

        private string _hostName;
        private string _port;
        private string _proconVersion;
        private bool _isEnabled;

        // =====================================================================
        // Plugin variables — configurable in the UI
        // =====================================================================

        private string _welcomeMessage = "Welcome to the server, %player%!";
        private int _announcementInterval = 300; // seconds
        private bool _enableWelcomeMessages = true;
        private string _logLevel = "Info"; // "Off", "Info", "Debug"

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

<h3>Features</h3>
<ul>
  <li>Welcome messages for joining players</li>
  <li>Configurable announcement interval</li>
  <li>Example of player events, chat commands, and admin actions</li>
</ul>

<h3>Settings</h3>
<ul>
  <li><b>Welcome Message</b> — Message sent to players on join. Use %player% for their name.</li>
  <li><b>Announcement Interval</b> — Seconds between server announcements (0 = disabled).</li>
  <li><b>Log Level</b> — Off, Info, or Debug.</li>
</ul>

<h3>Developing Your Own Plugin</h3>
<p>Copy this file, rename the class, and start coding. See the
<a href='https://github.com/your-repo/docs/PLUGIN-REFACTORING-GUIDE.md'>Plugin Refactoring Guide</a>
for migration tips from v1.x plugins.</p>
";
        }

        // =====================================================================
        // Plugin variables — UI configuration
        // =====================================================================

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            // Variables shown in the UI when the plugin is selected
            return new List<CPluginVariable>
            {
                new CPluginVariable("Messages|Welcome Message", typeof(string), _welcomeMessage),
                new CPluginVariable("Messages|Enable Welcome Messages", typeof(bool), _enableWelcomeMessages),
                new CPluginVariable("Timing|Announcement Interval (seconds)", typeof(int), _announcementInterval),
                new CPluginVariable("Debug|Log Level", "enum.LogLevel(Off|Info|Debug)", _logLevel),
            };
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            // All variables (used for config save/restore)
            return new List<CPluginVariable>
            {
                new CPluginVariable("Welcome Message", typeof(string), _welcomeMessage),
                new CPluginVariable("Enable Welcome Messages", typeof(bool), _enableWelcomeMessages),
                new CPluginVariable("Announcement Interval (seconds)", typeof(int), _announcementInterval),
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
        // Chat events — handle player commands
        // =====================================================================

        public override void OnGlobalChat(string speaker, string message)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        public override void OnTeamChat(string speaker, string message, int teamId)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            if (!_isEnabled || string.IsNullOrEmpty(message)) return;
            HandleChatCommand(speaker, message);
        }

        private void HandleChatCommand(string speaker, string message)
        {
            // Example: respond to "!help" in chat
            if (message.Trim().Equals("!help", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteCommand("procon.protected.send", "admin.say",
                    "Available commands: !help, !info", "player", speaker);
            }
            else if (message.Trim().Equals("!info", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteCommand("procon.protected.send", "admin.say",
                    string.Format("Server: {0}:{1} | PRoCon {2}", _hostName, _port, _proconVersion),
                    "player", speaker);
            }
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
        // Helper methods
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
