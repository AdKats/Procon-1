/*	EventLogger.cs -  Procon Plugin [BF3, BF4, BFH, BC2]

	Version: 1.0.0.0

	Development by maxdralle@gmx.com

	This plugin file is part of PRoCon Frostbite.

	This plugin is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This plugin is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Web;
using System.Data;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

using MySql.Data.MySqlClient;

namespace PRoConEvents {
//Aliases
using EventType = PRoCon.Core.Events.EventType;
using CapturableEvent = PRoCon.Core.Events.CapturableEvents;

public class EventLogger : PRoConPluginAPI, IPRoConPluginInterface {
/* Inherited:
	this.PunkbusterPlayerInfoList = new Dictionary<String, CPunkbusterInfo>();
	this.FrostbitePlayerInfoList = new Dictionary<String, CPlayerInfo>();
*/

private bool fIsEnabled;
private string ServerIP;
private string ServerPort;

private DateTime OnRoundOverTime = DateTime.UtcNow;

private enumBoolYesNo SettingPlayerDisconnectedChat;
private enumBoolYesNo SettingPlayerDisconnectedConsole;
private enumBoolYesNo SettingPlayerDisconnectedLogfile;
private enumBoolYesNo SettingPlayerDisconnectedDB;
private enumBoolYesNo SettingPlayerKickedChat;
private enumBoolYesNo SettingPlayerKickedConsole;
private enumBoolYesNo SettingPlayerKickedLogfile;
private enumBoolYesNo SettingPlayerKickedDB;
private enumBoolYesNo SettingPlayerKickedByAdminChat;
private enumBoolYesNo SettingPlayerKickedByAdminConsole;
private enumBoolYesNo SettingPlayerKickedByAdminLogfile;
private enumBoolYesNo SettingPlayerKickedByAdminDB;
private enumBoolYesNo SettingPlayerKilledByAdminChat;
private enumBoolYesNo SettingPlayerKilledByAdminConsole;
private enumBoolYesNo SettingPlayerKilledByAdminLogfile;
private enumBoolYesNo SettingPlayerKilledByAdminDB;
private enumBoolYesNo SettingPlayerMovedByAdminChat;
private enumBoolYesNo SettingPlayerMovedByAdminConsole;
private enumBoolYesNo SettingPlayerMovedByAdminLogfile;
private enumBoolYesNo SettingPlayerMovedByAdminDB;
private enumBoolYesNo SettingPlayerForceMovedByAdminChat;
private enumBoolYesNo SettingPlayerForceMovedByAdminConsole;
private enumBoolYesNo SettingPlayerForceMovedByAdminLogfile;
private enumBoolYesNo SettingPlayerForceMovedByAdminDB;
private enumBoolYesNo SettingBanAddedChat;
private enumBoolYesNo SettingBanAddedConsole;
private enumBoolYesNo SettingBanAddedLogfile;
private enumBoolYesNo SettingBanAddedDB;
private enumBoolYesNo SettingTimeBanAddedChat;
private enumBoolYesNo SettingTimeBanAddedConsole;
private enumBoolYesNo SettingTimeBanAddedLogfile;
private enumBoolYesNo SettingTimeBanAddedDB;
private enumBoolYesNo SettingProconLayerChat;
private enumBoolYesNo SettingProconLayerConsole;
private enumBoolYesNo SettingProconLayerLogfile;
private enumBoolYesNo SettingProconLayerDB;


private List<string> tmpLogfile = new List<string>();
private List<string> AdkatsBanCheckList = new List<string>();
private Dictionary<String, DateTime> onJoinTime = new Dictionary<string, DateTime>();
private Dictionary<String, DateTime> AdkatsBanChecked = new Dictionary<string, DateTime>();
private Dictionary<String, String> AdkatsBanned = new Dictionary<string, String>();
private Dictionary<String, String> GuidToNameList = new Dictionary<string, String>();

private string SettingFilterPlayername;
private string SettingFilterReason;
private enumBoolYesNo SettingNoEmptyStringInDisReason;
private enumBoolYesNo SettingBetweenRoundsNoLog;
private enumBoolYesNo SettingUsePassword;
private string SettingPassword;
private string SettingPasswordCheck;
private string SettingFilterAutoBan;
private enumBoolYesNo SettingLocked;
private enumBoolYesNo SettingEnableBanEnforcer;
private enumBoolYesNo SettingEnableBanEnforcerOnlyPerma;
private string SettingAdkatsDBIP;
private string SettingAdkatsDBName;
private string SettingAdkatsDBUser;
private string SettingAdkatsDBPw;
private string SettingAdkatsBanReason;
private int SettingAdkatsSyncTime;

private enumBoolYesNo SettingSqlEnabled;
private string SettingStrSqlHostname;
private string SettingStrSqlPort;
private string SettingStrSqlDatabase;
private string SettingStrSqlUsername;
private string SettingStrSqlPassword;
private string SettingStrSqlGameserver;
private int SettingDBCleaner;
private DateTime LayerStartingTime = DateTime.UtcNow;
private DateTime lastOnListPlayersTS = DateTime.UtcNow;
private DateTime lastAdkatsSyncTS = DateTime.UtcNow;
private int fDebugLevel;
private bool SqlTableExist;

public EventLogger() {
	this.fIsEnabled = false;
	this.ServerIP = String.Empty;
	this.ServerPort = String.Empty;
	this.SettingPlayerDisconnectedChat = enumBoolYesNo.Yes;
	this.SettingPlayerDisconnectedConsole = enumBoolYesNo.Yes;
	this.SettingPlayerDisconnectedLogfile = enumBoolYesNo.Yes;
	this.SettingPlayerDisconnectedDB = enumBoolYesNo.No;
	this.SettingPlayerKickedChat = enumBoolYesNo.No;
	this.SettingPlayerKickedConsole = enumBoolYesNo.No;
	this.SettingPlayerKickedLogfile = enumBoolYesNo.No;
	this.SettingPlayerKickedDB = enumBoolYesNo.No;
	this.SettingPlayerKickedByAdminChat = enumBoolYesNo.No;
	this.SettingPlayerKickedByAdminConsole = enumBoolYesNo.No;
	this.SettingPlayerKickedByAdminLogfile = enumBoolYesNo.No;
	this.SettingPlayerKickedByAdminDB = enumBoolYesNo.No;
	this.SettingPlayerKilledByAdminChat = enumBoolYesNo.No;
	this.SettingPlayerKilledByAdminConsole = enumBoolYesNo.No;
	this.SettingPlayerKilledByAdminLogfile = enumBoolYesNo.No;
	this.SettingPlayerKilledByAdminDB = enumBoolYesNo.No;
	this.SettingPlayerMovedByAdminChat = enumBoolYesNo.No;
	this.SettingPlayerMovedByAdminConsole = enumBoolYesNo.No;
	this.SettingPlayerMovedByAdminLogfile = enumBoolYesNo.No;
	this.SettingPlayerMovedByAdminDB = enumBoolYesNo.No;
	this.SettingPlayerForceMovedByAdminChat = enumBoolYesNo.No;
	this.SettingPlayerForceMovedByAdminConsole = enumBoolYesNo.No;
	this.SettingPlayerForceMovedByAdminLogfile = enumBoolYesNo.No;
	this.SettingPlayerForceMovedByAdminDB = enumBoolYesNo.No;
	this.SettingBanAddedChat = enumBoolYesNo.No;
	this.SettingBanAddedConsole = enumBoolYesNo.No;
	this.SettingBanAddedLogfile = enumBoolYesNo.No;
	this.SettingBanAddedDB = enumBoolYesNo.No;
	this.SettingTimeBanAddedChat = enumBoolYesNo.No;
	this.SettingTimeBanAddedConsole = enumBoolYesNo.No;
	this.SettingTimeBanAddedLogfile = enumBoolYesNo.No;
	this.SettingTimeBanAddedDB = enumBoolYesNo.No;
	this.SettingProconLayerChat = enumBoolYesNo.No;
	this.SettingProconLayerConsole = enumBoolYesNo.No;
	this.SettingProconLayerLogfile = enumBoolYesNo.No;
	this.SettingProconLayerDB = enumBoolYesNo.No;
	this.SettingFilterAutoBan = "^Restricted Area!$|^Restricted Area$|^PunkBuster kicked player.*Cheater banned by GGC-Stream.NET*Ban on GUID|^PunkBuster kicked player.*Cheater banned by GGC-Stream.*Ban on GUID|^PunkBuster permanent ban issued on this Game Server for player|^PunkBuster kicked player.*Violation.*AIMBOT|^PunkBuster kicked player.*Violation.*MULTIHACK|^PunkBuster kicked player.*Violation.*WALLHACK|^PunkBuster kicked player.*Violation.*GAME HACK|^PunkBuster kicked player.*Violation.*GAMEHACK|^PunkBuster kicked player.*enforced a previous MBi Ban for|BF4DB.*Appeal at.*bf4db.com/player/ban";
	this.SettingFilterPlayername = String.Empty;
	this.SettingFilterReason = "PLAYER_CONN_LOST|Service Communication Failure|PLAYER_LEFT|Missing Content";
	this.SettingNoEmptyStringInDisReason = enumBoolYesNo.Yes;
	this.SettingBetweenRoundsNoLog = enumBoolYesNo.Yes;
	this.SettingEnableBanEnforcer = enumBoolYesNo.No;
	this.SettingEnableBanEnforcerOnlyPerma = enumBoolYesNo.No;
	this.SettingAdkatsDBIP = String.Empty;
	this.SettingAdkatsDBName = String.Empty;
	this.SettingAdkatsDBUser = String.Empty;
	this.SettingAdkatsDBPw = String.Empty;
	this.SettingAdkatsBanReason = "Shared Ban for %player% ...";
	this.SettingAdkatsSyncTime = 20;

	this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Layer restart detected.");
	this.SettingUsePassword = enumBoolYesNo.No;
	this.SettingPassword = String.Empty;
	this.SettingPasswordCheck = String.Empty;
	this.SettingLocked = enumBoolYesNo.No;
	this.SettingStrSqlHostname = String.Empty;
	this.SettingStrSqlPort = "3306";
	this.SettingStrSqlDatabase = String.Empty;
	this.SettingStrSqlUsername = String.Empty;
	this.SettingStrSqlPassword = String.Empty;
	this.SettingStrSqlGameserver = "#0 xy";
	this.SettingDBCleaner = 3;
	this.SettingSqlEnabled = enumBoolYesNo.No;
	this.fDebugLevel = 3;
	this.SqlTableExist = false;
}


