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

namespace PRoCon.Controls.ServerSettings
{
    using Core;
    using Core.Remote;
    using Core.TextChatModeration;
    public partial class uscServerSettingsTextChatModeration : uscServerSettings
    {

        private int m_iPreviousSuccessTextChatSpamTriggerCount;
        private int m_iPreviousSuccessTextChatSpamDetectionTime;
        private int m_iPreviousSuccessTextChatSpamCoolDownTime;

        public uscServerSettingsTextChatModeration()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.textChatModerationMode", new AsyncStyleSetting(this.picSettingsModerationMode, null, new Control[] { this.rdoSettingsModerationModeFree, this.rdoSettingsModerationModeModerated, this.rdoSettingsModerationModeMuted }, true));
            this.AsyncSettingControls.Add("vars.textChatSpamTriggerCount", new AsyncStyleSetting(this.picSettingsModerationTriggerCount, this.numSettingsModerationTriggerCount, new Control[] { this.numSettingsModerationTriggerCount, this.lnkSettingsModerationTriggerCount }, true));
            this.AsyncSettingControls.Add("vars.textChatSpamDetectionTime", new AsyncStyleSetting(this.picSettingsModerationDetectionTime, this.numSettingsModerationDetectionTime, new Control[] { this.numSettingsModerationDetectionTime, this.lnkSettingsModerationDetectionTime }, true));
            this.AsyncSettingControls.Add("vars.textChatSpamCoolDownTime", new AsyncStyleSetting(this.picSettingsModerationCooldownTime, this.numSettingsModerationCooldownTime, new Control[] { this.numSettingsModerationCooldownTime, this.lnkSettingsModerationCooldownTime }, true));

            this.m_iPreviousSuccessTextChatSpamTriggerCount = 5;
            this.m_iPreviousSuccessTextChatSpamDetectionTime = 10;
            this.m_iPreviousSuccessTextChatSpamCoolDownTime = 60;
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsTextChatModeration.DisplayName");

            this.lblSettingsModerationMode.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lblSettingsModerationMode");
            this.rdoSettingsModerationModeFree.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.rdoSettingsModerationModeFree");
            this.rdoSettingsModerationModeModerated.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.rdoSettingsModerationModeModerated");
            this.rdoSettingsModerationModeMuted.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.rdoSettingsModerationModeMuted");

            this.lblSettingsModerationTriggerCount.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lblSettingsModerationTriggerCount");
            this.lnkSettingsModerationTriggerCount.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lnkSettingsModerationTriggerCount");

            this.lblSettingsModerationDetectionTime.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lblSettingsModerationDetectionTime");
            this.lnkSettingsModerationDetectionTime.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lnkSettingsModerationDetectionTime");

            this.lblSettingsModerationCooldownTime.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lblSettingsModerationCooldownTime");
            this.lnkSettingsModerationCooldownTime.Text = this.Language.GetLocalized("uscServerSettingsTextChatModeration.lnkSettingsModerationCooldownTime");
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
                this.Client.Game.TextChatModerationMode += new FrostbiteClient.TextChatModerationModeHandler(Game_TextChatModerationMode);
                this.Client.Game.TextChatSpamTriggerCount += new FrostbiteClient.LimitHandler(Game_TextChatSpamTriggerCount);
                this.Client.Game.TextChatSpamDetectionTime += new FrostbiteClient.LimitHandler(Game_TextChatSpamDetectionTime);
                this.Client.Game.TextChatSpamCoolDownTime += new FrostbiteClient.LimitHandler(Game_TextChatSpamCoolDownTime);

