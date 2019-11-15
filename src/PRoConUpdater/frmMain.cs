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
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Drawing;
using Ionic.Zip;
using System.Text;

namespace PRoConUpdater {
    public partial class frmMain : Form {

        private string[] ma_strArgs;
        private Thread m_thUpdater;
        private StringBuilder m_errorLog;

        //private bool m_blUpdateComplete;

        #region Delegates and Events

        public delegate void EmptyParameterHandler();
        public delegate void StringParameterHandler(string text);
        public delegate void DiscoveredReloadingStatusHandler(bool status);

        #endregion

        private delegate void AppendStatusHandler(string strStatusText);

        public frmMain(string[] args) {
            this.ma_strArgs = args;
            InitializeComponent();

            this.m_errorLog = new StringBuilder();
            //this.m_blUpdateComplete = false;
        }

        #region Config Backup Callbacks

        private void StartingConfigBackup() {
            this.picBackingUpConfigs.Image = this.picLoading.Image;
            this.lblBackingUpConfigs.ForeColor = SystemColors.ControlText;
        }

        private void ConfigBackupSuccess(string filename) {
            this.picBackingUpConfigs.Image = this.picSuccess.Image;
            this.lblBackingUpConfigs.ForeColor = SystemColors.ControlText;
            this.lblBackingUpConfigs.Text = "Configs backed up to " + filename;
        }

        private void ConfigBackupError(string error) {
            this.picBackingUpConfigs.Image = this.picError.Image;
            this.lblBackingUpConfigs.ForeColor = Color.Maroon;
            this.lblBackingUpConfigs.Text = "Error: " + error;
        }

        #endregion

        #region Procon Open Checks

        private void StartingProconOpenCheck() {
            this.picCheckingProconOpen.Image = this.picLoading.Image;
            this.lblCheckingProconOpen.ForeColor = SystemColors.ControlText;
        }

        private void WaitingForProconToClose() {
            this.picCheckingProconOpen.Image = this.picLoading.Image;
            this.lblCheckingProconOpen.Text = "Waiting for PRoCon to shutdown..";
        }

        private void KillingProconProcess() {
            this.picCheckingProconOpen.Image = this.picLoading.Image;
            this.lblCheckingProconOpen.Text = "Killing PRoCon process..";
        }

        private void ProconClosed() {
            this.picCheckingProconOpen.Image = this.picSuccess.Image;
            this.lblCheckingProconOpen.Text = "PRoCon closed.";
            this.lblCheckingProconOpen.ForeColor = Color.DimGray;
        }

        #endregion

        #region File Updates

        private void BeginFileUpdates() {
            this.picUpdatingDirectory.Image = this.picLoading.Image;
            this.lblUpdatingDirectory.ForeColor = SystemColors.ControlText;
        }

        private void BeginDirectoryCopy(string directory) {
            this.lblUpdatingDirectory.Text = "Updating [" + directory.Replace(AppDomain.CurrentDomain.BaseDirectory, "") + "] ..";
        }

        private void EndFileUpdates() {
            this.picUpdatingDirectory.Image = this.picSuccess.Image;
            this.lblUpdatingDirectory.ForeColor = SystemColors.ControlText;
        }

        private void EndFileUpdatesError() {
            this.picUpdatingDirectory.Image = this.picError.Image;
            this.lblUpdatingDirectory.ForeColor = Color.Maroon;
        }

        #endregion

        private void DiscoveredReloadingStatus(bool status) {
            this.lblProconReloading.Visible = status;
        }

        private void AppendNewlineStatus(string strStatusText) {
            //this.m_errorLog.AppendFormat("{1}\r\n", strStatusText);
            this.m_errorLog.AppendLine(strStatusText);
            //this.txtUpdateProgress.Text = String.Format("{0}{1}\r\n", this.txtUpdateProgress.Text, strStatusText);
        }

        private void AppendStatus(string strStatusText) {
            this.m_errorLog.Append(strStatusText);
            //this.txtUpdateProgress.Text = String.Format("{0}{1}", this.txtUpdateProgress.Text, strStatusText);
        }

