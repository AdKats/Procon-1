using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public partial class SpectatorListPanel : UserControl
    {
        private PRoConClient _client;
        private readonly List<string> _spectatorPlayers = new List<string>();

        public SpectatorListPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            if (_client?.Game != null)
            {
                _client.Game.SpectatorListList -= OnSpectatorListList;
                _client.Game.SpectatorListPlayerAdded -= OnSpectatorListPlayerAdded;
                _client.Game.SpectatorListPlayerRemoved -= OnSpectatorListPlayerRemoved;
            }

            _client = client;

            if (_client?.Game != null)
            {
                _client.Game.SpectatorListList += OnSpectatorListList;
                _client.Game.SpectatorListPlayerAdded += OnSpectatorListPlayerAdded;
                _client.Game.SpectatorListPlayerRemoved += OnSpectatorListPlayerRemoved;
            }
        }

        public void LoadData()
        {
            if (_client?.Game != null)
            {
                _client.Game.SendSpectatorListListPacket();
            }
        }

        private void OnSpectatorListList(FrostbiteClient sender, List<string> soldierNames)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _spectatorPlayers.Clear();
                _spectatorPlayers.AddRange(soldierNames);
                RefreshListDisplay();
            });
        }

        private void OnSpectatorListPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!_spectatorPlayers.Contains(strSoldierName))
                {
                    _spectatorPlayers.Add(strSoldierName);
                    RefreshListDisplay();
                }
            });
        }

        private void OnSpectatorListPlayerRemoved(FrostbiteClient sender, string strSoldierName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _spectatorPlayers.Remove(strSoldierName);
                RefreshListDisplay();
            });
        }

        private void RefreshListDisplay()
        {
            SpectatorSlotsList.ItemsSource = null;
            SpectatorSlotsList.ItemsSource = new List<string>(_spectatorPlayers);
        }

        private void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string playerName = PlayerNameInput?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(playerName)) return;

            _client.Game.SendSpectatorListAddPlayerPacket(playerName);
            _client.Game.SendSpectatorListSavePacket();

            if (PlayerNameInput != null)
                PlayerNameInput.Text = string.Empty;
        }

        private void OnRemovePlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string selected = SpectatorSlotsList.SelectedItem as string;
            if (string.IsNullOrEmpty(selected)) return;

            _client.Game.SendSpectatorListRemovePlayerPacket(selected);
            _client.Game.SendSpectatorListSavePacket();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
