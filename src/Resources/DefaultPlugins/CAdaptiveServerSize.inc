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
    public class CAdaptiveServerSize : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private enumBoolYesNo m_enServerSize;
        private enumBoolYesNo m_enAssumeMax;
        private string m_strRestartOption;
        private int m_iWaitingPlayersNeeded;
        private int m_iRestartLimit;
        private int m_iMaxServerSize;
        private int[] m_iPlayerServerSize;
        private int m_iServerSizeAtStart;
        private int m_iRestartOption;
        private int m_iCurrentServerSize;
        private int m_iCurrentPlayerCount;
        private bool m_blRoundEnded;
        private bool m_blNextRoundStarted;
        private string m_strCurrentGameMode;
        private long m_lLastLevelLoaded;
        private long m_lLastRoundEnded;
        private Dictionary<string, int> m_DGameModeMax;
        private Dictionary<string, string> m_DGameModePublic;
        private Dictionary<string, long> m_DPlayerJoined;
        private int m_iCurrentGameModeMax;
        private int m_iDesiredServerSize;
        private int m_iCurrentRoundTime;
        private long m_lLastMessage;
        private List<string> m_LStartMessageShown;
        private bool m_blRestartRequested;

        private enumBoolYesNo m_enShowMessages;
        private int m_iMessageMaxUsers;
        private string m_sWelcome1;
        private string m_sWelcome2;
        private string m_sOnJoin1;
        private string m_sOnJoin2;

        private enumBoolYesNo m_enIdleKick;
        private int m_iDisableIdleKickUntil;
        private int m_iDesiredIdleKick;
        private int m_iCurrentIdleKick;

        private enumBoolYesNo m_enQuickMatch;
        private int m_iDefaultRoundStart;
        private int m_iDefaultRoundRestart;
        private int m_iQuickMatchApplied;

        private enumBoolYesNo m_enDoDebugOutput;

        private bool m_isPluginEnabled;
        private bool m_isPluginInitialized;
        private string m_strServerGameType;
        private string m_strGameMod;
        private string m_strServerVersion;

        public CAdaptiveServerSize() {

            this.m_enServerSize            = enumBoolYesNo.No;
            this.m_enAssumeMax             = enumBoolYesNo.No;
            this.m_iMaxServerSize          = 64;
            this.m_iPlayerServerSize       = new int[65] {8, 8, 8, 8, 8, 8, 8, 16, 16, 16, 16, 16, 16, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64};
            this.m_iServerSizeAtStart      = 0;
            this.m_iCurrentServerSize      = 0;
            this.m_iCurrentPlayerCount     = 0;
            this.m_blRoundEnded            = false;
            this.m_blNextRoundStarted      = false;
            this.m_strCurrentGameMode      = "RushLarge0";
            this.m_lLastLevelLoaded        = DateTime.UtcNow.Ticks/10000000;
            this.m_lLastRoundEnded         = DateTime.UtcNow.Ticks/10000000;
            this.m_iCurrentGameModeMax     = 64;
            this.m_iDesiredServerSize      = 64;
            this.m_iCurrentRoundTime       = -1;
            this.m_lLastMessage            = DateTime.UtcNow.Ticks/10000000;
            this.m_blRestartRequested      = false;

            this.m_DGameModeMax            = new Dictionary<string, int>();
            this.m_DGameModePublic         = new Dictionary<string, string>();
            this.m_DPlayerJoined           = new Dictionary<string, long>();
            this.m_LStartMessageShown      = new List<string>();

            this.m_enShowMessages          = enumBoolYesNo.No;
            this.m_iMessageMaxUsers        = 3;
            this.m_sWelcome1               = "Welcome [soldierName]! Please stick around to help us get our server going!";
            this.m_sWelcome2               = "We will notify you when new players are joining!";
            this.m_sOnJoin1                = "Please stand by; [joinCount] more player(s) are joining.";
            this.m_sOnJoin2                = "Keep in mind that it may take over a minute for their game(s) to load.";

            this.m_enIdleKick              = enumBoolYesNo.No;
            this.m_iDisableIdleKickUntil   = 8;
            this.m_iDesiredIdleKick        = 300;
            this.m_iCurrentIdleKick        = -1;

            this.m_enQuickMatch            = enumBoolYesNo.No;
            this.m_iDefaultRoundStart      = 4;
            this.m_iDefaultRoundRestart    = 2;
            this.m_iQuickMatchApplied      = -1;

            this.m_enDoDebugOutput         = enumBoolYesNo.No;

            this.m_isPluginEnabled         = false;
            this.m_isPluginInitialized     = false;
            this.m_strServerGameType       = "none";
        }

        public string GetPluginName() {
            return "Adaptive Server Size";
        }

        public string GetPluginVersion() {
            return "1.3.5.3";
        }

        public string GetPluginAuthor() {
            return "falcontx";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net/forumvb/showthread.php?2939";
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
    <input type=""hidden"" name=""item_name"" value=""Support Free Plugin Development (Adaptive Server Size)"">
    <input type=""hidden"" name=""currency_code"" value=""USD"">
    <input type=""hidden"" name=""bn"" value=""PP-DonationsBF:btn_donate_LG.gif:NonHosted"">
    <input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!""><br>
    <br>
    <img alt="""" border=""0"" src=""https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg""
    </form>
    </td>
    </tr>
    </table>


<h2>Description</h2>
    <p>This plug-in is intended to change the server size adaptively based upon the number of players currently on the server. The player count and desired server size is reevaluated when a player joins or leaves and changes occur dynamically throughout each round. When calculating the current number of players, the plug-in includes players who are joining the game, but are not in the PRoCon player list, in order to attempt to mimic the number displayed on Battlelog. Due to the fact that player timeouts aren't reported to PRoCon, it's not perfect, but it's accurate 80-90% of the time.</p>

    <p>Normally, if the server size is modified to a value greater than what it was when the round began, players exceeding the previous size are required to wait at a black screen until the round ends. This plugin actively works around this issue by setting the server to it's max size before each round begins.</p>

    <p>Optionally, the server can display welcome messages to new players when only a limited number of players are on the server in an effort to convince them to stick around and start the server. It will also notify all players on the server when new players join so that they will hopefully stick around if they see people joining.</p>

    <p>An adaptive idle kick feature is also available, which will disable the idle kick when less than a specified number of players online.</p>
<h2>Commands</h2>
    <p>This plug-in has no in-game commands.</p>

<h2>Settings</h2>
    <br><h3>Adaptive Server Size</h3>
        <blockquote><h4>Enable adaptive server size?</h4> If enabled, changes the server size based upon the current number of players.</blockquote>
        <blockquote><h4>On startup, assume round started at max server size?</h4> When the plugin initially starts up, it has no way of knowing if the current round started at the maximum server size, or not. By default, if the server is empty, it will restart the round at the maximum server size, but if players are present, it will assume that the round started with the current server size, and will not increase the server size until the current round ends. If this option is enabled, when the plugin starts up, it will assume that the current round started at the maximum server size and make immediate server size changes, as necessary.</blockquote>
        <blockquote><h4>Display welcome/join messages to help start server?</h4> If enabled, the server will display welcome messages to new players when only a limited number of players are on the server to encourage them to stick around and help start the server. It will also notify all players on the server when new players join so that they will hopefully stay if they see people joining.</blockquote>
        <blockquote><h4>Display messages until how many players are on?</h4> The welcome/join messages will only be displayed until this number of players have joined. It should probably be set to somewhere between 3 and 5 players, as once 4 to 6 players are on your server, it should begin to fill up on it's own.</blockquote>
        <blockquote><h4>Welcome messages</h4> These messages are sent to new players as soon as they reach the deploy screen. The second message is sent 5 seconds after the first. <b>[soldierName]</b> is replaced by the new player's name.</blockquote>
        <blockquote><h4>Player joining messages</h4> These messages are sent to all players currently on the server when new player(s) are joining. The second message is sent 5 seconds after the first. <b>[joinCount]</b> is replaced by the number of players who are currently joining.</blockquote>
        <blockquote><h4>Maximum server size</h4> The maximum number of slots that the server is allowed to have.</blockquote>
        <blockquote><h4>Size with [#] players</h4> The server size that will be set when X players are active on the server.</blockquote>
        <blockquote><h4>Maximum [Game Mode] size</h4> The maximum server size that will be set when the specified game mode is active</blockquote>
    <br><h3>Adaptive Idle Kick</h3>
        <blockquote><h4>Enable adaptive idle kick?</h4> If enabled, disables the idle kick time when less than the specified number of players are online.</blockquote>
        <blockquote><h4>Disable idle kick until how many players are on?</h4> Once this number of players is reached, idle kick will be enabled and set to the specified value.</blockquote>
        <blockquote><h4>Desired idle kick time, in seconds</h4> The number of seconds that the idle kick timer will be set to, once it has been enabled.</blockquote>
    <br><h3>Extras</h3>
        <blockquote><h4>Enable debug output?</h4> If enabled, displays debug info in the console window.</blockquote>

<br><h2>Development</h2>
    <br><h3>Changelog</h3>
        <blockquote><h4>1.3.5.3 (06/14/2015)</h4>
            - improved compatibility with BFHL
        </blockquote>
        <blockquote><h4>1.3.5.2 (06/13/2015)</h4>
            - improved compatibility with BFHL
        </blockquote>
        <blockquote><h4>1.3.5.1 (03/30/2015)</h4>
            - idle kick disable sets idle kick to 86400 for BFHL since 0 isn't valid
        </blockquote>
        <blockquote><h4>1.3.5.0 (03/26/2015)</h4>
            - added support for BFHL
        </blockquote>
        <blockquote><h4>1.3.4.6 (02/05/2014)</h4>
            - idle kick disable sets idle kick to 86400 for BF4 since 0 isn't valid
        </blockquote>
        <blockquote><h4>1.3.4.5 (11/26/2013)</h4>
            - changed max players back to 64 for BF4
        </blockquote>
        <blockquote><h4>1.3.4.4 (11/05/2013)</h4>
            - added game detection and support for up to 70 players for BF4
        </blockquote>
        <blockquote><h4>1.3.4.1 (10/31/2013)</h4>
            - added include file in BF4 folder
        </blockquote>
        <blockquote><h4>1.3.4.0 (06/14/2012)</h4>
            - game modes now pulled from BF3.def to ensure compatibility with new and all future game modes<br/>
            - welcome message now sent only to new player instead of team<br/>
            - player joining messages now limited to once every 15-30 seconds in order to prevent spam<br/>
        </blockquote>
        <blockquote><h4>1.3.2.7 (03/30/2012)</h4>
            - more minor compatibility changes due to PRoCon/R-20 updates<br/>
        </blockquote>
        <blockquote><h4>1.3.2.6 (03/29/2012)</h4>
            - minor compatibility changes due to upcoming PRoCon/R-20 updates<br/>
        </blockquote>
        <blockquote><h4>1.3.2.5 (01/05/2012)</h4>
            - fixed wrong number of players detected when listplayers not called with 'all'<br/>
            - welcome/join messages will be skipped if they are left empty<br/>
        </blockquote>
        <blockquote><h4>1.3.2.3 (12/31/2011)</h4>
            - added code to prevent restart if certain variables are not initialized for some reason<br/>
        </blockquote>
        <blockquote><h4>1.3.2.2 (12/28/2011)</h4>
            - fixed server resize not working immediately after server restart in some cases<br/>
        </blockquote>
        <blockquote><h4>1.3.2.1 (12/22/2011)</h4>
            - fixed idle kick timeout was not being reset after server restart<br/>
        </blockquote>
        <blockquote><h4>1.3.2.0 (12/19/2011)</h4>
            - updated to properly override API methods<br/>
            - changed friendly names for map sizes to match PRoCon 1.1.3.1<br/>
            - added Adaptive Idle Kick (commissioned by Guy0510)<br/>
            - raised the max player limit for displaying welcome messages (previously 8)<br/>
        </blockquote>
        <blockquote><h4>1.3.1.2 (12/13/2011)</h4>
            - added maximum size for Conquest Small Alt maps<br/>
        </blockquote>
        <blockquote><h4>1.3.1.1 (12/12/2011)</h4>
            - added round detection timer in case OnLevelLoaded event isn't triggered<br/>
        </blockquote>
        <blockquote><h4>1.3.1.0 (12/12/2011)</h4>
            - changed timer method to prevent sleep lock<br/>
            - fixed round start detection error caused by spawn after round end<br/>
        </blockquote>
        <blockquote><h4>1.3.0.1 (12/10/2011)</h4>
            - fixed unable to change number of players for welcome message display<br/>
        </blockquote>
        <blockquote><h4>1.3.0.0 (12/09/2011)</h4>
            - added optional messages to encourage players to stay when server is just getting started<br/>
            - removed QuickMatch options, since it's been documented that setting the variables to 8/4 is not required<br/>
            - minimum server size no longer restricted<br/>
            - fixed multiple restarts after server stops responding for a minute at the end of a round<br/>
        </blockquote>
        <blockquote><h4>1.2.3.1 (12/06/2011)</h4>
            - adjusted allowable option values due to R11 patch<br/>
        </blockquote>
        <blockquote><h4>1.2.3.0 (12/05/2011)</h4>
            - added additional checks on player join/leave in order to improve accuracy<br/>
            - added PayPal donation option<br/>
        </blockquote>
        <blockquote><h4>1.2.2.0 (11/26/2011)</h4>
            - now includes joining players when calculating server size, in order to match Battlelog (most of the time)<br/>
        </blockquote>
        <blockquote><h4>1.2.1.0 (11/22/2011)</h4>
            - fixed limits for various game modes not being enforced<br/>
            - adjusted allowable option values due to R9 patch<br/>
        </blockquote>
        <blockquote><h4>1.2.0.1 (11/22/2011)</h4>
            - fixed inability to change max team deathmatch size<br/>
        </blockquote>
        <blockquote><h4>1.2.0.0 (11/21/2011)</h4>
            - server size not initialized properly when ""On startup, assume round started at max server size?"" is set<br/>
            - set server size to round start size when plugin is disabled<br/>
            - fixed bug caused by serverInfo occasionally providing incorrect server size<br/>
            - added max player options for various game modes to prevent increases beyond what's desired<br/>
            - other minor fixes<br/>
        </blockquote>
        <blockquote><h4>1.1.0.0 (11/20/2011)</h4>
            - max server size is set before each level is loaded, in order to provide dynamic changes without restarting<br/>
            - restart options removed, since they are no longer necessary<br/>
            - size for x players options are now saved and restored properly<br/>
        </blockquote>
        <blockquote><h4>1.0.1.0 (11/19/2011)</h4>
            - server size was not being adjusted in some instances<br/>
            - minor fixes<br/>
        </blockquote>
        <blockquote><h4>1.0.0.0 (11/19/2011)</h4>
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

            if (this.m_strServerGameType == "bf4")
            {
                this.m_iPlayerServerSize = new int[65] {16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 24, 24, 24, 24, 32, 32, 32, 32, 32, 32, 32, 32, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64};
            }
            foreach (string strGameModePublic in this.GetMapList("{GameMode}").ToArray())
            {
                string strGameMode = this.GetMapByFormattedName("{GameMode}", strGameModePublic).PlayList; // get system gamemode from public game mode
                int size = 64;
                if (strGameMode.CompareTo("RushLarge0") == 0 || strGameMode.CompareTo("ConquestSmall0") == 0 || strGameMode.CompareTo("ConquestAssaultSmall0") == 0 || strGameMode.CompareTo("ConquestAssaultSmall1") == 0 || strGameMode.CompareTo("Obliteration0") == 0 || strGameMode.CompareTo("TurfWarSmall0") == 0 || strGameMode.CompareTo("Heist0") == 0 || strGameMode.CompareTo("Hotwire0") == 0 || strGameMode.CompareTo("BloodMoney0") == 0)
                {
                    size = 32;
                }
                else if (strGameMode.CompareTo("TeamDeathMatch0") == 0)
                {
                    size = 24;
                }
                else if (strGameMode.CompareTo("SquadDeathMatch0") == 0 || strGameMode.CompareTo("TeamDeathMatchC0") == 0 || strGameMode.CompareTo("Domination0") == 0 || strGameMode.CompareTo("GunMaster0") == 0)
                {
                    size = 16;
                }
                else if (strGameMode.CompareTo("Defuse0") == 0 || strGameMode.CompareTo("Hostage0") == 0 || strGameMode.CompareTo("Hit0") == 0)
                {
                    size = 10;
                }
                else if (strGameMode.CompareTo("SquadRush0") == 0)
                {
                    size = 8;
                }
                this.m_DGameModeMax.Add(strGameMode, size);
                this.m_DGameModePublic.Add(strGameMode, strGameModePublic);
            }
      
            this.RegisterEvents(this.GetType().Name, "OnLogin", "OnServerInfo", "OnRoundOverTeamScores", "OnLevelLoaded", "OnRestartLevel", "OnRunNextLevel", "OnPlayerLimit", "OnListPlayers", "OnPlayerJoin", "OnPlayerSpawned", "OnPlayerLeft", "OnPlayerTeamChange");
        }

        public void OnPluginEnable() {
            this.m_isPluginEnabled = true;
            ResetVars();
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdaptiveServerSize: ^2Enabled!");
            this.ExecuteCommand("procon.protected.send", "vars.maxPlayers");
        }

        public void OnPluginDisable() {
            this.m_isPluginEnabled = false;
            //this.ExecuteCommand("procon.protected.send", "vars.maxPlayers", this.m_iServerSizeAtStart.ToString());
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdaptiveServerSize: ^1Disabled =(" );
        }

        private void ResetVars() {
            this.m_isPluginInitialized = false;
            this.m_iServerSizeAtStart = 0;
            this.m_iCurrentRoundTime = -1;
            this.m_iQuickMatchApplied = -1;
            this.m_blRoundEnded = false;
            this.m_blNextRoundStarted = false;
            this.m_DPlayerJoined.Clear();
            this.m_LStartMessageShown = new List<string>();
            this.m_iCurrentIdleKick = -1;
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option 
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Adaptive Server Size|Enable adaptive server size?", typeof(enumBoolYesNo), this.m_enServerSize));
            if (this.m_enServerSize == enumBoolYesNo.Yes)
            { 
                if (this.m_strServerGameType != "bfhl") {
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|On startup, assume round started at max server size?", typeof(enumBoolYesNo), this.m_enAssumeMax));
                }
                lstReturn.Add(new CPluginVariable("Adaptive Server Size|Display welcome/join messages to help start server?", typeof(enumBoolYesNo), this.m_enShowMessages));
                if (this.m_enShowMessages == enumBoolYesNo.Yes)
                { 
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Display messages until how many players are on?", this.m_iMessageMaxUsers.GetType(), this.m_iMessageMaxUsers));
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Welcome message 1", this.m_sWelcome1.GetType(), this.m_sWelcome1));
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Welcome message 2", this.m_sWelcome2.GetType(), this.m_sWelcome2));
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Player joining message 1", this.m_sOnJoin1.GetType(), this.m_sOnJoin1));
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Player joining message 2", this.m_sOnJoin2.GetType(), this.m_sOnJoin2));
                }
                lstReturn.Add(new CPluginVariable("Adaptive Server Size|Maximum server size", this.m_iMaxServerSize.GetType(), this.m_iMaxServerSize));
                for (int i = 0; i <= this.m_iMaxServerSize; i++)
                {
                    if (this.m_iPlayerServerSize[i] > this.m_iMaxServerSize)
                    {
                        this.m_iPlayerServerSize[i] = this.m_iMaxServerSize;
                    }
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|    Size with "+ i +" players", this.m_iPlayerServerSize[i].GetType(), this.m_iPlayerServerSize[i]));
                }
                foreach (KeyValuePair<string, string> gameMode in this.m_DGameModePublic)
                {
                    if (this.m_DGameModeMax[gameMode.Key] > this.m_iMaxServerSize)
                    {
                        this.m_DGameModeMax[gameMode.Key] = this.m_iMaxServerSize;
                    }
                    lstReturn.Add(new CPluginVariable("Adaptive Server Size|Maximum " + gameMode.Value + " size", this.m_DGameModeMax[gameMode.Key].GetType(), this.m_DGameModeMax[gameMode.Key]));
                }
            }
            lstReturn.Add(new CPluginVariable("Adaptive Idle Kick|Enable adaptive idle kick?", typeof(enumBoolYesNo), this.m_enIdleKick));
            if (this.m_enIdleKick == enumBoolYesNo.Yes)
            { 
                lstReturn.Add(new CPluginVariable("Adaptive Idle Kick|Disable idle kick until how many players are on?", this.m_iDisableIdleKickUntil.GetType(), this.m_iDisableIdleKickUntil));
                lstReturn.Add(new CPluginVariable("Adaptive Idle Kick|Desired idle kick time, in seconds", this.m_iDesiredIdleKick.GetType(), this.m_iDesiredIdleKick));
            }
            lstReturn.Add(new CPluginVariable("Xtras|Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Enable adaptive server size?", typeof(enumBoolYesNo), this.m_enServerSize));
            lstReturn.Add(new CPluginVariable("On startup, assume round started at max server size?", typeof(enumBoolYesNo), this.m_enAssumeMax));
            lstReturn.Add(new CPluginVariable("Display welcome/join messages to help start server?", typeof(enumBoolYesNo), this.m_enShowMessages));
            lstReturn.Add(new CPluginVariable("    Display messages until how many players are on?", this.m_iMessageMaxUsers.GetType(), this.m_iMessageMaxUsers));
            lstReturn.Add(new CPluginVariable("    Welcome message 1", this.m_sWelcome1.GetType(), this.m_sWelcome1));
            lstReturn.Add(new CPluginVariable("    Welcome message 2", this.m_sWelcome2.GetType(), this.m_sWelcome2));
            lstReturn.Add(new CPluginVariable("    Player joining message 1", this.m_sOnJoin1.GetType(), this.m_sOnJoin1));
            lstReturn.Add(new CPluginVariable("    Player joining message 2", this.m_sOnJoin2.GetType(), this.m_sOnJoin2));
            lstReturn.Add(new CPluginVariable("Maximum server size", this.m_iMaxServerSize.GetType(), this.m_iMaxServerSize));
            foreach (KeyValuePair<string, string> gameMode in this.m_DGameModePublic)
            {
                lstReturn.Add(new CPluginVariable("Maximum " + gameMode.Value + " size", this.m_DGameModeMax[gameMode.Key].GetType(), this.m_DGameModeMax[gameMode.Key]));
            }
            for (int i = 0; i <= this.m_iMaxServerSize; i++)
            {
                lstReturn.Add(new CPluginVariable("    Size with "+ i +" players", this.m_iPlayerServerSize[i].GetType(), this.m_iPlayerServerSize[i]));
            }
            lstReturn.Add(new CPluginVariable("Enable adaptive idle kick?", typeof(enumBoolYesNo), this.m_enIdleKick));
            lstReturn.Add(new CPluginVariable("Disable idle kick until how many players are on?", this.m_iDisableIdleKickUntil.GetType(), this.m_iDisableIdleKickUntil));
            lstReturn.Add(new CPluginVariable("Desired idle kick time, in seconds", this.m_iDesiredIdleKick.GetType(), this.m_iDesiredIdleKick));
            lstReturn.Add(new CPluginVariable("Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iValue = 0;

            if (strVariable.CompareTo("Enable adaptive server size?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enServerSize = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("On startup, assume round started at max server size?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enAssumeMax = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Display welcome/join messages to help start server?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enShowMessages = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("    Display messages until how many players are on?") == 0 && int.TryParse(strValue, out iValue) == true)
			{
				if (iValue < 1)
				{
					iValue = 1;
				}
                else if (iValue > 64)
                {
                    iValue = 64;
                }
				this.m_iMessageMaxUsers = iValue;
			}
            else if (strVariable.CompareTo("    Welcome message 1") == 0)
            {
                this.m_sWelcome1 = strValue;
            }
            else if (strVariable.CompareTo("    Welcome message 2") == 0)
            {
                this.m_sWelcome2 = strValue;
            }
            else if (strVariable.CompareTo("    Player joining message 1") == 0)
            {
                this.m_sOnJoin1 = strValue;
            }
            else if (strVariable.CompareTo("    Player joining message 2") == 0)
            {
                this.m_sOnJoin2 = strValue;
            }

            else if (strVariable.CompareTo("Maximum server size") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                if (iValue < 1)
                {
                    iValue = 1;
                }
                else if (iValue > 64)
                {
                    iValue = 64;
                }
                this.m_iMaxServerSize = iValue;
            }

            else if (strVariable.Substring(0,8).CompareTo("Maximum ") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                string strGameMode = "";
                if (strVariable.Substring(8).CompareTo("Conquest Assault64 size") == 0)
                {
                    strGameMode = "ConquestAssaultLarge0";
                }
                else if (strVariable.Substring(8).CompareTo("Conquest Assault size") == 0)
                {
                    strGameMode = "ConquestAssaultSmall0";
                }
                else if (strVariable.Substring(8).CompareTo("Conquest Assault #2 size") == 0)
                {
                    strGameMode = "ConquestAssaultSmall1";
                }
                else if (strVariable.Substring(8).CompareTo("Team Deathmatch size") == 0)
                {
                    strGameMode = "TeamDeathMatch0";
                }
                else if (strVariable.Substring(8).CompareTo("Squad Deathmatch size") == 0)
                {
                    strGameMode = "SquadDeathMatch0";
                }
                else
                {
                    strGameMode = this.GetMapByFormattedName("{GameMode}", strVariable.Substring(8, (strVariable.IndexOf(" size") - 8))).PlayList;
                }

                this.m_DGameModeMax[strGameMode] = iValue;

                if (iValue < 1)
                {
                    this.m_DGameModeMax[strGameMode] = 1;
                }
                else if (iValue > this.m_iMaxServerSize)
                {
                    this.m_DGameModeMax[strGameMode] = this.m_iMaxServerSize;
                }
            }
            else if (strVariable.Substring(0,13).CompareTo("    Size with") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                int i = int.Parse(strVariable.Substring(14,2).Trim());
                this.m_iPlayerServerSize[i] = iValue;

                if (iValue < 1)
                {
                    this.m_iPlayerServerSize[i] = 1;
                }
                else if (iValue < i)
                {
                    this.m_iPlayerServerSize[i] = i;
                }
                else if (iValue > this.m_iMaxServerSize)
                {
                    this.m_iPlayerServerSize[i] = this.m_iMaxServerSize;
                }
            }
            else if (strVariable.CompareTo("Enable adaptive idle kick?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enIdleKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Disable idle kick until how many players are on?") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                if (iValue < 1)
                {
                    iValue = 1;
                }
                else if (iValue > 64)
                {
                    iValue = 64;
                }
                this.m_iDisableIdleKickUntil = iValue;
            }
            else if (strVariable.CompareTo("Desired idle kick time, in seconds") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iDesiredIdleKick = iValue;

                if (iValue < 60)
                {
                    this.m_iDesiredIdleKick = 60;
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

        public override void OnLogin() {
            ResetVars();
            this.ExecuteCommand("procon.protected.send", "vars.maxPlayers");
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }

        public override void OnServerInfo(CServerInfo csiServerInfo) {
            //this.m_iCurrentPlayerCount = csiServerInfo.PlayerCount;
            this.m_iCurrentRoundTime = csiServerInfo.RoundTime;
            this.m_strCurrentGameMode = csiServerInfo.GameMode;
            this.m_iCurrentGameModeMax = this.m_DGameModeMax[this.m_strCurrentGameMode];
            if (this.m_blRoundEnded == true && DateTime.UtcNow.Ticks/10000000 - this.m_lLastRoundEnded > 120) {
                WritePluginConsole("INFO -> Detected level loaded. (timer)");
                StartRound();
            }
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores) {
            if (this.m_enServerSize == enumBoolYesNo.Yes) {
                this.m_blRoundEnded = true;
                this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000;
                WritePluginConsole("INFO -> Round ended.");
                if (this.m_strServerGameType != "bfhl") {
                    this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "60", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "SetMaxTemp");
                }
            }
        }

        public override void OnRestartLevel() {
            if (this.m_enServerSize == enumBoolYesNo.Yes) {
                WritePluginConsole("INFO -> Round restarted.");
                this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000 - 90;
                this.m_blRoundEnded = true;
                this.m_blRestartRequested = false;
                SetMaxTemp();
            }
        }

        public override void OnRunNextLevel() {
            if (this.m_enServerSize == enumBoolYesNo.Yes) {
                WritePluginConsole("INFO -> Next round requested.");
                this.m_lLastRoundEnded = DateTime.UtcNow.Ticks/10000000 - 90;
                this.m_blRoundEnded = true;
                SetMaxTemp();
            }
        }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            if (this.m_enServerSize == enumBoolYesNo.Yes && DateTime.UtcNow.Ticks/10000000 - this.m_lLastLevelLoaded > 5 && !this.m_blRestartRequested) {
                if (this.m_blRoundEnded == false)
                {
                    this.m_blRestartRequested = true;
                    this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                }
                else
                {
                    WritePluginConsole("INFO -> Level loaded.");
                    StartRound();
                }
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) {
            if (this.m_blRoundEnded == true && DateTime.UtcNow.Ticks/10000000 - this.m_lLastRoundEnded > 60) {
                WritePluginConsole("INFO -> Detected level loaded. (player spawn)");
                StartRound();
            }
        }

        private void StartRound() {
            this.m_lLastLevelLoaded = DateTime.UtcNow.Ticks/10000000;
            this.m_blRoundEnded = false;
            this.m_blNextRoundStarted = false;
            this.m_iServerSizeAtStart = this.m_iCurrentServerSize;
            this.ExecuteCommand("procon.protected.send", "admin.listPlayers all");
        }
      
        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                this.m_iCurrentPlayerCount = lstPlayers.Count;

                #region CheckServerSize
                if (this.m_enServerSize == enumBoolYesNo.Yes) {
                    if (this.m_isPluginInitialized == false)
                    {
                        if (this.m_enAssumeMax == enumBoolYesNo.Yes)
                        {
                            this.m_iServerSizeAtStart = this.m_iMaxServerSize;
                        }
                        else
                        {
                            this.m_iServerSizeAtStart = this.m_iCurrentServerSize;
                        }
                        this.m_lLastLevelLoaded = DateTime.UtcNow.Ticks/10000000 - this.m_iCurrentRoundTime;
                        this.m_isPluginInitialized = true;
                    }
                    else if (this.m_iServerSizeAtStart == 0 && this.m_iCurrentRoundTime >= 0)
                    {
                        this.m_iServerSizeAtStart = this.m_iCurrentServerSize;
                    }
                    if (this.m_isPluginInitialized)
                    {
                        foreach (CPlayerInfo cpiPlayer in lstPlayers)
                        {
                            if (this.m_DPlayerJoined.ContainsKey(cpiPlayer.SoldierName))
                            {
                                //WritePluginConsole("Player moved to player list: " + cpiPlayer.SoldierName);
                                this.m_DPlayerJoined.Remove(cpiPlayer.SoldierName);
                            }
                        }
                        foreach (KeyValuePair<string, long> pair in this.m_DPlayerJoined)
                        {
                            if (pair.Value + 30 < DateTime.UtcNow.Ticks/10000000)
                            {
                                this.m_DPlayerJoined.Remove(pair.Key);
                                //WritePluginConsole("Player hold expired: " + pair.Key);
                            }
                        }
                        if (this.m_enShowMessages == enumBoolYesNo.Yes && this.m_lLastMessage + 30 < DateTime.UtcNow.Ticks/10000000 && this.m_iCurrentPlayerCount <= this.m_iMessageMaxUsers && this.m_DPlayerJoined.Count > 0)
                        {
                            if (this.m_sOnJoin1.CompareTo("") != 0)
                            {
                                this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "0", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessage", this.m_sOnJoin1.Replace("[joinCount]", this.m_DPlayerJoined.Count.ToString()));
                            }
                            if (this.m_sOnJoin2.CompareTo("") != 0)
                            {
                                this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "5", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessage", this.m_sOnJoin2.Replace("[joinCount]", this.m_DPlayerJoined.Count.ToString()));
                            }
                            this.m_lLastMessage = DateTime.UtcNow.Ticks/10000000;
                        }

                        CheckServerSize();
                    }
                }
                #endregion

                #region CheckIdleKick
                if (this.m_enIdleKick == enumBoolYesNo.Yes)
                {
                    if (this.m_iCurrentPlayerCount >= this.m_iDisableIdleKickUntil && this.m_iCurrentIdleKick != this.m_iDesiredIdleKick)
                    {
                        WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players online. Idle kick timer set to " + this.m_iDesiredIdleKick + " seconds.");
                        this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", this.m_iDesiredIdleKick.ToString());
                        this.m_iCurrentIdleKick = this.m_iDesiredIdleKick;
                    }
                    else if (this.m_iCurrentPlayerCount < this.m_iDisableIdleKickUntil && this.m_iCurrentIdleKick != 0)
                    {
                        WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players online. Idle kick disabled.");
                        if (this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl") {
                            this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", "86400");
                        } else {
                            this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", "0");
                        }
                        this.m_iCurrentIdleKick = 0;
                    }
                }
                #endregion
            }
        }
        
        public override void OnPlayerLimit(int limit) {
            if (this.m_enServerSize == enumBoolYesNo.Yes) {
                this.m_iCurrentServerSize = limit;
            }
        }

        public override void OnPlayerJoin(string soldierName)
        {
            if (this.m_enServerSize == enumBoolYesNo.Yes) {
                //WritePluginConsole("Player joining; hold added: " + soldierName);
                if (this.m_DPlayerJoined.ContainsKey(soldierName))
                {
                    this.m_DPlayerJoined[soldierName] = DateTime.UtcNow.Ticks/10000000;
                }
                else
                {
                    this.m_DPlayerJoined.Add(soldierName, DateTime.UtcNow.Ticks/10000000);
                    if (this.m_isPluginInitialized && !this.m_blRoundEnded)
                    {
                        CheckServerSize();
                    }
                }
                if (this.m_enShowMessages == enumBoolYesNo.Yes && this.m_lLastMessage + 15 < DateTime.UtcNow.Ticks/10000000 && this.m_iCurrentPlayerCount <= this.m_iMessageMaxUsers && this.m_DPlayerJoined.Count > 0)
                {
                    if (this.m_sOnJoin1.CompareTo("") != 0)
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "0", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessage", this.m_sOnJoin1.Replace("[joinCount]", this.m_DPlayerJoined.Count.ToString()));
                    }
                    if (this.m_sOnJoin2.CompareTo("") != 0)
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "5", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessage", this.m_sOnJoin2.Replace("[joinCount]", this.m_DPlayerJoined.Count.ToString()));
                    }
                    this.m_lLastMessage = DateTime.UtcNow.Ticks/10000000;
                }
            }
        }

        public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId) {
            if (this.m_enShowMessages == enumBoolYesNo.Yes && this.m_enServerSize == enumBoolYesNo.Yes && this.m_iCurrentPlayerCount < this.m_iMessageMaxUsers && !this.m_LStartMessageShown.Contains(soldierName))
            {
                this.m_LStartMessageShown.Add(soldierName);
                if (this.m_sWelcome1.CompareTo("") != 0)
                {
                    this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "2", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessagePlayer", this.m_sWelcome1.Replace("[soldierName]", soldierName), soldierName);
                }
                if (this.m_sWelcome2.CompareTo("") != 0)
                {
                    this.ExecuteCommand("procon.protected.tasks.add", "CAdaptiveServerSize", "7", "1", "1", "procon.protected.plugins.call", "CAdaptiveServerSize", "WriteMessagePlayer", this.m_sWelcome2.Replace("[soldierName]", soldierName), soldierName);
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
            if (this.m_enServerSize == enumBoolYesNo.Yes && this.m_isPluginInitialized && !this.m_blRoundEnded)
            {
                if (this.m_DPlayerJoined.ContainsKey(playerInfo.SoldierName))
                {
                    this.m_DPlayerJoined.Remove(playerInfo.SoldierName);
                }
                else
                {
                    this.m_iCurrentPlayerCount--;
                }
                if (this.m_LStartMessageShown.Contains(playerInfo.SoldierName))
                {
                    this.m_LStartMessageShown.Remove(playerInfo.SoldierName);
                }
                CheckServerSize();
            }
        }

      #endregion

        public void SetMaxTemp() {
            if (this.m_iCurrentServerSize != this.m_iMaxServerSize)
            {
                WritePluginConsole("WORK -> Server size changed to maximum value (" + this.m_iMaxServerSize + " players) temporarily.");
                this.ExecuteCommand("procon.protected.send", "vars.maxPlayers", this.m_iMaxServerSize.ToString());
            }
        }

        private void CheckServerSize() {
            this.m_iDesiredServerSize = Math.Min(this.m_iCurrentGameModeMax, this.m_iPlayerServerSize[Math.Min(this.m_iCurrentPlayerCount + this.m_DPlayerJoined.Count, this.m_iMaxServerSize)]);
            if (this.m_strServerGameType != "bfhl" && (this.m_iCurrentPlayerCount == 0 || DateTime.UtcNow.Ticks/10000000 - this.m_lLastLevelLoaded < 5) && !this.m_blRestartRequested && this.m_iServerSizeAtStart < this.m_iMaxServerSize && !this.m_blRoundEnded && this.m_iServerSizeAtStart > 0)
            {
                WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Round started at " + this.m_iServerSizeAtStart + ". Restarting round at max server size.");
                this.m_blRestartRequested = true;
                this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
            }
            else if (this.m_iDesiredServerSize == this.m_iCurrentServerSize)
            {
                if (this.m_strServerGameType == "bfhl" || this.m_iCurrentServerSize <= this.m_iServerSizeAtStart)
                { // Server size is correct, or is less than when the round started
                    WritePluginConsole("IDLE -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Server size set to " + this.m_iCurrentServerSize + ". Round started at " + this.m_iServerSizeAtStart + ".");
                }
                else
                { // Server size is correct, but is greater than it was when the round started, so change it back
                    WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Round started at " + this.m_iServerSizeAtStart + ". Server size changed to " + this.m_iServerSizeAtStart + " until round is over.");
                    this.ExecuteCommand("procon.protected.send", "vars.maxPlayers", this.m_iDesiredServerSize.ToString());
                }
            }
            else
            {
                if (this.m_strServerGameType != "bfhl" && this.m_blRoundEnded == true)
                {
                    WritePluginConsole("TASK -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Server size set to " + this.m_iCurrentServerSize + ", but should be " + this.m_iDesiredServerSize + ". Waiting for next round to start.");
                }
                else if (this.m_strServerGameType != "bfhl" && this.m_iDesiredServerSize > this.m_iServerSizeAtStart)
                {
                    WritePluginConsole("TASK -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Server size set to " + this.m_iCurrentServerSize + ", but should be " + this.m_iDesiredServerSize + ". Round started at " + this.m_iServerSizeAtStart + ". Waiting for round to end.");
                }
                else
                {
                    WritePluginConsole("WORK -> " + this.m_iCurrentPlayerCount + " players online. " + this.m_DPlayerJoined.Count + " players joining. Server size changed to " + this.m_iDesiredServerSize + ". Round started at " + this.m_iServerSizeAtStart + ".");
                    this.ExecuteCommand("procon.protected.send", "vars.maxPlayers", this.m_iDesiredServerSize.ToString());
                }
            }
        }

#region helper_functions

        private void WritePluginConsole(string message)
        {
            string line = String.Format("AdaptiveServerSize: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes) { 
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        public void WriteMessage(string message)
        {
            List<string> wordWrappedLines = this.WordWrap(message, 100);
            foreach (string line in wordWrappedLines)
            {
                string formattedLine = String.Format("{0}", line);
                this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "all");
            }
        }

        public void WriteMessagePlayer(string message, string player)
        {
            List<string> wordWrappedLines = this.WordWrap(message, 100);
            foreach (string line in wordWrappedLines)
            {
                string formattedLine = String.Format("{0}", line);
                this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "player", player);
            }
        }

#endregion

    }
}
