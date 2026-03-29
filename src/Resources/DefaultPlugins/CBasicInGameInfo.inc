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

 2.1.0.0: Updated to IPRoConPluginInterface6 and added a "Hello World!" for
          the http server as an example.
 
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
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using PRoCon.Core.HttpServer;

namespace PRoConEvents {
    public class CBasicInGameInfo : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private string[] m_astrTimeDescription;

        enumBoolYesNo m_enAllowPlayersToGetServerCountry;
        enumBoolYesNo m_enAllowPlayersToGetOthersCountry;
        enumBoolYesNo m_enAllowPlayersToGuidOthers;
        enumBoolYesNo m_enAllowPlayersToTimeOthers;

        private bool m_isPluginEnabled;

        public CBasicInGameInfo() {

            this.m_enAllowPlayersToGetServerCountry = enumBoolYesNo.Yes;
            this.m_enAllowPlayersToGetOthersCountry = enumBoolYesNo.Yes;
            this.m_enAllowPlayersToGuidOthers = enumBoolYesNo.Yes;
            this.m_enAllowPlayersToTimeOthers = enumBoolYesNo.Yes;

            base.PunkbusterPlayerInfoList = new Dictionary<string, CPunkbusterInfo>();
            base.FrostbitePlayerInfoList = new Dictionary<string, CPlayerInfo>();

            this.m_astrTimeDescription = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };

            this.m_isPluginEnabled = false;
        }

        public string GetPluginName() {
            return "Basic In-Game Info";
        }

        public string GetPluginVersion() {
            return "3.0.2.0";
        }

        public string GetPluginAuthor() {
            return "Phogue";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net";
        }

        public string GetPluginDescription() {
            return @"
<h2>Description</h2>
    <p>Provides some very basic in game information for all players.</p>

<h2>Commands</h2>
    <blockquote><h4>@myguid</h4>Privately tells the player their own partial GUID</blockquote>
    <blockquote><h4>@myguid [full]</h4>Privately tells the player their own full GUID</blockquote>
    <blockquote><h4>@guid [playername]</h4>Privately tells the player another players partial GUID</blockquote>
    <blockquote><h4>@loc</h4>Privately tells the player what country the server is located in</blockquote>
    <blockquote><h4>@loc [playername]</h4>Privately tells the player what country another player is in</blockquote>
    <blockquote><h4>@help</h4>Privately lists available commands registered with procon</blockquote>
    <blockquote><h4>@help [command]</h4>Privately describes a command registered with procon</blockquote>
    <blockquote><h4>@mytimes</h4>Privately tells the player how long they are already playing since joining</blockquote>
    <blockquote><h4>@times [playername]</h4>Privately tells the player another players play time</blockquote>
    <blockquote><h4>@version</h4>Privately describes the server and rcon tool running</blockquote>

<h2>Settings</h2>
    <h3>Miscellaneous</h3>
        <blockquote><h4>Allow players to get server country</h4>Enabled or disables the @loc command</blockquote>
        <blockquote><h4>Allow players to get other players country</h4>Enabled or disables the @loc [playername] command</blockquote>
        <blockquote><h4>Allow players to get other players P/GUIDs</h4>Enabled or disables the @guid [playername] command</blockquote>
        <blockquote><h4>Allow players to get other players play time</h4>Enabled or disables the @times [playername] command</blockquote>
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
			
			this.RegisterEvents(this.GetType().Name, "OnPlayerJoin", "OnPlayerLeft", "OnListPlayers", "OnPunkbusterPlayerInfo", "OnRegisteredCommand", "OnUnregisteredCommand", "OnHttpRequest");
        }

        public void OnPluginEnable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bBasic In-Game Info ^2Enabled!");

            this.m_isPluginEnabled = true;
            this.RegisterAllCommands();
        }

        public void OnPluginDisable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bBasic In-Game Info ^1Disabled =(" );

            this.m_isPluginEnabled = false;
            this.UnregisterAllCommands();
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Allow players to get server country", typeof(enumBoolYesNo), this.m_enAllowPlayersToGetServerCountry));
            lstReturn.Add(new CPluginVariable("Allow players to get other players country", typeof(enumBoolYesNo), this.m_enAllowPlayersToGetOthersCountry));
            lstReturn.Add(new CPluginVariable("Allow players to get other players P/GUIDs", typeof(enumBoolYesNo), this.m_enAllowPlayersToGuidOthers));
            lstReturn.Add(new CPluginVariable("Allow players to get other players play time", typeof(enumBoolYesNo), this.m_enAllowPlayersToTimeOthers));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            return GetDisplayPluginVariables();
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) {
            if (strVariable.CompareTo("Allow players to get server country") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enAllowPlayersToGetServerCountry = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow players to get other players country") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enAllowPlayersToGetOthersCountry = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow players to get other players P/GUIDs") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enAllowPlayersToGuidOthers = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow players to get other players play time") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enAllowPlayersToTimeOthers = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

            this.RegisterAllCommands();
        }

        private List<string> GetExcludedCommandStrings(string strAccountName) {

            List<string> lstReturnCommandStrings = new List<string>();

            List<MatchCommand> lstCommands = this.GetRegisteredCommands();

            CPrivileges privileges = this.GetAccountPrivileges(strAccountName);

            foreach (MatchCommand mtcCommand in lstCommands) {

                if (mtcCommand.Requirements.HasValidPermissions(privileges) == true && lstReturnCommandStrings.Contains(mtcCommand.Command) == false) {
                    lstReturnCommandStrings.Add(mtcCommand.Command);
                }
            }

            return lstReturnCommandStrings;
        }

        private List<string> GetCommandStrings() {

            List<string> lstReturnCommandStrings = new List<string>();

            List<MatchCommand> lstCommands = this.GetRegisteredCommands();

            foreach (MatchCommand mtcCommand in lstCommands) {

                if (lstReturnCommandStrings.Contains(mtcCommand.Command) == false) {
                    lstReturnCommandStrings.Add(mtcCommand.Command);
                }
            }

            return lstReturnCommandStrings;
        }

        private void UnregisterAllCommands() {
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyGUID", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their partial PB GUID"));
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyGUIDFull", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("full", this.Listify<string>("full"))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their full PB GUID"));

            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandServerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with the servers location"));
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.PunkbusterPlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players location"));
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerGUID", this.Listify<string>("@", "!", "#"), "guid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.PunkbusterPlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players partial PB guid"));

            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandVersion", this.Listify<string>("@", "!", "#"), "version", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Describes the server and rcon tools running."));

            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandHelp", this.Listify<string>("@", "!", "#"), "help", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a list of commands available to the player"));
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandHelpSpecific", this.Listify<string>("@", "!", "#"), "help", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("command", this.GetCommandStrings())), new ExecutionRequirements(ExecutionScope.All), "Provides help on a specific in game command"));

            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyTimes", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their play time"));
            this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerTimes", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("full", this.Listify<string>("full"))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players play time"));

        }

        private void SetupHelpCommands() {
            this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandHelp", this.Listify<string>("@", "!", "#"), "help", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a list of commands available to the player"));
            this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandHelpSpecific", this.Listify<string>("@", "!", "#"), "help", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("command", this.GetCommandStrings())), new ExecutionRequirements(ExecutionScope.All), "Provides help on a specific in game command"));
        }

        private void RegisterAllCommands() {

            if (this.m_isPluginEnabled == true) {

                this.SetupHelpCommands();

                this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyGUID", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their partial PB GUID"));
                this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyGUIDFull", this.Listify<string>("@", "!", "#"), "myguid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("full", this.Listify<string>("full"))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their full PB GUID"));

                if (this.m_enAllowPlayersToGetServerCountry == enumBoolYesNo.Yes) {
                    this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandServerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with the servers location"));
                }
                else {
                    this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandServerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with the servers location"));
                }

                if (this.m_enAllowPlayersToGetOthersCountry == enumBoolYesNo.Yes) {
                    this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players location"));
                }
                else {
                    this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerLocation", this.Listify<string>("@", "!", "#"), "loc", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players location"));
                }

                if (this.m_enAllowPlayersToGuidOthers == enumBoolYesNo.Yes) {
                    this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerGUID", this.Listify<string>("@", "!", "#"), "guid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players partial PB guid"));
                }
                else {
                    this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerGUID", this.Listify<string>("@", "!", "#"), "guid", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players partial PB guid"));
                }

                this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandVersion", this.Listify<string>("@", "!", "#"), "version", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Describes the server and rcon tools running."));

                this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandMyTimes", this.Listify<string>("@", "!", "#"), "mytimes", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their play time"));
                if (this.m_enAllowPlayersToTimeOthers == enumBoolYesNo.Yes) {
                    this.RegisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerTimes", this.Listify<string>("@", "!", "#"), "times", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players play time"));
                } else {
                    this.UnregisterCommand(new MatchCommand("CBasicInGameInfo", "OnCommandPlayerTimes", this.Listify<string>("@", "!", "#"), "times", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(base.FrostbitePlayerInfoList.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another players play time"));
                }
            }
        }

		#region Events

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
			base.OnPlayerLeft(playerInfo);
			
            this.RegisterAllCommands();
        }


        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer) {
			base.OnPunkbusterPlayerInfo(cpbiPlayer);
            
			this.RegisterAllCommands();
        }


        public override void OnRegisteredCommand(MatchCommand mtcCommand) {
            if (String.Compare(mtcCommand.Command, "help", true) != 0) {
                this.SetupHelpCommands();
            }
        }

        public override void OnUnregisteredCommand(MatchCommand mtcCommand) {
            if (String.Compare(mtcCommand.Command, "help", true) != 0) {
                this.SetupHelpCommands();
            }
        }

        public override HttpWebServerResponseData OnHttpRequest(HttpWebServerRequestData data) {

            if (data.Query.Get("echo") != null) {
                return new HttpWebServerResponseData(data.Query.Get("echo"));
            }
            else {
                return new HttpWebServerResponseData("Hello World!");
            }
        }
        
		#endregion

        #region In Game Commands

        public void OnCommandMyGUID(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (base.PunkbusterPlayerInfoList.ContainsKey(strSpeaker) == true && base.PunkbusterPlayerInfoList[strSpeaker].GUID.Length == 32) {

                this.ExecuteCommand("procon.protected.send", "admin.say", "Your P/GUID " + base.PunkbusterPlayerInfoList[strSpeaker].GUID.Substring(24, 8), "player", strSpeaker);
            }
            else {
                this.ExecuteCommand("procon.protected.send", "admin.say", "No pb info yet, try again =(", "player", strSpeaker);
            }
        }

        public void OnCommandMyGUIDFull(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (base.PunkbusterPlayerInfoList.ContainsKey(strSpeaker) == true && base.PunkbusterPlayerInfoList[strSpeaker].GUID.Length == 32) {

                this.ExecuteCommand("procon.protected.send", "admin.say", "Your GUID " + base.PunkbusterPlayerInfoList[strSpeaker].GUID, "player", strSpeaker);
            }
            else {
                this.ExecuteCommand("procon.protected.send", "admin.say", "No pb info yet, try again =(", "player", strSpeaker);
            }
        }

        public void OnCommandServerLocation(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            this.ExecuteCommand("procon.protected.send", "admin.say", "Server location: " + this.GetVariable<string>("SERVER_COUNTRY", ""), "player", strSpeaker);
        }

        public void OnCommandPlayerLocation(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAllowPlayersToGetOthersCountry == enumBoolYesNo.Yes) {
                if (base.PunkbusterPlayerInfoList.ContainsKey(capCommand.MatchedArguments[0].Argument) == true) {
                    this.ExecuteCommand("procon.protected.send", "admin.say", base.PunkbusterPlayerInfoList[capCommand.MatchedArguments[0].Argument].SoldierName + "'s location: " + base.PunkbusterPlayerInfoList[capCommand.MatchedArguments[0].Argument].PlayerCountry, "player", strSpeaker);
                }
                else {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Player not found", "player", strSpeaker);
                }
            }
        }

        public void OnCommandPlayerGUID(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAllowPlayersToGuidOthers == enumBoolYesNo.Yes) {
                if (base.PunkbusterPlayerInfoList.ContainsKey(capCommand.MatchedArguments[0].Argument) == true && base.PunkbusterPlayerInfoList[capCommand.MatchedArguments[0].Argument].GUID.Length == 32) {
                    this.ExecuteCommand("procon.protected.send", "admin.say", base.PunkbusterPlayerInfoList[capCommand.MatchedArguments[0].Argument].SoldierName + "'s P/GUID: " + base.PunkbusterPlayerInfoList[capCommand.MatchedArguments[0].Argument].GUID.Substring(24, 8), "player", strSpeaker);
                }
                else {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Player not found", "player", strSpeaker);
                }
            }
        }

        public void OnCommandHelp(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("Type \"{0}help [command]\" for more information about a command:", capCommand.ResposeScope), "player", strSpeaker);

            foreach (string strCommandOutput in this.WordWrap(String.Join(", ", this.GetExcludedCommandStrings(strSpeaker).ToArray()), 100)) {
                this.ExecuteCommand("procon.protected.send", "admin.say", strCommandOutput, "player", strSpeaker);
            }
        }

        public void OnCommandHelpSpecific(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            List<MatchCommand> lstCommands = this.GetRegisteredCommands();

            CPrivileges privileges = this.GetAccountPrivileges(strSpeaker);

            foreach (MatchCommand mtcLoopedCommand in lstCommands) {
                if (String.Compare(mtcLoopedCommand.Command, capCommand.MatchedArguments[0].Argument, true) == 0 && mtcLoopedCommand.Requirements.HasValidPermissions(privileges) == true) {
                    foreach (string strLine in this.WordWrap(String.Format("> {0}; {1}", mtcLoopedCommand.ToString(), mtcLoopedCommand.Description), 100)) {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strLine, "player", strSpeaker);
                    }
                }
            }
        }

        public void OnCommandVersion(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("IP: {0}, Location: {1}, Rcon: PRoCon {2}", this.m_strHostName, this.GetVariable<string>("SERVER_COUNTRY", ""), this.m_strPRoConVersion), "player", strSpeaker);
        }

        public void OnCommandMyTimes(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (base.FrostbitePlayerInfoList.ContainsKey(strSpeaker) == true && base.FrostbitePlayerInfoList[strSpeaker].SessionTime != 0) {
                string strOutTime = this.SecondsToText((UInt32)base.FrostbitePlayerInfoList[strSpeaker].SessionTime, this.m_astrTimeDescription, true);
                this.ExecuteCommand("procon.protected.send", "admin.say", "Your current playTime: " + strOutTime, "player", strSpeaker);
            } else {
                this.ExecuteCommand("procon.protected.send", "admin.say", "Sorry, you've just joined =(", "player", strSpeaker);
            }
        }
        
        public void OnCommandPlayerTimes(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_enAllowPlayersToTimeOthers == enumBoolYesNo.Yes) {
                if (base.FrostbitePlayerInfoList.ContainsKey(capCommand.MatchedArguments[0].Argument) == true && base.FrostbitePlayerInfoList[capCommand.MatchedArguments[0].Argument].SessionTime != 0) {
                    string strOutTime = this.SecondsToText((UInt32)base.FrostbitePlayerInfoList[capCommand.MatchedArguments[0].Argument].SessionTime, this.m_astrTimeDescription, true);
                    this.ExecuteCommand("procon.protected.send", "admin.say", base.FrostbitePlayerInfoList[capCommand.MatchedArguments[0].Argument].SoldierName + "'s playTime: " + strOutTime, "player", strSpeaker);
                } else {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Player not found / or just joined", "player", strSpeaker);
                }
            }
        }
        
        #endregion
    }
}