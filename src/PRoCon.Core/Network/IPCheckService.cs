using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
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

    internal class IPCacheRow
    {
        public string ip { get; set; }
        public string country_name { get; set; }
        public string country_code { get; set; }
        public string city { get; set; }
        public string provider { get; set; }
        public int is_proxy { get; set; }
        public int is_vpn { get; set; }
        public int is_tor { get; set; }
        public int risk { get; set; }
        public string cached_at { get; set; }

        public IPCheckResult ToResult() => new IPCheckResult
        {
            IP = ip,
            CountryName = country_name ?? "",
            CountryCode = country_code ?? "",
            City = city ?? "",
            Provider = provider ?? "",
            IsProxy = is_proxy == 1,
            IsVPN = is_vpn == 1,
            IsTor = is_tor == 1,
            Risk = risk,
            CachedAt = DateTime.Parse(cached_at, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        };
    }

    internal class QueryCountRow
    {
        public string date { get; set; }
        public int count { get; set; }
    }

    public class IPCheckService : IDisposable
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly ConcurrentDictionary<string, IPCheckResult> _memoryCache = new();
        private readonly SemaphoreSlim _rateLimiter = new(5, 5);
        private readonly object _dailyCountLock = new();
        private readonly object _dbLock = new();
        private readonly SqliteConnection _conn;
        private string _apiKey;
        private int _dailyQueries;
        private DateTime _queryCountResetDate;
        private const int FreeQueryLimit = 950;
        private static readonly TimeSpan CacheTTL = TimeSpan.FromHours(48);

        public IPCheckService(string cacheDirectory, string apiKey = null)
        {
            _apiKey = apiKey;
            _queryCountResetDate = DateTime.UtcNow.Date;

            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            string dbPath = Path.Combine(cacheDirectory, "ipcache.db");
            _conn = new SqliteConnection($"Data Source={dbPath}");
            _conn.Open();

            // WAL mode allows concurrent reads and avoids "database is locked"
            _conn.Execute("PRAGMA journal_mode=WAL;");

            InitializeDatabase();
            LoadQueryCount();
        }

        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        public int DailyQueriesUsed => _dailyQueries;
        public int DailyQueryLimit => string.IsNullOrEmpty(_apiKey) ? FreeQueryLimit : 100000;

        private void InitializeDatabase()
        {
            lock (_dbLock)
            {
                _conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS ip_cache (
                        ip TEXT PRIMARY KEY,
                        country_name TEXT,
                        country_code TEXT,
                        city TEXT,
                        provider TEXT,
                        is_proxy INTEGER,
                        is_vpn INTEGER,
                        is_tor INTEGER,
                        risk INTEGER,
                        cached_at TEXT
                    );

                    CREATE TABLE IF NOT EXISTS query_count (
                        id INTEGER PRIMARY KEY CHECK (id = 1),
                        date TEXT,
                        count INTEGER
                    );

                    CREATE INDEX IF NOT EXISTS idx_ip_cache_cached_at ON ip_cache(cached_at);
                ");
            }
        }

        public async Task<IPCheckResult> LookupAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "none" || ip.StartsWith("127.") || ip == "0.0.0.0")
                return null;

            // Strip port if present
            int colonIdx = ip.LastIndexOf(':');
            if (colonIdx > 0 && !ip.Contains("::"))
                ip = ip.Substring(0, colonIdx);

            // Check memory cache
            if (_memoryCache.TryGetValue(ip, out var cached) && DateTime.UtcNow - cached.CachedAt < CacheTTL)
            {
                cached.FromCache = true;
                return cached;
            }

            // Check SQLite cache
            var dbResult = LoadFromDb(ip);
            if (dbResult != null && DateTime.UtcNow - dbResult.CachedAt < CacheTTL)
            {
                dbResult.FromCache = true;
                _memoryCache[ip] = dbResult;
                return dbResult;
            }

            // Check daily query limit (thread-safe)
            lock (_dailyCountLock)
            {
                ResetDailyCountIfNeeded();
                if (_dailyQueries >= DailyQueryLimit)
                    return dbResult ?? cached;
            }

            // Rate limit
            if (!await _rateLimiter.WaitAsync(TimeSpan.FromSeconds(5)))
                return dbResult ?? cached;

            try
            {
                var result = await FetchFromAPI(ip);
                if (result != null)
                {
                    result.CachedAt = DateTime.UtcNow;
                    _memoryCache[ip] = result;
                    SaveToDb(result);

                    lock (_dailyCountLock)
                    {
                        _dailyQueries++;
                        SaveQueryCount();
                    }
                }
                return result;
            }
            catch
            {
                return dbResult ?? cached;
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

        private IPCheckResult LoadFromDb(string ip)
        {
            try
            {
                lock (_dbLock)
                {
                    var row = _conn.QueryFirstOrDefault<IPCacheRow>(
                        "SELECT * FROM ip_cache WHERE ip = @ip", new { ip });
                    return row?.ToResult();
                }
            }
            catch { return null; }
        }

        private void SaveToDb(IPCheckResult result)
        {
            try
            {
                lock (_dbLock)
                {
                    _conn.Execute(@"
                        INSERT OR REPLACE INTO ip_cache
                            (ip, country_name, country_code, city, provider, is_proxy, is_vpn, is_tor, risk, cached_at)
                        VALUES
                            (@ip, @country_name, @country_code, @city, @provider, @is_proxy, @is_vpn, @is_tor, @risk, @cached_at)",
                        new
                        {
                            ip = result.IP,
                            country_name = result.CountryName ?? "",
                            country_code = result.CountryCode ?? "",
                            city = result.City ?? "",
                            provider = result.Provider ?? "",
                            is_proxy = result.IsProxy ? 1 : 0,
                            is_vpn = result.IsVPN ? 1 : 0,
                            is_tor = result.IsTor ? 1 : 0,
                            risk = result.Risk,
                            cached_at = result.CachedAt.ToString("O"),
                        });
                }
            }
            catch { }
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
                lock (_dbLock)
                {
                    var row = _conn.QueryFirstOrDefault<QueryCountRow>(
                        "SELECT date, count FROM query_count WHERE id = 1");

                    if (row != null && DateTime.Parse(row.date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).Date == DateTime.UtcNow.Date)
                        _dailyQueries = row.count;

                    _queryCountResetDate = DateTime.UtcNow.Date;
                }
            }
            catch { }
        }

        private void SaveQueryCount()
        {
            try
            {
                lock (_dbLock)
                {
                    _conn.Execute(@"
                        INSERT OR REPLACE INTO query_count (id, date, count)
                        VALUES (1, @date, @count)",
                        new { date = DateTime.UtcNow.Date.ToString("O"), count = _dailyQueries });
                }
            }
            catch { }
        }

        /// <summary>
        /// Removes expired entries from the database.
        /// </summary>
        public int PurgeExpiredEntries()
        {
            try
            {
                lock (_dbLock)
                {
                    return _conn.Execute(
                        "DELETE FROM ip_cache WHERE cached_at < @cutoff",
                        new { cutoff = (DateTime.UtcNow - CacheTTL).ToString("O") });
                }
            }
            catch { return 0; }
        }

        public void Dispose()
        {
            _rateLimiter?.Dispose();
            lock (_dbLock)
            {
                _conn?.Close();
                _conn?.Dispose();
            }
        }
    }
}
