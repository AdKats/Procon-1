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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PRoCon.Forms {
    using Core;
    partial class frmAbout : Form {

        private CLocalization m_clocLanguage = null;

        public frmAbout() {
            InitializeComponent();
            //this.Text = String.Format("About {0}", AssemblyTitle);
            //this.lblProductName.Text = AssemblyTitle;
            //this.labelProductName.Text = AssemblyProduct;
            //this.lblVersion.Text = String.Format("Version {0}", AssemblyVersion);
            //this.labelCopyright.Text = AssemblyCopyright;
            //this.labelCompanyName.Text = AssemblyCompany;
            //this.textBoxDescription.Text = AssemblyDescription;
        }

        public void SetLocalization(CLocalization clocLanguage) {
            this.m_clocLanguage = clocLanguage;

            this.Text = this.m_clocLanguage.GetLocalized("frmAbout.Title", new string[] { this.AssemblyTitle });
            this.okButton.Text = this.m_clocLanguage.GetLocalized("global.close", null);

            this.tabAbout.Text = this.m_clocLanguage.GetLocalized("frmAbout.tabAbout.Title", null);
            this.lblVersion.Text = this.m_clocLanguage.GetLocalized("frmAbout.tabAbout.lblVersion", new string[] { this.AssemblyVersion });
            this.lnkVisitForum.Text = this.m_clocLanguage.GetLocalized("frmAbout.tabAbout.lnkVisitForum", null);
            //this.lnkVisitForum.LinkArea = new LinkArea(0, this.lnkVisitForum.Text.Length);
            this.tabThanks.Text = this.m_clocLanguage.GetLocalized("frmAbout.tabThanks.Title", null);
            this.tabCopyright.Text = this.m_clocLanguage.GetLocalized("frmAbout.tabCopyright.Title", null);
        }

        public string AssemblyTitle {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "") {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://myrcon.net/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://myrcon.net/");
        }

        private void okButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void lnkMaxMind_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.maxmind.com");
        }

        private void picSpacefishSteve_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("http://www.facebook.com/group.php?gid=474809860261&ref=nf");
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://dotnetzip.codeplex.com/");
        }

        private void lnkMySQLconnector_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://dev.mysql.com/downloads/connector/net/");
        }

        private void lnkTimmsy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.u3.net.au");
        }

        private void lnkSinex_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.u3.net.au");
        }

        private void lnkIntruder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.solstice-gaming.eu/");
        }

        private void lnk1349_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.facebook.com/group.php?gid=474809860261&ref=nf");
        }

        private void lnkCptNeeda_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.habitat4hookers.org/");
        }

        private void lnkZboss_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.z-gaming.org/");
            
        }
    }
}
