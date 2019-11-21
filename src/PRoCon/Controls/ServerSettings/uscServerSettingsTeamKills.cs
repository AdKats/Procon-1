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
using System.Text;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsTeamKills : uscServerSettings
    {
        public uscServerSettingsTeamKills()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.teamkillcountforkick 0", new AsyncStyleSetting(this.picSettingsTeamkillCountLimit, this.chkSettingsTeamkillCountLimit, new Control[] { this.chkSettingsTeamkillCountLimit }, true));
            this.AsyncSettingControls.Add("vars.teamkillcountforkick", new AsyncStyleSetting(this.picSettingsTeamkillCountLimit, this.numSettingsTeamkillCountLimit, new Control[] { this.numSettingsTeamkillCountLimit, this.lnkSettingsTeamkillCountLimit }, true));

            this.AsyncSettingControls.Add("vars.teamkillvalueforkick 0", new AsyncStyleSetting(this.picSettingsTeamkillValueLimit, this.chkSettingsTeamkillValueLimit, new Control[] { this.chkSettingsTeamkillValueLimit }, true));
            this.AsyncSettingControls.Add("vars.teamkillvalueforkick", new AsyncStyleSetting(this.picSettingsTeamkillValueLimit, this.numSettingsTeamkillValueLimit, new Control[] { this.numSettingsTeamkillValueLimit, this.lnkSettingsTeamkillValueLimit }, true));

            this.AsyncSettingControls.Add("vars.teamkillvalueincrease", new AsyncStyleSetting(this.picSettingsTeamKillValueIncrease, this.numSettingsTeamKillValueIncrease, new Control[] { this.numSettingsTeamKillValueIncrease, this.lnkSettingsTeamKillValueIncrease }, true));
            this.AsyncSettingControls.Add("vars.teamkillvaluedecreasepersecond", new AsyncStyleSetting(this.picTeamKillValueDecreasePerSecond, this.numTeamKillValueDecreasePerSecond, new Control[] { this.numTeamKillValueDecreasePerSecond, this.lnkTeamKillValueDecreasePerSecond }, true));
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsTeamkillCountLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsTeamkillCountLimit");
            this.lnkSettingsTeamkillCountLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsTeamkillCountLimit");

            this.chkSettingsTeamkillValueLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsTeamkillValueLimit");
            this.lnkSettingsTeamkillValueLimit.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsTeamkillValueLimit");

            this.lblTeamKillValueDecreasePerSecond.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblTeamKillValueDecreasePerSecond");
            this.lnkTeamKillValueDecreasePerSecond.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkTeamKillValueDecreasePerSecond");

            this.lblSettingsTeamKillValueIncrease.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsTeamKillValueIncrease");
            this.lnkSettingsTeamKillValueIncrease.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsTeamKillValueIncrease");

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsTeamKill");

            this.UpdateTeamkillExplanations();
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
                this.Client.Game.TeamKillCountForKick += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillCountForKick);
                this.Client.Game.TeamKillValueForKick += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueForKick);
                this.Client.Game.TeamKillValueIncrease += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueIncrease);
                this.Client.Game.TeamKillValueDecreasePerSecond += new FrostbiteClient.LimitHandler(m_prcClient_TeamKillValueDecreasePerSecond);
            });
        }

        #region Team Kill Count For Kick

        private int m_iPreviousSuccessTeamKillCountForKick;

        private void m_prcClient_TeamKillCountForKick(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessTeamKillCountForKick = limit;

            if (this.m_iPreviousSuccessTeamKillCountForKick == 0)
            {
                this.OnSettingResponse("vars.teamkillcountforkick 0", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.teamkillcountforkick", (decimal)this.m_iPreviousSuccessTeamKillCountForKick, true);
            }
        }

        private void chkSettingsTeamkillCountLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.pnlSettingsTeamkillCountLimit.Enabled = !this.chkSettingsTeamkillCountLimit.Checked;
                this.pnlSettingsTeamkillCountLimit.Visible = !this.chkSettingsTeamkillCountLimit.Checked;

                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.teamkillcountforkick 0"].IgnoreEvent == false)
                {
                    if (this.chkSettingsTeamkillCountLimit.Checked == true)
                    {
                        this.WaitForSettingResponse("vars.teamkillcountforkick 0", !this.chkSettingsTeamkillCountLimit.Checked);

                        this.Client.Game.SendSetVarsTeamKillCountForKickPacket(0);
                    }
                }
            }
        }

        private void lnkSettingsTeamkillCountLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsTeamkillCountLimit.Focus();
                this.WaitForSettingResponse("vars.teamkillcountforkick", (decimal)this.m_iPreviousSuccessTeamKillCountForKick);

                this.Client.Game.SendSetVarsTeamKillCountForKickPacket((int)this.numSettingsTeamkillCountLimit.Value);
                //this.SendCommand("vars.teamKillCountForKick", this.numSettingsTeamkillCountLimit.Value.ToString());
            }
        }

        #endregion

        #region Team Killing

        private int m_iPreviousSuccessTeamKillValueForKick;

        private void m_prcClient_TeamKillValueForKick(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessTeamKillValueForKick = limit;

            if (this.m_iPreviousSuccessTeamKillValueForKick == 0)
            {
                this.OnSettingResponse("vars.teamkillvalueforkick 0", true, true);
            }
            else
            {
                this.OnSettingResponse("vars.teamkillvalueforkick", (decimal)this.m_iPreviousSuccessTeamKillValueForKick, true);
            }
        }

        private void chkSettingsTeamkillValueLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.pnlSettingsTeamkillValueLimit.Enabled = !this.chkSettingsTeamkillValueLimit.Checked;
                this.pnlSettingsTeamkillValueLimit.Visible = !this.chkSettingsTeamkillValueLimit.Checked;

                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.teamkillvalueforkick 0"].IgnoreEvent == false)
                {
                    if (this.chkSettingsTeamkillValueLimit.Checked == true)
                    {
                        this.WaitForSettingResponse("vars.teamkillvalueforkick 0", !this.chkSettingsTeamkillValueLimit.Checked);

                        this.Client.Game.SendSetVarsTeamKillValueForKickPacket(0);

                        //this.SendCommand("vars.teamKillValueForKick", "0");
                    }
                }
            }
        }

        private void lnkSettingsTeamkillValueLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsTeamkillValueLimit.Focus();
                this.WaitForSettingResponse("vars.teamkillvalueforkick", (decimal)this.m_iPreviousSuccessTeamKillValueForKick);

                this.Client.Game.SendSetVarsTeamKillValueForKickPacket((int)this.numSettingsTeamkillValueLimit.Value);
            }
        }


        private int m_iPreviousSuccessTeamKillValueIncrease;

        void m_prcClient_TeamKillValueIncrease(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessTeamKillValueIncrease = limit;

            this.OnSettingResponse("vars.teamkillvalueincrease", (decimal)limit, true);
        }

        private void lnkSettingsTeamKillValueIncrease_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsTeamKillValueIncrease.Focus();
                this.WaitForSettingResponse("vars.teamkillvalueincrease", (decimal)this.m_iPreviousSuccessTeamKillValueIncrease);

                this.Client.Game.SendSetVarsTeamKillValueIncreasePacket((int)this.numSettingsTeamKillValueIncrease.Value);
            }
        }

        private int m_iPreviousSuccessTeamKillValueDecreasePerSecond;

        void m_prcClient_TeamKillValueDecreasePerSecond(FrostbiteClient sender, int limit)
        {
            this.m_iPreviousSuccessTeamKillValueDecreasePerSecond = limit;

            this.OnSettingResponse("vars.teamkillvaluedecreasepersecond", (decimal)limit, true);
        }

        private void lnkTeamKillValueDecreasePerSecond_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numTeamKillValueDecreasePerSecond.Focus();
                this.WaitForSettingResponse("vars.teamkillvaluedecreasepersecond", (decimal)this.m_iPreviousSuccessTeamKillValueDecreasePerSecond);

                this.Client.Game.SendSetVarsTeamKillValueDecreasePerSecondPacket((int)this.numTeamKillValueDecreasePerSecond.Value);
            }
        }

        private void UpdateTeamkillExplanations()
        {

            float burstSecond = (float)this.numSettingsTeamkillValueLimit.Value / (float)this.numSettingsTeamKillValueIncrease.Value;
            float minimumRate = 60.0F / ((float)this.numSettingsTeamKillValueIncrease.Value / ((float)this.numTeamKillValueDecreasePerSecond.Value - 1.0F)) + burstSecond;
            float forgivenSeconds = (float)this.numSettingsTeamKillValueIncrease.Value / (float)this.numTeamKillValueDecreasePerSecond.Value;

            this.lblSettingsTkCountExplanation.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsTkCountExplanation", this.numSettingsTeamkillCountLimit.Value.ToString());

            StringBuilder valueExplanation = new StringBuilder();
            valueExplanation.AppendLine(this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsTkValueExplanation.Minimums", burstSecond.ToString("0.0"), minimumRate.ToString("0.00")));
            //valueExplanation.AppendLine(this.m_clocLanguage.GetLocalized("uscServerSettingsPanel.lblSettingsTkValueExplanation.MinimumRateMinute", minimumRate.ToString("0.00")));
            valueExplanation.AppendLine(this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsTkValueExplanation.ForgivenSingle", forgivenSeconds.ToString("0.0")));

            this.lblSettingsTkValueExplanation.Text = valueExplanation.ToString();

        }

        private void numSettingsTeamkillValueLimit_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateTeamkillExplanations();
        }

        private void numSettingsTeamKillValueIncrease_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateTeamkillExplanations();
        }

        private void numTeamKillValueDecreasePerSecond_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateTeamkillExplanations();
        }

        private void numSettingsTeamkillCountLimit_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateTeamkillExplanations();
        }

        #endregion
    }
}