public enum MessageType { Warning, Error, Exception, Normal };

public String FormatMessage(String msg, MessageType type) {
	String prefix = "[Event Logger] ";

	if (type.Equals(MessageType.Warning))
		prefix += "^1^bWARNING^0^n: ";
	else if (type.Equals(MessageType.Error))
		prefix += "^1^bERROR^n^0: ";
	else if (type.Equals(MessageType.Exception))
		prefix += "^1^bEXCEPTION^0^n: ";

	return prefix + msg;
}

public void LogWrite(String msg) {
	this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
}

public void ConsoleWrite(String msg, MessageType type) {
	LogWrite(FormatMessage(msg, type));
}

public void ConsoleWrite(String msg) {
	ConsoleWrite(msg, MessageType.Normal);
}

public void ConsoleWarn(String msg) {
	ConsoleWrite(msg, MessageType.Warning);
}

public void ConsoleError(String msg) {
	ConsoleWrite("^8" + msg + "^0", MessageType.Error);
}

public void ConsoleException(String msg) {
	ConsoleWrite(msg, MessageType.Exception);
}

public void DebugWrite(String msg, int level) {
	if (this.fDebugLevel >= level) ConsoleWrite(msg, MessageType.Normal);
}

public void ServerCommand(params String[] args) {
	List<String> list = new List<String>();
	list.Add("procon.protected.send");
	list.AddRange(args);
	this.ExecuteCommand(list.ToArray());
}

public String GetPluginName() {
	return "Event Logger";
}

public String GetPluginVersion() {
	return "1.0.0.0";
}

public String GetPluginAuthor() {
	return "maxdralle";
}

public String GetPluginWebsite() {
	return "forum.myrcon.com/member.php?37189-maxdralle";
}

public String GetPluginDescription() {
	return @"
<h2>Description</h2>
<p>Logfile path: Procon Layer / CONFIGS/EVENT-LOGGER.txt</p>
<p>Logfile auto delete: beginning of the month</p>
<p>SQL database table: event_logger</p>
<p></p>
<p></p>
<blockquote><b>Sample Code to create SQL log entrie via ProconRulz (ingame cmd !log):</b><br>
On Say; Text !log; Exec procon.protected.plugins.call EventLogger SqlLog &quot;Test Entrie&quot; %p% &quot;SAMPLE LOG > test test asdf&quot;;</blockquote>
<p></p>
";
}


public List<CPluginVariable> GetPluginVariables() {
	List<CPluginVariable> lstReturn = new List<CPluginVariable>();
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|Enable SQL", typeof(enumBoolYesNo), this.SettingSqlEnabled));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Host", this.SettingStrSqlHostname.GetType(), this.SettingStrSqlHostname));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Port", this.SettingStrSqlPort.GetType(), this.SettingStrSqlPort));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Database", this.SettingStrSqlDatabase.GetType(), this.SettingStrSqlDatabase));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Username", this.SettingStrSqlUsername.GetType(), this.SettingStrSqlUsername));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Passwd.", this.SettingStrSqlPassword.GetType(), this.SettingStrSqlPassword));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Gameserver name", this.SettingStrSqlGameserver.GetType(), this.SettingStrSqlGameserver));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Auto DB Cleaner (delete old log after xy days)", this.SettingDBCleaner.GetType(), this.SettingDBCleaner));
	lstReturn.Add(new CPluginVariable("0. MySQL Details (optional)|  - Debug Level (1-5)", this.fDebugLevel.GetType(), this.fDebugLevel));

	lstReturn.Add(new CPluginVariable("1. Events|Player Disconnected - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerDisconnectedChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Disconnected - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerDisconnectedConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Disconnected - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerDisconnectedLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Disconnected - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerDisconnectedDB));

	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerKickedChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerKickedConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerKickedLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerKickedDB));
		
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked By Admin - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerKickedByAdminChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked By Admin - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerKickedByAdminConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked By Admin - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerKickedByAdminLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Kicked By Admin - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerKickedByAdminDB));
		
	lstReturn.Add(new CPluginVariable("1. Events|Player Killed By Admin - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerKilledByAdminChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Killed By Admin - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerKilledByAdminConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Killed By Admin - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerKilledByAdminLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Killed By Admin - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerKilledByAdminDB));
		
	lstReturn.Add(new CPluginVariable("1. Events|Player Moved By Admin - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerMovedByAdminChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Moved By Admin - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerMovedByAdminConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Moved By Admin - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerMovedByAdminLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Moved By Admin - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerMovedByAdminDB));
		
	lstReturn.Add(new CPluginVariable("1. Events|Player Force Moved By Admin - show in chat windows", typeof(enumBoolYesNo), this.SettingPlayerForceMovedByAdminChat));
	lstReturn.Add(new CPluginVariable("1. Events|Player Force Moved By Admin - show in plugin console", typeof(enumBoolYesNo), this.SettingPlayerForceMovedByAdminConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Player Force Moved By Admin - save to logfile", typeof(enumBoolYesNo), this.SettingPlayerForceMovedByAdminLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Player Force Moved By Admin - save to SQL", typeof(enumBoolYesNo), this.SettingPlayerForceMovedByAdminDB));
		
	lstReturn.Add(new CPluginVariable("1. Events|Time-Ban - show in chat windows", typeof(enumBoolYesNo), this.SettingTimeBanAddedChat));
	lstReturn.Add(new CPluginVariable("1. Events|Time-Ban - show in plugin console", typeof(enumBoolYesNo), this.SettingTimeBanAddedConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Time-Ban - save to logfile", typeof(enumBoolYesNo), this.SettingTimeBanAddedLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Time-Ban - save to SQL", typeof(enumBoolYesNo), this.SettingTimeBanAddedDB));

	lstReturn.Add(new CPluginVariable("1. Events|Perma-Ban - show in chat windows", typeof(enumBoolYesNo), this.SettingBanAddedChat));
	lstReturn.Add(new CPluginVariable("1. Events|Perma-Ban - show in plugin console", typeof(enumBoolYesNo), this.SettingBanAddedConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Perma-Ban - save to logfile", typeof(enumBoolYesNo), this.SettingBanAddedLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Perma-Ban - save to SQL", typeof(enumBoolYesNo), this.SettingBanAddedDB));

	lstReturn.Add(new CPluginVariable("1. Events|Procon Layer Accounts - show in chat windows", typeof(enumBoolYesNo), this.SettingProconLayerChat));
	lstReturn.Add(new CPluginVariable("1. Events|Procon Layer Accounts - show in plugin console", typeof(enumBoolYesNo), this.SettingProconLayerConsole));
	lstReturn.Add(new CPluginVariable("1. Events|Procon Layer Accounts - save to logfile", typeof(enumBoolYesNo), this.SettingProconLayerLogfile));
	lstReturn.Add(new CPluginVariable("1. Events|Procon Layer Accounts - save to SQL", typeof(enumBoolYesNo), this.SettingProconLayerDB));

	lstReturn.Add(new CPluginVariable("2. Filter|Exclude Filter Regex Playername", this.SettingFilterPlayername.GetType(), this.SettingFilterPlayername));
	lstReturn.Add(new CPluginVariable("2. Filter|Exclude Filter Regex Reason", this.SettingFilterReason.GetType(), this.SettingFilterReason));
	lstReturn.Add(new CPluginVariable("2. Filter|Exclude Empty String in Disconnect Reason", typeof(enumBoolYesNo), this.SettingNoEmptyStringInDisReason));
	lstReturn.Add(new CPluginVariable("2. Filter|Exclude Force Moved by Admin on round end", typeof(enumBoolYesNo), this.SettingBetweenRoundsNoLog));
	lstReturn.Add(new CPluginVariable("2. Filter|Auto Perma Ban if Disconnected Reason triggers this Regex Filter", this.SettingFilterAutoBan.GetType(), this.SettingFilterAutoBan));

	lstReturn.Add(new CPluginVariable("3. Plugin Protection|Password needed to edit settings", typeof(enumBoolYesNo), this.SettingUsePassword));
	if (this.SettingUsePassword == enumBoolYesNo.Yes) {
		lstReturn.Add(new CPluginVariable("3. Plugin Protection|Change Password", this.SettingPassword.GetType(), this.SettingPassword));
		lstReturn.Add(new CPluginVariable("3. Plugin Protection|LOCK SETTINGS NOW", typeof(enumBoolYesNo), this.SettingLocked));
	}
	
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Enable Ban Enforcer form Adkats DB", typeof(enumBoolYesNo), this.SettingEnableBanEnforcer));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Enforce Perma Ban Only", typeof(enumBoolYesNo), this.SettingEnableBanEnforcerOnlyPerma));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Adkats DB IP:", this.SettingAdkatsDBIP.GetType(), this.SettingAdkatsDBIP));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Adkats DB Name:", this.SettingAdkatsDBName.GetType(), this.SettingAdkatsDBName));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Adkats DB User:", this.SettingAdkatsDBUser.GetType(), this.SettingAdkatsDBUser));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Adkats DB Pw:", this.SettingAdkatsDBPw.GetType(), this.SettingAdkatsDBPw));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Change All Kick/Ban Reason to", this.SettingAdkatsBanReason.GetType(), this.SettingAdkatsBanReason));
	lstReturn.Add(new CPluginVariable("5. Adkats Ban Enforcer|Check clear players again after xy minutes", this.SettingAdkatsSyncTime.GetType(), this.SettingAdkatsSyncTime));
	return lstReturn;	

}

