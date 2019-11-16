using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PRoCon.Controls.ControlsEx;
using PRoCon.Controls.Data;
using PRoCon.Core;
using PRoCon.Core.Remote;
using PRoCon.Forms;

namespace PRoCon.Controls {
    public partial class uscListControlPanel : UserControl {

        private frmMain m_frmMain;
        private uscServerConnection m_uscConnectionPanel;

        private PRoConClient m_prcClient;

        private ListViewColumnSorter m_lvwReservedSlotsColumnSorter;
        private ListViewColumnSorter m_lvwSpectatorSlotsColumnSorter;
        private ListViewColumnSorter m_lvwBanlistColumnSorter;
        private Font m_fntComboBoxSelectedFont;

        private Dictionary<string, AsyncStyleSetting> m_dicAsyncSettingControls;
        //private Dictionary<string, string> m_dicFriendlyPlaylistNames; // strPlaylist, strGamemode
        //private Dictionary<string, Dictionary<string, string>> m_dicMaplistsPerPlaylist; // strPlaylist, Dictionary<strLevel, strPublicLevelName>>

        //public delegate void SendCommandDelegate(List<string> lstCommand);
        //public event SendCommandDelegate SendCommand;

       // private int m_iReselectShufflingMapIndex = 0;

        private bool m_blSettingAppendingReservedPlayer;
        private bool m_blSettingRemovingReservedPlayer;
        
        private bool m_blSettingAppendingSpectatorPlayer;
        private bool m_blSettingRemovingSpectatorPlayer;
        
        //private bool m_blSettingAppendingSingleMap;
        //private bool m_blSettingNewPlaylist;

        private string[] ma_strTimeDescriptionsShort;
        private string[] ma_strTimeDescriptionsLong;

        private CPrivileges m_spPrivileges;

        private Regex m_regIP = null;
        private Regex m_regPbGUID = null;
        private Regex m_regBc2GUID = null;

        /// <summary>
        /// A filterable/pagable source of bans
        /// </summary>
        protected ISource BansSource { get; set; }

        /*
        public List<string> SetListsSettings {
            set {
                bool blChecked = true;

                if (value.Count >= 1 && bool.TryParse(value[0], out blChecked) == true) {
                    this.spltBanlistManualBans.Panel2Collapsed = !blChecked;
                    this.picCloseOpenManualBans_Click(null, null);
                }
            }
        }

        public string ListsSettings {
            get {
                return String.Format("{0}", this.spltBanlistManualBans.Panel2Collapsed);
            }
        }
        */
        public uscListControlPanel() {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true); 

            this.m_regIP = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
            this.m_regPbGUID = new Regex("^[A-Fa-f0-9]{32}$", RegexOptions.Compiled);
            this.m_regBc2GUID = new Regex("^(?:E|e)(?:A|a)_[A-Fa-f0-9]{32}$", RegexOptions.Compiled);
            
            this.m_lvwReservedSlotsColumnSorter = new ListViewColumnSorter();
            this.lsvReservedList.ListViewItemSorter = this.m_lvwReservedSlotsColumnSorter;

            this.m_lvwSpectatorSlotsColumnSorter = new ListViewColumnSorter();
            this.lsvSpectatorList.ListViewItemSorter = this.m_lvwSpectatorSlotsColumnSorter;
            
            this.m_lvwBanlistColumnSorter = new ListViewColumnSorter();
            this.lsvBanlist.ListViewItemSorter = this.m_lvwBanlistColumnSorter;

            this.pnlReservedPanel.Dock = DockStyle.Fill;
            this.pnlSpectatorPanel.Dock = DockStyle.Fill;

            this.m_fntComboBoxSelectedFont = new Font("Segoe UI", 10, FontStyle.Bold);

            this.m_blSettingAppendingReservedPlayer = false;
            this.m_blSettingRemovingReservedPlayer = false;

            this.m_blSettingAppendingSpectatorPlayer = false;
            this.m_blSettingRemovingSpectatorPlayer = false;

            this.m_dicAsyncSettingControls = new Dictionary<string, AsyncStyleSetting>();
            
            // Reservedlist updates
            this.m_dicAsyncSettingControls.Add("local.reservedlist.list", new AsyncStyleSetting(this.picReservedList, this.lsvReservedList, new Control[] { this.btnReservedSlotsListRefresh }, true)); 
            this.m_dicAsyncSettingControls.Add("local.reservedlist.append", new AsyncStyleSetting(this.picReservedAddSoldierName, this.lsvReservedList, new Control[] { this.lblReservedAddSoldierName, this.txtReservedAddSoldierName, this.lnkReservedAddSoldierName }, true));
            this.m_dicAsyncSettingControls.Add("local.reservedlist.remove", new AsyncStyleSetting(this.picReservedList, this.lsvReservedList, new Control[] { this.btnReservedRemoveSoldier }, true));

            // SpectatorList updates
            this.m_dicAsyncSettingControls.Add("local.spectatorlist.list", new AsyncStyleSetting(this.picSpectatorList, this.lsvSpectatorList, new Control[] { this.btnSpectatorSlotsListRefresh }, true));
            this.m_dicAsyncSettingControls.Add("local.spectatorlist.append", new AsyncStyleSetting(this.picSpectatorAddSoldierName, this.lsvSpectatorList, new Control[] { this.lblSpectatorAddSoldierName, this.txtSpectatorAddSoldierName, this.lnkSpectatorAddSoldierName }, true));
            this.m_dicAsyncSettingControls.Add("local.spectatorlist.remove", new AsyncStyleSetting(this.picSpectatorList, this.lsvSpectatorList, new Control[] { this.btnSpectatorRemoveSoldier }, true));

            this.m_dicAsyncSettingControls.Add("local.banlist.clearlist", new AsyncStyleSetting(this.picClearLists, this.btnBanlistClearBanlist, new Control[] { this.btnBanlistClearBanlist }, true));
            this.m_dicAsyncSettingControls.Add("local.banlist.unban", new AsyncStyleSetting(this.picUnbanPlayer, this.btnBanlistUnban, new Control[] { this.btnBanlistUnban }, true));

            this.m_dicAsyncSettingControls.Add("local.banlist.banning", new AsyncStyleSetting(this.picBanlistManualBanOkay, this.btnBanlistAddBan, new Control[] { this.btnBanlistAddBan }, false));

            this.ma_strTimeDescriptionsShort = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };
            this.ma_strTimeDescriptionsLong = new string[] { " year ", " years ", " month ", " months ", " week ", " weeks ", " day ", " days ", " hour ", " hours ", " minute ", " minutes ", " second", " seconds" };
            this.cboBanlistTimeMultiplier.SelectedIndex = 0;

            this.m_spPrivileges = new CPrivileges(CPrivileges.FullPrivilegesFlags);

