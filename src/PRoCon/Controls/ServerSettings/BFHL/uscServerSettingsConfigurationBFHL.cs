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
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.BFHL
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsConfigurationBFHL : uscServerSettings
    {

        private int m_iPreviousSuccessPlayerLimit;
        private int m_iPreviousSuccessMaxSpectators;
        private int m_iPreviousSuccessIdleTimeoutLimit;
        private int m_iPreviousSuccessIdleBanRoundsLimit;

        private string m_strPreviousSuccessAdminPassword;
        private string m_strPreviousSuccessGamePassword;

        public uscServerSettingsConfigurationBFHL()
        {
            InitializeComponent();

            //this.AsyncSettingControls.Add("vars.punkbuster", new AsyncStyleSetting(this.picSettingsPunkbuster, this.chkSettingsPunkbuster, new Control[] { this.chkSettingsPunkbuster }, false));
            //this.AsyncSettingControls.Add("vars.ranked", new AsyncStyleSetting(this.picSettingsRanked, this.chkSettingsRanked, new Control[] { this.chkSettingsRanked }, false));

            this.AsyncSettingControls.Add("vars.playerlimit", new AsyncStyleSetting(this.picSettingsPlayerLimit, this.numSettingsPlayerLimit, new Control[] { this.numSettingsPlayerLimit, this.lnkSettingsSetPlayerLimit }, true));

            this.AsyncSettingControls.Add("vars.maxSpectators", new AsyncStyleSetting(this.picSettingsMaxSpectators, this.numSettingsMaxSpectators, new Control[] { this.numSettingsMaxSpectators, this.lnkSettingsSetMaxSpectators }, true));

            this.AsyncSettingControls.Add("vars.idletimeout 86400", new AsyncStyleSetting(this.picSettingsIdleKickLimit, this.chkSettingsNoIdleKickLimit, new Control[] { this.chkSettingsNoIdleKickLimit }, false));
            this.AsyncSettingControls.Add("vars.idletimeout", new AsyncStyleSetting(this.picSettingsIdleKickLimit, this.numSettingsIdleKickLimit, new Control[] { this.numSettingsIdleKickLimit, this.lnkSettingsSetidleKickLimit }, false));

            this.AsyncSettingControls.Add("vars.idlebanrounds 0", new AsyncStyleSetting(this.picSettingsNoIdleBanRoundsLimit, this.chkSettingsNoIdleBanRoundsLimit, new Control[] { this.chkSettingsNoIdleBanRoundsLimit }, true));
            this.AsyncSettingControls.Add("vars.idlebanrounds", new AsyncStyleSetting(this.picSettingsNoIdleBanRoundsLimit, this.numSettingsIdleBanRoundsLimit, new Control[] { this.numSettingsIdleBanRoundsLimit, this.lnkSettingsSetIdleBanRoundsLimit }, true));

            this.AsyncSettingControls.Add("vars.gamepassword", new AsyncStyleSetting(this.picSettingsGamePassword, this.txtSettingsGamePassword, new Control[] { this.lblSettingsGamePassword, this.txtSettingsGamePassword, this.lnkSettingsSetGamePassword }, true));
            this.AsyncSettingControls.Add("vars.adminpassword", new AsyncStyleSetting(this.picSettingsAdminPassword, this.txtSettingsAdminPassword, new Control[] { this.lblSettingsAdminPassword, this.txtSettingsAdminPassword, this.lnkSettingsSetAdminPassword }, true));

            this.AsyncSettingControls.Add("fairFight.isActive", new AsyncStyleSetting(this.picSettingsFairFight, this.chkSettingsFairFight, new Control[] { this.chkSettingsFairFight }, true));

            this.AsyncSettingControls.Add("vars.hacker", new AsyncStyleSetting(this.picSettingsCommander, this.chkSettingsCommander, new Control[] { this.chkSettingsCommander }, false));
            this.AsyncSettingControls.Add("vars.alwaysAllowSpectators", new AsyncStyleSetting(this.picSettingsAlwaysAllowSpectators, this.chkSettingsAlwaysAllowSpectators, new Control[] { this.chkSettingsAlwaysAllowSpectators }, true));

            this.AsyncSettingControls.Add("reservedslotslist.aggressivejoin", new AsyncStyleSetting(this.picSettingsAggressiveJoin, this.chkSettingsAggressiveJoin, new Control[] { this.chkSettingsAggressiveJoin }, true));

            this.AsyncSettingControls.Add("vars.teamFactionOverride 1", new AsyncStyleSetting(this.picSettingsTeam1FactionOverride, this.cboSettingsTeam1FactionOverride, new Control[] { this.cboSettingsTeam1FactionOverride }, true));
            this.AsyncSettingControls.Add("vars.teamFactionOverride 2", new AsyncStyleSetting(this.picSettingsTeam2FactionOverride, this.cboSettingsTeam2FactionOverride, new Control[] { this.cboSettingsTeam2FactionOverride }, true));
            this.AsyncSettingControls.Add("vars.teamFactionOverride 3", new AsyncStyleSetting(this.picSettingsTeam3FactionOverride, this.cboSettingsTeam3FactionOverride, new Control[] { this.cboSettingsTeam3FactionOverride }, true));
            this.AsyncSettingControls.Add("vars.teamFactionOverride 4", new AsyncStyleSetting(this.picSettingsTeam4FactionOverride, this.cboSettingsTeam4FactionOverride, new Control[] { this.cboSettingsTeam4FactionOverride }, true));

            this.m_iPreviousSuccessPlayerLimit = 50;
            this.m_iPreviousSuccessIdleTimeoutLimit = 0;
            this.m_iPreviousSuccessIdleBanRoundsLimit = 0;
            this.m_strPreviousSuccessAdminPassword = String.Empty;
            this.m_strPreviousSuccessGamePassword = String.Empty;
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsPunkbuster.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsPunkbuster");
            this.lblSettingsServerType.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsServerType");
            this.lblSettingsMpExperience.Text = this.Language.GetDefaultLocalized("MP Experience", "uscServerSettingsPanel.lblSettingsMpExperience");
            this.chkSettingsCommander.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsCommander");
            this.chkSettingsAlwaysAllowSpectators.Text = this.Language.GetDefaultLocalized("Public spectators", "uscServerSettingsPanel.chkSettingsAlwaysAllowSpectators");

            this.chkSettingsFairFight.Text = this.Language.GetDefaultLocalized("use FairFight", "uscServerSettingsPanel.chkSettingsFairFight");

            this.lblSettingsMaxSpectators.Text = this.Language.GetDefaultLocalized("Spectator limit", "uscServerSettingsPanel.lblSettingsMaxSpectator");
            this.lnkSettingsSetMaxSpectators.Text = this.Language.GetDefaultLocalized("Apply", "uscServerSettingsPanel.lnkSettingsSetMaxSpectators");

            this.lblSettingsPlayerLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsPlayerLimit");
            this.lnkSettingsSetPlayerLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetPlayerLimit");

            this.lblSettingsEffectivePlayerLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsEffectivePlayerLimit");

            this.lblSettingsGamePassword.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsGamePassword");
            this.lnkSettingsSetGamePassword.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetGamePassword");
            this.lblSettingsAdminPassword.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsAdminPassword");
            this.lnkSettingsSetAdminPassword.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetAdminPassword");

            this.chkSettingsNoIdleKickLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsNoIdleKickLimit");
            this.chkSettingsNoIdleBanRoundsLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsNoIdleBanRoundsLimit");
            this.lnkSettingsSetidleKickLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetidleKickLimit");
            this.lnkSettingsSetIdleBanRoundsLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetIdleBanRoundsLimit");

            this.chkSettingsAggressiveJoin.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsAggressiveJoin");

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsConfiguration");

            this.lblSettingsTeam1FactionOverride.Text = this.Language.GetDefaultLocalized("Team 1 Faction Override", "uscServerSettingsPanel.lblSettingsTeam1FactionOverride");
            this.lblSettingsTeam2FactionOverride.Text = this.Language.GetDefaultLocalized("Team 2 Faction Override", "uscServerSettingsPanel.lblSettingsTeam2FactionOverride");
            this.lblSettingsTeam3FactionOverride.Text = this.Language.GetDefaultLocalized("Team 3 Faction Override", "uscServerSettingsPanel.lblSettingsTeam3FactionOverride");
            this.lblSettingsTeam4FactionOverride.Text = this.Language.GetDefaultLocalized("Team 4 Faction Override", "uscServerSettingsPanel.lblSettingsTeam4FactionOverride");
            this.lnkSettingsTeam1FactionOverride.Text = this.lnkSettingsTeam2FactionOverride.Text = this.lnkSettingsTeam3FactionOverride.Text = this.lnkSettingsTeam4FactionOverride.Text = this.Language.GetDefaultLocalized("Apply", "uscServerSettingsPanel.lnkSettingsTeamFactionOverride");
            // 0 = COPS, 1 = CRIM

            this.cboSettingsTeam1FactionOverride.Items.Clear();
            this.cboSettingsTeam1FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Cops", "uscServerSettingsPanel.TeamFactions.COPS"));
            this.cboSettingsTeam1FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Criminals", "uscServerSettingsPanel.TeamFactions.CRIM"));
            this.cboSettingsTeam1FactionOverride.SelectedIndex = 0;

            this.cboSettingsTeam2FactionOverride.Items.Clear();
            this.cboSettingsTeam2FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Cops", "uscServerSettingsPanel.TeamFactions.COPS"));
            this.cboSettingsTeam2FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Criminals", "uscServerSettingsPanel.TeamFactions.CRIM"));
            this.cboSettingsTeam2FactionOverride.SelectedIndex = 0;

            this.cboSettingsTeam3FactionOverride.Items.Clear();
            this.cboSettingsTeam3FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Cops", "uscServerSettingsPanel.TeamFactions.COPS"));
            this.cboSettingsTeam3FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Criminals", "uscServerSettingsPanel.TeamFactions.CRIM"));
            this.cboSettingsTeam3FactionOverride.SelectedIndex = 0;

            this.cboSettingsTeam4FactionOverride.Items.Clear();
            this.cboSettingsTeam4FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Cops", "uscServerSettingsPanel.TeamFactions.COPS"));
            this.cboSettingsTeam4FactionOverride.Items.Add(this.Language.GetDefaultLocalized("Criminals", "uscServerSettingsPanel.TeamFactions.CRIM"));
            this.cboSettingsTeam4FactionOverride.SelectedIndex = 0;

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
                this.Client.Game.Punkbuster += new FrostbiteClient.IsEnabledHandler(m_prcClient_Punkbuster);

                this.Client.Game.FairFight += new FrostbiteClient.IsEnabledHandler(Game_FairFight);
                this.Client.Game.IsCommander += new FrostbiteClient.IsEnabledHandler(Game_IsCommander);
                this.Client.Game.ServerType += new FrostbiteClient.VarsStringHandler(Game_ServerType);

                this.Client.Game.GamePassword += new FrostbiteClient.PasswordHandler(m_prcClient_GamePassword);
                this.Client.Game.AdminPassword += new FrostbiteClient.PasswordHandler(m_prcClient_AdminPassword);

                this.Client.Game.PlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_PlayerLimit);
                this.Client.Game.MaxPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_MaxPlayerLimit);
                this.Client.Game.CurrentPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_CurrentPlayerLimit);

                this.Client.Game.MaxSpectators += new FrostbiteClient.LimitHandler(Game_MaxSpectators);
                this.Client.Game.AlwaysAllowSpectators += new FrostbiteClient.IsEnabledHandler(Game_AlwaysAllowSpectators);

                this.Client.Game.IdleTimeout += new FrostbiteClient.LimitHandler(m_prcClient_IdleTimeout);
                this.Client.Game.IdleBanRounds += new FrostbiteClient.LimitHandler(m_prcClient_IdleBanRounds);

                this.Client.Game.ReservedSlotsListAggressiveJoin += new FrostbiteClient.IsEnabledHandler(Game_ReservedSlotsAggressiveJoin);

                this.Client.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);

                this.Client.Game.BF4preset += new FrostbiteClient.BF4presetHandler(Tab_BF4preset);

                this.Client.Game.MpExperience += new FrostbiteClient.MpExperienceHandler(Game_MpExperience);

                this.Client.Game.TeamFactionOverride += new FrostbiteClient.TeamFactionOverrideHandler(Game_TeamFactionOverride);
            });
        }

        private void m_prcClient_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.InvokeIfRequired(() =>
            {
                if (csiServerInfo.MaxPlayerCount > 0 && csiServerInfo.MaxPlayerCount <= this.numSettingsPlayerLimit.Maximum)
                {
                    //this.numSettingsPlayerLimit.Value = (decimal)csiServerInfo.MaxPlayerCount;
                    this.numSettingsEffectivePlayerLimit.Value = (decimal)csiServerInfo.MaxPlayerCount;
                }

                this.chkSettingsPunkbuster.Checked = csiServerInfo.PunkBuster;
                //this.chkSettingsRanked.Checked = csiServerInfo.Ranked;
            });
        }

        private void Tab_BF4preset(FrostbiteClient sender, string mode, bool locked)
        {
            this.InvokeIfRequired(() =>
            {
                this.numSettingsIdleKickLimit.Enabled = !locked;
                this.lnkSettingsSetidleKickLimit.Enabled = !locked;
                this.chkSettingsNoIdleKickLimit.Enabled = !locked;
                this.chkSettingsCommander.Enabled = !locked;
            });
        }

        #region Server Type

        void Game_ServerType(FrostbiteClient sender, string value)
        {
            this.InvokeIfRequired(() =>
            {
                // This value is read only.
                this.txtSettingsServerType.Text = value;
            });
        }

        #endregion

        #region MpExperience

        void Game_MpExperience(FrostbiteClient sender, string mpExperience)
        {
            this.InvokeIfRequired(() =>
            {
                this.txtSettingsMpExperience.Text = mpExperience;
            });
        }

        #endregion

        #region Passwords

        private void m_prcClient_GamePassword(FrostbiteClient sender, string password)
        {
            this.OnSettingResponse("vars.gamepassword", password, true);
            this.m_strPreviousSuccessGamePassword = password;
        }

        private void lnkSettingsSetGamePassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.txtSettingsGamePassword.Focus();
                this.WaitForSettingResponse("vars.gamepassword", this.m_strPreviousSuccessGamePassword);

                this.Client.Game.SendSetVarsGamePasswordPacket(this.txtSettingsGamePassword.Text);
            }
        }

        private void m_prcClient_AdminPassword(FrostbiteClient sender, string password)
        {
            this.OnSettingResponse("vars.adminpassword", password, true);
            this.m_strPreviousSuccessAdminPassword = password;
        }

        private void lnkSettingsSetAdminPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.txtSettingsAdminPassword.Focus();
                this.WaitForSettingResponse("vars.adminpassword", this.m_strPreviousSuccessAdminPassword);

                this.Client.Game.SendSetVarsAdminPasswordPacket(this.txtSettingsAdminPassword.Text);
            }
        }

        #endregion

        #region Punkbuster

        private void m_prcClient_Punkbuster(FrostbiteClient sender, bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkSettingsPunkbuster.Checked = isEnabled; });
            //this.OnSettingResponse("vars.punkbuster", isEnabled, true);
        }

        /*
        private void chkSettingsPunkbuster_CheckedChanged(object sender, EventArgs e) {

            if (this.Client != null && this.Client.Game != null) {

                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.punkbuster"].IgnoreEvent == false) {
                    this.WaitForSettingResponse("vars.punkbuster", !this.chkSettingsPunkbuster.Checked);

                    this.Client.Game.SendSetVarsPunkBusterPacket(this.chkSettingsPunkbuster.Checked);
                    //this.SendCommand("vars.punkBuster", Packet.bltos(this.chkSettingsPunkbuster.Checked));
                }
            }
        }
        */

        #endregion

        #region FairFight

        private void Game_FairFight(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("fairFight.isActive", isEnabled, true);
        }

        private void chkSettingsFairFight_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["fairFight.isActive"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("fairFight.isActive", this.chkSettingsFairFight.Checked);

                    this.Client.Game.SendSetVarsFairFightPacket(this.chkSettingsFairFight.Checked);
                    this.Client.Game.SendGetVarsFairFightPacket();
                }
            }
        }

        #endregion

        #region Player Limit

        private void m_prcClient_CurrentPlayerLimit(FrostbiteClient sender, int limit)
        {
            this.InvokeIfRequired(() =>
            {
                if (limit > 0 && limit <= this.numSettingsPlayerLimit.Maximum)
                {
                    this.numSettingsPlayerLimit.Value = (decimal)limit;
                }
            });
        }

        private void m_prcClient_MaxPlayerLimit(FrostbiteClient sender, int limit)
        {
            this.InvokeIfRequired(() => { this.numSettingsPlayerLimit.Maximum = (decimal)limit; });
        }

        private void m_prcClient_PlayerLimit(FrostbiteClient sender, int limit)
        {
            this.OnSettingResponse("vars.playerlimit", (decimal)limit, true);
            this.m_iPreviousSuccessPlayerLimit = limit;
        }

        private void lnkSettingsSetPlayerLimt_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsPlayerLimit.Focus();
                this.WaitForSettingResponse("vars.playerlimit", (decimal)this.m_iPreviousSuccessPlayerLimit);

                this.Client.Game.SendSetVarsPlayerLimitPacket((int)this.numSettingsPlayerLimit.Value);
                //this.SendCommand("vars.playerLimit", this.numSettingsPlayerLimit.Value.ToString());
            }
        }

        #endregion

        #region Spectator Limit (maxSpectators)

        private void Game_MaxSpectators(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessMaxSpectators = limit;

            this.OnSettingResponse("vars.maxSpectators", (decimal)limit, true);
        }

        private void lnkSettingsSetMaxSpectators_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsMaxSpectators.Focus();
                this.WaitForSettingResponse("vars.maxSpectators", (decimal)this.m_iPreviousSuccessMaxSpectators);

                this.Client.Game.SendSetVarsMaxSpectatorsPacket((int)this.numSettingsMaxSpectators.Value);
                this.Client.Game.SendSetVarsMaxSpectatorsPacket((int)this.numSettingsMaxSpectators.Value);
                // doubled because of Blace backend roundtrip in BF4 (PK)
            }
        }

        #endregion

        #region Idle Timeout

        private void m_prcClient_IdleTimeout(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessIdleTimeoutLimit = limit;

            if (this.m_iPreviousSuccessIdleTimeoutLimit == 0 || this.m_iPreviousSuccessIdleTimeoutLimit == 86400)
            {
                this.OnSettingResponse("vars.idletimeout 86400", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.idletimeout", (decimal)this.m_iPreviousSuccessIdleTimeoutLimit, true);
                this.OnSettingResponse("vars.idletimeout 86400", false, true);
            }
        }

        private void chkSettingsNoIdleKickLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsSetidleKickLimit.Enabled = !this.chkSettingsNoIdleKickLimit.Checked;
            // for BF4 86400s are max and this is not disabled, so it has to be visulized in a propper way
            if (this.chkSettingsNoIdleKickLimit.Checked == true)
            {
                this.numSettingsIdleKickLimit.Value = 86400;
            }
            //this.pnlSettingsSetidleKickLimit.Visible = !this.chkSettingsNoIdleKickLimit.Checked;

            this.chkSettingsNoIdleBanRoundsLimit.Enabled = !this.chkSettingsNoIdleKickLimit.Checked;
            this.chkSettingsNoIdleBanRoundsLimit.Visible = !this.chkSettingsNoIdleKickLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.idletimeout 86400"].IgnoreEvent == false)
            {
                if (this.chkSettingsNoIdleKickLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.idletimeout 86400", !this.chkSettingsNoIdleKickLimit.Checked);

                    this.Client.Game.SendSetVarsIdleTimeoutPacket(86400);
                    //this.SendCommand("vars.idleTimeout", "0");
                }
                if (this.chkSettingsNoIdleKickLimit.Checked == false)
                {
                    this.WaitForSettingResponse("vars.idletimeout 300", this.chkSettingsNoIdleKickLimit.Checked);

                    this.Client.Game.SendSetVarsIdleTimeoutPacket(300);
                    this.numSettingsIdleKickLimit.Value = 300;
                }
            }
        }

        private void lnkSettingsSetidleKickLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsIdleKickLimit.Focus();
            this.WaitForSettingResponse("vars.idletimeout", (decimal)this.m_iPreviousSuccessIdleTimeoutLimit);

            this.Client.Game.SendSetVarsIdleTimeoutPacket((int)this.numSettingsIdleKickLimit.Value);
            //this.SendCommand("vars.idleTimeout", this.numSettingsIdleKickLimit.Value.ToString());
        }

        #endregion

        #region idleBanRounds

        private void m_prcClient_IdleBanRounds(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessIdleBanRoundsLimit = limit;

            if (this.m_iPreviousSuccessIdleBanRoundsLimit == 0)
            {
                this.OnSettingResponse("vars.idlebanrounds 0", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.idlebanrounds", (decimal)this.m_iPreviousSuccessIdleBanRoundsLimit, true);
                this.OnSettingResponse("vars.idlebanrounds 0", false, true);
            }
        }

        private void chkSettingsNoIdleBanRoundsLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.lnkSettingsSetIdleBanRoundsLimit.Enabled = !this.chkSettingsNoIdleBanRoundsLimit.Checked;
            this.lnkSettingsSetIdleBanRoundsLimit.Visible = !this.chkSettingsNoIdleBanRoundsLimit.Checked;

            this.numSettingsIdleBanRoundsLimit.Enabled = !this.chkSettingsNoIdleBanRoundsLimit.Checked;
            this.numSettingsIdleBanRoundsLimit.Visible = !this.chkSettingsNoIdleBanRoundsLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.idlebanrounds 0"].IgnoreEvent == false)
            {
                if (this.chkSettingsNoIdleBanRoundsLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.idlebanrounds 0", !this.chkSettingsNoIdleBanRoundsLimit.Checked);

                    this.Client.Game.SendSetVarsIdleBanRoundsPacket(0);
                }
                if (this.chkSettingsNoIdleBanRoundsLimit.Checked == false)
                {
                    this.WaitForSettingResponse("vars.idlebanrounds 2", this.chkSettingsNoIdleBanRoundsLimit.Checked);

                    this.Client.Game.SendSetVarsIdleBanRoundsPacket(2);
                    this.numSettingsIdleBanRoundsLimit.Value = 2;
                }
            }
        }

        private void lnkSettingsSetIdleBanRoundsLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.numSettingsIdleBanRoundsLimit.Focus();
            this.WaitForSettingResponse("vars.idlebanrounds", (decimal)this.m_iPreviousSuccessIdleBanRoundsLimit);

            this.Client.Game.SendSetVarsIdleBanRoundsPacket((int)this.numSettingsIdleBanRoundsLimit.Value);
        }
        #endregion

        #region ReservedSlotsAggressiveJoin

        private void Game_ReservedSlotsAggressiveJoin(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("reservedslotslist.aggressivejoin", isEnabled, true);
        }

        private void chkSettingsAggressiveJoin_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["reservedslotslist.aggressivejoin"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("reservedslotslist.aggressivejoin", !this.chkSettingsAggressiveJoin.Checked);

                    this.Client.Game.SendSetReservedSlotsListAggressiveJoinPacket(this.chkSettingsAggressiveJoin.Checked);
                }
            }
        }
        #endregion

        #region Commander

        private void Game_IsCommander(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hacker", isEnabled, true);
        }
        private void chkSettingsCommander_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hacker"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hacker", this.chkSettingsCommander.Checked);

                    this.Client.Game.SendSetVarsCommander(this.chkSettingsCommander.Checked);
                }
            }
        }

        #endregion

        #region AlwaysAllowSpectators

        private void Game_AlwaysAllowSpectators(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.alwaysAllowSpectators", isEnabled, true);
        }
        private void chkSettingsAlwaysAllowSpectators_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.alwaysAllowSpectators"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.alwaysAllowSpectators", this.chkSettingsAlwaysAllowSpectators.Checked);

                    this.Client.Game.SendSetVarsAlwaysAllowSpectators(this.chkSettingsAlwaysAllowSpectators.Checked);
                }
            }
        }

        #endregion

        #region TeamFactionOverride

        private int m_strPreviousSuccessTeam1FactionOverride;
        private int m_strPreviousSuccessTeam2FactionOverride;
        private int m_strPreviousSuccessTeam3FactionOverride;
        private int m_strPreviousSuccessTeam4FactionOverride;

        private void Game_TeamFactionOverride(FrostbiteClient sender, int teamId, int faction)
        {
            this.OnSettingResponse("vars.teamFactionOverride " + teamId, faction, true);
            switch (teamId)
            {
                case 1:
                    this.m_strPreviousSuccessTeam1FactionOverride = faction;
                    this.cboSettingsTeam1FactionOverride.SelectedIndex = faction;
                    break;
                case 2:
                    this.m_strPreviousSuccessTeam2FactionOverride = faction;
                    this.cboSettingsTeam2FactionOverride.SelectedIndex = faction;
                    break;
                case 3:
                    this.m_strPreviousSuccessTeam3FactionOverride = faction;
                    this.cboSettingsTeam3FactionOverride.SelectedIndex = faction;
                    break;
                case 4:
                    this.m_strPreviousSuccessTeam4FactionOverride = faction;
                    this.cboSettingsTeam4FactionOverride.SelectedIndex = faction;
                    break;
            }
        }

        private void lnkSettingsTeam1FactionOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.cboSettingsTeam1FactionOverride.Focus();
                this.WaitForSettingResponse("vars.teamFactionOverride 1", this.m_strPreviousSuccessTeam1FactionOverride);

                this.Client.Game.SendSetVarsTeamFactionOverridePacket(1, this.cboSettingsTeam1FactionOverride.SelectedIndex);
            }
        }

        private void lnkSettingsTeam2FactionOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.cboSettingsTeam2FactionOverride.Focus();
                this.WaitForSettingResponse("vars.teamFactionOverride 2", this.m_strPreviousSuccessTeam2FactionOverride);

                this.Client.Game.SendSetVarsTeamFactionOverridePacket(2, this.cboSettingsTeam2FactionOverride.SelectedIndex);
            }
        }

        private void lnkSettingsTeam3FactionOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.cboSettingsTeam3FactionOverride.Focus();
                this.WaitForSettingResponse("vars.teamFactionOverride 3", this.m_strPreviousSuccessTeam3FactionOverride);

                this.Client.Game.SendSetVarsTeamFactionOverridePacket(3, this.cboSettingsTeam3FactionOverride.SelectedIndex);
            }
        }

        private void lnkSettingsTeam4FactionOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.cboSettingsTeam4FactionOverride.Focus();
                this.WaitForSettingResponse("vars.teamFactionOverride 4", this.m_strPreviousSuccessTeam4FactionOverride);

                this.Client.Game.SendSetVarsTeamFactionOverridePacket(4, this.cboSettingsTeam4FactionOverride.SelectedIndex);
            }
        }

        #endregion

    }
}
