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
using System.Windows.Forms;

namespace PRoCon.Controls
{
    using Core;

    public partial class uscPrivilegesSelection : UserControl
    {

        private CLocalization m_clocLanguage = null;

        public delegate void OnCancelPrivilegesDelegate();
        public event OnCancelPrivilegesDelegate OnCancelPrivileges;

        public delegate void OnUpdatePrivilegesDelegate(string strAccountName, CPrivileges spUpdatedPrivs);
        public event OnUpdatePrivilegesDelegate OnUpdatePrivileges;

        private string m_strEditingAccountName;
        public string AccountName
        {
            set
            {
                this.m_strEditingAccountName = value;
                this.lblAccountPrivilegesTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblAccountPrivilegesTitle", new string[] { value });

                this.rdoNoProconAccess.Checked = true;
                this.chkAlterServerSettings.Checked = false;
                this.chkChangeCurrentMapFunctions.Checked = false;
                this.rdoNoPlayerPunishment.Checked = true;
                this.rdoNotAllowedToIssuePunkbusterCommands.Checked = true;
                this.chkEditMapList.Checked = false;
                this.chkEditBanList.Checked = false;
                this.chkEditReservedSlotsList.Checked = false;

                this.chkAllowConnectionLogin.Checked = false;

                this.pnlSubPrivileges.AutoScrollPosition = new Point(0, 0);
            }
        }

        public uscPrivilegesSelection()
        {
            InitializeComponent();
        }

        public void SetLocalization(CLocalization clocLanguage)
        {

            if ((this.m_clocLanguage = clocLanguage) != null)
            {

                this.chkAllowConnectionLogin.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkAllowConnectionLogin");

                this.rdoNoProconAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoNoProconAccess");
                this.rdoLimitedProconPluginAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoLimitedProconPluginAccess");
                this.rdoLimitedProconAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoLimitedProconAccess");
                this.rdoFullProconAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoFullProconAccess");
                this.chkCanShutdownServer.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkCanShutdownServer");

                this.lblCommandsviaRconConsolePlugins.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblCommandsviaRconConsolePlugins");

                this.lblServerStateTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblServerStateTitle");
                this.chkAlterServerSettings.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkAlterServerSettings");
                this.chkChangeCurrentMapFunctions.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkChangeCurrentMapFunctions");

                this.lblPlayersTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblPlayersTitle");
                this.rdoNoPlayerPunishment.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoNoPlayerPunishment");
                this.rdoKillingPlayersOnly.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoKillingPlayersOnly");
                this.rdoKickingPlayers.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoKickingPlayers");
                this.rdoKickingTemporaryOnly.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoKickingTemporaryOnly");
                this.rdoKicingTemporaryPermanent.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoKicingTemporaryPermanent");
                this.chkMovePlayers.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkMovePlayers");

                this.lblPunkbusterTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblPunkbusterTitle");
                this.rdoNotAllowedToIssuePunkbusterCommands.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoNotAllowedToIssuePunkbusterCommands");
                this.rdoLimitedPunkbusterAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoLimitedPunkbusterAccess");
                this.rdoFullPunkbusterAccess.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.rdoFullPunkbusterAccess");

                this.lblBattlemapTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblBattlemapTitle");
                this.chkEditMapZones.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkEditMapZones");

                this.lblListsTitle.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.lblListsTitle");
                this.chkEditMapList.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkEditMapList");
                this.chkEditBanList.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkEditBanList");
                this.chkEditReservedSlotsList.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkEditReservedSlotsList");
                this.chkEditTextModerationList.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.chkEditTextModerationList");

                this.btnSavePrivileges.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.btnSavePrivileges");
                this.btnCancelPrivileges.Text = this.m_clocLanguage.GetLocalized("uscAccountsPanel.btnCancelPrivileges");
            }
        }

