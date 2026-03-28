using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PRoCon.Core.AutoUpdates
{
    public class UpdateDownloader
    {
        public delegate void CustomDownloadErrorHandler(string strError);

        public delegate void DownloadUnzipCompleteHandler();

        public delegate void UpdateDownloadingHandler(CDownloadFile cdfDownloading);

        protected CDownloadFile ProconUpdate;

        protected string UpdatesDirectoryName;

        public UpdateDownloader(string updatesDirectoryName)
        {
            UpdatesDirectoryName = updatesDirectoryName;
            VersionChecker = new CDownloadFile("https://api.myrcon.net/procon/version");
            VersionChecker.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(VersionChecker_DownloadComplete);
        }

        public CDownloadFile VersionChecker { get; private set; }
        public event DownloadUnzipCompleteHandler DownloadUnzipComplete;
        public event UpdateDownloadingHandler UpdateDownloading;
        public event CustomDownloadErrorHandler CustomDownloadError;

        public void DownloadLatest()
        {
            VersionChecker.BeginDownload();
        }

        private string HashData(byte[] data)
        {
            var stringifyHash = new StringBuilder();

            using (var hasher = SHA256.Create())
            {
                byte[] hash = hasher.ComputeHash(data);

                for (int x = 0; x < hash.Length; x++)
                {
                    stringifyHash.Append(hash[x].ToString("x2"));
                }
            }

            return stringifyHash.ToString();
        }

        private void VersionChecker_DownloadComplete(CDownloadFile sender)
        {
            string[] versionData = Encoding.UTF8.GetString(sender.CompleteFileData).Split('\n');

            if (versionData.Length >= 4 && (ProconUpdate == null || ProconUpdate.FileDownloading == false))
            {
                // Download file, alert or auto apply once complete with release notes.
                ProconUpdate = new CDownloadFile(versionData[2], versionData[3]);
                ProconUpdate.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(CdfPRoConUpdate_DownloadComplete);

                if (UpdateDownloading != null)
                {
                    this.UpdateDownloading(ProconUpdate);
                }

                ProconUpdate.BeginDownload();
            }
        }

        private void CdfPRoConUpdate_DownloadComplete(CDownloadFile sender)
        {
            if (String.Compare(HashData(sender.CompleteFileData), (string)sender.AdditionalData, StringComparison.OrdinalIgnoreCase) == 0)
            {
                string updatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UpdatesDirectoryName);

                try
                {
                    if (Directory.Exists(updatesFolder) == false)
                    {
                        Directory.CreateDirectory(updatesFolder);
                    }

                    ExtractZipFromBytes(sender.CompleteFileData, updatesFolder);

                    if (DownloadUnzipComplete != null)
                    {
                        this.DownloadUnzipComplete();
                    }
                }
                catch (Exception e)
                {
                    if (CustomDownloadError != null)
                    {
                        this.CustomDownloadError(e.Message);
                    }
                }
            }
            else
            {
                if (CustomDownloadError != null)
                {
                    this.CustomDownloadError("Downloaded file failed checksum, please try again or download direct from https://myrcon.net");
                }
            }
        }

        private static void ExtractZipFromBytes(byte[] zipData, string destinationFolder)
        {
            using (var stream = new MemoryStream(zipData))
            using (var zipInputStream = new ZipInputStream(stream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    string entryPath = Path.GetFullPath(Path.Combine(destinationFolder, entry.Name));
                    if (!entryPath.StartsWith(Path.GetFullPath(destinationFolder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        continue; // Skip malicious entries

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(entryPath);
                        continue;
                    }

                    string directoryName = Path.GetDirectoryName(entryPath);
                    if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    using (var fileStream = File.Create(entryPath))
                    {
                        zipInputStream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}