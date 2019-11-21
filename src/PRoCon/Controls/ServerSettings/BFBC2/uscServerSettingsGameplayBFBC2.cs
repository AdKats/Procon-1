using System;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.BFBC2
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsGameplayBFBC2 : uscServerSettingsGameplay
    {

        private int m_iPreviousSuccessRankLimit;

        public uscServerSettingsGameplayBFBC2()
            : base()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.ranklimit -1", new AsyncStyleSetting(this.picSettingsRankLimit, this.chkSettingsNoRankLimit, new Control[] { this.chkSettingsNoRankLimit }, true));
            this.AsyncSettingControls.Add("vars.ranklimit", new AsyncStyleSetting(this.picSettingsRankLimit, this.numSettingsRankLimit, new Control[] { this.numSettingsRankLimit, this.lnkSettingsSetRankLimit }, true));

            this.AsyncSettingControls.Add("vars.teambalance", new AsyncStyleSetting(this.picSettingsTeamBalance, this.chkSettingsTeamBalance, new Control[] { this.chkSettingsTeamBalance }, true));
            this.AsyncSettingControls.Add("vars.killcam", new AsyncStyleSetting(this.picSettingsKillCam, this.chkSettingsKillCam, new Control[] { this.chkSettingsKillCam }, true));
            this.AsyncSettingControls.Add("vars.minimap", new AsyncStyleSetting(this.picSettingsMinimap, this.chkSettingsMinimap, new Control[] { this.chkSettingsMinimap }, true));
            this.AsyncSettingControls.Add("vars.crosshair", new AsyncStyleSetting(this.picSettingsCrosshair, this.chkSettingsCrosshair, new Control[] { this.chkSettingsCrosshair }, true));
            this.AsyncSettingControls.Add("vars.3dspotting", new AsyncStyleSetting(this.picSettings3DSpotting, this.chkSettings3DSpotting, new Control[] { this.chkSettings3DSpotting }, true));
            this.AsyncSettingControls.Add("vars.minimapspotting", new AsyncStyleSetting(this.picSettingsMinimapSpotting, this.chkSettingsMinimapSpotting, new Control[] { this.chkSettingsMinimapSpotting }, true));
            this.AsyncSettingControls.Add("vars.thirdpersonvehiclecameras", new AsyncStyleSetting(this.picSettingsThirdPersonVehicleCameras, this.chkSettingsThirdPersonVehicleCameras, new Control[] { this.chkSettingsThirdPersonVehicleCameras }, true));

            this.m_iPreviousSuccessRankLimit = 50;
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

            this.chkSettingsNoRankLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsNoRankLimit");
            this.lnkSettingsSetRankLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetRankLimit");
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
                this.Client.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMap);
                this.Client.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(m_prcClient_CrossHair);
                this.Client.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThreeDSpotting);
                this.Client.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMapSpotting);
                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);
                this.Client.Game.RankLimit += new FrostbiteClient.LimitHandler(m_prcClient_RankLimit);
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


        #region Rank Limit

        private void m_prcClient_RankLimit(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRankLimit = limit;

            if (this.m_iPreviousSuccessRankLimit == -1)
            {
                this.OnSettingResponse("vars.ranklimit -1", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.ranklimit", (decimal)this.m_iPreviousSuccessRankLimit, true);
            }
        }

        private void chkSettingsNoRankLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.pnlSettingsSetRankLimit.Enabled = !this.chkSettingsNoRankLimit.Checked;
                this.pnlSettingsSetRankLimit.Visible = !this.chkSettingsNoRankLimit.Checked;

                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.ranklimit -1"].IgnoreEvent == false)
                {
                    if (chkSettingsNoRankLimit.Checked == true)
                    {
                        this.WaitForSettingResponse("vars.ranklimit -1", !this.chkSettingsNoRankLimit.Checked);

                        this.Client.Game.SendSetVarsRankLimitPacket(-1);
                        //this.SendCommand("vars.rankLimit", "-1");
                    }
                }
            }
        }

        private void lnkSettingsSetRankLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsRankLimit.Focus();
                this.WaitForSettingResponse("vars.ranklimit", (decimal)this.m_iPreviousSuccessRankLimit);

                this.Client.Game.SendSetVarsRankLimitPacket((int)this.numSettingsRankLimit.Value);
                //this.SendCommand("vars.rankLimit", this.numSettingsRankLimit.Value.ToString());
            }
        }

        #endregion

    }
}
