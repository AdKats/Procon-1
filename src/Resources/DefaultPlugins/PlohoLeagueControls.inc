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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

using PRoCon.Core.Logging;

// Add @timeout

// Validate { and } is resolved.

// Add @map command (in game admin?)

namespace PRoConEvents {
    public class PlohoLeagueControls : PRoConPluginAPI, IPRoConPluginInterface {

        private enumBoolOnOff m_onOffDebug;
        
        private string m_strCommandPrefix;
        private string m_strReadyCommand;
        private string m_strScoresCommand;
        private string m_strWeaponDumpCommand;
        private string m_strMapCommand;

        private string m_strLeagueNamePrefix;

        private enumBoolYesNo m_teamsConfirmReadyCommand;
        private enumBoolYesNo m_teamsConfirmMapCommand;

        private GamemodeInformationDictionary m_gameModeSettings;
        private string m_strSelectedGamemode;

        private int m_iTimerDisplayInterval;
        private int m_iTimerDisplayLength;
        private int m_iTimerFinalCountdown;

        private bool m_isPluginEnabled;
        private bool m_blRestarting;

        private WeaponDictionary m_weapons;

        // Will expand on this if it's needed.
        private RoundInformation m_currentRound;
        private RoundInformation m_previousRound;

        private string m_fileHostNamePort;
        
        public PlohoLeagueControls() {
            this.m_gameModeSettings = new GamemodeInformationDictionary();

            this.m_strCommandPrefix = "@";
            this.m_strReadyCommand = "ready";
            this.m_strScoresCommand = "scores";
            this.m_strWeaponDumpCommand = "dump";
            this.m_strMapCommand = "map";

            this.m_strLeagueNamePrefix = "PRoCon";

            this.m_strSelectedGamemode = String.Empty;

            this.m_iTimerDisplayInterval = 60;
            this.m_iTimerDisplayLength = 4;
            this.m_iTimerFinalCountdown = 30;

            this.m_teamsConfirmReadyCommand = enumBoolYesNo.No;
            this.m_teamsConfirmMapCommand = enumBoolYesNo.Yes;

            this.m_isPluginEnabled = false;
            this.m_blRestarting = false;
        }

        public string GetPluginName() {
            return "Ploho League Controls";
        }

        public string GetPluginVersion() {
            return "2.0.0.0";
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
        <p>Provides some in game controls for leagues and competitions.</p>

    <h2>Commands</h2>
        <blockquote><h4>@ready</h4>Toggles a readied status for a team, or executes the startmatch.cfg file if ready confirmation is disabled.</blockquote>
        <blockquote><h4>@scores</h4>Displays the scores for the previous round (the same information that is printed at the end of the round)</blockquote>
        <blockquote><h4>@map</h4>Displays a list of allowed maps for whatever gametype the current map is.</blockquote>
        <blockquote><h4>@map [map name]</h4>Puts forward a map to be confirmed by the other team or immediately forwarded if map confirmation is disabled.  The map can be a public name (Port Valdez) or a file name (Levels/mp_012sr) and is matched against a dictionary (partial matches/misspelling allowed)</blockquote>
        <blockquote><h4>@dump</h4>Dumps a list of player names, pbguids and all the weapons they used to get kills in the previous round.  This is for basic client side dispute evidence collection.</blockquote>

    <h2>Settings</h2>
        <h3>Miscellaneous</h3>
            <blockquote><h4>Debug mode</h4>Diagnostic option to show some of the processes of the plugin</blockquote>

        <h3>Commands</h3>
            <blockquote><h4>Command Prefix</h4>The prefix in front of the command ('@', '!') etc.</blockquote>
            <blockquote><h4>Ready Command</h4>The (default) @ready command name</blockquote>
            <blockquote><h4>Last Round Scores Command</h4>The (default) @scores command name</blockquote>
            <blockquote><h4>Last Round Weapon Dump Command</h4>The (default) @dump command name</blockquote>
            <blockquote><h4>Map Command</h4>The (default) @map command name</blockquote>

        <h3>Confirmations</h3>
            <blockquote><h4>Confirm Ready Command</h4>If set to Yes then both teams will need to toggle their ready status to ready with the @ready command, otherwise any time a player issues the @ready command the map will restart</blockquote>
            <blockquote><h4>Confirm Map Command</h4>If set to Yes then both teams will need to submit the same map for the map to change, otherwise any player can change the map with the @map [map name] command.</blockquote>

        <h3>League</h3>
            <blockquote><h4>Output Prefix</h4>The text at the beginning of each line.  www.cybergamer.com.au might want to change this to ""CG"" or ""CyberGamer"" just to make the output look more official to their league/ladder.</blockquote>
        
        <h3>Gamemode Settings</h3>
            <blockquote><h4>Gamemode</h4>Not an actual setting, just changes what options to display for each gamemode.</blockquote>
            <blockquote><h4>Gamemode - Allowed maplist</h4>A list of maps allowed for the gamemode.  By default all the maps for the gamemode will be available, but some competitions may not allow certain maps for their own reasons</blockquote>
            <blockquote><h4>Gamemode - Use time limit</h4>If the map should use a set time limit.</blockquote>
            <blockquote><h4>Gamemode - Time limit (Minutes)</h4>The time limit for the gamemode.</blockquote>
            <blockquote><h4>Gamemode - Winning Team</h4>The team to be declared the winners when the timer reaches zero.  If set to neutral procon will calculate the winner by their current score at round end.</blockquote>

        <h3>Timers</h3>
            <blockquote><h4>Display Round Timer Interval (Seconds)</h4>How often the timer should display in the middle of the screen.</blockquote>
            <blockquote><h4>Display Round Timer Additional (Seconds)</h4>How much the timer should display before and after the interval.</blockquote>
            <blockquote><h4>Display Final Countdown (Seconds)</h4>When the final countdown to the round end should show.</blockquote>

    <h2>Additional Information</h2>
        <ul>
            <li>This plugin will become unresponsive if you edit startmatch.cfg and don't include a restart command in it.  You'll need to reload the plugin.</li>
            <li>The @ready triggers the execution of startmatch config located at ""/Configs/PlohoLeagueControls/startmatch.cfg"".  The config by default has a quick countdown then restarts the map.</li>
            <li>Timers begin when the first player spawns in.</li>
        </ul>";
        }
        

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {

			this.RegisterEvents(this.GetType().Name, "OnLevelStarted", "OnLoadingLevel", "OnRoundOverTeamScores", "OnRoundOver", "OnRoundOverPlayers", "OnPlayerJoin", "OnPlayerLeft", "OnPunkbusterPlayerInfo", "OnListPlayers", "OnPlayerSpawned", "OnServerInfo", "OnPlayerKilled", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnPlayerTeamChange", "OnPlayerSquadChange");

            this.m_fileHostNamePort = String.Format("{0}_{1}", strHostName, strPort);
            this.m_weapons = this.GetWeaponDefines();

            foreach (string gameMode in this.GetMapList("{GameMode}")) { // , "RUSH", "CONQUEST", "SQDM", "SQRUSH"
                
                CMap anyMap = this.GetMapByFormattedName("{GameMode}", gameMode);

                if (anyMap != null && anyMap.TeamNames.Count > 0) {
                    
                    this.m_gameModeSettings.Add(new GamemodeInformation(gameMode, anyMap.PlayList, this.GetMapList("{FileName}", anyMap.PlayList).ToArray(), anyMap.TeamNames[0].LocalizationKey));

                    if (this.m_strSelectedGamemode.Length == 0) {
                        this.m_strSelectedGamemode = gameMode;
                    }
                }
            }
        }

        public void OnPluginEnable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPloho League Controls ^2Enabled!");

            this.m_isPluginEnabled = true;
            this.RegisterAllCommands();
        }

