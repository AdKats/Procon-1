/*
BF4DB Server Protection by skuIIs is licensed under a Creative Commons Attribution-NoDerivatives 4.0 International License.
Permissions beyond the scope of this license may be available at https://bf4db.com/tos.
*/

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Data;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Reflection;

using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;

namespace PRoConEvents
{

    public class checkPlayer
    {
        public string currentSpeaker;
        public string currentMessage;
    }

    public class kickPlayer
    {
        public string PlayerName;
        public string bf4db_APIKey;
    }

    public class checkChat
    {
        public string currentSpeaker;
        public string currentMessage;
    }

    public class BF4DBPlayer
    {
        string name;
        bool isChecked;
        bool punkbuster;
        bool spawnLooped;
        bool whiteListed;
        bool manualCheck;
        DateTime lastUpdate;
    }
    public class BF4DB : PRoConPluginAPI, IPRoConPluginInterface
    {

        /* Inherited:
            this.PunkbusterPlayerInfoList = new Dictionary<string, CPunkbusterInfo>();
            this.FrostbitePlayerInfoList = new Dictionary<string, CPlayerInfo>();
        */
        public string version = "2.0.16";
        #region globalVars
        private bool bf4db_IsEnabled;
        private bool bf4db_IsValid;
        private int bf4db_DebugLevel;
        private String bf4db_APIKey;
		enumBoolYesNo bf4db_EnableVPNKick;
        enumBoolYesNo bf4db_EnableGlitchKick;
        enumBoolYesNo bf4db_EnableHackedKick;
        enumBoolYesNo bf4db_EnableAutoBan;
        enumBoolYesNo bf4db_EnableAnnouncements;
        enumBoolYesNo bf4db_EnableCleanAnnouncements;
        enumBoolYesNo bf4db_EnableVerifiedAnnouncements;
        private String bf4db_CheckCommand;
        private WebClient bf4db_client;
        private List<BF4DBPlayer> bf4db_players;
        private List<string> bf4db_AllPlayers;
        private List<string> bf4db_CheckedPlayers;
        private List<string> bf4db_PBPlayers;
        private List<string> bf4db_whitelist;
        private List<string> bf4db_manualChecks;
        private DateTime bf4db_lastServerUpdate;
        private DateTime bf4db_lastPlayerUpdate;
        private Int64 bf4db_ServerType;
        private Int64 bf4db_currentRound;
        private object bf4db_API;
        private Boolean AdKatsIntegration;
        public enum MessageType { Success, Warning, Error, Exception, Normal };
        private string bf4db_currentMap;
        #endregion

        #region Variable setups
        public BF4DB()
        {
            this.bf4db_IsEnabled = false;
            this.bf4db_IsValid = false;
            this.bf4db_APIKey = "";
			this.bf4db_EnableVPNKick = enumBoolYesNo.No;
            this.bf4db_EnableGlitchKick = enumBoolYesNo.No;
            this.bf4db_EnableHackedKick = enumBoolYesNo.Yes;
            this.bf4db_EnableAutoBan = enumBoolYesNo.Yes;
            this.bf4db_EnableAnnouncements = enumBoolYesNo.Yes;
            this.bf4db_EnableCleanAnnouncements = enumBoolYesNo.No;
            this.bf4db_EnableVerifiedAnnouncements = enumBoolYesNo.No;
            this.bf4db_CheckCommand = "check";
            this.bf4db_DebugLevel = 0;
            bf4db_players = new List<BF4DBPlayer>();
            bf4db_AllPlayers = new List<string>();
            bf4db_CheckedPlayers = new List<string>();
            bf4db_PBPlayers = new List<string>();
            bf4db_whitelist = new List<string>();
            bf4db_manualChecks = new List<string>();
            bf4db_client = null;
            this.bf4db_lastServerUpdate = DateTime.MinValue;
            this.bf4db_lastPlayerUpdate = DateTime.MinValue;
            this.bf4db_ServerType = 0;
            this.bf4db_currentRound = 0;
            this.AdKatsIntegration = false;

            String fullPath = "";
            if (Type.GetType("Mono.Runtime") != null)
            {
                fullPath = Path.GetFullPath(@"Plugins/BF4/BF4DB_API.cs");
            }
            else
            {
                fullPath = Path.GetFullPath(@"Plugins\BF4\BF4DB_API.cs");
            }

            Assembly myDllAssembly = Assembly.LoadFrom(fullPath);
            bf4db_API = myDllAssembly.CreateInstance("BF4DB_API.BF4DB_API");

        }
        #endregion

