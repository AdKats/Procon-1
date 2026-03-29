using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PRoCon.Core
{
    /// <summary>
    /// Centralized data directory resolution for PRoCon v2.
    /// The exe can live anywhere — all user data (configs, plugins, logs, cache) goes to a
    /// platform-appropriate location.
    ///
    /// Windows:      %APPDATA%\PRoCon\
    /// Linux:        ~/.config/procon/   ($XDG_CONFIG_HOME/procon/ if set)
    /// macOS:        ~/Library/Application Support/PRoCon/
    /// Docker/K8s:   /config/
    ///
    /// Override: set PROCON_DATA_DIR environment variable, or pass --datadir on command line.
    /// </summary>
    public static class ProConPaths
    {
        private static string _dataDirectory;

        /// <summary>
        /// Root data directory for all PRoCon user data.
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                if (_dataDirectory == null)
                    _dataDirectory = ResolveDataDirectory();
                return _dataDirectory;
            }
        }

        /// <summary>
        /// Override the data directory (e.g. from --datadir command line arg).
        /// Must be called before any other path access.
        /// </summary>
        public static void SetDataDirectory(string path)
        {
            _dataDirectory = Path.GetFullPath(path);
        }

        // Convenience accessors for common subdirectories
        public static string ConfigsDirectory => Path.Combine(DataDirectory, "Configs");
        public static string PluginsDirectory => Path.Combine(DataDirectory, "Plugins");
        public static string LogsDirectory => Path.Combine(DataDirectory, "Logs");
        public static string LocalizationDirectory => Path.Combine(DataDirectory, "Localization");
        public static string MediaDirectory => Path.Combine(DataDirectory, "Media");
        public static string CacheDirectory => Path.Combine(DataDirectory, "Cache");
        public static string ImportDirectory => Path.Combine(DataDirectory, "Import");

        /// <summary>
        /// Ensures core directories exist. Called once at startup.
        /// </summary>
        public static readonly string[] GameTypes = { "BF3", "BF4", "BFBC2", "BFHL", "MOH", "MOHW" };

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(ConfigsDirectory);
            Directory.CreateDirectory(PluginsDirectory);
            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(CacheDirectory);

            foreach (string gameType in GameTypes)
                Directory.CreateDirectory(Path.Combine(PluginsDirectory, gameType));
        }

        /// <summary>
        /// Where the executable lives. Use only for loading assemblies/binaries.
        /// </summary>
        public static string ApplicationDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// True if running inside a Docker container or Kubernetes pod.
        /// </summary>
        public static bool IsContainer { get; private set; }

        private static string ResolveDataDirectory()
        {
            // 1. Environment variable override (highest priority)
            string envDir = Environment.GetEnvironmentVariable("PROCON_DATA_DIR");
            if (!string.IsNullOrWhiteSpace(envDir))
                return Path.GetFullPath(envDir);

            // 2. Docker / Kubernetes detection → /config/
            if (DetectContainer())
            {
                IsContainer = true;
                return "/config";
            }

            // 3. Portable mode: if a "Configs" folder exists next to the exe, use exe directory.
            //    Preserves backward compat for existing installs and manual server deployments.
            string portableConfigs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            if (Directory.Exists(portableConfigs))
                return AppDomain.CurrentDomain.BaseDirectory;

            // 4. Platform-specific default
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // %APPDATA%\PRoCon\
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PRoCon");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // ~/Library/Application Support/PRoCon/
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PRoCon");
            }
            else
            {
                // Linux: $XDG_CONFIG_HOME/procon/ or ~/.config/procon/
                string xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (!string.IsNullOrWhiteSpace(xdg))
                    return Path.Combine(xdg, "procon");

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config", "procon");
            }
        }

        private static bool DetectContainer()
        {
            // Docker: /.dockerenv file exists
            if (File.Exists("/.dockerenv"))
                return true;

            // Kubernetes: KUBERNETES_SERVICE_HOST is always set in pods
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST")))
                return true;

            // Docker (fallback): check cgroup for docker/containerd/kubepods
            try
            {
                if (File.Exists("/proc/1/cgroup"))
                {
                    string cgroup = File.ReadAllText("/proc/1/cgroup");
                    if (cgroup.Contains("docker") || cgroup.Contains("kubepods") || cgroup.Contains("containerd"))
                        return true;
                }
            }
            catch { }

            return false;
        }
    }
}
