using System;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.MOH
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsGameplayMoH : uscServerSettingsGameplay
    {

        private int[] ma_iPreviousSuccessSkillLimit;
        private int m_iPreviousSuccessTDMScoreLimit;

        private int[] ma_iPreviousSuccessPreroundLimit;
        private int m_iPreviousSuccessRoundStartTimerPlayersLimit;
        private int m_iPreviousSuccessRoundStartTimerDelay;

        public uscServerSettingsGameplayMoH()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.clanTeams", new AsyncStyleSetting(this.picSettingsClanTeams, this.chkSettingsClanTeams, new Control[] { this.chkSettingsClanTeams }, true));
            this.AsyncSettingControls.Add("vars.noCrosshairs", new AsyncStyleSetting(this.picSettingsNoCrosshairs, this.chkSettingsNoCrosshairs, new Control[] { this.chkSettingsNoCrosshairs }, true));
            this.AsyncSettingControls.Add("vars.realisticHealth", new AsyncStyleSetting(this.picSettingsRealisticHealth, this.chkSettingsRealisticHealth, new Control[] { this.chkSettingsRealisticHealth }, true));
            this.AsyncSettingControls.Add("vars.noUnlocks", new AsyncStyleSetting(this.picSettingsNoUnlocks, this.chkSettingsNoUnlocks, new Control[] { this.chkSettingsNoUnlocks }, true));
            this.AsyncSettingControls.Add("vars.noAmmoPickups", new AsyncStyleSetting(this.picSettingsNoAmmoPickups, this.chkSettingsNoAmmoPickups, new Control[] { this.chkSettingsNoAmmoPickups }, true));

            this.AsyncSettingControls.Add("vars.tdmScoreCounterMaxScore", new AsyncStyleSetting(this.picSettingsTDMScoreCounterLimit, this.numSettingsTDMScoreCounterLimit, new Control[] { this.chkSettingsTDMScoreCounterLimit, this.numSettingsTDMScoreCounterLimit, this.lnkSettingsTDMScoreCounterLimit }, true));
            this.AsyncSettingControls.Add("vars.tdmScoreCounterMaxScore -1", new AsyncStyleSetting(this.picSettingsTDMScoreCounterLimit, this.chkSettingsTDMScoreCounterLimit, new Control[] { this.chkSettingsTDMScoreCounterLimit, this.numSettingsTDMScoreCounterLimit, this.lnkSettingsTDMScoreCounterLimit }, true));

            this.AsyncSettingControls.Add("vars.skillLimit 0 0", new AsyncStyleSetting(this.picSettingsRankLimit, this.chkSettingsNoSkillLimit, new Control[] { this.chkSettingsNoSkillLimit, this.numSettingsSkillLimitLower, this.numSettingsSkillLimitUpper, this.lnkSettingsSkillLimitApply }, true));
            this.AsyncSettingControls.Add("vars.skillLimit", new AsyncStyleSetting(this.picSettingsRankLimit, null, new Control[] { this.chkSettingsNoSkillLimit, this.numSettingsSkillLimitLower, this.numSettingsSkillLimitUpper, this.lnkSettingsSkillLimitApply }, true));

            this.AsyncSettingControls.Add("admin.roundStartTimerEnabled", new AsyncStyleSetting(this.picSettingsRoundStartTimer, this.chkSettingsRoundStartTimer, new Control[] { this.chkSettingsRoundStartTimer }, true));

            // Need -1 disabled setting
            this.AsyncSettingControls.Add("vars.roundStartTimerDelay -1", new AsyncStyleSetting(this.picSettingsRoundStartTimerDelay, this.chkSettingsRoundStartTimerDelay, new Control[] { this.chkSettingsRoundStartTimerDelay, this.numSettingsRoundStartTimerDelay, this.lnkSettingsRoundStartTimerDelay }, true));
            this.AsyncSettingControls.Add("vars.roundStartTimerDelay", new AsyncStyleSetting(this.picSettingsRoundStartTimerDelay, this.numSettingsRoundStartTimerDelay, new Control[] { this.chkSettingsRoundStartTimerDelay, this.numSettingsRoundStartTimerDelay, this.lnkSettingsRoundStartTimerDelay }, true));

            this.AsyncSettingControls.Add("vars.roundStartTimerPlayersLimit", new AsyncStyleSetting(this.picSettingsRoundStartTimerPlayersLimit, this.numSettingsRoundStartTimerPlayersLimit, new Control[] { this.numSettingsRoundStartTimerPlayersLimit, this.lnkSettingsRoundStartTimerPlayersLimit }, true));

            this.AsyncSettingControls.Add("vars.preRoundLimit 1 1", new AsyncStyleSetting(this.picSettingsPreRoundLimits, this.chkSettingsPreroundLimit, new Control[] { this.chkSettingsPreroundLimit, this.numSettingsPreroundLimitLower, this.numSettingsPreroundLimitUpper, this.lnkSettingsPreroundLimitApply }, true));
            this.AsyncSettingControls.Add("vars.preRoundLimit", new AsyncStyleSetting(this.picSettingsPreRoundLimits, null, new Control[] { this.chkSettingsPreroundLimit, this.numSettingsPreroundLimitLower, this.numSettingsPreroundLimitUpper, this.lnkSettingsPreroundLimitApply }, true));

            this.ma_iPreviousSuccessSkillLimit = new int[2] { 0, 5000 };
            this.ma_iPreviousSuccessPreroundLimit = new int[2] { 1, 24 };
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsClanTeams.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsClanTeams");
            this.chkSettingsNoCrosshairs.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsNoCrosshairs");
            this.chkSettingsRealisticHealth.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsRealisticHealth");
            this.chkSettingsNoUnlocks.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsNoUnlocks");
            this.chkSettingsNoAmmoPickups.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsNoAmmoPickups");

            this.chkSettingsNoSkillLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsNoSkillLimit");
            this.lblSettingsSkillLimitLower.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsSkillLimitLower");
            this.lblSettingsSkillLimitUpper.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsSkillLimitUpper");
            this.lnkSettingsSkillLimitApply.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lnkSettingsSkillLimitApply");

            this.chkSettingsTDMScoreCounterLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsTDMScoreCounterLimit");
            this.lnkSettingsTDMScoreCounterLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lnkSettingsTDMScoreCounterLimit");

            this.lblPreroundSettingsTitle.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblPreroundSettingsTitle");

            this.chkSettingsRoundStartTimer.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsRoundStartTimer");

            this.chkSettingsPreroundLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsPreroundLimit");
            this.lblSettingsPreroundLimitLower.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsPreroundLimitLower");
            this.lblSettingsPreroundLimitUpper.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsPreroundLimitUpper");
            this.lnkSettingsPreroundLimitApply.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lnkSettingsPreroundLimitApply");
            this.lblSettingsPreroundLimitExplanation.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsPreroundLimitExplanation");

            this.lblSettingsRoundStartTimerPlayersLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lblSettingsRoundStartTimerPlayersLimit");
            this.lnkSettingsRoundStartTimerPlayersLimit.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lnkSettingsRoundStartTimerPlayersLimit");

            this.chkSettingsRoundStartTimerDelay.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.chkSettingsRoundStartTimerDelay");
            this.lnkSettingsRoundStartTimerDelay.Text = this.Language.GetLocalized("uscServerSettingsMoHGameplay.lnkSettingsRoundStartTimerDelay");

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
                this.Client.Game.ClanTeams += new FrostbiteClient.IsEnabledHandler(Game_ClanTeams);
                this.Client.Game.NoCrosshairs += new FrostbiteClient.IsEnabledHandler(Game_NoCrosshairs);
                this.Client.Game.RealisticHealth += new FrostbiteClient.IsEnabledHandler(Game_RealisticHealth);
                this.Client.Game.NoUnlocks += new FrostbiteClient.IsEnabledHandler(Game_NoUnlocks);
                this.Client.Game.NoAmmoPickups += new FrostbiteClient.IsEnabledHandler(Game_NoAmmoPickups);

                this.Client.Game.TdmScoreCounterMaxScore += new FrostbiteClient.LimitHandler(Game_TdmScoreCounterMaxScore);
                this.Client.Game.SkillLimit += new FrostbiteClient.UpperLowerLimitHandler(Game_SkillLimit);
                this.Client.Game.PreRoundLimit += new FrostbiteClient.UpperLowerLimitHandler(Game_PreRoundLimit);
                this.Client.Game.RoundStartTimer += new FrostbiteClient.IsEnabledHandler(Game_RoundStartTimer);
                this.Client.Game.RoundStartTimerDelay += new FrostbiteClient.LimitHandler(Game_RoundStartTimerDelay);
                this.Client.Game.RoundStartTimerPlayerLimit += new FrostbiteClient.LimitHandler(Game_RoundStartTimerPlayerLimit);

                this.Client.Game.Ranked += new FrostbiteClient.IsEnabledHandler(Game_Ranked);
            });
        }

        private void SetNumberPickerValue(NumericUpDown control, int value)
        {
            this.InvokeIfRequired(() =>
            {

                if (value < control.Minimum)
                {
                    control.Value = control.Minimum;
                }
                else if (value > control.Maximum)
                {
                    control.Value = control.Maximum;
                }
                else
                {
                    control.Value = value;
                }
            });
        }

        #region Clan Teams

        private void chkSettingsClanTeams_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.clanTeams"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.clanTeams", !this.chkSettingsClanTeams.Checked);

                    this.Client.Game.SendSetVarsClanTeamsPacket(this.chkSettingsClanTeams.Checked);
                }
            }
        }

        private void Game_ClanTeams(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.clanTeams", isEnabled, true);
        }

        #endregion

        #region No Crosshairs

        private void chkSettingsNoCrosshairs_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.noCrosshairs"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.noCrosshairs", !this.chkSettingsNoCrosshairs.Checked);

                    this.Client.Game.SendSetVarsNoCrosshairsPacket(this.chkSettingsNoCrosshairs.Checked);
                }
            }
        }

        private void Game_NoCrosshairs(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.noCrosshairs", isEnabled, true);
        }

        #endregion

        #region Realistic Health

        private void chkSettingsRealisticHealth_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.realisticHealth"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.realisticHealth", !this.chkSettingsRealisticHealth.Checked);

                    this.Client.Game.SendSetVarsRealisticHealthPacket(this.chkSettingsRealisticHealth.Checked);
                }
            }
        }

        private void Game_RealisticHealth(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.realisticHealth", isEnabled, true);
        }

        #endregion

        #region No Unlocks

        private void chkSettingsNoUnlocks_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.noUnlocks"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.noUnlocks", !this.chkSettingsNoUnlocks.Checked);

                    this.Client.Game.SendSetVarsNoUnlocksPacket(this.chkSettingsNoUnlocks.Checked);
                }
            }
        }

        private void Game_NoUnlocks(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.noUnlocks", isEnabled, true);
        }

        #endregion

        #region No Ammo Pickups

        private void chkSettingsNoAmmoPickups_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.noAmmoPickups"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.noAmmoPickups", !this.chkSettingsNoAmmoPickups.Checked);

                    this.Client.Game.SendSetVarsNoAmmoPickupsPacket(this.chkSettingsNoAmmoPickups.Checked);
                }
            }
        }

        private void Game_NoAmmoPickups(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.noAmmoPickups", isEnabled, true);
        }

        #endregion

        #region TDM Score Limit

        private void Game_TdmScoreCounterMaxScore(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessTDMScoreLimit = limit;

            if (this.m_iPreviousSuccessTDMScoreLimit == -1)
            {
                this.OnSettingResponse("vars.tdmScoreCounterMaxScore -1", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.tdmScoreCounterMaxScore", (decimal)this.m_iPreviousSuccessTDMScoreLimit, true);
            }
        }

        private void chkSettingsTDMScoreCounterLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsTDMScoreCounterLimit.Enabled = !this.chkSettingsTDMScoreCounterLimit.Checked;
            this.pnlSettingsTDMScoreCounterLimit.Visible = !this.chkSettingsTDMScoreCounterLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.tdmScoreCounterMaxScore -1"].IgnoreEvent == false)
            {
                if (this.chkSettingsTDMScoreCounterLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.tdmScoreCounterMaxScore -1", !this.chkSettingsTDMScoreCounterLimit.Checked);

                    this.Client.Game.SendSetVarsTdmScoreCounterMaxScorePacket(-1);
                }
            }
        }

        private void lnkSettingsTDMScoreCounterLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsTDMScoreCounterLimit.Focus();

            this.WaitForSettingResponse("vars.tdmScoreCounterMaxScore", (decimal)this.m_iPreviousSuccessTDMScoreLimit);

            this.Client.Game.SendSetVarsTdmScoreCounterMaxScorePacket((int)this.numSettingsTDMScoreCounterLimit.Value);
        }

        #endregion

        #region Skill Limit

        private void Game_SkillLimit(FrostbiteClient sender, int upperLimit, int lowerLimit)
        {
            this.ma_iPreviousSuccessSkillLimit[0] = lowerLimit;
            this.ma_iPreviousSuccessSkillLimit[1] = upperLimit;

            if (lowerLimit == 0 && upperLimit == 0)
            {
                this.OnSettingResponse("vars.skillLimit 0 0", true, true);
            }
            else
            {
                this.SetNumberPickerValue(this.numSettingsSkillLimitLower, lowerLimit);
                this.SetNumberPickerValue(this.numSettingsSkillLimitUpper, upperLimit);

                this.OnSettingResponse("vars.skillLimit", null, true);
            }
        }

        private void chkSettingsNoSkillLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsSetSkillLimit.Enabled = !this.chkSettingsNoSkillLimit.Checked;
            this.pnlSettingsSetSkillLimit.Visible = !this.chkSettingsNoSkillLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.skillLimit 0 0"].IgnoreEvent == false)
            {
                if (this.chkSettingsNoSkillLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.skillLimit 0 0", !this.chkSettingsNoSkillLimit.Checked);

                    this.Client.Game.SendSetVarsSkillLimitPacket(1, 1);
                }
            }
        }

        private void lnkSettingsSkillLimitApply_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsSkillLimitLower.Focus();

            this.WaitForSettingResponse("vars.skillLimit", null);

            this.Client.Game.SendSetVarsSkillLimitPacket((int)this.numSettingsSkillLimitUpper.Value, (int)this.numSettingsSkillLimitLower.Value);
        }

        private void numSettingsSkillLimitLower_ValueChanged(object sender, EventArgs e)
        {
            if (this.numSettingsSkillLimitLower.Value > this.numSettingsSkillLimitUpper.Value)
            {
                this.numSettingsSkillLimitUpper.Value = this.numSettingsSkillLimitLower.Value;
            }
        }

        private void numSettingsSkillLimitUpper_ValueChanged(object sender, EventArgs e)
        {
            if (this.numSettingsSkillLimitLower.Value > this.numSettingsSkillLimitUpper.Value)
            {
                this.numSettingsSkillLimitLower.Value = this.numSettingsSkillLimitUpper.Value;
            }
        }

        #endregion

        #region Preround Limit

        private void Game_PreRoundLimit(FrostbiteClient sender, int upperLimit, int lowerLimit)
        {
            this.ma_iPreviousSuccessPreroundLimit[0] = lowerLimit;
            this.ma_iPreviousSuccessPreroundLimit[1] = upperLimit;

            if (lowerLimit == 1 && upperLimit == 1)
            {
                this.OnSettingResponse("vars.preRoundLimit 1 1", true, true);
            }
            else
            {
                this.SetNumberPickerValue(this.numSettingsPreroundLimitLower, lowerLimit);
                this.SetNumberPickerValue(this.numSettingsPreroundLimitUpper, upperLimit);

                this.OnSettingResponse("vars.preRoundLimit", null, true);
            }
        }

        private void chkSettingsPreroundLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsPreroundLimit.Enabled = !this.chkSettingsPreroundLimit.Checked;
            this.pnlSettingsPreroundLimit.Visible = !this.chkSettingsPreroundLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.preRoundLimit 1 1"].IgnoreEvent == false)
            {
                if (this.chkSettingsPreroundLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.preRoundLimit 1 1", !this.chkSettingsPreroundLimit.Checked);

                    this.Client.Game.SendSetVarsPreRoundLimitPacket(1, 1);
                }
            }
        }

        private void lnkSettingsPreroundLimitApply_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsPreroundLimitLower.Focus();

            this.WaitForSettingResponse("vars.preRoundLimit", null);

            this.Client.Game.SendSetVarsPreRoundLimitPacket((int)this.numSettingsPreroundLimitUpper.Value, (int)this.numSettingsPreroundLimitLower.Value);
        }

        #endregion

        #region Round Start Timer Enabled

        void Game_RoundStartTimer(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("admin.roundStartTimerEnabled", isEnabled, true);
        }

        private void chkSettingsRoundStartTimer_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IgnoreEvents == false && this.AsyncSettingControls["admin.roundStartTimerEnabled"].IgnoreEvent == false)
            {
                this.WaitForSettingResponse("admin.roundStartTimerEnabled", !this.chkSettingsRoundStartTimer.Checked);

                this.Client.Game.SendSetAdminRoundStartTimerEnabledPacket(this.chkSettingsRoundStartTimer.Checked);
            }
        }

        #endregion

        #region Preround Limit Delay

        void Game_RoundStartTimerDelay(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessRoundStartTimerDelay = limit;

            if (this.m_iPreviousSuccessRoundStartTimerDelay == -1)
            {
                this.OnSettingResponse("vars.roundStartTimerDelay -1", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.roundStartTimerDelay", (decimal)this.m_iPreviousSuccessRoundStartTimerDelay, true);
            }
        }

        private void chkSettingsRoundStartTimerDelay_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsRoundStartTimerDelay.Enabled = !this.chkSettingsRoundStartTimerDelay.Checked;
            this.pnlSettingsRoundStartTimerDelay.Visible = !this.chkSettingsRoundStartTimerDelay.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.roundStartTimerDelay -1"].IgnoreEvent == false)
            {
                if (this.chkSettingsRoundStartTimerDelay.Checked == true)
                {
                    this.WaitForSettingResponse("vars.roundStartTimerDelay -1", !this.chkSettingsRoundStartTimerDelay.Checked);

                    this.Client.Game.SendSetVarsRoundStartTimerDelayPacket(-1);
                }
            }
        }

        private void lnkSettingsRoundStartTimerDelay_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsRoundStartTimerDelay.Focus();

            this.WaitForSettingResponse("vars.roundStartTimerDelay", (decimal)this.m_iPreviousSuccessRoundStartTimerDelay);

            this.Client.Game.SendSetVarsRoundStartTimerDelayPacket((int)this.numSettingsRoundStartTimerDelay.Value);
        }

        #endregion

        #region Preround Player Limit

        private void Game_RoundStartTimerPlayerLimit(FrostbiteClient sender, int limit)
        {
            this.OnSettingResponse("vars.roundStartTimerPlayersLimit", (decimal)limit, true);
            this.m_iPreviousSuccessRoundStartTimerPlayersLimit = limit;
        }

        private void lnkSettingsRoundStartTimerPlayersLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsRoundStartTimerPlayersLimit.Focus();
                this.WaitForSettingResponse("vars.roundStartTimerPlayersLimit", (decimal)this.m_iPreviousSuccessRoundStartTimerPlayersLimit);

                this.Client.Game.SendSetVarsRoundStartTimerPlayersLimitPacket((int)this.numSettingsRoundStartTimerPlayersLimit.Value);
            }
        }

        // Kinda belongs here..
        private void Game_Ranked(FrostbiteClient sender, bool isEnabled)
        {
            this.InvokeIfRequired(() =>
            {
                this.lblSettingsPreroundLimitExplanation.Visible = isEnabled;

                if (isEnabled == true)
                {
                    this.numSettingsPreroundLimitLower.Minimum = 2;
                    this.numSettingsPreroundLimitUpper.Minimum = 4;
                }
                else
                {
                    // They should use the checkbox if they want 1-1.
                    this.numSettingsPreroundLimitLower.Minimum = 1;
                    this.numSettingsPreroundLimitUpper.Minimum = 2;
                }
            });
        }

        private void numSettingsPreroundLimitLower_ValueChanged(object sender, EventArgs e)
        {
            if (this.numSettingsPreroundLimitLower.Value > this.numSettingsPreroundLimitUpper.Value)
            {
                this.numSettingsPreroundLimitUpper.Value = this.numSettingsPreroundLimitLower.Value;
            }
        }

        private void numSettingsPreroundLimitUpper_ValueChanged(object sender, EventArgs e)
        {
            if (this.numSettingsPreroundLimitLower.Value > this.numSettingsPreroundLimitUpper.Value)
            {
                this.numSettingsPreroundLimitLower.Value = this.numSettingsPreroundLimitUpper.Value;
            }
        }

        #endregion

    }
}
