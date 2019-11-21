using System;
//using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.MOHW
{
    using Core;
    using Core.PlaylistMOHW;
    using Core.Remote;
    public partial class uscServerSettingsGameplayMOHW : uscServerSettingsGameplay
    {

        public uscServerSettingsGameplayMOHW()
            : base()
        {
            InitializeComponent();

            this.chkSettingsHardcore.Enabled = false;
            this.chkSettingsHudCrosshair.Enabled = true;

            this.AsyncSettingControls.Add("vars.teambalance", new AsyncStyleSetting(this.picSettingsTeamBalance, this.chkSettingsTeamBalance, new Control[] { this.chkSettingsTeamBalance }, true));
            this.AsyncSettingControls.Add("vars.killcam", new AsyncStyleSetting(this.picSettingsKillCam, this.chkSettingsKillCam, new Control[] { this.chkSettingsKillCam }, true));

            // deprecated R-5 this.AsyncSettingControls.Add("vars.allUnlocksUnlocked", new AsyncStyleSetting(this.picSettingsAllUnlocksUnlocked, this.chkSettingsAllUnlocksUnlocked, new Control[] { this.chkSettingsAllUnlocksUnlocked }, true));
            this.AsyncSettingControls.Add("vars.buddyOutline", new AsyncStyleSetting(this.picSettingsBuddyOutline, this.chkSettingsBuddyOutline, new Control[] { this.chkSettingsBuddyOutline }, true));

            this.AsyncSettingControls.Add("vars.hudBuddyInfo", new AsyncStyleSetting(this.picSettingsHudBuddyInfo, this.chkSettingsHudBuddyInfo, new Control[] { this.chkSettingsHudBuddyInfo }, true));
            this.AsyncSettingControls.Add("vars.hudClassAbility", new AsyncStyleSetting(this.picSettingsHudClassAbility, this.chkSettingsHudClassAbility, new Control[] { this.chkSettingsHudClassAbility }, true));
            this.AsyncSettingControls.Add("vars.hudCrosshair", new AsyncStyleSetting(this.picSettingsHudCrosshair, this.chkSettingsHudCrosshair, new Control[] { this.chkSettingsHudCrosshair }, true));
            this.AsyncSettingControls.Add("vars.hudEnemyTag", new AsyncStyleSetting(this.picSettingsHudEnemyTag, this.chkSettingsHudEnemyTag, new Control[] { this.chkSettingsHudEnemyTag }, true));
            this.AsyncSettingControls.Add("vars.hudExplosiveIcons", new AsyncStyleSetting(this.picSettingsHudExplosiveIcons, this.chkSettingsHudExplosiveIcons, new Control[] { this.chkSettingsHudExplosiveIcons }, true));
            this.AsyncSettingControls.Add("vars.hudGameMode", new AsyncStyleSetting(this.picSettingsHudGameMode, this.chkSettingsHudGameMode, new Control[] { this.chkSettingsHudGameMode }, true));
            this.AsyncSettingControls.Add("vars.hudHealthAmmo", new AsyncStyleSetting(this.picSettingsHudHealthAmmo, this.chkSettingsHudHealthAmmo, new Control[] { this.chkSettingsHudHealthAmmo }, true));
            this.AsyncSettingControls.Add("vars.hudMinimap", new AsyncStyleSetting(this.picSettingsHudMinimap, this.chkSettingsHudMinimap, new Control[] { this.chkSettingsHudMinimap }, true));
            this.AsyncSettingControls.Add("vars.hudObiturary", new AsyncStyleSetting(this.picSettingsHudObiturary, this.chkSettingsHudObiturary, new Control[] { this.chkSettingsHudObiturary }, true));
            this.AsyncSettingControls.Add("vars.hudPointsTracker", new AsyncStyleSetting(this.picSettingsHudPointsTracker, this.chkSettingsHudPointsTracker, new Control[] { this.chkSettingsHudPointsTracker }, true));
            this.AsyncSettingControls.Add("vars.hudUnlocks", new AsyncStyleSetting(this.picSettingsHudUnlocks, this.chkSettingsHudUnlocks, new Control[] { this.chkSettingsHudUnlocks }, true));

            this.AsyncSettingControls.Add("vars.thirdpersonvehiclecameras", new AsyncStyleSetting(this.picSettingsThirdPersonVehicleCameras, this.chkSettingsThirdPersonVehicleCameras, new Control[] { this.chkSettingsThirdPersonVehicleCameras }, true));
            this.AsyncSettingControls.Add("vars.regeneratehealth", new AsyncStyleSetting(this.picSettingsRegenerateHealth, this.chkSettingsRegenerateHealth, new Control[] { this.chkSettingsRegenerateHealth }, true));

            this.AsyncSettingControls.Add("vars.bulletdamage", new AsyncStyleSetting(this.picSettingsBulletDamage, this.numSettingsBulletDamage, new Control[] { this.numSettingsBulletDamage, this.lnkSettingsBulletDamage }, true));
            this.AsyncSettingControls.Add("vars.roundrestartplayercount", new AsyncStyleSetting(this.picSettingsRoundRestartPlayerCount, this.numSettingsRoundRestartPlayerCount, new Control[] { this.numSettingsRoundRestartPlayerCount, this.lnkSettingsRoundRestartPlayerCount }, true));
            this.AsyncSettingControls.Add("vars.roundstartplayercount", new AsyncStyleSetting(this.picSettingsRoundStartPlayerCount, this.numSettingsRoundStartPlayerCount, new Control[] { this.numSettingsRoundStartPlayerCount, this.lnkSettingsRoundStartPlayerCount }, true));
            this.AsyncSettingControls.Add("vars.soldierhealth", new AsyncStyleSetting(this.picSettingsSoldierHealth, this.numSettingsSoldierHealth, new Control[] { this.numSettingsSoldierHealth, this.lnkSettingsSoldierHealth }, true));

            this.AsyncSettingControls.Add("vars.playerrespawntime", new AsyncStyleSetting(this.picSettingsPlayerRespawnTime, this.numSettingsPlayerRespawnTime, new Control[] { this.numSettingsPlayerRespawnTime, this.lnkSettingsPlayerRespawnTime }, true));

            this.AsyncSettingControls.Add("vars.playlist", new AsyncStyleSetting(this.picSettingsPlaylist, this.cboSettingsPlaylist, new Control[] { this.cboSettingsPlaylist }, true));

            this.AsyncSettingControls.Add("vars.gameModeCounter", new AsyncStyleSetting(this.picSettingsGameModeCounter, this.numSettingsGameModeCounter, new Control[] { this.numSettingsGameModeCounter, this.lnkSettingsGameModeCounter }, true));
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsKillCam.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsKillCam");

            // deprecated R-5 this.chkSettingsAllUnlocksUnlocked.Text = this.Language.GetDefaultLocalized("allUnlocksUnlocked", "uscServerSettingsPanel.chkSettingsAllUnlocksUnlocked");
            this.chkSettingsBuddyOutline.Text = this.Language.GetDefaultLocalized("buddyOutline", "uscServerSettingsPanel.chkSettingsBuddyOutline");

            this.lblHudSettings.Text = this.Language.GetDefaultLocalized("HUD settings:", "uscServerSettingsPanel.lblHudSettings");
            this.chkSettingsHudBuddyInfo.Text = this.Language.GetDefaultLocalized("hudBuddyInfo", "uscServerSettingsPanel.chkSettingsHudBuddyInfo");
            this.chkSettingsHudClassAbility.Text = this.Language.GetDefaultLocalized("hudClassAbility", "uscServerSettingsPanel.chkSettingsHudClassAbility");
            this.chkSettingsHudCrosshair.Text = this.Language.GetDefaultLocalized("hudCorsshair", "uscServerSettingsPanel.chkSettingsCrosshair");
            this.chkSettingsHudEnemyTag.Text = this.Language.GetDefaultLocalized("hudEnemyTag", "uscServerSettingsPanel.chkSettingsHudEnemyTag");
            this.chkSettingsHudExplosiveIcons.Text = this.Language.GetDefaultLocalized("hudExplosiveIcons", "uscServerSettingsPanel.chkSettingsHudExplosiveIcons");
            this.chkSettingsHudGameMode.Text = this.Language.GetDefaultLocalized("hudGameMode", "uscServerSettingsPanel.chkSettingsHudGameMode");
            this.chkSettingsHudHealthAmmo.Text = this.Language.GetDefaultLocalized("hudHealthAmmo", "uscServerSettingsPanel.chkSettingsHudHealthAmmo");
            this.chkSettingsHudMinimap.Text = this.Language.GetDefaultLocalized("hudMinimap", "uscServerSettingsPanel.chkSettingsMinimap");
            this.chkSettingsHudObiturary.Text = this.Language.GetDefaultLocalized("hudObiturary", "uscServerSettingsPanel.chkSettingsHudObiturary");
            this.chkSettingsHudPointsTracker.Text = this.Language.GetDefaultLocalized("hudPointsTracker", "uscServerSettingsPanel.chkSettingsHudPointsTracker");
            this.chkSettingsHudUnlocks.Text = this.Language.GetDefaultLocalized("hudUnlocks", "uscServerSettingsPanel.chkSettingsHudUnlocks");

            this.chkSettingsThirdPersonVehicleCameras.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsThirdPersonVehicleCameras");
            this.chkSettingsTeamBalance.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsTeamBalance");

            this.chkSettingsRegenerateHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsRegenerateHealth");

            this.lblSettingsBulletDamage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsBulletDamage");
            this.lnkSettingsBulletDamage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsBulletDamage");
            this.lblSettingsRoundRestartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsRoundRestartPlayerCount");
            this.lnkSettingsRoundRestartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsRoundRestartPlayerCount");
            this.lblSettingsRoundStartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsRoundStartPlayerCount");
            this.lnkSettingsRoundStartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsRoundStartPlayerCount");
            this.lblSettingsSoldierHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsSoldierHealth");
            this.lnkSettingsSoldierHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSoldierHealth");


            this.lblSettingsPlayerRespawnTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsPlayerRespawnTime");
            this.lnkSettingsPlayerRespawnTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsPlayerRespawnTime");
            this.lblSettingsGameModeCounter.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsGameModeCounter");
            this.lnkSettingsGameModeCounter.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsGameModeCounter");

            this.lblSettingsPlaylist.Text = this.Language.GetDefaultLocalized("Playlist Presets", "uscServerSettingsPanel.lblSettingsPlaylist");
            this.lnkSettingsPlaylist.Text = this.Language.GetDefaultLocalized("Apply", "uscServerSettingsPanel.lnkSettingsPlaylist");

            ArrayList Playlist = new ArrayList();
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("BombSquadPL", "uscServerSettingsPanel.cboSettingsPlaylist.BombSquadPL"), PlaylistMOHWType.BombSquadPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("CombatMissionPL", "uscServerSettingsPanel.cboSettingsPlaylist.CombatMissionPL"), PlaylistMOHWType.CombatMissionPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("CustomPL", "uscServerSettingsPanel.cboSettingsPlaylist.CustomPL"), PlaylistMOHWType.CustomPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("PlatoonPL", "uscServerSettingsPanel.cboSettingsPlaylist.PlatoonPL"), PlaylistMOHWType.PlatoonPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("RealOpsPL", "uscServerSettingsPanel.cboSettingsPlaylist.RealOpsPL"), PlaylistMOHWType.RealOpsPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("SectorControlPL", "uscServerSettingsPanel.cboSettingsPlaylist.SectorControlPL"), PlaylistMOHWType.SectorControlPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("SportPL", "uscServerSettingsPanel.cboSettingsPlaylist.SportPL"), PlaylistMOHWType.SportPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("TeamDeathMatchPL", "uscServerSettingsPanel.cboSettingsPlaylist.TeamDeathMatchPL"), PlaylistMOHWType.TeamDeathMatchPL.ToString()));
            Playlist.Add(new PlaylistMOHW(this.Language.GetDefaultLocalized("WarfighterPL", "uscServerSettingsPanel.cboSettingsPlaylist.WarfighterPL"), PlaylistMOHWType.WarfighterPL.ToString()));

            this.cboSettingsPlaylist.DataSource = Playlist;
            this.cboSettingsPlaylist.DisplayMember = "LongName";
            this.cboSettingsPlaylist.ValueMember = "ShortName";

            this.cboGameplayPresets.Items.Clear();
            this.cboGameplayPresets.Items.Add("-select-");
            this.cboGameplayPresets.Items.Add("Normal");
            this.cboGameplayPresets.Items.Add("Immersive");
            this.cboGameplayPresets.SelectedIndex = 0;

        }

        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if (this.Client != null)
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
                this.Client.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(m_prcClient_TeamBalance);
                this.Client.Game.KillCam += new FrostbiteClient.IsEnabledHandler(m_prcClient_KillCam);

                // deprecated R-5 this.Client.Game.AllUnlocksUnlocked += new FrostbiteClient.IsEnabledHandler(Game_AllUnlocksUnlocked);
                this.Client.Game.BuddyOutline += new FrostbiteClient.IsEnabledHandler(Game_BuddyOutline);

                this.Client.Game.HudBuddyInfo += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudBuddyInfo);
                this.Client.Game.HudClassAbility += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudClassAbility);
                this.Client.Game.HudCrosshair += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudCrosshair);
                this.Client.Game.HudEnemyTag += new FrostbiteClient.IsEnabledHandler(Game_HudEnemyTag);
                this.Client.Game.HudExplosiveIcons += new FrostbiteClient.IsEnabledHandler(Game_HudExplosiveIcons);
                this.Client.Game.HudGameMode += new FrostbiteClient.IsEnabledHandler(Game_HudGameMode);
                this.Client.Game.HudHealthAmmo += new FrostbiteClient.IsEnabledHandler(Game_HudHealthAmmo);
                this.Client.Game.HudMinimap += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudMinimap);
                this.Client.Game.HudObiturary += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudObiturary);
                this.Client.Game.HudPointsTracker += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudPointsTracker);
                this.Client.Game.HudUnlocks += new FrostbiteClient.IsEnabledHandler(m_prcClient_HudUnlocks);

                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);

                this.Client.Game.RegenerateHealth += new FrostbiteClient.IsEnabledHandler(Game_RegenerateHealth);

                this.Client.Game.BulletDamage += new FrostbiteClient.LimitHandler(Game_BulletDamage);
                this.Client.Game.RoundRestartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundRestartPlayerCount);
                this.Client.Game.RoundStartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundStartPlayerCount);

                this.Client.Game.SoldierHealth += new FrostbiteClient.LimitHandler(Game_SoldierHealth);

                this.Client.Game.PlayerRespawnTime += new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);

                this.Client.Game.Playlist += new FrostbiteClient.PlaylistSetHandler(Game_Playlist);

                this.Client.Game.GameModeCounter += new FrostbiteClient.LimitHandler(Game_GameModeCounter);
            });
        }


        #region Third Person Vehicle Cameras

        private void chkSettingsThirdPersonVehicleCameras_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.thirdpersonvehiclecameras"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.thirdpersonvehiclecameras", !this.chkSettingsThirdPersonVehicleCameras.Checked);

                    this.Client.Game.SendSetVarsThirdPersonVehicleCamerasPacket(this.chkSettingsThirdPersonVehicleCameras.Checked);
                }
            }
        }

        private void m_prcClient_ThirdPersonVehicleCameras(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.thirdpersonvehiclecameras", isEnabled, true);
        }

        #endregion

        #region Kill cam

        private void chkSettingsKillCam_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.killcam"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.killcam", !this.chkSettingsKillCam.Checked);

                    this.Client.Game.SendSetVarsKillCamPacket(this.chkSettingsKillCam.Checked);
                }
            }
        }

        private void m_prcClient_KillCam(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.killcam", isEnabled, true);
        }

        #endregion

        #region Team Balance

        private void chkSettingsTeamBalance_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.teambalance"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.teambalance", !this.chkSettingsTeamBalance.Checked);

                    this.Client.Game.SendSetVarsTeamBalancePacket(this.chkSettingsTeamBalance.Checked);
                }
            }
        }

        private void m_prcClient_TeamBalance(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.teambalance", isEnabled, true);
        }

        #endregion

        #region Regenerate Health

        private void chkSettingsRegenerateHealth_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.regeneratehealth"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.regeneratehealth", !this.chkSettingsRegenerateHealth.Checked);

                    this.Client.Game.SendSetVarsRegenerateHealthPacket(this.chkSettingsRegenerateHealth.Checked);
                }
            }
        }

        private void Game_RegenerateHealth(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.regeneratehealth", isEnabled, true);
        }

        #endregion

        #region Bullet Damage

        private int m_iPreviousSuccessBulletDamage;

        void Game_BulletDamage(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessBulletDamage = limit;

            this.OnSettingResponse("vars.bulletdamage", (decimal)limit, true);
        }

        private void lnkSettingsBulletDamage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsBulletDamage.Focus();
                this.WaitForSettingResponse("vars.bulletdamage", (decimal)this.m_iPreviousSuccessBulletDamage);

                this.Client.Game.SendSetVarsBulletDamagePacket((int)this.numSettingsBulletDamage.Value);
            }
        }

        #endregion

        #region Round Restart Player Count

        private int m_iPreviousSuccessRoundRestartPlayerCount;

        void Game_RoundRestartPlayerCount(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRoundRestartPlayerCount = limit;

            this.OnSettingResponse("vars.roundrestartplayercount", (decimal)limit, true);
        }

        private void lnkSettingsRoundRestartPlayerCount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsRoundRestartPlayerCount.Focus();
                this.WaitForSettingResponse("vars.roundrestartplayercount", (decimal)this.m_iPreviousSuccessRoundRestartPlayerCount);

                this.Client.Game.SendSetVarsRoundRestartPlayerCountPacket((int)this.numSettingsRoundRestartPlayerCount.Value);
            }
        }

        #endregion

        #region Round Start Player Count

        private int m_iPreviousSuccessRoundStartPlayerCount;

        void Game_RoundStartPlayerCount(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRoundStartPlayerCount = limit;

            this.OnSettingResponse("vars.roundstartplayercount", (decimal)limit, true);
        }

        private void lnkSettingsRoundStartPlayerCount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsRoundStartPlayerCount.Focus();
                this.WaitForSettingResponse("vars.roundstartplayercount", (decimal)this.m_iPreviousSuccessRoundStartPlayerCount);

                this.Client.Game.SendSetVarsRoundStartPlayerCountPacket((int)this.numSettingsRoundStartPlayerCount.Value);
            }
        }

        #endregion

        #region Soldier Health

        private int m_iPreviousSuccessSoldierHealth;

        void Game_SoldierHealth(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessSoldierHealth = limit;

            this.OnSettingResponse("vars.soldierhealth", (decimal)limit, true);
        }

        private void lnkSettingsSoldierHealth_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsSoldierHealth.Focus();
                this.WaitForSettingResponse("vars.soldierhealth", (decimal)this.m_iPreviousSuccessSoldierHealth);

                this.Client.Game.SendSetVarsSoldierHealthPacket((int)this.numSettingsSoldierHealth.Value);
            }
        }

        #endregion

        #region Player Respawn Time

        private int m_iPreviousSuccessPlayerRespawnTimePacket;

        void Game_PlayerRespawnTime(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessPlayerRespawnTimePacket = limit;

            this.OnSettingResponse("vars.playerrespawntime", (decimal)limit, true);
        }

        private void lnkSettingsPlayerRespawnTime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsPlayerRespawnTime.Focus();
                this.WaitForSettingResponse("vars.playerrespawntime", (decimal)this.m_iPreviousSuccessPlayerRespawnTimePacket);

                this.Client.Game.SendSetVarsPlayerRespawnTimePacket((int)this.numSettingsPlayerRespawnTime.Value);
            }
        }

        #endregion

        #region GameModeCounter

        private int m_iPreviousSuccessGameModeCounterPacket;

        void Game_GameModeCounter(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessGameModeCounterPacket = limit;

            this.OnSettingResponse("vars.gameModeCounter", (decimal)limit, true);
        }

        private void lnkSettingsGameModeCounter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsGameModeCounter.Focus();
                this.WaitForSettingResponse("vars.gameModeCounter", (decimal)this.m_iPreviousSuccessGameModeCounterPacket);

                this.Client.Game.SendSetVarsGameModeCounterPacket((int)this.numSettingsGameModeCounter.Value);
            }
        }

        #endregion

        // MoHW Specific

        #region AllUnlocksUnlocked // deprecated with R-5
        /* deprecated R-5
        private void chkSettingsAllUnlocksUnlocked_CheckedChanged(object sender, EventArgs e) {
            if (this.Client != null && this.Client.Game != null) {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.allUnlocksUnlocked"].IgnoreEvent == false) {
                    this.WaitForSettingResponse("vars.allUnlocksUnlocked", !this.chkSettingsAllUnlocksUnlocked.Checked);

                    this.Client.Game.SendSetVarsAllUnlocksUnlockedPacket(this.chkSettingsAllUnlocksUnlocked.Checked);
                }
            }
        }
        
        private void Game_AllUnlocksUnlocked(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.buddyOutline", isEnabled, true);
        }
        */
        #endregion

        #region Buddy Outline

        private void chkSettingsBuddyOutline_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.buddyOutline"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.buddyOutline", !this.chkSettingsBuddyOutline.Checked);

                    this.Client.Game.SendSetVarsBuddyOutlinePacket(this.chkSettingsBuddyOutline.Checked);
                }
            }
        }

        private void Game_BuddyOutline(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.buddyOutline", isEnabled, true);
        }

        #endregion

        #region hudBuddyInfo

        private void chkSettingsHudBuddyInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudBuddyInfo"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudBuddyInfo", !this.chkSettingsHudBuddyInfo.Checked);

                    this.Client.Game.SendSetVarsHudBuddyInfoPacket(this.chkSettingsHudBuddyInfo.Checked);
                }
            }
        }

        private void m_prcClient_HudBuddyInfo(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudBuddyInfo", isEnabled, true);
        }

        #endregion

        #region hudClassAbility

        private void chkSettingsHudClassAbility_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudClassAbility"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudClassAbility", !this.chkSettingsHudClassAbility.Checked);

                    this.Client.Game.SendSetVarsHudClassAbilityPacket(this.chkSettingsHudClassAbility.Checked);
                }
            }
        }

        private void m_prcClient_HudClassAbility(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudClassAbility", isEnabled, true);
        }

        #endregion

        #region hudCrosshair

        private void chkSettingsHudCrosshair_CheckedChanged(object sender, EventArgs e)
        {

            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudCrosshair"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudCrosshair", !this.chkSettingsHudCrosshair.Checked);

                    this.Client.Game.SendSetVarsHudCrosshairPacket(this.chkSettingsHudCrosshair.Checked);
                }
            }
        }

        private void m_prcClient_HudCrosshair(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudCrosshair", isEnabled, true);
        }

        #endregion

        #region hudEnemyTag

        private void chkSettingsHudEnemyTag_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudEnemyTag"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudEnemyTag", !this.chkSettingsHudEnemyTag.Checked);

                    this.Client.Game.SendSetVarsHudEnemyTagPacket(this.chkSettingsHudEnemyTag.Checked);
                }
            }
        }

        private void Game_HudEnemyTag(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudEnemyTag", isEnabled, true);
        }

        #endregion

        #region hudExplosiveIcons

        private void chkSettingsHudExplosiveIcons_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudExplosiveIcons"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudExplosiveIcons", !this.chkSettingsHudExplosiveIcons.Checked);

                    this.Client.Game.SendSetVarsHudExplosiveIconsPacket(this.chkSettingsHudExplosiveIcons.Checked);
                }
            }
        }

        private void Game_HudExplosiveIcons(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudExplosiveIcons", isEnabled, true);
        }

        #endregion

        #region hudGameMode

        private void chkSettingsHudGameMode_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudGameMode"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudGameMode", !this.chkSettingsHudGameMode.Checked);

                    this.Client.Game.SendSetVarsHudGameModePacket(this.chkSettingsHudGameMode.Checked);
                }
            }
        }

        private void Game_HudGameMode(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudGameMode", isEnabled, true);
        }

        #endregion

        #region hudHealthAmmo

        private void chkSettingsHudHealthAmmo_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudHealthAmmo"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudHealthAmmo", !this.chkSettingsHudHealthAmmo.Checked);

                    this.Client.Game.SendSetVarsHudHealthAmmoPacket(this.chkSettingsHudHealthAmmo.Checked);
                }
            }
        }

        private void Game_HudHealthAmmo(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudHealthAmmo", isEnabled, true);
        }

        #endregion

        #region hudMinimap

        private void chkSettingsHudMinimap_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudMinimap"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudMinimap", !this.chkSettingsHudMinimap.Checked);

                    this.Client.Game.SendSetVarsHudMinimapPacket(this.chkSettingsHudMinimap.Checked);
                }
            }
        }

        private void m_prcClient_HudMinimap(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudMinimap", isEnabled, true);
        }

        #endregion

        #region hudObiturary

        private void chkSettingsHudObiturary_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudObiturary"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudObiturary", !this.chkSettingsHudObiturary.Checked);

                    this.Client.Game.SendSetVarsHudObituraryPacket(this.chkSettingsHudObiturary.Checked);
                }
            }
        }

        private void m_prcClient_HudObiturary(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudObiturary", isEnabled, true);
        }

        #endregion

        #region hudPointsTracker

        private void chkSettingsHudPointsTracker_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudPointsTracker"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudPointsTracker", !this.chkSettingsHudPointsTracker.Checked);

                    this.Client.Game.SendSetVarsHudPointsTrackerPacket(this.chkSettingsHudPointsTracker.Checked);
                }
            }
        }

        private void m_prcClient_HudPointsTracker(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudPointsTracker", isEnabled, true);
        }

        #endregion

        #region hudUnlocks

        private void chkSettingsHudUnlocks_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hudUnlocks"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hudUnlocks", !this.chkSettingsHudUnlocks.Checked);

                    this.Client.Game.SendSetVarsHudUnlocksPacket(this.chkSettingsHudUnlocks.Checked);
                }
            }
        }

        private void m_prcClient_HudUnlocks(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hudUnlocks", isEnabled, true);
        }

        #endregion

        #region PlaylistMOH

        private string m_strPreviousSuccessPlaylist;

        private void Game_Playlist(FrostbiteClient sender, string mode)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_strPreviousSuccessPlaylist = mode.ToString();
                this.OnSettingResponse("vars.playlist", mode.ToString(), true);

                this.cboSettingsPlaylist.SelectedValue = mode.ToString();
            });
        }

        private void lnkSettingsPlaylist_LinkClicked(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.playlist"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.playlist", this.cboSettingsPlaylist.SelectedValue.ToString());
                    this.Client.Game.SendSetVarsPlaylistPacket(this.cboSettingsPlaylist.SelectedValue.ToString());
                    this.Client.Game.SendMapListListRoundsPacket();
                }
            }
        }

        #endregion



        private void btnGameplayPresets_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show(String.Format("Are you sure you wish to overwrite your current config with the \"{0}\" preset?", this.cboGameplayPresets.SelectedItem), "Confirm update", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                bool hudMinimap = true,
                     hudCrosshair = true,
                     hudExplosiveIcons = true,
                     hudHealthAmmo = true,
                     hudObiturary = true,
                     hudPointsTracker = true,
                     hudEnemyTag = true,
                     hudGameMode = true,
                     hudBuddyInfo = true,
                     hudUnlocks = true,
                     buddyOutline = true;


                switch (this.cboGameplayPresets.SelectedIndex)
                {
                    case 0: // just select
                        return;
                    case 1: // Normal
                        this.Client.Game.SendSetVarsRegenerateHealthPacket(true);
                        this.Client.Game.SendSetVarsGameModeCounterPacket(100);
                        this.Client.Game.SendSetVarsBulletDamagePacket(100);
                        this.Client.Game.SendSetVarsSoldierHealthPacket(100);
                        this.Client.Game.SendSetVarsPlayerRespawnTimePacket(100);
                        break;
                    case 2: // Immersive
                        hudMinimap = false;
                        hudCrosshair = false;
                        hudExplosiveIcons = true;
                        hudHealthAmmo = false;
                        hudObiturary = false;
                        hudPointsTracker = false;
                        hudEnemyTag = false;
                        hudGameMode = true;
                        hudBuddyInfo = false;
                        hudUnlocks = true;
                        buddyOutline = false;
                        break;
                }

                this.Client.Game.SendSetVarsHudMinimapPacket(hudMinimap);
                this.Client.Game.SendSetVarsHudCrosshairPacket(hudCrosshair);
                this.Client.Game.SendSetVarsHudExplosiveIconsPacket(hudExplosiveIcons);
                this.Client.Game.SendSetVarsHudHealthAmmoPacket(hudHealthAmmo);
                this.Client.Game.SendSetVarsHudObituraryPacket(hudObiturary);
                this.Client.Game.SendSetVarsHudPointsTrackerPacket(hudPointsTracker);
                this.Client.Game.SendSetVarsHudEnemyTagPacket(hudEnemyTag);
                this.Client.Game.SendSetVarsHudGameModePacket(hudGameMode);
                this.Client.Game.SendSetVarsHudBuddyInfoPacket(hudBuddyInfo);
                this.Client.Game.SendSetVarsHudUnlocksPacket(hudUnlocks);
                this.Client.Game.SendSetVarsBuddyOutlinePacket(buddyOutline);

                this.cboGameplayPresets.SelectedIndex = 0;
            }
        }
    }
}
