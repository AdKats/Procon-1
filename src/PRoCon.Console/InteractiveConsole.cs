using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Remote;

namespace PRoCon.Console
{
    public class InteractiveConsole
    {
        private readonly PRoConApplication _application;
        private PRoConClient _activeClient;
        private List<CPlayerInfo> _playerCache = new List<CPlayerInfo>();
        private readonly ManualResetEvent _exitEvent;

        public InteractiveConsole(PRoConApplication application, ManualResetEvent exitEvent)
        {
            _application = application;
            _exitEvent = exitEvent;
        }

        public void Start()
        {
            Log("Interactive mode enabled. Type 'help' for commands.");

            _application.Connections.ConnectionAdded += OnConnectionAdded;

            foreach (PRoConClient client in _application.Connections)
                WireClient(client);

            if (_application.Connections.Count == 1)
            {
                _activeClient = _application.Connections.Cast<PRoConClient>().First();
                Log($"Auto-selected: {_activeClient.HostNamePort}");
            }

            var inputThread = new Thread(InputLoop) { IsBackground = true };
            inputThread.Start();
        }

        private void InputLoop()
        {
            while (true)
            {
                string line = System.Console.ReadLine();
                if (line == null) break;

                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    ProcessCommand(line);
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
            }
        }

        private void ProcessCommand(string input)
        {
            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1] : "";

            switch (cmd)
            {
                case "help":
                case "?":
                    PrintHelp();
                    break;
                case "servers":
                    ListServers();
                    break;
                case "select":
                    SelectServer(arg);
                    break;
                case "status":
                    ShowStatus();
                    break;
                case "players":
                    ListPlayers();
                    break;
                case "kill":
                    KillPlayer(arg);
                    break;
                case "kick":
                    KickPlayer(arg);
                    break;
                case "ban":
                    BanPlayer(arg);
                    break;
                case "say":
                    SayMessage(arg);
                    break;
                case "yell":
                    YellMessage(arg);
                    break;
                case "raw":
                    RawCommand(arg);
                    break;
                case "quit":
                case "exit":
                    Log("Shutting down...");
                    _application.Shutdown();
                    _exitEvent.Set();
                    break;
                default:
                    RawCommand(input);
                    break;
            }
        }

        private void PrintHelp()
        {
            Log("Commands:");
            Log("  servers           - List connected servers");
            Log("  select <N>        - Select server by number");
            Log("  status            - Show selected server status");
            Log("  players           - List players (numbered)");
            Log("  kill <N>          - Kill player by number");
            Log("  kick <N> [reason] - Kick player by number");
            Log("  ban <N> [reason]  - Ban player by number (permanent)");
            Log("  say <message>     - Say message to all players");
            Log("  yell <message>    - Yell message to all players");
            Log("  raw <command>     - Send raw RCON command");
            Log("  quit              - Shutdown PRoCon");
            Log("  (any other input is sent as raw RCON)");
        }

        private void ListServers()
        {
            int i = 1;
            foreach (PRoConClient client in _application.Connections)
            {
                string marker = client == _activeClient ? " *" : "";
                string state = client.Game?.IsLoggedIn == true ? "connected" : "disconnected";
                Log($"  [{i}] {client.HostNamePort} ({state}){marker}");
                i++;
            }
            if (i == 1) Log("  No servers configured.");
        }

        private void SelectServer(string arg)
        {
            if (!int.TryParse(arg, out int index) || index < 1 || index > _application.Connections.Count)
            {
                Log($"Usage: select <1-{_application.Connections.Count}>");
                return;
            }
            _activeClient = _application.Connections.Cast<PRoConClient>().ElementAt(index - 1);
            Log($"Selected: {_activeClient.HostNamePort}");
        }

        private void ShowStatus()
        {
            if (!RequireActive()) return;
            var game = _activeClient.Game;
            if (game == null) { Log("Not connected."); return; }

            Log($"Server:  {_activeClient.HostNamePort}");
            Log($"Logged:  {(game.IsLoggedIn ? "yes" : "no")}");
            Log($"Game:    {game.GameType ?? "unknown"}");
            Log($"Version: {game.FriendlyVersionNumber ?? game.VersionNumber ?? "?"}");
        }

