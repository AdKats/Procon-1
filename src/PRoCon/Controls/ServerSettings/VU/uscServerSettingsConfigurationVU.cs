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

namespace PRoCon.Controls.ServerSettings.VU
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsConfigurationVU : uscServerSettings
    {

        private int m_iPreviousSuccessPlayerLimit;
        private int m_iPreviousSuccessIdleTimeoutLimit;
        private int m_iPreviousSuccessIdleBanRoundsLimit;

        private string m_strPreviousSuccessAdminPassword;
        private string m_strPreviousSuccessGamePassword;

        public uscServerSettingsConfigurationVU()
        {
            InitializeComponent();

            //this.AsyncSettingControls.Add("vars.punkbuster", new AsyncStyleSetting(this.picSettingsPunkbuster, this.chkSettingsPunkbuster, new Control[] { this.chkSettingsPunkbuster }, false));
            //this.AsyncSettingControls.Add("vars.ranked", new AsyncStyleSetting(this.picSettingsRanked, this.chkSettingsRanked, new Control[] { this.chkSettingsRanked }, false));

            this.AsyncSettingControls.Add("vars.playerlimit", new AsyncStyleSetting(this.picSettingsPlayerLimit, this.numSettingsPlayerLimit, new Control[] { this.numSettingsPlayerLimit, this.lnkSettingsSetPlayerLimit }, true));

            this.AsyncSettingControls.Add("vars.idletimeout 0", new AsyncStyleSetting(this.picSettingsIdleKickLimit, this.chkSettingsNoIdleKickLimit, new Control[] { this.chkSettingsNoIdleKickLimit }, true));
            this.AsyncSettingControls.Add("vars.idletimeout", new AsyncStyleSetting(this.picSettingsIdleKickLimit, this.numSettingsIdleKickLimit, new Control[] { this.numSettingsIdleKickLimit, this.lnkSettingsSetidleKickLimit }, true));

            this.AsyncSettingControls.Add("vars.idlebanrounds 0", new AsyncStyleSetting(this.picSettingsNoIdleBanRoundsLimit, this.chkSettingsNoIdleBanRoundsLimit, new Control[] { this.chkSettingsNoIdleBanRoundsLimit }, true));
            this.AsyncSettingControls.Add("vars.idlebanrounds", new AsyncStyleSetting(this.picSettingsNoIdleBanRoundsLimit, this.numSettingsIdleBanRoundsLimit, new Control[] { this.numSettingsIdleBanRoundsLimit, this.lnkSettingsSetIdleBanRoundsLimit }, true));

            this.AsyncSettingControls.Add("vars.gamepassword", new AsyncStyleSetting(this.picSettingsGamePassword, this.txtSettingsGamePassword, new Control[] { this.lblSettingsGamePassword, this.txtSettingsGamePassword, this.lnkSettingsSetGamePassword }, true));
            this.AsyncSettingControls.Add("vars.adminpassword", new AsyncStyleSetting(this.picSettingsAdminPassword, this.txtSettingsAdminPassword, new Control[] { this.lblSettingsAdminPassword, this.txtSettingsAdminPassword, this.lnkSettingsSetAdminPassword }, true));

            if (Program.ProconApplication.OptionsSettings.ShowDICESpecialOptions == true)
            {
                this.chkSettingsPremiumStatus.Enabled = Program.ProconApplication.OptionsSettings.ShowDICESpecialOptions;
                this.AsyncSettingControls.Add("vars.premiumstatus", new AsyncStyleSetting(this.picSettingsPremiumStatus, this.chkSettingsPremiumStatus, new Control[] { this.chkSettingsPremiumStatus }, true));
            }

            this.AsyncSettingControls.Add("reservedslotslist.aggressivejoin", new AsyncStyleSetting(this.picSettingsAggressiveJoin, this.chkSettingsAggressiveJoin, new Control[] { this.chkSettingsAggressiveJoin }, true));

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
            this.chkSettingsRanked.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsRanked");

            this.chkSettingsPremiumStatus.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsPremiumStatus");

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
                this.Client.Game.Ranked += new FrostbiteClient.IsEnabledHandler(m_prcClient_Ranked);

                this.Client.Game.PremiumStatus += new FrostbiteClient.IsEnabledHandler(Game_PremiumStatus);

                this.Client.Game.GamePassword += new FrostbiteClient.PasswordHandler(m_prcClient_GamePassword);
                this.Client.Game.AdminPassword += new FrostbiteClient.PasswordHandler(m_prcClient_AdminPassword);

                this.Client.Game.PlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_PlayerLimit);
                this.Client.Game.MaxPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_MaxPlayerLimit);
                this.Client.Game.CurrentPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_CurrentPlayerLimit);
                this.Client.Game.IdleTimeout += new FrostbiteClient.LimitHandler(m_prcClient_IdleTimeout);
                this.Client.Game.IdleBanRounds += new FrostbiteClient.LimitHandler(m_prcClient_IdleBanRounds);

                this.Client.Game.ReservedSlotsListAggressiveJoin += new FrostbiteClient.IsEnabledHandler(Game_ReservedSlotsAggressiveJoin);

                this.Client.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);
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
                this.chkSettingsRanked.Checked = csiServerInfo.Ranked;
            });
        }

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

        #region Ranked

        private void m_prcClient_Ranked(FrostbiteClient sender, bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkSettingsRanked.Checked = isEnabled; });
            //this.OnSettingResponse("vars.ranked", isEnabled, true);
        }

        /*
        private void chkSettingsRanked_CheckedChanged(object sender, EventArgs e) {
            if (this.Client != null && this.Client.Game != null) {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.ranked"].IgnoreEvent == false) {
                    this.WaitForSettingResponse("vars.ranked", !this.chkSettingsRanked.Checked);

                    this.Client.Game.SendSetVarsRankedPacket(this.chkSettingsPunkbuster.Checked);
                    //this.SendCommand("vars.ranked", Packet.bltos(this.chkSettingsRanked.Checked));
                }
            }
        }
        */

        #endregion

        #region PremiumStatus

        private void Game_PremiumStatus(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.premiumstatus", isEnabled, true);
        }

        private void chkSettingsPremiumStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.premiumstatus"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.premiumstatus", !this.chkSettingsPremiumStatus.Checked);

                    this.Client.Game.SendSetVarsPremiumStatusPacket(this.chkSettingsPremiumStatus.Checked);
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

        #region Idle Timeout

        private void m_prcClient_IdleTimeout(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessIdleTimeoutLimit = limit;

            if (this.m_iPreviousSuccessIdleTimeoutLimit == 0)
            {
                this.OnSettingResponse("vars.idletimeout 0", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.idletimeout", (decimal)this.m_iPreviousSuccessIdleTimeoutLimit, true);
                this.OnSettingResponse("vars.idletimeout 0", false, true);
            }
        }

        private void chkSettingsNoIdleKickLimit_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlSettingsSetidleKickLimit.Enabled = !this.chkSettingsNoIdleKickLimit.Checked;
            this.pnlSettingsSetidleKickLimit.Visible = !this.chkSettingsNoIdleKickLimit.Checked;

            this.chkSettingsNoIdleBanRoundsLimit.Enabled = !this.chkSettingsNoIdleKickLimit.Checked;
            this.chkSettingsNoIdleBanRoundsLimit.Visible = !this.chkSettingsNoIdleKickLimit.Checked;

            if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.idletimeout 0"].IgnoreEvent == false)
            {
                if (this.chkSettingsNoIdleKickLimit.Checked == true)
                {
                    this.WaitForSettingResponse("vars.idletimeout 0", !this.chkSettingsNoIdleKickLimit.Checked);

                    this.Client.Game.SendSetVarsIdleTimeoutPacket(0);
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

    }
}
