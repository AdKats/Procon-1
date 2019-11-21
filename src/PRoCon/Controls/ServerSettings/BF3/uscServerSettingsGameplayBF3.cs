using System;
//using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.BF3
{
    using Core;
    using Core.GunMasterWeaponsPreset;
    using Core.Remote;
    using Core.UnlockMode;
    public partial class uscServerSettingsGameplayBF3 : uscServerSettingsGameplay
    {

        public uscServerSettingsGameplayBF3()
            : base()
        {
            InitializeComponent();

            this.chkSettingsHardcore.Enabled = false;
            this.chkSettingsCrosshair.Enabled = false;

            this.AsyncSettingControls.Add("vars.teambalance", new AsyncStyleSetting(this.picSettingsTeamBalance, this.chkSettingsTeamBalance, new Control[] { this.chkSettingsTeamBalance }, true));
            this.AsyncSettingControls.Add("vars.killcam", new AsyncStyleSetting(this.picSettingsKillCam, this.chkSettingsKillCam, new Control[] { this.chkSettingsKillCam }, true));
            this.AsyncSettingControls.Add("vars.minimap", new AsyncStyleSetting(this.picSettingsMinimap, this.chkSettingsMinimap, new Control[] { this.chkSettingsMinimap }, true));
            //this.AsyncSettingControls.Add("vars.crosshair", new AsyncStyleSetting(this.picSettingsCrosshair, this.chkSettingsCrosshair, new Control[] { this.chkSettingsCrosshair }, true));
            this.AsyncSettingControls.Add("vars.3dspotting", new AsyncStyleSetting(this.picSettings3DSpotting, this.chkSettings3DSpotting, new Control[] { this.chkSettings3DSpotting }, true));
            this.AsyncSettingControls.Add("vars.minimapspotting", new AsyncStyleSetting(this.picSettingsMinimapSpotting, this.chkSettingsMinimapSpotting, new Control[] { this.chkSettingsMinimapSpotting }, true));
            this.AsyncSettingControls.Add("vars.thirdpersonvehiclecameras", new AsyncStyleSetting(this.picSettingsThirdPersonVehicleCameras, this.chkSettingsThirdPersonVehicleCameras, new Control[] { this.chkSettingsThirdPersonVehicleCameras }, true));

            this.AsyncSettingControls.Add("vars.nametag", new AsyncStyleSetting(this.picSettingsNameTag, this.chkSettingsNameTag, new Control[] { this.chkSettingsNameTag }, true));
            this.AsyncSettingControls.Add("vars.regeneratehealth", new AsyncStyleSetting(this.picSettingsRegenerateHealth, this.chkSettingsRegenerateHealth, new Control[] { this.chkSettingsRegenerateHealth }, true));
            this.AsyncSettingControls.Add("vars.hud", new AsyncStyleSetting(this.picSettingsHud, this.chkSettingsHud, new Control[] { this.chkSettingsHud }, true));
            this.AsyncSettingControls.Add("vars.onlysquadleaderspawn", new AsyncStyleSetting(this.picSettingsOnlySquadLeaderSpawn, this.chkSettingsOnlySquadLeaderSpawn, new Control[] { this.chkSettingsOnlySquadLeaderSpawn }, true));

            this.AsyncSettingControls.Add("vars.unlockmode", new AsyncStyleSetting(this.picSettingsUnlockMode, this.cboSettingsUnlockMode, new Control[] { this.cboSettingsUnlockMode }, true));
            this.AsyncSettingControls.Add("vars.gunMasterWeaponsPreset", new AsyncStyleSetting(this.picSettingsGunMasterWeaponsPreset, this.cboSettingsGunMasterWeaponsPreset, new Control[] { this.cboSettingsGunMasterWeaponsPreset }, true));

            this.AsyncSettingControls.Add("vars.vehiclespawnallowed", new AsyncStyleSetting(this.picSettingsVehicleSpawnAllowed, this.chkSettingsVehicleSpawnAllowed, new Control[] { this.chkSettingsVehicleSpawnAllowed }, true));
            this.AsyncSettingControls.Add("vars.vehiclespawndelay", new AsyncStyleSetting(this.picSettingsVehicleSpawnDelay, this.numSettingsVehicleSpawnDelay, new Control[] { this.numSettingsVehicleSpawnDelay, this.lnkSettingsVehicleSpawnDelay }, true));

            this.AsyncSettingControls.Add("vars.bulletdamage", new AsyncStyleSetting(this.picSettingsBulletDamage, this.numSettingsBulletDamage, new Control[] { this.numSettingsBulletDamage, this.lnkSettingsBulletDamage }, true));
            this.AsyncSettingControls.Add("vars.roundrestartplayercount", new AsyncStyleSetting(this.picSettingsRoundRestartPlayerCount, this.numSettingsRoundRestartPlayerCount, new Control[] { this.numSettingsRoundRestartPlayerCount, this.lnkSettingsRoundRestartPlayerCount }, true));
            this.AsyncSettingControls.Add("vars.roundstartplayercount", new AsyncStyleSetting(this.picSettingsRoundStartPlayerCount, this.numSettingsRoundStartPlayerCount, new Control[] { this.numSettingsRoundStartPlayerCount, this.lnkSettingsRoundStartPlayerCount }, true));
            this.AsyncSettingControls.Add("vars.soldierhealth", new AsyncStyleSetting(this.picSettingsSoldierHealth, this.numSettingsSoldierHealth, new Control[] { this.numSettingsSoldierHealth, this.lnkSettingsSoldierHealth }, true));
            this.AsyncSettingControls.Add("vars.playermandowntime", new AsyncStyleSetting(this.picSettingsPlayerManDownTime, this.numSettingsPlayerManDownTime, new Control[] { this.numSettingsPlayerManDownTime, this.lnkSettingsPlayerManDownTime }, true));
            this.AsyncSettingControls.Add("vars.playerrespawntime", new AsyncStyleSetting(this.picSettingsPlayerRespawnTime, this.numSettingsPlayerRespawnTime, new Control[] { this.numSettingsPlayerRespawnTime, this.lnkSettingsPlayerRespawnTime }, true));

            this.AsyncSettingControls.Add("vars.gameModeCounter", new AsyncStyleSetting(this.picSettingsGameModeCounter, this.numSettingsGameModeCounter, new Control[] { this.numSettingsGameModeCounter, this.lnkSettingsGameModeCounter }, true));
            this.AsyncSettingControls.Add("vars.ctfRoundTimeModifier", new AsyncStyleSetting(this.picSettingsCtfRoundTimeModifier, this.numSettingsCtfRoundTimeModifier, new Control[] { this.numSettingsCtfRoundTimeModifier, this.lnkSettingsCtfRoundTimeModifier }, true));
            this.AsyncSettingControls.Add("vars.roundLockdownCountdown", new AsyncStyleSetting(this.picSettingsLockdownCountdown, this.numSettingsLockdownCountdown, new Control[] { this.numSettingsLockdownCountdown, this.lnkSettingsLockdownCountdown }, true));
            this.AsyncSettingControls.Add("vars.roundWarmupTimeout", new AsyncStyleSetting(this.picSettingsWarmupTimeout, this.numSettingsWarmupTimeout, new Control[] { this.numSettingsWarmupTimeout, this.lnkSettingsWarmupTimeout }, true));
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsKillCam.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsKillCam");

            this.chkSettingsMinimap.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsMinimap");
            this.chkSettingsCrosshair.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsCrosshair");
            this.chkSettings3DSpotting.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettings3DSpotting");
            this.chkSettingsMinimapSpotting.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsMinimapSpotting");
            this.chkSettingsThirdPersonVehicleCameras.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsThirdPersonVehicleCameras");
            this.chkSettingsTeamBalance.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsTeamBalance");

            this.chkSettingsNameTag.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsNameTag");
            this.chkSettingsRegenerateHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsRegenerateHealth");
            this.chkSettingsHud.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsHud");
            this.chkSettingsOnlySquadLeaderSpawn.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsOnlySquadLeaderSpawn");

            this.chkSettingsVehicleSpawnAllowed.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsVehicleSpawnAllowed");
            this.lblSettingsVehicleSpawnDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsVehicleSpawnDelay");
            this.lnkSettingsVehicleSpawnDelay.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsVehicleSpawnDelay");
            this.lblSettingsBulletDamage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsBulletDamage");
            this.lnkSettingsBulletDamage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsBulletDamage");
            this.lblSettingsRoundRestartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsRoundRestartPlayerCount");
            this.lnkSettingsRoundRestartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsRoundRestartPlayerCount");
            this.lblSettingsRoundStartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsRoundStartPlayerCount");
            this.lnkSettingsRoundStartPlayerCount.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsRoundStartPlayerCount");
            this.lblSettingsSoldierHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsSoldierHealth");
            this.lnkSettingsSoldierHealth.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSoldierHealth");
            this.lblSettingsPlayerManDownTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsPlayerManDownTime");
            this.lnkSettingsPlayerManDownTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsPlayerManDownTime");
            this.lblSettingsPlayerRespawnTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsPlayerRespawnTime");
            this.lnkSettingsPlayerRespawnTime.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsPlayerRespawnTime");
            this.lblSettingsGameModeCounter.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsGameModeCounter");
            this.lnkSettingsGameModeCounter.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsGameModeCounter");
            this.lblSettingsCtfRoundTimeModifier.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsCtfRoundTimeModifier");
            this.lnkSettingsCtfRoundTimeModifier.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsCtfRoundTimeModifier");
            this.lblSettingsLockdownCountdown.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsLockdownCountdown");
            this.lnkSettingsLockdownCountdown.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsLockdownCountdown");
            this.lblSettingsWarmupTimeout.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsWarmupTimeout");
            this.lnkSettingsWarmupTimeout.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsWarmupTimeout");

            this.lblSettingsUnlockMode.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsUnlockMode");
            this.lnkSettingsUnlockMode.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsUnlockMode");

            this.cboGameplayPresets.Items.Clear();
            this.cboGameplayPresets.Items.Add("Quickmatch");
            this.cboGameplayPresets.Items.Add("Normal");
            this.cboGameplayPresets.Items.Add("Hardcore");
            this.cboGameplayPresets.Items.Add("Infantry Only");
            this.cboGameplayPresets.SelectedIndex = 0;

            ArrayList UnlockModes = new ArrayList();
            UnlockModes.Add(new UnlockMode(this.Language.GetLocalized("uscServerSettingsPanel.cboSettingsUnlockMode.None"), UnlockModeType.none.ToString()));
            UnlockModes.Add(new UnlockMode(this.Language.GetLocalized("uscServerSettingsPanel.cboSettingsUnlockMode.All"), UnlockModeType.all.ToString()));
            UnlockModes.Add(new UnlockMode(this.Language.GetLocalized("uscServerSettingsPanel.cboSettingsUnlockMode.Common"), UnlockModeType.common.ToString()));
            UnlockModes.Add(new UnlockMode(this.Language.GetLocalized("uscServerSettingsPanel.cboSettingsUnlockMode.Stats"), UnlockModeType.stats.ToString()));

            this.cboSettingsUnlockMode.DataSource = UnlockModes;
            this.cboSettingsUnlockMode.DisplayMember = "LongName";
            this.cboSettingsUnlockMode.ValueMember = "ShortName";

            this.lblSettingsGunMasterWeaponsPreset.Text = this.Language.GetDefaultLocalized(this.lblSettingsGunMasterWeaponsPreset.Text, "uscServerSettingsPanel.lblSettingsGunMasterWeaponsPreset");
            this.lnkSettingsGunMasterWeaponsPreset.Text = this.Language.GetDefaultLocalized(this.lnkSettingsGunMasterWeaponsPreset.Text, "uscServerSettingsPanel.lnkSettingsGunMasterWeaponsPreset");

            ArrayList GunMasterWeaponsPresets = new ArrayList();
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Standard", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.Standard"), ((int)GunMasterWeaponsPresetType.standard).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Standard reversed", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.Reversed"), ((int)GunMasterWeaponsPresetType.reversed).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Leight Weight", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.LightWeight"), ((int)GunMasterWeaponsPresetType.light_weigth).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Heavy Gear", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.HeavyGear"), ((int)GunMasterWeaponsPresetType.heavy_gear).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Pistol run", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.PistolRun"), ((int)GunMasterWeaponsPresetType.pistol_run).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("Snipers Heaven", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.SnipersHeaven"), ((int)GunMasterWeaponsPresetType.snipers_heaven).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("US arms race", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.UsArmsRace"), ((int)GunMasterWeaponsPresetType.us_arms_race).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("RU arms race", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.RuArmsRace"), ((int)GunMasterWeaponsPresetType.ru_arms_race).ToString()));
            GunMasterWeaponsPresets.Add(new GunMasterWeaponsPreset(this.Language.GetDefaultLocalized("EU arms race", "uscServerSettingsPanel.cboSettingsGunMasterWeaponsPreset.EuArmsRace"), ((int)GunMasterWeaponsPresetType.eu_arms_race).ToString()));

            this.cboSettingsGunMasterWeaponsPreset.DataSource = GunMasterWeaponsPresets;
            this.cboSettingsGunMasterWeaponsPreset.DisplayMember = "LongName";
            this.cboSettingsGunMasterWeaponsPreset.ValueMember = "ShortName";


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

            // override RoundStart minimum in case server is unranked
            if (this.Client.CurrentServerInfo.Ranked == false)
            {
                this.numSettingsRoundStartPlayerCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(m_prcClient_TeamBalance);
                this.Client.Game.KillCam += new FrostbiteClient.IsEnabledHandler(m_prcClient_KillCam);
                this.Client.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMap);
                this.Client.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(m_prcClient_CrossHair);
                this.Client.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThreeDSpotting);
                this.Client.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMapSpotting);
                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);

                this.Client.Game.NameTag += new FrostbiteClient.IsEnabledHandler(Game_NameTag);
                this.Client.Game.OnlySquadLeaderSpawn += new FrostbiteClient.IsEnabledHandler(Game_OnlySquadLeaderSpawn);
                this.Client.Game.RegenerateHealth += new FrostbiteClient.IsEnabledHandler(Game_RegenerateHealth);
                this.Client.Game.Hud += new FrostbiteClient.IsEnabledHandler(Game_Hud);

                this.Client.Game.UnlockMode += new FrostbiteClient.UnlockModeHandler(Game_UnlockMode);
                this.Client.Game.GunMasterWeaponsPreset += new FrostbiteClient.GunMasterWeaponsPresetHandler(Game_GunMasterWeaponsPreset);

                this.Client.Game.VehicleSpawnAllowed += new FrostbiteClient.IsEnabledHandler(Game_VehicleSpawnAllowed);
                this.Client.Game.VehicleSpawnDelay += new FrostbiteClient.LimitHandler(Game_VehicleSpawnDelay);

                this.Client.Game.BulletDamage += new FrostbiteClient.LimitHandler(Game_BulletDamage);
                this.Client.Game.RoundRestartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundRestartPlayerCount);
                this.Client.Game.RoundStartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundStartPlayerCount);

                this.Client.Game.SoldierHealth += new FrostbiteClient.LimitHandler(Game_SoldierHealth);
                this.Client.Game.PlayerManDownTime += new FrostbiteClient.LimitHandler(Game_PlayerManDownTime);

                this.Client.Game.PlayerRespawnTime += new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);

                this.Client.Game.GameModeCounter += new FrostbiteClient.LimitHandler(Game_GameModeCounter);
                this.Client.Game.CtfRoundTimeModifier += new FrostbiteClient.LimitHandler(Game_CtfRoundTimeModifier);
                this.Client.Game.RoundLockdownCountdown += new FrostbiteClient.LimitHandler(Game_RoundLockdownCountdown);
                this.Client.Game.RoundWarmupTimeout += new FrostbiteClient.LimitHandler(Game_RoundWarmupTimeout);
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

        #region Minimap Spotting

        private void chkSettingsMinimapSpotting_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.minimapspotting"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.minimapspotting", !this.chkSettingsMinimapSpotting.Checked);

                    this.Client.Game.SendSetVarsMiniMapSpottingPacket(this.chkSettingsMinimapSpotting.Checked);
                }
            }
        }

        private void m_prcClient_MiniMapSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.minimapspotting", isEnabled, true);
        }

        #endregion

        #region 3d Spotting

        private void chkSettings3DSpotting_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.3dspotting"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.3dspotting", !this.chkSettings3DSpotting.Checked);

                    this.Client.Game.SendSetVars3dSpottingPacket(this.chkSettings3DSpotting.Checked);
                }
            }
        }

        private void m_prcClient_ThreeDSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.3dspotting", isEnabled, true);
        }

        #endregion

        #region Cross hair

        private void chkSettingsCrosshair_CheckedChanged(object sender, EventArgs e)
        {

            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.crosshair"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.crosshair", !this.chkSettingsCrosshair.Checked);

                    this.Client.Game.SendSetVarsCrossHairPacket(this.chkSettingsCrosshair.Checked);
                }
            }
        }

        private void m_prcClient_CrossHair(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.crosshair", isEnabled, true);
        }

        #endregion

        #region Mini map

        private void chkSettingsMinimap_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.minimap"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.minimap", !this.chkSettingsMinimap.Checked);

                    this.Client.Game.SendSetVarsMiniMapPacket(this.chkSettingsMinimap.Checked);
                }
            }
        }

        private void m_prcClient_MiniMap(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.minimap", isEnabled, true);
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

        // BF3 Specific

        #region Name Tag

        private void chkSettingsNameTag_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.nametag"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.nametag", !this.chkSettingsNameTag.Checked);

                    this.Client.Game.SendSetVarsNameTagPacket(this.chkSettingsNameTag.Checked);
                }
            }
        }

        private void Game_NameTag(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.nametag", isEnabled, true);
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

        #region Show HUD

        private void chkSettingsHud_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hud"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hud", !this.chkSettingsHud.Checked);

                    this.Client.Game.SendSetVarsHudPacket(this.chkSettingsHud.Checked);
                }
            }
        }

        private void Game_Hud(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hud", isEnabled, true);
        }

        #endregion

        #region Only Squad Leader Spawn

        private void chkSettingsOnlySquadLeaderSpawn_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.onlysquadleaderspawn"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.onlysquadleaderspawn", !this.chkSettingsOnlySquadLeaderSpawn.Checked);

                    this.Client.Game.SendSetVarsOnlySquadLeaderSpawnPacket(this.chkSettingsOnlySquadLeaderSpawn.Checked);
                }
            }
        }

        private void Game_OnlySquadLeaderSpawn(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.onlysquadleaderspawn", isEnabled, true);
        }

        #endregion

        #region UnlockMode

        private string m_strPreviousSuccessUnlockMode;

        private void Game_UnlockMode(FrostbiteClient sender, string mode)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_strPreviousSuccessUnlockMode = mode.ToString();
                this.OnSettingResponse("vars.unlockmode", mode.ToString(), true);

                this.cboSettingsUnlockMode.SelectedValue = mode.ToString().ToLower();
            });
        }

        private void lnkSettingsUnlockMode_LinkClicked(object sender, EventArgs e)
        {
            // see line 126 uscServerSettingsTextChatModeration.cs
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.unlockmode"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.unlockmode", this.cboSettingsUnlockMode.SelectedValue.ToString());

                    this.Client.Game.SendSetVarsUnlockModePacket(this.cboSettingsUnlockMode.SelectedValue.ToString());
                }
            }
        }

        #endregion

        #region GunMasterWeaponsPreset

        private int m_iPreviousSuccessGunMasterWeaponsPreset;

        private void Game_GunMasterWeaponsPreset(FrostbiteClient sender, int preset)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_iPreviousSuccessGunMasterWeaponsPreset = preset;
                this.OnSettingResponse("vars.gunMasterWeaponsPreset", (decimal)preset, true);

                this.cboSettingsGunMasterWeaponsPreset.SelectedValue = preset.ToString();
            });
        }

        private void lnkSettingsGunMasterWeaponsPreset_LinkClicked(object sender, EventArgs e)
        {
            // see line 126 uscServerSettingsTextChatModeration.cs
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.gunMasterWeaponsPreset"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.gunMasterWeaponsPreset", (decimal)this.m_iPreviousSuccessGunMasterWeaponsPreset);

                    this.Client.Game.SendSetVarsGunMasterWeaponsPresetPacket(Convert.ToInt32(this.cboSettingsGunMasterWeaponsPreset.SelectedValue));
                }
            }
        }

        #endregion

        #region Vehicle Spawning

        private void chkSettingsVehicleSpawnAllowed_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.vehiclespawnallowed"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.vehiclespawnallowed", !this.chkSettingsVehicleSpawnAllowed.Checked);

                    this.Client.Game.SendSetVarsVehicleSpawnAllowedPacket(this.chkSettingsVehicleSpawnAllowed.Checked);
                }
            }
        }

        private void Game_VehicleSpawnAllowed(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.vehiclespawnallowed", isEnabled, true);
        }

        #endregion

        #region Vehicle Spawn Delay

        private int m_iPreviousSuccessVehicleSpawnDelaySecond;

        void Game_VehicleSpawnDelay(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessVehicleSpawnDelaySecond = limit;

            this.OnSettingResponse("vars.vehiclespawndelay", (decimal)limit, true);
        }

        private void lnkSettingsVehicleSpawnDelay_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsVehicleSpawnDelay.Focus();
                this.WaitForSettingResponse("vars.vehiclespawndelay", (decimal)this.m_iPreviousSuccessVehicleSpawnDelaySecond);

                this.Client.Game.SendSetVarsVehicleSpawnDelayPacket((int)this.numSettingsVehicleSpawnDelay.Value);
            }
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
            if (limit == -1) { limit = 2; }
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
            if (limit == -1) { limit = 4; }
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

        #region Player ManDown Time

        private int m_iPreviousSuccessPlayerManDownTimePacket;

        void Game_PlayerManDownTime(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessPlayerManDownTimePacket = limit;

            this.OnSettingResponse("vars.playermandowntime", (decimal)limit, true);
        }

        private void lnkSettingsPlayerManDownTime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsPlayerManDownTime.Focus();
                this.WaitForSettingResponse("vars.playermandowntime", (decimal)this.m_iPreviousSuccessPlayerManDownTimePacket);

                this.Client.Game.SendSetVarsPlayerManDownTimePacket((int)this.numSettingsPlayerManDownTime.Value);
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
                this.numSettingsPlayerManDownTime.Focus();
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

        #region CtfRoundTimeModifier

        private int m_iPreviousSuccessCtfRoundTimeModifierPacket;

        void Game_CtfRoundTimeModifier(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessCtfRoundTimeModifierPacket = limit;

            this.OnSettingResponse("vars.ctfRoundTimeModifier", (decimal)limit, true);
        }

        private void lnkSettingsCtfRoundTimeModifier_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsCtfRoundTimeModifier.Focus();
                this.WaitForSettingResponse("vars.ctfRoundTimeModifier", (decimal)this.m_iPreviousSuccessCtfRoundTimeModifierPacket);

                this.Client.Game.SendSetVarsCtfRoundTimeModifierPacket((int)this.numSettingsCtfRoundTimeModifier.Value);
            }
        }

        #endregion

        #region RoundLockdownCountdown & RoundWarmupTimeout

        private int m_iPreviousSuccessRoundLockdownCountdownPacket;

        void Game_RoundLockdownCountdown(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRoundLockdownCountdownPacket = limit;

            this.OnSettingResponse("vars.roundLockdownCountdown", (decimal)limit, true);
        }

        private void lnkSettingsLockdownCountdown_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsLockdownCountdown.Focus();
                this.WaitForSettingResponse("vars.roundLockdownCountdown", (decimal)this.m_iPreviousSuccessRoundLockdownCountdownPacket);

                this.Client.Game.SendSetVarsRoundLockdownCountdownPacket((int)this.numSettingsLockdownCountdown.Value);
            }
        }

        private int m_iPreviousSuccessRoundWarmupTimeout;

        void Game_RoundWarmupTimeout(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRoundWarmupTimeout = limit;

            this.OnSettingResponse("vars.roundWarmupTimeout", (decimal)limit, true);
        }

        private void lnkSettingsWarmupTimeout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsWarmupTimeout.Focus();
                this.WaitForSettingResponse("vars.roundWarmupTimeout", (decimal)this.m_iPreviousSuccessRoundWarmupTimeout);

                this.Client.Game.SendSetVarsRoundWarmupTimeoutPacket((int)this.numSettingsWarmupTimeout.Value);
            }
        }

        #endregion

        private void btnGameplayPresets_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show(String.Format("Are you sure you wish to overwrite your current config with the \"{0}\" preset?", this.cboGameplayPresets.SelectedItem), "Confirm update", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                bool friendlyFire = false,
                    killCam = true,
                    hud = true,
                    threedSpotting = true,
                    nameTag = true,
                    thirdPCam = true,
                    vehicleSpawnAllowed = true,
                    onlySquadLeaderSpawn = false,
                    regenerateHealth = true;

                int soldierHealth = 100;

                switch (this.cboGameplayPresets.SelectedIndex)
                {
                    case 0: // Quickmatch
                        this.Client.Game.SendSetVarsRoundStartPlayerCountPacket(8);
                        this.Client.Game.SendSetVarsRoundRestartPlayerCountPacket(4);
                        this.Client.Game.SendSetVarsRegenerateHealthPacket(true);
                        this.Client.Game.SendSetVarsCrossHairPacket(true);
                        this.Client.Game.SendSetVarsVehicleSpawnDelayPacket(100);
                        this.Client.Game.SendSetVarsGameModeCounterPacket(100);
                        this.Client.Game.SendSetVarsCtfRoundTimeModifierPacket(100);
                        break;
                    case 1: // Normal

                        break;
                    case 2: // Hardcore
                        friendlyFire = true;
                        killCam = false;
                        hud = false;
                        threedSpotting = false;
                        nameTag = false;
                        thirdPCam = false;
                        soldierHealth = 60;
                        onlySquadLeaderSpawn = true;
                        regenerateHealth = false;
                        break;
                    case 3: // Infantry Only
                        thirdPCam = false;
                        vehicleSpawnAllowed = false;
                        break;
                }

                this.Client.Game.SendSetVarsTeamBalancePacket(true);
                this.Client.Game.SendSetVarsFriendlyFirePacket(friendlyFire);
                this.Client.Game.SendSetVarsKillCamPacket(killCam);
                this.Client.Game.SendSetVarsMiniMapPacket(true);
                this.Client.Game.SendSetVarsHudPacket(hud);
                this.Client.Game.SendSetVars3dSpottingPacket(threedSpotting);
                this.Client.Game.SendSetVarsMiniMapSpottingPacket(true);
                this.Client.Game.SendSetVarsNameTagPacket(nameTag);
                this.Client.Game.SendSetVarsThirdPersonVehicleCamerasPacket(thirdPCam);
                this.Client.Game.SendSetVarsVehicleSpawnAllowedPacket(vehicleSpawnAllowed);
                this.Client.Game.SendSetVarsSoldierHealthPacket(soldierHealth);
                this.Client.Game.SendSetVarsRegenerateHealthPacket(regenerateHealth);
                this.Client.Game.SendSetVarsPlayerRespawnTimePacket(100);
                this.Client.Game.SendSetVarsPlayerManDownTimePacket(100);
                this.Client.Game.SendSetVarsBulletDamagePacket(100);
                this.Client.Game.SendSetVarsOnlySquadLeaderSpawnPacket(onlySquadLeaderSpawn);
            }
        }
    }
}
