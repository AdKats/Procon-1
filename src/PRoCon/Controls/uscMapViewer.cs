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
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    using Controls.Battlemap;
    using Controls.Battlemap.KillDisplay;
    using Controls.Battlemap.MapImagePacks;
    using Core;
    using Core.Battlemap;
    using Core.Players;
    using Core.Remote;
    using Core.Variables;
    using PRoCon.Forms;

    public partial class uscMapViewer : UserControl
    {

        private PRoConClient m_prcClient;
        private CLocalization m_clocLanguage;

        private frmMain m_frmMain;

        //private string m_strCurrentMap;
        //private string m_strMapPath;

        public string MapPath
        {
            get;
            private set;
        }

        public MapImagePackDictionary MapImagePacks
        {
            get;
            private set;
        }

        public MapImagePack LoadedMapImagePack
        {
            get;
            private set;
        }

        private bool m_isMapSelected;
        public bool IsMapSelected
        {
            get
            {
                return this.m_isMapSelected;
            }
            set
            {
                this.m_isMapSelected = value;

                if (this.LoadedMapImagePack != null)
                {
                    if (this.FullyLoadMap == true)
                    {
                        this.LoadedMapImagePack.LoadMap(this.LoadedMapImagePack.LoadedMapFileName, true);
                    }
                    else
                    {
                        this.LoadedMapImagePack.UnloadMapImage();
                    }
                }

                this.uscBattlemap.FullyLoadMap = this.FullyLoadMap;
            }
        }

        private bool m_isMapLoaded;
        public bool IsMapLoaded
        {
            get
            {
                return this.m_isMapLoaded;
            }
            set
            {
                this.m_isMapLoaded = value;

                if (this.LoadedMapImagePack != null)
                {
                    if (this.FullyLoadMap == true)
                    {
                        this.LoadedMapImagePack.LoadMap(this.LoadedMapImagePack.LoadedMapFileName, true);
                    }
                    else
                    {
                        this.LoadedMapImagePack.UnloadMapImage();
                    }
                }

                this.uscBattlemap.FullyLoadMap = this.FullyLoadMap;
            }
        }

        public bool FullyLoadMap
        {
            get
            {
                return this.IsMapSelected && this.IsMapLoaded;
            }
        }

        public uscMapViewer()
        {
            InitializeComponent();

            this.uscBattlemap.CreateMapZone += new BattlemapView.CreateMapZoneHandler(uscBattlemap_CreateMapZone);
            this.uscBattlemap.DeleteMapZone += new BattlemapView.DeleteMapZoneHandler(uscBattlemap_DeleteMapZone);
            this.uscBattlemap.ModifyMapZone += new BattlemapView.ModifyMapZoneHandler(uscBattlemap_ModifyMapZone);

            this.uscBattlemap.MapZoneSelected += new BattlemapView.MapZoneSelectedHandler(uscBattlemap_MapZoneSelected);
            //this.m_strCurrentMap = String.Empty;
            //this.m_strMapPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), "Maps");

            //this.LoadMapImagePacks();
        }

        private void uscMapViewer_Load(object sender, EventArgs e)
        {
            //this.LoadMapImagePacks();

            this.cboImagePacks.Items.Clear();
            if (this.m_prcClient != null)
            {

                foreach (MapImagePack mipPack in this.MapImagePacks)
                {
                    this.cboImagePacks.Items.Add(mipPack);
                }

                this.cboImagePacks.SelectedItem = this.LoadedMapImagePack;

                foreach (MapZoneDrawing zone in this.m_prcClient.MapGeometry.MapZones)
                {
                    this.MapZones_MapZoneAdded(zone);
                }

                if (this.m_prcClient.SV_Variables.Contains("ZONE_TAG_LIST") == true)
                {
                    this.CheckTagsVariable(this.m_prcClient.SV_Variables["ZONE_TAG_LIST"]);
                }
                else if (this.m_prcClient.Variables.Contains("ZONE_TAG_LIST") == true)
                {
                    this.CheckTagsVariable(this.m_prcClient.Variables["ZONE_TAG_LIST"]);
                }

                this.IsMapLoaded = true;
            }
        }

        private void LoadMapImagePacks()
        {

            this.MapImagePacks.Clear();

            if (Directory.Exists(this.MapPath) == true)
            {
                string[] a_strMapImagePacks = Directory.GetDirectories(this.MapPath);

                foreach (string strMapImagePack in a_strMapImagePacks)
                {
                    if (File.Exists(Path.Combine(Path.Combine(this.MapPath, strMapImagePack), "data.map")) == true)
                    {
                        this.MapImagePacks.Add(new MapImagePack(Path.Combine(this.MapPath, strMapImagePack), new CLocalization(Path.Combine(Path.Combine(this.MapPath, strMapImagePack), "data.map"), "data.map")));
                    }
                }
            }
        }

        public void Initalize(frmMain frmMain)
        {
            this.m_frmMain = frmMain;

            this.tsbPointer.Image = this.m_frmMain.iglIcons.Images["cursor.png"];
            this.tsbCalibration.Image = this.m_frmMain.iglIcons.Images["shape_square_edit.png"];
            this.btnCounterClockwise.Image = this.m_frmMain.iglIcons.Images["shape_rotate_anticlockwise.png"];
            this.btnClockwise.Image = this.m_frmMain.iglIcons.Images["shape_rotate_clockwise.png"];

            this.tsbMeasuringTool.Image = this.m_frmMain.iglIcons.Images["layer-shape-line.png"];

            this.tsbTeamColours.Image = this.m_frmMain.iglIcons.Images["block.png"];

            this.tsbMapZonesTools.Image = this.m_frmMain.iglIcons.Images["layers-ungroup.png"];

            this.btnAddTag.Image = this.m_frmMain.iglIcons.Images["add.png"];
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            if ((this.m_clocLanguage = clocLanguage) != null)
            {
                this.uscBattlemap.SetLocalization(clocLanguage);

                this.tsbPointer.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbPointer");
                // Set the mouse over text for the kill colour
                this.uscBattlemap.KillColours = this.uscBattlemap.KillColours;

                this.tsbMeasuringTool.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbMeasuringTool");
                this.tsbMapZonesTools.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbMapZonesTools");
                this.lblImagePack.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.lblImagePack");
                this.tsbCalibration.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbCalibration");

                this.grpZoneTags.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpZoneTags.Title");
                this.lblTagsHelp.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpZoneTags.lblTagsHelp");

                this.grpMarkerCollection.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpMarkerCollection.Title");
                this.lblTrackPlayersList.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpMarkerCollection.lblTrackPlayersList");
                this.btnClearMarkers.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpMarkerCollection.btnClearMarkers");

                this.grpOffset.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpOffset.Title");
                this.chkLockAxis.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpOffset.chkLockAxis");

                this.grpZoom.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpZoom.Title");
                this.grpRotation.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.grpRotation.Title");

                this.btnSaveCalibration.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.btnSaveCalibration");

            }
        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_prcClient = prcClient) != null)
            {

                if (this.m_prcClient.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.m_prcClient.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.MapImagePacks = new MapImagePackDictionary();
                this.MapPath = Path.Combine(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), sender.GameType), "Maps");
                this.LoadMapImagePacks();

                if (this.MapImagePacks.Count > 0)
                {
                    this.LoadedMapImagePack = this.MapImagePacks[0];
                }

                this.uscBattlemap.SetLocalization(this.m_prcClient.Language);

                //this.m_prcClient.ServerInfo += new FrostbiteConnection.ServerInfoHandler(m_prcClient_ServerInfo);
                this.m_prcClient.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);
                this.m_prcClient.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
                this.m_prcClient.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
                this.m_prcClient.Game.ListPlayers += new FrostbiteClient.ListPlayersHandler(m_prcClient_ListPlayers);
                this.m_prcClient.Game.LoadingLevel += new FrostbiteClient.LoadingLevelHandler(m_prcClient_LoadingLevel);
                this.m_prcClient.Game.LevelLoaded += new FrostbiteClient.LevelLoadedHandler(m_prcClient_LevelLoaded);
                //this.m_prcClient.MapGeometry.MapLoaded += new MapGeometry.MapLoadedHandler(MapGeometry_MapLoaded);

                this.uscBattlemap.MapDetails = new List<CMap>(this.m_prcClient.MapListPool);

                this.cboPlayers.Items.Clear();
                foreach (CPlayerInfo cpiPlayer in this.m_prcClient.PlayerList)
                {
                    if (this.cboPlayers.Items.Contains(cpiPlayer.SoldierName) == false)
                    {
                        this.cboPlayers.Items.Add(cpiPlayer.SoldierName);
                    }
                }

                this.m_prcClient.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);

                this.m_prcClient.Variables.VariableAdded += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableAdded);
                this.m_prcClient.Variables.VariableRemoved += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableRemoved);
                this.m_prcClient.Variables.VariableUpdated += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableUpdated);
                this.m_prcClient.SV_Variables.VariableRemoved += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(SV_Variables_VariableRemoved);
                this.m_prcClient.SV_Variables.VariableUpdated += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(SV_Variables_VariableUpdated);
                this.m_prcClient.SV_Variables.VariableAdded += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(SV_Variables_VariableAdded);

                this.m_prcClient.MapGeometry.MapZones.MapZoneAdded += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneAdded);
                this.m_prcClient.MapGeometry.MapZones.MapZoneRemoved += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneRemoved);
                this.m_prcClient.MapGeometry.MapZones.MapZoneChanged += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneChanged);

                this.m_prcClient.MapZoneDeleted += new PRoConClient.MapZoneEditedHandler(m_prcClient_MapZoneDeleted);
                this.m_prcClient.MapZoneCreated += new PRoConClient.MapZoneEditedHandler(m_prcClient_MapZoneCreated);
                this.m_prcClient.MapZoneModified += new PRoConClient.MapZoneEditedHandler(m_prcClient_MapZoneModified);

                this.m_prcClient.ListMapZones += new PRoConClient.ListMapZonesHandler(m_prcClient_ListMapZones);

                this.m_prcClient.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(m_prcClient_ProconPrivileges);
            });
        }

        private void m_prcClient_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.LoadedMapImagePack != null)
                {

                    if (this.LoadedMapImagePack.LoadedMapFileName.Length == 0 && String.Compare(this.LoadedMapImagePack.LoadedMapFileName, csiServerInfo.Map, true) != 0)
                    {
                        // Load new map.
                        //this.CurrentMapFilename = csiServerInfo.Map.ToLower();

                        if (this.LoadedMapImagePack != null)
                        {
                            this.LoadedMapImagePack.LoadMap(csiServerInfo.Map.ToLower(), this.FullyLoadMap);

                            this.MapLoaded(csiServerInfo.Map.ToLower());
                        }

                    }
                }

                //this.CurrentMapFilename = csiServerInfo.Map.ToLower();
            });
        }

        private void m_prcClient_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_prcClient.SendProconBattlemapListZonesPacket();
                this.m_prcClient.SendGetProconVarsPacket("ZONE_TAG_LIST");

                if ((this.tsbMapZonesTools.Enabled = spPrivs.CanEditMapZones) == false)
                {
                    this.tsbPointer.Checked = true;
                }
            });
        }

        private void m_prcClient_ListPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            this.InvokeIfRequired(() =>
            {
                if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
                {

                    string strSelectedName = (string)this.cboPlayers.SelectedItem;

                    this.cboPlayers.Items.Clear();
                    foreach (CPlayerInfo cpiPlayer in this.m_prcClient.PlayerList)
                    {
                        if (this.cboPlayers.Items.Contains(cpiPlayer.SoldierName) == false)
                        {
                            this.cboPlayers.Items.Add(cpiPlayer.SoldierName);
                        }
                    }

                    if (strSelectedName != null && this.cboPlayers.Items.Contains(strSelectedName) == true)
                    {
                        this.cboPlayers.SelectedItem = strSelectedName;
                    }

                }
            });
        }

        private void m_prcClient_PlayerJoin(FrostbiteClient sender, string playerName)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.cboPlayers.Items.Contains(playerName) == false)
                {
                    this.cboPlayers.Items.Add(playerName);
                }
            });
        }

        private void m_prcClient_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.cboPlayers.Items.Contains(playerName) == true)
                {

                    if (String.Compare((string)this.cboPlayers.SelectedItem, playerName) == 0)
                    {
                        this.uscBattlemap.CalibrationMarkers.Clear();
                    }

                    this.cboPlayers.Items.Remove(playerName);
                }
            });
        }

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kKillerVictimDetails)
        {
            this.InvokeIfRequired(() =>
            {
                this.uscBattlemap.AddKill(kKillerVictimDetails);

                // If suicide and monitoring for suicides for this player.
                if (this.spltCalibration.Panel2Collapsed == false && String.Compare(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName) == 0 && String.Compare((string)this.cboPlayers.SelectedItem, kKillerVictimDetails.Killer.SoldierName) == 0)
                {
                    this.uscBattlemap.CalibrationMarkers.Add(kKillerVictimDetails);
                }
            });
        }

        private CMap GetMap(string strFileName)
        {
            CMap returnMap = new CMap(String.Empty, strFileName, String.Empty, strFileName, 0);

            foreach (CMap map in this.m_prcClient.MapListPool)
            {
                if (String.Compare(map.FileName, strFileName, true) == 0)
                {
                    returnMap = map;
                    break;
                }
            }

            return returnMap;
        }

        private void m_prcClient_LoadingLevel(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal)
        {
            this.InvokeIfRequired(() => this.uscBattlemap.AddRoundChange(this.GetMap(mapFileName)));
        }

        private void m_prcClient_LevelLoaded(FrostbiteClient sender, string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            this.InvokeIfRequired(() => this.uscBattlemap.AddRoundChange(this.GetMap(mapFileName)));
        }

        private void SetMapDetails()
        {
            if (this.LoadedMapImagePack != null)
            {

                this.btnSaveCalibration.Enabled = this.grpMarkerCollection.Enabled = this.grpOffset.Enabled = this.grpZoom.Enabled = this.grpRotation.Enabled = !this.LoadedMapImagePack.Readonly;

                this.chkLockAxis.Checked = (this.LoadedMapImagePack.MapOrigin.X == this.LoadedMapImagePack.MapOrigin.Y);

                if (this.LoadedMapImagePack.MapOrigin.X < this.trkOriginX.Minimum)
                {
                    this.trkOriginX.Value = this.trkOriginX.Minimum;
                }
                else if (this.LoadedMapImagePack.MapOrigin.X > this.trkOriginX.Maximum)
                {
                    this.trkOriginX.Value = this.trkOriginX.Maximum;
                }
                else
                {
                    this.trkOriginX.Value = this.LoadedMapImagePack.MapOrigin.X;
                }

                if (this.LoadedMapImagePack.MapOrigin.Y < this.trkOriginY.Minimum)
                {
                    this.trkOriginY.Value = this.trkOriginY.Minimum;
                }
                else if (this.LoadedMapImagePack.MapOrigin.Y > this.trkOriginY.Maximum)
                {
                    this.trkOriginY.Value = this.trkOriginY.Maximum;
                }
                else
                {
                    this.trkOriginY.Value = this.LoadedMapImagePack.MapOrigin.Y;
                }

                // 0.1 = 1, 0.2 = 2
                int iTrackbarScale = (int)(this.LoadedMapImagePack.MapScale.X * 100.0F);
                if (iTrackbarScale < this.trkZoomX.Minimum)
                {
                    this.trkZoomX.Value = this.trkZoomX.Minimum;
                }
                else if (iTrackbarScale > this.trkZoomX.Maximum)
                {
                    this.trkZoomX.Value = this.trkZoomX.Maximum;
                }
                else
                {
                    this.trkZoomX.Value = iTrackbarScale;
                }

                iTrackbarScale = (int)(this.LoadedMapImagePack.MapScale.Y * 100.0F);
                if (iTrackbarScale < this.trkZoomY.Minimum)
                {
                    this.trkZoomY.Value = this.trkZoomY.Minimum;
                }
                else if (iTrackbarScale > this.trkZoomY.Maximum)
                {
                    this.trkZoomY.Value = this.trkZoomY.Maximum;
                }
                else
                {
                    this.trkZoomY.Value = iTrackbarScale;
                }

                if (this.LoadedMapImagePack.MapRotation < 0.0F)
                {
                    this.numRotation.Value = 0;
                }
                else
                {
                    this.numRotation.Value = (decimal)(this.LoadedMapImagePack.MapRotation % 360.0F);
                }

                //this.lblRotation.Text = String.Format("{0:0}°", this.LoadedMapImagePack.MapRotation);

                if (this.LoadedMapImagePack.MapImage != null)
                {
                    //this.ovMapView.Map = new Bitmap(this.LoadedMapImagePack.MapImage);
                    this.uscBattlemap.Visible = true;
                    this.uscBattlemap.FitOnScreen();
                }
                else
                {
                    this.uscBattlemap.Visible = false;
                }

                if (this.LoadedMapImagePack.Readonly == true)
                {
                    this.lblMapPackFilePath.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.lblMapPackFilePath.ReadOnly");
                }
                else
                {
                    this.lblMapPackFilePath.Text = this.LoadedMapImagePack.MapImagePackDataFile.FilePath;
                }

                this.uscBattlemap.CalibrationMarkers.Clear();
            }
        }

        private void MapLoaded(string strMapName)
        {

            this.uscBattlemap.LoadedMapImagePack = this.LoadedMapImagePack;

            if (this.LoadedMapImagePack != null)
            {
                this.LoadedMapImagePack.MapLoaded += new MapImagePack.MapLoadedHandler(LoadedMapImagePack_MapLoaded);
            }

            this.SetMapDetails();
        }

        private void LoadedMapImagePack_MapLoaded()
        {
            this.SetMapDetails();
        }

        private void cboImagePacks_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cboImagePacks.SelectedItem != null)
            {
                this.SetMapPack((MapImagePack)cboImagePacks.SelectedItem);
            }
        }

        private void uscMapViewer_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        public void SetMapPack(MapImagePack mpiPack)
        {

            string strCurrentMapFileName = String.Empty;

            if (this.LoadedMapImagePack != null)
            {
                strCurrentMapFileName = this.LoadedMapImagePack.LoadedMapFileName;
            }

            this.LoadedMapImagePack = mpiPack;

            if (strCurrentMapFileName.Length > 0)
            {
                this.LoadedMapImagePack.LoadMap(strCurrentMapFileName, this.FullyLoadMap);

                this.MapLoaded(strCurrentMapFileName);

                //if (this.MapLoaded != null) {
                //    this.MapLoaded(strCurrentMapFileName);
                //}
            }
        }

        private void btnCounterClockwise_Click(object sender, EventArgs e)
        {

            this.numRotation.Value = (decimal)((this.LoadedMapImagePack.MapRotation + 90.0F) % 360.0F);

            //if (this.LoadedMapImagePack != null) {
            //    this.LoadedMapImagePack.MapRotation = (this.LoadedMapImagePack.MapRotation + 90.0F) % 360.0F;
            //    this.lblRotation.Text = String.Format("{0:0}°", this.LoadedMapImagePack.MapRotation);
            //}
        }

        private void btnClockwise_Click(object sender, EventArgs e)
        {

            this.numRotation.Value = (decimal)((this.LoadedMapImagePack.MapRotation - 90.0F + 360.0F) % 360.0F);

            //if (this.LoadedMapImagePack != null) {
            //    this.LoadedMapImagePack.MapRotation = (this.LoadedMapImagePack.MapRotation - 9.0F + 360.0F) % 360.0F;
            //    this.lblRotation.Text = String.Format("{0:0}°", this.LoadedMapImagePack.MapRotation);
            //}
        }

        private void numRotation_ValueChanged(object sender, EventArgs e)
        {

            if (this.numRotation.Value < 0)
            {
                this.numRotation.Value = 359;
            }
            else if (this.numRotation.Value == 360)
            {
                this.numRotation.Value = 0;
            }
            else if (this.LoadedMapImagePack != null)
            {
                this.LoadedMapImagePack.MapRotation = (float)this.numRotation.Value;
            }
        }

        private void chkLockAxis_CheckedChanged(object sender, EventArgs e)
        {
            this.trkOriginY.Enabled = !this.chkLockAxis.Checked;
            this.trkOriginY.Value = this.trkOriginX.Value;
        }

        private void trkOriginX_ValueChanged(object sender, EventArgs e)
        {
            if (this.LoadedMapImagePack != null)
            {

                if (this.chkLockAxis.Checked == true)
                {
                    this.trkOriginY.Value = this.trkOriginX.Value;
                }

                this.lblOffsetXValue.Text = String.Format("{0}", this.trkOriginX.Value);

                this.LoadedMapImagePack.MapOrigin = new Point(this.trkOriginX.Value, this.LoadedMapImagePack.MapOrigin.Y);
            }
        }

        private void trkOriginY_ValueChanged(object sender, EventArgs e)
        {
            if (this.LoadedMapImagePack != null)
            {
                this.lblOffsetYValue.Text = String.Format("{0}", this.trkOriginY.Value);

                this.LoadedMapImagePack.MapOrigin = new Point(this.LoadedMapImagePack.MapOrigin.X, this.trkOriginY.Value);
            }
        }

        private void trkZoom_ValueChanged(object sender, EventArgs e)
        {
            if (this.LoadedMapImagePack != null)
            {
                this.lblZoomXValue.Text = String.Format("{0:0.00}x", (float)this.trkZoomX.Value / 100.0F);

                this.LoadedMapImagePack.MapScale = new PointF(this.trkZoomX.Value / 100.0F, this.LoadedMapImagePack.MapScale.Y);
                // this.LoadedMapImagePack.MapScale = (float)this.trkZoomX.Value / 10.0F;
            }
        }


        private void trkZoomY_ValueChanged(object sender, EventArgs e)
        {
            if (this.LoadedMapImagePack != null)
            {
                this.lblZoomYValue.Text = String.Format("{0:0.00}x", (float)this.trkZoomY.Value / 100.0F);

                this.LoadedMapImagePack.MapScale = new PointF(this.LoadedMapImagePack.MapScale.X, this.trkZoomY.Value / 100.0F);
                // this.LoadedMapImagePack.MapScale = (float)this.trkZoomX.Value / 10.0F;
            }
        }

        private void btnClearMarkers_Click(object sender, EventArgs e)
        {
            this.uscBattlemap.CalibrationMarkers.Clear();
        }

        private void btnSaveCalibration_Click(object sender, EventArgs e)
        {
            if (this.LoadedMapImagePack != null)
            {

                string strMapFileName = this.LoadedMapImagePack.LoadedMapFileName;

                this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.Translate.X", strMapFileName), this.LoadedMapImagePack.MapOrigin.X.ToString(CultureInfo.InvariantCulture.NumberFormat));
                this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.Translate.Y", strMapFileName), this.LoadedMapImagePack.MapOrigin.Y.ToString(CultureInfo.InvariantCulture.NumberFormat));

                this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.ScaleX", strMapFileName), this.LoadedMapImagePack.MapScale.X.ToString(CultureInfo.InvariantCulture.NumberFormat));
                this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.ScaleY", strMapFileName), this.LoadedMapImagePack.MapScale.Y.ToString(CultureInfo.InvariantCulture.NumberFormat));
                this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.Rotation", strMapFileName), this.LoadedMapImagePack.MapRotation.ToString(CultureInfo.InvariantCulture.NumberFormat));

                // REMOVE AFTER LEGACY UPDATE
                // this.LoadedMapImagePack.MapImagePackDataFile.SetLocalized(String.Format("{0}.Legacy", strMapFileName), bool.FalseString);
                // END REMOVE AFTER LEGACY UPDATE
            }
        }

        private void tsbCalibration_CheckedChanged(object sender, EventArgs e)
        {
            this.spltCalibration.Panel2Collapsed = !this.tsbCalibration.Checked;
            this.uscBattlemap.DisplayCalibrationGrid = this.tsbCalibration.Checked;
        }

        private void DeselectTools(ToolStripButton tlsButton)
        {
            if (this.tsbPointer != tlsButton) this.tsbPointer.Checked = false;
            if (this.tsbMeasuringTool != tlsButton) this.tsbMeasuringTool.Checked = false;
            if (this.tsbMapZonesTools != tlsButton) this.tsbMapZonesTools.Checked = false;

            this.spltZoneTags.Panel2Collapsed = true;
        }

        private void tsbPointer_CheckedChanged(object sender, EventArgs e)
        {
            if (this.tsbPointer.Checked == true)
            {
                this.DeselectTools(this.tsbPointer);
                this.uscBattlemap.SelectedTool = BattlemapViewTools.Pointer;
            }
        }

        private void tsbDistanceTool_CheckedChanged(object sender, EventArgs e)
        {
            if (this.tsbMeasuringTool.Checked == true)
            {
                this.DeselectTools(this.tsbMeasuringTool);
                this.uscBattlemap.SelectedTool = BattlemapViewTools.Measuring;
            }
        }

        private void tsbMapZonesTools_CheckedChanged(object sender, EventArgs e)
        {
            if (this.tsbMapZonesTools.Checked == true)
            {
                this.DeselectTools(this.tsbMapZonesTools);
                this.uscBattlemap.SelectedTool = BattlemapViewTools.Zones;
                this.spltZoneTags.Panel2Collapsed = false;
            }
        }

        private void tsbTeamColours_Click(object sender, EventArgs e)
        {

            if (this.uscBattlemap.KillColours == KillDisplayColours.EnemyColours)
            {
                this.uscBattlemap.KillColours = KillDisplayColours.TeamColours;
            }
            else
            {
                this.uscBattlemap.KillColours = KillDisplayColours.EnemyColours;
            }
        }

        private void uscBattlemap_KillColoursChanged(KillDisplayColours newKillColour)
        {
            if (newKillColour == KillDisplayColours.EnemyColours)
            {
                this.tsbTeamColours.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbTeamColours.TeamColours");
                // Change to Team Colours image
            }
            else if (newKillColour == KillDisplayColours.TeamColours)
            {
                this.tsbTeamColours.Text = this.m_clocLanguage.GetLocalized("uscMapViewer.tsbTeamColours.EnemyColours");
                // Change to Enemy Colours image
            }
        }

        #region Map Zones

        private void CheckTagsVariable(Variable item)
        {
            if (String.Compare(item.Name, "ZONE_TAG_LIST", true) == 0)
            {
                ZoneTagList tags = new ZoneTagList(item.Value);

                this.cboTagList.Items.Clear();

                foreach (string zoneTag in tags)
                {
                    this.cboTagList.Items.Add(zoneTag);
                }
            }
        }

        private void Variables_VariableAdded(Variable item)
        {
            if (this.m_prcClient.IsPRoConConnection == false)
            {
                this.CheckTagsVariable(item);
            }
        }

        private void Variables_VariableUpdated(Variable item)
        {
            if (this.m_prcClient.IsPRoConConnection == false)
            {
                this.CheckTagsVariable(item);
            }
        }

        private void Variables_VariableRemoved(Variable item)
        {
            if (this.m_prcClient.IsPRoConConnection == false)
            {
                this.CheckTagsVariable(item);
            }
        }

        private void SV_Variables_VariableAdded(Variable item)
        {
            this.CheckTagsVariable(item);
        }

        private void SV_Variables_VariableUpdated(Variable item)
        {
            this.CheckTagsVariable(item);
        }

        private void SV_Variables_VariableRemoved(Variable item)
        {
            this.CheckTagsVariable(item);
        }

        private void btnAddTag_Click(object sender, EventArgs e)
        {

            ZoneTagList tags = new ZoneTagList(this.txtTagList.Text);

            if (this.cboTagList.SelectedItem != null && tags.Contains((string)this.cboTagList.SelectedItem) == false)
            {
                tags.Add((string)this.cboTagList.SelectedItem);
                this.txtTagList.Text = tags.ToString();
            }
        }

        private bool m_isModifyingTags;
        private void txtTagList_TextChanged(object sender, EventArgs e)
        {

            if (this.m_isModifyingTags == false && this.m_prcClient != null && this.txtTagList.Tag != null && this.txtTagList.Tag is MapZoneDrawing)
            {

                this.m_isModifyingTags = true;

                MapZoneDrawing selectedZone = (MapZoneDrawing)this.txtTagList.Tag;

                if (this.m_prcClient.IsPRoConConnection == true)
                {
                    // Send to layer

                    this.m_prcClient.SendProconBattlemapModifyZoneTagsPacket(selectedZone.UID, this.txtTagList.Text);
                }
                else
                {

                    if (this.m_prcClient.MapGeometry.MapZones.Contains(selectedZone.UID) == true)
                    {

                        this.m_prcClient.MapGeometry.MapZones[selectedZone.UID].Tags.FromString(this.txtTagList.Text);
                    }
                }

                this.m_isModifyingTags = false;
            }
        }

        private void uscBattlemap_MapZoneSelected(MapZoneDrawing zone)
        {
            if (zone != null)
            {
                this.m_isModifyingTags = true;

                this.txtTagList.Tag = zone;
                this.txtTagList.Text = zone.Tags.ToString();
                this.spltZoneTags.Panel2.Enabled = true;

                this.m_isModifyingTags = false;
            }
            else
            {
                this.spltZoneTags.Panel2.Enabled = false;
            }
        }

        private void uscBattlemap_ModifyMapZone(string strUid, Point3D[] zonePoints)
        {
            if (this.m_prcClient != null)
            {

                if (this.m_prcClient.IsPRoConConnection == true)
                {
                    //List<string> list = new List<string>() { "procon.battlemap.modifyZonePoints", strUid };
                    //list.Add(zonePoints.Length.ToString());
                    //list.AddRange(Point3D.ToStringList(zonePoints));
                    //this.m_prcClient.SendRequest(list);

                    this.m_prcClient.SendProconBattlemapModifyZonePointsPacket(strUid, zonePoints);
                }
                else
                {
                    this.m_prcClient.MapGeometry.MapZones.ModifyMapZonePoints(strUid, zonePoints);
                }
            }
        }

        private void uscBattlemap_DeleteMapZone(string strUid)
        {
            if (this.m_prcClient != null)
            {

                if (this.m_prcClient.IsPRoConConnection == true)
                {
                    this.m_prcClient.SendProconBattlemapDeleteZonePacket(strUid);
                }
                else
                {
                    if (this.m_prcClient.MapGeometry.MapZones.Contains(strUid) == true)
                    {
                        this.m_prcClient.MapGeometry.MapZones.Remove(strUid);
                    }
                }

            }
        }

        private void uscBattlemap_CreateMapZone(string mapFileName, Point3D[] zonePoints)
        {
            if (this.m_prcClient != null)
            {

                if (this.m_prcClient.IsPRoConConnection == true)
                {
                    // Create it on the layer..
                    //List<string> list = new List<string>() { "procon.battlemap.createZone", mapFileName };
                    //list.Add(zonePoints.Length.ToString());
                    //list.AddRange(Point3D.ToStringList(zonePoints));
                    this.m_prcClient.SendProconBattlemapCreateZonePacket(mapFileName, zonePoints);
                }
                else
                {
                    // Create it locally.
                    this.m_prcClient.MapGeometry.MapZones.CreateMapZone(mapFileName, zonePoints);
                }
            }
        }

        private void MapZones_MapZoneChanged(MapZoneDrawing item)
        {
            if (this.m_isModifyingTags == false)
            {
                this.m_isModifyingTags = true;

                this.uscBattlemap.SetMapZonePoints(item);
                this.uscBattlemap.SetMapZoneTags(item);

                if (this.txtTagList.Tag != null && String.Compare(item.UID, ((MapZoneDrawing)this.txtTagList.Tag).UID) == 0)
                {
                    this.txtTagList.Text = item.Tags.ToString();
                    this.txtTagList.Select(this.txtTagList.Text.Length, 0);
                }

                this.m_isModifyingTags = false;
            }
        }

        private void MapZones_MapZoneAdded(MapZoneDrawing item)
        {
            this.uscBattlemap.AddMapZone(item);
        }

        private void MapZones_MapZoneRemoved(MapZoneDrawing item)
        {
            this.uscBattlemap.RemoveMapZone(item);
        }

        private void m_prcClient_ListMapZones(PRoConClient sender, List<MapZoneDrawing> zones)
        {
            this.InvokeIfRequired(() =>
            {
                foreach (MapZoneDrawing zone in zones)
                {
                    this.uscBattlemap.AddMapZone(zone);
                }
            });
        }

        void m_prcClient_MapZoneModified(PRoConClient sender, MapZoneDrawing zone)
        {
            this.InvokeIfRequired(() => this.MapZones_MapZoneChanged(zone));
        }

        void m_prcClient_MapZoneCreated(PRoConClient sender, MapZoneDrawing zone)
        {
            this.InvokeIfRequired(() => this.MapZones_MapZoneAdded(zone));
        }

        void m_prcClient_MapZoneDeleted(PRoConClient sender, MapZoneDrawing zone)
        {
            this.InvokeIfRequired(() => this.MapZones_MapZoneRemoved(zone));
        }

        #endregion

    }
}
