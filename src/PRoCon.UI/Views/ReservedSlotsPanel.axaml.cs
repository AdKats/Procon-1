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
        private bool _pendingRefresh;

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
                _client.Game.ReservedSlotsSave -= OnReservedSlotsSaved;
            }

            _client = client;

            if (_client?.Game != null)
            {
                _client.Game.ReservedSlotsList += OnReservedSlotsList;
                _client.Game.ReservedSlotsPlayerAdded += OnReservedSlotsPlayerAdded;
                _client.Game.ReservedSlotsPlayerRemoved += OnReservedSlotsPlayerRemoved;
                _client.Game.ReservedSlotsSave += OnReservedSlotsSaved;
                System.Console.WriteLine("[ReservedSlotsPanel] Client set: " + client.HostNamePort);
            }
        }

        public void LoadData()
        {
            System.Console.WriteLine("[ReservedSlotsPanel] LoadData");
            if (_client?.Game != null)
            {
                _client.Game.SendReservedSlotsListPacket();
            }
        }

        private void OnReservedSlotsSaved(FrostbiteClient sender)
        {
            System.Console.WriteLine("[ReservedSlotsPanel] ReservedSlotsSave OK received — refreshing list");
            // Server confirmed save — now re-list
            if (_pendingRefresh && _client?.Game != null)
            {
                _pendingRefresh = false;
                _client.Game.SendReservedSlotsListPacket();
            }
        }

        private void OnReservedSlotsList(FrostbiteClient sender, List<string> soldierNames)
        {
            System.Console.WriteLine("[ReservedSlotsPanel] ReservedSlotsList: " + soldierNames.Count + " players");
            Dispatcher.UIThread.Post(() =>
            {
                _reservedPlayers.Clear();
                _reservedPlayers.AddRange(soldierNames);
                RefreshListDisplay();
            });
        }

        private void OnReservedSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            System.Console.WriteLine("[ReservedSlotsPanel] PlayerAdded: " + strSoldierName);
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
            System.Console.WriteLine("[ReservedSlotsPanel] PlayerRemoved: " + strSoldierName);
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
            System.Console.WriteLine("[ReservedSlotsPanel] UI refreshed: " + _reservedPlayers.Count + " items");
        }

        private void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string playerName = PlayerNameInput?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(playerName)) return;

            System.Console.WriteLine("[ReservedSlotsPanel] ADD: " + playerName);
            _pendingRefresh = true;
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

            System.Console.WriteLine("[ReservedSlotsPanel] REMOVE: " + selected);
            _pendingRefresh = true;
            _client.Game.SendReservedSlotsRemovePlayerPacket(selected);
            _client.Game.SendReservedSlotsSavePacket();

            // Immediate local feedback
            _reservedPlayers.Remove(selected);
            RefreshListDisplay();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("[ReservedSlotsPanel] Manual refresh");
            LoadData();
        }
    }
}