        #region helper functions
        public String FormatMessage(String msg, MessageType type)
        {
            String prefix = "[^bBF4DB] ";

            if (type.Equals(MessageType.Success))
                prefix += "^2Success: ";
            else if (type.Equals(MessageType.Warning))
                prefix += "^1WARNING: ";
            else if (type.Equals(MessageType.Error))
                prefix += "^1ERROR: ";
            else if (type.Equals(MessageType.Exception))
                prefix += "^1EXCEPTION: ";
            return prefix + msg;
        }

        public void LogWrite(String msg)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }

        public void ConsoleWrite(string msg, MessageType type)
        {
            LogWrite(FormatMessage(msg, type));
        }

        public void ConsoleWrite(string msg)
        {
            ConsoleWrite(msg, MessageType.Normal);
        }

        public void ConsoleWarn(String msg)
        {
            ConsoleWrite(msg, MessageType.Warning);
        }

        public void ConsoleError(String msg)
        {
            ConsoleWrite(msg, MessageType.Error);
        }

        public void ConsoleException(String msg)
        {
            ConsoleWrite(msg, MessageType.Exception);
        }

        public void DebugWrite(string msg, int level)
        {
            if (bf4db_DebugLevel >= level) ConsoleWrite(msg, MessageType.Normal);
        }


        public void ServerCommand(params String[] args)
        {
            List<string> list = new List<string>();
            list.Add("procon.protected.send");
            list.AddRange(args);
            this.ExecuteCommand(list.ToArray());
        }
        #endregion

        public string GetPluginName()
        {
            return "BF4DB";
        }

        public string GetPluginVersion()
        {
            return "2.0.16";
        }

        public string GetPluginAuthor()
        {
            return "skuIIs";
        }

        public string GetPluginWebsite()
        {
            return "https://bf4db.com";
        }

