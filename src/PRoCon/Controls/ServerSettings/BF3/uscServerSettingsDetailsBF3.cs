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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings.BF3
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsDetailsBF3 : uscServerSettings
    {

        private string m_strPreviousSuccessServerName;
        private string m_strPreviousSuccessServerDescription;
        private string m_strPreviousSuccessServerMessage;
        private string m_strPreviousSuccessBannerURL;

        private CDownloadFile m_cdfBanner;

        public uscServerSettingsDetailsBF3()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.servername", new AsyncStyleSetting(this.picSettingsServerName, this.txtSettingsServerName, new Control[] { this.lblSettingsServerName, this.txtSettingsServerName, this.lnkSettingsSetServerName }, true));
            this.AsyncSettingControls.Add("vars.serverdescription", new AsyncStyleSetting(this.picSettingsDescription, this.txtSettingsDescription, new Control[] { this.lblSettingsDescription, this.txtSettingsDescription, this.lnkSettingsSetDescription }, true));
            this.AsyncSettingControls.Add("vars.servermessage", new AsyncStyleSetting(this.picSettingsMessage, this.txtSettingsMessage, new Control[] { this.lblSettingsMessage, this.txtSettingsMessage, this.lnkSettingsSetMessage }, true));
            this.AsyncSettingControls.Add("vars.bannerurl", new AsyncStyleSetting(this.picSettingsBannerURL, this.txtSettingsBannerURL, new Control[] { this.lblSettingsBannerURL, this.txtSettingsBannerURL, this.lnkSettingsSetBannerURL }, true));

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
            this.lblSettingsBannerURL.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsBannerURL");
            this.lnkSettingsSetBannerURL.Text = this.Language.GetLocalized("uscServerSettingsPanel.lnkSettingsSetBannerURL");

            this.lblSettingsDownloadedBannerURLError.Text = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsDownloadedBannerURLError");

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
                this.Client.Game.BannerUrl += new FrostbiteClient.BannerUrlHandler(m_prcClient_BannerUrl);
                this.Client.Game.ServerDescription += new FrostbiteClient.ServerDescriptionHandler(m_prcClient_ServerDescription);
                this.Client.Game.ServerMessage += new FrostbiteClient.ServerMessageHandler(m_prcClient_ServerMessage);
            });
        }

        #region Banner URL

        private void m_prcClient_BannerUrl(FrostbiteClient sender, string url)
        {
            this.InvokeIfRequired(() =>
            {
                this.OnSettingResponse("vars.bannerurl", url, true);

                if (String.Compare(this.m_strPreviousSuccessBannerURL, url) != 0)
                {

                    if (String.IsNullOrEmpty(url) == false)
                    {
                        this.DownloadBannerURL(url);
                    }
                    else
                    {
                        this.cdfBanner_DownloadComplete(null);
                    }
                }

                this.m_strPreviousSuccessBannerURL = url;
            });
        }

        public void OnSettingsBannerURLSuccess(FrostbiteClient sender, string strSuccessBannerURL)
        {
            this.InvokeIfRequired(() =>
            {
                if (String.Compare(this.m_strPreviousSuccessBannerURL, strSuccessBannerURL) != 0)
                {

                    if (String.IsNullOrEmpty(strSuccessBannerURL) == false)
                    {
                        this.DownloadBannerURL(strSuccessBannerURL);
                    }
                    else
                    {
                        this.cdfBanner_DownloadComplete(null);
                    }
                }

                this.m_strPreviousSuccessBannerURL = strSuccessBannerURL;
            });
        }

        private void lnkSettingsSetBannerURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                // TO DO: More error reporting about the image.
                if (String.IsNullOrEmpty(this.txtSettingsBannerURL.Text) == false)
                {
                    this.DownloadBannerURL(this.txtSettingsBannerURL.Text);
                }
                else
                {
                    this.cdfBanner_DownloadComplete(null);
                }
                //this.picSettingsDownloadedBannerURL.ImageLocation = this.txtSettingsBannerURL.Text;

                this.txtSettingsBannerURL.Focus();
                this.WaitForSettingResponse("vars.bannerurl", this.m_strPreviousSuccessBannerURL);

                this.Client.Game.SendSetVarsBannerUrlPacket(this.txtSettingsBannerURL.Text);
            }
        }

        #endregion

        #region Server Description

        private void m_prcClient_ServerDescription(FrostbiteClient sender, string serverDescription)
        {
            this.InvokeIfRequired(() =>
            {
                this.m_strPreviousSuccessServerDescription = serverDescription.Replace("|", Environment.NewLine);

                if (this.m_strPreviousSuccessServerDescription.Length >= 255)
                {
                    this.m_strPreviousSuccessServerDescription = this.m_strPreviousSuccessServerDescription.Substring(0, 255);
                }

                this.OnSettingResponse("vars.serverdescription", this.m_strPreviousSuccessServerDescription, true);
            });

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
            this.InvokeIfRequired(() =>
            {
                this.m_strPreviousSuccessServerMessage = serverMessage.Replace("|", Environment.NewLine);

                if (this.m_strPreviousSuccessServerMessage.Length >= 255)
                {
                    this.m_strPreviousSuccessServerMessage = this.m_strPreviousSuccessServerMessage.Substring(0, 255);
                }

                this.OnSettingResponse("vars.servermessage", this.m_strPreviousSuccessServerMessage, true);
            });
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

        #region Download Banner

        private void DownloadBannerURL(string strUrl)
        {
            if (strUrl != null)
            {
                this.m_cdfBanner = new CDownloadFile(strUrl);
                this.m_cdfBanner.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(cdfBanner_DownloadComplete);
                this.m_cdfBanner.DownloadError += new CDownloadFile.DownloadFileEventDelegate(cdfBanner_DownloadError);
                this.m_cdfBanner.BeginDownload();
            }
        }

        private void cdfBanner_DownloadError(CDownloadFile cdfSender)
        {
            this.lblSettingsDownloadedBannerURLError.Visible = true;
            this.picSettingsDownloadedBannerURL.Image = null;
        }

        private void cdfBanner_DownloadComplete(CDownloadFile cdfSender)
        {
            this.lblSettingsDownloadedBannerURLError.Visible = false;

            if (cdfSender != null)
            {
                MemoryStream msImage = new MemoryStream(cdfSender.CompleteFileData);
                Image imgCompleted = Image.FromStream(msImage);

                this.picSettingsDownloadedBannerURL.Image = imgCompleted;
            }
            else
            {
                this.picSettingsDownloadedBannerURL.Image = null;
            }
        }

        #endregion
    }
}