        public void OnPluginDisable() {
            this.m_isPluginEnabled = false;
            this.UnregisterAllCommands();

            this.ExecuteCommand("procon.protected.tasks.remove", "PlohoLeagueControlsRoundTimeLimit");

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPloho League Controls ^1Disabled =(");
        }

        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Miscellaneous|Debug Mode", typeof(enumBoolOnOff), this.m_onOffDebug));

            // Commands
            lstReturn.Add(new CPluginVariable("Commands|Command Prefix", this.m_strCommandPrefix.GetType(), this.m_strCommandPrefix));
            lstReturn.Add(new CPluginVariable("Commands|Ready Command", this.m_strReadyCommand.GetType(), this.m_strReadyCommand));
            lstReturn.Add(new CPluginVariable("Commands|Last Round Scores Command", this.m_strScoresCommand.GetType(), this.m_strScoresCommand));
            lstReturn.Add(new CPluginVariable("Commands|Last Round Weapon Dump Command", this.m_strWeaponDumpCommand.GetType(), this.m_strWeaponDumpCommand));
            lstReturn.Add(new CPluginVariable("Commands|Map Command", this.m_strMapCommand.GetType(), this.m_strMapCommand));
            
            // Confirmations
            lstReturn.Add(new CPluginVariable("Confirmations|Confirm Ready Command", this.m_teamsConfirmReadyCommand.GetType(), this.m_teamsConfirmReadyCommand));
            lstReturn.Add(new CPluginVariable("Confirmations|Confirm Map Command", this.m_teamsConfirmMapCommand.GetType(), this.m_teamsConfirmMapCommand));

            // Customization
            lstReturn.Add(new CPluginVariable("League|Output Prefix", this.m_strLeagueNamePrefix.GetType(), this.m_strLeagueNamePrefix));

            // Gamemode Settings
            lstReturn.Add(this.GetMapListPluginVariable("Gamemode Settings|Gamemode", "PlohoLeagueControlsGamemodes", this.m_strSelectedGamemode, "{GameMode}"));

            if (this.m_strSelectedGamemode.Length > 0 && this.m_gameModeSettings.Contains(this.m_strSelectedGamemode) == true) {

                lstReturn.Add(new CPluginVariable(String.Format("Gamemode Settings|{0} - Allowed maplist", this.m_strSelectedGamemode), typeof(string[]), this.m_gameModeSettings[this.m_strSelectedGamemode].AllowedMapListFileNames));
                lstReturn.Add(new CPluginVariable(String.Format("Gamemode Settings|{0} - Use time limit", this.m_strSelectedGamemode), this.m_gameModeSettings[this.m_strSelectedGamemode].UseTimeLimit.GetType(), this.m_gameModeSettings[this.m_strSelectedGamemode].UseTimeLimit));
                lstReturn.Add(new CPluginVariable(String.Format("Gamemode Settings|{0} - Time limit (Minutes)", this.m_strSelectedGamemode), this.m_gameModeSettings[this.m_strSelectedGamemode].TimeLimit.GetType(), this.m_gameModeSettings[this.m_strSelectedGamemode].TimeLimit));

                CMap theMap = this.GetMapByFormattedName("{GameMode}", this.m_strSelectedGamemode);

                if (theMap != null) {
                    lstReturn.Add(this.GetTeamListPluginVariable(String.Format("Gamemode Settings|{0} - Winning Team", this.m_strSelectedGamemode), String.Format("PlohoLeagueControlsTeamNames{0}", this.m_strSelectedGamemode), this.GetLocalized("", this.m_gameModeSettings[this.m_strSelectedGamemode].WinningTeamOnTimeLimitLocalizationKey), "{TeamName}", theMap.PlayList));
                }
            }

            lstReturn.Add(new CPluginVariable("Timers|Display Round Timer Interval (Seconds)", this.m_iTimerDisplayInterval.GetType(), this.m_iTimerDisplayInterval));
            lstReturn.Add(new CPluginVariable("Timers|Display Round Timer Additional (Seconds)", this.m_iTimerDisplayLength.GetType(), this.m_iTimerDisplayLength));
            lstReturn.Add(new CPluginVariable("Timers|Display Final Countdown (Seconds)", this.m_iTimerFinalCountdown.GetType(), this.m_iTimerFinalCountdown));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Debug Mode", typeof(enumBoolOnOff), this.m_onOffDebug));

            lstReturn.Add(new CPluginVariable("Command Prefix", this.m_strCommandPrefix.GetType(), this.m_strCommandPrefix));
            lstReturn.Add(new CPluginVariable("Ready Command", this.m_strReadyCommand.GetType(), this.m_strReadyCommand));
            lstReturn.Add(new CPluginVariable("Last Round Scores Command", this.m_strScoresCommand.GetType(), this.m_strScoresCommand));
            lstReturn.Add(new CPluginVariable("Last Round Weapon Dump Command", this.m_strWeaponDumpCommand.GetType(), this.m_strWeaponDumpCommand));
            lstReturn.Add(new CPluginVariable("Map Command", this.m_strMapCommand.GetType(), this.m_strMapCommand));

            lstReturn.Add(new CPluginVariable("Confirm Ready Command", this.m_teamsConfirmReadyCommand.GetType(), this.m_teamsConfirmReadyCommand));
            lstReturn.Add(new CPluginVariable("Confirm Map Command", this.m_teamsConfirmMapCommand.GetType(), this.m_teamsConfirmMapCommand));

            lstReturn.Add(new CPluginVariable("Output Prefix", this.m_strLeagueNamePrefix.GetType(), this.m_strLeagueNamePrefix));

            lstReturn.Add(this.GetMapListPluginVariable("Gamemode", "PlohoLeagueControlsGamemodes", this.m_strSelectedGamemode, "{GameMode}"));

            foreach (GamemodeInformation gamemode in this.m_gameModeSettings) {
                lstReturn.Add(new CPluginVariable(String.Format("{0} - Allowed maplist", gamemode.Name), typeof(string[]), gamemode.AllowedMapListFileNames));
                lstReturn.Add(new CPluginVariable(String.Format("{0} - Use time limit", gamemode.Name), gamemode.UseTimeLimit.GetType(), gamemode.UseTimeLimit));
                lstReturn.Add(new CPluginVariable(String.Format("{0} - Time limit (Minutes)", gamemode.Name), gamemode.TimeLimit.GetType(), gamemode.TimeLimit));

                CMap theMap = this.GetMapByFormattedName("{GameMode}", gamemode.Name);

                if (theMap != null) {
                    lstReturn.Add(this.GetTeamListPluginVariable(String.Format("{0} - Winning Team", gamemode.Name), String.Format("PlohoLeagueControlsTeamNames{0}", gamemode.Name), this.GetLocalized("", gamemode.WinningTeamOnTimeLimitLocalizationKey), "{TeamName}", theMap.PlayList));
                }
            }

            lstReturn.Add(new CPluginVariable("Display Round Timer Interval (Seconds)", this.m_iTimerDisplayInterval.GetType(), this.m_iTimerDisplayInterval));
            lstReturn.Add(new CPluginVariable("Display Round Timer Additional (Seconds)", this.m_iTimerDisplayLength.GetType(), this.m_iTimerDisplayLength));
            lstReturn.Add(new CPluginVariable("Display Final Countdown (Seconds)", this.m_iTimerFinalCountdown.GetType(), this.m_iTimerFinalCountdown));

            return lstReturn;
        }

        public void SetPluginVariable(string strVariable, string strValue) {
            this.UnregisterAllCommands();

            int displaySeconds = 0;

            if (strVariable.CompareTo("Debug Mode") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true) {
                this.m_onOffDebug = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);
            }
            else if (strVariable.CompareTo("Confirm Ready Command") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_teamsConfirmReadyCommand = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Confirm Map Command") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_teamsConfirmMapCommand = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Command Prefix") == 0) {
                this.m_strCommandPrefix = strValue;
            }
            else if (strVariable.CompareTo("Ready Command") == 0) {
                this.m_strReadyCommand = strValue;
            }
            else if (strVariable.CompareTo("Map Command") == 0) {
                this.m_strMapCommand = strValue;
            }
            else if (strVariable.CompareTo("Last Round Scores Command") == 0) {
                this.m_strScoresCommand = strValue;
            }
            else if (strVariable.CompareTo("Last Round Weapon Dump Command") == 0) {
                this.m_strWeaponDumpCommand = strValue;
            }
            else if (strVariable.CompareTo("Output Prefix") == 0) {
                this.m_strLeagueNamePrefix = strValue;
            }
            else if (strVariable.CompareTo("Gamemode") == 0) {
                this.m_strSelectedGamemode = strValue;
            }
            else if (strVariable.CompareTo("Display Round Timer Interval (Seconds)") == 0 && int.TryParse(strValue, out displaySeconds) == true) {
                this.m_iTimerDisplayInterval = displaySeconds;
            }
            else if (strVariable.CompareTo("Display Round Timer Additional (Seconds)") == 0 && int.TryParse(strValue, out displaySeconds) == true) {
                this.m_iTimerDisplayLength = displaySeconds;
            }
            else if (strVariable.CompareTo("Display Final Countdown (Seconds)") == 0 && int.TryParse(strValue, out displaySeconds) == true) {
                this.m_iTimerFinalCountdown = displaySeconds;
            }
            else {
                foreach (GamemodeInformation gamemode in this.m_gameModeSettings) {

                    double timeLimit = 0.0D;

                    if (String.Compare(strVariable, String.Format("{0} - Use time limit", gamemode.Name), true) == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                        gamemode.UseTimeLimit = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                    }
                    else if (String.Compare(strVariable, String.Format("{0} - Time limit (Minutes)", gamemode.Name), true) == 0 && double.TryParse(strValue, out timeLimit) == true) {
                        gamemode.TimeLimit = timeLimit;
                    }
                    else if (String.Compare(strVariable, String.Format("{0} - Allowed maplist", gamemode.Name), true) == 0) {
                        gamemode.AllowedMapListFileNames = CPluginVariable.DecodeStringArray(strValue);

                    }
                    else if (String.Compare(strVariable, String.Format("{0} - Winning Team", gamemode.Name), true) == 0) {
                        CTeamName selectedTeamName = this.GetTeamNameByFormattedTeamName("{TeamName}", strValue);

                        if (selectedTeamName != null) {
                            gamemode.WinningTeamOnTimeLimitLocalizationKey = selectedTeamName.LocalizationKey;
                        }
                    }
                }
            }

