// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
// 
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Ionic.Zip;
using PRoCon.Core.Remote;

namespace PRoCon.Core.AutoUpdates {
    public class AutoUpdater {
        public delegate void CheckingUpdatesHandler();

        public delegate void CustomDownloadErrorHandler(string strError);

        public delegate void DownloadUnzipCompleteHandler();

        public delegate void UpdateDownloadingHandler(CDownloadFile cdfDownloading);

        public static string[] Arguments;
        protected readonly List<CDownloadFile> DownloadingGameConfigs;

        protected readonly List<CDownloadFile> DownloadingLocalizations;

        protected readonly object DownloadingGameConfigsLock = new object();
        protected readonly object DownloadingLocalizationsLock = new object();

        protected readonly PRoConApplication Application;

        protected bool GameConfigHint;
        protected CDownloadFile ProconUpdate;

        public AutoUpdater(PRoConApplication praApplication, string[] args) {
            Arguments = args;
            Application = praApplication;

            VersionChecker = new CDownloadFile("https://api.myrcon.net/procon/version");
            VersionChecker.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(VersionChecker_DownloadComplete);
            DownloadingLocalizations = new List<CDownloadFile>();
            DownloadingGameConfigs = new List<CDownloadFile>();
        }

        public CDownloadFile VersionChecker { get; private set; }
        public event DownloadUnzipCompleteHandler DownloadUnzipComplete;
        public event CheckingUpdatesHandler CheckingUpdates;
        public event CheckingUpdatesHandler NoVersionAvailable;
        public event CheckingUpdatesHandler GameConfigUpdated;
        public event UpdateDownloadingHandler UpdateDownloading;
        public event CustomDownloadErrorHandler CustomDownloadError;

        public void CheckVersion() {
            if (Application.BlockUpdateChecks == false) {
                if (CheckingUpdates != null) {
                    this.CheckingUpdates();
                }

                VersionChecker.BeginDownload();
            }
        }

        private void DownloadLocalizationFile(string strDownloadSource, string strLocalizationFilename) {
            lock (DownloadingLocalizationsLock) {
                var cdfUpdatedLocalization = new CDownloadFile(strDownloadSource, strLocalizationFilename);
                cdfUpdatedLocalization.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_cdfUpdatedLocalization_DownloadComplete);
                DownloadingLocalizations.Add(cdfUpdatedLocalization);

                cdfUpdatedLocalization.BeginDownload();
            }
        }

        private void m_cdfUpdatedLocalization_DownloadComplete(CDownloadFile cdfSender) {
            string strLocalizationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization");

            try {
                if (Directory.Exists(strLocalizationFolder) == false) {
                    Directory.CreateDirectory(strLocalizationFolder);
                }

                using (ZipFile zip = ZipFile.Read(cdfSender.CompleteFileData)) {
                    zip.ExtractAll(strLocalizationFolder, ExtractExistingFileAction.OverwriteSilently);
                }

                Application.LoadLocalizationFiles();
            }
            catch (Exception) {
            }
        }

