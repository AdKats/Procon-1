/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote;
using PRoCon.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscParentLayerControl : UserControl
    {

        private frmMain m_frmMain;
        private CLocalization m_clocLanguage = null;

        private PRoConClient m_prcClient;

        private bool m_blUpdatingPlugins;
        private Dictionary<string, PluginDetails> m_dicRemotePlugins;

        //public delegate void SendCommandDelegate(List<string> lstCommand);
        //public event SendCommandDelegate SendCommand;

        private Dictionary<string, AsyncStyleSetting> m_dicAsyncSettingControls;

        public uscParentLayerControl()
        {
            InitializeComponent();

            this.m_dicRemotePlugins = new Dictionary<string, PluginDetails>();
            this.m_blUpdatingPlugins = false;

            this.pnlMainLayerServer.Dock = DockStyle.Fill;
            this.pnlAccountPrivileges.Dock = DockStyle.Fill;

            this.uscPlugins.OnTabChange += new uscServerConnection.OnTabChangeDelegate(uscPlugins_OnTabChange);

            this.m_dicAsyncSettingControls = new Dictionary<string, AsyncStyleSetting>();
            this.m_dicAsyncSettingControls.Add("procon.account.create", new AsyncStyleSetting(this.picCreateAccount, this.lnkCreateAccount, new Control[] { this.lnkEditAccount, this.lnkDeleteAccount, this.lnkCreateAccount, this.lnkChangeAccountPassword }, true));
            this.m_dicAsyncSettingControls.Add("procon.account.setPassword", new AsyncStyleSetting(this.picChangeAccountPassword, this.lnkCreateAccount, new Control[] { this.lnkEditAccount, this.lnkDeleteAccount, this.lnkCreateAccount, this.lnkChangeAccountPassword }, true));
            this.m_dicAsyncSettingControls.Add("procon.account.delete", new AsyncStyleSetting(this.picDeleteAccount, this.lnkCreateAccount, new Control[] { this.lnkEditAccount, this.lnkDeleteAccount, this.lnkCreateAccount, this.lnkChangeAccountPassword }, true));
            this.m_dicAsyncSettingControls.Add("procon.layer.setPrivileges", new AsyncStyleSetting(this.picEditAccount, this.lnkCreateAccount, new Control[] { this.lnkEditAccount, this.lnkDeleteAccount, this.lnkCreateAccount, this.lnkChangeAccountPassword }, true));

            this.uscPrivileges.OnUpdatePrivileges += new uscPrivilegesSelection.OnUpdatePrivilegesDelegate(uscPrivileges_OnUpdatePrivileges);
            this.uscPrivileges.OnCancelPrivileges += new uscPrivilegesSelection.OnCancelPrivilegesDelegate(uscPrivileges_OnCancelPrivileges);
        }

        public void Initialize(frmMain frmMainWindow, uscServerConnection uscParent)
        {

            this.m_frmMain = frmMainWindow;

            this.uscPlugins.Initialize(this.m_frmMain, uscParent);
            this.lsvLayerAccounts.SmallImageList = this.m_frmMain.iglIcons;
        }

        public void SetColour(string strVariable, string strValue)
        {
            this.uscPlugins.SetColour(strVariable, strValue);
        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_prcClient = prcClient) != null)
            {
                this.uscPlugins.SetConnection(prcClient);

                this.m_prcClient.RemoteAccountLoggedIn += new PRoConClient.RemoteAccountLoginStatusHandler(m_prcClient_RemoteAccountLoggedIn);
                this.m_prcClient.RemoteAccountCreated += new PRoConClient.RemoteAccountHandler(m_prcClient_RemoteAccountCreated);
                this.m_prcClient.RemoteAccountChangePassword += new PRoConClient.EmptyParamterHandler(m_prcClient_RemoteAccountChangePassword);
                this.m_prcClient.RemoteAccountDeleted += new PRoConClient.RemoteAccountHandler(m_prcClient_RemoteAccountDeleted);
                this.m_prcClient.RemoteAccountAltered += new PRoConClient.RemoteAccountAlteredHandler(m_prcClient_RemoteAccountAltered);

                this.m_prcClient.RemoteLoadedPlugins += new PRoConClient.RemoteLoadedPluginsHandler(m_prcClient_RemoteLoadedPlugins);
                this.m_prcClient.RemoteEnabledPlugins += new PRoConClient.RemoteEnabledPluginsHandler(m_prcClient_RemoteEnabledPlugins);
                this.m_prcClient.RemotePluginLoaded += new PRoConClient.RemotePluginLoadedHandler(m_prcClient_RemotePluginLoaded);
                this.m_prcClient.RemotePluginEnabled += new PRoConClient.RemotePluginEnabledHandler(m_prcClient_RemotePluginEnabled);
                this.m_prcClient.RemotePluginVariables += new PRoConClient.RemotePluginVariablesHandler(m_prcClient_RemotePluginVariables);
                this.m_prcClient.ReadRemotePluginConsole += new PRoConClient.ReadRemoteConsoleHandler(m_prcClient_ReadRemotePluginConsole);

                this.m_prcClient.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_prcClient_ProconPrivileges);
            }
        }

        public void SetLocalization(CLocalization clocLanguage)
        {

            if ((this.m_clocLanguage = clocLanguage) != null)
            {

                this.tabPlugins.Text = this.tabPlugins.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabPlugins", null);
                this.uscPlugins.SetLocalization(this.m_clocLanguage);
                //this.tbpPRoConLayer.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.Title", null);

                this.tabAccounts.Text = this.tabAccounts.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabAccounts", null);
                this.lblLayerServerSetupTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblLayerServerSetupTitle", null);
                this.lnkStartStopLayer.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lnkStartStopLayer.Start", null);
                //this.lnkStartStopLayer.LinkArea = new LinkArea(0, this.lnkStartStopLayer.Text.Length);
                this.lblLayerServerStatus.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblLayerServerStatus.Offline", null);

                this.lnkLayerForwardedTest.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lnkLayerForwardedTest", null);
                //this.lnkLayerForwardedTest.LinkArea = new LinkArea(0, this.lnkLayerForwardedTest.Text.Length);
                this.lblLayerForwardedTestStatus.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblLayerForwardedTestStatus.Unknown", null);

                this.lnkWhatIsPRoConLayer.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lnkWhatIsPRoConLayer", null);
                //this.lnkWhatIsPRoConLayer.LinkArea = new LinkArea(0, this.lnkWhatIsPRoConLayer.Text.Length);

                this.lblLayerAssignAccountPrivilegesTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblLayerAssignAccountPrivilegesTitle", null);

                this.colAccounts.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.colAccounts", null);
                this.colPrivileges.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.colPrivileges", null);
                this.colRConAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.colRConAccess", null);

                this.lnkEditAccount.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkEditAccount", null);
                this.lnkDeleteAccount.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkDeleteAccount", null);
                this.lblCreateNewAccount.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lblCreateNewAccount", null);
                this.lblUsername.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lblUsername", null);
                this.lblPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lblPassword", null);
                this.lnkCreateAccount.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkCreateAccount", null);
                this.lblUserNameExistsError.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lblUserNameExistsError", null);
                this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Blank", null);

                this.uscPrivileges.SetLocalization(this.m_clocLanguage);

                this.RefreshLayerPrivilegesPanel();
            }
        }

        public event uscServerConnection.OnTabChangeDelegate OnTabChange;

        private void tbcLayerControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OnTabChange != null)
            {
                Stack<string> stkTabIndexes = new Stack<string>();
                stkTabIndexes.Push(tbcLayerControl.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        public void SetTabIndexes(Stack<string> stkTabIndexes)
        {
            if (tbcLayerControl.TabPages.ContainsKey(stkTabIndexes.Peek()) == true)
            {
                this.tbcLayerControl.SelectedTab = tbcLayerControl.TabPages[stkTabIndexes.Pop()];

                if (stkTabIndexes.Count > 0)
                {
                    switch (tbcLayerControl.SelectedTab.Name)
                    {
                        case "tabPlugins":
                            this.uscPlugins.SetTabIndexes(stkTabIndexes);
                            break;
                    }
                }
            }
        }

        void uscPlugins_OnTabChange(object sender, Stack<string> stkTabIndexes)
        {
            if (this.OnTabChange != null)
            {
                stkTabIndexes.Push(tbcLayerControl.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        #region Remote Plugins

        private void m_prcClient_RemoteLoadedPlugins(PRoConClient sender, Dictionary<string, PluginDetails> loadedPlugins)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_dicRemotePlugins = loadedPlugins;
                this.m_blUpdatingPlugins = true;

                this.uscPlugins.SetLoadedPlugins(new List<string>(this.m_dicRemotePlugins.Keys));

                this.m_blUpdatingPlugins = false;
            });
        }
        /*
        public void SetRemoteLoadedPlugins(Dictionary<string, SPluginDetails> dicRemotePlugins) {

            this.m_dicRemotePlugins = dicRemotePlugins;
            this.m_blUpdatingPlugins = true;

            this.uscPlugins.SetLoadedPlugins(new List<string>(this.m_dicRemotePlugins.Keys));

            this.m_blUpdatingPlugins = false;
        }
        */
        private PluginDetails uscPlugins_GetPluginDetails(string strClassName)
        {

            PluginDetails spdReturnDetails = default(PluginDetails);

            if (this.m_dicRemotePlugins != null)
            {

                if (this.m_dicRemotePlugins.ContainsKey(strClassName) == true)
                {
                    spdReturnDetails = this.m_dicRemotePlugins[strClassName];
                }
            }

            return spdReturnDetails;
        }

        void m_prcClient_RemoteEnabledPlugins(PRoConClient sender, List<string> enabledPluginClasses)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_blUpdatingPlugins = true;

                foreach (ListViewItem lviPlugin in this.uscPlugins.LoadedPlugins)
                {

                    if (enabledPluginClasses.Contains(lviPlugin.Name) == true)
                    {
                        lviPlugin.Checked = true;
                    }
                    else
                    {
                        lviPlugin.Checked = false;
                    }
                }

                this.m_blUpdatingPlugins = false;
            });
        }

        /*
        public void SetRemoteEnabledPlugins(List<string> lstClassNames) {


        }
        */

        private void m_prcClient_RemotePluginLoaded(PRoConClient sender, PluginDetails spdDetails)
        {
            this.InvokeIfRequired(() =>
            {

                if (this.m_dicRemotePlugins.ContainsKey(spdDetails.ClassName) == true)
                {
                    this.m_dicRemotePlugins[spdDetails.ClassName] = spdDetails;
                }
                else
                {
                    this.m_dicRemotePlugins.Add(spdDetails.ClassName, spdDetails);
                }

                this.m_blUpdatingPlugins = true;
                this.uscPlugins.SetLoadedPlugins(new List<string>(this.m_dicRemotePlugins.Keys));
                this.m_blUpdatingPlugins = false;
            });
        }

        /*
        public void OnRemotePluginLoaded(SPluginDetails spdDetails) {

        }
        */

        private void m_prcClient_RemotePluginEnabled(PRoConClient sender, string strClassName, bool isEnabled)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_blUpdatingPlugins = true;

                if (this.uscPlugins.LoadedPlugins.ContainsKey(strClassName) == true)
                {
                    this.uscPlugins.LoadedPlugins[strClassName].Checked = isEnabled;
                }

                this.m_blUpdatingPlugins = false;
            });
        }

        /*
        public void RemotePluginEnabled(string strClassName, bool blEnabled) {

        }
        */

        private void m_prcClient_RemotePluginVariables(PRoConClient sender, string strClassName, List<CPluginVariable> lstVariables)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.m_dicRemotePlugins.ContainsKey(strClassName) == true)
                {
                    PluginDetails spdUpdatedDetails = this.m_dicRemotePlugins[strClassName];
                    spdUpdatedDetails.DisplayPluginVariables = lstVariables;
                    this.m_dicRemotePlugins[strClassName] = spdUpdatedDetails;

                    this.m_blUpdatingPlugins = true;
                    this.uscPlugins.SetLoadedPlugins(new List<string>(this.m_dicRemotePlugins.Keys));
                    this.m_blUpdatingPlugins = false;
                }
            });
        }

        private void uscPlugins_SetPluginVariable(string strClassName, string strVariable, string strValue)
        {

            if (this.m_blUpdatingPlugins == false)
            {
                this.m_prcClient.SendProconPluginSetVariablePacket(strClassName, strVariable, strValue);
            }
        }

        private void uscPlugins_PluginEnabled(string strClassName, bool blEnabled)
        {

            if (this.m_blUpdatingPlugins == false)
            {

                this.m_prcClient.SendProconPluginEnablePacket(strClassName, blEnabled);
            }
        }

        private void m_prcClient_ReadRemotePluginConsole(PRoConClient sender, DateTime loggedTime, string text)
        {
            this.InvokeIfRequired(() => this.uscPlugins.Write(loggedTime, text));
        }

        #endregion

        #region Settings Animator

        private void SetControlValue(Control ctrlTarget, object objValue)
        {

            if (objValue != null)
            {
                if (ctrlTarget is TextBox)
                {
                    ((TextBox)ctrlTarget).Text = (string)objValue;
                }
                else if (ctrlTarget is CheckBox)
                {
                    ((CheckBox)ctrlTarget).Checked = (bool)objValue;
                }
                else if (ctrlTarget is NumericUpDown)
                {
                    ((NumericUpDown)ctrlTarget).Value = (decimal)objValue;
                }
                else if (ctrlTarget is Label)
                {
                    ((Label)ctrlTarget).Text = (string)objValue;
                }
            }
        }

        private void WaitForSettingResponse(string strResponseCommand)
        {

            if (this.m_dicAsyncSettingControls.ContainsKey(strResponseCommand) == true)
            {
                //this.m_dicAsyncSettingControls[strResponseCommand].m_objOriginalValue = String.Empty;
                this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleLoading.Image;
                this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_TIMEOUT_TICKS;

                this.tmrTimeoutCheck.Enabled = true;

                foreach (Control ctrlEnable in this.m_dicAsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                {
                    if (ctrlEnable is TextBox)
                    {
                        ((TextBox)ctrlEnable).ReadOnly = true;
                    }
                    else
                    {
                        ctrlEnable.Enabled = false;
                    }
                }
            }
        }

        public void OnSettingResponse(string strResponseCommand, bool blSuccess)
        {

            if (this.m_dicAsyncSettingControls.ContainsKey(strResponseCommand) == true)
            {

                if (this.m_dicAsyncSettingControls[strResponseCommand].m_blReEnableControls == true)
                {
                    foreach (Control ctrlEnable in this.m_dicAsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs)
                    {
                        if (ctrlEnable is TextBox)
                        {
                            ((TextBox)ctrlEnable).ReadOnly = false;
                        }
                        else
                        {
                            ctrlEnable.Enabled = true;
                        }
                    }
                }

                this.m_dicAsyncSettingControls[strResponseCommand].IgnoreEvent = true;

                if (blSuccess == true)
                {
                    this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleSuccess.Image;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_blSuccess = true;
                }
                else
                {
                    this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleFail.Image;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_blSuccess = false;
                }

                this.tmrTimeoutCheck.Enabled = true;

                this.m_dicAsyncSettingControls[strResponseCommand].IgnoreEvent = false;
            }
        }


        private int CountTicking()
        {
            int i = 0;

            foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsync in this.m_dicAsyncSettingControls)
            {
                if (kvpAsync.Value.m_iTimeout >= 0)
                {
                    i++;
                }
            }

            return i;
        }

        private void tmrSettingsAnimator_Tick(object sender, EventArgs e)
        {
            //if (((from o in this.m_dicAsyncSettingControls where o.Value.m_iTimeout >= 0 select o).Count()) > 0) {
            if (this.CountTicking() > 0)
            {
                foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsyncSetting in this.m_dicAsyncSettingControls)
                {

                    kvpAsyncSetting.Value.m_iTimeout--;
                    if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == false)
                    {
                        kvpAsyncSetting.Value.m_picStatus.Image = this.m_frmMain.picAjaxStyleFail.Image;
                        kvpAsyncSetting.Value.m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;

                        kvpAsyncSetting.Value.m_blSuccess = true;
                    }
                    else if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == true)
                    {
                        kvpAsyncSetting.Value.m_picStatus.Image = null;

                        if (kvpAsyncSetting.Value.m_blReEnableControls == true)
                        {
                            foreach (Control ctrlEnable in kvpAsyncSetting.Value.ma_ctrlEnabledInputs)
                            {
                                if (ctrlEnable is TextBox)
                                {
                                    ((TextBox)ctrlEnable).ReadOnly = false;
                                }
                                else
                                {
                                    ctrlEnable.Enabled = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.tmrTimeoutCheck.Enabled = false;
            }
        }

        #endregion

        #region Remote Accounts

        private bool m_blEditingPrivileges = false;
        private void ShowLayerPanel(Panel pnlShow)
        {
            this.pnlMainLayerServer.Hide();
            this.pnlAccountPrivileges.Hide();

            this.m_blEditingPrivileges = false;

            if (pnlShow == this.pnlMainLayerServer)
            {
                this.lsvLayerAccounts.SelectedItems.Clear();
            }
            else if (pnlShow == this.pnlAccountPrivileges)
            {
                this.m_blEditingPrivileges = true;

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
                if (lviItem.SubItems[1].Tag != null && this.m_clocLanguage != null)
                {
                    CPrivileges spDetails = (CPrivileges)lviItem.SubItems[1].Tag;

                    if (spDetails.HasNoRconAccess == true)
                    {
                        lviItem.SubItems["rconaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.None", null);
                    }
                    else if (spDetails.HasLimitedRconAccess == true)
                    {
                        lviItem.SubItems["rconaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Limited", null);
                    }
                    else
                    {
                        lviItem.SubItems["rconaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Full", null);
                    }

                    if (spDetails.HasNoLocalAccess == true)
                    {
                        lviItem.SubItems["localaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.None", null);
                    }
                    else if (spDetails.HasLimitedLocalAccess == true)
                    {
                        lviItem.SubItems["localaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Limited", null);
                    }
                    else
                    {
                        lviItem.SubItems["localaccess"].Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lstLayerAccounts.Privileges.Full", null);
                    }

                }
            }

            if (this.m_blEditingPrivileges == true)
            {
                this.ShowLayerPanel(this.pnlAccountPrivileges);
            }

            foreach (ColumnHeader ch in this.lsvLayerAccounts.Columns)
            {
                ch.Width = -2;
            }

        }

        private void m_prcClient_RemoteAccountCreated(PRoConClient sender, string accountName)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(accountName) == false)
                {

                    this.OnSettingResponse("procon.account.create", true);

                    ListViewItem lviNewAccount = new ListViewItem(accountName);
                    lviNewAccount.Name = accountName;
                    lviNewAccount.ImageKey = "status_offline.png";

                    ListViewItem.ListViewSubItem lsviNewSubitem = new ListViewItem.ListViewSubItem();
                    //lsviNewSubitem.Text = "none";
                    lsviNewSubitem.Name = "rconaccess";
                    lsviNewSubitem.Tag = new CPrivileges();
                    lviNewAccount.SubItems.Add(lsviNewSubitem);

                    lsviNewSubitem = new ListViewItem.ListViewSubItem();
                    //lsviNewSubitem.Text = "none";
                    lsviNewSubitem.Name = "localaccess";
                    lviNewAccount.SubItems.Add(lsviNewSubitem);

                    this.lsvLayerAccounts.Items.Add(lviNewAccount);

                    this.RefreshLayerPrivilegesPanel();
                }
            });
        }
        /*
        public void OnAccountCreated(string strAccountName) {

        }
        */
        private void m_prcClient_RemoteAccountChangePassword(PRoConClient sender)
        {
            this.OnSettingResponse("procon.account.setPassword", true);
        }

        //public void OnAccountChangePasswordConfirmation() {
        //    this.OnSettingResponse("procon.account.setPassword", true);
        //}

        private void m_prcClient_RemoteAccountDeleted(PRoConClient sender, string accountName)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(accountName) == true)
                {
                    this.OnSettingResponse("procon.account.delete", true);
                    this.lsvLayerAccounts.Items.Remove(this.lsvLayerAccounts.Items[accountName]);
                }
            });
        }

        //public void OnAccountDeleted(string strAccountName) {

        //}

        private void m_prcClient_RemoteAccountAltered(PRoConClient sender, string accountName, CPrivileges accountPrivileges)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(accountName) == true)
                {
                    this.OnSettingResponse("procon.layer.setPrivileges", true);
                    this.lsvLayerAccounts.Items[accountName].SubItems["rconaccess"].Tag = accountPrivileges;

                    this.RefreshLayerPrivilegesPanel();
                }
            });
        }

        //public void OnAccountAltered(string strAccountName, CPrivileges spPrivs) {

        //}

        private void m_prcClient_RemoteAccountLoggedIn(PRoConClient sender, string accountName, bool isOnline)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(accountName) == true)
                {
                    this.lsvLayerAccounts.Items[accountName].ImageKey = isOnline == true ? "status_online.png" : "status_offline.png";
                }
            });
        }

        private void uscPrivileges_OnUpdatePrivileges(string strAccountName, CPrivileges spUpdatedPrivs)
        {
            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {
                this.WaitForSettingResponse("procon.layer.setPrivileges");
                this.m_prcClient.SendProconLayerSetPrivilegesPacket(this.lsvLayerAccounts.SelectedItems[0].Text, spUpdatedPrivs.PrivilegesFlags);
            }

            this.ShowLayerPanel(this.pnlMainLayerServer);
        }

        private void uscPrivileges_OnCancelPrivileges()
        {
            this.ShowLayerPanel(this.pnlMainLayerServer);
        }


        /*
        private void btnSavePrivileges_Click(object sender, EventArgs e) {

            UInt32 i = 0;

            i |= (1 & Convert.ToUInt32(this.chkAllowConnectionLogin.Checked));
            i |= (1 & Convert.ToUInt32(this.chkAlterServerSettings.Checked)) << 1;
            i |= (1 & Convert.ToUInt32(this.chkChangeCurrentMapFunctions.Checked)) << 2;

            i |= (1 & Convert.ToUInt32(this.rdoNoPlayerPunishment.Checked)) << 3;
            i |= (1 & Convert.ToUInt32(this.rdoKickingPlayersOnly.Checked)) << 4;
            i |= (1 & Convert.ToUInt32(this.rdoKickingTemporaryOnly.Checked)) << 5;
            i |= (1 & Convert.ToUInt32(this.rdoKicingTemporaryPermanent.Checked)) << 6;

            i |= (1 & Convert.ToUInt32(this.rdoNotAllowedToIssuePunkbusterCommands.Checked)) << 7;
            i |= (1 & Convert.ToUInt32(this.rdoLimitedPunkbusterAccess.Checked)) << 8;
            i |= (1 & Convert.ToUInt32(this.rdoFullPunkbusterAccess.Checked)) << 9;

            i |= (1 & Convert.ToUInt32(this.chkEditMapList.Checked)) << 10;
            i |= (1 & Convert.ToUInt32(this.chkEditBanList.Checked)) << 11;
            i |= (1 & Convert.ToUInt32(this.chkEditReservedSlotsList.Checked)) << 12;

            i |= (1 & Convert.ToUInt32(this.rdoNoProconAccess.Checked)) << 13;
            i |= (1 & Convert.ToUInt32(this.rdoLimitedProconAccess.Checked)) << 14;
            i |= (1 & Convert.ToUInt32(this.rdoFullProconAccess.Checked)) << 15;

            Privilege spUpdatedPrivileges = new Privilege();
            spUpdatedPrivileges.PrivilegesFlags = i;

            // TO DO: SendCommand Privs
            if (this.lsvLayerAccounts.SelectedItems.Count > 0 && this.SendCommand != null) {
                this.WaitForSettingResponse("procon.layer.setPrivileges");
                this.SendCommand(new List<string>() { "procon.layer.setPrivileges", this.lsvLayerAccounts.SelectedItems[0].Text, spUpdatedPrivileges.PrivilegesFlags.ToString() });
            }

            this.ShowLayerPanel(this.pnlMainLayerServer);
        }
        *
        private void btnCancelPrivileges_Click(object sender, EventArgs e) {
            this.ShowLayerPanel(this.pnlMainLayerServer);
        }
        

        private void chkAllowConnectionLogin_CheckedChanged(object sender, EventArgs e) {
            this.pnlRconAccess.Enabled = this.chkAllowConnectionLogin.Checked;

            if (this.pnlRconAccess.Enabled == false) {
                this.rdoNoProconAccess.Checked = true;
            }
        }
        */
        private void lnkEditAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {
                this.ShowLayerPanel(this.pnlAccountPrivileges);
            }
        }

        private void lnkCreateAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            this.WaitForSettingResponse("procon.account.create");
            this.m_prcClient.SendProconAccountCreatePacket(this.txtUsername.Text, this.txtPassword.Text);

            this.txtUsername.Focus();
            this.txtUsername.Clear();
            this.txtPassword.Clear();
        }

        private void lnkDeleteUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {
                this.WaitForSettingResponse("procon.account.delete");
                this.m_prcClient.SendProconAccountDeletePacket(this.lsvLayerAccounts.SelectedItems[0].Text);
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

            if (this.txtUsername.Text.Length > 0)
            {
                if (this.lsvLayerAccounts.Items.ContainsKey(this.txtUsername.Text) == true)
                {
                    this.lblUserNameExistsError.Visible = true;
                    this.lnkCreateAccount.Enabled = false;

                    this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Username", new string[] { this.txtUsername.Text });

                    if (this.txtPassword.Text.Length > 0)
                    {
                        this.lnkChangeAccountPassword.Enabled = true;
                    }
                }
                else
                {
                    if (this.txtPassword.Text.Length > 0)
                    {
                        this.lnkCreateAccount.Enabled = true;
                    }
                    this.lblUserNameExistsError.Visible = false;
                    this.lnkChangeAccountPassword.Enabled = false;
                    this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Blank", null);
                }
            }
            else
            {
                this.lnkCreateAccount.Enabled = false;
                this.lblUserNameExistsError.Visible = false;
                this.lnkChangeAccountPassword.Enabled = false;
                this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Blank", null);
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

            if (this.txtUsername.Text.Length > 0 && this.txtPassword.Text.Length > 0 && this.lsvLayerAccounts.Items.ContainsKey(this.txtUsername.Text) == false)
            {
                this.lnkCreateAccount.Enabled = true;
                this.lnkChangeAccountPassword.Enabled = false;
                this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Blank", null);
            }
            else if (this.txtUsername.Text.Length > 0 && this.txtPassword.Text.Length > 0 && this.lsvLayerAccounts.Items.ContainsKey(this.txtUsername.Text) == true)
            {
                this.lnkCreateAccount.Enabled = false;
                this.lnkChangeAccountPassword.Enabled = true;
                this.lnkChangeAccountPassword.Text = this.m_clocLanguage.GetLocalized("uscParentLayerControl.tabAccounts.lnkChangeAccountPassword.Username", new string[] { this.txtUsername.Text });
            }
            else
            {
                this.lnkCreateAccount.Enabled = false;
                this.lnkChangeAccountPassword.Enabled = false;
            }
        }

        private void lsvLayerAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {
                this.lnkEditAccount.Enabled = true;
                this.lnkDeleteAccount.Enabled = true;
                this.txtUsername.Text = this.lsvLayerAccounts.SelectedItems[0].Text;
            }
            else
            {
                this.lnkEditAccount.Enabled = false;
                this.lnkDeleteAccount.Enabled = false;
                this.txtUsername.Text = String.Empty;
            }

        }

        private void lnkChangeAccountPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.lsvLayerAccounts.SelectedItems.Count > 0)
            {
                this.WaitForSettingResponse("procon.account.setPassword");
                this.m_prcClient.SendProconAccountSetPasswordPacket(this.txtUsername.Text, this.txtPassword.Text);

                this.txtUsername.Focus();
                this.txtUsername.Clear();
                this.txtPassword.Clear();
            }
        }

        #endregion

        private bool m_isFormLoaded = false, m_isConnectionValid = false;
        private void RequestInitialSettings()
        {
            //if (this.m_isFormLoaded == true && this.m_isConnectionValid == true) {
            this.m_prcClient.SendProconAccountListAccountsPacket();
            this.m_prcClient.SendProconAccountListLoggedInPacket();

            this.m_prcClient.SendProconPluginListLoadedPacket();
            this.m_prcClient.SendProconPluginListEnabledPacket();
            //}
        }

        void m_prcClient_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_isConnectionValid = true;
                this.RequestInitialSettings();
            });
        }

        private void uscParentLayerControl_Load(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.m_isFormLoaded = true;
                this.RequestInitialSettings();
            }
        }
    }
}
