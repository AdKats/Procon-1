using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using PRoCon.Core;
using PRoCon.Core.Logging;
using PRoCon.Core.Players;
using PRoCon.Core.Remote;
using PRoCon.UI.Models;
using PRoCon.UI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PRoCon.UI.Views
{
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
        private SpectatorListPanel _spectatorListPanel;
        private PunkBusterPanel _punkBusterPanel;
        private TextChatModerationPanel _textChatModerationPanel;
        private OptionsPanel _optionsPanel;

        // Cached control references (populated by CacheControls)
        private ListBox _serverList;
        private ContentControl _mapListContent;
        private ContentControl _banListContent;
        private ContentControl _reservedSlotsContent;
        private ContentControl _pluginsContent;
        private ContentControl _accountsContent;
        private ContentControl _eventsContent;
        private ContentControl _serverSettingsContent;
        private ContentControl _layerContent;
        private ListBox _consoleLogList;
        private TextBlock _chatLog;
        private ScrollViewer _chatScroller;
        private TextBox _chatInput;
        private ListBox _killFeedList;
        private Canvas _playerGraphCanvas;
        private TextBlock _statusText;
        private Avalonia.Controls.Shapes.Ellipse _statusIndicator;
        private Border _disconnectedOverlay;
        private Border _tabBarBorder;
        private WrapPanel _tabBar;
        private Button _layerTabButton;
        private TextBlock _overlayTitle;
        private TextBlock _overlayIcon;
        private TextBlock _disconnectedSubtext;
        private TextBlock _dashServerName;
        private TextBlock _dashGameType;
        private TextBlock _dashServerVersion;
        private TextBlock _dashMapMode;
        private TextBlock _dashPlayerCount;
        private Border _dashRankedBadge;
        private TextBlock _dashRankedText;
        private Border _dashPbBadge;
        private TextBlock _dashRound;
        private TextBlock _dashUptime;
        private TextBlock _dashRoundTime;
        private TextBlock _dashRegion;
        private TextBlock _dashConnectionInfo;
        private TextBlock _dashGraphRange;
        private Button _connectSelectedButton;
        private Button _disconnectButton;
        private Button _removeServerButton;
        private TextBlock _connectionCountText;
        private TextBlock _landingServerCount;
        private TextBlock _landingConnectedCount;
        private TextBlock _landingTotalPlayers;
        private Grid _teamGrid;
        private TextBox _consoleInput;
        private ListBox _consoleSuggestions;

        // Array-cached controls for indexed lookups
        private Border[] _tabs; // Tab0..Tab13
        private ListBox[] _teamLists; // TeamList1..TeamList4
        private TextBlock[] _teamHeaders; // TeamHeader1..TeamHeader4
        private Border[] _teamPanels; // TeamPanel1..TeamPanel4
        private TextBlock[] _dashTeamScores; // DashTeam1Score, DashTeam2Score

        private void CacheControls()
        {
            _serverList = this.FindControl<ListBox>("ServerList");
            _mapListContent = this.FindControl<ContentControl>("MapListContent");
            _banListContent = this.FindControl<ContentControl>("BanListContent");
            _reservedSlotsContent = this.FindControl<ContentControl>("ReservedSlotsContent");
            _pluginsContent = this.FindControl<ContentControl>("PluginsContent");
            _accountsContent = this.FindControl<ContentControl>("AccountsContent");
            _eventsContent = this.FindControl<ContentControl>("EventsContent");
            _serverSettingsContent = this.FindControl<ContentControl>("ServerSettingsContent");
            _layerContent = this.FindControl<ContentControl>("LayerContent");
            _consoleLogList = this.FindControl<ListBox>("ConsoleLogList");
            _chatLog = this.FindControl<TextBlock>("ChatLog");
            _chatScroller = this.FindControl<ScrollViewer>("ChatScroller");
            _chatInput = this.FindControl<TextBox>("ChatInput");
            _killFeedList = this.FindControl<ListBox>("KillFeedList");
            _playerGraphCanvas = this.FindControl<Canvas>("PlayerGraphCanvas");
            _statusText = this.FindControl<TextBlock>("StatusText");
            _statusIndicator = this.FindControl<Avalonia.Controls.Shapes.Ellipse>("StatusIndicator");
            _disconnectedOverlay = this.FindControl<Border>("DisconnectedOverlay");
            _tabBarBorder = this.FindControl<Border>("TabBarBorder");
            _tabBar = this.FindControl<WrapPanel>("TabBar");
            _layerTabButton = this.FindControl<Button>("LayerTabButton");
            _overlayTitle = this.FindControl<TextBlock>("OverlayTitle");
            _overlayIcon = this.FindControl<TextBlock>("OverlayIcon");
            _disconnectedSubtext = this.FindControl<TextBlock>("DisconnectedSubtext");
            _dashServerName = this.FindControl<TextBlock>("DashServerName");
            _dashGameType = this.FindControl<TextBlock>("DashGameType");
            _dashServerVersion = this.FindControl<TextBlock>("DashServerVersion");
            _dashMapMode = this.FindControl<TextBlock>("DashMapMode");
            _dashPlayerCount = this.FindControl<TextBlock>("DashPlayerCount");
            _dashRankedBadge = this.FindControl<Border>("DashRankedBadge");
            _dashRankedText = this.FindControl<TextBlock>("DashRankedText");
            _dashPbBadge = this.FindControl<Border>("DashPbBadge");
            _dashRound = this.FindControl<TextBlock>("DashRound");
            _dashUptime = this.FindControl<TextBlock>("DashUptime");
            _dashRoundTime = this.FindControl<TextBlock>("DashRoundTime");
            _dashRegion = this.FindControl<TextBlock>("DashRegion");
            _dashConnectionInfo = this.FindControl<TextBlock>("DashConnectionInfo");
            _dashGraphRange = this.FindControl<TextBlock>("DashGraphRange");
            _connectSelectedButton = this.FindControl<Button>("ConnectSelectedButton");
            _disconnectButton = this.FindControl<Button>("DisconnectButton");
            _removeServerButton = this.FindControl<Button>("RemoveServerButton");
            _connectionCountText = this.FindControl<TextBlock>("ConnectionCountText");
            _landingServerCount = this.FindControl<TextBlock>("LandingServerCount");
            _landingConnectedCount = this.FindControl<TextBlock>("LandingConnectedCount");
            _landingTotalPlayers = this.FindControl<TextBlock>("LandingTotalPlayers");
            _teamGrid = this.FindControl<Grid>("TeamGrid");
            _consoleInput = this.FindControl<TextBox>("ConsoleInput");
            _consoleSuggestions = this.FindControl<ListBox>("ConsoleSuggestions");

            // Array-cached controls
            _tabs = new Border[16];
            for (int i = 0; i <= 15; i++)
                _tabs[i] = this.FindControl<Border>($"Tab{i}");

            _teamLists = new ListBox[4];
            _teamHeaders = new TextBlock[4];
            _teamPanels = new Border[4];
            for (int t = 0; t < 4; t++)
            {
                _teamLists[t] = this.FindControl<ListBox>($"TeamList{t + 1}");
                _teamHeaders[t] = this.FindControl<TextBlock>($"TeamHeader{t + 1}");
                _teamPanels[t] = this.FindControl<Border>($"TeamPanel{t + 1}");
            }

            _dashTeamScores = new TextBlock[2];
            _dashTeamScores[0] = this.FindControl<TextBlock>("DashTeam1Score");
            _dashTeamScores[1] = this.FindControl<TextBlock>("DashTeam2Score");
        }

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
                _spectatorListPanel = new SpectatorListPanel();
                _punkBusterPanel = new PunkBusterPanel();
                _textChatModerationPanel = new TextChatModerationPanel();
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

        protected override void OnClosing(Avalonia.Controls.WindowClosingEventArgs e)
        {
            // IPCheckService disposed by PRoConApplication.Shutdown()
            foreach (var entry in _servers)
            {
                entry.ConsoleLogger?.Dispose();
                entry.ConsoleLogger = null;
            }
            try { _application?.Shutdown(); } catch { }
            base.OnClosing(e);
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

            // Note: CacheControls() is called at the end of this method.
            // For these early assignments, use FindControl directly since cache isn't populated yet.
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
            var spectatorContent = this.FindControl<ContentControl>("SpectatorContent");
            if (spectatorContent != null) spectatorContent.Content = _spectatorListPanel;
            var pbContent = this.FindControl<ContentControl>("PunkBusterContent");
            if (pbContent != null) pbContent.Content = _punkBusterPanel;
            var textChatModContent = this.FindControl<ContentControl>("TextChatModerationContent");
            if (textChatModContent != null) textChatModContent.Content = _textChatModerationPanel;
            // Options panel is shown in a dialog, not embedded in tabs

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

            UpdateConnectionCount();
            UpdateContentVisibility();

            // IP check service is shared from PRoConApplication

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

            CacheControls();
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

            // Wire console RCON traffic — retry multiple times as Console may init late
            WireConsoleEvents(client, entry);
            foreach (int delay in new[] { 2000, 5000, 10000, 20000 })
            {
                System.Threading.Tasks.Task.Delay(delay).ContinueWith(_ =>
                    Dispatcher.UIThread.Post(() => WireConsoleEvents(client, entry)));
            }

            client.GameTypeDiscovered += sender =>
            {
                if (sender.Game != null)
                    WireGameEvents(sender.Game, entry);
                // Also try wiring console when game type discovered
                Dispatcher.UIThread.Post(() => WireConsoleEvents(client, entry));
            };

            // Wire PunkBuster player info for IP tracking
            client.PunkbusterPlayerInfo += (sender, pbInfo) =>
            {
                if (!string.IsNullOrEmpty(pbInfo.SoldierName) && !string.IsNullOrEmpty(pbInfo.Ip))
                {
                    string ip = pbInfo.Ip;
                    int colonIdx = ip.IndexOf(':');
                    if (colonIdx > 0) ip = ip.Substring(0, colonIdx);
                    entry.PlayerIPs[pbInfo.SoldierName] = ip;

                    // Async IP check — fire and forget, updates UI when done
                    if (_application?.IPCheckService != null)
                    {
                        _ = RunIPCheckAsync(entry, pbInfo.SoldierName, ip);
                    }
                }
            };

            if (client.Game != null)
                WireGameEvents(client.Game, entry);
        }

        private async System.Threading.Tasks.Task RunIPCheckAsync(ServerEntry entry, string soldierName, string ip)
        {
            try
            {
                var result = await _application?.IPCheckService.LookupAsync(ip);
                if (result == null) return;

                Dispatcher.UIThread.Post(() =>
                {
                    // Find the player in the team lists and update
                    for (int t = 1; t <= 4; t++)
                    {
                        if (!entry.TeamPlayers.ContainsKey(t)) continue;
                        foreach (var player in entry.TeamPlayers[t])
                        {
                            if (string.Equals(player.Name, soldierName, StringComparison.OrdinalIgnoreCase))
                            {
                                player.Country = result.CountryName;
                                player.CountryCode = result.CountryCode;
                                player.IsVPN = result.IsVPN;
                                player.IsProxy = result.IsProxy;
                                player.IP = ip;
                                break;
                            }
                        }
                    }
                });
            }
            catch { }
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
                string safeHostPort = client.HostNamePort.Replace(":", "_");
                string logDir = Path.Combine(PRoCon.Core.ProConPaths.LogsDirectory, safeHostPort);
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

                // Cap at ~2000 lines (batch trim to avoid N individual re-layouts)
                if (entry.ConsoleLines.Count > 2200)
                {
                    var keep = entry.ConsoleLines.Skip(entry.ConsoleLines.Count - 1500).ToList();
                    entry.ConsoleLines.Clear();
                    foreach (var item in keep)
                        entry.ConsoleLines.Add(item);
                }

                if (_selectedServer == entry)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            if (_consoleLogList != null && _consoleLogList.ItemCount > 0)
                                _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);
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
                UpdateLandingStats();
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

                // ObservableCollection auto-updates the UI — no need to reassign ItemsSource
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
            // Block Layer tab for layer connections
            if (index == 12 && _selectedServer?.IsLayerConnection == true)
                return;

            _activeTab = index;
            for (int i = 0; i <= 15; i++)
            {
                if (_tabs[i] != null) _tabs[i].IsVisible = (i == index);
            }

            if (_tabBar != null)
            {
                foreach (var child in _tabBar.Children)
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
                        if (_consoleLogList != null && _consoleLogList.ItemCount > 0)
                            _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);
                    }
                    catch { }
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        // --- Server Selection & Connection ---

        private void OnGoHome(object sender, RoutedEventArgs e)
        {
            // Deselect server, show landing page
            _selectedServer = null;
            if (_serverList != null) _serverList.SelectedItem = null;

            ClearServerContext();
            UpdateStatus("#8899aa", "PRoCon Frostbite 2.0");
            UpdateSidebarButtons();
            UpdateContentVisibility();
        }

        private async void OnShowConnectForm(object sender, RoutedEventArgs e)
        {
            var dialog = new AddServerDialog();
            await dialog.ShowDialog(this);

            if (!dialog.Confirmed) return;

            string host = dialog.Host;
            ushort port = dialog.Port;
            string password = dialog.Password;
            string username = dialog.Username;
            bool isLayer = dialog.IsLayerConnection;
            string hostPort = $"{host}:{port}";

            UpdateStatus("#ffab40", $"Connecting to {hostPort}...");

            try
            {
                if (isLayer)
                {
                    // Layer connection — connect to remote PRoCon via SignalR
                    // For now, use the same RCON connection path with username as the account
                    // The layer protocol will be handled at the PRoConClient level
                    var entry = EnsureServerEntry(hostPort);
                    entry.GameType = "Layer";
                    entry.IsLayerConnection = true;
                    entry.LayerUsername = username;

                    PRoConClient client;
                    if (_application.Connections.Contains(hostPort))
                    {
                        client = _application.Connections[hostPort];
                    }
                    else
                    {
                        client = _application.AddConnection(host, port, username, password);
                    }

                    if (client != null)
                    {
                        WireClientEvents(client, entry);
                        _selectedServer = entry;
                        entry.State = ServerConnectionState.Connecting;
                        client.AutomaticallyConnect = true;

                        if (_serverList != null) _serverList.SelectedItem = entry;

                        LoadServerView(entry);
                        UpdateSidebarButtons();
                        UpdateContentVisibility();
                        UpdateConnectionCount();

                        _application.SaveMainConfig();
                    }
                }
                else
                {
                    // Direct RCON connection
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

                    if (_serverList != null) _serverList.SelectedItem = entry;

                    LoadServerView(entry);
                    UpdateSidebarButtons();
                    UpdateContentVisibility();
                    UpdateConnectionCount();
                    SwitchTab(6);

                    _application.SaveMainConfig();
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus("#ef5350", $"Error: {ex.Message}");
            }
        }

        private void OnServerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_serverList?.SelectedItem is not ServerEntry entry) return;

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
            ShowConnectButton(false);
            ShowDisconnectButton(false);
            ShowRemoveButton(false);
            UpdateConnectionCount();
            _application.SaveMainConfig();

            // Go back to home — clear context and show landing page
            ClearServerContext();
            UpdateContentVisibility();

            if (_serverList != null) _serverList.SelectedItem = null;
        }

        // --- Load Server View (switch main panel to selected server's data) ---

        private void ClearServerContext()
        {
            // Clear all panels so stale data from previous server doesn't show
            _mapListPanel?.SetClient(null);
            _banListPanel?.SetClient(null);
            _reservedSlotsPanel?.SetClient(null);
            _pluginsPanel?.SetClient(null);
            _accountsPanel?.SetClient(null);
            _eventsPanel?.SetClient(null);
            _serverSettingsPanel?.SetClient(null);
            _playerActionsPanel?.SetClient(null);
            _layerPanel?.SetClient(null);
            _spectatorListPanel?.SetClient(null);
            _punkBusterPanel?.SetClient(null);
            _textChatModerationPanel?.SetClient(null);

            if (_chatLog != null) _chatLog.Text = "";

            if (_consoleLogList != null) _consoleLogList.ItemsSource = null;

            if (_killFeedList != null) _killFeedList.ItemsSource = null;

            // Clear team panels
            for (int t = 0; t < 4; t++)
            {
                if (_teamLists[t] != null) _teamLists[t].ItemsSource = null;
                if (_teamHeaders[t] != null) _teamHeaders[t].Text = $"Team {t + 1} (0)";
            }

            // Clear dashboard
            if (_playerGraphCanvas != null) _playerGraphCanvas.Children.Clear();

            TextBlock[] dashFields = { _dashServerName, _dashGameType, _dashServerVersion,
                _dashMapMode, _dashPlayerCount, _dashRound, _dashUptime,
                _dashRoundTime, _dashRegion, _dashTeamScores[0], _dashTeamScores[1],
                _dashConnectionInfo, _dashGraphRange };
            foreach (var tb in dashFields)
            {
                if (tb != null) tb.Text = "--";
            }
        }

        private void LoadServerView(ServerEntry entry)
        {
            ClearServerContext();

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
            _spectatorListPanel?.SetClient(client);
            _punkBusterPanel?.SetClient(client);
            _textChatModerationPanel?.SetClient(client);
            _optionsPanel?.SetApplication(_application);

            // Load data for connected servers
            if (connected)
            {
                _mapListPanel?.LoadData();
                _banListPanel?.LoadData();
                _reservedSlotsPanel?.LoadData();
                _textChatModerationPanel?.LoadData();
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
            if (_chatLog != null) _chatLog.Text = string.Join("\n", entry.ChatLines);

            // Players
            UpdateTeamPanels(entry);

            // Server Info dashboard
            var dashClient = GetClient(entry.HostPort);
            if (dashClient?.Game != null)
                UpdateDashboard(entry, dashClient.Game);

            // Kill feed
            if (_killFeedList != null) _killFeedList.ItemsSource = entry.KillFeed;

            // Console
            if (_consoleLogList != null)
            {
                _consoleLogList.ItemsSource = entry.ConsoleLines;
                // Scroll to bottom after loading
                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        if (_consoleLogList.ItemCount > 0)
                            _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);
                    }
                    catch { }
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        // --- Chat ---

        private void AppendChat(ServerEntry entry, string line)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            entry.ChatLines.Enqueue($"[{timestamp}] {line}");
            while (entry.ChatLines.Count > ServerEntry.MaxChatLines)
                entry.ChatLines.Dequeue();

            if (_selectedServer == entry)
            {
                if (_chatLog != null) _chatLog.Text = string.Join("\n", entry.ChatLines);
                _chatScroller?.ScrollToEnd();
            }
        }

        private void OnSendChat(object sender, RoutedEventArgs e)
        {
            var client = SelectedClient;
            if (_chatInput == null || string.IsNullOrWhiteSpace(_chatInput.Text) || client?.Game == null || _selectedServer == null)
                return;

            string msg = _chatInput.Text;
            string chatName = _selectedServer.IsLayerConnection && !string.IsNullOrEmpty(_selectedServer.LayerUsername)
                ? _selectedServer.LayerUsername : "Admin";
            client.Game.SendAdminSayPacket(msg, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            AppendChat(_selectedServer, $"[{chatName}] {msg}");
            _chatInput.Text = "";
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
                if (_consoleSuggestions != null) _consoleSuggestions.IsVisible = false;
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
                if (_consoleSuggestions != null && _consoleSuggestions.IsVisible && _consoleSuggestions.ItemCount > 0)
                {
                    string selected = (_consoleSuggestions.SelectedItem ?? _consoleSuggestions.Items.Cast<object>().First())?.ToString();
                    if (selected != null)
                    {
                        string cmdName = selected.Split(' ')[0];
                        consoleInput.Text = cmdName + " ";
                        consoleInput.CaretIndex = consoleInput.Text.Length;
                        _consoleSuggestions.IsVisible = false;
                    }
                }
                else
                {
                    string text = consoleInput.Text ?? "";
                    string prefix = text.Split(' ')[0];
                    var matches = new List<RconCommandDef>();
                    foreach (var cmd in RconCommandDatabase.Commands)
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
                if (_consoleSuggestions != null) _consoleSuggestions.IsVisible = false;
                e.Handled = true;
            }
        }

        private void OnSendConsoleCommand(object sender, RoutedEventArgs e)
        {
            var client = SelectedClient;
            if (_consoleInput == null || string.IsNullOrWhiteSpace(_consoleInput.Text) || client?.Game == null || _selectedServer == null)
                return;

            string cmd = _consoleInput.Text.Trim();
            var words = new List<string>(cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            if (words.Count == 0) return;

            // Validate command
            string cmdName = words[0];
            int paramCount = words.Count - 1;

            if (RconCommandDatabase.Lookup.TryGetValue(cmdName, out var cmdDef))
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
                    if (_consoleLogList != null && _consoleLogList.ItemCount > 0)
                        _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);
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
                    if (_consoleLogList != null && _consoleLogList.ItemCount > 0)
                        _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);
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
            _consoleInput.Text = "";

            if (_consoleLogList != null && _consoleLogList.ItemCount > 0)
                _consoleLogList.ScrollIntoView(_consoleLogList.ItemCount - 1);

            client.SendRequest(words);
        }

        private void OnConsoleInputTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            var consoleInput = sender as TextBox;
            if (consoleInput == null || _consoleSuggestions == null) return;

            string text = consoleInput.Text ?? "";
            string prefix = text.Split(' ')[0];

            if (prefix.Length >= 2)
            {
                var matches = new List<string>();
                foreach (var cmd in RconCommandDatabase.Commands)
                {
                    if (cmd.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && IsCommandForCurrentGame(cmd))
                        matches.Add(cmd.Signature);
                }

                if (matches.Count > 0 && matches.Count <= 20)
                {
                    _consoleSuggestions.ItemsSource = matches;
                    _consoleSuggestions.IsVisible = true;
                }
                else
                {
                    _consoleSuggestions.IsVisible = false;
                }
            }
            else
            {
                _consoleSuggestions.IsVisible = false;
            }
        }

        private void OnSuggestionSelected(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var suggestionsBox = sender as ListBox;
            if (suggestionsBox?.SelectedItem == null || _consoleInput == null) return;

            // Extract just the command name from the signature
            string selected = suggestionsBox.SelectedItem.ToString();
            string cmdName = selected.Split(' ')[0];
            _consoleInput.Text = cmdName + " ";
            _consoleInput.CaretIndex = _consoleInput.Text.Length;
            suggestionsBox.IsVisible = false;
            _consoleInput.Focus();
        }

        private void OnCopyConsoleSelected(object sender, RoutedEventArgs e)
        {
            if (_consoleLogList?.SelectedItems == null) return;
            var sb = new StringBuilder();
            foreach (var item in _consoleLogList.SelectedItems)
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

        private async void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            _optionsPanel.SetApplication(_application);
            var dialog = new SettingsDialog();
            dialog.SetContent(_optionsPanel);
            await dialog.ShowDialog(this);
        }

        private void UpdateStatus(string color, string text)
        {
            if (_statusIndicator != null)
            {
                _statusIndicator.Fill = new SolidColorBrush(Color.Parse(color));
                // Pulse when connected (green) or connecting (orange)
                bool shouldPulse = color == "#66bb6a" || color == "#ffab40";
                if (shouldPulse && !_statusIndicator.Classes.Contains("pulse"))
                    _statusIndicator.Classes.Add("pulse");
                else if (!shouldPulse)
                    _statusIndicator.Classes.Remove("pulse");
            }
            if (_statusText != null) _statusText.Text = text;
        }

        private void UpdateServerInfoPanel(string title, string details)
        {
            // Status is shown in bottom bar and dashboard — this is now a no-op
        }

        private void UpdateContentVisibility()
        {
            bool hasServer = _selectedServer != null;
            bool connected = hasServer &&
                (_selectedServer.State == ServerConnectionState.Connected ||
                 _selectedServer.State == ServerConnectionState.Connecting);

            if (_disconnectedOverlay != null) _disconnectedOverlay.IsVisible = !connected;
            if (_tabBarBorder != null) _tabBarBorder.IsVisible = connected;

            // Hide Layer tab for layer connections (can't manage a layer from a layer)
            if (_layerTabButton != null)
                _layerTabButton.IsVisible = !(_selectedServer?.IsLayerConnection == true);

            // Hide all tab content when disconnected
            if (!connected)
            {
                for (int i = 1; i <= 15; i++)
                {
                    if (_tabs != null && i < _tabs.Length && _tabs[i] != null) _tabs[i].IsVisible = false;
                }
            }

            // Update overlay content based on state

            if (!hasServer || _selectedServer.State == ServerConnectionState.Disconnected)
            {
                bool isLanding = !hasServer || _servers.Count == 0;
                if (_overlayTitle != null) _overlayTitle.Text = isLanding ? "Welcome" : "Disconnected";
                if (_overlayIcon != null) _overlayIcon.Text = isLanding ? "+" : "/";
                if (_disconnectedSubtext != null)
                    _disconnectedSubtext.Text = isLanding
                        ? "Add a game server to get started."
                        : $"Disconnected from {_selectedServer?.DisplayName ?? "server"}.\nClick Connect in the sidebar to reconnect.";
            }

            // Update landing page stats
            UpdateLandingStats();
        }

        private void UpdateDashboard(ServerEntry entry, FrostbiteClient game)
        {
            var info = entry.LastServerInfo;
            if (info == null) return;

            // Hero
            if (_dashServerName != null) _dashServerName.Text = info.ServerName ?? "Unknown";

            if (_dashGameType != null) _dashGameType.Text = game?.GameType ?? entry.GameType ?? "??";

            if (_dashServerVersion != null) _dashServerVersion.Text = entry.GameVersion ?? "??";

            string mapName = GameData.GetMapName(info.Map ?? "") ?? info.Map ?? "Unknown";
            string modeName = GameData.GetModeName(info.GameMode ?? "") ?? info.GameMode ?? "Unknown";
            if (_dashMapMode != null) _dashMapMode.Text = $"{mapName} — {modeName}";

            if (_dashPlayerCount != null) _dashPlayerCount.Text = $"{info.PlayerCount}/{info.MaxPlayerCount}";

            // Badges
            if (_dashRankedBadge != null)
            {
                _dashRankedBadge.Background = new SolidColorBrush(Color.Parse(info.Ranked ? "#66bb6a" : "#ef5350"));
                if (_dashRankedText != null) _dashRankedText.Text = info.Ranked ? "RANKED" : "UNRANKED";
            }
            if (_dashPbBadge != null) _dashPbBadge.IsVisible = info.PunkBuster;

            // Stats cards
            if (_dashRound != null) _dashRound.Text = $"{info.CurrentRound + 1} / {info.TotalRounds}";

            if (_dashUptime != null && info.ServerUptime > 0)
            {
                var ts = TimeSpan.FromSeconds(info.ServerUptime);
                _dashUptime.Text = ts.TotalHours >= 24
                    ? $"{(int)ts.TotalDays}d {ts.Hours}h"
                    : ts.TotalHours >= 1
                        ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
                        : $"{ts.Minutes}m";
            }

            if (_dashRoundTime != null && info.RoundTime > 0)
            {
                var rt = TimeSpan.FromSeconds(info.RoundTime);
                _dashRoundTime.Text = rt.TotalHours >= 1 ? $"{(int)rt.TotalHours}h {rt.Minutes}m" : $"{rt.Minutes}m {rt.Seconds}s";
            }

            if (_dashRegion != null) _dashRegion.Text = info.ServerCountry ?? info.ServerRegion ?? info.PingSite ?? "--";

            // Team scores
            if (info.TeamScores != null)
            {
                for (int i = 0; i < info.TeamScores.Count && i < 2; i++)
                {
                    if (_dashTeamScores[i] != null)
                        _dashTeamScores[i].Text = info.TeamScores[i].Score.ToString("F0");
                }
            }

            // Connection details
            if (_dashConnectionInfo != null)
            {
                _dashConnectionInfo.Text = $"IP: {entry.HostPort}\n" +
                                $"Game: {game?.GameType ?? "?"} {entry.GameVersion}\n" +
                                $"Build: {game?.VersionNumber ?? "?"}\n" +
                                $"PunkBuster: {(info.PunkBuster ? $"Active ({info.PunkBusterVersion})" : "Inactive")}\n" +
                                $"Password: {(info.Passworded ? "Yes" : "No")}\n" +
                                $"Join Queue: {(info.JoinQueueEnabled ? "Enabled" : "Disabled")}\n" +
                                $"External: {info.ExternalGameIpandPort ?? "N/A"}";
            }

            // Kill feed
            if (_killFeedList != null) _killFeedList.ItemsSource = entry.KillFeed;

            // Player graph
            DrawPlayerGraph(entry);
        }

        private void DrawPlayerGraph(ServerEntry entry)
        {
            if (_playerGraphCanvas == null || entry.PlayerHistory.Count < 2) return;

            _playerGraphCanvas.Children.Clear();

            double w = _playerGraphCanvas.Bounds.Width > 0 ? _playerGraphCanvas.Bounds.Width : 300;
            double h = _playerGraphCanvas.Bounds.Height > 0 ? _playerGraphCanvas.Bounds.Height : 120;
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
                _playerGraphCanvas.Children.Add(gridLine);

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
                _playerGraphCanvas.Children.Add(label);
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
            _playerGraphCanvas.Children.Add(fill);

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
            _playerGraphCanvas.Children.Add(line);

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
                _playerGraphCanvas.Children.Add(dot);
            }

            // Time range label
            if (_dashGraphRange != null && points.Count > 1)
            {
                var span = points[points.Count - 1].Time - points[0].Time;
                _dashGraphRange.Text = span.TotalMinutes < 2 ? "Just started" : $"Last {(int)span.TotalMinutes} minutes";
            }
        }

        private void UpdateLandingStats()
        {
            int totalServers = _servers.Count;
            int connectedCount = 0;
            int totalPlayers = 0;
            int totalSlots = 0;
            foreach (var s in _servers)
            {
                if (s.IsConnected) connectedCount++;
                if (s.LastServerInfo != null)
                {
                    totalPlayers += s.LastServerInfo.PlayerCount;
                    totalSlots += s.LastServerInfo.MaxPlayerCount;
                }
            }

            if (_landingServerCount != null) _landingServerCount.Text = totalServers.ToString();
            if (_landingConnectedCount != null) _landingConnectedCount.Text = connectedCount.ToString();
            if (_landingTotalPlayers != null) _landingTotalPlayers.Text = totalSlots > 0 ? $"{totalPlayers}/{totalSlots}" : totalPlayers.ToString();
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
            if (_connectSelectedButton != null) _connectSelectedButton.IsVisible = show;
        }

        private void ShowDisconnectButton(bool show)
        {
            if (_disconnectButton != null) _disconnectButton.IsVisible = show;
        }

        private void ShowRemoveButton(bool show)
        {
            if (_removeServerButton != null) _removeServerButton.IsVisible = show;
        }

        private void UpdateConnectionCount()
        {
            if (_connectionCountText == null) return;

            int connected = 0;
            foreach (var s in _servers)
                if (s.IsConnected) connected++;

            _connectionCountText.Text = $"{connected}/{_servers.Count} connected";
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
            if (_teamGrid != null)
            {
                _teamGrid.RowDefinitions.Clear();
                if (hasFourTeams)
                {
                    _teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                    _teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                }
                else
                {
                    _teamGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                }
            }

            for (int t = 0; t < 4; t++)
            {
                var players = entry.TeamPlayers.ContainsKey(t + 1) ? entry.TeamPlayers[t + 1] : new List<PlayerDisplayInfo>();

                if (_teamLists[t] != null)
                    _teamLists[t].ItemsSource = players;

                if (_teamHeaders[t] != null)
                    _teamHeaders[t].Text = $"Team {t + 1} ({players.Count})";

                // Show teams 3 and 4 only if they have players
                if (t >= 2 && _teamPanels[t] != null)
                    _teamPanels[t].IsVisible = players.Count > 0;
            }
        }

        private void RefreshPlayerList()
        {
            var client = SelectedClient;
            if (client?.Game != null)
                client.Game.SendAdminListPlayersPacket(new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
        }

        // --- Player Context Menu Handlers ---

        private PlayerDisplayInfo GetPlayerFromMenuContext(object sender)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is PlayerDisplayInfo player)
                return player;
            return null;
        }

        private void OnPlayerKill(object sender, RoutedEventArgs e)
        {
            var player = GetPlayerFromMenuContext(sender);
            var client = SelectedClient;
            if (player == null || client == null) return;

            client.SendRequest(new List<string> { "admin.killPlayer", player.Name });
        }

        private async void OnPlayerKick(object sender, RoutedEventArgs e)
        {
            var player = GetPlayerFromMenuContext(sender);
            var client = SelectedClient;
            if (player == null || client == null) return;

            var dialog = new TextInputDialog("Kick Player", $"Reason for kicking {player.Name}:", "Kicked by admin");
            await dialog.ShowDialog(this);

            if (dialog.Confirmed)
            {
                string reason = string.IsNullOrWhiteSpace(dialog.ResultText) ? "Kicked by admin" : dialog.ResultText;
                client.SendRequest(new List<string> { "admin.kickPlayer", player.Name, reason });
            }
        }

        private void MovePlayerToTeam(object sender, int teamId)
        {
            var player = GetPlayerFromMenuContext(sender);
            var client = SelectedClient;
            if (player == null || client == null) return;

            client.SendRequest(new List<string> { "admin.movePlayer", player.Name, teamId.ToString(), "0", "true" });
        }

        private void OnPlayerMoveTeam1(object sender, RoutedEventArgs e) => MovePlayerToTeam(sender, 1);
        private void OnPlayerMoveTeam2(object sender, RoutedEventArgs e) => MovePlayerToTeam(sender, 2);
        private void OnPlayerMoveTeam3(object sender, RoutedEventArgs e) => MovePlayerToTeam(sender, 3);
        private void OnPlayerMoveTeam4(object sender, RoutedEventArgs e) => MovePlayerToTeam(sender, 4);

        private async void OnPlayerBan(object sender, RoutedEventArgs e)
        {
            var player = GetPlayerFromMenuContext(sender);
            var client = SelectedClient;
            if (player == null || client == null) return;

            var dialog = new TextInputDialog("Ban Player", $"Reason for banning {player.Name}:", "Banned by admin");
            await dialog.ShowDialog(this);

            if (dialog.Confirmed)
            {
                string reason = string.IsNullOrWhiteSpace(dialog.ResultText) ? "Banned by admin" : dialog.ResultText;
                client.SendRequest(new List<string> { "banList.add", "name", player.Name, "perm", reason });
                client.SendRequest(new List<string> { "banList.save" });
            }
        }

        private void OnPlayerCopyName(object sender, RoutedEventArgs e)
        {
            var player = GetPlayerFromMenuContext(sender);
            if (player == null) return;

            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(player.Name);
        }
    }
}