        private void DownloadGameConfigFile(string strDownloadSource, string strGameConfigFilename) {
            lock (DownloadingGameConfigsLock) {
                var cdfUpdatedGameConfig = new CDownloadFile(strDownloadSource, strGameConfigFilename);
                cdfUpdatedGameConfig.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_cdfUpdatedGameConfig_DownloadComplete);
                DownloadingGameConfigs.Add(cdfUpdatedGameConfig);

                cdfUpdatedGameConfig.BeginDownload();
            }
        }

        private void m_cdfUpdatedGameConfig_DownloadComplete(CDownloadFile cdfSender) {
            string strGameConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");

            try {
                if (Directory.Exists(strGameConfigFolder) == false) {
                    Directory.CreateDirectory(strGameConfigFolder);
                }

                using (ZipFile zip = ZipFile.Read(cdfSender.CompleteFileData)) {
                    zip.ExtractAll(strGameConfigFolder, ExtractExistingFileAction.OverwriteSilently);
                }

                // GameConfigs require Procon restart
                if (GameConfigHint == false) {
                    GameConfigInfo();
                }
            }
            catch (Exception) {
            }
        }

        private void GameConfigInfo() {
            GameConfigHint = true;
            if (GameConfigUpdated != null) {
                this.GameConfigUpdated();
            }
        }

        private string MD5File(string strFileName) {
            var sbStringifyHash = new StringBuilder();

            if (File.Exists(strFileName) == true) {
                MD5 md5Hasher = MD5.Create();

                byte[] hash = md5Hasher.ComputeHash(File.ReadAllBytes(strFileName));

                for (int x = 0; x < hash.Length; x++) {
                    sbStringifyHash.Append(hash[x].ToString("x2"));
                }
            }

            return sbStringifyHash.ToString();
        }

        private string MD5Data(byte[] data) {
            var sbStringifyHash = new StringBuilder();

            MD5 md5Hasher = MD5.Create();

            byte[] hash = md5Hasher.ComputeHash(data);

            for (int x = 0; x < hash.Length; x++) {
                sbStringifyHash.Append(hash[x].ToString("x2"));
            }

            return sbStringifyHash.ToString();
        }

        private void VersionChecker_DownloadComplete(CDownloadFile sender) {
            string[] versionData = Encoding.UTF8.GetString(sender.CompleteFileData).Split('\n');
            GameConfigHint = false;

            if (versionData.Length >= 4 && (ProconUpdate == null || ProconUpdate.FileDownloading == false)) {
                bool blContinueFileDownload = true;

                try {
                    if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates")) == true) {
                        AssemblyName proconAssemblyName = AssemblyName.GetAssemblyName(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates"), "PRoCon.exe"));

                        // If an update has already been downloaded but not installed..
                        if (new Version(versionData[0]).CompareTo(proconAssemblyName.Version) >= 0) {
                            blContinueFileDownload = false;
                        }
                    }
                }
                catch (Exception) {
                }

                if (blContinueFileDownload == true) {
                    if (new Version(versionData[0]).CompareTo(Assembly.GetExecutingAssembly().GetName().Version) > 0) {
                        // Download file, alert or auto apply once complete with release notes.
                        ProconUpdate = new CDownloadFile(versionData[2], versionData[3]);
                        ProconUpdate.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(cdfPRoConUpdate_DownloadComplete);

                        if (UpdateDownloading != null) {
                            this.UpdateDownloading(ProconUpdate);
                        }

                        ProconUpdate.BeginDownload();
                    }
                    else {
                        if (NoVersionAvailable != null) {
                            this.NoVersionAvailable();
                        }

                        lock (DownloadingLocalizationsLock) {
                            foreach (CDownloadFile cdfFile in DownloadingLocalizations) {
                                cdfFile.EndDownload();
                            }

                            DownloadingLocalizations.Clear();
                        }

                        lock (DownloadingGameConfigsLock) {
                            foreach (CDownloadFile cdfFile in DownloadingGameConfigs) {
                                cdfFile.EndDownload();
                            }

                            DownloadingGameConfigs.Clear();
                        }

                        for (int i = 4; i < versionData.Length; i++) {
                            List<string> lstExtensibilityVersion = Packet.Wordify(versionData[i]);

                            if (lstExtensibilityVersion.Count >= 4 && System.String.Compare(lstExtensibilityVersion[0], "localization", System.StringComparison.OrdinalIgnoreCase) == 0) {
                                try {
                                    if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"), lstExtensibilityVersion[2])) == true) {
                                        if (System.String.Compare(lstExtensibilityVersion[1], MD5File(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"), lstExtensibilityVersion[2])), System.StringComparison.OrdinalIgnoreCase) != 0) {
                                            // Download new localization file and tell options to reload it once completed.
                                            DownloadLocalizationFile(lstExtensibilityVersion[3], lstExtensibilityVersion[2]);
                                            Thread.Sleep(100); // I don't know how many languages there may be later so sleep on it to prevent spam.
                                        }
                                    }
                                    else {
                                        // Download new localization file and tell options to load it once completed.
                                        DownloadLocalizationFile(lstExtensibilityVersion[3], lstExtensibilityVersion[2]);
                                        Thread.Sleep(100);
                                    }
                                }
                                catch (Exception) {
                                }
                            }

                            // GameConfigs
                            if (lstExtensibilityVersion.Count >= 4 && System.String.Compare(lstExtensibilityVersion[0], "gameconfig", System.StringComparison.OrdinalIgnoreCase) == 0 && Application.OptionsSettings.AutoCheckGameConfigsForUpdates == true) {
                                try {
                                    if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), lstExtensibilityVersion[2])) == true) {
                                        if (System.String.Compare(lstExtensibilityVersion[1], MD5File(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), lstExtensibilityVersion[2])), System.StringComparison.OrdinalIgnoreCase) != 0) {
                                            // Download new GameConfig file 
                                            DownloadGameConfigFile(lstExtensibilityVersion[3], lstExtensibilityVersion[2]);
                                            Thread.Sleep(100); // we don't know how many will come.
                                        }
                                    }
                                    else {
                                        // Download new GameConfig file
                                        DownloadGameConfigFile(lstExtensibilityVersion[3], lstExtensibilityVersion[2]);
                                        Thread.Sleep(100);
                                    }
                                }
                                catch (Exception) {
                                }
                            }
                        }
                    }
                }
                else {
                    DownloadedUnzippedComplete();
                }
            }
        }

        private void cdfPRoConUpdate_DownloadComplete(CDownloadFile cdfSender) {
            if (System.String.Compare(MD5Data(cdfSender.CompleteFileData), (string) cdfSender.AdditionalData, System.StringComparison.OrdinalIgnoreCase) == 0) {
                string strUpdatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");

                try {
                    if (Directory.Exists(strUpdatesFolder) == false) {
                        Directory.CreateDirectory(strUpdatesFolder);
                    }

                    using (ZipFile zip = ZipFile.Read(cdfSender.CompleteFileData)) {
                        zip.ExtractAll(strUpdatesFolder, ExtractExistingFileAction.OverwriteSilently);
                    }

                    DownloadedUnzippedComplete();
                }
                catch (Exception e) {
                    if (CustomDownloadError != null) {
                        this.CustomDownloadError(e.Message);
                    }

                    //this.Invoke(new DownloadErrorDelegate(DownloadError_Callback), e.Message);
                }
            }
            else {
                if (CustomDownloadError != null) {
                    this.CustomDownloadError("Downloaded file failed checksum, please try again or download direct from https://myrcon.com");
                }

                //this.Invoke(new DownloadErrorDelegate(DownloadError_Callback), "Downloaded file failed checksum, please try again or download direct from https://myrcon.com");
            }
        }

        private void DownloadedUnzippedComplete() {
            if (Application.OptionsSettings.AutoApplyUpdates == true) {
                BeginUpdateProcess(Application);
            }
            else {
                if (DownloadUnzipComplete != null) {
                    this.DownloadUnzipComplete();
                }
            }
        }

        public void Shutdown() {
            lock (DownloadingLocalizationsLock) {
                if (ProconUpdate != null)
                    ProconUpdate.EndDownload();
                if (VersionChecker != null)
                    VersionChecker.EndDownload();

                foreach (CDownloadFile cdfFile in DownloadingLocalizations) {
                    cdfFile.EndDownload();
                }

                DownloadingLocalizations.Clear();
            }

            if (DownloadingGameConfigs != null) {
                lock (DownloadingGameConfigsLock) {
                    foreach (CDownloadFile cdfFile in DownloadingGameConfigs) {
                        cdfFile.EndDownload();
                    }

                    DownloadingGameConfigs.Clear();
                }
            }
        }

        /*
        public static void BeginUpdateProcess() {

            // Check if the autoupdater needs updating.. if not then we'll forget about it.

            string strUpdatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");

            if (Directory.Exists(strUpdatesFolder) == true) {

                int iDeleteChecker = 0;

                string strCurrentProconUpdaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoConUpdater.exe");
                // Overwrite proconupdater.exe

                if (File.Exists(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe")) == true) {

                    do {
                        try {
                            File.Copy(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe"), strCurrentProconUpdaterPath, true);

                            File.Delete(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe"));
                        }
                        catch (Exception) { }

                        Thread.Sleep(100);
                        iDeleteChecker++;
                    } while (File.Exists(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe")) == true && iDeleteChecker < 10);
                }

                if (File.Exists(strCurrentProconUpdaterPath) == true) {

                    if (AutoUpdater.m_strArgs.Length == 0) {
                        System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe");
                    }
                    else {
                        System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe", String.Join(" ", AutoUpdater.m_strArgs));
                    }

                    Application.Exit();
                }
            }
        }
        */

        public static void BeginUpdateProcess(PRoConApplication praApplication) {
            // Check if the autoupdater needs updating.. if not then we'll forget about it.

            string strUpdatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");

            if (Directory.Exists(strUpdatesFolder) == true) {
                AssemblyName proconUpdaterAssemblyName = null;
                AssemblyName proconUpdaterUpdatesDirAssemblyName = null;

                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoConUpdater.exe")) == true) {
                    proconUpdaterAssemblyName = AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoConUpdater.exe"));
                }
                if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates"), "PRoConUpdater.exe")) == true) {
                    proconUpdaterUpdatesDirAssemblyName = AssemblyName.GetAssemblyName(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates"), "PRoConUpdater.exe"));
                }

                // If the old updater is.. old =)
                if ((proconUpdaterAssemblyName == null && proconUpdaterUpdatesDirAssemblyName != null) || (proconUpdaterAssemblyName != null && proconUpdaterUpdatesDirAssemblyName != null && proconUpdaterUpdatesDirAssemblyName.Version.CompareTo(proconUpdaterAssemblyName.Version) > 0)) {
                    //int iDeleteChecker = 0;

                    string strCurrentProconUpdaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRoConUpdater.exe");
                    // Overwrite proconupdater.exe

                    //do {
                    //    //if (iDeleteChecker > 0) {
                    //    //    MessageBox.Show("Please close the PRoConUpdater to continue the update process..");
                    //    //}

                    //    try {
                    //        //File.Delete(strCurrentProconUpdaterPath);
                    //    }
                    //    catch (Exception) { }

                    //    Thread.Sleep(100);
                    //    iDeleteChecker++;
                    //} while (File.Exists(strCurrentProconUpdaterPath) == true && iDeleteChecker < 5);

                    try {
                        try {
                            File.Copy(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe"), strCurrentProconUpdaterPath, true);
                            File.Delete(Path.Combine(strUpdatesFolder, "PRoConUpdater.exe"));
                        }
                        catch (Exception) {
                        }

                        if (Arguments != null && Arguments.Length == 0) {
                            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe");
                        }
                        else {
                            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe", String.Join(" ", Arguments));
                        }

                        if (praApplication != null) {
                            praApplication.Shutdown();
                        }

                        System.Windows.Forms.Application.Exit();
                    }
                    catch (Exception) {
                    }
                }
                else {
                    // Same or newer version, we're running with the same autoupdater.exe
                    if (Arguments != null && Arguments.Length == 0) {
                        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe");
                    }
                    else {
                        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe", String.Join(" ", Arguments));
                    }

                    if (praApplication != null) {
                        praApplication.Shutdown();
                    }

                    //System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "PRoConUpdater.exe");
                    System.Windows.Forms.Application.Exit();
                }
            }
        }
    }
}