using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PRoCon.Forms
{
    using Core;
    using Core.AutoUpdates;
    using Core.Remote;
    using PRoCon.Controls;

    public partial class frmMain : Form
    {

        public delegate PRoConApplication WindowLoadedHandler(bool execute);
        public event WindowLoadedHandler WindowLoaded;

        private Dictionary<string, uscPage> m_dicPages = new Dictionary<string, uscPage>();
        private CLocalization m_clocLanguage;

        private frmAbout m_frmAbout;
        private frmManageAccounts m_frmManageAccounts;
        private frmOptions m_frmOptions;
        //private frmPluginRepository m_frmPluginRepository;


        private ContextMenu m_cnmNotificationMenu;
        private MenuItem m_mnuHideTrayIcon;
        private MenuItem m_mnuSeparator;
        private MenuItem m_mnuExit;

        //private Rectangle m_recNormalBounds;

        //private int m_iMaxGspServers;
        //private int m_iBlockUpdateChecks;
        private bool m_blExit;

        //private CDownloadFile m_cdfVersionChecker;
        //private CDownloadFile m_cdfPRoConUpdate;

        //private static readonly object m_objDownloadingLocalizations = new object();
        //private List<CDownloadFile> m_lstDownloadingLocalizations;

        private PRoConApplication m_paProcon;

        public frmMain(string[] args)
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.m_blExit = false;

            this.m_frmAbout = new frmAbout();

            this.m_cnmNotificationMenu = new ContextMenu();

            this.m_mnuHideTrayIcon = new MenuItem
            {
                Index = 0,
                Text = @"Hide Tray Icon"
            };
            this.m_mnuHideTrayIcon.Click += new System.EventHandler(this.m_mnuHideTrayIcon_Click);
            this.m_cnmNotificationMenu.MenuItems.Add(this.m_mnuHideTrayIcon);

            this.m_mnuSeparator = new MenuItem
            {
                Index = 1,
                Text = @"-"
            };
            this.m_cnmNotificationMenu.MenuItems.Add(this.m_mnuSeparator);

            this.m_mnuExit = new MenuItem
            {
                Index = 2,
                Text = @"Exit"
            };
            this.m_mnuExit.Click += new System.EventHandler(this.m_mnuExit_Click);
            this.m_cnmNotificationMenu.MenuItems.Add(this.m_mnuExit);

            this.ntfIcon.ContextMenu = this.m_cnmNotificationMenu;

            this.toolsStripDropDownButton.Image = this.iglIcons.Images["wrench.png"];
            this.manageAccountsToolStripMenuItem.Image = this.iglIcons.Images["vcard.png"];

            this.btnStartPage.Image = this.iglIcons.Images["home.png"];
            this.btnShiftServerPrevious.Image = this.iglIcons.Images["arrow-transition-180.png"];
            this.btnShiftServerNext.Image = this.iglIcons.Images["arrow-transition.png"];

            this.checkForUpdatesToolStripMenuItem.Image = this.iglIcons.Images["check.png"];

            //this.toolStripDownloading.Image = picAjaxStyleLoading.Image;

            this.cboServerList.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.cboServerList.ComboBox.DrawItem += new DrawItemEventHandler(ComboBox_DrawItem);


            //this.m_cdfVersionChecker = new CDownloadFile("http://www.phogue.net/procon/version3.php");
            //this.m_cdfVersionChecker.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_cdfVersionChecker_DownloadComplete);
            //this.m_cdfVersionChecker.DownloadError += new CDownloadFile.DownloadFileEventDelegate(m_cdfVersionChecker_DownloadError);

            //this.m_lstDownloadingLocalizations = new List<CDownloadFile>();

            // Default language.
            //this.m_frmOptions.LocalizationFilename = "au.loc";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            this.m_paProcon = this.WindowLoaded(false);

            string licenseAgreementRevision = "October 20, 2011";
            if (this.m_paProcon.LicenseAgreements.Contains(licenseAgreementRevision) == false)
            {
                new LicenseAgreement(this.m_paProcon, licenseAgreementRevision).ShowDialog();
            }

            this.SetupStartPage();

            this.m_paProcon.Connections.ConnectionAdded += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionAdded);
            this.m_paProcon.Connections.ConnectionRemoved += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionRemoved);
            this.m_paProcon.ShowNotification += new PRoConApplication.ShowNotificationHandler(m_paProcon_ShowNotification);
            this.m_paProcon.CurrentLanguageChanged += new PRoConApplication.CurrentLanguageHandler(m_paProcon_CurrentLanguageChanged);
            this.m_paProcon.OptionsSettings.ShowTrayIconChanged += new PRoCon.Core.Options.OptionsSettings.OptionsEnabledHandler(OptionsSettings_ShowTrayIconChanged);

            this.m_paProcon.AutoUpdater.CustomDownloadError += new AutoUpdater.CustomDownloadErrorHandler(AutoUpdater_CustomDownloadError);
            this.m_paProcon.AutoUpdater.DownloadUnzipComplete += new AutoUpdater.DownloadUnzipCompleteHandler(AutoUpdater_DownloadUnzipComplete);
            this.m_paProcon.AutoUpdater.CheckingUpdates += new AutoUpdater.CheckingUpdatesHandler(m_paProcon_CheckingUpdates);
            this.m_paProcon.AutoUpdater.NoVersionAvailable += new AutoUpdater.CheckingUpdatesHandler(m_paProcon_NoVersionAvailable);
            this.m_paProcon.AutoUpdater.VersionChecker.DownloadError += new CDownloadFile.DownloadFileEventDelegate(VersionChecker_DownloadError);
            this.m_paProcon.AutoUpdater.UpdateDownloading += new AutoUpdater.UpdateDownloadingHandler(m_paProcon_UpdateDownloading);

            this.m_paProcon.AutoUpdater.GameConfigUpdated += new AutoUpdater.CheckingUpdatesHandler(m_paProcon_GameConfigUpdated);

            this.m_paProcon.Execute();

            if (this.m_paProcon.CustomTitle.Length > 0)
            {
                this.Text = this.m_paProcon.CustomTitle;
            }

            this.m_frmManageAccounts = new frmManageAccounts(this.m_paProcon, this);
            this.m_frmOptions = new frmOptions(this.m_paProcon, this);

            this.WindowState = this.m_paProcon.SavedWindowState;
            this.Bounds = this.m_paProcon.SavedWindowBounds;
            this.Refresh();

            //foreach (PRoConClient prcClient in this.m_paProcon.Connections) {
            //    this.Connections_ConnectionAdded(prcClient);
            //}

            this.m_paProcon.CurrentLanguage = this.m_paProcon.CurrentLanguage;
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            this.optionsToolStripMenuItem.Text = this.m_clocLanguage.GetLocalized("frmMain.optionsToolStripMenuItem") + "..";
            this.manageAccountsToolStripMenuItem.Text = this.m_clocLanguage.GetLocalized("frmMain.manageAccountsToolStripMenuItem") + "..";

            this.changelogToolStripMenuItem.Text = this.m_clocLanguage.GetLocalized("frmMain.changelogToolStripMenuItem");

            this.toolsStripDropDownButton.Text = this.m_clocLanguage.GetLocalized("frmMain.toolsToolStripMenuItem");

            this.checkForUpdatesToolStripMenuItem.Text = this.m_clocLanguage.GetLocalized("frmMain.checkForUpdatesToolStripMenuItem");
            this.aboutToolStripMenuItem.Text = this.m_clocLanguage.GetLocalized("frmMain.aboutToolStripMenuItem") + "..";

            this.btnStartPage.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnStartPage");
            this.btnShiftServerPrevious.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnShiftServerPrevious");
            this.btnShiftServerNext.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnShiftServerNext");

            this.chkAutomaticallyConnect.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.chkAutomaticallyConnect");

            foreach (KeyValuePair<string, uscPage> kvpPanel in this.m_dicPages)
            {
                kvpPanel.Value.SetLocalization(this.m_clocLanguage);
            }

            if (this.m_frmManageAccounts != null)
            {
                this.m_frmManageAccounts.SetLocalization(this.m_clocLanguage);
            }

            if (this.m_frmOptions != null)
            {
                this.m_frmOptions.SetLocalization(this.m_clocLanguage);
            }

            //if (this.m_frmPluginRepository != null) {
            //    this.m_frmPluginRepository.SetLocalization(this.m_clocLanguage);
            //}

            if (this.m_frmAbout != null)
            {
                this.m_frmAbout.SetLocalization(this.m_clocLanguage);
            }

            this.m_mnuHideTrayIcon.Text = this.m_clocLanguage.GetLocalized("m_cnmNotificationMenu.m_mnuHideTrayIcon", null);
            this.m_mnuExit.Text = this.m_clocLanguage.GetLocalized("m_cnmNotificationMenu.m_mnuExit", null);

            this.cboServerList.Size = new Size(this.tlsConnections.Bounds.Width - this.toolsStripDropDownButton.Bounds.Width - this.cboServerList.Bounds.Left - 15, 23);
        }

        public void SetupStartPage()
        {

            uscStartPage startPage = new uscStartPage(this.m_paProcon);
            startPage.ConnectionPage += new uscStartPage.ConnectionPageHandler(startPage_ConnectionPage);

            this.pnlWindows.Controls.Add(startPage);
            startPage.Dock = DockStyle.Fill;
            this.m_dicPages.Add("Start Page", startPage);
            this.cboServerList.ComboBox.Items.Insert(0, startPage);
            this.cboServerList.SelectedItem = startPage;
        }

        private void startPage_ConnectionPage(string hostNamePort)
        {

            foreach (Object page in this.cboServerList.Items)
            {

                if (page is uscServerConnection)
                {

                    if (String.Compare(((uscServerConnection)page).Client.HostNamePort, hostNamePort) == 0)
                    {
                        this.cboServerList.SelectedItem = page;
                        break;
                    }
                }
            }
        }

        #region Manage accounts and options events

        private void m_paProcon_CurrentLanguageChanged(CLocalization language)
        {
            this.InvokeIfRequired(() => this.SetLocalization(language));
        }

        void OptionsSettings_ShowTrayIconChanged(bool blEnabled)
        {
            this.ntfIcon.Visible = blEnabled;
            this.ShowInTaskbar = true;
        }

        #endregion

        private void Connections_ConnectionAdded(PRoConClient item)
        {
            this.InvokeIfRequired(() =>
            {
                uscServerConnection uscNewConnectionPanel = null;

                uscNewConnectionPanel = new uscServerConnection(this.m_paProcon, item, this, this.m_frmManageAccounts);
                uscNewConnectionPanel.Dock = DockStyle.Fill;
                //uscNewConnectionPanel.SetLocalization(this.m_clocLanguage);
                //uscNewConnectionPanel.BFBC2Connection.Connect();

                uscNewConnectionPanel.ManageAccountsRequest += new uscServerConnection.ManageAccountsRequestDelegate(uscServerConnection_ManageAccountsRequest);
                uscNewConnectionPanel.OnTabChange += new uscServerConnection.OnTabChangeDelegate(uscNewConnectionPanel_OnTabChange);

                this.pnlWindows.Controls.Add(uscNewConnectionPanel);
                this.m_dicPages.Add(item.HostNamePort, uscNewConnectionPanel);

                this.cboServerList.ComboBox.Items.Add(uscNewConnectionPanel);
                if (this.cboServerList.SelectedItem == null)
                {
                    this.cboServerList.SelectedItem = uscNewConnectionPanel;
                }

                item.ConnectionClosed += new PRoConClient.EmptyParamterHandler(item_ConnectionClosed);
                item.ConnectionFailure += new PRoConClient.FailureHandler(item_ConnectionFailure);
                item.ConnectSuccess += new PRoConClient.EmptyParamterHandler(item_ConnectSuccess);
                item.Login += new PRoConClient.EmptyParamterHandler(item_Login);
                item.ConnectAttempt += new PRoConClient.EmptyParamterHandler(item_ConnectAttempt);
                item.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(item_GameTypeDiscovered);
                item.AutomaticallyConnectChanged += new PRoConClient.AutomaticallyConnectHandler(item_AutomaticallyConnectChanged);

                this.RefreshServerListing();
            });
        }

        private void Connections_ConnectionRemoved(PRoConClient item)
        {
            this.InvokeIfRequired(() =>
            {

                if (this.m_dicPages.ContainsKey(item.HostNamePort) == true)
                {

                    if (this.cboServerList.Items.Contains(this.m_dicPages[item.HostNamePort]) == true)
                    {

                        if (this.cboServerList.SelectedItem == this.m_dicPages[item.HostNamePort])
                        {
                            this.cboServerList.SelectedIndex = 0;
                        }

                        this.cboServerList.Items.Remove(this.m_dicPages[item.HostNamePort]);
                    }

                    this.m_dicPages.Remove(item.HostNamePort);
                }

                this.RefreshServerListing();
            });
        }

        private void AddServer(string strHost, UInt16 iu16Port, string strUsername, string strPassword, bool blConnect)
        {
            PRoConClient prcClient = this.m_paProcon.AddConnection(strHost, iu16Port, strUsername, strPassword);

            if (blConnect == true && prcClient != null)
            {
                prcClient.Connect();
            }
        }

        private void uscNewConnectionPanel_OnTabChange(object sender, Stack<string> stkTabIndexes)
        {

            string[] a_strReversedStack = stkTabIndexes.ToArray();
            Array.Reverse(a_strReversedStack);

            foreach (KeyValuePair<string, uscPage> kvpPanel in this.m_dicPages)
            {
                if (kvpPanel.Value != sender && kvpPanel.Value is uscServerConnection)
                {

                    if (((uscServerConnection)kvpPanel.Value).Client.State == ConnectionState.Connected)
                    {

                        try
                        {
                            ((uscServerConnection)kvpPanel.Value).SetTabIndexes(new Stack<string>(a_strReversedStack));
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void uscServerConnection_ManageAccountsRequest(object sender, EventArgs e)
        {
            this.m_frmManageAccounts.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_frmAbout.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_blExit = true;
            this.Close();
        }

        private void userManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_frmManageAccounts.ShowDialog();
        }

        private void donateTodayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://phogue.net/?page_id=380");
        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://phogue.net/procon/changelog.php");
            System.Diagnostics.Process.Start("https://myrcon.net/files/file/29-procon-client/");

        }

        private void pRoConHostingProvidersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://phogue.net/procon/proconhosting.php");
            System.Diagnostics.Process.Start("https://repo.myrcon.com/procon1/proconhosting.php");
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.m_frmOptions.ShowDialog();
        }

        //private void uscServerPlayerTreeviewListing_AutoConnectRequest(object sender) {
        //    if (this.m_dicConnectionPages.ContainsKey(e.ServerHostnamePort) == true) {
        //        this.m_dicConnectionPages[e.ServerHostnamePort].BFBC2Connection.AutomaticallyConnect = e.AutoConnect;

        //        //this.SaveMainConfig();
        //    }
        //}

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (KeyValuePair<string, uscPage> kvpPanel in this.m_dicPages)
            {
                kvpPanel.Value.Dispose();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (this.m_blExit == false && e.CloseReason == CloseReason.UserClosing && this.m_paProcon.OptionsSettings.CloseToTray == true)
            {
                e.Cancel = true;

                this.WindowState = FormWindowState.Minimized;
            }
            else
            {

                if (this.WindowState == FormWindowState.Normal)
                {
                    this.m_paProcon.SavedWindowBounds = this.Bounds;
                    //this.m_recNormalBounds = this.Bounds;
                }

                this.m_paProcon.Shutdown();
            }
        }

        #region Notification area and to-tray effect

        private void m_paProcon_ShowNotification(int timeout, string title, string text, bool isError)
        {

            ToolTipIcon ttiDisplayIcon = ToolTipIcon.Info;

            if (isError == true)
            {
                ttiDisplayIcon = ToolTipIcon.Error;
            }

            this.ntfIcon.ShowBalloonTip(timeout, title, text, ttiDisplayIcon);
        }

        private void ntfIcon_DoubleClick(object sender, EventArgs e)
        {

            // Strange bug, without setting first to Normal then back to owner draw 
            // the result is a blank server listing, but the items are all still there.
            this.cboServerList.ComboBox.DrawMode = DrawMode.Normal;
            this.cboServerList.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;

            if (this.WindowState == FormWindowState.Minimized)
            {

                this.ShowInTaskbar = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
                //this.Show();

                bool bah = this.cboServerList.IsDisposed;
            }

            this.Activate();
        }

        private void m_mnuExit_Click(object Sender, EventArgs e)
        {
            this.m_blExit = true;
            this.Close();
        }

        private void m_mnuHideTrayIcon_Click(object Sender, EventArgs e)
        {
            this.m_paProcon.OptionsSettings.ShowTrayIcon = false;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {

            this.cboServerList.Size = new Size(this.tlsConnections.Bounds.Width - this.toolsStripDropDownButton.Bounds.Width - this.cboServerList.Bounds.Left - 15, 23);

            if (this.WindowState == FormWindowState.Minimized && (this.m_paProcon.OptionsSettings.MinimizeToTray == true || this.m_paProcon.OptionsSettings.CloseToTray == true))
            {
                this.Hide();

                this.ShowInTaskbar = false;
            }
            else
            {
                this.Show();

                this.ShowInTaskbar = true;
            }

            if (this.WindowState == FormWindowState.Normal)
            {
                this.m_paProcon.SavedWindowBounds = this.Bounds;
            }

            this.m_paProcon.SavedWindowState = this.WindowState;
        }

        #endregion

        #region Version Checker..

        // Now only used if they manually check for an update and it comes back false.
        private bool m_blPopupVersionResults = false;
        private bool m_blPopupGcCheckResults = false;

        private void m_paProcon_CheckingUpdates()
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloading.ForeColor = SystemColors.WindowText;
                this.toolStripDownloading.IsLink = false;
                this.toolStripDownloading.Text = this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Checking");
                this.toolStripDownloading.Image = picAjaxStyleLoading.Image;
                this.toolStripDownloading.Visible = true;
            });
        }

        void m_paProcon_NoVersionAvailable()
        {
            this.InvokeIfRequired(() =>
            {
                if (this.m_blPopupVersionResults == true)
                {
                    MessageBox.Show(this.m_clocLanguage.GetLocalized("frmMain.MessageBox.NoUpdateAvailable", null), "PRoCon Frostbite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.m_blPopupVersionResults = false;
                }

                this.toolStripDownloading.Visible = false;
            });
        }

        void m_paProcon_GameConfigUpdated()
        {
            this.InvokeIfRequired(() =>
            {
                if (this.m_blPopupGcCheckResults == true)
                {
                    MessageBox.Show(this.m_clocLanguage.GetLocalized("frmMain.MessageBox.GameConfigUpdated", null), "PRoCon Frostbite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.m_blPopupGcCheckResults = false;
                }
            });
        }

        void VersionChecker_DownloadError(CDownloadFile cdfSender)
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloadProgress.Visible = false;

                this.toolStripDownloading.ForeColor = Color.Maroon;
                this.toolStripDownloading.Image = null;
                this.toolStripDownloading.Text = this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Error", cdfSender.Error);
            });
        }

        void AutoUpdater_CustomDownloadError(string strError)
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloadProgress.Visible = false;

                this.toolStripDownloading.ForeColor = Color.Maroon;
                this.toolStripDownloading.Image = null;
                this.toolStripDownloading.Text = this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Error", strError);
            });
        }


        private void m_paProcon_UpdateDownloading(CDownloadFile cdfDownloading)
        {
            this.InvokeIfRequired(() =>
            {
                cdfDownloading.DownloadProgressUpdate += new CDownloadFile.DownloadFileEventDelegate(cdfDownloading_DownloadProgressUpdate);
                cdfDownloading.DownloadError += new CDownloadFile.DownloadFileEventDelegate(cdfDownloading_DownloadError);
                cdfDownloading.DownloadDiscoveredFileSize += new CDownloadFile.DownloadFileEventDelegate(cdfDownloading_DownloadDiscoveredFileSize);
            });
        }

        private void cdfDownloading_DownloadDiscoveredFileSize(CDownloadFile cdfSender)
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloading.ForeColor = SystemColors.WindowText;
                this.toolStripDownloading.Image = picAjaxStyleLoading.Image;
                this.toolStripDownloadProgress.Visible = true;

                this.toolStripDownloadProgress.Maximum = cdfSender.FileSize;
            });
        }

        private void cdfDownloading_DownloadError(CDownloadFile cdfSender)
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloadProgress.Visible = false;

                this.toolStripDownloading.ForeColor = Color.Maroon;
                this.toolStripDownloading.Image = null;
                this.toolStripDownloading.Text = this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Error", cdfSender.Error);
            });
        }

        private void cdfDownloading_DownloadProgressUpdate(CDownloadFile cdfSender)
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloadProgress.Value = cdfSender.BytesDownloaded;
                this.toolStripDownloading.Text = String.Format("{0} {1}", this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading", null), cdfSender.GetLabelProgress());
            });
        }



        private void AutoUpdater_DownloadUnzipComplete()
        {
            this.InvokeIfRequired(() =>
            {
                this.toolStripDownloading.IsLink = true;
                this.toolStripDownloading.Image = this.iglIcons.Images["star.png"];
                this.toolStripDownloadProgress.Visible = false;

                this.toolStripDownloading.Text = this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Complete");
                this.tltpUpdateComplete.Show(this.m_clocLanguage.GetLocalized("frmMain.toolStripDownloading.Complete"), this, this.toolStripDownloading.Bounds.X + (this.toolStripDownloading.Bounds.Width / 2), this.stsMain.Bounds.Y, 5000);
            });
        }

        private void toolStripDownloading_Click(object sender, EventArgs e)
        {

            if (this.toolStripDownloading.IsLink == true)
            {
                DialogResult dlgVisitPage = MessageBox.Show(this.m_clocLanguage.GetLocalized("frmMain.MessageBox.RestartProcon"), "PRoCon Frostbite", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dlgVisitPage == DialogResult.Yes)
                {

                    AutoUpdater.BeginUpdateProcess(this.m_paProcon);

                    //System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe", String.Format("\"{0}\" \"{1}\"", this.m_strReleaseNotesLink, this.m_strDownloadSourceLink));
                    //this.Close();
                }
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.m_paProcon.AutoUpdater.VersionChecker.FileDownloading == false)
            {
                this.m_blPopupVersionResults = true;
                this.m_blPopupGcCheckResults = true;
                this.m_paProcon.AutoUpdater.CheckVersion();
            }
        }

        #endregion

        private void lblUpdateAvailable_Click(object sender, EventArgs e)
        {

            DialogResult dlgVisitPage = MessageBox.Show(this.m_clocLanguage.GetLocalized("frmMain.MessageBox.UpdateAvailable", null), "PRoCon Frostbite", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (dlgVisitPage == DialogResult.Yes)
            {
                //if (Regex.Match(strReleaseNotesLink, "^http://.*?$").Success == false) {
                //    strReleaseNotesLink = "http://" + strReleaseNotesLink;
                //}
                //System.Diagnostics.Process.Start(strReleaseNotesLink);

                //System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe", String.Format("\"{0}\" \"{1}\"", this.m_strReleaseNotesLink, this.m_strDownloadSourceLink));
                this.Close();
            }
        }


        private void m_frmNewConnection_CreateNewConnection(string hostname, string port, string userName, string password)
        {
            this.AddServer(hostname, ushort.Parse(port), userName, password, true);
        }

        private void cboServerList_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.cboServerList.IsDisposed == false && this.cboServerList.SelectedItem != null)
            {

                uscPage selectedServer = (uscPage)this.cboServerList.SelectedItem;

                selectedServer.Show();

                foreach (KeyValuePair<string, uscPage> kvpPanel in this.m_dicPages)
                {
                    if (kvpPanel.Value != selectedServer)
                    {
                        kvpPanel.Value.Hide();
                    }
                }

                this.btnConnectDisconnect.Enabled = true;

                this.btnConnectDisconnect_MouseLeave(this, null);
                /*
                if (selectedServer.BFBC2Connection.Connected == true) {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-connect.png"];
                }
                else {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-disconnect.png"];
                }
                */

                if (selectedServer is uscServerConnection)
                {
                    this.chkAutomaticallyConnect.Checked = ((uscServerConnection)selectedServer).Client.AutomaticallyConnect;

                    if (this.chkAutomaticallyConnect.Checked == true)
                    {
                        this.chkAutomaticallyConnect.Image = this.iglIcons.Images["tick.png"];
                    }
                    else
                    {
                        this.chkAutomaticallyConnect.Image = null;
                    }

                    this.btnConnectDisconnect.Enabled = this.chkAutomaticallyConnect.Enabled = true;
                }
                else
                {
                    this.btnConnectDisconnect.Enabled = this.chkAutomaticallyConnect.Enabled = false;
                }

            }
            else
            {
                this.btnConnectDisconnect.Enabled = false;
            }
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {

            if (e.Index != -1)
            {

                e.DrawBackground();
                e.DrawFocusRectangle();

                if (this.cboServerList.ComboBox.Items[e.Index] is uscServerConnection)
                {

                    uscServerConnection drawItem = ((uscServerConnection)this.cboServerList.ComboBox.Items[e.Index]);

                    if (drawItem.Client.CurrentServerInfo != null)
                    {

                        if (drawItem.Client.State == ConnectionState.Connected && drawItem.Client.IsLoggedIn == true)
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["tick-button.png"], e.Bounds.Left + 57, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }
                        else if (drawItem.Client.State == ConnectionState.Error)
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["cross-button.png"], e.Bounds.Left + 57, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }
                        else
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["exclamation-button.png"], e.Bounds.Left + 57, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }

                        e.Graphics.DrawString(String.Format("[{0}/{1}] {2} [{3}]", drawItem.Client.CurrentServerInfo.PlayerCount, drawItem.Client.CurrentServerInfo.MaxPlayerCount, drawItem.Client.CurrentServerInfo.ServerName, drawItem.Client.HostNamePort), this.cboServerList.Font, SystemBrushes.WindowText, e.Bounds.Left + 75, e.Bounds.Top);

                        if (drawItem.Client.Game != null)
                        {
                            if (drawItem.Client.CurrentServerInfo.GameMod == GameMods.None)
                            {
                                e.Graphics.DrawImage(this.iglGameIcons.Images[String.Format("{0}.png", drawItem.Client.Game.GameType).ToLower()], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglGameIcons.ImageSize.Width, this.iglGameIcons.ImageSize.Height);
                            }
                            else
                            {
                                e.Graphics.DrawImage(this.iglGameIcons.Images[String.Format("{0}.{1}.png", drawItem.Client.Game.GameType, drawItem.Client.CurrentServerInfo.GameMod).ToLower()], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglGameIcons.ImageSize.Width, this.iglGameIcons.ImageSize.Height);
                            }
                        }
                    }
                    else
                    {

                        if (drawItem.Client.State == ConnectionState.Connected && drawItem.Client.IsLoggedIn == true)
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["tick-button.png"], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }
                        else if (drawItem.Client.State == ConnectionState.Error)
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["cross-button.png"], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }
                        else
                        {
                            e.Graphics.DrawImage(this.iglIcons.Images["exclamation-button.png"], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);
                        }

                        if (drawItem.Client.ConnectionServerName != String.Empty)
                        {
                            e.Graphics.DrawString(String.Format("{0} [{1}]", drawItem.Client.ConnectionServerName, drawItem.Client.HostNamePort), this.cboServerList.Font, SystemBrushes.WindowText, e.Bounds.Left + 21, e.Bounds.Top);
                        }
                        else
                        {
                            e.Graphics.DrawString(drawItem.Client.HostNamePort, this.cboServerList.Font, SystemBrushes.WindowText, e.Bounds.Left + 21, e.Bounds.Top);
                        }
                    }

                }
                else if (this.cboServerList.ComboBox.Items[e.Index] is uscStartPage)
                {
                    uscStartPage drawItem = ((uscStartPage)this.cboServerList.ComboBox.Items[e.Index]);

                    e.Graphics.DrawImage(this.iglIcons.Images["home.png"], e.Bounds.Left + 2, e.Bounds.Top + 1, this.iglIcons.ImageSize.Width, this.iglIcons.ImageSize.Height);

                    if (this.m_clocLanguage == null)
                    {
                        e.Graphics.DrawString("Start Page", this.cboServerList.Font, SystemBrushes.WindowText, e.Bounds.Left + 21, e.Bounds.Top);
                    }
                    else
                    {
                        e.Graphics.DrawString(this.m_clocLanguage.GetLocalized("uscStartPage.Title"), this.cboServerList.Font, SystemBrushes.WindowText, e.Bounds.Left + 21, e.Bounds.Top);
                    }
                }

                e.Graphics.Dispose();
            }
        }

        private void btnShiftServerPrevious_Click(object sender, EventArgs e)
        {
            if (this.cboServerList.Items.Count > 0)
            {
                if (this.cboServerList.SelectedIndex - 1 < 0)
                {
                    this.cboServerList.SelectedIndex = this.cboServerList.Items.Count - 1;
                }
                else
                {
                    this.cboServerList.SelectedIndex--;
                }
            }
        }

        private void btnShiftServerNext_Click(object sender, EventArgs e)
        {
            if (this.cboServerList.Items.Count > 0)
            {
                if (this.cboServerList.SelectedIndex + 1 >= this.cboServerList.Items.Count)
                {
                    this.cboServerList.SelectedIndex = 0;
                }
                else
                {
                    this.cboServerList.SelectedIndex++;
                }
            }
        }

        private void btnConnectDisconnect_MouseEnter(object sender, EventArgs e)
        {
            if (this.cboServerList.SelectedItem != null && this.cboServerList.SelectedItem is uscServerConnection)
            {

                uscServerConnection selectedServer = (uscServerConnection)this.cboServerList.SelectedItem;

                this.btnConnectDisconnect.Enabled = true;
                if (selectedServer.Client.State == ConnectionState.Connected)
                {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-disconnect.png"];
                    if (this.m_clocLanguage != null)
                    {
                        this.btnConnectDisconnect.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnConnectDisconnect.Disconnect");
                    }
                }
                else
                {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-connect.png"];
                    if (this.m_clocLanguage != null)
                    {
                        this.btnConnectDisconnect.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnConnectDisconnect.Connect");
                    }
                }
            }
        }

        private void btnConnectDisconnect_MouseLeave(object sender, EventArgs e)
        {
            if (this.cboServerList.SelectedItem != null && this.cboServerList.SelectedItem is uscServerConnection)
            {

                uscServerConnection selectedServer = (uscServerConnection)this.cboServerList.SelectedItem;

                this.btnConnectDisconnect.Enabled = true;
                if (selectedServer.Client.State == ConnectionState.Connected)
                {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-connect.png"];
                    if (this.m_clocLanguage != null)
                    {
                        this.btnConnectDisconnect.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnConnectDisconnect.Connect");
                    }
                }
                else
                {
                    this.btnConnectDisconnect.Image = this.iglIcons.Images["plug-disconnect.png"];
                    if (this.m_clocLanguage != null)
                    {
                        this.btnConnectDisconnect.Text = this.m_clocLanguage.GetLocalized("fmrMain.tlsConnections.btnConnectDisconnect.Disconnect");
                    }
                }
            }
        }

        private void btnConnectDisconnect_Click(object sender, EventArgs e)
        {
            if (this.cboServerList.SelectedItem != null && this.cboServerList.SelectedItem is uscServerConnection)
            {

                uscServerConnection selectedServer = (uscServerConnection)this.cboServerList.SelectedItem;

                if (selectedServer.Client.State == ConnectionState.Connected)
                {
                    selectedServer.Client.ForceDisconnect();
                    selectedServer.Client.AutomaticallyConnect = false;
                }
                else
                {
                    selectedServer.Client.Connect();
                }
            }
        }

        private void RefreshServerListing()
        {
            if (this.IsDisposed == false)
            {
                this.cboServerList_SelectedIndexChanged(null, null);

                Point cursor = this.PointToClient(Cursor.Position);

                uscServerConnection item = this.cboServerList.SelectedItem as uscServerConnection;

                if (item != null)
                {
                    uscServerConnection selectedServer = item;

                    if (selectedServer.Client.State == ConnectionState.Connecting || (selectedServer.Client.State == ConnectionState.Connected && selectedServer.Client.IsLoggedIn == false))
                    {
                        if (this.btnConnectDisconnect.Bounds.Contains(cursor) == true)
                        {
                            this.btnConnectDisconnect_MouseEnter(null, null);
                        }
                    }
                }
            }
        }

        private void item_ConnectAttempt(PRoConClient sender)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_ConnectSuccess(PRoConClient sender)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_Login(PRoConClient sender)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_ConnectionFailure(PRoConClient sender, Exception exception)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_ConnectionClosed(PRoConClient sender)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_AutomaticallyConnectChanged(PRoConClient sender, bool isEnabled)
        {
            this.InvokeIfRequired(this.RefreshServerListing);
        }

        private void item_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                if (sender.Game != null)
                {
                    sender.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(Game_ServerInfo);
                }

                this.RefreshServerListing();
            });
        }

        private void Game_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.InvokeIfRequired(() =>
            {

                if (this.cboServerList.SelectedItem is uscServerConnection)
                {

                    if (((uscServerConnection)this.cboServerList.SelectedItem).Client.Game == sender)
                    {
                        this.cboServerList.ComboBox.Refresh();
                    }
                }
            });
        }

        private void m_frmConfirmation_ConfirmationSuccess()
        {
            if (this.cboServerList.SelectedItem != null)
            {
                uscServerConnection selectedServer = (uscServerConnection)this.cboServerList.SelectedItem;

                string strConfigFile = String.Format("{0}.cfg", selectedServer.Client.FileHostNamePort);

                selectedServer.Hide();

                // Find another panel to show while we remove this server.
                foreach (KeyValuePair<string, uscPage> kvpPanel in this.m_dicPages)
                {
                    if (kvpPanel.Value != selectedServer)
                    {
                        this.cboServerList.SelectedItem = kvpPanel.Value;
                        break;
                    }
                }

                //this.m_dicConnectionPages[strServerHostnamePort].BFBC2Connection.Destroying();
                if (this.cboServerList.ComboBox.Items.Contains(selectedServer) == true)
                {
                    this.cboServerList.ComboBox.Items.Remove(selectedServer);
                }

                this.m_paProcon.Connections.Remove(selectedServer.Client);
                selectedServer.Dispose();

                this.m_dicPages.Remove(selectedServer.Client.HostNamePort);
                //this.uscServerPlayerTreeviewListing.OnServerDeleted(strServerHostnamePort);

                try
                {
                    if (File.Exists(Path.Combine("Configs", strConfigFile)) == true)
                    {
                        File.Delete(Path.Combine("Configs", strConfigFile));
                    }
                }
                catch (Exception) { }
            }
        }

        private void chkAutomaticallyConnect_CheckedChanged(object sender, EventArgs e)
        {

            if (this.cboServerList.SelectedItem is uscServerConnection)
            {
                ((uscServerConnection)this.cboServerList.SelectedItem).Client.AutomaticallyConnect = this.chkAutomaticallyConnect.Checked;
            }

            if (this.chkAutomaticallyConnect.Checked == true)
            {
                this.chkAutomaticallyConnect.Image = this.iglIcons.Images["tick.png"];
            }
            else
            {
                this.chkAutomaticallyConnect.Image = null;
            }

        }

        private void btnStartPage_Click(object sender, EventArgs e)
        {

            foreach (uscPage page in this.cboServerList.Items)
            {
                if (page is uscStartPage)
                {
                    this.cboServerList.SelectedItem = page;
                }
            }

        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            this.cboServerList.Size = new Size(this.tlsConnections.Bounds.Width - this.toolsStripDropDownButton.Bounds.Width - this.cboServerList.Bounds.Left - 15, 23);
        }

        private void tlsConnections_SizeChanged(object sender, EventArgs e)
        {
            //this.cboServerList.Size = new Size(this.tlsConnections.Bounds.Width - this.toolsStripDropDownButton.Bounds.Width - this.cboServerList.Bounds.Left - 15, 23);
        }

    }
}