        private void CreateConfigBackup() {

            try {
                FileVersionInfo currentFv = FileVersionInfo.GetVersionInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe"));
                FileVersionInfo updatedFv = FileVersionInfo.GetVersionInfo(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates"), "PRoCon.exe"));

                string zipFileName = String.Format("{0}_to_{1}_backup.zip", currentFv.FileVersion, updatedFv.FileVersion);

                using (ZipFile zip = new ZipFile()) {

                    DirectoryInfo configsDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"));
                    FileInfo[] configFiles = configsDirectory.GetFiles("*.cfg");
                    
                    foreach (FileInfo config in configFiles) {
                        zip.AddFile(config.FullName, "");
                    }

                    DirectoryInfo[] connectionConfigDirectories = configsDirectory.GetDirectories("*_*");

                    foreach (DirectoryInfo connectionConfigDirectory in connectionConfigDirectories) {
                        zip.AddDirectory(connectionConfigDirectory.FullName, connectionConfigDirectory.Name);
                    }

                    if (Directory.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), "Backups")) == false) {
                        Directory.CreateDirectory(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), "Backups"));
                    }

                    zip.Save(Path.Combine(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), "Backups"), zipFileName));
                }

                this.Invoke(new StringParameterHandler(this.ConfigBackupSuccess), zipFileName);

            }
            catch (Exception e) {
                this.Invoke(new StringParameterHandler(this.ConfigBackupError), e.Message);
            }
        }

        private void BeginUpdate() {

            DialogResult dlgUpdateErrorsPage = DialogResult.Retry;
            string strUpdateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");

            bool blDisplayedStatusUpdate = false;
            bool blProconRunning = false;

            this.CreateConfigBackup();

            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), "Checking if PRoCon is open..");
            this.Invoke(new EmptyParameterHandler(this.StartingProconOpenCheck));

            int iWaitCounter = 0;

            do {
                blProconRunning = false;

                try {
                    foreach (Process pcProcon in Process.GetProcessesByName("PRoCon")) {
                        try {
                            if (string.Compare(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe"), Path.GetFullPath(pcProcon.MainModule.FileName), true) == 0) {
                                blProconRunning = true;

                                if (blDisplayedStatusUpdate == false) {
                                    if (iWaitCounter == 0) {
                                        this.Invoke(new EmptyParameterHandler(this.WaitingForProconToClose));
                                        
                                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Waiting for PRoCon at {0} to close", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe")));
                                    }
                                    else {
                                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendStatus), ".");
                                    }
                                }

                                // If we've been waiting for longer than 2 seconds, try and kill the process.
                                if (iWaitCounter > 40) {
                                    pcProcon.Kill();

                                    if (blDisplayedStatusUpdate == false) {
                                        this.Invoke(new EmptyParameterHandler(this.KillingProconProcess));
                                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), "Killing process..");
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }

                iWaitCounter++;
                Thread.Sleep(100);
            } while (blProconRunning == true);

            this.Invoke(new EmptyParameterHandler(this.ProconClosed));

            this.Invoke(new EmptyParameterHandler(this.BeginFileUpdates));

            do {
                // Move the update files..
                this.MoveContents(strUpdateDir);

                try {
                    Directory.Delete(strUpdateDir);
                }
                catch (Exception ex) { }

                dlgUpdateErrorsPage = DialogResult.None;
                if (Directory.Exists(strUpdateDir) == true) {
                    dlgUpdateErrorsPage = MessageBox.Show("There were some errors during the update process.  If these problems persist you can see the errors encountered in the update.log file.", "PRoCon Frostbite", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information);
                }
            } while (dlgUpdateErrorsPage == DialogResult.Retry && Directory.Exists(strUpdateDir) == true);

            //this.m_blUpdateComplete = true;

            // Find out if we should restart procon.

            #region Quick hacky xml read.

            bool blRestartProcon = true;

            if (File.Exists("PRoConUpdater.xml") == true) {

                XmlDocument doc = new XmlDocument();
                doc.Load("PRoConUpdater.xml");

                XmlNodeList OptionsList = doc.GetElementsByTagName("options");
                if (OptionsList.Count > 0) {
                    OptionsList = ((XmlElement)OptionsList[0]).GetElementsByTagName("restart");

                    if (OptionsList.Count > 0) {
                        if (bool.TryParse(OptionsList[0].InnerText, out blRestartProcon) == false) {
                            blRestartProcon = true;
                        }
                    }
                }
            }

            this.Invoke(new DiscoveredReloadingStatusHandler(this.DiscoveredReloadingStatus), blRestartProcon);

            #endregion

            Thread.Sleep(500);

            // Throw update complete..
            if (dlgUpdateErrorsPage == DialogResult.None) {

                this.Invoke(new EmptyParameterHandler(this.EndFileUpdates));

                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe")) == true) {

                    if (blRestartProcon == true) {

                        if (this.ma_strArgs.Length == 0) {
                            System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe"));
                        }
                        else {
                            System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe"), String.Join(" ", this.ma_strArgs));
                        }
                    }
                }
                else {
                    this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Cannot find {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoCon.exe")));
                    Thread.Sleep(1000);
                }
            }
            else {
                this.Invoke(new EmptyParameterHandler(this.EndFileUpdatesError));

                this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), "Update canceled by user.  You can manually move the files from your /Updates dir or download the update from https://myrcon.net");
            }

            try {
                File.WriteAllText("update.log", this.m_errorLog.ToString());
            }
            catch (Exception) { }

            // output this.m_errorLog

            Application.Exit();
        }

        public void MoveContents(string strPath) {

            this.Invoke(new StringParameterHandler(this.BeginDirectoryCopy), strPath);

            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Moving contents of directory {0}", strPath));

            if (Directory.Exists(strPath) == true) {
                foreach (string strFile in Directory.GetFiles(strPath, "*")) {
                    if (String.Compare(Path.GetFileName(strFile), "PRoConUpdater.exe") == 0 || String.Compare(Path.GetFileName(strFile), "PRoConUpdater.pdb") == 0 || String.Compare(Path.GetFileName(strFile), "Ionic.Zip.Reduced.dll") == 0) {
                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Ignoring updater {0}", strFile));

                        try {
                            File.Delete(strFile);
                        }
                        catch (Exception e) {
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\tError deleting {0}, file and /Updates directory must be manually deleted before starting procon", strFile));
                        }
                    }
                    else {

                        string strDestination = strFile.Remove(strFile.LastIndexOf("Updates" + Path.DirectorySeparatorChar), ("Updates" + Path.DirectorySeparatorChar).Length);

                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Deleting old file at {0}", strDestination));

                        try {
                            File.Delete(strDestination);
                        }
                        catch (Exception e) {
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\tError deleting old file at {0}..", strDestination));
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\t{0}..", e.Message));
                        }

                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("Moving new file from {0} to {1}", strFile, strDestination));

                        try {

                            if (Directory.Exists(Path.GetDirectoryName(strDestination)) == false) {
                                Directory.CreateDirectory(Path.GetDirectoryName(strDestination));
                            }

                            File.Move(strFile, strDestination);
                        }
                        catch (Exception e) {
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\tError moving new file at {0}", strFile));
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\t{0}", e.Message));
                        }

                        if (File.Exists(strFile) == true) {
                            this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\tError: {0} still exists!", strFile));
                        }

                        /*
                        if (File.Exists(strFile) == true) {
                            try {
                                File.Delete(strFile);
                            }
                            catch (Exception e) {
                                if (blDisplayErrors == true) {
                                    MessageBox.Show(e.Message + " " + strFile, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        */
                    }
                }

                foreach (string strDirectory in Directory.GetDirectories(strPath)) {
                    this.MoveContents(strDirectory);
                    try {
                        Directory.Delete(strDirectory);
                    }
                    catch (Exception e) {
                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\tError removing directory {0}", strDirectory));
                        this.Invoke(new frmMain.AppendStatusHandler(this.AppendNewlineStatus), String.Format("\t{0}..", e.Message));
                    }
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e) {
            this.m_thUpdater = new Thread(this.BeginUpdate);
            this.m_thUpdater.Start();
        }
    }
}