            this.BansSource = new BansSource {
                Take = this.pagination1.ItemsPerPage = 30
            };
            this.pagination1.Source = this.BansSource;
            this.BansSource.Changed += BansSourceOnChanged;
        }

        public void m_prcClient_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs) {
            this.InvokeIfRequired(() => {
                this.m_spPrivileges = spPrivs;

                this.m_dicAsyncSettingControls["local.reservedlist.append"].m_blReEnableControls = this.m_spPrivileges.CanEditReservedSlotsList;
                this.m_dicAsyncSettingControls["local.reservedlist.remove"].m_blReEnableControls = this.m_spPrivileges.CanEditReservedSlotsList;
                if (this.lsvReservedList.SelectedItems.Count > 0) {
                    this.btnReservedRemoveSoldier.Enabled = this.m_spPrivileges.CanEditReservedSlotsList;
                } // ELSE It'll already be disabled 
                this.lnkReservedAddSoldierName.Enabled = this.m_spPrivileges.CanEditReservedSlotsList;

                this.m_dicAsyncSettingControls["local.spectatorlist.append"].m_blReEnableControls = this.m_spPrivileges.CanEditReservedSlotsList;
                this.m_dicAsyncSettingControls["local.spectatorlist.remove"].m_blReEnableControls = this.m_spPrivileges.CanEditReservedSlotsList;
                if (this.lsvSpectatorList.SelectedItems.Count > 0) {
                    this.btnSpectatorRemoveSoldier.Enabled = this.m_spPrivileges.CanEditReservedSlotsList;
                } // ELSE It'll already be disabled 
                this.lnkSpectatorAddSoldierName.Enabled = this.m_spPrivileges.CanEditReservedSlotsList;

                this.m_dicAsyncSettingControls["local.banlist.clearlist"].m_blReEnableControls = this.m_spPrivileges.CanEditBanList;
                this.m_dicAsyncSettingControls["local.banlist.unban"].m_blReEnableControls = this.m_spPrivileges.CanEditBanList;
                this.btnBanlistClearBanlist.Enabled = this.m_spPrivileges.CanEditBanList;

                if (this.lsvBanlist.SelectedItems.Count > 0) {
                    this.btnBanlistUnban.Enabled = this.m_spPrivileges.CanEditBanList;
                } // ELSE It'll already be disabled 

                // Manual banning..
                this.rdoBanlistPbGUID.Enabled = this.rdoBanlistPermanent.Enabled = this.m_spPrivileges.CanPermanentlyBanPlayers;

                if (this.rdoBanlistPbGUID.Checked == true && this.rdoBanlistPbGUID.Enabled == false) {
                    this.rdoBanlistName.Checked = true;
                }

                if (this.rdoBanlistPermanent.Checked == true && this.rdoBanlistPermanent.Enabled == false) {
                    this.rdoBanlistTemporary.Checked = true;
                }

                this.spltBanlistManualBans.Panel2.Enabled = this.m_spPrivileges.CanTemporaryBanPlayers;
            });
        }

        public void Initialize(frmMain frmMainWindow, uscServerConnection uscConnectionPanel) {
            this.m_frmMain = frmMainWindow;
            this.m_uscConnectionPanel = uscConnectionPanel;

            this.uscMaplist1.Initialize(frmMainWindow, uscConnectionPanel);

            this.tbcLists.ImageList = this.m_frmMain.iglIcons;

            this.tabBanlist.ImageKey = "mouse_ban.png";
            this.tabMaplist.ImageKey = "world.png";
            this.tabReservedSlots.ImageKey = "user.png";
            this.tabSpectatorSlots.ImageKey = "user.png";

            this.btnBanlistRefresh.ImageList = this.m_frmMain.iglIcons;
            this.btnBanlistRefresh.ImageKey = "arrow_refresh.png";

            this.picBansFilterMagnifier.Image = this.m_frmMain.iglIcons.Images["magnifier.png"];

            this.btnReservedRemoveSoldier.ImageList = this.m_frmMain.iglIcons;
            this.btnReservedRemoveSoldier.ImageKey = "cross.png";

            this.btnReservedSlotsListRefresh.ImageList = this.m_frmMain.iglIcons;
            this.btnReservedSlotsListRefresh.ImageKey = "arrow_refresh.png";

            this.btnSpectatorRemoveSoldier.ImageList = this.m_frmMain.iglIcons;
            this.btnSpectatorRemoveSoldier.ImageKey = "cross.png";

            this.btnSpectatorSlotsListRefresh.ImageList = this.m_frmMain.iglIcons;
            this.btnSpectatorSlotsListRefresh.ImageKey = "arrow_refresh.png";

            this.picCloseOpenManualBans.Image = this.m_frmMain.iglIcons.Images["arrow_down.png"];

            this.picBanlistIPError.Image = this.picBanlistGUIDError.Image = this.m_frmMain.iglIcons.Images["cross.png"];
            //this.picBanlistManualBanOkay.Image = this.m_frmMain.iglIcons.Images["tick.png"];

            this.copyToolStripMenuItem.Image = this.m_frmMain.iglIcons.Images["page_copy.png"];

            this.uscTextChatModerationListcs1.Initialize(frmMainWindow);
        }

        private void uscListControlPanel_Load(object sender, EventArgs e) {
            if (this.m_prcClient != null) {
                //this.RefreshLocalMaplist();

                if (this.m_prcClient.GameType == "BF3" || this.m_prcClient.GameType == "BF4" || this.m_prcClient.GameType == "MOHW")
                {
                    this.SendCommand("mapList.list");
                }
                else
                {
                    this.SendCommand("mapList.list", "rounds");
                }

                if (this.m_prcClient.GameType == "BF3" || this.m_prcClient.GameType == "MOHW") {
                    this.tabSpectatorSlots.Enabled = false;
                }
                
                if (this.m_prcClient.GameType == "MOH") {
                    this.tabReservedSlots.Enabled = false;
                    this.tabSpectatorSlots.Enabled = false;
                    this.lblMohNotice.Visible = true;
                }
            }
        }

        public void SetConnection(PRoConClient prcClient) {
            if ((this.m_prcClient = prcClient) != null) {
                if (this.m_prcClient.Game != null) {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else {
                    this.m_prcClient.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }

                this.uscMaplist1.SetConnection(prcClient);
                this.uscTextChatModerationListcs1.SetConnection(prcClient);
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender) {
            this.InvokeIfRequired(() => {
                this.m_prcClient.Game.ReservedSlotsSave += new FrostbiteClient.EmptyParamterHandler(this.OnReservedSlotsSave);
                this.m_prcClient.Game.ReservedSlotsList += new FrostbiteClient.ReservedSlotsListHandler(this.OnReservedSlotsList);
                this.m_prcClient.Game.ReservedSlotsPlayerAdded += new FrostbiteClient.ReservedSlotsPlayerHandler(this.OnReservedSlotsPlayerAdded);
                this.m_prcClient.Game.ReservedSlotsPlayerRemoved += new FrostbiteClient.ReservedSlotsPlayerHandler(this.OnReservedSlotsPlayerRemoved);

                this.m_prcClient.Game.SpectatorListSave += new FrostbiteClient.EmptyParamterHandler(this.OnSpectatorSlotsSave);
                this.m_prcClient.Game.SpectatorListList += new FrostbiteClient.SpectatorListListHandler(this.OnSpectatorSlotsList);
                this.m_prcClient.Game.SpectatorListPlayerAdded += new FrostbiteClient.SpectatorListPlayerHandler(this.OnSpectatorSlotsPlayerAdded);
                this.m_prcClient.Game.SpectatorListPlayerRemoved += new FrostbiteClient.SpectatorListPlayerHandler(this.OnSpectatorSlotsPlayerRemoved);

                this.m_prcClient.Game.BanListClear += new FrostbiteClient.EmptyParamterHandler(this.OnClearBanList);
                this.m_prcClient.FullBanListList += new PRoConClient.FullBanListListHandler(this.OnBanList);
                this.m_prcClient.PunkbusterPlayerUnbanned += new PRoConClient.PunkbusterBanHandler(m_prcClient_PunkbusterPlayerUnbanned);
                this.m_prcClient.Game.BanListRemove += new FrostbiteClient.BanListRemoveHandler(this.OnUnban);
                this.m_prcClient.PunkbusterPlayerBanned += new PRoConClient.PunkbusterBanHandler(this.OnPbGuidBan);
                this.m_prcClient.Game.BanListAdd += new FrostbiteClient.BanListAddHandler(Game_BanListAdd);

                this.m_prcClient.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_prcClient_ProconPrivileges);

                //this.m_prcClient.Reasons.ItemRemoved += new NotificationList<string>.ItemModifiedHandler(Reasons_ItemRemoved);
                this.m_prcClient.Reasons.ItemAdded += new NotificationList<string>.ItemModifiedHandler(Reasons_ItemAdded);

                this.m_prcClient.ListSettings.ManualBansVisibleChange += new PRoCon.Core.Lists.ListsSettings.ManualBansVisibleChangeHandler(ListSettings_ManualBansVisibleChange);

                this.m_prcClient.ListSettings.ManualBansVisible = this.m_prcClient.ListSettings.ManualBansVisible;

                this.cboBanlistReason.Items.Clear();
                foreach (string strReason in this.m_prcClient.Reasons) {
                    this.Reasons_ItemAdded(0, strReason);
                }

                if (this.m_prcClient.FullVanillaBanList != null) {
                    this.OnBanList(this.m_prcClient, this.m_prcClient.FullVanillaBanList);
                }

                if (this.m_prcClient.ReservedSlotList != null) {
                    this.OnReservedSlotsList(this.m_prcClient.Game, new List<string>(this.m_prcClient.ReservedSlotList));
                }

                if (this.m_prcClient.SpectatorList != null) {
                    this.OnSpectatorSlotsList(this.m_prcClient.Game, new List<string>(this.m_prcClient.SpectatorList));
                }

                if (sender.Game is BFHLClient || sender.Game is BF4Client || sender.Game is BF3Client || sender.Game is MOHWClient) {
                    this.tbcLists.TabPages.Remove(this.tabTextChatModeration);
                }

                if (!(sender.Game is BFHLClient) && !(sender.Game is BF4Client)) {
                    this.tbcLists.TabPages.Remove(this.tabSpectatorSlots);
                }
            });
        }

        public void SetLocalization(CLocalization clocLanguage) {
            //this.m_prcClient.Language = clocLanguage;

            // private string[] m_astrTimeDescriptionsShort = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };
            this.ma_strTimeDescriptionsShort[13] = clocLanguage.GetLocalized("global.Seconds.Short", null);
            this.ma_strTimeDescriptionsShort[12] = clocLanguage.GetLocalized("global.Seconds.Short", null);
            this.ma_strTimeDescriptionsShort[11] = clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[10] = clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[9] = clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[8] = clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[7] = clocLanguage.GetLocalized("global.Days.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[6] = clocLanguage.GetLocalized("global.Days.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[5] = clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[4] = clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[3] = clocLanguage.GetLocalized("global.Months.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[2] = clocLanguage.GetLocalized("global.Months.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[1] = clocLanguage.GetLocalized("global.Years.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[0] = clocLanguage.GetLocalized("global.Years.Short", null) + " ";

            this.ma_strTimeDescriptionsLong[13] = " " + clocLanguage.GetLocalized("global.Seconds.Plural", null).ToLower();
            this.ma_strTimeDescriptionsLong[12] = " " + clocLanguage.GetLocalized("global.Seconds.Singular", null).ToLower();
            this.ma_strTimeDescriptionsLong[11] = " " + clocLanguage.GetLocalized("global.Minutes.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[10] = " " + clocLanguage.GetLocalized("global.Minutes.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[9] = " " + clocLanguage.GetLocalized("global.Hours.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[8] = " " + clocLanguage.GetLocalized("global.Hours.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[7] = " " + clocLanguage.GetLocalized("global.Days.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[6] = " " + clocLanguage.GetLocalized("global.Days.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[5] = " " + clocLanguage.GetLocalized("global.Weeks.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[4] = " " + clocLanguage.GetLocalized("global.Weeks.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[3] = " " + clocLanguage.GetLocalized("global.Months.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[2] = " " + clocLanguage.GetLocalized("global.Months.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[1] = " " + clocLanguage.GetLocalized("global.Years.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[0] = " " + clocLanguage.GetLocalized("global.Years.Singular", null).ToLower() + " ";

            this.cboBanlistTimeMultiplier.Items[0] = clocLanguage.GetLocalized("global.Minutes.Plural", null);
            this.cboBanlistTimeMultiplier.Items[1] = clocLanguage.GetLocalized("global.Hours.Plural", null);
            this.cboBanlistTimeMultiplier.Items[2] = clocLanguage.GetLocalized("global.Days.Plural", null);
            this.cboBanlistTimeMultiplier.Items[3] = clocLanguage.GetLocalized("global.Weeks.Plural", null);
            this.cboBanlistTimeMultiplier.Items[4] = clocLanguage.GetLocalized("global.Months.Plural", null);

            this.tabBanlist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist", null);
            this.colName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colName", null);
            this.colIP.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colIP", null);
            this.colGUID.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colGUID", null);
            this.colType.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colType", null);
            //this.colTime.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colTime", null);
            //this.colBanLength.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colBanLength", null);
            this.colTimeRemaining.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colTimeRemaining", null);
            this.colReason.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colReason", null);
            this.btnBanlistClearBanlist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistClearBanlist", null);
            //this.btnBanlistClearBanlist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistClearNameList", null);
            //this.btnBanlistClearIPList.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistClearIPList", null);
            this.btnBanlistUnban.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistUnban", null);

            this.tabMaplist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist");

            this.tabReservedSlots.Text = clocLanguage.GetLocalized("uscListControlPanel.tabReservedSlots", null);
            this.lblReservedCurrent.Text = clocLanguage.GetLocalized("uscListControlPanel.tabReservedSlots.lblReservedCurrent", null);
            this.colSoldierNames.Text = clocLanguage.GetLocalized("uscListControlPanel.tabReservedSlots.lsvReservedList.colSoldierNames", null);
            this.lblReservedAddSoldierName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabReservedSlots.lblReservedAddSoldierName", null);
            this.lnkReservedAddSoldierName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabReservedSlots.lnkReservedAddSoldierName", null);

            this.tabSpectatorSlots.Text = clocLanguage.GetLocalized("uscListControlPanel.tabSpectatorSlots", null);
            this.lblSpectatorCurrent.Text = clocLanguage.GetLocalized("uscListControlPanel.tabSpectatorSlots.lblSpectatorCurrent", null);
            this.colSpectatorSoldierNames.Text = clocLanguage.GetLocalized("uscListControlPanel.tabSpectatorSlots.lsvSpectatorList.colSpectatorSoldierNames", null);
            this.lblSpectatorAddSoldierName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabSpectatorSlots.lblSpectatorAddSoldierName", null);
            this.lnkSpectatorAddSoldierName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabSpectatorSlots.lnkSpectatorAddSoldierName", null);

            this.lnkCloseOpenManualBans.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lnkCloseOpenManualBans.Close", null);
            this.rdoBanlistName.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.rdoBanlistName", null);
            //this.rdoBanlistPbGUID.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.rdoBanlistGUID", null);
            this.lblBanlistReason.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lblBanlistReason", null) + ":";
            this.rdoBanlistPermanent.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.rdoBanlistPermanent", null);
            this.rdoBanlistTemporary.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.rdoBanlistTemporary", null);
            this.lblBanlistTime.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.lblBanlistTime", null) + ":";
            this.btnBanlistAddBan.Text = clocLanguage.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistAddBan", null);

            this.uscMaplist1.SetLocalization(clocLanguage);

            this.tabTextChatModeration.Text = clocLanguage.GetLocalized("uscTextChatModerationList.Title", null);
            this.uscTextChatModerationListcs1.SetLocalization(clocLanguage);
        }

        //public delegate void OnTabChangeDelegate(object sender, Stack<string> stkTabIndexes);
        public event uscServerConnection.OnTabChangeDelegate OnTabChange;

        private void tbcLists_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.OnTabChange != null) {
                Stack<string> stkTabIndexes = new Stack<string>();
                stkTabIndexes.Push(tbcLists.SelectedTab.Name);

                this.OnTabChange(this, stkTabIndexes);
            }
        }

        public void SetTabIndexes(Stack<string> stkTabIndexes) {
            if (tbcLists.TabPages.ContainsKey(stkTabIndexes.Peek()) == true) {
                this.tbcLists.SelectedTab = tbcLists.TabPages[stkTabIndexes.Pop()];
            }
        }

        private void SendCommand(params string[] a_strCommand) {
            if (this.m_prcClient != null) {
                this.m_prcClient.SendRequest(new List<string>(a_strCommand));
            }
        }

        #region Settings Animator

        private void SetControlValue(Control ctrlTarget, object objValue) {

            if (objValue != null) {
                if (ctrlTarget is TextBox) {
                    ((TextBox)ctrlTarget).Text = (string)objValue;
                }
                else if (ctrlTarget is CheckBox) {
                    ((CheckBox)ctrlTarget).Checked = (bool)objValue;
                }
                else if (ctrlTarget is NumericUpDown) {
                    ((NumericUpDown)ctrlTarget).Value = (decimal)objValue;
                }
                else if (ctrlTarget is Label) {
                    ((Label)ctrlTarget).Text = (string)objValue;
                }
            }
        }

        private void WaitForSettingResponse(string strResponseCommand) {

            if (this.m_dicAsyncSettingControls.ContainsKey(strResponseCommand) == true) {
                //this.m_dicAsyncSettingControls[strResponseCommand].m_objOriginalValue = String.Empty;
                this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleLoading.Image;
                this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_TIMEOUT_TICKS;

                this.tmrTimeoutCheck.Enabled = true;


                foreach (Control ctrlEnable in this.m_dicAsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs) {
                    if (ctrlEnable is TextBox) {
                        ((TextBox)ctrlEnable).ReadOnly = true;
                    }
                    else {
                        ctrlEnable.Enabled = false;
                    }
                }
            }
        }

        public void OnSettingResponse(string strResponseCommand, bool blSuccess) {

            if (this.m_dicAsyncSettingControls.ContainsKey(strResponseCommand) == true) {

                if (this.m_dicAsyncSettingControls[strResponseCommand].m_blReEnableControls == true) {
                    foreach (Control ctrlEnable in this.m_dicAsyncSettingControls[strResponseCommand].ma_ctrlEnabledInputs) {
                        if (ctrlEnable is TextBox) {
                            ((TextBox)ctrlEnable).ReadOnly = false;
                        }
                        else {
                            ctrlEnable.Enabled = true;
                        }
                    }
                }

                this.m_dicAsyncSettingControls[strResponseCommand].IgnoreEvent = true;

                if (blSuccess == true) {
                    this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleSuccess.Image;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_blSuccess = true;
                }
                else {
                    this.m_dicAsyncSettingControls[strResponseCommand].m_picStatus.Image = this.m_frmMain.picAjaxStyleFail.Image;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;
                    this.m_dicAsyncSettingControls[strResponseCommand].m_blSuccess = false;
                }

                this.tmrTimeoutCheck.Enabled = true;

                this.m_dicAsyncSettingControls[strResponseCommand].IgnoreEvent = false;
            }
        }


        private int CountTicking() {
            int i = 0;

            foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsync in this.m_dicAsyncSettingControls) {
                if (kvpAsync.Value.m_iTimeout >= 0) {
                    i++;
                }
            }

            return i;
        }

        private void tmrSettingsAnimator_Tick(object sender, EventArgs e) {
            //if (((from o in this.m_dicAsyncSettingControls where o.Value.m_iTimeout >= 0 select o).Count()) > 0) {
            if (this.CountTicking() > 0) {
                foreach (KeyValuePair<string, AsyncStyleSetting> kvpAsyncSetting in this.m_dicAsyncSettingControls) {

                    kvpAsyncSetting.Value.m_iTimeout--;
                    if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == false) {
                        kvpAsyncSetting.Value.m_picStatus.Image = this.m_frmMain.picAjaxStyleFail.Image;
                        kvpAsyncSetting.Value.m_iTimeout = AsyncStyleSetting.INT_ANIMATEDSETTING_SHOWRESULT_TICKS;

                        kvpAsyncSetting.Value.m_blSuccess = true;
                    }
                    else if (kvpAsyncSetting.Value.m_iTimeout == 0 && kvpAsyncSetting.Value.m_blSuccess == true) {
                        kvpAsyncSetting.Value.m_picStatus.Image = null;

                        if (kvpAsyncSetting.Value.m_blReEnableControls == true) {
                            foreach (Control ctrlEnable in kvpAsyncSetting.Value.ma_ctrlEnabledInputs) {
                                if (ctrlEnable is TextBox) {
                                    ((TextBox)ctrlEnable).ReadOnly = false;
                                }
                                else {
                                    ctrlEnable.Enabled = true;
                                }
                            }
                        }
                    }
                }
            }
            else {
                this.tmrTimeoutCheck.Enabled = false;
            }
        }

        #endregion

        #region Reserved Slots

        public void OnReservedSlotsList(FrostbiteClient sender, List<string> lstSoldierNames) {
            this.InvokeIfRequired(() => {
                this.lsvReservedList.BeginUpdate();
                this.lsvReservedList.Items.Clear();
                foreach (string strSoldierName in lstSoldierNames) {
                    if (this.lsvReservedList.Items.ContainsKey(strSoldierName) == false) {

                        ListViewItem lsvNewSoldier = new ListViewItem(strSoldierName);
                        lsvNewSoldier.Name = strSoldierName;

                        this.lsvReservedList.Items.Add(lsvNewSoldier);
                    }
                }
                this.lsvReservedList.EndUpdate();
            });
        }

        public void OnReservedSlotsPlayerRemoved(FrostbiteClient sender, string strSoldierName) {
            this.InvokeIfRequired(() => {
                if (this.lsvReservedList.Items.ContainsKey(strSoldierName) == true) {
                    this.lsvReservedList.Items.RemoveByKey(strSoldierName);
                }
            });
        }

        public void OnReservedSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName) {
            this.InvokeIfRequired(() => {
                if (this.lsvReservedList.Items.ContainsKey(strSoldierName) == false) {

                    ListViewItem lsvNewSoldier = new ListViewItem(strSoldierName);
                    lsvNewSoldier.Name = strSoldierName;

                    this.lsvReservedList.Items.Add(lsvNewSoldier);
                }
            });
        }

        public void OnReservedSlotsSave(FrostbiteClient sender) {
            this.InvokeIfRequired(() => {
                if (this.m_blSettingAppendingReservedPlayer == true) {
                    this.OnSettingResponse("local.reservedlist.append", true);
                    this.m_blSettingAppendingReservedPlayer = false;
                }
                else if (this.m_blSettingRemovingReservedPlayer == true) {
                    this.OnSettingResponse("local.reservedlist.remove", true);
                    this.m_blSettingRemovingReservedPlayer = false;
                }
            });
        }

        private void lnkReservedAddSoldierName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {

            if (this.txtReservedAddSoldierName.Text.Length > 0) {
                this.m_blSettingAppendingReservedPlayer = true;
                this.WaitForSettingResponse("local.reservedlist.append");

                this.m_prcClient.Game.SendReservedSlotsAddPlayerPacket(this.txtReservedAddSoldierName.Text);
                //this.SendCommand("reservedSlots.addPlayer", );

                this.m_prcClient.Game.SendReservedSlotsSavePacket();
                //this.SendCommand("reservedSlots.save");

                this.txtReservedAddSoldierName.Clear();
                this.txtReservedAddSoldierName.Focus();
            }
        }

        private void lsvReservedList_SelectedIndexChanged(object sender, EventArgs e) {

            if (this.lsvReservedList.SelectedItems.Count > 0 && this.lsvReservedList.FocusedItem != null) {
                this.btnReservedRemoveSoldier.Enabled = true && this.m_spPrivileges.CanEditReservedSlotsList;
            }

        }

        private void btnReservedRemoveSoldier_Click(object sender, EventArgs e) {

            if (this.lsvReservedList.SelectedItems.Count > 0) {

                this.m_blSettingRemovingReservedPlayer = true;
                this.WaitForSettingResponse("local.reservedlist.remove");

                this.m_prcClient.Game.SendReservedSlotsRemovePlayerPacket(this.lsvReservedList.SelectedItems[0].Name);
                //this.SendCommand("reservedSlots.removePlayer", this.lsvReservedList.SelectedItems[0].Name);

                this.m_prcClient.Game.SendReservedSlotsSavePacket();
                //this.SendCommand("reservedSlots.save");
            }
        }

        private void txtReservedAddSoldierName_TextChanged(object sender, EventArgs e) {

            if (this.txtReservedAddSoldierName.Text.Length > 0) {
                this.lnkReservedAddSoldierName.Enabled = true && this.m_spPrivileges.CanEditReservedSlotsList;
            }
            else {
                this.lnkReservedAddSoldierName.Enabled = false;
            }
        }

        private void txtReservedAddSoldierName_KeyDown(object sender, KeyEventArgs e) {
            if (this.txtReservedAddSoldierName.Text.Length > 0 && e.KeyData == Keys.Enter) {
                this.lnkReservedAddSoldierName_LinkClicked(this, null);
                e.SuppressKeyPress = true;
            }
        }

        private void lsvReservedList_ColumnClick(object sender, ColumnClickEventArgs e) {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.m_lvwReservedSlotsColumnSorter.SortColumn) {
                // Reverse the current sort direction for this column.
                if (this.m_lvwReservedSlotsColumnSorter.Order == SortOrder.Ascending) {
                    this.m_lvwReservedSlotsColumnSorter.Order = SortOrder.Descending;
                }
                else {
                    this.m_lvwReservedSlotsColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else {
                // Set the column number that is to be sorted; default to ascending.
                this.m_lvwReservedSlotsColumnSorter.SortColumn = e.Column;
                this.m_lvwReservedSlotsColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvReservedList.Sort();
        }

        private void btnReservedSlotsListRefresh_Click(object sender, EventArgs e)
        {
            this.m_prcClient.Game.SendReservedSlotsLoadPacket();
            this.m_prcClient.Game.SendReservedSlotsListPacket();
        }

        #endregion

        #region Spectator Slots

        public void OnSpectatorSlotsList(FrostbiteClient sender, List<string> lstSoldierNames) {
            this.InvokeIfRequired(() => {
                this.lsvSpectatorList.BeginUpdate();
                this.lsvSpectatorList.Items.Clear();
                foreach (string strSoldierName in lstSoldierNames) {
                    if (this.lsvSpectatorList.Items.ContainsKey(strSoldierName) == false) {

                        ListViewItem lsvNewSoldier = new ListViewItem(strSoldierName);
                        lsvNewSoldier.Name = strSoldierName;

                        this.lsvSpectatorList.Items.Add(lsvNewSoldier);
                    }
                }
                this.lsvSpectatorList.EndUpdate();
            });
        }

        public void OnSpectatorSlotsPlayerRemoved(FrostbiteClient sender, string strSoldierName) {
            this.InvokeIfRequired(() => {
                if (this.lsvSpectatorList.Items.ContainsKey(strSoldierName) == true) {
                    this.lsvSpectatorList.Items.RemoveByKey(strSoldierName);
                }
            });
        }

        public void OnSpectatorSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName) {
            this.InvokeIfRequired(() => {
                if (this.lsvSpectatorList.Items.ContainsKey(strSoldierName) == false) {

                    ListViewItem lsvNewSoldier = new ListViewItem(strSoldierName);
                    lsvNewSoldier.Name = strSoldierName;

                    this.lsvSpectatorList.Items.Add(lsvNewSoldier);
                }
            });
        }

        public void OnSpectatorSlotsSave(FrostbiteClient sender) {
            this.InvokeIfRequired(() => {
                if (this.m_blSettingAppendingSpectatorPlayer == true) {
                    this.OnSettingResponse("local.spectatorlist.append", true);
                    this.m_blSettingAppendingSpectatorPlayer = false;
                }
                else if (this.m_blSettingRemovingSpectatorPlayer == true) {
                    this.OnSettingResponse("local.spectatorlist.remove", true);
                    this.m_blSettingRemovingSpectatorPlayer = false;
                }
            });
        }

        private void lnkSpectatorAddSoldierName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this.txtSpectatorAddSoldierName.Text.Length > 0)
            {
                this.m_blSettingAppendingSpectatorPlayer = true;
                this.WaitForSettingResponse("local.spectatorlist.append");

                this.m_prcClient.Game.SendSpectatorListAddPlayerPacket(this.txtSpectatorAddSoldierName.Text);
                //this.SendCommand("spectatorList.add", );

                this.m_prcClient.Game.SendSpectatorListSavePacket();
                //this.SendCommand("spectatorList.save");

                this.txtSpectatorAddSoldierName.Clear();
                this.txtSpectatorAddSoldierName.Focus();
            }
        }

        private void lsvSpectatorList_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.lsvSpectatorList.SelectedItems.Count > 0 && this.lsvSpectatorList.FocusedItem != null)
            {
                this.btnSpectatorRemoveSoldier.Enabled = true && this.m_spPrivileges.CanEditReservedSlotsList;
            }

        }

        private void btnSpectatorRemoveSoldier_Click(object sender, EventArgs e)
        {

            if (this.lsvSpectatorList.SelectedItems.Count > 0)
            {

                this.m_blSettingRemovingSpectatorPlayer = true;
                this.WaitForSettingResponse("local.spectatorlist.remove");

                this.m_prcClient.Game.SendSpectatorListRemovePlayerPacket(this.lsvSpectatorList.SelectedItems[0].Name);
                //this.SendCommand("spectatorList.remove", this.lsvSpectatorList.SelectedItems[0].Name);

                this.m_prcClient.Game.SendSpectatorListSavePacket();
                //this.SendCommand("spectatorList.save");
            }
        }

        private void txtSpectatorAddSoldierName_TextChanged(object sender, EventArgs e)
        {

            if (this.txtSpectatorAddSoldierName.Text.Length > 0)
            {
                this.lnkSpectatorAddSoldierName.Enabled = true && this.m_spPrivileges.CanEditReservedSlotsList;
            }
            else
            {
                this.lnkSpectatorAddSoldierName.Enabled = false;
            }
        }

        private void txtSpectatorAddSoldierName_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.txtSpectatorAddSoldierName.Text.Length > 0 && e.KeyData == Keys.Enter)
            {
                this.lnkSpectatorAddSoldierName_LinkClicked(this, null);
                e.SuppressKeyPress = true;
            }
        }

        private void lsvSpectatorList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.m_lvwSpectatorSlotsColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.m_lvwSpectatorSlotsColumnSorter.Order == SortOrder.Ascending)
                {
                    this.m_lvwSpectatorSlotsColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    this.m_lvwSpectatorSlotsColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.m_lvwSpectatorSlotsColumnSorter.SortColumn = e.Column;
                this.m_lvwSpectatorSlotsColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvSpectatorList.Sort();
        }

        private void btnSpectatorSlotsListRefresh_Click(object sender, EventArgs e)
        {
            this.m_prcClient.Game.SendSpectatorListLoadPacket();
            this.m_prcClient.Game.SendSpectatorListListPacket();
        }

        #endregion

        
        #region Banlist

        private void Reasons_ItemRemoved(int iIndex, string item) {
            if (this.cboBanlistReason.Items.Contains(item) == true) {
                this.cboBanlistReason.Items.Remove(item);
            }
        }

        private void Reasons_ItemAdded(int iIndex, string item) {
            this.cboBanlistReason.Items.Add(item);
        }

        /*
        public ComboBox.ObjectCollection PunkbusterReasons {
            get {
                return this.cboBanlistReason.Items;
            }
        }
        */

        public string SecondsToText(UInt32 iSeconds, string[] a_strTimeDescriptions) {
            string strReturn = String.Empty;

            double dblSeconds = iSeconds;
            double dblMinutes = (iSeconds / 60);
            double dblHours = (dblMinutes / 60);
            double dblDays = (dblHours / 24);
            double dblWeeks = (dblDays / 7);
            double dblMonths = (dblWeeks / 4);
            double dblYears = (dblMonths / 12);

            if ((Int32)dblYears > 0) {
                strReturn += String.Empty + ((Int32)dblYears) + (((Int32)dblYears) == 1 ? a_strTimeDescriptions[0] : a_strTimeDescriptions[1]);
            }
            if ((Int32)dblMonths % 12 > 0) {
                strReturn += String.Empty + ((Int32)dblMonths) % 12 + (((Int32)dblMonths % 12) == 1 ? a_strTimeDescriptions[2] : a_strTimeDescriptions[3]);
            }
            if ((Int32)dblWeeks % 4 > 0) {
                strReturn += String.Empty + ((Int32)dblWeeks) % 4 + (((Int32)dblWeeks % 4) == 1 ? a_strTimeDescriptions[4] : a_strTimeDescriptions[5]);
            }
            if ((Int32)dblDays % 7 > 0) {
                strReturn += String.Empty + ((Int32)dblDays) % 7 + (((Int32)dblDays % 7) == 1 ? a_strTimeDescriptions[6] : a_strTimeDescriptions[7]);
            }
            if ((Int32)dblHours % 24 > 0) {
                strReturn += String.Empty + ((Int32)dblHours) % 24 + (((Int32)dblHours % 24) == 1 ? a_strTimeDescriptions[8] : a_strTimeDescriptions[9]);
            }
            if ((Int32)dblMinutes % 60 > 0) {
                strReturn += String.Empty + ((Int32)dblMinutes) % 60 + (((Int32)dblMinutes % 60) == 1 ? a_strTimeDescriptions[10] : a_strTimeDescriptions[11]);
            }

            if (iSeconds < 60) {
                if ((Int32)dblSeconds % 60 > 0) {
                    strReturn += String.Empty + ((Int32)dblSeconds) % 60 + (((Int32)dblSeconds % 60) == 1 ? a_strTimeDescriptions[12] : a_strTimeDescriptions[13]);
                }
            }

            return strReturn;
        }

        private ListViewItem CreateBlankBanEntry(string strName) {
            ListViewItem lviNewBanEntry = new ListViewItem();
            lviNewBanEntry.Name = strName;
            lviNewBanEntry.Text = String.Empty;

            ListViewItem.ListViewSubItem lvisName = new ListViewItem.ListViewSubItem();
            lvisName.Name = "name";
            lvisName.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisName);

            ListViewItem.ListViewSubItem lvisIp = new ListViewItem.ListViewSubItem();
            lvisIp.Name = "ip";
            lvisIp.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisIp);

            ListViewItem.ListViewSubItem lvisGuid = new ListViewItem.ListViewSubItem();
            lvisGuid.Name = "guid";
            lvisGuid.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisGuid);

            ListViewItem.ListViewSubItem lvisType = new ListViewItem.ListViewSubItem();
            lvisType.Name = "type";
            lvisType.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisType);

            /*
            ListViewItem.ListViewSubItem lvisTimeOfBan = new ListViewItem.ListViewSubItem();
            lvisTimeOfBan.Name = "timedate";
            lvisTimeOfBan.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisTimeOfBan);

            ListViewItem.ListViewSubItem lvisBanLength = new ListViewItem.ListViewSubItem();
            lvisBanLength.Name = "banlength";
            lvisBanLength.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisBanLength);
            */

            ListViewItem.ListViewSubItem lvisTimeRemaining = new ListViewItem.ListViewSubItem();
            lvisTimeRemaining.Name = "timeremaining";
            lvisTimeRemaining.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisTimeRemaining);

            ListViewItem.ListViewSubItem lvisReason = new ListViewItem.ListViewSubItem();
            lvisReason.Name = "reason";
            lvisReason.Text = String.Empty;
            lviNewBanEntry.SubItems.Add(lvisReason);

            return lviNewBanEntry;
        }

        private string GetFriendlyTypeName(string strType) {
            string strFriendlyTypeName = String.Empty;

            if (String.Compare(strType, "name", true) == 0) {
                strFriendlyTypeName = this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colType.Name", null);
            }
            else if (String.Compare(strType, "ip", true) == 0) {
                strFriendlyTypeName = "IpAddress";
            }
            else if (String.Compare(strType, "guid", true) == 0) {
                strFriendlyTypeName = "Guid";
            }
            else if (String.Compare(strType, "pbguid", true) == 0) {
                strFriendlyTypeName = "PB Guid";
            }

            return strFriendlyTypeName;
        }

        private ListViewItem CreateBanEntry(CBanInfo cbiPlayerBan) {

            ListViewItem lviNewBanEntry = null;

            if (String.Compare("name", cbiPlayerBan.IdType, true) == 0 || String.Compare("persona", cbiPlayerBan.IdType, true) == 0) {
                lviNewBanEntry = this.CreateBlankBanEntry(String.Format("{0}\r\n\r\n", cbiPlayerBan.SoldierName));
                lviNewBanEntry.Text = cbiPlayerBan.Offset.HasValue == true ? (cbiPlayerBan.Offset.Value + 1).ToString(CultureInfo.InvariantCulture) : "-";

                lviNewBanEntry.SubItems["name"].Text = cbiPlayerBan.SoldierName;

                lviNewBanEntry.SubItems["type"].Tag = cbiPlayerBan.IdType;
                lviNewBanEntry.SubItems["type"].Text = this.GetFriendlyTypeName(cbiPlayerBan.IdType);

                //lviNewBanEntry.SubItems["banlength"].Tag = cbiPlayerBan.BanLength;
                lviNewBanEntry.SubItems["timeremaining"].Tag = cbiPlayerBan.BanLength;

                lviNewBanEntry.SubItems["reason"].Text = cbiPlayerBan.Reason;
            }
            else if (String.Compare("ip", cbiPlayerBan.IdType, true) == 0) {

                lviNewBanEntry = this.CreateBlankBanEntry(String.Format("\r\n{0}\r\n", cbiPlayerBan.IpAddress));
                lviNewBanEntry.Text = cbiPlayerBan.Offset.HasValue == true ? (cbiPlayerBan.Offset.Value + 1).ToString(CultureInfo.InvariantCulture) : "-";

                lviNewBanEntry.SubItems["name"].Text = cbiPlayerBan.SoldierName;
                lviNewBanEntry.SubItems["ip"].Text = cbiPlayerBan.IpAddress;

                lviNewBanEntry.SubItems["type"].Tag = cbiPlayerBan.IdType;
                lviNewBanEntry.SubItems["type"].Text = this.GetFriendlyTypeName(cbiPlayerBan.IdType);

                //lviNewBanEntry.SubItems["banlength"].Tag = cbiPlayerBan.BanLength;
                lviNewBanEntry.SubItems["timeremaining"].Tag = cbiPlayerBan.BanLength;

                lviNewBanEntry.SubItems["reason"].Text = cbiPlayerBan.Reason;

            }
            else if (String.Compare("guid", cbiPlayerBan.IdType, true) == 0) {

                lviNewBanEntry = this.CreateBlankBanEntry(String.Format("\r\n\r\n{0}", cbiPlayerBan.Guid));
                lviNewBanEntry.Text = cbiPlayerBan.Offset.HasValue == true ? (cbiPlayerBan.Offset.Value + 1).ToString(CultureInfo.InvariantCulture) : "-";

                lviNewBanEntry.SubItems["name"].Text = cbiPlayerBan.SoldierName;
                lviNewBanEntry.SubItems["guid"].Text = cbiPlayerBan.Guid;
                lviNewBanEntry.SubItems["ip"].Text = cbiPlayerBan.IpAddress;

                lviNewBanEntry.SubItems["type"].Tag = cbiPlayerBan.IdType;
                lviNewBanEntry.SubItems["type"].Text = this.GetFriendlyTypeName(cbiPlayerBan.IdType);

                //lviNewBanEntry.SubItems["banlength"].Tag = cbiPlayerBan.BanLength;
                lviNewBanEntry.SubItems["timeremaining"].Tag = cbiPlayerBan.BanLength;

                lviNewBanEntry.SubItems["reason"].Text = cbiPlayerBan.Reason;
            }

            else if (String.Compare("pbguid", cbiPlayerBan.IdType, true) == 0) {

                lviNewBanEntry = this.CreateBlankBanEntry(String.Format("\r\n\r\n{0}", cbiPlayerBan.Guid));
                lviNewBanEntry.SubItems["name"].Text = cbiPlayerBan.SoldierName;
                lviNewBanEntry.SubItems["guid"].Text = cbiPlayerBan.Guid;
                lviNewBanEntry.SubItems["ip"].Text = cbiPlayerBan.IpAddress;

                lviNewBanEntry.SubItems["type"].Tag = cbiPlayerBan.IdType;
                lviNewBanEntry.SubItems["type"].Text = this.GetFriendlyTypeName(cbiPlayerBan.IdType);

                //lviNewBanEntry.SubItems["banlength"].Tag = cbiPlayerBan.BanLength;
                lviNewBanEntry.SubItems["timeremaining"].Tag = cbiPlayerBan.BanLength;

                lviNewBanEntry.SubItems["reason"].Text = cbiPlayerBan.Reason.TrimEnd('"'); ;
            }

            return lviNewBanEntry;
        }

        public void RemoveDeletedBans(IEnumerable<CBanInfo> bans) {

            for (int i = 0; i < this.lsvBanlist.Items.Count; i++) {
                bool blFoundBan = false;
                foreach (CBanInfo cbiBan in bans) {

                    switch ((string)this.lsvBanlist.Items[i].SubItems["type"].Tag) {
                        case "name":
                        case "persona":
                            blFoundBan = (String.Compare(this.lsvBanlist.Items[i].Name, String.Format("{0}\r\n\r\n", cbiBan.SoldierName)) == 0);
                            break;
                        case "ip":
                            blFoundBan = (String.Compare(this.lsvBanlist.Items[i].Name, String.Format("\r\n{0}\r\n", cbiBan.IpAddress)) == 0);
                            break;
                        case "pbguid":
                        case "guid":
                            blFoundBan = (String.Compare(this.lsvBanlist.Items[i].Name, String.Format("\r\n\r\n{0}", cbiBan.Guid)) == 0);
                            break;
                        default:
                            break;
                    }

                    if (blFoundBan == true) {
                        break;
                    }
                }

                if (blFoundBan == false) {
                    this.lsvBanlist.Items.Remove(this.lsvBanlist.Items[i]);
                    i--;
                }
            }
        }


        private void BansSourceOnChanged() {
            this.InvokeIfRequired(() => {
                IEnumerable<CBanInfo> bans = this.BansSource.Fetch<CBanInfo>();

                this.lsvBanlist.BeginUpdate();

                foreach (CBanInfo cbiBan in bans) {

                    string strKey = String.Empty;

                    if (String.Compare(cbiBan.IdType, "name") == 0 || String.Compare(cbiBan.IdType, "persona") == 0) {
                        strKey = String.Format("{0}\r\n\r\n", cbiBan.SoldierName);
                    }
                    else if (String.Compare(cbiBan.IdType, "ip") == 0) {
                        strKey = String.Format("\r\n{0}\r\n", cbiBan.IpAddress);
                    }
                    else if (String.Compare(cbiBan.IdType, "guid") == 0 || String.Compare(cbiBan.IdType, "pbguid") == 0) {
                        strKey = String.Format("\r\n\r\n{0}", cbiBan.Guid);
                    }

                    if (this.lsvBanlist.Items.ContainsKey(strKey) == false) {
                        this.lsvBanlist.Items.Add(this.CreateBanEntry(cbiBan));
                    }
                    else {
                        ListViewItem lviBanEntry = this.lsvBanlist.Items[strKey];
                        lviBanEntry.Text = cbiBan.Offset.HasValue == true ? (cbiBan.Offset.Value + 1).ToString(CultureInfo.InvariantCulture) : "-";
                        lviBanEntry.SubItems["name"].Text = cbiBan.SoldierName;
                        lviBanEntry.SubItems["type"].Tag = cbiBan.IdType;
                        lviBanEntry.SubItems["type"].Text = this.GetFriendlyTypeName(cbiBan.IdType);
                        lviBanEntry.SubItems["timeremaining"].Tag = cbiBan.BanLength;
                        lviBanEntry.SubItems["reason"].Text = cbiBan.Reason;
                    }
                }

                this.RemoveDeletedBans(bans);

                for (int i = 0; i < this.lsvBanlist.Columns.Count; i++) {
                    this.lsvBanlist.Columns[i].Width = -2;
                }

                this.tmrRefreshBanlist_Tick(null, null);

                this.lsvBanlist.EndUpdate();
            });
        }

        private void BansFilter_TextChanged(object sender, EventArgs e) {
            this.BansSource.Filter = this.BansFilter.Text;
        }

        public void OnBanList(PRoConClient sender, List<CBanInfo> lstBans) {
            this.InvokeIfRequired(() => this.BansSource.Set(lstBans));
        }

        private void m_prcClient_PunkbusterPlayerUnbanned(PRoConClient sender, CBanInfo unbanned) {
            this.InvokeIfRequired(() => {
                this.BansSource.Remove(unbanned);

                this.OnSettingResponse("local.banlist.unban", true);
            });
        }

        public void OnUnban(FrostbiteClient sender, CBanInfo unbanned) {
            this.InvokeIfRequired(() => { 
                this.BansSource.Remove(unbanned);

                this.OnSettingResponse("local.banlist.unban", true);
            });
        }

        private void tmrRefreshBanlist_Tick(object sender, EventArgs e) {
            this.lsvBanlist.BeginUpdate();

            foreach (ListViewItem lviBanEntry in this.lsvBanlist.Items) {

                if (lviBanEntry.SubItems["timeremaining"].Tag != null) {

                    TimeoutSubset ctsTimeout = (TimeoutSubset)lviBanEntry.SubItems["timeremaining"].Tag;

                    if (ctsTimeout.Subset == TimeoutSubset.TimeoutSubsetType.Permanent) {
                        lviBanEntry.SubItems["timeremaining"].Text = this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colTimeRemaining.Permanent", null);
                    }
                    else if (ctsTimeout.Subset == TimeoutSubset.TimeoutSubsetType.Round) {
                        lviBanEntry.SubItems["timeremaining"].Text =
							ctsTimeout.Timeout + " " + this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colTimeRemaining.Round", null);
                    }
                    else if (ctsTimeout.Subset == TimeoutSubset.TimeoutSubsetType.Seconds) {

                        if (ctsTimeout.Timeout > 0) {
                            lviBanEntry.SubItems["timeremaining"].Text = this.SecondsToText((UInt32)ctsTimeout.Timeout, this.ma_strTimeDescriptionsShort);

                            ctsTimeout.Timeout -= (this.tmrRefreshBanlist.Interval / 1000);
                        }
                        else {
                            // I was going to remove it here but I want it to display unbanned until next banList update.
                            lviBanEntry.SubItems["timeremaining"].Text = this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lsvBanlist.colTimeRemaining.Unbanned", null);
                        }
                    }
                }
            }

            this.lsvBanlist.EndUpdate();
        }

        public void OnPbGuidUnban(CBanInfo unbanned) {
            this.InvokeIfRequired(() => {
                this.BansSource.Remove(unbanned);
            
                this.OnSettingResponse("local.banlist.unban", true);
            });
        }

        public void OnPbGuidBan(PRoConClient sender, CBanInfo ban) {
            this.InvokeIfRequired(() => this.BansSource.Append(ban));
        }

        public void Game_BanListAdd(FrostbiteClient sender, CBanInfo ban) {
            this.InvokeIfRequired(() => this.BansSource.Append(ban));
        }
        
        private void unbanToolStripMenuItem_Click(object sender, EventArgs e) {
            this.btnBanlistUnban_Click(sender, e);
        }

        private void btnBanlistUnban_Click(object sender, EventArgs e) {

            if (this.lsvBanlist.SelectedItems.Count > 0) {
                this.WaitForSettingResponse("local.banlist.unban");

                if (String.Compare((string)this.lsvBanlist.SelectedItems[0].SubItems["type"].Tag, "name") == 0 || String.Compare((string)this.lsvBanlist.SelectedItems[0].SubItems["type"].Tag, "persona") == 0) {
                    this.SendCommand("banList.remove", "name", this.lsvBanlist.SelectedItems[0].SubItems["name"].Text);
                }
                else if (String.Compare((string)this.lsvBanlist.SelectedItems[0].SubItems["type"].Tag, "ip") == 0) {
                    this.SendCommand("banList.remove", "ip", this.lsvBanlist.SelectedItems[0].SubItems["ip"].Text);
                }
                else if (String.Compare((string)this.lsvBanlist.SelectedItems[0].SubItems["type"].Tag, "guid") == 0) {
                    this.SendCommand("banList.remove", "guid", this.lsvBanlist.SelectedItems[0].SubItems["guid"].Text);
                }
                else if (String.Compare((string)this.lsvBanlist.SelectedItems[0].SubItems["type"].Tag, "pbguid") == 0) {
                    this.SendCommand("punkBuster.pb_sv_command", String.Format("pb_sv_unbanguid {0}", this.lsvBanlist.SelectedItems[0].SubItems["guid"].Text));
                    this.SendCommand("punkBuster.pb_sv_command", "pb_sv_updbanfile");
                    this.SendCommand("punkBuster.pb_sv_command", "pb_sv_banempty");
                    this.SendCommand("punkBuster.pb_sv_command", "pb_sv_banload");
                }

                this.m_prcClient.Game.SendBanListSavePacket();
            }
        }

        private void lsvBanlist_SelectedIndexChanged(object sender, EventArgs e) {

            if (this.lsvBanlist.SelectedItems.Count > 0) {
                this.btnBanlistUnban.Enabled = true && this.m_spPrivileges.CanEditBanList;
            }
            else if (this.lsvBanlist.FocusedItem != null) {
                this.btnBanlistUnban.Enabled = false;
            }

        }

        private void lsvBanlist_ColumnClick(object sender, ColumnClickEventArgs e) {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.m_lvwBanlistColumnSorter.SortColumn) {
                // Reverse the current sort direction for this column.
                if (this.m_lvwBanlistColumnSorter.Order == SortOrder.Ascending) {
                    this.m_lvwBanlistColumnSorter.Order = SortOrder.Descending;
                }
                else {
                    this.m_lvwBanlistColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else {
                // Set the column number that is to be sorted; default to ascending.
                this.m_lvwBanlistColumnSorter.SortColumn = e.Column;
                this.m_lvwBanlistColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvBanlist.Sort();
        }

        public void OnClearBanList(FrostbiteClient sender) {
            this.OnSettingResponse("local.banlist.clearlist", true);
        }

        private void btnBanlistClearNameList_Click(object sender, EventArgs e) {

            DialogResult dlgClearList = MessageBox.Show(this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistClearBanlist.Question", null), this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.btnBanlistClearBanlist.Title", null), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dlgClearList == DialogResult.Yes) {
                this.WaitForSettingResponse("local.banlist.clearlist");

                this.m_prcClient.Game.SendBanListClearPacket();
                this.m_prcClient.Game.SendBanListSavePacket();
            }
        }

        private void rdoBanlistName_CheckedChanged(object sender, EventArgs e) {
            if (this.rdoBanlistName.Checked == true) {
                //this.ValidateManualBan();
                this.txtBanlistManualBanName.Focus();
                this.txtBanlistManualBanName.Enabled = true;
                this.txtBanlistManualBanIP.Enabled = false;
                this.txtBanlistManualBanGUID.Enabled = false;
                //this.cboBanlistReason.Enabled = false;
                //this.lblBanlistReason.Enabled = false;

                this.rdoBanlistTemporary.Enabled = true;

                this.UpdateConfirmationLabel();
            }
        }

        private void rdoBanlistIP_CheckedChanged(object sender, EventArgs e) {
            if (this.rdoBanlistIP.Checked == true) {
                this.txtBanlistManualBanIP.Focus();
                this.txtBanlistManualBanName.Enabled = false;
                this.txtBanlistManualBanIP.Enabled = true;
                this.txtBanlistManualBanGUID.Enabled = false;
                //this.cboBanlistReason.Enabled = false;
                //this.lblBanlistReason.Enabled = false;

                this.rdoBanlistTemporary.Enabled = true;

                this.UpdateConfirmationLabel();
            }
        }

        private void rdoBanlistBc2GUID_CheckedChanged(object sender, EventArgs e) {
            if (this.rdoBanlistBc2GUID.Checked == true) {
                this.txtBanlistManualBanGUID.Focus();
                this.txtBanlistManualBanName.Enabled = false;
                this.txtBanlistManualBanIP.Enabled = false;
                this.txtBanlistManualBanGUID.Enabled = true;
                //this.cboBanlistReason.Enabled = true;
                //this.lblBanlistReason.Enabled = true;
                this.txtBanlistManualBanIP.ForeColor = SystemColors.WindowText;

                this.rdoBanlistTemporary.Enabled = true;
                //this.rdoBanlistPermanent.Checked = true;

                this.UpdateConfirmationLabel();
            }
        }

        private void rdoBanlistGUID_CheckedChanged(object sender, EventArgs e) {
            if (this.rdoBanlistPbGUID.Checked == true) {
                this.txtBanlistManualBanGUID.Focus();
                this.txtBanlistManualBanName.Enabled = true;
                this.txtBanlistManualBanIP.Enabled = true;
                this.txtBanlistManualBanGUID.Enabled = true;
                //this.cboBanlistReason.Enabled = true;
                //this.lblBanlistReason.Enabled = true;
                this.txtBanlistManualBanIP.ForeColor = SystemColors.WindowText;

                this.rdoBanlistTemporary.Enabled = false;
                this.rdoBanlistPermanent.Checked = true;

                this.UpdateConfirmationLabel();
            }
        }

        private void ListSettings_ManualBansVisibleChange(bool isVisible) {
            if (isVisible == true) {
                this.lnkCloseOpenManualBans.Text = this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lnkCloseOpenManualBans.Close", null);
                this.picCloseOpenManualBans.Image = this.m_frmMain.iglIcons.Images["arrow_down.png"];

                this.spltBanlistManualBans.Panel2Collapsed = false;
            }
            else {
                this.lnkCloseOpenManualBans.Text = this.m_prcClient.Language.GetLocalized("uscListControlPanel.tabBanlist.lnkCloseOpenManualBans.Open", null);
                this.picCloseOpenManualBans.Image = this.m_frmMain.iglIcons.Images["arrow_up.png"];

                this.spltBanlistManualBans.Panel2Collapsed = true;
            }
        }

        private void lnkAddBan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (this.m_prcClient != null) {
                this.m_prcClient.ListSettings.ManualBansVisible = !this.m_prcClient.ListSettings.ManualBansVisible;
            }
        }

        private void picCloseOpenManualBans_Click(object sender, EventArgs e) {
            if (this.m_prcClient != null) {
                this.m_prcClient.ListSettings.ManualBansVisible = !this.m_prcClient.ListSettings.ManualBansVisible;
            }
        }

        private void btnBanlistAddBan_Click(object sender, EventArgs e) {

            string m_strReasonAdmin = this.cboBanlistReason.Text;
            string accountName = this.m_prcClient.Username;
            if (Program.ProconApplication.OptionsSettings.EnableAdminReason && accountName.Length > 0)
            {
                int iBanInfo = (80 - 5 - (accountName.Length + 3));
                if (m_strReasonAdmin.Length > iBanInfo)
                {
                    m_strReasonAdmin = m_strReasonAdmin.Substring(0, iBanInfo);
                }
                m_strReasonAdmin = m_strReasonAdmin + " (" + accountName + ")";
            }

            if (this.rdoBanlistName.Checked == true) {
                // obsolete with BF3 R-8 string name = this.m_prcClient.Game is BF3Client ? "persona" : "name";
                string name = "name";

                if (this.rdoBanlistPermanent.Checked == true) {
                    this.SendCommand("banList.add", name, this.txtBanlistManualBanName.Text, "perm", m_strReasonAdmin);
                }
                else {
                    this.SendCommand("banList.add", name, this.txtBanlistManualBanName.Text, "seconds", (uscPlayerPunishPanel.GetBanLength(this.txtBanlistTime, this.cboBanlistTimeMultiplier) * 60).ToString(), m_strReasonAdmin);
                }
            }
            else if (this.rdoBanlistIP.Checked == true) {
                if (this.rdoBanlistPermanent.Checked == true) {
                    this.SendCommand("banList.add", "ip", this.txtBanlistManualBanIP.Text, "perm", m_strReasonAdmin);
                }
                else {
                    this.SendCommand("banList.add", "ip", this.txtBanlistManualBanIP.Text, "seconds", (uscPlayerPunishPanel.GetBanLength(this.txtBanlistTime, this.cboBanlistTimeMultiplier) * 60).ToString(), m_strReasonAdmin);
                }
            }
            else if (this.rdoBanlistBc2GUID.Checked == true) {
                if (this.rdoBanlistPermanent.Checked == true) {
                    this.SendCommand("banList.add", "guid", this.txtBanlistManualBanGUID.Text.ToUpper(), "perm", m_strReasonAdmin);
                }
                else {
                    this.SendCommand("banList.add", "guid", this.txtBanlistManualBanGUID.Text.ToUpper(), "seconds", (uscPlayerPunishPanel.GetBanLength(this.txtBanlistTime, this.cboBanlistTimeMultiplier) * 60).ToString(), m_strReasonAdmin);
                }
            }
            else if (this.rdoBanlistPbGUID.Checked == true) {
                this.SendCommand("punkBuster.pb_sv_command", String.Format("pb_sv_banguid {0} \"{1}\" \"{2}\" \"BC2! {3}\"", this.txtBanlistManualBanGUID.Text, this.txtBanlistManualBanName.Text.Length > 0 ? this.txtBanlistManualBanName.Text : "???", this.txtBanlistManualBanIP.Text.Length > 0 ? this.txtBanlistManualBanIP.Text : "???", m_strReasonAdmin));
                this.SendCommand("punkBuster.pb_sv_command", this.m_prcClient.Variables.GetVariable<string>("PUNKBUSTER_BANLIST_REFRESH", "pb_sv_banlist BC2! "));
            }
            
            this.txtBanlistManualBanName.Focus();
            this.txtBanlistManualBanName.Clear();
            this.txtBanlistManualBanGUID.Clear();
            this.txtBanlistManualBanIP.Clear();

            this.m_prcClient.Game.SendBanListSavePacket();
        }

        private void txtBanlistManualBanName_TextChanged(object sender, EventArgs e) {
            this.UpdateConfirmationLabel();
        }

        private void txtBanlistManualBanIP_TextChanged(object sender, EventArgs e) {
            this.UpdateConfirmationLabel();
        }

        private void txtBanlistManualBanGUID_TextChanged(object sender, EventArgs e) {
            this.UpdateConfirmationLabel();
        }

        private void lsvBanlist_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {

                if (this.lsvBanlist.SelectedItems.Count > 0) {
                    Point pntMouseLocation = new Point(e.X, e.Y);
                    this.ctxBanlistMenuStrip.Show(this.lsvBanlist, pntMouseLocation);
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {

            if (this.lsvBanlist.SelectedItems.Count > 0) {

                string strClipboard = this.lsvBanlist.SelectedItems[0].Text;

                foreach (ListViewItem.ListViewSubItem lvsiItem in this.lsvBanlist.SelectedItems[0].SubItems) {
                    strClipboard += " " + lvsiItem.Text;
                }

                try {
                    Clipboard.SetDataObject(strClipboard, true, 5, 10);
                }
                catch (Exception) {
                    // Nope, another thread is accessing the clipboard..
                }
            }
        }

        private void rdoBanlistTemporary_CheckedChanged(object sender, EventArgs e) {
            this.pnlBanlistTime.Enabled = this.rdoBanlistTemporary.Checked;
            this.UpdateConfirmationLabel();
        }

        private void rdoBanlistPermanent_CheckedChanged(object sender, EventArgs e) {
            this.pnlBanlistTime.Enabled = this.rdoBanlistTemporary.Checked;
            this.UpdateConfirmationLabel();
        }

        private void UpdateConfirmationLabel() {

            string strBanDescription = String.Empty;

            if (this.rdoBanlistPbGUID.Checked == true) {
                strBanDescription = this.txtBanlistManualBanGUID.Text;
            }
            else if (this.rdoBanlistBc2GUID.Checked == true) {
                strBanDescription = this.txtBanlistManualBanGUID.Text.ToUpper();
            }
            else if (this.rdoBanlistIP.Checked == true) {
                strBanDescription = this.txtBanlistManualBanIP.Text;
            }
            else if (this.rdoBanlistName.Checked == true) {
                strBanDescription = this.txtBanlistManualBanName.Text;
            }

            bool blAbleToPunish = false;

            if (this.m_uscConnectionPanel != null) {
                this.lblBanlistConfirmation.Text = uscPlayerPunishPanel.GetConfirmationLabel(out blAbleToPunish, strBanDescription, this.m_spPrivileges,
                                                                                             this.m_prcClient.Language, false, false, this.rdoBanlistPermanent.Checked,
                                                                                             this.rdoBanlistTemporary.Checked, this.txtBanlistTime, this.cboBanlistTimeMultiplier,
                                                                                             this.ma_strTimeDescriptionsLong, this.m_prcClient.SV_Variables.GetVariable<int>("TEMP_BAN_CEILING", 3600));
            }

            if (this.rdoBanlistIP.Checked == true) {
                this.btnBanlistAddBan.Enabled = (this.txtBanlistManualBanIP.Text.Length > 0 && this.m_regIP.Match(this.txtBanlistManualBanIP.Text).Success) && blAbleToPunish == true;
                this.picBanlistIPError.Visible = !this.btnBanlistAddBan.Enabled && blAbleToPunish == true;

                if (this.btnBanlistAddBan.Enabled == false && blAbleToPunish == true) {
                    this.txtBanlistManualBanIP.ForeColor = Color.Maroon;
                }
                else {
                    this.txtBanlistManualBanIP.ForeColor = SystemColors.WindowText;
                }
            }
            else {
                this.picBanlistIPError.Visible = false;
            }

            if (this.rdoBanlistPbGUID.Checked == true) {
                this.btnBanlistAddBan.Enabled = (this.txtBanlistManualBanGUID.Text.Length > 0 && this.m_regPbGUID.Match(this.txtBanlistManualBanGUID.Text).Success) && blAbleToPunish == true;
                this.picBanlistGUIDError.Visible = !this.btnBanlistAddBan.Enabled && blAbleToPunish == true;

                if (this.btnBanlistAddBan.Enabled == false && blAbleToPunish == true) {
                    this.txtBanlistManualBanGUID.ForeColor = Color.Maroon;
                }
                else {
                    this.txtBanlistManualBanGUID.ForeColor = SystemColors.WindowText;
                }
            }
            else if (this.rdoBanlistBc2GUID.Checked == true) {
                this.btnBanlistAddBan.Enabled = (this.txtBanlistManualBanGUID.Text.Length > 0 && this.m_regBc2GUID.Match(this.txtBanlistManualBanGUID.Text).Success) && blAbleToPunish == true;
                this.picBanlistGUIDError.Visible = !this.btnBanlistAddBan.Enabled && blAbleToPunish == true;

                if (this.btnBanlistAddBan.Enabled == false && blAbleToPunish == true) {
                    this.txtBanlistManualBanGUID.ForeColor = Color.Maroon;
                }
                else {
                    this.txtBanlistManualBanGUID.ForeColor = SystemColors.WindowText;
                }
            }
            else {
                this.picBanlistGUIDError.Visible = false;
            }

            if (this.rdoBanlistName.Checked == true) {
                this.btnBanlistAddBan.Enabled = (this.txtBanlistManualBanName.Text.Length > 0) && blAbleToPunish == true;
            }

        }

        private void txtBanlistTime_TextChanged(object sender, EventArgs e) {
            this.UpdateConfirmationLabel();
        }

        private void cboBanlistTimeMultiplier_SelectedIndexChanged(object sender, EventArgs e) {
            this.UpdateConfirmationLabel();
        }

        private void txtBanlistTime_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b');
        }

        private void btnBanlistRefresh_Click(object sender, EventArgs e) {

            this.m_prcClient.Game.SendBanListListPacket();
            // .SendPunkbusterThing
            this.SendCommand("punkBuster.pb_sv_command", this.m_prcClient.Variables.GetVariable<string>("PUNKBUSTER_BANLIST_REFRESH", "pb_sv_banlist BC2! "));
        }

        #endregion

    }
}
