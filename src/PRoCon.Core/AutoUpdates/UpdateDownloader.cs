using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;
using PRoCon.Core.Remote;

namespace PRoCon.Core.AutoUpdates {
    public class UpdateDownloader {
        public delegate void CustomDownloadErrorHandler(string strError);

        public delegate void DownloadUnzipCompleteHandler();

        public delegate void UpdateDownloadingHandler(CDownloadFile cdfDownloading);

        protected CDownloadFile ProconUpdate;

        protected string UpdatesDirectoryName;

        public UpdateDownloader(string updatesDirectoryName) {
            UpdatesDirectoryName = updatesDirectoryName;
            VersionChecker = new CDownloadFile("https://api.myrcon.net/procon/version");
            VersionChecker.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(VersionChecker_DownloadComplete);
        }

        public CDownloadFile VersionChecker { get; private set; }
        public event DownloadUnzipCompleteHandler DownloadUnzipComplete;
        public event UpdateDownloadingHandler UpdateDownloading;
        public event CustomDownloadErrorHandler CustomDownloadError;

        public void DownloadLatest() {
            VersionChecker.BeginDownload();
        }

        private string MD5Data(byte[] data) {
            var stringifyHash = new StringBuilder();

            MD5 md5Hasher = MD5.Create();

            byte[] hash = md5Hasher.ComputeHash(data);

            for (int x = 0; x < hash.Length; x++) {
                stringifyHash.Append(hash[x].ToString("x2"));
            }

            return stringifyHash.ToString();
        }

        private void VersionChecker_DownloadComplete(CDownloadFile sender) {
            string[] versionData = Encoding.UTF8.GetString(sender.CompleteFileData).Split('\n');

            if (versionData.Length >= 4 && (ProconUpdate == null || ProconUpdate.FileDownloading == false)) {
                // Download file, alert or auto apply once complete with release notes.
                ProconUpdate = new CDownloadFile(versionData[2], versionData[3]);
                ProconUpdate.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(CdfPRoConUpdate_DownloadComplete);

                if (UpdateDownloading != null) {
                    this.UpdateDownloading(ProconUpdate);
                }

                ProconUpdate.BeginDownload();
            }
        }

        private void CdfPRoConUpdate_DownloadComplete(CDownloadFile sender) {
            if (String.Compare(MD5Data(sender.CompleteFileData), (string) sender.AdditionalData, StringComparison.OrdinalIgnoreCase) == 0) {
                string updatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UpdatesDirectoryName);

                try {
                    if (Directory.Exists(updatesFolder) == false) {
                        Directory.CreateDirectory(updatesFolder);
                    }

                    using (ZipFile zip = ZipFile.Read(sender.CompleteFileData)) {
                        zip.ExtractAll(updatesFolder, ExtractExistingFileAction.OverwriteSilently);
                    }

                    if (DownloadUnzipComplete != null) {
                        this.DownloadUnzipComplete();
                    }
                }
                catch (Exception e) {
                    if (CustomDownloadError != null) {
                        this.CustomDownloadError(e.Message);
                    }
                }
            }
            else {
                if (CustomDownloadError != null) {
                    this.CustomDownloadError("Downloaded file failed checksum, please try again or download direct from https://myrcon.net");
                }
            }
        }
    }
}