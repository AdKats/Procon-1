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

using PRoCon.Core;
using PRoCon.Core.Remote;
using PRoCon.Forms;
using System;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscLoginPanel : UserControl
    {

        private uscServerConnection m_uscConnectionPanel;
        private CLocalization m_clocLanguage;

        private PRoConClient m_prcClient;

        public string BackgroundHostPort
        {
            set
            {
                this.lblLargeServer.Text = value;
            }
        }

        public string ErrorMessage
        {
            set
            {
                this.lblError.Text = value;
            }
        }

        public uscLoginPanel()
        {
            InitializeComponent();
        }

        public void Initalize(frmMain frmMain, uscServerConnection uscConnectionPanel)
        {
            this.m_uscConnectionPanel = uscConnectionPanel;

            this.picConnecting.Image = frmMain.picAjaxStyleLoading.Image;
        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_prcClient = prcClient) != null)
            {
                this.m_prcClient.ConnectAttempt += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectAttempt);
                this.m_prcClient.ConnectionClosed += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);
                this.m_prcClient.ConnectionFailure += new PRoConClient.FailureHandler(m_prcClient_ConnectionFailure);
                this.m_prcClient.SocketException += new PRoConClient.SocketExceptionHandler(m_prcClient_SocketException);
                this.m_prcClient.Logout += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogout);
                this.m_prcClient.LoginFailure += new PRoConClient.AuthenticationFailureHandler(m_prcClient_CommandLoginFailure);
                this.m_prcClient.Login += new PRoConClient.EmptyParamterHandler(m_prcClient_Login);

                this.m_prcClient.AutomaticallyConnectChanged += new PRoConClient.AutomaticallyConnectHandler(m_prcClient_AutomaticallyConnectChanged);
            }
        }

        private bool Connecting
        {
            set
            {
                this.picConnecting.Visible = value;
                this.lnkConnectionsConnect.Enabled = !value;

                if (value == true)
                {
                    this.lblError.Text = String.Empty;
                }

            }
        }

        private bool LoggedIn
        {
            set
            {
                if (value == true)
                {
                    this.Hide();
                }
                else
                {
                    this.Dock = DockStyle.Fill;
                    this.Show();
                }
            }
        }

        private void m_prcClient_ConnectAttempt(PRoConClient sender)
        {
            this.InvokeIfRequired(() => { this.Connecting = true; });
        }

        private void m_prcClient_CommandLogout(PRoConClient sender)
        {
            this.InvokeIfRequired(() => { this.LoggedIn = false; });
        }

        private void m_prcClient_Login(PRoConClient sender)
        {
            this.InvokeIfRequired(() => { this.LoggedIn = true; });
        }

        private void m_prcClient_CommandLoginFailure(PRoConClient sender, string strError)
        {
            this.InvokeIfRequired(() =>
            {
                if (String.Compare(strError, "InsufficientPrivileges", true) == 0)
                {
                    this.ErrorMessage = this.m_clocLanguage.GetLocalized("uscLoginPanel.ErrorMessage.InsufficientPrivileges");
                }
                else if (String.Compare(strError, "InvalidUsername", true) == 0)
                {
                    this.ErrorMessage = this.m_clocLanguage.GetLocalized("uscLoginPanel.ErrorMessage.InvalidUsername");
                }
                else if (String.Compare(strError, "InvalidPassword", true) == 0 || String.Compare(strError, "InvalidPasswordHash", true) == 0)
                {
                    this.ErrorMessage = this.m_clocLanguage.GetLocalized("uscLoginPanel.ErrorMessage.InvalidPassword");
                }

                this.Connecting = this.LoggedIn = false;
            });
        }

        private void m_prcClient_ConnectionFailure(PRoConClient sender, Exception exception)
        {
            this.InvokeIfRequired(() =>
            {
                this.ErrorMessage = this.m_clocLanguage.GetLocalized("uscServerConnection.OnServerConnectionFailure", exception.Message);

                this.Connecting = this.LoggedIn = false;
            });
        }

        private void m_prcClient_SocketException(PRoConClient sender, System.Net.Sockets.SocketException se)
        {
            this.InvokeIfRequired(() =>
            {
                this.ErrorMessage = this.m_clocLanguage.GetLocalized("uscServerConnection.OnServerConnectionFailure", se.Message);

                this.Connecting = this.LoggedIn = false;
            });
        }

        private void m_prcClient_ConnectionClosed(PRoConClient sender)
        {
            this.InvokeIfRequired(() => { this.Connecting = this.LoggedIn = false; });
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            this.lblConnectionUsername.Text = this.m_clocLanguage.GetLocalized("uscLoginPanel.lblConnectionUsername", null) + ":";
            this.lblConnectionPassword.Text = this.m_clocLanguage.GetLocalized("uscLoginPanel.lblConnectionPassword", null) + ":";
            this.chkAutomaticallyConnect.Text = this.m_clocLanguage.GetLocalized("uscLoginPanel.chkAutomaticallyConnect", null);
            this.lnkConnectionsConnect.Text = this.m_clocLanguage.GetLocalized("uscLoginPanel.lnkConnectionsConnect", null);
        }

        private void lnkConnectionsConnect_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.m_prcClient.Username = this.txtConnectionsUsername.Text;
            this.m_prcClient.Password = this.txtConnectionsPassword.Text;

            this.m_prcClient.Connect();

            //if (this.OnAttemptConnection != null) {
            //    this.OnAttemptConnection(this.txtConnectionsUsername.Text, this.txtConnectionsPassword.Text);
            //}
        }

        private void txtConnectionsUsername_Enter(object sender, EventArgs e)
        {
            // this.txtConnectionsUsername.Too
        }

        private void uscLoginPanel_Load(object sender, EventArgs e)
        {
            if (this.m_prcClient != null)
            {
                this.chkAutomaticallyConnect.Checked = this.m_prcClient.AutomaticallyConnect;

                this.txtConnectionsUsername.Text = this.m_prcClient.Username;
                this.txtConnectionsPassword.Text = this.m_prcClient.Password;
            }
        }

        private void chkAutomaticallyConnect_CheckedChanged(object sender, EventArgs e)
        {
            this.m_prcClient.AutomaticallyConnect = this.chkAutomaticallyConnect.Checked;
        }

        void m_prcClient_AutomaticallyConnectChanged(PRoConClient sender, bool isEnabled)
        {
            this.InvokeIfRequired(() => { this.chkAutomaticallyConnect.Checked = isEnabled; });
        }
    }
}
