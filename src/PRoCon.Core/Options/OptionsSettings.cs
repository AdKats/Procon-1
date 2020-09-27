using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

// This can be moved into .Core once the contents of PRoCon.Plugin.* have been moved to .Core.
namespace PRoCon.Core.Options
{
    public class OptionsSettings
    {

        private PRoConApplication m_praApplication;

        public delegate void OptionsEnabledHandler(bool blEnabled);
        public event OptionsEnabledHandler ConsoleLoggingChanged;
        public event OptionsEnabledHandler EventsLoggingChanged;
        public event OptionsEnabledHandler PluginsLoggingChanged;
        public event OptionsEnabledHandler ChatLoggingChanged;
        public event OptionsEnabledHandler AutoCheckDownloadUpdatesChanged;
        public event OptionsEnabledHandler AutoApplyUpdatesChanged;
        public event OptionsEnabledHandler AutoCheckGameConfigsForUpdatesChanged;
        public event OptionsEnabledHandler ShowTrayIconChanged;
        public event OptionsEnabledHandler CloseToTrayChanged;
        public event OptionsEnabledHandler MinimizeToTrayChanged;

        public event OptionsEnabledHandler RunPluginsInTrustedSandboxChanged;
        public event OptionsEnabledHandler AllowAllODBCConnectionsChanged;
        public event OptionsEnabledHandler AllowAllSmtpConnectionsChanged;

        public event OptionsEnabledHandler AdminMoveMessageChanged;
        public event OptionsEnabledHandler ChatDisplayAdminNameChanged;
        public event OptionsEnabledHandler EnableAdminReasonChanged;

        public event OptionsEnabledHandler LayerHideLocalPluginsChanged;
        public event OptionsEnabledHandler LayerHideLocalAccountsChanged;

        public event OptionsEnabledHandler ShowRoundTimerConstantlyChanged;
        public event OptionsEnabledHandler ShowCfmMsgRoundRestartNextChanged;
        public event OptionsEnabledHandler ShowDICESpecialOptionsChanged;

        public event OptionsEnabledHandler AllowAnonymousUsageDataChanged;

        public event OptionsEnabledHandler UseGeoIpFileOnlyChanged;
        public event OptionsEnabledHandler BlockRssFeedNewsChanged;

        public event OptionsEnabledHandler UsePluginOldStyleLoadChanged;

        public event OptionsEnabledHandler EnablePluginDebuggingChanged;

        private bool m_isConsoleLoggingEnabled;
        public bool ConsoleLogging
        {
            get
            {
                return this.m_isConsoleLoggingEnabled;
            }
            set
            {
                this.m_isConsoleLoggingEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ConsoleLoggingChanged != null)
                {
                    this.ConsoleLoggingChanged(value);
                }
            }
        }

