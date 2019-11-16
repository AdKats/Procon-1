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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using PRoCon.Controls.ControlsEx;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;
using PRoCon.Core.Logging;
using PRoCon.Core.Remote;
using PRoCon.Forms;

namespace PRoCon.Controls {
    public partial class uscServerConnection : uscPage {

        private frmMain m_frmParent = null;
        private frmManageAccounts m_frmAccounts = null;
        private PRoConClient m_prcConnection = null;
        private CLocalization m_clocLanguage = null;
        private CPrivileges m_cpPrivileges;

        private PRoConApplication m_praApplication;

        private ListViewColumnSorter m_lvwColumnSorter = new ListViewColumnSorter();

        private TabPage m_tabParentLayerControl;
        private uscParentLayerControl m_uscParentLayerControl;

        private string[] ma_timeDescriptionsShort = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };

        #region Delegates

        delegate void DispatchEventDelegate();
        delegate void DispatchExceptionDelegate(Exception eError);
        delegate void DispatchServerInfoDelegate(CServerInfo csiServerDetails);
        delegate void DispatchPlayerInfoListDelegate(List<CPlayerInfo> lstPlayers);
        delegate void DispatchPunkbusterInfoDelegate(CPunkbusterInfo pbInfo);
        delegate void DispatchStringListDelegate(List<string> lstCollection);
        delegate void DispatchConsoleOutputEventCallback(string strConsoleOutput);
        delegate void DispatchBanInfoDelegate(CBanInfo cbiBan);
        delegate void DispatchBanInfoCollectionDelegate(List<CBanInfo> cbiBans);
        delegate void DispatchPrivilegesInfoDelegate(CPrivileges spPrivs);

        delegate void SetLoadedPluginsCallback(List<string> lstClassNames);
        delegate void SetPluginVariablesCallback(string strClassName, string strScriptName, Dictionary<string, string[]> dicSvVariables);

        delegate void DispatchStringEventDelegate(string strValue);

        #endregion

        public PRoConClient Client {
            get { return this.m_prcConnection; }
        }

        public int ConnectingFrame {
            get;
            set;
        }

        private void uscServerConnection_Resize(object sender, EventArgs e) {
            this.Refresh();
        }

        public uscServerConnection(PRoConApplication paProcon, PRoConClient prcConnection, frmMain frmParent, frmManageAccounts frmAccounts) {
        //public uscServerConnection(PRoConApplication paProcon, ProConClient prcConnection, frmMain frmParent, frmManageAccounts frmAccounts, uscServerPlayerTreeview uscServerPlayerTree, string strHost, UInt16 iu16Port, string strUsername, string strPassword) {

            InitializeComponent();

            this.m_praApplication = paProcon;
            this.m_prcConnection = prcConnection;
            this.m_prcConnection.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcConnection_GameTypeDiscovered);

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.m_cpPrivileges = new CPrivileges(CPrivileges.FullPrivilegesFlags);

            this.m_frmParent = frmParent;
            this.m_frmAccounts = frmAccounts;

            this.tbcClientTabs.ImageList = this.m_frmParent.iglIcons;

            this.uscLogin.BackgroundHostPort = prcConnection.HostNamePort;

            if (prcConnection.State != ConnectionState.Connected) {
                this.uscLogin.Dock = DockStyle.Fill;
                this.uscLogin.Show();
            }
            else {
                this.uscLogin.Hide();
            }

            this.uscLists.OnTabChange += new OnTabChangeDelegate(uscLists_OnTabChange);

            this.tabPlayerList.ImageKey = "mouse.png";
            this.tabLists.ImageKey = "table.png";
            this.tabChat.ImageKey = "comments.png";
            this.tabEvents.ImageKey = "flag_blue.png";
            this.tabMapView.ImageKey = "map-pin.png";
            this.tabServerSettings.ImageKey = "server_edit.png";
            this.tabPlugins.ImageKey = "plugin.png";
            this.tabAccounts.ImageKey = "vcard.png";
            this.tabConsole.ImageKey = "application_xp_terminal.png";

            #region Map Controls

            this.SettingLoading = this.m_frmParent.picAjaxStyleLoading.Image;
            this.SettingSuccess = this.m_frmParent.picAjaxStyleSuccess.Image;
            this.SettingFail = this.m_frmParent.picAjaxStyleFail.Image;

            this.btnRestartRound.Image = this.m_frmParent.iglIcons.Images["arrow-retweet.png"];
            this.btnNextRound.Image = this.m_frmParent.iglIcons.Images["arrow-step-over.png"];

            this.AsyncSettingControls.Add("admin.runNextRound", new AsyncStyleSetting(this.picNextRound, null, new Control[] { this.btnNextRound }, true));
            this.AsyncSettingControls.Add("admin.restartRound", new AsyncStyleSetting(this.picRestartRound, null, new Control[] { this.btnRestartRound }, true));

            #endregion

            this.uscPlugins.GetPluginDetails += new uscPluginPanel.GetPluginDetailsDelegate(uscPlugins_GetPluginDetails);
            this.uscPlugins.SetPluginVariable += new uscPluginPanel.SetPluginVariableDelegate(uscPlugins_SetPluginVariable);
            this.uscPlugins.PluginEnabled += new uscPluginPanel.PluginEnabledDelegate(uscPlugins_PluginEnabled);
            this.uscPlugins.PluginLoaded += new uscPluginPanel.PluginEventDelegate(uscPlugins_PluginLoaded);
            this.uscPlugins.PluginVariablesAltered += new uscPluginPanel.PluginEventDelegate(uscPlugins_PluginVariablesAltered);
            this.uscPlugins.ReloadPlugins += new uscPluginPanel.EventDelegate(uscPlugins_ReloadPlugins);
            this.uscPlugins.OnTabChange += new OnTabChangeDelegate(uscPlugins_OnTabChange);

            this.uscAccounts.ManageAccountsRequest += new uscAccountsPanel.ManageAccountsRequestDelegate(uscAccounts_ManageAccountsRequest);