        public string GetPluginDescription()
        {
            return @"
	<h1><img src=""http://bf4db.com/assets/images/logo.png"" width=""60px""> SERVER PROTECTION</h1>

	<h2>Description</h2>
	<p>This plugin will protect your server from cheaters using our extensive and trusted ban list. Any cheater caught on this server that is banned on BF4DB.com will be removed.</p>

	<h2>Commands</h2>
	<p>Commands can be preceded by any of the following: ! @ #</p>
	<p>
		<blockquote>
			<h4>!check</h4>
			Queues a player for verification<br/>
		</blockquote>
	</p>

	<h2>Settings</h2>
	<h4>API Key - </h4>
	<p>You must claim this server on BF4DB.com to receive an API Key for this server</p>
	<br/>
	<h4>Enable Auto Bans - </h4>
	<p>When set to Yes, any players banned on BF4DB will be removed from your server. Defaults to Yes.</p>
	<br/>
    <h4>Enable VPN Kicks - </h4>
	<p>When set to Yes, any players using a VPN will be removed from your server. Defaults to No.</p>
	<br/>
    <h4>Enable Glitcher Kicks - </h4>
	<p>When set to Yes, any players marked as a glitcher on BF4DB will be removed from your server. Defaults to No.</p>
	<br/>
    <h4>Enable Hacked Account Kicks - </h4>
	<p>When set to Yes, any players marked as a hacked account on BF4DB will be removed from your server. Defaults to Yes.</p>
	<br/>
	<h4>Enable Announcements - </h4>
	<p>When set to Yes, player status during verification will be announced in chat. Defaults to Yes.</p>
	<br/>
	<h4>Player Whitelist - </h4>
	<p>Place any player names(one per line) to be excluded from BF4DB checks and server removal.</p>
	<br/>
	<h4>Debug Level - </h4>
	<p>Mainly for BF4DB developers. If you encounter an issue please set to one of the following levels before submitting a bug report:</p>
	<ul>
		<li><b>0</b>: Disabled. Not debugging in the console will occur.</li>
		<li><b>1</b>: Only player information will be logged.</li>
		<li><b>2</b>: Player and server information will be logged.</li>
		<li><b>3</b>: Player, server, and PunkBuster information will be logged.</li>
	</ul>

	<h2>Development</h2>
	<p>For any support or bug reports please visit our forums <a href=""http://bf4db.com/forum/thread/bf4db-procon-plugin-support-122"">here</a></p>
	<h3>Changelog</h3>
    <blockquote>
		<h4>2.0.16 (02-FEBRUARY-2023)</h4>
		- Added kick option for glitchers.<br/>
        - Added kick option for hacked accounts.<br/>
	</blockquote>
    <blockquote>
		<h4>2.0.15 (19-JULY-2020)</h4>
		- Update to kick method.<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.14 (13-JUNE-2020)</h4>
		- Added VPN checking, credits to Russao.<br/>
	</blockquote>
    <blockquote>
		<h4>2.0.13 (27-DECEMBER-2019)</h4>
		- Fixed issue with running on ProCon version 1.5.3.1.<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.12 (02-OCTOBER-2018)</h4>
		- Threading rewrite<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.11 (17-SEPTEMBER-2018)</h4>
		- Small performance fix<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.9 (18-OCTOBER-2017)</h4>
		- Changed wording<br/>
	</blockquote>

	<blockquote>
		<h4>2.0.8 (22-AUGUST-2017)</h4>
		- Fix for !check command<br/>
	</blockquote>

	<blockquote>
		<h4>2.0.7 (20-AUGUST-2017)</h4>
		- Fixed an issue with weapon reporting<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.5 (19-AUGUST-2017)</h4>
		- Added options to enable clean and whitelisted player announcments<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.4 (15-AUGUST-2017)</h4>
		- Added plugin versioning and improved load times around the board<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.3 (27-JULY-2017)</h4>
		- ReMoVeD kIcKs On OfFiCiAl SeRvErS bEcAuSe DiCe/Ea<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.2 (21-JULY-2017)</h4>
		- Fixed player kicks on official servers<br/>
		- Bug fixes<br/>
	</blockquote>
	<blockquote>
		<h4>2.0.1 (17-JULY-2017)</h4>
		- Optimized plugin performance<br/>
		- Added proper Adkats support<br/>
		- Reduced debug spam<br/>
	</blockquote>
	<br/>
	<blockquote>
		<h4>1.0.0 (25-MAY-2017)</h4>
		- initial version<br/>
	</blockquote>";
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("API Key", bf4db_APIKey.GetType(), this.bf4db_APIKey));			
			lstReturn.Add(new CPluginVariable("Enable VPN Kick", typeof(enumBoolYesNo), this.bf4db_EnableVPNKick));
            lstReturn.Add(new CPluginVariable("Enable Glitcher Kick", typeof(enumBoolYesNo), this.bf4db_EnableGlitchKick));
            lstReturn.Add(new CPluginVariable("Enable Hacked Account Kick", typeof(enumBoolYesNo), this.bf4db_EnableHackedKick));
            lstReturn.Add(new CPluginVariable("Enable Auto Bans", typeof(enumBoolYesNo), this.bf4db_EnableAutoBan));
            lstReturn.Add(new CPluginVariable("Enable Cheat Announcements", typeof(enumBoolYesNo), this.bf4db_EnableAnnouncements));
            lstReturn.Add(new CPluginVariable("Enable Clean Announcements", typeof(enumBoolYesNo), this.bf4db_EnableCleanAnnouncements));
            lstReturn.Add(new CPluginVariable("Enable Whitelist Announcements", typeof(enumBoolYesNo), this.bf4db_EnableVerifiedAnnouncements));
            lstReturn.Add(new CPluginVariable("Whitelist", typeof(string[]), this.bf4db_whitelist.ToArray()));
            lstReturn.Add(new CPluginVariable("Debug Level", bf4db_DebugLevel.GetType(), this.bf4db_DebugLevel));
            lstReturn.Add(new CPluginVariable("Check Command(/!#@)", bf4db_CheckCommand.GetType(), this.bf4db_CheckCommand));

            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            if (strVariable.CompareTo("API Key") == 0)
            {
                this.bf4db_APIKey = strValue;
            }
			if (strVariable.CompareTo("Enable VPN Kick") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableVPNKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Glitcher Kick") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableGlitchKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Hacked Account Kick") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableHackedKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Auto Bans") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableAutoBan = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Cheat Announcements") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableAnnouncements = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Clean Announcements") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableCleanAnnouncements = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Enable Whitelist Announcements") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.bf4db_EnableVerifiedAnnouncements = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("Whitelist") == 0)
            {
                this.bf4db_whitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            if (strVariable.CompareTo("Check Command(/!#@)") == 0)
            {
                this.bf4db_CheckCommand = strValue;
            }
            if (strVariable.CompareTo("Debug Level") == 0)
            {
                int tmp = 1;
                int.TryParse(strValue, out tmp);
                this.bf4db_DebugLevel = tmp;
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            var events = new[]
            {
				"OnServerInfo",
				"OnServerName",
				"OnServerType",
				"OnListPlayers",
				"OnPlayerLeft",
				"OnPunkbusterMessage",
				"OnPunkbusterPlayerInfo",
				"OnLevelLoaded",
				"OnGlobalChat",
				"OnTeamChat",
				"OnVersion",
				"OnSquadChat",
				"OnPlayerKilled",
			};

            this.RegisterEvents(this.GetType().Name, events);
            // should only send once
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }

        public void OnPluginEnable()
        {
            // should only send once
            this.ExecuteCommand("procon.protected.send", "version");
        }

        public void OnPluginDisable()
        {
            bf4db_IsEnabled = false;
            bf4db_IsValid = false;
            AdKatsIntegration = false;
            bf4db_players.Clear();
            bf4db_AllPlayers.Clear();
            bf4db_CheckedPlayers.Clear();
            bf4db_PBPlayers.Clear();
            bf4db_manualChecks.Clear();

            ConsoleWrite("Disabled!");
        }

        public override void OnGlobalChat(string speaker, string message)
        {
            if (bf4db_IsValid == true)
            {
                checkChat chat = new checkChat();
                chat.currentSpeaker = speaker;
                chat.currentMessage = message;
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkCommand), chat);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public override void OnVersion(string serverType, string version)
        {
            ConsoleWrite("OnVersion" + version + " " + serverType);
            if (bf4db_IsValid == false)
            {
                PluginLogin(version);
            }
        }

        public void PluginLogin(string version)
        {
            this.ExecuteCommand("procon.protected.send", "serverInfo");
            bf4db_IsEnabled = true;
            verifyKey(bf4db_APIKey, version);
            ConsoleWrite("Plugin Enabled!");
            Boolean adKatsFound = GetRegisteredCommands().Any(command => command.RegisteredClassname == "AdKats" && command.RegisteredMethodName == "PluginEnabled");
            if (adKatsFound)
            {
                ConsoleWrite("Adkats found and running!");
                this.AdKatsIntegration = true;
            }
            this.bf4db_lastPlayerUpdate = DateTime.UtcNow;
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }


        public override void OnTeamChat(string speaker, string message, int teamId)
        {
            if (bf4db_IsValid == true)
            {
                checkChat chat = new checkChat();
                chat.currentSpeaker = speaker;
                chat.currentMessage = message;
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkCommand), chat);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            if (bf4db_IsValid == true)
            {
                checkChat chat = new checkChat();
                chat.currentSpeaker = speaker;
                chat.currentMessage = message;
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkCommand), chat);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public override void OnServerInfo(CServerInfo serverInfo)
        {
            if (bf4db_IsValid == true)
            {
                this.bf4db_currentMap = serverInfo.Map;
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadServerUpdate), null);
            }
        }

        public override void OnServerName(string serverName)
        {
            if (bf4db_IsValid == true)
            {
                DebugWrite("Server Name " + serverName, 2);
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadServerUpdate), null);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public override void OnServerType(String serverType)
        {
            if (bf4db_IsValid == true)
            {
                DebugWrite("Server Type " + bf4db_ServerType, 2);
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadServerUpdate), null);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public override void OnPlayerKilled(Kill k)
        {
            if (bf4db_IsValid == true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadKill), k);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public void threadKill(Object state)
        {
            Kill k = (Kill)state;
            try
            {
                if (k == null)
                {
                    DebugWrite("Error getting kill code", 1);
                    return;
                }

                if (k.Killer == null)
                {
                    DebugWrite("Error getting killer object", 1);
                    return;
                }

                string killer = k.Killer.SoldierName;
                string weapon = k.DamageType;
                string[] dragonsTeeth = { "XP3_MarketPl", "XP3_Prpganda", "XP3_UrbanGdn", "XP3_WtrFront" };
                string[] finalStand = { "XP4_Arctic", "XP4_SubBase", "XP4_Titan", "XP4_WlkrFtry" };
                string[] suspectWeapons = {
					"U_Medkit",
					"U_Ammobag",
					"U_PortableMedicpack",
					"Weapons/Gadgets/SOFLAM/SOFLAM_PDA",
					"Gameplay/Gadgets/SOFLAM/SOFLAM_Projectile",
					"U_UGS",
					"U_Handflare",
					"U_M18"
				};

                if (suspectWeapons.Contains(weapon))
                {
                    violationWeapon(killer, weapon, bf4db_APIKey);
                    return;
                }

                //Railgun or XD-1 Accipator kill
                if (weapon == "U_Railgun" || weapon == "XP4/Gameplay/Gadgets/MKV/MKV")
                {
                    if (!finalStand.Contains(this.bf4db_currentMap))
                    {
                        violationWeapon(killer, weapon, bf4db_APIKey);
                        return;
                    }
                }

                //Rawr kill
                if (weapon == "XP3/Gameplay/Vehicles/RAWR/RAWR")
                {
                    if (!dragonsTeeth.Contains(this.bf4db_currentMap))
                    {
                        violationWeapon(killer, weapon, bf4db_APIKey);
                        return;
                    }
                }


            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
            }
        }

        public void threadServerUpdate(Object state)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - this.bf4db_lastServerUpdate;
            double mins = elapsed.TotalMinutes;
            int minsAgo = (int)mins;
            if (minsAgo >= 1)
            {
                updateServer(bf4db_APIKey);
            }
            else
            {
                DebugWrite("Not time to update server. Updated: " + mins + " mins ago", 2);
            }
        }

        public bool lastPlayerCheck()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - this.bf4db_lastPlayerUpdate;
            double secs = elapsed.TotalSeconds;
            int secsAgo = (int)secs;
            if (secsAgo > 10 || secsAgo < 0)
            {
                return true;
            }
            else
            {
                DebugWrite("Not time to update player. Updated: " + secsAgo + " seconds ago " + elapsed.TotalSeconds, 2);
                return false;
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            if (bf4db_IsValid == true)
            {
                if (!lastPlayerCheck())
                {
                    return;
                }
                else
                {
                    this.bf4db_lastPlayerUpdate = DateTime.UtcNow;
                }
                DebugWrite("Getting Player List", 1);
                foreach (CPlayerInfo player in players)
                {
                    CPlayerInfo currentPlayer = player;
                    lock (currentPlayer)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(threadPlayer), currentPlayer);
                    }
                }
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public void threadPlayer(Object state)
        {
            CPlayerInfo player = (CPlayerInfo)state;			

            lock (player)
            {
                if (!bf4db_AllPlayers.Contains(player.SoldierName))
                {
                    bf4db_AllPlayers.Add(player.SoldierName);
                }

                //If the users pb info is available and the user hasnt yet been checked do so
                if (!bf4db_CheckedPlayers.Contains(player.SoldierName) && bf4db_PBPlayers.Contains(player.SoldierName))
                {
                    // add to the checked players list and remove from manual check list then check the player
                    DebugWrite("Verifying player " + player.SoldierName, 1);
                    bf4db_CheckedPlayers.Add(player.SoldierName);
                    // check the player
                    int checkResult = checkPlayer(player.SoldierName, player.GUID, bf4db_APIKey); 
                }
                //Not in already bf4db_CheckedPlayers as clean or bf4db_SpawnLoopPlayers as banned and does NOT have pb info
                if (!bf4db_CheckedPlayers.Contains(player.SoldierName) && !bf4db_PBPlayers.Contains(player.SoldierName))
                {
                    DebugWrite("Waiting on PB for player " + player.SoldierName, 1);
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            // list cleanup
            if (bf4db_AllPlayers.Contains(playerInfo.SoldierName))
            {
                bf4db_AllPlayers.Remove(playerInfo.SoldierName);
            }
            if (bf4db_CheckedPlayers.Contains(playerInfo.SoldierName))
            {
                bf4db_CheckedPlayers.Remove(playerInfo.SoldierName);
            }
            if (bf4db_PBPlayers.Contains(playerInfo.SoldierName))
            {
                bf4db_PBPlayers.Remove(playerInfo.SoldierName);
            }
            if (bf4db_manualChecks.Contains(playerInfo.SoldierName))
            {
                bf4db_manualChecks.Remove(playerInfo.SoldierName);
            }
        }

        public void OnPunkbusterMessage(string strPunkbusterMessage)
        {
            if (bf4db_IsValid == true)
            {
                string message = strPunkbusterMessage;
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadPunkbusterMessage), message);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public void threadPunkbusterMessage(Object state)
        {
            string strPunkbusterMessage = (string)state;

            string violation = @"PunkBuster Server: VIOLATION \(([a-zA-Z]+)\) \#([0-9]+)\: ([a-zA-Z0-9_\-]+) \(slot #([0-9]+)\) Violation \(([a-zA-Z]+)\) \#([0-9]+) \[([0-9a-f]{32})\(-\) (([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3}))\:([0-9]{1,5})\]";

            Match computedViolation = Regex.Match(strPunkbusterMessage, violation, RegexOptions.IgnoreCase);
            if (computedViolation.Success)
            {
                ConsoleWarn(strPunkbusterMessage);
                String playerName = computedViolation.Groups[3].Value;
                String encodedMsg = System.Uri.EscapeDataString(strPunkbusterMessage);
                violationPlayer(playerName, encodedMsg, bf4db_APIKey);
            }
        }

        public void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer)
        {
            if (bf4db_IsValid == true)
            {
                CPunkbusterInfo player = cpbiPlayer;
                ThreadPool.QueueUserWorkItem(new WaitCallback(threadPunkbusterPlayerInfo), player);
            }
            else
            {
                ConsoleError("API Key is invalid! Please fix it and toggle plugin.");
            }
        }

        public void threadPunkbusterPlayerInfo(Object state)
        {
            CPunkbusterInfo cpbiPlayer = (CPunkbusterInfo)state;

            if (!bf4db_PBPlayers.Contains(cpbiPlayer.SoldierName))
            {
                bf4db_PBPlayers.Add(cpbiPlayer.SoldierName);
                updatePB(cpbiPlayer, bf4db_APIKey);
            }
            else
            {
                DebugWrite("Player " + cpbiPlayer.SoldierName + " already updated!", 1);
            }
        }

        public override void OnLevelLoaded(String strMapFileName, String strMapMode, Int32 roundsPlayed, Int32 roundsTotal)
        {
            bf4db_players.Clear();
            bf4db_AllPlayers.Clear();
            bf4db_CheckedPlayers.Clear();
            bf4db_PBPlayers.Clear();
            bf4db_manualChecks.Clear();
            DebugWrite("New round, lists cleared!", 1);
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }

        public void checkCommand(Object state)
        {
            checkChat chat = (checkChat)state;
            String speaker = chat.currentSpeaker;
            String message = chat.currentMessage;

            if (speaker != "server" && speaker != "Server" && speaker != "")
            {
                String command = @"[/!@#]" + @bf4db_CheckCommand + @"\s+([^\s]+)";
                Match cmd = Regex.Match(message, command, RegexOptions.IgnoreCase);
                if (cmd.Success)
                {
                    DebugWrite(speaker + ": " + message, 1);
                    int found = 0;
                    String name = cmd.Groups[1].Value;
                    String target = null;
                    DebugWrite("checking for name" + name, 1);
                    foreach (String player in bf4db_AllPlayers)
                    {
                        DebugWrite(player, 1);
                        if (player == null)
                            continue;

                        if (Regex.Match(player, name, RegexOptions.IgnoreCase).Success)
                        {
                            ++found;
                            target = player;
                        }
                    }

                    if (found == 0)
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] No player name matches (" + name + ")", "player", speaker);
                        return;
                    }
                    if (found > 1)
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] Multiple player name matches (" + name + "), try again!", "player", speaker);
                        return;
                    }

                    if (bf4db_CheckedPlayers.Contains(target))
                    {
                        bf4db_CheckedPlayers.Remove(target);
                        bf4db_manualChecks.Add(target);
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + target + " is queued for verification", "player", speaker);
                        return;
                    }
                    else
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + target + " is queued for verification", "player", speaker);
                        return;
                    }

                    return;
                }
            }
            return;
        }

        public void KickPlayer(String PlayerName, String reason, String guid, Boolean sendMessage)
        {
            kickPlayer kickPlayer = new kickPlayer();
            kickPlayer.PlayerName = PlayerName;
            kickPlayer.bf4db_APIKey = bf4db_APIKey;
            ThreadPool.QueueUserWorkItem(new WaitCallback(kickPlayerReport), kickPlayer);
            if (!sendMessage)
            {
                return;
            }
			if(guid == "") {
				this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", PlayerName, reason);
			} else {
				this.ExecuteCommand("procon.protected.send", "banList.add", "guid", guid, "seconds", "1", reason);
			}
        }

        public Hashtable verifyKey(String apiKey, string serverVersion)
        {
            try
            {
                string pluginVersion = GetPluginVersion();
                String result = (string)bf4db_API.GetType().GetMethod("verifyKey").Invoke(bf4db_API, new object[] { (object)apiKey, (object)pluginVersion, (object)serverVersion });

                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                // check we got a valid response
                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                }

                String response = (String)json["response"];
                String message = (String)json["message"];

                // verify we got a success message
                if (!(response.StartsWith("success") && message.StartsWith("API key is valid")))
                {
                    ConsoleException(message);
                }
                else
                {
                    this.bf4db_IsValid = true;
                }

                ConsoleWrite(message);

                return json;
            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return null;
            }
        }

        public Hashtable updateServer(String apiKey)
        {
            try
            {
                this.bf4db_lastServerUpdate = DateTime.UtcNow;

                String result = (string)bf4db_API.GetType().GetMethod("updateServer").Invoke(bf4db_API, new object[] { (object)apiKey });

                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                // check we got a valid response
                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("BF4DB, No Response!");
                }

                String response = (String)json["response"];
                String message = (String)json["message"];

                // verify we got a success message
                if (!(response.StartsWith("success")))
                {
                    ConsoleException("BF4DB, " + message);
                }

                this.bf4db_currentRound = Int64.Parse(json["gameId"].ToString());
                this.bf4db_ServerType = Int64.Parse(json["type"].ToString());

                DebugWrite(message, 2);

                return json;
            }
            catch (Exception ex)
            {

                ConsoleException(ex.Message);
                return null;
            }
        }

        public void kickPlayerReport(Object state)
        {
            kickPlayer kickPlayer = (kickPlayer)state;
            String playername = kickPlayer.PlayerName;
            String apiKey = kickPlayer.bf4db_APIKey;

            try
            {
                String result = (string)bf4db_API.GetType().GetMethod("kickPlayer").Invoke(bf4db_API, new object[] { (object)playername, (object)apiKey });
                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                    return;
                }

                String kickResponse = (String)json["response"];
                String kickMessage = (String)json["message"];
                DebugWrite(kickMessage, 1);
                return;
            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return;
            }
        }

        public int violationWeapon(String playername, String weapon, String apiKey)
        {
            try
            {
                String result = (string)bf4db_API.GetType().GetMethod("violationWeapon").Invoke(bf4db_API, new object[] { (object)playername, (object)weapon, (object)apiKey });
                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                    return -1;
                }

                String violationResponse = (String)json["response"];
                String violationMessage = (String)json["message"];
                DebugWrite(violationMessage, 1);
                return 1;
            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return -1;
            }
        }

        public int checkPlayer(String playername, String guid, String apiKey)
        {

            try
            {
				
                String result = (string)bf4db_API.GetType().GetMethod("checkPlayer").Invoke(bf4db_API, new object[] { (object)playername, (object)guid, (object)apiKey });
                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                    return -1;
                }

                String checkResponse = (String)json["response"];
                string checkID = json["id"].ToString();
                int checkStatus = Int32.Parse(json["status"].ToString());
                String checkMessage = (String)json["message"];
                String checkReason = (String)json["reason"];
                String pbguid = (String)json["pb_guid"];
                String eaguid = (String)json["ea_guid"];

                if (!(checkResponse.StartsWith("success")))
                {
                    ConsoleException(checkMessage);
                    return -1;
                }

                if (checkStatus == 0)
                {
                    if (bf4db_EnableAnnouncements == enumBoolYesNo.Yes && (bf4db_manualChecks.Contains(playername) || bf4db_EnableCleanAnnouncements == enumBoolYesNo.Yes))
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + playername + " has not been reported yet.", "all");
                    }
                }

                if (checkStatus == 1 || checkStatus == 4)
                {
                    bool needsKick = false;
                    
                    if (checkStatus == 1 && checkReason != "Hacked Account" && bf4db_EnableAutoBan == enumBoolYesNo.Yes) {
                        needsKick = true;
                    }

                    if (checkStatus == 4 && bf4db_EnableGlitchKick == enumBoolYesNo.Yes) {
                        needsKick = true;
                    }

                    if (checkReason == "Hacked Account" && bf4db_EnableHackedKick == enumBoolYesNo.Yes) {
                        needsKick = true;
                    }

                    if (!bf4db_whitelist.Contains(playername))
                    {
                        if (bf4db_ServerType != 1)
                        {
                            if (bf4db_EnableAnnouncements == enumBoolYesNo.Yes && needsKick)
                            {
                                ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + playername + " is banned for " + checkReason, "all");
                            }
                            if (needsKick)
                            {
                                if (AdKatsIntegration)
                                {
                                    ExecuteCommand("procon.protected.plugins.call", "AdKats", "IssueCommand", "BF4DB", JSON.JsonEncode(new Hashtable {
                                        {"caller_identity", "BF4DB"},
                                        {"response_requested", false},
                                        {"command_type", "player_kick"},
                                        {"source_name", "BF4DB"},
                                        {"target_name", playername},
                                        {"target_guid", guid},
                                        {"record_message", "[BF4DB] " + checkReason + @". Appeal at https://bf4db.com/player/ban/" + checkID}
                                    }));

                                    //Just to make sure
                                    System.Threading.Thread.Sleep(1000);
                                    KickPlayer(playername, "[BF4DB] " + checkReason + @". Appeal at https://bf4db.com/player/ban/" + checkID, guid,  false);
                                }
                                else
                                {
                                    KickPlayer(playername, "[BF4DB] " + checkReason + @". Appeal at https://bf4db.com/player/ban/" + checkID, guid, true);
                                }
                            }
                        }
                        else
                        {
                            if (bf4db_EnableAnnouncements == enumBoolYesNo.Yes)
                            {
                                ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + playername + " is banned for " + checkReason, "all");
                            }
                        }
                        DebugWrite(checkMessage, 1);
                    }
                    else
                    {
                        DebugWrite(playername + " is whitelisted and will not be banned!", 1);
                    }
                }

                if (bf4db_EnableAnnouncements == enumBoolYesNo.Yes && (bf4db_manualChecks.Contains(playername) || bf4db_EnableCleanAnnouncements == enumBoolYesNo.Yes))
                {
                    if (checkStatus == 2)
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + playername + " is clean", "all");
                    }
                    else if (checkStatus == 3)
                    {
                        ExecuteCommand("procon.protected.send", "admin.say", "[BF4DB] " + playername + " is whitelisted", "all");
                    }
                }

                if (bf4db_manualChecks.Contains(playername))
                {
                    bf4db_manualChecks.Remove(playername);
                }

                return checkStatus;

            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return 0;
            }
        }

        public void violationPlayer(String playername, String encodedMsg, String apiKey)
        {
            try
            {
                String result = (string)bf4db_API.GetType().GetMethod("violationPlayer").Invoke(bf4db_API, new object[] { (object)playername, (object)encodedMsg, (object)apiKey });

                Hashtable json = (Hashtable)JSON.JsonDecode(result);

                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                }

                String response = (String)json["response"];
                String message = (String)json["message"];

                if (!(response.StartsWith("success")))
                {
                    ConsoleException(message);
                }

                DebugWrite(message, 1);

            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return;
            }
        }

        public Hashtable updatePB(CPunkbusterInfo cpbiPlayer, String apiKey)
        {
            try
            {
                String result = (string)bf4db_API.GetType().GetMethod("updatePB").Invoke(bf4db_API, new object[] { (object)cpbiPlayer.SoldierName, (object)cpbiPlayer.GUID, (object)cpbiPlayer.Ip, (object)cpbiPlayer.PlayerCountryCode, (object)apiKey });

                Hashtable json = (Hashtable)JSON.JsonDecode(result);
				
                // check we got a valid response
                if (!(json.ContainsKey("response")))
                {
                    ConsoleException("No Response!");
                }

                String response = (String)json["response"];
                String message = (String)json["message"];
                String block = (String)json["block"];
                String reason = (String)json["block_reason"];
                String ea_guid = (String)json["ea_guid"];

				if (bf4db_EnableVPNKick == enumBoolYesNo.Yes && (!bf4db_whitelist.Contains(cpbiPlayer.SoldierName)) && block == "yes")  {
					KickPlayer(cpbiPlayer.SoldierName, "[BF4DB] VPN-KICK - "  + reason  + " - "  + (string) cpbiPlayer.Ip , ea_guid, true);
				}
				
                // verify we got a success message
                if (!(response.StartsWith("success")))
                {
                    ConsoleException("BF4DB, " + message);
                }

                DebugWrite(message, 1);

                return json;
            }
            catch (Exception ex)
            {
                ConsoleException(ex.Message);
                return null;
            }
        }
    } // end BF4DB

} // end namespace PRoConEvents