                this.Client.Game.ResponseError += new FrostbiteClient.ResponseErrorHandler(Game_ResponseError);
            });
        }

        private void Game_ResponseError(FrostbiteClient sender, Packet originalRequest, string errorMessage)
        {

            // if set moderation mode fail - Request current moderation mode.
            if (originalRequest.Words.Count >= 2 && String.Compare(originalRequest.Words[0], "vars.textChatModerationMode") == 0)
            {
                this.OnSettingResponse("vars.textChatModerationMode", null, false);
            }

        }

        #region Moderation Mode

        private void rdoSettingsModerationModeFree_CheckedChanged(object sender, EventArgs e)
        {

            if (this.rdoSettingsModerationModeFree.Checked == true && this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.textChatModerationMode"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.textChatModerationMode", null);

                    this.Client.Game.SendSetVarsTextChatModerationModePacket(ServerModerationModeType.Free);
                }
            }
        }

        private void rdoSettingsModerationModeModerated_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoSettingsModerationModeModerated.Checked == true && this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.textChatModerationMode"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.textChatModerationMode", null);

                    this.Client.Game.SendSetVarsTextChatModerationModePacket(ServerModerationModeType.Moderated);
                }
            }
        }

        private void rdoSettingsModerationModeMuted_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoSettingsModerationModeMuted.Checked == true && this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.textChatModerationMode"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.textChatModerationMode", null);

                    this.Client.Game.SendSetVarsTextChatModerationModePacket(ServerModerationModeType.Muted);
                }
            }
        }

        private void Game_TextChatModerationMode(FrostbiteClient sender, ServerModerationModeType mode)
        {
            this.InvokeIfRequired(() =>
            {
                this.OnSettingResponse("vars.textChatModerationMode", null, true);

                this.IgnoreEvents = true;

                if (mode == ServerModerationModeType.Free)
                {
                    this.rdoSettingsModerationModeFree.Checked = true;
                }
                else if (mode == ServerModerationModeType.Moderated)
                {
                    this.rdoSettingsModerationModeModerated.Checked = true;
                }
                else if (mode == ServerModerationModeType.Muted)
                {
                    this.rdoSettingsModerationModeMuted.Checked = true;
                }
                else
                {
                    this.rdoSettingsModerationModeFree.Checked = this.rdoSettingsModerationModeModerated.Checked = this.rdoSettingsModerationModeMuted.Checked = false;
                }

                this.IgnoreEvents = false;
            });
        }

        #endregion

        #region Trigger count

        private void lnkSettingsModerationTriggerCount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsModerationTriggerCount.Focus();
                this.WaitForSettingResponse("vars.textChatSpamTriggerCount", (decimal)this.m_iPreviousSuccessTextChatSpamTriggerCount);

                this.Client.Game.SendSetVarsTextChatSpamTriggerCountPacket((int)this.numSettingsModerationTriggerCount.Value);
            }
        }

        private void Game_TextChatSpamTriggerCount(FrostbiteClient sender, int limit)
        {
            this.OnSettingResponse("vars.textChatSpamTriggerCount", (decimal)limit, true);
            this.m_iPreviousSuccessTextChatSpamTriggerCount = limit;
        }

        #endregion

        #region Detection time

        private void lnkSettingsModerationDetectionTime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsModerationDetectionTime.Focus();
                this.WaitForSettingResponse("vars.textChatSpamDetectionTime", (decimal)this.m_iPreviousSuccessTextChatSpamDetectionTime);

                this.Client.Game.SendSetVarsTextChatSpamDetectionTimePacket((int)this.numSettingsModerationDetectionTime.Value);
                //this.SendCommand("vars.playerLimit", this.numSettingsPlayerLimit.Value.ToString());
            }
        }

        private void Game_TextChatSpamDetectionTime(FrostbiteClient sender, int limit)
        {
            this.OnSettingResponse("vars.textChatSpamDetectionTime", (decimal)limit, true);
            this.m_iPreviousSuccessTextChatSpamDetectionTime = limit;
        }

        #endregion

        #region Cooldown time

        private void lnkSettingsModerationCooldownTime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.numSettingsModerationCooldownTime.Focus();
                this.WaitForSettingResponse("vars.textChatSpamCoolDownTime", (decimal)this.m_iPreviousSuccessTextChatSpamCoolDownTime);

                this.Client.Game.SendSetVarsTextChatSpamCoolDownTimePacket((int)this.numSettingsModerationCooldownTime.Value);
                //this.SendCommand("vars.playerLimit", this.numSettingsPlayerLimit.Value.ToString());
            }
        }

        private void Game_TextChatSpamCoolDownTime(FrostbiteClient sender, int limit)
        {
            this.OnSettingResponse("vars.textChatSpamCoolDownTime", (decimal)limit, true);
            this.m_iPreviousSuccessTextChatSpamCoolDownTime = limit;
        }

        #endregion




    }
}