            this.uscServerConsole.SendCommand += new uscConsolePanel.SendCommandDelegate(uscServerConsole_SendCommand);
            this.uscServerConsole.SendListCommand += new uscConsolePanel.SendListCommandDelegate(uscServerConsole_SendListCommand);
            this.uscServerConsole.OnTabChange += new OnTabChangeDelegate(uscServerConsole_OnTabChange);

            this.m_tabParentLayerControl = new TabPage("Parent Layer Control");
            this.m_tabParentLayerControl.Name = "tabLayerControl";
            this.m_tabParentLayerControl.Padding = new Padding(8);
            this.m_tabParentLayerControl.UseVisualStyleBackColor = true;

            this.m_uscParentLayerControl = new uscParentLayerControl();
            this.m_uscParentLayerControl.Dock = DockStyle.Fill;
            this.m_uscParentLayerControl.BackColor = Color.Transparent;
            //this.m_uscParentLayerControl.SendCommand += new uscParentLayerControl.SendCommandDelegate(m_uscParentLayerControl_SendCommand);
            this.m_uscParentLayerControl.Initialize(this.m_frmParent, this);
            this.m_tabParentLayerControl.Controls.Add(m_uscParentLayerControl);
            this.m_uscParentLayerControl.OnTabChange += new OnTabChangeDelegate(m_uscParentLayerControl_OnTabChange);

            this.uscPlugins.Initialize(this.m_frmParent, this);
            this.uscPlugins.SetConnection(this.m_prcConnection);
            this.uscLogin.Initalize(this.m_frmParent, this);
            this.uscLogin.SetConnection(this.m_prcConnection);
            this.uscLogin.SetLocalization(this.m_prcConnection.Language);

            this.uscMap.SetConnection(this.m_prcConnection);
            this.uscEvents.SetConnection(this.m_prcConnection);
            this.uscLists.SetConnection(this.m_prcConnection);
            this.uscSettings.SetConnection(this.m_prcConnection);
            this.uscServerConsole.SetConnection(this.m_prcConnection);
            this.uscChat.SetConnection(this.m_prcConnection);
            this.uscPlayers.SetConnection(this.m_prcConnection);
            this.m_uscParentLayerControl.SetConnection(this.m_prcConnection);

