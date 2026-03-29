using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Accounts;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public class AccountEntry : INotifyPropertyChanged
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class AccountsPanel : UserControl
    {
        private PRoConClient _client;
        private PRoConApplication _application;
        private readonly ObservableCollection<AccountEntry> _accounts = new ObservableCollection<AccountEntry>();
        private string _selectedAccountName;

        public AccountsPanel()
        {
            InitializeComponent();

            var accountList = this.FindControl<ListBox>("AccountListBox");
            if (accountList != null)
                accountList.ItemsSource = _accounts;
        }

        public void SetClient(PRoConClient client)
        {
            _client = client;
            _application = client?.Parent;
            WireEvents();
            RefreshAccountList();
        }

        public void SetApplication(PRoConApplication app)
        {
            _application = app;
            WireEvents();
            RefreshAccountList();
        }

        private bool _eventsWired;

        private void WireEvents()
        {
            if (_eventsWired || _application?.AccountsList == null)
                return;

            _application.AccountsList.AccountAdded += OnAccountAddedEvent;
            _application.AccountsList.AccountRemoved += OnAccountRemovedEvent;
            _eventsWired = true;
        }

        private void OnAccountAddedEvent(Account item)
        {
            Dispatcher.UIThread.Post(RefreshAccountList);
        }

        private void OnAccountRemovedEvent(Account item)
        {
            Dispatcher.UIThread.Post(RefreshAccountList);
        }

        private void RefreshAccountList()
        {
            _accounts.Clear();

            if (_application?.AccountsList == null)
                return;

            try
            {
                foreach (Account account in _application.AccountsList)
                {
                    _accounts.Add(new AccountEntry { Name = account.Name });
                }
            }
            catch
            {
                // Collection may be modified during iteration
            }

            var countText = this.FindControl<TextBlock>("AccountCountText");
            if (countText != null) countText.Text = $"{_accounts.Count} account{(_accounts.Count != 1 ? "s" : "")}";
        }

        private void OnAccountSelected(object sender, SelectionChangedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("AccountListBox");
            if (listBox?.SelectedItem is not AccountEntry entry)
                return;

            _selectedAccountName = entry.Name;

            var headerText = this.FindControl<TextBlock>("PrivilegesHeaderText");
            if (headerText != null)
                headerText.Text = $"Privileges for: {entry.Name}";

            // Show privilege groups and save button
            string[] panels = { "PresetsPanel", "PrivGroupAccess", "PrivGroupPlayers",
                "PrivGroupLists", "PrivGroupProcon", "SavePrivilegesButton" };
            foreach (string name in panels)
            {
                var ctrl = this.FindControl<Avalonia.Controls.Control>(name);
                if (ctrl != null) ctrl.IsVisible = true;
            }

            LoadPrivileges(entry.Name);
        }

        private void LoadPrivileges(string accountName)
        {
            // Reset all checkboxes
            SetAllCheckboxes(false);

            if (_application?.AccountsList == null || !_application.AccountsList.Contains(accountName))
                return;

            // Find the account's privileges from any connected client
            CPrivileges privs = FindPrivileges(accountName);
            if (privs == null)
                return;

            SetCheckbox("ChkCanLogin", privs.CanLogin);
            SetCheckbox("ChkCanAlterServerSettings", privs.CanAlterServerSettings);
            SetCheckbox("ChkCanUseMapFunctions", privs.CanUseMapFunctions);
            SetCheckbox("ChkCanKillPlayers", privs.CanKillPlayers);
            SetCheckbox("ChkCanKickPlayers", privs.CanKickPlayers);
            SetCheckbox("ChkCanTemporaryBanPlayers", privs.CanTemporaryBanPlayers);
            SetCheckbox("ChkCanPermanentlyBanPlayers", privs.CanPermanentlyBanPlayers);
            SetCheckbox("ChkCanMovePlayers", privs.CanMovePlayers);
            SetCheckbox("ChkCanIssueLimitedPunkbusterCommands", privs.CanIssueLimitedPunkbusterCommands);
            SetCheckbox("ChkCanIssueAllPunkbusterCommands", privs.CanIssueAllPunkbusterCommands);
            SetCheckbox("ChkCanEditMapList", privs.CanEditMapList);
            SetCheckbox("ChkCanEditBanList", privs.CanEditBanList);
            SetCheckbox("ChkCanEditReservedSlotsList", privs.CanEditReservedSlotsList);
            SetCheckbox("ChkCanIssueLimitedProconCommands", privs.CanIssueLimitedProconCommands);
            SetCheckbox("ChkCanIssueAllProconCommands", privs.CanIssueAllProconCommands);
            SetCheckbox("ChkCanIssueLimitedProconPluginCommands", privs.CanIssueLimitedProconPluginCommands);
            SetCheckbox("ChkCanEditMapZones", privs.CanEditMapZones);
            SetCheckbox("ChkCanEditTextChatModerationList", privs.CanEditTextChatModerationList);
            SetCheckbox("ChkCanShutdownServer", privs.CanShutdownServer);
        }

        private CPrivileges FindPrivileges(string accountName)
        {
            // Try from the current client's layer privileges first
            if (_client != null)
            {
                try
                {
                    CPrivileges privs = _client.GetAccountPrivileges(accountName);
                    if (privs != null)
                        return privs;
                }
                catch
                {
                    // Ignore
                }
            }

            // Try from any connection
            if (_application?.Connections != null)
            {
                foreach (PRoConClient conn in _application.Connections)
                {
                    try
                    {
                        CPrivileges privs = conn.GetAccountPrivileges(accountName);
                        if (privs != null)
                            return privs;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            return null;
        }

        private void SetCheckbox(string name, bool value)
        {
            var cb = this.FindControl<CheckBox>(name);
            if (cb != null) cb.IsChecked = value;
        }

        private bool GetCheckbox(string name)
        {
            var cb = this.FindControl<CheckBox>(name);
            return cb?.IsChecked == true;
        }

        private void SetAllCheckboxes(bool value)
        {
            string[] names = {
                "ChkCanLogin", "ChkCanAlterServerSettings", "ChkCanUseMapFunctions",
                "ChkCanKillPlayers", "ChkCanKickPlayers", "ChkCanTemporaryBanPlayers",
                "ChkCanPermanentlyBanPlayers", "ChkCanMovePlayers",
                "ChkCanIssueLimitedPunkbusterCommands", "ChkCanIssueAllPunkbusterCommands",
                "ChkCanEditMapList", "ChkCanEditBanList", "ChkCanEditReservedSlotsList",
                "ChkCanIssueLimitedProconCommands", "ChkCanIssueAllProconCommands",
                "ChkCanIssueLimitedProconPluginCommands", "ChkCanEditMapZones",
                "ChkCanEditTextChatModerationList", "ChkCanShutdownServer"
            };

            foreach (string n in names)
                SetCheckbox(n, value);
        }

        private void OnAddAccount(object sender, RoutedEventArgs e)
        {
            if (_application?.AccountsList == null)
                return;

            var usernameInput = this.FindControl<TextBox>("AccountUsernameInput");
            var passwordInput = this.FindControl<TextBox>("AccountPasswordInput");

            string username = usernameInput?.Text?.Trim() ?? "";
            string password = passwordInput?.Text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return;

            _application.AccountsList.CreateAccount(username, password);
            RefreshAccountList();

            if (usernameInput != null) usernameInput.Text = "";
            if (passwordInput != null) passwordInput.Text = "";
        }

        private void OnRemoveAccount(object sender, RoutedEventArgs e)
        {
            if (_application?.AccountsList == null || string.IsNullOrEmpty(_selectedAccountName))
                return;

            _application.AccountsList.DeleteAccount(_selectedAccountName);
            _selectedAccountName = null;
            RefreshAccountList();

            var headerText = this.FindControl<TextBlock>("PrivilegesHeaderText");
            if (headerText != null) headerText.Text = "Select an account to edit privileges";

            var saveBtn = this.FindControl<Button>("SavePrivilegesButton");
            if (saveBtn != null) saveBtn.IsVisible = false;

            SetAllCheckboxes(false);
        }

        private void OnChangePassword(object sender, RoutedEventArgs e)
        {
            if (_application?.AccountsList == null)
                return;

            var usernameInput = this.FindControl<TextBox>("AccountUsernameInput");
            var passwordInput = this.FindControl<TextBox>("AccountPasswordInput");

            string username = usernameInput?.Text?.Trim() ?? "";
            string password = passwordInput?.Text ?? "";

            if (string.IsNullOrEmpty(username))
                username = _selectedAccountName ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return;

            _application.AccountsList.ChangePassword(username, password);

            if (passwordInput != null) passwordInput.Text = "";
        }

        private void OnPresetFullAdmin(object sender, RoutedEventArgs e) => SetAllCheckboxes(true);

        private void OnPresetModerator(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxes(false);
            SetCheckbox("ChkCanLogin", true);
            SetCheckbox("ChkCanKillPlayers", true);
            SetCheckbox("ChkCanKickPlayers", true);
            SetCheckbox("ChkCanMovePlayers", true);
            SetCheckbox("ChkCanTemporaryBanPlayers", true);
            SetCheckbox("ChkCanEditBanList", true);
            SetCheckbox("ChkCanIssueLimitedPunkbusterCommands", true);
        }

        private void OnPresetSpectator(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxes(false);
            SetCheckbox("ChkCanLogin", true);
        }

        private void OnPresetClear(object sender, RoutedEventArgs e) => SetAllCheckboxes(false);

        private void OnSavePrivileges(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAccountName) || _application?.AccountsList == null)
                return;

            if (!_application.AccountsList.Contains(_selectedAccountName))
                return;

            UInt32 flags = 0;

            if (GetCheckbox("ChkCanLogin")) flags |= (UInt32)Privileges.CanLogin;
            if (GetCheckbox("ChkCanAlterServerSettings")) flags |= (UInt32)Privileges.CanAlterServerSettings;
            if (GetCheckbox("ChkCanUseMapFunctions")) flags |= (UInt32)Privileges.CanUseMapFunctions;
            if (GetCheckbox("ChkCanKillPlayers")) flags |= (UInt32)Privileges.CanKillPlayers;
            if (GetCheckbox("ChkCanKickPlayers")) flags |= (UInt32)Privileges.CanKickPlayers;
            if (GetCheckbox("ChkCanTemporaryBanPlayers")) flags |= (UInt32)Privileges.CanTemporaryBanPlayers;
            if (GetCheckbox("ChkCanPermanentlyBanPlayers")) flags |= (UInt32)Privileges.CanPermanentlyBanPlayers;
            if (GetCheckbox("ChkCanMovePlayers")) flags |= (UInt32)Privileges.CanMovePlayers;
            if (GetCheckbox("ChkCanIssueLimitedPunkbusterCommands")) flags |= (UInt32)Privileges.CanIssueLimitedPunkbusterCommands;
            if (GetCheckbox("ChkCanIssueAllPunkbusterCommands")) flags |= (UInt32)Privileges.CanIssueAllPunkbusterCommands;
            if (GetCheckbox("ChkCanEditMapList")) flags |= (UInt32)Privileges.CanEditMapList;
            if (GetCheckbox("ChkCanEditBanList")) flags |= (UInt32)Privileges.CanEditBanList;
            if (GetCheckbox("ChkCanEditReservedSlotsList")) flags |= (UInt32)Privileges.CanEditReservedSlotsList;
            if (GetCheckbox("ChkCanIssueLimitedProconCommands")) flags |= (UInt32)Privileges.CanIssueLimitedProconCommands;
            if (GetCheckbox("ChkCanIssueAllProconCommands")) flags |= (UInt32)Privileges.CanIssueAllProconCommands;
            if (GetCheckbox("ChkCanIssueLimitedProconPluginCommands")) flags |= (UInt32)Privileges.CanIssueLimitedProconPluginCommands;
            if (GetCheckbox("ChkCanEditMapZones")) flags |= (UInt32)Privileges.CanEditMapZones;
            if (GetCheckbox("ChkCanEditTextChatModerationList")) flags |= (UInt32)Privileges.CanEditTextChatModerationList;
            if (GetCheckbox("ChkCanShutdownServer")) flags |= (UInt32)Privileges.CanShutdownServer;

            CPrivileges newPrivs = new CPrivileges(flags);

            // Apply privileges through the client if available
            if (_client != null)
            {
                try
                {
                    Account account = _application.AccountsList[_selectedAccountName];
                    _client.ProconProtectedLayerSetPrivileges(account, newPrivs);
                    var status = this.FindControl<TextBlock>("SaveStatusText");
                    if (status != null) status.Text = $"Privileges saved for {_selectedAccountName}";
                }
                catch
                {
                    var status = this.FindControl<TextBlock>("SaveStatusText");
                    if (status != null) status.Text = "Failed to save privileges";
                }
            }
        }
    }
}
