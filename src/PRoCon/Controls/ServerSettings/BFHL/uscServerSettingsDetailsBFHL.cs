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
    public partial class uscServerSettingsDetailsBFHL : uscServerSettings
    {

        private string m_strPreviousSuccessServerName;
        private string m_strPreviousSuccessServerDescription;
        private string m_strPreviousSuccessServerMessage;
        private string m_strPreviousSuccessBannerURL;

        private CDownloadFile m_cdfBanner;

        public uscServerSettingsDetailsBFHL()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.servername", new AsyncStyleSetting(this.picSettingsServerName, this.txtSettingsServerName, new Control[] { this.lblSettingsServerName, this.txtSettingsServerName, this.lnkSettingsSetServerName }, true));
            this.AsyncSettingControls.Add("vars.serverdescription", new AsyncStyleSetting(this.picSettingsDescription, this.txtSettingsDescription, new Control[] { this.lblSettingsDescription, this.txtSettingsDescription, this.lnkSettingsSetDescription }, true));
            this.AsyncSettingControls.Add("vars.servermessage", new AsyncStyleSetting(this.picSettingsMessage, this.txtSettingsMessage, new Control[] { this.lblSettingsMessage, this.txtSettingsMessage, this.lnkSettingsSetMessage }, true));

            this.m_strPreviousSuccessServerName = String.Empty;
            this.m_strPreviousSuccessServerDescription = String.Empty;
            this.m_strPreviousSuccessServerMessage = String.Empty;
            this.m_strPreviousSuccessBannerURL = String.Empty;
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.lblSettingsDescription.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsDescription");
            this.lnkSettingsSetDescription.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetDescription");
            this.lblSettingsMessage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsMessage");
            this.lnkSettingsSetMessage.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetMessage");

            this.lblSettingsServerName.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsServerName");
            this.lnkSettingsSetServerName.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetServerName");

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsDetails");
        }

        public override void SetConnection(PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if (this.Client != null)
            {
                if (this.Client.Game != null)
                {
                    this.Client_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.Client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(Client_GameTypeDiscovered);
                }
            }
        }


        private void Client_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Game.ServerName += new FrostbiteClient.ServerNameHandler(m_prcClient_ServerName);
                this.Client.Game.ServerDescription += new FrostbiteClient.ServerDescriptionHandler(m_prcClient_ServerDescription);
                this.Client.Game.ServerMessage += new FrostbiteClient.ServerMessageHandler(m_prcClient_ServerMessage);
            });
        }

        #region Server Description

        private void m_prcClient_ServerDescription(FrostbiteClient sender, string serverDescription)
        {
            this.m_strPreviousSuccessServerDescription = serverDescription.Replace("|", Environment.NewLine);

            if (this.m_strPreviousSuccessServerDescription.Length >= 255)
            {
                this.m_strPreviousSuccessServerDescription = this.m_strPreviousSuccessServerDescription.Substring(0, 255);
            }

            this.OnSettingResponse("vars.serverdescription", this.m_strPreviousSuccessServerDescription, true);

        }

        private void lnkSettingsSetDescription_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.txtSettingsDescription.Focus();
                this.WaitForSettingResponse("vars.serverdescription", this.m_strPreviousSuccessServerDescription);

                this.Client.Game.SendSetVarsServerDescriptionPacket(this.txtSettingsDescription.Text.Replace(Environment.NewLine, "|"));
                //this.SendCommand("vars.serverDescription", );
            }
        }

        #endregion

        #region Server Message

        private void m_prcClient_ServerMessage(FrostbiteClient sender, string serverMessage)
        {
            this.m_strPreviousSuccessServerMessage = serverMessage.Replace("|", Environment.NewLine);

            if (this.m_strPreviousSuccessServerMessage.Length >= 255)
            {
                this.m_strPreviousSuccessServerMessage = this.m_strPreviousSuccessServerMessage.Substring(0, 255);
            }

            this.OnSettingResponse("vars.servermessage", this.m_strPreviousSuccessServerMessage, true);

        }

        private void lnkSettingsSetMessage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.txtSettingsMessage.Focus();
                this.WaitForSettingResponse("vars.servermessage", this.m_strPreviousSuccessServerMessage);

                this.Client.Game.SendSetVarsServerMessagePacket(this.txtSettingsMessage.Text.Replace(Environment.NewLine, "|"));
                //this.SendCommand("vars.serverMessage", );
            }
        }

        #endregion


        #region Server Name

        private void m_prcClient_ServerName(FrostbiteClient sender, string strServerName)
        {
            this.OnSettingResponse("vars.servername", strServerName, true);
            this.m_strPreviousSuccessServerName = strServerName;
        }

        private void lnkSettingsSetServerName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                this.txtSettingsServerName.Focus();
                this.WaitForSettingResponse("vars.servername", this.m_strPreviousSuccessServerName);

                this.Client.Game.SendSetVarsServerNamePacket(this.txtSettingsServerName.Text);
                //this.SendCommand("vars.serverName", this.txtSettingsServerName.Text);
            }
        }

        #endregion
    }
}
