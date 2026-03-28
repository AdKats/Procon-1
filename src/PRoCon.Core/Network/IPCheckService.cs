using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PRoCon.Core.Network
{
    public class IPCheckResult
    {
        public string IP { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Provider { get; set; }
        public bool IsProxy { get; set; }
        public bool IsVPN { get; set; }
        public bool IsTor { get; set; }
        public int Risk { get; set; }
        public DateTime CachedAt { get; set; }
        public bool FromCache { get; set; }
    }

    public class IPCheckService : IDisposable
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly ConcurrentDictionary<string, IPCheckResult> _memoryCache = new();
        private readonly SemaphoreSlim _rateLimiter = new(5, 5); // max 5 concurrent requests
        private readonly string _cacheDir;
        private string _apiKey;
        private int _dailyQueries;
        private DateTime _queryCountResetDate;
        private const int FreeQueryLimit = 950; // stay under 1K
        private static readonly TimeSpan CacheTTL = TimeSpan.FromHours(48);

        public IPCheckService(string cacheDirectory, string apiKey = null)
        {
            _cacheDir = cacheDirectory;
            _apiKey = apiKey;
            _queryCountResetDate = DateTime.UtcNow.Date;

            if (!Directory.Exists(_cacheDir))
                Directory.CreateDirectory(_cacheDir);

            LoadQueryCount();
        }

        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        public int DailyQueriesUsed => _dailyQueries;
        public int DailyQueryLimit => string.IsNullOrEmpty(_apiKey) ? FreeQueryLimit : 100000;

        public async Task<IPCheckResult> LookupAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "none" || ip.StartsWith("127.") || ip == "0.0.0.0")
                return null;

            // Strip port if present
            int colonIdx = ip.LastIndexOf(':');
            if (colonIdx > 0 && !ip.Contains("::")) // not IPv6
                ip = ip.Substring(0, colonIdx);

            // Check memory cache
            if (_memoryCache.TryGetValue(ip, out var cached) && DateTime.UtcNow - cached.CachedAt < CacheTTL)
            {
                cached.FromCache = true;
                return cached;
            }

            // Check disk cache
            var diskResult = LoadFromDisk(ip);
            if (diskResult != null && DateTime.UtcNow - diskResult.CachedAt < CacheTTL)
            {
                diskResult.FromCache = true;
                _memoryCache[ip] = diskResult;
                return diskResult;
            }

            // Check daily query limit
            ResetDailyCountIfNeeded();
            if (_dailyQueries >= DailyQueryLimit)
            {
                // Return stale cache if available, otherwise null
                return diskResult ?? cached;
            }

            // Rate limit
            if (!await _rateLimiter.WaitAsync(TimeSpan.FromSeconds(5)))
                return diskResult ?? cached;

            try
            {
                var result = await FetchFromAPI(ip);
                if (result != null)
                {
                    result.CachedAt = DateTime.UtcNow;
                    _memoryCache[ip] = result;
                    SaveToDisk(result);
                    Interlocked.Increment(ref _dailyQueries);
                    SaveQueryCount();
                }
                return result;
            }
            catch
            {
                return diskResult ?? cached;
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        private async Task<IPCheckResult> FetchFromAPI(string ip)
        {
            string url = $"https://proxycheck.io/v3/{ip}";
            if (!string.IsNullOrEmpty(_apiKey))
                url += $"?key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);

            if (root["status"]?.ToString() != "ok")
                return null;

            var ipData = root[ip] as JObject;
            if (ipData == null)
                return null;

            var location = ipData["location"] as JObject;
            var network = ipData["network"] as JObject;
            var detections = ipData["detections"] as JObject;

            return new IPCheckResult
            {
                IP = ip,
                CountryName = location?["country_name"]?.ToString() ?? "",
                CountryCode = location?["country_code"]?.ToString() ?? "",
                City = location?["city_name"]?.ToString() ?? "",
                Provider = network?["provider"]?.ToString() ?? network?["organisation"]?.ToString() ?? "",
                IsProxy = detections?["proxy"]?.Value<bool>() ?? false,
                IsVPN = detections?["vpn"]?.Value<bool>() ?? false,
                IsTor = detections?["tor"]?.Value<bool>() ?? false,
                Risk = detections?["risk"]?.Value<int>() ?? 0,
            };
        }

        private IPCheckResult LoadFromDisk(string ip)
        {
            try
            {
                string path = GetCachePath(ip);
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<IPCheckResult>(json);
            }
            catch { return null; }
        }

        private void SaveToDisk(IPCheckResult result)
        {
            try
            {
                string path = GetCachePath(result.IP);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                File.WriteAllText(path, json);
            }
            catch { }
        }

        private string GetCachePath(string ip)
        {
            string safe = ip.Replace('.', '_').Replace(':', '_');
            return Path.Combine(_cacheDir, $"{safe}.json");
        }

        private void ResetDailyCountIfNeeded()
        {
            if (DateTime.UtcNow.Date > _queryCountResetDate)
            {
                _dailyQueries = 0;
                _queryCountResetDate = DateTime.UtcNow.Date;
                SaveQueryCount();
            }
        }

        private void LoadQueryCount()
        {
            try
            {
                string path = Path.Combine(_cacheDir, "_query_count.json");
                if (File.Exists(path))
                {
                    var obj = JObject.Parse(File.ReadAllText(path));
                    var date = obj["date"]?.Value<DateTime>() ?? DateTime.MinValue;
                    if (date.Date == DateTime.UtcNow.Date)
                        _dailyQueries = obj["count"]?.Value<int>() ?? 0;
                    _queryCountResetDate = DateTime.UtcNow.Date;
                }
            }
            catch { }
        }

        private void SaveQueryCount()
        {
            try
            {
                string path = Path.Combine(_cacheDir, "_query_count.json");
                var obj = new JObject
                {
                    ["date"] = DateTime.UtcNow.Date,
                    ["count"] = _dailyQueries
                };
                File.WriteAllText(path, obj.ToString());
            }
            catch { }
        }

        public void Dispose()
        {
            _rateLimiter?.Dispose();
        }
    }
}
