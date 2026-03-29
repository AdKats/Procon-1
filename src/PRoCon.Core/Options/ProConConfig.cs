using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PRoCon.Core.Options
{
    /// <summary>
    /// JSON configuration model for PRoCon v2. Replaces the legacy procon.cfg command-based format.
    /// Legacy procon.cfg is still loaded for backwards compatibility but new saves always use JSON.
    /// </summary>
    public class ProConConfig
    {
        [JsonProperty("version")]
        public int Version { get; set; } = 2;

        [JsonProperty("window")]
        public WindowConfig Window { get; set; } = new WindowConfig();

        [JsonProperty("options")]
        public OptionsConfig Options { get; set; } = new OptionsConfig();

        [JsonProperty("servers")]
        public List<ServerConfig> Servers { get; set; } = new List<ServerConfig>();

        [JsonProperty("accounts")]
        public List<AccountConfig> Accounts { get; set; } = new List<AccountConfig>();
    }

    public class WindowConfig
    {
        [JsonProperty("state"), JsonConverter(typeof(StringEnumConverter))]
        public FormWindowState State { get; set; } = FormWindowState.Normal;

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; } = 1024;

        [JsonProperty("height")]
        public int Height { get; set; } = 768;
    }

    public class OptionsConfig
    {
        [JsonProperty("language")]
        public string Language { get; set; } = "au.loc";

        [JsonProperty("chatLogging")]
        public bool ChatLogging { get; set; }

        [JsonProperty("consoleLogging")]
        public bool ConsoleLogging { get; set; }

        [JsonProperty("eventsLogging")]
        public bool EventsLogging { get; set; }

        [JsonProperty("pluginLogging")]
        public bool PluginLogging { get; set; }

        [JsonProperty("showTrayIcon")]
        public bool ShowTrayIcon { get; set; }

        [JsonProperty("minimizeToTray")]
        public bool MinimizeToTray { get; set; }

        [JsonProperty("closeToTray")]
        public bool CloseToTray { get; set; }

        [JsonProperty("runPluginsInSandbox")]
        public bool RunPluginsInSandbox { get; set; }

        [JsonProperty("allowAllODBCConnections")]
        public bool AllowAllODBCConnections { get; set; }

        [JsonProperty("allowAllSmtpConnections")]
        public bool AllowAllSmtpConnections { get; set; }

        [JsonProperty("adminMoveMessage")]
        public bool AdminMoveMessage { get; set; }

        [JsonProperty("chatDisplayAdminName")]
        public bool ChatDisplayAdminName { get; set; }

        [JsonProperty("enableAdminReason")]
        public bool EnableAdminReason { get; set; }

        [JsonProperty("layerHideLocalPlugins")]
        public bool LayerHideLocalPlugins { get; set; } = true;

        [JsonProperty("layerHideLocalAccounts")]
        public bool LayerHideLocalAccounts { get; set; } = true;

        [JsonProperty("showRoundTimerConstantly")]
        public bool ShowRoundTimerConstantly { get; set; }

        [JsonProperty("showCfmMsgRoundRestartNext")]
        public bool ShowCfmMsgRoundRestartNext { get; set; } = true;

        [JsonProperty("showDICESpecialOptions")]
        public bool ShowDICESpecialOptions { get; set; }

        [JsonProperty("useGeoIpFileOnly")]
        public bool UseGeoIpFileOnly { get; set; }

        [JsonProperty("blockRssFeedNews")]
        public bool BlockRssFeedNews { get; set; }

        [JsonProperty("usePluginOldStyleLoad")]
        public bool UsePluginOldStyleLoad { get; set; }

        [JsonProperty("enablePluginDebugging")]
        public bool EnablePluginDebugging { get; set; }

        [JsonProperty("pluginMaxRuntimeMinutes")]
        public int PluginMaxRuntimeMinutes { get; set; }

        [JsonProperty("pluginMaxRuntimeSeconds")]
        public int PluginMaxRuntimeSeconds { get; set; } = 59;

        [JsonProperty("statsLinksMaxNum")]
        public int StatsLinksMaxNum { get; set; } = 4;

        [JsonProperty("proxyCheckApiKey")]
        public string ProxyCheckApiKey { get; set; } = "";

        [JsonProperty("trustedHosts")]
        public List<TrustedHostConfig> TrustedHosts { get; set; } = new List<TrustedHostConfig>();

        [JsonProperty("statsLinks")]
        public List<StatsLinkConfig> StatsLinks { get; set; } = new List<StatsLinkConfig>();
    }

    public class TrustedHostConfig
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }
    }

    public class StatsLinkConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class ServerConfig
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; } = "";

        [JsonProperty("username")]
        public string Username { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("autoConnect")]
        public bool AutoConnect { get; set; }
    }

    public class AccountConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
