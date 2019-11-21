using PRoCon.Core;
using PRoCon.Core.Accounts;
using PRoCon.Core.Remote;
using PRoCon.Core.Remote.Layer;
using PRoCon.Forms;
using System;
using System.Drawing;
using System.Globalization;
using System.Net.Sockets;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscAccountsPanel : UserControl
    {

        private frmMain _main;
        private CLocalization _language;

        private PRoConApplication _application;
        private PRoConClient _client;

        //private CPrivileges m_spPrivileges;

        // This variable is only used by this panel to show an example of the Layer Name.
        private string _serverName;
        public string ServerName
        {
            set
            {
                this._serverName = value;
                this.lblExampleLayerName.Text = this.txtLayerName.Text.Replace("%servername%", this._serverName);
            }
        }

        public uscAccountsPanel()
        {
            InitializeComponent();

            //this.m_uscParentPanel = null;
            this._main = null;
            this._language = null;

            //this.m_spPrivileges = new CPrivileges();
            //this.m_spPrivileges.PrivilegesFlags = CPrivileges.FullPrivilegesFlags;

            this.uscPrivileges.OnUpdatePrivileges += new uscPrivilegesSelection.OnUpdatePrivilegesDelegate(uscPrivileges_OnUpdatePrivileges);
            this.uscPrivileges.OnCancelPrivileges += new uscPrivilegesSelection.OnCancelPrivilegesDelegate(uscPrivileges_OnCancelPrivileges);
        }

        private void uscAccountsPanel_Load(object sender, EventArgs e)
        {

            if (this._application != null && this._client != null)
            {
                foreach (Account accLoadedAccount in this._application.AccountsList)
                {
                    this.AccountsList_AccountAdded(accLoadedAccount);

                    if (this._client.Layer.AccountPrivileges.Contains(accLoadedAccount.Name) == true)
                    {
                        this.uscAccountsPanel_AccountPrivilegesChanged(this._client.Layer.AccountPrivileges[accLoadedAccount.Name]);
                    }
                }

                this.txtLayerName.Text = this._client.Layer.NameFormat;
                this.txtLayerBindingAddress.Text = this._client.Layer.BindingAddress;
                this.txtLayerStartPort.Text = this._client.Layer.ListeningPort.ToString(CultureInfo.InvariantCulture);

                if (this._client.Layer.IsOnline == true)
                {
                    this.Layer_LayerOnline();
                }
            }
        }

        public void Initalize(frmMain frmMain, uscServerConnection uscConnectionPanel)
        {
            this._main = frmMain;

            this.picLayerServerStatus.Image = this._main.picLayerOffline.Image;

            this.pnlMainLayerServer.Dock = DockStyle.Fill;
            this.pnlStartPRoConLayer.Dock = DockStyle.Fill;
            this.pnlAccountPrivileges.Dock = DockStyle.Fill;

            this.lsvLayerAccounts.SmallImageList = this._main.iglIcons;
        }

        public void SetConnection(PRoConApplication praApplication, PRoConClient prcClient)
        {
            if ((this._application = praApplication) != null && (this._client = prcClient) != null)
            {

                if (this._client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this._client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this._application.AccountsList.AccountAdded += new PRoCon.Core.Accounts.AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
                this._application.AccountsList.AccountRemoved += new PRoCon.Core.Accounts.AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);

                foreach (Account acAccount in this._application.AccountsList)
                {
                    acAccount.AccountPasswordChanged += new Account.AccountPasswordChangedHandler(acAccount_AccountPasswordChanged);

                    if (this._client.Layer.AccountPrivileges.Contains(acAccount.Name) == true)
                    {
                        this._client.Layer.AccountPrivileges[acAccount.Name].AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(uscAccountsPanel_AccountPrivilegesChanged);
                    }
                }

                this._client.Layer.LayerStarted += Layer_LayerOnline;
                this._client.Layer.LayerShutdown += Layer_LayerOffline;
                this._client.Layer.SocketError += Layer_LayerSocketError;

                this._client.Layer.ClientConnected += Layer_ClientConnected;
            });
        }

        void Layer_ClientConnected(ILayerClient client)
        {
            client.Login += Layer_LayerClientLogin;
            client.Logout += Layer_LayerClientLogout;
        }

        public void SetLocalization(CLocalization clocLanguage)
        {

            if ((this._language = clocLanguage) != null)
            {

                this.lblLayerServerSetupTitle.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerServerSetupTitle", null);
                this.lnkStartStopLayer.Text = this._language.GetLocalized("uscAccountsPanel.lnkStartStopLayer.Start", null);
                this.lblLayerServerStatus.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerServerStatus.Offline", null);

                this.lblLayerAssignAccountPrivilegesTitle.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerAssignAccountPrivilegesTitle", null);

                this.colAccounts.Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.colAccounts", null);
                this.colPrivileges.Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.colPrivileges", null);
                this.colRConAccess.Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.colRConAccess", null);

                this.lnkManageAccounts.Text = this._language.GetLocalized("uscAccountsPanel.lnkManageAccounts", null);

                this.lblLayerStartTitle.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerStartTitle", null);
                this.lblLayerStartPort.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerStartPort", null);
                this.lblLayerBindingIP.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerBindingIP", null);
                this.lblBindingExplanation.Text = this._language.GetLocalized("uscAccountsPanel.lblBindingExplanation", null);
                this.lblLayerName.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerName", null);
                this.btnInsertName.Text = this._language.GetLocalized("uscAccountsPanel.btnInsertName", null);
                this.lblExampleLayerName.Text = this._language.GetLocalized("uscAccountsPanel.lblExampleLayerName", new[] { this.txtLayerName.Text.Replace("%servername%", this._serverName) });
                this.btnLayerStart.Text = this._language.GetLocalized("uscAccountsPanel.btnLayerStart", null);
                this.btnCancelLayerStart.Text = this._language.GetLocalized("uscAccountsPanel.btnCancelLayerStart", null);

                this.uscPrivileges.SetLocalization(this._language);

                this.RefreshLayerPrivilegesPanel();
            }
        }

        public delegate void ManageAccountsRequestDelegate(object sender, EventArgs e);
        public event ManageAccountsRequestDelegate ManageAccountsRequest;

        private bool _editingPrivileges;
        private void ShowLayerPanel(Control pnlShow)
        {
            this.pnlMainLayerServer.Hide();
            this.pnlStartPRoConLayer.Hide();
            this.pnlAccountPrivileges.Hide();

            this._editingPrivileges = false;

            if (pnlShow == this.pnlMainLayerServer)
            {
                this.lsvLayerAccounts.SelectedItems.Clear();
            }
            else if (pnlShow == this.pnlAccountPrivileges)
            {
                this._editingPrivileges = true;

                // Should be but still..
                if (this.lsvLayerAccounts.SelectedItems.Count > 0)
                {
                    this.uscPrivileges.AccountName = this.lsvLayerAccounts.SelectedItems[0].Text;
                }

                if (this.lsvLayerAccounts.SelectedItems.Count > 0)
                {
                    CPrivileges spPrivs = (CPrivileges)this.lsvLayerAccounts.SelectedItems[0].SubItems[1].Tag;

                    this.uscPrivileges.Privileges = spPrivs;
                }
                else
                {
                    this.uscPrivileges.Privileges = new CPrivileges();
                }
            }

            pnlShow.Show();
        }

        private void RefreshLayerPrivilegesPanel()
        {
            foreach (ListViewItem lviItem in this.lsvLayerAccounts.Items)
            {
                if (lviItem.SubItems[1].Tag != null && this._language != null && this._client != null)
                {

                    if (this._client.Layer.AccountPrivileges.Contains(lviItem.Text) == true)
                    {
                        CPrivileges spDetails = this._client.Layer.AccountPrivileges[lviItem.Text].Privileges;

                        if (spDetails.HasNoRconAccess == true)
                        {
                            lviItem.SubItems["rconaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.None", null);
                        }
                        else if (spDetails.HasLimitedRconAccess == true)
                        {
                            lviItem.SubItems["rconaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Limited", null);
                        }
                        else
                        {
                            lviItem.SubItems["rconaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Full", null);
                        }

                        if (spDetails.HasNoLocalAccess == true)
                        {
                            lviItem.SubItems["localaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.None", null);
                        }
                        else if (spDetails.HasLimitedLocalAccess == true)
                        {
                            lviItem.SubItems["localaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Limited", null);
                        }
                        else
                        {
                            lviItem.SubItems["localaccess"].Text = this._language.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Full", null);
                        }
                    }
                }
            }

            if (this._editingPrivileges == true)
            {
                this.ShowLayerPanel(this.pnlAccountPrivileges);
            }

            foreach (ColumnHeader ch in this.lsvLayerAccounts.Columns)
            {
                ch.Width = -2;
            }
        }

        void uscAccountsPanel_AccountPrivilegesChanged(AccountPrivilege item)
        {

            if (this.lsvLayerAccounts.Items.ContainsKey(item.Owner.Name) == true)
            {
                ListViewItem lviAccount = this.lsvLayerAccounts.Items[item.Owner.Name];

                lviAccount.SubItems["rconaccess"].Tag = item.Privileges;

                this.RefreshLayerPrivilegesPanel();
            }
        }

        void acAccount_AccountPasswordChanged(Account item)
        {

            if (this.lsvLayerAccounts.Items.ContainsKey(item.Name) == true)
            {
                this.lsvLayerAccounts.Items[item.Name].Tag = item.Password;
            }
        }

        void AccountsList_AccountRemoved(Account item)
        {
            item.AccountPasswordChanged -= new Account.AccountPasswordChangedHandler(acAccount_AccountPasswordChanged);

            if (this.lsvLayerAccounts.Items.ContainsKey(item.Name) == true)
            {
                this.lsvLayerAccounts.Items.Remove(this.lsvLayerAccounts.Items[item.Name]);
            }
        }

        void AccountsList_AccountAdded(Account item)
        {
            item.AccountPasswordChanged += new Account.AccountPasswordChangedHandler(acAccount_AccountPasswordChanged);

            if (this.lsvLayerAccounts.Items.ContainsKey(item.Name) == false)
            {

                ListViewItem lviNewAccount = new ListViewItem(item.Name)
                {
                    Name = item.Name,
                    Tag = item.Password,
                    ImageKey = @"status_offline.png"
                };

                ListViewItem.ListViewSubItem lsviNewSubitem = new ListViewItem.ListViewSubItem
                {
                    Name = @"rconaccess",
                    Tag = new CPrivileges()
                };
                lviNewAccount.SubItems.Add(lsviNewSubitem);

                lsviNewSubitem = new ListViewItem.ListViewSubItem
                {
                    Name = @"localaccess"
                };
                lviNewAccount.SubItems.Add(lsviNewSubitem);

                lsviNewSubitem = new ListViewItem.ListViewSubItem
                {
                    Text = String.Empty,
                    Name = @"ip"
                };
                lviNewAccount.SubItems.Add(lsviNewSubitem);

                this.lsvLayerAccounts.Items.Add(lviNewAccount);

                this.RefreshLayerPrivilegesPanel();
            }
        }

        private void lnkStartStopLayer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this._client.Layer.IsOnline == false)
            {
                this.ShowLayerPanel(this.pnlStartPRoConLayer);
            }
            else
            {
                this._client.Layer.IsEnabled = false;

                this._client.Layer.Shutdown();
            }
        }

        private void btnLayerStart_Click(object sender, EventArgs e)
        {
            this.ShowLayerPanel(this.pnlMainLayerServer);

            this._client.Layer.IsEnabled = true;
            this._client.Layer.BindingAddress = this.txtLayerBindingAddress.Text;
            this._client.Layer.ListeningPort = Convert.ToUInt16(this.txtLayerStartPort.Text);
            this._client.Layer.NameFormat = this.txtLayerName.Text;
            this._client.Layer.Start();
        }

        private void btnCancelLayerStart_Click(object sender, EventArgs e)
        {
            this.ShowLayerPanel(this.pnlMainLayerServer);
        }

        private void lstLayerAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {

                this.ShowLayerPanel(this.pnlAccountPrivileges);
            }
        }

        void uscPrivileges_OnCancelPrivileges()
        {
            this.ShowLayerPanel(this.pnlMainLayerServer);
        }

        void uscPrivileges_OnUpdatePrivileges(string strAccountName, CPrivileges spUpdatedPrivs)
        {
            if (this._client.Layer.AccountPrivileges.Contains(strAccountName) == true)
            {
                this._client.Layer.AccountPrivileges[strAccountName].SetPrivileges(spUpdatedPrivs);
            }

            this.ShowLayerPanel(this.pnlMainLayerServer);

            this.RefreshLayerPrivilegesPanel();
        }

        private void lnkManageAccounts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ManageAccountsRequest(this, new EventArgs());
        }

        private void Layer_LayerOnline()
        {
            this.InvokeIfRequired(() =>
            {
                this.picLayerServerStatus.Image = this._main.picLayerOnline.Image;

                this.lblLayerServerStatus.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerServerStatus.Online", new[] { this._client.Layer.ListeningPort.ToString(CultureInfo.InvariantCulture) });
                this.lblLayerServerStatus.ForeColor = Color.ForestGreen;

                this.lnkStartStopLayer.Text = this._language.GetLocalized("uscAccountsPanel.lnkStartStopLayer.Stop", null);
            });
        }

        private void Layer_LayerOffline()
        {
            this.InvokeIfRequired(() =>
            {
                this.picLayerServerStatus.Image = this._main.picLayerOffline.Image;

                this.lblLayerServerStatus.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerServerStatus.Offline", null);
                this.lblLayerServerStatus.ForeColor = Color.Maroon;

                this.lnkStartStopLayer.Text = this._language.GetLocalized("uscAccountsPanel.lnkStartStopLayer.Start", null);
            });
        }

        private void Layer_LayerSocketError(SocketException se)
        {
            this.InvokeIfRequired(() =>
            {
                this.picLayerServerStatus.Image = this._main.picLayerOffline.Image;

                this.lblLayerServerStatus.Text = this._language.GetLocalized("uscAccountsPanel.lblLayerServerStatus.Error", new[] { se.Message });
                this.lblLayerServerStatus.ForeColor = Color.Maroon;

                this.lnkStartStopLayer.Text = this._language.GetLocalized("uscAccountsPanel.lnkStartStopLayer.Start", null);
            });
        }

        private void Layer_LayerClientLogin(ILayerClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(sender.Username) == true)
                {
                    this.lsvLayerAccounts.Items[sender.Username].ImageKey = @"status_online.png";
                }
            });
        }

        private void Layer_LayerClientLogout(ILayerClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(sender.Username) == true)
                {
                    this.lsvLayerAccounts.Items[sender.Username].ImageKey = @"status_offline.png";
                }
            });
        }

        private void txtLayerName_TextChanged(object sender, EventArgs e)
        {
            this.lblExampleLayerName.Text = this.txtLayerName.Text.Replace("%servername%", this._serverName);
        }

        private void btnInsertName_Click(object sender, EventArgs e)
        {

            int iInsertPosition = this.txtLayerName.SelectionStart;

            this.txtLayerName.Text = this.txtLayerName.Text.Remove(iInsertPosition, this.txtLayerName.SelectionLength);
            this.txtLayerName.Text = this.txtLayerName.Text.Insert(iInsertPosition, "%servername%");
        }

        private void txtLayerStartPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b');
        }

    }
}
