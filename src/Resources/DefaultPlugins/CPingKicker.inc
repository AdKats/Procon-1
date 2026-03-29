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

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Players;

namespace PRoConEvents {
    public class CPingKicker : PRoConPluginAPI, IPRoConPluginInterface {

        private Dictionary<string, CPunkbusterInfo> m_dicPbInfo = new Dictionary<string, CPunkbusterInfo>();

        private List<string> m_lstAlreadyKickedPlayers;
        private Dictionary<string, int[]> m_dicPlayerPings;
        private int m_iPreviousStampIndex;
        private DateTime m_dtPreviousEntryStamp;

        private enumBoolYesNo m_enYellKicks;
        private enumBoolYesNo m_enSendKicksPrivately;
        private int m_iDelayBetweenMessageAndKick;
        private string m_strKickingMessagePrivate;
        private string m_strKickingMessagePublic;

        private int m_iMaximumAveragePing;
        private int m_iOverPeriodInSeconds;

        private enumBoolYesNo m_enRequestMoreDataFromServer;
        private int m_iGetAdditionalDataSeconds;

        //private enumBoolYesNo m_enKickAccountHolders;
        
        // Whitelist instead of account holders //
        private List<string> pingWhiteList;

        //private enumBoolOnOff m_enRunInDebugMode;

        private bool m_blPluginEnabled = false;

        public CPingKicker() {
            this.m_enYellKicks = enumBoolYesNo.No;
            this.m_enSendKicksPrivately = enumBoolYesNo.Yes;
            this.m_iDelayBetweenMessageAndKick = 10;

            this.m_strKickingMessagePrivate = "Sorry %playername%, your ping is over %maxping%.  Cya!";
            this.m_strKickingMessagePublic = "Kicking %playername% for having an average ping of %averageping% (%playerloc%)";

            this.m_iMaximumAveragePing = 400;
            this.m_iOverPeriodInSeconds = 65;

            this.m_enRequestMoreDataFromServer = enumBoolYesNo.No;
            this.m_iGetAdditionalDataSeconds = 10;

            //this.m_enKickAccountHolders = enumBoolYesNo.No;
            
            this.pingWhiteList = new List<string>();
            this.pingWhiteList.Add("HeliMagnet");

            //this.m_enRunInDebugMode = enumBoolOnOff.On;

            this.m_lstAlreadyKickedPlayers = new List<string>();
            this.m_dicPlayerPings = new Dictionary<string, int[]>();
            this.m_iPreviousStampIndex = 0;
            this.m_dtPreviousEntryStamp = DateTime.Now;
        }

        public string GetPluginName() {
            return "Ping Kicker";
        }

        public string GetPluginVersion() {
            return "2.0.0.0";
        }

        public string GetPluginAuthor() {
            return "Phogue - Modified by [LAB]HeliMagnet";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net";
        }

        public string GetPluginDescription() {
            return @"
<h2>Description</h2>
<p>Kicks a player if their average ping/latency is above 400 (variable) ms over a period of 65 (variable) seconds.</p>

<p>By default Procon will request ping data once every 30 seconds.  This interval is potentially increased with admins connecting to your layer because all those instances of procon will also ask for ping data every 30 seconds as well.  You can set this plugin to request more frequent updates of the ping data and while it has a scary ""Higher Bandwidth"" message it would probably equate to 30 MB's extra per day if your server has a constant 32 players in it and you request ping data at 5 second intervals.  It's peanuts yes, but just keeping you informed is all.</p>

<h2>Settings</h2>
    <h3>Communication</h3>
        <blockquote><h4>Yell kicks</h4>Option between yelling (the big text in the middle of the screen) or saying (chat box in the top left corner)</blockquote>
        <blockquote><h4>Send privately to player</h4>This will send the kick message directly to the player or shout the kick to all players</blockquote>
        <blockquote><h4>Delay between message and kick</h4>Optional delay in telling the player/server the player is about to be kicked and the actual kick</blockquote>
        <blockquote><h4>Private kick message</h4>The message to use when privately telling the player they are about to be kicked as well as the reason displayed in the kick message</blockquote>
        <blockquote><h4>Public kick message</h4>The message to use when shouting a kick message to the whole server</blockquote>
    <h3>Kick Thresholds</h3>
        <blockquote><h4>Kick player if average ping &gt;</h4>The upper limit an average ping may be before a kick</blockquote>
        <blockquote><h4>over a period of (seconds)</h4>How long must the players ping be above the threshold, this accounts for spikes in a players ping</blockquote>
    <h3>Additional updates</h3>
        <blockquote><h4>Request more data from the server</h4>Queues additional requests to the server for more precise averages of pings</blockquote>
        <blockquote><h4>Get additional updates every (seconds)</h4>How often procon should nag the server for additional updates</blockquote>
    <h3>Whitelist options</h3>
        <blockquote><h4>White List</h4>A list of players exempt from being kicked for a high ping</blockquote>

<h2>Settings - Additional Options</h2>
    <h3>""Private kick message"" and ""Public kick message"" context replacements</h3>
        <blockquote><h4>%playername%</h4>The player being kicked</blockquote>
        <blockquote><h4>%playerloc%</h4>The country the player is from (punkbuster must be enabled)</blockquote>
        <blockquote><h4>%averageping%</h4>The ping that broke the camels back</blockquote>
        <blockquote><h4>%maxping%</h4>The maximum ping threshold you have set</blockquote>
        <blockquote><h4>%maxinterval%</h4>The maximum ping threshold seconds</blockquote>

<h2>Additional Information</h2>
    <ul>
        <li>The private kick message is also used as the reason displayed when kicking the player</li>
    </ul>

";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
			this.RegisterEvents(this.GetType().Name, "OnPlayerLeft", "OnPunkbusterPlayerInfo", "OnPlayerKicked", "OnListPlayers");
        }