        private void btnSavePrivileges_Click(object sender, EventArgs e)
        {

            UInt32 i = 0;

            i |= (1 & Convert.ToUInt32(this.chkAllowConnectionLogin.Checked));
            i |= (1 & Convert.ToUInt32(this.chkAlterServerSettings.Checked)) << 1;
            i |= (1 & Convert.ToUInt32(this.chkChangeCurrentMapFunctions.Checked)) << 2;

            i |= (1 & Convert.ToUInt32(this.rdoNoPlayerPunishment.Checked)) << 3;
            i |= (1 & Convert.ToUInt32(this.rdoKickingPlayers.Checked)) << 4;
            i |= (1 & Convert.ToUInt32(this.rdoKickingTemporaryOnly.Checked)) << 5;
            i |= (1 & Convert.ToUInt32(this.rdoKicingTemporaryPermanent.Checked)) << 6;

            i |= (1 & Convert.ToUInt32(this.rdoNotAllowedToIssuePunkbusterCommands.Checked)) << 7;
            i |= (1 & Convert.ToUInt32(this.rdoLimitedPunkbusterAccess.Checked)) << 8;
            i |= (1 & Convert.ToUInt32(this.rdoFullPunkbusterAccess.Checked)) << 9;

            i |= (1 & Convert.ToUInt32(this.chkEditMapList.Checked)) << 10;
            i |= (1 & Convert.ToUInt32(this.chkEditBanList.Checked)) << 11;
            i |= (1 & Convert.ToUInt32(this.chkEditReservedSlotsList.Checked)) << 12;

            i |= (1 & Convert.ToUInt32(this.rdoNoProconAccess.Checked)) << 13;
            i |= (1 & Convert.ToUInt32(this.rdoLimitedProconAccess.Checked)) << 14;
            i |= (1 & Convert.ToUInt32(this.rdoFullProconAccess.Checked)) << 15;

            // 0.3.4.0 additions
            i |= (1 & Convert.ToUInt32(this.rdoKillingPlayersOnly.Checked)) << 16;
            i |= (1 & Convert.ToUInt32(this.rdoLimitedProconPluginAccess.Checked)) << 17;
            i |= (1 & Convert.ToUInt32(this.chkMovePlayers.Checked)) << 18;

            // 0.5.4.0 additions
            i |= (1 & Convert.ToUInt32(this.chkEditMapZones.Checked)) << 19;

            // 0.6.0.0 additions
            i |= (1 & Convert.ToUInt32(this.chkEditTextModerationList.Checked)) << 20;
            i |= (1 & Convert.ToUInt32(this.chkCanShutdownServer.Checked)) << 21;

            CPrivileges spUpdatedPrivileges = new CPrivileges();
            spUpdatedPrivileges.PrivilegesFlags = i;

            // TO DO: Event Privileges set ()
            if (this.OnUpdatePrivileges != null)
            {
                this.OnUpdatePrivileges(this.m_strEditingAccountName, spUpdatedPrivileges);
            }
        }

        private void btnCancelPrivileges_Click(object sender, EventArgs e)
        {
            if (this.OnCancelPrivileges != null)
            {
                this.OnCancelPrivileges();
            }
        }

        private void chkAllowConnectionLogin_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlRconAccess.Enabled = this.chkAllowConnectionLogin.Checked;

            if (this.pnlRconAccess.Enabled == false)
            {
                this.rdoNoProconAccess.Checked = true;
            }
        }

        public CPrivileges Privileges
        {

            set
            {

                this.rdoNoProconAccess.Checked = value.CannotIssueProconCommands;
                this.rdoLimitedProconPluginAccess.Checked = value.CanIssueLimitedProconPluginCommands;
                this.rdoLimitedProconAccess.Checked = value.CanIssueLimitedProconCommands;
                this.rdoFullProconAccess.Checked = value.CanIssueAllProconCommands;

                if (this.rdoLimitedProconPluginAccess.Checked == false && this.rdoNoProconAccess.Checked == false && this.rdoLimitedProconAccess.Checked == false && this.rdoFullProconAccess.Checked == false)
                {
                    this.rdoNoProconAccess.Checked = true;
                }

                this.chkAlterServerSettings.Checked = value.CanAlterServerSettings;
                this.chkChangeCurrentMapFunctions.Checked = value.CanUseMapFunctions;

                this.rdoNoPlayerPunishment.Checked = value.CannotPunishPlayers;
                this.rdoKillingPlayersOnly.Checked = value.CanKillPlayers;
                this.rdoKickingPlayers.Checked = value.CanKickPlayers;
                this.rdoKickingTemporaryOnly.Checked = value.CanTemporaryBanPlayers;
                this.rdoKicingTemporaryPermanent.Checked = value.CanPermanentlyBanPlayers;

                this.chkMovePlayers.Checked = value.CanMovePlayers;

                if (this.rdoKillingPlayersOnly.Checked == false && this.rdoNoPlayerPunishment.Checked == false && this.rdoKickingPlayers.Checked == false && this.rdoKickingTemporaryOnly.Checked == false && this.rdoKicingTemporaryPermanent.Checked == false)
                {
                    this.rdoNoPlayerPunishment.Checked = true;
                }

                this.rdoNotAllowedToIssuePunkbusterCommands.Checked = value.CannotIssuePunkbusterCommands;
                this.rdoLimitedPunkbusterAccess.Checked = value.CanIssueLimitedPunkbusterCommands;
                this.rdoFullPunkbusterAccess.Checked = value.CanIssueAllPunkbusterCommands;

                if (this.rdoNotAllowedToIssuePunkbusterCommands.Checked == false && this.rdoLimitedPunkbusterAccess.Checked == false && this.rdoFullPunkbusterAccess.Checked == false)
                {
                    this.rdoNotAllowedToIssuePunkbusterCommands.Checked = true;
                }

                this.chkEditMapList.Checked = value.CanEditMapList;
                this.chkEditBanList.Checked = value.CanEditBanList;
                this.chkEditReservedSlotsList.Checked = value.CanEditReservedSlotsList;

                this.chkAllowConnectionLogin.Checked = value.CanLogin;

                // 0.5.4.0 additions
                this.chkEditMapZones.Checked = value.CanEditMapZones;

                // 0.6.0.0
                this.chkEditTextModerationList.Checked = value.CanEditTextChatModerationList;
                this.chkCanShutdownServer.Checked = value.CanShutdownServer;

                this.pnlSubPrivileges.AutoScrollPosition = new Point(0, 0);
            }

        }


    }
}