public List<CPluginVariable> GetDisplayPluginVariables() {
	List<CPluginVariable> lstReturn = new List<CPluginVariable>();
	lstReturn.Add(new CPluginVariable("3. Plugin Protection|LOCK SETTINGS NOW", typeof(enumBoolYesNo), this.SettingLocked));
	if (this.SettingLocked == enumBoolYesNo.Yes) {
		lstReturn.Clear();
		lstReturn.Add(new CPluginVariable("Plugin Protection|Enter Password to unlock settings", this.SettingPasswordCheck.GetType(), this.SettingPasswordCheck));
		return lstReturn;
	} else {
		return GetPluginVariables();
	}
}

public void SetPluginVariable(String strVariable, String strValue) {
	bool layerReady = (((DateTime.UtcNow - this.LayerStartingTime).TotalSeconds) > 60);
	if (this.SettingLocked == enumBoolYesNo.Yes) {
		if (Regex.Match(strVariable, @"Enter Password to unlock settings").Success) {
			if (this.SettingPassword == strValue) {
				// show settings
				this.SettingLocked = enumBoolYesNo.No;
				if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlugin Protection^n > Settings unlocked.") ; }
				if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlugin Protection^n > Settings unlocked.") ; }
				if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Plugin Protection > Settings unlocked.") ; }
				//this.GetDisplayPluginVariables();
			} else {
				ConsoleWrite("^b^8WRONG PASSWORD!^0^n");
				if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlugin Protection^n > Wrong password to unlock settings.") ; }
				if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlugin Protection^n > Wrong password to unlock settings.") ; }
				if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Plugin Protection > Wrong password to unlock settings.") ; }
			}
		}
	}
	
	if (Regex.Match(strVariable, @"Debug Level").Success) {
		int tmp = 3;
		int.TryParse(strValue, out tmp);
		if (tmp >= 0 && tmp <= 5) {
			this.fDebugLevel = tmp;
		} else {
			ConsoleError("Invalid value for Debug Level: '" + strValue + "'. It must be a number between 1 and 5. (e.g.: 3)");
		}
	} else if (Regex.Match(strVariable, @"Enable SQL").Success) {
		this.SettingSqlEnabled = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
		if ((this.SettingProconLayerChat == enumBoolYesNo.Yes) && (this.fIsEnabled) && (layerReady)) { this.SqlLog("","",""); }
	} else if (Regex.Match(strVariable, @"Host").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		this.SettingStrSqlHostname = strValue.Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Port").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		int tmpport = 3306;
		int.TryParse(strValue, out tmpport);
		if (tmpport > 0 && tmpport < 65536) {
			this.SettingStrSqlPort = tmpport.ToString();
		} else {
			ConsoleError("Invalid value for MySQL Port: '" + strValue + "'. Port must be a number between 1 and 65535. (e.g.: 3306)");
		}
	} else if (Regex.Match(strVariable, @"Database").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		this.SettingStrSqlDatabase = strValue.Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Username").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		this.SettingStrSqlUsername = strValue.Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Passwd.").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		this.SettingStrSqlPassword = strValue.Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Gameserver name").Success) {
		if ((this.fIsEnabled) && (layerReady) && (this.SqlTableExist)) { ConsoleError("SQL Settings locked! Please disable the Plugin and try again..."); return; }
		this.SettingStrSqlGameserver = strValue;
	} else if (Regex.Match(strVariable, @"Auto DB Cleaner").Success) {
		int tmpDBCleaner = 1;
		int.TryParse(strValue, out tmpDBCleaner);
		if (tmpDBCleaner >= 1 && tmpDBCleaner <= 999) {
			this.SettingDBCleaner = tmpDBCleaner;
		} else {
			ConsoleError("Invalid value. It must be a number between 1 and 999. (e.g.: 90)");
		}
	} else if (Regex.Match(strVariable, @"Player Disconnected - show in chat windows").Success) {
		this.SettingPlayerDisconnectedChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Disconnected - show in plugin console").Success) {
		this.SettingPlayerDisconnectedConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Disconnected - save to logfile").Success) {
		this.SettingPlayerDisconnectedLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Disconnected - save to SQL").Success) {
		this.SettingPlayerDisconnectedDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked - show in chat windows").Success) {
		this.SettingPlayerKickedChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked - show in plugin console").Success) {
		this.SettingPlayerKickedConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked - save to logfile").Success) {
		this.SettingPlayerKickedLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked - save to SQL").Success) {
		this.SettingPlayerKickedDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked By Admin - show in chat windows").Success) {
		this.SettingPlayerKickedByAdminChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked By Admin - show in plugin console").Success) {
		this.SettingPlayerKickedByAdminConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked By Admin - save to logfile").Success) {
		this.SettingPlayerKickedByAdminLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Kicked By Admin - save to SQL").Success) {
		this.SettingPlayerKickedByAdminDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Killed By Admin - show in chat windows").Success) {
		this.SettingPlayerKilledByAdminChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Killed By Admin - show in plugin console").Success) {
		this.SettingPlayerKilledByAdminConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Killed By Admin - save to logfile").Success) {
		this.SettingPlayerKilledByAdminLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Killed By Admin - save to SQL").Success) {
		this.SettingPlayerKilledByAdminDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Moved By Admin - show in chat windows").Success) {
		this.SettingPlayerMovedByAdminChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Moved By Admin - show in plugin console").Success) {
		this.SettingPlayerMovedByAdminConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Moved By Admin - save to logfile").Success) {
		this.SettingPlayerMovedByAdminLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Moved By Admin - save to SQL").Success) {
		this.SettingPlayerMovedByAdminDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Force Moved By Admin - show in chat windows").Success) {
		this.SettingPlayerForceMovedByAdminChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Force Moved By Admin - show in plugin console").Success) {
		this.SettingPlayerForceMovedByAdminConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Force Moved By Admin - save to logfile").Success) {
		this.SettingPlayerForceMovedByAdminLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Player Force Moved By Admin - save to SQL").Success) {
		this.SettingPlayerForceMovedByAdminDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Time-Ban - show in chat windows").Success) {
		this.SettingTimeBanAddedChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Time-Ban - show in plugin console").Success) {
		this.SettingTimeBanAddedConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Time-Ban - save to logfile").Success) {
		this.SettingTimeBanAddedLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Time-Ban - save to SQL").Success) {
		this.SettingTimeBanAddedDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Perma-Ban - show in chat windows").Success) {
		this.SettingBanAddedChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Perma-Ban - show in plugin console").Success) {
		this.SettingBanAddedConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Perma-Ban - save to logfile").Success) {
		this.SettingBanAddedLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Perma-Ban - save to SQL").Success) {
		this.SettingBanAddedDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Procon Layer Accounts - show in chat windows").Success) {
		this.SettingProconLayerChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Procon Layer Accounts - show in plugin console").Success) {
		this.SettingProconLayerConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Procon Layer Accounts - save to logfile").Success) {
		this.SettingProconLayerLogfile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Procon Layer Accounts - save to SQL").Success) {
		this.SettingProconLayerDB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Exclude Filter Regex Playername").Success) {
		this.SettingFilterPlayername = strValue;
	} else if (Regex.Match(strVariable, @"Exclude Filter Regex Reason").Success) {
		this.SettingFilterReason = strValue;
	} else if (Regex.Match(strVariable, @"Exclude Empty String in Disconnect Reason").Success) {
		this.SettingNoEmptyStringInDisReason = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Exclude Force Moved by Admin on round end").Success) {
		this.SettingBetweenRoundsNoLog = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Auto Perma Ban if Disconnected Reason triggers this Regex Filter").Success) {
		this.SettingFilterAutoBan = strValue;
	} else if (Regex.Match(strVariable, @"Enable Ban Enforcer form Adkats DB").Success) {
		this.SettingEnableBanEnforcer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Enforce Perma Ban Only").Success) {
		this.SettingEnableBanEnforcerOnlyPerma = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Adkats DB IP").Success) {
		this.SettingAdkatsDBIP = strValue.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Adkats DB Name").Success) {
		this.SettingAdkatsDBName = strValue.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Adkats DB User").Success) {
		this.SettingAdkatsDBUser = strValue.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Adkats DB Pw").Success) {
		this.SettingAdkatsDBPw = strValue.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Change All Kick/Ban Reason to").Success) {
		this.SettingAdkatsBanReason = strValue.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
	} else if (Regex.Match(strVariable, @"Check clear players again after xy minutes").Success) {
		int tmpSyncBan = 1;
		int.TryParse(strValue, out tmpSyncBan);
		if (tmpSyncBan >= 10 && tmpSyncBan <= 240) {
			this.SettingAdkatsSyncTime = tmpSyncBan;
		} else {
			ConsoleError("Invalid value. It must be a number between 10 and 240. (e.g.: 20)");
		}
	} else if (Regex.Match(strVariable, @"Password needed to edit settings").Success) {
		this.SettingUsePassword = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
	} else if (Regex.Match(strVariable, @"Change Password").Success) {
		this.SettingPassword = strValue;
	} else if (Regex.Match(strVariable, @"LOCK SETTINGS NOW").Success) {
		if (this.SettingPassword != String.Empty) {
			this.SettingLocked = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
			if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlugin Protection^n > Settings locked.") ; }
			if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlugin Protection^n > Settings locked.") ; }
			if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Plugin Protection > Settings locked.") ; }
		} else {
			ConsoleWrite("^b^8ERROR:^0^n Set a password to lock settings");
		}
	}
}

//////////////////////
//Server Events
//////////////////////

