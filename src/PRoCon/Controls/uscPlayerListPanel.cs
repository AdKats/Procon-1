using PRoCon.Controls.Containers;
using PRoCon.Controls.ControlsEx;
using PRoCon.Core;
using PRoCon.Core.Options;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Remote;
using PRoCon.Core.TextChatModeration;
using PRoCon.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace PRoCon.Controls
{
    public partial class uscPlayerListPanel : UserControl
    {
        /// <summary>
        /// The main window, which because of legacy is used to store icons and such.
        /// </summary>
        protected frmMain Main { get; set; }

        /// <summary>
        /// The parent connection panel.
        /// </summary>
        protected uscServerConnection ConnectionPanel { get; set; }

        /// <summary>
        /// The connected procon client
        /// </summary>
        protected PRoConClient Client { get; set; }

        /// <summary>
        /// The current language to localize with
        /// </summary>
        protected CLocalization Language { get; set; }

        /// <summary>
        /// Central column sorter for sorting the four lists on this panel
        /// </summary>
        protected PlayerListColumnSorter ColumnSorter { get; set; }

        /// <summary>
        /// Set of privileges for the currently logged in user viewing this form
        /// </summary>
        protected CPrivileges Privileges { get; set; }

        /// <summary>
        /// The squad id to specify no squad
        /// </summary>
        protected const int NeutralSquad = 0;

        /// <summary>
        /// The team id to specify no team
        /// </summary>
        protected const int NeutralTeam = 0;

        /// <summary>
        /// The maximum number of teams, including no team.
        /// </summary>
        /// <remarks>0 = neutral, 1, 2, 3, 4..</remarks>
        protected const int MaxTeams = 5;

        /// <summary>
        /// Is the splitter currently grabbed or being moved?
        /// </summary>
        protected bool IsSplitterBeingSet { get; set; }

        /// <summary>
        /// Lock used when interacting with the players dictionary
        /// </summary>
        protected readonly object PlayerDictionaryLocker = new object();

        /// <summary>
        /// Stores all the list view items displayed on any of the four lists, just so we have a central
        /// location to modify from.
        /// </summary>
        protected Dictionary<String, ListViewItem> Players = new Dictionary<String, ListViewItem>();

        /// <summary>
        /// List of pings attached to players.. some reason it's here?
        /// </summary>
        protected Dictionary<String, int> Pings = new Dictionary<String, int>();

        /// <summary>
        /// A players index has been modified and is currently spreading throughout the lists
        /// </summary>
        protected bool PropogatingIndexChange { get; set; }

        /// <summary>
        /// If the team placeholders have been created/drawn
        /// </summary>
        protected bool PlaceHoldersDrawn { get; set; }

        /// <summary>
        /// A list of procon developer uids for highlighting in the player panel
        /// </summary>
        protected List<String> DeveloperUids { get; private set; }

        /// <summary>
        /// A list of procon staff uids for highlighting in the player panel
        /// </summary>
        protected List<String> StaffUids { get; private set; }

        /// <summary>
        /// A list of procon plugin developer uids for highlighting in the player panel
        /// </summary>
        protected List<String> PluginDeveloperUids { get; private set; }

        public uscPlayerListPanel()
        {
            InitializeComponent();

            this.Language = null;
            this.ColumnSorter = new PlayerListColumnSorter();
            this.Main = null;
            this.ConnectionPanel = null;

            this.Privileges = new CPrivileges
            {
                PrivilegesFlags = CPrivileges.FullPrivilegesFlags
            };

            this.spltListAdditionalInfo.Panel2Collapsed = true;
            this.spltTwoSplit.Panel2Collapsed = true;
            this.spltFourSplit.Panel2Collapsed = true;

            this.DeveloperUids = new List<string>();
            this.StaffUids = new List<string>();
            this.PluginDeveloperUids = new List<string>();
        }

        protected void SetSplitterDistances()
        {

            this.IsSplitterBeingSet = true;

            if (this.Client != null && this.Client.PlayerListSettings != null)
            {
                int twoSplitterDistance = (int)(this.spltTwoSplit.Width * this.Client.PlayerListSettings.TwoSplitterPercentage);
                int fourSplitterDistance = (int)(this.spltFourSplit.Height * this.Client.PlayerListSettings.FourSplitterPercentage);

                if (twoSplitterDistance < this.spltTwoSplit.Panel1MinSize)
                {
                    this.spltBottomTwoSplit.SplitterDistance = this.spltTwoSplit.SplitterDistance = this.spltTwoSplit.Panel1MinSize;
                }
                else if (twoSplitterDistance > this.spltTwoSplit.Width - this.spltTwoSplit.Panel2MinSize)
                {
                    this.spltBottomTwoSplit.SplitterDistance = this.spltTwoSplit.SplitterDistance = this.spltTwoSplit.Width - this.spltTwoSplit.Panel2MinSize;
                }
                else
                {
                    this.spltBottomTwoSplit.SplitterDistance = this.spltTwoSplit.SplitterDistance = twoSplitterDistance;
                }

                if (fourSplitterDistance < this.spltFourSplit.Panel1MinSize)
                {
                    this.spltFourSplit.SplitterDistance = this.spltFourSplit.Panel1MinSize;
                }
                else if (fourSplitterDistance > this.spltFourSplit.Height - this.spltFourSplit.Panel2MinSize)
                {
                    this.spltFourSplit.SplitterDistance = this.spltFourSplit.Height - this.spltFourSplit.Panel2MinSize;
                }
                else
                {
                    this.spltFourSplit.SplitterDistance = fourSplitterDistance;
                }

                for (int i = 0; i < this.lsvTeamOnePlayers.Columns.Count; i++)
                {
                    this.lsvTeamOnePlayers.Columns[i].Width = -2;
                    this.lsvTeamTwoPlayers.Columns[i].Width = -2;
                    this.lsvTeamThreePlayers.Columns[i].Width = -2;
                    this.lsvTeamFourPlayers.Columns[i].Width = -2;
                }

                this.Invalidate();
            }

            this.IsSplitterBeingSet = false;
        }

        protected void uscPlayerListPanel_Load(object sender, EventArgs e)
        {
            if (this.Client != null)
            {
                this.Language = this.Client.Language;
            }
        }

        public void Initialize(frmMain frmMainWindow, uscServerConnection uscConnectionPanel)
        {
            this.Main = frmMainWindow;
            this.ConnectionPanel = uscConnectionPanel;

            this.kbpPunkbusterPunishPanel.Punkbuster = true;
            this.kbpPunkbusterPunishPanel.PunishPlayer += new uscPlayerPunishPanel.PunishPlayerDelegate(kbpPunkbusterPunishPanel_PunishPlayer);
            this.kbpPunkbusterPunishPanel.Initialize(uscConnectionPanel);
            this.kbpBfbcPunishPanel.PunishPlayer += new uscPlayerPunishPanel.PunishPlayerDelegate(kbpBfbcPunishPanel_PunishPlayer);
            this.kbpBfbcPunishPanel.Initialize(uscConnectionPanel);

            this.lsvTeamOnePlayers.SmallImageList = this.Main.iglFlags;
            this.lsvTeamTwoPlayers.SmallImageList = this.Main.iglFlags;
            this.lsvTeamThreePlayers.SmallImageList = this.Main.iglFlags;
            this.lsvTeamFourPlayers.SmallImageList = this.Main.iglFlags;
            this.lsvTeamOnePlayers.ListViewItemSorter = this.ColumnSorter;
            this.lsvTeamTwoPlayers.ListViewItemSorter = this.ColumnSorter;
            this.lsvTeamThreePlayers.ListViewItemSorter = this.ColumnSorter;
            this.lsvTeamFourPlayers.ListViewItemSorter = this.ColumnSorter;

            this.btnCloseAdditionalInfo.ImageList = this.Main.iglIcons;
            this.btnCloseAdditionalInfo.ImageKey = @"cross.png";

            this.btnSplitTeams.ImageList = this.Main.iglIcons;
            this.btnSplitTeams.ImageKey = @"application_tile_horizontal.png";

            this.cboEndRound.SelectedIndex = 0;

            // not necessary anymore
            //this.updateDeveloperUids();
        }

        // If we disconnect clear the player list so it's fresh on reconnection.
        private void Client_ConnectionClosed(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                foreach (var player in this.Players)
                {
                    player.Value.Remove();
                }

                this.Players.Clear();
                this.Pings.Clear();
            });
        }

        public void PlayerSelectionChange(string strSoldierName)
        {
            this.SelectPlayer(strSoldierName);
        }

        private void kbpBfbcPunishPanel_PunishPlayer(List<string> lstWords)
        {
            this.Client.SendRequest(lstWords);

            this.Client.Game.SendBanListSavePacket();
            this.Client.Game.SendBanListListPacket();
        }

        private void kbpPunkbusterPunishPanel_PunishPlayer(List<string> lstWords)
        {
            this.Client.SendRequest(lstWords);
        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.Client = prcClient) != null)
            {
                if (this.Client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.Client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Game.ListPlayers += new FrostbiteClient.ListPlayersHandler(m_prcClient_ListPlayers);
                if (this.Client.Game.GameType.Equals("BF3") == true)
                {
                    this.Client.Game.PlayerPingedByAdmin += new FrostbiteClient.PlayerPingedByAdminHandler(Game_PlayerPingedByAdmin);
                    this.Client.ProconAdminPinging += new PRoConClient.ProconAdminPlayerPinged(Game_PlayerPingedByAdmin);
                }
                this.Client.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
                this.Client.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
                this.Client.PunkbusterPlayerInfo += new PRoConClient.PunkbusterPlayerInfoHandler(m_prcClient_PunkbusterPlayerInfo);
                this.Client.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);

                this.Client.Game.PlayerChangedTeam += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedTeam);
                this.Client.Game.PlayerChangedSquad += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedSquad);

                this.Client.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(Client_Serverinfo_EndRound_Update);

                this.Client.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(Client_ProconPrivileges);

                this.Client.ConnectionClosed += new PRoConClient.EmptyParamterHandler(Client_ConnectionClosed);

                this.Client.Game.LevelStarted += new FrostbiteClient.EmptyParamterHandler(m_prcClient_LevelStarted);

                this.kbpPunkbusterPunishPanel.SetConnection(this.Client);
                this.kbpBfbcPunishPanel.SetConnection(this.Client);

                this.Client.Reasons.ItemAdded += new NotificationList<string>.ItemModifiedHandler(Reasons_ItemAdded);
                this.Client.Reasons.ItemRemoved += new NotificationList<string>.ItemModifiedHandler(Reasons_ItemRemoved);

                this.Client.PlayerListSettings.SplitTypeChanged += new PlayerListSettings.IndexChangedHandler(PlayerListSettings_SplitTypeChanged);
                this.Client.PlayerListSettings.TwoSplitterPercentageChanged += new PlayerListSettings.PercentageChangedHandler(PlayerListSettings_TwoSplitterPercentageChanged);
                this.Client.PlayerListSettings.FourSplitterPercentageChanged += new PlayerListSettings.PercentageChangedHandler(PlayerListSettings_FourSplitterPercentageChanged);

                this.Client.PlayerSpawned += new PRoConClient.PlayerSpawnedHandler(m_prcClient_PlayerSpawned);

                foreach (string strReason in this.Client.Reasons)
                {
                    this.Reasons_ItemAdded(0, strReason);
                }

                this.Client.PlayerListSettings.SplitType = this.Client.PlayerListSettings.SplitType;

                this.m_prcClient_ListPlayers(this.Client.Game, new List<CPlayerInfo>(this.Client.PlayerList), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));

                if (sender.Game.HasSquads == false)
                {
                    this.lsvTeamOnePlayers.Columns.Remove(this.colSquad1);
                    this.lsvTeamTwoPlayers.Columns.Remove(this.colSquad2);
                    this.lsvTeamThreePlayers.Columns.Remove(this.colSquad3);
                    this.lsvTeamFourPlayers.Columns.Remove(this.colSquad4);
                }

                this.SetSplitterDistances();
            });
        }

        private void PlayerListSettings_SplitTypeChanged(int index)
        {
            if (index == 1)
            {
                this.btnSplitTeams.ImageKey = @"application_tile_horizontal.png";

                this.spltTwoSplit.Panel2Collapsed = true;
                this.spltFourSplit.Panel2Collapsed = true;
            }
            else if (index == 2)
            {
                this.btnSplitTeams.ImageKey = @"application_tile.png";

                this.spltTwoSplit.Panel2Collapsed = false;
                this.spltFourSplit.Panel2Collapsed = true;
            }
            else if (index == 4)
            {
                this.btnSplitTeams.ImageKey = @"application.png";

                this.spltTwoSplit.Panel2Collapsed = false;
                this.spltFourSplit.Panel2Collapsed = false;
            }
        }

        private void Reasons_ItemRemoved(int iIndex, string item)
        {
            if (this.kbpBfbcPunishPanel.Reasons.Contains(item) == true)
            {
                this.kbpBfbcPunishPanel.Reasons.Remove(item);
            }

            if (this.kbpPunkbusterPunishPanel.Reasons.Contains(item) == true)
            {
                this.kbpPunkbusterPunishPanel.Reasons.Remove(item);
            }
        }

        private void Reasons_ItemAdded(int iIndex, string item)
        {
            this.kbpBfbcPunishPanel.Reasons.Add(item);
            this.kbpPunkbusterPunishPanel.Reasons.Add(item);
        }

        private void Client_ProconPrivileges(PRoConClient sender, CPrivileges privileges)
        {
            this.InvokeIfRequired(() =>
            {
                this.Privileges = privileges;

                this.kbpPunkbusterPunishPanel.Enabled = (!this.Privileges.CannotPunishPlayers && this.Privileges.CanIssueLimitedPunkbusterCommands);
                this.kbpPunkbusterPunishPanel.SetPrivileges(this.Privileges);

                this.kbpBfbcPunishPanel.Enabled = !this.Privileges.CannotPunishPlayers;
                this.kbpBfbcPunishPanel.SetPrivileges(this.Privileges);
            });
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.Language = clocLanguage;

            this.colSlotID1.Text = this.colSlotID2.Text = this.colSlotID3.Text = this.colSlotID4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colSlotID", null);
            this.colTags1.Text = this.colTags2.Text = this.colTags3.Text = this.colTags4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colTags", null);
            this.colPlayerName1.Text = this.colPlayerName2.Text = this.colPlayerName3.Text = this.colPlayerName4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colPlayerName", null);
            this.colSquad1.Text = this.colSquad2.Text = this.colSquad3.Text = this.colSquad4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colSquad", null);
            this.colKit1.Text = this.colKit2.Text = this.colKit3.Text = this.colKit4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colKit", null);
            this.colKills1.Text = this.colKills2.Text = this.colKills3.Text = this.colKills4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colKills", null);
            this.colDeaths1.Text = this.colDeaths2.Text = this.colDeaths3.Text = this.colDeaths4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colDeaths", null);
            this.colKdr1.Text = this.colKdr2.Text = this.colKdr3.Text = this.colKdr4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colKdr", null);
            this.colScore1.Text = this.colScore2.Text = this.colScore3.Text = this.colScore4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colScore", null);
            this.colPing1.Text = this.colPing2.Text = this.colPing3.Text = this.colPing4.Text = this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colPing", null);
            this.colRank1.Text = this.colRank2.Text = this.colRank3.Text = this.colRank4.Text = this.Language.GetDefaultLocalized("Rank", "uscPlayerListPanel.lsvPlayers.colRank", null);
            this.colTime1.Text = this.colTime2.Text = this.colTime3.Text = this.colTime4.Text = this.Language.GetDefaultLocalized("PlayTime", "uscPlayerListPanel.lsvPlayers.colTime", null);
            this.colType1.Text = this.colType2.Text = this.colType3.Text = this.colType4.Text = this.Language.GetDefaultLocalized("Type", "uscPlayerListPanel.lsvPlayers.colType", null);

            this.btnPlayerListSelectedCheese.Text = this.Language.GetLocalized("uscPlayerListPanel.btnPlayerListSelectedCheese", null);

            this.chkPlayerListShowTeams.Text = this.Language.GetLocalized("uscPlayerListPanel.chkPlayerListShowTeams", null);

            this.tabCourtMartialBFBC.Text = this.Language.GetLocalized("uscPlayerListPanel.tabCourtMartialBFBC", null);
            this.tabCourtMartialPunkbuster.Text = this.Language.GetLocalized("uscPlayerListPanel.tabCourtMartialPunkbuster", null);

            this.lblInventory.Text = this.Language.GetLocalized("uscPlayerListPanel.lblInventory") + @":";

            // Player Context Menu
            this.textChatModerationToolStripMenuItem.Text = this.Language.GetLocalized("uscPlayerListPanel.ctxPlayerOptions.textChatModerationToolStripMenuItem");
            this.reservedSlotToolStripMenuItem.Text = this.Language.GetLocalized("uscPlayerListPanel.ctxPlayerOptions.reservedSlotToolStripMenuItem");
            this.spectatorListToolStripMenuItem.Text = this.Language.GetDefaultLocalized("Spectator list", "uscPlayerListPanel.ctxPlayerOptions.spectatorListToolStripMenuItem");

            this.statsLookupToolStripMenuItem.Text = this.Language.GetDefaultLocalized("Stats Lookup", "uscPlayerListPanel.ctxPlayerOptions.statsLookupToolStripMenuItem");
            this.punkBusterScreenshotToolStripMenuItem.Text = this.Language.GetDefaultLocalized("PunkBuster Screenshot", "uscPlayerListPanel.ctxPlayerOptions.punkBusterScreenshotToolStripMenuItem");

            // cboEndRound
            this.cboEndRound.Items.Clear();
            this.cboEndRound.Items.AddRange(new object[] {
                this.Language.GetDefaultLocalized("Select winning team to end round:", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Label"),
                this.Language.GetDefaultLocalized("Team 1", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Team1"),
                this.Language.GetDefaultLocalized("Team 2", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Team2"),
                this.Language.GetDefaultLocalized("Team 3", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Team3"),
                this.Language.GetDefaultLocalized("Team 4", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Team4")
            });
            this.cboEndRound.SelectedIndex = 0;

            Graphics cboEndRoundGrafphics = cboEndRound.CreateGraphics();
            this.cboEndRound.Width = 18 + (int)cboEndRoundGrafphics.MeasureString(this.cboEndRound.Text, this.cboEndRound.Font).Width;

            this.kbpBfbcPunishPanel.SetLocalization(this.Language);
            this.kbpPunkbusterPunishPanel.SetLocalization(this.Language);
        }

        private ListViewItem CreateTotalsPlayer(CPlayerInfo cpiDummyPlayer, int iTeamID)
        {
            ListViewItem lviReturn = this.CreatePlayer(new CPlayerInfo(cpiDummyPlayer.SoldierName, String.Empty, iTeamID, 0));
            lviReturn.Name = cpiDummyPlayer.ClanTag;
            lviReturn.Font = new Font(this.Font, FontStyle.Bold);

            return lviReturn;
        }

        private ListViewItem CreatePlayer(CPlayerInfo player)
        {
            ListViewItem newListPlayer = new ListViewItem("")
            {
                Name = player.SoldierName,
                Tag = null,
                UseItemStyleForSubItems = true
            };

            AdditionalPlayerInfo additional = new AdditionalPlayerInfo
            {
                Player = player,
                ResolvedHostName = String.Empty
            };
            newListPlayer.Tag = additional;

            ListViewItem.ListViewSubItem tags = new ListViewItem.ListViewSubItem
            {
                Name = @"tags",
                Text = player.ClanTag
            };
            newListPlayer.SubItems.Add(tags);

            ListViewItem.ListViewSubItem tagsName = new ListViewItem.ListViewSubItem
            {
                Name = @"soldiername",
                Text = player.SoldierName
            };
            newListPlayer.SubItems.Add(tagsName);

            if (this.Client != null && this.Client.Game != null && this.Client.Game.HasSquads == true)
            {
                ListViewItem.ListViewSubItem squad = new ListViewItem.ListViewSubItem
                {
                    Name = @"squad"
                };

                if (player.SquadID != uscPlayerListPanel.NeutralSquad)
                {
                    squad.Text = this.Language.GetLocalized("global.Squad" + player.SquadID.ToString(CultureInfo.InvariantCulture), null);
                }
                newListPlayer.SubItems.Add(squad);
            }

            ListViewItem.ListViewSubItem kit = new ListViewItem.ListViewSubItem
            {
                Name = @"kit",
                Text = String.Empty
            };
            newListPlayer.SubItems.Add(kit);

            ListViewItem.ListViewSubItem score = new ListViewItem.ListViewSubItem
            {
                Name = @"score",
                Text = player.Score.ToString(CultureInfo.InvariantCulture)
            };
            newListPlayer.SubItems.Add(score);

            ListViewItem.ListViewSubItem kills = new ListViewItem.ListViewSubItem
            {
                Name = @"kills",
                Tag = (Double)player.Kills,
                Text = player.Kills.ToString(CultureInfo.InvariantCulture)
            };
            newListPlayer.SubItems.Add(kills);

            ListViewItem.ListViewSubItem deaths = new ListViewItem.ListViewSubItem
            {
                Name = @"deaths",
                Tag = (Double)player.Deaths,
                Text = player.Deaths.ToString(CultureInfo.InvariantCulture)
            };
            newListPlayer.SubItems.Add(deaths);

            ListViewItem.ListViewSubItem kdr = new ListViewItem.ListViewSubItem
            {
                Name = @"kdr",
                Text = player.Deaths > 0 ? String.Format("{0:0.00}", (Double)player.Kills / player.Deaths) : String.Format("{0:0.00}", (Double)player.Kills)
            };
            newListPlayer.SubItems.Add(kdr);

            ListViewItem.ListViewSubItem ping = new ListViewItem.ListViewSubItem
            {
                Name = @"ping",
                Text = player.Ping.ToString(CultureInfo.InvariantCulture)
            };
            newListPlayer.SubItems.Add(ping);

            ListViewItem.ListViewSubItem rank = new ListViewItem.ListViewSubItem
            {
                Name = @"rank",
                Text = player.Rank.ToString(CultureInfo.InvariantCulture)
            };
            newListPlayer.SubItems.Add(rank);

            ListViewItem.ListViewSubItem time = new ListViewItem.ListViewSubItem
            {
                Name = @"time",
                Text = player.SessionTime > 60 ? String.Format("{0:0}", player.SessionTime / 60) : "0"
            };
            newListPlayer.SubItems.Add(time);

            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem
            {
                Name = @"type"
            };

            if (player.Type == 0)
            {
                type.Text = String.Empty;
            }
            else if (player.Type == 1)
            {
                type.Text = this.Language.GetDefaultLocalized("Spectator", "uscPlayerListPanel.lsvPlayers.Type.Spectator", null);
            }
            else if (player.Type == 2)
            {
                type.Text = this.Language.GetDefaultLocalized("Commander (PC)", "uscPlayerListPanel.lsvPlayers.Type.CommanderPC", null);
            }
            else if (player.Type == 3)
            {
                type.Text = this.Language.GetDefaultLocalized("Commander (Tablet)", "uscPlayerListPanel.lsvPlayers.Type.CommanderTablet", null);
            }

            newListPlayer.SubItems.Add(type);

            return newListPlayer;
        }

        private static int GetPlayerTeamID(ListViewItem player)
        {
            int teamId = 0;

            if (player.Tag != null && ((AdditionalPlayerInfo)player.Tag).Player != null)
            {
                teamId = ((AdditionalPlayerInfo)player.Tag).Player.TeamID;
            }

            return teamId;
        }

        private int GetTotalPlayersByTeamID(int teamId)
        {
            // - 2 to account for the totals.
            return this.Players.Count(player => GetPlayerTeamID(player.Value) == teamId) - 2;
        }

        private static void SetPlayerTeamID(ListViewItem player, int iTeamID)
        {
            var tag = player.Tag as AdditionalPlayerInfo;

            if (tag != null && tag.Player != null)
            {
                tag.Player.TeamID = iTeamID;
            }
        }

        private void SetPlayerSquadID(ListViewItem player, int squadId)
        {
            var tag = player.Tag as AdditionalPlayerInfo;

            if (tag != null && tag.Player != null)
            {
                if (this.Client != null && this.Client.Game != null && this.Client.Game.HasSquads == true)
                {
                    tag.Player.SquadID = squadId;

                    player.SubItems["squad"].Text = squadId != uscPlayerListPanel.NeutralSquad ? this.Language.GetLocalized("global.Squad" + tag.Player.SquadID.ToString(CultureInfo.InvariantCulture), null) : String.Empty;
                }
            }
        }

        private void UpdateTeamNames()
        {
            // All four lists have the same number of groups in them..
            for (int i = 0; i < uscPlayerListPanel.MaxTeams; i++)
            {
                String score = String.Empty;

                if (this.Client != null && this.Client.CurrentServerInfo != null)
                {
                    if (this.Client.CurrentServerInfo.TeamScores != null)
                    {
                        if (i > 0 && this.Client.CurrentServerInfo.TeamScores.Count > i - 1)
                        {
                            score = String.Format(" - {0} {1}", this.Client.CurrentServerInfo.TeamScores[i - 1].Score, this.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.Groups.Tickets"));
                        }
                    }

                    this.lsvTeamOnePlayers.Groups[i].Header = String.Format("{1} - {0}{2}", this.Client.GetLocalizedTeamName(i, this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode), this.lsvTeamOnePlayers.Groups[i].Items.Count - 2, score);
                    this.lsvTeamTwoPlayers.Groups[i].Header = String.Format("{1} - {0}{2}", this.Client.GetLocalizedTeamName(i, this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode), this.lsvTeamTwoPlayers.Groups[i].Items.Count - 2, score);
                    this.lsvTeamThreePlayers.Groups[i].Header = String.Format("{1} - {0}{2}", this.Client.GetLocalizedTeamName(i, this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode), this.lsvTeamThreePlayers.Groups[i].Items.Count - 2, score);
                    this.lsvTeamFourPlayers.Groups[i].Header = String.Format("{1} - {0}{2}", this.Client.GetLocalizedTeamName(i, this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode), this.lsvTeamFourPlayers.Groups[i].Items.Count - 2, score);
                }
            }
        }

        private void SetTotalsZero(int teamId)
        {
            if (this.Players.ContainsKey(String.Format("procon.playerlist.totals{0}", teamId)) == true)
            {
                var tag = this.Players[String.Format("procon.playerlist.totals{0}", teamId)].Tag as AdditionalPlayerInfo;

                if (tag != null)
                {
                    tag.KitCounter.Clear();
                    tag.Player.Kills = 0;
                    tag.Player.Deaths = 0;
                    tag.Player.Score = 0;
                    tag.Player.Ping = 0;
                    tag.Player.Rank = 0;
                    tag.Player.SessionTime = 0;
                    tag.Player.Type = 0;
                    tag.Player.SquadID = 0;
                    tag.Player.Kdr = 0.0F;
                }

                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["kit"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["kills"].Text = @"0";
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["deaths"].Text = @"0";
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["score"].Text = @"0";
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["ping"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["rank"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["time"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["type"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.totals{0}", teamId)].SubItems["kdr"].Text = @"0.00";

                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["kit"].Text = String.Empty;
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["kills"].Text = @"0.00";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["deaths"].Text = @"0.00";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["score"].Text = @"0.00";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["ping"].Text = @"0.00";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["rank"].Text = @"-";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["time"].Text = @"-";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["type"].Text = @"-";
                this.Players[String.Format("procon.playerlist.averages{0}", teamId)].SubItems["kdr"].Text = @"0.00";
            }
        }

        private void AddTotalsPlayerDetails(int iTeamID, AdditionalPlayerInfo player)
        {
            if (this.Players.ContainsKey(String.Format("procon.playerlist.totals{0}", iTeamID)) == true)
            {
                var tag = this.Players[String.Format("procon.playerlist.totals{0}", iTeamID)].Tag as AdditionalPlayerInfo;

                if (tag != null)
                {
                    tag.Player.Kills += player.Player.Kills;
                    tag.Player.Deaths += player.Player.Deaths;
                    tag.Player.Score += player.Player.Score;
                    tag.Player.Ping += player.Player.Ping;
                    tag.Player.Rank += player.Player.Rank;
                    tag.Player.Type += player.Player.Type;
                    tag.Player.Kdr += (player.Player.Deaths > 0 ? (float)player.Player.Kills / player.Player.Deaths : player.Player.Kills);
                    tag.Player.SquadID++;

                    if (player.SpawnedInventory != null)
                    {
                        tag.AddKitCount(player.SpawnedInventory.Kit);
                    }
                }
            }
        }

        private void AddKillToTeamTotal(int iTeamID)
        {
            string proconPlayerListTotals = String.Format("procon.playerlist.totals{0}", iTeamID);

            AdditionalPlayerInfo proconPlayerListTotalsObject = ((AdditionalPlayerInfo)this.Players[proconPlayerListTotals].Tag);

            proconPlayerListTotalsObject.Player.Kills += 1;
        }

        private void AddDeathToTeamTotal(int iTeamID)
        {
            string proconPlayerListTotals = String.Format("procon.playerlist.totals{0}", iTeamID);

            AdditionalPlayerInfo proconPlayerListTotalsObject = ((AdditionalPlayerInfo)this.Players[proconPlayerListTotals].Tag);
            ListViewItem proconPlayerListTotalsListItem = this.Players[proconPlayerListTotals];

            proconPlayerListTotalsObject.Player.Deaths += 1;
        }

        private void FinalizeTotalsAverages()
        {
            this.FinalizeTotalsAveragesPlayerDetails(1);
            this.FinalizeTotalsAveragesPlayerDetails(2);
            this.FinalizeTotalsAveragesPlayerDetails(3);
            this.FinalizeTotalsAveragesPlayerDetails(4);
        }

        private void FinalizeTotalsAveragesPlayerDetails(int iTeamID)
        {
            if (this.Players.ContainsKey(String.Format("procon.playerlist.totals{0}", iTeamID)) == true)
            {

                string proconPlayerListTotals = String.Format("procon.playerlist.totals{0}", iTeamID),
                       proconPlayerListAverages = String.Format("procon.playerlist.averages{0}", iTeamID);

                AdditionalPlayerInfo proconPlayerListTotalsObject = ((AdditionalPlayerInfo)this.Players[proconPlayerListTotals].Tag);
                ListViewItem proconPlayerListTotalsListItem = this.Players[proconPlayerListTotals],
                             proconPlayerListAveragesListItem = this.Players[proconPlayerListAverages];

                proconPlayerListTotalsListItem.SubItems["kills"].Text = proconPlayerListTotalsObject.Player.Kills.ToString();
                proconPlayerListTotalsListItem.SubItems["deaths"].Text = proconPlayerListTotalsObject.Player.Deaths.ToString();
                proconPlayerListTotalsListItem.SubItems["score"].Text = proconPlayerListTotalsObject.Player.Score.ToString();
                proconPlayerListTotalsListItem.SubItems["ping"].Text = proconPlayerListTotalsObject.Player.Ping.ToString();
                proconPlayerListTotalsListItem.SubItems["kdr"].Text = String.Format("{0:0.00}", proconPlayerListTotalsObject.Player.Kdr);

                proconPlayerListAveragesListItem.SubItems["kills"].Text = String.Format("{0:0.00}", (float)proconPlayerListTotalsObject.Player.Kills / (float)proconPlayerListTotalsObject.Player.SquadID);
                proconPlayerListAveragesListItem.SubItems["deaths"].Text = String.Format("{0:0.00}", (float)proconPlayerListTotalsObject.Player.Deaths / (float)proconPlayerListTotalsObject.Player.SquadID);
                proconPlayerListAveragesListItem.SubItems["score"].Text = String.Format("{0:0.00}", (float)proconPlayerListTotalsObject.Player.Score / (float)proconPlayerListTotalsObject.Player.SquadID);
                proconPlayerListAveragesListItem.SubItems["ping"].Text = String.Format("{0:0}", (int)proconPlayerListTotalsObject.Player.Ping / (float)proconPlayerListTotalsObject.Player.SquadID);
                proconPlayerListAveragesListItem.SubItems["rank"].Text = String.Format("{0:0}", (int)proconPlayerListTotalsObject.Player.Rank / (float)proconPlayerListTotalsObject.Player.SquadID);
                proconPlayerListAveragesListItem.SubItems["time"].Text = String.Empty;
                proconPlayerListAveragesListItem.SubItems["type"].Text = String.Empty;
                proconPlayerListAveragesListItem.SubItems["kdr"].Text = String.Format("{0:0.00}", proconPlayerListTotalsObject.Player.Kdr / (float)proconPlayerListTotalsObject.Player.SquadID);

                int mostUsedKitCount = 0;
                Kits mostUsedKit = Kits.None;
                List<string> kitTotals = new List<string>();

                foreach (KeyValuePair<Kits, int> kitCount in ((AdditionalPlayerInfo)proconPlayerListTotalsListItem.Tag).KitCounter)
                {
                    if (kitCount.Value > mostUsedKitCount)
                    {
                        mostUsedKitCount = kitCount.Value;
                        mostUsedKit = kitCount.Key;
                    }

                    kitTotals.Add(String.Format("{0}{1}", kitCount.Value, this.Language.GetLocalized(String.Format("global.Kits.{0}.Short", kitCount.Key.ToString()))));
                }

                proconPlayerListTotalsListItem.SubItems["kit"].Text = String.Join(",", kitTotals.ToArray());

                proconPlayerListAveragesListItem.SubItems["kit"].Text = this.Language.GetLocalized(String.Format("global.Kits.{0}", mostUsedKit.ToString()));

            }
        }

        // Simply puts the players into the correct list.
        private void ArrangePlayers()
        {

            this.PropogatingIndexChange = true;
            if (this.Client != null)
            {

                this.SetTotalsZero(1);
                this.SetTotalsZero(2);
                this.SetTotalsZero(3);
                this.SetTotalsZero(4);

                if (this.Client.PlayerListSettings.SplitType == 1)
                {
                    this.lsvTeamOnePlayers.BeginUpdate();

                    foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
                    {

                        int iPlayerTeamID = GetPlayerTeamID(kvpPlayer.Value);
                        bool isTotalsPlayer = this.ColumnSorter.TotalsAveragesChecker.IsMatch(kvpPlayer.Key);

                        if (isTotalsPlayer == false)
                        {
                            this.AddTotalsPlayerDetails(iPlayerTeamID, (AdditionalPlayerInfo)kvpPlayer.Value.Tag);
                        }

                        if (isTotalsPlayer == false || this.GetTotalPlayersByTeamID(iPlayerTeamID) > 0)
                        {

                            if (this.lsvTeamOnePlayers.Items.ContainsKey(kvpPlayer.Key) == false)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iPlayerTeamID];
                                this.lsvTeamOnePlayers.Items.Add(kvpPlayer.Value);
                            }
                            else
                            {
                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iPlayerTeamID];
                            }

                        }
                    }

                    this.lsvTeamOnePlayers.EndUpdate();
                }
                else if (this.Client.PlayerListSettings.SplitType == 2)
                {
                    this.lsvTeamOnePlayers.BeginUpdate();
                    this.lsvTeamTwoPlayers.BeginUpdate();

                    foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
                    {
                        int iTeamID = GetPlayerTeamID(kvpPlayer.Value);
                        bool isTotalsPlayer = this.ColumnSorter.TotalsAveragesChecker.IsMatch(kvpPlayer.Key); ;

                        if (isTotalsPlayer == false)
                        {
                            this.AddTotalsPlayerDetails(iTeamID, (AdditionalPlayerInfo)kvpPlayer.Value.Tag);
                        }

                        if (isTotalsPlayer == false || this.GetTotalPlayersByTeamID(iTeamID) > 0)
                        {

                            if (this.lsvTeamOnePlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 1 || iTeamID == 3)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iTeamID];
                                this.lsvTeamOnePlayers.Items.Add(kvpPlayer.Value);
                            }
                            else if (this.lsvTeamOnePlayers.Items.ContainsKey(kvpPlayer.Key) == true && iTeamID == 1 || iTeamID == 3)
                            {
                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iTeamID];
                            }
                            else if (this.lsvTeamTwoPlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 2 || iTeamID == 4)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamTwoPlayers.Groups[iTeamID];
                                this.lsvTeamTwoPlayers.Items.Add(kvpPlayer.Value);
                            }
                            else if (this.lsvTeamOnePlayers.Items.ContainsKey(kvpPlayer.Key) == true && iTeamID == 2 || iTeamID == 4)
                            {
                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iTeamID];
                            }
                        }
                    }

                    this.lsvTeamTwoPlayers.EndUpdate();
                    this.lsvTeamOnePlayers.EndUpdate();
                }
                else if (this.Client.PlayerListSettings.SplitType == 4)
                {
                    this.lsvTeamOnePlayers.BeginUpdate();
                    this.lsvTeamTwoPlayers.BeginUpdate();
                    this.lsvTeamThreePlayers.BeginUpdate();
                    this.lsvTeamFourPlayers.BeginUpdate();

                    foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
                    {
                        int iTeamID = GetPlayerTeamID(kvpPlayer.Value);
                        bool isTotalsPlayer = this.ColumnSorter.TotalsAveragesChecker.IsMatch(kvpPlayer.Key);

                        if (isTotalsPlayer == false)
                        {
                            this.AddTotalsPlayerDetails(iTeamID, (AdditionalPlayerInfo)kvpPlayer.Value.Tag);
                        }

                        if (isTotalsPlayer == false || this.GetTotalPlayersByTeamID(iTeamID) > 0)
                        {

                            if (this.lsvTeamOnePlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 1)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamOnePlayers.Groups[iTeamID];
                                this.lsvTeamOnePlayers.Items.Add(kvpPlayer.Value);
                            }
                            else if (this.lsvTeamTwoPlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 2)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamTwoPlayers.Groups[iTeamID];
                                this.lsvTeamTwoPlayers.Items.Add(kvpPlayer.Value);
                            }
                            else if (this.lsvTeamThreePlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 3)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamThreePlayers.Groups[iTeamID];
                                this.lsvTeamThreePlayers.Items.Add(kvpPlayer.Value);
                            }
                            else if (this.lsvTeamFourPlayers.Items.ContainsKey(kvpPlayer.Key) == false && iTeamID == 4)
                            {
                                kvpPlayer.Value.Remove();

                                kvpPlayer.Value.Group = this.lsvTeamFourPlayers.Groups[iTeamID];
                                this.lsvTeamFourPlayers.Items.Add(kvpPlayer.Value);
                            }
                        }
                    }

                    this.lsvTeamFourPlayers.EndUpdate();
                    this.lsvTeamThreePlayers.EndUpdate();
                    this.lsvTeamTwoPlayers.EndUpdate();
                    this.lsvTeamOnePlayers.EndUpdate();

                    GC.Collect();
                }

                this.FinalizeTotalsAverages();
                this.UpdateTeamNames();

                // All three lists have the same number of columns in them..
                for (int i = 0; i < this.lsvTeamOnePlayers.Columns.Count; i++)
                {
                    this.lsvTeamOnePlayers.Columns[i].Width = -2;
                    this.lsvTeamTwoPlayers.Columns[i].Width = -2;
                    this.lsvTeamThreePlayers.Columns[i].Width = -2;
                    this.lsvTeamFourPlayers.Columns[i].Width = -2;
                }

                this.lsvTeamOnePlayers.Sort();
                this.lsvTeamTwoPlayers.Sort();
                this.lsvTeamThreePlayers.Sort();
                this.lsvTeamFourPlayers.Sort();
            }
            this.PropogatingIndexChange = false;
        }

        private void m_prcClient_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.Players.ContainsKey(playerName) == true)
                {
                    this.Players[playerName].Remove();
                    this.Players.Remove(playerName);
                }
                if (this.Pings.ContainsKey(playerName) == true)
                {
                    this.Pings.Remove(playerName);
                }
                this.UpdateTeamNames();

                this.RefreshSelectedPlayer();
            });
        }

        private void m_prcClient_PlayerJoin(FrostbiteClient sender, string playerName)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.Players.ContainsKey(playerName) == false)
                {
                    this.Players.Add(playerName, this.CreatePlayer(new CPlayerInfo(playerName, String.Empty, 0, 0)));

                    this.ArrangePlayers();
                }
            });
        }

        /*
        public void OnPlayerJoin(string strSoldierName) {

        }

        public void OnPlayerLeave(string strSoldierName) {


        }
         * 
        */

        private void m_prcClient_PlayerSpawned(PRoConClient sender, string soldierName, Inventory spawnedInventory)
        {
            this.InvokeIfRequired(() =>
            {
                this.PropogatingIndexChange = true;

                if (this.Players.ContainsKey(soldierName) == true)
                {
                    AdditionalPlayerInfo sapiAdditional;

                    if (this.Players[soldierName].Tag != null)
                    {
                        sapiAdditional = (AdditionalPlayerInfo)this.Players[soldierName].Tag;

                        sapiAdditional.SpawnedInventory = spawnedInventory;

                        if (this.Players.ContainsKey(soldierName) == true)
                        {
                            this.Players[soldierName].SubItems["kit"].Text = this.Language.GetLocalized(String.Format("global.Kits.{0}", spawnedInventory.Kit.ToString()));
                        }

                        if (sapiAdditional.Punkbuster != null)
                        {
                            if (this.Main.iglFlags.Images.ContainsKey(sapiAdditional.Punkbuster.PlayerCountryCode + ".png") == true)
                            {
                                this.Players[sapiAdditional.Punkbuster.SoldierName].ImageIndex = this.Main.iglFlags.Images.IndexOfKey(sapiAdditional.Punkbuster.PlayerCountryCode + ".png");
                            }
                        }

                        this.Players[soldierName].Tag = sapiAdditional;
                    }

                    this.RefreshSelectedPlayer();
                }

                //this.ArrangePlayers();

                this.PropogatingIndexChange = false;
            });
        }

        private void m_prcClient_PunkbusterPlayerInfo(PRoConClient sender, CPunkbusterInfo pbInfo)
        {
            this.InvokeIfRequired(() =>
            {
                this.PropogatingIndexChange = true;

                if (this.Players.ContainsKey(pbInfo.SoldierName) == true)
                {

                    AdditionalPlayerInfo sapiAdditional;

                    if (this.Players[pbInfo.SoldierName].Tag == null)
                    {
                        sapiAdditional = new AdditionalPlayerInfo();
                        sapiAdditional.ResolvedHostName = String.Empty;
                    }
                    else
                    {
                        sapiAdditional = (AdditionalPlayerInfo)this.Players[pbInfo.SoldierName].Tag;
                    }

                    sapiAdditional.Punkbuster = pbInfo;

                    this.Players[pbInfo.SoldierName].Tag = sapiAdditional;

                    this.Players[pbInfo.SoldierName].Text = pbInfo.SlotID;

                    //string strCountryCode = this.m_frmMain.GetCountryCode(pbInfo.Ip);
                    if (this.Main.iglFlags.Images.ContainsKey(pbInfo.PlayerCountryCode + ".png") == true && this.Players[sapiAdditional.Punkbuster.SoldierName].ImageIndex < 0)
                    {
                        this.Players[pbInfo.SoldierName].ImageIndex = this.Main.iglFlags.Images.IndexOfKey(pbInfo.PlayerCountryCode + ".png");
                    }

                    this.RefreshSelectedPlayer();
                }

                this.PropogatingIndexChange = false;
            });
        }

        /*
        public void OnPlayerPunkbusterInfo(CPunkbusterInfo pbInfo) {


        }
        */

        private void m_prcClient_ListPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            this.InvokeIfRequired(() =>
            {
                if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
                {
                    foreach (CPlayerInfo cpiPlayer in lstPlayers)
                    {
                        if (this.Players.ContainsKey(cpiPlayer.SoldierName) == true)
                        {

                            ListViewItem playerListItem = this.Players[cpiPlayer.SoldierName];

                            if (this.Client != null && this.Client.Game != null && this.Client.Game.HasSquads == true)
                            {
                                if (cpiPlayer.SquadID != uscPlayerListPanel.NeutralSquad)
                                {
                                    if (String.Compare(playerListItem.SubItems["squad"].Text, cpiPlayer.ClanTag) == 0)
                                    {
                                        playerListItem.SubItems["squad"].Text = this.Language.GetLocalized("global.Squad" + cpiPlayer.SquadID.ToString(), null);
                                    }
                                }
                                else
                                {
                                    if (String.IsNullOrEmpty(playerListItem.SubItems["squad"].Text) != false)
                                    {
                                        playerListItem.SubItems["squad"].Text = String.Empty;
                                    }
                                }
                            }

                            if (String.Compare(playerListItem.SubItems["tags"].Text, cpiPlayer.ClanTag) != 0)
                            {
                                playerListItem.SubItems["tags"].Text = cpiPlayer.ClanTag;
                            }

                            if (String.Compare(playerListItem.SubItems["score"].Text, cpiPlayer.Score.ToString()) != 0)
                            {
                                playerListItem.SubItems["score"].Text = cpiPlayer.Score.ToString();
                            }
                            playerListItem.SubItems["kills"].Tag = (Double)cpiPlayer.Kills;
                            if (String.Compare(playerListItem.SubItems["kills"].Text, cpiPlayer.Kills.ToString()) != 0)
                            {
                                playerListItem.SubItems["kills"].Text = cpiPlayer.Kills.ToString();
                            }

                            playerListItem.SubItems["deaths"].Tag = (Double)cpiPlayer.Deaths;
                            if (String.Compare(playerListItem.SubItems["deaths"].Text, cpiPlayer.Kills.ToString()) != 0)
                            {
                                playerListItem.SubItems["deaths"].Text = cpiPlayer.Deaths.ToString();
                            }

                            string kdr = cpiPlayer.Deaths > 0 ? String.Format("{0:0.00}", (Double)cpiPlayer.Kills / (Double)cpiPlayer.Deaths) : String.Format("{0:0.00}", (Double)cpiPlayer.Kills);

                            if (String.Compare(playerListItem.SubItems["kdr"].Text, kdr) == 0)
                            {
                                playerListItem.SubItems["kdr"].Text = kdr;
                            }

                            //if (String.Compare(playerListItem.SubItems["ping"].Text, cpiPlayer.Ping.ToString()) != 0) { playerListItem.SubItems["ping"].Text = cpiPlayer.Ping.ToString(); }
                            if ((this.Client.Game.GameType.Equals("BF3") == true) && (this.Pings.ContainsKey(cpiPlayer.SoldierName) == true))
                            {
                                if (String.Compare(playerListItem.SubItems["ping"].Text, this.Pings[cpiPlayer.SoldierName].ToString()) != 0)
                                {
                                    playerListItem.SubItems["ping"].Text = this.Pings[cpiPlayer.SoldierName].ToString();
                                    cpiPlayer.Ping = this.Pings[cpiPlayer.SoldierName];
                                }
                            }
                            else
                            {
                                if (String.Compare(playerListItem.SubItems["ping"].Text, cpiPlayer.Ping.ToString()) != 0)
                                {
                                    playerListItem.SubItems["ping"].Text = cpiPlayer.Ping.ToString();
                                }
                            }

                            if (String.Compare(playerListItem.SubItems["rank"].Text, cpiPlayer.Rank.ToString()) != 0)
                            {
                                playerListItem.SubItems["rank"].Text = cpiPlayer.Rank.ToString();
                            }

                            string strTime = cpiPlayer.SessionTime > 60 ? String.Format("{0:0}", cpiPlayer.SessionTime / 60) : "0";
                            playerListItem.SubItems["time"].Text = strTime;

                            if (String.Compare(playerListItem.SubItems["type"].Text, cpiPlayer.Type.ToString()) != 0)
                            {
                                if (cpiPlayer.Type == 0)
                                {
                                    //playerListItem.SubItems["type"].Text = this.m_clocLanguage.GetDefaultLocalized("Player", "uscPlayerListPanel.lsvPlayers.Type.Player", null);
                                    playerListItem.SubItems["type"].Text = String.Empty;
                                }
                                else if (cpiPlayer.Type == 1)
                                {
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Spectator", "uscPlayerListPanel.lsvPlayers.Type.Spectator", null);
                                }
                                else if (cpiPlayer.Type == 2)
                                {
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Commander (PC)", "uscPlayerListPanel.lsvPlayers.Type.CommanderPC", null);
                                }
                                else if (cpiPlayer.Type == 3)
                                {
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Commander (Tablet)", "uscPlayerListPanel.lsvPlayers.Type.CommanderTablet", null);
                                }
                            }

                            if (String.IsNullOrEmpty(cpiPlayer.GUID) == false)
                            {
                                if (this.DeveloperUids.Contains(cpiPlayer.GUID.ToLowerInvariant()))
                                {
                                    playerListItem.ForeColor = Color.CornflowerBlue;
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Procon Developer", "uscPlayerListPanel.lsvPlayers.Type.Developer", null);
                                }
                                else if (this.StaffUids.Contains(cpiPlayer.GUID.ToLowerInvariant()))
                                {
                                    playerListItem.ForeColor = Color.DeepSkyBlue;
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Myrcon Staff", "uscPlayerListPanel.lsvPlayers.Type.Staff", null);
                                }
                                else if (this.PluginDeveloperUids.Contains(cpiPlayer.GUID.ToLowerInvariant()))
                                {
                                    playerListItem.ForeColor = Color.LightSkyBlue;
                                    playerListItem.SubItems["type"].Text = this.Language.GetDefaultLocalized("Plugin Developer", "uscPlayerListPanel.lsvPlayers.Type.PluginDeveloper", null);
                                }
                                else if (this.Client.ReservedSlotList.Contains(cpiPlayer.SoldierName))
                                {
                                    playerListItem.ForeColor = Color.FromArgb(0,200,200);
                                    playerListItem.SubItems["type"].Text = "Reserved Slot";
                                }
                                else if (playerListItem.ForeColor != SystemColors.WindowText)
                                {
                                    // reserver slot players are colored in player list panel
                                    playerListItem.ForeColor = SystemColors.WindowText;
                                }
                            }

                            AdditionalPlayerInfo sapiAdditional;

                            if (playerListItem.Tag == null)
                            {
                                sapiAdditional = new AdditionalPlayerInfo();
                                sapiAdditional.ResolvedHostName = String.Empty;
                            }
                            else
                            {
                                sapiAdditional = (AdditionalPlayerInfo)playerListItem.Tag;
                            }

                            sapiAdditional.Player = cpiPlayer;
                            playerListItem.Tag = sapiAdditional;
                        }
                        else
                        {
                            this.Players.Add(cpiPlayer.SoldierName, this.CreatePlayer(cpiPlayer));
                        }
                    }

                    List<string> lstKeys = new List<string>(this.Players.Keys);

                    for (int i = 0; i < lstKeys.Count; i++)
                    {
                        bool blFoundPlayer = false;

                        foreach (CPlayerInfo cpiPlayer in lstPlayers)
                        {
                            if (String.Compare(cpiPlayer.SoldierName, this.Players[lstKeys[i]].Name) == 0)
                            {
                                blFoundPlayer = true;
                                break;
                            }
                        }

                        if (blFoundPlayer == false)
                        {
                            this.Players[lstKeys[i]].Remove();
                            this.Players.Remove(lstKeys[i]);
                        }
                    }

                    this.Players.Add("procon.playerlist.totals1", this.CreateTotalsPlayer(new CPlayerInfo("Totals", "procon.playerlist.totals1", 1, 0), 1));
                    this.Players.Add("procon.playerlist.totals2", this.CreateTotalsPlayer(new CPlayerInfo("Totals", "procon.playerlist.totals2", 2, 0), 2));
                    this.Players.Add("procon.playerlist.totals3", this.CreateTotalsPlayer(new CPlayerInfo("Totals", "procon.playerlist.totals3", 3, 0), 3));
                    this.Players.Add("procon.playerlist.totals4", this.CreateTotalsPlayer(new CPlayerInfo("Totals", "procon.playerlist.totals4", 4, 0), 4));

                    this.Players.Add("procon.playerlist.averages1", this.CreateTotalsPlayer(new CPlayerInfo("Averages", "procon.playerlist.averages1", 1, 0), 1));
                    this.Players.Add("procon.playerlist.averages2", this.CreateTotalsPlayer(new CPlayerInfo("Averages", "procon.playerlist.averages2", 2, 0), 2));
                    this.Players.Add("procon.playerlist.averages3", this.CreateTotalsPlayer(new CPlayerInfo("Averages", "procon.playerlist.averages3", 3, 0), 3));
                    this.Players.Add("procon.playerlist.averages4", this.CreateTotalsPlayer(new CPlayerInfo("Averages", "procon.playerlist.averages4", 4, 0), 4));

                    this.ArrangePlayers();
                }
            });
        }

        private void Game_PlayerPingedByAdmin(FrostbiteClient sender, string soldierName, int ping)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.Players.ContainsKey(soldierName) == true)
                {

                    if (this.Pings.ContainsKey(soldierName) == true)
                    {
                        this.Pings[soldierName] = ping;
                    }
                    else
                    {
                        this.Pings.Add(soldierName, ping);
                    }
                }
            });
        }

        private void Game_PlayerPingedByAdmin(PRoConClient sender, string soldierName, int ping)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.Players.ContainsKey(soldierName) == true)
                {

                    if (this.Pings.ContainsKey(soldierName) == true)
                    {
                        this.Pings[soldierName] = ping;
                    }
                    else
                    {
                        this.Pings.Add(soldierName, ping);
                    }
                }
            });
        }

        /*
        public void OnPlayerList(List<CPlayerInfo> lstPlayers) {


        }
        */

        private void m_prcClient_LevelStarted(FrostbiteClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
                {
                    kvpPlayer.Value.SubItems["score"].Text = String.Empty;
                    kvpPlayer.Value.SubItems["kills"].Tag = new Double();
                    kvpPlayer.Value.SubItems["kills"].Text = String.Empty;
                    kvpPlayer.Value.SubItems["deaths"].Tag = new Double();
                    kvpPlayer.Value.SubItems["deaths"].Text = String.Empty;
                    kvpPlayer.Value.SubItems["kdr"].Text = String.Empty;
                    kvpPlayer.Value.SubItems["kit"].Text = String.Empty;

                    if (kvpPlayer.Value.ImageIndex >= 0)
                    {
                        kvpPlayer.Value.ImageIndex = this.Main.iglFlags.Images.IndexOfKey("flag_death.png");
                    }

                    if (kvpPlayer.Value.Tag != null)
                    {
                        ((AdditionalPlayerInfo)kvpPlayer.Value.Tag).SpawnedInventory = null;
                    }
                }
            });
        }

        private void chkPlayerListShowTeams_CheckedChanged(object sender, EventArgs e)
        {
            this.lsvTeamOnePlayers.ShowGroups = this.lsvTeamTwoPlayers.ShowGroups = this.lsvTeamThreePlayers.ShowGroups = this.lsvTeamFourPlayers.ShowGroups = this.chkPlayerListShowTeams.Checked;

            this.ArrangePlayers();
        }

        private void btnCloseAdditionalInfo_Click(object sender, EventArgs e)
        {
            this.SelectNoPlayer();

            this.ClearPunishmentPanel();
        }

        private void uscPlayerListPanel_Resize(object sender, EventArgs e)
        {

            this.lsvTeamOnePlayers.Scrollable = false;
            this.lsvTeamTwoPlayers.Scrollable = false;
            this.lsvTeamThreePlayers.Scrollable = false;
            this.lsvTeamFourPlayers.Scrollable = false;

            this.SetSplitterDistances();

            this.lsvTeamOnePlayers.Scrollable = true;
            this.lsvTeamTwoPlayers.Scrollable = true;
            this.lsvTeamThreePlayers.Scrollable = true;
            this.lsvTeamFourPlayers.Scrollable = true;
        }

        private void ClearPunishmentPanel()
        {

            if (this.PropogatingIndexChange == false)
            {

                this.kbpBfbcPunishPanel.SoldierName = String.Empty;
                this.kbpPunkbusterPunishPanel.SoldierName = String.Empty;

                this.txtPlayerListSelectedIP.Text = String.Empty;
                this.lblPlayerListSelectedName.Text = String.Empty;
                this.txtPlayerListSelectedGUID.Text = String.Empty;
                this.txtPlayerListSelectedBc2GUID.Text = String.Empty;

                this.pnlAdditionalInfo.Enabled = false;
                this.tbcCourtMartial.Enabled = false;

                this.spltListAdditionalInfo.Panel2Collapsed = true;
                this.btnCloseAdditionalInfo.Enabled = false;
            }
        }

        private void SelectPlayer(string strPlayerName)
        {

            this.PropogatingIndexChange = true;

            foreach (ListViewItem lviPlayer in this.lsvTeamOnePlayers.Items)
            {
                lviPlayer.Selected = System.String.CompareOrdinal(lviPlayer.Name, strPlayerName) == 0;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamTwoPlayers.Items)
            {
                lviPlayer.Selected = System.String.CompareOrdinal(lviPlayer.Name, strPlayerName) == 0;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamThreePlayers.Items)
            {
                lviPlayer.Selected = System.String.CompareOrdinal(lviPlayer.Name, strPlayerName) == 0;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamFourPlayers.Items)
            {
                lviPlayer.Selected = System.String.CompareOrdinal(lviPlayer.Name, strPlayerName) == 0;
            }

            this.PropogatingIndexChange = false;
        }

        private void SelectNoPlayer()
        {
            this.PropogatingIndexChange = true;

            foreach (ListViewItem lviPlayer in this.lsvTeamOnePlayers.Items)
            {
                lviPlayer.Selected = false;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamTwoPlayers.Items)
            {
                lviPlayer.Selected = false;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamThreePlayers.Items)
            {
                lviPlayer.Selected = false;
            }

            foreach (ListViewItem lviPlayer in this.lsvTeamFourPlayers.Items)
            {
                lviPlayer.Selected = false;
            }

            this.PropogatingIndexChange = false;

            this.ClearPunishmentPanel();
        }

        private void RefreshSelectedPlayer()
        {
            if (this.lsvTeamOnePlayers.SelectedItems.Count > 0)
            {
                this.lsvPlayers_SelectedIndexChanged(this.lsvTeamOnePlayers, null);
            }
            else if (this.lsvTeamTwoPlayers.SelectedItems.Count > 0)
            {
                this.lsvPlayers_SelectedIndexChanged(this.lsvTeamTwoPlayers, null);
            }
            else if (this.lsvTeamThreePlayers.SelectedItems.Count > 0)
            {
                this.lsvPlayers_SelectedIndexChanged(this.lsvTeamThreePlayers, null);
            }
            else if (this.lsvTeamFourPlayers.SelectedItems.Count > 0)
            {
                this.lsvPlayers_SelectedIndexChanged(this.lsvTeamFourPlayers, null);
            }
            else
            {
                this.ClearPunishmentPanel();
            }
        }

        private void lsvPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems.Count > 0 && this.ColumnSorter.TotalsAveragesChecker.IsMatch(((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Name) == false)
            {

                if (this.PropogatingIndexChange == false)
                {
                    this.SelectPlayer(((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Name);
                }

                if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems.Count > 0)
                {

                    this.spltListAdditionalInfo.Panel2Collapsed = false;
                    this.spltInfoPunish.Panel1MinSize = 306;
                    this.spltInfoPunish.Panel2MinSize = 191;
                    this.spltInfoPunish.FixedPanel = FixedPanel.Panel2;
                    this.btnCloseAdditionalInfo.Enabled = true;

                    this.kbpBfbcPunishPanel.SoldierName = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Name;
                    this.kbpPunkbusterPunishPanel.SoldierName = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Name;
                    this.tbcCourtMartial.Enabled = true;

                    //PunkbusterInfo pbInfo = null;
                    //string strCountryName = String.Empty;

                    AdditionalPlayerInfo sapiAdditional = (AdditionalPlayerInfo)((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Tag;

                    if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Tag != null && sapiAdditional.Punkbuster != null)
                    {

                        //string strResolvedHost = (string)((object[])this.lsvPlayers.SelectedItems[0].Tag)[2];

                        this.lblPlayerListSelectedName.Text = String.Format("{0} {1} ({2})", ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].SubItems["tags"].Text, ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].SubItems["soldiername"].Text, sapiAdditional.Punkbuster.PlayerCountry);

                        // new string[] { strID, strSoldierName, strGUID, strIP, this.m_frmParent.GetCountryName(strIP) }
                        this.txtPlayerListSelectedGUID.Text = sapiAdditional.Punkbuster.GUID;

                        string[] a_strSplitIp = sapiAdditional.Punkbuster.Ip.Split(':');

                        if (sapiAdditional.ResolvedHostName.Length > 0)
                        {
                            this.txtPlayerListSelectedIP.Text = String.Format("{0} ({1})", sapiAdditional.Punkbuster.Ip, sapiAdditional.ResolvedHostName);
                        }
                        else
                        {
                            if (this.ResolvePlayerHost() == true && a_strSplitIp.Length >= 1)
                            {
                                try
                                {
                                    SResolvePlayerIP srpResolve = new SResolvePlayerIP();
                                    srpResolve.lviPlayer = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0];
                                    srpResolve.plpListPanel = this;

                                    Dns.BeginGetHostEntry(IPAddress.Parse(a_strSplitIp[0]), m_asyncResolvePlayerIP, srpResolve);
                                }
                                catch (Exception) { }
                            }

                            this.txtPlayerListSelectedIP.Text = sapiAdditional.Punkbuster.Ip;
                        }

                        this.pnlAdditionalInfo.Enabled = true;

                        this.kbpPunkbusterPunishPanel.SlotID = sapiAdditional.Punkbuster.SlotID;
                        this.kbpPunkbusterPunishPanel.IP = a_strSplitIp.Length > 0 ? a_strSplitIp[0] : String.Empty;
                        this.kbpPunkbusterPunishPanel.GUID = sapiAdditional.Punkbuster.GUID;
                        this.kbpBfbcPunishPanel.IP = a_strSplitIp.Length > 0 ? a_strSplitIp[0] : String.Empty;

                        this.kbpPunkbusterPunishPanel.Enabled = true && (!this.Privileges.CannotPunishPlayers && this.Privileges.CanIssueLimitedPunkbusterCommands);
                    }
                    else
                    {
                        this.lblPlayerListSelectedName.Text = String.Format("{0} {1}", ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].SubItems["tags"].Text, ((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].SubItems["soldiername"].Text);
                        //this.tabCourtMartialPunkbuster.Hide();
                        this.txtPlayerListSelectedGUID.Text = String.Empty;
                        this.txtPlayerListSelectedIP.Text = String.Empty;

                        this.kbpPunkbusterPunishPanel.SlotID = String.Empty;
                        this.kbpPunkbusterPunishPanel.IP = String.Empty;
                        this.kbpPunkbusterPunishPanel.GUID = String.Empty;
                        this.kbpBfbcPunishPanel.IP = String.Empty;
                        this.kbpPunkbusterPunishPanel.Enabled = false;
                    }

                    if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Tag != null && sapiAdditional.Player != null)
                    {
                        this.txtPlayerListSelectedBc2GUID.Text = sapiAdditional.Player.GUID;
                        this.kbpBfbcPunishPanel.GUID = sapiAdditional.Player.GUID;

                        this.pnlAdditionalInfo.Enabled = true;
                    }
                    else
                    {
                        this.txtPlayerListSelectedBc2GUID.Text = String.Empty;
                        this.kbpBfbcPunishPanel.GUID = String.Empty;
                    }

                    if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).SelectedItems[0].Tag != null && sapiAdditional.SpawnedInventory != null)
                    {

                        //List<string> inventory = new List<string>();

                        string[] inventory = new string[6];

                        foreach (Weapon weapon in sapiAdditional.SpawnedInventory.Weapons)
                        {

                            int weaponSlot = -1;

                            if (weapon.Slot == WeaponSlots.Primary)
                            {
                                weaponSlot = 0;
                            }
                            else if (weapon.Slot == WeaponSlots.Auxiliary)
                            {
                                weaponSlot = 1;
                            }
                            else if (weapon.Slot == WeaponSlots.Secondary)
                            {
                                weaponSlot = 2;
                            }

                            if (weaponSlot >= 0)
                            {
                                inventory[weaponSlot] = this.Language.GetLocalized(String.Format("global.Weapons.{0}", weapon.Name.ToLower()));
                            }
                        }

                        int specializationSlot = 3;

                        foreach (Specialization spec in sapiAdditional.SpawnedInventory.Specializations)
                        {
                            inventory[specializationSlot++] = this.Language.GetLocalized(String.Format("global.Specialization.{0}", spec.Name));
                        }

                        List<string> inventoryList = new List<string>(inventory);
                        inventoryList.RemoveAll(String.IsNullOrEmpty);
                        this.lblPlayersInventory.Text = String.Join(", ", inventoryList.ToArray());
                    }
                    else
                    {
                        this.lblPlayersInventory.Text = String.Empty;
                    }

                }
            }
            else if ((((PRoCon.Controls.ControlsEx.ListViewNF)sender).FocusedItem != null || ((((PRoCon.Controls.ControlsEx.ListViewNF)sender).FocusedItem != null && this.ColumnSorter.TotalsAveragesChecker.IsMatch(((PRoCon.Controls.ControlsEx.ListViewNF)sender).FocusedItem.Name) == true) && this.PropogatingIndexChange == false)))
            {
                this.SelectNoPlayer();
            }

            this.kbpBfbcPunishPanel.RefreshPanel();
            this.kbpPunkbusterPunishPanel.RefreshPanel();
        }

        private void m_prcClient_PlayerChangedTeam(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            this.InvokeIfRequired(() =>
            {
                this.PropogatingIndexChange = true;

                lock (this.PlayerDictionaryLocker)
                {

                    if (this.Players.ContainsKey(strSoldierName) == true)
                    {
                        SetPlayerTeamID(this.Players[strSoldierName], iTeamID);

                        this.ArrangePlayers();
                        // Save the SquadChange event for onSquadChange
                        //this.SetPlayerSquadID(this.m_dicPlayers[strSoldierName], iSquadID);
                    }

                }

                this.PropogatingIndexChange = false;
            });
        }

        private void m_prcClient_PlayerChangedSquad(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            this.InvokeIfRequired(() =>
            {
                lock (this.PlayerDictionaryLocker)
                {
                    if (this.Players.ContainsKey(strSoldierName) == true)
                    {
                        SetPlayerTeamID(this.Players[strSoldierName], iTeamID);
                        this.SetPlayerSquadID(this.Players[strSoldierName], iSquadID);
                    }
                }
            });
        }

        // Called by all three lists..
        private void lsvPlayers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.ColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.ColumnSorter.Order == SortOrder.Ascending)
                {
                    this.ColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    this.ColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.ColumnSorter.SortColumn = e.Column;
                this.ColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvTeamOnePlayers.Sort();
            this.lsvTeamTwoPlayers.Sort();
            this.lsvTeamThreePlayers.Sort();
            this.lsvTeamFourPlayers.Sort();

            //this.lsvTeamOnePlayers.
        }

        private AsyncCallback m_asyncResolvePlayerIP = new AsyncCallback(uscPlayerListPanel.ResolvePlayerIP);

        private struct SResolvePlayerIP
        {
            public ListViewItem lviPlayer;
            public uscPlayerListPanel plpListPanel;
        }

        public delegate void PlayerIPResolvedDelegate(ListViewItem lviPlayer, string strHostName);
        private void PlayerIPResolved(ListViewItem lviPlayer, string strHostName)
        {

            AdditionalPlayerInfo sapiAdditional = (AdditionalPlayerInfo)lviPlayer.Tag;
            sapiAdditional.ResolvedHostName = strHostName;
            lviPlayer.Tag = sapiAdditional;

            this.RefreshSelectedPlayer();
        }

        private static void ResolvePlayerIP(IAsyncResult ar)
        {

            try
            {
                if (ar != null)
                {
                    SResolvePlayerIP srpResolve = (SResolvePlayerIP)ar.AsyncState;
                    IPHostEntry ipHost = Dns.EndGetHostEntry(ar);
                    srpResolve.plpListPanel.Invoke(new PlayerIPResolvedDelegate(srpResolve.plpListPanel.PlayerIPResolved), new object[] { srpResolve.lviPlayer, ipHost.HostName });
                }
            }
            catch (Exception) { }
        }

        public bool ResolvePlayerHost()
        {
            return this.Client.Variables.GetVariable("RESOLVE_PLAYER_HOST", false);
        }

        private void btnSplitTeams_Click(object sender, EventArgs e)
        {
            if (this.Client != null)
            {
                switch (this.Client.PlayerListSettings.SplitType)
                {
                    case 1:
                        this.Client.PlayerListSettings.SplitType = 2;
                        break;
                    case 2:
                        this.Client.PlayerListSettings.SplitType = 4;
                        break;
                    case 4:
                        this.Client.PlayerListSettings.SplitType = 1;
                        break;
                }

                this.ArrangePlayers();
            }
        }

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kill)
        {
            this.InvokeIfRequired(() =>
            {
                lock (this.PlayerDictionaryLocker)
                {
                    var killer = this.Players.ContainsKey(kill.Killer.SoldierName) == true ? this.Players[kill.Killer.SoldierName] : null;
                    var victim = this.Players.ContainsKey(kill.Victim.SoldierName) == true ? this.Players[kill.Victim.SoldierName] : null;

                    // Don't award a kill for a suicide
                    if (killer != null && kill.IsSuicide == false)
                    {
                        var tag = killer.Tag as AdditionalPlayerInfo;

                        if (tag != null)
                        {
                            if (killer.Tag != null && tag.Player != null)
                            {
                                tag.Player.Kills++;
                            }

                            killer.SubItems["kills"].Tag = ((Double)killer.SubItems["kills"].Tag) + 1;
                            killer.SubItems["kills"].Text = ((Double)killer.SubItems["kills"].Tag).ToString(CultureInfo.InvariantCulture);

                            killer.SubItems["kdr"].Tag = kill;

                            if (killer.SubItems["deaths"].Tag != null && (Double)killer.SubItems["deaths"].Tag > 0)
                            {
                                killer.SubItems["kdr"].Text = String.Format("{0:0.00}", ((Double)killer.SubItems["kills"].Tag / (Double)killer.SubItems["deaths"].Tag));
                            }
                            else
                            {
                                killer.SubItems["kdr"].Text = String.Format("{0:0.00}", (Double)killer.SubItems["kills"].Tag);
                            }

                            if (tag.Player != null)
                            {
                                this.AddKillToTeamTotal(tag.Player.TeamID);
                            }
                        }

                    }

                    if (victim != null)
                    {
                        var tag = victim.Tag as AdditionalPlayerInfo;

                        if (tag != null)
                        {
                            if (victim.Tag != null && tag.Player != null)
                            {
                                tag.Player.Deaths++;
                            }

                            victim.SubItems["deaths"].Tag = (Double)victim.SubItems["deaths"].Tag + 1;
                            victim.SubItems["deaths"].Text = ((Double)victim.SubItems["deaths"].Tag).ToString(CultureInfo.InvariantCulture);

                            victim.SubItems["kdr"].Tag = kill;

                            if (victim.SubItems["deaths"].Tag != null && (Double)victim.SubItems["deaths"].Tag > 0)
                            {
                                victim.SubItems["kdr"].Text = String.Format("{0:0.00}", ((Double)victim.SubItems["kills"].Tag / (Double)victim.SubItems["deaths"].Tag));
                            }
                            else
                            {
                                victim.SubItems["kdr"].Text = String.Format("{0:0.00}", (Double)victim.SubItems["kills"].Tag);
                            }

                            victim.ImageIndex = this.Main.iglFlags.Images.IndexOfKey("flag_death.png");

                            if (tag.Player != null)
                            {
                                this.AddDeathToTeamTotal(tag.Player.TeamID);
                            }
                        }
                    }

                    this.tmrKillDeathHighlight.Enabled = true;
                }

                this.FinalizeTotalsAverages();
                // this.ArrangePlayers();
            });
        }

        private void lsvPlayers_DragDrop(object sender, DragEventArgs e)
        {
            Point pntClient = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).PointToClient(new Point(e.X, e.Y));
            ListViewItem lviHover = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y);

            lviHover = lviHover == null ? ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y - 1) : lviHover;
            lviHover = lviHover == null ? ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y + 1) : lviHover;

            if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).Items.Count > 0 && lviHover == null)
            {
                // This includes the group header (team name)
                lviHover = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y + ((PRoCon.Controls.ControlsEx.ListViewNF)sender).Items[0].Bounds.Height);
            }

            CPlayerInfo cpiSwitchingPlayer = ((CPlayerInfo)e.Data.GetData(typeof(CPlayerInfo)));

            // TO DO: PunishPlayer should be renamed to SendCommand
            if (lviHover != null && lviHover.Tag != null && ((AdditionalPlayerInfo)lviHover.Tag).Player != null && cpiSwitchingPlayer != null && cpiSwitchingPlayer.TeamID != ((AdditionalPlayerInfo)lviHover.Tag).Player.TeamID)
            {
                if (Program.ProconApplication.OptionsSettings.AdminMoveMessage)
                    this.Client.Game.SendAdminSayPacket("You have been moved to another team/squad by an admin.", new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, cpiSwitchingPlayer.SoldierName));
                this.Client.Game.SendAdminMovePlayerPacket(cpiSwitchingPlayer.SoldierName, ((AdditionalPlayerInfo)lviHover.Tag).Player.TeamID, this.Client.GetDefaultSquadIDByMapname(this.Client.CurrentServerInfo.Map), true);

                //this.m_prcClient.SendRequest(new List<string>() { "admin.say", "You have been moved to another team/squad by an admin.", "player", cpiSwitchingPlayer.SoldierName });
                //this.m_prcClient.SendRequest((new List<string>() { "admin.movePlayer", cpiSwitchingPlayer.SoldierName, ((AdditionalPlayerInfo)lviHover.Tag).m_cpiPlayer.TeamID.ToString(), this.m_prcClient.GetDefaultSquadIDByMapname(this.m_prcClient.CurrentServerInfo.Map).ToString(), "true" }));
            }

            this.HoverTeamBackground(null, -1);
        }

        private void lsvPlayers_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private ListViewItem CreatePlaceHolder(PRoCon.Controls.ControlsEx.ListViewNF lsvList, int iTeamID)
        {
            ListViewItem lviPlaceHolder = new ListViewItem(".");
            lviPlaceHolder.ForeColor = SystemColors.WindowText;
            lviPlaceHolder.UseItemStyleForSubItems = true;

            AdditionalPlayerInfo sapiInfo = new AdditionalPlayerInfo();
            sapiInfo.ResolvedHostName = String.Empty;
            sapiInfo.Punkbuster = null;
            sapiInfo.Player = new CPlayerInfo("", String.Empty, iTeamID, 0);
            lviPlaceHolder.Tag = sapiInfo;

            lviPlaceHolder.Group = lsvList.Groups[iTeamID];

            return lviPlaceHolder;
        }

        private void AddTeamPlaceHolders()
        {

            this.PlaceHoldersDrawn = true;

            if (this.Client != null)
            {

                int iTeams = this.Client.GetLocalizedTeamNameCount(this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode);

                if (this.Client.PlayerListSettings.SplitType == 1)
                {
                    for (int i = 1; i < iTeams && i < this.lsvTeamOnePlayers.Groups.Count; i++)
                    {
                        if (this.lsvTeamOnePlayers.Groups[i].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, i));
                        }
                    }
                }
                else if (this.Client.PlayerListSettings.SplitType == 2)
                {

                    if (iTeams == 5)
                    {
                        if (this.lsvTeamOnePlayers.Groups[1].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, 1));
                        }

                        if (this.lsvTeamOnePlayers.Groups[3].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, 3));
                        }

                        if (this.lsvTeamTwoPlayers.Groups[2].Items.Count <= 0)
                        {
                            this.lsvTeamTwoPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamTwoPlayers, 2));
                        }

                        if (this.lsvTeamTwoPlayers.Groups[4].Items.Count <= 0)
                        {
                            this.lsvTeamTwoPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamTwoPlayers, 4));
                        }
                    }
                    else if (iTeams == 3)
                    {
                        if (this.lsvTeamOnePlayers.Groups[1].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, 1));
                        }

                        if (this.lsvTeamTwoPlayers.Groups[2].Items.Count <= 0)
                        {
                            this.lsvTeamTwoPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamTwoPlayers, 2));
                        }
                    }
                }
                else if (this.Client.PlayerListSettings.SplitType == 4)
                {
                    if (iTeams == 5)
                    {
                        if (this.lsvTeamOnePlayers.Groups[1].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, 1));
                        }

                        if (this.lsvTeamTwoPlayers.Groups[2].Items.Count <= 0)
                        {
                            this.lsvTeamTwoPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamTwoPlayers, 2));
                        }

                        if (this.lsvTeamThreePlayers.Groups[3].Items.Count <= 0)
                        {
                            this.lsvTeamThreePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamThreePlayers, 3));
                        }

                        if (this.lsvTeamFourPlayers.Groups[4].Items.Count <= 0)
                        {
                            this.lsvTeamFourPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamFourPlayers, 4));
                        }
                    }
                    else if (iTeams == 3)
                    {
                        if (this.lsvTeamOnePlayers.Groups[1].Items.Count <= 0)
                        {
                            this.lsvTeamOnePlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamOnePlayers, 1));
                        }

                        if (this.lsvTeamTwoPlayers.Groups[2].Items.Count <= 0)
                        {
                            this.lsvTeamTwoPlayers.Items.Add(this.CreatePlaceHolder(this.lsvTeamTwoPlayers, 2));
                        }
                    }
                }
            }
        }

        private void RemovePlaceHolders(PRoCon.Controls.ControlsEx.ListViewNF lsvList)
        {
            for (int i = 0; i < lsvList.Items.Count; i++)
            {
                if (String.Compare(lsvList.Items[i].Name, String.Empty) == 0)
                {
                    lsvList.Items[i].Remove();
                    i--;
                }
            }

            this.PlaceHoldersDrawn = false;

            //this.ArrangePlayers();
        }

        public void BeginDragDrop()
        {
            this.AddTeamPlaceHolders();
        }

        public void EndDragDrop()
        {
            this.RemovePlaceHolders(this.lsvTeamOnePlayers);
            this.RemovePlaceHolders(this.lsvTeamTwoPlayers);
            this.RemovePlaceHolders(this.lsvTeamThreePlayers);
            this.RemovePlaceHolders(this.lsvTeamFourPlayers);
        }

        private void lsvPlayers_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lviSelected = (ListViewItem)e.Item;

            if (e.Button == MouseButtons.Left)
            {

                if (lviSelected != null && lviSelected.Tag != null && ((AdditionalPlayerInfo)lviSelected.Tag).Player != null && this.ColumnSorter.TotalsAveragesChecker.IsMatch(lviSelected.Name) == false)
                {

                    if (this.ConnectionPanel.BeginDragDrop() == true)
                    {
                        ((PRoCon.Controls.ControlsEx.ListViewNF)sender).DoDragDrop(((AdditionalPlayerInfo)lviSelected.Tag).Player, DragDropEffects.None | DragDropEffects.Move);

                        this.ConnectionPanel.EndDragDrop();
                    }
                }
            }
        }

        private void SetPlaceHoldersColours(PRoCon.Controls.ControlsEx.ListViewNF lsvList, ListViewItem lviIgnoreItem, Color clForeColor, Color clBackColor)
        {
            for (int i = 0; i < lsvList.Items.Count; i++)
            {
                if (String.Compare(lsvList.Items[i].Name, String.Empty) == 0 && lsvList.Items[i] != lviIgnoreItem)
                {
                    lsvList.Items[i].BackColor = clBackColor;
                    lsvList.Items[i].ForeColor = clForeColor;
                }
            }
        }

        private void HoverTeamBackground(ListViewItem lviReserved, int iTeamID)
        {

            Color clLighLightHighlight = ControlPaint.LightLight(SystemColors.Highlight);

            foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
            {
                if (kvpPlayer.Value.Tag != null && ((AdditionalPlayerInfo)kvpPlayer.Value.Tag).Player != null)
                {
                    if (((AdditionalPlayerInfo)kvpPlayer.Value.Tag).Player.TeamID == iTeamID)
                    {
                        kvpPlayer.Value.BackColor = clLighLightHighlight;
                        kvpPlayer.Value.ForeColor = SystemColors.HighlightText;
                    }
                    else
                    {
                        kvpPlayer.Value.BackColor = SystemColors.Window;
                        kvpPlayer.Value.ForeColor = SystemColors.WindowText;
                    }
                }
            }

            if (lviReserved != null && String.Compare(lviReserved.Name, String.Empty) == 0)
            {
                lviReserved.BackColor = clLighLightHighlight;
                lviReserved.ForeColor = clLighLightHighlight;
            }

            this.SetPlaceHoldersColours(this.lsvTeamOnePlayers, lviReserved, SystemColors.Window, SystemColors.Window);
            this.SetPlaceHoldersColours(this.lsvTeamTwoPlayers, lviReserved, SystemColors.Window, SystemColors.Window);
            this.SetPlaceHoldersColours(this.lsvTeamThreePlayers, lviReserved, SystemColors.Window, SystemColors.Window);
            this.SetPlaceHoldersColours(this.lsvTeamFourPlayers, lviReserved, SystemColors.Window, SystemColors.Window);
        }

        private void lsvPlayers_DragOver(object sender, DragEventArgs e)
        {

            Point pntClient = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).PointToClient(new Point(e.X, e.Y));
            ListViewItem lviHover = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y);

            lviHover = lviHover == null ? ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y - 1) : lviHover;
            lviHover = lviHover == null ? ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y + 1) : lviHover;

            if (((PRoCon.Controls.ControlsEx.ListViewNF)sender).Items.Count > 0 && lviHover == null)
            {
                // This includes the group header (team name)
                lviHover = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(pntClient.X, pntClient.Y + ((PRoCon.Controls.ControlsEx.ListViewNF)sender).Items[0].Bounds.Height);
            }

            if (lviHover == null)
            {
                e.Effect = DragDropEffects.None;
                this.HoverTeamBackground(null, -1);
            }
            else
            {
                if (lviHover.Tag != null && ((AdditionalPlayerInfo)lviHover.Tag).Player != null && ((CPlayerInfo)e.Data.GetData(typeof(CPlayerInfo))).TeamID != ((AdditionalPlayerInfo)lviHover.Tag).Player.TeamID)
                {
                    e.Effect = DragDropEffects.Move;

                    this.HoverTeamBackground(lviHover, ((AdditionalPlayerInfo)lviHover.Tag).Player.TeamID);
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    this.HoverTeamBackground(lviHover, -1);
                }
            }
        }

        private void PlayerListSettings_TwoSplitterPercentageChanged(float percentage)
        {
            this.SetSplitterDistances();
        }

        private void PlayerListSettings_FourSplitterPercentageChanged(float percentage)
        {
            this.SetSplitterDistances();
        }

        private bool m_isSettingSlaveSplitter;

        private void spltTwoSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.Client != null && this.IsSplitterBeingSet == false && this.m_isSettingSlaveSplitter == false)
            {
                this.Client.PlayerListSettings.TwoSplitterPercentage = (float)e.SplitX / (float)this.spltTwoSplit.Width;
            }
        }

        private void spltBottomTwoSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.Client != null && this.IsSplitterBeingSet == false && this.m_isSettingSlaveSplitter == false)
            {
                //    this.m_prcClient.PlayerListSettings.TwoSplitterPercentage = (float)e.SplitX / (float)this.spltBottomTwoSplit.Width;
                this.m_isSettingSlaveSplitter = true;
                this.spltTwoSplit.SplitterDistance = e.SplitX;
                this.m_isSettingSlaveSplitter = false;
            }
        }

        private void spltFourSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.Client != null && this.IsSplitterBeingSet == false)
            {
                this.Client.PlayerListSettings.FourSplitterPercentage = (float)this.spltFourSplit.SplitterDistance / (float)this.spltFourSplit.Height;
            }
        }

        private void tmrKillDeathHighlight_Tick(object sender, EventArgs e)
        {

            if (this.PlaceHoldersDrawn == false)
            {

                lock (this.PlayerDictionaryLocker)
                {

                    bool isStillFadingKill = false;

                    foreach (KeyValuePair<string, ListViewItem> kvpPlayer in this.Players)
                    {
                        if (kvpPlayer.Value.SubItems["kdr"].Tag != null && kvpPlayer.Value.SubItems["kdr"].Tag is Kill)
                        {

                            TimeSpan tsDifference = DateTime.Now - ((Kill)kvpPlayer.Value.SubItems["kdr"].Tag).TimeOfDeath;

                            float Opacity = ((5000.0F - (float)tsDifference.TotalMilliseconds) / 5000.0F) * 0.5F;

                            if (Opacity <= 0.0F)
                            {
                                kvpPlayer.Value.SubItems["kdr"].Tag = null;
                                kvpPlayer.Value.BackColor = SystemColors.Window;
                            }
                            else if (Opacity > 0.0F && Opacity <= 1.0F)
                            {
                                if (String.Compare(kvpPlayer.Key, ((Kill)kvpPlayer.Value.SubItems["kdr"].Tag).Victim.SoldierName) == 0)
                                {
                                    kvpPlayer.Value.BackColor = Color.FromArgb((int)((Color.Maroon.R - 255) * Opacity + 255), (int)((Color.Maroon.G - 255) * Opacity + 255), (int)((Color.Maroon.B - 255) * Opacity + 255));
                                }
                                else
                                {
                                    kvpPlayer.Value.BackColor = Color.FromArgb((int)((Color.LightSeaGreen.R - 255) * Opacity + 255), (int)((Color.LightSeaGreen.G - 255) * Opacity + 255), (int)((Color.LightSeaGreen.B - 255) * Opacity + 255));
                                    //kvpPlayer.Value.BackColor = Color.FromArgb((int)(255 * Opacity), Color.Maroon);
                                }

                                isStillFadingKill = true;
                            }
                        }
                    }

                    if (isStillFadingKill == false)
                    {
                        this.tmrKillDeathHighlight.Enabled = false;
                    }
                }
            }
        }

        #region Right Click Context Menu

        private void lsvPlayers_MouseDown(object sender, MouseEventArgs e)
        {

            //Point pntClient = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).PointToClient(new Point(e.X, e.Y));
            ListViewItem lviSelected = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {

                if (lviSelected == null)
                {
                    this.SelectNoPlayer();
                }
                else
                {
                    if (this.ColumnSorter.TotalsAveragesChecker.IsMatch(lviSelected.Name) == true)
                    {
                        this.SelectNoPlayer();
                    }
                    //else {
                    //    this.SelectPlayer(lviSelected.Name);
                    //}
                }
            }
            else if (e.Button == MouseButtons.Right && lviSelected != null && lviSelected.Tag != null && lviSelected.Tag is AdditionalPlayerInfo)
            {

                CPlayerInfo player = ((AdditionalPlayerInfo)lviSelected.Tag).Player;

                if (player != null && lviSelected != null && this.ColumnSorter.TotalsAveragesChecker.IsMatch(lviSelected.Name) == false)
                {

                    this.moveToSquadToolStripMenuItem.Text = this.Language.GetDefaultLocalized("Move Player to..", "uscPlayerListPanel.ctxPlayerOptions.moveToSquadToolStripMenuItem", player.SoldierName);
                    this.moveToSquadToolStripMenuItem.DropDownItems.Clear();

                    foreach (CTeamName team in this.Client.TeamNameList)
                    {
                        if (this.Client.GameType == "BFBC2" || this.Client.GameType == "MoH")
                        {
                            if (String.Compare(team.MapFilename, this.Client.CurrentServerInfo.Map, true) == 0 && team.TeamID != uscPlayerListPanel.NeutralTeam)
                            {
                                ToolStripMenuItem teamChange = new ToolStripMenuItem(this.Language.GetDefaultLocalized(String.Format("Team {0}", team.TeamID), "uscPlayerListPanel.ctxPlayerOptions.moveToSquadToolStripMenuItem.Team", this.Language.GetLocalized(team.LocalizationKey)));
                                teamChange.Tag = new object[] { player, team };
                                teamChange.Click += new EventHandler(teamChange_Click);
                                if (team.TeamID == player.TeamID)
                                {
                                    teamChange.Checked = true;
                                    teamChange.Enabled = false;
                                }
                                this.moveToSquadToolStripMenuItem.DropDownItems.Add(teamChange);
                            }
                        }
                        else
                        {
                            if (String.Compare(team.MapFilename, this.Client.CurrentServerInfo.Map, true) == 0 && String.Compare(team.Playlist, this.Client.CurrentServerInfo.GameMode, true) == 0 && team.TeamID != uscPlayerListPanel.NeutralTeam)
                            {

                                ToolStripMenuItem teamChange = new ToolStripMenuItem(this.Language.GetDefaultLocalized(String.Format("Team {0}", team.TeamID), "uscPlayerListPanel.ctxPlayerOptions.moveToSquadToolStripMenuItem.Team", this.Language.GetLocalized(team.LocalizationKey)));
                                teamChange.Tag = new object[] { player, team };
                                teamChange.Click += new EventHandler(teamChange_Click);

                                if (team.TeamID == player.TeamID)
                                {
                                    teamChange.Checked = true;
                                    teamChange.Enabled = false;
                                }

                                this.moveToSquadToolStripMenuItem.DropDownItems.Add(teamChange);
                            }
                        }
                    }
                    // uscPlayerListPanel.INT_NEUTRAL_TEAM
                    if (this.Client.Game.HasSquads == true)
                    {

                        this.moveToSquadToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

                        int iMaxSquadID = 8;
                        if (this.Client.GameType == "BF3")
                        {
                            iMaxSquadID = 32;
                        }
                        else
                        {
                            iMaxSquadID = 16;
                        }

                        for (int i = 0; i <= iMaxSquadID; i++)
                        {

                            ToolStripMenuItem squadChange = new ToolStripMenuItem(this.Language.GetDefaultLocalized(String.Format("Squad {0}", i), "uscPlayerListPanel.ctxPlayerOptions.moveToSquadToolStripMenuItem.Squad", this.Language.GetLocalized(String.Format("global.Squad{0}", i))));
                            squadChange.Tag = new object[] { player, i };
                            squadChange.Click += new EventHandler(squadChange_Click);

                            if (player.SquadID == i)
                            {
                                squadChange.Checked = true;
                                squadChange.Enabled = false;
                            }

                            this.moveToSquadToolStripMenuItem.DropDownItems.Add(squadChange);
                        }
                    }

                    this.reservedSlotToolStripMenuItem.Checked = this.Client.ReservedSlotList.Contains(player.SoldierName);
                    this.reservedSlotToolStripMenuItem.Tag = player;

                    this.spectatorListToolStripMenuItem.Checked = this.Client.SpectatorList.Contains(player.SoldierName);
                    this.spectatorListToolStripMenuItem.Tag = player;

                    if (this.Client.FullTextChatModerationList.Contains(player.SoldierName) == true)
                    {

                        TextChatModerationEntry entry = this.Client.FullTextChatModerationList[player.SoldierName];

                        this.mutedToolStripMenuItem.Checked = (entry.PlayerModerationLevel == PlayerModerationLevelType.Muted);
                        this.normalToolStripMenuItem.Checked = (entry.PlayerModerationLevel == PlayerModerationLevelType.Normal);
                        this.voiceToolStripMenuItem.Checked = (entry.PlayerModerationLevel == PlayerModerationLevelType.Voice);
                        this.adminToolStripMenuItem.Checked = (entry.PlayerModerationLevel == PlayerModerationLevelType.Admin);
                    }
                    else
                    {
                        this.mutedToolStripMenuItem.Checked = this.voiceToolStripMenuItem.Checked = this.adminToolStripMenuItem.Checked = false;
                        this.normalToolStripMenuItem.Checked = true;
                    }

                    this.mutedToolStripMenuItem.Enabled = !this.mutedToolStripMenuItem.Checked;
                    this.normalToolStripMenuItem.Enabled = !this.normalToolStripMenuItem.Checked;
                    this.voiceToolStripMenuItem.Enabled = !this.voiceToolStripMenuItem.Checked;
                    this.adminToolStripMenuItem.Enabled = !this.adminToolStripMenuItem.Checked;

                    this.statsLookupToolStripMenuItem.Tag = this.mutedToolStripMenuItem.Tag = this.normalToolStripMenuItem.Tag = this.voiceToolStripMenuItem.Tag = this.adminToolStripMenuItem.Tag = player;

                    // this.statsLookupToolStripMenuItem1.Tag = this.statsLookupToolStripMenuItem2.Tag = this.statsLookupToolStripMenuItem3.Tag = this.statsLookupToolStripMenuItem4.Tag = this.statsLookupToolStripMenuItem.Tag;
                    this.statsLookupToolStripMenuItem.DropDownItems.Clear();
                    if (Program.ProconApplication.OptionsSettings.StatsLinkNameUrl.Count > 0)
                    {
                        // _PK_
                        foreach (StatsLinkNameUrl statsLink in Program.ProconApplication.OptionsSettings.StatsLinkNameUrl)
                        {
                            ToolStripMenuItem statsLookup = new ToolStripMenuItem(statsLink.LinkName);
                            statsLookup.Tag = new object[] { player, statsLink.LinkUrl };
                            statsLookup.Click += new EventHandler(statsLookupToolStripMenuItemCustom_Click);
                            this.statsLookupToolStripMenuItem.DropDownItems.Add(statsLookup);
                        }
                    }


                    CPunkbusterInfo pb_player = ((AdditionalPlayerInfo)lviSelected.Tag).Punkbuster;
                    this.punkBusterScreenshotToolStripMenuItem.Tag = pb_player;

                    if (this.Client != null && this.Client.GameType == "MOH")
                    {
                        this.reservedSlotToolStripMenuItem.Enabled = false;
                    }

                    if (this.Client != null && (this.Client.GameType == "BFBC2" || this.Client.GameType == "MOH" || this.Client.GameType == "MOHW" || this.Client.GameType == "BF3"))
                    {
                        this.spectatorListToolStripMenuItem.Enabled = false;
                    }

                    if (this.Client != null && (this.Client.GameType == "MOHW" || this.Client.GameType == "BF3" || this.Client.GameType == "BF4"))
                    {
                        textChatModerationToolStripMenuItem.Enabled = false;
                    }

                    //show the context menu strip
                    Point menuPosition = Cursor.Position;
                    menuPosition.Offset(1, 1);
                    ctxPlayerOptions.Show(this, this.PointToClient(menuPosition));
                }
            }
        }

        private void squadChange_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                CPlayerInfo player = (CPlayerInfo)((object[])item.Tag)[0];
                int destinationSquadId = (int)((object[])item.Tag)[1];

                this.Client.Game.SendAdminMovePlayerPacket(player.SoldierName, player.TeamID, destinationSquadId, true);
            }
        }

        private void teamChange_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                CPlayerInfo player = (CPlayerInfo)((object[])item.Tag)[0];
                CTeamName destinationTeam = (CTeamName)((object[])item.Tag)[1];

                this.Client.Game.SendAdminMovePlayerPacket(player.SoldierName, destinationTeam.TeamID, this.Client.GetDefaultSquadIDByMapname(destinationTeam.MapFilename), true);
            }
        }

        private void reservedSlotToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (this.reservedSlotToolStripMenuItem.Tag is CPlayerInfo)
            {
                if (this.reservedSlotToolStripMenuItem.Checked == false)
                {
                    this.Client.Game.SendReservedSlotsAddPlayerPacket(((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName);
                }
                else
                {
                    this.Client.Game.SendReservedSlotsRemovePlayerPacket(((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName);
                }

                this.Client.Game.SendReservedSlotsSavePacket();
            }
        }

        private void spectatorListToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (this.spectatorListToolStripMenuItem.Tag is CPlayerInfo)
            {
                if (this.spectatorListToolStripMenuItem.Checked == false)
                {
                    this.Client.Game.SendSpectatorListAddPlayerPacket(((CPlayerInfo)this.spectatorListToolStripMenuItem.Tag).SoldierName);
                }
                else
                {
                    this.Client.Game.SendSpectatorListRemovePlayerPacket(((CPlayerInfo)this.spectatorListToolStripMenuItem.Tag).SoldierName);
                }

                this.Client.Game.SendSpectatorListSavePacket();
            }
        }

        private void mutedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.mutedToolStripMenuItem.Tag is CPlayerInfo)
            {
                this.Client.Game.SendTextChatModerationListAddPacket(new TextChatModerationEntry(PlayerModerationLevelType.Muted, ((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName));

                this.Client.Game.SendTextChatModerationListSavePacket();
            }
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.normalToolStripMenuItem.Tag is CPlayerInfo)
            {
                this.Client.Game.SendTextChatModerationListAddPacket(new TextChatModerationEntry(PlayerModerationLevelType.Normal, ((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName));

                this.Client.Game.SendTextChatModerationListSavePacket();
            }
        }

        private void voiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.voiceToolStripMenuItem.Tag is CPlayerInfo)
            {
                this.Client.Game.SendTextChatModerationListAddPacket(new TextChatModerationEntry(PlayerModerationLevelType.Voice, ((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName));

                this.Client.Game.SendTextChatModerationListSavePacket();
            }
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.adminToolStripMenuItem.Tag is CPlayerInfo)
            {
                this.Client.Game.SendTextChatModerationListAddPacket(new TextChatModerationEntry(PlayerModerationLevelType.Admin, ((CPlayerInfo)this.reservedSlotToolStripMenuItem.Tag).SoldierName));

                this.Client.Game.SendTextChatModerationListSavePacket();
            }
        }

        private void statsLookupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CPlayerInfo tag = this.voiceToolStripMenuItem.Tag as CPlayerInfo;
            if (tag != null)
            {
                if (this.Client.Game is MoHClient)
                {
                    System.Diagnostics.Process.Start("http://mohstats.com/stats_pc/" + tag.SoldierName);
                }
                else if (this.Client.Game is BFBC2Client)
                {
                    System.Diagnostics.Process.Start("http://bfbcs.com/stats_pc/" + tag.SoldierName);
                }
                else if (this.Client.Game is BF3Client)
                {
                    System.Diagnostics.Process.Start("http://bf3stats.com/stats_pc/" + tag.SoldierName);
                }
                else if (this.Client.Game is BF4Client)
                {
                    System.Diagnostics.Process.Start("http://bf4stats.com/pc/" + tag.SoldierName);
                }
                else if (this.Client.Game is MOHWClient)
                {
                    System.Diagnostics.Process.Start("http://mohwstats.com/stats_pc/" + tag.SoldierName);
                }
            }
        }

        private void statsLookupToolStripMenuItemCustom_Click(object sender, EventArgs e)
        {
            string statsUrl = String.Empty;

            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                String statsUrlBuildError = String.Empty;

                CPlayerInfo player = (CPlayerInfo)((object[])item.Tag)[0];
                statsUrl = ((object[])item.Tag)[1].ToString();

                statsUrl = statsUrl.Replace("%game%", this.Client.GameType.ToLower());

                string[] ipPort = this.Client.CurrentServerInfo.ExternalGameIpandPort.Split(':');
                statsUrl = statsUrl.Replace("%srv_ip%", ipPort[0]);
                statsUrl = statsUrl.Replace("%srv_port%", ipPort[1]);
                statsUrl = statsUrl.Replace("%srv_ip_port%", this.Client.CurrentServerInfo.ExternalGameIpandPort);

                if (this.voiceToolStripMenuItem.Tag is CPlayerInfo)
                {
                    statsUrl = statsUrl.Replace("%player_name%", ((CPlayerInfo)this.voiceToolStripMenuItem.Tag).SoldierName);
                    statsUrl = statsUrl.Replace("%player_EAguid%", ((CPlayerInfo)this.voiceToolStripMenuItem.Tag).GUID);
                }
                else
                {
                    if (statsUrl.Contains("%player_name%") == true)
                    {
                        statsUrlBuildError = "Missing information for player name replacement in url";
                    }

                    if (statsUrl.Contains("%player_EAguid%") == true)
                    {
                        statsUrlBuildError = "Missing information for ea guid replacement in url";
                    }
                }

                if (this.punkBusterScreenshotToolStripMenuItem.Tag is CPunkbusterInfo)
                {
                    statsUrl = statsUrl.Replace("%player_PBguid%", ((CPunkbusterInfo)this.punkBusterScreenshotToolStripMenuItem.Tag).GUID);
                    statsUrl = statsUrl.Replace("%player_IP%", ((CPunkbusterInfo)this.punkBusterScreenshotToolStripMenuItem.Tag).Ip);
                }
                else
                {
                    if (statsUrl.Contains("%player_PBguid%") == true)
                    {
                        statsUrlBuildError = "Missing punkbuster information for player guid replacement in url";
                    }

                    if (statsUrl.Contains("%player_IP%") == true)
                    {
                        statsUrlBuildError = "Missing punkbuster information for player ip replacement in url";
                    }
                }

                if (statsUrlBuildError.Length == 0)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(statsUrl);
                    }
                    catch
                    {
                        MessageBox.Show(@"Error opening url, possibly missing data for url replacement.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show(statsUrlBuildError, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void punkBusterScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.punkBusterScreenshotToolStripMenuItem.Tag is CPunkbusterInfo)
            {
                // ((CPunkbusterInfo)this.punkBusterScreenshotToolStripMenuItem.Tag).SlotID
                this.Client.SendRequest(new List<string>() { "punkBuster.pb_sv_command", "pb_sv_getss " + ((CPunkbusterInfo)this.punkBusterScreenshotToolStripMenuItem.Tag).SlotID });
            }
        }

        #endregion

        #region Double Click on player

        private void lsvPlayers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem lviSelected = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(e.X, e.Y);

            if (lviSelected != null && lviSelected.Tag != null && lviSelected.Tag is AdditionalPlayerInfo)
            {
                CPlayerInfo player = ((AdditionalPlayerInfo)lviSelected.Tag).Player;

                if (player != null && this.ColumnSorter.TotalsAveragesChecker.IsMatch(lviSelected.Name) == false)
                {
                    if (player.SoldierName == null)
                    {
                        return;
                    }
                    try
                    {
                        Clipboard.SetDataObject(player.SoldierName, true, 5, 10);
                    }
                    catch (Exception)
                    {
                        // Nope, another thread is accessing the clipboard..
                    }
                }
            }

        }

        #endregion

        #region EndRound

        private void cboEndRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboEndRound.SelectedIndex > 0)
            {
                if (Program.ProconApplication.OptionsSettings.ShowCfmMsgRoundRestartNext == true)
                { //End this round with {0} winning? this.m_clocLanguage.GetLocalized("uscPlayerListPanel.MessageBox.EndRound")
                    DialogResult cfmEndRound = MessageBox.Show(this.Language.GetLocalized("uscPlayerListPanel.MessageBox.EndRound", new String[] { this.cboEndRound.Text }),
                        @"PRoCon Frostbite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (cfmEndRound == DialogResult.No)
                    {
                        this.cboEndRound.SelectedIndex = 0;
                        return;
                    }
                    if (this.Client.Game is MoHClient)
                    {
                        this.Client.SendRequest(new List<string>() { "admin.endRound", this.cboEndRound.SelectedIndex.ToString(CultureInfo.InvariantCulture) });
                    }
                    else if (this.Client.Game is BFBC2Client)
                    {
                        this.Client.SendRequest(new List<string>() { "admin.endRound", this.cboEndRound.SelectedIndex.ToString(CultureInfo.InvariantCulture) });
                    }
                    else if (this.Client.Game is BF3Client || this.Client.Game is BF4Client || this.Client.Game is BFHLClient)
                    {
                        this.Client.SendRequest(new List<string>() { "mapList.endRound", this.cboEndRound.SelectedIndex.ToString(CultureInfo.InvariantCulture) });
                    }
                    else if (this.Client.Game is MOHWClient)
                    {
                        this.Client.SendRequest(new List<string>() { "mapList.endRound", this.cboEndRound.SelectedIndex.ToString(CultureInfo.InvariantCulture) });
                    }
                }
            }
            this.cboEndRound.SelectedIndex = 0;
        }

        private void Client_Serverinfo_EndRound_Update(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.InvokeIfRequired(() =>
            {
                //
                int iTeams = this.Client.GetLocalizedTeamNameCount(this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode);

                this.cboEndRound.Items.Clear();
                this.cboEndRound.Items.AddRange(new object[] { this.Language.GetDefaultLocalized("Select winning team to end round:", "uscPlayerListPanel.ctxPlayerOptions.EndRound.Label") });
                this.cboEndRound.SelectedIndex = 0;

                for (int i = 1; i < iTeams; i++)
                {
                    this.cboEndRound.Items.AddRange(new object[] { String.Format("{0} - {1}", this.Language.GetDefaultLocalized("Team " + i.ToString(), "uscPlayerListPanel.ctxPlayerOptions.EndRound.Team" + i.ToString(CultureInfo.InvariantCulture)), this.Client.GetLocalizedTeamName(i, this.Client.CurrentServerInfo.Map, this.Client.CurrentServerInfo.GameMode)) });
                }

                Graphics cboEndRoundGrafphics = cboEndRound.CreateGraphics();
                this.cboEndRound.Width = 18 + (int)cboEndRoundGrafphics.MeasureString(this.cboEndRound.Text, this.cboEndRound.Font).Width;
            });
        }

        #endregion

        private void updateDeveloperUids()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load("https://myrcon.com/procon/streams/developers/format/xml");

                    foreach (XmlElement developer in document.GetElementsByTagName("developer"))
                    {
                        XmlNodeList developerUids = developer.GetElementsByTagName("ea_guid");

                        if (developerUids.Count > 0)
                        {
                            XmlNode developerUid = developerUids.Item(0);

                            XmlNodeList developerTypes = developer.GetElementsByTagName("type");
                            if (developerTypes.Count > 0)
                            {
                                XmlNode developerType = developerTypes.Item(0);

                                if (developerType != null && developerType.InnerText.Length > 0)
                                {
                                    switch (developerType.InnerText)
                                    {
                                        case "developer":
                                            if (developerUid != null && developerUid.InnerText.Length > 0 && this.DeveloperUids.Contains(developerUid.InnerText) == false)
                                            {
                                                this.DeveloperUids.Add(developerUid.InnerText);
                                            }
                                            break;
                                        case "staff":
                                            if (developerUid != null && developerUid.InnerText.Length > 0 && this.StaffUids.Contains(developerUid.InnerText) == false)
                                            {
                                                this.StaffUids.Add(developerUid.InnerText);
                                            }
                                            break;
                                        case "plugindeveloper":
                                            if (developerUid != null && developerUid.InnerText.Length > 0 && this.PluginDeveloperUids.Contains(developerUid.InnerText) == false)
                                            {
                                                this.PluginDeveloperUids.Add(developerUid.InnerText);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }
            });
        }
    }
}
