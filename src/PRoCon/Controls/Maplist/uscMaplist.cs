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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PRoCon.Controls.Maplist
{
    using Core;
    using Core.Maps;
    using Core.Remote;
    using PRoCon.Forms;
    public partial class uscMaplist : uscPage
    {

        private frmMain m_frmMain;
        //private uscServerConnection m_uscConnectionPanel;
        private PRoConClient m_client;

        //private List<Core.Maps.MaplistEntry> m_previousMapList;

        private bool m_blSettingNewPlaylist;
        private bool m_blSettingAppendingSingleMap;

        private CPrivileges m_privileges;

        private int m_iReselectShufflingMapIndex = 0;
        private int m_iCurMapIndex = -1;
        private int m_iNextMapIndex = -1;

        public uscMaplist()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.AsyncSettingControls.Add("local.playlist.change", new AsyncStyleSetting(this.picMaplistChangePlaylist, this.lsvMaplist, new Control[] { this.pnlMaplistAddMap, this.lsvMaplist }, true));
            this.AsyncSettingControls.Add("local.maplist.change", new AsyncStyleSetting(this.picMaplistAlterMaplist, this.lsvMaplist, new Control[] { this.pnlMaplistAddMap, this.lsvMaplist }, true));

            this.m_blSettingAppendingSingleMap = false;
            this.AsyncSettingControls.Add("local.maplist.append", new AsyncStyleSetting(this.picMaplistAppendMap, this.lsvMaplist, new Control[] { this.lblMaplistPool }, true));
            //this.AsyncSettingControls.Add("local.maplist.setnextlevel", new AsyncStyleSetting(this.picMaplistAppendMap, this.lsvMaplist, new Control[] { this.lblMaplistPool, this.lnkMaplistAddMapToList, this.lnkMaplistSetNextMap }, true));

            this.m_privileges = new CPrivileges(CPrivileges.FullPrivilegesFlags);
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {

            this.lblMaplistCurrentPlayList.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lblMaplistCurrentPlayList");
            this.lnkMaplistChangePlaylist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lnkMaplistChangePlaylist");
            this.lblMaplistCurrentMaplist.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lblMaplistCurrentMaplist");

            this.colPoolGameType.Text = this.colGameType.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.colGametype");
            this.colPoolMapname.Text = this.colMapname.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.colMapname");
            this.colPoolMapFilename.Text = this.colMapFilename.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.colMapFilename");
            this.lblMaplistMustChangeWarning.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lblMaplistMustChangeWarning");
            this.lblMaplistPool.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lblMaplistPool");

            this.colMapRounds.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.colMapRounds");
            this.lblMaplistRounds.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lblMaplistRounds");

            this.setNextMapToolStripMenuItem.Text = clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.ctxMapOptions.setNextMapToolStripMenuItem");

            this.toolTipMaplistRefresh.SetToolTip(this.btnMaplistRefresh, clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.MaplistRefreshToolTip"));
            this.toolTipMaplistRefresh.SetToolTip(this.btnMaplistClear, clocLanguage.GetLocalized("uscListControlPanel.tabMaplist.lsvMaplist.MaplistClearToolTip"));
        }


        public void Initialize(frmMain frmMainWindow, uscServerConnection uscConnectionPanel)
        {
            this.m_frmMain = frmMainWindow;

            this.SettingFail = frmMainWindow.picAjaxStyleFail.Image;
            this.SettingSuccess = frmMainWindow.picAjaxStyleSuccess.Image;
            this.SettingLoading = frmMainWindow.picAjaxStyleLoading.Image;

            this.btnAddMap.Image = frmMainWindow.iglIcons.Images["arrow-curve-000-left.png"];
            this.btnRemoveMap.Image = frmMainWindow.iglIcons.Images["arrow-curve-180-left.png"];

            this.btnMaplistRefresh.ImageList = this.m_frmMain.iglIcons;
            this.btnMaplistRefresh.ImageKey = "arrow_refresh.png";

            this.btnMaplistClear.ImageList = this.m_frmMain.iglIcons;
            this.btnMaplistClear.ImageKey = "cross.png";
        }

        public override void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_client = prcClient) != null)
            {
                if (this.m_client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.m_client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {

                this.m_client.Game.PlaylistSet += new FrostbiteClient.PlaylistSetHandler(Game_PlaylistSet);

                this.m_client.Game.MapListSave += new FrostbiteClient.EmptyParamterHandler(Game_MapListSave);
                this.m_client.Game.MapListMapAppended += new FrostbiteClient.MapListAppendedHandler(Game_MapListMapAppended);
                this.m_client.Game.MapListCleared += new FrostbiteClient.EmptyParamterHandler(Game_MapListCleared);
                this.m_client.Game.MapListListed += new FrostbiteClient.MapListListedHandler(Game_MapListListed);
                this.m_client.Game.MapListMapInserted += new FrostbiteClient.MapListMapInsertedHandler(Game_MapListMapInserted);
                this.m_client.Game.MapListMapRemoved += new FrostbiteClient.MapListLevelIndexHandler(Game_MapListMapRemoved);

                this.m_client.Game.MapListGetMapIndices += new FrostbiteClient.MapListGetMapIndicesHandler(Game_MapListGetMapIndices);
                this.m_client.Game.MapListNextLevelIndex += new FrostbiteClient.MapListLevelIndexHandler(Game_MapListNextLevelIndex);

                this.m_client.MapListPool.ItemAdded += new NotificationList<CMap>.ItemModifiedHandler(MapListPool_ItemAdded);

                this.m_client.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_prcClient_ProconPrivileges);

                this.splitContainer1.Panel1Collapsed = this.m_client.Game.HasOpenMaplist;

                this.RefreshLocalMaplist();
            });
        }

        private void RefreshLocalMaplist()
        {

            if (this.m_client != null && this.m_client.Game != null)
            {

                if (this.m_client.Game.HasOpenMaplist == false)
                {

                    CMap[] a_objItems = new CMap[this.cboMaplistPlaylists.Items.Count];
                    this.cboMaplistPlaylists.Items.CopyTo(a_objItems, 0);
                    List<CMap> lstCurrentList = new List<CMap>(a_objItems);

                    foreach (CMap mapPool in this.m_client.MapListPool)
                    {
                        if (lstCurrentList.Find(map => String.Compare(map.PlayList, mapPool.PlayList, true) == 0) == null)
                        {
                            this.cboMaplistPlaylists.Items.Add(mapPool);

                            if (String.Compare(mapPool.PlayList, this.m_client.ListSettings.CurrentPlaylist, true) == 0)
                            {
                                this.cboMaplistPlaylists.SelectedItem = mapPool;
                            }

                            a_objItems = new CMap[this.cboMaplistPlaylists.Items.Count];
                            this.cboMaplistPlaylists.Items.CopyTo(a_objItems, 0);
                            lstCurrentList = new List<CMap>(a_objItems);
                        }
                    }
                }

                if (this.cboMaplistPlaylists.SelectedItem != null || this.m_client.Game.HasOpenMaplist == true)
                {

                    this.lsvMaplistPool.BeginUpdate();

                    // Update the available maplist pool.
                    this.lsvMaplistPool.Items.Clear();
                    List<CMap> lstMapPool;
                    if (this.m_client.Game.HasOpenMaplist == true)
                    {
                        lstMapPool = new List<CMap>(this.m_client.MapListPool);
                    }
                    else
                    {
                        lstMapPool = this.m_client.MapListPool.FindAll(map => String.Compare(map.PlayList, ((CMap)cboMaplistPlaylists.SelectedItem).PlayList, true) == 0);
                    }

                    foreach (CMap map in lstMapPool)
                    {

                        if (this.lsvMaplistPool.Items.ContainsKey(map.FileName.ToLower()) == false)
                        {
                            ListViewItem mapPoolItem = new ListViewItem();
                            mapPoolItem.Tag = new MaplistEntry(map.PlayList, map.FileName, 0);
                            //mapPoolItem.Tag = new MaplistEntry(map.FileName);
                            mapPoolItem.Text = map.GameMode;
                            mapPoolItem.SubItems.Add(new ListViewItem.ListViewSubItem(mapPoolItem, map.FileName));
                            mapPoolItem.SubItems.Add(new ListViewItem.ListViewSubItem(mapPoolItem, map.PublicLevelName));

                            this.lsvMaplistPool.Items.Add(mapPoolItem);
                        }
                    }

                    #region ColorSameGamemode
                    bool blDoColor = false;
                    for (int i = 0; i < this.lsvMaplistPool.Items.Count; i++)
                    {
                        if (i == 0) { continue; }
                        if (!this.lsvMaplistPool.Items[i].Text.Equals(this.lsvMaplistPool.Items[i - 1].Text))
                        {
                            if (blDoColor == true) { blDoColor = false; } else { blDoColor = true; }
                        }

                        if (blDoColor == true)
                        {
                            this.lsvMaplistPool.Items[i].BackColor = Color.FromArgb(233, 233, 233);
                        }
                    }
                    #endregion

                    this.lsvMaplistPool.EndUpdate();
                }

                for (int i = 0; i < this.lsvMaplistPool.Columns.Count; i++)
                {
                    this.lsvMaplistPool.Columns[i].Width = -2;
                }
            }
        }

        private void m_prcClient_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_privileges = spPrivs;

                this.lsvMaplistPool.Enabled = this.m_privileges.CanEditMapList;
                this.pnlMaplistAddMap.Enabled = this.m_privileges.CanEditMapList;
                this.lnkMaplistChangePlaylist.Enabled = this.m_privileges.CanEditMapList;

                // The main map list though can be used if the user can use map functions (skip map, restart etc)
                this.lsvMaplist.Enabled = this.m_privileges.CanEditMapList || this.m_privileges.CanUseMapFunctions;
            });
        }

        private void MapListPool_ItemAdded(int iIndex, CMap item)
        {
            this.InvokeIfRequired(this.RefreshLocalMaplist);
        }

        private void Game_MapListMapInserted(FrostbiteClient sender, MaplistEntry mapEntry)
        {// int mapIndex, string mapFileName, int rounds) {
            this.InvokeIfRequired(() => this.InsertMapInMapList(mapEntry.Index, mapEntry));
        }

        private void Game_MapListMapRemoved(FrostbiteClient sender, int mapIndex)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.lsvMaplist.Items.Count > mapIndex)
                {
                    this.lsvMaplist.Items.RemoveAt(mapIndex);

                    if (this.m_iReselectShufflingMapIndex < this.lsvMaplist.Items.Count)
                    {
                        this.lsvMaplist.Items[this.m_iReselectShufflingMapIndex].Selected = true;
                    }

                    this.UpdateMaplistIndexes();
                }
            });
        }

        private void Game_MapListListed(FrostbiteClient sender, List<Core.Maps.MaplistEntry> lstMapList)
        {
            this.InvokeIfRequired(() =>
            {
                this.lsvMaplist.BeginUpdate();

                this.lsvMaplist.Items.Clear();

                for (int i = 0; i < lstMapList.Count; i++)
                {
                    ListViewItem lviMap = new ListViewItem();
                    lviMap.Tag = lstMapList[i];
                    lviMap.Name = lstMapList[i].MapFileName;
                    lviMap.Text = Convert.ToString(i + 1);

                    if (lstMapList[i].Gamemode != null && lstMapList[i].Gamemode.Length > 0)
                    {
                        //lviMap.SubItems.Add(this.m_client.GetFriendlyGamemode(lstMapList[i].Gamemode));
                        CMap tmpMap = this.m_client.GetFriendlyMapByFilenamePlayList(lstMapList[i].MapFileName, lstMapList[i].Gamemode);
                        lviMap.SubItems.Add(tmpMap != null ? tmpMap.GameMode : String.Empty);
                        lviMap.SubItems.Add(tmpMap != null ? tmpMap.PublicLevelName : String.Empty);
                    }
                    else
                    {
                        lviMap.SubItems.Add(this.m_client.GetFriendlyGamemodeByMap(lstMapList[i].MapFileName));
                        lviMap.SubItems.Add(this.m_client.GetFriendlyMapname(lstMapList[i].MapFileName));
                    }

                    //lviMap.SubItems.Add(this.m_client.GetFriendlyMapname(lstMapList[i].MapFileName));
                    lviMap.SubItems.Add(lstMapList[i].MapFileName);

                    ListViewItem.ListViewSubItem lviRounds = new ListViewItem.ListViewSubItem();
                    lviRounds.Name = "rounds";

                    if (lstMapList[i].Rounds == 0)
                    {
                        lviRounds.Text = "2";
                        lviRounds.Tag = 0;
                    }
                    else
                    {
                        lviRounds.Text = lstMapList[i].Rounds.ToString();
                        lviRounds.Tag = lstMapList[i].Rounds;
                    }

                    lviMap.SubItems.Add(lviRounds);

                    this.lsvMaplist.Items.Add(lviMap);
                }

                for (int i = 0; i < this.lsvMaplist.Columns.Count; i++)
                {
                    this.lsvMaplist.Columns[i].Width = -2;
                }

                this.Game_MapListDoUpdateMarkers(m_iCurMapIndex, m_iNextMapIndex);

                this.lsvMaplist.EndUpdate();
            });
        }

        private void Game_MapListCleared(FrostbiteClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.lsvMaplist.BeginUpdate();

                this.lsvMaplist.Items.Clear();

                this.lsvMaplist.EndUpdate();
            });
        }

        private void UpdateMaplistIndexes()
        {
            for (int i = 0; i < this.lsvMaplist.Items.Count; i++)
            {
                this.lsvMaplist.Items[i].Text = (i + 1).ToString();
            }
        }

        private void Game_MapListNextLevelIndex(FrostbiteClient sender, int nextIndex)
        {
            this.InvokeIfRequired(() => this.Game_MapListDoUpdateMarkers(this.m_iCurMapIndex, nextIndex));
        }

        private void Game_MapListGetMapIndices(FrostbiteClient sender, int mapIndex, int nextIndex)
        {
            this.InvokeIfRequired(() => this.Game_MapListDoUpdateMarkers(mapIndex, nextIndex));
        }

        private void Game_MapListDoUpdateMarkers(int mapIndex, int nextIndex)
        {
            this.InvokeIfRequired(() =>
            {
                this.lsvMaplist.BeginUpdate();
                for (int i = 0; i < this.lsvMaplist.Items.Count; i++)
                {
                    if (i == mapIndex || i == nextIndex)
                    {
                        if (i == mapIndex)
                        {
                            this.m_iCurMapIndex = mapIndex;
                            //this.lsvMaplist.Items[i].BackColor = Color.Silver;
                            this.lsvMaplist.Items[i].BackColor = Color.LightSteelBlue;
                        }
                        if (i == nextIndex)
                        {
                            this.m_iNextMapIndex = nextIndex;
                            this.lsvMaplist.Items[i].BackColor = Color.Gainsboro;
                        }
                    }
                    else
                    {
                        this.lsvMaplist.Items[i].BackColor = SystemColors.Window;
                    }
                }
                this.lsvMaplist.EndUpdate();
            });
        }

        private void InsertMapInMapList(int insertIndex, Core.Maps.MaplistEntry mapEntry)
        {
            this.lsvMaplist.BeginUpdate();

            ListViewItem lviMap = new ListViewItem();

            lviMap.Tag = mapEntry;
            lviMap.Name = mapEntry.MapFileName;
            lviMap.Text = Convert.ToString(this.lsvMaplist.Items.Count + 1);

            if (mapEntry.Gamemode != null && mapEntry.Gamemode.Length > 0)
            {
                //lviMap.SubItems.Add(this.m_client.GetFriendlyGamemode(mapEntry.Gamemode));
                CMap tmpMap = this.m_client.GetFriendlyMapByFilenamePlayList(mapEntry.MapFileName, mapEntry.Gamemode);
                lviMap.SubItems.Add(tmpMap != null ? tmpMap.GameMode : String.Empty);
                lviMap.SubItems.Add(tmpMap != null ? tmpMap.PublicLevelName : String.Empty);
            }
            else
            {
                lviMap.SubItems.Add(this.m_client.GetFriendlyGamemodeByMap(mapEntry.MapFileName));
                lviMap.SubItems.Add(this.m_client.GetFriendlyMapname(mapEntry.MapFileName));
            }

            //lviMap.SubItems.Add(this.m_client.GetFriendlyMapname(mapEntry.MapFileName));
            lviMap.SubItems.Add(mapEntry.MapFileName);

            ListViewItem.ListViewSubItem lviRounds = new ListViewItem.ListViewSubItem();
            lviRounds.Name = "rounds";

            if (mapEntry.Rounds == 0)
            {
                lviRounds.Text = "2";
                lviRounds.Tag = 0;
            }
            else
            {
                lviRounds.Text = mapEntry.Rounds.ToString();
                lviRounds.Tag = mapEntry.Rounds;
            }

            lviMap.SubItems.Add(lviRounds);

            if (insertIndex >= 0)
            {
                this.lsvMaplist.Items.Insert(insertIndex, lviMap);
            }
            else
            {
                this.lsvMaplist.Items.Add(lviMap);
            }

            this.UpdateMaplistIndexes();

            for (int i = 0; i < this.lsvMaplist.Columns.Count; i++)
            {
                this.lsvMaplist.Columns[i].Width = -2;
            }

            if (this.m_blSettingAppendingSingleMap == true)
            {
                this.OnSettingResponse("local.maplist.append", true);
            }

            this.lsvMaplist.EndUpdate();
        }

        private void Game_MapListMapAppended(FrostbiteClient sender, Core.Maps.MaplistEntry mapEntry)
        {
            this.InvokeIfRequired(() => this.InsertMapInMapList(-1, mapEntry));
        }

        private void Game_MapListSave(FrostbiteClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.m_iReselectShufflingMapIndex < this.lsvMaplist.Items.Count)
                {
                    this.lsvMaplist.Items[this.m_iReselectShufflingMapIndex].Selected = true;
                }

                if (this.m_blSettingNewPlaylist == true)
                {
                    this.OnSettingResponse("local.playlist.change", true);
                }

                this.OnSettingResponse("local.maplist.change", true);

                this.m_blSettingNewPlaylist = false;

                this.lsvMaplist.EndUpdate();
            });
        }

        private void Game_PlaylistSet(FrostbiteClient sender, string playlist)
        {
            this.InvokeIfRequired(() =>
            {
                // If we've just set the playlist..
                if (this.m_blSettingNewPlaylist == true && this.m_client.MapListPool.Find(map => String.Compare(map.PlayList, playlist, true) == 0) != null)
                {
                    // Add all the supported maps

                    this.WaitForSettingResponse("local.playlist.change");
                    this.WaitForSettingResponse("local.maplist.change");

                    this.lsvMaplist.BeginUpdate();

                    sender.SendMapListClearPacket();

                    foreach (CMap map in this.m_client.MapListPool.FindAll(map => String.Compare(map.PlayList, playlist, true) == 0))
                    {
                        sender.SendMapListAppendPacket(new MaplistEntry(map.PlayList, map.FileName, 0));
                    }

                    sender.SendMapListSavePacket();
                }

                foreach (CMap cmPlayList in this.cboMaplistPlaylists.Items)
                {
                    if (String.Compare(playlist, cmPlayList.PlayList, true) == 0)
                    {
                        this.cboMaplistPlaylists.SelectedItem = cmPlayList;

                        this.lnkMaplistChangePlaylist.Enabled = false;
                        this.pnlMaplistAddMap.Enabled = true && this.m_privileges.CanEditMapList;
                        //this.pnlMaplistAddMap.Enabled = true && this.m_privileges.CanEditMapList && this.lsvMaplist.SelectedItems.Count > 0;

                        this.lblMaplistMustChangeWarning.Visible = false;

                        this.cboMaplistPlaylists.Refresh();

                        this.RefreshLocalMaplist();
                    }
                }

                this.cboMaplistPlaylists_SelectedIndexChanged(null, null);
            });
        }

        private void cboMaplistPlaylists_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cboMaplistPlaylists.SelectedIndex >= 0)
            {
                if (String.Compare(((CMap)cboMaplistPlaylists.Items[cboMaplistPlaylists.SelectedIndex]).PlayList, this.m_client.ListSettings.CurrentPlaylist, true) == 0)
                {

                    this.lnkMaplistChangePlaylist.Enabled = false;
                    this.pnlMaplistAddMap.Enabled = true && this.m_privileges.CanEditMapList;
                    //this.pnlMaplistPositionControls.Enabled = true && this.m_privileges.CanEditMapList && this.lsvMaplist.SelectedItems.Count > 0;

                    this.lblMaplistMustChangeWarning.Visible = false;
                    this.splitContainer1.Panel2.Enabled = true;
                }
                else
                {
                    this.lnkMaplistChangePlaylist.Enabled = true && this.m_privileges.CanEditMapList;
                    this.pnlMaplistAddMap.Enabled = false;
                    //this.pnlMaplistPositionControls.Enabled = false;

                    this.lblMaplistMustChangeWarning.Visible = true;
                    this.splitContainer1.Panel2.Enabled = false;
                }

                this.RefreshLocalMaplist();
            }
        }

        private void cboMaplistPlaylists_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {

                Font ftnSelectFont = this.Font;
                Brush clrBrushColour = SystemBrushes.WindowText;
                CMap mpGamemode = ((CMap)cboMaplistPlaylists.Items[e.Index]);

                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

                if (String.Compare(mpGamemode.PlayList, this.m_client.ListSettings.CurrentPlaylist, true) == 0)
                {
                    ftnSelectFont = new Font(this.Font, FontStyle.Bold);
                    clrBrushColour = Brushes.MediumSeaGreen;
                }

                if (this.m_client.MapListPool.Find(map => String.Compare(map.PlayList, mpGamemode.PlayList, true) == 0) != null)
                {
                    e.Graphics.DrawString(mpGamemode.GameMode, ftnSelectFont, clrBrushColour, e.Bounds.Left + 5, e.Bounds.Top);

                    string strSupportedMaps = this.m_client.Language.GetLocalized("uscListControlPanel.tabMaplist.cboMaplistPlaylists.SupportedMaps", this.m_client.MapListPool.FindAll(map => String.Compare(map.PlayList, mpGamemode.PlayList, true) == 0).Count.ToString());
                    e.Graphics.DrawString(strSupportedMaps, ftnSelectFont, clrBrushColour, e.Bounds.Right - TextRenderer.MeasureText(strSupportedMaps, ftnSelectFont).Width - 10, e.Bounds.Top);
                }
                else
                {
                    e.Graphics.DrawString(mpGamemode.GameMode, ftnSelectFont, clrBrushColour, e.Bounds.Left + 5, e.Bounds.Top);
                }
            }
        }

        #region Maplist Control Events

        private void lsvMaplist_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            e.CancelEdit = true;
        }

        private void lsvMaplist_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void lsvMaplist_DragOver(object sender, DragEventArgs e)
        {

            if (e.Data is MaplistEntry)
            {
                e.Effect = DragDropEffects.Move;
            }

            //this.lsvMaplist.BeginUpdate();
            this.lsvMaplist.Refresh();
            //this.lsvMaplist.EndUpdate();

            Point cp = this.lsvMaplist.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = this.lsvMaplist.GetItemAt(cp.X, cp.Y);
            Rectangle itemBounds = new Rectangle();
            if (dragToItem != null)
            {
                itemBounds = dragToItem.GetBounds(ItemBoundsPortion.Entire);
            }
            else
            {
                if (this.lsvMaplist.Items.Count > 0)
                {
                    dragToItem = this.lsvMaplist.Items[this.lsvMaplist.Items.Count - 1];
                    itemBounds = dragToItem.GetBounds(ItemBoundsPortion.Entire);
                    itemBounds.Y += itemBounds.Height;
                }
            }

            if (dragToItem != null)
            {
                Graphics g = this.lsvMaplist.CreateGraphics();

                GraphicsPath gp = new GraphicsPath();
                gp.AddLines(
                    new Point[]
                    {
                        new Point(0, itemBounds.Y - 4),
                        new Point(5, itemBounds.Y),
                        new Point(0, itemBounds.Y + 4)
                    }
                    );

                g.FillPath(Brushes.Black, gp);
                gp.Dispose();

                g.DrawLine(new Pen(Brushes.Black, 2.0F), 0, itemBounds.Y, itemBounds.Width, itemBounds.Y);
                g.Dispose();
            }

        }

        private void lsvMaplist_DragDrop(object sender, DragEventArgs e)
        {
            this.lsvMaplist.Refresh();

            MaplistEntry insertingMap = ((MaplistEntry)e.Data.GetData(typeof(MaplistEntry)));
            if (this.m_client != null && this.m_client.Game != null)
            {

                Point cp = this.lsvMaplist.PointToClient(new Point(e.X, e.Y));
                ListViewItem dragToItem = this.lsvMaplist.GetItemAt(cp.X, cp.Y);

                this.m_blSettingAppendingSingleMap = true;

                if (insertingMap.Index >= 0)
                {
                    this.m_client.Game.SendMapListRemovePacket(insertingMap.Index);
                }

                if (dragToItem != null)
                {
                    this.WaitForSettingResponse("local.maplist.append");
                    this.m_client.Game.SendMapListInsertPacket(new MaplistEntry(insertingMap.Gamemode, insertingMap.MapFileName, insertingMap.Rounds, dragToItem.Index));
                }
                else
                {
                    this.WaitForSettingResponse("local.maplist.append");

                    this.m_client.Game.SendMapListAppendPacket(insertingMap);
                }

                this.m_client.Game.SendMapListSavePacket();
            }
        }

        private void lsvMaplist_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lviSelected = (ListViewItem)e.Item;

            if (e.Button == MouseButtons.Left)
            {

                int rounds = 0;

                if (lviSelected != null && lviSelected.Tag != null && lviSelected.Tag != null && int.TryParse(lviSelected.SubItems[4].Text, out rounds) == true)
                {
                    ((PRoCon.Controls.ControlsEx.ListViewNF)sender).DoDragDrop(new MaplistEntry(((MaplistEntry)lviSelected.Tag).Gamemode, ((MaplistEntry)lviSelected.Tag).MapFileName, rounds, lviSelected.Index), DragDropEffects.None | DragDropEffects.Scroll | DragDropEffects.Move);
                }
            }
        }

        private void lsvMaplist_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnRemoveMap.Enabled = (this.lsvMaplist.SelectedItems.Count > 0);
        }

        private void lsvMaplist_DragLeave(object sender, EventArgs e)
        {
            this.lsvMaplist.Refresh();
        }

        private void lsvMaplist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.m_client != null && this.m_client.Game != null)
            {
                if (lsvMaplist.SelectedItems.Count > 0)
                {
                    this.m_client.Game.SendMapListNextLevelIndexPacket(lsvMaplist.SelectedItems[0].Index);
                    this.m_client.Game.SendAdminRunNextRoundPacket();
                }
            }
        }

        #region Right Click Context Menu
        private void lsvMaplist_MouseDown(object sender, MouseEventArgs e)
        {

            //Point pntClient = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).PointToClient(new Point(e.X, e.Y));
            ListViewItem lviSelected = ((PRoCon.Controls.ControlsEx.ListViewNF)sender).GetItemAt(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                //
            }
            else if (e.Button == MouseButtons.Right && lviSelected != null && lviSelected.Tag != null && lviSelected.Tag is MaplistEntry)
            {
                if (lsvMaplist.SelectedItems.Count > 0)
                {
                    int mapIndex = lviSelected.Index;

                    if (lviSelected != null)
                    {
                        this.setNextMapToolStripMenuItem.Tag = mapIndex;

                        //show the context menu strip
                        Point menuPosition = Cursor.Position;
                        menuPosition.Offset(1, 1);
                        ctxMapOptions.Show(this, this.PointToClient(menuPosition));
                    }
                }
            }
        }

        private void setNextMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.setNextMapToolStripMenuItem.Tag is int)
            {
                int Index = (int)this.setNextMapToolStripMenuItem.Tag;
                // mapList.setNextMapIndex
                this.m_client.Game.SendMapListNextLevelIndexPacket(Index);
            }
        }
        #endregion

        #endregion

        #region Maplist Pool Control Events

        private void lsvMaplistPool_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lviSelected = (ListViewItem)e.Item;

            if (e.Button == MouseButtons.Left)
            {

                if (lviSelected != null && lviSelected.Tag != null && lviSelected.Tag != null)
                {

                    ((PRoCon.Controls.ControlsEx.ListViewNF)sender).DoDragDrop(new MaplistEntry(((MaplistEntry)lviSelected.Tag).Gamemode, ((MaplistEntry)lviSelected.Tag).MapFileName, (int)this.numRoundsSelect.Value, -1), DragDropEffects.None | DragDropEffects.Scroll | DragDropEffects.Move);
                }
            }
        }

        private void lsvMaplistPool_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnAddMap.Enabled = (this.lsvMaplistPool.SelectedItems.Count > 0);
        }

        #endregion

        #region Map Add/Remove Controls

        private void btnAddMap_Click(object sender, EventArgs e)
        {
            if (this.m_client != null && this.m_client.Game != null && this.lsvMaplistPool.SelectedItems.Count > 0)
            {

                if (this.lsvMaplistPool.SelectedItems[0].Tag != null)
                {
                    this.WaitForSettingResponse("local.maplist.append");
                    this.m_blSettingAppendingSingleMap = true;

                    MaplistEntry entry = ((MaplistEntry)this.lsvMaplistPool.SelectedItems[0].Tag);
                    this.m_client.Game.SendMapListAppendPacket(new MaplistEntry(entry.Gamemode, entry.MapFileName, (int)this.numRoundsSelect.Value));

                    this.m_client.Game.SendMapListSavePacket();
                }
            }
        }

        private void btnRemoveMap_Click(object sender, EventArgs e)
        {
            if (this.m_client != null && this.m_client.Game != null && this.lsvMaplist.SelectedItems.Count > 0)
            {
                this.m_iReselectShufflingMapIndex = this.lsvMaplist.SelectedItems[0].Index;

                this.m_client.Game.SendMapListRemovePacket(this.lsvMaplist.SelectedItems[0].Index);

                this.m_client.Game.SendMapListSavePacket();
            }
        }

        #endregion

        private void lnkMaplistChangePlaylist_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.cboMaplistPlaylists.SelectedIndex >= 0)
            {

                CMap cmPlaylist = (CMap)this.cboMaplistPlaylists.SelectedItem;

                // Tell this.CurrentPlaylist the next playlist command back is a response to us setting it.
                this.m_blSettingNewPlaylist = true;
                this.m_client.Game.SendAdminSetPlaylistPacket(cmPlaylist.PlayList);
            }
        }

        #region MaplistRefresh

        private void btnMaplistRefresh_Click(object sender, EventArgs e)
        {
            if (this.m_client != null && this.m_client.Game != null)
            {
                this.m_client.Game.SendMapListLoadPacket();
                //this.m_client.Game.SendMapListListRoundsPacket();
                this.OnSettingResponse("local.maplist.append", true);
            }
        }

        #endregion

        #region MaplistClear

        private void btnMaplistClear_Click(object sender, EventArgs e)
        {
            if (this.m_client != null && this.m_client.Game != null)
            {
                this.m_client.Game.SendMapListClearPacket();
                this.OnSettingResponse("local.maplist.append", true);
            }
        }

        #endregion

    }
}