            this.RegisterAllCommands();
        }

        public void UnregisterAllCommands() {
            this.UnregisterCommand(
                new MatchCommand(
                    this.Listify<string>(this.m_strCommandPrefix),
                    this.m_strReadyCommand,
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    this.Listify<string>(this.m_strCommandPrefix),
                    this.m_strScoresCommand,
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    this.Listify<string>(this.m_strCommandPrefix),
                    this.m_strWeaponDumpCommand,
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    this.Listify<string>(this.m_strCommandPrefix),
                    this.m_strMapCommand,
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    this.Listify<string>(this.m_strCommandPrefix),
                    this.m_strMapCommand,
                    this.Listify<MatchArgumentFormat>(
                        new MatchArgumentFormat("map name", new List<string>())
                    )
                )
            );
        }

        public void RegisterAllCommands() {

            if (this.m_isPluginEnabled == true) {

                this.RegisterCommand(
                    new MatchCommand(
                        "PlohoLeagueControls",
                        "OnCommandReady",
                        this.Listify<string>(this.m_strCommandPrefix),
                        this.m_strReadyCommand,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(ExecutionScope.All),
                        "Restarts the map for a live match"
                    )
                );

                this.RegisterCommand(
                    new MatchCommand(
                        "PlohoLeagueControls",
                        "OnCommandScores",
                        this.Listify<string>(this.m_strCommandPrefix),
                        this.m_strScoresCommand,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(ExecutionScope.All),
                        "Displays the scores for the last round"
                    )
                );

                this.RegisterCommand(
                    new MatchCommand(
                        "PlohoLeagueControls",
                        "OnCommandWeaponDump",
                        this.Listify<string>(this.m_strCommandPrefix),
                        this.m_strWeaponDumpCommand,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(ExecutionScope.All),
                        "Dumps an ugly list of all players weapons used in the last round"
                    )
                );

                if (this.m_currentRound != null && this.m_currentRound.Gamemode != null) {
                    
                    // What's the current game type?
                    this.RegisterCommand(
                        new MatchCommand(
                            "PlohoLeagueControls",
                            "OnCommandMapList",
                            this.Listify<string>(this.m_strCommandPrefix),
                            this.m_strMapCommand,
                            this.Listify<MatchArgumentFormat>(),
                            new ExecutionRequirements(ExecutionScope.All),
                            "Displays a list of potential maps to put forward"
                        )
                    );

                    this.RegisterCommand(
                        new MatchCommand(
                            "PlohoLeagueControls",
                            "OnCommandMapSet",
                            this.Listify<string>(this.m_strCommandPrefix),
                            this.m_strMapCommand,
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat("map name", this.ListifyMaplist(this.m_currentRound.Gamemode.AllowedMapListFileNames))
                            ),
                            new ExecutionRequirements(ExecutionScope.All),
                            "Puts forward a map for your team.  Map can be a name or filename."
                        )
                    );
                }
            }
        }

        private void WriteMessage(string message, CPlayerSubset audience) {

            string strPrefix = String.Format("{0} > ", this.m_strLeagueNamePrefix);

            List<string> wordWrappedLines = this.WordWrap(message, 100 - strPrefix.Length);

            foreach (string line in wordWrappedLines) {
                string formattedLine = String.Format("{0}{1}", strPrefix.Replace("{", "{{").Replace("}", "}}"), line);

                if (audience.Subset == CPlayerSubset.PlayerSubsetType.All) {
                    this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "all");
                }
                else if (audience.Subset == CPlayerSubset.PlayerSubsetType.Player) {
                    this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "player", audience.SoldierName);
                }
            }
        }

        private List<string> ListifyMaplist(string[] mapList) {

            List<string> returnMapList = new List<string>();
            List<CMap> mapDefines = this.GetMapDefines();

            foreach (string allowedMap in mapList) {
                foreach (CMap map in mapDefines) {
                    if (String.Compare(map.FileName, allowedMap, true) == 0) {
                        returnMapList.Add(String.Format("{0} ({1})", map.PublicLevelName, map.FileName));
                    }
                }
            }

            return returnMapList;
        }

        public void OnCommandMapList(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (this.m_currentRound != null && this.m_currentRound.Gamemode != null) {

                List<string> mapList = this.ListifyMaplist(this.m_currentRound.Gamemode.AllowedMapListFileNames);

                this.WriteMessage(String.Format("Available maps: {0}", String.Join(", ", mapList.ToArray())), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
            }
        }

        public void OnCommandMapSet(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            
            if (this.m_currentRound != null && this.m_currentRound.Gamemode != null) {

                string teamLocalizationKey = this.m_currentRound.GetPlayerTeamLocalizationKey(strSpeaker);
                CMap selectedMap = this.GetMapByFormattedName("{PublicLevelName} ({FileName})", capCommand.MatchedArguments[0].Argument);

                if (this.m_currentRound.TeamConfirmationCommands.Contains(teamLocalizationKey) == true) {
                    this.m_currentRound.TeamConfirmationCommands[teamLocalizationKey].ProposedMapChange = selectedMap.FileName;

                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnCommandMapSet: Setting " + teamLocalizationKey + " proposed map to " + selectedMap.FileName);
                    }

                    this.WriteMessage(String.Format("{0} have put forward {1} ({2})", this.GetLocalized("", teamLocalizationKey), selectedMap.PublicLevelName, selectedMap.FileName), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                }
                else if (this.m_onOffDebug == enumBoolOnOff.On) {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnCommandMapSet: TeamConfirmationCommands[" + teamLocalizationKey + "] was not found ");
                }

                string changeMapFileName = this.m_currentRound.TeamConfirmationCommands.GetConfirmedMap(this.m_teamsConfirmMapCommand == enumBoolYesNo.Yes ? this.m_currentRound.TeamConfirmationCommands.Count : 1, this.m_teamsConfirmMapCommand == enumBoolYesNo.Yes);

                if (changeMapFileName.Length > 0) {
                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnCommandMapSet: Minimum teams (" + (this.m_teamsConfirmMapCommand == enumBoolYesNo.Yes ? this.m_currentRound.TeamConfirmationCommands.Count : 1) + " of " + this.m_currentRound.TeamConfirmationCommands.Count + ") agree on map " + changeMapFileName);
                    }

                    if (this.m_blRestarting == false) {
                        this.ExecuteCommand("procon.protected.send", "admin.setPlaylist", selectedMap.PlayList);
                        this.ExecuteCommand("procon.protected.send", "mapList.clear");
                        this.ExecuteCommand("procon.protected.send", "mapList.append", selectedMap.FileName);
                        this.ExecuteCommand("procon.protected.send", "admin.runNextLevel");
                    }
                    else if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnCommandMapSet: Teams agreed on map, but round is restarting.");
                    }
                }
                else {

                    List<string> waitingTeams = new List<string>();

                    foreach (ConfirmationCommand command in this.m_currentRound.TeamConfirmationCommands) {
                        if (command.ProposedMapChange.Length == 0) {
                            waitingTeams.Add(this.GetLocalized("", command.TeamNameLocalizationKey));
                        }
                    }

                    // If all teams have entered a map (but they don't agree on the map)
                    if (waitingTeams.Count == 0) {

                        List<string> proposedMaps = new List<string>();

                        foreach (ConfirmationCommand command in this.m_currentRound.TeamConfirmationCommands) {
                            if (command.ProposedMapChange.Length > 0) {

                                foreach (CMap proposedMap in this.GetMapDefines()) {
                                    if (String.Compare(command.ProposedMapChange, proposedMap.FileName, true) == 0) {
                                        proposedMaps.Add(String.Format("{0} ({1} - {2})", this.GetLocalized("", command.TeamNameLocalizationKey), proposedMap.PublicLevelName, proposedMap.FileName));
                                        break;
                                    }
                                }
                            }
                        }

                        this.WriteMessage(String.Format("Waiting for teams to agree on a map ({0}{1}); {2}", this.m_strCommandPrefix, this.m_strMapCommand, String.Join(", ", proposedMaps.ToArray())), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                    }
                    else {
                        this.WriteMessage(String.Format("Waiting for {0} to put forward a map ({1}{2})", String.Join(", ", waitingTeams.ToArray()), this.m_strCommandPrefix, this.m_strMapCommand), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                    }
                }
            }
        }

        public void OnCommandReady(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (this.m_currentRound != null) {

                string teamLocalizationKey = this.m_currentRound.GetPlayerTeamLocalizationKey(strSpeaker);

                if (this.m_currentRound.TeamConfirmationCommands.Contains(teamLocalizationKey) == true) {
                    bool confirmed = this.m_currentRound.TeamConfirmationCommands[teamLocalizationKey].ToggleReadyUpConfirmed();

                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnCommandReady: Setting " + teamLocalizationKey + " ready status to " + confirmed);
                    }
                }
                else if (this.m_onOffDebug == enumBoolOnOff.On) {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnCommandReady: TeamConfirmationCommands[" + teamLocalizationKey + "] was not found ");
                }

                if (this.m_currentRound.TeamConfirmationCommands.IsReadyUpConfirmed(this.m_teamsConfirmReadyCommand == enumBoolYesNo.Yes ? this.m_currentRound.TeamConfirmationCommands.Count : 1) == true) {

                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnCommandReady: All teams are readied up");
                    }

                    if (this.m_blRestarting == false) {
                        this.m_blRestarting = true;

                        this.m_currentRound.LogReadyCommand(strSpeaker);
                        this.m_currentRound.RoundOverCondition = RoundOverConditions.ReadiedUp;

                        this.m_currentRound.LogWinningTeam(0);
                        this.m_currentRound.LogWinningScores(this.Listify<TeamScore>(new TeamScore(1, 0, 0), new TeamScore(2, 0, 0)));
						
						#include "PlohoLeagueControls.StartMatch.inc"
						
                        //this.ExecuteCommand("procon.protected.config.exec", Path.Combine("PlohoLeagueControls", "startmatch.cfg"));
                    }
					else {
						if (this.m_onOffDebug == enumBoolOnOff.On) {
							this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnCommandReady: All teams are readied up but the round is already restarting.");
						}
					}
                }
                else {

                    List<string> waitingTeams = new List<string>();

                    foreach (ConfirmationCommand command in this.m_currentRound.TeamConfirmationCommands) {
                        if (command.ReadyUpConfirmed == false) {
                            waitingTeams.Add(this.GetLocalized("", command.TeamNameLocalizationKey));
                        }
                    }

                    this.WriteMessage(String.Format("Waiting for {0} to confirm readiness ({1}{2})", String.Join(", ", waitingTeams.ToArray()), this.m_strCommandPrefix, this.m_strReadyCommand), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                }
            }
        }

        public void OnCommandScores(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (this.m_previousRound != null) {
                try {
                    this.DisplayRoundScores(this.m_previousRound, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
                }
                catch (Exception e) {
                    if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnCommandScores: Exception " + e.Message);
                    }
                }
            }
            else {
                this.WriteMessage("No information is available for the previous round", new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
            }
        }

        public void OnCommandWeaponDump(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (this.m_previousRound != null) {

                if (this.m_previousRound.RoundStartTime != null) {
                    this.WriteMessage(String.Format("Round start: {0} {1}", this.m_previousRound.RoundStartTime.ToShortDateString(), this.m_previousRound.RoundStartTime.ToShortTimeString()), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
                }

                foreach (PlayerInformation player in this.m_previousRound.Players) {
                    this.WriteMessage(player.ToString(), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
                }

                if (this.m_previousRound.RoundEndTime != null) {
                    this.WriteMessage(String.Format("Round end: {0} {1}", this.m_previousRound.RoundEndTime.ToShortDateString(), this.m_previousRound.RoundEndTime.ToShortTimeString()), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
                }
            }
            else {
                this.WriteMessage("No information is available for the previous round", new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Player, strSpeaker));
            }
        }

        public override void OnLevelStarted() {
            this.m_blRestarting = false;
        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) {

            foreach (CMap map in this.GetMapDefines()) {
                if (String.Compare(map.FileName, mapFileName, true) == 0) {

                    List<CTeamName> localizedTeamNames = new List<CTeamName>();

                    foreach (CTeamName teamName in map.TeamNames) {
                        localizedTeamNames.Add(
                            new CTeamName(
                                teamName.MapFilename,
                                teamName.TeamID,
                                teamName.LocalizationKey,
                                this.GetLocalized(teamName.LocalizationKey, teamName.LocalizationKey)
                            )
                        );
                    }

                    if (this.m_previousRound != null) {
                        this.m_previousRound.GetLocalization -= new RoundInformation.GetLocalizationHandler(m_currentRound_GetLocalization);
                        this.m_previousRound.RoundBegin -= new RoundInformation.RoundEventHandler(m_currentRound_RoundBegin);
                        this.m_previousRound.RoundEnd -= new RoundInformation.RoundEventHandler(m_currentRound_RoundEnd);
                    }

                    this.m_previousRound = this.m_currentRound;

                    if (this.m_currentRound != null) {
                        
                        // if some information is missing from the end round..
                        if (this.m_currentRound.IsRoundComplete == false) {

                            if (this.m_currentRound.RoundOverCondition == RoundOverConditions.None) {
                                this.m_currentRound.RoundOverCondition = RoundOverConditions.Unexpected;
                            }

                            this.m_currentRound.LogWinningTeam(0);
                            this.m_currentRound.LogWinningScores(this.Listify<TeamScore>(new TeamScore(1, 0, 0), new TeamScore(2, 0, 0)));
                        }

                        this.m_currentRound.LogRoundEnd();

                        if (this.m_currentRound.RoundOverCondition == RoundOverConditions.ReadiedUp) {
                            this.DisplayRoundScores(this.m_currentRound, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
                        }

                        this.ExecuteCommand("procon.protected.tasks.remove", "PlohoLeagueControlsRoundTimeLimit");
                    }

                    if (this.m_gameModeSettings.Contains(map.GameMode) == true) {

                        this.m_currentRound = new RoundInformation(this.m_fileHostNamePort, localizedTeamNames, this.m_gameModeSettings[map.GameMode], this.GetWeaponDefines(), this.GetSpecializationDefines());
                        this.m_currentRound.GetLocalization += new RoundInformation.GetLocalizationHandler(m_currentRound_GetLocalization);
                        this.m_currentRound.RoundBegin += new RoundInformation.RoundEventHandler(m_currentRound_RoundBegin);
                        this.m_currentRound.RoundEnd += new RoundInformation.RoundEventHandler(m_currentRound_RoundEnd);

                        this.m_currentRound.LogRoundBegin(mapFileName);
                    }
                    else if (this.m_onOffDebug == enumBoolOnOff.On) {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnLoadingLevel(string, int, int): m_gameModeSettings missing " + map.GameMode);
                    }

                    // Now update the playerlist so we know what teams people are on
                    this.ExecuteCommand("admin.listPlayers", "all");

                    // Now update the @map commands in case the gamemode has changed
                    this.RegisterAllCommands();

                    break;
                }
            }
        }

        //private bool m_calculateWinningTeamOnScoreUpdate;

        public void OnRoundTimeout() {

            if (this.m_currentRound != null) {
                CTeamName winningTeam = this.GetTeamNameByFormattedTeamName("{TeamName}", this.GetLocalized("", this.m_currentRound.Gamemode.WinningTeamOnTimeLimitLocalizationKey));

                this.m_currentRound.RoundOverCondition = RoundOverConditions.OutOfRoundTime;

                if (winningTeam.TeamID > 0) {
                    this.ExecuteCommand("procon.protected.send", "admin.endRound", winningTeam.TeamID.ToString());
                }
                // else procon needs to calculate the score.
                else {
                    this.m_currentRound.CalculateWinningTeamOnScoreUpdate = true;
                    this.ExecuteCommand("procon.protected.send", "serverInfo");
                }
                
            }
        }

        private void m_currentRound_RoundBegin(PlohoLeagueControls.RoundInformation sender) {

            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.m_currentRound_RoundBegin: Beginning new round of " + sender.Gamemode.Name);
            }
        }

        private void m_currentRound_RoundEnd(PlohoLeagueControls.RoundInformation sender) {

            if (this.m_onOffDebug == enumBoolOnOff.On) {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.m_currentRound_RoundEnd: Round ended of " + sender.Gamemode.Name + "; RoundOverCondition: " + sender.RoundOverCondition.ToString());
            }

            // Remove all timers
            this.ExecuteCommand("procon.protected.tasks.remove", "PlohoLeagueControlsRoundTimeLimit");
        }

        private string m_currentRound_GetLocalization(string localizationKey) {
            return this.GetLocalized(localizationKey, localizationKey);
        }

        private void DisplayRoundScores(RoundInformation round, CPlayerSubset displayToSubset) {
            
            if (round != null) {
                if (round.IsRoundComplete == true) {
                    this.WriteMessage(round.ToString(), displayToSubset);
                }
                else if (this.m_onOffDebug == enumBoolOnOff.On) {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.DisplayRoundScores: Previous round was incomplete");
                }
            }
        }

        public override void OnRoundOverTeamScores(List<TeamScore> lstTeamScores) {

            if (this.m_currentRound != null) {
                foreach (TeamScore score in lstTeamScores) {
                    if (score.Score == score.WinningScore) {
                        this.m_currentRound.RoundOverCondition = RoundOverConditions.TicketBleed;

                        if (this.m_currentRound.LastLoggedKill != null) {

                            this.WriteMessage(String.Format("Last kill: {0} [{1}{2}] {3}", this.m_currentRound.LastLoggedKill.Killer.SoldierName, (this.m_currentRound.LastLoggedKill.Headshot == true ? "|" : ""), this.GetLocalized("", "global.Weapons." + this.m_currentRound.LastLoggedKill.DamageType.ToLower()), this.m_currentRound.LastLoggedKill.Victim.SoldierName), new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));

                            //this.m_currentRound.LastLoggedKill
                        }

                        break;
                    }
                }

                if (this.m_currentRound.RoundOverCondition == RoundOverConditions.None) {
                    this.m_currentRound.RoundOverCondition = RoundOverConditions.Objectives;
                }

                this.m_currentRound.LogWinningScores(lstTeamScores);

                this.DisplayRoundScores(this.m_currentRound, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));

                this.ExecuteCommand("procon.protected.tasks.remove", "PlohoLeagueControlsRoundTimeLimit");
            }
        }

        public override void OnRoundOver(int iWinningTeamID) {
            if (this.m_currentRound != null) {
                this.m_currentRound.LogWinningTeam(iWinningTeamID);

                this.DisplayRoundScores(this.m_currentRound, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));

                this.ExecuteCommand("procon.protected.tasks.remove", "PlohoLeagueControlsRoundTimeLimit");
            }
        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> lstPlayers) {
            if (this.m_currentRound != null) {
                this.m_currentRound.LogPlayersScores(lstPlayers);
            }
        }

        // Player events
        public override void OnPlayerJoin(string strSoldierName) {
            if (this.m_currentRound != null) {
                this.m_currentRound.UpdatePlayer(strSoldierName, new CPlayerInfo(strSoldierName, "", 0, 24), null, true);

                this.m_currentRound.LogPlayerJoin(strSoldierName);
            }
        }

        public override void OnPlayerLeft(CPlayerInfo cpiPlayer) {
            if (this.m_currentRound != null) {
                this.m_currentRound.LogPlayerLeave(cpiPlayer);
            }
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer) {

            if (this.m_currentRound != null) {
                if (cpbiPlayer != null) {
                    this.m_currentRound.UpdatePlayer(cpbiPlayer.SoldierName, null, cpbiPlayer, false);
                }
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {

            if (this.m_currentRound != null) {
                if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All) {
                    foreach (CPlayerInfo cpiPlayer in lstPlayers) {
                        this.m_currentRound.UpdatePlayer(cpiPlayer.SoldierName, cpiPlayer, null, false);
                    }
                }
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) {
            
            if (this.m_currentRound != null) {
                this.m_currentRound.UpdatePlayer(soldierName, new CPlayerInfo(soldierName, "", 0, 24), null, true);

                this.m_currentRound.LogPlayerSpawn(soldierName, spawnedInventory);

                if (this.m_currentRound.InitialSpawn == false) {
                    this.m_currentRound.InitialSpawn = true;

                    if (this.m_currentRound.Gamemode.UseTimeLimit == enumBoolYesNo.Yes) {

                        if (this.m_onOffDebug == enumBoolOnOff.On) {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnPlayerSpawned(string, Inventory): Gamemode uses time limits");
                        }

                        double timeLimitSeconds = this.m_currentRound.Gamemode.TimeLimit * 60.0D;
                        string message;

                        CTeamName winningTeam = this.GetTeamNameByFormattedTeamName("{TeamName}", this.GetLocalized("", this.m_currentRound.Gamemode.WinningTeamOnTimeLimitLocalizationKey));

                        if (winningTeam != null) {

                            TimeSpan timeLeft = new TimeSpan(TimeSpan.TicksPerSecond * (int)timeLimitSeconds);
                            if (winningTeam.TeamID != 0) {
                                message = String.Format("{0} hold the line for {1:00}:{2:00} minute(s)", this.GetLocalized("", this.m_currentRound.Gamemode.WinningTeamOnTimeLimitLocalizationKey), timeLeft.Minutes, timeLeft.Seconds);
                            }
                            else {
                                message = String.Format("{0:00}:{1:00} minutes remaining..", timeLeft.TotalMinutes, timeLeft.Seconds);
                            }
                            this.ExecuteCommand("procon.protected.send", "admin.say", message, "all");

                            for (double i = timeLimitSeconds; i > this.m_iTimerFinalCountdown - this.m_iTimerDisplayInterval; i -= this.m_iTimerDisplayInterval) {

                                for (int displayCount = (int)i + (this.m_iTimerDisplayLength / 2); displayCount >= (int)i - (this.m_iTimerDisplayLength / 2) && displayCount >= 0; displayCount--) {

                                    TimeSpan displayTime = new TimeSpan(TimeSpan.TicksPerSecond * ((int)timeLimitSeconds - (int)displayCount));

                                    if (displayTime.Minutes >= 0 && displayTime.Seconds >= 0) {
                                        this.ExecuteCommand("procon.protected.tasks.add", "PlohoLeagueControlsRoundTimeLimit", displayCount.ToString(), "1", "1", "procon.protected.send", "admin.yell", String.Format(" {0:00}:{1:00}", displayTime.Minutes, displayTime.Seconds), "1000", "all");
                                    }
                                }
                            }

                            for (int finalCountdown = this.m_iTimerFinalCountdown; finalCountdown >= 0; finalCountdown--) {

                                TimeSpan displayTime = new TimeSpan(TimeSpan.TicksPerSecond * finalCountdown);

                                if (displayTime.Minutes >= 0 && displayTime.Seconds >= 0) {
                                    this.ExecuteCommand("procon.protected.tasks.add", "PlohoLeagueControlsRoundTimeLimit", ((int)timeLimitSeconds - finalCountdown).ToString(), "1", "1", "procon.protected.send", "admin.yell", String.Format(" {0:00}:{1:00}", displayTime.Minutes, displayTime.Seconds), "1000", "all");
                                }
                            }

                            this.ExecuteCommand("procon.protected.tasks.add", "PlohoLeagueControlsRoundTimeLimit", ((int)timeLimitSeconds).ToString(), "1", "1", "procon.protected.plugins.call", "PlohoLeagueControls", "OnRoundTimeout");
                            
                            this.m_currentRound.LogTimersStart();

                            if (this.m_onOffDebug == enumBoolOnOff.On) {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPlohoLeagueControls.OnPlayerSpawned(string, Inventory): Completed round timer task addition; timeLimitSeconds = " + timeLimitSeconds.ToString("0.00"));
                            }
                        }
                        else if (this.m_onOffDebug == enumBoolOnOff.On) {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^1PlohoLeagueControls.OnPlayerSpawned(string, Inventory): Unable to find winning team: " + this.m_currentRound.Gamemode.WinningTeamOnTimeLimitLocalizationKey + "; Localized: " + this.GetLocalized("", this.m_currentRound.Gamemode.WinningTeamOnTimeLimitLocalizationKey));
                        }
                    }
                }
            }
        }

        public override void OnServerInfo(CServerInfo csiServerInfo) {
            if (this.m_currentRound != null) {
                this.m_currentRound.LogScores("ScoreUpdate", csiServerInfo.TeamScores);

                // End of round timer has been declared and the team was neutral
                if (this.m_currentRound.CalculateWinningTeamOnScoreUpdate == true) {
                    this.m_currentRound.CalculateWinningTeamOnScoreUpdate = false;

                    int winningScore = 0;
                    int winningTeamId = 0;

                    foreach (TeamScore score in csiServerInfo.TeamScores) {
                        // All game types highest ticket count == winner?
                        if (score.Score >= winningScore) {
                            winningScore = score.Score;
                            winningTeamId = score.TeamID;
                        }
                    }

                    // If it's a draw then we still need to declare one team the winner anyway.
                    this.ExecuteCommand("procon.protected.send", "admin.endRound", winningTeamId.ToString());
                }
                else if (csiServerInfo.PlayerCount == 0) {
                    this.m_currentRound.LogRoundEnd();

                    if (this.m_previousRound != null) {
                        this.m_previousRound.GetLocalization -= new RoundInformation.GetLocalizationHandler(m_currentRound_GetLocalization);
                        this.m_previousRound.RoundBegin -= new RoundInformation.RoundEventHandler(m_currentRound_RoundBegin);
                        this.m_previousRound.RoundEnd -= new RoundInformation.RoundEventHandler(m_currentRound_RoundEnd);
                    }

                    this.m_previousRound = this.m_currentRound;

                    this.m_currentRound = null;
                }
            }
            else if (csiServerInfo.PlayerCount > 0 && this.m_currentRound == null) {
                this.OnLoadingLevel(csiServerInfo.Map, csiServerInfo.CurrentRound, csiServerInfo.TotalRounds);
            }
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails) {

            if (this.m_currentRound != null) {
                this.m_currentRound.UpdatePlayer(kKillerVictimDetails.Killer.SoldierName, new CPlayerInfo(kKillerVictimDetails.Killer.SoldierName, "", 0, 24), null, true);

                this.m_currentRound.LogKill(kKillerVictimDetails);
            }
        }

        public override void OnGlobalChat(string strSpeaker, string strMessage) {
            if (this.m_currentRound != null && String.Compare(strSpeaker, "Server", true) != 0) {
                this.m_currentRound.LogPlayerChat(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            }
        }

        public override void OnTeamChat(string strSpeaker, string strMessage, int iTeamID) {
            if (this.m_currentRound != null && String.Compare(strSpeaker, "Server", true) != 0) {
                this.m_currentRound.LogPlayerChat(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Team, iTeamID));
            }
        }

        public override void OnSquadChat(string strSpeaker, string strMessage, int iTeamID, int iSquadID) {
            if (this.m_currentRound != null && String.Compare(strSpeaker, "Server", true) != 0) {
                this.m_currentRound.LogPlayerChat(strSpeaker, strMessage, new CPlayerSubset(CPlayerSubset.PlayerSubsetType.Squad, iTeamID, iSquadID));
            }
        }

        public override void OnPlayerTeamChange(string strSoldierName, int iTeamID, int iSquadID) {
            if (this.m_currentRound != null) {
                this.m_currentRound.UpdatePlayer(strSoldierName, new CPlayerInfo(strSoldierName, "", iTeamID, iSquadID), null, true);

                this.m_currentRound.UpdatePlayerTeamSquad(strSoldierName, iTeamID, iSquadID);
            }
        }

        public override void OnPlayerSquadChange(string strSoldierName, int iTeamID, int iSquadID) {
            if (this.m_currentRound != null) {
                this.m_currentRound.UpdatePlayer(strSoldierName, new CPlayerInfo(strSoldierName, "", iTeamID, iSquadID), null, true);

                this.m_currentRound.UpdatePlayerTeamSquad(strSoldierName, iTeamID, iSquadID);
            }
        }

        internal class RoundInformation : Loggable {

            public delegate string GetLocalizationHandler(string localizationKey);
            public event GetLocalizationHandler GetLocalization;

            public delegate void RoundEventHandler(RoundInformation sender);
            public event RoundEventHandler RoundBegin;
            public event RoundEventHandler RoundEnd;

            private List<CTeamName> m_mapTeamNames;
            private ConfirmationCommandDictionary m_teamConfirmationCommands;
            public ConfirmationCommandDictionary TeamConfirmationCommands {
                get {
                    return this.m_teamConfirmationCommands;
                }
            }
  
            private EndRoundFlags m_roundRaceCondition;
            private List<TeamScore> m_roundScores;
            private int m_roundWinningTeamId;

            private DateTime m_roundStart, m_roundEnd;

            private WeaponDictionary m_weaponDefines;
            private SpecializationDictionary m_specializationDefines;

            public DateTime RoundStartTime {
                get {
                    return this.m_roundStart;
                }
            }

            public DateTime RoundEndTime {
                get {
                    return this.m_roundEnd;
                }
            }

            public bool IsLiveRound {
                get {
                    return this.RoundOverCondition != RoundOverConditions.ReadiedUp;
                }
            }

            public bool IsRoundComplete {
                get {
                    return (this.m_roundRaceCondition == (EndRoundFlags.EventWinningTeam | EndRoundFlags.EventTeamScores));
                }
            }

            private RoundOverConditions m_roundOverCondition;
            public RoundOverConditions RoundOverCondition {
                get {
                    return m_roundOverCondition;
                }
                set {
                    this.m_roundOverCondition = value;
                }
            }

            private PlayerInformationDictionary m_dicPlayers;
            public PlayerInformationDictionary Players {
                get {
                    return this.m_dicPlayers;
                }
            }

            private GamemodeInformation m_gameMode;
            public GamemodeInformation Gamemode {
                get {
                    return this.m_gameMode;
                }
            }

            /// <summary>
            /// Only start the timers once a single player has spawned in.
            /// </summary>
            private bool m_initialSpawn;
            public bool InitialSpawn {
                get {
                    return this.m_initialSpawn;
                }
                set {
                    this.m_initialSpawn = value;
                }
            }

            private bool m_calculateWinningTeamOnScoreUpdate;
            public bool CalculateWinningTeamOnScoreUpdate {
                get {
                    return this.m_calculateWinningTeamOnScoreUpdate;
                }
                set {
                    this.m_calculateWinningTeamOnScoreUpdate = value;
                }
            }

            private Kill m_lastLoggedKill;
            public Kill LastLoggedKill {
                get {
                    return this.m_lastLoggedKill;
                }
            }

            public RoundInformation(string fileHostNamePort, List<CTeamName> mapTeamNames, GamemodeInformation gameMode, WeaponDictionary weapons, SpecializationDictionary specializations) {
                this.m_dicPlayers = new PlayerInformationDictionary();
                this.m_gameMode = gameMode;

                this.m_mapTeamNames = mapTeamNames;
                this.m_teamConfirmationCommands = new ConfirmationCommandDictionary();
                foreach (CTeamName teamName in this.m_mapTeamNames) {
                    if (teamName.TeamID > 0) {
                        if (this.m_teamConfirmationCommands.Contains(teamName.LocalizationKey) == false) {
                            this.m_teamConfirmationCommands.Add(new ConfirmationCommand(teamName.LocalizationKey));
                        }
                    }
                }

                this.RoundOverCondition = RoundOverConditions.None;
                this.m_roundStart = DateTime.Now;

                this.FileHostNamePort = fileHostNamePort;
                this.LoggingStartedPrefix = "League Controls logging started";
                this.LoggingStoppedPrefix = "League Controls stopped";
                this.FileNameSuffix = "PlohoLeagueControls";
                this.Logging = true;

                this.m_weaponDefines = weapons;
                this.m_specializationDefines = specializations;
            }

            public void UpdatePlayer(string soldierName, CPlayerInfo vanillaInfo, CPunkbusterInfo punkbusterInfo, bool useOnlyIfNull) {

                if (this.Players.Contains(soldierName) == true) {
                    if (vanillaInfo != null) {
                        if (useOnlyIfNull == false || (useOnlyIfNull == true && this.Players[soldierName].VanillaInfo == null)) {
                            this.Players[soldierName].VanillaInfo = vanillaInfo;
                        }
                    }

                    if (punkbusterInfo != null) {

                        // If it's the first time we've seen this punkbuster info for this round..
                        if (this.Players[soldierName].PunkbusterInfo == null) {
                            this.LogPlayerPunkbusterInfo(punkbusterInfo);
                        }
                            
                        if (useOnlyIfNull == false || (useOnlyIfNull == true && this.Players[soldierName].PunkbusterInfo == null)) {
                            this.Players[soldierName].PunkbusterInfo = punkbusterInfo;
                        }
                    }
                }
                else {

                    PlayerInformation newPlayer = new PlayerInformation(vanillaInfo, punkbusterInfo);
                    newPlayer.GetLocalization += new GetLocalizationHandler(newPlayer_GetLocalization);
                    this.Players.Add(newPlayer);
                }
            }

            public void UpdatePlayerTeamSquad(string soldierName, int teamId, int squadId) {
                if (this.Players.Contains(soldierName) == true && this.Players[soldierName].VanillaInfo == null) {
                    this.Players[soldierName].VanillaInfo.TeamID = teamId;
                    this.Players[soldierName].VanillaInfo.SquadID = squadId;
                }
            }

            private string newPlayer_GetLocalization(string localizationKey) {
                return this.GetLocalization(localizationKey);
            }

            public void LogRoundBegin(string mapFileName) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"RoundBegin\": {{ \"Time\": \"{0}\", \"MapFileName\": \"{1}\" }} }}", String.Format("{0} {1}", this.m_roundStart.ToLongDateString(), this.m_roundStart.ToLongTimeString()), mapFileName));

                if (this.RoundBegin != null) {
                    this.RoundBegin(this);
                }
            }

            public void LogTimersStart() {
                this.WriteLog(DateTime.Now, String.Format("{{ \"TimersStart\": {{ \"Time\": \"{0}\" }} }}", String.Format("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString())));
            }

            public void LogRoundEnd() {
                this.m_roundEnd = DateTime.Now;
                this.WriteLog(DateTime.Now, String.Format("{{ \"RoundEnd\": {{ \"Time\": \"{0}\" }} }}", String.Format("{0} {1}", this.m_roundEnd.ToLongDateString(), this.m_roundEnd.ToLongTimeString())));

                if (this.RoundEnd != null) {
                    this.RoundEnd(this);
                }

                this.Logging = false;
            }

            public void LogKill(Kill kill) {

                Weapon weaponUsed = null;

                this.m_lastLoggedKill = kill;

                if (this.m_weaponDefines.Contains(kill.DamageType) == true) {
                    weaponUsed = this.m_weaponDefines[kill.DamageType];

                    // Record for the @dump command
                    if (weaponUsed != null && this.Players.Contains(kill.Killer.SoldierName) == true) {

                        if (this.Players[kill.Killer.SoldierName].Kills.ContainsKey(weaponUsed) == true) {
                            this.Players[kill.Killer.SoldierName].Kills[weaponUsed]++;
                        }
                        else {
                            this.Players[kill.Killer.SoldierName].Kills.Add(weaponUsed, 1);
                        }
                    }

                    this.WriteLog(DateTime.Now, String.Format("{{ \"Kill\": {{ \"Killer\": \"{0}\", \"Victim\": \"{1}\", \"Weapon\": \"{2}\", \"WeaponLocalized\": \"{3}\", \"Headshot\": {4} }} }}", kill.Killer.SoldierName, kill.Victim.SoldierName, weaponUsed.Name, this.GetLocalization("global.Weapons." + weaponUsed.Name.ToLower()), kill.Headshot.ToString().ToLower()));
                }
            }

            public void LogPlayerPunkbusterInfo(CPunkbusterInfo pbInfo) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"Punkbuster\": {{ \"Player\": \"{0}\", \"SlotID\": {1}, \"GUID\": \"{2}\", \"Ip\": \"{3}\", \"Country\": \"{4}\", \"CountryCode\": \"{5}\" }} }}", pbInfo.SoldierName, pbInfo.SlotID, pbInfo.GUID, pbInfo.Ip, pbInfo.PlayerCountry, pbInfo.PlayerCountryCode));
            }

            private string SerializePlayer(CPlayerInfo player) {
                return String.Format("\"{0}\": {{ \"ClanTag\": \"{1}\", \"GUID\": \"{2}\", \"Score\": {3}, \"Kills\": {4}, \"Deaths\": {5}, \"TeamId\": {6}, \"TeamNameLocalized\": \"{7}\"  }}", player.SoldierName, player.ClanTag, player.GUID, player.Score, player.Kills, player.Deaths, player.TeamID, this.GetTeamName(player.TeamID));
            }

            public void LogPlayerScores(CPlayerInfo player) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"Players\": [ {0} ] }}", this.SerializePlayer(player)));
            }

            public void LogPlayersScores(List<CPlayerInfo> players) {

                List<string> formattedPlayers = new List<string>();

                foreach (CPlayerInfo player in players) {
                    formattedPlayers.Add(this.SerializePlayer(player));
                }

                this.WriteLog(DateTime.Now, String.Format("{{ \"Players\": [ {0} ] }}", String.Join(", ", formattedPlayers.ToArray())));
            }

            private void LogWinningTeamReason() {
                if (this.IsRoundComplete == true) {
                    this.WriteLog(DateTime.Now, String.Format("{{ \"WinningTeam\": {{ \"Teamid\": \"{0}\", \"TeamNameLocalized\": \"{1}\", \"WinCondition\": \"{2}\" }} }}", this.m_roundWinningTeamId, this.GetTeamName(this.m_roundWinningTeamId), this.m_roundOverCondition.ToString()));
                }
            }

            public void LogWinningTeam(int teamId) {

                this.m_roundWinningTeamId = teamId;
                this.m_roundEnd = DateTime.Now;

                this.m_roundRaceCondition |= EndRoundFlags.EventWinningTeam;

                this.LogWinningTeamReason();

                //this.WriteLog(DateTime.Now, String.Format("{{ \"WinningTeam\": {{ \"Teamid\": \"{0}\", \"TeamNameLocalized\": \"{1}\", \"WinCondition\": \"{2}\" }} }}", teamId, this.GetTeamName(teamId), this.m_roundOverCondition.ToString()));
            }

            public void LogWinningScores(List<TeamScore> scores) {

                this.m_roundScores = scores;
                this.m_roundRaceCondition |= EndRoundFlags.EventTeamScores;

                this.LogScores("WinningScores", scores);

                this.LogWinningTeamReason();
            }

            public void LogScores(string keyName, List<TeamScore> scores) {

                List<string> roundScores = new List<string>();

                foreach (TeamScore score in scores) {
                    roundScores.Add(String.Format(" {{ \"{0}\": {{  \"Name\": \"{1}\", \"Score\": \"{2}\"  }} }} ", score.TeamID, this.GetTeamName(score.TeamID), this.GetTeamScore(score.Score, score.TeamID)));
                }

                this.WriteLog(DateTime.Now, String.Format("{{ \"{0}\": [ {1} ] }}", keyName, String.Join(", ", roundScores.ToArray())));
            }

            public void LogReadyCommand(string speaker) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"ReadyUp\": {{ \"Player\": \"{0}\" }} }}", speaker));
            }

            public void LogPlayerChat(string speaker, string message, CPlayerSubset audience) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"Chat\": {{ \"Speaker\": \"{0}\", \"Message\": \"{1}\", \"Scope\": \"{2}\", \"TeamId\": {3}, \"TeamNameLocalized\": \"{4}\" \"SquadId\": {5} }} }}", speaker, message, audience.Subset.ToString(), audience.TeamID, this.GetTeamName(audience.TeamID), audience.SquadID));
            }

            public void LogPlayerSpawn(string soldierName, Inventory items) {

                List<string> formattedWeapons = new List<string>();
                List<string> formattedSpecializations = new List<string>();

                foreach (Weapon weapon in items.Weapons) {
                    formattedWeapons.Add(String.Format("{{ \"Weapon\": {{ \"Name\": \"{0}\", \"NameLocalized\": \"{1}\", \"DamageType\": \"{2}\" }} }}", weapon.Name, this.GetLocalization("global.Weapons." + weapon.Name.ToLower()), weapon.Damage.ToString()));
                }

                foreach (Specialization spec in items.Specializations) {
                    formattedSpecializations.Add(String.Format("{{ \"Specialization\": {{ \"Name\": \"{0}\", \"NameLocalized\": \"{1}\" }} }}", spec.Name, this.GetLocalization("global.Specialization." + spec.Name.ToLower())));
                }

                if (this.InitialSpawn == false) {
                    this.WriteLog(DateTime.Now, String.Format("{{ \"FirstSpawn\": {{ \"Player\": \"{0}\" }} }}", soldierName));
                }

                this.WriteLog(DateTime.Now, String.Format("{{ \"Spawn\": {{ \"Player\": \"{0}\", \"Kit\": \"{1}\", \"Weapons\": [ {2} ], \"Specializations\": [ {3} ] }} }}", soldierName, items.Kit.ToString(), String.Join(", ", formattedWeapons.ToArray()), String.Join(", ", formattedSpecializations.ToArray())));
            }

            public void LogPlayerJoin(string soldierName) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"PlayerJoin\": {{ \"Player\": \"{0}\"}} }}", soldierName));
            }

            public void LogPlayerLeave(CPlayerInfo statsOnLeave) {
                this.WriteLog(DateTime.Now, String.Format("{{ \"PlayerLeave\": {{ \"Player\": \"{0}\"}} }}", statsOnLeave.SoldierName));
                this.LogPlayerScores(statsOnLeave);
            }

            private string GetTeamName(int teamId) {

                string returnTeamName = String.Empty;

                foreach (CTeamName teamName in this.m_mapTeamNames) {
                    if (teamName.TeamID == teamId) {
                        returnTeamName = teamName.ImageKey;
                        break;
                    }
                }

                return returnTeamName;
            }

            private string GetTeamLocalizationKey(int teamId) {

                string returnTeamName = String.Empty;

                foreach (CTeamName teamName in this.m_mapTeamNames) {
                    if (teamName.TeamID == teamId) {
                        returnTeamName = teamName.LocalizationKey;
                        break;
                    }
                }

                return returnTeamName;
            }

            private string GetTeamScore(int score, int teamId) {

                string returnScore = String.Empty;

                foreach (CTeamName teamName in this.m_mapTeamNames) {
                    if (teamName.TeamID == teamId) {

                        if (String.Compare(teamName.LocalizationKey, "global.rush.defenders", true) == 0) {
                            returnScore = "Infinite";
                        }
                        else {
                            returnScore = score.ToString();
                        }

                        break;
                    }
                }

                return returnScore;
            }

            private void WriteLog(DateTime dtLoggedTime, string strText) {
                this.WriteLogLine(String.Format("[{0}] {1}", dtLoggedTime.ToString("HH:mm:ss"), strText.Replace("{", "{{").Replace("}", "}}")));
            }

            public string GetPlayerTeamLocalizationKey(string soldierName) {

                string returnKey = String.Empty;

                if (this.Players.Contains(soldierName) == true && this.Players[soldierName].VanillaInfo != null) {
                    returnKey = this.GetTeamLocalizationKey(this.Players[soldierName].VanillaInfo.TeamID);
                }

                return returnKey;
            }

            private string RoundOverConditionsToString(RoundOverConditions condition) {
                string returnCondition = String.Empty;

                if (condition == RoundOverConditions.None || condition == RoundOverConditions.Unexpected) {
                    returnCondition = "Unknown";
                }
                else if (condition == RoundOverConditions.TicketBleed) {
                    returnCondition = "Tickets";
                }
                else if (condition == RoundOverConditions.Objectives) {
                    returnCondition = "Objectives";
                }
                else if (condition == RoundOverConditions.OutOfRoundTime) {
                    returnCondition = "Time Limit";
                }
                else if (condition == RoundOverConditions.ReadiedUp) {
                    returnCondition = "Readied Up";
                }

                return returnCondition;
            }

            public override string ToString() {

                List<string> roundScores = new List<string>();

                foreach (TeamScore score in this.m_roundScores) {
                    roundScores.Add(String.Format("{0} {1}", this.GetTeamName(score.TeamID), this.GetTeamScore(score.Score, score.TeamID)));
                }

                return String.Format("{0} {1} to {2} {3}Scores: {4} - Winner: {5} by {6}", this.m_roundStart.ToShortDateString(), this.m_roundStart.ToShortTimeString(), this.m_roundEnd.ToShortTimeString(), this.IsLiveRound == false ? "WARMUP " : "", String.Join(", ", roundScores.ToArray()), this.GetTeamName(this.m_roundWinningTeamId), this.RoundOverConditionsToString(this.m_roundOverCondition));
            }
        }

        #region Confirmation Commands

        internal class ConfirmationCommand {

            private string m_teamNameLocalizationKey;
            public string TeamNameLocalizationKey {
                get {
                    return this.m_teamNameLocalizationKey;
                }
                set {
                    this.m_teamNameLocalizationKey = value;
                }
            }

            private bool m_readyUpConfirmed;
            
            public bool ReadyUpConfirmed {
                get {
                    return this.m_readyUpConfirmed;
                }
            }
            
            public bool ToggleReadyUpConfirmed() {

                return (this.m_readyUpConfirmed = !m_readyUpConfirmed);
            }

            private string m_proposedMapChange;
            public string ProposedMapChange {
                get {
                    return this.m_proposedMapChange;
                }
                set {
                    this.m_proposedMapChange = value;
                }
            }

            private bool m_timeoutConfirmed;
            public bool TimeoutConfirmed {
                get {
                    return this.m_timeoutConfirmed;
                }
                set {
                    this.m_timeoutConfirmed = value;
                }
            }

            public ConfirmationCommand(string teamNameLocalizationKey) {
                this.TeamNameLocalizationKey = teamNameLocalizationKey;
                this.ProposedMapChange = String.Empty;
            }
        }

        internal class ConfirmationCommandDictionary : KeyedCollection<string, ConfirmationCommand> {

            protected override string GetKeyForItem(ConfirmationCommand item) {
                return item.TeamNameLocalizationKey;
            }

            public bool IsReadyUpConfirmed(int minimumTeams) {
                int readyTeams = 0;

                foreach (ConfirmationCommand command in this) {
                    if (command.ReadyUpConfirmed == true) {
                        readyTeams++;
                    }
                }

                return (readyTeams >= minimumTeams);
            }

            // I'll edit this if a majority rule is needed later for a SQDM match..
            // though I couldn't think why now so its not.
            public string GetConfirmedMap(int minimumTeams, bool demeritDifferences) {
                int confirmedTeams = 0;
                string mapFileName = String.Empty;

                foreach (ConfirmationCommand command in this) {

                    if (command.ProposedMapChange.Length > 0) {
                        if (mapFileName.Length == 0) {
                            mapFileName = command.ProposedMapChange;
                            confirmedTeams++;
                        }
                        else {
                            if (String.Compare(mapFileName, command.ProposedMapChange) == 0) {
                                confirmedTeams++;
                            }
                            else {
                                confirmedTeams--;
                            }
                        }

                        if (confirmedTeams >= minimumTeams) {
                            // Found our map, return it.
                            break;
                        }
                    }
                    else if (demeritDifferences == true) {
                        confirmedTeams--;
                    }
                }

                if (confirmedTeams < minimumTeams) {
                    mapFileName = String.Empty;
                }

                return mapFileName;
            }

            public bool IsTimeoutConfirmed(int minimumTeams) {
                int timeoutTeams = 0;

                foreach (ConfirmationCommand command in this) {
                    if (command.TimeoutConfirmed == true) {
                        timeoutTeams++;
                    }
                }

                return (timeoutTeams >= minimumTeams);
            }
        }

        #endregion

        #region Gamemode

        internal class GamemodeInformationDictionary : KeyedCollection<string, GamemodeInformation> {

            protected override string GetKeyForItem(GamemodeInformation item) {
                return item.Name;
            }

            public new GamemodeInformation this[string key] {
                get {
                    GamemodeInformation keyedGamemode = null;

                    foreach (GamemodeInformation gamemode in this) {
                        if (String.Compare(gamemode.Name, key, true) == 0) {
                            keyedGamemode = gamemode;
                            break;
                        }
                    }

                    return keyedGamemode;
                }
            }

            public new bool Contains(string key) {

                bool isKeyed = false;

                foreach (GamemodeInformation gamemode in this) {
                    if (String.Compare(gamemode.Name, key, true) == 0) {
                        isKeyed = true;
                        break;
                    }
                }

                return isKeyed;
            }
        }

        internal class GamemodeInformation {

            private string m_gamemodeName;
            public string Name {
                get {
                    return this.m_gamemodeName;
                }
            }

            private string m_playListName;
            public string PlayListName {
                get {
                    return this.m_playListName;
                }
            }

            private enumBoolYesNo m_useTimeLimit;
            public enumBoolYesNo UseTimeLimit {
                get {
                    return this.m_useTimeLimit;
                }
                set {
                    this.m_useTimeLimit = value;
                }
            }

            private double m_timeLimit;
            public double TimeLimit {
                get {
                    return this.m_timeLimit;
                }
                set {
                    this.m_timeLimit = value;
                }
            }

            private string m_winningTeamOnTimeLimitLocalizationKey;
            public string WinningTeamOnTimeLimitLocalizationKey {
                get {
                    return this.m_winningTeamOnTimeLimitLocalizationKey;
                }
                set {
                    this.m_winningTeamOnTimeLimitLocalizationKey = value;
                }
            }

            private string[] ma_allowedMapListFileNames;
            public string[] AllowedMapListFileNames {
                get {
                    return this.ma_allowedMapListFileNames;
                }
                set {
                    this.ma_allowedMapListFileNames = value;
                }
            }

            public GamemodeInformation(string gameMode, string playlistName, string[] a_allowedMapListFileNames, string winningTeamOnTimeLimitLocalizationKey) {
                this.m_gamemodeName = gameMode;
                this.m_playListName = playlistName;
                this.m_useTimeLimit = enumBoolYesNo.No;
                this.m_timeLimit = 15.0D;

                this.AllowedMapListFileNames = a_allowedMapListFileNames;

                this.m_winningTeamOnTimeLimitLocalizationKey = winningTeamOnTimeLimitLocalizationKey;
            }
        }

        #endregion

        internal class PlayerInformationDictionary : KeyedCollection<string, PlayerInformation> {

            protected override string GetKeyForItem(PlayerInformation item) {
                return item.SoldierName;
            }
        }

        internal class PlayerInformation {

            public event RoundInformation.GetLocalizationHandler GetLocalization;

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

            public string PartialGuid {
                get {
                    string returnGuid = String.Empty;

                    if (this.PunkbusterInfo != null) {
                        if (this.PunkbusterInfo.GUID.Length >= 32) {
                            returnGuid = this.PunkbusterInfo.GUID.Substring(24, 8);
                        }
                    }

                    return returnGuid;
                }
            }

            private Dictionary<Weapon, int> m_kills;
            public Dictionary<Weapon, int> Kills {
                get {
                    return m_kills;
                }
            }

            public PlayerInformation(CPlayerInfo vanillaInfo, CPunkbusterInfo punkbusterInfo) {
                this.VanillaInfo = vanillaInfo;
                this.PunkbusterInfo = punkbusterInfo;

                this.m_kills = new Dictionary<Weapon, int>();
            }

            public override string ToString() {

                List<string> localizedKills = new List<string>();

                foreach (KeyValuePair<Weapon, int> kill in this.Kills) {
                    localizedKills.Add(String.Format("{0} = {1}", this.GetLocalization(String.Format("global.Weapons.{0}", kill.Key.Name.ToLower())), kill.Value));
                }

                return String.Format("{0}; {1}; {2}", this.SoldierName, this.PartialGuid, String.Join("; ", localizedKills.ToArray()));
            }
        }

        internal enum RoundOverConditions {
            None,
            Unexpected,
            TicketBleed,
            Objectives,
            OutOfRoundTime,
            ReadiedUp,
            Timeout,
        }

        internal enum EndRoundFlags {
            None,
            EventTeamScores,
            EventWinningTeam,
        }
    }
}