public void OnPluginLoaded(String strHostName, String strPort, String strPRoConVersion) {
	this.RegisterEvents(this.GetType().Name, "OnPunkbusterMessage", "OnPlayerJoin", "OnListPlayers", "OnPlayerDisconnected", "OnRoundOver", "OnPlayerKicked", "OnPlayerKickedByAdmin", "OnPlayerKilledByAdmin", "OnPlayerMovedByAdmin", "OnBanAdded", "OnAccountCreated", "OnAccountDeleted", "OnAccountLogin", "OnAccountLogout");
	this.ServerIP = strHostName;
	this.ServerPort = strPort;
}

public void OnPluginEnable() {
	this.fIsEnabled = true;
	ConsoleWrite("^b^2Enabled!^0^n");
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Plugin enabled."); }

	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "500", "999", "-1", "procon.protected.plugins.call", "EventLogger", "WriteLogfile");
	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "300", "9999", "-1", "procon.protected.plugins.call", "EventLogger", "AutoDBCleaner");
	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "20", "30", "1", "procon.protected.plugins.call", "EventLogger", "SqlLog", "", "", "");
	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "60", "1", "1", "procon.protected.plugins.call", "EventLogger", "CheckRestart");
	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "60", "29", "-1", "procon.protected.plugins.call", "EventLogger", "AdkatsBanEnforcerThread");
	this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "5", "80000", "-1", "procon.protected.plugins.call", "EventLogger", "TmpListCleaner");

}

public void OnPluginDisable() {
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Plugin disabled."); }
	this.WriteLogfile();
	this.onJoinTime.Clear();
	this.ExecuteCommand("procon.protected.tasks.remove", "EventLogger");
	TmpListCleaner();
	this.fIsEnabled = false;
	ConsoleWrite("^b^8Disabled!^0^n");
}

public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingEnableBanEnforcer == enumBoolYesNo.No) { return; }

	if (DateTime.UtcNow.Subtract(this.lastOnListPlayersTS).TotalSeconds >= 20) {
		try {
			if (CPlayerSubset.PlayerSubsetType.All == subset.Subset) {
				this.lastOnListPlayersTS = DateTime.UtcNow;
				DebugWrite("[OnListPlayers] Receive playerlist with EAGUID", 5);
				foreach (CPlayerInfo playerinfo in players) {
					if (playerinfo.GUID.StartsWith("EA_")) {
						if (this.GuidToNameList.ContainsKey(playerinfo.GUID)) {
							if (this.GuidToNameList[playerinfo.GUID] != playerinfo.SoldierName) { this.GuidToNameList[playerinfo.GUID] = playerinfo.SoldierName; }
						} else {
							this.GuidToNameList.Add(playerinfo.GUID, playerinfo.SoldierName);
						}
						if (this.AdkatsBanned.ContainsKey(playerinfo.GUID)) {
							// banned player found
							// xxxxxxxxxx kick with yell
							DebugWrite("[BanEnforceThread] Kick BANNED player (Adkats DB): ^b" + playerinfo.SoldierName + "^n: " + this.AdkatsBanned[playerinfo.GUID], 2);
							this.ExecuteCommand("procon.protected.send", "admin.yell", this.AdkatsBanned[playerinfo.GUID], "10", "player", playerinfo.SoldierName);
							this.ExecuteCommand("procon.protected.send", "admin.say", this.AdkatsBanned[playerinfo.GUID], "player", playerinfo.SoldierName);
							this.ExecuteCommand("procon.protected.chat.write", "(PlayerYell " + playerinfo.SoldierName + ") " + this.AdkatsBanned[playerinfo.GUID]);
							this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "5", "1", "1", "procon.protected.send", "admin.kickPlayer", playerinfo.SoldierName, this.AdkatsBanned[playerinfo.GUID]);
						} else {
							if (this.AdkatsBanChecked.ContainsKey(playerinfo.GUID)) {
								if (DateTime.UtcNow.Subtract(this.AdkatsBanChecked[playerinfo.GUID]).TotalSeconds >= (60 + (this.SettingAdkatsSyncTime * 60))) {
									// time to check again
									if (!this.AdkatsBanCheckList.Contains(playerinfo.GUID)) { this.AdkatsBanCheckList.Add(playerinfo.GUID); }
								}
							} else {
								// check player
								if (!this.AdkatsBanCheckList.Contains(playerinfo.GUID)) { this.AdkatsBanCheckList.Add(playerinfo.GUID); }
							}
						}
					}
				}
			}
		}
		catch (Exception ex) {
			DebugWrite("[OnListPlayers] ^bERROR:^n On Event OnListPlayers. ERROR: " + ex, 5);
		}
	}
}

public void OnPunkbusterMessage(string strPunkbusterMessage) {
	return;
	string violation = @"PunkBuster Server: VIOLATION \(([a-zA-Z]+)\) \#([0-9]+)\: ([a-zA-Z0-9_\-]+) \(slot #([0-9]+)\) Violation \(([a-zA-Z]+)\) \#([0-9]+) \[([0-9a-f]{32})\(-\) (([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3}))\:([0-9]{1,5})\]";

	Match computedViolation = Regex.Match(strPunkbusterMessage, violation, RegexOptions.IgnoreCase);
	if (computedViolation.Success)
	{
		DebugWrite("punkbuster orig msg: " + strPunkbusterMessage , 1);
		String playerName = computedViolation.Groups[3].Value;
		String encodedMsg = System.Uri.EscapeDataString(strPunkbusterMessage);
		DebugWrite("punkbuster ban: ^b" + playerName + "^n for: " + encodedMsg, 1);
	}
}

public void AdkatsBanEnforcerThread() {
	if (!this.fIsEnabled) { return; }
	if (this.SettingEnableBanEnforcer == enumBoolYesNo.No) { return; }
	if (this.AdkatsBanCheckList.Count <= 0) { return; }

	if (DateTime.UtcNow.Subtract(this.lastAdkatsSyncTS).TotalSeconds >= 25) {
		
		Thread ThreadWorkerEventLogger1 = new Thread(new ThreadStart(delegate() {
			string tmp_adkats_db_login = "Server=" + this.SettingAdkatsDBIP + ";Port=3306;Database=" + this.SettingAdkatsDBName + ";" + "Uid=" + this.SettingAdkatsDBUser + ";" + "Pwd=" + this.SettingAdkatsDBPw + ";" + "Connection Timeout=5;";
			try {
				string SQL = String.Empty;
				string tmp_eaguid = String.Empty;
				string tmp_banreason = String.Empty;
				string tmp_playername = String.Empty;
				string tmp_currentName = String.Empty;
				int tmp_time = 0;
				int tmp_counter = 0;
				List<string> tmp_list = new List<string>();
				tmp_list = this.AdkatsBanCheckList;
				using (MySqlConnection Con = new MySqlConnection(this.AdkatsSqlLogin())) {
					this.lastAdkatsSyncTS = DateTime.UtcNow;
					Con.Open();
					if (Con.State == ConnectionState.Open) {
						try {
							foreach (string pguid in tmp_list) {
								tmp_banreason = String.Empty;
								tmp_playername = String.Empty;
								tmp_counter++;
								if (!this.fIsEnabled) { return; }
								if (tmp_counter <= 16) {
										// check player
										tmp_eaguid = pguid;
										SQL = "SELECT tpd.`SoldierName`, tpd.`EAGUID`, tpd.`PlayerID`, abr.`record_message`, TIMESTAMPDIFF(Minute,UTC_TIMESTAMP(),adk.`ban_endTime`) AS timestamp FROM `tbl_playerdata` tpd INNER JOIN `tbl_server_player` tsp ON tsp.`PlayerID` = tpd.`PlayerID` INNER JOIN `adkats_bans` adk ON adk.`player_id` = tpd.`PlayerID` LEFT JOIN `adkats_records_main` abr ON abr.`record_id` = adk.`latest_record_id` WHERE adk.`ban_status` = 'Active' AND tpd.`EAGUID` = '" + tmp_eaguid + "' GROUP BY tpd.`PlayerID` ORDER BY tpd.`SoldierName` ASC";
										DebugWrite("[BanEnforceThread] Checking EA-GUID: " + pguid + ". SQL Cmd: " + SQL, 5);
										using (MySqlCommand MyCommand = new MySqlCommand(SQL)) {
											DataTable resultTable = this.AdkatsSQLquery(MyCommand);
											if (resultTable.Rows != null) {
												foreach (DataRow row in resultTable.Rows) {
													// reading sql
													tmp_time = Convert.ToInt32(row["timestamp"]);
													tmp_banreason = row["record_message"].ToString();
													tmp_playername = row["SoldierName"].ToString();
												}
												if (((this.SettingEnableBanEnforcerOnlyPerma == enumBoolYesNo.Yes) && (tmp_time > 525600)) || (this.SettingEnableBanEnforcerOnlyPerma == enumBoolYesNo.No)) {
													if ((tmp_banreason.Length > 2) && (tmp_playername != String.Empty)) {
														// banned player found
														tmp_currentName = this.GuidToName(pguid);
														tmp_banreason = this.BanTimeString(tmp_time) + this.BanReasonCleaned(tmp_banreason, tmp_currentName);
														DebugWrite("[BanEnforceThread] Kick BANNED player (Adkats DB): ^b" + tmp_currentName + "^n: " + tmp_banreason, 2);
														if (!this.AdkatsBanned.ContainsKey(pguid) && (tmp_time > 5000)) { this.AdkatsBanned.Add(pguid, tmp_banreason); }
														if (this.AdkatsBanned.ContainsKey(pguid) && (tmp_time < 5000)) { this.AdkatsBanned.Remove(pguid); }
														this.ExecuteCommand("procon.protected.send", "admin.yell", tmp_banreason, "10", "player", tmp_currentName);
														this.ExecuteCommand("procon.protected.send", "admin.say", tmp_banreason, "player", tmp_currentName);
														this.ExecuteCommand("procon.protected.chat.write", "(PlayerYell " + tmp_currentName + ") " + tmp_banreason);
														//this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", tmp_currentName, tmp_banreason);
														this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "15", "1", "1", "procon.protected.send", "admin.kickPlayer", tmp_currentName, tmp_banreason);
														this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "12", "1", "1", "procon.protected.send", "admin.yell", tmp_banreason, "10", "player", tmp_currentName);
														this.ExecuteCommand("procon.protected.tasks.add", "EventLogger", "12", "1", "1", "admin.say", tmp_banreason, "player", tmp_currentName);
														if (this.AdkatsBanChecked.ContainsKey(pguid)) { this.AdkatsBanChecked.Remove(pguid); }
													}
												}
											}
										}
									if ((tmp_banreason.Length < 1) || (tmp_playername == String.Empty) || ((this.SettingEnableBanEnforcerOnlyPerma == enumBoolYesNo.Yes) && (tmp_time > 525600))) {
										if (this.AdkatsBanChecked.ContainsKey(pguid)) {
											this.AdkatsBanChecked[pguid] = DateTime.UtcNow;
										} else {
											this.AdkatsBanChecked.Add(pguid, DateTime.UtcNow);
										}
									}
								}
							}
							this.AdkatsBanCheckList.Clear();
							
							if (Con.State == ConnectionState.Open) {
								DebugWrite("[BanEnforceThread] Close SQL Connection (Con)", 5);
								Con.Close();
							}
						}
						catch (Exception c) {
							ConsoleError("[BanEnforceThread] Error, can not read from SQL database (MyCommand): " + c);
						}
						if (Con.State == ConnectionState.Open) {
							DebugWrite("[BanEnforceThread] Close SQL Connection (Con)", 5);
							Con.Close();
						}
					}
				}
			}
			catch (Exception c) {
				DebugWrite("[BanEnforceThread] ^bERROR: Check your SQL credentials from Adkats DB^n", 1);
				ConsoleError("[BanEnforceThread] Error (Con): " + c );
			}
		}));
		ThreadWorkerEventLogger1.IsBackground = true;
		ThreadWorkerEventLogger1.Name = "ThreadWorkerEventLogger1";
		ThreadWorkerEventLogger1.Start();
	}

}