        private void ListPlayers()
        {
            if (!RequireActive()) return;
            var game = _activeClient.Game;
            if (game == null) { Log("Not connected."); return; }

            game.SendAdminListPlayersPacket(new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            Log("Player list requested. Results will appear when server responds.");
        }

        private void KillPlayer(string arg)
        {
            string name = ResolvePlayer(arg);
            if (name == null) return;
            _activeClient.SendRequest(new List<string> { "admin.killPlayer", name });
            Log($"Kill sent: {name}");
        }

        private void KickPlayer(string arg)
        {
            string[] parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string name = ResolvePlayer(parts.Length > 0 ? parts[0] : "");
            if (name == null) return;
            string reason = parts.Length > 1 ? parts[1] : "Kicked by admin";
            _activeClient.SendRequest(new List<string> { "admin.kickPlayer", name, reason });
            Log($"Kicked: {name} ({reason})");
        }

        private void BanPlayer(string arg)
        {
            string[] parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string name = ResolvePlayer(parts.Length > 0 ? parts[0] : "");
            if (name == null) return;
            string reason = parts.Length > 1 ? parts[1] : "Banned by admin";
            _activeClient.SendRequest(new List<string> { "banList.add", "name", name, "perm", reason });
            _activeClient.Game?.SendBanListSavePacket();
            Log($"Banned: {name} ({reason})");
        }

        private void SayMessage(string msg)
        {
            if (!RequireActive() || string.IsNullOrWhiteSpace(msg)) { Log("Usage: say <message>"); return; }
            _activeClient.SendRequest(new List<string> { "admin.say", msg, "all" });
            Log($"Said: {msg}");
        }

        private void YellMessage(string msg)
        {
            if (!RequireActive() || string.IsNullOrWhiteSpace(msg)) { Log("Usage: yell <message>"); return; }
            _activeClient.SendRequest(new List<string> { "admin.yell", msg, "10", "all" });
            Log($"Yelled: {msg}");
        }

        private void RawCommand(string raw)
        {
            if (!RequireActive() || string.IsNullOrWhiteSpace(raw)) return;
            var words = new List<string>(raw.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            _activeClient.SendRequest(words);
            Log($"Sent: {raw}");
        }

        private void OnConnectionAdded(PRoConClient client)
        {
            WireClient(client);
            if (_activeClient == null)
            {
                _activeClient = client;
                Log($"Auto-selected: {client.HostNamePort}");
            }
        }

        private void WireClient(PRoConClient client)
        {
            client.ConnectSuccess += sender => Log($"[{sender.HostNamePort}] Connected");
            client.ConnectionClosed += sender => Log($"[{sender.HostNamePort}] Disconnected");
            client.Login += sender =>
            {
                Log($"[{sender.HostNamePort}] Logged in");
                if (sender.Game != null)
                    WireGameEvents(sender, sender.Game);
            };
            client.GameTypeDiscovered += sender =>
            {
                if (sender.Game != null)
                    WireGameEvents(sender, sender.Game);
            };
        }

        private readonly HashSet<string> _wiredGames = new HashSet<string>();

        private void WireGameEvents(PRoConClient client, FrostbiteClient game)
        {
            if (!_wiredGames.Add(client.HostNamePort)) return;

            string tag = client.HostNamePort;

            game.ServerInfo += (sender, info) =>
                Log($"[{tag}] {info.ServerName} | {info.Map} | {info.GameMode} | {info.PlayerCount}/{info.MaxPlayerCount}");

            game.Chat += (sender, rawChat) =>
            {
                if (rawChat.Count >= 3)
                    Log($"[{tag}] [Chat] {rawChat[0]}: {rawChat[1]}");
            };

            game.PlayerJoin += (sender, name) =>
                Log($"[{tag}] + {name} joined");

            game.PlayerLeft += (sender, name, info) =>
                Log($"[{tag}] - {name} left");

            game.PlayerKilled += (sender, killer, victim, weapon, headshot, kPos, vPos) =>
            {
                string hs = headshot ? " [HS]" : "";
                Log($"[{tag}] {killer} [{weapon}] {victim}{hs}");
            };

            game.ListPlayers += (sender, players, subset) =>
            {
                if (client != _activeClient) return;
                _playerCache = players;
                Log($"--- Players ({players.Count}) ---");
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    Log($"  [{i + 1}] {p.SoldierName,-20} Score:{p.Score,-6} K/D:{p.Kills}/{p.Deaths,-4} Team:{p.TeamID} Squad:{p.SquadID}");
                }
                Log("---");
            };
        }

        private string ResolvePlayer(string arg)
        {
            if (!RequireActive()) return null;

            if (int.TryParse(arg, out int idx) && idx >= 1 && idx <= _playerCache.Count)
                return _playerCache[idx - 1].SoldierName;

            if (!string.IsNullOrWhiteSpace(arg))
                return arg;

            Log("Usage: provide a player number (from 'players') or name");
            return null;
        }

        private bool RequireActive()
        {
            if (_activeClient == null)
            {
                Log("No server selected. Use 'servers' and 'select <N>'.");
                return false;
            }
            return true;
        }

        private static void Log(string message)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            System.Console.WriteLine($"[{ts}] {message}");
        }
    }
}
