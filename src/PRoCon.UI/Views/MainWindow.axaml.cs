using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using PRoCon.Core;
using PRoCon.Core.Logging;
using PRoCon.Core.Players;
using PRoCon.Core.Remote;
using PRoCon.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PRoCon.UI.Views
{
    public enum ServerConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }

    public class ConsoleLine
    {
        public string Text { get; set; }
        public string RawText { get; set; }
        public Avalonia.Media.IBrush ColorBrush { get; set; }
        public Avalonia.Media.FontWeight Weight { get; set; } = Avalonia.Media.FontWeight.Normal;
    }

    public class ServerEntry : INotifyPropertyChanged
    {
        public string HostPort { get; set; }

        private string _serverName;
        public string ServerName
        {
            get => _serverName;
            set { _serverName = value; Notify(nameof(ServerName)); Notify(nameof(DisplayLabel)); Notify(nameof(DisplayName)); Notify(nameof(IsNameOverflow)); }
        }

        private string _gameType;
        public string GameType
        {
            get => _gameType;
            set { _gameType = value; Notify(nameof(GameType)); Notify(nameof(DisplayLabel)); Notify(nameof(GameTypeLabel)); Notify(nameof(GameHeaderText)); }
        }

        public string GameTypeLabel => !string.IsNullOrEmpty(GameType) ? $"[{GameType}]" : "";

        private ServerConnectionState _state = ServerConnectionState.Disconnected;
        public ServerConnectionState State
        {
            get => _state;
            set { _state = value; Notify(nameof(State)); Notify(nameof(IsConnected)); Notify(nameof(IsPulsing)); Notify(nameof(StatusColor)); Notify(nameof(DisplayName)); }
        }

        public bool IsConnected => _state == ServerConnectionState.Connected;
        public bool IsPulsing => _state == ServerConnectionState.Connected || _state == ServerConnectionState.Connecting;

        // Display label: ServerName if available, otherwise HostPort
        public string DisplayLabel
        {
            get
            {
                string name = !string.IsNullOrEmpty(ServerName) ? ServerName : HostPort;
                if (!string.IsNullOrEmpty(GameType))
                    return $"[{GameType}] {name}";
                return name;
            }
        }

        // Status color brush for the indicator dot
        public Avalonia.Media.ISolidColorBrush StatusColor
        {
            get
            {
                return _state switch
                {
                    ServerConnectionState.Connected => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#66bb6a")),
                    ServerConnectionState.Connecting => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffab40")),
                    _ => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ef5350")),
                };
            }
        }

        // Per-server state
        public StringBuilder ChatBuffer { get; } = new StringBuilder();
        public ObservableCollection<ConsoleLine> ConsoleLines { get; } = new ObservableCollection<ConsoleLine>();
        public ConsoleFileLogger ConsoleLogger { get; set; }
        public List<string> PlayerItems { get; set; } = new List<string>();
        public HashSet<string> SupportedCommands { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal bool _pendingAdminHelp;
        internal int _adminHelpLineCount;
        public Dictionary<int, List<PlayerDisplayInfo>> TeamPlayers { get; set; } = new Dictionary<int, List<PlayerDisplayInfo>>
        {
            { 1, new List<PlayerDisplayInfo>() },
            { 2, new List<PlayerDisplayInfo>() },
            { 3, new List<PlayerDisplayInfo>() },
            { 4, new List<PlayerDisplayInfo>() }
        };
        public string ServerInfoText { get; set; } = "";
        public CServerInfo LastServerInfo { get; set; }

        // Dashboard data
        public List<(DateTime Time, int Count)> PlayerHistory { get; } = new List<(DateTime, int)>();
        public ObservableCollection<string> KillFeed { get; } = new ObservableCollection<string>();
        public string GameVersion { get; set; } = "";

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(ServerName))
                    return ServerName;
                return HostPort;
            }
        }

        // Group header support
        private bool _showGameHeader;
        public bool ShowGameHeader
        {
            get => _showGameHeader;
            set { _showGameHeader = value; Notify(nameof(ShowGameHeader)); }
        }

        public string GameHeaderText => !string.IsNullOrEmpty(GameType) ? GameType : "Unknown";

        // Marquee for long names (> ~18 chars at 12px font in 160px width)
        public bool IsNameOverflow => (DisplayName?.Length ?? 0) > 20;

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public override string ToString() => DisplayName;
    }

    public class PlayerDisplayInfo
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Ping { get; set; }
        public int Squad { get; set; }

        public string ScoreText => Score.ToString();
        public string KillsText => Kills.ToString();
        public string DeathsText => Deaths.ToString();
        public string PingText => Ping.ToString();
        public string SquadText => Squad > 0 ? Squad.ToString() : "-";
    }

    public partial class MainWindow : Window
    {
        private PRoConApplication _application;
        private ServerEntry _selectedServer;
        private int _activeTab = 6; // Default to Info tab
        private readonly ObservableCollection<ServerEntry> _servers = new ObservableCollection<ServerEntry>();
        private readonly Dictionary<string, ServerEntry> _serverLookup = new Dictionary<string, ServerEntry>(StringComparer.OrdinalIgnoreCase);

        // Console command history & autocomplete
        private readonly List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;

        private class RconCommandDef
        {
            public string Name;
            public string Signature; // display: "command <param1> <param2> [optional]"
            public int MinParams;    // minimum required params (excluding command name)
            public int MaxParams;    // max params (-1 = unlimited)
            public string Games;     // null = all games, otherwise comma-separated: "BF4,BF3,BFHL"
        }

        private static readonly RconCommandDef[] RconCommands = {
            // Auth & Misc
            new RconCommandDef { Name = "login.plainText", Signature = "login.plainText <password>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "login.hashed", Signature = "login.hashed [passwordHash]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "logout", Signature = "logout", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "quit", Signature = "quit", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "version", Signature = "version", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "serverInfo", Signature = "serverInfo", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "currentLevel", Signature = "currentLevel", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "listPlayers", Signature = "listPlayers <all|team <teamId>|player <name>>", MinParams = 1, MaxParams = 4 },
            // Admin
            new RconCommandDef { Name = "admin.eventsEnabled", Signature = "admin.eventsEnabled [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "admin.help", Signature = "admin.help", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "admin.kickPlayer", Signature = "admin.kickPlayer <soldierName> [reason]", MinParams = 1, MaxParams = 2 },
            new RconCommandDef { Name = "admin.killPlayer", Signature = "admin.killPlayer <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "admin.listPlayers", Signature = "admin.listPlayers <all|team <teamId>|squad <teamId> <squadId>>", MinParams = 1, MaxParams = 4 },
            new RconCommandDef { Name = "admin.movePlayer", Signature = "admin.movePlayer <soldierName> <teamId> <squadId> <forceKill>", MinParams = 4, MaxParams = 4 },
            new RconCommandDef { Name = "admin.password", Signature = "admin.password [password]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "admin.say", Signature = "admin.say <message> <all|team <teamId>|player <name>>", MinParams = 2, MaxParams = 4 },
            new RconCommandDef { Name = "admin.shutDown", Signature = "admin.shutDown", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "admin.yell", Signature = "admin.yell <message> [duration] [all|team <teamId>|player <name>]", MinParams = 1, MaxParams = 5 },
            new RconCommandDef { Name = "admin.effectiveMaxPlayers", Signature = "admin.effectiveMaxPlayers", MinParams = 0, MaxParams = 0, Games = "BF3" },
            // Ban List
            new RconCommandDef { Name = "banList.add", Signature = "banList.add <name|ip|guid> <id> <perm|rounds <n>|seconds <n>> [reason]", MinParams = 3, MaxParams = 5 },
            new RconCommandDef { Name = "banList.remove", Signature = "banList.remove <name|ip|guid> <id>", MinParams = 2, MaxParams = 2 },
            new RconCommandDef { Name = "banList.clear", Signature = "banList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "banList.list", Signature = "banList.list [startIndex]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "banList.load", Signature = "banList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "banList.save", Signature = "banList.save", MinParams = 0, MaxParams = 0 },
            // Map List
            new RconCommandDef { Name = "mapList.add", Signature = "mapList.add <mapName> <gamemode> <rounds> [index]", MinParams = 3, MaxParams = 4 },
            new RconCommandDef { Name = "mapList.remove", Signature = "mapList.remove <index>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.clear", Signature = "mapList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.list", Signature = "mapList.list [startIndex]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.load", Signature = "mapList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.save", Signature = "mapList.save", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.setNextMapIndex", Signature = "mapList.setNextMapIndex <index>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "mapList.getMapIndices", Signature = "mapList.getMapIndices", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.getRounds", Signature = "mapList.getRounds", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.availableMaps", Signature = "mapList.availableMaps", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.restartRound", Signature = "mapList.restartRound", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.runNextRound", Signature = "mapList.runNextRound", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "mapList.endRound", Signature = "mapList.endRound <winningTeamId>", MinParams = 1, MaxParams = 1 },
            // Reserved Slots
            new RconCommandDef { Name = "reservedSlotsList.add", Signature = "reservedSlotsList.add <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "reservedSlotsList.remove", Signature = "reservedSlotsList.remove <soldierName>", MinParams = 1, MaxParams = 1 },
            new RconCommandDef { Name = "reservedSlotsList.clear", Signature = "reservedSlotsList.clear", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.list", Signature = "reservedSlotsList.list", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.load", Signature = "reservedSlotsList.load", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.save", Signature = "reservedSlotsList.save", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "reservedSlotsList.aggressiveJoin", Signature = "reservedSlotsList.aggressiveJoin [true|false]", MinParams = 0, MaxParams = 1 },
            // Player queries (BF4 only)
            new RconCommandDef { Name = "player.idleDuration", Signature = "player.idleDuration <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            new RconCommandDef { Name = "player.isAlive", Signature = "player.isAlive <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            new RconCommandDef { Name = "player.ping", Signature = "player.ping <soldierName>", MinParams = 1, MaxParams = 1, Games = "BF4" },
            // PunkBuster
            new RconCommandDef { Name = "punkBuster.activate", Signature = "punkBuster.activate", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.deactivate", Signature = "punkBuster.deactivate", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.isActive", Signature = "punkBuster.isActive", MinParams = 0, MaxParams = 0 },
            new RconCommandDef { Name = "punkBuster.pb_sv_command", Signature = "punkBuster.pb_sv_command <command>", MinParams = 1, MaxParams = -1 },
            // FairFight (BF4 only)
            new RconCommandDef { Name = "fairFight.activate", Signature = "fairFight.activate", MinParams = 0, MaxParams = 0, Games = "BF4" },
            new RconCommandDef { Name = "fairFight.deactivate", Signature = "fairFight.deactivate", MinParams = 0, MaxParams = 0, Games = "BF4" },
            new RconCommandDef { Name = "fairFight.isActive", Signature = "fairFight.isActive", MinParams = 0, MaxParams = 0, Games = "BF4" },
            // Vars - boolean
            new RconCommandDef { Name = "vars.3dSpotting", Signature = "vars.3dSpotting [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.3pCam", Signature = "vars.3pCam [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.alwaysAllowSpectators", Signature = "vars.alwaysAllowSpectators [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.autoBalance", Signature = "vars.autoBalance [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.commander", Signature = "vars.commander [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.crossHair", Signature = "vars.crossHair [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.forceReloadWholeMags", Signature = "vars.forceReloadWholeMags [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.friendlyFire", Signature = "vars.friendlyFire [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.hitIndicatorsEnabled", Signature = "vars.hitIndicatorsEnabled [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.hud", Signature = "vars.hud [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.killCam", Signature = "vars.killCam [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.miniMap", Signature = "vars.miniMap [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.miniMapSpotting", Signature = "vars.miniMapSpotting [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.nameTag", Signature = "vars.nameTag [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.onlySquadLeaderSpawn", Signature = "vars.onlySquadLeaderSpawn [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.regenerateHealth", Signature = "vars.regenerateHealth [true|false]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.vehicleSpawnAllowed", Signature = "vars.vehicleSpawnAllowed [true|false]", MinParams = 0, MaxParams = 1 },
            // Vars - integer
            new RconCommandDef { Name = "vars.bulletDamage", Signature = "vars.bulletDamage [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.gameModeCounter", Signature = "vars.gameModeCounter [modifier: 0-500]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.idleBanRounds", Signature = "vars.idleBanRounds [rounds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.idleTimeout", Signature = "vars.idleTimeout [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.maxPlayers", Signature = "vars.maxPlayers [playerLimit]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.maxSpectators", Signature = "vars.maxSpectators [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.playerRespawnTime", Signature = "vars.playerRespawnTime [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundLockdownCountdown", Signature = "vars.roundLockdownCountdown [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundRestartPlayerCount", Signature = "vars.roundRestartPlayerCount [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundStartPlayerCount", Signature = "vars.roundStartPlayerCount [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundTimeLimit", Signature = "vars.roundTimeLimit [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.roundWarmupTimeout", Signature = "vars.roundWarmupTimeout [seconds]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.soldierHealth", Signature = "vars.soldierHealth [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillCountForKick", Signature = "vars.teamKillCountForKick [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillKickForBan", Signature = "vars.teamKillKickForBan [count]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueDecreasePerSecond", Signature = "vars.teamKillValueDecreasePerSecond [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueForKick", Signature = "vars.teamKillValueForKick [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamKillValueIncrease", Signature = "vars.teamKillValueIncrease [value]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.ticketBleedRate", Signature = "vars.ticketBleedRate [modifier: 0-500]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.vehicleSpawnDelay", Signature = "vars.vehicleSpawnDelay [modifier: 0-300]", MinParams = 0, MaxParams = 1 },
            // Vars - string
            new RconCommandDef { Name = "vars.gamePassword", Signature = "vars.gamePassword [password]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverDescription", Signature = "vars.serverDescription [description]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverMessage", Signature = "vars.serverMessage [message]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverName", Signature = "vars.serverName [name]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.serverType", Signature = "vars.serverType [Official|Ranked|Unranked|Private]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.unlockMode", Signature = "vars.unlockMode [mode]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.preset", Signature = "vars.preset [Normal|Hardcore|Infantry|Custom] [lockPresetSetting]", MinParams = 0, MaxParams = 2 },
            new RconCommandDef { Name = "vars.mpExperience", Signature = "vars.mpExperience [experience]", MinParams = 0, MaxParams = 1 },
            new RconCommandDef { Name = "vars.teamFactionOverride", Signature = "vars.teamFactionOverride <teamId> [factionId]", MinParams = 1, MaxParams = 2 },
        };

        private static readonly Dictionary<string, RconCommandDef> RconCommandLookup;
        static MainWindow()
        {
            RconCommandLookup = new Dictionary<string, RconCommandDef>(StringComparer.OrdinalIgnoreCase);
            foreach (var cmd in RconCommands)
                RconCommandLookup[cmd.Name] = cmd;
        }

        private bool IsCommandForCurrentGame(RconCommandDef cmd)
        {
            // If server reported supported commands via admin.help, use that
            if (_selectedServer?.SupportedCommands.Count > 0)
                return _selectedServer.SupportedCommands.Contains(cmd.Name);

            // Fall back to static game-type filtering
            if (cmd.Games == null) return true;
            string gameType = _selectedServer?.GameType;
            if (string.IsNullOrEmpty(gameType)) return true;
            return cmd.Games.Contains(gameType);
        }

        // Panel instances
        private MapListPanel _mapListPanel;
        private BanListPanel _banListPanel;
        private ReservedSlotsPanel _reservedSlotsPanel;
        private PluginsPanel _pluginsPanel;
        private AccountsPanel _accountsPanel;
        private EventsPanel _eventsPanel;
        private ServerSettingsPanel _serverSettingsPanel;
        private PlayerActionsPanel _playerActionsPanel;
        private LayerPanel _layerPanel;
        private OptionsPanel _optionsPanel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("procon-ui-crash.log", $"InitializeComponent failed:\n{ex}");
                throw;
            }

            try
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
                PRoConLog.Initialize(loggerFactory);

                // Create panel instances
                _mapListPanel = new MapListPanel();
                _banListPanel = new BanListPanel();
                _reservedSlotsPanel = new ReservedSlotsPanel();
                _pluginsPanel = new PluginsPanel();
                _accountsPanel = new AccountsPanel();
                _eventsPanel = new EventsPanel();
                _serverSettingsPanel = new ServerSettingsPanel();
                _playerActionsPanel = new PlayerActionsPanel();
                _layerPanel = new LayerPanel();
                _optionsPanel = new OptionsPanel();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("procon-ui-crash.log", $"Panel creation failed:\n{ex}");
                throw;
            }

            // Set window icon for both title bar and taskbar
            try
            {
                var uri = new Uri("avares://PRoCon.UI/procon.ico");
                this.Icon = new WindowIcon(Avalonia.Platform.AssetLoader.Open(uri));
            }
            catch { }

            this.Opened += MainWindow_Opened;
        }

        private PRoConClient GetClient(string hostPort)
        {
            if (_application != null && _application.Connections.Contains(hostPort))
                return _application.Connections[hostPort];
            return null;
        }

        private PRoConClient SelectedClient => _selectedServer != null ? GetClient(_selectedServer.HostPort) : null;

        // --- Initialization ---

        private void MainWindow_Opened(object sender, EventArgs e)
        {
            try
            {
            if (_application == null)
            {
                _application = new PRoConApplication(false, new string[0]);
                _application.Execute();
            }

            var serverList = this.FindControl<ListBox>("ServerList");
            if (serverList != null)
                serverList.ItemsSource = _servers;

            // Wire panels into ContentControls
            var mapContent = this.FindControl<ContentControl>("MapListContent");
            if (mapContent != null) mapContent.Content = _mapListPanel;
            var banContent = this.FindControl<ContentControl>("BanListContent");
            if (banContent != null) banContent.Content = _banListPanel;
            var reservedContent = this.FindControl<ContentControl>("ReservedSlotsContent");
            if (reservedContent != null) reservedContent.Content = _reservedSlotsPanel;
            var pluginsContent = this.FindControl<ContentControl>("PluginsContent");
            if (pluginsContent != null) pluginsContent.Content = _pluginsPanel;
            var accountsContent = this.FindControl<ContentControl>("AccountsContent");
            if (accountsContent != null) accountsContent.Content = _accountsPanel;
            var eventsContent = this.FindControl<ContentControl>("EventsContent");
            if (eventsContent != null) eventsContent.Content = _eventsPanel;
            var settingsContent = this.FindControl<ContentControl>("ServerSettingsContent");
            if (settingsContent != null) settingsContent.Content = _serverSettingsPanel;
            var layerContent = this.FindControl<ContentControl>("LayerContent");
            if (layerContent != null) layerContent.Content = _layerPanel;
            var optionsContent = this.FindControl<ContentControl>("OptionsContent");
            if (optionsContent != null) optionsContent.Content = _optionsPanel;

            // Load existing connections
            foreach (PRoConClient client in _application.Connections)
            {
                var entry = EnsureServerEntry(client.HostNamePort);
                WireClientEvents(client, entry);

                if (client.CurrentServerInfo?.ServerName != null)
                    entry.ServerName = client.CurrentServerInfo.ServerName;

                if (client.Game != null && client.Game.IsLoggedIn)
                    entry.State = ServerConnectionState.Connected;
            }

            // Ensure test servers exist
            EnsureServer("65.75.210.194", 47300, "REDACTED");   // BF4
            EnsureServer("104.238.220.182", 47210, "REDACTED"); // BF3
            EnsureServer("65.75.210.194", 47250, "REDACTED");  // BFH

            UpdateConnectionCount();

            // Listen for new connections
            _application.Connections.ConnectionAdded += conn =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var entry = EnsureServerEntry(conn.HostNamePort);
                    WireClientEvents(conn, entry);
                    UpdateConnectionCount();
                });
            };
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"MainWindow_Opened failed: {ex}");
                System.IO.File.WriteAllText("procon-ui-crash.log", $"Opened failed:\n{ex}");
            }
        }

        private void EnsureServer(string host, ushort port, string password, bool autoConnect = true)
        {
            string hostPort = $"{host}:{port}";
            if (!_application.Connections.Contains(hostPort))
                _application.AddConnection(host, port, "default", password);
            var entry = EnsureServerEntry(hostPort);
            var client = GetClient(hostPort);
            if (client != null)
            {
                WireClientEvents(client, entry);
                if (autoConnect)
                {
                    entry.State = ServerConnectionState.Connecting;
                    client.AutomaticallyConnect = true;
                }
            }
        }

        private ServerEntry EnsureServerEntry(string hostPort)
        {
            if (_serverLookup.TryGetValue(hostPort, out var existing))
                return existing;

            var entry = new ServerEntry { HostPort = hostPort };
            _servers.Add(entry);
            _serverLookup[hostPort] = entry;
            return entry;
        }

        private readonly HashSet<string> _wiredClients = new HashSet<string>();

        private void WireClientEvents(PRoConClient client, ServerEntry entry)
        {
            if (!_wiredClients.Add(client.HostNamePort))
                return; // Already wired

            client.ConnectAttempt += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Connecting;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ffab40", $"Connecting to {entry.HostPort}...");
                    UpdateSidebarButtons();
                    UpdateContentVisibility();
                }
            });

            client.ConnectSuccess += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Connecting;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ffab40", "Connected, logging in...");
                    UpdateContentVisibility();
                }
            });

            client.ConnectionClosed += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Disconnected;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ef5350", "Disconnected");
                    UpdateSidebarButtons();
                    UpdateContentVisibility();
                }
            });

            client.LoginAttempt += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Connecting;
                if (_selectedServer == entry)
                    UpdateServerInfoPanel("Authenticating...", "Sending RCON credentials...");
            });

            client.Login += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Connected;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#66bb6a", $"Logged in to {entry.HostPort}");
                    UpdateSidebarButtons();
                    UpdateContentVisibility();
                    SwitchTab(6); // Show dashboard on connect
                }

                // Request supported commands from server
                if (client.Game != null)
                {
                    entry._pendingAdminHelp = true;
                    client.SendRequest(new List<string> { "admin.help" });
                }
            });

            client.Logout += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Disconnected;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ffab40", "Logged out");
                    UpdateContentVisibility();
                }
            });

            // Wire console RCON traffic
            WireConsoleEvents(client, entry);
            if (client.Console == null)
            {
                // Console may not exist yet — retry
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
                    Dispatcher.UIThread.Post(() => WireConsoleEvents(client, entry)));
                System.Threading.Tasks.Task.Delay(5000).ContinueWith(_ =>
                    Dispatcher.UIThread.Post(() => WireConsoleEvents(client, entry)));
            }

            client.GameTypeDiscovered += sender =>
            {
                if (sender.Game != null)
                    WireGameEvents(sender.Game, entry);
            };

            if (client.Game != null)
                WireGameEvents(client.Game, entry);
        }

        private void OnClientEvent(ServerEntry entry, Action action)
        {
            Dispatcher.UIThread.Post(action);
        }

        private readonly HashSet<string> _wiredConsoles = new HashSet<string>();

        private static readonly System.Text.RegularExpressions.Regex ColorCodeRegex =
            new System.Text.RegularExpressions.Regex(@"\^[0-9a-zA-Z]", System.Text.RegularExpressions.RegexOptions.Compiled);

        private static readonly Avalonia.Media.IBrush DefaultConsoleBrush =
            new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#eceff1"));

        private static Avalonia.Media.IBrush GetColorBrushForCode(string code)
        {
            return code switch
            {
                "^0" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#263238")),  // black
                "^1" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ef5350")),  // red
                "^2" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#66bb6a")),  // green
                "^3" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffab40")),  // yellow/orange
                "^4" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4fc3f7")),  // blue
                "^5" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00bcd4")),  // cyan
                "^6" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ce93d8")),  // magenta
                "^7" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#eceff1")),  // white
                "^8" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#8899aa")),  // gray
                "^9" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#f48fb1")),  // pink
                _ => null
            };
        }

        private static readonly string[] ErrorResponses = {
            "InvalidArguments", "InvalidPlayerName", "InvalidTeamId", "InvalidSquadId",
            "InvalidPassword", "PlayerNotFound", "InvalidMapName", "InvalidGameModeOnMap",
            "InvalidRoundsPerMap", "InvalidCommand", "UnknownCommand", "LogInRequired",
            "CommandIsReadOnly", "TooLongMessage", "SetTooLongMessage",
            "ServerFull", "InvalidBanIdType", "BanListFull", "MapListFull"
        };

        private static ConsoleLine ParseConsoleLine(string rawText, string displayPrefix)
        {
            string cleanText = ColorCodeRegex.Replace(rawText, "");
            string displayText = $"{displayPrefix}{cleanText}";

            // Check for error responses
            bool isError = false;
            foreach (string err in ErrorResponses)
            {
                if (cleanText.Contains(err))
                {
                    isError = true;
                    break;
                }
            }

            if (isError)
            {
                return new ConsoleLine
                {
                    Text = displayText,
                    RawText = rawText,
                    ColorBrush = new SolidColorBrush(Color.Parse("#ef5350")), // red
                    Weight = Avalonia.Media.FontWeight.Bold
                };
            }

            // Find the first color code to determine line color
            Avalonia.Media.IBrush brush = DefaultConsoleBrush;
            bool bold = false;

            var match = ColorCodeRegex.Match(rawText);
            while (match.Success)
            {
                string code = match.Value.ToLowerInvariant();
                if (code == "^b")
                {
                    bold = true;
                }
                else if (code == "^n" || code == "^i")
                {
                    // normal/italic - skip
                }
                else
                {
                    var codeBrush = GetColorBrushForCode(code);
                    if (codeBrush != null)
                    {
                        brush = codeBrush;
                        break; // Use the first color code found
                    }
                }
                match = match.NextMatch();
            }

            return new ConsoleLine
            {
                Text = displayText,
                RawText = rawText,
                ColorBrush = brush,
                Weight = bold ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal
            };
        }

        private void EnsureConsoleLogger(PRoConClient client, ServerEntry entry)
        {
            if (entry.ConsoleLogger != null) return;
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string safeHostPort = client.HostNamePort.Replace(":", "_");
                string logDir = Path.Combine(appDir, "Logs", safeHostPort);
                entry.ConsoleLogger = new ConsoleFileLogger(logDir);
            }
            catch
            {
                // Non-critical - continue without file logging
            }
        }

        private void WireConsoleEvents(PRoConClient client, ServerEntry entry)
        {
            if (client?.Console == null) return;
            if (!_wiredConsoles.Add(client.HostNamePort)) return; // Already wired

            EnsureConsoleLogger(client, entry);

            client.Console.WriteConsole += (dtLoggedTime, strLoggedText) => Dispatcher.UIThread.Post(() =>
            {
                string timestamp = dtLoggedTime.ToString("HH:mm:ss");
                var line = ParseConsoleLine(strLoggedText, $"[{timestamp}] ");

                entry.ConsoleLines.Add(line);

                // Parse admin.help response: single line "OK cmd1 cmd2 cmd3 ..."
                if (entry._pendingAdminHelp)
                {
                    string clean = ColorCodeRegex.Replace(strLoggedText, "").Trim();
                    if (clean.StartsWith("OK ") && clean.Contains("admin.help"))
                    {
                        entry._pendingAdminHelp = false;
                        var parts = clean.Split(' ');
                        foreach (string part in parts)
                        {
                            if (part != "OK" && part.Contains('.'))
                                entry.SupportedCommands.Add(part);
                        }
                    }
                }

                // Log to file
                entry.ConsoleLogger?.WriteLine(line.Text);

                // Cap at ~2000 lines
                while (entry.ConsoleLines.Count > 2000)
                    entry.ConsoleLines.RemoveAt(0);

                if (_selectedServer == entry)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            var list = this.FindControl<ListBox>("ConsoleLogList");
                            if (list != null && list.ItemCount > 0)
                                list.ScrollIntoView(list.ItemCount - 1);
                        }
                        catch { }
                    }, Avalonia.Threading.DispatcherPriority.Background);
                }
            });
        }

        private void WireGameEvents(FrostbiteClient game, ServerEntry entry)
        {
            game.ServerInfo += (sender, info) => Dispatcher.UIThread.Post(() =>
            {
                string name = info.ServerName ?? "Unknown Server";
                string map = info.Map ?? "Unknown";
                string mode = info.GameMode ?? "Unknown";

                entry.ServerName = name;
                entry.GameType = sender.GameType ?? "";
                entry.State = ServerConnectionState.Connected;
                entry.LastServerInfo = info;
                SortAndGroupServers();
                entry.GameVersion = sender.FriendlyVersionNumber ?? sender.VersionNumber ?? "";

                // Track player count history
                entry.PlayerHistory.Add((DateTime.Now, info.PlayerCount));
                // Keep last 60 minutes of data
                var cutoff = DateTime.Now.AddMinutes(-60);
                while (entry.PlayerHistory.Count > 0 && entry.PlayerHistory[0].Time < cutoff)
                    entry.PlayerHistory.RemoveAt(0);

                if (_selectedServer == entry)
                {
                    UpdateStatus("#66bb6a", name);
                    UpdateServerInfoPanel(name, $"{map} — {mode} — {info.PlayerCount}/{info.MaxPlayerCount} players");
                    UpdateDashboard(entry, sender);
                }

                UpdateConnectionCount();
            });

            game.PlayerJoin += (sender, playerName) => Dispatcher.UIThread.Post(() =>
            {
                AppendChat(entry, $"[Join] {playerName} joined the server");
                if (_selectedServer == entry)
                    RefreshPlayerList();
            });

            game.PlayerLeft += (sender, playerName, info) => Dispatcher.UIThread.Post(() =>
            {
                AppendChat(entry, $"[Leave] {playerName} left the server");
                if (_selectedServer == entry)
                    RefreshPlayerList();
            });

            game.PlayerKilled += (sender, killer, victim, weapon, headshot, killerPos, victimPos) => Dispatcher.UIThread.Post(() =>
            {
                string hs = headshot ? " [HS]" : "";
                string feedLine = $"{killer} [{weapon}] {victim}{hs}";
                entry.KillFeed.Insert(0, feedLine);
                while (entry.KillFeed.Count > 50)
                    entry.KillFeed.RemoveAt(entry.KillFeed.Count - 1);

                if (_selectedServer == entry)
                {
                    var killList = this.FindControl<ListBox>("KillFeedList");
                    if (killList != null) killList.ItemsSource = entry.KillFeed;
                }
            });

            game.Chat += (sender, rawChat) => Dispatcher.UIThread.Post(() =>
            {
                if (rawChat.Count >= 3)
                {
                    string source = rawChat[0];
                    string message = rawChat[1];
                    string target = rawChat[2];
                    AppendChat(entry, $"[{target}] {source}: {message}");
                }
            });

            game.ListPlayers += (sender, players, subset) => Dispatcher.UIThread.Post(() =>
            {
                // Clear all teams
                for (int t = 1; t <= 4; t++)
                    entry.TeamPlayers[t].Clear();

                // Sort players into teams
                foreach (var player in players)
                {
                    int teamId = player.TeamID;
                    if (teamId < 1 || teamId > 4) teamId = 1;

                    entry.TeamPlayers[teamId].Add(new PlayerDisplayInfo
                    {
                        Name = player.SoldierName,
                        Score = player.Score,
                        Kills = player.Kills,
                        Deaths = player.Deaths,
                        Ping = player.Ping,
                        Squad = player.SquadID
                    });
                }

                // Sort each team by score descending
                for (int t = 1; t <= 4; t++)
                    entry.TeamPlayers[t] = entry.TeamPlayers[t].OrderByDescending(p => p.Score).ToList();

                // Also keep flat list for backward compat
                var items = new List<string>();
                foreach (var player in players)
                    items.Add($"{player.SoldierName}  —  Score: {player.Score}  K/D: {player.Kills}/{player.Deaths}  Squad: {player.SquadID}  Team: {player.TeamID}");
                entry.PlayerItems = items;

                if (_selectedServer == entry)
                    UpdateTeamPanels(entry);
            });
        }

        // --- Tab Switching ---

        private void OnTabClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr && int.TryParse(tagStr, out int tabIndex))
                SwitchTab(tabIndex);
        }

        private void SwitchTab(int index)
        {
            _activeTab = index;
            for (int i = 0; i <= 13; i++)
            {
                var tab = this.FindControl<Border>($"Tab{i}");
                if (tab != null) tab.IsVisible = (i == index);
            }

            var tabBar = this.FindControl<WrapPanel>("TabBar");
            if (tabBar != null)
            {
                foreach (var child in tabBar.Children)
                {
                    if (child is Button tabBtn && tabBtn.Tag is string ts && int.TryParse(ts, out int ti))
                    {
                        tabBtn.Foreground = new SolidColorBrush(Color.Parse(ti == index ? "#4fc3f7" : "#8899aa"));
                        tabBtn.FontWeight = ti == index ? Avalonia.Media.FontWeight.SemiBold : Avalonia.Media.FontWeight.Normal;
                    }
                }
            }

            // Auto-scroll console to bottom when switching to Console tab
            if (index == 11)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        var list = this.FindControl<ListBox>("ConsoleLogList");
                        if (list != null && list.ItemCount > 0)
                            list.ScrollIntoView(list.ItemCount - 1);
                    }
                    catch { }
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        // --- Server Selection & Connection ---

        private async void OnShowConnectForm(object sender, RoutedEventArgs e)
        {
            var dialog = new AddServerDialog();
            await dialog.ShowDialog(this);

            if (dialog.Confirmed)
            {
                string host = dialog.Host;
                ushort port = dialog.Port;
                string password = dialog.Password;
                string hostPort = $"{host}:{port}";

                UpdateStatus("#ffab40", $"Connecting to {hostPort}...");

                try
                {
                    PRoConClient client;
                    if (_application.Connections.Contains(hostPort))
                    {
                        client = _application.Connections[hostPort];
                    }
                    else
                    {
                        client = _application.AddConnection(host, port, "default", password);
                    }

                    if (client == null)
                    {
                        UpdateStatus("#ef5350", "Failed to create connection");
                        return;
                    }

                    var entry = EnsureServerEntry(hostPort);
                    WireClientEvents(client, entry);
                    _selectedServer = entry;
                    entry.State = ServerConnectionState.Connecting;
                    client.AutomaticallyConnect = true;

                    var serverList = this.FindControl<ListBox>("ServerList");
                    if (serverList != null) serverList.SelectedItem = entry;

                    LoadServerView(entry);
                    UpdateSidebarButtons();
                    UpdateConnectionCount();
                    SwitchTab(6);

                    _application.SaveMainConfig();
                }
                catch (System.Exception ex)
                {
                    UpdateStatus("#ef5350", $"Error: {ex.Message}");
                }
            }
        }

        private void OnServerSelected(object sender, SelectionChangedEventArgs e)
        {
            var serverList = this.FindControl<ListBox>("ServerList");
            if (serverList?.SelectedItem is not ServerEntry entry) return;

            _selectedServer = entry;
            ShowRemoveButton(true);

            // Load this server's state into the view
            LoadServerView(entry);
            UpdateSidebarButtons();
            UpdateContentVisibility();

            // Switch to Info if connected
            if (entry.IsConnected || entry.State == ServerConnectionState.Connecting)
                SwitchTab(6);
        }

        private void OnServerDoubleClick(object sender, Avalonia.Input.TappedEventArgs e)
        {
            ConnectSelectedServer();
        }

        private void OnConnectSelected(object sender, RoutedEventArgs e)
        {
            ConnectSelectedServer();
        }

        private void ConnectSelectedServer()
        {
            if (_selectedServer == null) return;
            var client = GetClient(_selectedServer.HostPort);
            if (client == null) return;

            _selectedServer.State = ServerConnectionState.Connecting;
            UpdateStatus("#ffab40", $"Connecting to {_selectedServer.HostPort}...");
            client.AutomaticallyConnect = true;
            UpdateSidebarButtons();
        }

        private void OnDisconnect(object sender, RoutedEventArgs e)
        {
            if (_selectedServer == null) return;
            var client = GetClient(_selectedServer.HostPort);
            if (client == null) return;

            client.AutomaticallyConnect = false;
            client.Shutdown();
            _selectedServer.State = ServerConnectionState.Disconnected;
            _selectedServer.ConsoleLogger?.Dispose();
            _selectedServer.ConsoleLogger = null;
            _wiredConsoles.Remove(_selectedServer.HostPort);

            UpdateStatus("#ef5350", "Disconnected");
            UpdateSidebarButtons();
            UpdateContentVisibility();
        }

        private async void OnRemoveServer(object sender, RoutedEventArgs e)
        {
            if (_selectedServer == null) return;

            // Confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Remove",
                Width = 350, Height = 150,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false
            };
            bool confirmed = false;
            var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 16 };
            panel.Children.Add(new TextBlock { Text = $"Remove server {_selectedServer.DisplayLabel}?", TextWrapping = Avalonia.Media.TextWrapping.Wrap });
            var btnPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
            var cancelBtn = new Button { Content = "Cancel", Padding = new Avalonia.Thickness(16, 6) };
            cancelBtn.Click += (s, a) => dialog.Close();
            var removeBtn = new Button { Content = "Remove", Padding = new Avalonia.Thickness(16, 6) };
            removeBtn.Click += (s, a) => { confirmed = true; dialog.Close(); };
            btnPanel.Children.Add(cancelBtn);
            btnPanel.Children.Add(removeBtn);
            panel.Children.Add(btnPanel);
            dialog.Content = panel;

            await dialog.ShowDialog(this);
            if (!confirmed) return;

            var entry = _selectedServer;
            var client = GetClient(entry.HostPort);

            if (client != null)
            {
                client.AutomaticallyConnect = false;
                client.Shutdown();
                _application.Connections.Remove(entry.HostPort);
                _wiredClients.Remove(entry.HostPort);
            }

            entry.ConsoleLogger?.Dispose();
            entry.ConsoleLogger = null;
            _servers.Remove(entry);
            _serverLookup.Remove(entry.HostPort);
            _selectedServer = null;

            UpdateStatus("#8899aa", "Server removed");
            UpdateServerInfoPanel("", "");
            ShowConnectButton(false);
            ShowDisconnectButton(false);
            ShowRemoveButton(false);
            UpdateConnectionCount();
            _application.SaveMainConfig();

            if (_servers.Count == 0) SwitchTab(6);
        }

        // --- Load Server View (switch main panel to selected server's data) ---

        private void LoadServerView(ServerEntry entry)
        {
            var client = GetClient(entry.HostPort);
            bool connected = entry.IsConnected && client?.Game != null;

            // Update all panels with the selected server's client
            _mapListPanel?.SetClient(client);
            _banListPanel?.SetClient(client);
            _reservedSlotsPanel?.SetClient(client);
            _pluginsPanel?.SetClient(client);
            _accountsPanel?.SetClient(client);
            _accountsPanel?.SetApplication(_application);
            _eventsPanel?.SetClient(client);
            _serverSettingsPanel?.SetClient(client);
            _playerActionsPanel?.SetClient(client);
            _layerPanel?.SetClient(client);
            _optionsPanel?.SetApplication(_application);

            // Load data for connected servers
            if (connected)
            {
                _mapListPanel?.LoadData();
                _banListPanel?.LoadData();
                _reservedSlotsPanel?.LoadData();
            }

            // Status
            if (connected)
                UpdateStatus("#66bb6a", entry.ServerName ?? entry.HostPort);
            else
                UpdateStatus("#8899aa", entry.HostPort);

            // Server info panel (on connection tab)
            if (connected && entry.LastServerInfo != null)
            {
                var info = entry.LastServerInfo;
                UpdateServerInfoPanel(entry.ServerName ?? entry.HostPort,
                    $"{info.Map ?? "?"} — {info.GameMode ?? "?"} — {info.PlayerCount}/{info.MaxPlayerCount} players");
            }
            else if (connected)
            {
                UpdateServerInfoPanel($"Connected: {entry.HostPort}", "Waiting for server info...");
            }
            else
            {
                UpdateServerInfoPanel("", "");
            }

            // Chat
            var chatLog = this.FindControl<TextBlock>("ChatLog");
            if (chatLog != null) chatLog.Text = entry.ChatBuffer.ToString();

            // Players
            UpdateTeamPanels(entry);

            // Server Info dashboard
            var dashClient = GetClient(entry.HostPort);
            if (dashClient?.Game != null)
                UpdateDashboard(entry, dashClient.Game);

            // Kill feed
            var killList = this.FindControl<ListBox>("KillFeedList");
            if (killList != null) killList.ItemsSource = entry.KillFeed;

            // Console
            var consoleLogList = this.FindControl<ListBox>("ConsoleLogList");
            if (consoleLogList != null)
            {
                consoleLogList.ItemsSource = entry.ConsoleLines;
                // Scroll to bottom after loading
                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        if (consoleLogList.ItemCount > 0)
                            consoleLogList.ScrollIntoView(consoleLogList.ItemCount - 1);
                    }
                    catch { }
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        // --- Chat ---

        private void AppendChat(ServerEntry entry, string line)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            entry.ChatBuffer.AppendLine($"[{timestamp}] {line}");

            if (_selectedServer == entry)
            {
                var chatLog = this.FindControl<TextBlock>("ChatLog");
                if (chatLog != null) chatLog.Text = entry.ChatBuffer.ToString();
                var scroller = this.FindControl<ScrollViewer>("ChatScroller");
                scroller?.ScrollToEnd();
            }
        }

        private void OnSendChat(object sender, RoutedEventArgs e)
        {
            var chatInput = this.FindControl<TextBox>("ChatInput");
            var client = SelectedClient;
            if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.Text) || client?.Game == null || _selectedServer == null)
                return;

            string msg = chatInput.Text;
            client.Game.SendAdminSayPacket(msg, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            AppendChat(_selectedServer, $"[Admin] {msg}");
            chatInput.Text = "";
        }

        private void OnChatInputKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                OnSendChat(sender, e);
                e.Handled = true;
            }
        }

        // --- Console ---

        private void OnConsoleInputKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            var consoleInput = sender as TextBox;
            if (consoleInput == null) return;

            if (e.Key == Avalonia.Input.Key.Enter)
            {
                var suggestionsBox = this.FindControl<ListBox>("ConsoleSuggestions");
                if (suggestionsBox != null) suggestionsBox.IsVisible = false;
                OnSendConsoleCommand(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Up)
            {
                // Navigate command history (older)
                if (_commandHistory.Count > 0)
                {
                    if (_historyIndex < _commandHistory.Count - 1)
                        _historyIndex++;
                    consoleInput.Text = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                    consoleInput.CaretIndex = consoleInput.Text?.Length ?? 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Down)
            {
                // Navigate command history (newer)
                if (_historyIndex > 0)
                {
                    _historyIndex--;
                    consoleInput.Text = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                    consoleInput.CaretIndex = consoleInput.Text?.Length ?? 0;
                }
                else
                {
                    _historyIndex = -1;
                    consoleInput.Text = "";
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Tab)
            {
                // Auto-complete: pick suggestion or complete common prefix
                var suggestionsBox = this.FindControl<ListBox>("ConsoleSuggestions");
                if (suggestionsBox != null && suggestionsBox.IsVisible && suggestionsBox.ItemCount > 0)
                {
                    string selected = (suggestionsBox.SelectedItem ?? suggestionsBox.Items.Cast<object>().First())?.ToString();
                    if (selected != null)
                    {
                        string cmdName = selected.Split(' ')[0];
                        consoleInput.Text = cmdName + " ";
                        consoleInput.CaretIndex = consoleInput.Text.Length;
                        suggestionsBox.IsVisible = false;
                    }
                }
                else
                {
                    string text = consoleInput.Text ?? "";
                    string prefix = text.Split(' ')[0];
                    var matches = new List<RconCommandDef>();
                    foreach (var cmd in RconCommands)
                    {
                        if (cmd.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && IsCommandForCurrentGame(cmd))
                            matches.Add(cmd);
                    }
                    if (matches.Count == 1)
                    {
                        consoleInput.Text = matches[0].Name + " ";
                        consoleInput.CaretIndex = consoleInput.Text.Length;
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            {
                var suggestionsBox = this.FindControl<ListBox>("ConsoleSuggestions");
                if (suggestionsBox != null) suggestionsBox.IsVisible = false;
                e.Handled = true;
            }
        }

        private void OnSendConsoleCommand(object sender, RoutedEventArgs e)
        {
            var consoleInput = this.FindControl<TextBox>("ConsoleInput");
            var client = SelectedClient;
            if (consoleInput == null || string.IsNullOrWhiteSpace(consoleInput.Text) || client?.Game == null || _selectedServer == null)
                return;

            string cmd = consoleInput.Text.Trim();
            var words = new List<string>(cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            if (words.Count == 0) return;

            // Validate command
            string cmdName = words[0];
            int paramCount = words.Count - 1;

            if (RconCommandLookup.TryGetValue(cmdName, out var cmdDef))
            {
                if (paramCount < cmdDef.MinParams)
                {
                    string timestamp2 = DateTime.Now.ToString("HH:mm:ss");
                    _selectedServer.ConsoleLines.Add(new ConsoleLine
                    {
                        Text = $"[{timestamp2}] Error: Too few parameters. Usage: {cmdDef.Signature}",
                        RawText = $"Error: Too few parameters for {cmdName}",
                        ColorBrush = new SolidColorBrush(Color.Parse("#ef5350")),
                        Weight = Avalonia.Media.FontWeight.Bold
                    });
                    var list2 = this.FindControl<ListBox>("ConsoleLogList");
                    if (list2 != null && list2.ItemCount > 0)
                        list2.ScrollIntoView(list2.ItemCount - 1);
                    return;
                }
                if (cmdDef.MaxParams >= 0 && paramCount > cmdDef.MaxParams)
                {
                    string timestamp2 = DateTime.Now.ToString("HH:mm:ss");
                    _selectedServer.ConsoleLines.Add(new ConsoleLine
                    {
                        Text = $"[{timestamp2}] Error: Too many parameters. Usage: {cmdDef.Signature}",
                        RawText = $"Error: Too many parameters for {cmdName}",
                        ColorBrush = new SolidColorBrush(Color.Parse("#ef5350")),
                        Weight = Avalonia.Media.FontWeight.Bold
                    });
                    var list2 = this.FindControl<ListBox>("ConsoleLogList");
                    if (list2 != null && list2.ItemCount > 0)
                        list2.ScrollIntoView(list2.ItemCount - 1);
                    return;
                }
            }

            // Add to command history
            if (_commandHistory.Count == 0 || _commandHistory[_commandHistory.Count - 1] != cmd)
                _commandHistory.Add(cmd);
            _historyIndex = -1;

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            var line = new ConsoleLine
            {
                Text = $"[{timestamp}] > {cmd}",
                RawText = cmd,
                ColorBrush = new SolidColorBrush(Color.Parse("#ffab40")),
                Weight = Avalonia.Media.FontWeight.Bold
            };
            _selectedServer.ConsoleLines.Add(line);
            _selectedServer.ConsoleLogger?.WriteLine(line.Text);
            consoleInput.Text = "";

            var list = this.FindControl<ListBox>("ConsoleLogList");
            if (list != null && list.ItemCount > 0)
                list.ScrollIntoView(list.ItemCount - 1);

            client.SendRequest(words);
        }

        private void OnConsoleInputTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            var consoleInput = sender as TextBox;
            var suggestionsBox = this.FindControl<ListBox>("ConsoleSuggestions");
            if (consoleInput == null || suggestionsBox == null) return;

            string text = consoleInput.Text ?? "";
            string prefix = text.Split(' ')[0];

            if (prefix.Length >= 2)
            {
                var matches = new List<string>();
                foreach (var cmd in RconCommands)
                {
                    if (cmd.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && IsCommandForCurrentGame(cmd))
                        matches.Add(cmd.Signature);
                }

                if (matches.Count > 0 && matches.Count <= 20)
                {
                    suggestionsBox.ItemsSource = matches;
                    suggestionsBox.IsVisible = true;
                }
                else
                {
                    suggestionsBox.IsVisible = false;
                }
            }
            else
            {
                suggestionsBox.IsVisible = false;
            }
        }

        private void OnSuggestionSelected(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var suggestionsBox = sender as ListBox;
            var consoleInput = this.FindControl<TextBox>("ConsoleInput");
            if (suggestionsBox?.SelectedItem == null || consoleInput == null) return;

            // Extract just the command name from the signature
            string selected = suggestionsBox.SelectedItem.ToString();
            string cmdName = selected.Split(' ')[0];
            consoleInput.Text = cmdName + " ";
            consoleInput.CaretIndex = consoleInput.Text.Length;
            suggestionsBox.IsVisible = false;
            consoleInput.Focus();
        }

        private void OnCopyConsoleSelected(object sender, RoutedEventArgs e)
        {
            var list = this.FindControl<ListBox>("ConsoleLogList");
            if (list?.SelectedItems == null) return;
            var sb = new StringBuilder();
            foreach (var item in list.SelectedItems)
                if (item is ConsoleLine line) sb.AppendLine(line.Text);
            if (sb.Length > 0)
                TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(sb.ToString());
        }

        private void OnCopyConsoleAll(object sender, RoutedEventArgs e)
        {
            if (_selectedServer == null) return;
            var sb = new StringBuilder();
            foreach (var line in _selectedServer.ConsoleLines)
                sb.AppendLine(line.Text);
            if (sb.Length > 0)
                TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(sb.ToString());
        }

        private void OnClearConsole(object sender, RoutedEventArgs e)
        {
            if (_selectedServer == null) return;
            _selectedServer.ConsoleLines.Clear();
        }

        // --- UI Helpers ---

        private void OnThemeToggle(object sender, RoutedEventArgs e) => App.ThemeManager.ToggleTheme();

        private void UpdateStatus(string color, string text)
        {
            var indicator = this.FindControl<Avalonia.Controls.Shapes.Ellipse>("StatusIndicator");
            var statusText = this.FindControl<TextBlock>("StatusText");
            if (indicator != null)
            {
                indicator.Fill = new SolidColorBrush(Color.Parse(color));
                // Pulse when connected (green) or connecting (orange)
                bool shouldPulse = color == "#66bb6a" || color == "#ffab40";
                if (shouldPulse && !indicator.Classes.Contains("pulse"))
                    indicator.Classes.Add("pulse");
                else if (!shouldPulse)
                    indicator.Classes.Remove("pulse");
            }
            if (statusText != null) statusText.Text = text;
        }

        private void UpdateServerInfoPanel(string title, string details)
        {
            // Status is shown in bottom bar and dashboard — this is now a no-op
        }

        private void UpdateContentVisibility()
        {
            bool connected = _selectedServer != null &&
                (_selectedServer.State == ServerConnectionState.Connected ||
                 _selectedServer.State == ServerConnectionState.Connecting);

            var overlay = this.FindControl<Border>("DisconnectedOverlay");
            var tabBar = this.FindControl<WrapPanel>("TabBar");

            if (overlay != null) overlay.IsVisible = !connected;
            if (tabBar != null) tabBar.IsVisible = connected;

            // Hide all tab content when disconnected
            if (!connected)
            {
                for (int i = 1; i <= 13; i++)
                {
                    var tab = this.FindControl<Border>($"Tab{i}");
                    if (tab != null) tab.IsVisible = false;
                }
            }

            // Update subtext
            var subtext = this.FindControl<TextBlock>("DisconnectedSubtext");
            if (subtext != null)
            {
                if (_selectedServer == null)
                    subtext.Text = "Select a server from the sidebar or add a new one to get started.";
                else
                    subtext.Text = $"Disconnected from {_selectedServer.DisplayName}. Click Connect in the sidebar to reconnect.";
            }
        }

        private void UpdateDashboard(ServerEntry entry, FrostbiteClient game)
        {
            var info = entry.LastServerInfo;
            if (info == null) return;

            var mapPanel = this.FindControl<Avalonia.Controls.Shapes.Ellipse>("StatusIndicator"); // just to check we have UI

            // Hero
            var dashName = this.FindControl<TextBlock>("DashServerName");
            if (dashName != null) dashName.Text = info.ServerName ?? "Unknown";

            var dashGame = this.FindControl<TextBlock>("DashGameType");
            if (dashGame != null) dashGame.Text = game?.GameType ?? entry.GameType ?? "??";

            var dashVer = this.FindControl<TextBlock>("DashServerVersion");
            if (dashVer != null) dashVer.Text = entry.GameVersion ?? "??";

            var dashMap = this.FindControl<TextBlock>("DashMapMode");
            string mapName = GameData.GetMapName(info.Map ?? "") ?? info.Map ?? "Unknown";
            string modeName = GameData.GetModeName(info.GameMode ?? "") ?? info.GameMode ?? "Unknown";
            if (dashMap != null) dashMap.Text = $"{mapName} — {modeName}";

            var dashPlayers = this.FindControl<TextBlock>("DashPlayerCount");
            if (dashPlayers != null) dashPlayers.Text = $"{info.PlayerCount}/{info.MaxPlayerCount}";

            // Badges
            var rankedBadge = this.FindControl<Border>("DashRankedBadge");
            var rankedText = this.FindControl<TextBlock>("DashRankedText");
            if (rankedBadge != null)
            {
                rankedBadge.Background = new SolidColorBrush(Color.Parse(info.Ranked ? "#66bb6a" : "#ef5350"));
                if (rankedText != null) rankedText.Text = info.Ranked ? "RANKED" : "UNRANKED";
            }
            var pbBadge = this.FindControl<Border>("DashPbBadge");
            if (pbBadge != null) pbBadge.IsVisible = info.PunkBuster;

            // Stats cards
            var dashRound = this.FindControl<TextBlock>("DashRound");
            if (dashRound != null) dashRound.Text = $"{info.CurrentRound + 1} / {info.TotalRounds}";

            var dashUptime = this.FindControl<TextBlock>("DashUptime");
            if (dashUptime != null && info.ServerUptime > 0)
            {
                var ts = TimeSpan.FromSeconds(info.ServerUptime);
                dashUptime.Text = ts.TotalHours >= 24
                    ? $"{(int)ts.TotalDays}d {ts.Hours}h"
                    : ts.TotalHours >= 1
                        ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
                        : $"{ts.Minutes}m";
            }

            var dashRoundTime = this.FindControl<TextBlock>("DashRoundTime");
            if (dashRoundTime != null && info.RoundTime > 0)
            {
                var rt = TimeSpan.FromSeconds(info.RoundTime);
                dashRoundTime.Text = rt.TotalHours >= 1 ? $"{(int)rt.TotalHours}h {rt.Minutes}m" : $"{rt.Minutes}m {rt.Seconds}s";
            }

            var dashRegion = this.FindControl<TextBlock>("DashRegion");
            if (dashRegion != null) dashRegion.Text = info.ServerCountry ?? info.ServerRegion ?? info.PingSite ?? "--";

            // Team scores
            if (info.TeamScores != null)
            {
                for (int i = 0; i < info.TeamScores.Count && i < 2; i++)
                {
                    var scoreText = this.FindControl<TextBlock>($"DashTeam{i + 1}Score");
                    if (scoreText != null)
                        scoreText.Text = info.TeamScores[i].Score.ToString("F0");
                }
            }

            // Connection details
            var connInfo = this.FindControl<TextBlock>("DashConnectionInfo");
            if (connInfo != null)
            {
                connInfo.Text = $"IP: {entry.HostPort}\n" +
                                $"Game: {game?.GameType ?? "?"} {entry.GameVersion}\n" +
                                $"Build: {game?.VersionNumber ?? "?"}\n" +
                                $"PunkBuster: {(info.PunkBuster ? $"Active ({info.PunkBusterVersion})" : "Inactive")}\n" +
                                $"Password: {(info.Passworded ? "Yes" : "No")}\n" +
                                $"Join Queue: {(info.JoinQueueEnabled ? "Enabled" : "Disabled")}\n" +
                                $"External: {info.ExternalGameIpandPort ?? "N/A"}";
            }

            // Kill feed
            var killList = this.FindControl<ListBox>("KillFeedList");
            if (killList != null) killList.ItemsSource = entry.KillFeed;

            // Player graph
            DrawPlayerGraph(entry);
        }

        private void DrawPlayerGraph(ServerEntry entry)
        {
            var canvas = this.FindControl<Canvas>("PlayerGraphCanvas");
            if (canvas == null || entry.PlayerHistory.Count < 2) return;

            canvas.Children.Clear();

            double w = canvas.Bounds.Width > 0 ? canvas.Bounds.Width : 300;
            double h = canvas.Bounds.Height > 0 ? canvas.Bounds.Height : 120;
            int maxPlayers = entry.LastServerInfo?.MaxPlayerCount ?? 64;
            if (maxPlayers <= 0) maxPlayers = 64;

            var points = entry.PlayerHistory;
            double xStep = w / Math.Max(points.Count - 1, 1);

            // Draw grid lines
            for (int i = 0; i <= 4; i++)
            {
                double y = h - (h * i / 4.0);
                var gridLine = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Avalonia.Point(0, y),
                    EndPoint = new Avalonia.Point(w, y),
                    Stroke = new SolidColorBrush(Color.Parse("#1a2a3a")),
                    StrokeThickness = 1
                };
                canvas.Children.Add(gridLine);

                // Label
                int val = maxPlayers * i / 4;
                var label = new TextBlock
                {
                    Text = val.ToString(),
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.Parse("#556677"))
                };
                Canvas.SetLeft(label, 2);
                Canvas.SetTop(label, y - 12);
                canvas.Children.Add(label);
            }

            // Draw filled area + line
            var geometry = new Avalonia.Media.StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Avalonia.Point(0, h), true);
                for (int i = 0; i < points.Count; i++)
                {
                    double x = i * xStep;
                    double y = h - (h * Math.Min(points[i].Count, maxPlayers) / (double)maxPlayers);
                    ctx.LineTo(new Avalonia.Point(x, y));
                }
                ctx.LineTo(new Avalonia.Point((points.Count - 1) * xStep, h));
                ctx.EndFigure(true);
            }

            // Fill
            var fill = new Avalonia.Controls.Shapes.Path
            {
                Data = geometry,
                Fill = new SolidColorBrush(Color.Parse("#1a4fc3f7")),
            };
            canvas.Children.Add(fill);

            // Line on top
            var lineGeometry = new Avalonia.Media.StreamGeometry();
            using (var ctx = lineGeometry.Open())
            {
                double x0 = 0, y0 = h - (h * Math.Min(points[0].Count, maxPlayers) / (double)maxPlayers);
                ctx.BeginFigure(new Avalonia.Point(x0, y0), false);
                for (int i = 1; i < points.Count; i++)
                {
                    double x = i * xStep;
                    double y = h - (h * Math.Min(points[i].Count, maxPlayers) / (double)maxPlayers);
                    ctx.LineTo(new Avalonia.Point(x, y));
                }
                ctx.EndFigure(false);
            }

            var line = new Avalonia.Controls.Shapes.Path
            {
                Data = lineGeometry,
                Stroke = new SolidColorBrush(Color.Parse("#4fc3f7")),
                StrokeThickness = 2
            };
            canvas.Children.Add(line);

            // Current value dot
            if (points.Count > 0)
            {
                double lastX = (points.Count - 1) * xStep;
                double lastY = h - (h * Math.Min(points[points.Count - 1].Count, maxPlayers) / (double)maxPlayers);
                var dot = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 6, Height = 6,
                    Fill = new SolidColorBrush(Color.Parse("#4fc3f7"))
                };
                Canvas.SetLeft(dot, lastX - 3);
                Canvas.SetTop(dot, lastY - 3);
                canvas.Children.Add(dot);
            }

            // Time range label
            var rangeLabel = this.FindControl<TextBlock>("DashGraphRange");
            if (rangeLabel != null && points.Count > 1)
            {
                var span = points[points.Count - 1].Time - points[0].Time;
                rangeLabel.Text = span.TotalMinutes < 2 ? "Just started" : $"Last {(int)span.TotalMinutes} minutes";
            }
        }

        private void UpdateSidebarButtons()
        {
            bool hasSelection = _selectedServer != null;
            bool connected = _selectedServer?.IsConnected == true;
            bool connecting = _selectedServer?.State == ServerConnectionState.Connecting;
            ShowConnectButton(hasSelection && !connected && !connecting);
            ShowDisconnectButton(hasSelection && (connected || connecting));
            ShowRemoveButton(hasSelection);
        }

        private void ShowConnectButton(bool show)
        {
            var btn = this.FindControl<Button>("ConnectSelectedButton");
            if (btn != null) btn.IsVisible = show;
        }

        private void ShowDisconnectButton(bool show)
        {
            var btn = this.FindControl<Button>("DisconnectButton");
            if (btn != null) btn.IsVisible = show;
        }

        private void ShowRemoveButton(bool show)
        {
            var btn = this.FindControl<Button>("RemoveServerButton");
            if (btn != null) btn.IsVisible = show;
        }

        private void UpdateConnectionCount()
        {
            var text = this.FindControl<TextBlock>("ConnectionCountText");
            if (text == null) return;

            int connected = 0;
            foreach (var s in _servers)
                if (s.IsConnected) connected++;

            text.Text = $"{connected}/{_servers.Count} connected";
        }

        private void SortAndGroupServers()
        {
            // Sort: by GameType then ServerName/HostPort
            var sorted = _servers.OrderBy(s => s.GameType ?? "ZZZ")
                                 .ThenBy(s => s.DisplayName ?? s.HostPort)
                                 .ToList();

            // Reorder the collection to match
            for (int i = 0; i < sorted.Count; i++)
            {
                int currentIndex = _servers.IndexOf(sorted[i]);
                if (currentIndex != i)
                    _servers.Move(currentIndex, i);
            }

            // Update group headers: show header on first item of each game type
            string lastGame = null;
            foreach (var s in _servers)
            {
                string game = s.GameType ?? "Unknown";
                s.ShowGameHeader = game != lastGame;
                lastGame = game;
            }
        }

        private void UpdateTeamPanels(ServerEntry entry)
        {
            bool hasTeam3 = entry.TeamPlayers.ContainsKey(3) && entry.TeamPlayers[3].Count > 0;
            bool hasTeam4 = entry.TeamPlayers.ContainsKey(4) && entry.TeamPlayers[4].Count > 0;
            bool hasFourTeams = hasTeam3 || hasTeam4;

            // Update grid layout: 2 cols always, add second row only for 4-team modes
            var teamGrid = this.FindControl<Grid>("TeamGrid");
            if (teamGrid != null)
            {
                teamGrid.RowDefinitions.Clear();
                if (hasFourTeams)
                {
                    teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                    teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                }
                else
                {
                    teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                }
            }

            for (int t = 1; t <= 4; t++)
            {
                var teamList = this.FindControl<ListBox>($"TeamList{t}");
                var teamHeader = this.FindControl<TextBlock>($"TeamHeader{t}");
                var teamPanel = this.FindControl<Border>($"TeamPanel{t}");

                var players = entry.TeamPlayers.ContainsKey(t) ? entry.TeamPlayers[t] : new List<PlayerDisplayInfo>();

                if (teamList != null)
                    teamList.ItemsSource = players;

                if (teamHeader != null)
                    teamHeader.Text = $"Team {t} ({players.Count})";

                // Show teams 3 and 4 only if they have players
                if (t >= 3 && teamPanel != null)
                    teamPanel.IsVisible = players.Count > 0;
            }
        }

        private void RefreshPlayerList()
        {
            var client = SelectedClient;
            if (client?.Game != null)
                client.Game.SendAdminListPlayersPacket(new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
        }
    }
}