public override void OnPlayerJoin(String soldierName) {
	if (!this.onJoinTime.ContainsKey(soldierName)) {
		this.onJoinTime.Add(soldierName, DateTime.UtcNow);
	}
}

public override void OnRoundOver(int winningTeamId) {
	this.OnRoundOverTime = DateTime.UtcNow;
}

public override void OnPlayerDisconnected(String playerName, String reason) {
	if (!this.fIsEnabled) { return; }
	if (this.onJoinTime.ContainsKey(playerName)) {this.onJoinTime.Remove(playerName);}

	if ((this.SettingPlayerDisconnectedConsole == enumBoolYesNo.Yes) || (this.SettingPlayerDisconnectedChat == enumBoolYesNo.Yes) || (this.SettingPlayerDisconnectedLogfile == enumBoolYesNo.Yes) || (this.SettingPlayerDisconnectedDB == enumBoolYesNo.Yes) || (this.SettingFilterAutoBan.Length > 4)) {
		//filter
		if ((this.SettingFilterPlayername != String.Empty) && (playerName != String.Empty)) {
			if (Regex.Match(playerName, this.SettingFilterPlayername).Success) {
				return;
			}
		}
		if ((this.SettingFilterReason != String.Empty) && (reason != String.Empty)) {
			if (Regex.Match(reason, this.SettingFilterReason).Success) {
				return;
			}
		}
		if (this.SettingNoEmptyStringInDisReason == enumBoolYesNo.Yes) {
			if (reason == String.Empty) {
				return;
			}
		}
		if ((this.SettingFilterAutoBan.Length > 4) && (reason.Length > 4)) {
			if (!reason.StartsWith("Triggered ")) {
				if (Regex.Match(reason, this.SettingFilterAutoBan).Success) {
					string tmp_reason = reason;
					if ((reason.Contains("PunkBuster kicked player")) && (reason.Contains("Cheater banned by GGC-Stream.NET")) && (this.SettingFilterAutoBan.Contains("banned by GGC-Stream"))) {
						// special, import ggc stream bans
						Match regexMatch1 = Regex.Match(reason, "GUID \\w+");
						if (regexMatch1.Success) {
							if (regexMatch1.Groups[0].Value.Length > 2) {
								tmp_reason = "Cheater banned by GGC-Stream (" + regexMatch1.Groups[0].Value.Replace("GUID ", "") + ")";
							}
						}
					} else if ((reason.Contains("PunkBuster kicked player")) && (reason.Contains("Violation")) && (this.SettingFilterAutoBan.Contains("PunkBuster")) && (this.SettingFilterAutoBan.Contains("Violation"))) {
						// special, import punkbuster bans
						Match regexMatch2 = Regex.Match(reason, "Violation\\s\\(\\w+\\)\\s.+");
						if (regexMatch2.Success) {
							if (regexMatch2.Groups[0].Value.Length > 2) {
								tmp_reason = "PunkBuster ban " + regexMatch2.Groups[0].Value;
							}
						}
					} else if ((reason.Contains("PunkBuster permanent ban issued on this Game Server for player")) && (reason.Contains("GUID BAN")) && (this.SettingFilterAutoBan.Contains("PunkBuster permanent ban issued on this Game Server for player"))) {
						// special, import punkbuster bans
						tmp_reason = "PunkBuster ban detected";
					}
					// Check for pbbans unofficial bans, e.g. "PunkBuster kicked player THE_SQUADs_manky (for 1200 minutes) ... PBBans.com enforced a previous MBi Ban for THE_SQUADs_manky. [Admin Decision]"
					else if ((reason.Contains("PunkBuster kicked player")) && (reason.Contains("PBBans.com enforced a previous MBi Ban")) && (this.SettingFilterAutoBan.Contains("enforced a previous MBi Ban"))) {
						// special, import unofficial pbbans MBi bans
						Match regexMatch3 = Regex.Match(reason, "previous MBi Ban for \\S+");
						if (regexMatch3.Success) {
							if (regexMatch3.Groups[0].Value.Length > 2) {
								tmp_reason = "PunkBuster previous MBi Ban (" + regexMatch3.Groups[0].Value.Replace("previous MBi Ban for ", "") + ")";
							}
						}
					}
					// Check for BF4DB bans, e.g. "[BF4DB] Suspicious Stats. Appeal at https://bf4db.com/player/ban/2942 [ARPAdmin]"
					else if ((reason.Contains("[BF4DB]")) && (reason.Contains("Appeal at")) && (this.SettingFilterAutoBan.Contains("BF4DB"))) {
						// special, import unofficial pbbans MBi bans
						Match regexMatch4 = Regex.Match(reason, @"\s*([^\n\r]*)Appeal at");
						if (regexMatch4.Success) {
							if (regexMatch4.Groups[1].Value.Length > 2) {
								tmp_reason = "[BF4DB] ( " + regexMatch4.Groups[1].Value.Replace("[BF4DB] ", "") + ")";
								tmp_reason = tmp_reason.Replace("Battlefield ", "BF").Replace("Battlefield", "BF").Replace("Suspicious", "Susp.");
							}
						}
					}
					// perma ban
					tmp_reason = "Triggered " + tmp_reason;
					if (tmp_reason.Length > 80) { tmp_reason = tmp_reason.Substring(0, 80); }
					DebugWrite("[PlayerDisconnected] [AutoBan] Ban player " + playerName + ". Reason: " + tmp_reason , 2);
					this.ExecuteCommand("procon.protected.send", "banList.add", "name", playerName, "perm", tmp_reason);
					this.ExecuteCommand("procon.protected.send", "banList.save");
					this.ExecuteCommand("procon.protected.send", "banList.list");
				}
			}
		}

		if (this.SettingPlayerDisconnectedConsole == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Disconnected^n > " + this.strBlack(playerName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerDisconnectedChat == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Disconnected^n > " + this.strBlack(playerName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerDisconnectedLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Player Disconnected > " + playerName + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerDisconnectedDB == enumBoolYesNo.Yes) { this.SqlLog("Player Disconnected", playerName, reason) ; }
	}
	//if ((this.AggressiveJoin) && (reason == "PLAYER_KICKED")) {
}

public override void OnPlayerKicked(String soldierName, String reason) {
	if (!this.fIsEnabled) { return; }
	if ((this.SettingPlayerKickedChat == enumBoolYesNo.Yes) || (this.SettingPlayerKickedConsole == enumBoolYesNo.Yes) || (this.SettingPlayerKickedLogfile == enumBoolYesNo.Yes) || (this.SettingPlayerKickedDB == enumBoolYesNo.Yes)) {
		//filter
		if ((this.SettingFilterPlayername != String.Empty) && (soldierName != String.Empty)) {
			if (Regex.Match(soldierName, this.SettingFilterPlayername).Success) {
				return;
			}
		}
		if ((this.SettingFilterReason != String.Empty) && (reason != String.Empty)) {
			if (Regex.Match(reason, this.SettingFilterReason).Success) {
				return;
			}
		}
		
		if (this.SettingPlayerKickedChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Kicked^n > " + this.strBlack(soldierName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Kicked^n > " + this.strBlack(soldierName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Player Kicked > " + soldierName + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedDB == enumBoolYesNo.Yes) { this.SqlLog("Player Kicked", soldierName, reason) ; }
	}
}

public override void OnPlayerKickedByAdmin(String soldierName, String reason) {
	if (!this.fIsEnabled) { return; }
	if ((this.SettingPlayerKickedByAdminChat == enumBoolYesNo.Yes) || (this.SettingPlayerKickedByAdminConsole == enumBoolYesNo.Yes) || (this.SettingPlayerKickedByAdminLogfile == enumBoolYesNo.Yes) || (this.SettingPlayerKickedByAdminDB == enumBoolYesNo.Yes)) {
		//filter
		if ((this.SettingFilterPlayername != String.Empty) && (soldierName != String.Empty)) {
			if (Regex.Match(soldierName, this.SettingFilterPlayername).Success) {
				return;
			}
		}
		if ((this.SettingFilterReason != String.Empty) && (reason != String.Empty)) {
			if (Regex.Match(reason, this.SettingFilterReason).Success) {
				return;
			}
		}
		
		if (this.SettingPlayerKickedByAdminChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Kicked By Admin^n > " + this.strBlack(soldierName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedByAdminConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Kicked By Admin^n > " + this.strBlack(soldierName) + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedByAdminLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Kicked By Admin > " + soldierName + " - RESAON: " + reason + " -") ; }
		if (this.SettingPlayerKickedByAdminDB == enumBoolYesNo.Yes) { this.SqlLog("Player Kicked By Admin", soldierName, reason) ; }
	}
}

public override void OnPlayerKilledByAdmin(String soldierName) {
	if (!this.fIsEnabled) { return; }
	if ((this.SettingPlayerKilledByAdminChat == enumBoolYesNo.Yes) || (this.SettingPlayerKilledByAdminConsole == enumBoolYesNo.Yes) || (this.SettingPlayerKilledByAdminLogfile == enumBoolYesNo.Yes) || (this.SettingPlayerKilledByAdminDB == enumBoolYesNo.Yes)) {
		//filter
		if ((this.SettingFilterPlayername != String.Empty) && (soldierName != String.Empty)) {
			if (Regex.Match(soldierName, this.SettingFilterPlayername).Success) {
				return;
			}
		}
		
		if (this.SettingPlayerKilledByAdminChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Killed By Admin^n > " + this.strBlack(soldierName)) ; }
		if (this.SettingPlayerKilledByAdminConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Killed By Admin^n > " + this.strBlack(soldierName)) ; }
		if (this.SettingPlayerKilledByAdminLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Killed By Admin > " + soldierName) ; }
		if (this.SettingPlayerKilledByAdminDB == enumBoolYesNo.Yes) { this.SqlLog("Player Kicked By Admin", soldierName, "") ; }
	}
}

public override void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingBetweenRoundsNoLog == enumBoolYesNo.Yes) {
		if (((DateTime.UtcNow - this.OnRoundOverTime).TotalSeconds) < 80) {
			return;
		}
		if (this.onJoinTime.ContainsKey(soldierName)) {
			if (((DateTime.UtcNow - this.onJoinTime[soldierName]).TotalSeconds) < 240) { 
				return;
			} else {
				this.onJoinTime.Remove(soldierName);
			}
		}
	}


	if (forceKilled) {
		if ((this.SettingPlayerForceMovedByAdminChat == enumBoolYesNo.Yes) || (this.SettingPlayerForceMovedByAdminConsole == enumBoolYesNo.Yes) || (this.SettingPlayerForceMovedByAdminLogfile == enumBoolYesNo.Yes) || (this.SettingPlayerForceMovedByAdminDB == enumBoolYesNo.Yes)) {
			//filter
			if ((this.SettingFilterPlayername != String.Empty) && (soldierName != String.Empty)) {
				if (Regex.Match(soldierName, this.SettingFilterPlayername).Success) {
					return;
				}
			}
			if (this.SettingPlayerForceMovedByAdminChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Force Moved By Admin^n > " + this.strBlack(soldierName)) ; }
			if (this.SettingPlayerForceMovedByAdminConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Force Moved By Admin^n > " + this.strBlack(soldierName)) ; }
			if (this.SettingPlayerForceMovedByAdminLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Player Force Moved By Admin > " + soldierName) ; }
			if (this.SettingPlayerForceMovedByAdminDB == enumBoolYesNo.Yes) { this.SqlLog("Player Force Moved By Admin", soldierName, "") ; }
		}
	} else {
		if ((this.SettingPlayerMovedByAdminChat == enumBoolYesNo.Yes) || (this.SettingPlayerMovedByAdminConsole == enumBoolYesNo.Yes) || (this.SettingPlayerMovedByAdminLogfile == enumBoolYesNo.Yes)) {
			//filter
			if ((this.SettingFilterPlayername != String.Empty) && (soldierName != String.Empty)) {
				if (Regex.Match(soldierName, this.SettingFilterPlayername).Success) {
					return;
				}
			}
			if (this.SettingPlayerMovedByAdminChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPlayer Moved By Admin^n > " + this.strBlack(soldierName)) ; }
			if (this.SettingPlayerMovedByAdminConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPlayer Moved By Admin^n > " + this.strBlack(soldierName)) ; }
			if (this.SettingPlayerMovedByAdminLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Player Moved By Admin > " + soldierName) ; }
			if (this.SettingPlayerForceMovedByAdminDB == enumBoolYesNo.Yes) { this.SqlLog("Player Moved By Admin", soldierName, "") ; }
		}
	}
}

//public override void OnPlayerKilled(Kill kKillerVictimDetails) {
//	DebugWrite("[OnPlayerKilled] " + kKillerVictimDetails.Killer.SoldierName + " killed " + this.strBlack(kKillerVictimDetails.Victim.SoldierName), 5);
//}

public override void OnBanAdded(CBanInfo ban) {
	if (!this.fIsEnabled) { return; }
	if (ban.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Permanent) {
		if ((this.SettingBanAddedChat == enumBoolYesNo.Yes) || (this.SettingBanAddedConsole == enumBoolYesNo.Yes) || (this.SettingBanAddedLogfile == enumBoolYesNo.Yes) || (this.SettingBanAddedDB == enumBoolYesNo.Yes)) {
			//filter
			if ((this.SettingFilterPlayername != String.Empty) && (ban.SoldierName != String.Empty)) {
				if (Regex.Match(ban.SoldierName, this.SettingFilterPlayername).Success) {
					return;
				}
			}
			if ((this.SettingFilterReason != String.Empty) && (ban.Reason != String.Empty)) {
				if (Regex.Match(ban.Reason, this.SettingFilterReason).Success) {
					return;
				}
			}
			
			if (this.SettingBanAddedChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bPerma-Ban^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingBanAddedConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bPerma-Ban^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingBanAddedLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Perma-Ban > " + ban.SoldierName + " ( " + ban.Guid + " )  - - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingBanAddedDB == enumBoolYesNo.Yes) { this.SqlLog("Perma-Ban", ban.SoldierName  + " ( " + ban.Guid + " )", ban.Reason) ; }
		}
	} else if (ban.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Round) {
		if ((this.SettingTimeBanAddedChat == enumBoolYesNo.Yes) || (this.SettingTimeBanAddedConsole == enumBoolYesNo.Yes) || (this.SettingTimeBanAddedLogfile == enumBoolYesNo.Yes)) {
			//filter
			if ((this.SettingFilterPlayername != String.Empty) && (ban.SoldierName != String.Empty)) {
				if (Regex.Match(ban.SoldierName, this.SettingFilterPlayername).Success) {
					return;
				}
			}
			if ((this.SettingFilterReason != String.Empty) && (ban.Reason != String.Empty)) {
				if (Regex.Match(ban.Reason, this.SettingFilterReason).Success) {
					return;
				}
			}
			
			if (this.SettingTimeBanAddedChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bTime-Ban > Round^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bTime-Ban > Round^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Time-Ban > Round > " + ban.SoldierName + " ( " + ban.Guid + " )  - - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedDB == enumBoolYesNo.Yes) { this.SqlLog("Time-Ban > Round", ban.SoldierName  + " ( " + ban.Guid + " )", ban.Reason) ; }
		}
	} else if (ban.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds) {
		if ((this.SettingTimeBanAddedChat == enumBoolYesNo.Yes) || (this.SettingTimeBanAddedConsole == enumBoolYesNo.Yes) || (this.SettingTimeBanAddedLogfile == enumBoolYesNo.Yes)) {
			//filter
			if ((this.SettingFilterPlayername != String.Empty) && (ban.SoldierName != String.Empty)) {
				if (Regex.Match(ban.SoldierName, this.SettingFilterPlayername).Success) {
					return;
				}
			}
			if ((this.SettingFilterReason != String.Empty) && (ban.Reason != String.Empty)) {
				if (Regex.Match(ban.Reason, this.SettingFilterReason).Success) {
					return;
				}
			}
			
			if (this.SettingTimeBanAddedChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bTime-Ban > " + this.banTime(ban.BanLength.Seconds) + " days^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bTime-Ban > " + this.banTime(ban.BanLength.Seconds) + " days^n > " + this.strBlack(ban.SoldierName) + " ( " + ban.Guid + " )  - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Time-Ban > " + ban.BanLength.Seconds + " days > " + ban.SoldierName + " ( " + ban.Guid + " )  - - RESAON: " + ban.Reason + " -") ; }
			if (this.SettingTimeBanAddedDB == enumBoolYesNo.Yes) { this.SqlLog("Time-Ban", ban.SoldierName + " ( " + ban.Guid + " )", ban.Reason  +" (" + this.banTime(ban.BanLength.Seconds) + "days)") ; }
		}
	}
}

public override void OnAccountCreated(String username) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bNew Admin Added^n > " + this.strBlack(username)) ; }
	if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bNew Admin Added^n > " + this.strBlack(username)) ; }
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > New Admin Added > " + username) ; }
	if (this.SettingProconLayerDB == enumBoolYesNo.Yes) { this.SqlLog("Layer", username, "added as new admin") ; }
}

public override void OnAccountDeleted(String username) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bAdmin Removed^n > " + this.strBlack(username)) ; }
	if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bAdmin Removed^n > " + this.strBlack(username)) ; }
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Admin Removed > " + username) ; }
	if (this.SettingProconLayerDB == enumBoolYesNo.Yes) { this.SqlLog("Layer", username, "removed as admin") ; }
}

public override void OnAccountLogin(string accountName, string ip, CPrivileges privileges) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bProcon Login^n > " + this.strBlack(accountName) + " (IP: " + ip + ")") ; }
	if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bProcon Login^n > " + this.strBlack(accountName) + " (IP: " + ip + ")") ; }
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Procon Login > " + accountName + " (IP: " + ip + ")") ; }
	if (this.SettingProconLayerDB == enumBoolYesNo.Yes) { this.SqlLog("Layer", accountName, "Procon login from IP " + ip) ; }
}

public override void OnAccountLogout(string accountName, string ip, CPrivileges privileges) {
	if (!this.fIsEnabled) { return; }
	if (this.SettingProconLayerChat == enumBoolYesNo.Yes) { this.ExecuteCommand("procon.protected.chat.write", "Event Logger > ^bProcon Logout^n > " + this.strBlack(accountName)) ; }
	if (this.SettingProconLayerConsole == enumBoolYesNo.Yes) { ConsoleWrite("^bProcon Logout^n > " + this.strBlack(accountName)) ; }
	if (this.SettingProconLayerLogfile == enumBoolYesNo.Yes) { this.tmpLogfile.Add("[" + DateTime.Now.ToString() + "]  Event Logger > Procon Logout > " + accountName) ; }
	if (this.SettingProconLayerDB == enumBoolYesNo.Yes) { this.SqlLog("Layer", accountName, "Procon logout") ; }
}

private string banTime(double timeinp) { return Math.Round((timeinp / 86400), 2).ToString(); }

private string strBlack(String StrInp) { return "^b" + StrInp + "^n"; }

public void WriteLogfile() {
	if (this.tmpLogfile.Count > 0) {
		if ((this.tmpLogfile.Count == 1) && (this.tmpLogfile[0].Contains("Layer restart detected"))  && (this.SettingProconLayerLogfile == enumBoolYesNo.No)) { return; }
		Thread ThreadWorker588 = new Thread(new ThreadStart(delegate() {
			string path = "Configs\\EVENT-LOGGER.txt";
			string tmp_stringbuilder = String.Empty;
			try {
				
				foreach (string tmp_line in this.tmpLogfile) {
					if (tmp_stringbuilder == String.Empty) {
						tmp_stringbuilder = tmp_line;
					} else {
						tmp_stringbuilder = tmp_stringbuilder + Environment.NewLine + tmp_line;
					}
				}
				if(File.Exists(path)) {
					if ((DateTime.Now.Day == 1) && (DateTime.Now.Hour == 3)) {
						File.WriteAllText(path, "[" + DateTime.Now.ToString() + "]  Event Logger > New logfile created.");
					}
				}
				//File.AppendAllLines(path, this.tmpLogfile.ToArray());
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true)) {
					file.WriteLine(tmp_stringbuilder);
					file.Close();
				}
				this.tmpLogfile.Clear();
			}
			catch (Exception e) {
				ConsoleError("[SAVE LOG] Can NOT write logfile. Requires Read+Write file permission. Path: " + path + "   - ERROR: " + e);
			}
		}));

		ThreadWorker588.IsBackground = true;
		ThreadWorker588.Name = "threadworker588";
		ThreadWorker588.Start();
	}
	if ((DateTime.Now.Hour == 6) || (DateTime.Now.Hour == 15) || (DateTime.Now.Hour == 21)) {
		this.onJoinTime.Clear();
	}
}



public void CheckRestart() {
	if ((this.fIsEnabled) && (this.SettingSqlEnabled == enumBoolYesNo.Yes) && (this.SettingProconLayerDB == enumBoolYesNo.Yes) && (((DateTime.UtcNow - this.LayerStartingTime).TotalSeconds) < 200)) { this.SqlLog("Layer", "Plugin", "Layer restart detected"); }
}
	
// sql zeugs
public void AutoDBCleaner() {
	if (!this.fIsEnabled) { return; }
	bool SqlConOK = false;
	string SQL = "DELETE FROM `event_logger` WHERE (TIMESTAMPDIFF(DAY, timestamp, UTC_TIMESTAMP()) > " + this.SettingDBCleaner.ToString() + ")";
	if (this.SettingSqlEnabled == enumBoolYesNo.Yes) {
		this.TableBuilder();
		if (this.SqlTableExist) {
			try {
				using (MySqlConnection Con = new MySqlConnection(this.SqlLogin())) {
					Con.Open();
					try {
						if (Con.State == ConnectionState.Open) {
							DebugWrite("[AutoDBCleaner] delete old entries in SQL database. SQL COMMAND (MyCom): " + SQL, 4);
							using (MySqlCommand MyCom = new MySqlCommand(SQL, Con)) {
								MyCom.ExecuteNonQuery();
								SqlConOK = true;
								MyCom.Connection.Close();
							}
						} else {
							DebugWrite("[AutoDBCleaner] Can NOT connect to SQL", 5);
							this.SqlTableExist = false;
						}
					}
					catch (Exception c) {
						ConsoleError("[AutoDBCleaner] SQL Error (Con): " + c);
						SqlConOK = false;
						this.SqlTableExist = false;
					}
					finally {
						Con.Close();
						DebugWrite("[AutoDBCleaner] Close SQL Connection (Con)", 5);
					}
				}
			}
			catch (Exception c) {
				ConsoleError("[AutoDBCleaner] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
				SqlConOK = false;
				this.SqlTableExist = false;
			}
		}

		if (!SqlConOK) {
			DebugWrite("[AutoDBCleaner] SQL Connection Error. Can not write in SQL", 2);
			this.SqlTableExist = false;
		}
	}
}

public void SqlLog(string layerEvent, string player, string reason) {
	bool SqlConOK = false;
	if (this.SettingSqlEnabled == enumBoolYesNo.Yes) {
		this.TableBuilder();
		if ((layerEvent.Length < 2) || (player.Length < 2)) { return; }
		if (this.SqlTableExist) {
			try {
				using (MySqlConnection Con = new MySqlConnection(this.SqlLogin())) {
					Con.Open();
					try {
						if (Con.State == ConnectionState.Open) {
							string SQL = "INSERT INTO `event_logger` (`gameserver`, `event`, `timestamp`, `playername`, `msg`) VALUES ('" + this.SettingStrSqlGameserver + "', '" + this.strSqlProtection(layerEvent) + "', UTC_TIMESTAMP() , '" + this.strSqlProtection(player) + "', '" + this.strSqlProtection(reason) + "')";

							using (MySqlCommand MyCom = new MySqlCommand(SQL, Con)) {
								MyCom.ExecuteNonQuery();
								SqlConOK = true;
								MyCom.Connection.Close();
							}
						} else {
							DebugWrite("[SqlLog] Can NOT connect to SQL", 5);
						}
					}
					catch (Exception c) {
						ConsoleError("[SqlLog] SQL Error (Con): " + c);
						SqlConOK = false;
						this.SqlTableExist = false;
					}
					finally {
						Con.Close();
						DebugWrite("[SqlLog] Close SQL Connection (Con)", 5);
					}
				}
			}
			catch (Exception c) {
				ConsoleError("[SqlLog] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c);
				SqlConOK = false;
				this.SqlTableExist = false;
			}
		}
		if (!SqlConOK) {
			DebugWrite("[SqlLog] SQL Connection Error. Can not write in SQL", 2);
			this.SqlTableExist = false;
		}
	}
}

private void TableBuilder() {
	if (!this.fIsEnabled) { return; }
	bool TableExist = false;
	bool TableCreated = false;

	if (this.SettingSqlEnabled == enumBoolYesNo.Yes) {
		if (!this.SqlTableExist) {
			if (this.SqlLoginsOk()) {
				DebugWrite("[SQL-TableBuilder] Connecting to SQL and check database", 4);
				Thread MySQLWorker4 = new Thread(new ThreadStart(delegate() {
					try {
						using (MySqlConnection Con = new MySqlConnection(this.SqlLogin())) {
							Con.Open();
							try {
								// check if table exist in SQL database
								string SQL = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='event_logger' AND table_schema='" + this.SettingStrSqlDatabase + "'";
								DebugWrite("[SQL-TableBuilder] [CheckExist] Connected to SQL. Check if table exist or not in SQL database. SQL COMMAND (MyCommand): " + SQL, 5);
								using (MySqlCommand MyCommand = new MySqlCommand(SQL)) {
									DataTable resultTable = this.SQLquery(MyCommand);
									if (resultTable.Rows != null) {
										DebugWrite("[SQL-TableBuilder] [CheckExist] Receive informations from SQL", 5);
										foreach (DataRow row in resultTable.Rows) {
											// reading sql
											if (row["COLUMN_NAME"].ToString() == "gameserver") {
												// yes, table 'event_logger' exist in SQL DB!!
												DebugWrite("[SQL-TableBuilder] [CheckExist] Table 'event_logger' exist in SQL database", 5);
												TableExist = true;
											}
										}
									} else {
										ConsoleError("[SQL-TableBuilder] [CheckExist] Table 'event_logger' NOT exist on your SQL Server");
									}
								}
							}
							catch (Exception c) {
								ConsoleError("[SQL-TableBuilder] [CheckExist] SQL Error (MyCommand): " + c);
								TableExist = false;
							}

							// create NEW table in SQL if not exist (first plugin start after installation)
							if (!TableExist) {
								////////////////////////////////
								// start table bulider
								////////////////////////////////
								try {
									string SqlTableBuild = String.Empty;
									if (!TableExist) {
										SqlTableBuild = "CREATE TABLE IF NOT EXISTS `event_logger` (`ID` INT NOT NULL AUTO_INCREMENT ,`gameserver` varchar(30) NOT NULL, `event` varchar(30) NOT NULL, `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, `playername` varchar(35) NULL DEFAULT NULL, `msg` varchar(400) NULL DEFAULT NULL,PRIMARY KEY (`ID`))ENGINE = InnoDB";
										ConsoleWrite("[SQL-TableBuilder] [CreateTable] Plugin create NEW table 'event_logger' SQL database");
										DebugWrite("^b[SQL-TableBuilder] [CreateTable] Connected to SQL.^n SQL COMMAND (MyCom): " + SqlTableBuild, 4);
										using (MySqlCommand MyCom = new MySqlCommand(SqlTableBuild, Con)) {
											MyCom.ExecuteNonQuery();
											MyCom.Connection.Close();
											TableCreated = true;
										}
									}
								}
								catch (MySqlException oe) {
									ConsoleError("[SQL-TableBuilder] [CreateTable] Error in Tablebuilder:");
									this.DisplayMySqlErrorCollection(oe);
									TableCreated = false;
								}
								catch (Exception c) {
									ConsoleError("[SQL-TableBuilder] [CreateTable] SQL Error (MyCom): " + c );
									TableCreated = false;
								}
								finally {
									DebugWrite("[[SQL-TableBuilder] Close SQL Connection (Con)", 5);
									Con.Close();
									if (TableCreated) {
										this.SqlTableExist = true;
										ConsoleWrite("[SQL-TableBuilder] ^b^2NEW table created successfully^0^n");
									}
								}
							} else {
								if (Con.State == ConnectionState.Open) {
									DebugWrite("[[SQL-TableBuilder] Close SQL Connection (Con)", 5);
									Con.Close(); 
								}
								this.SqlTableExist = true;
							}
						}
					}
					catch (Exception c) {
						ConsoleError("[SQL-TableBuilder] ERROR: CAN NOT CONNECT TO SQL SERVER. Error (Con): " + c );
					}
				}));
				MySQLWorker4.IsBackground = true;
				MySQLWorker4.Name = "mysqlworker4";
				MySQLWorker4.Start();
			}
		}
	}
}

// sql zeugs main
private DataTable SQLquery(MySqlCommand selectQuery) {
	DataTable MyDataTable = new DataTable();
	try {
		if (selectQuery == null) {
			ConsoleWrite("SQLquery: selectQuery is null");
			return MyDataTable;
		} else if (selectQuery.CommandText.Equals(String.Empty) == true) {
			DebugWrite("[SQLquery] CommandText is empty", 4);
			return MyDataTable;
		}

		try {
			using (MySqlConnection Connection = new MySqlConnection(this.SqlLogin())) {
				selectQuery.Connection = Connection;
				using (MySqlDataAdapter MyAdapter = new MySqlDataAdapter(selectQuery)) {
					if (MyAdapter != null) {
						MyAdapter.Fill(MyDataTable);
					} else {
						DebugWrite("[SQLquery] MyAdapter is null", 4);
					}
				}
				Connection.Close();
			}
		}
		catch (MySqlException me) {
			ConsoleError("[SQLquery] Error in SQL.");
			this.DisplayMySqlErrorCollection(me);
			this.SqlTableExist = false;
		}
		catch (Exception c) {
			ConsoleError("[SQLquery] Error in SQL Query: " + c);
			this.SqlTableExist = false;
		}
	}
	catch (Exception c) {
		ConsoleError("[SQLquery] SQLQuery OuterException: " + c);
		this.SqlTableExist = false;
	}
	return MyDataTable;
}

// sql zeugs main 2
private DataTable AdkatsSQLquery(MySqlCommand selectQuery) {
	DataTable MyDataTable = new DataTable();
	try {
		if (selectQuery == null) {
			ConsoleWrite("SQLquery: selectQuery is null");
			return MyDataTable;
		} else if (selectQuery.CommandText.Equals(String.Empty) == true) {
			DebugWrite("[SQLquery] CommandText is empty", 4);
			return MyDataTable;
		}

		try {
			using (MySqlConnection Connection = new MySqlConnection(this.AdkatsSqlLogin())) {
				selectQuery.Connection = Connection;
				using (MySqlDataAdapter MyAdapter = new MySqlDataAdapter(selectQuery)) {
					if (MyAdapter != null) {
						MyAdapter.Fill(MyDataTable);
					} else {
						DebugWrite("[SQLquery] MyAdapter is null", 4);
					}
				}
				Connection.Close();
			}
		}
		catch (MySqlException me) {
			ConsoleError("[SQLquery] Error in SQL.");
			this.DisplayMySqlErrorCollection(me);
			this.SqlTableExist = false;
		}
		catch (Exception c) {
			ConsoleError("[SQLquery] Error in SQL Query: " + c);
			this.SqlTableExist = false;
		}
	}
	catch (Exception c) {
		ConsoleError("[SQLquery] SQLQuery OuterException: " + c);
		this.SqlTableExist = false;
	}
	return MyDataTable;
}

public void DisplayMySqlErrorCollection(MySqlException myException) {
	this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Message: " + myException.Message + "^0");
	this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Native: " + myException.ErrorCode.ToString() + "^0");
	this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Source: " + myException.Source.ToString() + "^0");
	this.ExecuteCommand("procon.protected.pluginconsole.write", "^1StackTrace: " + myException.StackTrace.ToString() + "^0");
	this.ExecuteCommand("procon.protected.pluginconsole.write", "^1InnerException: " + myException.InnerException.ToString() + "^0");
}

public void TmpListCleaner() {
	if (!this.fIsEnabled) { return; }
	AdkatsBanChecked.Clear();
	AdkatsBanned.Clear();
	GuidToNameList.Clear();
}

private bool SqlLoginsOk() {
	if ((this.SettingStrSqlHostname != String.Empty) && (this.SettingStrSqlPort != String.Empty) && (this.SettingStrSqlDatabase != String.Empty) && (this.SettingStrSqlUsername != String.Empty) && (this.SettingStrSqlPassword != String.Empty)) {
		return true;
	} else {
		ConsoleWrite("[SqlLoginDetails]^8^b SQL Server Details not completed (Host IP, Port, Database, Username, PW). Please check your Plugin settings.^0^n");
		DebugWrite("[SqlLoginDetails] SQL Details: Host=`" + this.SettingStrSqlHostname + "` ; Port=`" + this.SettingStrSqlPort + "` ; Database=`" + this.SettingStrSqlDatabase + "` ; Username=`" + this.SettingStrSqlUsername + "` ; Password=`" + this.SettingStrSqlPassword + "`" ,2);

		return false;
	}
}

private string SqlLogin() {	return "Server=" + this.SettingStrSqlHostname + ";" + "Port=" + this.SettingStrSqlPort + ";" + "Database=" + this.SettingStrSqlDatabase + ";" + "Uid=" + this.SettingStrSqlUsername + ";" + "Pwd=" + this.SettingStrSqlPassword + ";" + "Connection Timeout=5;"; }

private string AdkatsSqlLogin() { return "Server=" + this.SettingAdkatsDBIP + ";" + "Port=" + this.SettingStrSqlPort + ";" + "Database=" + this.SettingAdkatsDBName + ";" + "Uid=" + this.SettingAdkatsDBUser + ";" + "Pwd=" + this.SettingAdkatsDBPw + ";" + "Connection Timeout=5;"; }

private string strSqlProtection(String StrInp) {return StrInp.Replace("\\0", "").Replace("\\b", "").Replace("\\0", "").Replace("\\n", "").Replace("\\r", "").Replace("\\t", "").Replace("\\Z", "").Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "").Replace("'", "").Replace("’", "").Replace("‘", "") ;}

private string BanTimeString(int Minutes) {
	int tmp_days = Minutes / 1440;
	int tmp_hours = (Minutes - (tmp_days * 1440)) / 60;
	string tmp_ret = "TIMEBAN ";
	if (this.SettingAdkatsBanReason.Length > 2) { return "BANNED FOR: "; }
	if (tmp_days >= 300) {
		tmp_ret = "BANNED permanent FOR: ";
	} else {
		if (tmp_days > 1) {
			tmp_ret += tmp_days + " days ";
		} else if (tmp_days == 1) {
			tmp_ret += tmp_days + " day ";
		}
		if (tmp_hours > 1) {
			tmp_ret += tmp_hours + " hours ";
		} else if (tmp_hours <= 1) {
			tmp_ret += tmp_hours + " hour ";
		}
		tmp_ret = tmp_ret + "FOR: ";
	}
	return tmp_ret;

}

private string GuidToName(string xguid) {
	if (this.GuidToNameList.ContainsKey(xguid)) { return this.GuidToNameList[xguid]; }
	return String.Empty;
}

private string BanReasonCleaned(string fullMessage, string xplayername) {
	if ((fullMessage.StartsWith("Permban")) || (fullMessage.StartsWith("Perm-Ban")) || (fullMessage.StartsWith("Permaban")) || (fullMessage.StartsWith("PermaBan")) || (fullMessage.StartsWith("permaban")) || (fullMessage.StartsWith("permban")) || (fullMessage.StartsWith("perm-ban")) || (fullMessage.StartsWith("Permanent Ban")) || (fullMessage.StartsWith("Permanent-Ban")) || (fullMessage.StartsWith("permanent-ban"))) {
		fullMessage = fullMessage.Replace("Permban", "").Replace("Perm-Ban", "").Replace("Permaban", "").Replace("PermaBan", "").Replace("permaban", "").Replace("permban", "").Replace("perm-ban", "").Replace("Permanent Ban", "").Replace("Permanent-Ban", "").Replace("permanent-ban", "");
	}
	fullMessage = fullMessage.Replace("TIMEBAN for 20 minutes by", "").Replace("TIME BAN (3 hours) ", "").Replace("(Temporary/20)", "").Replace(" > TIME BAN (20min) ", " > ").Replace("(Permanent)", "");
	if (this.SettingAdkatsBanReason.Length > 2) { fullMessage = this.SettingAdkatsBanReason.Replace("%player%", xplayername).Replace("%reason%", fullMessage); }
	if (fullMessage.Length > 170) { fullMessage = fullMessage.Substring(0, 170); }
	return fullMessage;
}


}
} // end namespace PRoConEvents
