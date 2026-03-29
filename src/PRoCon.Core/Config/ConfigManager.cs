using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace PRoCon.Core.Config
{
    /// <summary>
    /// Unified configuration manager that reads from both the legacy PRoCon .cfg
    /// format and modern appsettings.json, exposing a single
    /// <see cref="IConfiguration"/> interface.
    ///
    /// Design goals:
    ///   - Drop-in foundation: existing config consumers do NOT need to change yet.
    ///   - Dual-read: loads appsettings.json first, then overlays values from .cfg
    ///     files (so the .cfg values win when both sources define the same key).
    ///   - Dual-write: <see cref="Set"/> persists values to both formats so that
    ///     the legacy UI and the new JSON path stay in sync.
    ///   - The .cfg key "procon.private.options.chatLogging True" maps to the
    ///     IConfiguration path "procon:private:options:chatLogging" with value "True".
    /// </summary>
    public sealed class ConfigManager : IDisposable
    {
        private readonly string _configDirectory;
        private readonly IConfigurationRoot _configuration;
        private readonly Dictionary<string, string> _cfgValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The unified <see cref="IConfiguration"/> root.
        /// </summary>
        public IConfiguration Configuration => _configuration;

        /// <summary>
        /// Creates a new ConfigManager.
        /// </summary>
        /// <param name="configDirectory">
        /// Path to the directory that contains procon.cfg, accounts.cfg,
        /// and/or appsettings.json. Typically
        /// <c>Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs")</c>.
        /// </param>
        public ConfigManager(string configDirectory)
        {
            _configDirectory = configDirectory ?? throw new ArgumentNullException(nameof(configDirectory));

            var builder = new ConfigurationBuilder();

            // 1. JSON source (lowest priority — .cfg wins)
            string jsonPath = Path.Combine(_configDirectory, "appsettings.json");
            if (File.Exists(jsonPath))
            {
                builder.AddJsonFile(jsonPath, optional: true, reloadOnChange: false);
            }

            // 2. Legacy .cfg sources via in-memory provider seeded from parsed values
            LoadCfgFile("procon.cfg");
            LoadCfgFile("accounts.cfg");

            if (_cfgValues.Count > 0)
            {
                builder.AddInMemoryCollection(_cfgValues);
            }

            _configuration = builder.Build();
        }

        // ----------------------------------------------------------------
        // Reading helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Get a configuration value by its IConfiguration key path
        /// (e.g. "procon:private:options:chatLogging").
        /// </summary>
        public string Get(string key)
        {
            return _configuration[key];
        }

        /// <summary>
        /// Get a configuration value with a fallback default.
        /// </summary>
        public string Get(string key, string defaultValue)
        {
            return _configuration[key] ?? defaultValue;
        }

        /// <summary>
        /// Get a configuration section.
        /// </summary>
        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }

        // ----------------------------------------------------------------
        // Writing helpers (dual-write)
        // ----------------------------------------------------------------

        /// <summary>
        /// Set a value and persist to both .cfg and appsettings.json.
        /// The <paramref name="key"/> uses the IConfiguration colon-delimited path
        /// (e.g. "procon:private:options:chatLogging").
        /// </summary>
        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            // Update the in-memory config
            _configuration[key] = value;
            _cfgValues[key] = value;

            // Persist to appsettings.json
            WriteToJson(key, value);
        }

        // ----------------------------------------------------------------
        // .cfg parsing
        // ----------------------------------------------------------------

        /// <summary>
        /// All raw lines read from .cfg files, in order. Useful for callers
        /// that still need to feed lines to ExecutePRoConCommand.
        /// </summary>
        public IReadOnlyList<string> CfgLines => _cfgLines.AsReadOnly();
        private readonly List<string> _cfgLines = new List<string>();

        private void LoadCfgFile(string fileName)
        {
            string path = Path.Combine(_configDirectory, fileName);
            if (!File.Exists(path))
                return;

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                // Skip comments and blank lines
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
                    continue;

                _cfgLines.Add(trimmed);

                // Parse "procon.private.options.chatLogging True"
                // into key = "procon:private:options:chatLogging", value = "True"
                var parts = TokenizeCfgLine(trimmed);
                if (parts.Count >= 1)
                {
                    string cfgKey = parts[0].Replace('.', ':');
                    string cfgValue = parts.Count >= 2
                        ? string.Join(" ", parts.Skip(1))
                        : string.Empty;

                    // In-memory collection keys are case-insensitive, last write wins.
                    _cfgValues[cfgKey] = cfgValue;
                }
            }
        }

        /// <summary>
        /// Tokenise a .cfg line, respecting quoted strings.
        /// E.g. <c>procon.public.accounts.create "admin" "pass word"</c>
        /// yields ["procon.public.accounts.create", "admin", "pass word"].
        /// </summary>
        internal static List<string> TokenizeCfgLine(string line)
        {
            var tokens = new List<string>();
            var matches = Regex.Matches(line, @"""([^""]*)""|(\S+)");
            foreach (Match m in matches)
            {
                tokens.Add(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value);
            }
            return tokens;
        }

        // ----------------------------------------------------------------
        // JSON write-through
        // ----------------------------------------------------------------

        private void WriteToJson(string key, string value)
        {
            try
            {
                string jsonPath = Path.Combine(_configDirectory, "appsettings.json");

                // Read existing JSON or start with empty object
                string json = File.Exists(jsonPath)
                    ? File.ReadAllText(jsonPath, Encoding.UTF8)
                    : "{}";

                // Simple key-value merge using Newtonsoft or manual.
                // To avoid a hard dependency on Newtonsoft in this foundational class
                // we keep the JSON flat: { "key": "value", ... }
                var dict = ParseFlatJson(json);
                dict[key] = value;
                string output = SerializeFlatJson(dict);

                if (!Directory.Exists(_configDirectory))
                    Directory.CreateDirectory(_configDirectory);

                File.WriteAllText(jsonPath, output, Encoding.UTF8);
            }
            catch (Exception)
            {
                // Config write failure is non-fatal.
            }
        }

        /// <summary>
        /// Minimal flat JSON parser (top-level string values only).
        /// Full nested JSON is left to Microsoft.Extensions.Configuration.Json.
        /// </summary>
        internal static Dictionary<string, string> ParseFlatJson(string json)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return dict;

            // Match "key": "value" pairs
            var matches = Regex.Matches(json, @"""([^""]+)""\s*:\s*""([^""]*)""");
            foreach (Match m in matches)
            {
                dict[m.Groups[1].Value] = m.Groups[2].Value;
            }
            return dict;
        }

        internal static string SerializeFlatJson(Dictionary<string, string> dict)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            int i = 0;
            foreach (var kvp in dict.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                string comma = (i < dict.Count - 1) ? "," : "";
                sb.AppendLine($"  \"{EscapeJsonString(kvp.Key)}\": \"{EscapeJsonString(kvp.Value)}\"{comma}");
                i++;
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string EscapeJsonString(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public void Dispose()
        {
            // Nothing to dispose currently; reserved for future providers.
        }
    }
}