            this.uscAccounts.SetConnection(this.m_praApplication, this.m_prcConnection);
        }

        void m_prcConnection_GameTypeDiscovered(PRoConClient sender) {
            this.InvokeIfRequired(() => {
                this.uscPlayers.Initialize(this.m_frmParent, this);
                this.uscLists.Initialize(this.m_frmParent, this);
                this.uscChat.Initialize(this);
                this.uscEvents.Initalize(this.m_frmParent, this);
                this.uscMap.Initalize(this.m_frmParent);
                this.uscSettings.Initialize(this.m_frmParent);

                this.uscAccounts.Initalize(this.m_frmParent, this);
                this.uscServerConsole.Initialize(this.m_frmParent, this);

                //this.m_prcConnection = new ProConClient(paProcon, strHost, iu16Port, strUsername, strPassword);
                //paProcon.Connections.Add(this.m_prcConnection);

                this.m_prcConnection.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_prcConnection_ProconPrivileges);
                this.m_prcConnection.ProconVersion += new PRoConClient.ProconVersionHandler(m_prcConnection_ProconVersion);

                this.m_prcConnection.PluginConsole.WriteConsole += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(PluginConsole_WriteConsole);

                this.m_prcConnection.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcConnection_ServerInfo);
                this.m_prcConnection.Game.LoadingLevel += new FrostbiteClient.LoadingLevelHandler(m_prcConnection_LoadingLevel);
                this.m_prcConnection.Game.LevelStarted += new FrostbiteClient.EmptyParamterHandler(Game_LevelStarted);
                this.m_prcConnection.Game.LevelLoaded += new FrostbiteClient.LevelLoadedHandler(m_prcConnection_LevelLoaded);

                //this.m_prcConnection.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcConnection_PlayerJoin);
                //this.m_prcConnection.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcConnection_PlayerLeft);
                this.m_prcConnection.Game.Version += new FrostbiteClient.VersionHandler(Game_Version);

                this.m_prcConnection.Game.RunNextRound += new FrostbiteClient.EmptyParamterHandler(Game_RunNextLevel);
                this.m_prcConnection.Game.RestartRound += new FrostbiteClient.EmptyParamterHandler(Game_RestartLevel);
                this.m_prcConnection.Game.ResponseError += new FrostbiteClient.ResponseErrorHandler(Game_ResponseError);

                this.m_prcConnection.PluginsCompiled += new PRoConClient.EmptyParamterHandler(m_prcConnection_PluginsCompiled);

                if (this.m_prcConnection.PluginsManager != null) {
                    this.m_prcConnection.PluginsManager.PluginVariableAltered += new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
                    this.m_prcConnection.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
                    this.m_prcConnection.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
                }

                if (this.m_prcConnection.PluginsManager != null) {
                    this.uscPlugins.SetLoadedPlugins(this.m_prcConnection.PluginsManager.Plugins.LoadedClassNames);
                    this.uscPlugins.SetEnabledPlugins(this.m_prcConnection.PluginsManager.Plugins.EnabledClassNames);
                }

                if (this.m_prcConnection.PluginConsole != null) {
                    foreach (LogEntry leEntry in this.m_prcConnection.PluginConsole.LogEntries) {
                        this.PluginConsole_WriteConsole(leEntry.Logged, leEntry.Text);
                    }
                }

                if (this.m_prcConnection.CurrentServerInfo.ServerName.Length > 0) {
                    this.m_prcConnection_ServerInfo(this.m_prcConnection.Game, this.m_prcConnection.CurrentServerInfo);
                }

                if (this.m_prcConnection.GameType == "BF3" || this.m_prcConnection.GameType == "BF4" || this.m_prcConnection.GameType == "BFHL" || this.m_prcConnection.GameType == "MOHW") {
                    this.tbcClientTabs.TabPages.Remove(this.tabMapView);
                }

                this.SetLocalization(this.m_prcConnection.Language);

                this.SetVersionInfoLabels(this.m_prcConnection.Game);
            });
        }

        // Minimizing to tray, then maximizing from tray will fire the Load event again.
        //private bool m_blFormLoaded = false;
        private void uscServerConnection_Load(object sender, EventArgs e) {

            if (Program.ProconApplication.OptionsSettings.ShowRoundTimerConstantly) {
                this.lblRoundTime.Visible = true;
            } 
            else {
                this.lblRoundTime.Visible = false;
            }

            if (this.m_prcConnection != null) {
                this.m_clocLanguage = this.m_prcConnection.Language;

                //this.lblVersion.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblVersion", new string[] { this.m_prcConnection.VersionNumber });

                if (this.m_prcConnection.PluginsManager != null) {
                    this.uscPlugins.SetLoadedPlugins(this.m_prcConnection.PluginsManager.Plugins.LoadedClassNames);
                    this.uscPlugins.SetEnabledPlugins(this.m_prcConnection.PluginsManager.Plugins.EnabledClassNames);
                }

                if (this.m_prcConnection.PluginConsole != null) {
                    foreach (LogEntry leEntry in new List<LogEntry>(this.m_prcConnection.PluginConsole.LogEntries)) {
                        this.PluginConsole_WriteConsole(leEntry.Logged, leEntry.Text);
                    }
                }

                if (this.m_prcConnection.State != ConnectionState.Connected) {
                    this.uscLogin.Dock = DockStyle.Fill;
                    this.uscLogin.Show();
                }
                else {
                    this.uscLogin.Hide();
                }

                if (this.m_prcConnection.IsPRoConConnection == true) {
                    this.m_prcConnection_ProconPrivileges(this.m_prcConnection, this.m_prcConnection.Privileges);
                }
            }
        }

        public override void SetLocalization(CLocalization clocLanguage) {
            this.m_clocLanguage = clocLanguage;

            if (this.m_clocLanguage != null) {

                //this.m_prcConnection.SetLocalization(this.m_clocLanguage);

                this.uscLogin.SetLocalization(this.m_clocLanguage);

                this.tabPlayerList.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabPlayerList", null);
                this.uscPlayers.SetLocalization(this.m_clocLanguage);
                this.tabChat.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabChat", null);
                this.uscChat.SetLocalization(this.m_clocLanguage);

                this.uscMap.SetLocalization(this.m_clocLanguage);

                this.tabMapView.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabMapView");
                this.tabEvents.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabEvents", null);
                this.uscEvents.SetLocalization(this.m_clocLanguage);
                this.tabLists.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabLists", null);
                this.uscLists.SetLocalization(this.m_clocLanguage);
                this.tabServerSettings.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabServerSettings", null);
                this.uscSettings.SetLocalization(this.m_clocLanguage);
                this.tabPlugins.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabPlugins", null);
                this.uscPlugins.SetLocalization(this.m_clocLanguage);
                this.tabAccounts.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabAccounts", null);
                this.uscAccounts.SetLocalization(this.m_clocLanguage);
                this.tabConsole.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabConsole", null);
                this.uscServerConsole.SetLocalization(this.m_clocLanguage);

                this.m_tabParentLayerControl.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.tabParentLayerControl", null);
                this.m_uscParentLayerControl.SetLocalization(this.m_clocLanguage);
                
                this.toolTipMapControls.SetToolTip(this.btnNextRound, this.m_clocLanguage.GetLocalized("uscServerConnection.btnNextRound.ToolTip"));
                this.toolTipMapControls.SetToolTip(this.btnRestartRound, this.m_clocLanguage.GetLocalized("uscServerConnection.btnRestartRound.ToolTip"));

                this.ma_timeDescriptionsShort[13] = this.m_clocLanguage.GetLocalized("global.Seconds.Short", null);
                this.ma_timeDescriptionsShort[12] = this.m_clocLanguage.GetLocalized("global.Seconds.Short", null);
                this.ma_timeDescriptionsShort[11] = this.m_clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
                this.ma_timeDescriptionsShort[10] = this.m_clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
                this.ma_timeDescriptionsShort[9] = this.m_clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
                this.ma_timeDescriptionsShort[8] = this.m_clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
                this.ma_timeDescriptionsShort[7] = this.m_clocLanguage.GetLocalized("global.Days.Short", null) + " ";
                this.ma_timeDescriptionsShort[6] = this.m_clocLanguage.GetLocalized("global.Days.Short", null) + " ";
                this.ma_timeDescriptionsShort[5] = this.m_clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
                this.ma_timeDescriptionsShort[4] = this.m_clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
                this.ma_timeDescriptionsShort[3] = this.m_clocLanguage.GetLocalized("global.Months.Short", null) + " ";
                this.ma_timeDescriptionsShort[2] = this.m_clocLanguage.GetLocalized("global.Months.Short", null) + " ";
                this.ma_timeDescriptionsShort[1] = this.m_clocLanguage.GetLocalized("global.Years.Short", null) + " ";
                this.ma_timeDescriptionsShort[0] = this.m_clocLanguage.GetLocalized("global.Years.Short", null) + " ";

                if (this.m_prcConnection.CurrentServerInfo != null) {
                    this.SetServerInfoLabels(this.m_prcConnection.CurrentServerInfo);
                    //this.lblPlayerCount.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblPlayerCount", new string[] { this.m_prcConnection.CurrentServerInfo.PlayerCount.ToString(), this.m_prcConnection.CurrentServerInfo.MaxPlayerCount.ToString() });
                    //this.lblCurrentGameMode.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentGameMode", new string[] {  });
                    //this.lblCurrentMapName.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentMapName", new string[] { this.m_prcConnection.GetFriendlyMapname(this.m_prcConnection.CurrentServerInfo.Map) });

                    //this.lblCurrentMapName.Text = String.Format("{0} - {1}", this.m_prcConnection.GetFriendlyGamemode(this.m_prcConnection.CurrentServerInfo.GameMode), this.m_prcConnection.GetFriendlyMapname(this.m_prcConnection.CurrentServerInfo.Map));
                    //this.lblCurrentRound.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentRound", this.m_prcConnection.CurrentServerInfo.CurrentRound.ToString(), this.m_prcConnection.CurrentServerInfo.TotalRounds.ToString());
                }
                else { 
                    //this.lblPlayerCount.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblPlayerCount", new string[] { "", "" });
                    //this.lblCurrentGameMode.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentGameMode", new string[] { "" });
                    //this.lblCurrentMapName.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentMapName", new string[] { "" });
                    this.lblCurrentMapName.Text = String.Empty;
                    this.lblCurrentRound.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentRound", "", "");
                }

                if (this.m_prcConnection.Game != null) {
                    this.SetVersionInfoLabels(this.m_prcConnection.Game);
                }
            }
        }

        #region Tab Changes

        public delegate void OnTabChangeDelegate(object sender, Stack<string> stkTabIndexes);
        public event OnTabChangeDelegate OnTabChange;

        private void tbcClientTabs_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.OnTabChange != null) {
                Stack<string> stkTabIndexes = new Stack<string>();
                stkTabIndexes.Push(tbcClientTabs.SelectedTab.Name);
                
                this.OnTabChange(this, stkTabIndexes);
            }

            if (this.tbcClientTabs.SelectedTab == this.tabMapView) {
                this.uscMap.IsMapSelected = true;
            }
            else {
                this.uscMap.IsMapSelected = false;
            }

        }

        public void SetTabIndexes(Stack<string> stkTabIndexes) {
            if (tbcClientTabs.TabPages.ContainsKey(stkTabIndexes.Peek()) == true) {
                this.tbcClientTabs.SelectedTab = tbcClientTabs.TabPages[stkTabIndexes.Pop()];
            }

            if (stkTabIndexes.Count > 0) {
                switch (tbcClientTabs.SelectedTab.Name) {
                    case "tabLists":
                        this.uscLists.SetTabIndexes(stkTabIndexes);
                        break;
                    case "tabPlugins":
                        this.uscPlugins.SetTabIndexes(stkTabIndexes);
                        break;
                    case "tabConsole":
                        this.uscServerConsole.SetTabIndexes(stkTabIndexes);
                        break;
                    case "tabLayerControl":
                        this.m_uscParentLayerControl.SetTabIndexes(stkTabIndexes);
                        break;
                }
            }
        }

        void uscServerConsole_OnTabChange(object sender, Stack<string> stkTabIndexes) {
            if (this.OnTabChange != null) {
                stkTabIndexes.Push(tbcClientTabs.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        void m_uscParentLayerControl_OnTabChange(object sender, Stack<string> stkTabIndexes) {
            if (this.OnTabChange != null) {
                stkTabIndexes.Push(tbcClientTabs.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        void uscPlugins_OnTabChange(object sender, Stack<string> stkTabIndexes) {
            if (this.OnTabChange != null) {
                stkTabIndexes.Push(tbcClientTabs.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        void uscLists_OnTabChange(object sender, Stack<string> stkTabIndexes) {
            if (this.OnTabChange != null) {
                stkTabIndexes.Push(tbcClientTabs.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        #endregion

        #region Connection and Login Events

        #region ServerInfo updates and simulators

        private void m_prcConnection_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo) {
            this.InvokeIfRequired(() => {
                this.SetServerInfoLabels(csiServerInfo);

                this.SetVersionInfoLabels(sender);
            });
        }

        private void SetServerInfoLabels(CServerInfo csiServerInfo) {
            //this.lblServerName.Text = String.Format("{0} [{1}]", csiServerInfo.ServerName, this.m_prcConnection.HostNamePort);
            this.uscAccounts.ServerName = csiServerInfo.ServerName;
            //this.lblCurrentMapName.Text = String.Format("{0} - {1}", this.m_prcConnection.GetFriendlyGamemode(csiServerInfo.GameMode), this.m_prcConnection.GetFriendlyMapname(csiServerInfo.Map));
            // add enhanced detection
            CMap tmpMap = this.m_prcConnection.GetFriendlyMapByFilenamePlayList(csiServerInfo.Map, csiServerInfo.GameMode);

            this.lblCurrentMapName.Text = tmpMap != null ? String.Format("{0} - {1}", tmpMap.GameMode, tmpMap.PublicLevelName) : String.Empty;

            this.toolTipMapControls.SetToolTip(this.lblCurrentMapName, csiServerInfo.Map);

            if ((this.Client.Game is BFHLClient || this.Client.Game is BF4Client || this.Client.Game is BF3Client || this.Client.Game is MOHWClient) && (csiServerInfo.CurrentRound != csiServerInfo.TotalRounds)) {
                this.lblCurrentRound.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentRound", (csiServerInfo.CurrentRound + 1).ToString(), csiServerInfo.TotalRounds.ToString());
            }
            else {
                this.lblCurrentRound.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblCurrentRound", csiServerInfo.CurrentRound.ToString(), csiServerInfo.TotalRounds.ToString());
            }
            
            this.lblMappack.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblMappack", csiServerInfo.Mappack.ToString());

            if (csiServerInfo.ConnectionState.Length > 0) {

                this.lblPlasmaStatus.Text = this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.lblPlasmaStatus.{0}", csiServerInfo.ConnectionState));
                // this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus, this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.lblPlasmaStatus.{0}.ToolTip", csiServerInfo.ConnectionState)));
                // &#xa; or Environment.NewLine 
                if (csiServerInfo.GameMod == GameMods.BC2 || csiServerInfo.GameMod == GameMods.VIETNAM) {
                    this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus, 
                        this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.lblPlasmaStatus.{0}.ToolTip", csiServerInfo.ConnectionState))
                        + Environment.NewLine + Environment.NewLine +
                        this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ExternalGameIpandPort.ToolTip") + "\t" + csiServerInfo.ExternalGameIpandPort
                        + Environment.NewLine +
                        this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.JoinQueueEnabled.ToolTip") + "\t"
                            + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.JoinQueueEnabled.{0}.ToolTip", csiServerInfo.JoinQueueEnabled))
                        + Environment.NewLine +
                        this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ServerRegion.ToolTip") + "\t\t"
                            + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.ServerRegion.{0}.ToolTip", csiServerInfo.ServerRegion))
                        + Environment.NewLine +
                        this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.PunkBusterVersion.ToolTip") + "\t" + csiServerInfo.PunkBusterVersion
                        + Environment.NewLine 
                        + Environment.NewLine
                    );
                } else {
                    this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus, this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.lblPlasmaStatus.{0}.ToolTip", csiServerInfo.ConnectionState)));
                }

                switch (csiServerInfo.ConnectionState) {
                    case "NotConnected":
                        this.lblPlasmaStatus.ForeColor = Color.Maroon;
                        break;
                    case "ConnectedToBackend":
                        this.lblPlasmaStatus.ForeColor = Color.Gold;
                        break;
                    case "AcceptingPlayers":
                        this.lblPlasmaStatus.ForeColor = Color.MediumSeaGreen;
                        break;
                    default: break;
                }
            }
            // BF3 & MOHW goes here cause it has ConnectionState parameter is empty
            if (this.Client.Game is BF3Client) {
                this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus,
                    this.m_clocLanguage.GetLocalized("uscServerConnection.lblPlasmaStatus.AcceptingPlayers.ToolTip")
                        + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ExternalGameIpandPort.ToolTip") + "\t" + csiServerInfo.ExternalGameIpandPort
                    + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.JoinQueueEnabled.ToolTip") + "\t"
                    + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.JoinQueueEnabled.{0}.ToolTip", csiServerInfo.JoinQueueEnabled))
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("QuickMatch Detected:", "uscServerConnection.extServerInfo.QuickMatch.ToolTip") + "\t"
                    + this.m_clocLanguage.GetDefaultLocalized(csiServerInfo.QuickMatch.ToString(), String.Format("uscServerConnection.extServerInfo.QuickMatch.{0}.ToolTip", csiServerInfo.QuickMatch.ToString()))
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ServerRegion.ToolTip") + "\t\t"
                    + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.ServerRegion.{0}.ToolTip", csiServerInfo.ServerRegion))
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Server Country:", "uscServerConnection.extServerInfo.ServerCountry.ToolTip") + "\t\t" + csiServerInfo.ServerCountry
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Closest Ping Site:", "uscServerConnection.extServerInfo.PingSite.ToolTip") + "\t\t" +
                    this.m_clocLanguage.GetDefaultLocalized(csiServerInfo.PingSite, String.Format("uscServerConnection.extServerInfo.PingSite.{0}.ToolTip", csiServerInfo.PingSite))
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.PunkBusterVersion.ToolTip") + "\t" + csiServerInfo.PunkBusterVersion
                    + Environment.NewLine
                    + Environment.NewLine
                );
            }
            if (this.Client.Game is BFHLClient || this.Client.Game is BF4Client) {
                this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus,
                    this.m_clocLanguage.GetLocalized("uscServerConnection.lblPlasmaStatus.AcceptingPlayers.ToolTip")
                        + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ExternalGameIpandPort.ToolTip") + "\t" + csiServerInfo.ExternalGameIpandPort
                    + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.JoinQueueEnabled.ToolTip") + "\t"
                    + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.JoinQueueEnabled.{0}.ToolTip", csiServerInfo.JoinQueueEnabled))
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("MatchMaking Enabled:", "uscServerConnection.extServerInfo.MatchMaking.ToolTip") + "\t"
                    + this.m_clocLanguage.GetDefaultLocalized(csiServerInfo.QuickMatch.ToString(), String.Format("uscServerConnection.extServerInfo.QuickMatch.{0}.ToolTip", csiServerInfo.QuickMatch.ToString()))
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ServerRegion.ToolTip") + "\t\t"
                    + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.ServerRegion.{0}.ToolTip", csiServerInfo.ServerRegion))
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Server Country:", "uscServerConnection.extServerInfo.ServerCountry.ToolTip") + "\t\t" + csiServerInfo.ServerCountry
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Closest Ping Site:", "uscServerConnection.extServerInfo.PingSite.ToolTip") + "\t\t" +
                    this.m_clocLanguage.GetDefaultLocalized(csiServerInfo.PingSite, String.Format("uscServerConnection.extServerInfo.PingSite.{0}.ToolTip", csiServerInfo.PingSite))
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.PunkBusterVersion.ToolTip") + "\t" + csiServerInfo.PunkBusterVersion
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Blaze Player Count:", "uscServerConnection.extServerInfo.BlazePlayerCount.ToolTip") + "\t\t" + csiServerInfo.BlazePlayerCount
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Blaze Game State:", "uscServerConnection.extServerInfo.BlazeGameState.ToolTip") + "\t\t" + csiServerInfo.BlazeGameState
                    + Environment.NewLine
                    + Environment.NewLine
                );
            }
            // MoHW is way different to BF3 R-33
            if (this.Client.Game is MOHWClient) {
                this.toolTipPlasma.SetToolTip(this.lblPlasmaStatus,
                    this.m_clocLanguage.GetLocalized("uscServerConnection.lblPlasmaStatus.AcceptingPlayers.ToolTip")
                        + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.ServerRegion.ToolTip") + "\t\t"
                        + this.m_clocLanguage.GetLocalized(String.Format("uscServerConnection.extServerInfo.ServerRegion.{0}.ToolTip", csiServerInfo.ServerRegion))
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Server Country:", "uscServerConnection.extServerInfo.ServerCountry.ToolTip") + "\t\t" + csiServerInfo.ServerCountry
                    + Environment.NewLine +
                    this.m_clocLanguage.GetDefaultLocalized("Closest Ping Site:", "uscServerConnection.extServerInfo.PingSite.ToolTip") + "\t\t" +
                    this.m_clocLanguage.GetDefaultLocalized(csiServerInfo.PingSite, String.Format("uscServerConnection.extServerInfo.PingSite.{0}.ToolTip", csiServerInfo.PingSite))
                    + Environment.NewLine + Environment.NewLine +
                    this.m_clocLanguage.GetLocalized("uscServerConnection.extServerInfo.PunkBusterVersion.ToolTip") + "\t" + csiServerInfo.PunkBusterVersion
                    + Environment.NewLine
                    + Environment.NewLine
                );
            }

        }

        private void m_prcConnection_LoadingLevel(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal) {
            this.InvokeIfRequired(() => {
                if (String.Compare(this.Client.GameType, "MOH", true) == 0) {
#pragma warning disable CS0612 // Type or member is obsolete
                    this.SetServerInfoLabels(new CServerInfo(this.m_prcConnection.CurrentServerInfo.ServerName,
                                                            mapFileName,
                                                            this.m_prcConnection.GetPlaylistByMapname(mapFileName),
                                                            this.m_prcConnection.CurrentServerInfo.PlayerCount,
                                                            this.m_prcConnection.CurrentServerInfo.MaxPlayerCount,
                                                            roundsPlayed + 1,
                                                            this.m_prcConnection.CurrentServerInfo.TotalRounds,
                                                            this.m_prcConnection.CurrentServerInfo.TeamScores,
                                                            this.m_prcConnection.CurrentServerInfo.ConnectionState));
#pragma warning restore CS0612 // Type or member is obsolete
                }
                else {
#pragma warning disable CS0612 // Type or member is obsolete
                    this.SetServerInfoLabels(new CServerInfo(this.m_prcConnection.CurrentServerInfo.ServerName,
                                                            mapFileName,
                                                            this.m_prcConnection.GetPlaylistByMapname(mapFileName),
                                                            this.m_prcConnection.CurrentServerInfo.PlayerCount,
                                                            this.m_prcConnection.CurrentServerInfo.MaxPlayerCount,
                                                            roundsPlayed,
                                                            this.m_prcConnection.CurrentServerInfo.TotalRounds,
                                                            this.m_prcConnection.CurrentServerInfo.TeamScores,
                                                            this.m_prcConnection.CurrentServerInfo.ConnectionState));
#pragma warning restore CS0612 // Type or member is obsolete
                }
                this.m_prcConnection.Game.SendGetMaplistNextLevelIndexPacket();
            });
        }

        private void Game_LevelStarted(FrostbiteClient sender) {
            sender.SendServerinfoPacket();
        }

        private void m_prcConnection_LevelLoaded(FrostbiteClient sender, string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            this.InvokeIfRequired(() => {
                if (String.Compare(this.Client.GameType, "BF3", true) == 0 || String.Compare(this.Client.GameType, "MOHW", true) == 0) {
#pragma warning disable CS0612 // Type or member is obsolete
                    this.SetServerInfoLabels(new CServerInfo(this.m_prcConnection.CurrentServerInfo.ServerName,
                                                            mapFileName,
                                                            Gamemode,
                                                            this.m_prcConnection.CurrentServerInfo.PlayerCount,
                                                            this.m_prcConnection.CurrentServerInfo.MaxPlayerCount,
                                                            roundsPlayed,
                                                            this.m_prcConnection.CurrentServerInfo.TotalRounds,
                                                            this.m_prcConnection.CurrentServerInfo.TeamScores,
                                                            this.m_prcConnection.CurrentServerInfo.ConnectionState));
#pragma warning restore CS0612 // Type or member is obsolete
                    this.m_prcConnection.Game.SendGetMaplistGetMapIndicesPacket();
                }
            });
        }

        #endregion

        private void SetVersionInfoLabels(FrostbiteClient sender) {
            this.InvokeIfRequired(() => {
                string version = sender.VersionNumber;

                if (sender.FriendlyVersionNumber.Length > 0) {
                    version = String.Format("{0} ({1})", sender.VersionNumber, sender.FriendlyVersionNumber);
                }

                this.lblVersion.Text = this.m_clocLanguage.GetLocalized("uscServerConnection.lblVersion", version);
            });
        }

        private void Game_Version(FrostbiteClient sender, string serverType, string serverVersion) {
            this.InvokeIfRequired(() => this.SetVersionInfoLabels(sender));
        }

        private void m_prcConnection_ProconVersion(PRoConClient sender, Version version) {
            this.InvokeIfRequired(() => {
                this.lblLayerVersion.Text = version.ToString();
                this.lblLayerVersion.Visible = true;

                if (sender.ConnectedLayerVersion != null) {

                    Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    int comparedVersions = assemblyVersion.CompareTo(sender.ConnectedLayerVersion);

                    if (comparedVersions > 0) {
                        // Older.
                        this.lblLayerVersion.ForeColor = Color.Maroon;
                        this.toolTipPlasma.SetToolTip(this.lblLayerVersion, this.m_clocLanguage.GetLocalized("uscServerConnection.lblLayerVersion.Older.ToolTip"));
                    }
                    else if (comparedVersions < 0) {
                        // Newer.
                        this.lblLayerVersion.ForeColor = Color.Maroon;
                        this.toolTipPlasma.SetToolTip(this.lblLayerVersion, this.m_clocLanguage.GetLocalized("uscServerConnection.lblLayerVersion.Newer.ToolTip"));
                    }
                    else {
                        // Same.
                        this.lblLayerVersion.ForeColor = Color.MediumSeaGreen;
                        this.toolTipPlasma.SetToolTip(this.lblLayerVersion, this.m_clocLanguage.GetLocalized("uscServerConnection.lblLayerVersion.Same.ToolTip"));
                    }
                }
            });
        }

        private void m_prcConnection_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs) {
            this.InvokeIfRequired(() => {
                if (spPrivs.CanIssueLimitedProconPluginCommands == true && this.tbcClientTabs.TabPages.Contains(this.m_tabParentLayerControl) == false) {
                    this.tbcClientTabs.TabPages.Add(this.m_tabParentLayerControl);
                    this.m_tabParentLayerControl.ImageKey = "sitemap_color.png";

                    /*
                this.m_prcConnection.SendCommand(new List<string> { "procon.account.listAccounts" });
                this.m_prcConnection.SendCommand(new List<string> { "procon.account.listLoggedIn" });

                this.m_prcConnection.SendCommand(new List<string> { "procon.plugin.listLoaded" });
                this.m_prcConnection.SendCommand(new List<string> { "procon.plugin.listEnabled" });
                */
                }
                else if (spPrivs.CanIssueLimitedProconCommands == false && this.tbcClientTabs.TabPages.Contains(this.m_tabParentLayerControl) == true) {
                    this.tbcClientTabs.TabPages.Remove(this.m_tabParentLayerControl);
                }

                if (this.m_praApplication.OptionsSettings.LayerHideLocalPlugins == true) {
                    this.tbcClientTabs.TabPages.Remove(this.tabPlugins);
                    // this.tabPlugins.Hide();
                }

                if (this.m_praApplication.OptionsSettings.LayerHideLocalAccounts == true) {
                    this.tbcClientTabs.TabPages.Remove(this.tabAccounts);
                    // this.tabAccounts.Hide();
                }

                this.pnlMapControls.Visible = spPrivs.CanUseMapFunctions;

                this.m_prcConnection.SendGetProconVarsPacket("TEMP_BAN_CEILING");
                //this.m_prcConnection.SendRequest(new List<string> { "procon.vars", "TEMP_BAN_CEILING" });
            });
        }

        #endregion

        #region Player List

        /*
        private void m_prcConnection_PlayerJoin(FrostbiteClient sender, string playerName) {
            this.SetServerInfoLabels(new CServerInfo(this.m_prcConnection.CurrentServerInfo.ServerName,
                                                    this.m_prcConnection.CurrentServerInfo.Map,
                                                    this.m_prcConnection.CurrentServerInfo.GameMode,
                                                    this.m_prcConnection.PlayerList.Count,
                                                    this.m_prcConnection.CurrentServerInfo.MaxPlayerCount,
                                                    this.m_prcConnection.CurrentServerInfo.CurrentRound,
                                                    this.m_prcConnection.CurrentServerInfo.TotalRounds));
        }

        private void m_prcConnection_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer) {
            this.SetServerInfoLabels(new CServerInfo(this.m_prcConnection.CurrentServerInfo.ServerName,
                                                    this.m_prcConnection.CurrentServerInfo.Map,
                                                    this.m_prcConnection.CurrentServerInfo.GameMode,
                                                    this.m_prcConnection.PlayerList.Count,
                                                    this.m_prcConnection.CurrentServerInfo.MaxPlayerCount,
                                                    this.m_prcConnection.CurrentServerInfo.CurrentRound,
                                                    this.m_prcConnection.CurrentServerInfo.TotalRounds));
        }
        */
        public void PlayerSelectionChange(string strSoldierName) {
            this.uscChat.PlayerSelectionChange(strSoldierName);
            this.uscPlayers.PlayerSelectionChange(strSoldierName);
        }

        public bool BeginDragDrop() {

            bool blBeginSuccess = false;

            if (this.m_cpPrivileges.CanMovePlayers == true) {
                //this.m_uscServerPlayerTree.BeginDragDrop(this);
                this.uscPlayers.BeginDragDrop();

                blBeginSuccess = true;
            }

            return blBeginSuccess;
        }

        public void EndDragDrop() {
            //this.m_uscServerPlayerTree.EndDragDrop(this);
            this.uscPlayers.EndDragDrop();
        }

        #endregion

        #region Plugins

        private bool m_blUpdatingPlugins = false;

        private void uscPlugins_ReloadPlugins() {
            this.m_prcConnection.CompilePlugins(this.m_praApplication.OptionsSettings.PluginPermissions);
        }

        private void m_prcConnection_PluginsCompiled(PRoConClient sender) {
            this.InvokeIfRequired(() => {
                this.uscPlugins.SetLoadedPlugins(this.m_prcConnection.PluginsManager.Plugins.LoadedClassNames);
                this.m_prcConnection.PluginsManager.PluginVariableAltered += new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
                this.m_prcConnection.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
                this.m_prcConnection.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
            });
        }

        private void Plugins_PluginVariableAltered(PluginDetails spdNewDetails) {
            this.InvokeIfRequired(() => this.uscPlugins.RefreshPlugin());
        }

        private void uscPlugins_PluginVariablesAltered(PluginDetails spdPlugin) {

        }

        private void uscPlugins_PluginLoaded(PluginDetails spdPlugin) {

        }

        private void Plugins_PluginDisabled(string strClassName) {
            this.InvokeIfRequired(() => {
                this.m_blUpdatingPlugins = true;

                if (this.uscPlugins.LoadedPlugins.ContainsKey(strClassName) == true) {
                    this.uscPlugins.LoadedPlugins[strClassName].Checked = false;
                }

                this.m_blUpdatingPlugins = false;
            });
        }

        private void Plugins_PluginEnabled(string strClassName) {
            this.InvokeIfRequired(() => {
                this.m_blUpdatingPlugins = true;

                if (this.uscPlugins.LoadedPlugins.ContainsKey(strClassName) == true) {
                    this.uscPlugins.LoadedPlugins[strClassName].Checked = true;
                }

                this.m_blUpdatingPlugins = false;
            });
        }

        private void uscPlugins_PluginEnabled(string strClassName, bool blEnabled) {

            if (this.m_blUpdatingPlugins == false) {

                if (blEnabled == true) {
                    this.m_prcConnection.PluginsManager.EnablePlugin(strClassName);
                }
                else {
                    this.m_prcConnection.PluginsManager.DisablePlugin(strClassName);
                }

                this.uscPlugins.RefreshPlugin();
            }
        }

        private void uscPlugins_SetPluginVariable(string strClassName, string strVariable, string strValue) {
            this.m_prcConnection.PluginsManager.SetPluginVariable(strClassName, strVariable, strValue);
        }

        private PluginDetails uscPlugins_GetPluginDetails(string strClassName) {
            PluginDetails spdReturn = new PluginDetails();
            spdReturn.ClassName = strClassName;
            spdReturn.Name = strClassName;

            if (this.m_prcConnection.PluginsManager != null) {
                spdReturn = this.m_prcConnection.PluginsManager.GetPluginDetails(strClassName);
            }

            return spdReturn;
        }

        private void PluginConsole_WriteConsole(DateTime dtLoggedTime, string strLoggedText) {
            this.InvokeIfRequired(() => this.uscPlugins.Write(dtLoggedTime, strLoggedText));
        }

        #endregion

        #region PRoCon Layer and accounts
        
        public delegate void ManageAccountsRequestDelegate(object sender, EventArgs e);
        public event ManageAccountsRequestDelegate ManageAccountsRequest;

        void uscAccounts_ManageAccountsRequest(object sender, EventArgs e) {
            this.ManageAccountsRequest(this, e);
        }

        #endregion

        #region Console

        // Redesign note; Leaving these two here instead of putting in ServerConsole.cs until ExecuteCommand is in PRoConClient.cs
        void uscServerConsole_SendListCommand(List<string> lstCommand) {
            this.m_prcConnection.SendRequest(lstCommand);
        }

        void uscServerConsole_SendCommand(string strCommand) {
            if (strCommand.Length > 0) {
                List<string> lstWords = Packet.Wordify(strCommand);

                if (lstWords.Count >= 1 && lstWords[0].Length >= 1 && lstWords[0][0] == '/') {
                    lstWords[0] = lstWords[0].Remove(0, 1);
                    this.m_praApplication.ExecutePRoConCommand(this.m_prcConnection, lstWords, 0);
                    //this.ExecutePRoConCommand(lstWords);
                }
                else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "punkBuster.pb_sv_command", true) == 0) {
                    lstWords.Clear();

                    lstWords.Add("punkBuster.pb_sv_command");
                    lstWords.Add(Regex.Replace(strCommand, "^punkbuster.pb_sv_command ", "", RegexOptions.IgnoreCase));

                    this.m_prcConnection.SendRequest(lstWords);
                }
                else {
                    this.m_prcConnection.SendRequest(lstWords);
                }
            }
        }

        #endregion

        #region Map Controls

        private void Game_RestartLevel(FrostbiteClient sender) {
            this.InvokeIfRequired(() => this.OnSettingResponse("admin.restartRound", true));
        }

        private void btnRestartRound_Click(object sender, EventArgs e) {
            if (this.m_prcConnection != null && this.m_prcConnection.Game != null) {
                if (this.m_praApplication.OptionsSettings.ShowCfmMsgRoundRestartNext == true) {
                    DialogResult cfmRestartRound = MessageBox.Show(this.m_clocLanguage.GetLocalized("uscServerConnection.MessageBox.RestartRound"), "PRoCon Frostbite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (cfmRestartRound == DialogResult.No) {
                        return;
                    }
                }
                this.WaitForSettingResponse("admin.restartRound");
                this.m_prcConnection.Game.SendAdminRestartRoundPacket();
                //this.m_prcConnection.Game.SendServerinfoPacket();
            }
        }

        private void Game_RunNextLevel(FrostbiteClient sender) {
            this.InvokeIfRequired(() => this.OnSettingResponse("admin.runNextRound", true));
        }

        private void btnNextRound_Click(object sender, EventArgs e) {
            if (this.m_prcConnection != null && this.m_prcConnection.Game != null) {
                if (this.m_praApplication.OptionsSettings.ShowCfmMsgRoundRestartNext == true) {
                    DialogResult cfmRestartLevel = MessageBox.Show(this.m_clocLanguage.GetLocalized("uscServerConnection.MessageBox.NextRound"), "PRoCon Frostbite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (cfmRestartLevel == DialogResult.No) {
                        return;
                    }
                }
                this.WaitForSettingResponse("admin.runNextRound");
                this.m_prcConnection.Game.SendAdminRunNextRoundPacket();
                //this.m_prcConnection.Game.SendServerinfoPacket();
            }
        }

        private void Game_ResponseError(FrostbiteClient sender, Packet originalRequest, string errorMessage) {
            this.InvokeIfRequired(() => {
                if (originalRequest.Words.Count >= 1) {
                    this.OnSettingResponse(originalRequest.Words[0].ToLower(), null, false);
                }
            });
        }

        #endregion

        private void tmrTimerTicks_Tick(object sender, EventArgs e) {

            if (this.m_prcConnection != null) {
                if (this.m_prcConnection.CurrentServerInfo != null) {

                    if (this.m_prcConnection.CurrentServerInfo.RoundTime >= 0) {
                        this.lblRoundTime.Text = uscPlayerPunishPanel.SecondsToText((UInt32)(this.m_prcConnection.CurrentServerInfo.RoundTime++), this.ma_timeDescriptionsShort);
                    }

                    if ((this.lblServerUptime.Visible = (this.m_prcConnection.CurrentServerInfo.ServerUptime >= 0) == true)) {
                        string uptimeText = uscPlayerPunishPanel.SecondsToText((UInt32)(this.m_prcConnection.CurrentServerInfo.ServerUptime++), this.ma_timeDescriptionsShort);
                        this.lblServerUptime.Text = this.m_clocLanguage.GetDefaultLocalized("Uptime: " + uptimeText, "uscServerConnection.lblServerUptime", uptimeText);
                    }
                }
            }
        }

        private void lblCurrentRound_MouseEnter(object sender, EventArgs e) {
            if (this.m_prcConnection != null) {
                if (this.m_prcConnection.CurrentServerInfo != null && this.m_prcConnection.CurrentServerInfo.RoundTime >= 0) {
                    this.lblRoundTime.Visible = true;
                    this.lblCurrentRound.Font = new Font(this.lblCurrentRound.Font, FontStyle.Bold);
                }
            }
        }

        private void lblCurrentRound_MouseLeave(object sender, EventArgs e) {
            if (this.m_prcConnection != null) {
                if (this.m_prcConnection.CurrentServerInfo != null && this.m_prcConnection.CurrentServerInfo.RoundTime >= 0) {
                    if (!Program.ProconApplication.OptionsSettings.ShowRoundTimerConstantly)
                    {
                        this.lblRoundTime.Visible = false;
                    }
                    this.lblCurrentRound.Font = new Font(this.lblCurrentRound.Font, FontStyle.Regular);
                }
            }
        }
    }
}