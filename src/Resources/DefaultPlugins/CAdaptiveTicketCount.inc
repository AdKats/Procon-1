/*  Copyright 2011 falcontx
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

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class CAdaptiveTicketCount : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        enumBoolYesNo m_enConquestOnly;
        private int m_iCurrentPlayerCount;
        private List<MaplistEntry> m_LMapList;
        private string m_sNextGameMode;
        private string m_strTicketCountUnit;
        private string m_strSetPreset;
        private bool m_blUnitActualNumber;
        private string m_sNextMapFileName;
        private bool m_blRoundEnded;
        private Dictionary<string, int[]> m_DPlayerTickets;
        private Dictionary<string, int[]> m_DMapTickets;
        private Dictionary<string, string> m_DEnabledGM;
        private Dictionary<string, string> m_DShortGM;
        private Dictionary<string, string> m_DLongGM;
        private bool m_blRestartRequested;
        private bool m_blIsLastRound;
        private int m_iOtherTicketCount;
        private int m_iCurrentGameModeCounter;
        private long m_lLastRoundEnded;
        private bool m_blUpdateGameModeCounter;
        private bool m_blTimerActive;

        enumBoolYesNo m_enDoDebugOutput;

        private bool m_isPluginEnabled;
        private bool m_isPluginInitialized;
        private string m_strServerGameType;
        private string m_strGameMod;
        private string m_strServerVersion;

        public CAdaptiveTicketCount() {

            this.m_iCurrentPlayerCount     = 0;
            this.m_LMapList                = new List<MaplistEntry>();
            this.m_sNextGameMode           = "";
            this.m_strTicketCountUnit      = "Percentage (default)";
            this.m_blUnitActualNumber      = false;
            this.m_strSetPreset            = "No";
            this.m_sNextMapFileName        = "";
            this.m_blRoundEnded            = false;
            this.m_blRestartRequested      = false;
            this.m_blIsLastRound           = true;
            this.m_iOtherTicketCount       = 100;
            this.m_iCurrentGameModeCounter = 0;
            this.m_lLastRoundEnded         = DateTime.UtcNow.Ticks/10000000;
            this.m_blUpdateGameModeCounter = false;
            this.m_blTimerActive           = false;

            this.m_DPlayerTickets          = new Dictionary<string, int[]>();
            this.m_DMapTickets             = new Dictionary<string, int[]>();
            this.m_DEnabledGM              = new Dictionary<string, string>();
            this.m_DShortGM                = new Dictionary<string, string>();
            this.m_DLongGM                 = new Dictionary<string, string>();

            this.m_enDoDebugOutput         = enumBoolYesNo.Yes;

            this.m_isPluginEnabled         = false;
            this.m_isPluginInitialized     = false;
            this.m_strServerGameType       = "none";
        }

        public string GetPluginName() {
            return "Adaptive Ticket Count";
        }

        public string GetPluginVersion() {
            return "1.2.1.1";
        }

        public string GetPluginAuthor() {
            return "falcontx";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net/forumvb/showthread.php?3316";
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
    <input type=""hidden"" name=""item_name"" value=""Support Free Plugin Development (Adaptive Ticket Count)"">
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
    <p>This plugin is intended to adjust the number of tickets based upon the number of players on the server just before the new maps is started (it checks right after the countdown on the stats screen finishes). The number of tickets can be specified as a percentage of the map default, or as an exact number (so that all maps will be the same).</p>
    <p>This plugin can be used for any or all game modes. Each game mode can be enabled or disabled explicitly, and can use a default list of ticket counts or a list specifically for that game mode.</p>

<h2>Commands</h2>
    <p>This plug-in has no in-game commands.</p>

<h2>Settings</h2>
    <br><h3>Adaptive Ticket Count</h3>
        <blockquote><h4>Enable for [Game Mode]?</h4> Used to enable the plugin for the specified game mode. If ""Use default ticket count"" is chosen, the ticket count values from the default list will be used. If ""Use game-mode-specific ticket count"" is chosen, a new ticket count list will be added for this game mode.</blockquote>
        <blockquote><h4>Ticket percentage for disabled game modes</h4> Ticket percentage to use on game modes for which the plugin is disabled.</blockquote>
        <blockquote><h4>Units used for ticket count</h4> Choose whether you would prefer to specify the number of tickets as a percentage of the map default or as an exact number (so that all maps will be the same). On maps that start with a different number of tickets for each side (i.e. Back To Karkand maps), the specified ticket count will be applied to the side with the larger number of tickets.</blockquote>
        <blockquote><h4>Set preset after map loads?</h4> [BF4 only] Optionally send a preset command after the round loads in order to show ""Normal"", ""Hardcore"", or ""Infantry"" in Battelog despite having a ticket count other than 100%.</blockquote>
    <br><h3>[Game Mode] Ticket Count</h3>
        <blockquote><h4>[XXX] Tickets with [#] players</h4> The percentage or number of tickets that will be set when X players are active on the server. There is a default list, as well as a list for each game mode that is configured to have it's own ticket ocunt list. If you don't run a 64-player server, just ignore the options above your max server size, as they will not be used.</blockquote>
    <br><h3>Extras</h3>
        <blockquote><h4>Enable debug output?</h4> If enabled, displays debug info in the console window.</blockquote>

<br><h2>Development</h2>
    <br><h3>Changelog</h3>
        <blockquote><h4>1.2.1.1 (12/8/2013)</h4>
            - Added Air Superiority for BF4<br/>
        </blockquote>
        <blockquote><h4>1.2.1.0 (12/7/2013)</h4>
            - Added China Rising maps for BF4<br/>
            - Added default ticket counts to hopefully support new map packs before the plugin is updated<br/>
            - Added percentage limits for BF4 (min 75, max 400)<br/>
            - Added option to set preset after map loads in BF4<br/>
        </blockquote>
        <blockquote><h4>1.2.0.1 (11/14/2013)</h4>
            - fixed ticket count not always being calculated correctly at round end in BF4<br/>
        </blockquote>
        <blockquote><h4>1.2.0.0 (11/11/2013)</h4>
            - added game detection<br/>
            - added support for BF4, including all new maps and game modes<br/>
            - ticket count is set between every round, even if it hasn't changed<br/>
            - ticket count now defaults to 100% on unsupported game modes (Obliteration, etc.)<br/>
        </blockquote>
        <blockquote><h4>1.1.4.2 (03/14/2013)</h4>
            - fixed Capture The Flag ticket percentage calculation<br/>
        </blockquote>
        <blockquote><h4>1.1.4.1 (03/12/2013)</h4>
            - fixed Capture The Flag ticket actual ticket calculation<br/>
        </blockquote>
        <blockquote><h4>1.1.4.0 (03/12/2013)</h4>
            - added support for Air Superiority, Capture The Flag and new maps<br/>
        </blockquote>
        <blockquote><h4>1.1.3.3 (01/31/2013)</h4>
            - added some missing game modes to various maps<br/>
        </blockquote>
        <blockquote><h4>1.1.3.2 (12/20/2012)</h4>
            - fixed ticket calculation bug caused by changes in version 1.1.3.1<br/>
        </blockquote>
        <blockquote><h4>1.1.3.1 (12/12/2012)</h4>
            - fixed ticket calculation bug when map list is changed<br/>
            - added support for TDM Close Quarters on the XP4 maps<br/>
        </blockquote>
        <blockquote><h4>1.1.3.0 (12/04/2012)</h4>
            - added support for Scavenger and new maps<br/>
        </blockquote>
        <blockquote><h4>1.1.2.0 (09/09/2012)</h4>
            - added support for Tank Superiority<br/>
        </blockquote>
        <blockquote><h4>1.1.1.3 (07/20/2012)</h4>
            - fixed minor bug that may have affected the ticket count for disabled game modes on startup<br/>
        </blockquote>
        <blockquote><h4>1.1.1.2 (07/10/2012)</h4>
            - fixed minor bug caused by changes in PRoCon 1.3<br/>
        </blockquote>
        <blockquote><h4>1.1.1.1 (06/15/2012)</h4>
            - fixed minor bug surrounding new game modes<br/>
        </blockquote>
        <blockquote><h4>1.1.1.0 (06/14/2012)</h4>
            - added support for new game modes<br/>
        </blockquote>
        <blockquote><h4>1.1.0.0 (04/17/2012)</h4>
            - added options to enable the plugin for each game mode individually<br/>
            - fixed ""Actual number of tickets"" calculation for non-Conquest game modes<br/>
            - minor compatibility changes due to PRoCon/R-20 updates<br/>
        </blockquote>
        <blockquote><h4>1.0.1.2 (01/05/2012)</h4>
            - fixed wrong number of players detected when listplayers not called with 'all'<br/>
        </blockquote>
        <blockquote><h4>1.0.1.1 (12/19/2011)</h4>
            - minor bug fix related to saving/restoring ticket values<br/>
            - plugin will no longer send command to server if new ticket count is the same as last round<br/>
        </blockquote>
        <blockquote><h4>1.0.1.0 (12/14/2011)</h4>
            - added defaults for Conquest Small maps, so ""Actual number of tickets"" is calculated properly in this mode<br/>
            - added option to specify value for non-Conquest maps
        </blockquote>
        <blockquote><h4>1.0.0.0 (12/14/2011)</h4>
            - initial version<br/>
        </blockquote>
";
        }

        #region pluginSetup
        public void OnPluginLoadingEnv(List<string> lstPluginEnv) {
            this.m_strServerGameType = lstPluginEnv[1].ToLower();
            this.m_strGameMod = lstPluginEnv[2];
            this.m_strServerVersion = lstPluginEnv[3];
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
      
            if (this.m_strServerGameType == "bf3")
            {
                this.m_DPlayerTickets.Add("default",               new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, -1, -1, 300});
                this.m_DPlayerTickets.Add("ConquestLarge0",        new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 0, 1, 300});
                this.m_DPlayerTickets.Add("ConquestAssaultLarge0", new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 2, 3, 300});
                this.m_DPlayerTickets.Add("ConquestSmall0",        new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 4, 5, 300});
                this.m_DPlayerTickets.Add("ConquestAssaultSmall0", new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 6, 7, 300});
                this.m_DPlayerTickets.Add("ConquestAssaultSmall1", new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 8, 9, 300});
                this.m_DPlayerTickets.Add("RushLarge0",            new int[68] {75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 10, 10, 75});
                this.m_DPlayerTickets.Add("SquadRush0",            new int[68] {20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 13, 13, 20});
                this.m_DPlayerTickets.Add("SquadDeathMatch0",      new int[68] {50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 12, 12, 50});
                this.m_DPlayerTickets.Add("TeamDeathMatch0",       new int[68] {100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 11, 11, 100});
                this.m_DPlayerTickets.Add("TeamDeathMatchC0",      new int[68] {100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 14, 14, 100});
                this.m_DPlayerTickets.Add("Domination0",           new int[68] {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 15, 15, 150});
                this.m_DPlayerTickets.Add("TankSuperiority0",      new int[68] {200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 320, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 16, 16, 200});
                this.m_DPlayerTickets.Add("Scavenger0",            new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 17, 17, 300});
                this.m_DPlayerTickets.Add("AirSuperiority0",       new int[68] {250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 330, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 18, 18, 250});
                this.m_DPlayerTickets.Add("CaptureTheFlag0",       new int[68] {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 19, 19, 3});

                // Values:                                        |   CQL   |  CQAL   |   CQS   |  CQAS0  |  CQAS1  |RUSH| TDM|SQDM|SQRU|TDMC| DOM| TS | SC | AS | CTF|
                // Indices:                                       |  0    1 |  2    3 |  4    5 |  6    7 |  8    9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 |
                this.m_DMapTickets.Add("default",     new int[20] { 350, 350, 350, 350, 250, 250, 250, 250, 250, 250,  75, 100,  50,  20, 100, 150, 200, 300, 250,   3});
                this.m_DMapTickets.Add("MP_Subway",   new int[20] { 350, 350,   0,   0, 350, 350,   0,   0,   0,   0, 100, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_013",      new int[20] { 300, 300,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_007",      new int[20] { 300, 300,   0,   0, 150, 150,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_012",      new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_003",      new int[20] { 300, 300,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_001",      new int[20] { 400, 400,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_018",      new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_017",      new int[20] { 300, 300,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("MP_011",      new int[20] { 400, 400,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP1_001",     new int[20] {   0,   0, 400, 350,   0,   0, 300, 250, 250, 200,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP1_002",     new int[20] { 350, 380,   0,   0, 300, 330, 230, 200,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP1_003",     new int[20] {   0,   0, 300, 350,   0,   0, 200, 250, 200, 220, 100, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP1_004",     new int[20] {   0,   0, 400, 300,   0,   0, 300, 220, 400, 300,  75, 100,  50,  20, 100,   0,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP2_Factory", new int[20] {   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  50,   0, 100, 150,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP2_Office",  new int[20] {   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  50,   0, 100, 100,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP2_Palace",  new int[20] {   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  50,   0, 100, 150,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP2_Skybar",  new int[20] {   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  50,   0, 100, 150,   0,   0,   0,   0});
                this.m_DMapTickets.Add("XP3_Alborz",  new int[20] { 250, 250,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0, 200,   0,   0,   0});
                this.m_DMapTickets.Add("XP3_Shield",  new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0, 200,   0,   0,   0});
                this.m_DMapTickets.Add("XP3_Desert",  new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0, 200,   0,   0,   0});
                this.m_DMapTickets.Add("XP3_Valley",  new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0, 200,   0,   0,   0});
                this.m_DMapTickets.Add("XP4_Quake",   new int[20] { 400, 400,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0, 300,   0,   0});
                this.m_DMapTickets.Add("XP4_FD",      new int[20] { 400, 400,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0, 300,   0,   0});
                this.m_DMapTickets.Add("XP4_Parl",    new int[20] { 400, 400,   0,   0, 250, 250,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0, 300,   0,   0});
                this.m_DMapTickets.Add("XP4_Rubble",  new int[20] {   0,   0, 350, 400,   0,   0, 250, 300,   0,   0,  75, 100,  50,  20, 100,   0,   0, 300,   0,   0});
                this.m_DMapTickets.Add("XP5_001",     new int[20] { 200, 200,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0, 250,   3});
                this.m_DMapTickets.Add("XP5_002",     new int[20] { 200, 200,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0, 250,   3});
                this.m_DMapTickets.Add("XP5_003",     new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0, 250,   3});
                this.m_DMapTickets.Add("XP5_004",     new int[20] { 300, 300,   0,   0, 200, 200,   0,   0,   0,   0,  75, 100,  50,  20, 100,   0,   0,   0, 250,   3});

                this.m_DEnabledGM.Add("ConquestLarge0", "Use default ticket count");
                this.m_DEnabledGM.Add("ConquestAssaultLarge0", "Use default ticket count");
                this.m_DEnabledGM.Add("ConquestSmall0", "Use default ticket count");
                this.m_DEnabledGM.Add("ConquestAssaultSmall0", "Use default ticket count");
                this.m_DEnabledGM.Add("ConquestAssaultSmall1", "Use default ticket count");
                this.m_DEnabledGM.Add("RushLarge0", "No");
                this.m_DEnabledGM.Add("SquadRush0", "No");
                this.m_DEnabledGM.Add("SquadDeathMatch0", "No");
                this.m_DEnabledGM.Add("TeamDeathMatch0", "No");
                this.m_DEnabledGM.Add("TeamDeathMatchC0", "No");
                this.m_DEnabledGM.Add("Domination0", "No");
                this.m_DEnabledGM.Add("TankSuperiority0", "No");
                this.m_DEnabledGM.Add("Scavenger0", "No");
                this.m_DEnabledGM.Add("AirSuperiority0", "No");
                this.m_DEnabledGM.Add("CaptureTheFlag0", "No");

                this.m_DShortGM.Add("ConquestLarge0", "C64");
                this.m_DShortGM.Add("ConquestAssaultLarge0", "A64");
                this.m_DShortGM.Add("ConquestSmall0", "CQS");
                this.m_DShortGM.Add("ConquestAssaultSmall0", "AS0");
                this.m_DShortGM.Add("ConquestAssaultSmall1", "AS1");
                this.m_DShortGM.Add("RushLarge0", "RSH");
                this.m_DShortGM.Add("SquadRush0", "SQR");
                this.m_DShortGM.Add("SquadDeathMatch0", "SDM");
                this.m_DShortGM.Add("TeamDeathMatch0", "TDM");
                this.m_DShortGM.Add("TeamDeathMatchC0", "TDC");
                this.m_DShortGM.Add("Domination0", "DOM");
                this.m_DShortGM.Add("TankSuperiority0", "TNK");
                this.m_DShortGM.Add("Scavenger0", "SCA");
                this.m_DShortGM.Add("AirSuperiority0", "AIR");
                this.m_DShortGM.Add("CaptureTheFlag0", "CTF");

                this.m_DLongGM.Add("ConquestLarge0", "Conquest Large");
                this.m_DLongGM.Add("ConquestAssaultLarge0", "Assault64");
                this.m_DLongGM.Add("ConquestSmall0", "Conquest Small");
                this.m_DLongGM.Add("ConquestAssaultSmall0", "Assault");
                this.m_DLongGM.Add("ConquestAssaultSmall1", "Assault #2");
                this.m_DLongGM.Add("RushLarge0", "Rush");
                this.m_DLongGM.Add("SquadRush0", "Squad Rush");
                this.m_DLongGM.Add("SquadDeathMatch0", "Squad Death Match");
                this.m_DLongGM.Add("TeamDeathMatch0", "Team Death Match");
                this.m_DLongGM.Add("TeamDeathMatchC0", "TDM Closed Quarters");
                this.m_DLongGM.Add("Domination0", "Domination");
                this.m_DLongGM.Add("TankSuperiority0", "Tank Superiority");
                this.m_DLongGM.Add("Scavenger0", "Scavenger");
                this.m_DLongGM.Add("AirSuperiority0", "Air Superiority");
                this.m_DLongGM.Add("CaptureTheFlag0", "Capture The Flag");
            }
            else if (this.m_strServerGameType == "bf4")
            {
                this.m_DPlayerTickets.Add("default",               new int[68] {400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, -1, -1, 400});
                this.m_DPlayerTickets.Add("ConquestLarge0",        new int[68] {800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1050, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1350, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 0, 1, 800});
                this.m_DPlayerTickets.Add("ConquestSmall0",        new int[68] {400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 525, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 675, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 2, 3, 400});
                this.m_DPlayerTickets.Add("RushLarge0",            new int[68] {75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 125, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 4, 4, 75});
                this.m_DPlayerTickets.Add("TeamDeathMatch0",       new int[68] {100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 5, 5, 100});
                this.m_DPlayerTickets.Add("SquadDeathMatch0",      new int[68] {50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 105, 6, 6, 50});
                this.m_DPlayerTickets.Add("Domination0",           new int[68] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 400, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 600, 7, 7, 300});
                //this.m_DPlayerTickets.Add("Elimination0",          new int[68] {2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 8, 8, 2});
                //this.m_DPlayerTickets.Add("Obliteration",          new int[68] {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 9, 9, 3});
                this.m_DPlayerTickets.Add("AirSuperiority0",       new int[68] {100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 10, 10, 100});

                // Values:                                         |   CQL   |   CQS   |RUSH| TDM|SQDM| DOM| ELI| OBL| AIR|
                // Indices:                                        |  0    1 |  2    3 |  4 |  5 |  6 |  7 |  8 |  9 |  10|
                this.m_DMapTickets.Add("default",      new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Abandoned", new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Damage",    new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Flooded",   new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Journey",   new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Naval",     new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Prison",    new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Resort",    new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_Seige",     new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("MP_TheDish",   new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3,   0});
                this.m_DMapTickets.Add("XP1_001",      new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3, 100});
                this.m_DMapTickets.Add("XP1_002",      new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3, 100});
                this.m_DMapTickets.Add("XP1_003",      new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3, 100});
                this.m_DMapTickets.Add("XP1_004",      new int[11] { 800, 800, 400, 400,  75, 100,  50, 300,   2,   3, 100});

                this.m_DEnabledGM.Add("ConquestLarge0", "Use default ticket count");
                this.m_DEnabledGM.Add("ConquestSmall0", "Use default ticket count");
                this.m_DEnabledGM.Add("RushLarge0", "No");
                this.m_DEnabledGM.Add("SquadDeathMatch0", "No");
                this.m_DEnabledGM.Add("TeamDeathMatch0", "No");
                this.m_DEnabledGM.Add("Domination0", "No");
                //this.m_DEnabledGM.Add("Elimination0", "No");
                //this.m_DEnabledGM.Add("Obliteration", "No");
                this.m_DEnabledGM.Add("AirSuperiority0", "No");

                this.m_DShortGM.Add("ConquestLarge0", "C64");
                this.m_DShortGM.Add("ConquestSmall0", "CQS");
                this.m_DShortGM.Add("RushLarge0", "RSH");
                this.m_DShortGM.Add("SquadDeathMatch0", "SDM");
                this.m_DShortGM.Add("TeamDeathMatch0", "TDM");
                this.m_DShortGM.Add("Domination0", "DOM");
                //this.m_DShortGM.Add("Elimination0", "ELI");
                //this.m_DShortGM.Add("Obliteration", "OBL");
                this.m_DShortGM.Add("AirSuperiority0", "AIR");

                this.m_DLongGM.Add("ConquestLarge0", "Conquest Large");
                this.m_DLongGM.Add("ConquestSmall0", "Conquest Small");
                this.m_DLongGM.Add("RushLarge0", "Rush");
                this.m_DLongGM.Add("SquadDeathMatch0", "Squad Death Match");
                this.m_DLongGM.Add("TeamDeathMatch0", "Team Death Match");
                this.m_DLongGM.Add("Domination0", "Domination");
                //this.m_DLongGM.Add("Elimination0", "Elimination");
                //this.m_DLongGM.Add("Obliteration", "Obliteration");
                this.m_DLongGM.Add("AirSuperiority0", "Air Superiority");
            }

            this.RegisterEvents(this.GetType().Name, "OnListPlayers", "OnMaplistList", "OnMaplistGetRounds", "OnMaplistGetMapIndices", "OnRestartLevel", "OnRunNextLevel", "OnPlayerLeft", "OnRoundOverTeamScores", "OnGameModeCounter", "OnLevelLoaded", "OnPlayerSpawned", "OnMaplistNextLevelIndex");
        }

        public void OnPluginEnable() {
            this.m_isPluginEnabled = true;
            this.m_blRoundEnded = false;
            this.m_blRestartRequested = false;
            this.m_blUpdateGameModeCounter = false;
            this.m_blTimerActive = false;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdaptiveTicketCount: ^2Enabled!");
        }

        public void OnPluginDisable() {
            this.m_isPluginEnabled = false;
            this.ExecuteCommand("procon.protected.tasks.remove", "CAdaptiveTicketCount");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdaptiveTicketCount: ^1Disabled =(" );
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option 
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            foreach (KeyValuePair<string, string> gameMode in this.m_DLongGM)
            {
                lstReturn.Add(new CPluginVariable("* Adaptive Ticket Count|Enable for " + gameMode.Value + "?", "enum.DEnabledGM(No|Use default ticket count|Use game-mode-specific ticket count)", this.m_DEnabledGM[gameMode.Key]));
            }
            lstReturn.Add(new CPluginVariable("* Adaptive Ticket Count|    Ticket percentage for disabled game modes", this.m_iOtherTicketCount.GetType(), this.m_iOtherTicketCount));
            lstReturn.Add(new CPluginVariable("* Adaptive Ticket Count|Units used for ticket count", "enum.strTicketCountUnit(Percentage (default)|Actual number of tickets)", this.m_strTicketCountUnit));
            if (this.m_strServerGameType == "bf4") {
                lstReturn.Add(new CPluginVariable("* Adaptive Ticket Count|Set preset after map loads?", "enum.strSetPreset(No|Normal|Hardcore|Infantry)", this.m_strSetPreset));
            }

            for (int i = 0; i <= 64; i++)
            {
                int count = this.m_DPlayerTickets["default"][i];
                if (!this.m_blUnitActualNumber)
                {
                    count = count * 100 / this.m_DPlayerTickets["default"][67];
                }
                lstReturn.Add(new CPluginVariable("* Default Ticket Count|[DEF] Tickets with "+ i +" players", this.m_DPlayerTickets["default"][i].GetType(), count));
            }
            foreach (KeyValuePair<string, string> enabled in this.m_DEnabledGM)
            {
                if (enabled.Value.CompareTo("Use game-mode-specific ticket count") == 0)
                {
                    for (int i = 0; i <= 64; i++)
                    {
                        int count = this.m_DPlayerTickets[enabled.Key][i];
                        if (!this.m_blUnitActualNumber)
                        {
                            count = (int)Math.Ceiling((double)count * 100 / this.m_DPlayerTickets[enabled.Key][67]);
                        }
                        lstReturn.Add(new CPluginVariable("[" + this.m_DShortGM[enabled.Key] + "] " + this.m_DLongGM[enabled.Key] + " Ticket Count|[" + this.m_DShortGM[enabled.Key] + "] Tickets with "+ i +" players", this.m_DPlayerTickets[enabled.Key][i].GetType(), count));
                    }
                }
            }

            lstReturn.Add(new CPluginVariable("Xtras|Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            foreach (KeyValuePair<string, string> gameMode in this.m_DLongGM)
            {
                lstReturn.Add(new CPluginVariable("Enable for " + gameMode.Value + "?", typeof(string), this.m_DEnabledGM[gameMode.Key]));
            }
            lstReturn.Add(new CPluginVariable("    Ticket percentage for disabled game modes", this.m_iOtherTicketCount.GetType(), this.m_iOtherTicketCount));
            lstReturn.Add(new CPluginVariable("Units used for ticket count", "enum.string(Percentage (default)|Actual number of tickets)", this.m_strTicketCountUnit));
            lstReturn.Add(new CPluginVariable("Set preset after map loads?", "enum.string(No|Normal|Hardcore|Infantry)", this.m_strSetPreset));
            for (int i = 0; i <= 64; i++)
            {
                lstReturn.Add(new CPluginVariable("[DEF] Tickets with "+ i +" players", typeof(string), "CONFIG:"+this.m_DPlayerTickets["default"][i]));
            }
            foreach (KeyValuePair<string, string> enabled in this.m_DEnabledGM)
            {
                for (int i = 0; i <= 64; i++)
                {
                    lstReturn.Add(new CPluginVariable("[" + this.m_DShortGM[enabled.Key] + "] Tickets with "+ i +" players", typeof(string), "CONFIG:"+this.m_DPlayerTickets[enabled.Key][i]));
                }
            }
            lstReturn.Add(new CPluginVariable("Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iValue = 0;
            bool loadedFromConfig = false;

            if (strValue.StartsWith("CONFIG:"))
            {
               loadedFromConfig = true; 
               strValue = strValue.Replace("CONFIG:", "");
            }

            if (strVariable.Length > 11 && strVariable.Substring(0, 11).CompareTo("Enable for ") == 0)
            {
                foreach (KeyValuePair<string, string> gameMode in this.m_DLongGM)
                {
                    if (strVariable.Substring(11) == (gameMode.Value + "?"))
                    {
                        this.m_DEnabledGM[gameMode.Key] = strValue;
                    }
                }
            }
            else if (strVariable.CompareTo("    Ticket percentage for disabled game modes") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                if (this.m_strServerGameType == "bf4" && iValue > 400) iValue = 400;
                if (this.m_strServerGameType == "bf4" && iValue < 75) iValue = 75;
                if (iValue < 0)
                {
                    iValue = 0;
                }

                this.m_iOtherTicketCount = iValue;
            }
            else if (strVariable.CompareTo("Units used for ticket count") == 0)
            {
                if (strValue.CompareTo("Actual number of tickets") == 0)
                {
                    this.m_strTicketCountUnit = strValue;
                    this.m_blUnitActualNumber = true;
                }
                else
                {
                    this.m_strTicketCountUnit = "Percentage (default)";
                    this.m_blUnitActualNumber = false;
                }
            }
            else if (strVariable.CompareTo("Set preset after map loads?") == 0)
            {
                this.m_strSetPreset = strValue;
            }
            else if (strVariable.Substring(6,12).CompareTo("Tickets with") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                int i = int.Parse(strVariable.Substring(19,2).Trim());
                string gamemode = "default";

                foreach (KeyValuePair<string, string> gameMode in this.m_DShortGM)
                {
                    if (strVariable.Substring(1,3) == (gameMode.Value))
                    {
                        gamemode = gameMode.Key;
                    }
                }

                if (!this.m_blUnitActualNumber && !loadedFromConfig)
                {
                    if (this.m_strServerGameType == "bf4" && iValue > 400) iValue = 400;
                    if (this.m_strServerGameType == "bf4" && iValue < 75) iValue = 75;
                    this.m_DPlayerTickets[gamemode][i] = iValue * this.m_DPlayerTickets[gamemode][67] / 100;
                }
                else
                {
                    this.m_DPlayerTickets[gamemode][i] = iValue;
                }

                if (iValue < 0)
                {
                    this.m_DPlayerTickets[gamemode][i] = 0;
                }
            }
            /* extras */
            else if (strVariable.CompareTo("Enable debug output?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
        }

        private void UnregisterAllCommands() {
        }

        private void SetupHelpCommands() {
        }

        private void RegisterAllCommands() {
        }

        #endregion
        #region Events

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                if (lstPlayers.Count > 64)
                {
                    this.m_iCurrentPlayerCount = 64;
                }
                else
                {
                    this.m_iCurrentPlayerCount = lstPlayers.Count;
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
            this.m_iCurrentPlayerCount--;
        }

        public override void OnMaplistList(List<MaplistEntry> lstMaplist) {
            if (this.m_blRoundEnded)
            {
                this.m_LMapList = lstMaplist;
            }
        }

        public override void OnMaplistGetRounds(int currentRound, int totalRounds) {
            this.m_blIsLastRound = (currentRound + 1 >= totalRounds);
        }

        public override void OnMaplistGetMapIndices(int mapIndex, int nextIndex) {
            if (this.m_blUpdateGameModeCounter)
            {
                this.m_blUpdateGameModeCounter = false;
                if (this.m_blRestartRequested || !this.m_blIsLastRound)
                {
                    this.m_sNextGameMode = this.m_LMapList[mapIndex].Gamemode;
                    this.m_sNextMapFileName = this.m_LMapList[mapIndex].MapFileName;
                }
                else
                {
                    this.m_sNextGameMode = this.m_LMapList[nextIndex].Gamemode;
                    this.m_sNextMapFileName = this.m_LMapList[nextIndex].MapFileName;
                }
                if (this.m_blRestartRequested)
                {
                    CheckTickets();
                }
                else if (!this.m_blTimerActive)
                {
                    this.m_blTimerActive = true;
                    this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveTicketCount", "55", "1", "1", "procon.protected.plugins.call", "CAdaptiveTicketCount", "CheckTickets");
                }
            }
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores) {
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000;
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = false;
            WritePluginConsole("INFO -> Round ended.");
            GetMapInfo();
        }      

        public override void OnRestartLevel() {
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000;
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = true;
            WritePluginConsole("INFO -> Round restart requested.");
            GetMapInfo();
        }

        public override void OnRunNextLevel() {
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000;
            this.m_blRoundEnded = true;
            this.m_blRestartRequested = true;
            WritePluginConsole("INFO -> Next round requested.");
            GetMapInfo();
        }

        public override void OnMaplistNextLevelIndex(int mapIndex) {
            if (this.m_blRoundEnded)
            {
                WritePluginConsole("INFO -> Next map changed.");
                GetMapInfo();
            }
        }

        public override void OnGameModeCounter(int limit) {
            this.m_iCurrentGameModeCounter = limit;
        }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            WritePluginConsole("INFO -> Level loaded.");
            this.m_blRoundEnded = false;
            if (this.m_strServerGameType == "bf4") {
                this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveTicketCount", "5", "1", "1", "procon.protected.plugins.call", "CAdaptiveTicketCount", "SetPreset");
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) {
            if (this.m_blRoundEnded && DateTime.UtcNow.Ticks/10000000 - this.m_lLastRoundEnded > 60) {
                WritePluginConsole("INFO -> Detected level loaded. (player spawn)");
                this.m_blRoundEnded = false;
            }
        }

        private void GetMapInfo() {
            this.m_isPluginInitialized = true;
            this.m_blUpdateGameModeCounter = true;
            this.ExecuteCommand("procon.protected.send", "vars.gameModeCounter");
            this.ExecuteCommand("procon.protected.send", "mapList.list");
            this.ExecuteCommand("procon.protected.send", "mapList.list", "100");
            this.ExecuteCommand("procon.protected.send", "mapList.getRounds");
            this.ExecuteCommand("procon.protected.send", "mapList.getMapIndices");
        }

        public void SetPreset() {
            if (this.m_strSetPreset != "No") {
                WritePluginConsole("WORK -> Server preset set to " + this.m_strSetPreset + ".");
                this.ExecuteCommand("procon.protected.send", "vars.preset", this.m_strSetPreset);
            }
        }

        public void CheckTickets() {
            if (this.m_isPluginInitialized)
            {
                string settickets = "100";
                this.m_blTimerActive = false;
                if (!this.m_DEnabledGM.ContainsKey(m_sNextGameMode)) {
                    WritePluginConsole("WORK -> Game mode not supported; Ticket count: 100%");
                }
                else if (this.m_DEnabledGM[this.m_sNextGameMode].CompareTo("No") != 0)
                {
                    int count = 100;
                    int tickets = -1;
                    string ticketcount = "";
                    string gamemode = "default";
                    string mapfilename = "default";

                    if (this.m_DEnabledGM[this.m_sNextGameMode].CompareTo("Use game-mode-specific ticket count") == 0)
                    {
                        gamemode = this.m_sNextGameMode;
                    }

                    int[] index = new int[2] { this.m_DPlayerTickets[this.m_sNextGameMode][65], this.m_DPlayerTickets[this.m_sNextGameMode][66] };
                    
                    if (this.m_DMapTickets.ContainsKey(this.m_sNextMapFileName)) {
                        mapfilename = this.m_sNextMapFileName;
                    }
                    if (index[0] != index[1])
                    {
                        if (this.m_DMapTickets[mapfilename][index[0]] != 0) {
                            tickets = Math.Max(this.m_DMapTickets[mapfilename][index[0]], this.m_DMapTickets[mapfilename][index[1]]);
                            if (this.m_blUnitActualNumber)
                                count = (int)Math.Ceiling((double)this.m_DPlayerTickets[gamemode][this.m_iCurrentPlayerCount] * 100 / tickets);
                            else
                                count = (int)Math.Ceiling((double)this.m_DPlayerTickets[gamemode][this.m_iCurrentPlayerCount] * 100 / this.m_DPlayerTickets[gamemode][67]);
                            if (this.m_strServerGameType == "bf4" && count > 400) count = 400;
                            if (this.m_strServerGameType == "bf4" && count < 75) count = 75;
                            ticketcount = " (" + (count * this.m_DMapTickets[mapfilename][index[0]] / 100) + "/" + (count * this.m_DMapTickets[mapfilename][index[1]] / 100) + " actual tickets)";
                        }
                    }
                    else
                    {
                        if (this.m_DMapTickets[mapfilename][index[0]] != 0) {
                            tickets = this.m_DMapTickets[mapfilename][index[0]];
                            if (this.m_blUnitActualNumber)
                                count = (int)Math.Ceiling((double)this.m_DPlayerTickets[gamemode][this.m_iCurrentPlayerCount] * 100 / tickets);
                            else
                                count = (int)Math.Ceiling((double)this.m_DPlayerTickets[gamemode][this.m_iCurrentPlayerCount] * 100 / this.m_DPlayerTickets[gamemode][67]);
                            if (this.m_strServerGameType == "bf4" && count > 400) count = 400;
                            if (this.m_strServerGameType == "bf4" && count < 75) count = 75;
                            ticketcount = " (" + (count * tickets / 100) + " actual tickets)";
                        }
                    }
                    if (this.m_blUnitActualNumber && tickets == -1)
                    {
                        WritePluginConsole("INFO -> Default ticket counts unknown for this map; using 100% ticket count.");
                    }
                    WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players; Ticket count: " + count + "%" + ticketcount);
                    settickets = count.ToString();
                }
                else
                {
                    WritePluginConsole("WORK -> Not enabled for this game mode; Ticket count: " + this.m_iOtherTicketCount + "%");
                    settickets = this.m_iOtherTicketCount.ToString();
                }
                //if (this.m_iCurrentGameModeCounter.ToString() != settickets)
                    this.ExecuteCommand("procon.protected.send", "vars.gameModeCounter", settickets);
                //}
            }
        }

        #endregion


#region helper_functions

        private void WritePluginConsole(string message)
        {
            string line = String.Format("AdaptiveTicketCount: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes) { 
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
