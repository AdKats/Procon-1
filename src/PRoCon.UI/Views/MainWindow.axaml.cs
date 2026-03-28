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
            set { _serverName = value; Notify(nameof(ServerName)); Notify(nameof(DisplayLabel)); Notify(nameof(DisplayName)); }
        }

        private string _gameType;
        public string GameType
        {
            get => _gameType;
            set { _gameType = value; Notify(nameof(GameType)); Notify(nameof(DisplayLabel)); Notify(nameof(GameTypeLabel)); }
        }

        public string GameTypeLabel => !string.IsNullOrEmpty(GameType) ? $"[{GameType}]" : "";

        private ServerConnectionState _state = ServerConnectionState.Disconnected;
        public ServerConnectionState State
        {
            get => _state;
            set { _state = value; Notify(nameof(State)); Notify(nameof(IsConnected)); Notify(nameof(StatusColor)); Notify(nameof(DisplayName)); }
        }

        public bool IsConnected => _state == ServerConnectionState.Connected;

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
        public Dictionary<int, List<PlayerDisplayInfo>> TeamPlayers { get; set; } = new Dictionary<int, List<PlayerDisplayInfo>>
        {
            { 1, new List<PlayerDisplayInfo>() },
            { 2, new List<PlayerDisplayInfo>() },
            { 3, new List<PlayerDisplayInfo>() },
            { 4, new List<PlayerDisplayInfo>() }
        };
        public string ServerInfoText { get; set; } = "";
        public CServerInfo LastServerInfo { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(ServerName))
                    return $"{ServerName} ({HostPort})";
                return HostPort;
            }
        }

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
        private int _activeTab = 0;
        private readonly ObservableCollection<ServerEntry> _servers = new ObservableCollection<ServerEntry>();
        private readonly Dictionary<string, ServerEntry> _serverLookup = new Dictionary<string, ServerEntry>(StringComparer.OrdinalIgnoreCase);

        // Console command history & autocomplete
        private readonly List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;
        private static readonly string[] RconCommands = {
            "admin.effectiveMaxPlayers", "admin.eventsEnabled", "admin.help", "admin.kickPlayer",
            "admin.killPlayer", "admin.listPlayers", "admin.movePlayer", "admin.password",
            "admin.say", "admin.shutDown", "admin.yell",
            "banList.add", "banList.clear", "banList.list", "banList.load", "banList.remove", "banList.save",
            "currentLevel",
            "fairFight.activate", "fairFight.deactivate", "fairFight.isActive",
            "listPlayers",
            "login.hashed", "login.plainText", "logout",
            "mapList.add", "mapList.availableMaps", "mapList.clear", "mapList.endRound",
            "mapList.getMapIndices", "mapList.getRounds", "mapList.list",
            "mapList.load", "mapList.remove", "mapList.restartRound",
            "mapList.runNextRound", "mapList.save", "mapList.setNextMapIndex",
            "player.idleDuration", "player.isAlive", "player.ping",
            "punkBuster.activate", "punkBuster.isActive", "punkBuster.pb_sv_command",
            "reservedSlotsList.add", "reservedSlotsList.aggressiveJoin",
            "reservedSlotsList.clear", "reservedSlotsList.list",
            "reservedSlotsList.load", "reservedSlotsList.remove", "reservedSlotsList.save",
            "serverInfo", "server.type", "version",
            "vars.3dSpotting", "vars.3pCam", "vars.alwaysAllowSpectators",
            "vars.autoBalance", "vars.bulletDamage", "vars.commander",
            "vars.crossHair", "vars.forceReloadWholeMags", "vars.friendlyFire",
            "vars.gameModeCounter", "vars.gamePassword", "vars.hitIndicatorsEnabled",
            "vars.hud", "vars.idleBanRounds", "vars.idleTimeout",
            "vars.killCam", "vars.maxPlayers", "vars.maxSpectators",
            "vars.miniMap", "vars.miniMapSpotting", "vars.mpExperience",
            "vars.nameTag", "vars.onlySquadLeaderSpawn", "vars.playerRespawnTime",
            "vars.preset", "vars.regenerateHealth", "vars.roundLockdownCountdown",
            "vars.roundRestartPlayerCount", "vars.roundStartPlayerCount",
            "vars.roundTimeLimit", "vars.roundWarmupTimeout",
            "vars.serverDescription", "vars.serverMessage", "vars.serverName",
            "vars.serverType", "vars.soldierHealth", "vars.teamFactionOverride",
            "vars.teamKillCountForKick", "vars.teamKillKickForBan",
            "vars.teamKillValueDecreasePerSecond", "vars.teamKillValueForKick",
            "vars.teamKillValueIncrease", "vars.ticketBleedRate",
            "vars.unlockMode", "vars.vehicleSpawnAllowed", "vars.vehicleSpawnDelay"
        };

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

        private void EnsureServer(string host, ushort port, string password)
        {
            string hostPort = $"{host}:{port}";
            if (!_application.Connections.Contains(hostPort))
                _application.AddConnection(host, port, "default", password);
            var entry = EnsureServerEntry(hostPort);
            var client = GetClient(hostPort);
            if (client != null)
                WireClientEvents(client, entry);
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
                    UpdateServerInfoPanel("Connecting...", "TCP connection attempt...");
                    UpdateSidebarButtons();
                }
            });

            client.ConnectSuccess += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Connecting;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ffab40", "Connected, logging in...");
                    UpdateServerInfoPanel("Connected", "Authenticating...");
                }
            });

            client.ConnectionClosed += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Disconnected;
                if (_selectedServer == entry)
                {
                    UpdateStatus("#ef5350", "Disconnected");
                    UpdateServerInfoPanel("Disconnected", "Connection was closed.");
                    UpdateSidebarButtons();
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
                    UpdateServerInfoPanel($"Logged in: {entry.HostPort}", "Waiting for server info...");
                    UpdateSidebarButtons();
                }
            });

            client.Logout += sender => OnClientEvent(entry, () =>
            {
                entry.State = ServerConnectionState.Disconnected;
                if (_selectedServer == entry)
                    UpdateStatus("#ffab40", "Logged out");
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

        private static ConsoleLine ParseConsoleLine(string rawText, string displayPrefix)
        {
            string cleanText = ColorCodeRegex.Replace(rawText, "");
            string displayText = $"{displayPrefix}{cleanText}";

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
                entry.ServerInfoText = $"Server Name: {name}\n" +
                                       $"Map: {map}\n" +
                                       $"Game Mode: {mode}\n" +
                                       $"Players: {info.PlayerCount}/{info.MaxPlayerCount}\n" +
                                       $"Round: {info.CurrentRound + 1}/{info.TotalRounds}\n" +
                                       $"Game Type: {sender.GameType}";

                if (_selectedServer == entry)
                {
                    UpdateStatus("#66bb6a", name);
                    UpdateServerInfoPanel(name, $"{map} — {mode} — {info.PlayerCount}/{info.MaxPlayerCount} players");
                    var details = this.FindControl<TextBlock>("ServerInfoDetails");
                    if (details != null) details.Text = entry.ServerInfoText;
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

        private void OnShowConnectForm(object sender, RoutedEventArgs e) => SwitchTab(0);

        private void OnServerSelected(object sender, SelectionChangedEventArgs e)
        {
            var serverList = this.FindControl<ListBox>("ServerList");
            if (serverList?.SelectedItem is not ServerEntry entry) return;

            _selectedServer = entry;
            ShowRemoveButton(true);

            // Load this server's state into the view
            LoadServerView(entry);
            UpdateSidebarButtons();
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

        private void OnConnect(object sender, RoutedEventArgs e)
        {
            var host = this.FindControl<TextBox>("HostInput")?.Text ?? "";
            var portText = this.FindControl<TextBox>("PortInput")?.Text ?? "";
            var password = this.FindControl<TextBox>("PasswordInput")?.Text ?? "";

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portText)) return;
            if (!ushort.TryParse(portText, out ushort port)) return;

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

                var serverList = this.FindControl<ListBox>("ServerList");
                if (serverList != null) serverList.SelectedItem = entry;

                client.AutomaticallyConnect = true;
                UpdateSidebarButtons();
                UpdateConnectionCount();
            }
            catch (Exception ex)
            {
                UpdateStatus("#ef5350", $"Error: {ex.Message}");
            }
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
            UpdateServerInfoPanel("Disconnected", "Connection closed.");
            UpdateSidebarButtons();
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
        }

        // --- Load Server View (switch main panel to selected server's data) ---

        private void OnSaveConnection(object sender, RoutedEventArgs e)
        {
            // Save updated connection details for the selected server
            if (_selectedServer == null) return;

            var host = this.FindControl<TextBox>("HostInput")?.Text ?? "";
            var portText = this.FindControl<TextBox>("PortInput")?.Text ?? "";
            var password = this.FindControl<TextBox>("PasswordInput")?.Text ?? "";

            // For now just update the display — full reconnect with new details would need
            // removing old connection and creating new one
            UpdateStatus("#ffab40", $"Connection settings updated for {_selectedServer.HostPort}");
        }

        private void LoadServerView(ServerEntry entry)
        {
            var client = GetClient(entry.HostPort);
            bool connected = entry.IsConnected && client?.Game != null;

            // Populate connection tab with this server's details
            var parts = entry.HostPort.Split(':');
            var hostInput = this.FindControl<TextBox>("HostInput");
            var portInput = this.FindControl<TextBox>("PortInput");
            var titleText = this.FindControl<TextBlock>("ConnectionTabTitle");
            var subtitleText = this.FindControl<TextBlock>("ConnectionTabSubtitle");
            var saveBtn = this.FindControl<Button>("SaveConnectionButton");

            if (hostInput != null && parts.Length >= 1) hostInput.Text = parts[0];
            if (portInput != null && parts.Length >= 2) portInput.Text = parts[1];
            if (titleText != null) titleText.Text = entry.DisplayLabel;
            if (subtitleText != null)
            {
                if (connected)
                    subtitleText.Text = $"Connected — {entry.GameType ?? "Unknown"} — {entry.HostPort}";
                else
                    subtitleText.Text = $"Disconnected — {entry.HostPort}";
            }
            if (saveBtn != null) saveBtn.IsVisible = true;

            // Switch to Chat tab automatically when connected (skip connection tab)
            if (connected && _activeTab == 0)
                SwitchTab(1);

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

            // Server Info tab
            var details = this.FindControl<TextBlock>("ServerInfoDetails");
            if (details != null) details.Text = entry.ServerInfoText;

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
                    // Pick selected or first suggestion
                    string selected = (suggestionsBox.SelectedItem ?? suggestionsBox.Items.Cast<object>().First())?.ToString();
                    if (selected != null)
                    {
                        consoleInput.Text = selected + " ";
                        consoleInput.CaretIndex = consoleInput.Text.Length;
                        suggestionsBox.IsVisible = false;
                    }
                }
                else
                {
                    // Try direct completion
                    string text = consoleInput.Text ?? "";
                    string prefix = text.Split(' ')[0];
                    var matches = new List<string>();
                    foreach (string cmd in RconCommands)
                    {
                        if (cmd.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            matches.Add(cmd);
                    }
                    if (matches.Count == 1)
                    {
                        consoleInput.Text = matches[0] + " ";
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

            string cmd = consoleInput.Text;

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

            var words = new List<string>(cmd.Split(' '));
            client.SendRequest(words);
        }

        private void OnConsoleInputTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            var consoleInput = sender as TextBox;
            var suggestionsBox = this.FindControl<ListBox>("ConsoleSuggestions");
            if (consoleInput == null || suggestionsBox == null) return;

            string text = consoleInput.Text ?? "";
            string prefix = text.Split(' ')[0];

            if (prefix.Length >= 2 && !text.Contains(' '))
            {
                var matches = new List<string>();
                foreach (string cmd in RconCommands)
                {
                    if (cmd.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        matches.Add(cmd);
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

            string selected = suggestionsBox.SelectedItem.ToString();
            consoleInput.Text = selected + " ";
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
            if (indicator != null) indicator.Fill = new SolidColorBrush(Color.Parse(color));
            if (statusText != null) statusText.Text = text;
        }

        private void UpdateServerInfoPanel(string title, string details)
        {
            var panel = this.FindControl<Border>("ServerInfoPanel");
            var nameText = this.FindControl<TextBlock>("ServerNameText");
            var detailsText = this.FindControl<TextBlock>("ServerDetailsText");
            if (panel != null) panel.IsVisible = !string.IsNullOrEmpty(title);
            if (nameText != null) nameText.Text = title;
            if (detailsText != null) detailsText.Text = details;
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

        private void UpdateTeamPanels(ServerEntry entry)
        {
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
