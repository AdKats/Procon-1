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

using PRoCon.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PRoCon.Forms
{
    using Core;
    using Core.Accounts;
    using Core.Remote;

    public partial class frmManageAccounts : Form
    {

        private frmMain m_frmMainWindow = null;
        private CLocalization m_clocLanguage = null;

        private const int INT_DELETING_ACCOUNT = 1;
        private const int INT_REMOVING_PRIVILEGES = 2;
        private const int INT_CHANGING_PASSWORD = 3;
        private const int INT_CHANGING_PRIVILEGES = 4;

        private int m_iConfirmationAction = 0;

        //public delegate void AccountDelegate(object sender, AccountEventArgs e);
        //public event AccountDelegate AccountCreated;
        //public event AccountDelegate AccountDeleted;
        //public event AccountDelegate AccountPrivilegesChanged;
        //public event AccountDelegate AccountPasswordChanged;
        //public event AccountDelegate AccountPrivilegesCleared;

        /*
        public List<string[]> UserList {
            get {

                List<string[]> lstReturnList = new List<string[]>();

                foreach (ListViewItem lviUser in this.lstAccounts.Items) {
                    lstReturnList.Add(new string[] { lviUser.Text, lviUser.Tag.ToString() });
                }

                return lstReturnList;
            }
        }
        */
        private PRoConApplication m_paProcon;

        public frmManageAccounts(PRoConApplication paProcon, frmMain frmMainWindow)
        {
            InitializeComponent();

            this.m_paProcon = paProcon;
            this.m_paProcon.AccountsList.AccountAdded += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
            this.m_paProcon.AccountsList.AccountRemoved += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);

            this.m_frmMainWindow = frmMainWindow;

            Rectangle recWindow = new Rectangle();
            recWindow.Location = new Point(0, 0);
            recWindow.Height = 540;
            recWindow.Width = 600;
            this.Bounds = recWindow;

            Rectangle recPanels = new Rectangle();
            recPanels.Location = new Point(0, 0);
            recPanels.Height = 500;
            recPanels.Width = 600;
            this.pnlChooseAccount.Bounds = this.pnlCreateAccount.Bounds = this.pnlAlterPrivileges.Bounds = this.pnlConfirmation.Bounds = this.pnlEditingUser.Bounds = recPanels;

            this.uscSetPrivileges.OnCancelPrivileges += new uscPrivilegesSelection.OnCancelPrivilegesDelegate(uscSetPrivileges_OnCancelPrivileges);
            this.uscSetPrivileges.OnUpdatePrivileges += new uscPrivilegesSelection.OnUpdatePrivilegesDelegate(uscSetPrivileges_OnUpdatePrivileges);

            this.picDeleteAccount.Image = this.m_frmMainWindow.iglIcons.Images["cross.png"];
            this.picCreateNewAccount.Image = this.m_frmMainWindow.iglIcons.Images["new.png"];
            this.picRemovePrivileges.Image = this.m_frmMainWindow.iglIcons.Images["key_delete.png"];
            this.picEditGlobalPrivileges.Image = this.m_frmMainWindow.iglIcons.Images["key.png"];
        }

        private void frmManageAccounts_Load(object sender, EventArgs e)
        {
            this.lstAccounts.Items.Clear();

            foreach (Account accAccount in this.m_paProcon.AccountsList)
            {
                this.AccountsList_AccountAdded(accAccount);
            }
        }

        void uscSetPrivileges_OnUpdatePrivileges(string strAccountName, CPrivileges spUpdatedPrivs)
        {
            this.ShowPanel(this.pnlEditingUser);

            if (this.m_paProcon.AccountsList.Contains(strAccountName) == true)
            {

                foreach (PRoConClient prcClient in this.m_paProcon.Connections)
                {
                    if (prcClient.Layer.AccountPrivileges.Contains(strAccountName) == true)
                    {
                        prcClient.Layer.AccountPrivileges[strAccountName].SetPrivileges(spUpdatedPrivs);
                    }
                }
            }

            //if (this.AccountPrivilegesChanged != null) {
            //    this.AccountPrivilegesChanged(this, new AccountEventArgs(strAccountName, spUpdatedPrivs, ""));
            //}
        }

        void uscSetPrivileges_OnCancelPrivileges()
        {
            this.ShowPanel(this.pnlEditingUser);
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            this.uscSetPrivileges.SetLocalization(this.m_clocLanguage);

            this.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.Title", null);

            // Choose account windows
            this.lblSelectAccountTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblSelectAccountTitle", null);
            this.lnkCreateNewAccount.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkCreateNewAccount", null);
            //this.lnkCreateNewAccount.LinkArea = new LinkArea(0,this.lnkCreateNewAccount.Text.Length);

            // Create new account window
            this.lblCreateNewAccountTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblCreateNewAccountTitle", null);
            this.lblUsername.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblUsername", null);
            this.lblPassword.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblPassword", null);
            this.lblUserNameExistsError.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblUserNameExistsError", null);
            this.btnCreateAccount.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.btnCreateAccount", null);
            this.btnCancelNewAccount.Text = this.m_clocLanguage.GetLocalized("global.cancel", null);

            // Editing account window
            this.lblChangePassword.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblChangePassword", null);
            this.lnkUpdatePassword.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkUpdatePassword", null);
            //this.lnkUpdatePassword.LinkArea = new LinkArea(0, this.lnkUpdatePassword.Text.Length);

            this.lnkSetGlobalPrivileges.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkSetGlobalPrivileges", null);
            this.tltipExtraInfo.SetToolTip(this.lnkSetGlobalPrivileges, this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkSetGlobalPrivileges.ToolTip", null));
            this.lnkRemovePrivileges.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkRemovePrivileges", null);
            //this.lnkRemovePrivileges.LinkArea = new LinkArea(0, this.lnkRemovePrivileges.Text.Length);
            this.tltipExtraInfo.SetToolTip(this.lnkRemovePrivileges, this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkRemovePrivileges.ToolTip", null));
            this.lnkDeleteUser.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkDeleteUser", null);
            //this.lnkDeleteUser.LinkArea = new LinkArea(0, this.lnkDeleteUser.Text.Length);
            this.tltipExtraInfo.SetToolTip(this.lnkDeleteUser, this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkDeleteUser.ToolTip", null));
            this.lnkEditAnotherAccount.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lnkEditAnotherAccount", null);
            //this.lnkEditAnotherAccount.LinkArea = new LinkArea(0, this.lnkEditAnotherAccount.Text.Length);

            // Confirmation window
            this.btnCancel.Text = this.m_clocLanguage.GetLocalized("global.cancel", null);

            this.btnClose.Text = this.m_clocLanguage.GetLocalized("global.close", null);
        }

        // TO DO: implement this so it changes the icon.. when i finally get icons from people =)
        public void OnAccountConnected(string strUsername)
        {

        }

        public void OnAccountDisconnected(string strUsername)
        {

        }

        private void AccountsList_AccountAdded(Account item)
        {
            if (this.lstAccounts.Items.ContainsKey(item.Name) == false)
            {
                ListViewItem lviNewAccount = new ListViewItem(item.Name);
                lviNewAccount.Tag = item.Password;
                lviNewAccount.Name = item.Name;
                lviNewAccount.ImageIndex = 0;

                this.lstAccounts.Items.Add(lviNewAccount);
            }
        }

        public void CreateNewAccount(string strUsername, string strPassword)
        {
            this.m_paProcon.AccountsList.CreateAccount(strUsername, strPassword);
            /*
            if (this.lstAccounts.Items.ContainsKey(strUsername) == false) {
                ListViewItem lviNewAccount = new ListViewItem(strUsername);
                lviNewAccount.Tag = strPassword;
                lviNewAccount.Name = strUsername;
                lviNewAccount.ImageIndex = 0;

                this.lstAccounts.Items.Add(lviNewAccount);

                // Throw event
                this.AccountCreated(this, new AccountEventArgs(strUsername, default(CPrivileges), strPassword));
            }
            */
        }

        private void AccountsList_AccountRemoved(Account item)
        {
            if (this.lstAccounts.Items.ContainsKey(item.Name) == true)
            {
                this.lstAccounts.Items.Remove(this.lstAccounts.Items[item.Name]);
            }
        }

        public void DeleteAccount(string strUsername)
        {

            if (this.m_paProcon.AccountsList.Contains(strUsername) == true)
            {
                this.m_paProcon.AccountsList.Remove(strUsername);
            }
            /*
            if (this.lstAccounts.Items.ContainsKey(strUsername) == true) {
                this.lstAccounts.Items.Remove(this.lstAccounts.Items[strUsername]);

                // Throw event
                this.AccountDeleted(this, new AccountEventArgs(strUsername, default(CPrivileges), ""));
            }
            */
        }

        public void ChangePassword(string strUsername, string strPassword)
        {

            if (this.m_paProcon.AccountsList.Contains(strUsername) == true)
            {
                this.m_paProcon.AccountsList[strUsername].Password = strPassword;
            }

            /*
            if (this.lstAccounts.Items.ContainsKey(strUsername) == true) {
                this.lstAccounts.Items[strUsername].Tag = strPassword;

                // Throw event
                this.AccountPasswordChanged(this, new AccountEventArgs(strUsername, default(CPrivileges), strPassword));
            }
            */
        }

        public void RemoveAllPrivileges(string strUsername)
        {

            foreach (PRoConClient prcClient in this.m_paProcon.Connections)
            {
                if (prcClient.Layer.AccountPrivileges.Contains(strUsername) == true)
                {
                    prcClient.Layer.AccountPrivileges[strUsername].SetPrivileges(default(CPrivileges));
                }
            }

            //if (this.m_paProcon.AccountsList.Contains(strUsername) == true) {
            //    foreach (AccountPrivilege apPrivileges in this.m_paProcon.AccountsList[strUsername].AccountPrivileges) {
            //        apPrivileges.SetPrivileges(default(CPrivileges));
            //    }
            //}

            /*
            if (this.lstAccounts.Items.ContainsKey(strUsername) == true) {

                // Throw event
                this.AccountPrivilegesCleared(this, new AccountEventArgs(strUsername, default(CPrivileges), ""));
            }
            */
        }

        private void ShowPanel(Panel pnlToShow)
        {
            this.pnlChooseAccount.Visible = false;
            this.pnlCreateAccount.Visible = false;
            this.pnlEditingUser.Visible = false;
            this.pnlConfirmation.Visible = false;
            this.pnlAlterPrivileges.Visible = false;

            if (pnlToShow == this.pnlChooseAccount)
            {
                this.lstAccounts.SelectedItems.Clear();
            }

            pnlToShow.Visible = true;
        }

        #region Create New Account

        private void lnkAddNewAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            this.txtUsername.Text = String.Empty;
            this.txtPassword.Text = String.Empty;

            this.ShowPanel(this.pnlCreateAccount);
        }

        private void ValidateCreateNewUser()
        {

            bool blUsernameExists = false;

            foreach (ListViewItem lviAccount in this.lstAccounts.Items)
            {
                if (lviAccount.Text.CompareTo(this.txtUsername.Text) == 0)
                {
                    blUsernameExists = true;
                    break;
                }
            }

            this.lblUserNameExistsError.Visible = true & blUsernameExists;

            // If the username is not blank AND the username is valid AND they have input a password
            if (this.txtUsername.Text.CompareTo(String.Empty) != 0 && blUsernameExists == false && this.txtPassword.Text.CompareTo(String.Empty) != 0)
            {
                this.btnCreateAccount.Enabled = true;
            }
            else
            {
                this.btnCreateAccount.Enabled = false;
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            this.ValidateCreateNewUser();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.ValidateCreateNewUser();
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            this.CreateNewAccount(this.txtUsername.Text, this.txtPassword.Text);

            this.ShowPanel(this.pnlChooseAccount);
        }

        private void btnCancelNewAccount_Click(object sender, EventArgs e)
        {
            this.ShowPanel(this.pnlChooseAccount);
        }

        #endregion

        #region Edit account

        private void lstAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.lstAccounts.SelectedItems.Count > 0)
            {

                this.lblEditingAccountTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblEditingAccountTitle", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.txtChangePassword.Text = String.Empty;

                this.ShowPanel(this.pnlEditingUser);
            }
        }

        private void lnkEditAnotherAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            this.ShowPanel(this.pnlChooseAccount);
        }

        private void txtChangePassword_TextChanged(object sender, EventArgs e)
        {

            if (this.txtChangePassword.Text.CompareTo(String.Empty) != 0)
            {
                this.lnkUpdatePassword.Enabled = true;
            }
            else
            {
                this.lnkUpdatePassword.Enabled = false;
            }
        }

        private void lnkUpdatePassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.lstAccounts.SelectedItems.Count > 0)
            {

                this.lblConfirmationTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationTitle.UpdatePassword", null);

                this.lblExtraInformation.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblExtraInformation.UpdatePassword", new string[] { this.lstAccounts.SelectedItems[0].Text });
                this.lblConfirmationQuestion.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationQuestion.UpdatePassword", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.btnConfirm.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.btnConfirm.UpdatePassword", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.m_iConfirmationAction = frmManageAccounts.INT_CHANGING_PASSWORD;
                this.ShowPanel(this.pnlConfirmation);
            }
        }

        private void lnkDeleteUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this.lstAccounts.SelectedItems.Count > 0)
            {
                this.lblConfirmationTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationTitle.DeleteAccount", null);

                this.lblExtraInformation.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblExtraInformation.DeleteAccount", new string[] { this.lstAccounts.SelectedItems[0].Text });
                this.lblConfirmationQuestion.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationQuestion.DeleteAccount", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.btnConfirm.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.btnConfirm.DeleteAccount", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.m_iConfirmationAction = frmManageAccounts.INT_DELETING_ACCOUNT;
                this.ShowPanel(this.pnlConfirmation);
            }
        }

        private void lnkRemovePrivileges_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this.lstAccounts.SelectedItems.Count > 0)
            {

                this.lblConfirmationTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationTitle.RemovePrivileges", null);

                this.lblExtraInformation.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblExtraInformation.RemovePrivileges", new string[] { this.lstAccounts.SelectedItems[0].Text });
                this.lblConfirmationQuestion.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationQuestion.RemovePrivileges", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.btnConfirm.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.btnConfirm.RemovePrivileges", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.m_iConfirmationAction = frmManageAccounts.INT_REMOVING_PRIVILEGES;
                this.ShowPanel(this.pnlConfirmation);
            }

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {

            if (this.lstAccounts.SelectedItems.Count > 0)
            {

                if (this.m_iConfirmationAction == frmManageAccounts.INT_DELETING_ACCOUNT)
                {
                    this.DeleteAccount(this.lstAccounts.SelectedItems[0].Text);

                    this.ShowPanel(this.pnlChooseAccount);
                }
                else if (this.m_iConfirmationAction == frmManageAccounts.INT_CHANGING_PASSWORD)
                {
                    this.ChangePassword(this.lstAccounts.SelectedItems[0].Text, this.txtChangePassword.Text);

                    this.ShowPanel(this.pnlEditingUser);
                }
                else if (this.m_iConfirmationAction == frmManageAccounts.INT_REMOVING_PRIVILEGES)
                {

                    this.RemoveAllPrivileges(this.lstAccounts.SelectedItems[0].Text);

                    this.ShowPanel(this.pnlEditingUser);
                }
                else if (this.m_iConfirmationAction == frmManageAccounts.INT_CHANGING_PRIVILEGES)
                {
                    this.uscSetPrivileges.AccountName = this.lstAccounts.SelectedItems[0].Text;
                    this.uscSetPrivileges.Privileges = this.CollectLowestPrivileges(this.lstAccounts.SelectedItems[0].Text);

                    this.ShowPanel(this.pnlAlterPrivileges);
                }
            }
        }

        public CPrivileges CollectLowestPrivileges(string strAccountName)
        {

            CPrivileges spLowestPrivileges = new CPrivileges();

            if (this.m_paProcon.AccountsList.Contains(strAccountName) == true)
            {
                spLowestPrivileges.PrivilegesFlags = CPrivileges.FullPrivilegesFlags;

                foreach (PRoConClient prcClient in this.m_paProcon.Connections)
                {
                    if (prcClient.Layer != null && prcClient.Layer.AccountPrivileges.Contains(strAccountName) == true)
                    {
                        spLowestPrivileges.SetLowestPrivileges(prcClient.Layer.AccountPrivileges[strAccountName].Privileges);
                    }
                }


                //if (this.m_paProcon.AccountsList.Contains(strAccountName) == true) {
                //    foreach (AccountPrivilege apPrivilege in this.m_paProcon.AccountsList[strAccountName].AccountPrivileges) {

                //    }
                //}
            }

            return spLowestPrivileges;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.ShowPanel(this.pnlEditingUser);
        }

        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmManageAccounts_Shown(object sender, EventArgs e)
        {
            this.ShowPanel(this.pnlChooseAccount);
        }

        private void lnkAlterGlobalPrivileges_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (this.lstAccounts.SelectedItems.Count > 0)
            {

                this.lblConfirmationTitle.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationTitle.AssignPrivileges", null);

                this.lblExtraInformation.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblExtraInformation.AssignPrivileges", null);
                this.lblConfirmationQuestion.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.lblConfirmationQuestion.AssignPrivileges", new string[] { this.lstAccounts.SelectedItems[0].Text });

                this.btnConfirm.Text = this.m_clocLanguage.GetLocalized("frmManageAccounts.btnConfirm.AssignPrivileges", null);

                this.m_iConfirmationAction = frmManageAccounts.INT_CHANGING_PRIVILEGES;
                this.ShowPanel(this.pnlConfirmation);
            }
        }

    }
}
