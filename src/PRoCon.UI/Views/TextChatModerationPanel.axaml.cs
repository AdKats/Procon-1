using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core.Remote;
using PRoCon.Core.TextChatModeration;

namespace PRoCon.UI.Views
{
    public partial class TextChatModerationPanel : UserControl
    {
        private PRoConClient _client;
        private readonly List<ModerationDisplayEntry> _entries = new List<ModerationDisplayEntry>();

        private ListBox _moderationList;
        private TextBox _playerNameInput;
        private ComboBox _moderationLevelCombo;

        public TextChatModerationPanel()
        {
            InitializeComponent();

            _moderationList = this.FindControl<ListBox>("ModerationList");
            _playerNameInput = this.FindControl<TextBox>("PlayerNameInput");
            _moderationLevelCombo = this.FindControl<ComboBox>("ModerationLevelCombo");
        }

        public void SetClient(PRoConClient client)
        {
            if (_client != null)
            {
                _client.FullTextChatModerationListList -= OnFullModerationList;

                if (_client.Game != null)
                {
                    _client.Game.TextChatModerationListAddPlayer -= OnModerationPlayerAdded;
                    _client.Game.TextChatModerationListRemovePlayer -= OnModerationPlayerRemoved;
                    _client.Game.TextChatModerationListClear -= OnModerationListCleared;
                }
            }

            _client = client;

            if (_client != null)
            {
                _client.FullTextChatModerationListList += OnFullModerationList;

                if (_client.Game != null)
                {
                    _client.Game.TextChatModerationListAddPlayer += OnModerationPlayerAdded;
                    _client.Game.TextChatModerationListRemovePlayer += OnModerationPlayerRemoved;
                    _client.Game.TextChatModerationListClear += OnModerationListCleared;
                }
            }
        }

        public void LoadData()
        {
            if (_client?.Game != null)
            {
                _client.Game.SendTextChatModerationListListPacket();
            }
        }

        private void OnFullModerationList(PRoConClient sender, TextChatModerationDictionary moderationList)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _entries.Clear();
                foreach (TextChatModerationEntry entry in moderationList)
                {
                    _entries.Add(new ModerationDisplayEntry(entry.SoldierName, entry.PlayerModerationLevel));
                }
                RefreshListDisplay();
            });
        }

        private void OnModerationPlayerAdded(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var existing = _entries.FirstOrDefault(e =>
                    string.Equals(e.SoldierName, playerEntry.SoldierName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.ModerationLevel = playerEntry.PlayerModerationLevel.ToString();
                }
                else
                {
                    _entries.Add(new ModerationDisplayEntry(playerEntry.SoldierName, playerEntry.PlayerModerationLevel));
                }
                RefreshListDisplay();
            });
        }

        private void OnModerationPlayerRemoved(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _entries.RemoveAll(e =>
                    string.Equals(e.SoldierName, playerEntry.SoldierName, StringComparison.OrdinalIgnoreCase));
                RefreshListDisplay();
            });
        }

        private void OnModerationListCleared(FrostbiteClient sender)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _entries.Clear();
                RefreshListDisplay();
            });
        }

        private void RefreshListDisplay()
        {
            if (_moderationList == null) return;
            _moderationList.ItemsSource = null;
            _moderationList.ItemsSource = new List<ModerationDisplayEntry>(_entries);
        }

        private void OnAddPlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            string playerName = _playerNameInput?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(playerName)) return;

            var selectedItem = _moderationLevelCombo?.SelectedItem as ComboBoxItem;
            string levelText = selectedItem?.Content?.ToString() ?? "Normal";

            PlayerModerationLevelType level = TextChatModerationEntry.GetPlayerModerationLevelType(levelText);
            var entry = new TextChatModerationEntry(level, playerName);

            _client.Game.SendTextChatModerationListAddPacket(entry);
            _client.Game.SendTextChatModerationListSavePacket();

            if (_playerNameInput != null)
                _playerNameInput.Text = string.Empty;
        }

        private void OnRemovePlayer(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            var selected = _moderationList?.SelectedItem as ModerationDisplayEntry;
            if (selected == null) return;

            _client.Game.SendTextChatModerationListRemovePacket(selected.SoldierName);
            _client.Game.SendTextChatModerationListSavePacket();
        }

        private void OnClearAll(object sender, RoutedEventArgs e)
        {
            if (_client?.Game == null) return;

            // No dedicated clear send method on FrostbiteClient, use PRoConClient.SendRequest
            _client.SendRequest(new List<string> { "textChatModerationList.clear" });
            _client.SendRequest(new List<string> { "textChatModerationList.save" });
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }

    public class ModerationDisplayEntry
    {
        public string SoldierName { get; set; }
        public string ModerationLevel { get; set; }

        public ModerationDisplayEntry(string soldierName, PlayerModerationLevelType level)
        {
            SoldierName = soldierName;
            ModerationLevel = level.ToString();
        }
    }
}
