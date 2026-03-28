using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public partial class BanListPanel : UserControl
    {
        private PRoConClient _client;
        private FrostbiteClient _wiredGame;
        private readonly List<CBanInfo> _banList = new List<CBanInfo>();
        private bool _pendingRefresh;

        private static readonly Regex GuidRegex = new Regex(@"^EA_[0-9A-Fa-f]{32}$", RegexOptions.Compiled);
        private static readonly Regex Ipv4Regex = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", RegexOptions.Compiled);

        public BanListPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            UnwireEvents();
            _client = client;

            if (_client != null)
            {
                _client.FullBanListList += OnFullBanListList;
                WireGameEvents();
                System.Console.WriteLine("[BanListPanel] Client set: " + client.HostNamePort);
            }
        }

        private void WireGameEvents()
        {
            if (_client?.Game == null || _client.Game == _wiredGame) return;
            if (_wiredGame != null)
                _wiredGame.BanListSave -= OnBanListSaved;

            _wiredGame = _client.Game;
            _wiredGame.BanListSave += OnBanListSaved;
            System.Console.WriteLine("[BanListPanel] Wired BanListSave");
        }

        private void UnwireEvents()
        {
            if (_client != null) _client.FullBanListList -= OnFullBanListList;
            if (_wiredGame != null) { _wiredGame.BanListSave -= OnBanListSaved; _wiredGame = null; }
        }

        public void LoadData()
        {
            WireGameEvents();
            System.Console.WriteLine("[BanListPanel] LoadData");
            if (_client?.Game != null) _client.Game.SendBanListListPacket(0);
        }

        private int _saveGeneration;
        private int _expectedBanCountAfterMutation = -1;
        private string _lastMutationType = "";

        private void OnBanListSaved(FrostbiteClient sender)
        {
            int gen = _saveGeneration;
            System.Console.WriteLine("[BanListPanel] >>> BanListSave OK! pendingRefresh=" + _pendingRefresh + " gen=" + gen + " mutation=" + _lastMutationType);
            if (_pendingRefresh && _client?.Game != null)
            {
                _pendingRefresh = false;
                int capturedGen = gen;
                var capturedClient = _client;
                string mutation = _lastMutationType;

                // For bulk operations or clears, use a longer delay
                int delay = mutation == "bulk" ? 2000 : mutation == "clear" ? 1000 : 500;

                System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(delay);
                    if (_saveGeneration == capturedGen && capturedClient?.Game != null)
                    {
                        System.Console.WriteLine("[BanListPanel] >>> Requesting banList.list 0 (gen=" + capturedGen + " delay=" + delay + "ms)");
                        capturedClient.Game.SendBanListListPacket(0);
                    }
                });
            }
        }

        private void OnFullBanListList(PRoConClient sender, List<CBanInfo> lstBans)
        {
            System.Console.WriteLine("[BanListPanel] FullBanListList: " + lstBans.Count + " bans");
            Dispatcher.UIThread.Post(() =>
            {
                _banList.Clear();
                _banList.AddRange(lstBans);
                RefreshListDisplay();
            });
        }

        private void RefreshListDisplay()
        {
            var items = new List<string>();
            for (int i = 0; i < _banList.Count; i++)
            {
                var ban = _banList[i];
                string identity = ban.SoldierName ?? ban.Guid ?? ban.IpAddress ?? "unknown";
                string idType = ban.IdType ?? "unknown";
                string reason = !string.IsNullOrEmpty(ban.Reason) ? ban.Reason : "(no reason)";
                string duration = FormatBanDuration(ban.BanLength);
                items.Add($"[{idType}] {identity}  |  {duration}  |  Reason: {reason}");
            }
            BanListBox.ItemsSource = null;
            BanListBox.ItemsSource = items;
            System.Console.WriteLine("[BanListPanel] UI refreshed: " + items.Count + " items");
        }

        private static string FormatBanDuration(TimeoutSubset timeout)
        {
            if (timeout == null) return "Unknown";
            switch (timeout.Subset)
            {
                case TimeoutSubset.TimeoutSubsetType.Permanent: return "Permanent";
                case TimeoutSubset.TimeoutSubsetType.Round: return $"Round ({timeout.Timeout} rounds)";
                case TimeoutSubset.TimeoutSubsetType.Seconds: return $"Temporary ({timeout.Timeout}s)";
                default: return "Unknown";
            }
        }

        // --- Validation ---

        private bool ValidateBanInput(string idType, string banId, string reason)
        {
            var msg = this.FindControl<TextBlock>("ValidationMessage");

            if (string.IsNullOrWhiteSpace(banId))
            {
                ShowValidation(msg, "ID is required");
                return false;
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                ShowValidation(msg, "Reason is required");
                return false;
            }

            switch (idType)
            {
                case "guid":
                    if (!GuidRegex.IsMatch(banId))
                    {
                        ShowValidation(msg, "GUID must be in format: EA_ followed by 32 hex chars");
                        return false;
                    }
                    break;
                case "ip":
                    if (!Ipv4Regex.IsMatch(banId))
                    {
                        ShowValidation(msg, "IP must be a valid IPv4 address (e.g. 192.168.1.1)");
                        return false;
                    }
                    break;
                case "name":
                    if (banId.Length < 1 || banId.Length > 32)
                    {
                        ShowValidation(msg, "Name must be 1-32 characters");
                        return false;
                    }
                    break;
            }

            if (msg != null) msg.IsVisible = false;
            return true;
        }

        private void ShowValidation(TextBlock msg, string text)
        {
            if (msg != null) { msg.Text = text; msg.IsVisible = true; }
        }

        // --- UI Events ---

        private void OnBanTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBox idInput;
            try { idInput = this.FindControl<TextBox>("BanIdInput"); }
            catch { return; } // Name scope not ready during InitializeComponent
            if (idInput == null) return;

            string idType = "name";
            if (BanIdTypeCombo?.SelectedItem is ComboBoxItem item)
                idType = item.Tag?.ToString() ?? "name";

            switch (idType)
            {
                case "guid": idInput.Watermark = "EA_XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"; break;
                case "ip": idInput.Watermark = "192.168.1.1"; break;
                default: idInput.Watermark = "Player name"; break;
            }
        }

        private void OnDurationTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            StackPanel tempPanel, roundPanel;
            try
            {
                tempPanel = this.FindControl<StackPanel>("TempDurationPanel");
                roundPanel = this.FindControl<StackPanel>("RoundDurationPanel");
            }
            catch { return; }

            string durType = "perm";
            if (BanDurationTypeCombo?.SelectedItem is ComboBoxItem item)
                durType = item.Tag?.ToString() ?? "perm";

            if (tempPanel != null) tempPanel.IsVisible = durType == "seconds";
            if (roundPanel != null) roundPanel.IsVisible = durType == "round";
        }

        private void OnTempDurationChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBox customInput;
            ComboBox combo;
            try
            {
                customInput = this.FindControl<TextBox>("CustomSecondsInput");
                combo = this.FindControl<ComboBox>("TempDurationCombo");
            }
            catch { return; }
            if (customInput == null || combo == null) return;

            string tag = (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
            customInput.IsVisible = tag == "custom";
        }

        private void OnAddBan(object sender, RoutedEventArgs e)
        {
            WireGameEvents();
            if (_client?.Game == null) return;

            string idType = "name";
            if (BanIdTypeCombo?.SelectedItem is ComboBoxItem idTypeItem)
                idType = idTypeItem.Tag?.ToString() ?? "name";

            string banId = BanIdInput?.Text?.Trim() ?? "";
            string reason = BanReasonInput?.Text?.Trim() ?? "";

            if (!ValidateBanInput(idType, banId, reason)) return;

            string durationType = "perm";
            if (BanDurationTypeCombo?.SelectedItem is ComboBoxItem durTypeItem)
                durationType = durTypeItem.Tag?.ToString() ?? "perm";

            var words = new List<string> { "banList.add", idType, banId };
            switch (durationType)
            {
                case "perm":
                    words.Add("perm");
                    break;
                case "seconds":
                    int seconds = 3600;
                    var tempCombo = this.FindControl<ComboBox>("TempDurationCombo");
                    var customInput = this.FindControl<TextBox>("CustomSecondsInput");
                    string tag = (tempCombo?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "3600";
                    if (tag == "custom")
                    {
                        int.TryParse(customInput?.Text, out seconds);
                        if (seconds <= 0) seconds = 3600;
                    }
                    else
                    {
                        int.TryParse(tag, out seconds);
                    }
                    words.Add("seconds");
                    words.Add(seconds.ToString());
                    break;
                case "round":
                    int rounds = 1;
                    var roundInput = this.FindControl<TextBox>("RoundValueInput");
                    if (roundInput != null) int.TryParse(roundInput.Text, out rounds);
                    if (rounds <= 0) rounds = 1;
                    words.Add("rounds");
                    words.Add(rounds.ToString());
                    break;
            }
            words.Add(reason);

            System.Console.WriteLine("[BanListPanel] ADD: " + string.Join(" ", words));
            _saveGeneration++;
            _pendingRefresh = true;
            _lastMutationType = "add";
            _client.SendRequest(words);
            _client.Game.SendBanListSavePacket();

            if (BanIdInput != null) BanIdInput.Text = "";
            if (BanReasonInput != null) BanReasonInput.Text = "";
        }

        private void OnUnban(object sender, RoutedEventArgs e)
        {
            WireGameEvents();
            if (_client?.Game == null) return;

            int index = BanListBox.SelectedIndex;
            if (index < 0 || index >= _banList.Count) return;

            var ban = _banList[index];
            string idType = ban.IdType ?? "name";
            string id = ban.SoldierName ?? ban.Guid ?? ban.IpAddress ?? "";
            if (string.IsNullOrEmpty(id)) return;

            System.Console.WriteLine("[BanListPanel] REMOVE: " + idType + " " + id);
            _saveGeneration++;
            _pendingRefresh = true;
            _lastMutationType = "remove";
            _client.SendRequest(new List<string> { "banList.remove", idType, id });
            _client.Game.SendBanListSavePacket();

            _banList.RemoveAt(index);
            RefreshListDisplay();
        }

        private void OnClearAllBans(object sender, RoutedEventArgs e)
        {
            WireGameEvents();
            if (_client?.Game == null) return;

            System.Console.WriteLine("[BanListPanel] CLEAR ALL BANS");
            _saveGeneration++;
            _pendingRefresh = true;
            _lastMutationType = "clear";
            _client.Game.SendBanListClearPacket();
            _client.Game.SendBanListSavePacket();

            _banList.Clear();
            RefreshListDisplay();
        }

        private void OnBulkAddBans(object sender, RoutedEventArgs e)
        {
            WireGameEvents();
            if (_client?.Game == null) return;

            var rng = new Random();
            int count = 300;
            System.Console.WriteLine("[BanListPanel] BULK ADD " + count + " test bans");

            for (int i = 0; i < count; i++)
            {
                string name = "TestBan_" + rng.Next(100000, 999999);
                string reason = "Bulk test ban #" + (i + 1);
                _client.SendRequest(new List<string> { "banList.add", "name", name, "perm", reason });
            }

            // Save after all added
            _saveGeneration++;
            _pendingRefresh = true;
            _lastMutationType = "bulk";
            _client.Game.SendBanListSavePacket();
            System.Console.WriteLine("[BanListPanel] Bulk add complete, save sent");
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("[BanListPanel] Manual refresh");
            LoadData();
        }
    }
}
