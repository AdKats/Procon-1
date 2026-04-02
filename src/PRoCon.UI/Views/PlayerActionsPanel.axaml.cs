using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PRoCon.Core;
using PRoCon.Core.Remote;
using PRoCon.UI.Models;

namespace PRoCon.UI.Views
{
    public partial class PlayerActionsPanel : UserControl
    {
        private PRoConClient _client;
        private readonly List<PlayerDisplayInfo> _selectedPlayers = new List<PlayerDisplayInfo>();

        public PlayerActionsPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            _client = client;
            _selectedPlayers.Clear();
            ClearPlayerInfo();

            if (_client == null)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
        }

        public void SetApplication(PRoConApplication app)
        {
            // Not needed for player actions.
        }

        /// <summary>
        /// Called when player selection changes. Accepts the full list of currently selected players.
        /// </summary>
        public void SetSelectedPlayers(List<PlayerDisplayInfo> players)
        {
            _selectedPlayers.Clear();

            if (players == null || players.Count == 0)
            {
                ClearPlayerInfo();
                return;
            }

            _selectedPlayers.AddRange(players);

            if (_selectedPlayers.Count == 1)
            {
                var p = _selectedPlayers[0];
                SetText("PlayerNameText", p.Name ?? "--");
                SetText("PlayerScoreText", p.ScoreText);
                SetText("PlayerKDText", $"{p.Kills}/{p.Deaths}");
                SetText("PlayerPingText", p.PingText);
                SetText("PlayerSquadText", p.SquadText);
            }
            else
            {
                // Multi-select: show count and abbreviated name list
                string names = string.Join(", ", _selectedPlayers.Select(p => p.Name));
                if (names.Length > 60)
                    names = names.Substring(0, 57) + "...";
                SetText("PlayerNameText", $"{_selectedPlayers.Count} players selected");
                SetText("PlayerScoreText", _selectedPlayers.Sum(p => p.Score).ToString());
                SetText("PlayerKDText", $"{_selectedPlayers.Sum(p => p.Kills)}/{_selectedPlayers.Sum(p => p.Deaths)}");
                SetText("PlayerPingText", "--");
                SetText("PlayerSquadText", "--");
                SetStatus(names);
            }
        }

        private void ClearPlayerInfo()
        {
            SetText("PlayerNameText", "No player selected");
            SetText("PlayerScoreText", "--");
            SetText("PlayerKDText", "--");
            SetText("PlayerPingText", "--");
            SetText("PlayerSquadText", "--");
            SetText("ActionStatusText", "");
        }

        // --- Actions ---

        private void OnKillPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            foreach (var player in _selectedPlayers)
                _client.SendRequest(new List<string> { "admin.killPlayer", player.Name });

            SetStatus($"Kill sent: {FormatNames()}");
        }

        private void OnKickPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            string reason = GetMessage();
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Kicked by admin";

            foreach (var player in _selectedPlayers)
                _client.SendRequest(new List<string> { "admin.kickPlayer", player.Name, reason });

            SetStatus($"Kicked: {FormatNames()}");
        }

        private void OnSayPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateClient()) return;

            string message = GetMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                SetStatus("Enter a message first.");
                return;
            }

            _client.SendRequest(new List<string> { "admin.say", message, "all" });
            SetStatus($"Said: {message}");
        }

        private void OnYellPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateClient()) return;

            string message = GetMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                SetStatus("Enter a message first.");
                return;
            }

            _client.SendRequest(new List<string> { "admin.yell", message, "10", "all" });
            SetStatus($"Yelled: {message}");
        }

        private void OnMoveTeam1(object sender, RoutedEventArgs e) => MoveToTeam(1);
        private void OnMoveTeam2(object sender, RoutedEventArgs e) => MoveToTeam(2);
        private void OnMoveTeam3(object sender, RoutedEventArgs e) => MoveToTeam(3);
        private void OnMoveTeam4(object sender, RoutedEventArgs e) => MoveToTeam(4);

        private void MoveToTeam(int teamId)
        {
            if (!ValidateSelection()) return;

            if (!int.TryParse(GetText("MoveSquadIdInput"), out int squadId))
                squadId = 0;

            foreach (var player in _selectedPlayers)
                _client.Game?.SendAdminMovePlayerPacket(player.Name, teamId, squadId, true);

            SetStatus($"Moved {FormatNames()} to Team {teamId}, Squad {squadId}");
        }

        private void OnBanPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            string reason = GetMessage();
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Banned by admin";

            var banPerm = this.FindControl<RadioButton>("BanPermanentRadio");
            var banTemp = this.FindControl<RadioButton>("BanTemporaryRadio");
            var banRound = this.FindControl<RadioButton>("BanRoundRadio");

            foreach (var player in _selectedPlayers)
            {
                var words = new List<string> { "banList.add", "name", player.Name };

                if (banPerm?.IsChecked == true)
                {
                    words.Add("perm");
                }
                else if (banRound?.IsChecked == true)
                {
                    words.Add("rounds");
                    words.Add("1");
                }
                else if (banTemp?.IsChecked == true)
                {
                    if (!int.TryParse(GetText("BanDurationInput"), out int minutes))
                        minutes = 60;

                    words.Add("seconds");
                    words.Add((minutes * 60).ToString(CultureInfo.InvariantCulture));
                }

                words.Add(reason);
                _client.SendRequest(words);
            }

            _client.Game?.SendBanListSavePacket();

            SetStatus($"Banned: {FormatNames()}");
        }

        // --- Helpers ---

        private bool ValidateClient()
        {
            if (_client == null)
            {
                SetStatus("No connection.");
                return false;
            }
            return true;
        }

        private bool ValidateSelection()
        {
            if (_client == null || _selectedPlayers.Count == 0)
            {
                SetStatus("No player selected or no connection.");
                return false;
            }
            return true;
        }

        private string FormatNames()
        {
            if (_selectedPlayers.Count == 1)
                return _selectedPlayers[0].Name;
            return $"{_selectedPlayers.Count} players ({string.Join(", ", _selectedPlayers.Select(p => p.Name))})";
        }

        private string GetMessage()
        {
            var tb = this.FindControl<TextBox>("MessageInput");
            return tb?.Text ?? "";
        }

        private void SetText(string controlName, string value)
        {
            var tb = this.FindControl<TextBlock>(controlName);
            if (tb != null) tb.Text = value;
        }

        private string GetText(string controlName)
        {
            var tb = this.FindControl<TextBox>(controlName);
            return tb?.Text ?? "";
        }

        private void SetStatus(string message)
        {
            var status = this.FindControl<TextBlock>("ActionStatusText");
            if (status != null) status.Text = message;
        }
    }
}
