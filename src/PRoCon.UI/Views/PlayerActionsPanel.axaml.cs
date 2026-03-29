using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public partial class PlayerActionsPanel : UserControl
    {
        private PRoConClient _client;
        private string _selectedPlayerName;

        public PlayerActionsPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            _client = client;
            _selectedPlayerName = null;
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
        /// Called externally when a player is selected in the player list.
        /// </summary>
        public void SetSelectedPlayer(CPlayerInfo player)
        {
            if (player == null)
            {
                _selectedPlayerName = null;
                ClearPlayerInfo();
                return;
            }

            _selectedPlayerName = player.SoldierName;

            var nameText = this.FindControl<TextBlock>("PlayerNameText");
            if (nameText != null) nameText.Text = player.SoldierName ?? "--";

            var scoreText = this.FindControl<TextBlock>("PlayerScoreText");
            if (scoreText != null) scoreText.Text = player.Score.ToString();

            var guidText = this.FindControl<TextBlock>("PlayerGuidText");
            if (guidText != null) guidText.Text = player.GUID ?? "--";

            var pingText = this.FindControl<TextBlock>("PlayerPingText");
            if (pingText != null) pingText.Text = player.Ping.ToString();

            var ipText = this.FindControl<TextBlock>("PlayerIpText");
            if (ipText != null) ipText.Text = "--";

            var kdText = this.FindControl<TextBlock>("PlayerKDText");
            if (kdText != null) kdText.Text = $"{player.Kills}/{player.Deaths}";
        }

        private void ClearPlayerInfo()
        {
            SetTextBlock("PlayerNameText", "--");
            SetTextBlock("PlayerScoreText", "--");
            SetTextBlock("PlayerGuidText", "--");
            SetTextBlock("PlayerPingText", "--");
            SetTextBlock("PlayerIpText", "--");
            SetTextBlock("PlayerKDText", "--");
        }

        // --- Actions ---

        private void OnKillPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            _client.SendRequest(new List<string> { "admin.killPlayer", _selectedPlayerName });
            SetStatus($"Kill command sent for {_selectedPlayerName}.");
        }

        private void OnKickPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            string reason = GetText("ReasonInput");
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Kicked by admin";

            _client.SendRequest(new List<string> { "admin.kickPlayer", _selectedPlayerName, reason });
            SetStatus($"Kick command sent for {_selectedPlayerName}.");
        }

        private void OnMoveTeam(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            if (!int.TryParse(GetText("MoveTeamIdInput"), out int teamId))
                teamId = 1;

            if (!int.TryParse(GetText("MoveSquadIdInput"), out int squadId))
                squadId = 0;

            _client.Game?.SendAdminMovePlayerPacket(_selectedPlayerName, teamId, squadId, true);
            SetStatus($"Move command sent for {_selectedPlayerName} to Team {teamId}, Squad {squadId}.");
        }

        private void OnMoveSquad(object sender, RoutedEventArgs e)
        {
            // Same as move team but uses the squad ID field
            OnMoveTeam(sender, e);
        }

        private void OnBanPlayer(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            string reason = GetText("ReasonInput");
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Banned by admin";

            var banPerm = this.FindControl<RadioButton>("BanPermanentRadio");
            var banTemp = this.FindControl<RadioButton>("BanTemporaryRadio");
            var banRound = this.FindControl<RadioButton>("BanRoundRadio");

            var words = new List<string> { "banList.add", "name", _selectedPlayerName };

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

            // Save the ban list
            _client.Game?.SendBanListSavePacket();

            SetStatus($"Ban command sent for {_selectedPlayerName}.");
        }

        // --- Helpers ---

        private bool ValidateSelection()
        {
            if (_client == null || string.IsNullOrEmpty(_selectedPlayerName))
            {
                SetStatus("No player selected or no connection.");
                return false;
            }
            return true;
        }

        private void SetTextBlock(string controlName, string value)
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
