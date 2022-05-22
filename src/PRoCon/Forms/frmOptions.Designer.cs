namespace PRoCon.Forms {
    partial class frmOptions {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOptions));
            this.tbcOptions = new System.Windows.Forms.TabControl();
            this.tabBasics = new System.Windows.Forms.TabPage();
            this.rssFeedOptCheckbox = new System.Windows.Forms.CheckBox();
            this.localGeoIPCheckbox = new System.Windows.Forms.CheckBox();
            this.chkBasicsAutoCheckGameConfigsForUpdates = new System.Windows.Forms.CheckBox();
            this.chkBasicsEnablePluginLogging = new System.Windows.Forms.CheckBox();
            this.chkBasicsAutoApplyUpdates = new System.Windows.Forms.CheckBox();
            this.chkBasicsEnableEventsLogging = new System.Windows.Forms.CheckBox();
            this.chkBasicsMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkBasicsCloseToTray = new System.Windows.Forms.CheckBox();
            this.cboBasicsShowWindow = new PRoCon.Controls.ControlsEx.ComboBoxEx();
            this.lblBasicsShowWindow = new System.Windows.Forms.Label();
            this.lblBasicPreferences = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chkBasicsAutoCheckDownloadForUpdates = new System.Windows.Forms.CheckBox();
            this.lblBasicsPrivacy = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chkBasicsEnableConsoleLogging = new System.Windows.Forms.CheckBox();
            this.chkBasicsEnableChatLogging = new System.Windows.Forms.CheckBox();
            this.lblBasicsLogging = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBasicsSetLanguage = new System.Windows.Forms.Button();
            this.lblBasicsLanguage = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lnkBasicsAuthor = new System.Windows.Forms.LinkLabel();
            this.lblBasicsAuthor = new System.Windows.Forms.Label();
            this.cboBasicsLanguagePicker = new PRoCon.Controls.ControlsEx.ComboBoxEx();
            this.tabPlugins = new System.Windows.Forms.TabPage();
            this.chkEnablePluginDebugging = new System.Windows.Forms.CheckBox();
            this.lblPluginsDebug = new System.Windows.Forms.Label();
            this.lblPluginsChangesAfterRestart = new System.Windows.Forms.Label();
            this.panel19 = new System.Windows.Forms.Panel();
            this.pnlSandboxOptions = new System.Windows.Forms.Panel();
            this.numPluginMaxRuntimeSec = new System.Windows.Forms.NumericUpDown();
            this.numPluginMaxRuntimeMin = new System.Windows.Forms.NumericUpDown();
            this.lblPluginMaxRuntimeSec = new System.Windows.Forms.Label();
            this.lblPluginMaxRuntimeMin = new System.Windows.Forms.Label();
            this.lblPluginMaxRuntime = new System.Windows.Forms.Label();
            this.panel16 = new System.Windows.Forms.Panel();
            this.chkAllowSmtpConnections = new System.Windows.Forms.CheckBox();
            this.lblPluginsMail = new System.Windows.Forms.Label();
            this.panel13 = new System.Windows.Forms.Panel();
            this.chkAllowODBCConnections = new System.Windows.Forms.CheckBox();
            this.lblPluginsDatabases = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.pnlPluginsAllowedDomains = new System.Windows.Forms.Panel();
            this.lsvTrustedHostDomainPorts = new PRoCon.Controls.ControlsEx.ListViewNF();
            this.colTrustedDomainsHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTrustedPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnPluginsAddTrustedHostDomain = new System.Windows.Forms.Button();
            this.btnPluginsRemoveTrustedHostDomain = new System.Windows.Forms.Button();
            this.txtPluginsTrustedPort = new System.Windows.Forms.TextBox();
            this.lblPluginsPort = new System.Windows.Forms.Label();
            this.txtPluginsTrustedHostDomain = new System.Windows.Forms.TextBox();
            this.lblPluginsTrustedHostDomain = new System.Windows.Forms.Label();
            this.lblPluginsOutgoingConnections = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblPluginsSecurity = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.cboPluginsSandboxOptions = new PRoCon.Controls.ControlsEx.ComboBoxEx();
            this.tabHttpServer = new System.Windows.Forms.TabPage();
            this.pnlHttpServerSettings = new System.Windows.Forms.Panel();
            this.lblBindingExplanation = new System.Windows.Forms.Label();
            this.txtHttpServerBindingAddress = new System.Windows.Forms.TextBox();
            this.lblHttpServerBindingIP = new System.Windows.Forms.Label();
            this.lblHttpServerStartPort = new System.Windows.Forms.Label();
            this.txtHttpServerStartPort = new System.Windows.Forms.TextBox();
            this.pnlHttpServerTester = new System.Windows.Forms.Panel();
            this.lnkHttpServerExampleLink = new System.Windows.Forms.LinkLabel();
            this.picHttpServerForwardedTestStatus = new System.Windows.Forms.PictureBox();
            this.lblHttpServerForwardedTestStatus = new System.Windows.Forms.Label();
            this.lnkHttpServerForwardedTest = new System.Windows.Forms.LinkLabel();
            this.lnkStartStopHttpServer = new System.Windows.Forms.LinkLabel();
            this.picHttpServerServerStatus = new System.Windows.Forms.PictureBox();
            this.lblHttpServerServerStatus = new System.Windows.Forms.Label();
            this.lblHttpServerTitle = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.tabAdv = new System.Windows.Forms.TabPage();
            this.lblAdvStartupChangeNotice = new System.Windows.Forms.Label();
            this.chkAdvUsePluginOldStyleLoad = new System.Windows.Forms.CheckBox();
            this.lblAdvStartup = new System.Windows.Forms.Label();
            this.panel17 = new System.Windows.Forms.Panel();
            this.chkAdvShowCfmMsgRoundRestartNext = new System.Windows.Forms.CheckBox();
            this.lblAdvShowDICESpecialOptionsNotice = new System.Windows.Forms.Label();
            this.chkAdvShowDICESpecialOptions = new System.Windows.Forms.CheckBox();
            this.lblAdvSpecialSwitches = new System.Windows.Forms.Label();
            this.panel14 = new System.Windows.Forms.Panel();
            this.chkAdvShowRoundTimerConstantly = new System.Windows.Forms.CheckBox();
            this.lblAdvConVisuals = new System.Windows.Forms.Label();
            this.lblAdvLayerTabsChangeNotice = new System.Windows.Forms.Label();
            this.chkAdvHideLocalAccountsTab = new System.Windows.Forms.CheckBox();
            this.chkAdvHideLocalPluginsTab = new System.Windows.Forms.CheckBox();
            this.lblAdvLayerTabs = new System.Windows.Forms.Label();
            this.panel11 = new System.Windows.Forms.Panel();
            this.chkAdvEnableChatAdminName = new System.Windows.Forms.CheckBox();
            this.lblAdvChatTab = new System.Windows.Forms.Label();
            this.panel10 = new System.Windows.Forms.Panel();
            this.chkAdvEnableAdminMoveMsg = new System.Windows.Forms.CheckBox();
            this.lblAdvPlayerTab = new System.Windows.Forms.Label();
            this.panel9 = new System.Windows.Forms.Panel();
            this.panel12 = new System.Windows.Forms.Panel();
            this.tabAdv2 = new System.Windows.Forms.TabPage();
            this.chkAdv2EnableAdminReason = new System.Windows.Forms.CheckBox();
            this.lblAdv2BanTab = new System.Windows.Forms.Label();
            this.panel18 = new System.Windows.Forms.Panel();
            this.tabPlayerLookup = new System.Windows.Forms.TabPage();
            this.lblStatsLinkHelpText = new System.Windows.Forms.Label();
            this.pnlStatsLinkManage = new System.Windows.Forms.Panel();
            this.btnAddStatsLink = new System.Windows.Forms.Button();
            this.btnRemoveStatsLink = new System.Windows.Forms.Button();
            this.lsvStatsLinksList = new PRoCon.Controls.ControlsEx.ListViewNF();
            this.colStatsLinksName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatsLinkUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtStatsLinkName = new System.Windows.Forms.TextBox();
            this.txtStatsLinkUrl = new System.Windows.Forms.TextBox();
            this.lblStatsLinkUrl = new System.Windows.Forms.Label();
            this.lblStatsLinkName = new System.Windows.Forms.Label();
            this.lblStatsPlayerTab = new System.Windows.Forms.Label();
            this.panel15 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.tbcOptions.SuspendLayout();
            this.tabBasics.SuspendLayout();
            this.tabPlugins.SuspendLayout();
            this.pnlSandboxOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPluginMaxRuntimeSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPluginMaxRuntimeMin)).BeginInit();
            this.pnlPluginsAllowedDomains.SuspendLayout();
            this.tabHttpServer.SuspendLayout();
            this.pnlHttpServerSettings.SuspendLayout();
            this.pnlHttpServerTester.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHttpServerForwardedTestStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHttpServerServerStatus)).BeginInit();
            this.tabAdv.SuspendLayout();
            this.tabAdv2.SuspendLayout();
            this.tabPlayerLookup.SuspendLayout();
            this.pnlStatsLinkManage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbcOptions
            // 
            this.tbcOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbcOptions.Controls.Add(this.tabBasics);
            this.tbcOptions.Controls.Add(this.tabPlugins);
            this.tbcOptions.Controls.Add(this.tabHttpServer);
            this.tbcOptions.Controls.Add(this.tabAdv);
            this.tbcOptions.Controls.Add(this.tabAdv2);
            this.tbcOptions.Controls.Add(this.tabPlayerLookup);
            this.tbcOptions.Location = new System.Drawing.Point(14, 14);
            this.tbcOptions.Name = "tbcOptions";
            this.tbcOptions.SelectedIndex = 0;
            this.tbcOptions.Size = new System.Drawing.Size(398, 503);
            this.tbcOptions.TabIndex = 0;
            // 
            // tabBasics
            // 
            this.tabBasics.Controls.Add(this.rssFeedOptCheckbox);
            this.tabBasics.Controls.Add(this.localGeoIPCheckbox);
            this.tabBasics.Controls.Add(this.chkBasicsAutoCheckGameConfigsForUpdates);
            this.tabBasics.Controls.Add(this.chkBasicsEnablePluginLogging);
            this.tabBasics.Controls.Add(this.chkBasicsAutoApplyUpdates);
            this.tabBasics.Controls.Add(this.chkBasicsEnableEventsLogging);
            this.tabBasics.Controls.Add(this.chkBasicsMinimizeToTray);
            this.tabBasics.Controls.Add(this.chkBasicsCloseToTray);
            this.tabBasics.Controls.Add(this.cboBasicsShowWindow);
            this.tabBasics.Controls.Add(this.lblBasicsShowWindow);
            this.tabBasics.Controls.Add(this.lblBasicPreferences);
            this.tabBasics.Controls.Add(this.panel4);
            this.tabBasics.Controls.Add(this.chkBasicsAutoCheckDownloadForUpdates);
            this.tabBasics.Controls.Add(this.lblBasicsPrivacy);
            this.tabBasics.Controls.Add(this.panel3);
            this.tabBasics.Controls.Add(this.chkBasicsEnableConsoleLogging);
            this.tabBasics.Controls.Add(this.chkBasicsEnableChatLogging);
            this.tabBasics.Controls.Add(this.lblBasicsLogging);
            this.tabBasics.Controls.Add(this.panel1);
            this.tabBasics.Controls.Add(this.btnBasicsSetLanguage);
            this.tabBasics.Controls.Add(this.lblBasicsLanguage);
            this.tabBasics.Controls.Add(this.panel2);
            this.tabBasics.Controls.Add(this.lnkBasicsAuthor);
            this.tabBasics.Controls.Add(this.lblBasicsAuthor);
            this.tabBasics.Controls.Add(this.cboBasicsLanguagePicker);
            this.tabBasics.Location = new System.Drawing.Point(4, 24);
            this.tabBasics.Name = "tabBasics";
            this.tabBasics.Padding = new System.Windows.Forms.Padding(3);
            this.tabBasics.Size = new System.Drawing.Size(390, 475);
            this.tabBasics.TabIndex = 0;
            this.tabBasics.Text = "Basics";
            this.tabBasics.UseVisualStyleBackColor = true;
            // 
            // rssFeedOptCheckbox
            // 
            this.rssFeedOptCheckbox.AutoSize = true;
            this.rssFeedOptCheckbox.Location = new System.Drawing.Point(220, 172);
            this.rssFeedOptCheckbox.Name = "rssFeedOptCheckbox";
            this.rssFeedOptCheckbox.Size = new System.Drawing.Size(105, 19);
            this.rssFeedOptCheckbox.TabIndex = 30;
            this.rssFeedOptCheckbox.Text = "Show RSS Feed";
            this.rssFeedOptCheckbox.UseVisualStyleBackColor = true;
            this.rssFeedOptCheckbox.CheckedChanged += new System.EventHandler(this.rssFeedOptCheckbox_CheckedChanged);
            // 
            // localGeoIPCheckbox
            // 
            this.localGeoIPCheckbox.AutoSize = true;
            this.localGeoIPCheckbox.Location = new System.Drawing.Point(220, 147);
            this.localGeoIPCheckbox.Name = "localGeoIPCheckbox";
            this.localGeoIPCheckbox.Size = new System.Drawing.Size(137, 19);
            this.localGeoIPCheckbox.TabIndex = 29;
            this.localGeoIPCheckbox.Text = "Use Local GeoIP Data";
            this.localGeoIPCheckbox.UseVisualStyleBackColor = true;
            this.localGeoIPCheckbox.CheckedChanged += new System.EventHandler(this.localGeoIPCheckbox_CheckedChanged);
            // 
            // chkBasicsAutoCheckGameConfigsForUpdates
            // 
            this.chkBasicsAutoCheckGameConfigsForUpdates.AutoSize = true;
            this.chkBasicsAutoCheckGameConfigsForUpdates.Checked = true;
            this.chkBasicsAutoCheckGameConfigsForUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBasicsAutoCheckGameConfigsForUpdates.Location = new System.Drawing.Point(34, 326);
            this.chkBasicsAutoCheckGameConfigsForUpdates.Name = "chkBasicsAutoCheckGameConfigsForUpdates";
            this.chkBasicsAutoCheckGameConfigsForUpdates.Size = new System.Drawing.Size(304, 19);
            this.chkBasicsAutoCheckGameConfigsForUpdates.TabIndex = 28;
            this.chkBasicsAutoCheckGameConfigsForUpdates.Text = "Include check for new game configs in update check";
            this.chkBasicsAutoCheckGameConfigsForUpdates.UseVisualStyleBackColor = true;
            this.chkBasicsAutoCheckGameConfigsForUpdates.CheckedChanged += new System.EventHandler(this.chkBasicsAutoCheckGameConfigsForUpdates_CheckedChanged);
            // 
            // chkBasicsEnablePluginLogging
            // 
            this.chkBasicsEnablePluginLogging.AutoSize = true;
            this.chkBasicsEnablePluginLogging.Location = new System.Drawing.Point(34, 221);
            this.chkBasicsEnablePluginLogging.Name = "chkBasicsEnablePluginLogging";
            this.chkBasicsEnablePluginLogging.Size = new System.Drawing.Size(142, 19);
            this.chkBasicsEnablePluginLogging.TabIndex = 27;
            this.chkBasicsEnablePluginLogging.Text = "Enable plugin logging";
            this.chkBasicsEnablePluginLogging.UseVisualStyleBackColor = true;
            this.chkBasicsEnablePluginLogging.CheckedChanged += new System.EventHandler(this.chkBasicsEnablePluginLogging_CheckedChanged);
            // 
            // chkBasicsAutoApplyUpdates
            // 
            this.chkBasicsAutoApplyUpdates.AutoSize = true;
            this.chkBasicsAutoApplyUpdates.Location = new System.Drawing.Point(48, 301);
            this.chkBasicsAutoApplyUpdates.Name = "chkBasicsAutoApplyUpdates";
            this.chkBasicsAutoApplyUpdates.Size = new System.Drawing.Size(255, 19);
            this.chkBasicsAutoApplyUpdates.TabIndex = 26;
            this.chkBasicsAutoApplyUpdates.Text = "Shutdown and apply updates automatically";
            this.chkBasicsAutoApplyUpdates.UseVisualStyleBackColor = true;
            this.chkBasicsAutoApplyUpdates.CheckedChanged += new System.EventHandler(this.chkBasicsAutoApplyUpdates_CheckedChanged);
            // 
            // chkBasicsEnableEventsLogging
            // 
            this.chkBasicsEnableEventsLogging.AutoSize = true;
            this.chkBasicsEnableEventsLogging.Location = new System.Drawing.Point(34, 171);
            this.chkBasicsEnableEventsLogging.Name = "chkBasicsEnableEventsLogging";
            this.chkBasicsEnableEventsLogging.Size = new System.Drawing.Size(142, 19);
            this.chkBasicsEnableEventsLogging.TabIndex = 25;
            this.chkBasicsEnableEventsLogging.Text = "Enable events logging";
            this.chkBasicsEnableEventsLogging.UseVisualStyleBackColor = true;
            this.chkBasicsEnableEventsLogging.CheckedChanged += new System.EventHandler(this.chkBasicsEnableEventsLogging_CheckedChanged);
            // 
            // chkBasicsMinimizeToTray
            // 
            this.chkBasicsMinimizeToTray.AutoSize = true;
            this.chkBasicsMinimizeToTray.Location = new System.Drawing.Point(91, 453);
            this.chkBasicsMinimizeToTray.Name = "chkBasicsMinimizeToTray";
            this.chkBasicsMinimizeToTray.Size = new System.Drawing.Size(112, 19);
            this.chkBasicsMinimizeToTray.TabIndex = 24;
            this.chkBasicsMinimizeToTray.Text = "Minimize to tray";
            this.chkBasicsMinimizeToTray.UseVisualStyleBackColor = true;
            this.chkBasicsMinimizeToTray.CheckedChanged += new System.EventHandler(this.chkBasicsMinimizeToTray_CheckedChanged);
            // 
            // chkBasicsCloseToTray
            // 
            this.chkBasicsCloseToTray.AutoSize = true;
            this.chkBasicsCloseToTray.Location = new System.Drawing.Point(91, 429);
            this.chkBasicsCloseToTray.Name = "chkBasicsCloseToTray";
            this.chkBasicsCloseToTray.Size = new System.Drawing.Size(92, 19);
            this.chkBasicsCloseToTray.TabIndex = 23;
            this.chkBasicsCloseToTray.Text = "Close to tray";
            this.chkBasicsCloseToTray.UseVisualStyleBackColor = true;
            this.chkBasicsCloseToTray.CheckedChanged += new System.EventHandler(this.chkBasicsCloseToTray_CheckedChanged);
            // 
            // cboBasicsShowWindow
            // 
            this.cboBasicsShowWindow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBasicsShowWindow.FormattingEnabled = true;
            this.cboBasicsShowWindow.Items.AddRange(new object[] {
            "in taskbar and tray",
            "in taskbar only"});
            this.cboBasicsShowWindow.Location = new System.Drawing.Point(62, 399);
            this.cboBasicsShowWindow.Name = "cboBasicsShowWindow";
            this.cboBasicsShowWindow.Size = new System.Drawing.Size(313, 23);
            this.cboBasicsShowWindow.TabIndex = 22;
            this.cboBasicsShowWindow.SelectedIndexChanged += new System.EventHandler(this.cboBasicsShowWindow_SelectedIndexChanged);
            // 
            // lblBasicsShowWindow
            // 
            this.lblBasicsShowWindow.AutoSize = true;
            this.lblBasicsShowWindow.Location = new System.Drawing.Point(31, 381);
            this.lblBasicsShowWindow.Name = "lblBasicsShowWindow";
            this.lblBasicsShowWindow.Size = new System.Drawing.Size(84, 15);
            this.lblBasicsShowWindow.TabIndex = 21;
            this.lblBasicsShowWindow.Text = "Show window:";
            this.lblBasicsShowWindow.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblBasicPreferences
            // 
            this.lblBasicPreferences.AutoSize = true;
            this.lblBasicPreferences.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBasicPreferences.Location = new System.Drawing.Point(17, 351);
            this.lblBasicPreferences.Name = "lblBasicPreferences";
            this.lblBasicPreferences.Size = new System.Drawing.Size(75, 15);
            this.lblBasicPreferences.TabIndex = 19;
            this.lblBasicPreferences.Text = "Preferences";
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel4.Location = new System.Drawing.Point(20, 360);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(355, 1);
            this.panel4.TabIndex = 20;
            // 
            // chkBasicsAutoCheckDownloadForUpdates
            // 
            this.chkBasicsAutoCheckDownloadForUpdates.AutoSize = true;
            this.chkBasicsAutoCheckDownloadForUpdates.Checked = true;
            this.chkBasicsAutoCheckDownloadForUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBasicsAutoCheckDownloadForUpdates.Location = new System.Drawing.Point(34, 276);
            this.chkBasicsAutoCheckDownloadForUpdates.Name = "chkBasicsAutoCheckDownloadForUpdates";
            this.chkBasicsAutoCheckDownloadForUpdates.Size = new System.Drawing.Size(200, 19);
            this.chkBasicsAutoCheckDownloadForUpdates.TabIndex = 18;
            this.chkBasicsAutoCheckDownloadForUpdates.Text = "Download updates automatically";
            this.chkBasicsAutoCheckDownloadForUpdates.UseVisualStyleBackColor = true;
            this.chkBasicsAutoCheckDownloadForUpdates.CheckedChanged += new System.EventHandler(this.chkBasicsAutoCheckDownloadForUpdates_CheckedChanged);
            // 
            // lblBasicsPrivacy
            // 
            this.lblBasicsPrivacy.AutoSize = true;
            this.lblBasicsPrivacy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBasicsPrivacy.Location = new System.Drawing.Point(17, 248);
            this.lblBasicsPrivacy.Name = "lblBasicsPrivacy";
            this.lblBasicsPrivacy.Size = new System.Drawing.Size(47, 15);
            this.lblBasicsPrivacy.TabIndex = 16;
            this.lblBasicsPrivacy.Text = "Privacy";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel3.Location = new System.Drawing.Point(20, 257);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(355, 1);
            this.panel3.TabIndex = 17;
            // 
            // chkBasicsEnableConsoleLogging
            // 
            this.chkBasicsEnableConsoleLogging.AutoSize = true;
            this.chkBasicsEnableConsoleLogging.Location = new System.Drawing.Point(34, 196);
            this.chkBasicsEnableConsoleLogging.Name = "chkBasicsEnableConsoleLogging";
            this.chkBasicsEnableConsoleLogging.Size = new System.Drawing.Size(149, 19);
            this.chkBasicsEnableConsoleLogging.TabIndex = 15;
            this.chkBasicsEnableConsoleLogging.Text = "Enable console logging";
            this.chkBasicsEnableConsoleLogging.UseVisualStyleBackColor = true;
            this.chkBasicsEnableConsoleLogging.CheckedChanged += new System.EventHandler(this.chkBasicsEnableConsoleLogging_CheckedChanged);
            // 
            // chkBasicsEnableChatLogging
            // 
            this.chkBasicsEnableChatLogging.AutoSize = true;
            this.chkBasicsEnableChatLogging.Location = new System.Drawing.Point(34, 147);
            this.chkBasicsEnableChatLogging.Name = "chkBasicsEnableChatLogging";
            this.chkBasicsEnableChatLogging.Size = new System.Drawing.Size(131, 19);
            this.chkBasicsEnableChatLogging.TabIndex = 14;
            this.chkBasicsEnableChatLogging.Text = "Enable chat logging";
            this.chkBasicsEnableChatLogging.UseVisualStyleBackColor = true;
            this.chkBasicsEnableChatLogging.CheckedChanged += new System.EventHandler(this.chkBasicsEnableChatLogging_CheckedChanged);
            // 
            // lblBasicsLogging
            // 
            this.lblBasicsLogging.AutoSize = true;
            this.lblBasicsLogging.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBasicsLogging.Location = new System.Drawing.Point(17, 119);
            this.lblBasicsLogging.Name = "lblBasicsLogging";
            this.lblBasicsLogging.Size = new System.Drawing.Size(51, 15);
            this.lblBasicsLogging.TabIndex = 12;
            this.lblBasicsLogging.Text = "Logging";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Location = new System.Drawing.Point(20, 128);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(355, 1);
            this.panel1.TabIndex = 13;
            // 
            // btnBasicsSetLanguage
            // 
            this.btnBasicsSetLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBasicsSetLanguage.Location = new System.Drawing.Point(261, 46);
            this.btnBasicsSetLanguage.Name = "btnBasicsSetLanguage";
            this.btnBasicsSetLanguage.Size = new System.Drawing.Size(114, 23);
            this.btnBasicsSetLanguage.TabIndex = 11;
            this.btnBasicsSetLanguage.Text = "Set Language";
            this.btnBasicsSetLanguage.UseVisualStyleBackColor = true;
            this.btnBasicsSetLanguage.Click += new System.EventHandler(this.btnSetLanguage_Click);
            // 
            // lblBasicsLanguage
            // 
            this.lblBasicsLanguage.AutoSize = true;
            this.lblBasicsLanguage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBasicsLanguage.Location = new System.Drawing.Point(17, 24);
            this.lblBasicsLanguage.Name = "lblBasicsLanguage";
            this.lblBasicsLanguage.Size = new System.Drawing.Size(60, 15);
            this.lblBasicsLanguage.TabIndex = 0;
            this.lblBasicsLanguage.Text = "Language";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel2.Location = new System.Drawing.Point(20, 33);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(355, 1);
            this.panel2.TabIndex = 10;
            // 
            // lnkBasicsAuthor
            // 
            this.lnkBasicsAuthor.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkBasicsAuthor.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkBasicsAuthor.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkBasicsAuthor.Location = new System.Drawing.Point(31, 92);
            this.lnkBasicsAuthor.Name = "lnkBasicsAuthor";
            this.lnkBasicsAuthor.Size = new System.Drawing.Size(303, 20);
            this.lnkBasicsAuthor.TabIndex = 6;
            this.lnkBasicsAuthor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBasicsAuthor_LinkClicked);
            // 
            // lblBasicsAuthor
            // 
            this.lblBasicsAuthor.AutoSize = true;
            this.lblBasicsAuthor.Location = new System.Drawing.Point(31, 72);
            this.lblBasicsAuthor.Name = "lblBasicsAuthor";
            this.lblBasicsAuthor.Size = new System.Drawing.Size(76, 15);
            this.lblBasicsAuthor.TabIndex = 2;
            this.lblBasicsAuthor.Text = "Translated by";
            // 
            // cboBasicsLanguagePicker
            // 
            this.cboBasicsLanguagePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboBasicsLanguagePicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBasicsLanguagePicker.FormattingEnabled = true;
            this.cboBasicsLanguagePicker.Location = new System.Drawing.Point(31, 46);
            this.cboBasicsLanguagePicker.Name = "cboBasicsLanguagePicker";
            this.cboBasicsLanguagePicker.Size = new System.Drawing.Size(223, 23);
            this.cboBasicsLanguagePicker.TabIndex = 1;
            this.cboBasicsLanguagePicker.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboBasicsLanguagePicker_DrawItem);
            this.cboBasicsLanguagePicker.SelectedIndexChanged += new System.EventHandler(this.cboBasicsLanguagePicker_SelectedIndexChanged);
            // 
            // tabPlugins
            // 
            this.tabPlugins.Controls.Add(this.chkEnablePluginDebugging);
            this.tabPlugins.Controls.Add(this.lblPluginsDebug);
            this.tabPlugins.Controls.Add(this.lblPluginsChangesAfterRestart);
            this.tabPlugins.Controls.Add(this.panel19);
            this.tabPlugins.Controls.Add(this.pnlSandboxOptions);
            this.tabPlugins.Controls.Add(this.lblPluginsSecurity);
            this.tabPlugins.Controls.Add(this.panel6);
            this.tabPlugins.Controls.Add(this.cboPluginsSandboxOptions);
            this.tabPlugins.Location = new System.Drawing.Point(4, 24);
            this.tabPlugins.Name = "tabPlugins";
            this.tabPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlugins.Size = new System.Drawing.Size(390, 475);
            this.tabPlugins.TabIndex = 1;
            this.tabPlugins.Text = "Plugins";
            this.tabPlugins.UseVisualStyleBackColor = true;
            // 
            // chkEnablePluginDebugging
            // 
            this.chkEnablePluginDebugging.AutoSize = true;
            this.chkEnablePluginDebugging.Location = new System.Drawing.Point(32, 408);
            this.chkEnablePluginDebugging.Name = "chkEnablePluginDebugging";
            this.chkEnablePluginDebugging.Size = new System.Drawing.Size(159, 19);
            this.chkEnablePluginDebugging.TabIndex = 287;
            this.chkEnablePluginDebugging.Text = "Enable plugin debugging";
            this.chkEnablePluginDebugging.UseVisualStyleBackColor = true;
            this.chkEnablePluginDebugging.CheckedChanged += new System.EventHandler(this.chkEnablePluginDebugging_CheckedChanged);
            // 
            // lblPluginsDebug
            // 
            this.lblPluginsDebug.AutoSize = true;
            this.lblPluginsDebug.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginsDebug.Location = new System.Drawing.Point(18, 387);
            this.lblPluginsDebug.Name = "lblPluginsDebug";
            this.lblPluginsDebug.Size = new System.Drawing.Size(68, 15);
            this.lblPluginsDebug.TabIndex = 285;
            this.lblPluginsDebug.Text = "Debugging";
            // 
            // lblPluginsChangesAfterRestart
            // 
            this.lblPluginsChangesAfterRestart.Location = new System.Drawing.Point(17, 434);
            this.lblPluginsChangesAfterRestart.Name = "lblPluginsChangesAfterRestart";
            this.lblPluginsChangesAfterRestart.Size = new System.Drawing.Size(355, 38);
            this.lblPluginsChangesAfterRestart.TabIndex = 25;
            this.lblPluginsChangesAfterRestart.Text = "Changes to plugin security require PRoCon to be restarted before they come into e" +
    "ffect";
            // 
            // panel19
            // 
            this.panel19.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel19.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel19.Location = new System.Drawing.Point(21, 396);
            this.panel19.Name = "panel19";
            this.panel19.Size = new System.Drawing.Size(355, 1);
            this.panel19.TabIndex = 286;
            // 
            // pnlSandboxOptions
            // 
            this.pnlSandboxOptions.Controls.Add(this.numPluginMaxRuntimeSec);
            this.pnlSandboxOptions.Controls.Add(this.numPluginMaxRuntimeMin);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginMaxRuntimeSec);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginMaxRuntimeMin);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginMaxRuntime);
            this.pnlSandboxOptions.Controls.Add(this.panel16);
            this.pnlSandboxOptions.Controls.Add(this.chkAllowSmtpConnections);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginsMail);
            this.pnlSandboxOptions.Controls.Add(this.panel13);
            this.pnlSandboxOptions.Controls.Add(this.chkAllowODBCConnections);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginsDatabases);
            this.pnlSandboxOptions.Controls.Add(this.panel8);
            this.pnlSandboxOptions.Controls.Add(this.pnlPluginsAllowedDomains);
            this.pnlSandboxOptions.Controls.Add(this.lblPluginsOutgoingConnections);
            this.pnlSandboxOptions.Controls.Add(this.panel5);
            this.pnlSandboxOptions.Location = new System.Drawing.Point(3, 60);
            this.pnlSandboxOptions.Name = "pnlSandboxOptions";
            this.pnlSandboxOptions.Size = new System.Drawing.Size(381, 324);
            this.pnlSandboxOptions.TabIndex = 24;
            // 
            // numPluginMaxRuntimeSec
            // 
            this.numPluginMaxRuntimeSec.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numPluginMaxRuntimeSec.Location = new System.Drawing.Point(184, 294);
            this.numPluginMaxRuntimeSec.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numPluginMaxRuntimeSec.Name = "numPluginMaxRuntimeSec";
            this.numPluginMaxRuntimeSec.Size = new System.Drawing.Size(44, 23);
            this.numPluginMaxRuntimeSec.TabIndex = 284;
            this.numPluginMaxRuntimeSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPluginMaxRuntimeSec.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numPluginMaxRuntimeSec.Validated += new System.EventHandler(this.numPluginMaxRuntimeSec_Validated);
            // 
            // numPluginMaxRuntimeMin
            // 
            this.numPluginMaxRuntimeMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numPluginMaxRuntimeMin.Location = new System.Drawing.Point(101, 294);
            this.numPluginMaxRuntimeMin.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numPluginMaxRuntimeMin.Name = "numPluginMaxRuntimeMin";
            this.numPluginMaxRuntimeMin.Size = new System.Drawing.Size(44, 23);
            this.numPluginMaxRuntimeMin.TabIndex = 283;
            this.numPluginMaxRuntimeMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPluginMaxRuntimeMin.Validated += new System.EventHandler(this.numPluginMaxRuntimeMin_Validated);
            // 
            // lblPluginMaxRuntimeSec
            // 
            this.lblPluginMaxRuntimeSec.AutoSize = true;
            this.lblPluginMaxRuntimeSec.Location = new System.Drawing.Point(234, 296);
            this.lblPluginMaxRuntimeSec.Name = "lblPluginMaxRuntimeSec";
            this.lblPluginMaxRuntimeSec.Size = new System.Drawing.Size(24, 15);
            this.lblPluginMaxRuntimeSec.TabIndex = 33;
            this.lblPluginMaxRuntimeSec.Text = "sec";
            // 
            // lblPluginMaxRuntimeMin
            // 
            this.lblPluginMaxRuntimeMin.AutoSize = true;
            this.lblPluginMaxRuntimeMin.Location = new System.Drawing.Point(151, 296);
            this.lblPluginMaxRuntimeMin.Name = "lblPluginMaxRuntimeMin";
            this.lblPluginMaxRuntimeMin.Size = new System.Drawing.Size(28, 15);
            this.lblPluginMaxRuntimeMin.TabIndex = 27;
            this.lblPluginMaxRuntimeMin.Text = "min";
            // 
            // lblPluginMaxRuntime
            // 
            this.lblPluginMaxRuntime.AutoSize = true;
            this.lblPluginMaxRuntime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginMaxRuntime.Location = new System.Drawing.Point(15, 275);
            this.lblPluginMaxRuntime.Name = "lblPluginMaxRuntime";
            this.lblPluginMaxRuntime.Size = new System.Drawing.Size(184, 15);
            this.lblPluginMaxRuntime.TabIndex = 30;
            this.lblPluginMaxRuntime.Text = "Plugin runtime limit (per plugin)";
            // 
            // panel16
            // 
            this.panel16.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel16.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel16.Location = new System.Drawing.Point(18, 284);
            this.panel16.Name = "panel16";
            this.panel16.Size = new System.Drawing.Size(355, 1);
            this.panel16.TabIndex = 31;
            // 
            // chkAllowSmtpConnections
            // 
            this.chkAllowSmtpConnections.AutoSize = true;
            this.chkAllowSmtpConnections.Location = new System.Drawing.Point(29, 250);
            this.chkAllowSmtpConnections.Name = "chkAllowSmtpConnections";
            this.chkAllowSmtpConnections.Size = new System.Drawing.Size(224, 19);
            this.chkAllowSmtpConnections.TabIndex = 29;
            this.chkAllowSmtpConnections.Text = "Allow all outgoing SMTP connections";
            this.chkAllowSmtpConnections.UseVisualStyleBackColor = true;
            this.chkAllowSmtpConnections.CheckedChanged += new System.EventHandler(this.chkAllowSmtpConnections_CheckedChanged);
            // 
            // lblPluginsMail
            // 
            this.lblPluginsMail.AutoSize = true;
            this.lblPluginsMail.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginsMail.Location = new System.Drawing.Point(15, 229);
            this.lblPluginsMail.Name = "lblPluginsMail";
            this.lblPluginsMail.Size = new System.Drawing.Size(30, 15);
            this.lblPluginsMail.TabIndex = 27;
            this.lblPluginsMail.Text = "Mail";
            // 
            // panel13
            // 
            this.panel13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel13.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel13.Location = new System.Drawing.Point(18, 238);
            this.panel13.Name = "panel13";
            this.panel13.Size = new System.Drawing.Size(355, 1);
            this.panel13.TabIndex = 28;
            // 
            // chkAllowODBCConnections
            // 
            this.chkAllowODBCConnections.AutoSize = true;
            this.chkAllowODBCConnections.Location = new System.Drawing.Point(29, 206);
            this.chkAllowODBCConnections.Name = "chkAllowODBCConnections";
            this.chkAllowODBCConnections.Size = new System.Drawing.Size(226, 19);
            this.chkAllowODBCConnections.TabIndex = 26;
            this.chkAllowODBCConnections.Text = "Allow all outgoing ODBC connections";
            this.chkAllowODBCConnections.UseVisualStyleBackColor = true;
            this.chkAllowODBCConnections.CheckedChanged += new System.EventHandler(this.chkAllowODBCConnections_CheckedChanged);
            // 
            // lblPluginsDatabases
            // 
            this.lblPluginsDatabases.AutoSize = true;
            this.lblPluginsDatabases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginsDatabases.Location = new System.Drawing.Point(15, 185);
            this.lblPluginsDatabases.Name = "lblPluginsDatabases";
            this.lblPluginsDatabases.Size = new System.Drawing.Size(63, 15);
            this.lblPluginsDatabases.TabIndex = 24;
            this.lblPluginsDatabases.Text = "Databases";
            // 
            // panel8
            // 
            this.panel8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel8.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel8.Location = new System.Drawing.Point(18, 194);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(355, 1);
            this.panel8.TabIndex = 25;
            // 
            // pnlPluginsAllowedDomains
            // 
            this.pnlPluginsAllowedDomains.Controls.Add(this.lsvTrustedHostDomainPorts);
            this.pnlPluginsAllowedDomains.Controls.Add(this.btnPluginsAddTrustedHostDomain);
            this.pnlPluginsAllowedDomains.Controls.Add(this.btnPluginsRemoveTrustedHostDomain);
            this.pnlPluginsAllowedDomains.Controls.Add(this.txtPluginsTrustedPort);
            this.pnlPluginsAllowedDomains.Controls.Add(this.lblPluginsPort);
            this.pnlPluginsAllowedDomains.Controls.Add(this.txtPluginsTrustedHostDomain);
            this.pnlPluginsAllowedDomains.Controls.Add(this.lblPluginsTrustedHostDomain);
            this.pnlPluginsAllowedDomains.Location = new System.Drawing.Point(3, 31);
            this.pnlPluginsAllowedDomains.Name = "pnlPluginsAllowedDomains";
            this.pnlPluginsAllowedDomains.Size = new System.Drawing.Size(375, 149);
            this.pnlPluginsAllowedDomains.TabIndex = 23;
            // 
            // lsvTrustedHostDomainPorts
            // 
            this.lsvTrustedHostDomainPorts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTrustedDomainsHost,
            this.colTrustedPort});
            this.lsvTrustedHostDomainPorts.FullRowSelect = true;
            this.lsvTrustedHostDomainPorts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lsvTrustedHostDomainPorts.HideSelection = false;
            this.lsvTrustedHostDomainPorts.Location = new System.Drawing.Point(25, 51);
            this.lsvTrustedHostDomainPorts.MultiSelect = false;
            this.lsvTrustedHostDomainPorts.Name = "lsvTrustedHostDomainPorts";
            this.lsvTrustedHostDomainPorts.Size = new System.Drawing.Size(304, 96);
            this.lsvTrustedHostDomainPorts.TabIndex = 26;
            this.lsvTrustedHostDomainPorts.UseCompatibleStateImageBehavior = false;
            this.lsvTrustedHostDomainPorts.View = System.Windows.Forms.View.Details;
            this.lsvTrustedHostDomainPorts.SelectedIndexChanged += new System.EventHandler(this.lsvTrustedHostDomainPorts_SelectedIndexChanged);
            // 
            // colTrustedDomainsHost
            // 
            this.colTrustedDomainsHost.Text = "Trusted host/domain";
            this.colTrustedDomainsHost.Width = 204;
            // 
            // colTrustedPort
            // 
            this.colTrustedPort.Text = "Port";
            // 
            // btnPluginsAddTrustedHostDomain
            // 
            this.btnPluginsAddTrustedHostDomain.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPluginsAddTrustedHostDomain.Enabled = false;
            this.btnPluginsAddTrustedHostDomain.FlatAppearance.BorderSize = 0;
            this.btnPluginsAddTrustedHostDomain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPluginsAddTrustedHostDomain.Location = new System.Drawing.Point(334, 1);
            this.btnPluginsAddTrustedHostDomain.Name = "btnPluginsAddTrustedHostDomain";
            this.btnPluginsAddTrustedHostDomain.Size = new System.Drawing.Size(35, 23);
            this.btnPluginsAddTrustedHostDomain.TabIndex = 25;
            this.btnPluginsAddTrustedHostDomain.UseVisualStyleBackColor = true;
            this.btnPluginsAddTrustedHostDomain.Click += new System.EventHandler(this.btnPluginsAddTrustedHostDomain_Click);
            // 
            // btnPluginsRemoveTrustedHostDomain
            // 
            this.btnPluginsRemoveTrustedHostDomain.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPluginsRemoveTrustedHostDomain.Enabled = false;
            this.btnPluginsRemoveTrustedHostDomain.FlatAppearance.BorderSize = 0;
            this.btnPluginsRemoveTrustedHostDomain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPluginsRemoveTrustedHostDomain.Location = new System.Drawing.Point(335, 80);
            this.btnPluginsRemoveTrustedHostDomain.Name = "btnPluginsRemoveTrustedHostDomain";
            this.btnPluginsRemoveTrustedHostDomain.Size = new System.Drawing.Size(35, 23);
            this.btnPluginsRemoveTrustedHostDomain.TabIndex = 24;
            this.btnPluginsRemoveTrustedHostDomain.UseVisualStyleBackColor = true;
            this.btnPluginsRemoveTrustedHostDomain.Click += new System.EventHandler(this.btnPluginsRemoveTrustedHostDomain_Click);
            // 
            // txtPluginsTrustedPort
            // 
            this.txtPluginsTrustedPort.Location = new System.Drawing.Point(263, 22);
            this.txtPluginsTrustedPort.Name = "txtPluginsTrustedPort";
            this.txtPluginsTrustedPort.Size = new System.Drawing.Size(66, 23);
            this.txtPluginsTrustedPort.TabIndex = 23;
            this.txtPluginsTrustedPort.TextChanged += new System.EventHandler(this.txtPluginsTrustedPort_TextChanged);
            this.txtPluginsTrustedPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPluginsTrustedPort_KeyPress);
            // 
            // lblPluginsPort
            // 
            this.lblPluginsPort.AutoSize = true;
            this.lblPluginsPort.Location = new System.Drawing.Point(260, 4);
            this.lblPluginsPort.Name = "lblPluginsPort";
            this.lblPluginsPort.Size = new System.Drawing.Size(29, 15);
            this.lblPluginsPort.TabIndex = 22;
            this.lblPluginsPort.Text = "Port";
            // 
            // txtPluginsTrustedHostDomain
            // 
            this.txtPluginsTrustedHostDomain.Location = new System.Drawing.Point(25, 22);
            this.txtPluginsTrustedHostDomain.Name = "txtPluginsTrustedHostDomain";
            this.txtPluginsTrustedHostDomain.Size = new System.Drawing.Size(232, 23);
            this.txtPluginsTrustedHostDomain.TabIndex = 21;
            this.txtPluginsTrustedHostDomain.TextChanged += new System.EventHandler(this.txtPluginsTrustedHostDomain_TextChanged);
            // 
            // lblPluginsTrustedHostDomain
            // 
            this.lblPluginsTrustedHostDomain.AutoSize = true;
            this.lblPluginsTrustedHostDomain.Location = new System.Drawing.Point(22, 4);
            this.lblPluginsTrustedHostDomain.Name = "lblPluginsTrustedHostDomain";
            this.lblPluginsTrustedHostDomain.Size = new System.Drawing.Size(117, 15);
            this.lblPluginsTrustedHostDomain.TabIndex = 20;
            this.lblPluginsTrustedHostDomain.Text = "Trusted host/domain";
            // 
            // lblPluginsOutgoingConnections
            // 
            this.lblPluginsOutgoingConnections.AutoSize = true;
            this.lblPluginsOutgoingConnections.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginsOutgoingConnections.Location = new System.Drawing.Point(14, 13);
            this.lblPluginsOutgoingConnections.Name = "lblPluginsOutgoingConnections";
            this.lblPluginsOutgoingConnections.Size = new System.Drawing.Size(129, 15);
            this.lblPluginsOutgoingConnections.TabIndex = 20;
            this.lblPluginsOutgoingConnections.Text = "Outgoing connections";
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel5.Location = new System.Drawing.Point(17, 22);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(355, 1);
            this.panel5.TabIndex = 21;
            // 
            // lblPluginsSecurity
            // 
            this.lblPluginsSecurity.AutoSize = true;
            this.lblPluginsSecurity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPluginsSecurity.Location = new System.Drawing.Point(17, 9);
            this.lblPluginsSecurity.Name = "lblPluginsSecurity";
            this.lblPluginsSecurity.Size = new System.Drawing.Size(90, 15);
            this.lblPluginsSecurity.TabIndex = 20;
            this.lblPluginsSecurity.Text = "Plugin Security";
            // 
            // panel6
            // 
            this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel6.Location = new System.Drawing.Point(20, 18);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(355, 1);
            this.panel6.TabIndex = 21;
            // 
            // cboPluginsSandboxOptions
            // 
            this.cboPluginsSandboxOptions.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboPluginsSandboxOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPluginsSandboxOptions.FormattingEnabled = true;
            this.cboPluginsSandboxOptions.Items.AddRange(new object[] {
            "Run plugins in a sandbox (recommended)",
            "Run plugins with no restrictions"});
            this.cboPluginsSandboxOptions.Location = new System.Drawing.Point(31, 31);
            this.cboPluginsSandboxOptions.Name = "cboPluginsSandboxOptions";
            this.cboPluginsSandboxOptions.Size = new System.Drawing.Size(304, 24);
            this.cboPluginsSandboxOptions.TabIndex = 23;
            this.cboPluginsSandboxOptions.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboSandboxOptions_DrawItem);
            this.cboPluginsSandboxOptions.SelectedIndexChanged += new System.EventHandler(this.cboSandboxOptions_SelectedIndexChanged);
            // 
            // tabHttpServer
            // 
            this.tabHttpServer.Controls.Add(this.pnlHttpServerSettings);
            this.tabHttpServer.Controls.Add(this.pnlHttpServerTester);
            this.tabHttpServer.Controls.Add(this.lnkStartStopHttpServer);
            this.tabHttpServer.Controls.Add(this.picHttpServerServerStatus);
            this.tabHttpServer.Controls.Add(this.lblHttpServerServerStatus);
            this.tabHttpServer.Controls.Add(this.lblHttpServerTitle);
            this.tabHttpServer.Controls.Add(this.panel7);
            this.tabHttpServer.Location = new System.Drawing.Point(4, 24);
            this.tabHttpServer.Name = "tabHttpServer";
            this.tabHttpServer.Padding = new System.Windows.Forms.Padding(3);
            this.tabHttpServer.Size = new System.Drawing.Size(390, 475);
            this.tabHttpServer.TabIndex = 2;
            this.tabHttpServer.Text = "HTTP";
            this.tabHttpServer.UseVisualStyleBackColor = true;
            // 
            // pnlHttpServerSettings
            // 
            this.pnlHttpServerSettings.Controls.Add(this.lblBindingExplanation);
            this.pnlHttpServerSettings.Controls.Add(this.txtHttpServerBindingAddress);
            this.pnlHttpServerSettings.Controls.Add(this.lblHttpServerBindingIP);
            this.pnlHttpServerSettings.Controls.Add(this.lblHttpServerStartPort);
            this.pnlHttpServerSettings.Controls.Add(this.txtHttpServerStartPort);
            this.pnlHttpServerSettings.Location = new System.Drawing.Point(18, 42);
            this.pnlHttpServerSettings.Name = "pnlHttpServerSettings";
            this.pnlHttpServerSettings.Size = new System.Drawing.Size(357, 120);
            this.pnlHttpServerSettings.TabIndex = 32;
            // 
            // lblBindingExplanation
            // 
            this.lblBindingExplanation.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblBindingExplanation.Location = new System.Drawing.Point(156, 28);
            this.lblBindingExplanation.Name = "lblBindingExplanation";
            this.lblBindingExplanation.Size = new System.Drawing.Size(198, 83);
            this.lblBindingExplanation.TabIndex = 37;
            this.lblBindingExplanation.Text = "Default \"0.0.0.0\" to bind to any address";
            // 
            // txtHttpServerBindingAddress
            // 
            this.txtHttpServerBindingAddress.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtHttpServerBindingAddress.Location = new System.Drawing.Point(13, 28);
            this.txtHttpServerBindingAddress.Name = "txtHttpServerBindingAddress";
            this.txtHttpServerBindingAddress.Size = new System.Drawing.Size(127, 23);
            this.txtHttpServerBindingAddress.TabIndex = 36;
            this.txtHttpServerBindingAddress.Text = "0.0.0.0";
            // 
            // lblHttpServerBindingIP
            // 
            this.lblHttpServerBindingIP.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblHttpServerBindingIP.AutoSize = true;
            this.lblHttpServerBindingIP.Location = new System.Drawing.Point(10, 7);
            this.lblHttpServerBindingIP.Name = "lblHttpServerBindingIP";
            this.lblHttpServerBindingIP.Size = new System.Drawing.Size(91, 15);
            this.lblHttpServerBindingIP.TabIndex = 35;
            this.lblHttpServerBindingIP.Text = "Binding address";
            // 
            // lblHttpServerStartPort
            // 
            this.lblHttpServerStartPort.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblHttpServerStartPort.AutoSize = true;
            this.lblHttpServerStartPort.Location = new System.Drawing.Point(10, 67);
            this.lblHttpServerStartPort.Name = "lblHttpServerStartPort";
            this.lblHttpServerStartPort.Size = new System.Drawing.Size(80, 15);
            this.lblHttpServerStartPort.TabIndex = 34;
            this.lblHttpServerStartPort.Text = "Listening port";
            // 
            // txtHttpServerStartPort
            // 
            this.txtHttpServerStartPort.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtHttpServerStartPort.Location = new System.Drawing.Point(13, 88);
            this.txtHttpServerStartPort.Name = "txtHttpServerStartPort";
            this.txtHttpServerStartPort.Size = new System.Drawing.Size(66, 23);
            this.txtHttpServerStartPort.TabIndex = 33;
            this.txtHttpServerStartPort.Text = "27360";
            // 
            // pnlHttpServerTester
            // 
            this.pnlHttpServerTester.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pnlHttpServerTester.Controls.Add(this.lnkHttpServerExampleLink);
            this.pnlHttpServerTester.Controls.Add(this.picHttpServerForwardedTestStatus);
            this.pnlHttpServerTester.Controls.Add(this.lblHttpServerForwardedTestStatus);
            this.pnlHttpServerTester.Controls.Add(this.lnkHttpServerForwardedTest);
            this.pnlHttpServerTester.Location = new System.Drawing.Point(18, 218);
            this.pnlHttpServerTester.Name = "pnlHttpServerTester";
            this.pnlHttpServerTester.Size = new System.Drawing.Size(513, 109);
            this.pnlHttpServerTester.TabIndex = 31;
            this.pnlHttpServerTester.Visible = false;
            // 
            // lnkHttpServerExampleLink
            // 
            this.lnkHttpServerExampleLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerExampleLink.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lnkHttpServerExampleLink.AutoSize = true;
            this.lnkHttpServerExampleLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkHttpServerExampleLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerExampleLink.Location = new System.Drawing.Point(57, 66);
            this.lnkHttpServerExampleLink.Name = "lnkHttpServerExampleLink";
            this.lnkHttpServerExampleLink.Size = new System.Drawing.Size(52, 15);
            this.lnkHttpServerExampleLink.TabIndex = 13;
            this.lnkHttpServerExampleLink.TabStop = true;
            this.lnkHttpServerExampleLink.Text = "Example";
            this.lnkHttpServerExampleLink.Visible = false;
            this.lnkHttpServerExampleLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerExampleLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHttpServerExampleLink_LinkClicked);
            // 
            // picHttpServerForwardedTestStatus
            // 
            this.picHttpServerForwardedTestStatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picHttpServerForwardedTestStatus.Location = new System.Drawing.Point(13, 8);
            this.picHttpServerForwardedTestStatus.Name = "picHttpServerForwardedTestStatus";
            this.picHttpServerForwardedTestStatus.Size = new System.Drawing.Size(32, 32);
            this.picHttpServerForwardedTestStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picHttpServerForwardedTestStatus.TabIndex = 9;
            this.picHttpServerForwardedTestStatus.TabStop = false;
            // 
            // lblHttpServerForwardedTestStatus
            // 
            this.lblHttpServerForwardedTestStatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblHttpServerForwardedTestStatus.ForeColor = System.Drawing.Color.Maroon;
            this.lblHttpServerForwardedTestStatus.Location = new System.Drawing.Point(57, 28);
            this.lblHttpServerForwardedTestStatus.Name = "lblHttpServerForwardedTestStatus";
            this.lblHttpServerForwardedTestStatus.Size = new System.Drawing.Size(300, 38);
            this.lblHttpServerForwardedTestStatus.TabIndex = 12;
            this.lblHttpServerForwardedTestStatus.Text = "Port 5555 is closed to incomming connections";
            // 
            // lnkHttpServerForwardedTest
            // 
            this.lnkHttpServerForwardedTest.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerForwardedTest.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lnkHttpServerForwardedTest.AutoSize = true;
            this.lnkHttpServerForwardedTest.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkHttpServerForwardedTest.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerForwardedTest.Location = new System.Drawing.Point(57, 8);
            this.lnkHttpServerForwardedTest.Name = "lnkHttpServerForwardedTest";
            this.lnkHttpServerForwardedTest.Size = new System.Drawing.Size(169, 15);
            this.lnkHttpServerForwardedTest.TabIndex = 2;
            this.lnkHttpServerForwardedTest.TabStop = true;
            this.lnkHttpServerForwardedTest.Text = "Test connection to HTTP server";
            this.lnkHttpServerForwardedTest.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkHttpServerForwardedTest.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHttpServerForwardedTest_LinkClicked);
            // 
            // lnkStartStopHttpServer
            // 
            this.lnkStartStopHttpServer.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkStartStopHttpServer.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lnkStartStopHttpServer.AutoSize = true;
            this.lnkStartStopHttpServer.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkStartStopHttpServer.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(220)))));
            this.lnkStartStopHttpServer.Location = new System.Drawing.Point(75, 168);
            this.lnkStartStopHttpServer.Name = "lnkStartStopHttpServer";
            this.lnkStartStopHttpServer.Size = new System.Drawing.Size(113, 15);
            this.lnkStartStopHttpServer.TabIndex = 28;
            this.lnkStartStopHttpServer.TabStop = true;
            this.lnkStartStopHttpServer.Text = "Turn HTTP server on";
            this.lnkStartStopHttpServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkStartStopHttpServer_LinkClicked);
            // 
            // picHttpServerServerStatus
            // 
            this.picHttpServerServerStatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picHttpServerServerStatus.Enabled = false;
            this.picHttpServerServerStatus.Location = new System.Drawing.Point(31, 168);
            this.picHttpServerServerStatus.Name = "picHttpServerServerStatus";
            this.picHttpServerServerStatus.Size = new System.Drawing.Size(32, 32);
            this.picHttpServerServerStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picHttpServerServerStatus.TabIndex = 30;
            this.picHttpServerServerStatus.TabStop = false;
            // 
            // lblHttpServerServerStatus
            // 
            this.lblHttpServerServerStatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblHttpServerServerStatus.ForeColor = System.Drawing.Color.Maroon;
            this.lblHttpServerServerStatus.Location = new System.Drawing.Point(75, 187);
            this.lblHttpServerServerStatus.Name = "lblHttpServerServerStatus";
            this.lblHttpServerServerStatus.Size = new System.Drawing.Size(300, 69);
            this.lblHttpServerServerStatus.TabIndex = 29;
            this.lblHttpServerServerStatus.Text = "Server is offline";
            // 
            // lblHttpServerTitle
            // 
            this.lblHttpServerTitle.AutoSize = true;
            this.lblHttpServerTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHttpServerTitle.Location = new System.Drawing.Point(17, 24);
            this.lblHttpServerTitle.Name = "lblHttpServerTitle";
            this.lblHttpServerTitle.Size = new System.Drawing.Size(76, 15);
            this.lblHttpServerTitle.TabIndex = 11;
            this.lblHttpServerTitle.Text = "HTTP server";
            // 
            // panel7
            // 
            this.panel7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel7.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel7.Location = new System.Drawing.Point(20, 33);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(355, 1);
            this.panel7.TabIndex = 12;
            // 
            // tabAdv
            // 
            this.tabAdv.Controls.Add(this.lblAdvStartupChangeNotice);
            this.tabAdv.Controls.Add(this.chkAdvUsePluginOldStyleLoad);
            this.tabAdv.Controls.Add(this.lblAdvStartup);
            this.tabAdv.Controls.Add(this.panel17);
            this.tabAdv.Controls.Add(this.chkAdvShowCfmMsgRoundRestartNext);
            this.tabAdv.Controls.Add(this.lblAdvShowDICESpecialOptionsNotice);
            this.tabAdv.Controls.Add(this.chkAdvShowDICESpecialOptions);
            this.tabAdv.Controls.Add(this.lblAdvSpecialSwitches);
            this.tabAdv.Controls.Add(this.panel14);
            this.tabAdv.Controls.Add(this.chkAdvShowRoundTimerConstantly);
            this.tabAdv.Controls.Add(this.lblAdvConVisuals);
            this.tabAdv.Controls.Add(this.lblAdvLayerTabsChangeNotice);
            this.tabAdv.Controls.Add(this.chkAdvHideLocalAccountsTab);
            this.tabAdv.Controls.Add(this.chkAdvHideLocalPluginsTab);
            this.tabAdv.Controls.Add(this.lblAdvLayerTabs);
            this.tabAdv.Controls.Add(this.panel11);
            this.tabAdv.Controls.Add(this.chkAdvEnableChatAdminName);
            this.tabAdv.Controls.Add(this.lblAdvChatTab);
            this.tabAdv.Controls.Add(this.panel10);
            this.tabAdv.Controls.Add(this.chkAdvEnableAdminMoveMsg);
            this.tabAdv.Controls.Add(this.lblAdvPlayerTab);
            this.tabAdv.Controls.Add(this.panel9);
            this.tabAdv.Controls.Add(this.panel12);
            this.tabAdv.Location = new System.Drawing.Point(4, 24);
            this.tabAdv.Name = "tabAdv";
            this.tabAdv.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdv.Size = new System.Drawing.Size(390, 475);
            this.tabAdv.TabIndex = 3;
            this.tabAdv.Text = "Advanced";
            this.tabAdv.UseVisualStyleBackColor = true;
            // 
            // lblAdvStartupChangeNotice
            // 
            this.lblAdvStartupChangeNotice.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvStartupChangeNotice.Location = new System.Drawing.Point(47, 315);
            this.lblAdvStartupChangeNotice.Name = "lblAdvStartupChangeNotice";
            this.lblAdvStartupChangeNotice.Size = new System.Drawing.Size(305, 46);
            this.lblAdvStartupChangeNotice.TabIndex = 37;
            this.lblAdvStartupChangeNotice.Text = "Disabling may break older plugins variable load. Changing will require a restart " +
    "to take effect.";
            // 
            // chkAdvUsePluginOldStyleLoad
            // 
            this.chkAdvUsePluginOldStyleLoad.AutoSize = true;
            this.chkAdvUsePluginOldStyleLoad.Location = new System.Drawing.Point(30, 293);
            this.chkAdvUsePluginOldStyleLoad.Name = "chkAdvUsePluginOldStyleLoad";
            this.chkAdvUsePluginOldStyleLoad.Size = new System.Drawing.Size(227, 19);
            this.chkAdvUsePluginOldStyleLoad.TabIndex = 36;
            this.chkAdvUsePluginOldStyleLoad.Text = "Use old style to load plugins at startup";
            this.chkAdvUsePluginOldStyleLoad.UseVisualStyleBackColor = true;
            this.chkAdvUsePluginOldStyleLoad.CheckedChanged += new System.EventHandler(this.chkAdvUsePluginOldStyleLoad_CheckedChanged);
            // 
            // lblAdvStartup
            // 
            this.lblAdvStartup.AutoSize = true;
            this.lblAdvStartup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvStartup.Location = new System.Drawing.Point(17, 275);
            this.lblAdvStartup.Name = "lblAdvStartup";
            this.lblAdvStartup.Size = new System.Drawing.Size(49, 15);
            this.lblAdvStartup.TabIndex = 34;
            this.lblAdvStartup.Text = "Startup";
            // 
            // panel17
            // 
            this.panel17.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel17.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel17.Location = new System.Drawing.Point(20, 284);
            this.panel17.Name = "panel17";
            this.panel17.Size = new System.Drawing.Size(355, 1);
            this.panel17.TabIndex = 35;
            // 
            // chkAdvShowCfmMsgRoundRestartNext
            // 
            this.chkAdvShowCfmMsgRoundRestartNext.AutoSize = true;
            this.chkAdvShowCfmMsgRoundRestartNext.Location = new System.Drawing.Point(29, 253);
            this.chkAdvShowCfmMsgRoundRestartNext.Name = "chkAdvShowCfmMsgRoundRestartNext";
            this.chkAdvShowCfmMsgRoundRestartNext.Size = new System.Drawing.Size(239, 19);
            this.chkAdvShowCfmMsgRoundRestartNext.TabIndex = 33;
            this.chkAdvShowCfmMsgRoundRestartNext.Text = "Need to confirm round restart / run next";
            this.chkAdvShowCfmMsgRoundRestartNext.UseVisualStyleBackColor = true;
            this.chkAdvShowCfmMsgRoundRestartNext.CheckedChanged += new System.EventHandler(this.chkAdvShowCfmMsgRoundRestartNext_CheckedChanged);
            // 
            // lblAdvShowDICESpecialOptionsNotice
            // 
            this.lblAdvShowDICESpecialOptionsNotice.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvShowDICESpecialOptionsNotice.Location = new System.Drawing.Point(46, 422);
            this.lblAdvShowDICESpecialOptionsNotice.Name = "lblAdvShowDICESpecialOptionsNotice";
            this.lblAdvShowDICESpecialOptionsNotice.Size = new System.Drawing.Size(305, 38);
            this.lblAdvShowDICESpecialOptionsNotice.TabIndex = 32;
            this.lblAdvShowDICESpecialOptionsNotice.Text = "Only activate if you know what you are doing and what effect those settings will " +
    "have";
            // 
            // chkAdvShowDICESpecialOptions
            // 
            this.chkAdvShowDICESpecialOptions.AutoSize = true;
            this.chkAdvShowDICESpecialOptions.Location = new System.Drawing.Point(30, 401);
            this.chkAdvShowDICESpecialOptions.Name = "chkAdvShowDICESpecialOptions";
            this.chkAdvShowDICESpecialOptions.Size = new System.Drawing.Size(142, 19);
            this.chkAdvShowDICESpecialOptions.TabIndex = 31;
            this.chkAdvShowDICESpecialOptions.Text = "DICE internal switches";
            this.chkAdvShowDICESpecialOptions.UseVisualStyleBackColor = true;
            this.chkAdvShowDICESpecialOptions.CheckedChanged += new System.EventHandler(this.chkAdvShowDICESpecialOptions_CheckedChanged);
            // 
            // lblAdvSpecialSwitches
            // 
            this.lblAdvSpecialSwitches.AutoSize = true;
            this.lblAdvSpecialSwitches.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvSpecialSwitches.Location = new System.Drawing.Point(17, 372);
            this.lblAdvSpecialSwitches.Name = "lblAdvSpecialSwitches";
            this.lblAdvSpecialSwitches.Size = new System.Drawing.Size(99, 15);
            this.lblAdvSpecialSwitches.TabIndex = 30;
            this.lblAdvSpecialSwitches.Text = "Special Switches";
            // 
            // panel14
            // 
            this.panel14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel14.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel14.Location = new System.Drawing.Point(18, 381);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(355, 1);
            this.panel14.TabIndex = 29;
            // 
            // chkAdvShowRoundTimerConstantly
            // 
            this.chkAdvShowRoundTimerConstantly.AutoSize = true;
            this.chkAdvShowRoundTimerConstantly.Location = new System.Drawing.Point(29, 228);
            this.chkAdvShowRoundTimerConstantly.Name = "chkAdvShowRoundTimerConstantly";
            this.chkAdvShowRoundTimerConstantly.Size = new System.Drawing.Size(178, 19);
            this.chkAdvShowRoundTimerConstantly.TabIndex = 28;
            this.chkAdvShowRoundTimerConstantly.Text = "Show Round time constantly";
            this.chkAdvShowRoundTimerConstantly.UseVisualStyleBackColor = true;
            this.chkAdvShowRoundTimerConstantly.CheckedChanged += new System.EventHandler(this.chkAdvShowRoundTimerConstantly_CheckedChanged);
            // 
            // lblAdvConVisuals
            // 
            this.lblAdvConVisuals.AutoSize = true;
            this.lblAdvConVisuals.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvConVisuals.Location = new System.Drawing.Point(17, 210);
            this.lblAdvConVisuals.Name = "lblAdvConVisuals";
            this.lblAdvConVisuals.Size = new System.Drawing.Size(110, 15);
            this.lblAdvConVisuals.TabIndex = 27;
            this.lblAdvConVisuals.Text = "Connection Visuals";
            // 
            // lblAdvLayerTabsChangeNotice
            // 
            this.lblAdvLayerTabsChangeNotice.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvLayerTabsChangeNotice.Location = new System.Drawing.Point(47, 172);
            this.lblAdvLayerTabsChangeNotice.Name = "lblAdvLayerTabsChangeNotice";
            this.lblAdvLayerTabsChangeNotice.Size = new System.Drawing.Size(305, 38);
            this.lblAdvLayerTabsChangeNotice.TabIndex = 26;
            this.lblAdvLayerTabsChangeNotice.Text = "Changes will require a restart before they come into effect";
            // 
            // chkAdvHideLocalAccountsTab
            // 
            this.chkAdvHideLocalAccountsTab.AutoSize = true;
            this.chkAdvHideLocalAccountsTab.Checked = true;
            this.chkAdvHideLocalAccountsTab.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAdvHideLocalAccountsTab.Location = new System.Drawing.Point(32, 150);
            this.chkAdvHideLocalAccountsTab.Name = "chkAdvHideLocalAccountsTab";
            this.chkAdvHideLocalAccountsTab.Size = new System.Drawing.Size(150, 19);
            this.chkAdvHideLocalAccountsTab.TabIndex = 22;
            this.chkAdvHideLocalAccountsTab.Text = "Hide local accounts tab";
            this.chkAdvHideLocalAccountsTab.UseVisualStyleBackColor = true;
            this.chkAdvHideLocalAccountsTab.CheckedChanged += new System.EventHandler(this.chkAdvHideLocalAccountsTab_CheckedChanged);
            // 
            // chkAdvHideLocalPluginsTab
            // 
            this.chkAdvHideLocalPluginsTab.AutoSize = true;
            this.chkAdvHideLocalPluginsTab.Checked = true;
            this.chkAdvHideLocalPluginsTab.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAdvHideLocalPluginsTab.Location = new System.Drawing.Point(32, 125);
            this.chkAdvHideLocalPluginsTab.Name = "chkAdvHideLocalPluginsTab";
            this.chkAdvHideLocalPluginsTab.Size = new System.Drawing.Size(141, 19);
            this.chkAdvHideLocalPluginsTab.TabIndex = 21;
            this.chkAdvHideLocalPluginsTab.Text = "Hide local plugins tab";
            this.chkAdvHideLocalPluginsTab.UseVisualStyleBackColor = true;
            this.chkAdvHideLocalPluginsTab.CheckedChanged += new System.EventHandler(this.chkAdvHideLocalPluginsTab_CheckedChanged);
            // 
            // lblAdvLayerTabs
            // 
            this.lblAdvLayerTabs.AutoSize = true;
            this.lblAdvLayerTabs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvLayerTabs.Location = new System.Drawing.Point(17, 104);
            this.lblAdvLayerTabs.Name = "lblAdvLayerTabs";
            this.lblAdvLayerTabs.Size = new System.Drawing.Size(64, 15);
            this.lblAdvLayerTabs.TabIndex = 19;
            this.lblAdvLayerTabs.Text = "Layer Tabs";
            // 
            // panel11
            // 
            this.panel11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel11.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel11.Location = new System.Drawing.Point(20, 116);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(355, 1);
            this.panel11.TabIndex = 20;
            // 
            // chkAdvEnableChatAdminName
            // 
            this.chkAdvEnableChatAdminName.AutoSize = true;
            this.chkAdvEnableChatAdminName.Checked = true;
            this.chkAdvEnableChatAdminName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAdvEnableChatAdminName.Location = new System.Drawing.Point(30, 82);
            this.chkAdvEnableChatAdminName.Name = "chkAdvEnableChatAdminName";
            this.chkAdvEnableChatAdminName.Size = new System.Drawing.Size(225, 19);
            this.chkAdvEnableChatAdminName.TabIndex = 18;
            this.chkAdvEnableChatAdminName.Text = "Enable Admin name on chat message";
            this.chkAdvEnableChatAdminName.UseVisualStyleBackColor = true;
            this.chkAdvEnableChatAdminName.CheckedChanged += new System.EventHandler(this.chkAdvEnableChatAdminName_CheckedChanged);
            // 
            // lblAdvChatTab
            // 
            this.lblAdvChatTab.AutoSize = true;
            this.lblAdvChatTab.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvChatTab.Location = new System.Drawing.Point(17, 64);
            this.lblAdvChatTab.Name = "lblAdvChatTab";
            this.lblAdvChatTab.Size = new System.Drawing.Size(54, 15);
            this.lblAdvChatTab.TabIndex = 16;
            this.lblAdvChatTab.Text = "Chat Tab";
            // 
            // panel10
            // 
            this.panel10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel10.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel10.Location = new System.Drawing.Point(19, 73);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(355, 1);
            this.panel10.TabIndex = 17;
            // 
            // chkAdvEnableAdminMoveMsg
            // 
            this.chkAdvEnableAdminMoveMsg.AutoSize = true;
            this.chkAdvEnableAdminMoveMsg.Location = new System.Drawing.Point(30, 42);
            this.chkAdvEnableAdminMoveMsg.Name = "chkAdvEnableAdminMoveMsg";
            this.chkAdvEnableAdminMoveMsg.Size = new System.Drawing.Size(182, 19);
            this.chkAdvEnableAdminMoveMsg.TabIndex = 15;
            this.chkAdvEnableAdminMoveMsg.Text = "Enable Admin Move Message";
            this.chkAdvEnableAdminMoveMsg.UseVisualStyleBackColor = true;
            this.chkAdvEnableAdminMoveMsg.CheckedChanged += new System.EventHandler(this.chkAdvEnableAdminMoveMsg_CheckedChanged);
            // 
            // lblAdvPlayerTab
            // 
            this.lblAdvPlayerTab.AutoSize = true;
            this.lblAdvPlayerTab.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdvPlayerTab.Location = new System.Drawing.Point(17, 24);
            this.lblAdvPlayerTab.Name = "lblAdvPlayerTab";
            this.lblAdvPlayerTab.Size = new System.Drawing.Size(63, 15);
            this.lblAdvPlayerTab.TabIndex = 1;
            this.lblAdvPlayerTab.Text = "Player Tab";
            // 
            // panel9
            // 
            this.panel9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel9.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel9.Location = new System.Drawing.Point(20, 33);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(355, 1);
            this.panel9.TabIndex = 11;
            // 
            // panel12
            // 
            this.panel12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel12.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel12.Location = new System.Drawing.Point(18, 219);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(355, 1);
            this.panel12.TabIndex = 12;
            // 
            // tabAdv2
            // 
            this.tabAdv2.Controls.Add(this.chkAdv2EnableAdminReason);
            this.tabAdv2.Controls.Add(this.lblAdv2BanTab);
            this.tabAdv2.Controls.Add(this.panel18);
            this.tabAdv2.Location = new System.Drawing.Point(4, 24);
            this.tabAdv2.Name = "tabAdv2";
            this.tabAdv2.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdv2.Size = new System.Drawing.Size(390, 475);
            this.tabAdv2.TabIndex = 5;
            this.tabAdv2.Text = "Advanced2";
            this.tabAdv2.UseVisualStyleBackColor = true;
            // 
            // chkAdv2EnableAdminReason
            // 
            this.chkAdv2EnableAdminReason.AutoSize = true;
            this.chkAdv2EnableAdminReason.Location = new System.Drawing.Point(30, 42);
            this.chkAdv2EnableAdminReason.Name = "chkAdv2EnableAdminReason";
            this.chkAdv2EnableAdminReason.Size = new System.Drawing.Size(207, 19);
            this.chkAdv2EnableAdminReason.TabIndex = 18;
            this.chkAdv2EnableAdminReason.Text = "Enable Admin name in ban reason";
            this.chkAdv2EnableAdminReason.UseVisualStyleBackColor = true;
            this.chkAdv2EnableAdminReason.CheckedChanged += new System.EventHandler(this.chkAdv2EnableAdminReason_CheckedChanged);
            // 
            // lblAdv2BanTab
            // 
            this.lblAdv2BanTab.AutoSize = true;
            this.lblAdv2BanTab.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAdv2BanTab.Location = new System.Drawing.Point(17, 24);
            this.lblAdv2BanTab.Name = "lblAdv2BanTab";
            this.lblAdv2BanTab.Size = new System.Drawing.Size(33, 15);
            this.lblAdv2BanTab.TabIndex = 16;
            this.lblAdv2BanTab.Text = "Bans";
            // 
            // panel18
            // 
            this.panel18.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel18.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel18.Location = new System.Drawing.Point(20, 33);
            this.panel18.Name = "panel18";
            this.panel18.Size = new System.Drawing.Size(355, 1);
            this.panel18.TabIndex = 17;
            // 
            // tabPlayerLookup
            // 
            this.tabPlayerLookup.Controls.Add(this.lblStatsLinkHelpText);
            this.tabPlayerLookup.Controls.Add(this.pnlStatsLinkManage);
            this.tabPlayerLookup.Controls.Add(this.lblStatsPlayerTab);
            this.tabPlayerLookup.Controls.Add(this.panel15);
            this.tabPlayerLookup.Location = new System.Drawing.Point(4, 24);
            this.tabPlayerLookup.Name = "tabPlayerLookup";
            this.tabPlayerLookup.Size = new System.Drawing.Size(390, 475);
            this.tabPlayerLookup.TabIndex = 4;
            this.tabPlayerLookup.Text = "Stats-Links";
            this.tabPlayerLookup.UseVisualStyleBackColor = true;
            // 
            // lblStatsLinkHelpText
            // 
            this.lblStatsLinkHelpText.AutoEllipsis = true;
            this.lblStatsLinkHelpText.Location = new System.Drawing.Point(35, 290);
            this.lblStatsLinkHelpText.Name = "lblStatsLinkHelpText";
            this.lblStatsLinkHelpText.Size = new System.Drawing.Size(301, 163);
            this.lblStatsLinkHelpText.TabIndex = 15;
            this.lblStatsLinkHelpText.Text = "Help text goes here. ";
            // 
            // pnlStatsLinkManage
            // 
            this.pnlStatsLinkManage.Controls.Add(this.btnAddStatsLink);
            this.pnlStatsLinkManage.Controls.Add(this.btnRemoveStatsLink);
            this.pnlStatsLinkManage.Controls.Add(this.lsvStatsLinksList);
            this.pnlStatsLinkManage.Controls.Add(this.txtStatsLinkName);
            this.pnlStatsLinkManage.Controls.Add(this.txtStatsLinkUrl);
            this.pnlStatsLinkManage.Controls.Add(this.lblStatsLinkUrl);
            this.pnlStatsLinkManage.Controls.Add(this.lblStatsLinkName);
            this.pnlStatsLinkManage.Location = new System.Drawing.Point(3, 43);
            this.pnlStatsLinkManage.Name = "pnlStatsLinkManage";
            this.pnlStatsLinkManage.Size = new System.Drawing.Size(387, 226);
            this.pnlStatsLinkManage.TabIndex = 14;
            // 
            // btnAddStatsLink
            // 
            this.btnAddStatsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddStatsLink.Enabled = false;
            this.btnAddStatsLink.FlatAppearance.BorderSize = 0;
            this.btnAddStatsLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddStatsLink.Location = new System.Drawing.Point(342, 41);
            this.btnAddStatsLink.Name = "btnAddStatsLink";
            this.btnAddStatsLink.Size = new System.Drawing.Size(35, 23);
            this.btnAddStatsLink.TabIndex = 29;
            this.btnAddStatsLink.UseVisualStyleBackColor = true;
            this.btnAddStatsLink.Click += new System.EventHandler(this.btnAddStatsLink_Click);
            // 
            // btnRemoveStatsLink
            // 
            this.btnRemoveStatsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveStatsLink.Enabled = false;
            this.btnRemoveStatsLink.FlatAppearance.BorderSize = 0;
            this.btnRemoveStatsLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveStatsLink.Location = new System.Drawing.Point(343, 120);
            this.btnRemoveStatsLink.Name = "btnRemoveStatsLink";
            this.btnRemoveStatsLink.Size = new System.Drawing.Size(35, 23);
            this.btnRemoveStatsLink.TabIndex = 28;
            this.btnRemoveStatsLink.UseVisualStyleBackColor = true;
            this.btnRemoveStatsLink.Click += new System.EventHandler(this.btnRemoveStatsLink_Click);
            // 
            // lsvStatsLinksList
            // 
            this.lsvStatsLinksList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colStatsLinksName,
            this.colStatsLinkUrl});
            this.lsvStatsLinksList.FullRowSelect = true;
            this.lsvStatsLinksList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lsvStatsLinksList.HideSelection = false;
            this.lsvStatsLinksList.Location = new System.Drawing.Point(29, 70);
            this.lsvStatsLinksList.MultiSelect = false;
            this.lsvStatsLinksList.Name = "lsvStatsLinksList";
            this.lsvStatsLinksList.ShowItemToolTips = true;
            this.lsvStatsLinksList.Size = new System.Drawing.Size(304, 136);
            this.lsvStatsLinksList.TabIndex = 27;
            this.lsvStatsLinksList.UseCompatibleStateImageBehavior = false;
            this.lsvStatsLinksList.View = System.Windows.Forms.View.Details;
            this.lsvStatsLinksList.SelectedIndexChanged += new System.EventHandler(this.lsvStatsLinksList_SelectedIndexChanged);
            this.lsvStatsLinksList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lsvStatsLinksList_MouseDoubleClick);
            // 
            // colStatsLinksName
            // 
            this.colStatsLinksName.Text = "Name";
            // 
            // colStatsLinkUrl
            // 
            this.colStatsLinkUrl.Text = "URL";
            this.colStatsLinkUrl.Width = 240;
            // 
            // txtStatsLinkName
            // 
            this.txtStatsLinkName.Location = new System.Drawing.Point(29, 41);
            this.txtStatsLinkName.Name = "txtStatsLinkName";
            this.txtStatsLinkName.Size = new System.Drawing.Size(64, 23);
            this.txtStatsLinkName.TabIndex = 22;
            this.txtStatsLinkName.TextChanged += new System.EventHandler(this.txtStatsLinkName_TextChanged);
            // 
            // txtStatsLinkUrl
            // 
            this.txtStatsLinkUrl.Location = new System.Drawing.Point(99, 41);
            this.txtStatsLinkUrl.Name = "txtStatsLinkUrl";
            this.txtStatsLinkUrl.Size = new System.Drawing.Size(234, 23);
            this.txtStatsLinkUrl.TabIndex = 23;
            this.txtStatsLinkUrl.TextChanged += new System.EventHandler(this.txtStatsLinkUrl_TextChanged);
            // 
            // lblStatsLinkUrl
            // 
            this.lblStatsLinkUrl.AutoSize = true;
            this.lblStatsLinkUrl.Location = new System.Drawing.Point(96, 23);
            this.lblStatsLinkUrl.Name = "lblStatsLinkUrl";
            this.lblStatsLinkUrl.Size = new System.Drawing.Size(28, 15);
            this.lblStatsLinkUrl.TabIndex = 1;
            this.lblStatsLinkUrl.Text = "URL";
            // 
            // lblStatsLinkName
            // 
            this.lblStatsLinkName.AutoSize = true;
            this.lblStatsLinkName.Location = new System.Drawing.Point(26, 23);
            this.lblStatsLinkName.Name = "lblStatsLinkName";
            this.lblStatsLinkName.Size = new System.Drawing.Size(39, 15);
            this.lblStatsLinkName.TabIndex = 0;
            this.lblStatsLinkName.Text = "Name";
            // 
            // lblStatsPlayerTab
            // 
            this.lblStatsPlayerTab.AutoSize = true;
            this.lblStatsPlayerTab.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatsPlayerTab.Location = new System.Drawing.Point(17, 24);
            this.lblStatsPlayerTab.Name = "lblStatsPlayerTab";
            this.lblStatsPlayerTab.Size = new System.Drawing.Size(63, 15);
            this.lblStatsPlayerTab.TabIndex = 12;
            this.lblStatsPlayerTab.Text = "Player Tab";
            // 
            // panel15
            // 
            this.panel15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel15.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel15.Location = new System.Drawing.Point(20, 33);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(355, 1);
            this.panel15.TabIndex = 13;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(337, 523);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 25;
            this.btnClose.Text = "Close";
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 557);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tbcOptions);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.tbcOptions.ResumeLayout(false);
            this.tabBasics.ResumeLayout(false);
            this.tabBasics.PerformLayout();
            this.tabPlugins.ResumeLayout(false);
            this.tabPlugins.PerformLayout();
            this.pnlSandboxOptions.ResumeLayout(false);
            this.pnlSandboxOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPluginMaxRuntimeSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPluginMaxRuntimeMin)).EndInit();
            this.pnlPluginsAllowedDomains.ResumeLayout(false);
            this.pnlPluginsAllowedDomains.PerformLayout();
            this.tabHttpServer.ResumeLayout(false);
            this.tabHttpServer.PerformLayout();
            this.pnlHttpServerSettings.ResumeLayout(false);
            this.pnlHttpServerSettings.PerformLayout();
            this.pnlHttpServerTester.ResumeLayout(false);
            this.pnlHttpServerTester.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHttpServerForwardedTestStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHttpServerServerStatus)).EndInit();
            this.tabAdv.ResumeLayout(false);
            this.tabAdv.PerformLayout();
            this.tabAdv2.ResumeLayout(false);
            this.tabAdv2.PerformLayout();
            this.tabPlayerLookup.ResumeLayout(false);
            this.tabPlayerLookup.PerformLayout();
            this.pnlStatsLinkManage.ResumeLayout(false);
            this.pnlStatsLinkManage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tbcOptions;
        private System.Windows.Forms.TabPage tabBasics;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblBasicsShowWindow;
        private System.Windows.Forms.Label lblBasicPreferences;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox chkBasicsAutoCheckDownloadForUpdates;
        private System.Windows.Forms.Label lblBasicsPrivacy;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox chkBasicsEnableConsoleLogging;
        private System.Windows.Forms.CheckBox chkBasicsEnableChatLogging;
        private System.Windows.Forms.Label lblBasicsLogging;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBasicsSetLanguage;
        private System.Windows.Forms.Label lblBasicsLanguage;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel lnkBasicsAuthor;
        private System.Windows.Forms.Label lblBasicsAuthor;
        private PRoCon.Controls.ControlsEx.ComboBoxEx cboBasicsLanguagePicker;
        private PRoCon.Controls.ControlsEx.ComboBoxEx cboBasicsShowWindow;
        private System.Windows.Forms.CheckBox chkBasicsMinimizeToTray;
        private System.Windows.Forms.CheckBox chkBasicsCloseToTray;
        private System.Windows.Forms.CheckBox chkBasicsEnableEventsLogging;
        private System.Windows.Forms.TabPage tabPlugins;
        private PRoCon.Controls.ControlsEx.ComboBoxEx cboPluginsSandboxOptions;
        private System.Windows.Forms.Label lblPluginsSecurity;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel pnlSandboxOptions;
        private System.Windows.Forms.Panel pnlPluginsAllowedDomains;
        private System.Windows.Forms.Label lblPluginsOutgoingConnections;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.TextBox txtPluginsTrustedHostDomain;
        private System.Windows.Forms.Label lblPluginsTrustedHostDomain;
        private System.Windows.Forms.TextBox txtPluginsTrustedPort;
        private System.Windows.Forms.Label lblPluginsPort;
        private System.Windows.Forms.Label lblPluginsDatabases;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Button btnPluginsAddTrustedHostDomain;
        private System.Windows.Forms.Button btnPluginsRemoveTrustedHostDomain;
        private System.Windows.Forms.CheckBox chkAllowODBCConnections;
        private System.Windows.Forms.Label lblPluginsChangesAfterRestart;
        private PRoCon.Controls.ControlsEx.ListViewNF lsvTrustedHostDomainPorts;
        private System.Windows.Forms.ColumnHeader colTrustedDomainsHost;
        private System.Windows.Forms.ColumnHeader colTrustedPort;
        private System.Windows.Forms.CheckBox chkBasicsAutoApplyUpdates;
        private System.Windows.Forms.TabPage tabHttpServer;
        private System.Windows.Forms.Label lblHttpServerTitle;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel pnlHttpServerTester;
        private System.Windows.Forms.PictureBox picHttpServerForwardedTestStatus;
        private System.Windows.Forms.Label lblHttpServerForwardedTestStatus;
        private System.Windows.Forms.LinkLabel lnkHttpServerForwardedTest;
        private System.Windows.Forms.LinkLabel lnkStartStopHttpServer;
        private System.Windows.Forms.PictureBox picHttpServerServerStatus;
        private System.Windows.Forms.Label lblHttpServerServerStatus;
        private System.Windows.Forms.Panel pnlHttpServerSettings;
        private System.Windows.Forms.Label lblBindingExplanation;
        private System.Windows.Forms.TextBox txtHttpServerBindingAddress;
        private System.Windows.Forms.Label lblHttpServerBindingIP;
        private System.Windows.Forms.Label lblHttpServerStartPort;
        private System.Windows.Forms.TextBox txtHttpServerStartPort;
        private System.Windows.Forms.LinkLabel lnkHttpServerExampleLink;
        private System.Windows.Forms.CheckBox chkBasicsEnablePluginLogging;
        private System.Windows.Forms.TabPage tabAdv;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label lblAdvPlayerTab;
        private System.Windows.Forms.CheckBox chkAdvEnableChatAdminName;
        private System.Windows.Forms.Label lblAdvChatTab;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.CheckBox chkAdvEnableAdminMoveMsg;
        private System.Windows.Forms.Label lblAdvLayerTabsChangeNotice;
        private System.Windows.Forms.CheckBox chkAdvHideLocalAccountsTab;
        private System.Windows.Forms.CheckBox chkAdvHideLocalPluginsTab;
        private System.Windows.Forms.Label lblAdvLayerTabs;
        private System.Windows.Forms.Panel panel11;
        private System.Windows.Forms.Label lblAdvConVisuals;
        private System.Windows.Forms.Panel panel12;
        private System.Windows.Forms.CheckBox chkAdvShowRoundTimerConstantly;
        private System.Windows.Forms.CheckBox chkAllowSmtpConnections;
        private System.Windows.Forms.Label lblPluginsMail;
        private System.Windows.Forms.Panel panel13;
        private System.Windows.Forms.CheckBox chkBasicsAutoCheckGameConfigsForUpdates;
        private System.Windows.Forms.Label lblAdvShowDICESpecialOptionsNotice;
        private System.Windows.Forms.CheckBox chkAdvShowDICESpecialOptions;
        private System.Windows.Forms.Label lblAdvSpecialSwitches;
        private System.Windows.Forms.Panel panel14;
        private System.Windows.Forms.CheckBox chkAdvShowCfmMsgRoundRestartNext;
        private System.Windows.Forms.TabPage tabPlayerLookup;
        private System.Windows.Forms.Label lblStatsPlayerTab;
        private System.Windows.Forms.Panel panel15;
        private System.Windows.Forms.Panel pnlStatsLinkManage;
        private System.Windows.Forms.Button btnAddStatsLink;
        private System.Windows.Forms.Button btnRemoveStatsLink;
        private Controls.ControlsEx.ListViewNF lsvStatsLinksList;
        private System.Windows.Forms.ColumnHeader colStatsLinksName;
        private System.Windows.Forms.ColumnHeader colStatsLinkUrl;
        private System.Windows.Forms.TextBox txtStatsLinkName;
        private System.Windows.Forms.TextBox txtStatsLinkUrl;
        private System.Windows.Forms.Label lblStatsLinkUrl;
        private System.Windows.Forms.Label lblStatsLinkName;
        private System.Windows.Forms.Label lblStatsLinkHelpText;
        private System.Windows.Forms.Label lblPluginMaxRuntimeSec;
        private System.Windows.Forms.Label lblPluginMaxRuntimeMin;
        private System.Windows.Forms.Label lblPluginMaxRuntime;
        private System.Windows.Forms.Panel panel16;
        private System.Windows.Forms.NumericUpDown numPluginMaxRuntimeMin;
        private System.Windows.Forms.NumericUpDown numPluginMaxRuntimeSec;
        private System.Windows.Forms.CheckBox chkAdvUsePluginOldStyleLoad;
        private System.Windows.Forms.Label lblAdvStartup;
        private System.Windows.Forms.Panel panel17;
        private System.Windows.Forms.Label lblAdvStartupChangeNotice;
        private System.Windows.Forms.TabPage tabAdv2;
        private System.Windows.Forms.CheckBox chkAdv2EnableAdminReason;
        private System.Windows.Forms.Label lblAdv2BanTab;
        private System.Windows.Forms.Panel panel18;
        private System.Windows.Forms.CheckBox chkEnablePluginDebugging;
        private System.Windows.Forms.Label lblPluginsDebug;
        private System.Windows.Forms.Panel panel19;
        private System.Windows.Forms.CheckBox rssFeedOptCheckbox;
        private System.Windows.Forms.CheckBox localGeoIPCheckbox;
    }
}