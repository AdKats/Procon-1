using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public partial class ReservedSlotsPanel : UserControl
    {
        private PRoConClient _client;
        private readonly List<string> _reservedPlayers = new List<string>();

        public ReservedSlotsPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            if (_client?.Game != null)
            {
                _client.Game.ReservedSlotsList -= OnReservedSlotsList;
                _client.Game.ReservedSlotsPlayerAdded -= OnReservedSlotsPlayerAdded;
                _client.Game.ReservedSlotsPlayerRemoved -= OnReservedSlotsPlayerRemoved;
            }

            _client = client;

            if (_client?.Game != null)
            {
                _client.Game.ReservedSlotsList += OnReservedSlotsList;
                _client.Game.ReservedSlotsPlayerAdded += OnReservedSlotsPlayerAdded;
                _client.Game.ReservedSlotsPlayerRemoved += OnReservedSlotsPlayerRemoved;
            }
        }

        public void LoadData()
        {
            if (_client?.Game != null)
            {
                _client.Game.SendReservedSlotsListPacket();
            }
        }

        private void OnReservedSlotsList(FrostbiteClient sender, List<string> soldierNames)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _reservedPlayers.Clear();
                _reservedPlayers.AddRange(soldierNames);
                RefreshListDisplay();
            });
        }

        private void OnReservedSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!_reservedPlayers.Contains(strSoldierName))
                {
                    _reservedPlayers.Add(strSoldierName);
                    RefreshListDisplay();
                }
            });
        }

        private void OnReservedSlotsPlayerRemoved(FrostbiteClient sender, string strSoldierName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _reservedPlayers.Remove(strSoldierName);
                RefreshListDisplay();
            });
        }

        private void RefreshListDisplay()
        {
            ReservedSlotsList.ItemsSource = null;
            ReservedSlotsList.ItemsSource = new List<string>(_reservedPlayers);
        }

        private void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string playerName = PlayerNameInput?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(playerName)) return;

            _client.Game.SendReservedSlotsAddPlayerPacket(playerName);
            _client.Game.SendReservedSlotsSavePacket();

            if (PlayerNameInput != null)
                PlayerNameInput.Text = string.Empty;
        }

        private void OnRemovePlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string selected = ReservedSlotsList.SelectedItem as string;
            if (string.IsNullOrEmpty(selected)) return;

            _client.Game.SendReservedSlotsRemovePlayerPacket(selected);
            _client.Game.SendReservedSlotsSavePacket();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
