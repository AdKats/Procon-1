using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace PRoCon.Forms {
    using PRoCon.Core.AutoUpdates;
    using PRoCon.Controls.ControlsEx;
    public partial class GspUpdater : Form {

        private List<string> m_lstIgnoreDirectories;

        private string m_gspUpdatesDirectory;
        private UpdateDownloader m_updateDownloader;
        private Version m_latestVersion;

        private ListViewColumnSorter m_lvwColumnSorter;

        public GspUpdater() {
            InitializeComponent();

            this.m_lvwColumnSorter = new ListViewColumnSorter();
            this.lsvInstalls.ListViewItemSorter = this.m_lvwColumnSorter;

            this.m_latestVersion = null;
            this.m_gspUpdatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GspLatestVersion");

            this.m_updateDownloader = new UpdateDownloader("GspLatestVersion");
            this.m_updateDownloader.UpdateDownloading += new UpdateDownloader.UpdateDownloadingHandler(m_updateDownloader_UpdateDownloading);

            this.m_updateDownloader.DownloadUnzipComplete += new UpdateDownloader.DownloadUnzipCompleteHandler(m_updateDownloader_DownloadUnzipComplete);
            this.m_updateDownloader.DownloadLatest();

            this.m_lstIgnoreDirectories = new List<string>() { "updates", "gsplatestversion", "localization", "plugins", "configs", "updates", "logs", "media" };
        }

        private void m_updateDownloader_UpdateDownloading(PRoCon.Core.CDownloadFile cdfDownloading) {
            cdfDownloading.DownloadProgressUpdate += new PRoCon.Core.CDownloadFile.DownloadFileEventDelegate(cdfDownloading_DownloadProgressUpdate);
            cdfDownloading.DownloadError += new PRoCon.Core.CDownloadFile.DownloadFileEventDelegate(cdfDownloading_DownloadError);
        }

        void cdfDownloading_DownloadError(PRoCon.Core.CDownloadFile cdfSender) {
            this.lblDownloadStatus.Text = "Error dowloading latest version";
        }

        private void cdfDownloading_DownloadProgressUpdate(PRoCon.Core.CDownloadFile cdfSender) {
            try
            {
                this.lblDownloadStatus.Text = cdfSender.GetLabelProgress();
            }
            catch (InvalidOperationException e)
            {
                // Handle exception
            }

        }

        private void m_updateDownloader_DownloadUnzipComplete() {
            this.lblDownloadStatus.Text = "Latest version downloaded";

            try {
                AssemblyName proconAssemblyName = AssemblyName.GetAssemblyName(Path.Combine(this.m_gspUpdatesDirectory, "PRoCon.exe"));

                this.m_latestVersion = proconAssemblyName.Version;
            }
            catch (Exception) { }

        }

        private void GspUpdater_Load(object sender, EventArgs e) {
            
            this.txtBrowseFolder.Text = AppDomain.CurrentDomain.BaseDirectory;
            this.folderBrowser.SelectedPath = this.txtBrowseFolder.Text;

            this.DiscoverProcons(this.txtBrowseFolder.Text);

            this.tmrUpdateChecker.Enabled = true;
        }

        public enum RunningStatus {
            None,
            Stopped,
            Running,
            Error,
        }

        private void SetStatus(RunningStatus status, string version, string directory, string proconPath) {

            if (this.lsvInstalls.Items.ContainsKey(proconPath) == true) {
                this.lsvInstalls.Items[proconPath].Text = status.ToString();
                this.lsvInstalls.Items[proconPath].Tag = status;
                this.lsvInstalls.Items[proconPath].SubItems["Version"].Text = version;
                this.lsvInstalls.Items[proconPath].SubItems["Directory"].Text = directory;
                this.lsvInstalls.Items[proconPath].SubItems["Path"].Text = proconPath;
            }
            else {
                ListViewItem newProcon = new ListViewItem();
                newProcon.Name = proconPath;
                newProcon.Text = status.ToString();
                newProcon.Tag = status;

                newProcon.UseItemStyleForSubItems = false;

                ListViewItem.ListViewSubItem newSubitem = new ListViewItem.ListViewSubItem();
                newSubitem.Name = "Version";
                newSubitem.Text = version;
                newSubitem.Font = new Font(this.Font, FontStyle.Bold);
                newProcon.SubItems.Add(newSubitem);

                newSubitem = new ListViewItem.ListViewSubItem();
                newSubitem.Name = "Directory";
                newSubitem.Text = directory;
                newProcon.SubItems.Add(newSubitem);

                newSubitem = new ListViewItem.ListViewSubItem();
                newSubitem.Name = "Path";
                newSubitem.Text = proconPath;
                newProcon.SubItems.Add(newSubitem);

                this.lsvInstalls.Items.Add(newProcon);
            }

            if (this.lsvInstalls.Items[proconPath].Tag != null && ((RunningStatus)this.lsvInstalls.Items[proconPath].Tag) == RunningStatus.Running) {
                this.lsvInstalls.Items[proconPath].ImageKey = "running.png";
                this.lsvInstalls.Items[proconPath].ForeColor = Color.LightSeaGreen;
                this.lsvInstalls.Items[proconPath].Font = new Font(this.Font, FontStyle.Bold);
            }
            else {
                this.lsvInstalls.Items[proconPath].ImageKey = "stopped.png";
                this.lsvInstalls.Items[proconPath].ForeColor = SystemColors.WindowText;
                this.lsvInstalls.Items[proconPath].Font = this.Font;
            }

            try {
                if (this.m_latestVersion != null) {
                    if (new Version(version).CompareTo(this.m_latestVersion) >= 0) {
                        this.lsvInstalls.Items[proconPath].SubItems["Version"].ForeColor = Color.LightSeaGreen;
                    }
                    else {
                        this.lsvInstalls.Items[proconPath].SubItems["Version"].ForeColor = Color.Maroon;
                    }
                }
            }
            catch (Exception) { }
            //

        }

        private void ProconStatus(string proconPath) {

            if (File.Exists(proconPath) == true) {
                RunningStatus status = RunningStatus.Stopped;
                string version = "Unknown";
                string directory = "Unknown";

                foreach (Process pcProcon in Process.GetProcessesByName("PRoCon")) {
                    try {
                        if (string.Compare(proconPath, Path.GetFullPath(pcProcon.MainModule.FileName), true) == 0) {
                            status = RunningStatus.Running;
                        }
                    }
                    catch (Exception) {
                        status = RunningStatus.Error;
                    }
                }

                try {
                    AssemblyName proconAssemblyName = AssemblyName.GetAssemblyName(proconPath);

                    version = proconAssemblyName.Version.ToString();
                }
                catch (Exception) {
                    version = "Error";
                }

                try {
                    DirectoryInfo dirInfo = new DirectoryInfo(proconPath);

                    directory = dirInfo.Parent.Name;
                }
                catch (Exception) {
                    directory = "Error";
                }

                this.SetStatus(status, version, directory, proconPath);
            }
        }

        private void DiscoverProcons(string path) {

            this.lsvInstalls.BeginUpdate();

            if (Directory.Exists(path) == true) {

                List<string> lstDirectories = new List<string>(Directory.GetDirectories(path));

                // Search sub directories, ignoring any updates directories 
                foreach (string directory in lstDirectories) {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    if (this.m_lstIgnoreDirectories.Contains(dirInfo.Name.ToLower()) == false) {
                        this.DiscoverProcons(directory);

                        this.ProconStatus(Path.Combine(directory, "PRoCon.exe"));
                    }
                }
            }

            foreach (ColumnHeader column in this.lsvInstalls.Columns) {
                column.Width = -2;
            }

            this.lsvInstalls.EndUpdate();
        }

        private void tmrUpdateChecker_Tick(object sender, EventArgs e) {

            this.lsvInstalls.BeginUpdate();

            foreach (ListViewItem item in this.lsvInstalls.Items) {
                this.ProconStatus(item.Name);
            }

            this.RefreshControls();

            foreach (ColumnHeader column in this.lsvInstalls.Columns) {
                column.Width = -2;
            }

            this.lsvInstalls.EndUpdate();
        }

        #region Buttons

        private void btnStart_Click(object sender, EventArgs e) {
            foreach (ListViewItem selectedItem in this.lsvInstalls.SelectedItems) {
                
                if (selectedItem.Tag == null || (selectedItem.Tag != null && ((RunningStatus)selectedItem.Tag) == RunningStatus.Stopped)) {
                    Process.Start(selectedItem.Name, this.txtArguments.Text.Replace("%directory%", selectedItem.SubItems["Directory"].Text));
                }
            }

            this.RefreshControls();
        }

        private void ShutdownProconInstance(string fullProconPath) {
            try {
                foreach (Process pcProcon in Process.GetProcessesByName("PRoCon")) {
                    try {
                        if (string.Compare(fullProconPath, Path.GetFullPath(pcProcon.MainModule.FileName), true) == 0) {
                            pcProcon.Kill();
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }

        private void btnStop_Click(object sender, EventArgs e) {

            foreach (ListViewItem selectedItem in this.lsvInstalls.SelectedItems) {
                this.ShutdownProconInstance(selectedItem.Name);
            }

            this.RefreshControls();
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            if (Directory.Exists(target.FullName) == false) {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles()) {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e) {

            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;

            int iUpdatedCopies = 0, iSkippedCopies = 0; ;

            if (Directory.Exists(this.m_gspUpdatesDirectory) == true) {
                foreach (ListViewItem selectedItem in this.lsvInstalls.SelectedItems) {
                    if (this.m_latestVersion != null) {
                        if (new Version(selectedItem.SubItems["Version"].Text).CompareTo(this.m_latestVersion) < 0) {
                            this.ShutdownProconInstance(selectedItem.Name);
                            GspUpdater.CopyAll(new DirectoryInfo(this.m_gspUpdatesDirectory), new DirectoryInfo(Path.GetDirectoryName(selectedItem.Name)));

                            iUpdatedCopies++;
                        }
                        else {
                            iSkippedCopies++;
                        }

                        this.lblDownloadStatus.Text = String.Format("Updated {0} of {1} ({2} Skipped)", iUpdatedCopies + iSkippedCopies, this.lsvInstalls.SelectedItems.Count, iSkippedCopies);
                    }
                }
            }

            this.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        #endregion

        #region List Control

        private void RefreshControls() {

            bool IsStopButtonEnabled = false;
            bool IsStartButtonEnabled = false;
            bool IsUpdateButtonEnabled = false;

            if (this.lsvInstalls.SelectedItems.Count > 0) {
                foreach (ListViewItem item in this.lsvInstalls.SelectedItems) {

                    if (item.Tag != null) {
                        if (((RunningStatus)item.Tag) == RunningStatus.Error || ((RunningStatus)item.Tag) == RunningStatus.Running) {
                            IsStartButtonEnabled = true;
                        }

                        if (((RunningStatus)item.Tag) == RunningStatus.Error || ((RunningStatus)item.Tag) == RunningStatus.Stopped) {
                            IsStopButtonEnabled = true;
                        }
                    }
                    
                    try {

                        if (this.m_latestVersion != null) {
                            if (new Version(item.SubItems["Version"].Text).CompareTo(this.m_latestVersion) < 0) {
                                IsUpdateButtonEnabled = true;
                            }
                        }

                    }
                    catch (Exception) { }

                }
            }

            this.btnStart.Enabled = IsStopButtonEnabled;
            this.btnStop.Enabled = IsStartButtonEnabled;

            this.btnUpdate.Enabled = IsUpdateButtonEnabled;
        }

        private void lstInstalls_SelectedIndexChanged(object sender, EventArgs e) {
            this.RefreshControls();
        }

        #endregion

        private void GspUpdater_Activated(object sender, EventArgs e) {

            this.tmrUpdateChecker.Enabled = false;

            this.lsvInstalls.Items.Clear();

            this.DiscoverProcons(this.txtBrowseFolder.Text);

            this.tmrUpdateChecker.Enabled = true;
        }

        private void lsvInstalls_ColumnClick(object sender, ColumnClickEventArgs e) {
            if (e.Column == this.m_lvwColumnSorter.SortColumn) {
                if (this.m_lvwColumnSorter.Order == SortOrder.Ascending) {
                    this.m_lvwColumnSorter.Order = SortOrder.Descending;
                }
                else {
                    this.m_lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else {
                this.m_lvwColumnSorter.SortColumn = e.Column;
                this.m_lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lsvInstalls.Sort();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            DialogResult result = this.folderBrowser.ShowDialog();

            if (result == DialogResult.OK) {
                if (Directory.Exists(this.folderBrowser.SelectedPath) == true) {
                    this.txtBrowseFolder.Text = this.folderBrowser.SelectedPath;

                    this.lsvInstalls.Items.Clear();

                    this.tmrUpdateChecker.Enabled = false;

                    this.DiscoverProcons(this.txtBrowseFolder.Text);

                    this.tmrUpdateChecker.Enabled = true;
                }
            }
        }

        private void lsvInstalls_DoubleClick(object sender, EventArgs e) {
            if (this.lsvInstalls.SelectedItems.Count > 0) {

                DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(this.lsvInstalls.SelectedItems[0].Name));

                Process.Start(info.ToString());
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.lsvInstalls.SelectedItems.Count > 0) {
                DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(this.lsvInstalls.SelectedItems[0].Name));

                Process.Start(info.ToString());
            }
        }

        private void lsvInstalls_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {

                this.ctxInstall.Show(this.lsvInstalls.PointToScreen(e.Location));
            }
        }

        private void selectToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (ListViewItem item in this.lsvInstalls.Items) {
                item.Selected = true;
            }
        }

        private void alToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (ListViewItem item in this.lsvInstalls.Items) {
                item.Selected = false;
            }
        }

    }
}
