/*  Copyright 2010 Geoffrey 'Phogue' Green

    This file is part of BFBC2 PRoCon.

    BFBC2 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BFBC2 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BFBC2 PRoCon.  If not, see <http://www.gnu.org/licenses/>.

 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class CAutoRoundRestarter : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        // Status
        private string m_strServerGameType;
        private string m_strGameMod;
        private string m_strServerVersion; 
        
        enumBoolYesNo m_enPluginOnLayer;
        enumBoolYesNo m_enDoConsoleOutput;
        enumBoolYesNo m_enDoDebugOutput;
        enumBoolYesNo m_enDoTestdrive;

        enumBoolYesNo m_enRestartRound;
        enumBoolYesNo m_enLoadFirstMap;
        enumBoolYesNo m_enDoForwardMap;
        enumBoolYesNo m_enDoServerRestart;
        enumBoolYesNo m_enDoPluginLoadOverride;

        enumBoolYesNo m_enOnLongRound;
        enumBoolYesNo m_enDisableIdleKickUntil;
        enumBoolYesNo m_enDoWatchdog;

        private bool m_blMapWasRestarted;
        private bool m_blFirstMapWasRestarted;
        private bool m_blServerIsRanked;

        private int m_iSrvInfoCurPlayerCount;
        private int m_iSrvInfoMaxPlayerCount;
        private int m_iSrvInfoActRoundTime;
        private int m_iSrvInfoCurRound;
        private int m_iSrvInfoTotalRounds;
        private int m_iForwardTasks;
        private int m_iRoundRestartCount;
        private int m_iRoundDuration;
        private int m_iForwardWait;
        private int m_iIdleUntilPlayers;
        private int m_iOldIdleTimeout;
        private int m_iWatchdogInterval;
        private int m_iTimeDivider;

        /// summary
        /// 0 = nothing
        /// 1 = deactivated
        /// 2 = activated
        /// summary
        private int m_iIdleKickChanged;
                
        private bool m_isPluginEnabled;

        public CAutoRoundRestarter() {

            this.m_enPluginOnLayer          = enumBoolYesNo.Yes;
            this.m_enDoConsoleOutput 	    = enumBoolYesNo.No;
            this.m_enDoDebugOutput 		    = enumBoolYesNo.No;
            this.m_enDoTestdrive            = enumBoolYesNo.No;

            this.m_enRestartRound           = enumBoolYesNo.No;
            this.m_enLoadFirstMap           = enumBoolYesNo.No;
            this.m_enDoForwardMap           = enumBoolYesNo.No;
            this.m_enDoServerRestart        = enumBoolYesNo.No;
            this.m_enDoPluginLoadOverride   = enumBoolYesNo.No;

            this.m_enOnLongRound            = enumBoolYesNo.No;
            this.m_enDisableIdleKickUntil   = enumBoolYesNo.No;
            this.m_enDoWatchdog             = enumBoolYesNo.No;

            this.m_blMapWasRestarted        = false;
            this.m_blFirstMapWasRestarted   = false;
            this.m_blServerIsRanked         = false;

            this.m_iSrvInfoCurPlayerCount	= 0;
	        this.m_iSrvInfoMaxPlayerCount	= 0;
            this.m_iSrvInfoActRoundTime     = 0;
            this.m_iSrvInfoCurRound         = 0;
            this.m_iSrvInfoTotalRounds      = 0;
            this.m_iForwardTasks            = 0;

	        this.m_iRoundRestartCount	    = 0;
            this.m_iRoundDuration           = 43200;
            this.m_iForwardWait             = 15 * 60;
            this.m_iIdleUntilPlayers        = 5;
            this.m_iOldIdleTimeout          = 300;
            this.m_iWatchdogInterval        = 24 * 60 * 60;

            this.m_iIdleKickChanged = 0;
            this.m_iTimeDivider = 1;

            this.m_isPluginEnabled          = false;
        }

        public string GetPluginName() {
            return "Automatic Round Restarter";
        }

        public string GetPluginVersion() {
            return "1.2.0.5";
        }

        public string GetPluginAuthor() {
            return "Phil_K";
        }

        public string GetPluginWebsite() {
            return "phogue.net/forumvb/showthread.php?2848-Automatic-Round-Restarter";
        }

        public string GetPluginDescription() {
            return @"<p>If you find my plugins useful, please feel free to donate</p>
<blockquote>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""encrypted"" value=""-----BEGIN PKCS7-----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-----END PKCS7-----"">
<input type=""image"" src=""https://www.paypalobjects.com/de_DE/DE/i/btn/btn_donate_SM.gif"" border=""0"" name=""submit"" alt=""Jetzt einfach, schnell und sicher online bezahlen – mit PayPal."">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>
</blockquote>

<h2>Description</h2>
    <p>This plug-in is intended to restart the running map as soon as the actual player count
    Restarts the current round or loads the first map of your mapList if player count is lower
    or equals a given value.</p>

    <p>This can help you to avoid dead-locks with battlelog where no player can join your server
    and/or gets the message that the server is currently changing map and therefor can not be
    joined.</p>

    <p> As an addition for whorst case you can select to restart your server. This will send the
    command admin.shutDown to your server. Depending on your GSPs configuration the server will
    be restarted or stays down. If that is the case you have to go to your GSPs config panel and
    start your server by your own. <b> USE WITH CARE! </b> </p>

    <p>If you select this option the plug-in will disable itself as a last line of warning.
    You also will see a message on the plugin console. For local running plug-ins there will be
    an information window.</p>

<h2>Commands</h2>
    This plug-in has no in-game command.

<h2>Settings</h2>
    <h3>Server</h3>
        <blockquote><h4>RestartRoundCount</h4> Sets the value against which the actual player count is checked. If it is less or equal the action will taken.</blockquote>
        <blockquote><h4>Restart round</h4> The actual round will be restarted.</blockquote>
        <blockquote><h4>Load first map</h4> The first map in your maplist will be loaded and started on its first round.</blockquote>
        <blockquote><h4>Restart server</h4> The server will receive a command to shutdown and will be restarted by its configuration. <b>USE WITH CARE!</b></blockquote>
        <blockquote><h4>Override disabling</h4> This will disable the automated deactivation of the plug-in on a Procon restart.</blockquote>
    <h3>Trigger (OR)</h3>
        <blockquote><h4>On long round</h4> Select YES if you want the round restarted (only) on long rounds, e.g. to prevent statspadding.</blockquote>
        <blockquote><h4>Max. Round duration (minutes)</h4> As said give the time in minutes a round should last max. This trigger only gets active if actual player count is >= 1.</blockquote>
        <blockquote><h4>Disable IdleKick until</h4> Enables a idleTimeout disabler. ;) Since R-9 only usable on unranked servers. That is checked on plugin enabled.</blockquote>
        <blockquote><h4>Players online</h4> idleTimeout stays disabled until this player count is reached. If that happens the old value is restored. In case you set a new time manually the new time is recognized and taken.</blockquote>
        <blockquote><h4>Forward map</h4> While empty for a given time the next map in the rotation will be loaded.</blockquote>
        <blockquote><h4>Forward after (minutes)</h4> The time to wait before forwarding to the next map in minutes.</blockquote>
        <blockquote><h4>Enable Watchdog</h4> Select YES to enable a server restart watchdog. The server will be restarted if it is empty and server uptime is greater than the given value.</blockquote>        
        <blockquote><h4>Watchdog interval (hours)</h4> The time the server has to be up before the watchdog triggers an admin.shutdown command to restart.</blockquote>
    <h3>Extra</h3>
        <blockquote><h4>On layer</h4> Select YES if the plug-in runs on a layer server. This prevents it from raising info windows.</blockquote>

<br/><br/>
<h2>Development</h2>
    <h3>Changelog</h3>
        <blockquote><h4>1.2.0.5 (2014-03-19)</h4>
            - added some workaround for roundCounts 0/0
            - added reset of idleTime in case plugin is deactivated
         </blockquote>
        <blockquote><h4>1.2.0.4 (2014-03-10)</h4>
            - for BF4 disable idleTimeout sets it to 86400
         </blockquote>
        <blockquote><h4>1.2.0.3 (2013-11-29)</h4>
            - fixed typo
         </blockquote>
        <blockquote><h4>1.2.0.2 (2013-11-28)</h4>
            - checked BF4 compatibility
            - added BF4 plugin subfolder 
    	</blockquote>
        <blockquote><h4>1.2.0.1 (2012-03-11)</h4>
            - added workaround for for restart on first map
    	</blockquote>
        <blockquote><h4>1.2.0.0 (2011-12-19)</h4>
            - fixed small bugs<br/>
            - added option to restart the server after a given number of hours uptime
    	</blockquote>
        <blockquote><h4>1.1.1.3 (2011-11-25)</h4>
            - reactivated option to use IdleTimeout disabler<br/>
    	</blockquote>
        <blockquote><h4>1.1.1.2 (2011-11-22)</h4>
            - Added check that forward map can be used while all restart options are disabled<br/>
    	</blockquote>
        <blockquote><h4>1.1.1.1</h4>
            - R9 based change; Disable idleKick can not be used on ranked servers anymore<br/>
    	</blockquote>
        <blockquote><h4>1.1.1.0 (2011-11-21)</h4>
            - added option to forward to the next map after x minutes<br/>
    	</blockquote>
        <blockquote><h4>1.1.0.0 (2011-11-19)</h4>
            - fixed plugin getting disabled on RestartServer == No<br/>
            - fixed taking 0 as idleTimout if the server runs with idleTimeout == 0 at the moment the plug-in is enabled.<br/>
              &nbsp;&nbsp;&nbsp;In that case 300 will be used. (default for idleTimeout if not given in startup.txt)<br/>
            - fixed leaving variables with values after plug-in is disabled. On Re-enable the plug-in now does a fresh start<br/>
            - added override option to disable plug-in while Procon is restarted and RestartServer == Yes<br/>
    	</blockquote>
        <blockquote><h4>1.0.2.0 (2011-11-14)</h4>
            - added option to disable IdleKick until X players are on the server. (Idea by Zaeed) :)<br/>
    	</blockquote>
        <blockquote><h4>1.0.1.0 (2011-11-13)</h4>
            - added long round check.<br/>
    	</blockquote>
        <blockquote><h4>1.0.0.0 (2011-11-12)</h4>
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

            if (this.m_strServerGameType == "bf3" || this.m_strServerGameType == "bf4") {
                this.m_iTimeDivider = 1000;
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;

            this.RegisterEvents(this.GetType().Name, "OnServerInfo", "OnIdleTimeout", "OnLevelLoaded");
        }

        public void OnPluginEnable() {
            this.ResetMyVars(1);
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAutoRoundRestarter: ^2Enabled!");
            this.ExecuteCommand("procon.protected.send", "vars.idleTimeout"); 
            this.ExecuteCommand("procon.protected.send", "serverInfo");
            this.m_isPluginEnabled = true;
        }

        public void OnPluginDisable() {
            this.ResetMyVars(0);
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAutoRoundRestarter: ^1Disabled =(" );
            this.m_isPluginEnabled = false;
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option 
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Server|RoundRestartCount", typeof(int), this.m_iRoundRestartCount));
            lstReturn.Add(new CPluginVariable("Server|Restart round", typeof(enumBoolYesNo), this.m_enRestartRound));
            lstReturn.Add(new CPluginVariable("Server|Load first map", typeof(enumBoolYesNo), this.m_enLoadFirstMap));
            lstReturn.Add(new CPluginVariable("Server|Restart server", typeof(enumBoolYesNo), this.m_enDoServerRestart));
            if (this.m_enDoServerRestart == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Server|Override disabling", typeof(enumBoolYesNo), this.m_enDoPluginLoadOverride));
            }
            lstReturn.Add(new CPluginVariable("Trigger (OR)|On long round", typeof(enumBoolYesNo), this.m_enOnLongRound));
            if (this.m_enOnLongRound == enumBoolYesNo.Yes)
            { 
                lstReturn.Add(new CPluginVariable("Trigger (OR)|Max. Round duration (minutes)", this.m_iRoundDuration.GetType(), this.m_iRoundDuration / 60));
            }
            lstReturn.Add(new CPluginVariable("Trigger (OR)|Disable IdleKick until", typeof(enumBoolYesNo), this.m_enDisableIdleKickUntil));
            if (this.m_enDisableIdleKickUntil == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Trigger (OR)|Players online", this.m_iIdleUntilPlayers.GetType(), this.m_iIdleUntilPlayers));
            }
            lstReturn.Add(new CPluginVariable("Trigger (OR)|Forward map", typeof(enumBoolYesNo), this.m_enDoForwardMap));
            if (this.m_enDoForwardMap == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Trigger (OR)|Forward after (minutes)", this.m_iForwardWait.GetType(), this.m_iForwardWait / 60));
            }
            lstReturn.Add(new CPluginVariable("Trigger (OR)|Enable Watchdog", typeof(enumBoolYesNo), this.m_enDoWatchdog));
            if (this.m_enDoWatchdog == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Trigger (OR)|Watchdog interval (hours)", this.m_iWatchdogInterval.GetType(), this.m_iWatchdogInterval / 60 / 60));
            } 
            lstReturn.Add(new CPluginVariable("Xtras|On layer", typeof(enumBoolYesNo), this.m_enPluginOnLayer));
            lstReturn.Add(new CPluginVariable("Xtras|Console output", typeof(enumBoolYesNo), this.m_enDoConsoleOutput));
            lstReturn.Add(new CPluginVariable("Xtras|Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            lstReturn.Add(new CPluginVariable("Xtras|Testdrive", typeof(enumBoolYesNo), this.m_enDoTestdrive));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("RoundRestartCount", typeof(int), this.m_iRoundRestartCount));
            lstReturn.Add(new CPluginVariable("Restart round", typeof(enumBoolYesNo), this.m_enRestartRound));
            lstReturn.Add(new CPluginVariable("Load first map", typeof(enumBoolYesNo), this.m_enLoadFirstMap));
            lstReturn.Add(new CPluginVariable("Override disabling", typeof(enumBoolYesNo), this.m_enDoPluginLoadOverride));
            lstReturn.Add(new CPluginVariable("Restart server", typeof(enumBoolYesNo), this.m_enDoServerRestart));
            lstReturn.Add(new CPluginVariable("On long round", typeof(enumBoolYesNo), this.m_enOnLongRound));
            lstReturn.Add(new CPluginVariable("Max. Round duration (minutes)", this.m_iRoundDuration.GetType(), this.m_iRoundDuration / 60));
            lstReturn.Add(new CPluginVariable("Disable IdleKick until", typeof(enumBoolYesNo), this.m_enDisableIdleKickUntil));
            lstReturn.Add(new CPluginVariable("Players online", this.m_iIdleUntilPlayers.GetType(), this.m_iIdleUntilPlayers));
            lstReturn.Add(new CPluginVariable("Forward map", typeof(enumBoolYesNo), this.m_enDoForwardMap));
            lstReturn.Add(new CPluginVariable("Forward after (minutes)", this.m_iForwardWait.GetType(), this.m_iForwardWait / 60));
            lstReturn.Add(new CPluginVariable("Enable Watchdog", typeof(enumBoolYesNo), this.m_enDoWatchdog));
            lstReturn.Add(new CPluginVariable("Watchdog interval (hours)", this.m_iForwardWait.GetType(), this.m_iWatchdogInterval / 60 / 60));
            lstReturn.Add(new CPluginVariable("On layer", typeof(enumBoolYesNo), this.m_enPluginOnLayer));
            lstReturn.Add(new CPluginVariable("Console output", typeof(enumBoolYesNo), this.m_enDoConsoleOutput));
            lstReturn.Add(new CPluginVariable("Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            lstReturn.Add(new CPluginVariable("Testdrive", typeof(enumBoolYesNo), this.m_enDoTestdrive));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) 
        {
            int iTMP = 2;
            int iTimeMinutes = 60;

            if (strVariable.CompareTo("RoundRestartCount") == 0 && int.TryParse(strValue, out iTMP) == true)
            {
                if (iTMP >= 8)
                {
                    iTMP = 8;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No) { MessageBox.Show("Please use a value between 0 and 8.", "Value Limit", MessageBoxButtons.OK); }
                    this.WritePluginConsole("RestartRoundCount-> Please use a value between 0 and 8.", 2);
                }
                if (iTMP < 0)
                {
                    iTMP = 0;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No) { MessageBox.Show("Please use a value between 0 and 8.", "Value Limit", MessageBoxButtons.OK); }
                    this.WritePluginConsole("RestartRoundCount-> Please use a value between 0 and 8.", 2);
                }
                this.m_iRoundRestartCount = iTMP;
            }
            else if (strVariable.CompareTo("Restart round") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enRestartRound = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enRestartRound == enumBoolYesNo.Yes && (this.m_enLoadFirstMap == enumBoolYesNo.Yes || this.m_enDoServerRestart == enumBoolYesNo.Yes))
                {
                    this.m_enLoadFirstMap = enumBoolYesNo.No;
                    this.m_enDoServerRestart = enumBoolYesNo.No;
                    this.m_enDoPluginLoadOverride = enumBoolYesNo.No;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Please decide: Restart round OR load first mapOR restart server", "Decission", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Please decide: Restart round OR load first mapOR restart server.", 2);
                }
            }
            else if (strVariable.CompareTo("Load first map") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enLoadFirstMap = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enLoadFirstMap == enumBoolYesNo.Yes && (this.m_enRestartRound == enumBoolYesNo.Yes || this.m_enDoServerRestart == enumBoolYesNo.Yes))
                {
                    this.m_enRestartRound = enumBoolYesNo.No;
                    this.m_enDoServerRestart = enumBoolYesNo.No;
                    this.m_enDoPluginLoadOverride = enumBoolYesNo.No;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Please decide: Restart round OR load first map OR restart server", "Decission", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Please decide: Restart round OR load first map OR restart server.", 2);
                }
            }
            else if (strVariable.CompareTo("Override disabling") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoPluginLoadOverride = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoPluginLoadOverride == enumBoolYesNo.Yes)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("ATTENTION! You have selected to override the automated deactivation of this plug-in! " +
                            "Be aware that this can lead to a restart of your server after Procon has been restarted.", "WARNING", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("^b^1Override selected -> ATTENTION! You have selected to override the automated deactivation of this plug-in! " +
                            "Be aware that this can lead to a restart of your server after Procon has been restarted.", 2);
                }
            }
            else if (strVariable.CompareTo("Restart server") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoServerRestart = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoServerRestart == enumBoolYesNo.No) { this.m_enDoPluginLoadOverride = enumBoolYesNo.No; }

                if (this.m_enDoServerRestart == enumBoolYesNo.Yes && this.m_isPluginEnabled == true & this.m_enDoPluginLoadOverride == enumBoolYesNo.No)
                {
                    this.m_enDoServerRestart = enumBoolYesNo.No;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("You can only select to RESTART your server while the plugin is disabled. Please read its description carefully " +
                            "BEFORE you set this option again to yes and re-enable the plugin.", "WARNING", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("^b^1Restart server -> You can only select to RESTART your server while the plugin is disabled. Please read its description carefully " +
                    "BEFORE you set this option again to yes and re-enable the plugin.", 2);
                    this.ExecuteCommand("procon.protected.plugins.enable", "CAutoRoundRestarter", "False");
                    this.ExecuteCommand("procon.protected.send", "procon.plugin.enable", "CAutoRoundRestarter False");
                }
                if (this.m_enDoServerRestart == enumBoolYesNo.Yes && (this.m_enRestartRound == enumBoolYesNo.Yes || this.m_enLoadFirstMap == enumBoolYesNo.Yes || this.m_enDoForwardMap == enumBoolYesNo.Yes))
                {
                    this.m_enRestartRound = enumBoolYesNo.No;
                    this.m_enLoadFirstMap = enumBoolYesNo.No;
                    this.m_enDoForwardMap = enumBoolYesNo.No;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Please decide: Restart round OR load first map OR forward map OR restart server", "Decission", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Please decide: Restart round OR load first map OR forward map OR restart server.", 2);
                }
            }
            /* Trigger */
            else if (strVariable.CompareTo("On long round") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enOnLongRound = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Max. Round duration (minutes)") == 0 && int.TryParse(strValue, out iTimeMinutes) == true)
            {
                this.m_iRoundDuration = iTimeMinutes * 60;

                if (iTimeMinutes <= 0)
                {
                    this.m_iRoundDuration = 1200;
                }
                else if (iTimeMinutes > 720)
                {
                    this.m_iRoundDuration = 43200;
                }
            }
            else if (strVariable.CompareTo("Disable IdleKick until") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDisableIdleKickUntil = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDisableIdleKickUntil == enumBoolYesNo.Yes && this.m_blServerIsRanked == true)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Server is assumed to be RANKED. Please activate the plugin with testdrive to check. " +
                            "IdleTimeout can only be disabled on unranked servers.", "Decission", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Server is assumed to be RANKED. Please activate the plugin with testdrive to check. " +
                            "IdleTimeout can only be disabled on unranked servers.", 2);
                    this.m_enDisableIdleKickUntil = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Players online") == 0 && int.TryParse(strValue, out iTMP) == true)
            {
                this.m_iIdleUntilPlayers = iTMP;

                if (iTMP <= 0)
                {
                    this.m_iIdleUntilPlayers = 0;
                }
                else if (iTMP > 64)
                {
                    this.m_iIdleUntilPlayers = 64;
                }
            }
            else if (strVariable.CompareTo("Forward map") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoForwardMap = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                /* if (this.m_enDoForwardMap == enumBoolYesNo.Yes && (this.m_enRestartRound == enumBoolYesNo.Yes || this.m_enDoServerRestart == enumBoolYesNo.Yes || this.m_enLoadFirstMap == enumBoolYesNo.Yes))
                {
                    this.m_enRestartRound = enumBoolYesNo.No;
                    this.m_enLoadFirstMap = enumBoolYesNo.No;
                    this.m_enDoServerRestart = enumBoolYesNo.No;
                    this.m_enDoPluginLoadOverride = enumBoolYesNo.No;
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Please decide: Restart round OR load first map OR forward map OR restart server", "Decission", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Please decide: Restart round OR load first map OR forward map OR restart server.", 2);
                } */
            }
            else if (strVariable.CompareTo("Forward after (minutes)") == 0 && int.TryParse(strValue, out iTimeMinutes) == true)
            {
                this.m_iForwardWait = iTimeMinutes * 60;

                if (iTimeMinutes <= 1)
                {
                    this.m_iForwardWait = 60;
                }
                else if (iTimeMinutes > 720)
                {
                    this.m_iForwardWait = 43200;
                }
            }
            else if (strVariable.CompareTo("Enable Watchdog") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoWatchdog = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Watchdog interval (hours)") == 0 && int.TryParse(strValue, out iTimeMinutes) == true)
            {
                this.m_iWatchdogInterval = iTimeMinutes * 60 *60;

                if (iTimeMinutes <= 1)
                {
                    this.m_iWatchdogInterval = 3600;
                }
                else if (iTimeMinutes > 168)
                {
                    this.m_iWatchdogInterval = 604800;
                }
            }
            /* extras */
            else if (strVariable.CompareTo("On layer") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enPluginOnLayer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Console output") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoConsoleOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Debug output") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Testdrive") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoTestdrive = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoTestdrive == enumBoolYesNo.No)
                {
                    this.m_blMapWasRestarted = false;
                    this.m_iIdleKickChanged = 0;
                }
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

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            // m_blMapWasRestarted / m_blFirstMapWasRestarted
            if (this.m_blFirstMapWasRestarted == true)
            {
                this.m_blFirstMapWasRestarted = false; 
                
                this.m_blMapWasRestarted = true;

                if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                {
                    this.WritePluginConsole("TESTDRIVE: Normally round has been restarted after load of first map.", 2);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                }
            }
        }
        
        public override void OnIdleTimeout(int limit) {
            this.WritePluginConsole("OnIdleTimeout event detected. (" + limit.ToString() +")", 1);
            if (this.m_iIdleKickChanged != 1 && (limit != 0 || (limit != 86400 && this.m_strServerGameType == "bf4")))
            {
                this.WritePluginConsole("OnIdleTimeout changed outside this plugin. Taking new value for Restore. (" + limit.ToString() + ")", 1);
                this.m_iOldIdleTimeout = limit;
            }
        }

        public override void OnServerInfo(CServerInfo csiServerInfo) {
            this.m_iSrvInfoCurPlayerCount = csiServerInfo.PlayerCount;
            this.m_iSrvInfoActRoundTime = csiServerInfo.RoundTime;
            this.m_iSrvInfoCurRound = csiServerInfo.CurrentRound;
            this.m_iSrvInfoTotalRounds = csiServerInfo.TotalRounds;
            // this.m_blServerIsRanked = csiServerInfo.Ranked;
            enumBoolYesNo p_enAllDeak = enumBoolYesNo.No;

            //this.WritePluginConsole("INFO: ranked server status is: " + this.m_blServerIsRanked.ToString(), 0);

            #region Check2Forward
            if (this.m_enDoForwardMap == enumBoolYesNo.Yes)
            {
                if (this.m_enRestartRound == enumBoolYesNo.No && this.m_enLoadFirstMap == enumBoolYesNo.No && this.m_enDoServerRestart == enumBoolYesNo.No) {
                    p_enAllDeak = enumBoolYesNo.Yes;
                }
                if (this.m_iSrvInfoCurPlayerCount == 0 && this.m_iSrvInfoActRoundTime >= this.m_iForwardWait && (this.m_blMapWasRestarted == true || this.m_blMapWasRestarted == false && p_enAllDeak == enumBoolYesNo.Yes))
                {
                    this.m_iForwardTasks = this.m_iSrvInfoTotalRounds - this.m_iSrvInfoCurRound;
                    this.m_iForwardTasks = (this.m_iForwardTasks <= 0) ? 1 : this.m_iForwardTasks;
                    this.WritePluginConsole("WORK -> Forward... check needed runNextRounds (" + this.m_iForwardTasks.ToString() + ")", 1);
                    for (int i = 1; i <= this.m_iForwardTasks; i++) {
                        if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                        {
                            this.WritePluginConsole("TESTDRIVE: Would send " + i.ToString() + " of " + this.m_iForwardTasks + " runNextMap", 2); 
                        } else {
                            this.WritePluginConsole("TASK: Sending " + i.ToString() + " of " + this.m_iForwardTasks + " runNextMap", 0);
                            this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                        }
                    }
                    this.m_blMapWasRestarted = true;
                }
            }
            #endregion

            #region CheckRoundRestartCound
            if (this.m_iSrvInfoCurPlayerCount > this.m_iRoundRestartCount) {
                this.WritePluginConsole("IDLE -> Round running with " + this.m_iSrvInfoCurPlayerCount + " players. Check against: " + this.m_iRoundRestartCount + ".", 1);
                this.m_blMapWasRestarted = false;
                this.m_blFirstMapWasRestarted = false;
            }
            else if (this.m_iSrvInfoCurPlayerCount <= this.m_iRoundRestartCount && this.m_blMapWasRestarted == true)
            {
                this.WritePluginConsole("IDLE -> Round/Server was restarted. So nothing todo until more than " + this.m_iRoundRestartCount + " players (" + 
                    this.m_iSrvInfoCurPlayerCount + ") are online.", 1);
            }
            else if (this.m_iSrvInfoCurPlayerCount <= this.m_iRoundRestartCount && this.m_blMapWasRestarted == false)
            {
                this.WritePluginConsole("WORK -> Round running with " + this.m_iSrvInfoCurPlayerCount + " players. Check against: " + this.m_iRoundRestartCount + ".", 1);

                if (this.m_enDoServerRestart == enumBoolYesNo.Yes)
                {
                    this.WritePluginConsole("TASK -> Restart server selected.", 0);
                    this.WritePluginConsole("TASK -> Sending info to all players.", 0);
                    this.WriteMessage("Server reached restart player limit. Server is going to be restarted.");
                    this.WritePluginConsole("TASK -> Restarting server...", 0);

                    this.m_blMapWasRestarted = true;
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        this.WritePluginConsole("TESTDRIVE: restart server command would normally be send", 2); 
                    } else {
                        this.ExecuteCommand("procon.protected.send", "admin.shutDown");
                    }
                }

                if (this.m_enLoadFirstMap == enumBoolYesNo.Yes)
                {
                    this.WritePluginConsole("TASK -> Restart round on first map selected..", 0);
                    this.WritePluginConsole("TASK -> Sending info to all players.", 0);
                    this.WriteMessage("Server reached restart player limit. New map/round will be loaded.");
                    this.WritePluginConsole("TASK -> Setting first map", 0);
                    if (this.m_enDoTestdrive == enumBoolYesNo.No)
                    {
                        this.ExecuteCommand("procon.protected.send", "mapList.setNextMapIndex", "0");
                    }
                    this.WritePluginConsole("TASK -> Forwarding to map.", 0);
                    
                    this.m_blMapWasRestarted = true;
                    this.m_blFirstMapWasRestarted = true;
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        this.WritePluginConsole("TESTDRIVE: Normally first map would be set and loaded.", 2);
                    } else {
                        this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                    }
                }

                if (this.m_enRestartRound == enumBoolYesNo.Yes)
                {
                    this.WritePluginConsole("TASK -> Restart round selected..", 0);
                    this.WritePluginConsole("TASK -> Sending info to all players.", 0);
                    this.WriteMessage("Server reached restart player limit. Round will be restarted.");
                    this.WritePluginConsole("TASK -> Restarting round.", 0);

                    this.m_blMapWasRestarted = true; 
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        this.WritePluginConsole("TESTDRIVE: Normally Round has been restarted.", 2); 
                    } else {
                        this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                    }
                }

                if (this.m_enRestartRound == enumBoolYesNo.No && this.m_enLoadFirstMap == enumBoolYesNo.No && this.m_enDoServerRestart == enumBoolYesNo.No)
                {
                    this.WritePluginConsole("DEAK -> RoundRestartCount reached, but you settings allow nothing to do. ;)", 1);
                }
            }
            #endregion

            #region CheckRoundDuration
            if (this.m_enOnLongRound == enumBoolYesNo.Yes)
            {
                this.WritePluginConsole("Checking RoundTime...", 1);
                if (this.m_iSrvInfoCurPlayerCount >= 1 && this.m_iSrvInfoActRoundTime >= this.m_iRoundDuration)
                {
                    this.WritePluginConsole("WORK -> RT: " + (this.m_iSrvInfoActRoundTime / 60).ToString() + " Limit: " + this.m_iRoundDuration + ".", 1);
                    
                    this.m_blMapWasRestarted = true;
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        this.WritePluginConsole("TESTDRIVE: Normally the round would now be restarted.", 2);
                    } else {
                        this.WritePluginConsole("TASK: Long round -> sending message to all players.", 0);
                        this.WriteMessage("Server reached restart round time limit. Round is going to be restarted.");
                        this.WritePluginConsole("TASK: Long round -> restarting round.", 0);
                        this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                    }
                }
                if (this.m_iSrvInfoCurPlayerCount < 1 || this.m_iSrvInfoActRoundTime < this.m_iRoundDuration || this.m_blMapWasRestarted == true)
                {
                    this.WritePluginConsole("IDLE -> No players or round time less limit.", 0);
                }
            }
            #endregion

            #region CheckIdleKickCount
            // R10 outdated if (this.m_enDisableIdleKickUntil == enumBoolYesNo.Yes && this.m_blServerIsRanked == false)
            if (this.m_enDisableIdleKickUntil == enumBoolYesNo.Yes)
            {
                if (this.m_iSrvInfoCurPlayerCount < this.m_iIdleUntilPlayers && this.m_iIdleKickChanged != 1)
                {
                    this.WritePluginConsole("WORK -> PlayerCount below limit, IdleTimeout should be disabled...", 1);
                    
                    this.m_iIdleKickChanged = 1;
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        if (this.m_strServerGameType == "bf4") {
                            this.WritePluginConsole("TESTDRIVE: (BF4) Normally idleTimeout would be set to 86400 to virtually disable it.", 2);
                        }
                        else {
                            this.WritePluginConsole("TESTDRIVE: Normally idleTimeout would be set to zero to disable.", 2);
                        }
                    }
                    else
                    {
                        if (this.m_strServerGameType == "bf4") {
                            this.WritePluginConsole("TASK: IdleKick -> setting vars.idleTimeout to zero. (86400 for BF4)", 0);
                            this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", "86400");
                        } else {
                            this.WritePluginConsole("TASK: IdleKick -> setting vars.idleTimeout to zero.", 0);
                            this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", "0");
                        }
                    }
                }
                if (this.m_iSrvInfoCurPlayerCount >= this.m_iIdleUntilPlayers && this.m_iIdleKickChanged != 2)
                {
                    this.WritePluginConsole("WORK -> PlayerCount reached limit, IdleTimeout should be set...", 1);

                    this.m_iIdleKickChanged = 2; 
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes)
                    {
                        this.WritePluginConsole("TESTDRIVE: Normally idleTimeout would be restored.", 2);
                    }
                    else
                    {
                        this.WritePluginConsole("TASK: IdleKick -> setting vars.idleTimeout to old value (" + this.m_iOldIdleTimeout.ToString() + ").", 0);
                        this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", this.m_iOldIdleTimeout.ToString());
                    }
                }
            }
            #endregion

            this.WatchdogCheck(csiServerInfo.ServerUptime, csiServerInfo.PlayerCount);

            this.RegisterAllCommands();
	    }

        #endregion
        #region Own_functions

        private void WatchdogCheck(int iServerUptime, int iPlayerCount)
        {
            int iChkUptimeHours = this.m_iWatchdogInterval / 60 / 60;
            if (this.m_enDoWatchdog == enumBoolYesNo.Yes)
            {
                this.WritePluginConsole("WORK -> Checking server Uptime...", 1);
                if (iServerUptime > this.m_iWatchdogInterval && iPlayerCount == 0)
                {
                    this.WritePluginConsole("INFO -> Server is empty and Uptime (" + iServerUptime.ToString() + ") is above " + iChkUptimeHours.ToString() + " hours ("
                        + this.m_iWatchdogInterval + ")", 0);
                    this.WritePluginConsole("TASK -> restarting server...", 0);
                    this.m_blMapWasRestarted = true;
                    if (this.m_enDoTestdrive == enumBoolYesNo.Yes) {
                        this.WritePluginConsole("TESTDRIVE: restart server command would normally be send", 2);
                    } else {
                        this.ExecuteCommand("procon.protected.send", "admin.shutDown");
                    }
                } else {
                    this.WritePluginConsole("IDLE -> Server Uptime (" + iServerUptime.ToString() + ") not above " + iChkUptimeHours.ToString() + " hours (" 
                        + this.m_iWatchdogInterval + ") or server not empty (" + iPlayerCount.ToString() + ").", 1);
                }
            }
        }
        
        #endregion

        #region helper_functions

        private void WritePluginConsole(string message, int debug)
        {
            string line = String.Format("AutoRoundRestarter: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes || (this.m_enDoConsoleOutput == enumBoolYesNo.Yes && debug == 0) || debug == 2) { 
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

        private void ResetMyVars(int iStatus)
        {
            this.m_blMapWasRestarted = false;
            this.m_blFirstMapWasRestarted = false;

            this.m_iSrvInfoCurPlayerCount = 0;
            this.m_iSrvInfoMaxPlayerCount = 0;
            this.m_iSrvInfoActRoundTime = 0;
            this.m_iSrvInfoCurRound = 0;
            this.m_iSrvInfoTotalRounds = 0;
            this.m_iForwardTasks = 0;
            this.m_iOldIdleTimeout = 300;

            if (iStatus == 0) {
                this.m_iIdleKickChanged = 2;
                if (this.m_enDoTestdrive == enumBoolYesNo.Yes) {
                    this.WritePluginConsole("TESTDRIVE: Normally idleTimeout would be restored.", 2);
                } else {
                    this.WritePluginConsole("TASK: IdleKick -> setting vars.idleTimeout to old value (" + this.m_iOldIdleTimeout.ToString() + ").", 0);
                    this.ExecuteCommand("procon.protected.send", "vars.idleTimeout", this.m_iOldIdleTimeout.ToString());
                }
            }

            this.m_iIdleKickChanged = 0;
        }

#endregion

    }
}
