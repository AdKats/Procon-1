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

using PRoCon.Controls.ServerSettings;
using PRoCon.Controls.ServerSettings.BF3;
using PRoCon.Controls.ServerSettings.BF4;
using PRoCon.Controls.ServerSettings.BFBC2;
using PRoCon.Controls.ServerSettings.BFHL;
using PRoCon.Controls.ServerSettings.MOH;
using PRoCon.Controls.ServerSettings.MOHW;
using PRoCon.Core;
using PRoCon.Core.Remote;
using PRoCon.Forms;
using System;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscServerSettingsPanel : UserControl
    {

        private frmMain m_frmMain;
        private CLocalization m_clocLanguage;

        public uscServerSettingsPanel()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.cboSelectedSettingsPanel.DisplayMember = "DisplayName";
        }

        private void uscServerSettingsPanel_Load(object sender, EventArgs e)
        {

        }

        public void Initialize(frmMain frmMainWindow)
        {
            this.m_frmMain = frmMainWindow;
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            /*
            this.cmiExportSettings.Text = this.m_clocLanguage.GetLocalized("uscServerSettingsPanel.cmiExportSettings");
            this.cmiImportSettings.Text = this.m_clocLanguage.GetLocalized("uscServerSettingsPanel.cmiImportSettings");
            this.cmiConfigGenerator.Text = this.m_clocLanguage.GetLocalized("uscServerSettingsPanel.cmiConfigGenerator");
            */

            foreach (uscServerSettings page in this.cboSelectedSettingsPanel.Items)
            {
                page.SetLocalization(clocLanguage);
            }
        }

        public void SetConnection(PRoConClient prcClient)
        {
            if (prcClient != null)
            {
                if (prcClient.Game != null)
                {
                    this.prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    prcClient.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(prcClient_GameTypeDiscovered);
                }
            }
        }

        private void prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                sender.ProconPrivileges += new PRoConClient.ProconPrivilegesHandler(sender_ProconPrivileges);

                if (sender.Game is BF3Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsDetailsBF3());
                }
                else if (sender.Game is BFHLClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsDetailsBFHL());
                }
                else if (sender.Game is BF4Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsDetailsBF4());
                }
                else if (sender.Game is MOHWClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsDetailsBF3());
                }
                else
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsDetails());
                }

                if (sender.Game is BF3Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigurationBF3());
                }
                else if (sender.Game is BFHLClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigurationBFHL());
                }
                else if (sender.Game is BF4Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigurationBF4());
                }
                else if (sender.Game is MOHWClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigurationMOHW());
                }
                else
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfiguration());
                }

                if (sender.Game is BFBC2Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayBFBC2());
                }
                else if (sender.Game is MoHClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayMoH());
                }
                else if (sender.Game is BF3Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayBF3());
                }
                else if (sender.Game is BFHLClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayBFHL());
                }
                else if (sender.Game is BF4Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayBF4());
                }
                else if (sender.Game is MOHWClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsGameplayMOHW());
                }

                if (sender.Game is BFBC2Client || sender.Game is MoHClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsTextChatModeration());
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsLevelVariables());
                }

                if (sender.Game is BFHLClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsTeamKillsBFHL());
                }
                else if (sender.Game is BF4Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsTeamKillsBF4());
                }
                else
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsTeamKills());
                }

                if (sender.Game is BFBC2Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorBFBC2());
                }
                else if (sender.Game is MoHClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorMoH());
                }
                else if (sender.Game is BF3Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorBF3());
                }
                else if (sender.Game is BFHLClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorBFHL());
                }
                else if (sender.Game is BF4Client)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorBF4());
                }
                else if (sender.Game is MOHWClient)
                {
                    this.cboSelectedSettingsPanel.Items.Add(new uscServerSettingsConfigGeneratorMOHW());
                }

                foreach (uscServerSettings page in this.cboSelectedSettingsPanel.Items)
                {
                    if (this.pnlSettingsPanels.Controls.Contains(page) == false)
                    {
                        this.pnlSettingsPanels.Controls.Add(page);
                        page.Dock = DockStyle.Fill;
                    }

                    if (this.cboSelectedSettingsPanel.SelectedItem == null)
                    {
                        this.cboSelectedSettingsPanel.SelectedItem = page;
                    }

                    if (this.m_frmMain != null)
                    {
                        page.SettingLoading = this.m_frmMain.picAjaxStyleLoading.Image;
                        page.SettingFail = this.m_frmMain.picAjaxStyleFail.Image;
                        page.SettingSuccess = this.m_frmMain.picAjaxStyleSuccess.Image;
                    }

                    page.SetConnection(sender);

                    if (this.m_clocLanguage != null)
                    {
                        page.SetLocalization(this.m_clocLanguage);
                    }
                }
            });
        }

        public void sender_ProconPrivileges(PRoConClient sender, CPrivileges spPrivs)
        {
            this.InvokeIfRequired(() => { this.pnlSettingsPanels.Enabled = spPrivs.CanAlterServerSettings; });
        }

        private void cboSelectedSettingsPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboSelectedSettingsPanel.SelectedItem != null)
            {

                foreach (uscServerSettings page in this.cboSelectedSettingsPanel.Items)
                {
                    if (page == this.cboSelectedSettingsPanel.SelectedItem)
                    {
                        page.Show();
                    }
                    else
                    {
                        page.Hide();
                    }
                }
            }
        }

    }
}
