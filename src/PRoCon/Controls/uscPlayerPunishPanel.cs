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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    public partial class uscPlayerPunishPanel : UserControl
    {

        /*
            Minutes * 1
            Hours * 60
            Days * 24 * 60 = 1440
            Weeks * 7 * 24 * 60 = 10080
            Months * 4 * 7 * 24 * 60 = 40320
            Years * 12 * 4 * 7 * 24 * 60 = 483840
         */
        private uscServerConnection m_uscConnectionPanel = null;
        private CLocalization m_clocLanguage = null;
        private string[] ma_strTimeDescriptionsLong = new string[] { " year ", " years ", " month ", " months ", " week ", " weeks ", " day ", " days ", " hour ", " hours ", " minute ", " minutes ", " second", " seconds" };
        private string[] ma_strTimeDescriptionsShort = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };

        private PRoConClient m_prcClient;

        //public delegate void PunishPlayerDelegate(string strBuildPunishPacket);
        //public event PunishPlayerDelegate PunishPlayer;

        public delegate void PunishPlayerDelegate(List<string> lstWordsPunishPacket);
        public event PunishPlayerDelegate PunishPlayer;

        private CPrivileges m_spPrivileges;

        private string m_strSoldierName = String.Empty;
        public string SoldierName
        {
            get
            {
                return this.m_strSoldierName;
            }
            set
            {
                this.m_strSoldierName = value;
            }
        }

        private string m_strSlotID = String.Empty;
        public string SlotID
        {
            get
            {
                return this.m_strSlotID;
            }
            set
            {
                this.m_strSlotID = value;
            }
        }

        private string m_strIP = String.Empty;
        public string IP
        {
            get
            {
                return this.m_strIP;
            }
            set
            {
                this.m_strIP = value;
            }
        }

        private string m_strGUID = String.Empty;
        public string GUID
        {
            get
            {
                return this.m_strGUID;
            }
            set
            {
                this.m_strGUID = value;
            }
        }

        public ComboBox.ObjectCollection Reasons
        {
            get
            {
                return this.cboReason.Items;
            }
        }

        private bool m_blPunkbuster = false;
        public bool Punkbuster
        {
            set
            {
                //this.cboReason.Enabled = value;
                //this.lblReason.Enabled = value;

                //this.rdoPunishOnName.Enabled = !value;
                /*
                this.rdoPunishOnIP.Enabled = !value;

                if (value) {
                    this.rdoPunishOnGUID.Enabled = value;
                    this.rdoPunishOnGUID.Checked = value;
                }
                else {
                    this.rdoPunishOnName.Checked = value;
                }
                */
                this.rdoPunishOnName.Checked = value;
                this.m_blPunkbuster = value;
            }
        }

        public uscPlayerPunishPanel()
        {
            InitializeComponent();

            this.cboTimeMultiplier.SelectedIndex = 0;

            this.m_spPrivileges = new CPrivileges();
            this.m_spPrivileges.PrivilegesFlags = CPrivileges.FullPrivilegesFlags;

        }

        public void SetConnection(PRoConClient prcClient)
        {
            if ((this.m_prcClient = prcClient) != null)
            {

            }
        }

        public void Initialize(uscServerConnection uscConnectionPanel)
        {
            this.m_uscConnectionPanel = uscConnectionPanel;
        }
        /*
        private int GetTempBanCeiling() {
            int iReturn = 3600;

            if (this.m_uscConnectionPanel != null && this.m_uscConnectionPanel.SV_Variables.ContainsKey("TEMP_BAN_CEILING") == true) {
                if (int.TryParse(this.m_uscConnectionPanel.SV_Variables["TEMP_BAN_CEILING"], out iReturn) == false) {
                    iReturn = 3600;
                }
            }
            // else return 3600

            return iReturn;
        }
        */
        public void SetPrivileges(CPrivileges spPrivileges)
        {

            this.m_spPrivileges = spPrivileges;

            //this.Enabled = !this.m_spPrivileges.CannotPunishPlayers;

            this.rdoKick.Enabled = this.m_spPrivileges.CanKickPlayers;
            this.rdoTemporaryBan.Enabled = this.m_spPrivileges.CanTemporaryBanPlayers;
            this.rdoPermanentlyBan.Enabled = this.m_spPrivileges.CanPermanentlyBanPlayers;

            if (this.rdoPermanentlyBan.Enabled == false && this.rdoPermanentlyBan.Checked == true)
            {
                this.rdoTemporaryBan.Checked = true;
            }

            if (this.rdoTemporaryBan.Enabled == false && this.rdoTemporaryBan.Checked == true)
            {
                this.rdoKick.Checked = true;
            }

            if (this.rdoKick.Enabled == false && this.rdoKick.Checked == true)
            {
                this.rdoKill.Checked = true;
            }

            if (this.rdoKill.Enabled == false && this.rdoKill.Checked == true)
            {
                this.rdoKill.Checked = false;
            }

            if (this.rdoKill.Checked == false && this.rdoKick.Checked == false && this.rdoTemporaryBan.Checked == false && this.rdoPermanentlyBan.Checked == false)
            {
                this.rdoKill.Checked = true;
            }
        }

        public void SetLocalization(CLocalization clocLanguage)
        {
            this.m_clocLanguage = clocLanguage;

            // private string[] m_astrTimeDescriptionsLong = new string[] { " year ", " years ", " month ", " months ", " week ", " weeks ", " day ", " days ", " hour ", " hours ", " minute ", " minutes ", " second", " seconds" };
            this.ma_strTimeDescriptionsLong[13] = " " + this.m_clocLanguage.GetLocalized("global.Seconds.Plural", null).ToLower();
            this.ma_strTimeDescriptionsLong[12] = " " + this.m_clocLanguage.GetLocalized("global.Seconds.Singular", null).ToLower();
            this.ma_strTimeDescriptionsLong[11] = " " + this.m_clocLanguage.GetLocalized("global.Minutes.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[10] = " " + this.m_clocLanguage.GetLocalized("global.Minutes.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[9] = " " + this.m_clocLanguage.GetLocalized("global.Hours.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[8] = " " + this.m_clocLanguage.GetLocalized("global.Hours.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[7] = " " + this.m_clocLanguage.GetLocalized("global.Days.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[6] = " " + this.m_clocLanguage.GetLocalized("global.Days.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[5] = " " + this.m_clocLanguage.GetLocalized("global.Weeks.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[4] = " " + this.m_clocLanguage.GetLocalized("global.Weeks.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[3] = " " + this.m_clocLanguage.GetLocalized("global.Months.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[2] = " " + this.m_clocLanguage.GetLocalized("global.Months.Singular", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[1] = " " + this.m_clocLanguage.GetLocalized("global.Years.Plural", null).ToLower() + " ";
            this.ma_strTimeDescriptionsLong[0] = " " + this.m_clocLanguage.GetLocalized("global.Years.Singular", null).ToLower() + " ";

            this.cboTimeMultiplier.Items[0] = this.m_clocLanguage.GetLocalized("global.Minutes.Plural", null);
            this.cboTimeMultiplier.Items[1] = this.m_clocLanguage.GetLocalized("global.Hours.Plural", null);
            this.cboTimeMultiplier.Items[2] = this.m_clocLanguage.GetLocalized("global.Days.Plural", null);
            this.cboTimeMultiplier.Items[3] = this.m_clocLanguage.GetLocalized("global.Weeks.Plural", null);
            this.cboTimeMultiplier.Items[4] = this.m_clocLanguage.GetLocalized("global.Months.Plural", null);

            // private string[] m_astrTimeDescriptionsShort = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };
            this.ma_strTimeDescriptionsShort[13] = this.m_clocLanguage.GetLocalized("global.Seconds.Short", null);
            this.ma_strTimeDescriptionsShort[12] = this.m_clocLanguage.GetLocalized("global.Seconds.Short", null);
            this.ma_strTimeDescriptionsShort[11] = this.m_clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[10] = this.m_clocLanguage.GetLocalized("global.Minutes.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[9] = this.m_clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[8] = this.m_clocLanguage.GetLocalized("global.Hours.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[7] = this.m_clocLanguage.GetLocalized("global.Days.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[6] = this.m_clocLanguage.GetLocalized("global.Days.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[5] = this.m_clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[4] = this.m_clocLanguage.GetLocalized("global.Weeks.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[3] = this.m_clocLanguage.GetLocalized("global.Months.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[2] = this.m_clocLanguage.GetLocalized("global.Months.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[1] = this.m_clocLanguage.GetLocalized("global.Years.Short", null) + " ";
            this.ma_strTimeDescriptionsShort[0] = this.m_clocLanguage.GetLocalized("global.Years.Short", null) + " ";

            this.rdoKill.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoKill", null);
            this.rdoKick.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoKick", null);
            this.rdoPermanentlyBan.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoPermanentlyBan", null);
            this.rdoTemporaryBan.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoTemporaryBan", null);
            this.lblTime.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblTime", null);
            this.lblReason.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblReason", null);
            this.btnPunish.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.btnPunish", null);
            this.rdoPunishOnName.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoPunishOnName", null);
            this.rdoPunishOnIP.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoPunishOnIP", null);
            this.rdoPunishOnGUID.Text = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.rdoPunishOnGUID", null);

            this.UpdateConfirmationLabel();
        }

        public void RefreshPanel()
        {
            if (this.rdoKill.Checked == false && this.rdoKick.Checked == false && this.rdoTemporaryBan.Checked == false && this.rdoPermanentlyBan.Checked == false)
            {
                this.rdoKill.Checked = true;
            }

            this.UpdateConfirmationLabel();
        }

        public static string SecondsToText(UInt32 iSeconds, string[] a_strTimeDescriptions)
        {
            string strReturn = String.Empty;

            double dblSeconds = iSeconds;
            double dblMinutes = (iSeconds / 60);
            double dblHours = (dblMinutes / 60);
            double dblDays = (dblHours / 24);
            double dblWeeks = (dblDays / 7);
            double dblMonths = (dblWeeks / 4);
            double dblYears = (dblMonths / 12);

            if ((Int32)dblYears > 0)
            {
                strReturn += String.Empty + ((Int32)dblYears) + (((Int32)dblYears) == 1 ? a_strTimeDescriptions[0] : a_strTimeDescriptions[1]);
            }
            if ((Int32)dblMonths % 12 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMonths) % 12 + (((Int32)dblMonths % 12) == 1 ? a_strTimeDescriptions[2] : a_strTimeDescriptions[3]);
            }
            if ((Int32)dblWeeks % 4 > 0)
            {
                strReturn += String.Empty + ((Int32)dblWeeks) % 4 + (((Int32)dblWeeks % 4) == 1 ? a_strTimeDescriptions[4] : a_strTimeDescriptions[5]);
            }
            if ((Int32)dblDays % 7 > 0)
            {
                strReturn += String.Empty + ((Int32)dblDays) % 7 + (((Int32)dblDays % 7) == 1 ? a_strTimeDescriptions[6] : a_strTimeDescriptions[7]);
            }
            if ((Int32)dblHours % 24 > 0)
            {
                strReturn += String.Empty + ((Int32)dblHours) % 24 + (((Int32)dblHours % 24) == 1 ? a_strTimeDescriptions[8] : a_strTimeDescriptions[9]);
            }
            if ((Int32)dblMinutes % 60 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMinutes) % 60 + (((Int32)dblMinutes % 60) == 1 ? a_strTimeDescriptions[10] : a_strTimeDescriptions[11]);
            }
            if ((Int32)dblSeconds % 60 > 0)
            {
                strReturn += String.Empty + ((Int32)dblSeconds) % 60 + (((Int32)dblSeconds % 60) == 1 ? a_strTimeDescriptions[12] : a_strTimeDescriptions[13]);
            }

            return strReturn;
        }

        public static int GetBanLength(TextBox txtSeconds, ComboBox cboTimeMultiplier)
        {
            int iReturn = 0;
            int[] a_iMultiplier = new int[] { 1, 60, 1440, 10080, 40320 };

            if (cboTimeMultiplier.SelectedIndex >= 0)
            {

                double dblAmount = 0.0;

                if (Double.TryParse(txtSeconds.Text, out dblAmount) == true)
                {
                    dblAmount *= a_iMultiplier[cboTimeMultiplier.SelectedIndex];

                    iReturn = (int)(dblAmount);
                }
            }

            return iReturn;
        }

        public static string GetBanLength(TextBox txtSeconds, ComboBox cboTimeMultiplier, string[] a_strTimeDescriptionsLong)
        {

            string strLabel = String.Empty;
            int[] a_iMultiplier = new int[] { 1, 60, 1440, 10080, 40320 };

            if (cboTimeMultiplier.SelectedIndex >= 0)
            {

                double dblAmount = 0.0;

                if (Double.TryParse(txtSeconds.Text, out dblAmount) == true)
                {
                    dblAmount *= a_iMultiplier[cboTimeMultiplier.SelectedIndex];

                    strLabel = uscPlayerPunishPanel.SecondsToText(((UInt32)dblAmount) * 60, a_strTimeDescriptionsLong).TrimEnd(null);
                }
            }

            return strLabel;
        }

        public static string GetConfirmationLabel(out bool blAbleToPunish, string strBanDescription, CPrivileges cpPrivileges, CLocalization clocLanguage, bool blKill, bool blKick, bool blPerm, bool blTemp, TextBox txtTime, ComboBox cboTimeMultiplier, string[] a_strTimeDescriptionsLong, int iTempBanCeiling)
        {

            string strLabel = String.Empty;

            blAbleToPunish = false;

            if (clocLanguage != null)
            {

                if (blKill == true)
                {
                    strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.Kill", new string[] { strBanDescription });
                    blAbleToPunish = true && (cpPrivileges.CanKillPlayers == true);
                }
                else if (blKick == true)
                {
                    strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.Kick", new string[] { strBanDescription });
                    blAbleToPunish = true && (cpPrivileges.CanKickPlayers == true);
                }
                else if (blPerm == true)
                {
                    strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.PermanentBan", new string[] { strBanDescription });
                    blAbleToPunish = true && (cpPrivileges.CanPermanentlyBanPlayers == true);
                }
                else if (blTemp == true)
                {

                    string strBanLength = uscPlayerPunishPanel.GetBanLength(txtTime, cboTimeMultiplier, a_strTimeDescriptionsLong);

                    if (strBanLength.Length > 0)
                    {
                        // strLabel += " for " + strBanLength;

                        if (cpPrivileges.CanTemporaryBanPlayers == true && cpPrivileges.CanPermanentlyBanPlayers == false && (uscPlayerPunishPanel.GetBanLength(txtTime, cboTimeMultiplier) * 60) > iTempBanCeiling)
                        {
                            strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan.ToLong", new string[] { uscPlayerPunishPanel.SecondsToText((UInt32)(iTempBanCeiling), a_strTimeDescriptionsLong) });
                            blAbleToPunish = false;
                        }
                        else
                        {
                            strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan", new string[] { strBanDescription, clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan.Time", new string[] { strBanLength }) });
                            blAbleToPunish = true && (cpPrivileges.CanTemporaryBanPlayers == true);
                        }
                    }
                    else
                    {
                        strLabel = clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan", new string[] { strBanDescription, String.Empty });
                        blAbleToPunish = false;
                    }
                }

                strLabel += ":";
            }

            return strLabel;
        }

        private void UpdateConfirmationLabel()
        {
            /*
            string strLabel = String.Empty;

            if (this.m_clocLanguage != null) {
                
                if (this.rdoKick.Checked == true) {
                    strLabel = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.Kick", new string[] { this.m_strSoldierName });
                    this.btnPunish.Enabled = true && (this.m_spPrivileges.CanKickPlayers == true);
                }
                else if (this.rdoPermanentlyBan.Checked == true) {
                    strLabel = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.PermanentBan", new string[] { this.m_strSoldierName });
                    this.btnPunish.Enabled = true && (this.m_spPrivileges.CanPermanentlyBanPlayers == true);
                }
                else if (this.rdoTemporaryBan.Checked == true) {

                    string strBanLength = this.GetBanLength(this.txtTime, this.cboTimeMultiplier);

                    if (strBanLength.Length > 0) {
                        // strLabel += " for " + strBanLength;
                        
                        if (this.m_spPrivileges.CanTemporaryBanPlayers == true && this.m_spPrivileges.CanPermanentlyBanPlayers == false && (this.GetBanLength() * 60) > this.GetTempBanCeiling()) {
                            strLabel = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan.ToLong", new string[] { this.SecondsToText((UInt32)(this.GetTempBanCeiling()), this.m_astrTimeDescriptionsLong) });
                            this.btnPunish.Enabled = false;
                        }
                        else {
                            strLabel = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan", new string[] { this.m_strSoldierName, this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan.Time", new string[] { strBanLength }) });
                            this.btnPunish.Enabled = true && (this.m_spPrivileges.CanTemporaryBanPlayers == true);
                        }
                    }
                    else {
                        strLabel = this.m_clocLanguage.GetLocalized("uscPlayerPunishPanel.lblConfirmation.TemporaryBan", new string[] { this.m_strSoldierName, "" });
                        this.btnPunish.Enabled = false;
                    }
                }

                strLabel += ":";
            }
            */
            bool blAbleToPunish = false;

            if (this.m_uscConnectionPanel != null && this.m_prcClient != null)
            {
                this.lblConfirmation.Text = uscPlayerPunishPanel.GetConfirmationLabel(out blAbleToPunish, this.m_strSoldierName, this.m_spPrivileges,
                                                                                      this.m_clocLanguage, this.rdoKill.Checked, this.rdoKick.Checked, this.rdoPermanentlyBan.Checked,
                                                                                      this.rdoTemporaryBan.Checked, this.txtTime, this.cboTimeMultiplier,
                                                                                      this.ma_strTimeDescriptionsLong, this.m_prcClient.SV_Variables.GetVariable<int>("TEMP_BAN_CEILING", 3600));
                this.btnPunish.Enabled = blAbleToPunish;
            }

            if (this.btnPunish.Enabled == false)
            {
                this.lblConfirmation.ForeColor = Color.Maroon;
            }
            else
            {
                this.lblConfirmation.ForeColor = Color.Black;
            }
        }

        private void rdoKill_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlTime.Enabled = false;

            this.rdoPunishOnName.Enabled = true; //& this.m_blPunkbuster;
            this.rdoPunishOnName.Checked = true;
            this.rdoPunishOnIP.Enabled = false;
            this.rdoPunishOnGUID.Enabled = false;

            this.UpdateConfirmationLabel();
        }

        private void rdoKick_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlTime.Enabled = false;

            this.rdoPunishOnName.Enabled = true; //& this.m_blPunkbuster;
            this.rdoPunishOnName.Checked = true;
            this.rdoPunishOnIP.Enabled = false;
            this.rdoPunishOnGUID.Enabled = false;

            this.UpdateConfirmationLabel();
        }

        private void rdoPermanentlyBan_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlTime.Enabled = false;

            this.rdoPunishOnName.Enabled = true & !this.m_blPunkbuster;
            this.rdoPunishOnIP.Enabled = true & !this.m_blPunkbuster;
            this.rdoPunishOnGUID.Enabled = true;// &!this.m_blPunkbuster;

            if (this.m_blPunkbuster == true)
            {
                this.rdoPunishOnGUID.Checked = true;
            }

            this.UpdateConfirmationLabel();
        }

        private void rdoTemporaryBan_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlTime.Enabled = true;

            //this.rdoPunishOnName.Enabled = true & !this.m_blPunkbuster;
            this.rdoPunishOnIP.Enabled = true & !this.m_blPunkbuster;
            this.rdoPunishOnName.Enabled = true;
            this.rdoPunishOnGUID.Enabled = true & !this.m_blPunkbuster;

            if (this.m_blPunkbuster == true)
            {
                this.rdoPunishOnName.Checked = true;
            }

            this.UpdateConfirmationLabel();
        }

        private void cboTimeMultiplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateConfirmationLabel();
        }

        private void txtTime_TextChanged(object sender, EventArgs e)
        {
            double dblAmount = 0.0;

            if (Double.TryParse(this.txtTime.Text, out dblAmount) == false)
            {
                this.txtTime.Text = String.Empty;
            }

            this.UpdateConfirmationLabel();
        }

        private void txtTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b');
        }

        private void btnPunish_Click(object sender, EventArgs e)
        {

            List<string> lstWords = new List<string>();
            // banList.add <id-type: id-type> <id: string> <timeout: timeout> <reason: string>

            //obsolet with BF-3 R-8 string name = this.m_prcClient.Game is BF3Client ? "persona" : "name";
            string name = "name";

            string m_strReasonAdmin = this.cboReason.Text;
            m_strReasonAdmin = m_strReasonAdmin.Replace("%player_name%", this.m_strSoldierName);

            string accountName = this.m_prcClient.Username;
            if (Program.ProconApplication.OptionsSettings.EnableAdminReason && accountName.Length > 0)
            {
                int iBanInfo = (80 - 5 - (accountName.Length + 3));
                if (m_strReasonAdmin.Length > iBanInfo)
                {
                    m_strReasonAdmin = m_strReasonAdmin.Substring(0, iBanInfo);
                }
                m_strReasonAdmin = m_strReasonAdmin + " (" + accountName + ")";
            }

            if (this.rdoKill.Checked == true && this.PunishPlayer != null)
            {

                if (this.cboReason.Text.Length > 0)
                {
                    this.PunishPlayer(new List<string>() { "admin.say", String.Format("You have been killed by an admin for {0}", this.cboReason.Text), "player", this.m_strSoldierName });
                }
                else
                {
                    this.PunishPlayer(new List<string>() { "admin.say", "You have been killed by an admin", "player", this.m_strSoldierName });
                }

                this.PunishPlayer(new List<string>() { "admin.killPlayer", this.m_strSoldierName });
            }
            else if (this.rdoKick.Checked == true)
            {
                /*
                if (this.rdoPunishOnName.Checked == true) {
                    lstWords.Add("admin.kickPlayer");
                    lstWords.Add(this.m_strSoldierName);
                    lstWords.Add(this.cboReason.Text);

                    //strBuildPacket = String.Format(@"admin.kickPlayer ""{0}""", this.m_strSoldierName);
                }
                else if (this.rdoPunishOnGUID.Checked == true) {
                    lstWords.Add("punkBuster.pb_sv_command");
                    lstWords.Add(String.Format(@"pb_sv_kick {0} 0 ""{1}""", this.m_strSlotID, this.cboReason.Text));

                    //strBuildPacket = String.Format(@"\""pb_sv_kick {0} 0 ""{1}"" ""{2}""\""", this.m_strSlotID, this.cboReason.Text, this.cboReason.Text);
                }
                */
                if (this.m_blPunkbuster == false)
                {
                    lstWords.Add("admin.kickPlayer");
                    lstWords.Add(this.m_strSoldierName);
                    lstWords.Add(this.cboReason.Text);
                }
                else
                {
                    lstWords.Add("punkBuster.pb_sv_command");
                    lstWords.Add(String.Format(@"pb_sv_kick {0} 0 ""{1}""", this.m_strSlotID, "BC2! " + m_strReasonAdmin));
                }

            }
            else if (this.rdoPermanentlyBan.Checked == true)
            {

                if (this.rdoPunishOnName.Checked == true)
                {
                    lstWords.Add("banList.add");
                    lstWords.Add(name);
                    lstWords.Add(this.m_strSoldierName);
                    lstWords.Add("perm");
                    lstWords.Add(m_strReasonAdmin);

                    //strBuildPacket = String.Format(@"admin.banPlayer ""{0}"" perm", this.m_strSoldierName);
                }
                else if (this.rdoPunishOnIP.Checked == true)
                {

                    lstWords.Add("banList.add");
                    lstWords.Add("ip");
                    lstWords.Add(this.m_strIP);
                    lstWords.Add("perm");
                    lstWords.Add(m_strReasonAdmin);

                    //strBuildPacket = String.Format(@"admin.banIP ""{0}"" perm", this.m_strIP);
                }
                else if (this.rdoPunishOnGUID.Checked == true)
                {
                    if (this.m_blPunkbuster == false)
                    {
                        lstWords.Add("banList.add");
                        lstWords.Add("guid");
                        lstWords.Add(this.m_strGUID);
                        lstWords.Add("perm");
                        lstWords.Add(m_strReasonAdmin);
                    }
                    else
                    {
                        lstWords.Add("punkBuster.pb_sv_command");
                        lstWords.Add(String.Format(@"pb_sv_ban {0} ""{1}""", this.m_strSlotID, "BC2! " + m_strReasonAdmin));
                    }

                    //strBuildPacket = String.Format(@"pb_sv_ban {0} ""{1}"" ""{2}""", this.m_strSlotID, this.cboReason.Text, this.cboReason.Text);
                }
            }
            else if (this.rdoTemporaryBan.Checked == true)
            {
                if (this.rdoPunishOnName.Checked == true)
                {

                    if (this.m_blPunkbuster == false)
                    {
                        lstWords.Add("banList.add");
                        lstWords.Add(name);
                        lstWords.Add(this.m_strSoldierName);
                        lstWords.Add("seconds");
                        lstWords.Add((uscPlayerPunishPanel.GetBanLength(this.txtTime, this.cboTimeMultiplier) * 60).ToString());
                        lstWords.Add(m_strReasonAdmin);
                    }
                    else
                    {
                        lstWords.Add("punkBuster.pb_sv_command");
                        lstWords.Add(String.Format(@"pb_sv_kick {0} {1} ""{2}""", this.m_strSlotID, uscPlayerPunishPanel.GetBanLength(this.txtTime, this.cboTimeMultiplier), "BC2! " + m_strReasonAdmin));
                    }
                    //strBuildPacket = String.Format(@"admin.banPlayer ""{0}"" seconds {1}", this.m_strSoldierName, this.GetBanLength() * 60);
                }
                else if (this.rdoPunishOnIP.Checked == true)
                {
                    lstWords.Add("banList.add");
                    lstWords.Add("ip");
                    lstWords.Add(this.m_strIP);
                    lstWords.Add("seconds");
                    lstWords.Add((uscPlayerPunishPanel.GetBanLength(this.txtTime, this.cboTimeMultiplier) * 60).ToString());
                    lstWords.Add(m_strReasonAdmin);

                    //strBuildPacket = String.Format(@"admin.banIP ""{0}"" seconds {1}", this.m_strIP, this.GetBanLength() * 60);
                }
                else if (this.rdoPunishOnGUID.Checked == true)
                {
                    lstWords.Add("banList.add");
                    lstWords.Add("guid");
                    lstWords.Add(this.m_strGUID);
                    lstWords.Add("seconds");
                    lstWords.Add((uscPlayerPunishPanel.GetBanLength(this.txtTime, this.cboTimeMultiplier) * 60).ToString());
                    lstWords.Add(m_strReasonAdmin);
                }
            }

            if (this.PunishPlayer != null && lstWords.Count > 0)
            {
                this.PunishPlayer(lstWords);
            }
        }
    }
}
