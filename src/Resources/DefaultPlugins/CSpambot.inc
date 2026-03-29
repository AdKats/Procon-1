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
    public class CSpambot : PRoConPluginAPI, IPRoConPluginInterface {

        private List<string> m_lstMessages;
        private int m_iDisplayTime;
        private int m_iIntervalBetweenMessages;

        // Status
        private string m_strServerGameType;
        private string m_strGameMod;
        private string m_strServerVersion;
        private string m_strPRoConVersion;

        private int m_iYellDivider;
        private bool m_blHasYellDuration;

        private bool m_blPluginEnabled = false;
        private bool m_blServerTypeChecked = false;

        private enumBoolYesNo m_enYellResponses;

        public CSpambot() {
            this.m_lstMessages = new List<string>();
            this.m_lstMessages.Add("Example message 1");
            this.m_lstMessages.Add("Example message 2");

            this.m_enYellResponses = enumBoolYesNo.No;

            this.m_iDisplayTime = 8000;
            this.m_iIntervalBetweenMessages = 60;

            this.m_strServerGameType = "none";
            this.m_iYellDivider = 1;
            this.m_blHasYellDuration = true;
        }

        public string GetPluginName() {
            return "Spambot";
        }

        public string GetPluginVersion() {
            return "2.0.1.0";
        }

        public string GetPluginAuthor() {
            return "Phogue";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net";
        }

        public string GetPluginDescription() {
            return @"Spams a message to all the players at a set interval";
        }

        public void OnPluginLoadingEnv(List<string> lstPluginEnv) {
            Version PRoConVersion = new Version(lstPluginEnv[0]);
            this.m_strPRoConVersion = PRoConVersion.ToString();
            this.m_strServerGameType = lstPluginEnv[1].ToLower();
            this.m_strGameMod = lstPluginEnv[2];
            this.m_strServerVersion = lstPluginEnv[3];

            if (this.m_strServerGameType == "bf3" || this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl") {
                this.m_iYellDivider = 1000;
            }
            if (this.m_strServerGameType == "mohw") {
                this.m_blHasYellDuration = false;
            }
        }
        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
			// This is just so procon knows this plugin wants to override the default
			// "fire every event" setting when no events are registered.
			this.RegisterEvents(this.GetType().Name, "OnPluginLoaded");
        }

        public void OnPluginEnable() {
            this.m_blPluginEnabled = true;
            this.UpdateSpamBot();

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bSpambot ^2Enabled!" );
        }

        public void OnPluginDisable() {
            this.m_blPluginEnabled = false;
            this.ExecuteCommand("procon.protected.tasks.remove", "CSpambot");

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bSpambot ^1Disabled =(");
        }

        private void UpdateSpamBot() {

            this.ExecuteCommand("procon.protected.tasks.remove", "CSpambot");

            if (this.m_blPluginEnabled == true) {

                int iDelay = 0;
                foreach (string strMessage in this.m_lstMessages) {
                    if (this.m_enYellResponses == enumBoolYesNo.Yes) {
                        if (this.m_blHasYellDuration == true) {
                            this.ExecuteCommand("procon.protected.tasks.add", "CSpambot", iDelay.ToString(), (this.m_lstMessages.Count * this.m_iIntervalBetweenMessages).ToString(), "-1", "procon.protected.send", "admin.yell", strMessage, (this.m_iDisplayTime / this.m_iYellDivider).ToString(), "all");
                        } else {
                            this.ExecuteCommand("procon.protected.tasks.add", "CSpambot", iDelay.ToString(), (this.m_lstMessages.Count * this.m_iIntervalBetweenMessages).ToString(), "-1", "procon.protected.send", "admin.yell", strMessage, "all");
                        }
                    }
                    else {
                        this.ExecuteCommand("procon.protected.tasks.add", "CSpambot", iDelay.ToString(), (this.m_lstMessages.Count * this.m_iIntervalBetweenMessages).ToString(), "-1", "procon.protected.send", "admin.say", strMessage, "all");
                    }

                    iDelay += this.m_iIntervalBetweenMessages;
                }
            }
        }

		// GetDisplayPluginVariables and GetPluginVariables
		#include "CSpambot.PluginVariables.inc"

        public void SetPluginVariable(string strVariable, string strValue) {
            int iTimeSeconds = 8;

            if (strVariable.CompareTo("Messages") == 0) {
                this.m_lstMessages = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Superspam! (Yell the spam)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enYellResponses = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Interval between messages (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iIntervalBetweenMessages = iTimeSeconds;
            }
            else if (strVariable.CompareTo("Show spam (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true) {
                this.m_iDisplayTime = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0) {
                    this.m_iDisplayTime = 1 * 1000;
                }
                else if (iTimeSeconds > 60) {
                    this.m_iDisplayTime = 59 * 1000;
                }
            }

            this.UpdateSpamBot();
        }
    }
}