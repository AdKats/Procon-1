using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PRoCon.Core.Updates
{
    public class ReleaseAsset
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public long Size { get; set; }
    }

    public class UpdateInfo
    {
        public string TagName { get; set; }
        public SemanticVersion Version { get; set; }
        public string HtmlUrl { get; set; }
        public bool IsPreRelease { get; set; }
        public DateTime PublishedAt { get; set; }
        public List<ReleaseAsset> Assets { get; set; } = new List<ReleaseAsset>();
    }

    /// <summary>
    /// Checks GitHub releases for newer PRoCon versions and downloads installers.
    /// Alpha channel checks for -alpha tags; stable channel ignores pre-release tags.
    /// </summary>
    public class UpdateChecker : IDisposable
    {
        private const string ReleasesUrl = "https://api.github.com/repos/AdKats/Procon-1/releases";
        private const string UserAgent = "PRoCon/2.0";
        private static readonly TimeSpan DefaultCheckInterval = TimeSpan.FromHours(4);
        private static readonly TimeSpan FirstCheckDelay = TimeSpan.FromSeconds(30);

        private readonly HttpClient _httpClient;
        private readonly SemanticVersion _currentVersion;
        private readonly bool _includePreReleases;
        private Timer _timer;
        private CancellationTokenSource _cts;
        private SemanticVersion? _lastReportedVersion;
        private bool _disposed;

        /// <summary>
        /// Fires when a newer version is found. Includes release metadata and asset list.
        /// </summary>
        public event Action<UpdateInfo> UpdateAvailable;

        public UpdateChecker(string currentVersion, bool includePreReleases)
        {
            SemanticVersion.TryParse(currentVersion, out _currentVersion);
            _includePreReleases = includePreReleases;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("PRoCon", "2.0"));
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Starts periodic update checks. First check after 30 seconds, then every 4 hours.
        /// </summary>
        public void StartPeriodicCheck()
        {
            StartPeriodicCheck(DefaultCheckInterval);
        }

        public void StartPeriodicCheck(TimeSpan interval)
        {
            _cts = new CancellationTokenSource();
            _timer = new Timer(_ =>
            {
                // Fire-and-forget but catch all exceptions to prevent process crash
                _ = Task.Run(async () =>
                {
                    try { await CheckSilently().ConfigureAwait(false); }
                    catch { /* must never crash */ }
                });
            }, null, FirstCheckDelay, interval);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Forces an immediate update check, resetting the "already reported" guard.
        /// </summary>
        public void ForceCheck()
        {
            _lastReportedVersion = null;
            _ = Task.Run(async () =>
            {
                try { await CheckSilently().ConfigureAwait(false); }
                catch { }
            });
        }

        private async Task CheckSilently()
        {
            try
            {
                var ct = _cts?.Token ?? CancellationToken.None;
                var update = await CheckForUpdateAsync(ct).ConfigureAwait(false);
                if (update != null)
                {
                    // Only fire event once per version
                    if (_lastReportedVersion == null || update.Version > _lastReportedVersion.Value)
                    {
                        _lastReportedVersion = update.Version;
                        UpdateAvailable?.Invoke(update);
                    }
                }
            }
            catch
            {
                // Update check failures must never crash the app
            }
        }

        /// <summary>
        /// Checks GitHub releases for a newer version. Returns null if up to date.
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdateAsync(CancellationToken ct = default)
        {
            using var response = await _httpClient.GetAsync(ReleasesUrl, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JArray releases = JArray.Parse(json);

            UpdateInfo best = null;
            SemanticVersion bestVersion = _currentVersion;

            foreach (JToken release in releases)
            {
                ct.ThrowIfCancellationRequested();

                string tagName = release.Value<string>("tag_name");
                if (string.IsNullOrEmpty(tagName))
                    continue;

                // Only consider v2.x releases
                if (!tagName.StartsWith("v2", StringComparison.OrdinalIgnoreCase))
                    continue;

                bool isPreRelease = release.Value<bool>("prerelease");

                // Stable channel: skip any tag containing '-' (only accept stable releases)
                if (!_includePreReleases && tagName.Contains('-'))
                    continue;

                // Pre-release channel: include all pre-releases (alpha, beta, rc, dev)
                // Stable releases are also included since they are always newer than pre-releases
                // of the same version per semver rules

                if (!SemanticVersion.TryParse(tagName, out SemanticVersion version))
                    continue;

                if (version > bestVersion)
                {
                    bestVersion = version;
                    best = new UpdateInfo
                    {
                        TagName = tagName,
                        Version = version,
                        HtmlUrl = release.Value<string>("html_url"),
                        IsPreRelease = isPreRelease,
                        PublishedAt = release.Value<DateTime>("published_at"),
                        Assets = ParseAssets(release["assets"])
                    };
                }
            }

            return best;
        }

        /// <summary>
        /// Downloads the installer .exe from a release. Reports progress 0.0-1.0.
        /// Returns the local file path of the downloaded installer.
        /// </summary>
        public async Task<string> DownloadInstallerAsync(UpdateInfo update, IProgress<double> progress = null, CancellationToken ct = default)
        {
            // Find the Setup.exe asset
            ReleaseAsset installerAsset = update.Assets
                .FirstOrDefault(a => a.Name.EndsWith("-Setup.exe", StringComparison.OrdinalIgnoreCase));

            if (installerAsset == null)
                return null;

            string downloadDir = Path.Combine(Path.GetTempPath(), "PRoCon-Updates");
            Directory.CreateDirectory(downloadDir);
            string filePath = Path.Combine(downloadDir, installerAsset.Name);

            // Delete any previous download of the same file
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (HttpResponseMessage response = await _httpClient.GetAsync(installerAsset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? installerAsset.Size;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    byte[] buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                        totalRead += bytesRead;

                        if (totalBytes > 0)
                            progress?.Report((double)totalRead / totalBytes);
                    }
                }
            }

            return filePath;
        }

        private static List<ReleaseAsset> ParseAssets(JToken assetsToken)
        {
            var assets = new List<ReleaseAsset>();
            if (assetsToken == null)
                return assets;

            foreach (JToken asset in assetsToken)
            {
                assets.Add(new ReleaseAsset
                {
                    Name = asset.Value<string>("name"),
                    DownloadUrl = asset.Value<string>("browser_download_url"),
                    Size = asset.Value<long>("size")
                });
            }
            return assets;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try { _cts?.Cancel(); } catch { }
                _cts?.Dispose();
                _timer?.Dispose();
                _httpClient?.Dispose();
            }
        }
    }
}
