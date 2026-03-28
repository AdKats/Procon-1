/*  Copyright 2011-2013 falcontx
    http://tag.bitgoblin.com

    This file is part of BF3 PRoCon.

    BF3 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BF3 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BF3 PRoCon.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class CUltimateMapManager : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        public static String NL = System.Environment.NewLine;

        private int m_iRestartLimit;
        private bool m_blRestartNow;
        private List<MaplistConfig> m_LMaplists;
        private enumBoolYesNo m_enMapManager;
        private enumBoolYesNo m_enAdminCommands;
        private enumBoolYesNo m_enRestartNow;
        private enumBoolYesNo m_enRestartWarning;
        private int m_enRestartWarningTime;
        private string m_strRestartWarningMessage;
        private enumBoolYesNo m_enCompleteRounds;
        private enumBoolYesNo m_enAllowPresets;
        private enumBoolYesNo m_enPresetsAlways;
        private enumBoolYesNo m_enLoadPresets;
        private enumBoolYesNo m_enEnableServerName;
        private enumBoolYesNo m_enTimeOptions;
        private enumBoolYesNo m_enTimeNotNow;
        private enumBoolYesNo m_enUseSystemTZ;
        private string m_strTimeZone;
        private int m_iCurrentMapList;
        private int m_iCurrentPlayerCount;
        private bool m_blRoundEnded;
        private bool m_blIsFirstRound;
        private bool m_blIsLastRound;
        private bool m_blRestartRequested;
        private bool m_blCheckPlayerCount;
        private List<MaplistEntry> m_LCurrentMapList;
        private string m_strCurrentMap;
        private int m_iNextMapIndex;
        private string m_strCurrentPreset;
        private string m_strCurrentServerName;
        private Dictionary<string, string[]> m_DPreset;
        private List<CPluginVariable> m_LGDPVList;
        private List<CPluginVariable> m_LGPVList;
        private bool m_blUseSystemTZ;
        private string m_strVotedMapFileName;
        private string m_strVotedGameMode;
        private int m_iCurrentRoundCount;

        private enumBoolYesNo m_enDoDebugOutput;

        private bool m_isPluginEnabled;
        private bool m_isPluginInitialized;
        private string m_strServerGameType;
        private string m_strGameMod;
        private string m_strServerVersion;
        private string m_strPRoConVersion;

        private class MaplistConfig {

            public int Index;
            public string Name;
            public enumBoolYesNo Enabled;
            public int MinPlayers;
            public int MaxPlayers;
            public string MapListStart;
            public List<MaplistInfo> Maplist;
            public string[] DaysValid;
            public string TimeStart;
            public string TimeStop;
            public string ServerName;
            public int MinRounds;

            public class MaplistInfo {
                public string Gamemode;
                public string PublicGamemode;
                public string MapFileName;
                public int Rounds;
                public int Index;
                public string Preset;
                public string LoadPreset;

                public MaplistInfo(int iIndex) {
                    this.Gamemode = "ConquestLarge0";
                    this.PublicGamemode = "Conquest Large";
                    this.MapFileName = "MP_001";
                    this.Rounds = 2;
                    this.Index = iIndex;
                    this.Preset = "Normal";
                    this.LoadPreset = "None";
                }

                public MaplistInfo(string strGamemode, string strPublicGamemode, string strMapFileName, int iRounds, int iIndex) {
                    this.Gamemode = strGamemode;
                    this.PublicGamemode = strPublicGamemode;
                    this.MapFileName = strMapFileName;
                    this.Rounds = iRounds;
                    this.Index = iIndex;
                    this.Preset = "Normal";
                    this.LoadPreset = "None";
                }
            }

            public MaplistConfig(int iIndex) {
                this.Index = iIndex;
                this.Name = "Map List Name";
                this.Enabled = enumBoolYesNo.Yes;
                this.ServerName = "";
                this.Maplist = new List<MaplistInfo>();
                this.MinPlayers = 0;
                this.MaxPlayers = 64;
                this.MapListStart = "None";
                this.MinRounds = 0;

                List<string> dw = new List<string>();
                for (int i = 0; i < 7; i++)
                {
                    dw.Add(DateTime.Now.AddDays(i).ToString("dddd"));
                }
                this.DaysValid = dw.ToArray();
                this.TimeStart = "0:00";
                this.TimeStop = "23:59";
            }

            public bool isValidDay(string timezone, bool useSystemTZ, string timeStart, string timeStop) {
                DateTime current = DateTime.Now;
                if (useSystemTZ)
                {
                    current = TimeZoneInformation.ToLocalTime(DateTime.UtcNow, timezone);
                }

                string[] partsStart = timeStart.Split(':');
                string[] partsStop = timeStop.Split(':');
                DateTime start = new DateTime(current.Year, current.Month, current.Day, Int32.Parse(partsStart[0]), Int32.Parse(partsStart[1]), 0, 0);
                DateTime stop = new DateTime(current.Year, current.Month, current.Day, Int32.Parse(partsStop[0]), Int32.Parse(partsStop[1]), 59, 999);

                if (DateTime.Compare(start, stop) > 0)
                {
                    if (DateTime.Compare(start, current) <= 0)
                    {
                        stop = stop.AddDays(1);
                    }
                    else if (DateTime.Compare(current, stop) <= 0)
                    {
                        start = start.AddDays(-1);
                    }
                }
                return (DateTime.Compare(start, current) <= 0 && DateTime.Compare(current, stop) <= 0 && (((IList<string>)this.DaysValid).Contains(start.ToString("d")) || ((IList<string>)this.DaysValid).Contains(start.ToString("dd")) || ((IList<string>)this.DaysValid).Contains(start.ToString("ddd")) || ((IList<string>)this.DaysValid).Contains(start.ToString("dddd"))));
            }
        }

        public class TimeZoneInformation
        {
            private TZI tzi; // Current time zone information.
            public string displayName; // Current time zone display name.
            public string standardName; // Current time zone standard name (non-DST).
            public string daylightName; // Current time zone daylight name (DST).
            public static readonly List<TimeZoneInformation> timeZones; // static list of all time zones on machine.

            private TimeZoneInformation()
            {
            }

            static TimeZoneInformation()
            {
                timeZones = new List<TimeZoneInformation>();

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"))
                {
                    string[] zoneNames = key.GetSubKeyNames();

                    foreach (string zoneName in zoneNames)
                    {
                        using (RegistryKey subKey = key.OpenSubKey(zoneName))
                        {
                            TimeZoneInformation tzi = new TimeZoneInformation();
                            tzi.displayName = ((string)subKey.GetValue("Display")).Replace(",", ";");
                            tzi.standardName = (string)subKey.GetValue("Std");
                            tzi.daylightName = (string)subKey.GetValue("Dlt");
                            tzi.InitTzi((byte[])subKey.GetValue("Tzi"));
                            timeZones.Add(tzi);
                        }
                    }
                }
            }

            private static int CompareTimeZones(string x, string y)
            {
                int retval = 0;
                x = x.Replace(" ", "").Replace(".", " ").Replace(":", " ");
                y = y.Replace(" ", "").Replace(".", " ").Replace(":", " ");
                if (x.Substring(4, 1) != ")" && y.Substring(4, 1) != ")")
                {
                    retval = Int32.Parse(x.Substring(4,3)) - Int32.Parse(y.Substring(4,3));
                }
                else if (x.Substring(4, 1) == ")" && y.Substring(4, 1) != ")")
                {
                    retval = 0 - Int32.Parse(y.Substring(4,3));
                }
                else if (y.Substring(4, 1) == ")" && x.Substring(4, 1) != ")")
                {
                    retval = Int32.Parse(x.Substring(4,3));
                }

                if (retval != 0)
                {
                    return retval;
                }
                else
                {
                    return x.CompareTo(y);
                }
            }

            public static string[] TimeZoneNames
            {
                get
                {
                    List<String> list = new List<String>();
                    foreach (TimeZoneInformation tzi in timeZones)
                    {
                        list.Add(tzi.displayName);
                    }
                    list.Sort(CompareTimeZones);
                    return list.ToArray();
                }
            }

            public static TimeZoneInformation GetTimeZone(string standardTimeZoneName)
            {
                if (standardTimeZoneName == null)
                    standardTimeZoneName = ".";
                if (standardTimeZoneName == ".")
                    standardTimeZoneName = TimeZone.CurrentTimeZone.StandardName;

                foreach (TimeZoneInformation tzi in TimeZoneInformation.timeZones)
                {
                    if (tzi.displayName.Equals(standardTimeZoneName, StringComparison.OrdinalIgnoreCase))
                        return tzi;
                }
                throw new ArgumentException("standardTimeZoneName not found.");
            }

            public DateTime ToLocalTime(DateTime utc)
            {
                // Convert to SYSTEMTIME
                SYSTEMTIME stUTC = DateTimeToSystemTime(utc);

                // Set up the TIME_ZONE_INFORMATION
                TIME_ZONE_INFORMATION tziNative = TziNative();
                SYSTEMTIME stLocal;
                NativeMethods.SystemTimeToTzSpecificLocalTime(ref tziNative, ref stUTC, out stLocal);

                // Convert back to DateTime
                return SystemTimeToDateTime(ref stLocal);
            }

            public static DateTime ToLocalTime(DateTime utc, string targetTimeZoneName)
            {
                TimeZoneInformation tzi = TimeZoneInformation.GetTimeZone(targetTimeZoneName);
                return tzi.ToLocalTime(utc);
            }

            private static SYSTEMTIME DateTimeToSystemTime(DateTime dt)
            {
                SYSTEMTIME st;
                FILETIME ft = new FILETIME();
                ft.dwHighDateTime = (int)(dt.Ticks >> 32);
                ft.dwLowDateTime = (int)(dt.Ticks & 0xFFFFFFFFL);
                NativeMethods.FileTimeToSystemTime(ref ft, out st);
                return st;
            }

            private static DateTime SystemTimeToDateTime(ref SYSTEMTIME st)
            {
                FILETIME ft = new FILETIME();
                NativeMethods.SystemTimeToFileTime(ref st, out ft);
                DateTime dt = new DateTime((((long)ft.dwHighDateTime) << 32) | (uint)ft.dwLowDateTime);
                return dt;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct TIME_ZONE_INFORMATION
            {
                [MarshalAs(UnmanagedType.I4)] public Int32 Bias;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string StandardName;
                public SYSTEMTIME StandardDate;
                [MarshalAs(UnmanagedType.I4)] public Int32 StandardBias;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DaylightName;
                public SYSTEMTIME DaylightDate;
                [MarshalAs(UnmanagedType.I4)] public Int32 DaylightBias;
            }

            public struct SYSTEMTIME
            {
                public ushort wYear;
                public ushort wMonth;
                public ushort wDayOfWeek;
                public ushort wDay;
                public ushort wHour;
                public ushort wMinute;
                public ushort wSecond;
                public ushort wMilliseconds;
            }

            private struct TZI
            {
                public int bias;
                public int standardBias;
                public int daylightBias;
                public SYSTEMTIME standardDate;
                public SYSTEMTIME daylightDate;
            }

            private TIME_ZONE_INFORMATION TziNative()
            {
                TIME_ZONE_INFORMATION tziNative = new TIME_ZONE_INFORMATION();
                tziNative.Bias = tzi.bias;
                tziNative.StandardDate = tzi.standardDate;
                tziNative.StandardBias = tzi.standardBias;
                tziNative.DaylightDate = tzi.daylightDate;
                tziNative.DaylightBias = tzi.daylightBias;
                return tziNative;
            }

            private struct NativeMethods
            {
                private const string KERNEL32 = "kernel32.dll";

                [DllImport(KERNEL32)] public static extern uint GetTimeZoneInformation(out TIME_ZONE_INFORMATION lpTimeZoneInformation);
                [DllImport(KERNEL32)] public static extern bool SystemTimeToTzSpecificLocalTime([In] ref TIME_ZONE_INFORMATION lpTimeZone, [In] ref SYSTEMTIME lpUniversalTime, out SYSTEMTIME lpLocalTime);
                [DllImport(KERNEL32)] public static extern bool SystemTimeToFileTime([In] ref SYSTEMTIME lpSystemTime, out FILETIME lpFileTime);
                [DllImport(KERNEL32)] public static extern bool FileTimeToSystemTime([In] ref FILETIME lpFileTime, out SYSTEMTIME lpSystemTime);
                [DllImport(KERNEL32)] public static extern bool TzSpecificLocalTimeToSystemTime([In] ref TIME_ZONE_INFORMATION lpTimeZone, [In] ref SYSTEMTIME lpLocalTime, out SYSTEMTIME lpUniversalTime);
            }

            private void InitTzi(byte[] info)
            {
                if (info.Length != Marshal.SizeOf(tzi))
                    throw new ArgumentException("Information size is incorrect", "info");

                GCHandle h = GCHandle.Alloc(info, GCHandleType.Pinned);
                try
                {
                    tzi = (TZI)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(TZI));
                }
                finally
                {
                    h.Free();
                }
            }
        }

        public CUltimateMapManager() {

            this.m_iRestartLimit            = 16;
            this.m_blRestartNow             = false;
            this.m_LMaplists                = new List<MaplistConfig>();
            this.m_enMapManager             = enumBoolYesNo.No;
            this.m_enAdminCommands          = enumBoolYesNo.No;
            this.m_enAllowPresets           = enumBoolYesNo.No;
            this.m_enPresetsAlways          = enumBoolYesNo.No;
            this.m_enLoadPresets            = enumBoolYesNo.No;
            this.m_enCompleteRounds         = enumBoolYesNo.No;
            this.m_enRestartNow             = enumBoolYesNo.No;
            this.m_enRestartWarning         = enumBoolYesNo.Yes;
            this.m_enRestartWarningTime     = 10;
            this.m_strRestartWarningMessage  = "Changing map rotation to [listname] in [secs] seconds! Points will not be lost!";
            this.m_enEnableServerName       = enumBoolYesNo.No;
            this.m_enTimeOptions            = enumBoolYesNo.No;
            this.m_enTimeNotNow             = enumBoolYesNo.No;
            this.m_enUseSystemTZ            = enumBoolYesNo.No;
            this.m_strTimeZone              = "(UTC) Coordinated Universal Time";
            this.m_iCurrentMapList          = -1;
            this.m_iCurrentPlayerCount      = 0;
            this.m_blRoundEnded             = false;
            this.m_blIsFirstRound           = true;
            this.m_blIsLastRound            = false;
            this.m_blRestartRequested       = false;
            this.m_blCheckPlayerCount       = false;
            this.m_LCurrentMapList          = new List<MaplistEntry>();
            this.m_strCurrentMap            = "";
            this.m_iNextMapIndex            = -1;
            this.m_strCurrentPreset         = "";
            this.m_strCurrentServerName     = "";
            this.m_LGDPVList                = new List<CPluginVariable>();
            this.m_LGPVList                 = new List<CPluginVariable>();
            this.m_blUseSystemTZ            = SecurityManager.IsGranted(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            this.m_strVotedMapFileName      = "";
            this.m_strVotedGameMode         = "";
            this.m_iCurrentRoundCount       = 0;

            this.m_DPreset                  = new Dictionary<string, string[]>();
            this.m_DPreset["Normal"]        = new string[] {};
            this.m_DPreset["Hardcore"]      = new string[] {};
            this.m_DPreset["Infantry Only"] = new string[] {};

            this.m_enDoDebugOutput          = enumBoolYesNo.Yes;

            this.m_isPluginEnabled          = false;
            this.m_isPluginInitialized      = false;
            this.m_strServerGameType = "none";
        }

        public string GetPluginName() {
            return "Ultimate Map Manager";
        }

        public string GetPluginVersion() {
            return "1.2.5.0";
        }

        public string GetPluginAuthor() {
            return "falcontx";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net/forumvb/showthread.php?3472";
        }

        public string GetPluginDescription() {
            return @"
<p>If you find this plugin useful, please consider supporting falcontx's development efforts. Donations help support the servers used for development and provide incentive for additional features and new plugins! Any amount would be appreciated!</p>

    <table class=""table"" border=""0"" cellpadding=""0"" cellspacing=""0"">
    <tr>
    <td style=""text-align:center"">
    <form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
    <input type=""hidden"" name=""cmd"" value=""_donations"">
    <input type=""hidden"" name=""business"" value=""XZBACYX9CK6YA"">
    <input type=""hidden"" name=""lc"" value=""US"">
    <input type=""hidden"" name=""item_name"" value=""Support Plugin Development (Ultimate Map Manager)"">
    <input type=""hidden"" name=""currency_code"" value=""USD"">
    <input type=""hidden"" name=""bn"" value=""PP-DonationsBF:btn_donate_LG.gif:NonHosted"">
    <input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!""><br>
    <br>
    <img alt="""" border=""0"" src=""https://www.paypalobjects.com/en_US/ebook/PP_ExpressCheckout_IntegrationGuide/images/PayPal_mark_50x34.gif""
    </form>
    </td>
    </tr>
    </table>

<h2>Description</h2>
    <p>This plug-in is intended to manage your map rotation based upon the number of players on the server. Optionally, it will also change the server name to match the current rotation, allow certain rotations to be used only on certain days, and manage server presets (including user-defined presets).</p>
    <p>With regards to adding new features, as a general rule, I prefer to recommend other plugins that meet a specific need rather than attempt duplicate them within my own plugin. The reason for this is that I believe that if a plugin is well known, well supported, and has been considered stable for a good amount of time, it's best use it as a supporting plugin, rather than try to ""reinvent the wheel."" It's much easier to manage bugs and provide more reliable plugins when each plugin has a specific task. As such, I am now including a list of supporting plugins that I recommend that provide features that users have requested.

<h2>Supporting Plugins</h2>
    <p>If you need map voting capabilities, the <b>xVotemap</b> plugin is highly recommended. I have worked with xVotemap's author in order to maintain the best possible compatibility. If a map has been voted on, and this plugin then changes to a different map list, it will still play the voted map and game mode if it exists on the new list. If the new list has a different game mode, it will still play the voted map, if it exists. Of course, if the voted map doesn't exist at all in the new map list, there's not much we can do, as using the proper map list is most important, in order to prevent killing the server. <i>[xVotemap v1.2.1 or later required]</i></p>
    <p>If you need map cycling or to return to the first map when the server is empty, the <b>Automatic Round Restarter</b> plugin is highly recommended. It is fully compatible with this plugin.

<h2>Known issues</h2>
    <ul>
    <li>If any given map list contains a map that does not allow commander mode, it will not be available for ANY of the maps in that map list. This is a server restriction, not a plugin bug.</li>
    </ul>

<h2>Commands</h2>
    <br><h3>Map List Admin</h3> (Must be enabled and requires ""Change current map functions"" access in PRoCon)
        <blockquote><h4>@maplist</h4>Privately tells the admin which map list is currently active</blockquote>
        <blockquote><h4>@maplist [index]</h4>Privately tells the admin the status of the specified map list</blockquote>
        <blockquote><h4>@maplist all</h4>Privately tells the admin the status of all map lists</blockquote>
        <blockquote><h4>@maplist [index] on</h4>Enables the specified map list</blockquote>
        <blockquote><h4>@maplist [index] off</h4>Disables the specified map list</blockquote>

<h2>Settings</h2>
    <br><h3>Ultimate Map Manager</h3>
        <blockquote><h4>Enable map list manager?</h4> If enabled, the plugin will become active. This must be set to ""No"" while you are making changes to the map list.</blockquote>
        <blockquote><h4>Enable in-game admin commands?</h4> If enabled, admins with proper access will be able to enable and disable map lists.</blockquote>
        <blockquote><h4>Switch to new map list immediately?</h4> If enabled, the current round will be interrupted if the minimum or maximum players is surpassed, and the new map rotation will be started immediately.</blockquote>
        <blockquote><h4>Do not switch if more than this number of players are online</h4> If the above option is enabled, it can be limited to act only when less than this number of players are online. The idea here is that if people are rapidly leaving your server, it may be beneficial to interrupt the current round and immediately load a map that is more popular or more suited for fewer players.</blockquote>
        <blockquote><h4>Warn (yell at) players before switching?</h4> If immediate switching is enabled, this option enables the plugin to yell at players, warning them that the map list will be changing soon.</blockquote>
        <blockquote><h4>Number of seconds to display warning</h4> If the immediate switch warning is enabled, this determines the number of seconds the players are warned before the switch occurs. The message will remain on the screen for the entire duration.</blockquote>
        <blockquote><h4>Warning message (use [listname] for map list name and [secs] for seconds)</h4> If the immediate switch warning is enabled, this is the message that will be displayed. <b>[listname]</b> is replaced by the name of the map list that is being switched to. <b>[secs]</b> is replaced by the number of seconds specified above.</blockquote>
        <blockquote><h4>Change map list only after map has completed it's total number of rounds?</h4> If enabled, the map list will not be changed until the current map has complete it's total number of rounds, unless the server is empty. For example, if the current map is a Rush map running 2 rounds, the map list will not change until both rounds have been completed. Can not be used if 'Switch to new map immediately' is enabled.</blockquote>
        <blockquote><h4>Enable server name change for each map list?</h4> If enabled, each map list can be assigned a different server name, so that when the new rotation is loaded, the server name will be changed to match.</blockquote>
        <blockquote><h4>Enable day/time-dependent map lists?</h4> If enabled, a list of days of the week/month can be provided for each map list, in order to limit the use of certain lists to certain days.</blockquote>
        <blockquote><h4>Ignore 'Switch to new map list immediately' setting for time-based changes?</h4> If enabled, the map list will never change immediately for time-based map list settings. In other words, if 'Switch to new map list immediately' and this setting are both enabled, the current round will be allowed to continue, even after the map list's end time has passed.</blockquote>
        <blockquote><h4>Use system/layer server Time Zone?</h4> If enabled, the time zone of the system the plugin is running on (either local PRoCon or layer server) will be used to determine the current time.</blockquote>
        <blockquote><h4>Time Zone used to determine current time</h4> When the prior option is disabled, this option specifies the Time Zone that will be used to determine the current time to be chosen. Some systems/hosts do not allow PRoCon to access the registry or call methods from the Windows API. In these cases, this option will not be available and the system time will be used.</blockquote>
        <blockquote><h4>Allow map-specific game presets?</h4> If enabled, a different server preset can be chosen for each map in each rotation. For example, this can be used to start with a Normal preset and change to Hardcore once more players are online, or to run certain maps as Infantry Only.</blockquote>
        <blockquote><h4>Send preset commands after every round?</h4> When the prior option is enabled, this option will send the preset commands between every round, instead of only when the preset changes. This generally isn't necessary, but may be useful if your preset commands are being reset for some reason external to this plugin.</blockquote>
        <blockquote><h4>Allow additional presets to be sent after each level loads?</h4> This option will allow a second preset to be sent after the round loads. This is useful if you would like to have one preset set before and a different one after for the purposes of having a proper server listing in Battlelog.</blockquote>
        <blockquote><h4>Add a new map list?</h4> If ""Create an empty map list"" is chosen, a new map list will be added with only one map. If ""Create a map list based on current map list"" is chosen, a new map list will be added to the plugin that matches the current map list shown in PRoCon.</blockquote>
    <br><h3>Map List [X]</h3>
        <blockquote><h4>Enable this map list?</h4> If enabled, the map list will be used; if disabled, it will not.</blockquote>
        <blockquote><h4>Map list name</h4> Your custom name for this map list.</blockquote>
        <blockquote><h4>Server name for this map list</h4> If enabled above, this is the server name that will be applied when this map rotation is active. It's limited by the server to 63 characters.</blockquote>
        <blockquote><h4>Minimum number of players</h4> The minimum number of players allowed for this map list. If the number of players drops below this number, the map list will be changed.</blockquote>
        <blockquote><h4>Maximum number of players</h4> The maximum number of players allowed for this map list. If the number of players increases above this number, the map list will be changed.</blockquote>
        <blockquote><h4>Days of week/month to use this map list</h4> If enabled above, this is the list of days that this map list map be active. You may use days of the week (Monday, Tuesday, etc.) or days of the month (1, 2, 15, 16, etc.) in this list. Press the '...' button to the right of 'String[] Array' to edit the list.</blockquote>
        <blockquote><h4>Time to start using this map list (24-hour format)</h4> If day/time-dependent map lists are enabled, this is the time that this map list will stop being used in 24-hour format (0:00 to 23:59). If the start time is higher than the stop time, the plugin will consider the stop time to be the following day. For example, if start is 18:00 and stop is 2:00, the map will be active from 18:00 today until 2:00 tomorrow.</blockquote>
        <blockquote><h4>Time to stop using this map list (24-hour format)</h4> If day/time-dependent map lists are enabled, this is the time that this map list will stop being used in 24-hour format (0:00 to 23:59).</blockquote>
        <blockquote><h4>List start/random option</h4>
        - ""Start with first map"": The map list will be loaded in it's original order and played from the beginning.<br/>
        - ""Start with first map unless it was just played"": The map list will be loaded in it's original order and played from the beginning unless the first map is the same as the one just played, in which case it will start with the second map.<br/>
        - ""Start with map after the map that was just played"": The map list will be loaded in it's original order and played from the map that comes just after the map that was just played.<br/>
        - ""Start with random map"": The map list will be loaded in it's original order and played starting with a random map.<br/>
        - ""Randomize entire map list"": The map list will be loaded in a random order and played from the beginning unless the first map is the same as the one just played, in which case it will start with the second map.<br/>
        </blockquote>
        <blockquote><h4>Minimum number of rounds to be played</h4> The minimum number of rounds that must be played on this map list before the plugin will change to a different map list. If the number of rounds has not been met, the map list will not change, even if 'switch to new map list immediately' is enabled, and even if the end time for the map list has passed (if using time-based map lists). However, it will change if the current map list is disabled or manually changed, or if no one is on the server.</blockquote>
        <blockquote><h4>Map options</h4> 
            <ul>
                <li><b>Game mode</b>: Game mode for the this map.</li>
                <li><b>Map Name</b>: Name of this map.</li>
                <li><b>Rounds</b>: Number of rounds this map will be played.</li>
                <li><b>Preset</b>: Server preset that will be used for this map. This preset is sent before the level loads.</li>
                <li><b>Preset (after level loads)</b>: Server preset that will be sent after the level loads.</li>
                <li><b>Manage Map</b>: Options to move the map within the list or remove it from the list.</li>
            </ul></blockquote>
        <blockquote><h4>Add a new map?</h4> Adds the selected number of new maps to this map list.</blockquote>
        <blockquote><h4>Manage Map List</h4> Options to change the order of the map lists or remove a map list entirely.</blockquote>
    <br><h3>Presets</h3>
        <blockquote><h4>Normal/Hardcore/Infantry Only (view only)</h4> These are provided simply for reference. You can't edit these values.</blockquote>
        <blockquote><h4>Custom Preset: [X]</h4> Custom preset definitions that can be modified to fit your needs. Press the '...' button to the right of 'String[] Array' to edit the list. The first line can be used to change the name of a custom preset using the format ""# Custom Preset: New Name"". Other lines beginning with '#' will be ignored.</blockquote>
        <blockquote><h4>Manage Presets</h4> Options to add and remove custom presets. If you remove a custom preset that's being used, maps using that preset will revert to the Normal preset.</blockquote>
    <br><h3>Extras</h3>
        <blockquote><h4>Enable debug output?</h4> If enabled, displays debug info in the console window.</blockquote>

<br><h2>Development</h2>
    <br><h3>Changelog</h3>
      <blockquote><h4>1.2.5.0 (03/26/2015)</h4>
            - added support for BFHL<br/>
      </blockquote>
      <blockquote><h4>1.2.4.0 (02/03/2014)</h4>
            - added a check for external map list changes<br/>
            - added in-game commands to enable/disable map lists<br/>
      </blockquote>
      <blockquote><h4>1.2.3.6 (12/11/2013)</h4>
            - fixed secondary preset config display bug<br/>
      </blockquote>
      <blockquote><h4>1.2.3.4 (12/07/2013)</h4>
            - fixed map management bug caused by last update<br/>
      </blockquote>
      <blockquote><h4>1.2.3.2 (12/07/2013)</h4>
            - added an option to send a different preset after the level loads<br/>
      </blockquote>
      <blockquote><h4>1.2.3.0 (11/26/2013)</h4>
            - changed max server size back to 64 for BF4<br/>
            - fixed possible issue detecting next map at end of round for BF4<br/>
            - fixed 'Send preset commands after every round?' setting was appearing even when game presets were disabled<br/>
      </blockquote>
      <blockquote><h4>1.2.2.6 (11/06/2013)</h4>
            - added correct presets for BF4<br/>
            - added option to send custom preset commands between every round<br/>
      </blockquote>
      <blockquote><h4>1.2.2.4 (11/05/2013)</h4>
            - added support for up to 70 players for BF4<br/>
      </blockquote>
      <blockquote><h4>1.2.2.2 (11/02/2013)</h4>
            - fixed issues with quotes and vertical lines in presets<br/>
      </blockquote>
      <blockquote><h4>1.2.2.0 (10/31/2013)</h4>
            - added support for BF4<br/>
      </blockquote>
      <blockquote><h4>1.2.1.0 (05/21/2013)</h4>
            - added support for MOHW (custom playlists only)<br/>
            - map lists using both day and time will now switch at the specified time rather than at 0:00 when the day changes<br/>
      </blockquote>
      <blockquote><h4>1.2.0.2 (08/24/2012)</h4>
            - fixed bug with 'Change map list only after map has completed it's total number of rounds' option<br/>
      </blockquote>
      <blockquote><h4>1.2.0.1 (07/19/2012)</h4>
            - remove debug code left in v1.2.0.0 by mistake
      </blockquote>
      <blockquote><h4>1.2.0.0 (07/19/2012)</h4>
            - fixed bug caused by changes in PRoCon 1.3<br/>
            - added option to yell before rotation is changed when using 'switch immediately'<br/>
            - added option to start on the same map that was just played<br/>
            - added round minimum for map lists<br/>
            - added option to change map list only after map has completed it's total number of rounds<br/>
            - added option to ignore 'switch immediately' for time-based changes<br/>
      </blockquote>
      <blockquote><h4>1.1.2.6 (05/14/2012)</h4>
            - fixed time zones with commas were missing from the list<br/>
            - added option to use system time zone<br/>
      </blockquote>
      <blockquote><h4>1.1.2.5 (03/30/2012)</h4>
            - more minor compatibility changes due to PRoCon/R-20 updates<br/>
      </blockquote>
      <blockquote><h4>1.1.2.4 (03/29/2012)</h4>
            - minor compatibility changes due to upcoming PRoCon/R-20 updates<br/>
      </blockquote>
      <blockquote><h4>1.1.2.3 (02/19/2012)</h4>
            - fixed time calculations were sometimes incorrect when start time was after stop time<br/>
      </blockquote>
      <blockquote><h4>1.1.2.2 (02/11/2012)</h4>
            - fixed map list names bug when '|' is attempted to be used<br/>
            - custom map names now shown in debug logs when map lists change<br/>
            - fixed time calculations weren't working properly on some systems<br/>
      </blockquote>
      <blockquote><h4>1.1.2.1 (02/07/2012)</h4>
            - fixed map list names not being saved<br/>
            - fixed bug related to sorting the time zone list on some systems<br/>
      </blockquote>
      <blockquote><h4>1.1.2.0 (02/06/2012)</h4>
            - fixed time zones greater than GMT were not being saved properly<br/>
            - improved compatibility with xVotemap; see second paragraph of the plugin description above<br/>
            - added list of recommended supporting plugins that provide features that have been previously requested<br/>
            - added time-dependent map lists<br/>
            - added ability to name map lists<br/>
      </blockquote>
      <blockquote><h4>1.1.1.0 (02/02/2012)</h4>
            - added another map list check when no players are on the server<br/>
            - added option to avoid playing the same map when a new map is loaded<br/>
            - added option to start with the map in the new list that follows the one that was just played<br/>
            - added ability to randomize entire map list<br/>
            - updated map list change method, which should prevent map list from not changing<br/>
      </blockquote>
      <blockquote><h4>1.1.0.2 (01/27/2012)</h4>
            - fixed problem adding new presets after some were renamed<br/>
            - new custom presets are now named based upon the template used to create them<br/>
            - fixed not starting at first map when not using 'start with random'<br/>
            - added the ability to disable map lists<br/>
      </blockquote>
      <blockquote><h4>1.1.0.1 (01/20/2012)</h4>
            - custom preset map assignments weren't being restored on restart<br/>
            - custom presets can now be renamed<br/>
            - day-dependent map lists no longer clear the options on some systems<br/>
      </blockquote>
      <blockquote><h4>1.1.0.0 (01/17/2012)</h4>
            - decreased options redraw time even more (almost instant now)<br/>
            - added custom presets<br/>
            - added day-dependent map lists<br/>
      </blockquote>
      <blockquote><h4>1.0.0.2 (01/09/2012)</h4>
            - greatly decreased load time and options redraw time with long/many map lists<br/>
            - fixed presets not always set properly or at correct time<br/>
            - presets for next map now set at end of round<br/>
            - fixed map list was not reloaded if number of rounds was changed<br/>
            - fixed map list changed too soon if 'Switch to new map immediately' was enabled<br/>
      </blockquote>
      <blockquote><h4>1.0.0.1 (01/05/2012)</h4>
            - fixed first map not set when map list changed after manual round restart<br/>
            - fixed map list would not update if changed when enabled via options panel<br/>
            - fixed wrong number of players detected when listplayers not called with 'all'<br/>
      </blockquote>
      <blockquote><h4>1.0.0.0 (12/31/2011)</h4>
            - initial version<br/>
      </blockquote>
";
        }

        #region pluginSetup
        public void OnPluginLoadingEnv(List<string> lstPluginEnv) {
            Version PRoConVersion = new Version(lstPluginEnv[0]);
            this.m_strPRoConVersion = PRoConVersion.ToString();
            this.m_strServerGameType = lstPluginEnv[1].ToLower();
            this.m_strGameMod = lstPluginEnv[2];
            this.m_strServerVersion = lstPluginEnv[3];

            if (this.m_strServerGameType == "bf4")
            {
                this.m_DPreset["Normal"] = new string[] {
                    "vars.preset normal",
                };
                this.m_DPreset["Hardcore"] = new string[] {
                    "vars.preset hardcore",
                };
                this.m_DPreset["Infantry Only"] = new string[] {
                    "vars.preset infantry",
                };
            }
            else if (this.m_strServerGameType == "bfhl")
            {
                this.m_DPreset["Normal"] = new string[] {
                    "vars.preset normal",
                };
                this.m_DPreset["Hardcore"] = new string[] {
                    "vars.preset hardcore",
                };
            }
            else
            {
                this.m_DPreset["Normal"] = new string[] {
                    "vars.friendlyFire false",
                    "vars.killCam true",
                    "vars.hud true",
                    "vars.3dSpotting true",
                    "vars.nameTag true",
                    "vars.3pCam true",
                    "vars.regenerateHealth true",
                    "vars.vehicleSpawnAllowed true",
                    "vars.soldierHealth 100",
                    "vars.onlySquadLeaderSpawn false",
                    "vars.miniMap true",
                    "vars.playerRespawnTime 100",
                    "vars.playerManDownTime 100",
                };
                this.m_DPreset["Hardcore"] = new string[] {
                    "vars.friendlyFire true",
                    "vars.killCam false",
                    "vars.hud false",
                    "vars.3dSpotting false",
                    "vars.nameTag false",
                    "vars.3pCam false",
                    "vars.regenerateHealth false",
                    "vars.vehicleSpawnAllowed true",
                    "vars.soldierHealth 60",
                    "vars.onlySquadLeaderSpawn true",
                    "vars.miniMap true",
                    "vars.playerRespawnTime 100",
                    "vars.playerManDownTime 100",
                };
                this.m_DPreset["Infantry Only"] = new string[] {
                    "vars.friendlyFire false",
                    "vars.killCam true",
                    "vars.hud true",
                    "vars.3dSpotting true",
                    "vars.nameTag true",
                    "vars.3pCam false",
                    "vars.regenerateHealth true",
                    "vars.vehicleSpawnAllowed false",
                    "vars.soldierHealth 100",
                    "vars.onlySquadLeaderSpawn false",
                    "vars.miniMap true",
                    "vars.playerRespawnTime 100",
                    "vars.playerManDownTime 100",
                };
            }
        
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;

            this.RegisterEvents(this.GetType().Name, "OnLogin", "OnListPlayers", "OnPlayerLeft", "OnRoundOverTeamScores", "OnRestartLevel", "OnRunNextLevel", "OnLevelLoaded", "OnMaplistList", "OnServerInfo", "OnMaplistGetMapIndices", "OnMaplistGetRounds", "OnMaplistSave");
        }

        public void OnPluginEnable() {
            this.m_isPluginEnabled = true;
            ResetVars();
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bUltimateMapManager: ^2Enabled!");
            this.ExecuteCommand("procon.protected.send", "mapList.list");
            this.RegisterAllCommands();
        }

        public void OnPluginDisable() {
            this.m_isPluginEnabled = false;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bUltimateMapManager: ^1Disabled =(" );
            this.UnregisterAllCommands();
        }

        private void ResetVars() {
            this.m_isPluginInitialized = false;
            this.m_iCurrentMapList = -1;
            this.m_iNextMapIndex = -1;
            this.m_blRoundEnded = false;
            this.m_blIsFirstRound = true;
            this.m_blIsLastRound = false;
            this.m_blRestartRequested = false;
            this.m_blCheckPlayerCount = false;
            this.m_strCurrentPreset = "";
            this.m_iCurrentRoundCount = 0;
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option 
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {
            if (this.m_LGDPVList.Count != 0)
            {
                return this.m_LGDPVList;
            }

            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Enable map list manager?", typeof(enumBoolYesNo), this.m_enMapManager));
            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Enable in-game admin commands?", typeof(enumBoolYesNo), this.m_enAdminCommands));
            if (this.m_enCompleteRounds == enumBoolYesNo.No)
            {
                this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Switch to new map list immediately?", typeof(enumBoolYesNo), this.m_enRestartNow));
                if (this.m_enRestartNow == enumBoolYesNo.Yes)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Do not switch if more than this number of players are online", this.m_iRestartLimit.GetType(), this.m_iRestartLimit));
                    this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Warn (yell at) players before switching?", typeof(enumBoolYesNo), this.m_enRestartWarning));
                    if (this.m_enRestartWarning == enumBoolYesNo.Yes)
                    {
                        this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|        Number of seconds to display warning", this.m_enRestartWarningTime.GetType(), this.m_enRestartWarningTime));
                        this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|        Warning message (use [listname] for map list name and [secs] for seconds)", this.m_strRestartWarningMessage.GetType(), this.m_strRestartWarningMessage));
                    }
                }
            }
            if (this.m_enRestartNow == enumBoolYesNo.No)
            {
                this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Change map list only after map has complete it's total number of rounds?", typeof(enumBoolYesNo), this.m_enCompleteRounds));
            }
            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Enable server name change for each map list?", typeof(enumBoolYesNo), this.m_enEnableServerName));
            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Enable day/time-dependent map lists?", typeof(enumBoolYesNo), this.m_enTimeOptions));
            if (this.m_enTimeOptions == enumBoolYesNo.Yes && this.m_blUseSystemTZ)
            {
                if (this.m_enRestartNow == enumBoolYesNo.Yes)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Ignore 'Switch to new map list immediately' setting for time-based changes?", typeof(enumBoolYesNo), this.m_enTimeNotNow));
                }
                this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Use system/layer server time zone?", typeof(enumBoolYesNo), this.m_enUseSystemTZ));
                if (this.m_enUseSystemTZ == enumBoolYesNo.No)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|        Time Zone used to determine current time", "enum.TimeZones(" + String.Join("|", TimeZoneInformation.TimeZoneNames) + ")", this.m_strTimeZone));
                }
            }
            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Allow map-specific game presets?", typeof(enumBoolYesNo), this.m_enAllowPresets));
            if (this.m_enAllowPresets == enumBoolYesNo.Yes)
            {
                this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Send preset commands after every round?", typeof(enumBoolYesNo), this.m_enPresetsAlways));
                this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|    Allow additional presets to be sent after each level loads?", typeof(enumBoolYesNo), this.m_enLoadPresets));
            }
            this.m_LGDPVList.Add(new CPluginVariable("* Ultimate Map Manager|Add a new map list?", "enum.AddNewMapList(...|Create an empty map list|Create a map list based on current map list)", "..."));
            foreach (MaplistConfig MCMaplist in this.m_LMaplists)
            {
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Enable this map list?", typeof(enumBoolYesNo), MCMaplist.Enabled));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Map list name", MCMaplist.Name.GetType(), MCMaplist.Name));
                if (this.m_enEnableServerName == enumBoolYesNo.Yes)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Server name for this map list", MCMaplist.ServerName.GetType(), MCMaplist.ServerName));
                }
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Minimum number of players", MCMaplist.MinPlayers.GetType(), MCMaplist.MinPlayers));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Maximum number of players", MCMaplist.MaxPlayers.GetType(), MCMaplist.MaxPlayers));
                if (this.m_enTimeOptions == enumBoolYesNo.Yes)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Days of week/month to use this map list", MCMaplist.DaysValid.GetType(), MCMaplist.DaysValid));
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Time to start using this map list (24-hour format)", MCMaplist.TimeStart.GetType(), MCMaplist.TimeStart));
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Time to stop using this map list (24-hour format)", MCMaplist.TimeStop.GetType(), MCMaplist.TimeStop));
                }
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] List start/random preference", "enum.MapRandomization(Start with first map|Start with first map unless it was just played|Start with map after the map that was just played|Start with same map that was just played|Start with random map|Randomize entire map list)", MCMaplist.MapListStart));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Minimum number of rounds to be played", MCMaplist.MinRounds.GetType(), MCMaplist.MinRounds));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|-------------------------", typeof(string), ""));
                foreach (MaplistConfig.MaplistInfo MEMap in MCMaplist.Maplist)
                {
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Game Mode", "enum.GameModes(" + String.Join("|", this.GetMapList("{GameMode}").ToArray()) + ")", MEMap.PublicGamemode));
                    List<string> LPublicGameModes = new List<string>();
                    foreach (CMap map in this.GetMapDefines())
                    {
                        if (MEMap.PublicGamemode == map.GameMode)
                        {
                            LPublicGameModes.Add(map.PublicLevelName);
                        }
                    }
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Map Name", "enum.Maps" + MEMap.PublicGamemode.Replace(" ", "") +"(" + String.Join("|", LPublicGameModes.ToArray()) + ")", this.GetMapByFilename(MEMap.MapFileName).PublicLevelName));
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Rounds", MEMap.Rounds.GetType(), MEMap.Rounds));
                    if (this.m_enAllowPresets == enumBoolYesNo.Yes)
                    {
                        List<string> presetList = new List<string>(this.m_DPreset.Keys);
                        this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Preset", "enum.Presets" + String.Join("", presetList.ToArray()).Replace(" ", "") + "(" + String.Join("|", presetList.ToArray()) + ")", MEMap.Preset));
                        if (this.m_enLoadPresets == enumBoolYesNo.Yes) {
                            this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Preset (after level loads)", "enum.LoadPresets" + String.Join("", presetList.ToArray()).Replace(" ", "") + "(None|" + String.Join("|", presetList.ToArray()) + ")", MEMap.LoadPreset));
                        }
                    }
                    this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] ["+MEMap.Index+"] Manage Map", "enum.ManageMap(...|Move Up|Move Down|-----|Remove Map)", "..."));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|-------------------------", typeof(string), ""));
                }
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Add a new map?", "enum.AddNewMap(...|Add 1 new map|Add 2 new maps|Add 3 new maps|Add 4 new maps|Add 5 new maps|Add 6 new maps|Add 7 new maps|Add 8 new maps|-----)", "..."));
                this.m_LGDPVList.Add(new CPluginVariable("Map List "+MCMaplist.Index+" ["+MCMaplist.Name+"]|["+MCMaplist.Index+"] Manage Map List", "enum.ManageList(...|Move Up|Move Down|-----|Remove Entire Map List)", "..."));
            }
            if (this.m_enAllowPresets == enumBoolYesNo.Yes)
            {
                List<string> deleteList = new List<string>();
                foreach (KeyValuePair<string, string[]> preset in this.m_DPreset)
                {
                    string viewMessage = "";
                    if (new List<string>(new string[] {"Normal", "Hardcore", "Infantry Only"}).Contains(preset.Key))
                    {
                        viewMessage = " (view only)";
                    }
                    else
                    {
                        deleteList.Add("|Remove " + preset.Key);
                    }
                    this.m_LGDPVList.Add(new CPluginVariable("Presets|"+preset.Key+viewMessage, preset.Value.GetType(), preset.Value));
                }
                this.m_LGDPVList.Add(new CPluginVariable("Presets|Manage Presets", "enum.ManagePresets" + String.Join("", deleteList.ToArray()).Replace(" ", "") + "(...|Add custom preset based on Normal|Add custom preset based on Hardcore|Add custom preset based on Infantry Only" + String.Join("", deleteList.ToArray()) + ")", "..."));
            }
            this.m_LGDPVList.Add(new CPluginVariable("Xtras|Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return this.m_LGDPVList;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            if (this.m_LGPVList.Count != 0)
            {
                return this.m_LGPVList;
            }

            this.m_LGPVList.Add(new CPluginVariable("Enable map list manager?", typeof(enumBoolYesNo), this.m_enMapManager));
            this.m_LGPVList.Add(new CPluginVariable("Enable in-game admin commands?", typeof(enumBoolYesNo), this.m_enAdminCommands));
            this.m_LGPVList.Add(new CPluginVariable("Switch to new map list immediately?", typeof(enumBoolYesNo), this.m_enRestartNow));
            this.m_LGPVList.Add(new CPluginVariable("    Do not switch if more than this number of players are online", this.m_iRestartLimit.GetType(), this.m_iRestartLimit));
            this.m_LGPVList.Add(new CPluginVariable("    Warn (yell at) players before switching?", typeof(enumBoolYesNo), this.m_enRestartWarning));
            this.m_LGPVList.Add(new CPluginVariable("        Number of seconds to display warning", this.m_enRestartWarningTime.GetType(), this.m_enRestartWarningTime));
            this.m_LGPVList.Add(new CPluginVariable("        Warning message (use [listname] for map list name and [secs] for seconds)", this.m_strRestartWarningMessage.GetType(), this.m_strRestartWarningMessage));
            this.m_LGPVList.Add(new CPluginVariable("Change map list only after map has complete it's total number of rounds?", typeof(enumBoolYesNo), this.m_enCompleteRounds));
            this.m_LGPVList.Add(new CPluginVariable("Enable server name change for each map list?", typeof(enumBoolYesNo), this.m_enEnableServerName));
            this.m_LGPVList.Add(new CPluginVariable("Enable day/time-dependent map lists?", typeof(enumBoolYesNo), this.m_enTimeOptions));
            this.m_LGPVList.Add(new CPluginVariable("    Ignore 'Switch to new map list immediately' setting for time-based changes?", typeof(enumBoolYesNo), this.m_enTimeNotNow));
            this.m_LGPVList.Add(new CPluginVariable("    Use system/layer server time zone?", typeof(enumBoolYesNo), this.m_enUseSystemTZ));
            this.m_LGPVList.Add(new CPluginVariable("        Time Zone used to determine current time", typeof(string), "CONFIG:"+this.m_strTimeZone.Replace("+", "%2B")));
            this.m_LGPVList.Add(new CPluginVariable("Allow map-specific game presets?", typeof(enumBoolYesNo), this.m_enAllowPresets));
            this.m_LGPVList.Add(new CPluginVariable("    Send preset commands after every round?", typeof(enumBoolYesNo), this.m_enPresetsAlways));
            this.m_LGPVList.Add(new CPluginVariable("    Allow additional presets to be sent after each level loads?", typeof(enumBoolYesNo), this.m_enLoadPresets));
            foreach (KeyValuePair<string, string[]> preset in this.m_DPreset)
            {
                if (!(new List<string>(new string[] {"Normal", "Hardcore", "Infantry Only"}).Contains(preset.Key)))
                {
                    this.m_LGPVList.Add(new CPluginVariable(preset.Key, preset.Value.GetType(), preset.Value));
                }
            }
            foreach (MaplistConfig MCMaplist in this.m_LMaplists)
            {
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Enable this map list?", typeof(string), "CONFIG:"+MCMaplist.Enabled));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Map list name", MCMaplist.Name.GetType(), "CONFIG:"+MCMaplist.Name));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Server name for this map list", MCMaplist.ServerName.GetType(), "CONFIG:"+MCMaplist.ServerName));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Minimum number of players", typeof(string), "CONFIG:"+MCMaplist.MinPlayers));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Maximum number of players", typeof(string), "CONFIG:"+MCMaplist.MaxPlayers));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Days of week/month to use this map list", typeof(string), "CONFIG:"+CPluginVariable.EncodeStringArray(MCMaplist.DaysValid)));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Time to start using this map list (24-hour format)", MCMaplist.TimeStart.GetType(), "CONFIG:"+MCMaplist.TimeStart));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Time to stop using this map list (24-hour format)", MCMaplist.TimeStop.GetType(), "CONFIG:"+MCMaplist.TimeStop));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] List start/random preference", typeof(string), "CONFIG:"+MCMaplist.MapListStart));
                this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] Minimum number of rounds to be played", typeof(string), "CONFIG:"+MCMaplist.MinRounds));
                foreach (MaplistConfig.MaplistInfo MEMap in MCMaplist.Maplist)
                {
                    this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] ["+MEMap.Index+"] Game Mode", MEMap.Gamemode.GetType(), "CONFIG:"+MEMap.Gamemode));
                    this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] ["+MEMap.Index+"] Map Name", MEMap.MapFileName.GetType(), "CONFIG:"+MEMap.MapFileName));
                    this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] ["+MEMap.Index+"] Rounds", typeof(string), "CONFIG:"+MEMap.Rounds));
                    this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] ["+MEMap.Index+"] Preset", MEMap.Preset.GetType(), "CONFIG:"+MEMap.Preset));
                    this.m_LGPVList.Add(new CPluginVariable("["+MCMaplist.Index+"] ["+MEMap.Index+"] Preset (after level loads)", MEMap.LoadPreset.GetType(), "CONFIG:"+MEMap.LoadPreset));
                }
            }
            this.m_LGPVList.Add(new CPluginVariable("Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return this.m_LGPVList;
        }

        private void UpdateVariableLists (string strVariableName, object objValue, object objDisplayValue, string strDisplayType, string strCommand) {
            foreach (CPluginVariable variable in this.m_LGPVList)
            {
                if (variable.Name.CompareTo(strVariableName) == 0)
                {
                    int index = this.m_LGPVList.IndexOf(variable);
                    this.m_LGPVList.RemoveAt(index);
                    if (strCommand.CompareTo("delete") != 0)
                    {
                        if (objValue.ToString().CompareTo("System.String[]") == 0)
                        {
                            objValue = CPluginVariable.EncodeStringArray((string[])objValue);
                        }
                        this.m_LGPVList.Insert(index, new CPluginVariable(variable.Name, variable.Type, "CONFIG:" + objValue.ToString()));
                    }
                    break;
                }
            }
            foreach (CPluginVariable variable in this.m_LGDPVList)
            {
                if (variable.Name.Substring(variable.Name.IndexOf("|")+1).CompareTo(strVariableName) == 0)
                {
                    int index = this.m_LGDPVList.IndexOf(variable);
                    this.m_LGDPVList.RemoveAt(index);
                    if (strCommand.CompareTo("delete") != 0)
                    {
                        if (variable.Type.CompareTo("stringarray") == 0)
                        {
                            objDisplayValue = CPluginVariable.EncodeStringArray((string[])objDisplayValue);
                        }
                        if (strDisplayType.CompareTo("") == 0)
                        {
                            this.m_LGDPVList.Insert(index, new CPluginVariable(variable.Name, variable.Type, objDisplayValue.ToString()));
                        }
                        else
                        {
                            this.m_LGDPVList.Insert(index, new CPluginVariable(variable.Name, strDisplayType, objDisplayValue.ToString()));
                        }
                    }
                    break;
                }
            }
        }

        private void UpdateVariableLists (string strVariableName, object objValue) {
            if (objValue.ToString().CompareTo("delete") == 0)
            {
                UpdateVariableLists(strVariableName, "", "", "", "delete");
            }
            else
            {
                UpdateVariableLists(strVariableName, objValue, objValue, "", "update");
            }
        }

        private void UpdateVariableLists (string strVariableName, object objValue, object objDisplayValue) {
            UpdateVariableLists(strVariableName, objValue, objDisplayValue, "", "update");
        }

        private void UpdateVariableLists (string strVariableName, object objValue, object objDisplayValue, string strDisplayType) {
            UpdateVariableLists(strVariableName, objValue, objDisplayValue, strDisplayType, "update");
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iValue = 0;
            bool loadedFromConfig = false;
			this.UnregisterAllCommands();

            if (strValue.StartsWith("CONFIG:"))
            {
               loadedFromConfig = true; 
               strValue = strValue.Replace("CONFIG:", "");
            }

            if (strVariable.CompareTo("Enable map list manager?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enMapManager = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enMapManager);
                this.m_iCurrentMapList = -1;
            }
            else if (strVariable.CompareTo("Enable in-game admin commands?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enAdminCommands = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enAdminCommands);
            }
            else if (strVariable.CompareTo("Switch to new map list immediately?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enRestartNow = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enRestartNow);
                if (this.m_enRestartNow == enumBoolYesNo.Yes)
                {
                    this.m_enCompleteRounds = enumBoolYesNo.No;
                    UpdateVariableLists("Change map list only after map has complete it's total number of rounds?", this.m_enCompleteRounds);
                }
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if (strVariable.CompareTo("    Do not switch if more than this number of players are online") == 0 && int.TryParse(strValue, out iValue) == true)
			{
				if (iValue < 0)
				{
					iValue = 0;
				}
                else if (iValue > 69 && (this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl"))
                {
                    iValue = 69;
                }
                else if (iValue > 63 && this.m_strServerGameType != "bf4" && this.m_strServerGameType != "bfhl")
                {
                    iValue = 63;
                }

				this.m_iRestartLimit = iValue;
                UpdateVariableLists(strVariable, this.m_iRestartLimit);
			}
            else if (strVariable.CompareTo("    Warn (yell at) players before switching?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enRestartWarning = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enRestartWarning);
                if (strValue.CompareTo("Yes") == 0)
                {
                    this.m_LGDPVList = new List<CPluginVariable>();
                }
                else
                {
                    UpdateVariableLists("        Number of seconds to display warning", "delete");
                    UpdateVariableLists("        Warning message (use [listname] for map list name and [secs] for seconds)", "delete");
                }
            }
            else if (strVariable.CompareTo("        Number of seconds to display warning") == 0 && int.TryParse(strValue, out iValue) == true)
			{
				this.m_enRestartWarningTime = iValue;
				
				if (iValue < 0)
				{
					this.m_enRestartWarningTime = 0;
				}
				else if (iValue > 30)
				{
					this.m_enRestartWarningTime = 30;
				}
                UpdateVariableLists(strVariable, this.m_enRestartWarningTime);
			}
            else if (strVariable.CompareTo("        Warning message (use [listname] for map list name and [secs] for seconds)") == 0)
            {
                this.m_strRestartWarningMessage = strValue;
                UpdateVariableLists(strVariable, this.m_strRestartWarningMessage);
            }
            else if (strVariable.CompareTo("Change map list only after map has complete it's total number of rounds?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enCompleteRounds = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enCompleteRounds);
                if (this.m_enCompleteRounds == enumBoolYesNo.Yes)
                {
                    this.m_enRestartNow = enumBoolYesNo.No;
                    UpdateVariableLists("Switch to new map list immediately?", this.m_enRestartNow);
                }
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if (strVariable.CompareTo("Enable server name change for each map list?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enEnableServerName = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enEnableServerName);
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if (strVariable.CompareTo("Allow map-specific game presets?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enAllowPresets = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enAllowPresets);
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if (strVariable.CompareTo("    Send preset commands after every round?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enPresetsAlways = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enPresetsAlways);
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if (strVariable.CompareTo("    Allow additional presets to be sent after each level loads?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enLoadPresets = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enLoadPresets);
                this.m_LGDPVList = new List<CPluginVariable>();
            }
            else if ((strVariable.CompareTo("Enable day-dependent map lists?") == 0 || strVariable.CompareTo("Enable day/time-dependent map lists?") == 0) && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enTimeOptions = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enTimeOptions);
                this.m_LGDPVList = new List<CPluginVariable>();
                if (!this.m_blUseSystemTZ && this.m_enTimeOptions == enumBoolYesNo.Yes)
                {
                    WritePluginConsole("INFO -> Current day/time on this system: " + DateTime.Now.ToString("dddd") + ", " + DateTime.Now.ToString("t"));
                }
            }
            else if (strVariable.CompareTo("    Ignore 'Switch to new map list immediately' setting for time-based changes?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enTimeNotNow = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enTimeNotNow);
            }
            else if (strVariable.CompareTo("    Use system/layer server time zone?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enUseSystemTZ = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                UpdateVariableLists(strVariable, this.m_enUseSystemTZ);
                this.m_LGDPVList = new List<CPluginVariable>();
                DateTime newtime = DateTime.Now;
                if (!loadedFromConfig && this.m_enUseSystemTZ == enumBoolYesNo.Yes)
                {
                    WritePluginConsole("INFO -> Current day/time on this system: " + newtime.ToString("dddd") + ", " + newtime.ToString("t"));
                }
            }
            else if (strVariable.CompareTo("        Time Zone used to determine current time") == 0 && ((IList<string>)TimeZoneInformation.TimeZoneNames).Contains(strValue))
            {
                this.m_strTimeZone = strValue;
                UpdateVariableLists(strVariable, this.m_strTimeZone.Replace("+", "%2B"), this.m_strTimeZone);
                DateTime newtime = TimeZoneInformation.ToLocalTime(DateTime.UtcNow, this.m_strTimeZone);
                if (!loadedFromConfig)
                {
                    WritePluginConsole("INFO -> Current day/time using selected Time Zone: " + newtime.ToString("dddd") + ", " + newtime.ToString("t"));
                }
            }
            else if (strVariable.CompareTo("Enable debug output?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                this.m_LGDPVList = new List<CPluginVariable>();
                this.m_LGPVList = new List<CPluginVariable>();
            }
            else if (strVariable.Length > 13 && strVariable.Substring(0, 13).CompareTo("Custom Preset") == 0)
            {
                string newName = strVariable;
                string[] presetVars = CPluginVariable.DecodeStringArray(strValue);
                if (presetVars[0].Length > 17 && presetVars[0].Substring(0, 17).CompareTo("# Custom Preset: ") == 0)
                {
                    newName = presetVars[0].Substring(2);
                }
                for (int i=0; i < presetVars.Length; i++)
                {
                    presetVars[i] = presetVars[i].Replace("|", "-");
                }
                if (newName.CompareTo(strVariable) == 0 || this.m_DPreset.ContainsKey(newName))
                {
                    this.m_DPreset[strVariable] = presetVars;
                    UpdateVariableLists(strVariable, this.m_DPreset[strVariable]);
                }
                else
                {
                    this.m_DPreset.Remove(strVariable);
                    this.m_DPreset.Add(newName, presetVars);
                    foreach (MaplistConfig MCMaplist in this.m_LMaplists)
                    {
                        foreach (MaplistConfig.MaplistInfo MEMap in MCMaplist.Maplist)
                        {
                            if (MEMap.Preset.CompareTo(strVariable) == 0)
                            {
                                this.m_LMaplists[MCMaplist.Index].Maplist[MEMap.Index].Preset = newName;
                            }
                        }
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }

            }
            else if (strVariable.CompareTo("Manage Presets") == 0)
            {
                if (strValue.Length > 26 && strValue.Substring(0, 26).CompareTo("Add custom preset based on") == 0)
                {
                    List<string> newPreset = new List<string>(this.m_DPreset[strValue.Substring(27)]);
                    for (int i = 1; true; i++)
                    {
                        if (!this.m_DPreset.ContainsKey("Custom Preset: " + strValue.Substring(27) + " " + i)) {
                            newPreset.Insert(0, "# Custom Preset: " + strValue.Substring(27) + " " + i);
                            this.m_DPreset.Add("Custom Preset: " + strValue.Substring(27) + " " + i, newPreset.ToArray());
                            break;
                        }
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
                else if (strValue.Length > 6 && strValue.Substring(0, 6).CompareTo("Remove") == 0)
                {
                    this.m_DPreset.Remove(strValue.Substring(7));
                    List<string> deleteList = new List<string>();
                    foreach (KeyValuePair<string, string[]> preset in this.m_DPreset)
                    {
                        if (!(new List<string>(new string[] {"Normal", "Hardcore", "Infantry Only"}).Contains(preset.Key)))
                        {
                            deleteList.Add("|Remove " + preset.Key);
                        }
                    }
                    UpdateVariableLists("Manage Presets", "", "...", "enum.ManagePresets" + String.Join("", deleteList.ToArray()).Replace(" ", "") + "(...|Add custom preset based on Normal|Add custom preset based on Hardcore|Add custom preset based on Infantry Only" + String.Join("", deleteList.ToArray()) + ")", "...");
                    foreach (MaplistConfig MCMaplist in this.m_LMaplists)
                    {
                        foreach (MaplistConfig.MaplistInfo MEMap in MCMaplist.Maplist)
                        {
                            if (MEMap.Preset.CompareTo(strValue.Substring(7)) == 0)
                            {
                                this.m_LMaplists[MCMaplist.Index].Maplist[MEMap.Index].Preset = "Normal";
                                UpdateVariableLists("[" + MCMaplist.Index + "] [" + MEMap.Index + "] Preset", "Normal");
                            }
                        }
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
            }
            else if (this.m_enMapManager == enumBoolYesNo.Yes && this.m_isPluginEnabled && !loadedFromConfig)
            {
                WritePluginConsole("WARN -> Map list can not be altered while plugin is active. Set 'Enable map list manager?' to 'No' before altering your map list.", true);
            }
            else if (this.m_enMapManager == enumBoolYesNo.No || !this.m_isPluginEnabled || loadedFromConfig)
            {
                if (strVariable.CompareTo("Add a new map list?") == 0)
                {
                    if (strValue.CompareTo("Create an empty map list") == 0)
                    {
                        int i = this.m_LMaplists.Count;
                        this.m_LMaplists.Add(new MaplistConfig(i));
                        this.m_LMaplists[i].Name = "New Map List";
                        this.m_LMaplists[i].ServerName = this.m_strCurrentServerName.Replace("+", "%2B");
                        if (this.m_strServerGameType == "bf3") {
                            this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo(0));
                        } else if (this.m_strServerGameType == "mohw") {
                            this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo("CombatMission", "CombatMission", "MP_03", 2, 0));
                        } else if (this.m_strServerGameType == "bf4") {
                            this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo("ConquestLarge0", "Conquest Large", "MP_Abandoned", 2, 0));
                        } else if (this.m_strServerGameType == "bfhl") {
                            this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo("TurfWarLarge0", "Conquest Large", "MP_Downtown", 2, 0));
                        }
                        this.m_LGDPVList = new List<CPluginVariable>();
                        this.m_LGPVList = new List<CPluginVariable>();
                    }
                    else if (strValue.CompareTo("Create a map list based on current map list") == 0)
                    {
                        if (!this.m_isPluginEnabled)
                        {
                            WritePluginConsole("WARN -> This command will not work if the plugin is disabled in PRoCon (no checkmark in the plugins list).", true);
                        }
                        else if (this.m_LCurrentMapList.Count == 0)
                        {
                            WritePluginConsole("WARN -> There currently are not any maps in the active map list.", true);
                        }
                        else
                        {
                            int i = this.m_LMaplists.Count;
                            this.m_LMaplists.Add(new MaplistConfig(i));
                            this.m_LMaplists[i].Name = "New Map List";
                            this.m_LMaplists[i].ServerName = this.m_strCurrentServerName.Replace("+", "%2B");
                            foreach (MaplistEntry MEMap in this.m_LCurrentMapList)
                            {
                                string strPublicGameMode = "";
                                foreach (CMap map in this.GetMapDefines())
                                {
                                    if (MEMap.Gamemode.ToLower() == map.PlayList.ToLower() && MEMap.MapFileName.ToLower() == map.FileName.ToLower())
                                    {
                                        strPublicGameMode = map.GameMode;
                                        break;
                                    }
                                }
                                this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo(MEMap.Gamemode, strPublicGameMode, MEMap.MapFileName, MEMap.Rounds, this.m_LMaplists[i].Maplist.Count));
                                WritePluginConsole(MEMap.Gamemode + "/" + strPublicGameMode + "/" + MEMap.MapFileName + "/" + MEMap.Rounds + "/" + this.m_LMaplists[i].Maplist.Count);
                            }
                            this.m_LGDPVList = new List<CPluginVariable>();
                            this.m_LGPVList = new List<CPluginVariable>();
                        }
                    }
                }
                else if (strVariable.Length > 21 && strVariable.Substring(strVariable.Length-21).CompareTo("Enable this map list?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    // Add construct on initial load; versions 1.1.0.2+
                    if (this.m_LMaplists.Count == i)
                    {
                        this.m_LMaplists.Add(new MaplistConfig(i));
                    }
                    this.m_LMaplists[i].Enabled = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                    UpdateVariableLists(strVariable, this.m_LMaplists[i].Enabled);
                }
                else if (strVariable.Length > 13 && strVariable.Substring(strVariable.Length-13).CompareTo("Map list name") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    this.m_LMaplists[i].Name = strValue.Substring(0, Math.Min(strValue.Length, 63)).Replace("+", "%2B").Replace("|", "-");
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
                else if (strVariable.Length > 29 && strVariable.Substring(strVariable.Length-29).CompareTo("Server name for this map list") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    // Add construct on initial load; versions 1.1.0.2 and before
                    if (this.m_LMaplists.Count == i)
                    {
                        this.m_LMaplists.Add(new MaplistConfig(i));
                    }
                    this.m_LMaplists[i].ServerName = strValue.Substring(0, Math.Min(strValue.Length, 63)).Replace("+", "%2B");
                    UpdateVariableLists(strVariable, this.m_LMaplists[i].ServerName);
                }
                else if (strVariable.Length > 39 && strVariable.Substring(strVariable.Length-39).CompareTo("Days of week/month to use this map list") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    this.m_LMaplists[i].DaysValid = CPluginVariable.DecodeStringArray(strValue);
                    UpdateVariableLists(strVariable, this.m_LMaplists[i].DaysValid);
                }
                else if (strVariable.Length > 50 && strVariable.Substring(strVariable.Length-50).CompareTo("Time to start using this map list (24-hour format)") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    string time = "0:00";
                    if (int.TryParse(strValue, out iValue) == true)
                    {
                        if (iValue < 24 && iValue >= 0)
                        {
                            time = iValue.ToString() + ":00";
                        }
                    }
                    else if (strValue.Length <= 5 && strValue.IndexOf(':') != -1)
                    {
                        string[] parts = strValue.Split(':');
                        if (parts.Length == 2 && parts[0].Length <= 2 && parts[1].Length <= 2 && Int32.Parse(parts[0]) < 24 && Int32.Parse(parts[0]) >= 0 && Int32.Parse(parts[1]) < 60 && Int32.Parse(parts[1]) >= 0)
                        {
                            time = strValue;
                        }
                    }
                    this.m_LMaplists[i].TimeStart = time;
                    UpdateVariableLists(strVariable, time);
                }
                else if (strVariable.Length > 49 && strVariable.Substring(strVariable.Length-49).CompareTo("Time to stop using this map list (24-hour format)") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    string time = "23:59";
                    if (int.TryParse(strValue, out iValue) == true)
                    {
                        if (iValue < 24 && iValue >= 0)
                        {
                            time = iValue.ToString() + ":00";
                        }
                    }
                    else if (strValue.Length <= 5 && strValue.IndexOf(':') != -1)
                    {
                        string[] parts = strValue.Split(':');
                        if (parts.Length == 2 && parts[0].Length <= 2 && parts[1].Length <= 2 && Int32.Parse(parts[0]) < 24 && Int32.Parse(parts[0]) >= 0 && Int32.Parse(parts[1]) < 60 && Int32.Parse(parts[1]) >= 0)
                        {
                            time = strValue;
                        }
                    }
                    this.m_LMaplists[i].TimeStop = time;
                    UpdateVariableLists(strVariable, time);
                }
                else if (strVariable.Length > 25 && strVariable.Substring(strVariable.Length-25).CompareTo("Minimum number of players") == 0 && int.TryParse(strValue, out iValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    if (iValue < 0)
                    {
                        iValue = 0;
                    }
                    else if (iValue > 64)
                    {
                        iValue = 64;
                    }
                    this.m_LMaplists[i].MinPlayers = iValue;
                    UpdateVariableLists(strVariable, iValue);
                }
                else if (strVariable.Length > 25 && strVariable.Substring(strVariable.Length-25).CompareTo("Maximum number of players") == 0 && int.TryParse(strValue, out iValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    if (iValue < 0)
                    {
                        iValue = 0;
                    }
                    else if (iValue > 64)
                    {
                        iValue = 64;
                    }
                    this.m_LMaplists[i].MaxPlayers = iValue;
                    UpdateVariableLists(strVariable, iValue);
                }
                else if (strVariable.Length > 22 && strVariable.Substring(strVariable.Length-22).CompareTo("Start with random map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    this.m_LMaplists[i].MapListStart = "Start with random map";
                    UpdateVariableLists(strVariable, this.m_LMaplists[i].MapListStart);
                }
                else if (strVariable.Length > 28 && strVariable.Substring(strVariable.Length-28).CompareTo("List start/random preference") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    this.m_LMaplists[i].MapListStart = strValue;
                    UpdateVariableLists(strVariable, this.m_LMaplists[i].MapListStart);
                }
                else if (strVariable.Length > 37 && strVariable.Substring(strVariable.Length-37).CompareTo("Minimum number of rounds to be played") == 0 && int.TryParse(strValue, out iValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));

                    if (iValue < 0)
                    {
                        iValue = 0;
                    }
                    this.m_LMaplists[i].MinRounds = iValue;
                    UpdateVariableLists(strVariable, iValue);
                }
                else if (strVariable.Length > 14 && strVariable.Substring(strVariable.Length-14).CompareTo("Add a new map?") == 0 && strValue.CompareTo("...") != 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    for (int j = 0; j < int.Parse(strValue.Substring(4, 1)); j++)
                    {
                        this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo(this.m_LMaplists[i].Maplist[this.m_LMaplists[i].Maplist.Count-1].Gamemode, this.m_LMaplists[i].Maplist[this.m_LMaplists[i].Maplist.Count-1].PublicGamemode, this.m_LMaplists[i].Maplist[this.m_LMaplists[i].Maplist.Count-1].MapFileName, this.m_LMaplists[i].Maplist[this.m_LMaplists[i].Maplist.Count-1].Rounds, this.m_LMaplists[i].Maplist.Count));
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
                else if (strVariable.Length > 15 && strVariable.Substring(strVariable.Length-15).CompareTo("Manage Map List") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    if (strValue.CompareTo("Move Up") == 0 && i > 0)
                    {
                        this.m_LMaplists.Insert(i-1, this.m_LMaplists[i]);
                        this.m_LMaplists.RemoveAt(i+1);
                        for (int j = i-1; j < this.m_LMaplists.Count; j++)
                        {
                            this.m_LMaplists[j].Index = j;
                        }
                    }
                    if (strValue.CompareTo("Move Down") == 0 && i < this.m_LMaplists.Count - 1)
                    {
                        this.m_LMaplists.Insert(i+2, this.m_LMaplists[i]);
                        this.m_LMaplists.RemoveAt(i);
                        for (int j = i; j < this.m_LMaplists.Count; j++)
                        {
                            this.m_LMaplists[j].Index = j;
                        }
                    }
                    if (strValue.CompareTo("Remove Entire Map List") == 0)
                    {
                        this.m_LMaplists.RemoveAt(i);
                        for (int j = i; j < this.m_LMaplists.Count; j++)
                        {
                            this.m_LMaplists[j].Index = j;
                        }
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
                else if (strVariable.Length > 9 && strVariable.Substring(strVariable.Length-9).CompareTo("Game Mode") == 0 && (this.GetMapByFormattedName("{GameMode}", strValue) != null || loadedFromConfig))
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    // Add construct on initial load
                    if (this.m_LMaplists[i].Maplist.Count == j)
                    {
                        this.m_LMaplists[i].Maplist.Add(new MaplistConfig.MaplistInfo(j));
                    }
                    if (loadedFromConfig)
                    {
                        this.m_LMaplists[i].Maplist[j].Gamemode = strValue;
                    }
                    else
                    {
                        this.m_LMaplists[i].Maplist[j].PublicGamemode = strValue;
                        this.m_LMaplists[i].Maplist[j].Gamemode = this.GetMapByFormattedName("{GameMode}", strValue).PlayList;
                        List<string> LAvailableMaps = new List<string>();
                        foreach (CMap map in this.GetMapDefines())
                        {
                            if (strValue == map.GameMode)
                            {
                                LAvailableMaps.Add(map.FileName);
                            }
                        }
                        if (!LAvailableMaps.Contains(this.m_LMaplists[i].Maplist[j].MapFileName))
                        {
                            this.m_LMaplists[i].Maplist[j].MapFileName = LAvailableMaps[0];
                        }
                        List<string> LPublicGameModes = new List<string>();
                        foreach (CMap map in this.GetMapDefines())
                        {
                            if (map.GameMode == strValue)
                            {
                                LPublicGameModes.Add(map.PublicLevelName);
                            }
                        }
                        UpdateVariableLists(strVariable, this.m_LMaplists[i].Maplist[j].Gamemode, strValue);
                        UpdateVariableLists("[" + i + "] [" + j + "] Map Name", this.m_LMaplists[i].Maplist[j].MapFileName, this.GetMapByFilename(this.m_LMaplists[i].Maplist[j].MapFileName).PublicLevelName, "enum.Maps" + strValue.Replace(" ", "") +"(" + String.Join("|", LPublicGameModes.ToArray()) + ")");
                    }
                }
                else if (strVariable.Length > 8 && strVariable.Substring(strVariable.Length-8).CompareTo("Map Name") == 0 && (this.GetMapByFormattedName("{PublicLevelName}", strValue) != null || loadedFromConfig))
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    if (loadedFromConfig)
                    {
                        this.m_LMaplists[i].Maplist[j].MapFileName = strValue;
                        string strPublicGameMode = "";
                        foreach (CMap map in this.GetMapDefines())
                        {
                            if (this.m_LMaplists[i].Maplist[j].Gamemode == map.PlayList && strValue == map.FileName)
                            {
                                this.m_LMaplists[i].Maplist[j].PublicGamemode = map.GameMode;
                                break;
                            }
                            else if (this.m_LMaplists[i].Maplist[j].Gamemode == "ConquestLarge0" && map.PlayList == "ConquestAssaultLarge0" && strValue == map.FileName)
                            {
                                this.m_LMaplists[i].Maplist[j].Gamemode = map.PlayList;
                                this.m_LMaplists[i].Maplist[j].PublicGamemode = map.GameMode;
                                break;
                            }
                            else if (this.m_LMaplists[i].Maplist[j].Gamemode == "ConquestSmall0" && map.PlayList == "ConquestAssaultSmall0" && strValue == map.FileName)
                            {
                                this.m_LMaplists[i].Maplist[j].Gamemode = map.PlayList;
                                this.m_LMaplists[i].Maplist[j].PublicGamemode = map.GameMode;
                                break;
                            }
                            else if (this.m_LMaplists[i].Maplist[j].Gamemode == "ConquestSmall1" && map.PlayList == "ConquestAssaultSmall1" && strValue == map.FileName)
                            {
                                this.m_LMaplists[i].Maplist[j].Gamemode = map.PlayList;
                                this.m_LMaplists[i].Maplist[j].PublicGamemode = map.GameMode;
                                break;
                            }
                            else if (this.m_LMaplists[i].Maplist[j].Gamemode == "ConquestSmall1" && map.PlayList == "ConquestAssaultSmall0" && strValue == map.FileName)
                            {
                                this.m_LMaplists[i].Maplist[j].Gamemode = map.PlayList;
                                this.m_LMaplists[i].Maplist[j].PublicGamemode = map.GameMode;
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.m_LMaplists[i].Maplist[j].MapFileName = GetMapByFormattedName("{PublicLevelName}", strValue).FileName;
                        UpdateVariableLists(strVariable, this.m_LMaplists[i].Maplist[j].MapFileName, strValue);
                    }
                }
                else if (strVariable.Length > 6 && strVariable.Substring(strVariable.Length-6).CompareTo("Rounds") == 0 && int.TryParse(strValue, out iValue) == true)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    if (iValue < 1)
                    {
                        iValue = 1;
                    }
                    this.m_LMaplists[i].Maplist[j].Rounds = iValue;
                    UpdateVariableLists(strVariable, iValue);
                }
                else if (strVariable.Length > 6 && strVariable.Substring(strVariable.Length-6).CompareTo("Preset") == 0 && this.m_DPreset.ContainsKey(strValue))
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    this.m_LMaplists[i].Maplist[j].Preset = strValue;
                    UpdateVariableLists(strVariable, strValue);
                }
                else if (strVariable.Length > 26 && strVariable.Substring(strVariable.Length-26).CompareTo("Preset (after level loads)") == 0 && (strValue == "None" || this.m_DPreset.ContainsKey(strValue)))
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    this.m_LMaplists[i].Maplist[j].LoadPreset = strValue;
                    UpdateVariableLists(strVariable, strValue);
                }
                else if (strVariable.Length > 10 && strVariable.Substring(strVariable.Length-10).CompareTo("Manage Map") == 0)
                {
                    int i = int.Parse(strVariable.Substring(1,strVariable.IndexOf("]")-1));
                    int j = int.Parse(strVariable.Substring(strVariable.LastIndexOf("[")+1, strVariable.LastIndexOf("]")-strVariable.LastIndexOf("[")-1));
                    if (strValue.CompareTo("Move Up") == 0 && j > 0)
                    {
                        this.m_LMaplists[i].Maplist.Insert(j-1, this.m_LMaplists[i].Maplist[j]);
                        this.m_LMaplists[i].Maplist.RemoveAt(j+1);
                        for (int k = j-1; k < this.m_LMaplists[i].Maplist.Count; k++)
                        {
                            this.m_LMaplists[i].Maplist[k].Index = k;
                        }
                    }
                    if (strValue.CompareTo("Move Down") == 0 && j < this.m_LMaplists[i].Maplist.Count - 1)
                    {
                        this.m_LMaplists[i].Maplist.Insert(j+2, this.m_LMaplists[i].Maplist[j]);
                        this.m_LMaplists[i].Maplist.RemoveAt(j);
                        for (int k = j; k < this.m_LMaplists[i].Maplist.Count; k++)
                        {
                            this.m_LMaplists[i].Maplist[k].Index = k;
                        }
                    }
                    if (strValue.CompareTo("Remove Map") == 0 && this.m_LMaplists[i].Maplist.Count > 1)
                    {
                        this.m_LMaplists[i].Maplist.RemoveAt(j);
                        for (int k = j; k < this.m_LMaplists[i].Maplist.Count; k++)
                        {
                            this.m_LMaplists[i].Maplist[k].Index = k;
                        }
                    }
                    this.m_LGDPVList = new List<CPluginVariable>();
                    this.m_LGPVList = new List<CPluginVariable>();
                }
            }

			this.RegisterAllCommands();
        }

        private void UnregisterAllCommands() {
            List<string> emptyList = new List<string>();
            this.UnregisterCommand(new MatchCommand(emptyList, "maplist", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("map list index",emptyList), new MatchArgumentFormat("on/off",emptyList))));
            this.UnregisterCommand(new MatchCommand(emptyList, "maplist", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("map list index",emptyList))));
            this.UnregisterCommand(new MatchCommand(emptyList, "maplist", this.Listify<MatchArgumentFormat>()));
        }

        private void SetupHelpCommands() {
        }

        private void RegisterAllCommands() {
            if (this.m_isPluginEnabled && this.m_enAdminCommands == enumBoolYesNo.Yes) {
                List<string> mapListIndices = new List<string>();
                foreach (MaplistConfig MCMaplist in this.m_LMaplists) {
                    mapListIndices.Add(MCMaplist.Index.ToString());
                }
                mapListIndices.Add("all");

                this.RegisterCommand(new MatchCommand("CUltimateMapManager","OnCommandMapList", this.Listify<string>("@", "!", "#"), "maplist", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("map list index", mapListIndices), new MatchArgumentFormat("on/off", new List<string>(new string[] {"on", "off"}))), new ExecutionRequirements(ExecutionScope.Privileges,Privileges.CanUseMapFunctions,"You do not have enough privileges to change the map list"),"Enables or disables the specified map list"));
                this.RegisterCommand(new MatchCommand("CUltimateMapManager","OnCommandMapListShow", this.Listify<string>("@", "!", "#"), "maplist", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("map list index", mapListIndices)), new ExecutionRequirements(ExecutionScope.Privileges,Privileges.CanUseMapFunctions,"You do not have enough privileges to change the map list"),"Displays the status of the specified map list"));
                this.RegisterCommand(new MatchCommand("CUltimateMapManager","OnCommandMapListShowCurrent", this.Listify<string>("@", "!", "#"), "maplist", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.Privileges,Privileges.CanUseMapFunctions,"You do not have enough privileges to change the map list"),"Displays the status of all map lists"));
            }
        }

        public override void OnLogin() {
            ResetVars();
        }

        public override void OnServerInfo(CServerInfo csiServerInfo) {
            this.m_strCurrentMap = csiServerInfo.Map;
            this.m_strCurrentServerName = csiServerInfo.ServerName;
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                this.m_iCurrentPlayerCount = lstPlayers.Count;
                if (this.m_isPluginInitialized)
                {
                    if (this.m_iCurrentPlayerCount == 0 && !this.m_blRoundEnded || this.m_enRestartNow == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount <= this.m_iRestartLimit && !this.m_blRoundEnded)
                    {
                        this.m_blCheckPlayerCount = true;
                        GetMapInfo();
                    }
                }
                else 
                {
                    GetMapInfo();
                }
            }
        }

        public override void OnMaplistSave() {
            this.ExecuteCommand("procon.protected.send", "mapList.list");
        }

        public override void OnMaplistGetRounds(int currentRound, int totalRounds) {
            this.m_blIsFirstRound = (currentRound == 0);
            this.m_blIsLastRound = (currentRound + 1 >= totalRounds);
        }

        public override void OnMaplistGetMapIndices(int mapIndex, int nextIndex) {
            if (this.m_blRestartRequested || !this.m_blIsLastRound)
            {
                this.m_iNextMapIndex = mapIndex;
            }
            else
            {
                this.m_iNextMapIndex = nextIndex;
            }
            if (this.m_blCheckPlayerCount)
            {
                this.m_blCheckPlayerCount = false;
                if (this.m_blRoundEnded && !this.m_blRestartRequested)
                {
                    this.ExecuteCommand("procon.protected.tasks.add", "CUltimateMapManager", "50", "1", "1", "procon.protected.plugins.call", "CUltimateMapManager", "CheckPlayerCount");
                }
                else
                {
                    CheckPlayerCount();
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
            if (this.m_isPluginInitialized)
            {
                this.m_iCurrentPlayerCount--;
                if (this.m_iCurrentPlayerCount == 0 && !this.m_blRoundEnded || this.m_enRestartNow == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount <= this.m_iRestartLimit && !this.m_blRoundEnded)
                {
                    this.m_blCheckPlayerCount = true;
                    GetMapInfo();
                }
            }
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores) {
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = false;
            this.m_blCheckPlayerCount = true;
            this.m_iCurrentRoundCount++;
            WritePluginConsole("INFO -> Round ended. Rounds run on this map list: " + this.m_iCurrentRoundCount);
            GetMapInfo();
        }      

        public override void OnRestartLevel() {
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = true;
            this.m_blCheckPlayerCount = true;
            this.m_iCurrentRoundCount++;
            WritePluginConsole("INFO -> Round restart requested. Rounds run on this map list: " + this.m_iCurrentRoundCount);
            GetMapInfo();
        }

        public override void OnRunNextLevel() {
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = true;
            this.m_blCheckPlayerCount = true;
            this.m_iCurrentRoundCount++;
            WritePluginConsole("INFO -> Next round requested. Rounds run on this map list: " + this.m_iCurrentRoundCount);
            GetMapInfo();
        }      

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            if (this.m_enLoadPresets == enumBoolYesNo.Yes) {
                this.ExecuteCommand("procon.protected.tasks.add", "CUltimateMapManager", "5", "1", "1", "procon.protected.plugins.call", "CUltimateMapManager", "SetLoadPreset");
            }
            if (this.m_iCurrentPlayerCount == 0) {
                this.m_blCheckPlayerCount = true;
                GetMapInfo();
            }
            WritePluginConsole("INFO -> Level loaded.");
        }

        public override void OnMaplistList(List<MaplistEntry> lstMaplist) {
            this.m_LCurrentMapList = lstMaplist;
            this.m_isPluginInitialized = true;
        }

        private void GetMapInfo() {
            this.ExecuteCommand("procon.protected.send", "mapList.list");
            this.ExecuteCommand("procon.protected.send", "mapList.list", "100");
            this.ExecuteCommand("procon.protected.send", "mapList.getRounds");
            this.ExecuteCommand("procon.protected.send", "mapList.getMapIndices");
        }

        private static List<T> Shuffle<T>(IList<T> list)  
        {  
            Random rng = new Random();  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
            return new List<T>(list);
        }

        public void VotedMapInfo(string mapFileName, string gameMode) {
            this.m_strVotedMapFileName = mapFileName;
            this.m_strVotedGameMode = gameMode;
        }

        private void CheckPreset() {
            if (this.m_strCurrentPreset != this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].Preset || this.m_enPresetsAlways == enumBoolYesNo.Yes)
            {
                this.m_strCurrentPreset = this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].Preset;
                WritePluginConsole("WORK -> Next map: " + this.GetMapByFilename(this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].MapFileName).PublicLevelName + " (" + this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].PublicGamemode + "). Setting server preset to " + this.m_strCurrentPreset + ".");
                foreach (string setting in this.m_DPreset[this.m_strCurrentPreset])
                {
                    if (setting.Length > 0 && setting.Substring(0,1).CompareTo("#") != 0)
                    {
                        int split = setting.IndexOf(' ');
                        this.ExecuteCommand("procon.protected.send", setting.Substring(0, split), setting.Substring(split+1).Trim('"'));
                    }
                }
            }
        }

        public void SetLoadPreset() {
            if (this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].LoadPreset != "None") {
                this.m_strCurrentPreset = this.m_LMaplists[this.m_iCurrentMapList].Maplist[this.m_iNextMapIndex].LoadPreset;
                WritePluginConsole("WORK -> Level loaded. Setting server preset to " + this.m_strCurrentPreset + ".");
                foreach (string setting in this.m_DPreset[this.m_strCurrentPreset])
                {
                    if (setting.Length > 0 && setting.Substring(0,1).CompareTo("#") != 0)
                    {
                        int split = setting.IndexOf(' ');
                        this.ExecuteCommand("procon.protected.send", setting.Substring(0, split), setting.Substring(split+1).Trim('"'));
                    }
                }
            }
        }

        public void OnCommandMapList(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAdminCommands == enumBoolYesNo.Yes) {
                int iMaplist = 0;
                if (int.TryParse(capCommand.MatchedArguments[0].Argument, out iMaplist) == true) {
                    if (capCommand.MatchedArguments[1].Argument == "on") {
                        this.ExecuteCommand("procon.protected.plugins.setVariable", "CUltimateMapManager", "["+this.m_LMaplists[iMaplist].Index+"] Enable this map list?", "CONFIG:Yes");
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+this.m_LMaplists[iMaplist].Index+" ("+this.m_LMaplists[iMaplist].Name+") has been ENABLED.", "player", strSpeaker);
                    } else if (capCommand.MatchedArguments[1].Argument == "off") {
                        this.ExecuteCommand("procon.protected.plugins.setVariable", "CUltimateMapManager", "["+this.m_LMaplists[iMaplist].Index+"] Enable this map list?", "CONFIG:No");
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+this.m_LMaplists[iMaplist].Index+" ("+this.m_LMaplists[iMaplist].Name+") has been DISABLED.", "player", strSpeaker);
                    }
                }
            }
        }

        public void OnCommandMapListShow(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAdminCommands == enumBoolYesNo.Yes) {
                int iMaplist = 0;
                if (capCommand.MatchedArguments[0].Argument == "all") {
                    foreach (MaplistConfig MCMaplist in this.m_LMaplists) {
                        if (MCMaplist.Enabled == enumBoolYesNo.Yes) {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+MCMaplist.Index+" ("+MCMaplist.Name+") is ENABLED.", "player", strSpeaker);
                        } else {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+MCMaplist.Index+" ("+MCMaplist.Name+") is DISABLED.", "player", strSpeaker);
                        }
                    }
                } else if (int.TryParse(capCommand.MatchedArguments[0].Argument, out iMaplist) == true) {
                    if (this.m_LMaplists[iMaplist].Enabled == enumBoolYesNo.Yes) {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+this.m_LMaplists[iMaplist].Index+" ("+this.m_LMaplists[iMaplist].Name+") is ENABLED.", "player", strSpeaker);
                    } else {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Map List "+this.m_LMaplists[iMaplist].Index+" ("+this.m_LMaplists[iMaplist].Name+") is DISABLED.", "player", strSpeaker);
                    }
                }
            }
        }

        public void OnCommandMapListShowCurrent(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAdminCommands == enumBoolYesNo.Yes) {
                this.ExecuteCommand("procon.protected.send", "admin.say", "Current map list is Map List "+this.m_iCurrentMapList+" ("+this.m_LMaplists[this.m_iCurrentMapList].Name+").", "player", strSpeaker);
            }
        }

        public void CheckPlayerCount() {
            bool mapListChanged = false;
            if (this.m_isPluginInitialized && this.m_enMapManager == enumBoolYesNo.Yes)
            {
                // determine current map list
                if (this.m_iCurrentMapList == -1)
                {
                    int foundCurrentMapList = -1;
                    foreach (MaplistConfig MCMaplist in this.m_LMaplists)
                    {
                        if (MCMaplist.Enabled == enumBoolYesNo.Yes && MCMaplist.Maplist.Count == this.m_LCurrentMapList.Count && this.m_iCurrentPlayerCount >= MCMaplist.MinPlayers && this.m_iCurrentPlayerCount <= MCMaplist.MaxPlayers && (this.m_enTimeOptions == enumBoolYesNo.No || this.m_enTimeOptions == enumBoolYesNo.Yes && MCMaplist.isValidDay(this.m_strTimeZone, (this.m_blUseSystemTZ && this.m_enUseSystemTZ == enumBoolYesNo.No), MCMaplist.TimeStart, MCMaplist.TimeStop)))
                        {
                            foundCurrentMapList = MCMaplist.Index;
                            foreach (MaplistConfig.MaplistInfo MEMap in MCMaplist.Maplist)
                            {
                                if (MEMap.MapFileName != this.m_LCurrentMapList[MEMap.Index].MapFileName || MEMap.Gamemode != this.m_LCurrentMapList[MEMap.Index].Gamemode || MEMap.Rounds != this.m_LCurrentMapList[MEMap.Index].Rounds)
                                {
                                    foundCurrentMapList = -1;
                                    break;
                                }
                            }
                            if (foundCurrentMapList != -1)
                            {
                                break;
                            }
                        }
                    }
                    if (foundCurrentMapList >= 0)
                    {
                        this.m_iCurrentMapList = foundCurrentMapList;
                        WritePluginConsole("INFO -> " + this.m_iCurrentPlayerCount + " players. Detected currently running Map List " + this.m_iCurrentMapList + " [" + this.m_LMaplists[this.m_iCurrentMapList].Name + "].");
                    }
                }

                // check map list
                //if (this.m_iCurrentMapList >=0)
                //{
                //    WritePluginConsole("DEBUG: List " + this.m_iCurrentMapList.ToString() + " / Round " + this.m_iCurrentRoundCount.ToString() + " / Min " + this.m_LMaplists[this.m_iCurrentMapList].MinRounds.ToString() + " / First? " + this.m_blIsFirstRound.ToString() + " / Last? " + this.m_blIsLastRound.ToString());
                //} else {
                //    WritePluginConsole("DEBUG: No active list.");
                //}
                if (this.m_iCurrentMapList < 0 || this.m_LMaplists[this.m_iCurrentMapList].Enabled == enumBoolYesNo.No || (this.m_iCurrentRoundCount >= this.m_LMaplists[this.m_iCurrentMapList].MinRounds || this.m_iCurrentMapList == -1 || this.m_iCurrentPlayerCount == 0) && (this.m_enCompleteRounds == enumBoolYesNo.No || this.m_enCompleteRounds == enumBoolYesNo.Yes && (this.m_iCurrentPlayerCount == 0 || this.m_blRoundEnded && (this.m_blRestartRequested && this.m_blIsFirstRound || !this.m_blRestartRequested && this.m_blIsLastRound))) && (this.m_iCurrentMapList == -1 || this.m_iCurrentPlayerCount > this.m_LMaplists[this.m_iCurrentMapList].MaxPlayers || this.m_iCurrentPlayerCount < this.m_LMaplists[this.m_iCurrentMapList].MinPlayers || this.m_enTimeOptions == enumBoolYesNo.Yes && (this.m_enTimeNotNow == enumBoolYesNo.No || this.m_enTimeNotNow == enumBoolYesNo.Yes && (this.m_iCurrentPlayerCount == 0 || this.m_blRoundEnded)) && !this.m_LMaplists[this.m_iCurrentMapList].isValidDay(this.m_strTimeZone, (this.m_blUseSystemTZ && this.m_enUseSystemTZ == enumBoolYesNo.No), this.m_LMaplists[this.m_iCurrentMapList].TimeStart, this.m_LMaplists[this.m_iCurrentMapList].TimeStop)))
                {
                    foreach (MaplistConfig MCMaplist in this.m_LMaplists)
                    {
                        if (MCMaplist.Enabled == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount >= MCMaplist.MinPlayers && this.m_iCurrentPlayerCount <= MCMaplist.MaxPlayers && (this.m_enTimeOptions == enumBoolYesNo.No || this.m_enTimeOptions == enumBoolYesNo.Yes && MCMaplist.isValidDay(this.m_strTimeZone, (this.m_blUseSystemTZ && this.m_enUseSystemTZ == enumBoolYesNo.No), MCMaplist.TimeStart, MCMaplist.TimeStop)))
                        {
                            WritePluginConsole("TASK -> " + this.m_iCurrentPlayerCount + " players. Need to change to Map List " + MCMaplist.Index + " [" + MCMaplist.Name + "].");
                            this.m_iCurrentMapList = MCMaplist.Index;
                            mapListChanged = true;
                            break;
                        }
                    }
                    if (!mapListChanged)
                    {
                        WritePluginConsole("WARN -> " + this.m_iCurrentPlayerCount + " players. No map list is available for this number of players; keeping current map list.");
                    }
                }

                // change server name
                if (this.m_enEnableServerName == enumBoolYesNo.Yes && this.m_LMaplists[this.m_iCurrentMapList].ServerName.CompareTo("") != 0 && this.m_blRoundEnded)
                {
                    this.ExecuteCommand("procon.protected.send", "vars.serverName", this.m_LMaplists[this.m_iCurrentMapList].ServerName.Replace("%2B", "+"));
                }

                // change map list, if necessary
                if (mapListChanged)
                {
                    this.m_iCurrentRoundCount = 0;
                    WritePluginConsole("WORK -> Changing to Map List " + this.m_iCurrentMapList + " [" + this.m_LMaplists[this.m_iCurrentMapList].Name + "].");
                    List<MaplistConfig.MaplistInfo> mapList = new List<MaplistConfig.MaplistInfo>(this.m_LMaplists[this.m_iCurrentMapList].Maplist);
                    if (this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Randomize entire map list") == 0)
                    {
                        mapList = Shuffle(mapList);
                    }
                    int mapIndex = 0;
                    int nextMapIndex = 0;
                    if (this.m_strVotedMapFileName != "")
                    {
                        foreach (MaplistConfig.MaplistInfo MEMap in mapList)
                        {
                            if (MEMap.MapFileName == this.m_strVotedMapFileName)
                            {
                                nextMapIndex = mapIndex;
                                if (MEMap.Gamemode == this.m_strVotedGameMode)
                                {
                                    break;
                                }
                            }
                            mapIndex++;
                        }
                    }
                    else if (this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Start with random map") == 0)
                    {
                        int randomNumber = (new Random()).Next(0, mapList.Count-1);
                        if (this.m_strCurrentMap == mapList[randomNumber].MapFileName)
                        {
                            nextMapIndex = (randomNumber + 1) % mapList.Count;
                        }
                        else
                        {
                            nextMapIndex = randomNumber;
                        }
                    }
                    else if ((this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Start with first map unless it was just played") == 0 || this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Randomize entire map list") == 0) && this.m_strCurrentMap == mapList[0].MapFileName)
                    {
                        nextMapIndex = 1;
                    }
                    else if (this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Start with map after the map that was just played") == 0)
                    {
                        foreach (MaplistConfig.MaplistInfo MEMap in mapList)
                        {
                            if (MEMap.MapFileName == this.m_strCurrentMap)
                            {
                                nextMapIndex = (mapIndex + 1) % mapList.Count;
                                break;
                            }
                            mapIndex++;
                        }
                    }
                    else if (this.m_LMaplists[this.m_iCurrentMapList].MapListStart.CompareTo("Start with same map that was just played") == 0)
                    {
                        foreach (MaplistConfig.MaplistInfo MEMap in mapList)
                        {
                            if (MEMap.MapFileName == this.m_strCurrentMap)
                            {
                                nextMapIndex = mapIndex;
                                break;
                            }
                            mapIndex++;
                        }
                    }
                    this.m_iNextMapIndex = nextMapIndex;
                    if (this.m_enAllowPresets == enumBoolYesNo.Yes) {
                        CheckPreset();
                    }
                    this.ExecuteCommand("procon.protected.send", "mapList.clear");
                    foreach (MaplistConfig.MaplistInfo MEMap in mapList)
                    {
                        this.ExecuteCommand("procon.protected.send", "mapList.add", MEMap.MapFileName, MEMap.Gamemode, MEMap.Rounds.ToString());
                    }
                    this.ExecuteCommand("procon.protected.send", "mapList.setNextMapIndex", nextMapIndex.ToString());
                    if (this.m_iCurrentPlayerCount == 0 && (!this.m_blRoundEnded || this.m_blRoundEnded && this.m_blRestartRequested) || this.m_enRestartNow == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount <= this.m_iRestartLimit && !this.m_blRoundEnded)
                    {
                        this.m_iCurrentRoundCount = -1;
                        if (this.m_enRestartWarning == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount > 0)
                        {
                            if (this.m_strServerGameType == "bf3" || this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl") {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", this.m_strRestartWarningMessage.Replace("[listname]", this.m_LMaplists[this.m_iCurrentMapList].Name).Replace("[secs]", this.m_enRestartWarningTime.ToString()), (this.m_enRestartWarningTime +1).ToString(), "all");
                            } else if (this.m_strServerGameType == "mohw") {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", this.m_strRestartWarningMessage.Replace("[listname]", this.m_LMaplists[this.m_iCurrentMapList].Name).Replace("[secs]", this.m_enRestartWarningTime.ToString()), (this.m_enRestartWarningTime +1).ToString(), "8", "all");
                            }
                            this.ExecuteCommand("procon.protected.tasks.add", "CUltimateMapManager", (this.m_enRestartWarningTime + 3).ToString(), "1", "1", "procon.protected.plugins.call", "CUltimateMapManager", "DelayedNextRound");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                        }
                    }
                    this.m_strVotedMapFileName = "";
                    this.m_strVotedGameMode = "";
                }
                else if (this.m_enAllowPresets == enumBoolYesNo.Yes && this.m_blRoundEnded)
                {
                    CheckPreset();
                }
            }
            this.m_blRoundEnded = false;
            this.m_blRestartRequested = false;
        }

        public void DelayedNextRound() {
            if (!this.m_blRoundEnded)
            {
                this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
            }
        }
      #endregion

#region helper_functions

        private void WritePluginConsole(string message)
        {
            string line = String.Format("UltimateMapManager: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes) { 
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        private void WritePluginConsole(string message, bool force)
        {
            string line = String.Format("UltimateMapManager: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes || force) { 
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        private void WriteMessage(string message)
        {
            List<string> wordWrappedLines = this.WordWrap(message, 100);
            foreach (string line in wordWrappedLines)
            {
                string formattedLine = String.Format("{0}", line);
                this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "all");
            }
        }
#endregion

    }
}
