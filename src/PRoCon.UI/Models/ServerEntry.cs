using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PRoCon.Core;
using PRoCon.Core.Logging;
using PRoCon.UI.Services;

namespace PRoCon.UI.Models
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
        public bool IsLayerConnection { get; set; }
        public string LayerUsername { get; set; }

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
        private static readonly Avalonia.Media.ISolidColorBrush ConnectedBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#66bb6a"));
        private static readonly Avalonia.Media.ISolidColorBrush ConnectingBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffab40"));
        private static readonly Avalonia.Media.ISolidColorBrush DisconnectedBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ef5350"));

        public Avalonia.Media.ISolidColorBrush StatusColor => _state switch
        {
            ServerConnectionState.Connected => ConnectedBrush,
            ServerConnectionState.Connecting => ConnectingBrush,
            _ => DisconnectedBrush,
        };

        // Per-server state
        public const int MaxChatLines = 500;
        public Queue<string> ChatLines { get; } = new Queue<string>();
        public ObservableCollection<ConsoleLine> ConsoleLines { get; } = new ObservableCollection<ConsoleLine>();
        public ConsoleFileLogger ConsoleLogger { get; set; }
        public List<string> PlayerItems { get; set; } = new List<string>();
        public HashSet<string> SupportedCommands { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<string, string> PlayerIPs { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal bool _pendingAdminHelp;
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
}