        private bool m_isEventsLoggingEnabled;
        public bool EventsLogging
        {
            get
            {
                return this.m_isEventsLoggingEnabled;
            }
            set
            {
                this.m_isEventsLoggingEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.EventsLoggingChanged != null)
                {
                    this.EventsLoggingChanged(value);
                }
            }
        }

        private bool m_isPluginLoggingEnabled;
        public bool PluginLogging
        {
            get
            {
                return this.m_isPluginLoggingEnabled;
            }
            set
            {
                this.m_isPluginLoggingEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.PluginsLoggingChanged != null)
                {
                    this.PluginsLoggingChanged(value);
                }
            }
        }

        private bool m_isChatLoggingEnabled;
        public bool ChatLogging
        {
            get
            {
                return this.m_isChatLoggingEnabled;
            }
            set
            {
                this.m_isChatLoggingEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ChatLoggingChanged != null)
                {
                    this.ChatLoggingChanged(value);
                }
            }
        }

        private bool m_isAutoCheckDownloadUpdatesEnabled;
        public bool AutoCheckDownloadUpdates
        {
            get
            {
                return this.m_isAutoCheckDownloadUpdatesEnabled;
            }
            set
            {
                if (this.m_praApplication.BlockUpdateChecks == true)
                {
                    this.m_isAutoCheckDownloadUpdatesEnabled = false;
                }
                else
                {
                    this.m_isAutoCheckDownloadUpdatesEnabled = value;
                }

                this.m_praApplication.SaveMainConfig();

                if (this.AutoCheckDownloadUpdatesChanged != null)
                {
                    this.AutoCheckDownloadUpdatesChanged(value);
                }
            }
        }

        private bool m_isAutoApplyUpdatesEnabled;
        public bool AutoApplyUpdates
        {
            get
            {
                return this.m_isAutoApplyUpdatesEnabled;
            }
            set
            {
                this.m_isAutoApplyUpdatesEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.AutoApplyUpdatesChanged != null)
                {
                    this.AutoApplyUpdatesChanged(value);
                }
            }
        }

        private bool m_isAutoCheckGameConfigsForUpdatesEnabled;
        public bool AutoCheckGameConfigsForUpdates
        {
            get
            {
                return this.m_isAutoCheckGameConfigsForUpdatesEnabled;
            }
            set
            {
                if (this.m_praApplication.BlockUpdateChecks == true)
                {
                    this.m_isAutoCheckGameConfigsForUpdatesEnabled = false;
                }
                else
                {
                    this.m_isAutoCheckGameConfigsForUpdatesEnabled = value;
                }

                this.m_praApplication.SaveMainConfig();

                if (this.AutoCheckDownloadUpdatesChanged != null)
                {
                    this.AutoCheckGameConfigsForUpdatesChanged(value);
                }
            }
        }

        private bool m_isTrayIconVisible;
        public bool ShowTrayIcon
        {
            get
            {
                return this.m_isTrayIconVisible;
            }
            set
            {
                this.m_isTrayIconVisible = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ShowTrayIconChanged != null)
                {
                    this.ShowTrayIconChanged(value);
                }
            }
        }

        private bool m_isCloseToTrayEnabled;
        public bool CloseToTray
        {
            get
            {
                return this.m_isCloseToTrayEnabled;
            }
            set
            {
                this.m_isCloseToTrayEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.CloseToTrayChanged != null)
                {
                    this.CloseToTrayChanged(value);
                }
            }
        }

        private bool m_isMinimizeToTrayEnabled;
        public bool MinimizeToTray
        {
            get
            {
                return this.m_isMinimizeToTrayEnabled;
            }
            set
            {
                this.m_isMinimizeToTrayEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.MinimizeToTrayChanged != null)
                {
                    this.MinimizeToTrayChanged(value);
                }
            }
        }

        private bool m_isRunPluginsInTrustedSandboxEnabled;
        public bool RunPluginsInTrustedSandbox
        {
            get
            {
                return this.m_isRunPluginsInTrustedSandboxEnabled;
            }
            set
            {
                this.m_isRunPluginsInTrustedSandboxEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.RunPluginsInTrustedSandboxChanged != null)
                {
                    this.RunPluginsInTrustedSandboxChanged(value);
                }
            }
        }

        private bool m_isAdminMoveMessageEnabled = true;
        public bool AdminMoveMessage
        {
            get
            {
                return this.m_isAdminMoveMessageEnabled;
            }
            set
            {
                this.m_isAdminMoveMessageEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.AdminMoveMessageChanged != null)
                {
                    this.AdminMoveMessageChanged(value);
                }
            }
        }

        // ChatDisplayAdminNameChanged
        private bool m_isChatDisplayAdminNameEnabled = true;
        public bool ChatDisplayAdminName
        {
            get
            {
                return this.m_isChatDisplayAdminNameEnabled;
            }
            set
            {
                this.m_isChatDisplayAdminNameEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ChatDisplayAdminNameChanged != null)
                {
                    this.ChatDisplayAdminNameChanged(value);
                }
            }
        }

        // EnableAdminReason
        private bool m_isEnableAdminReasonEnabled = true;
        public bool EnableAdminReason
        {
            get
            {
                return this.m_isEnableAdminReasonEnabled;
            }
            set
            {
                this.m_isEnableAdminReasonEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.EnableAdminReasonChanged != null)
                {
                    this.EnableAdminReasonChanged(value);
                }
            }
        }

        private bool m_isLayerHideLocalPluginsEnabled = true;
        public bool LayerHideLocalPlugins
        {
            get
            {
                return this.m_isLayerHideLocalPluginsEnabled;
            }
            set
            {
                this.m_isLayerHideLocalPluginsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.LayerHideLocalPluginsChanged != null)
                {
                    this.LayerHideLocalPluginsChanged(value);
                }
            }
        }

        private bool m_isLayerHideLocalAccountsEnabled = true;
        public bool LayerHideLocalAccounts
        {
            get
            {
                return this.m_isLayerHideLocalAccountsEnabled;
            }
            set
            {
                this.m_isLayerHideLocalAccountsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.LayerHideLocalAccountsChanged != null)
                {
                    this.LayerHideLocalAccountsChanged(value);
                }
            }
        }

        // ShowRoundTimerConstantly
        private bool m_isShowRoundTimerConstantlyEnabled;
        public bool ShowRoundTimerConstantly
        {
            get
            {
                return this.m_isShowRoundTimerConstantlyEnabled;
            }
            set
            {
                this.m_isShowRoundTimerConstantlyEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ShowRoundTimerConstantlyChanged != null)
                {
                    this.ShowRoundTimerConstantlyChanged(value);
                }
            }
        }

        // ShowCfmMsgRoundRestartNext
        private bool m_isShowCfmMsgRoundRestartNextEnabled;
        public bool ShowCfmMsgRoundRestartNext
        {
            get
            {
                return this.m_isShowCfmMsgRoundRestartNextEnabled;
            }
            set
            {
                this.m_isShowCfmMsgRoundRestartNextEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ShowCfmMsgRoundRestartNextChanged != null)
                {
                    this.ShowCfmMsgRoundRestartNextChanged(value);
                }
            }
        }

        // ShowDICESpecialOptions
        private bool m_isShowDICESpecialOptionsEnabled;
        public bool ShowDICESpecialOptions
        {
            get
            {
                return this.m_isShowDICESpecialOptionsEnabled;
            }
            set
            {
                this.m_isShowDICESpecialOptionsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.ShowDICESpecialOptionsChanged != null)
                {
                    this.ShowDICESpecialOptionsChanged(value);
                }
            }
        }

        private bool m_isAllowAllODBCConnectionsEnabled;
        public bool AllowAllODBCConnections
        {
            get
            {
                return this.m_isAllowAllODBCConnectionsEnabled;
            }
            set
            {
                this.m_isAllowAllODBCConnectionsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.AllowAllODBCConnectionsChanged != null)
                {
                    this.AllowAllODBCConnectionsChanged(value);
                }
            }
        }

        private bool m_isAllowAllSmtpConnectionsEnabled;
        public bool AllowAllSmtpConnections
        {
            get
            {
                return this.m_isAllowAllSmtpConnectionsEnabled;
            }
            set
            {
                this.m_isAllowAllSmtpConnectionsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.AllowAllSmtpConnectionsChanged != null)
                {
                    this.AllowAllSmtpConnectionsChanged(value);
                }
            }
        }

        private bool m_isAnonymousUsageDataEnabled;
        public bool AllowAnonymousUsageData
        {
            get
            {
                return this.m_isAnonymousUsageDataEnabled;
            }
            set
            {
                this.m_isAnonymousUsageDataEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.AllowAnonymousUsageDataChanged != null)
                {
                    this.AllowAnonymousUsageDataChanged(value);
                }
            }
        }

        private bool m_isUseGeoIpFileOnlyEnabled;
        public bool UseGeoIpFileOnly
        {
            get
            {
                return this.m_isUseGeoIpFileOnlyEnabled;
            }
            set
            {
                this.m_isUseGeoIpFileOnlyEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.UseGeoIpFileOnlyChanged != null)
                {
                    this.UseGeoIpFileOnlyChanged(value);
                }
            }
        }

        private bool m_isBlockRssFeedNewsEnabled;
        public bool BlockRssFeedNews
        {
            get
            {
                return this.m_isBlockRssFeedNewsEnabled;
            }
            set
            {
                this.m_isBlockRssFeedNewsEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.BlockRssFeedNewsChanged != null)
                {
                    this.BlockRssFeedNewsChanged(value);
                }
            }
        }

        public NotificationList<TrustedHostWebsitePort> TrustedHostsWebsitesPorts
        {
            get;
            private set;
        }

        public NotificationList<StatsLinkNameUrl> StatsLinkNameUrl
        {
            get;
            private set;
        }

        public int StatsLinksMaxNum
        {
            get;
            set;
        }

        private int m_PluginMaxRuntime_s;
        public int PluginMaxRuntime_s
        {
            get
            {
                return this.m_PluginMaxRuntime_s;
            }
            set
            {
                if (value <= 0) { value = 10; }
                if (value >= 60) { value = 59; }
                this.m_PluginMaxRuntime_s = value;
                this.m_praApplication.SaveMainConfig();
            }
        }

        private int m_PluginMaxRuntime_m;
        public int PluginMaxRuntime_m
        {
            get
            {
                return this.m_PluginMaxRuntime_m;
            }
            set
            {
                if (value < 0) { value = 0; }
                if (value >= 60) { value = 59; }
                this.m_PluginMaxRuntime_m = value;
                this.m_praApplication.SaveMainConfig();
            }
        }

        private bool m_isPluginMaxRuntimeLocked;
        public bool PluginMaxRuntimeLocked
        {
            get
            {
                return m_isPluginMaxRuntimeLocked;
            }
            set
            {
                this.m_isPluginMaxRuntimeLocked = value;
            }
        }

        private bool m_isUsePluginOldStyleLoadEnabled;
        public bool UsePluginOldStyleLoad
        {
            get
            {
                return this.m_isUsePluginOldStyleLoadEnabled;
            }
            set
            {
                this.m_isUsePluginOldStyleLoadEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.UsePluginOldStyleLoadChanged != null)
                {
                    this.UsePluginOldStyleLoadChanged(value);
                }
            }
        }

        private bool m_isPluginDebuggingEnabled;
        public bool EnablePluginDebugging
        {
            get
            {
                return this.m_isPluginDebuggingEnabled;
            }
            set
            {
                this.m_isPluginDebuggingEnabled = value;
                this.m_praApplication.SaveMainConfig();

                if (this.EnablePluginDebuggingChanged != null)
                {
                    this.EnablePluginDebuggingChanged(value);
                }
            }
        }

        public PermissionSet PluginPermissions
        {

            get
            {

                PermissionSet psetPluginPermissions;

                if (this.RunPluginsInTrustedSandbox == true)
                {

                    psetPluginPermissions = new PermissionSet(PermissionState.None);

                    try
                    {

                        psetPluginPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, AppDomain.CurrentDomain.BaseDirectory));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization")));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs")));
                        psetPluginPermissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media")));
                        psetPluginPermissions.AddPermission(new UIPermission(PermissionState.Unrestricted));
                        psetPluginPermissions.AddPermission(new System.Net.DnsPermission(PermissionState.Unrestricted));

                        // TO DO: rename to something like "Allow all database connections"
                        if (this.AllowAllODBCConnections == true)
                        {
                            psetPluginPermissions.AddPermission(new System.Data.Odbc.OdbcPermission(PermissionState.Unrestricted));
                            // Also allow all MySQL connections when ODBC is enabled, allowing the .NET MySQL connector to work with sandbox enabled
                            psetPluginPermissions.AddPermission(new MySql.Data.MySqlClient.MySqlClientPermission(PermissionState.Unrestricted));
                        }

                        if (this.AllowAllSmtpConnections == true)
                        {
                            psetPluginPermissions.AddPermission(new System.Net.Mail.SmtpPermission(System.Net.Mail.SmtpAccess.ConnectToUnrestrictedPort));
                        }

                        // TO DO: Fixup.
                        psetPluginPermissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
                    }
                    catch (Exception)
                    {

                    }

                    foreach (TrustedHostWebsitePort trusted in this.TrustedHostsWebsitesPorts)
                    {

                        try
                        {
                            Regex rxHostRegex;
                            string strSocketHost = "*";
                            if (trusted.HostWebsite == "*" || trusted.HostWebsite == "*.*.*.*")
                            {
                                rxHostRegex = new Regex(@".*/.*");
                            }
                            else
                            {
                                rxHostRegex = new Regex(trusted.HostWebsite.Replace(".", @"\.") + ".*", RegexOptions.IgnoreCase);
                                strSocketHost = Regex.Replace(trusted.HostWebsite, "^(.*:\\/\\/)", "", RegexOptions.IgnoreCase);
                            }
                            //psetPluginPermissions.AddPermission(new System.Net.WebPermission(System.Net.NetworkAccess.Connect, new Regex(trusted.HostWebsite.Replace(".", @"\.") + ".*", RegexOptions.IgnoreCase)));
                            psetPluginPermissions.AddPermission(new System.Net.WebPermission(System.Net.NetworkAccess.Connect, rxHostRegex));
                            if (trusted.Port == (UInt16)0)
                            {
                                psetPluginPermissions.AddPermission(new System.Net.SocketPermission(System.Net.NetworkAccess.Connect, System.Net.TransportType.All, strSocketHost, System.Net.SocketPermission.AllPorts));
                            }
                            else
                            {
                                psetPluginPermissions.AddPermission(new System.Net.SocketPermission(System.Net.NetworkAccess.Connect, System.Net.TransportType.All, strSocketHost, trusted.Port));
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                else
                {
                    psetPluginPermissions = new PermissionSet(PermissionState.Unrestricted);
                }

                return psetPluginPermissions;
            }
        }

        public OptionsSettings(PRoConApplication praApplication)
        {
            this.m_praApplication = praApplication;
            this.AutoCheckDownloadUpdates = true;
            this.AutoCheckGameConfigsForUpdates = true;
            this.AllowAnonymousUsageData = true;

            this.EnableAdminReason = false;

            this.LayerHideLocalAccounts = true;
            this.LayerHideLocalPlugins = true;

            this.ShowCfmMsgRoundRestartNext = true;
            this.ShowDICESpecialOptions = false;

            this.ShowTrayIcon = true;

            this.TrustedHostsWebsitesPorts = new NotificationList<TrustedHostWebsitePort>();

            this.StatsLinksMaxNum = 4;
            this.StatsLinkNameUrl = new NotificationList<StatsLinkNameUrl>();
            this.StatsLinkNameUrl.Add(new StatsLinkNameUrl("Battlelog", "https://battlelog.battlefield.com/%game%/user/%player_name%"));
            this.StatsLinkNameUrl.Add(new StatsLinkNameUrl("PunkBuster", "https://www.pbbans.com/mbi-guid-search-%player_PBguid%.html"));
            this.StatsLinkNameUrl.Add(new StatsLinkNameUrl("BF4DB", "https://bf4db.com/player/search?query=%player_name%"));


            this.PluginMaxRuntime_s = 59;
            this.PluginMaxRuntime_m = 0;

            this.UsePluginOldStyleLoad = false;
            this.EnablePluginDebugging = false;
        }
    }
}
