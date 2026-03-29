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
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.ComponentModel;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class GoogleTranslatorAPI : PRoConPluginAPI, IPRoConPluginInterface {

        private PlayerInformationDictionary m_dicPlayers;
        
        private enumBoolOnOff m_onOffDebug;
        private enumBoolOnOff m_onOffDefaultStatusOfPlayer;

        private enumBoolYesNo m_yesNoTranslationFromServerToPlayer;
        private enumBoolYesNo m_yesNoTranslationFromPlayerToPlayer;
        private int m_iMinimumTranslationLength;

        public GoogleTranslatorAPI() {
            this.m_dicPlayers = new PlayerInformationDictionary();
            this.m_dicPlayers.SendTranslatedMessage += new PlayerInformationDictionary.SendTranslatedMessageHandler(m_dicPlayers_SendTranslatedMessage);
            this.m_dicPlayers.TranslationError += new PlayerMessage.TranslationStatusHandler(m_dicPlayers_TranslationError);
            this.m_dicPlayers.TranslationProgress += new PlayerMessage.TranslationStatusHandler(m_dicPlayers_TranslationProgress);

            this.m_onOffDebug = enumBoolOnOff.Off;
            this.m_onOffDefaultStatusOfPlayer = enumBoolOnOff.Off;
            this.m_iMinimumTranslationLength = 15;

            this.m_yesNoTranslationFromServerToPlayer = enumBoolYesNo.No;
            this.m_yesNoTranslationFromPlayerToPlayer = enumBoolYesNo.Yes;
        }

        public string GetPluginName() {
            return "Google Translator API";
        }

        public string GetPluginVersion() {
            return "2.0.0.1";
        }

        public string GetPluginAuthor() {
            return "Phogue";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net";
        }

        public string GetPluginDescription() {
            return @"<p>For support regarding this plugin please visit <a href=""http://www.phogue.net"" target=""_blank"">www.phogue.net</a></p>

<p><a href=""http://www.google.com"" target=""_blank""><img src=""http://books.google.com/googlebooks/images/poweredby.png""></a></p>

<h2>Description</h2>
<p>Provides an in game interface to Google's Translator API to translate text for admins/players to another players detected or preferred language.</p>
<p>A German, a Swede and an Englishman walk into a server, the German says ""Dies ist ein grosser nachricht"".  The swede sees ""Google Translator (GermanPlayerName): Detta ar ett stort budskap"" while the englishman sees ""Google Translator (GermanPlayerName): This is a great message"".</p>
<p>It's a bit of fun, I wouldn't host peace negotiations between nuclear nations in case the translator swings and misses.  You should be able to tell if some one is angry at you and hopefully it will convey ""GET OUT OF MAIN BASE OR I SMACK YOU"" =)</p>
    <h2>Requirements</h2>
        <h4>Plugin sandbox mode (Tools -> Options -> Plugins -> Plugin Security)</h4>
            <ul>
                <li>URL: http://ajax.googleapis.com</li>
                <li>Port: 80</li>
            </ul>
            <p>If you have your procon hosted you will need to nag your service provider.</p>

    <h2>Commands</h2>
        <blockquote><h4>@translator</h4>Gives a summary about a players current translation settings.</blockquote>
        <blockquote><h4>@translator [on/off]</h4>A player can enable/disable translation for themselves.</blockquote>
        <blockquote><h4>@translator [language]</h4>A player can set their preferred language.</blockquote>

    <h2>Settings</h2>
        <h3>Miscellaneous</h3>
            <blockquote><h4>Debug mode</h4>Diagnostic option to show the translation process.</blockquote>
        <h3>Player settings</h3>
            <blockquote><h4>Default status of translator</h4>When a player joins should their translator turned On or Off by default?</blockquote>
        <h3>Translation</h3>
            <blockquote><h4>Minimum message length for translation</h4>How long must a message be before we nag Google?  Generally speaking you can tell from the context of a conversation that ""ja"" is german for ""yes"", so don't spam the console with google messages.  Other things like ""lol"" and ""rofl"", along with smiley faces also don't need translation ;)</blockquote>
            <blockquote><h4>Translate messages from server to player</h4>If messages from the server should be translated (can become spammy if using commands like !help or killstreak plugins)</blockquote>
            <blockquote><h4>Translate messages from player to player/server</h4>If players should have whatever they say translated between each other.  This will also display translations of what a player said into the chat window on the layer and any connected clients.</blockquote>

    <h2>Additional Information</h2>
        <ul>
            <li>This plugin is compatible with Basic In-Game Info's @help command</li>
            <li>It is part of the terms of use that you leave the Google Translator prefix within this code</li>
            <li>To test this plugin you must have at least two people in the server who's languages differ (see @translator [language]).  They must then speak in this language for a translation to occur</li>
            <li>The translation will only be displayed to whatever scope it was said in (Only a squad will see the translation if said in squad chat)</li>
            <li>No translations will be posted if google tells procon it's less than 25% sure of the detected language.  Providing longer sentences increases this confidence value, thus default 15 chars</li>
            <li>I've done my best but it may pick an incorrect default language per country.  Let me know if you identify a mismatch in a country and default language</li>
        </ul>
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
		
			this.RegisterEvents(this.GetType().Name, "OnSaying", "OnYelling", "OnPunkbusterPlayerInfo", "OnListPlayers", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnPlayerJoin");
		
            this.m_dicPlayers.UpdatePlayer("Server", null, new CPunkbusterInfo(String.Empty, "Server", String.Empty, strHostName, this.GetVariable<string>("SERVER_COUNTRY", "Australia"), this.GetVariable<string>("SERVER_COUNTRY_CODE", "AU")), false);
            this.m_dicPlayers.PlayerEnableTranslator("Server", true);
        }

        public void OnPluginEnable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGoogle Translator API ^2Enabled!");

            // Register
            this.RegisterCommand(new MatchCommand("GoogleTranslatorAPI", "OnCommandTranslator", this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Displays your translators status and preferred language"));
            this.RegisterCommand(new MatchCommand("GoogleTranslatorAPI", "OnCommandTranslatorOnOff", this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("on/off", this.Listify<string>("on", "off"))), new ExecutionRequirements(ExecutionScope.All), "Enables or disables the Google Translator with your detected language"));
            this.RegisterCommand(new MatchCommand("GoogleTranslatorAPI", "OnCommandTranslatorLanguage", this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("language", new List<string>(this.m_dicPlayers.AcceptableLanguages.Values) )), new ExecutionRequirements(ExecutionScope.All), "Enables the Google Translator with a preferred language"));

            // due to Google Translater v2 is non-free
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGoogle Translator API ^3The free google translator API is no longer available!");
            this.ExecuteCommand("procon.protected.plugins.enable", "GoogleTranslatorAPI", "False");
            this.ExecuteCommand("procon.protected.send", "procon.plugin.enable", "GoogleTranslatorAPI False");
            //this.OnPluginDisable();
        }

        public void OnPluginDisable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGoogle Translator API ^1Disabled =(");

            // Unregister
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>()));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("on/off", null))));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("!", "@", "#"), "translator", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("language", null))));
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Miscellaneous|Debug Mode", typeof(enumBoolOnOff), this.m_onOffDebug));
            lstReturn.Add(new CPluginVariable("Player settings|Default status of translator", typeof(enumBoolOnOff), this.m_onOffDefaultStatusOfPlayer));
            lstReturn.Add(new CPluginVariable("Translation|Minimum message length for translation", typeof(int), this.m_iMinimumTranslationLength));

            lstReturn.Add(new CPluginVariable("Translation|Translate messages from server to player", typeof(enumBoolYesNo), this.m_yesNoTranslationFromServerToPlayer));
            lstReturn.Add(new CPluginVariable("Translation|Translate messages from player to player/server", typeof(enumBoolYesNo), this.m_yesNoTranslationFromPlayerToPlayer));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Debug Mode", typeof(enumBoolOnOff), this.m_onOffDebug));
            lstReturn.Add(new CPluginVariable("Default status of translator", typeof(enumBoolOnOff), this.m_onOffDefaultStatusOfPlayer));
            lstReturn.Add(new CPluginVariable("Minimum message length for translation", typeof(int), this.m_iMinimumTranslationLength));

            lstReturn.Add(new CPluginVariable("Translate messages from server to player", typeof(enumBoolYesNo), this.m_yesNoTranslationFromServerToPlayer));
            lstReturn.Add(new CPluginVariable("Translate messages from player to player/server", typeof(enumBoolYesNo), this.m_yesNoTranslationFromPlayerToPlayer));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) {
            int iValue = 0;
            
            if (strVariable.CompareTo("Debug Mode") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true) {
                this.m_onOffDebug = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);
            }
            else if (strVariable.CompareTo("Default status of translator") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true) {
                this.m_onOffDefaultStatusOfPlayer = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);

                if (this.m_onOffDefaultStatusOfPlayer == enumBoolOnOff.On) {
                    foreach (PlayerInformation player in this.m_dicPlayers) {
                        this.m_dicPlayers.PlayerEnableTranslator(player.SoldierName, true);
                    }
                }

            }
            else if (strVariable.CompareTo("Minimum message length for translation") == 0 && int.TryParse(strValue, out iValue) == true) {
                if (iValue < 1) {
                    this.m_iMinimumTranslationLength = 1;
                }
                else if (iValue > 99) {
                    this.m_iMinimumTranslationLength = 99;
                }
                else {
                    this.m_iMinimumTranslationLength = iValue;
                }
            }
            else if (strVariable.CompareTo("Translate messages from server to player") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_yesNoTranslationFromServerToPlayer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Translate messages from player to player/server") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_yesNoTranslationFromPlayerToPlayer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

        }

        // Player events
        public override void OnPlayerJoin(string strSoldierName) {

            this.m_dicPlayers.UpdatePlayer(strSoldierName, new CPlayerInfo(strSoldierName, "", 0, 24), null, this.m_onOffDefaultStatusOfPlayer == enumBoolOnOff.On);
        }


        public override void OnPlayerLeft(CPlayerInfo playerInfo) {

            this.m_dicPlayers.RemovePlayer(playerInfo.SoldierName);
        }


        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer) {

            if (cpbiPlayer != null) {
                this.m_dicPlayers.UpdatePlayer(cpbiPlayer.SoldierName, null, cpbiPlayer, this.m_onOffDefaultStatusOfPlayer == enumBoolOnOff.On);
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {

            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All) {
                foreach (CPlayerInfo cpiPlayer in lstPlayers) {
                    this.m_dicPlayers.UpdatePlayer(cpiPlayer.SoldierName, cpiPlayer, null, this.m_onOffDefaultStatusOfPlayer == enumBoolOnOff.On);
                }
                
                // Remove stored players not on the server.
                foreach (PlayerInformation storedPlayer in this.m_dicPlayers) {

                    bool isWithinList = false;

                    foreach (CPlayerInfo playerInfo in lstPlayers) {
                        if (String.Compare(storedPlayer.SoldierName, playerInfo.SoldierName) == 0) {
                            isWithinList = true;
                            break;
                        }
                    }

                    if (isWithinList == false) {
                        this.m_dicPlayers.RemovePlayer(storedPlayer.SoldierName);
                    }
                }
            }
        }

        private bool IsProcessable(string speaker, string message) {

            bool isProcessable = true;

            // If it's from the server but we're not processing those messages..
            if (String.Compare(speaker, "Server", true) == 0 && this.m_yesNoTranslationFromServerToPlayer == enumBoolYesNo.No) {
                isProcessable = false;
            }
            // Else if it's not from the server but we're not even processing those messages..
            else if (String.Compare(speaker, "Server", true) != 0 && this.m_yesNoTranslationFromPlayerToPlayer == enumBoolYesNo.No) {
                isProcessable = false;
            }

            // Finally ignore the message no matter the settings if it was sent by this plugin.
            if (this.m_sentMessages.Contains(message) == true) {
                isProcessable = false;
            }

            return isProcessable;
        }

        // Communication Events
        public override void OnYelling(string strMessage, int iMessageDuration, CPlayerSubset cpsSubset) {
            // Capture personal messages to a player
            if (this.IsProcessable("Server", strMessage) == true && strMessage.Length >= this.m_iMinimumTranslationLength && cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.Player) {
                this.m_dicPlayers.PlayerSay("Server", strMessage, cpsSubset);
            }
        }

        public override void OnSaying(string strMessage, CPlayerSubset cpsSubset) {
            // Capture personal messages to a player
            if (this.IsProcessable("Server", strMessage) == true && strMessage.Length >= this.m_iMinimumTranslationLength && cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.Player) {
                this.m_dicPlayers.PlayerSay("Server", strMessage, cpsSubset);
            }
        }

        public override void OnGlobalChat(string strSpeaker, string strMessage) {

            if (this.IsProcessable(strSpeaker, strMessage) == true && strMessage.Length >= this.m_iMinimumTranslationLength) {
                this.m_dicPlayers.PlayerSay(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            }
        }

        public override void OnTeamChat(string strSpeaker, string strMessage, int iTeamID) {
            if (this.IsProcessable(strSpeaker, strMessage) == true && strMessage.Length >= this.m_iMinimumTranslationLength) {
                this.m_dicPlayers.PlayerSay(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Team, iTeamID));
            }
        }

        public override void OnSquadChat(string strSpeaker, string strMessage, int iTeamID, int iSquadID) {
            if (this.IsProcessable(strSpeaker, strMessage) == true && strMessage.Length >= this.m_iMinimumTranslationLength) {
                this.m_dicPlayers.PlayerSay(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Squad, iTeamID, iSquadID));
            }
        }

        private void m_dicPlayers_TranslationError(string message) {
            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", String.Format("^b^1{0}", message));
            }
        }

        private void m_dicPlayers_TranslationProgress(string message) {
            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", String.Format("^b{0}", message.Replace("{", "{{").Replace("}", "}}")));
            }
        }

        private void m_dicPlayers_SendTranslatedMessage(string speaker, string targetName, string translatedText, string detectedLanguageCode, string translatedLanguageCode) {

            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.WriteMessage(speaker, targetName, String.Format("{0} -> {1}: {2}", detectedLanguageCode, translatedLanguageCode, translatedText));
            }
            else {
                if (String.Compare(speaker, targetName) != 0) {
                    this.WriteMessage(speaker, targetName, translatedText);
                }
            }
        }

        #region In Game Commands

        // Store a list sent messages to avoid a loop (translating what this plugin is outputting)
        private Queue<string> m_sentMessages = new Queue<string>();

        private void WriteMessage(string speaker, string targetPlayer, string translatedMessage) {

            string strPrefix = String.Format("Google Translator ({0}) > ", speaker);

            if (String.Compare(targetPlayer, "Server", true) == 0) {
                this.ExecuteCommand("procon.protected.chat.write", String.Format("^b^6{0}{1}", strPrefix, translatedMessage));
            }
            else {

                List<string> wordWrappedLines = this.WordWrap(translatedMessage, 100 - strPrefix.Length);

                foreach (string line in wordWrappedLines) {
                    string formattedLine = String.Format("{0}{1}", strPrefix.Replace("{", "{{").Replace("}", "}}"), line);

                    this.m_sentMessages.Enqueue(formattedLine);
                    this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "player", targetPlayer);
                }

                while (this.m_sentMessages.Count > 25) {
                    this.m_sentMessages.Dequeue();
                }
            }
        }

        public void OnCommandTranslator(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            string playerDetails;

            if (this.m_dicPlayers.Contains(strSpeaker) == true) {
                try {
                    playerDetails = this.m_dicPlayers.PlayerToString(strSpeaker);

                    if (String.IsNullOrEmpty(playerDetails) == false) {
                        playerDetails += String.Format("; Minimum message length: {0} chars", this.m_iMinimumTranslationLength);

                        this.WriteMessage(strSpeaker, strSpeaker, playerDetails);
                    }
                }
                catch (Exception e) {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", e.Message);
                }
            }
            else {
                this.WriteMessage(strSpeaker, strSpeaker, "No information available at this time");
            }
        }

        public void OnCommandTranslatorOnOff(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (String.Compare(capCommand.MatchedArguments[0].Argument, "on", true) == 0) {
                this.m_dicPlayers.PlayerEnableTranslator(strSpeaker, true);

                this.WriteMessage(strSpeaker, strSpeaker, "Enabled");
            }
            else {
                this.m_dicPlayers.PlayerEnableTranslator(strSpeaker, false);

                this.WriteMessage(strSpeaker, strSpeaker, "Disabled");
            }

            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", String.Format("^bGoogleTranslatorAPI > ^n{0} set their translator to {1}", strSpeaker, capCommand.MatchedArguments[0].Argument));
            }
        }

        public void OnCommandTranslatorLanguage(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            foreach(KeyValuePair<string, string> acceptableLanguage in this.m_dicPlayers.AcceptableLanguages) {

                if (String.Compare(acceptableLanguage.Value, capCommand.MatchedArguments[0].Argument, true) == 0) {
                    this.m_dicPlayers.PlayerPreferredLanguage(strSpeaker, acceptableLanguage.Key);
                    this.m_dicPlayers.PlayerEnableTranslator(strSpeaker, true);

                    this.WriteMessage(strSpeaker, strSpeaker, String.Format("Setting your preferred language to {0}", acceptableLanguage.Value));

                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", String.Format("^bGoogleTranslatorAPI > ^n{0} set their language to {1}", strSpeaker, capCommand.MatchedArguments[0].Argument));
                    }

                    break;
                }
            }
        }
        
        #endregion

        #region internal player information

        internal class PlayerInformationDictionary : KeyedCollection<string, PlayerInformation> {

            public delegate void SendTranslatedMessageHandler(string speaker, string targetName, string translatedText, string detectedLanguageCode, string translatedLanguageCode);
            public event SendTranslatedMessageHandler SendTranslatedMessage;

            public event PlayerMessage.TranslationStatusHandler TranslationError;
            public event PlayerMessage.TranslationStatusHandler TranslationProgress;

            private Dictionary<string, string> m_dicCountryToOfficialLanguage;
            private Dictionary<string, string> m_acceptableLanguages;
          
            public Dictionary<string, string> AcceptableLanguages {
                get {
                    return this.m_acceptableLanguages;
                }
            }

            public PlayerInformationDictionary() {

                this.m_dicCountryToOfficialLanguage = new Dictionary<string, string>();

                /*
                 * This list is compiled by Geoff 'Phogue' Green from data found
                 * on the CIA website regarding most common or official language
                 * of a country.  It's not reliable and may contain errors.
                 * 
                 * ISO 3166-1 alpha-2 to ISO 639-1 code
                 */
                this.m_dicCountryToOfficialLanguage.Add("AL", "el");
                this.m_dicCountryToOfficialLanguage.Add("DZ", "ar");
                this.m_dicCountryToOfficialLanguage.Add("AD", "ca");
                this.m_dicCountryToOfficialLanguage.Add("AO", "pt");
                this.m_dicCountryToOfficialLanguage.Add("AI", "en");
                this.m_dicCountryToOfficialLanguage.Add("AG", "en");
                this.m_dicCountryToOfficialLanguage.Add("AR", "es");
                this.m_dicCountryToOfficialLanguage.Add("AM", "hy");
                this.m_dicCountryToOfficialLanguage.Add("AU", "en");
                this.m_dicCountryToOfficialLanguage.Add("AT", "de");
                this.m_dicCountryToOfficialLanguage.Add("AZ", "az");
                this.m_dicCountryToOfficialLanguage.Add("BH", "ar");
                this.m_dicCountryToOfficialLanguage.Add("BB", "en");
                this.m_dicCountryToOfficialLanguage.Add("BY", "be");
                this.m_dicCountryToOfficialLanguage.Add("BE", "nl");
                this.m_dicCountryToOfficialLanguage.Add("BZ", "es");
                this.m_dicCountryToOfficialLanguage.Add("BJ", "fr");
                this.m_dicCountryToOfficialLanguage.Add("BM", "en");
                this.m_dicCountryToOfficialLanguage.Add("BO", "es");
                this.m_dicCountryToOfficialLanguage.Add("BA", "hr");
                this.m_dicCountryToOfficialLanguage.Add("BR", "de");
                this.m_dicCountryToOfficialLanguage.Add("BG", "bg");
                this.m_dicCountryToOfficialLanguage.Add("BF", "fr");
                this.m_dicCountryToOfficialLanguage.Add("BI", "fr");
                this.m_dicCountryToOfficialLanguage.Add("KH", "km");
                this.m_dicCountryToOfficialLanguage.Add("CM", "en");
                this.m_dicCountryToOfficialLanguage.Add("CA", "en");
                this.m_dicCountryToOfficialLanguage.Add("CV", "pt");
                this.m_dicCountryToOfficialLanguage.Add("KY", "en");
                this.m_dicCountryToOfficialLanguage.Add("CF", "fr");
                this.m_dicCountryToOfficialLanguage.Add("TD", "fr");
                this.m_dicCountryToOfficialLanguage.Add("CL", "es");
                this.m_dicCountryToOfficialLanguage.Add("CX", "en");
                this.m_dicCountryToOfficialLanguage.Add("CO", "es");
                this.m_dicCountryToOfficialLanguage.Add("KM", "ar");
                this.m_dicCountryToOfficialLanguage.Add("CK", "en");
                this.m_dicCountryToOfficialLanguage.Add("CR", "es");
                this.m_dicCountryToOfficialLanguage.Add("CI", "fr");
                this.m_dicCountryToOfficialLanguage.Add("HR", "hr");
                this.m_dicCountryToOfficialLanguage.Add("CU", "es");
                this.m_dicCountryToOfficialLanguage.Add("CY", "el");
                this.m_dicCountryToOfficialLanguage.Add("CZ", "cs");
                this.m_dicCountryToOfficialLanguage.Add("DK", "da");
                this.m_dicCountryToOfficialLanguage.Add("DJ", "fr");
                this.m_dicCountryToOfficialLanguage.Add("DM", "en");
                this.m_dicCountryToOfficialLanguage.Add("DO", "es");
                this.m_dicCountryToOfficialLanguage.Add("EC", "es");
                this.m_dicCountryToOfficialLanguage.Add("EG", "ar");
                this.m_dicCountryToOfficialLanguage.Add("SV", "es");
                this.m_dicCountryToOfficialLanguage.Add("GQ", "es");
                this.m_dicCountryToOfficialLanguage.Add("ER", "ar");
                this.m_dicCountryToOfficialLanguage.Add("EE", "et");
                this.m_dicCountryToOfficialLanguage.Add("FJ", "en");
                this.m_dicCountryToOfficialLanguage.Add("FI", "fi");
                this.m_dicCountryToOfficialLanguage.Add("FR", "fr");
                this.m_dicCountryToOfficialLanguage.Add("PF", "fr");
                this.m_dicCountryToOfficialLanguage.Add("GA", "fr");
                this.m_dicCountryToOfficialLanguage.Add("GE", "ka");
                this.m_dicCountryToOfficialLanguage.Add("DE", "de");
                this.m_dicCountryToOfficialLanguage.Add("GI", "es");
                this.m_dicCountryToOfficialLanguage.Add("GR", "el");
                this.m_dicCountryToOfficialLanguage.Add("GL", "da");
                this.m_dicCountryToOfficialLanguage.Add("GD", "en");
                this.m_dicCountryToOfficialLanguage.Add("GU", "en");
                this.m_dicCountryToOfficialLanguage.Add("GT", "es");
                this.m_dicCountryToOfficialLanguage.Add("GG", "en");
                this.m_dicCountryToOfficialLanguage.Add("GN", "fr");
                this.m_dicCountryToOfficialLanguage.Add("GW", "pt");
                this.m_dicCountryToOfficialLanguage.Add("GY", "en");
                this.m_dicCountryToOfficialLanguage.Add("HT", "fr");
                this.m_dicCountryToOfficialLanguage.Add("HN", "es");
                this.m_dicCountryToOfficialLanguage.Add("HK", "zh");
                this.m_dicCountryToOfficialLanguage.Add("HU", "hu");
                this.m_dicCountryToOfficialLanguage.Add("IS", "is");
                this.m_dicCountryToOfficialLanguage.Add("IN", "hi");
                this.m_dicCountryToOfficialLanguage.Add("ID", "en");
                this.m_dicCountryToOfficialLanguage.Add("IQ", "ar");
                this.m_dicCountryToOfficialLanguage.Add("IE", "en");
                this.m_dicCountryToOfficialLanguage.Add("IM", "en");
                this.m_dicCountryToOfficialLanguage.Add("IL", "iw");
                this.m_dicCountryToOfficialLanguage.Add("IT", "it");
                this.m_dicCountryToOfficialLanguage.Add("JM", "en");
                this.m_dicCountryToOfficialLanguage.Add("JP", "ja");
                this.m_dicCountryToOfficialLanguage.Add("JE", "en");
                this.m_dicCountryToOfficialLanguage.Add("JO", "ar");
                this.m_dicCountryToOfficialLanguage.Add("KE", "en");
                this.m_dicCountryToOfficialLanguage.Add("KI", "en");
                this.m_dicCountryToOfficialLanguage.Add("KW", "ar");
                this.m_dicCountryToOfficialLanguage.Add("KG", "ky");
                this.m_dicCountryToOfficialLanguage.Add("LV", "lv");
                this.m_dicCountryToOfficialLanguage.Add("LB", "ar");
                this.m_dicCountryToOfficialLanguage.Add("LS", "en");
                this.m_dicCountryToOfficialLanguage.Add("LR", "en");
                this.m_dicCountryToOfficialLanguage.Add("LI", "de");
                this.m_dicCountryToOfficialLanguage.Add("LT", "lt");
                this.m_dicCountryToOfficialLanguage.Add("MK", "mk");
                this.m_dicCountryToOfficialLanguage.Add("MG", "en");
                this.m_dicCountryToOfficialLanguage.Add("MY", "en");
                this.m_dicCountryToOfficialLanguage.Add("ML", "fr");
                this.m_dicCountryToOfficialLanguage.Add("MT", "mt");
                this.m_dicCountryToOfficialLanguage.Add("MR", "fr");
                this.m_dicCountryToOfficialLanguage.Add("MC", "fr");
                this.m_dicCountryToOfficialLanguage.Add("ME", "sr");
                this.m_dicCountryToOfficialLanguage.Add("MS", "en");
                this.m_dicCountryToOfficialLanguage.Add("MA", "ar");
                this.m_dicCountryToOfficialLanguage.Add("NA", "en");
                this.m_dicCountryToOfficialLanguage.Add("NP", "ne");
                this.m_dicCountryToOfficialLanguage.Add("NL", "nl");
                this.m_dicCountryToOfficialLanguage.Add("NC", "fr");
                this.m_dicCountryToOfficialLanguage.Add("NZ", "en");
                this.m_dicCountryToOfficialLanguage.Add("NI", "es");
                this.m_dicCountryToOfficialLanguage.Add("NE", "fr");
                this.m_dicCountryToOfficialLanguage.Add("NG", "en");
                this.m_dicCountryToOfficialLanguage.Add("NF", "en");
                this.m_dicCountryToOfficialLanguage.Add("OM", "ar");
                this.m_dicCountryToOfficialLanguage.Add("PK", "pa");
                this.m_dicCountryToOfficialLanguage.Add("PW", "ja");
                this.m_dicCountryToOfficialLanguage.Add("PA", "es");
                this.m_dicCountryToOfficialLanguage.Add("PG", "en");
                this.m_dicCountryToOfficialLanguage.Add("PY", "es");
                this.m_dicCountryToOfficialLanguage.Add("PE", "es");
                this.m_dicCountryToOfficialLanguage.Add("PN", "en");
                this.m_dicCountryToOfficialLanguage.Add("PL", "pl");
                this.m_dicCountryToOfficialLanguage.Add("PT", "pt");
                this.m_dicCountryToOfficialLanguage.Add("PR", "es");
                this.m_dicCountryToOfficialLanguage.Add("QA", "ar");
                this.m_dicCountryToOfficialLanguage.Add("RO", "ro");
                this.m_dicCountryToOfficialLanguage.Add("RW", "fr");
                this.m_dicCountryToOfficialLanguage.Add("BL", "fr");
                this.m_dicCountryToOfficialLanguage.Add("KN", "en");
                this.m_dicCountryToOfficialLanguage.Add("LC", "en");
                this.m_dicCountryToOfficialLanguage.Add("MF", "en");
                this.m_dicCountryToOfficialLanguage.Add("PM", "fr");
                this.m_dicCountryToOfficialLanguage.Add("VC", "en");
                this.m_dicCountryToOfficialLanguage.Add("SM", "it");
                this.m_dicCountryToOfficialLanguage.Add("ST", "pt");
                this.m_dicCountryToOfficialLanguage.Add("SA", "ar");
                this.m_dicCountryToOfficialLanguage.Add("SN", "fr");
                this.m_dicCountryToOfficialLanguage.Add("RS", "sr");
                this.m_dicCountryToOfficialLanguage.Add("SK", "sk");
                this.m_dicCountryToOfficialLanguage.Add("SI", "sl");
                this.m_dicCountryToOfficialLanguage.Add("SO", "ar");
                this.m_dicCountryToOfficialLanguage.Add("SD", "ar");
                this.m_dicCountryToOfficialLanguage.Add("SR", "nl");
                this.m_dicCountryToOfficialLanguage.Add("SE", "sv");
                this.m_dicCountryToOfficialLanguage.Add("CH", "de");
                this.m_dicCountryToOfficialLanguage.Add("TJ", "tg");
                this.m_dicCountryToOfficialLanguage.Add("TH", "th");
                this.m_dicCountryToOfficialLanguage.Add("TL", "pt");
                this.m_dicCountryToOfficialLanguage.Add("TT", "en");
                this.m_dicCountryToOfficialLanguage.Add("TN", "fr");
                this.m_dicCountryToOfficialLanguage.Add("TR", "tr");
                this.m_dicCountryToOfficialLanguage.Add("TC", "en");
                this.m_dicCountryToOfficialLanguage.Add("TV", "en");
                this.m_dicCountryToOfficialLanguage.Add("UG", "sw");
                this.m_dicCountryToOfficialLanguage.Add("UA", "uk");
                this.m_dicCountryToOfficialLanguage.Add("AE", "ar");
                this.m_dicCountryToOfficialLanguage.Add("GB", "en");
                this.m_dicCountryToOfficialLanguage.Add("US", "en");
                this.m_dicCountryToOfficialLanguage.Add("UY", "es");
                this.m_dicCountryToOfficialLanguage.Add("UZ", "uz");
                this.m_dicCountryToOfficialLanguage.Add("VE", "es");
                this.m_dicCountryToOfficialLanguage.Add("VN", "vi");
                this.m_dicCountryToOfficialLanguage.Add("YE", "ar");
                this.m_dicCountryToOfficialLanguage.Add("ZW", "en");

                this.m_acceptableLanguages = new Dictionary<string, string>();
                this.m_acceptableLanguages.Add("af", "Afrikaans");
                this.m_acceptableLanguages.Add("sq", "Albanian");
                this.m_acceptableLanguages.Add("am", "Amharic");
                //this.m_acceptableLanguages.Add("ar", "Arabic");
                //this.m_acceptableLanguages.Add("hy", "Armenian");
                this.m_acceptableLanguages.Add("az", "Azerbaijani");
                this.m_acceptableLanguages.Add("eu", "Basque");
                //this.m_acceptableLanguages.Add("be", "Belarusian");
                this.m_acceptableLanguages.Add("bn", "Bengali");
                this.m_acceptableLanguages.Add("bh", "Bihari");
                this.m_acceptableLanguages.Add("br", "Breton");
                //this.m_acceptableLanguages.Add("bg", "Bulgarian");
                //this.m_acceptableLanguages.Add("my", "Burmese");
                this.m_acceptableLanguages.Add("ca", "Catalan");
                this.m_acceptableLanguages.Add("chr", "Cherokee");
                //this.m_acceptableLanguages.Add("zh", "Chinese");
                //this.m_acceptableLanguages.Add("zh-CN", "Chinese_Simplified");
                //this.m_acceptableLanguages.Add("zh-TW", "Chinese_Traditional");
                //this.m_acceptableLanguages.Add("co", "Corsican");
                this.m_acceptableLanguages.Add("hr", "Croatian");
                this.m_acceptableLanguages.Add("cs", "Czech");
                this.m_acceptableLanguages.Add("da", "Danish");
                this.m_acceptableLanguages.Add("dv", "Dhivehi");
                this.m_acceptableLanguages.Add("nl", "Dutch");
                this.m_acceptableLanguages.Add("en", "English");
                this.m_acceptableLanguages.Add("eo", "Esperanto");
                this.m_acceptableLanguages.Add("et", "Estonian");
                this.m_acceptableLanguages.Add("fo", "Faroese");
                this.m_acceptableLanguages.Add("tl", "Filipino");
                this.m_acceptableLanguages.Add("fi", "Finnish");
                this.m_acceptableLanguages.Add("fr", "French");
                this.m_acceptableLanguages.Add("fy", "Frisian");
                this.m_acceptableLanguages.Add("gl", "Galician");
                //this.m_acceptableLanguages.Add("ka", "Georgian");
                this.m_acceptableLanguages.Add("de", "German");
                this.m_acceptableLanguages.Add("el", "Greek");
                this.m_acceptableLanguages.Add("gu", "Gujarati");
                this.m_acceptableLanguages.Add("ht", "Haitian_Creole");
                //this.m_acceptableLanguages.Add("iw", "Hebrew");
                //this.m_acceptableLanguages.Add("hi", "Hindi");
                //this.m_acceptableLanguages.Add("hu", "Hungarian");
                this.m_acceptableLanguages.Add("is", "Icelandic");
                this.m_acceptableLanguages.Add("id", "Indonesian");
                this.m_acceptableLanguages.Add("iu", "Inuktitut");
                this.m_acceptableLanguages.Add("ga", "Irish");
                this.m_acceptableLanguages.Add("it", "Italian");
                //this.m_acceptableLanguages.Add("ja", "Japanese");
                this.m_acceptableLanguages.Add("jw", "Javanese");
                this.m_acceptableLanguages.Add("kn", "Kannada");
                this.m_acceptableLanguages.Add("kk", "Kazakh");
                this.m_acceptableLanguages.Add("km", "Khmer");
                //this.m_acceptableLanguages.Add("ko", "Korean");
                this.m_acceptableLanguages.Add("ku", "Kurdish");
                this.m_acceptableLanguages.Add("ky", "Kyrgyz");
                this.m_acceptableLanguages.Add("lo", "Lao");
                this.m_acceptableLanguages.Add("la", "Latin");
                this.m_acceptableLanguages.Add("lv", "Latvian");
                this.m_acceptableLanguages.Add("lt", "Lithuanian");
                this.m_acceptableLanguages.Add("lb", "Luxembourgish");
                //this.m_acceptableLanguages.Add("mk", "Macedonian");
                this.m_acceptableLanguages.Add("ms", "Malay");
                this.m_acceptableLanguages.Add("ml", "Malayalam");
                this.m_acceptableLanguages.Add("mt", "Maltese");
                this.m_acceptableLanguages.Add("mi", "Maori");
                this.m_acceptableLanguages.Add("mr", "Marathi");
                this.m_acceptableLanguages.Add("mn", "Mongolian");
                this.m_acceptableLanguages.Add("ne", "Nepali");
                this.m_acceptableLanguages.Add("no", "Norwegian");
                this.m_acceptableLanguages.Add("oc", "Occitan");
                this.m_acceptableLanguages.Add("or", "Oriya");
                this.m_acceptableLanguages.Add("ps", "Pashto");
                this.m_acceptableLanguages.Add("fa", "Persian");
                this.m_acceptableLanguages.Add("pl", "Polish");
                this.m_acceptableLanguages.Add("pt", "Portuguese");
                this.m_acceptableLanguages.Add("pt-PT", "Portuguese_Portugal");
                this.m_acceptableLanguages.Add("pa", "Punjabi");
                this.m_acceptableLanguages.Add("qu", "Quechua");
                this.m_acceptableLanguages.Add("ro", "Romanian");
                //this.m_acceptableLanguages.Add("ru", "Russian");
                this.m_acceptableLanguages.Add("sa", "Sanskrit");
                this.m_acceptableLanguages.Add("gd", "Scots_Gaelic");
                this.m_acceptableLanguages.Add("sr", "Serbian");
                this.m_acceptableLanguages.Add("sd", "Sindhi");
                this.m_acceptableLanguages.Add("si", "Sinhalese");
                this.m_acceptableLanguages.Add("sk", "Slovak");
                this.m_acceptableLanguages.Add("sl", "Slovenian");
                this.m_acceptableLanguages.Add("es", "Spanish");
                this.m_acceptableLanguages.Add("su", "Sundanese");
                this.m_acceptableLanguages.Add("sw", "Swahili");
                this.m_acceptableLanguages.Add("sv", "Swedish");
                this.m_acceptableLanguages.Add("syr", "Syriac");
                this.m_acceptableLanguages.Add("tg", "Tajik");
                this.m_acceptableLanguages.Add("ta", "Tamil");
                this.m_acceptableLanguages.Add("tt", "Tatar");
                this.m_acceptableLanguages.Add("te", "Telugu");
                //this.m_acceptableLanguages.Add("th", "Thai");
                this.m_acceptableLanguages.Add("bo", "Tibetan");
                this.m_acceptableLanguages.Add("to", "Tonga");
                this.m_acceptableLanguages.Add("tr", "Turkish");
                //this.m_acceptableLanguages.Add("uk", "Ukrainian");
                //this.m_acceptableLanguages.Add("ur", "Urdu");
                this.m_acceptableLanguages.Add("uz", "Uzbek");
                this.m_acceptableLanguages.Add("ug", "Uighur");
                this.m_acceptableLanguages.Add("vi", "Vietnamese");
                this.m_acceptableLanguages.Add("cy", "Welsh");
                //this.m_acceptableLanguages.Add("yi", "Yiddish");
                this.m_acceptableLanguages.Add("yo", "Yoruba");


            }

            protected override string GetKeyForItem(PlayerInformation item) {
                return item.SoldierName;
            }

            public void UpdatePlayer(string soldierName, CPlayerInfo vanillaInfo, CPunkbusterInfo punkbusterInfo, bool isDefaultTranslatorEnabled) {

                if (this.Contains(soldierName) == true) {
                    if (vanillaInfo != null) {
                        this[soldierName].VanillaInfo = vanillaInfo;
                    }

                    if (punkbusterInfo != null) {
                        this[soldierName].PunkbusterInfo = punkbusterInfo;
                    }
                }
                else {
                    this.Add(new PlayerInformation(vanillaInfo, punkbusterInfo, isDefaultTranslatorEnabled));
                }
            }

            public void RemovePlayer(string soldierName) {
                if (String.Compare(soldierName, "Server", true) != 0 && this.Contains(soldierName) == true) {
                    this.Remove(soldierName);
                }
            }

            public void PlayerPreferredLanguage(string soldierName, string languageCode) {
                if (this.Contains(soldierName) == true) {
                    this[soldierName].PreferredLanguageCode = languageCode;
                }
            }

            public void PlayerEnableTranslator(string soldierName, bool isEnabled) {
                if (this.Contains(soldierName) == true) {
                    this[soldierName].IsTranslatorEnabled = isEnabled;
                }
            }

            private string GetPlayerLanguageCode(string soldierName) {
                string languageCode = String.Empty;

                if (this.Contains(soldierName) == true) {

                    if (this[soldierName].PreferredLanguageCode.Length > 0) {
                        languageCode = this[soldierName].PreferredLanguageCode;
                    }
                    else if (this[soldierName].PunkbusterInfo != null && this.m_dicCountryToOfficialLanguage.ContainsKey(this[soldierName].PunkbusterInfo.PlayerCountryCode.ToUpper()) == true) {
                        languageCode = this.m_dicCountryToOfficialLanguage[this[soldierName].PunkbusterInfo.PlayerCountryCode.ToUpper()];
                    }
                }

                return languageCode;
            }

            public string PlayerToString(string soldierName) {
                string text = String.Empty;
                string languageCode = String.Empty;

                if (this.Contains(soldierName) == true) {
                    text = String.Format("Status: {0}", this[soldierName].IsTranslatorEnabled == true ? "Enabled" : "Disabled");

                    languageCode = this.GetPlayerLanguageCode(soldierName);

                    if (languageCode.Length > 0 && this.m_acceptableLanguages.ContainsKey(languageCode)) {
                        //text += String.Format("; Language: {0}", languageCode);
                        text += String.Format("; Language: {0}", this.m_acceptableLanguages[languageCode]);
                    }
                }

                return text;
            }

            private bool IsDetectionRequired(PlayerMessage speakersMessage) {

                bool isDetectionRequired = false;
                string speakerLanguageCode = this.GetPlayerLanguageCode(speakersMessage.SoldierName);

                if (this.TranslationProgress != null) {
                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerInformationDictionary.IsDetectionRequired: Checking if detection is required; Speaker: {0}; LanguageCode: {1}", speakersMessage.SoldierName, speakerLanguageCode));
                }

                foreach (PlayerInformation player in this) {

                    // If the message author is not this player AND (the player is in the same subset as  the message OR the player is the server)
                    if (String.Compare(speakersMessage.SoldierName, player.SoldierName, true) != 0 && (speakersMessage.IsWithinSubset(player) == true || String.Compare(player.SoldierName, "Server", true) == 0)) {

                        if (player.IsTranslatorEnabled == true) {
                            string languageCode = this.GetPlayerLanguageCode(player.SoldierName);

                            // If the speaker and recepient have different set language codes
                            if (String.Compare(speakerLanguageCode, languageCode, true) != 0) {

                                if (this.TranslationProgress != null) {
                                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerInformationDictionary.IsDetectionRequired: Encountered {0} who needs translation to {1}", player.SoldierName, languageCode));
                                }

                                isDetectionRequired = true;
                                break;
                            }
                        }
                    }
                }

                if (this.TranslationProgress != null) {
                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerInformationDictionary.IsDetectionRequired: isDetectionRequired = {0}", isDetectionRequired));
                }

                return isDetectionRequired;
            }

            private string SplitIP(string ip) {

                string returnIp = ip;
                string[] a_splitIp = null;

                if ((a_splitIp = ip.Split(':')) != null && a_splitIp.Length >= 1) {
                    returnIp = a_splitIp[0];
                }

                return returnIp;
            }

            public void PlayerSay(string soldierName, string playerText, CPlayerSubset subset) {
                
                if (String.Compare(playerText, "Please even the teams. Auto-balancing will commence shortly.", true) != 0 &&
                    String.Compare(playerText, "Scoring disabled due to too few players", true) != 0) {

                    PlayerMessage newMessage = new PlayerMessage(soldierName, this.GetPlayerLanguageCode(soldierName), playerText, subset);

                    if (this.IsDetectionRequired(newMessage) == true) {

                        if (this.Contains(soldierName) == true && this[soldierName].PunkbusterInfo != null && this[soldierName].PunkbusterInfo.Ip.Length > 0) {
                            newMessage.LanguageDetected += new PlayerMessage.LanguageHandler(newMessage_LanguageDetected);
                            newMessage.MessageTranslated += new PlayerMessage.LanguageHandler(newMessage_MessageTranslated);
                            newMessage.TranslationError += new PlayerMessage.TranslationStatusHandler(newMessage_TranslationError);
                            newMessage.TranslationProgress += new PlayerMessage.TranslationStatusHandler(newMessage_TranslationProgress);
                            newMessage.DetectLanguage(this.SplitIP(this[soldierName].PunkbusterInfo.Ip));
                        }
                    }
                }

            }

            void newMessage_TranslationProgress(string message) {
                if (this.TranslationProgress != null) {
                    this.TranslationProgress(message);
                }
            }

            private void newMessage_TranslationError(string message) {
                if (this.TranslationError != null) {
                    this.TranslationError(message);
                }
            }

            private void newMessage_MessageTranslated(PlayerMessage sender) {
                try {
                    foreach (PlayerInformation player in this) {

                        if (sender.IsWithinSubset(player) == true && player.PunkbusterInfo != null) {

                            if (this.m_dicCountryToOfficialLanguage.ContainsKey(player.PunkbusterInfo.PlayerCountryCode.ToUpper()) == true) {
                                string playersLanguageCode = this.m_dicCountryToOfficialLanguage[player.PunkbusterInfo.PlayerCountryCode.ToUpper()];

                                if (player.PreferredLanguageCode.Length > 0) {
                                    playersLanguageCode = player.PreferredLanguageCode;
                                }

                                if (sender.Translations.ContainsKey(playersLanguageCode) == true) {
                                    if (this.SendTranslatedMessage != null) {
                                        this.SendTranslatedMessage(sender.SoldierName, player.SoldierName, sender.Translations[playersLanguageCode], sender.DetectedLanguageCode, playersLanguageCode);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            private void newMessage_LanguageDetected(PlayerMessage sender) {

                List<string> translationLanguageCodes = new List<string>();

                try {
                    foreach (PlayerInformation player in this) {

                        string playersLanguageCode = this.GetPlayerLanguageCode(player.SoldierName);

                        // if the languages differ..
                        if (String.Compare(sender.DetectedLanguageCode, playersLanguageCode, true) != 0) {
                            if (String.IsNullOrEmpty(playersLanguageCode) == false && translationLanguageCodes.Contains(playersLanguageCode) == false) {
                                
                                // is it a valid translation to google?
                                // Removes translation requests for russian/chinese etc, fonts that cannot be displayed in game.
                                if (this.m_acceptableLanguages.ContainsKey(playersLanguageCode) == true) {
                                    translationLanguageCodes.Add(playersLanguageCode);
                                }
                            }
                        }

                        /*
                        if (this.m_dicCountryToOfficialLanguage.ContainsKey(player.PunkbusterInfo.PlayerCountryCode.ToUpper()) == true) {

                            


                            string playersLanguageCode = this.m_dicCountryToOfficialLanguage[player.PunkbusterInfo.PlayerCountryCode.ToUpper()];

                            // en != en && en != de
                            if (String.Compare(sender.DetectedLanguageCode, playersLanguageCode, true) != 0 || (player.PreferredLanguageCode.Length > 0 && String.Compare(sender.DetectedLanguageCode, player.PreferredLanguageCode, true) != 0)) {

                                if (player.PreferredLanguageCode.Length > 0) {
                                    if (translationLanguageCodes.Contains(player.PreferredLanguageCode) == false && this.m_acceptableLanguages.ContainsKey(player.PreferredLanguageCode) == true) {
                                        translationLanguageCodes.Add(player.PreferredLanguageCode);
                                    }
                                }
                                else {
                                    if (translationLanguageCodes.Contains(playersLanguageCode) == false && this.m_acceptableLanguages.ContainsKey(playersLanguageCode) == true) {
                                        translationLanguageCodes.Add(playersLanguageCode);
                                    }
                                }
                            }
                        }
                        */
                    }

                    if (this.Contains(sender.SoldierName) == true && this[sender.SoldierName].PunkbusterInfo != null && this[sender.SoldierName].PunkbusterInfo.Ip.Length > 0) {
                        sender.Translate(this.SplitIP(this[sender.SoldierName].PunkbusterInfo.Ip), translationLanguageCodes);
                    }
                }
                catch (Exception) { }
            }
        }
        
        internal class PlayerMessage {

            public delegate void LanguageHandler(PlayerMessage sender);
            public event LanguageHandler LanguageDetected;
            public event LanguageHandler MessageTranslated;

            public delegate void TranslationStatusHandler(string message);
            public event TranslationStatusHandler TranslationError;
            public event TranslationStatusHandler TranslationProgress;

            private Hashtable m_languageDiacriticSubstitutes;

            private CDownloadFile m_detectLanguage;
            private CDownloadFile m_translator;

            private string m_soldierName;
            public string SoldierName {
                get {
                    return this.m_soldierName;
                }
                set {
                    this.m_soldierName = value;
                }
            }

            private string m_soldiersLanguage;
            public string SoldiersLanguage {
                get {
                    return this.m_soldiersLanguage;
                }
                set {
                    this.m_soldiersLanguage = value;
                }
            }

            private string m_message;
            public string Message {
                get {
                    return this.m_message;
                }
                set {
                    this.m_message = value;
                }
            }

            private string m_detectedLanguageCode;
            public string DetectedLanguageCode {
                get {
                    return this.m_detectedLanguageCode;
                }
                set {
                    this.m_detectedLanguageCode = value;
                }
            }

            private CPlayerSubset m_audienceSubset;
            public CPlayerSubset AudienceSubset {
                get {
                    return this.m_audienceSubset;
                }
                private set {
                    this.m_audienceSubset = value;
                }
            }

            private List<string> m_translationCodes;

            private Dictionary<string, string> m_translations;
            public Dictionary<string, string> Translations {
                get {
                    return this.m_translations;
                }
            }

            public PlayerMessage(string soldierName, string soldiersLanguage, string message, CPlayerSubset subset) {
                this.SoldierName = soldierName;
                this.SoldiersLanguage = soldiersLanguage;
                this.Message = message;
                this.AudienceSubset = subset;

                this.m_translations = new Dictionary<string, string>();

                this.m_languageDiacriticSubstitutes = new Hashtable();

                // German
                Hashtable deDiacriticSubstitutes = new Hashtable();
                deDiacriticSubstitutes.Add("Ö", "OE");
                deDiacriticSubstitutes.Add("ö", "oe");
                deDiacriticSubstitutes.Add("Ä", "AE");
                deDiacriticSubstitutes.Add("ä", "ae");
                deDiacriticSubstitutes.Add("Ü", "UE");
                deDiacriticSubstitutes.Add("ü", "ue");
                deDiacriticSubstitutes.Add("ß", "ss");
                this.m_languageDiacriticSubstitutes.Add("de", deDiacriticSubstitutes);

                // Norwegian
                Hashtable noDiacriticSubstitutes = new Hashtable();
                noDiacriticSubstitutes.Add("Æ", "AE");
                noDiacriticSubstitutes.Add("æ", "ae");
                noDiacriticSubstitutes.Add("Ø", "OE");
                noDiacriticSubstitutes.Add("Å", "AA");
                this.m_languageDiacriticSubstitutes.Add("no", noDiacriticSubstitutes);

                // Danish
                Hashtable daDiacriticSubstitutes = new Hashtable();
                daDiacriticSubstitutes.Add("Æ", "AE");
                daDiacriticSubstitutes.Add("æ", "ae");
                this.m_languageDiacriticSubstitutes.Add("da", daDiacriticSubstitutes);

            }

            public void Translate(string strUserIp, List<string> translationCodes) {

                if (this.TranslationProgress != null) {
                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.Translate: Attempting for IP: {0}", strUserIp));
                }

                // It can never be > 5000, but it's part of the TOS to block any requests over 5000.
                if (this.m_translator == null && this.Message.Length < 5000 && translationCodes.Count > 0) {

                    try {
                        this.m_translationCodes = translationCodes;

                        string translateUrl = String.Format("http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&userip={0}&q={1}", strUserIp, CPluginVariable.Encode(this.Message));

                        foreach (string translationCode in this.m_translationCodes) {
                            translateUrl += String.Format("&langpair={0}|{1}", this.DetectedLanguageCode, translationCode);
                        }

                        if (this.TranslationProgress != null) {
                            this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.Translate: IP: {0}, LanguageCodes: {1}, URL: {2}", strUserIp, String.Join("|", translationCodes.ToArray()), translateUrl));
                        }

                        this.m_translator = new CDownloadFile(translateUrl);
                        this.m_translator.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_translator_DownloadComplete);
                        this.m_translator.DownloadError += new CDownloadFile.DownloadFileEventDelegate(m_translator_DownloadError);
                        this.m_translator.BeginDownload();
                    }
                    catch (Exception e) {
                        if (this.TranslationProgress != null) {
                            this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.Translate: Error: {0}", e.Message));
                        }
                    }
                }
                else {
                    if (this.TranslationProgress != null) {
                        this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.Translate: Failed initial checks: this.m_translator == null: {0}, translationCodes.Count > 0: {1}, translationCodes.Count == {2}", this.m_translator == null, translationCodes.Count > 0, translationCodes.Count));
                    }
                }
            }

            private void ExtractTranslations(ArrayList responseDataList) {

                for (int i = 0; i < responseDataList.Count && i < this.m_translationCodes.Count; i++) {

                    Hashtable translatedTextHash = (Hashtable)responseDataList[i];

                    if (translatedTextHash.ContainsKey("responseData") == true) {

                        if (((Hashtable)translatedTextHash["responseData"]).ContainsKey("translatedText") == true) {
                            
                            if (this.m_translations.ContainsKey(this.m_translationCodes[i]) == false) {
                                this.m_translations.Add(this.m_translationCodes[i], this.RemoveDiacritics(ReplaceDiacritics(this.m_translationCodes[i], CPluginVariable.HtmlDecode((string)((Hashtable)translatedTextHash["responseData"])["translatedText"]))));
                            }
                        }
                    }
                    else if (translatedTextHash.ContainsKey("translatedText") == true) {
                        if (this.m_translations.ContainsKey(this.m_translationCodes[i]) == false) {
                            this.m_translations.Add(this.m_translationCodes[i], this.RemoveDiacritics(ReplaceDiacritics(this.m_translationCodes[i], CPluginVariable.HtmlDecode((string)translatedTextHash["translatedText"]))));
                        }
                    }
                }

                if (this.MessageTranslated != null) {
                    this.MessageTranslated(this);
                }
            }

            private void m_translator_DownloadError(CDownloadFile cdfSender) {
                if (this.TranslationError != null) {
                    this.TranslationError(String.Format("GoogleTranslatorAPI.PlayerMessage.m_translator_DownloadError: {0}", cdfSender.Error));
                }
            }

            private void m_translator_DownloadComplete(CDownloadFile cdfSender) {

                string strData = System.Text.Encoding.UTF8.GetString(cdfSender.CompleteFileData);
                Hashtable translatedTextHash = (Hashtable)JSON.JsonDecode(strData);

                if (this.TranslationProgress != null) {
                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.m_translator_DownloadComplete: Data: {0}", strData));
                }

                if (translatedTextHash.ContainsKey("responseData") == true && translatedTextHash["responseData"] != null) {

                    if (translatedTextHash["responseData"] is ArrayList) {
                        this.ExtractTranslations((ArrayList)translatedTextHash["responseData"]);
                    }
                    else if (translatedTextHash["responseData"] is Hashtable) {
                        ArrayList list = new ArrayList();
                        list.Add(translatedTextHash["responseData"]);

                        this.ExtractTranslations(list);
                    }
                }
                else if (translatedTextHash.ContainsKey("responseDetails") == true && translatedTextHash["responseDetails"] is String) {
                    if (this.TranslationError != null) {

                        this.TranslationError(String.Format("GoogleTranslatorAPI.PlayerMessage.m_translator_DownloadComplete: {0} - {1}", cdfSender.FileName, (String)translatedTextHash["responseDetails"]));
                    }
                }
            }

            public void DetectLanguage(string strUserIp) {
                // It can never be > 5000, but it's part of the TOS to block any requests over 5000.
                if (this.m_detectLanguage == null && this.Message.Length < 5000) {

                    string detectUrl = String.Format("http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&userip={0}&q={1}", strUserIp, CPluginVariable.Encode(this.Message));

                    if (this.TranslationProgress != null) {
                        this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.DetectLanguage: IP: {0}, URL: {1}", strUserIp, detectUrl));
                    }

                    try {
                        this.m_detectLanguage = new CDownloadFile(detectUrl);
                        this.m_detectLanguage.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_detectLanguage_DownloadComplete);
                        this.m_detectLanguage.DownloadError += new CDownloadFile.DownloadFileEventDelegate(m_detectLanguage_DownloadError);
                        this.m_detectLanguage.BeginDownload();
                    }
                    catch (Exception e) {
                        if (this.TranslationProgress != null) {
                            this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.DetectLanguage: Error: {0}", e.Message));
                        }
                    }
                }
            }

            void m_detectLanguage_DownloadError(CDownloadFile cdfSender) {
                if (this.TranslationError != null) {
                    this.TranslationError(String.Format("GoogleTranslatorAPI.PlayerMessage.m_detectLanguage_DownloadError: {0}", cdfSender.Error));
                }
            }

            private void m_detectLanguage_DownloadComplete(CDownloadFile cdfSender) {

                string strData = System.Text.Encoding.UTF8.GetString(cdfSender.CompleteFileData);
                Hashtable detectedLanguageHash = (Hashtable)JSON.JsonDecode(strData);

                if (this.TranslationProgress != null) {
                    this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.m_detectLanguage_DownloadComplete: Data: {0}", strData));
                }

                if (detectedLanguageHash.ContainsKey("responseData") == true && detectedLanguageHash["responseData"] != null) {
                    if (((Hashtable)detectedLanguageHash["responseData"]).ContainsKey("language") == true && ((Hashtable)detectedLanguageHash["responseData"]).ContainsKey("confidence") == true) {

                        if (((Hashtable)detectedLanguageHash["responseData"])["confidence"] is Double && (Double)((Hashtable)detectedLanguageHash["responseData"])["confidence"] >= 0.25) {
                            this.DetectedLanguageCode = (string)((Hashtable)detectedLanguageHash["responseData"])["language"];
                        }
                        else {
                            this.DetectedLanguageCode = this.SoldiersLanguage;

                            if (this.TranslationProgress != null) {
                                this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.m_detectLanguage_DownloadComplete: Failed confidence check, using players language: {0}", this.SoldiersLanguage));
                            }
                        }

                        if (this.TranslationProgress != null) {
                            this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.m_detectLanguage_DownloadComplete: this.DetectedLanguageCode = {0}", this.DetectedLanguageCode));
                        }

                        if (this.LanguageDetected != null) {
                            this.LanguageDetected(this);
                        }

                    }
                }
                else if (detectedLanguageHash.ContainsKey("responseDetails") == true && detectedLanguageHash["responseDetails"] is String) {
                    if (this.TranslationError != null) {
                        this.TranslationError(String.Format("GoogleTranslatorAPI.PlayerMessage.m_detectLanguage_DownloadComplete: {0} - {1}", cdfSender.FileName, (String)detectedLanguageHash["responseDetails"]));
                    }
                }
            }

            public bool IsWithinSubset(PlayerInformation player) {

                bool isWithinSubset = false;

                if (this.AudienceSubset.Subset == CPlayerSubset.PlayerSubsetType.All) {
                    isWithinSubset = true;
                }
                else if (this.AudienceSubset.Subset == CPlayerSubset.PlayerSubsetType.Server) {
                    isWithinSubset = true;
                }
                else if (this.AudienceSubset.Subset == CPlayerSubset.PlayerSubsetType.Player) {
                    if (String.Compare(this.AudienceSubset.SoldierName, player.SoldierName) == 0) {
                        isWithinSubset = true;
                    }
                }
                else if (player.VanillaInfo != null) {

                    if (this.AudienceSubset.Subset == CPlayerSubset.PlayerSubsetType.Team) {
                        if (this.AudienceSubset.TeamID == player.VanillaInfo.TeamID) {
                            isWithinSubset = true;
                        }
                    }
                    else if (this.AudienceSubset.Subset == CPlayerSubset.PlayerSubsetType.Squad) {
                        if (this.AudienceSubset.TeamID == player.VanillaInfo.TeamID && this.AudienceSubset.SquadID == player.VanillaInfo.SquadID) {
                            isWithinSubset = true;
                        }
                    }
                }

                return isWithinSubset;
            }

            public string ReplaceDiacritics(string languageCode, string text) {

                string replaced = text;

                if (this.m_languageDiacriticSubstitutes.ContainsKey(languageCode) == true) {

                    Hashtable language = (Hashtable)this.m_languageDiacriticSubstitutes[languageCode];

                    if (this.TranslationProgress != null) {
                        this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.ReplaceDiacritics: Before Replaced = {0}", replaced));
                    }

                    foreach (string key in language.Keys) {
                        replaced = replaced.Replace(key, (string)language[key]);
                    }

                    if (this.TranslationProgress != null) {
                        this.TranslationProgress(String.Format("GoogleTranslatorAPI.PlayerMessage.ReplaceDiacritics: After Replaced = {0}", replaced));
                    }
                }

                return replaced;
            }

            public string RemoveDiacritics(string text) {
                string normalizedString = text.Normalize(NormalizationForm.FormD);
                StringBuilder building = new StringBuilder();

                for (int i = 0; i < normalizedString.Length; i++) {
                    if (CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]) != UnicodeCategory.NonSpacingMark) {
                        building.Append(normalizedString[i]);
                    }
                }

                return building.ToString();
            }
        }

        internal class PlayerInformation  {

            private CPlayerInfo m_vanillaInfo;
            public CPlayerInfo VanillaInfo {
                get {
                    return this.m_vanillaInfo;
                }
                set {
                    this.m_vanillaInfo = value;
                }
            }

            private CPunkbusterInfo m_punkbusterInfo;
            public CPunkbusterInfo PunkbusterInfo {
                get {
                    return this.m_punkbusterInfo;
                }
                set {
                    this.m_punkbusterInfo = value;
                }
            }

            private string m_strPreferredLanguageCode;
            public string PreferredLanguageCode {
                get {
                    return this.m_strPreferredLanguageCode;
                }
                set {
                    this.m_strPreferredLanguageCode = value;
                }
            }

            public string SoldierName {
                get {
                    string soldierName = String.Empty;

                    if (this.VanillaInfo != null) {
                        soldierName = this.VanillaInfo.SoldierName;
                    }
                    else if (this.PunkbusterInfo != null) {
                        soldierName = this.PunkbusterInfo.SoldierName;
                    }

                    return soldierName;
                }
            }

            private bool m_isTranslatorEnabled;
            public bool IsTranslatorEnabled {
                get {
                    return this.m_isTranslatorEnabled;
                }
                set {
                    this.m_isTranslatorEnabled = value;
                }
            }

            public PlayerInformation(CPlayerInfo vanillaInfo, CPunkbusterInfo punkbusterInfo, bool isTranslatorEnabled) {
                this.VanillaInfo = vanillaInfo;
                this.PunkbusterInfo = punkbusterInfo;
                this.PreferredLanguageCode = "";

                this.IsTranslatorEnabled = isTranslatorEnabled;
            }
        }

        #endregion

        #region Unused interface implementations

        // Account created
        public void OnAccountCreated(string strUsername) {

        }

        public void OnAccountDeleted(string strUsername) {

        }

        public void OnAccountPrivilegesUpdate(string strUsername, CPrivileges cpPrivs) {

        }

        public void OnReceiveProconVariable(string strVariableName, string strValue) {

        }

        // Connection
        public void OnConnectionClosed() {

        }

        public void OnPlayerAuthenticated(string strSoldierName, string strGuid) {

        }

        public void OnPlayerKilled(string strKillerSoldierName, string strVictimSoldierName) {

        }

        public void OnLoadingLevel(string strMapFileName) {

        }

        public void OnLevelStarted() {

        }

        public void OnPunkbusterMessage(string strPunkbusterMessage) {

        }

        public void OnPunkbusterBanInfo(CBanInfo cbiPunkbusterBan) {

        }

        // Global or misc..
        public void OnResponseError(List<string> lstRequestWords, string strError) {

        }

        // Login events
        public void OnLogin() {

        }

        public void OnLogout() {

        }

        public void OnQuit() {

        }

        public void OnVersion(string strServerType, string strVersion) {

        }

        public void OnHelp(List<string> lstCommands) {

        }

        public void OnRunScript(string strScriptFileName) {

        }

        public void OnRunScriptError(string strScriptFileName, int iLineError, string strErrorDescription) {

        }

        // Query Events
        public void OnServerInfo(CServerInfo csiServerInfo) {

        }

        // Level Events
        public void OnRunNextLevel() {

        }

        public void OnCurrentLevel(string strCurrentLevel) {

        }

        public void OnSetNextLevel(string strNextLevel) {

        }

        public void OnRestartLevel() {

        }

        // Does not work in R3, never called for now.
        public void OnSupportedMaps(string strPlayList, List<string> lstSupportedMaps) {

        }

        public void OnPlaylistSet(string strPlaylist) {

        }

        public void OnListPlaylists(List<string> lstPlaylists) {

        }


        // Player Kick/List Events
        public void OnPlayerKicked(string strSoldierName, string strReason) {

        }

        public void OnPlayerTeamChange(string strSoldierName, int iTeamID, int iSquadID) {

        }

        public void OnPlayerSquadChange(string strSpeaker, int iTeamID, int iSquadID) {

        }

        // Banning and Banlist Events
        public void OnBanList(List<CBanInfo> lstBans) {

        }

        public void OnBanAdded(CBanInfo cbiBan) {

        }

        public void OnBanRemoved(CBanInfo cbiUnban) {

        }

        public void OnBanListClear() {

        }

        public void OnBanListLoad() {

        }

        public void OnBanListSave() {

        }

        // Reserved Slots Events
        public void OnReservedSlotsConfigFile(string strConfigFilename) {

        }

        public void OnReservedSlotsLoad() {

        }

        public void OnReservedSlotsSave() {

        }

        public void OnReservedSlotsPlayerAdded(string strSoldierName) {

        }

        public void OnReservedSlotsPlayerRemoved(string strSoldierName) {

        }

        public void OnReservedSlotsCleared() {

        }

        public void OnReservedSlotsList(List<string> lstSoldierNames) {

        }

        // Maplist Events
        public void OnMaplistConfigFile(string strConfigFilename) {

        }

        public void OnMaplistLoad() {

        }

        public void OnMaplistSave() {

        }

        public void OnMaplistMapAppended(string strMapFileName) {

        }

        public void OnMaplistMapRemoved(int iMapIndex) {

        }

        public void OnMaplistCleared() {

        }

        public void OnMaplistList(List<string> lstMapFileNames) {

        }

        public void OnMaplistNextLevelIndex(int iMapIndex) {

        }

        public void OnMaplistMapInserted(int iMapIndex, string strMapFileName) {

        }

        // Vars
        public void OnGamePassword(string strGamePassword) {

        }

        public void OnPunkbuster(bool blEnabled) {

        }

        public void OnHardcore(bool blEnabled) {

        }

        public void OnRanked(bool blEnabled) {

        }

        public void OnRankLimit(int iRankLimit) {

        }

        public void OnTeamBalance(bool blEnabled) {

        }

        public void OnFriendlyFire(bool blEnabled) {

        }

        public void OnMaxPlayerLimit(int iMaxPlayerLimit) {

        }

        public void OnCurrentPlayerLimit(int iCurrentPlayerLimit) {

        }

        public void OnPlayerLimit(int iPlayerLimit) {

        }

        public void OnBannerURL(string strURL) {

        }

        public void OnServerDescription(string strServerDescription) {

        }

        public void OnKillCam(bool blEnabled) {

        }

        public void OnMiniMap(bool blEnabled) {

        }

        public void OnCrossHair(bool blEnabled) {

        }

        public void On3dSpotting(bool blEnabled) {

        }

        public void OnMiniMapSpotting(bool blEnabled) {

        }

        public void OnThirdPersonVehicleCameras(bool blEnabled) {

        }

        #endregion
    }

}