        public void OnPluginEnable() {
            this.m_blPluginEnabled = true;
            this.UpdateAdditionalRequests();

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPing kicker ^2Enabled!" );
        }

        public void OnPluginDisable() {
            this.m_blPluginEnabled = false;
            this.ExecuteCommand("procon.protected.tasks.remove", "CPingKicker");

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPing kicker ^1Disabled =(");
        }

		// GetDisplayPluginVariables and GetPluginVariables
		#include "CPingKicker.PluginVariables.inc"

        public void SetPluginVariable(string strVariable, string strValue) {
            int iTimeSeconds = 8;

            if (strVariable.CompareTo("Yell kicks") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enYellKicks = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Send privately to player") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enSendKicksPrivately = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Delay between message and kick") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iDelayBetweenMessageAndKick = iTimeSeconds;

                if (iTimeSeconds <= 0) {
                    this.m_iDelayBetweenMessageAndKick = 1;
                }
                else if (iTimeSeconds > 60) {
                    this.m_iDelayBetweenMessageAndKick = 60;
                }
            }
            else if (strVariable.CompareTo("Private kick message") == 0) {
                this.m_strKickingMessagePrivate = strValue;
            }
            else if (strVariable.CompareTo("Public kick message") == 0) {
                this.m_strKickingMessagePublic = strValue;
            }
            else if (strVariable.CompareTo("Kick player if average ping >") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iMaximumAveragePing = iTimeSeconds;

                if (iTimeSeconds <= 50) {
                    this.m_iMaximumAveragePing = 50;
                }
                else if (iTimeSeconds > 1000) {
                    this.m_iMaximumAveragePing = 1000;
                }
            }
            else if (strVariable.CompareTo("over a period of (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iOverPeriodInSeconds = iTimeSeconds;

                if (iTimeSeconds <= 35) {
                    this.m_iOverPeriodInSeconds = 35;
                }
                else if (iTimeSeconds > 600) {
                    this.m_iOverPeriodInSeconds = 600;
                }

                lock (this.m_dicPlayerPings) {
                    this.m_dicPlayerPings.Clear();
                }
            }
            else if (strVariable.CompareTo("Request more data from the server") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enRequestMoreDataFromServer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);

                this.UpdateAdditionalRequests();
            }
            else if (strVariable.CompareTo("Get additional updates every (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iGetAdditionalDataSeconds = iTimeSeconds;

                if (iTimeSeconds <= 5) {
                    this.m_iGetAdditionalDataSeconds = 5;
                }
                else if (iTimeSeconds > 60) {
                    this.m_iGetAdditionalDataSeconds = 60;
                }

                this.UpdateAdditionalRequests();
            }
            //else if (strVariable.CompareTo("Kick account holders") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
            //    this.m_enKickAccountHolders = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            //}
            else if (strVariable.CompareTo("White List") == 0)
            {
				this.pingWhiteList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            //else if (strVariable.CompareTo("Debug mode") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true) {
            //    this.m_enRunInDebugMode = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);
            //}
        }

        private void UpdateAdditionalRequests() {

            this.ExecuteCommand("procon.protected.tasks.remove", "CPingKicker");

            if (this.m_blPluginEnabled == true && this.m_enRequestMoreDataFromServer == enumBoolYesNo.Yes) {
                this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", "0", this.m_iGetAdditionalDataSeconds.ToString(), "-1", "procon.protected.send", "admin.listPlayers", "all");
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
            lock (this.m_dicPlayerPings) {
                if (this.m_dicPlayerPings.ContainsKey(playerInfo.SoldierName) == true) {
                    this.m_dicPlayerPings.Remove(playerInfo.SoldierName);
                }
            }

            this.m_lstAlreadyKickedPlayers.Remove(playerInfo.SoldierName);

            if (this.m_dicPbInfo.ContainsKey(playerInfo.SoldierName) == true) {
                this.m_dicPbInfo.Remove(playerInfo.SoldierName);
            }
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer) {
            if (cpbiPlayer != null) {
                if (this.m_dicPbInfo.ContainsKey(cpbiPlayer.SoldierName) == false) {
                    this.m_dicPbInfo.Add(cpbiPlayer.SoldierName, cpbiPlayer);
                }
                else {
                    this.m_dicPbInfo[cpbiPlayer.SoldierName] = cpbiPlayer;
                }
            }
        }

        // Player Kick/List Events
        public override void OnPlayerKicked(string strSoldierName, string strReason) {

            // Also taken out in OnPlayerLeft, but this is just a double check is all.
            // It wasn't needed, but it would piss me off if the plugin stopped working because of
            // old records about the player not being removed when they left..
            lock (this.m_dicPlayerPings) {
                if (this.m_dicPlayerPings.ContainsKey(strSoldierName) == true) {
                    this.m_dicPlayerPings.Remove(strSoldierName);
                }
            }

            this.m_lstAlreadyKickedPlayers.Remove(strSoldierName);

            if (this.m_dicPbInfo.ContainsKey(strSoldierName) == true) {
                this.m_dicPbInfo.Remove(strSoldierName);
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {

            try {
                if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All) {

                    lock (this.m_dicPlayerPings) {

                        int iNewStampIndexOffset = (int)(DateTime.Now - m_dtPreviousEntryStamp).TotalSeconds;

                        foreach (CPlayerInfo cpiPlayer in lstPlayers) {
                            CPrivileges cpPlayerPrivs = this.GetAccountPrivileges(cpiPlayer.SoldierName);
                            //this.ExecuteCommand("procon.protected.pluginconsole.write", cpiPlayer.SoldierName);

                            // Include them if they don't have an account or if we're kicking everyone (account holders included).
                            //if ((this.m_enKickAccountHolders == enumBoolYesNo.Yes || cpPlayerPrivs == null) && this.m_lstAlreadyKickedPlayers.Contains(cpiPlayer.SoldierName) == false) {
                            if (cpPlayerPrivs == null && this.pingWhiteList.Contains(cpiPlayer.SoldierName) == false && this.m_lstAlreadyKickedPlayers.Contains(cpiPlayer.SoldierName) == false) {

                                if (this.m_dicPlayerPings.ContainsKey(cpiPlayer.SoldierName) == true) {

                                    int iCurrentPlayerStampIndex = this.m_iPreviousStampIndex;
                                    int[] a_iPings = this.m_dicPlayerPings[cpiPlayer.SoldierName];

                                    // Fill in the blanks from the last entry..
                                    for (int i = 0; i < iNewStampIndexOffset; i++) {
                                        a_iPings[iCurrentPlayerStampIndex] = a_iPings[this.m_iPreviousStampIndex];

                                        iCurrentPlayerStampIndex = (iCurrentPlayerStampIndex + 1) % this.m_iOverPeriodInSeconds;
                                    }

                                    a_iPings[iCurrentPlayerStampIndex] = cpiPlayer.Ping;

                                    this.m_dicPlayerPings[cpiPlayer.SoldierName] = a_iPings;
                                }
                                else {
                                    int[] a_iNewPings = new int[this.m_iOverPeriodInSeconds];
                                    for (int i = 0; i < a_iNewPings.Length; i++) a_iNewPings[i] = 0;

                                    a_iNewPings[(this.m_iPreviousStampIndex + iNewStampIndexOffset) % this.m_iOverPeriodInSeconds] = cpiPlayer.Ping;

                                    this.m_dicPlayerPings.Add(cpiPlayer.SoldierName, a_iNewPings);
                                }

                                double dblAveragePing = 0.0D;
                                for (int x = 0; x < this.m_iOverPeriodInSeconds; x++) {
                                    dblAveragePing += (double)this.m_dicPlayerPings[cpiPlayer.SoldierName][(this.m_iPreviousStampIndex + iNewStampIndexOffset + x) % this.m_iOverPeriodInSeconds];
                                }

                                dblAveragePing /= (double)this.m_iOverPeriodInSeconds;

                                if (dblAveragePing >= (double)this.m_iMaximumAveragePing) {
                                    //if (this.m_enRunInDebugMode == enumBoolOnOff.On) {
                                    //    this.ExecuteCommand("procon.protected.pluginconsole.write", String.Format("^bCPingKicker.OnListPlayers Debug (no action taken): Kicking {0} for average ping of {1:0.00} in the previous {2} seconds", cpiPlayer.SoldierName, dblAveragePing, this.m_iOverPeriodInSeconds));
                                    //}
                                    //else {

                                    string strPrivateMessage = this.m_strKickingMessagePrivate;
                                    string strPublicMessage = this.m_strKickingMessagePublic;

                                    strPrivateMessage = strPrivateMessage.Replace("%playername%", cpiPlayer.SoldierName);
                                    strPublicMessage = strPublicMessage.Replace("%playername%", cpiPlayer.SoldierName);

                                    if (this.m_dicPbInfo.ContainsKey(cpiPlayer.SoldierName) == true) {
                                        strPrivateMessage = strPrivateMessage.Replace("%playerloc%", this.m_dicPbInfo[cpiPlayer.SoldierName].PlayerCountry);
                                        strPublicMessage = strPublicMessage.Replace("%playerloc%", this.m_dicPbInfo[cpiPlayer.SoldierName].PlayerCountry);
                                    }
                                    else {
                                        strPrivateMessage = strPrivateMessage.Replace("%playerloc%", String.Empty);
                                        strPublicMessage = strPublicMessage.Replace("%playerloc%", String.Empty);
                                    }

                                    strPrivateMessage = strPrivateMessage.Replace("%averageping%", String.Format("{0:0}", dblAveragePing));
                                    strPublicMessage = strPublicMessage.Replace("%averageping%", String.Format("{0:0}", dblAveragePing));

                                    strPrivateMessage = strPrivateMessage.Replace("%maxping%", this.m_iMaximumAveragePing.ToString());
                                    strPublicMessage = strPublicMessage.Replace("%maxping%", this.m_iMaximumAveragePing.ToString());

                                    strPrivateMessage = strPrivateMessage.Replace("%maxinterval%", this.m_iOverPeriodInSeconds.ToString());
                                    strPublicMessage = strPublicMessage.Replace("%maxinterval%", this.m_iOverPeriodInSeconds.ToString());

                                    if (this.m_enSendKicksPrivately == enumBoolYesNo.Yes) {
                                        if (this.m_enYellKicks == enumBoolYesNo.Yes) {
                                            this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", "0", "1", "1", "procon.protected.send", "admin.yell", strPrivateMessage, "8000", "player", cpiPlayer.SoldierName);
                                        }
                                        else {
                                            this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", "0", "1", "1", "procon.protected.send", "admin.say", strPrivateMessage, "player", cpiPlayer.SoldierName);
                                        }

                                        this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", this.m_iDelayBetweenMessageAndKick.ToString(), "1", "1", "procon.protected.send", "admin.kickPlayer", cpiPlayer.SoldierName, strPrivateMessage);
                                    }
                                    else {
                                        if (this.m_enYellKicks == enumBoolYesNo.Yes) {
                                            this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", "0", "1", "1", "procon.protected.send", "admin.yell", strPublicMessage, "8000", "all");
                                        }
                                        else {
                                            this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", "0", "1", "1", "procon.protected.send", "admin.say", strPublicMessage, "all");
                                        }

                                        this.ExecuteCommand("procon.protected.tasks.add", "CPingKicker", this.m_iDelayBetweenMessageAndKick.ToString(), "1", "1", "procon.protected.send", "admin.kickPlayer", cpiPlayer.SoldierName, strPrivateMessage);
                                    }

                                    this.m_lstAlreadyKickedPlayers.Add(cpiPlayer.SoldierName);
                                    //}

                                }
                            }
                        }

                        this.m_iPreviousStampIndex = (this.m_iPreviousStampIndex + iNewStampIndexOffset) % this.m_iOverPeriodInSeconds;

                        this.m_dtPreviousEntryStamp = DateTime.Now;

                    }
                }
            }
            catch (Exception e) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "CPingKicker.OnListPlayers Exception: " + e.Message);
            }
        }
    }
}