/*  Copyright 2013 [GWC]XpKillerhx

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
//using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Data;
using System.Configuration;
using System.ComponentModel;
using System.Threading;
//using System.Net;

//MySQL native includes
//using MySql.Data;
using MySql.Data.MySqlClient;


//Procon includes
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;

namespace PRoConEvents
{
    public class CChatGUIDStatsLogger : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region Variables and Constructor

        private MatchCommand loggerStatusCommand;

        //Proconvariables
        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        //Tablebuilder
        private readonly object tablebuilderlock;

        //other locks
        private readonly object chatloglock;
        private readonly object sqlquerylock;
        private readonly object sessionlock;
        private readonly object streamlock;
        private readonly object ConnectionStringBuilderlock;
        private readonly object registerallcomandslock;

        //Dateoffset
        private myDateTime_W MyDateTime;
        private double m_dTimeOffset;

        //Logging
        private Dictionary<string, CPunkbusterInfo> m_dicPbInfo = new Dictionary<string, CPunkbusterInfo>();
        //Chatlog
        private static List<CLogger> ChatLog = new List<CLogger>();
        private List<string> lstStrChatFilterRules;
        private List<Regex> lstChatFilterRules;
        //Statslog
        private Dictionary<string, CStats> StatsTracker = new Dictionary<string, CStats>();
        //Dogtags
        private Dictionary<CKillerVictim, int> m_dicKnifeKills = new Dictionary<CKillerVictim, int>();
        //Session
        private Dictionary<string, CStats> m_dicSession = new Dictionary<string, CStats>();
        private CMapstats Mapstats;
        private CMapstats Nextmapinfo;
        private List<CStats> lstpassedSessions = new List<CStats>();

        //GameMod
        //private string m_strGameMod;

        //Spamprotection
        private int numberOfAllowedRequests;
        private CSpamprotection Spamprotection;

        //Keywords
        private List<string> m_lstTableconfig = new List<string>();
        private Dictionary<string, List<string>> m_dicKeywords = new Dictionary<string, List<string>>();

        //Weapondic
        private Dictionary<string, Dictionary<string, CStats.CUsedWeapon>> weaponDic = new Dictionary<string, Dictionary<string, CStats.CUsedWeapon>>();

        //DamageClassDic
        private Dictionary<string, string> DamageClass = new Dictionary<string, string>();

        //WelcomeStatsDic
        private Dictionary<string, DateTime> welcomestatsDic = new Dictionary<string, DateTime>();

        //Weapon Mapping Dictionary
        private Dictionary<string, int> WeaponMappingDic = new Dictionary<string, int>();

        //ServerID
        private int ServerID;

        //Awards
        private List<string> m_lstAwardTable = new List<string>();

        //Tablenames
        private string tbl_playerdata;
        private string tbl_playerstats;
        private string tbl_weaponstats;
        private string tbl_dogtags;
        private string tbl_mapstats;
        private string tbl_chatlog;
        private string tbl_bfbcs;
        private string tbl_awards;
        private string tbl_server;
        private string tbl_server_player;
        private string tbl_server_stats;
        private string tbl_playerrank;
        private string tbl_sessions;
        private string tbl_currentplayers;
        private string tbl_weapons;
        private string tbl_weapons_stats;
        private string tbl_games;
        private string tbl_teamscores;

        // Timelogging
        private bool bool_roundStarted;
        private DateTime Time_RankingStarted;

        //Other
        private Dictionary<string, CPlayerInfo> m_dicPlayers = new Dictionary<string, CPlayerInfo>();   //Players

        //ID Cache
        private Dictionary<string, C_ID_Cache> m_ID_cache = new Dictionary<string, C_ID_Cache>();

        //Various Variables
        //private int m_strUpdateInterval;
        private bool isStreaming;
        private string serverName;
        private bool m_isPluginEnabled;
        private bool boolTableEXISTS;
        private bool boolKeywordDicReady;
        private string tableSuffix;
        private bool MySql_Connection_is_activ;
        //Last time Stat Logger actively interacted with the database
        private DateTime lastDBInteraction = DateTime.MinValue;

        //Update skipswitches
        private bool boolSkipGlobalUpdate;
        private bool boolSkipServerUpdate;
        private bool boolSkipServerStatsUpdate;

        //Transaction retry
        private int TransactionRetryCount;

        //Playerstartcount
        private int intRoundStartCount;
        private int intRoundRestartCount;

        //Webrequest
        private int m_requestIntervall;
        private string m_webAddress;

        //BFBCS
        //private double BFBCS_UpdateInterval;
        //private int BFBCS_Min_Request;

        //Database Connection Variables
        private string m_strHost;
        private string m_strDBPort;
        private string m_strDatabase;
        private string m_strUserName;
        private string m_strPassword;
        //private string m_strDatabaseDriver;

        //Stats Message Variables        
        private List<string> m_lstPlayerStatsMessage;
        private List<string> m_lstPlayerOfTheDayMessage;
        private List<string> m_lstPlayerWelcomeStatsMessage;
        private List<string> m_lstNewPlayerWelcomeMsg;
        private List<string> m_lstWeaponstatsMsg;
        private List<string> m_lstServerstatsMsg;
        //private string m_strPlayerWelcomeMsg;
        //private string m_strNewPlayerWelcomeMsg;
        private int int_welcomeStatsDelay;
        private string m_strTop10Header;
        private string m_strTop10RowFormat;
        private string m_strWeaponTop10Header;
        private string m_strWeaponTop10RowFormat;

        //top10 for Period
        private string m_strTop10HeaderForPeriod;

        //Session
        private List<string> m_lstSessionMessage;

        //Debug
        private string GlobalDebugMode;

        //ServerGroup
        private int intServerGroup; 

        //Bools for switch on and off funktions
        private enumBoolYesNo m_enNoServerMsg;	//Logging of Server Messages
        private enumBoolYesNo m_enLogSTATS; 	//Statslogging
        private enumBoolYesNo m_enWelcomeStats;	//WelcomeStats
        private enumBoolYesNo m_enYellWelcomeMSG;	// Yell Welcome Message
        private enumBoolYesNo m_enTop10ingame;		//Top10 ingame
        private enumBoolYesNo m_enRankingByScore;	//Ranking by Score
        private enumBoolYesNo m_enInstantChatlogging;	//Realtime Chatlogging
        private enumBoolYesNo m_enChatloggingON;	// Chatlogging On
        private enumBoolYesNo m_enChatlogFilter;    //Turn on the Chatlogfilter
        private enumBoolYesNo m_enSendStatsToAll;	//All Player see the Stats if someone enter @stats  @rank
        private enumBoolYesNo m_mapstatsON;			//Mapstats
        private enumBoolYesNo m_sessionON; 			//Sessionstats
        private enumBoolYesNo m_weaponstatsON;		//Turn Weaponstats On and Off
        private enumBoolYesNo m_getStatsfromBFBCS;  //Turn Statsfetching from BFBCS On and Off
        private enumBoolYesNo m_awardsON;			//Turn Awards on or off
        private enumBoolYesNo m_enWebrequest;		// Webrequest
        private enumBoolYesNo m_enOverallRanking;   //Overall Ranking
        private enumBoolYesNo m_enableInGameCommands; // Turn InGame Commands on and off
        private enumBoolOnOff m_highPerformanceConnectionMode;
        private enumBoolYesNo m_enSessionTracking;
        private enumBoolYesNo m_kdrCorrection; //Kill death Ratio Correction
        private enumBoolYesNo m_enableCurrentPlayerstatsTable; // experimental
        private enumBoolYesNo m_enLogPlayerDataOnly;
        private enumBoolOnOff m_connectionPooling; //Connection Pooling
        private enumBoolOnOff m_Connectioncompression;

        private int m_maxPoolSize; //Connection Pooling
        private int m_minPoolSize; //Connection Pooling

        //More Database Variables
        //Commands
        //Transactions
        private MySql.Data.MySqlClient.MySqlTransaction MySqlTrans;
        //Connections
        private MySql.Data.MySqlClient.MySqlConnection MySqlCon; //Select Querys 1
        private MySql.Data.MySqlClient.MySqlConnection MySqlChatCon; //MySqlConnection for Chatlogging 
        private MySql.Data.MySqlClient.MySqlConnection MySqlConn; //StartStreaming 2 

        MySqlConnectionStringBuilder myCSB = new MySqlConnectionStringBuilder();

        //ServerInfo Event fix
        private DateTime dtLastServerInfoEvent;
        private int minIntervalllenght;

        //Double Roundendfix
        private DateTime dtLastRoundendEvent;
        private DateTime dtLastOnListPlayersEvent;

        //Top10 for Period
        private int m_intDaysForPeriodTop10;

        //New In-Game Command System
        private string m_IngameCommands_stats;
        private string m_IngameCommands_serverstats;
        private string m_IngameCommands_session;
        private string m_IngameCommands_dogtags;
        private string m_IngameCommands_top10;
        private string m_IngameCommands_playerOfTheDay;
        private string m_IngameCommands_top10ForPeriod;

        private Dictionary<string,CStatsIngameCommands> dicIngameCommands = new Dictionary<string,CStatsIngameCommands>();

        //ServerGametype
        private string strServerGameType = String.Empty;
        private int intServerGameType_ID;

        public CChatGUIDStatsLogger()
        {
            loggerStatusCommand = new MatchCommand("CChatGUIDStatsLogger", "GetStatus", new List<string>(), "CChatGUIDStatsLogger_Status", new List<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.None), "Useable by other plugins to determine the current status of this plugin.");

            //tablebuilderlock
            this.tablebuilderlock = new object();
            //other locks
            this.chatloglock = new object();
            this.sqlquerylock = new object();
            this.sessionlock = new object();
            this.streamlock = new object();
            this.ConnectionStringBuilderlock = new object();
            this.registerallcomandslock = new object();


            //update skipswitch
            this.boolSkipGlobalUpdate = false;
            this.boolSkipServerUpdate = false;
            this.boolSkipServerStatsUpdate = false;

            //Timeoffset
            this.m_dTimeOffset = 0;
            this.MyDateTime = new myDateTime_W(this.m_dTimeOffset);

            //this.m_strUpdateInterval = 30;
            this.isStreaming = true;
            this.serverName = String.Empty;
            this.m_ID_cache = new Dictionary<string, C_ID_Cache>();
            this.m_dicKeywords = new Dictionary<string, List<string>>();
            this.boolKeywordDicReady = false;
            this.tableSuffix = String.Empty;
            this.Mapstats = new CMapstats(MyDateTime.Now, "START", 0, 0, this.m_dTimeOffset);
            this.MySql_Connection_is_activ = false;
            this.numberOfAllowedRequests = 10;

            //Transaction retry count
            TransactionRetryCount = 3;

            //Chatlog
            this.lstStrChatFilterRules = new List<string>();
            this.lstChatFilterRules = new List<Regex>();

            //BFBCS
            //this.BFBCS_UpdateInterval = 72; // hours
            //this.BFBCS_Min_Request = 2; //min Packrate

            //Playerstartcount
            this.intRoundStartCount = 2;
            this.intRoundRestartCount = 1;

            //Webrequest
            this.m_webAddress = String.Empty;
            this.m_requestIntervall = 60;

            //Databasehost
            this.m_strHost = String.Empty;
            this.m_strDBPort = String.Empty;
            this.m_strDatabase = String.Empty;
            this.m_strUserName = String.Empty;
            this.m_strPassword = String.Empty;

            //Various Bools
            this.bool_roundStarted = false;
            this.m_isPluginEnabled = false;
            this.boolTableEXISTS = false;

            //ServerGroup
            this.intServerGroup = 0;

            //Debug
            this.GlobalDebugMode = "Error";

            //Functionswitches
            this.m_enLogSTATS = enumBoolYesNo.No;
            this.m_enWelcomeStats = enumBoolYesNo.No;
            this.m_enYellWelcomeMSG = enumBoolYesNo.No;
            this.m_enTop10ingame = enumBoolYesNo.No;
            this.m_enRankingByScore = enumBoolYesNo.Yes;
            this.m_enNoServerMsg = enumBoolYesNo.No;
            this.m_enInstantChatlogging = enumBoolYesNo.No;
            this.m_enChatloggingON = enumBoolYesNo.No;
            this.m_enChatlogFilter = enumBoolYesNo.No;
            this.m_enSendStatsToAll = enumBoolYesNo.No;
            this.m_mapstatsON = enumBoolYesNo.No;
            this.m_sessionON = enumBoolYesNo.No;
            this.m_weaponstatsON = enumBoolYesNo.Yes;
            this.m_getStatsfromBFBCS = enumBoolYesNo.No;
            this.m_awardsON = enumBoolYesNo.No;
            this.m_enOverallRanking = enumBoolYesNo.No;
            this.m_enableInGameCommands = enumBoolYesNo.Yes;
            this.m_highPerformanceConnectionMode = enumBoolOnOff.Off;

            this.m_kdrCorrection = enumBoolYesNo.Yes; //Kill death Ratio Correction
            this.m_enableCurrentPlayerstatsTable = enumBoolYesNo.No; // experimental

            this.m_enLogPlayerDataOnly = enumBoolYesNo.No;

            this.m_connectionPooling = enumBoolOnOff.On; //Connection Pooling
            this.m_Connectioncompression = enumBoolOnOff.Off;

            this.m_minPoolSize = 0; //Connection Pooling
            this.m_maxPoolSize = 10; //Connection Pooling
            

            //Welcomestats
            //this.m_strPlayerWelcomeMsg = "[yell,4]Nice to see you on our Server again, %playerName%";
            //this.m_strNewPlayerWelcomeMsg = "[yell,4]Welcome to the %serverName% Server, %playerName%";
            this.int_welcomeStatsDelay = 5;
            this.welcomestatsDic = new Dictionary<string, DateTime>();

            //Playerstats
            this.m_lstPlayerStatsMessage = new List<string>();
            this.m_lstPlayerStatsMessage.Add("Serverstats for %playerName%:");
            this.m_lstPlayerStatsMessage.Add("Score: %playerScore%  %playerKills% Kills %playerHeadshots% HS  %playerDeaths% Deaths K/D: %playerKDR%");
            this.m_lstPlayerStatsMessage.Add("Your Serverrank is: %playerRank% of %allRanks%");

            //Player of the day
            this.m_lstPlayerOfTheDayMessage = new List<string>();
            this.m_lstPlayerOfTheDayMessage.Add("%playerName% is the Player of the day");
            this.m_lstPlayerOfTheDayMessage.Add("Score: %playerScore%  %playerKills% Kills %playerHeadshots% HS  %playerDeaths% Deaths K/D: %playerKDR%");
            this.m_lstPlayerOfTheDayMessage.Add("His Serverrank is: %playerRank% of %allRanks%");
            this.m_lstPlayerOfTheDayMessage.Add("Overall playtime for today: %playerPlaytime%");

            //Welcomestats
            this.m_lstPlayerWelcomeStatsMessage = new List<string>();
            this.m_lstPlayerWelcomeStatsMessage.Add("Nice to see you on our Server again, %playerName%");
            this.m_lstPlayerWelcomeStatsMessage.Add("Serverstats for %playerName%:");
            this.m_lstPlayerWelcomeStatsMessage.Add("Score: %playerScore%  %playerKills% Kills %playerHeadshots% HS  %playerDeaths% Deaths K/D: %playerKDR%");
            this.m_lstPlayerWelcomeStatsMessage.Add("Your Serverrank is: %playerRank% of %allRanks%");

            //Welcomestats new Player
            this.m_lstNewPlayerWelcomeMsg = new List<string>();
            this.m_lstNewPlayerWelcomeMsg.Add("Welcome to the %serverName% Server, %playerName%");

            //Weaponstats
            this.m_lstWeaponstatsMsg = new List<string>();
            this.m_lstWeaponstatsMsg.Add("%playerName%'s Stats for %Weapon%:");
            this.m_lstWeaponstatsMsg.Add("%playerKills% Kills  %playerHeadshots% Headshots  Headshotrate: %playerKHR%%");
            this.m_lstWeaponstatsMsg.Add("Your Weaponrank is: %playerRank% of %allRanks%");

            //Serverstats
            this.m_lstServerstatsMsg = new List<string>();
            this.m_lstServerstatsMsg.Add("Serverstatistics for server %serverName%");
            this.m_lstServerstatsMsg.Add("Unique Players: %countPlayer%  Totalplaytime: %sumPlaytime%");
            this.m_lstServerstatsMsg.Add("Totalscore: %sumScore% Avg. Score: %avgScore% Avg. SPM: %avgSPM%");
            this.m_lstServerstatsMsg.Add("Totalkills: %sumKills% Avg. Kills: %avgKills% Avg. KPM: %avgKPM%");

            //Session
            this.m_lstSessionMessage = new List<string>();
            this.m_lstSessionMessage.Add("%playerName%'s Session Data  Session started %SessionStarted%");
            this.m_lstSessionMessage.Add("Score: %playerScore%  %playerKills% Kills  %playerHeadshots% HS  %playerDeaths% Deaths K/D: %playerKDR%");
            this.m_lstSessionMessage.Add("Your Rank: %playerRank% (%RankDif%)  Sessionlength: %SessionDuration% Minutes");

            //Top10 Headers
            this.m_strTop10Header = "Top 10 Player of the %serverName% Server";
            this.m_strTop10RowFormat = "%Rank%. %playerName%  Score: %playerScore%  %playerKills% Kills  %playerHeadshots% Headshots  %playerDeaths% Deaths  KDR: %playerKDR%";
            this.m_strWeaponTop10Header = "Top 10 Player with %Weapon% of the %serverName%";
            this.m_strWeaponTop10RowFormat = "%Rank%.  %playerName%  %playerKills% Kills  %playerHeadshots% Headshots  %playerDeaths% Deaths Headshotrate: %playerKHR%%";

            //Top10 for Period
            this.m_strTop10HeaderForPeriod = "Top 10 Player of the %serverName% Server over the last %intervaldays% days";

            //Awards
            this.m_lstAwardTable = new List<string>();
            this.m_lstAwardTable.Add("First");
            this.m_lstAwardTable.Add("Second");
            this.m_lstAwardTable.Add("Third");
            this.m_lstAwardTable.Add("Purple_Heart");
            this.m_lstAwardTable.Add("Best_Combat");
            this.m_lstAwardTable.Add("Killstreak_5");
            this.m_lstAwardTable.Add("Killstreak_10");
            this.m_lstAwardTable.Add("Killstreak_15");
            this.m_lstAwardTable.Add("Killstreak_20");

            //ServerID
            this.ServerID = 0;

            //Tableconfig Tweaks for friendly weapon names
            this.m_lstTableconfig = new List<string>();
			this.m_lstTableconfig.Add("870MCS{870,870MCS}");
			this.m_lstTableconfig.Add("AEK-971{AEK,AEK971,AEK-971}");
			this.m_lstTableconfig.Add("AKS-74u{AKSU,AKS-74,AKSU-74,AKS-74U}");
			this.m_lstTableconfig.Add("AN-94 Abakan{ABAKAN,AN94,AN-94}");
			this.m_lstTableconfig.Add("AS Val{ASVAL,AS-VAL,AS VAL}");
			this.m_lstTableconfig.Add("DAO-12{DAO12,DAO,DAO-12}");
			this.m_lstTableconfig.Add("death{DEATH}");
			this.m_lstTableconfig.Add("Defib{DEFIBRILLATOR,DEFIB,PADDLE,PADDLES}");
			this.m_lstTableconfig.Add("F2000{F2000}");
			this.m_lstTableconfig.Add("FAMAS{FAMAS}");
			this.m_lstTableconfig.Add("FGM-148{JAVELIN,FGM148,FGM-148}");
			this.m_lstTableconfig.Add("FIM92{STINGER,FIM92,FIM-92}");
			this.m_lstTableconfig.Add("Glock18{GLOCK,GLOCK18,GLOCK-18}");
			this.m_lstTableconfig.Add("HK53{HK53,HK-53,G53,G-53,HK-G53}");
			this.m_lstTableconfig.Add("jackhammer{JACKHAMMER,MK3A1,MK3}");
			this.m_lstTableconfig.Add("JNG90{JNG-90,JNG90,JNG}");
			this.m_lstTableconfig.Add("L96{L-96,L96}");
			this.m_lstTableconfig.Add("LSAT{LSAT}");
			this.m_lstTableconfig.Add("M416{M-416,M416}");
			this.m_lstTableconfig.Add("M417{M-417,M417}");
			this.m_lstTableconfig.Add("M1014{M-1014,1014,M1014}");
			this.m_lstTableconfig.Add("M15 AT Mine{M15,M15 MINE,AT MINE,ATMINE,ATM,M15-ATM}");
			this.m_lstTableconfig.Add("M16A4{M-16,M16,M16A3,M16-A3,M16A4,M16-A4}");
			this.m_lstTableconfig.Add("M1911{1911,M1911}");
			this.m_lstTableconfig.Add("M240{M-240,M240}");
			this.m_lstTableconfig.Add("M249{M-249,M249,SAW}");
			this.m_lstTableconfig.Add("M26Mass{M26,M-26,MASS,M26MASS}");
			this.m_lstTableconfig.Add("M27IAR{M27,M-27,M27IAR}");
			this.m_lstTableconfig.Add("M320{M-320,GRENADE LAUNCHER,M320}");
			this.m_lstTableconfig.Add("M39{M-39,M39}");
			this.m_lstTableconfig.Add("M40A5{M40,M-40,M40A5}");
			this.m_lstTableconfig.Add("M4A1{M4,M-4,M4A1}");
			this.m_lstTableconfig.Add("M60{M-60,M60}");
			this.m_lstTableconfig.Add("M67{HANDGRENADE,GRENADE,M67,M-67}");
			this.m_lstTableconfig.Add("M9{M-9,M9}");
			this.m_lstTableconfig.Add("M93R{M93,M93R}");
			this.m_lstTableconfig.Add("Medkit{MEDKIT}");
			this.m_lstTableconfig.Add("MG36{MG-36,MG36}");
			this.m_lstTableconfig.Add("Mk11{MK-11,MK11}");
			this.m_lstTableconfig.Add("Model98B{M98,M98B,MODEL98,MODEL-98,MODEL98B,MODEL-98B}");
			this.m_lstTableconfig.Add("MP7{MP-7,MP7}");
			this.m_lstTableconfig.Add("Pecheneg{PKP-PECHENEG,PKP,PECHENEG}");
			this.m_lstTableconfig.Add("PP-19{PP19,PP-19}");
			this.m_lstTableconfig.Add("PP-2000{PP2000,PP-2000}");
			this.m_lstTableconfig.Add("QBB-95{QBB,QBB95,QBB-95}");
			this.m_lstTableconfig.Add("QBU-88{QBU,QBU88,QBU-88}");
			this.m_lstTableconfig.Add("QBZ-95{QBZ,QBZ95,QBZ-95}");
			this.m_lstTableconfig.Add("Repair Tool{REPAIRTOOL,TOOL,TORCH,BLOWTORCH}");
			this.m_lstTableconfig.Add("RoadKill{ROADKILL}");
			this.m_lstTableconfig.Add("RPG-7{RPG,RPG7,RPG7V2,RPG-7V2}");
			this.m_lstTableconfig.Add("RPK-74M{RPK,RPK74,RPK-74,RPK74M,RPK-74M}");
            this.m_lstTableconfig.Add("Weapons/SCAR-H/SCAR-H{SCAR,SCAR-H,SCARH}");
			this.m_lstTableconfig.Add("SCAR-L{SCARL,SCAR-L}");
			this.m_lstTableconfig.Add("SG 553 LB{SG553,SG-553,SG-553LB}");
			this.m_lstTableconfig.Add("Siaga20k{SAIGA,SAIGA20K,SIAGA,SIAGA20K}");
			this.m_lstTableconfig.Add("SKS{SKS}");
			this.m_lstTableconfig.Add("SMAW{SMAW}");
			this.m_lstTableconfig.Add("SPAS-12{SPAS12,SPAS,SPAS-12}");
			this.m_lstTableconfig.Add("Suicide{SUICIDE}");
			this.m_lstTableconfig.Add("SV98{SV-98,SV98}");
			this.m_lstTableconfig.Add("SVD{SVD,DRAGUNOV}");
			this.m_lstTableconfig.Add("Steyr AUG{STEYR,AUGA3,AUG-A3,AUG}");
			this.m_lstTableconfig.Add("Taurus .44{TAURUS,.44MAGNUM,TAURUS.44,MAGNUM,.44}");
			this.m_lstTableconfig.Add("Type88{TYPE88,TYPE-88}");
            this.m_lstTableconfig.Add("USAS-12{USAS12,USAS}");
			this.m_lstTableconfig.Add("Weapons/A91/A91{A91,A-91}");
			this.m_lstTableconfig.Add("Weapons/AK74M/AK74{AK74,AK-74,AKM,AK-74M,AK74M}");
			this.m_lstTableconfig.Add("Weapons/G36C/G36C{G36,G36C,G-36,G-36C}");
			this.m_lstTableconfig.Add("Weapons/G3A3/G3A3{G3,G-3,G3A3,G3-A3}");
			this.m_lstTableconfig.Add("Weapons/Gadgets/C4/C4{C4,C-4}");
			this.m_lstTableconfig.Add("Weapons/Gadgets/Claymore/Claymore{CLAYMORE,LANDMINE,APMINE,AP-MINE,APM,M18,M-18,M18-CLAYMORE}");
			this.m_lstTableconfig.Add("Weapons/KH2002/KH2002{KH2002,KH-2002}");
			this.m_lstTableconfig.Add("Weapons/Knife/Knife{KNIFE,MELEE}");
			this.m_lstTableconfig.Add("Weapons/MagpulPDR/MagpulPDR{PDW-R,PDWR,PDR,PDW}");
			this.m_lstTableconfig.Add("Weapons/MP412Rex/MP412REX{MP412REX,REX,MP-412,MP412}");
			this.m_lstTableconfig.Add("Weapons/MP443/MP443{MP-443,MP443,GRACH}");
			this.m_lstTableconfig.Add("Weapons/P90/P90{P-90,P90}");
			this.m_lstTableconfig.Add("Weapons/Sa18IGLA/Sa18IGLA{SA18,SA-18,IGLA,SA18IGLA,SA18-IGLA,SA-18IGLA}");
			this.m_lstTableconfig.Add("Weapons/UMP45/UMP45{UMP45,UMP-45,UMP}");
			this.m_lstTableconfig.Add("Weapons/XP1_L85A2/L85A2{L85,L85A2,L-85,L-85A2,L85-A2}");
			this.m_lstTableconfig.Add("Weapons/XP2_ACR/ACR{ACWR,ACW-R,ACR,AC-R}");
			this.m_lstTableconfig.Add("Weapons/XP2_L86/L86{L86,L86A2,L-86,L-86A2,L86-A2}");
			this.m_lstTableconfig.Add("Weapons/XP2_MP5K/MP5K{MP5,MP5K,M5K,MP-5,MP-5K,M5-K}");
			this.m_lstTableconfig.Add("Weapons/XP2_MTAR/MTAR{MTAR,MTAR21,MTAR-21}");

            //ServerInfo Event fix
            this.dtLastServerInfoEvent = DateTime.Now;
            this.minIntervalllenght = 60;

            //Double Roundendfix
            this.dtLastRoundendEvent = DateTime.MinValue;
            this.dtLastOnListPlayersEvent = DateTime.MinValue;

            //Top10 for Period
            this.m_intDaysForPeriodTop10 = 7;

            //New In-Game Command System
            this.m_IngameCommands_stats = "stats,rank";
            this.m_IngameCommands_serverstats = "serverstats";
            this.m_IngameCommands_session = "session";
            this.m_IngameCommands_dogtags = "dogtags";
            this.m_IngameCommands_top10 = "top10";
            this.m_IngameCommands_playerOfTheDay = "playeroftheday,potd";
            this.m_IngameCommands_top10ForPeriod = "weektop10,wtop10";
        }
        #endregion

        #region PluginSetup
        public string GetPluginName()
        {
            return "PRoCon Chat, GUID, Stats and Map Logger";
        }

        public string GetPluginVersion()
        {
            return "1.0.0.3";
        }

        public string GetPluginAuthor()
        {
            return "[GWC]XpKiller";
        }

        public string GetPluginWebsite()
        {
            return "www.german-wildcards.de";
        }

        public string GetPluginDescription()
        {
            return @"
If you like my Plugins, please feel free to donate<br>
<p><form action='https://www.paypal.com/cgi-bin/webscr' target='_blank' method='post'>
<input type='hidden' name='cmd' value='_s-xclick'>
<input type='hidden' name='hosted_button_id' value='3B2FEDDHHWUW8'>
<input type='image' src='https://www.paypal.com/en_US/i/btn/btn_donate_SM.gif' border='0' name='submit' alt='PayPal - The safer, easier way to pay online!'>
<img alt='' border='0' src='https://www.paypal.com/de_DE/i/scr/pixel.gif' width='1' height='1'>
</form></p>

   
<h2>Description</h2>
    <p>This plugin is used to log player chat, player GUID's, player Stats, Weaponstats and Mapstats.</p>
    <p>This inludes: Chat, PBGUID, EAGUID, IP, Stats, Weaponstats, Dogtags, Killstreaks, Country, ClanTag, ... to be continued.. ;-)</p>
    
<h2>Requirements</h2>
	<p>It requires the use of a MySQL database with INNODB engine, that allows remote connections.(MYSQL Version 5.1.x, 5.5.x or higher is recommendend!!!)</p>
	<p>Also you should give INNODB some more Ram because the plugin mainly uses this engine if you need help feel free to ask me</p>
	<p>The Plugin will create the tables by itself.</p>
	<p>Pls Give FEEDBACK !!!</p>

<h2>Installation</h2>
<p>Download and install this plugin</p>
<p>Setup your Database, this means create a database and the user for it. I highly recommend NOT to use your root user. Just create a user with all rights for your newly created database </p>
<p>I recommend MySQL 5.1.x, 5.5.x or greater (5.0.x could work too, not tested) Important: <b>Your database need INNODB Support</b></p>
<p>Start Procon</p>
<p>Go to Tools --> Options --> Plugins --> Enter you databaseserver under outgoing Connections and allow all outgoing connections</p>
<p>Restart Procon</p>
<p>Enter your settings into Plugin Settings and THEN enable the plugin</p>
<p>Now the plugin should work if not request help in the <a href='https://forum.myrcon.com' target='_blank'>Forum</a></p>

	
<h2>Things you have to know:</h2>
You can add additional Names for weapons in the Pluginsettings 
Use comma to seperate the words. <br>
Example: M16A4{M16} --> 40MMGL{M16,M16A3}  <br><br>



<h2>Ingame Commands (defaults!)</h2>
	<blockquote><h4>[@,#,!]stats</h4>Tells the Player their own Serverstats</blockquote>
	<blockquote><h4>[@,#,!]rank</h4>Tells the Player their own Serverstats</blockquote>
	<blockquote><h4>[@,#,!]potd</h4>Show up the player of the day</blockquote>
	<blockquote><h4>[@,#,!]playeroftheday</h4>Show up the player of the day</blockquote>
	<blockquote><h4>[@,#,!]session</h4>Tells the Player their own Sessiondata</blockquote>
	<blockquote><h4>[@,#,!]top10</h4>Tells the Player the Top10 players of the server</blockquote>
	<blockquote><h4>[@,#,!]wtop10</h4>Tells the Player the Top10 players of the server for specific period</blockquote>
	<blockquote><h4>[@,#,!]weektop10</h4>Tells the Player the Top10 players of the server for specific period</blockquote>
	<blockquote><h4>[@,#,!]stats WeaponName</h4>Tells the Player their own Weaponstats for the specific Weapon</blockquote>
	<blockquote><h4>[@,#,!]rank WeaponName</h4>Privately Tells the Player their own Weaponstats for the specific Weapon</blockquote>
	<blockquote><h4>[@,#,!]top10 WeaponName</h4>Privately Tells the Player the Top10 Player for the specific Weapon of the server</blockquote>
	<blockquote><h4>[@,#,!]dogtags WeaponName</h4>Privately Tells the Player his Dogtagstats </blockquote>
	<blockquote><h4>[@,#,!]serverstats</h4>Tells the Player the Serverstats</blockquote>

<h2>Replacement Strings for Playerstats and Player of the day</h2>
	
	<table border ='1'>
	<tr><th>Replacement String</th><th>Effect</th></tr>
	<tr><td>%playerName%</td><td>Will be replaced by the player's name</td></tr>
	<tr><td>%playerScore%</td><td>Will be replaced by the player's totalscore on this server</td></tr>
	<tr><td>%SPM%</td><td>Will be replaced by the Player's score per minute on this server</td></tr>
	<tr><td>%playerKills%</td><td>Will be replaced by the player's totalkills on this server</td></tr>
	<tr><td>%playerHeadshots%</td><td>Will be replaced by the player's totalheadshots on this server</td></tr>
	<tr><td>%playerDeaths%</td><td>Will be replaced by the player's totaldeaths on this server</td></tr>
	<tr><td>%playerKDR%</td><td>Will be replaced by the player's kill death ratio on this server</td></tr>
	<tr><td>%playerSucide%</td><td>Will be replaced by the player's sucides on this server</td></tr>
	<tr><td>%playerPlaytime%</td><td>Will be replaced by the player's totalplaytime on this server in hh:mm:ss format</td></tr>
	<tr><td>%rounds%</td><td>Will be replaced by the player's totalrounds played on this server</td></tr>
	<tr><td>%playerRank%</td><td>Will be replaced by the player's concurrent serverrank</td></tr>
	<tr><td>%allRanks%</td><td>Will be replaced by the player's concurrent serverrank</td></tr>
	<tr><td>%killstreak%</td><td>Will be replaced by the player's best Killstreak</td></tr>
	<tr><td>%deathstreak%</td><td>Will be replaced by the player's worst Deathstreak</td></tr>
	</table>
	<br>

<h2>Replacement Strings for Top10</h2>
	
	<table border ='1'>
	<tr><th>Replacement String</th><th>Effect</th></tr>
    <tr><td>%serverName%</td><td>Will be replaced by the Server name (Header only)</td></tr>
    <tr><td>%Rank%</td><td>Will be replaced by the player's rank</td></tr>
	<tr><td>%playerName%</td><td>Will be replaced by the player's name</td></tr>
	<tr><td>%playerScore%</td><td>Will be replaced by the player's totalscore on this server</td></tr>
	<tr><td>%playerKills%</td><td>Will be replaced by the player's totalkills on this server</td></tr>
	<tr><td>%playerHeadshots%</td><td>Will be replaced by the player's totalheadshots on this server</td></tr>
	<tr><td>%playerDeaths%</td><td>Will be replaced by the player's totaldeaths on this server</td></tr>
	<tr><td>%playerKDR%</td><td>Will be replaced by the player's kill death ratio on this server</td></tr>
    <tr><td>%playerKHR%</td><td>Will be replaced by the player's Headshot Kill ratio on this server</td></tr>
    <tr><td>%intervaldays%</td><td>Will be replaced interval of days (top10 for a period only)</td></tr>
	</table>
	<br>
	
	<h2>Replacement Strings for Weaponstats</h2>
	
	<table border ='1'>
	<tr><th>Replacement String</th><th>Effect</th></tr>
	<tr><td>%playerName%</td><td>Will be replaced by the player's name</td></tr>
	<tr><td>%playerKills%</td><td>Will be replaced by the player's Totalkills on this server with the specific Weapon</td></tr>
	<tr><td>%playerHeadshots%</td><td>Will be replaced by the player's Totalheadshotkills on this server the specific Weapon</td></tr>
	<tr><td>%playerDeaths%</td><td>Will be replaced by the player's totaldeaths on this server caused by this specific Weapon</td></tr>
	<tr><td>%playerKHR%</td><td>Will be replaced by the player's Headshotkill ratio on this server with the specific Weapon</td></tr>
	<tr><td>%playerKDR%</td><td>Will be replaced by the player's kill death ratio on this server with the specific Weapon</td></tr>
	<tr><td>%playerRank%</td><td>Will be replaced by the player's current Serverrank for the specific Weapon</td></tr>
	<tr><td>%allRanks%</td><td>Will be replaced by current Number of Player in Serverrank for the specific Weapon</td></tr>
	<tr><td>%killstreak%</td><td>Will be replaced by the player's best Killstreak</td></tr>
	<tr><td>%deathstreak%</td><td>Will be replaced by the player's worst Deathstreak</td></tr>
	</table>

    <h2>Replacement Strings for serverstats</h2>
	
	<table border ='1'>
	<tr><th>Replacement String</th><th>Effect</th></tr>
	<tr><td>%countPlayer%</td><td>Will be replaced by the number of unique players on this server</td></tr>
    <tr><td>%sumScore%</td><td>Will be replaced by the Totalscore of all players combined on this server</td></tr>
    <tr><td>%sumKills%</td><td>Will be replaced by the Totalkills of all players combined on this server</td></tr>
    <tr><td>%sumHeadshots%</td><td>Will be replaced by the TotalHeadshots of all players combined on this server</td></tr>
    <tr><td>%sumDeaths%</td><td>Will be replaced by the Totaldeaths of all players combined on this server</td></tr>
    <tr><td>%sumTKs%</td><td>Will be replaced by the TotalTeamkills of all players combined on this server</td></tr>
    <tr><td>%sumRounds%</td><td>Will be replaced by the Totalrounds of all players combined on this server</td></tr>
    <tr><td>%sumSuicide%</td><td>Will be replaced by the Totalsuicide of all players combined on this server</td></tr>
    <tr><td>%avgScore%</td><td>Will be replaced by the average score of all players combined on this server</td></tr>
    <tr><td>%avgKills%</td><td>Will be replaced by the average kills of all players combined on this server</td></tr>
    <tr><td>%avgHeadshots%</td><td>Will be replaced by the average Headshots of all players combined on this server</td></tr>
    <tr><td>%avgDeaths%</td><td>Will be replaced by the average deaths of all players combined on this server</td></tr>
    <tr><td>%avgTKs%</td><td>Will be replaced by the average teamkills of all players combined on this server</td></tr>
    <tr><td>%avgSuicide</td><td>Will be replaced by the average suicides of all players combined on this server</td></tr>
    <tr><td>%avgRounds%</td><td>Will be replaced by the average rounds of all players combined on this server</td></tr>
    <tr><td>%avgSPM%</td><td>Will be replaced by the average Score per Minute of all players combined on this server</td></tr>
    <tr><td>%avgKPM%</td><td>Will be replaced by the average Kills per Minute of all players combined on this server</td></tr>
    <tr><td>%avgHPM%</td><td>Will be replaced by the average Headshots per Minute of all players combined on this server</td></tr>
    <tr><td>%avgHPK%</td><td>Will be replaced by the average Headshots per Kills (unit procent (%)) of all players combined on this server</td></tr>
    <tr><td>%sumPlaytime%</td><td>Will be replaced by the Total Playtime (format: dd:hh:mm:ss) of all players combined on this server</td></tr>
    <tr><td>%avgPlaytime%</td><td>Will be replaced by the average Playtime (format: dd:hh:mm:ss) of all players combined on this server</td></tr>
    <tr><td>%sumPlaytimeHours%</td><td>Will be replaced by the Total Playtime (format rounded hours) of all players combined on this server</td></tr>
    <tr><td>%avgPlaytimeHours%</td><td>Will be replaced by the average Playtime (format rounded hours) of all players combined on this server</td></tr>
    <tr><td>%sumPlaytimeDays%</td><td>Will be replaced by the Total Playtime (format rounded days) of all players combined on this server</td></tr>  
    <tr><td>%avgPlaytimeDays%</td><td>Will be replaced by the average Playtime (format rounded days) of all players combined on this server</td></tr>  
	</table>
	
	<h2>Replacement Strings for PlayerSession</h2>
	
	<table border ='1'>
	<tr><th>Replacement String</th><th>Effect</th></tr>
	<tr><td>%playerName%</td><td>Will be replaced by the player's name</td></tr>
	<tr><td>%playerScore%</td><td>Will be replaced by the player's totalscore of the concurrent Session</td></tr>
	<tr><td>%playerKills%</td><td>Will be replaced by the player's totalkills of the concurrent Session</td></tr>
	<tr><td>%playerHeadshots%</td><td>Will be replaced by the player's totalheadshots of the concurrent Session</td></tr>
	<tr><td>%playerDeaths%</td><td>Will be replaced by the player's totaldeaths of the concurrent Session</td></tr>
	<tr><td>%playerKDR%</td><td>Will be replaced by the player's kill death ratio of the concurrent Session</td></tr>
	<tr><td>%playerSucide%</td><td>Will be replaced by the player's sucides of the concurrent Session</td></tr>
	<tr><td>%SessionDuration%</td><td>Will be replaced by the player's totalplaytime of the concurrent Session in Minutes</td></tr>
	<tr><td>%playerRank%</td><td>Will be replaced by the player's concurrent serverrank</td></tr>
	<tr><td>%RankDif%</td><td>Will be replaced by the player's rank change</td></tr>
	<tr><td>%SessionStarted%</td><td>Will be replaced by the player's start of the Session</td></tr>
	<tr><td>%killstreak%</td><td>Will be replaced by the player's best Killstreak of the Session</td></tr>
	<tr><td>%deathstreak%</td><td>Will be replaced by the player's worst Deathstreak of the Session</td></tr>
	</table>
	<br>
	
    <h2>How to yell/say messages</h2>
    <p>Every ingame messages can be yelled to the Player.</p>
    <p>Just add the yelltag in front of every line of you message which should be yelled.</p>
    <p>Usage:[yell,duration in seconds]Your messages</p>
    <p>Like:[yell,3]Welcome on our server!</p>
    <p>This would be yell for 3 seconds</p>
    <p>Hint: You can mixed normal say and yell without any problems.</p>
    <p>Messages without Tag will will be transmitted with the say command.</p>
<br>
    


	<h3>NOTE:</h3>
		<p>Tracked stats are: Kills, Headshots, Deaths, All Weapons, TKs, Suicides, Score, Playtime, Rounds, MapStats, Dogtags </p>
		<p>The Rank is created dynamical from Query in  my opinion much better than write it back to database.</p>
		<p>The Stats are written to the Database at the end of the round</p>
	
<h3>Known issues:</h3>
<p>Vehicles cannot be tracked due limitations in the Rcon Protocol blame EA/Dice for it</p>

		
<h3>Changelog:</h3><br>
<b>1.0.0.2</b><br>
Bugfixes for column errors.<br>
Bugfixes for the sessions streaming bug<br>
Weaponstats working again. <br>
Bugfix for Identifier name is too long.<br>

<br>
<b>1.0.0.1</b><br>
Bugfixes for value too long for column errors.<br>
Bugfixes for some other bugs<br>
Changed deprecated Tracemessages<br>
Added an error prefix in pluginlog <br>
New feature: Tickets/teamscores are now tracked in tbl_teamscores<br>
New feature: Simple Stats (collects playerdata only)<br>
New feature: Switch for disabling weaponstats
<br><br>
<b>1.0.0.0</b><br>
First Release<br>
Multigame Support<br>
<br><br>


";
        }

        public void OnPluginLoadingEnv(List<string> lstPluginEnv)
        {  
            this.strServerGameType = lstPluginEnv[1].ToUpper();
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
            this.RegisterEvents(this.GetType().Name, "OnListPlayers", "OnPlayerAuthenticated", "OnPlayerJoin", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnPunkbusterMessage", "OnPunkbusterPlayerInfo", "OnServerInfo", "OnLevelLoaded",
                                                     "OnPlayerKilled", "OnPlayerLeft", "OnRoundOverPlayers", "OnPlayerSpawned", "OnLoadingLevel", "OnCommandStats", "OnCommandTop10", "OnCommandDogtags", "OnCommandServerStats",
                                                     "OnRoundStartPlayerCount", "OnRoundRestartPlayerCount", "OnRoundOver");

            // Register the logger status match command
            // This command can be called for status whether logger is enabled or not
            this.RegisterCommand(loggerStatusCommand);
        }

        public void OnPluginEnable()
        {
            isStreaming = true;
            this.serverName = String.Empty;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPRoCon Chat, GUID and Stats Logger ^2Enabled");
            this.Spamprotection = new CSpamprotection(numberOfAllowedRequests);
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPRoCon Chat, GUID and Stats Logger: ^2 Floodprotection set to " + this.numberOfAllowedRequests.ToString() + " Request per Round for each Player");
            // Register Commands
            this.m_isPluginEnabled = true;
            this.prepareTablenames();
            this.setGameMod();
            this.MyDateTime = new myDateTime_W(this.m_dTimeOffset);
            //Webrequest
            if (this.m_enWebrequest == enumBoolYesNo.Yes)
            {
                //this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger", "30", (this.m_requestIntervall * 60).ToString(), "-1", "procon.protected.plugins.call", "CChatGUIDStatsLogger", "Threadstarter_Webrequest");
            }
            this.RegisterAllCommands();
            this.generateWeaponList();
            //Start intial tablebuilder thread
            ThreadPool.QueueUserWorkItem(delegate { this.tablebuilder(); });
        }

        public void OnPluginDisable()
        {
            isStreaming = false;
            if (MySqlCon != null)
                if (MySqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        MySqlCon.Close();
                    }
                    catch{}
                }
            if (MySqlConn != null)
                if (MySqlConn.State == ConnectionState.Open)
                {
                    try
                    {
                        MySqlConn.Close();
                    }
                    catch { }
                }

            //Destroying all current Coonection Pool if availble:
            try
            {
                MySqlConnection.ClearAllPools();
            }
            catch { }

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPRoCon Chat, GUID and Stats Logger ^1Disabled");

            //Unregister Commands
            this.m_isPluginEnabled = false;
            //Webrequest
            this.ExecuteCommand("procon.protected.tasks.remove", "CChatGUIDStatsLogger");
            this.UnregisterAllCommands();
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            lstReturn.Add(new CPluginVariable("Server Details|Host", this.m_strHost.GetType(), this.m_strHost));
            lstReturn.Add(new CPluginVariable("Server Details|Port", this.m_strDBPort.GetType(), this.m_strDBPort));
            lstReturn.Add(new CPluginVariable("Server Details|Database Name", this.m_strDatabase.GetType(), this.m_strDatabase));
            lstReturn.Add(new CPluginVariable("Server Details|UserName", this.m_strUserName.GetType(), this.m_strUserName));
            lstReturn.Add(new CPluginVariable("Server Details|Password", this.m_strPassword.GetType(), this.m_strPassword));
            if (this.m_connectionPooling == enumBoolOnOff.Off)
            {
                lstReturn.Add(new CPluginVariable("Server Details|High performance mode(no connection limit!)", typeof(enumBoolOnOff), this.m_highPerformanceConnectionMode));
            }
            lstReturn.Add(new CPluginVariable("Server Details|Connection Pooling", typeof(enumBoolOnOff), this.m_connectionPooling));
            if (this.m_connectionPooling == enumBoolOnOff.On)
            {
                lstReturn.Add(new CPluginVariable("Server Details|Min Connection Pool Size", this.m_minPoolSize.GetType(), this.m_minPoolSize));
                lstReturn.Add(new CPluginVariable("Server Details|Max Connection Pool Size", this.m_maxPoolSize.GetType(), this.m_maxPoolSize));
            }
            lstReturn.Add(new CPluginVariable("Server Details|Failed Transaction retry attempts", this.TransactionRetryCount.GetType(), this.TransactionRetryCount));
            lstReturn.Add(new CPluginVariable("Server Details|Minimum time(sec) between ServerInfo Updates", this.minIntervalllenght.GetType(), this.minIntervalllenght));

            lstReturn.Add(new CPluginVariable("Chatlogging|Enable Chatlogging?", typeof(enumBoolYesNo), this.m_enChatloggingON));
            if (this.m_enChatloggingON == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Chatlogging|Log ServerSPAM?", typeof(enumBoolYesNo), this.m_enNoServerMsg));
                lstReturn.Add(new CPluginVariable("Chatlogging|Instant Logging of Chat Messages?", typeof(enumBoolYesNo), this.m_enInstantChatlogging));
                lstReturn.Add(new CPluginVariable("Chatlogging|Enable chatlog filter(Regex)?", typeof(enumBoolYesNo), this.m_enChatlogFilter));
                if (this.m_enChatlogFilter == enumBoolYesNo.Yes)
                {
                    this.lstStrChatFilterRules = new List<string>(this.ListReplace(this.lstStrChatFilterRules,"&#124","|"));
                    this.lstStrChatFilterRules = this.ListReplace(this.lstStrChatFilterRules, "&#43", "+");
                    lstReturn.Add(new CPluginVariable("Chatlogging|Chatfilterrules(Regex)", typeof(string[]), this.lstStrChatFilterRules.ToArray()));
                }
            }
            lstReturn.Add(new CPluginVariable("Stats|Enable Statslogging?", typeof(enumBoolYesNo), this.m_enLogSTATS));
            if (this.m_enLogSTATS == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Stats|Enable Weaponstats?", typeof(enumBoolYesNo), this.m_weaponstatsON));
                lstReturn.Add(new CPluginVariable("Stats|Ranking by Score?", typeof(enumBoolYesNo), this.m_enRankingByScore));
                lstReturn.Add(new CPluginVariable("Stats|Enable ingame commands?", typeof(enumBoolYesNo), this.m_enableInGameCommands));
                lstReturn.Add(new CPluginVariable("Stats|Overall ranking(merged Serverranking)", typeof(enumBoolYesNo), this.m_enOverallRanking));
                lstReturn.Add(new CPluginVariable("Stats|Server group (0 - 128)", this.intServerGroup.GetType(), this.intServerGroup));
                lstReturn.Add(new CPluginVariable("Stats|Send Stats to all Players?", typeof(enumBoolYesNo), this.m_enSendStatsToAll));
                lstReturn.Add(new CPluginVariable("Stats|Enable KDR correction?", typeof(enumBoolYesNo), this.m_kdrCorrection));
                lstReturn.Add(new CPluginVariable("Stats|PlayerMessage", typeof(string[]), this.m_lstPlayerStatsMessage.ToArray()));
                //Player of the day
                lstReturn.Add(new CPluginVariable("Stats|Player of the day Message", typeof(string[]), this.m_lstPlayerOfTheDayMessage.ToArray()));
                lstReturn.Add(new CPluginVariable("Stats|Weaponstats Message ", typeof(string[]), this.m_lstWeaponstatsMsg.ToArray()));
                //Serverstats
                lstReturn.Add(new CPluginVariable("Stats|Serverstats Message", typeof(string[]), this.m_lstServerstatsMsg.ToArray()));
                lstReturn.Add(new CPluginVariable("Stats|Enable Livescoreboard in DB?", typeof(enumBoolYesNo), this.m_enableCurrentPlayerstatsTable));
                //Simplestats
                lstReturn.Add(new CPluginVariable("Stats|Log playerdata only (no playerstats)?", typeof(enumBoolYesNo), this.m_enLogPlayerDataOnly));
                //lstReturn.Add(new CPluginVariable("Stats|Awards ON?", typeof(enumBoolYesNo), this.m_awardsON));
                lstReturn.Add(new CPluginVariable("WelcomeStats|Enable Welcomestats?", typeof(enumBoolYesNo), this.m_enWelcomeStats));
                if (this.m_enWelcomeStats == enumBoolYesNo.Yes)
                {
                    //lstReturn.Add(new CPluginVariable("WelcomeStats|Yell Welcome Message(not the stats)?", typeof(enumBoolYesNo), this.m_enYellWelcomeMSG));
                    //lstReturn.Add(new CPluginVariable("WelcomeStats|Welcome Message", this.m_strPlayerWelcomeMsg.GetType(), this.m_strPlayerWelcomeMsg));
                    lstReturn.Add(new CPluginVariable("WelcomeStats|Welcome Message", typeof(string[]), this.m_lstPlayerWelcomeStatsMessage.ToArray()));
                    lstReturn.Add(new CPluginVariable("WelcomeStats|Welcome Message for new Player", typeof(string[]), this.m_lstNewPlayerWelcomeMsg.ToArray()));
                    lstReturn.Add(new CPluginVariable("WelcomeStats|Welcomestats Delay", this.int_welcomeStatsDelay.GetType(), this.int_welcomeStatsDelay));
                }
                //top10
                lstReturn.Add(new CPluginVariable("Stats|Top10 ingame", this.m_enTop10ingame.GetType(), this.m_enTop10ingame));
                if (this.m_enTop10ingame == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Stats|Top10 header line", this.m_strTop10Header.GetType(), this.m_strTop10Header));
                    lstReturn.Add(new CPluginVariable("Stats|Top10 row format", this.m_strTop10RowFormat.GetType(), this.m_strTop10RowFormat));
                    //top10 for period
                    lstReturn.Add(new CPluginVariable("Stats|Top10 for period header line", this.m_strTop10HeaderForPeriod.GetType(), this.m_strTop10HeaderForPeriod));
                    lstReturn.Add(new CPluginVariable("Stats|Top10 for period interval days", this.m_intDaysForPeriodTop10.GetType(), this.m_intDaysForPeriodTop10));
                    //Weapontop10
                    lstReturn.Add(new CPluginVariable("Stats|WeaponTop10 header line", this.m_strWeaponTop10Header.GetType(), this.m_strWeaponTop10Header));
                    lstReturn.Add(new CPluginVariable("Stats|WeaponTop10 row format", this.m_strWeaponTop10RowFormat.GetType(), this.m_strWeaponTop10RowFormat));
                }
            }
            lstReturn.Add(new CPluginVariable("Debug|DebugLevel", "enum.Actions(Trace|Info|Warning|Error)", this.GlobalDebugMode));
            lstReturn.Add(new CPluginVariable("Table|Keywordlist", typeof(string[]), this.m_lstTableconfig.ToArray()));
            lstReturn.Add(new CPluginVariable("Table|tableSuffix", this.tableSuffix.GetType(), this.tableSuffix));
            lstReturn.Add(new CPluginVariable("MapStats|MapStats ON?", typeof(enumBoolYesNo), this.m_mapstatsON));
            lstReturn.Add(new CPluginVariable("Session|Session ON?", typeof(enumBoolYesNo), this.m_sessionON));
            lstReturn.Add(new CPluginVariable("Session|SessionMessage", typeof(string[]), this.m_lstSessionMessage.ToArray()));
            lstReturn.Add(new CPluginVariable("Session|Save Sessiondata to DB?", typeof(enumBoolYesNo), this.m_enSessionTracking));
            lstReturn.Add(new CPluginVariable("FloodProtection|Playerrequests per Round", this.numberOfAllowedRequests.GetType(), this.numberOfAllowedRequests));
            lstReturn.Add(new CPluginVariable("TimeOffset|Servertime Offset", this.m_dTimeOffset.GetType(), this.m_dTimeOffset));
            //Ingame Command Setup
            /*
            this.m_IngameCommands_stats = "stats,rank";
            this.m_IngameCommands_serverstats = "serverstats";
            this.m_IngameCommands_session = "session";
            this.m_IngameCommands_dogtags = "dogtags";
            this.m_IngameCommands_top10 = "top10";
            this.m_IngameCommands_playerOfTheDay = "playeroftheday,potd";
            this.m_IngameCommands_top10ForPeriod = "weektop10,wtop10";
             */
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Stats Command:", this.m_IngameCommands_stats.GetType(), this.m_IngameCommands_stats));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|ServerStats Command:", this.m_IngameCommands_serverstats.GetType(), this.m_IngameCommands_serverstats));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Session Command:", this.m_IngameCommands_session.GetType(), this.m_IngameCommands_session));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Dogtags Command:", this.m_IngameCommands_dogtags.GetType(), this.m_IngameCommands_dogtags));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Top10 Command:", this.m_IngameCommands_top10.GetType(), this.m_IngameCommands_top10));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Player Of The Day Command:", this.m_IngameCommands_playerOfTheDay.GetType(), this.m_IngameCommands_playerOfTheDay));
            lstReturn.Add(new CPluginVariable("Ingame Command Setup|Top10 for period Command:", this.m_IngameCommands_top10ForPeriod.GetType(), this.m_IngameCommands_top10ForPeriod));


            //lstReturn.Add(new CPluginVariable("BFBCS|Fetch Stats from BFBCS", typeof(enumBoolYesNo), this.m_getStatsfromBFBCS));

            if (this.m_getStatsfromBFBCS == enumBoolYesNo.Yes)
            {
                //lstReturn.Add(new CPluginVariable("BFBCS|Updateinterval (hours)", this.BFBCS_UpdateInterval.GetType(), this.BFBCS_UpdateInterval));
                //lstReturn.Add(new CPluginVariable("BFBCS|Request Packrate", this.BFBCS_Min_Request.GetType(), this.BFBCS_Min_Request));
                //lstReturn.Add(new CPluginVariable("Cheaterprotection|Statsbased Protection", typeof(enumBoolYesNo), this.m_cheaterProtection));
                //lstReturn.Add(new CPluginVariable("Ranklimiter|Ranklimiter ON?", typeof(enumBoolYesNo), this.m_enRanklimiter));
            }
            /*
            lstReturn.Add(new CPluginVariable("Webrequest|Periodical Webrequest On?(P&S Stats)", typeof(enumBoolYesNo), this.m_enWebrequest));
            if (this.m_enWebrequest == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Webrequest|Webaddress", this.m_webAddress.GetType(), this.m_webAddress));
                lstReturn.Add(new CPluginVariable("Webrequest|Webrequest Intervall", this.m_requestIntervall.GetType(), this.m_requestIntervall));
            }
            */
            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Host", this.m_strHost.GetType(), this.m_strHost));
            lstReturn.Add(new CPluginVariable("Port", this.m_strDBPort.GetType(), this.m_strDBPort));
            lstReturn.Add(new CPluginVariable("Database Name", this.m_strDatabase.GetType(), this.m_strDatabase));
            lstReturn.Add(new CPluginVariable("UserName", this.m_strUserName.GetType(), this.m_strUserName));
            lstReturn.Add(new CPluginVariable("Password", this.m_strPassword.GetType(), this.m_strPassword));
            if (this.m_connectionPooling == enumBoolOnOff.Off)
            {
                lstReturn.Add(new CPluginVariable("High performance mode(no connection limit!)", typeof(enumBoolOnOff), this.m_highPerformanceConnectionMode));
            }
            lstReturn.Add(new CPluginVariable("Connection Pooling", typeof(enumBoolOnOff), this.m_connectionPooling));
            if (this.m_connectionPooling == enumBoolOnOff.On)
            {
                lstReturn.Add(new CPluginVariable("Min Connection Pool Size", this.m_minPoolSize.GetType(), this.m_minPoolSize));
                lstReturn.Add(new CPluginVariable("Max Connection Pool Size", this.m_maxPoolSize.GetType(), this.m_maxPoolSize));
            }
            lstReturn.Add(new CPluginVariable("Failed Transaction retry attempts", this.TransactionRetryCount.GetType(), this.TransactionRetryCount));
            lstReturn.Add(new CPluginVariable("Minimum time(sec) between ServerInfo Updates", this.minIntervalllenght.GetType(), this.minIntervalllenght));
            // Switch for Stats Logging
            lstReturn.Add(new CPluginVariable("Enable Chatlogging?", typeof(enumBoolYesNo), this.m_enChatloggingON));
            if (this.m_enChatloggingON == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Log ServerSPAM?", typeof(enumBoolYesNo), this.m_enNoServerMsg));
                lstReturn.Add(new CPluginVariable("Instant Logging of Chat Messages?", typeof(enumBoolYesNo), this.m_enInstantChatlogging));
                lstReturn.Add(new CPluginVariable("Enable chatlog filter(Regex)?", typeof(enumBoolYesNo), this.m_enChatlogFilter));

                if (this.m_enChatlogFilter == enumBoolYesNo.Yes)
                {
                    this.lstStrChatFilterRules = new List<string>(this.ListReplace(this.lstStrChatFilterRules, "|", "&#124"));
                    this.lstStrChatFilterRules = this.ListReplace(this.lstStrChatFilterRules, "&#43", "+");
                    lstReturn.Add(new CPluginVariable("Chatfilterrules(Regex)", typeof(string[]), this.lstStrChatFilterRules.ToArray()));
                }
            }
            lstReturn.Add(new CPluginVariable("Enable Statslogging?", typeof(enumBoolYesNo), this.m_enLogSTATS));
            lstReturn.Add(new CPluginVariable("Enable Weaponstats?", typeof(enumBoolYesNo), this.m_weaponstatsON));
            //lstReturn.Add(new CPluginVariable("Update EA GUID?", typeof(enumBoolYesNo), this.m_UpdateEA_GUID));
            //lstReturn.Add(new CPluginVariable("Update PB-GUID (NOT recommended!!!)?", typeof(enumBoolYesNo), this.m_UpdatePB_GUID));
            lstReturn.Add(new CPluginVariable("Ranking by Score?", typeof(enumBoolYesNo), this.m_enRankingByScore));
            lstReturn.Add(new CPluginVariable("Enable ingame commands?", typeof(enumBoolYesNo), this.m_enableInGameCommands));
            lstReturn.Add(new CPluginVariable("Overall ranking(merged Serverranking)", typeof(enumBoolYesNo), this.m_enOverallRanking));
            lstReturn.Add(new CPluginVariable("Server group (0 - 128)", this.intServerGroup.GetType(), this.intServerGroup));
            lstReturn.Add(new CPluginVariable("Send Stats to all Players?", typeof(enumBoolYesNo), this.m_enSendStatsToAll));
            lstReturn.Add(new CPluginVariable("Enable Livescoreboard in DB?", typeof(enumBoolYesNo), this.m_enableCurrentPlayerstatsTable));
            lstReturn.Add(new CPluginVariable("Enable KDR correction?", typeof(enumBoolYesNo), this.m_kdrCorrection));
            lstReturn.Add(new CPluginVariable("PlayerMessage", typeof(string[]), this.m_lstPlayerStatsMessage.ToArray()));
            //Player of the day
            lstReturn.Add(new CPluginVariable("Player of the day Message", typeof(string[]), this.m_lstPlayerOfTheDayMessage.ToArray()));
            lstReturn.Add(new CPluginVariable("Weaponstats Message ", typeof(string[]), this.m_lstWeaponstatsMsg.ToArray()));
            //Serverstats
            lstReturn.Add(new CPluginVariable("Serverstats Message", typeof(string[]), this.m_lstServerstatsMsg.ToArray()));

            lstReturn.Add(new CPluginVariable("Awards ON?", typeof(enumBoolYesNo), this.m_awardsON));
            lstReturn.Add(new CPluginVariable("Enable Welcomestats?", typeof(enumBoolYesNo), this.m_enWelcomeStats));
            lstReturn.Add(new CPluginVariable("Yell Welcome Message(not the stats)?", typeof(enumBoolYesNo), this.m_enYellWelcomeMSG));
            lstReturn.Add(new CPluginVariable("Welcome Message", typeof(string[]), this.m_lstPlayerWelcomeStatsMessage.ToArray()));
            lstReturn.Add(new CPluginVariable("Welcome Message for new Player", typeof(string[]), this.m_lstNewPlayerWelcomeMsg.ToArray()));
            lstReturn.Add(new CPluginVariable("Welcomestats Delay", this.int_welcomeStatsDelay.GetType(), this.int_welcomeStatsDelay));
            lstReturn.Add(new CPluginVariable("Top10 ingame", this.m_enTop10ingame.GetType(), this.m_enTop10ingame));
            lstReturn.Add(new CPluginVariable("Top10 header line", this.m_strTop10Header.GetType(), this.m_strTop10Header));
            lstReturn.Add(new CPluginVariable("Top10 row format", this.m_strTop10RowFormat.GetType(), this.m_strTop10RowFormat));
            //top10 for period
            lstReturn.Add(new CPluginVariable("Top10 for period header line", this.m_strTop10HeaderForPeriod.GetType(), this.m_strTop10HeaderForPeriod));
            lstReturn.Add(new CPluginVariable("Top10 for period interval days", this.m_intDaysForPeriodTop10.GetType(), this.m_intDaysForPeriodTop10));
            //Weapontop10
            lstReturn.Add(new CPluginVariable("WeaponTop10 header line", this.m_strWeaponTop10Header.GetType(), this.m_strWeaponTop10Header));
            lstReturn.Add(new CPluginVariable("WeaponTop10 row format", this.m_strWeaponTop10RowFormat.GetType(), this.m_strWeaponTop10RowFormat));
            //
            lstReturn.Add(new CPluginVariable("DebugLevel", "enum.Actions(Trace|Info|Warning|Error)", this.GlobalDebugMode));
            lstReturn.Add(new CPluginVariable("Keywordlist", typeof(string[]), this.m_lstTableconfig.ToArray()));
            lstReturn.Add(new CPluginVariable("tableSuffix", this.tableSuffix.GetType(), this.tableSuffix));
            lstReturn.Add(new CPluginVariable("MapStats ON?", typeof(enumBoolYesNo), this.m_mapstatsON));
            lstReturn.Add(new CPluginVariable("Session ON?", typeof(enumBoolYesNo), this.m_sessionON));
            lstReturn.Add(new CPluginVariable("SessionMessage", typeof(string[]), this.m_lstSessionMessage.ToArray()));
            lstReturn.Add(new CPluginVariable("Save Sessiondata to DB?", typeof(enumBoolYesNo), this.m_enSessionTracking));
            lstReturn.Add(new CPluginVariable("Playerrequests per Round", this.numberOfAllowedRequests.GetType(), this.numberOfAllowedRequests));
            lstReturn.Add(new CPluginVariable("Servertime Offset", this.m_dTimeOffset.GetType(), this.m_dTimeOffset));
            
            //Ingame Command Setup
            /*
            this.m_IngameCommands_stats = "stats,rank";
            this.m_IngameCommands_serverstats = "serverstats";
            this.m_IngameCommands_session = "session";
            this.m_IngameCommands_dogtags = "dogtags";
            this.m_IngameCommands_top10 = "top10";
            this.m_IngameCommands_playerOfTheDay = "playeroftheday,potd";
            this.m_IngameCommands_top10ForPeriod = "weektop10,wtop10";
             */
            lstReturn.Add(new CPluginVariable("Stats Command:", this.m_IngameCommands_stats.GetType(), this.m_IngameCommands_stats));
            lstReturn.Add(new CPluginVariable("ServerStats Command:", this.m_IngameCommands_serverstats.GetType(), this.m_IngameCommands_serverstats));
            lstReturn.Add(new CPluginVariable("Session Command:", this.m_IngameCommands_session.GetType(), this.m_IngameCommands_session));
            lstReturn.Add(new CPluginVariable("Dogtags Command:", this.m_IngameCommands_dogtags.GetType(), this.m_IngameCommands_dogtags));
            lstReturn.Add(new CPluginVariable("Top10 Command:", this.m_IngameCommands_top10.GetType(), this.m_IngameCommands_top10));
            lstReturn.Add(new CPluginVariable("Player Of The Day Command:", this.m_IngameCommands_playerOfTheDay.GetType(), this.m_IngameCommands_playerOfTheDay));
            lstReturn.Add(new CPluginVariable("Top10 for period Command:", this.m_IngameCommands_top10ForPeriod.GetType(), this.m_IngameCommands_top10ForPeriod));

            lstReturn.Add(new CPluginVariable("Periodical Webrequest On?(P&S Stats)", typeof(enumBoolYesNo), this.m_enWebrequest));
            if (this.m_enWebrequest == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Webaddress", this.m_webAddress.GetType(), this.m_webAddress));
                lstReturn.Add(new CPluginVariable("Webrequest Intervall", this.m_requestIntervall.GetType(), this.m_requestIntervall));
            }
            //Simple Stats
            lstReturn.Add(new CPluginVariable("Log playerdata only (no playerstats)?", typeof(enumBoolYesNo), this.m_enLogPlayerDataOnly));
            return lstReturn;
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            if (strVariable.CompareTo("Host") == 0)
            {
                this.m_strHost = strValue;
            }
            else if (strVariable.CompareTo("Port") == 0)
            {
                this.m_strDBPort = strValue;
            }
            else if (strVariable.CompareTo("Database Name") == 0)
            {
                this.m_strDatabase = strValue;
            }
            else if (strVariable.CompareTo("UserName") == 0)
            {
                this.m_strUserName = strValue;
            }
            else if (strVariable.CompareTo("Password") == 0)
            {
                this.m_strPassword = strValue;
            }
            else if (strVariable.CompareTo("High performance mode(no connection limit!)") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true)
            {
                this.m_highPerformanceConnectionMode = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);
            }
            else if (strVariable.CompareTo("Connection Pooling") == 0 && Enum.IsDefined(typeof(enumBoolOnOff), strValue) == true)
            {
                this.m_connectionPooling = (enumBoolOnOff)Enum.Parse(typeof(enumBoolOnOff), strValue);
                this.m_highPerformanceConnectionMode = enumBoolOnOff.On;
            }
            else if (strVariable.CompareTo("Min Connection Pool Size") == 0)
            {
                Int32.TryParse(strValue, out this.m_minPoolSize);
                if (this.m_minPoolSize < 0 || this.m_minPoolSize > this.m_maxPoolSize)
                {
                    this.m_minPoolSize = 0;
                }
            }
            else if (strVariable.CompareTo("Max Connection Pool Size") == 0)
            {
                Int32.TryParse(strValue, out this.m_maxPoolSize);
                if (this.m_maxPoolSize < 1 || this.m_minPoolSize > this.m_maxPoolSize)
                {
                    this.m_maxPoolSize = 10;
                }
            }
            else if (strVariable.CompareTo("Failed Transaction retry attempts") == 0)
            {
                Int32.TryParse(strValue, out TransactionRetryCount);
                if (TransactionRetryCount < 1)
                {
                    TransactionRetryCount = 3;
                }
            }
            else if (strVariable.CompareTo("Minimum time(sec) between ServerInfo Updates") == 0)
            {
                if (Int32.TryParse(strValue, out this.minIntervalllenght))
                {
                    if (this.minIntervalllenght < 1)
                    {
                        this.minIntervalllenght = 30;
                    }
                }
                else
                {
                    this.minIntervalllenght = 30;
                }
            }
            else if (strVariable.CompareTo("Enable Chatlogging?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enChatloggingON = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Log ServerSPAM?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enNoServerMsg = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Instant Logging of Chat Messages?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enInstantChatlogging = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

            else if (strVariable.CompareTo("Enable chatlog filter(Regex)?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enChatlogFilter = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Chatfilterrules(Regex)") == 0)
            {
                this.lstStrChatFilterRules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
                this.BuildRegexRuleset();
            }
            else if (strVariable.CompareTo("Enable Statslogging?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enLogSTATS = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            //Log playerdata only (no playerstats)?
            else if (strVariable.CompareTo("Log playerdata only (no playerstats)?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enLogPlayerDataOnly = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Weaponstats?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_weaponstatsON = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Ranking by Score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enRankingByScore = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable ingame commands?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enableInGameCommands = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Overall ranking(merged Serverranking)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enOverallRanking = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Server group (0 - 128)") == 0)
            {
                if (Int32.TryParse(strValue, out this.intServerGroup))
                {
                    if (this.intServerGroup > 128 || this.intServerGroup < 0)
                    {
                        this.intServerGroup = 0;
                    }
                }
                else
                {
                    this.intServerGroup = 0;
                }
            }
            else if (strVariable.CompareTo("Send Stats to all Players?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enSendStatsToAll = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Livescoreboard in DB?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enableCurrentPlayerstatsTable = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable KDR correction?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_kdrCorrection = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("PlayerMessage") == 0)
            {
                this.m_lstPlayerStatsMessage = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            // player of the day
            else if (strVariable.CompareTo("Player of the day Message") == 0)
            {
                this.m_lstPlayerOfTheDayMessage = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Weaponstats Message ") == 0)
            {
                this.m_lstWeaponstatsMsg = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            //Serverstats
            else if (strVariable.CompareTo("Serverstats Message") == 0)
            {
                this.m_lstServerstatsMsg = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Enable Welcomestats?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enWelcomeStats = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Awards ON?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_awardsON = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell Welcome Message(not the stats)?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enYellWelcomeMSG = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Welcome Message") == 0)
            {
                //this.m_strPlayerWelcomeMsg = strValue;
                this.m_lstPlayerWelcomeStatsMessage = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Welcome Message for new Player") == 0)
            {
                this.m_lstNewPlayerWelcomeMsg = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Welcomestats Delay") == 0 && Int32.TryParse(strValue, out int_welcomeStatsDelay) == true)
            {
                this.int_welcomeStatsDelay = Convert.ToInt32(strValue);
            }
            else if (strVariable.CompareTo("Top10 ingame") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enTop10ingame = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            //top10
            else if (strVariable.CompareTo("Top10 header line") == 0)
            {
                this.m_strTop10Header = strValue;
            }
            else if (strVariable.CompareTo("Top10 row format") == 0)
            {
                this.m_strTop10RowFormat = strValue;
            }
            // top10 for period
            else if (strVariable.CompareTo("Top10 for period header line") == 0)
            {
                this.m_strTop10HeaderForPeriod = strValue;
            }
            else if (strVariable.CompareTo("Top10 for period interval days") == 0)
            {
                if (Int32.TryParse(strValue, out this.m_intDaysForPeriodTop10) == false)
                {
                    this.m_intDaysForPeriodTop10 = 7;
                }
            }
            else if (strVariable.CompareTo("WeaponTop10 header line") == 0)
            {
                this.m_strWeaponTop10Header = strValue;
            }
            else if (strVariable.CompareTo("WeaponTop10 row format") == 0)
            {
                this.m_strWeaponTop10RowFormat = strValue;
            }
            else if (strVariable.CompareTo("DebugLevel") == 0)
            {
                this.GlobalDebugMode = strValue;
            }
            else if (strVariable.CompareTo("Keywordlist") == 0)
            {
                this.m_lstTableconfig = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("tableSuffix") == 0)
            {
                this.tableSuffix = strValue;
                this.prepareTablenames();
                this.setGameMod();
                this.boolTableEXISTS = false;
            }
            else if (strVariable.CompareTo("MapStats ON?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_mapstatsON = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Session ON?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_sessionON = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("SessionMessage") == 0)
            {
                this.m_lstSessionMessage = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Save Sessiondata to DB?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enSessionTracking = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Playerrequests per Round") == 0 && Int32.TryParse(strValue, out numberOfAllowedRequests) == true)
            {
                this.numberOfAllowedRequests = Convert.ToInt32(strValue);
            }
            else if (strVariable.CompareTo("Servertime Offset") == 0 && Double.TryParse(strValue, out m_dTimeOffset) == true)
            {
                this.m_dTimeOffset = Convert.ToDouble(strValue);
                this.MyDateTime = new myDateTime_W(this.m_dTimeOffset);
            }
           
            //Webrequest
            /*
        else if (strVariable.CompareTo("Periodical Webrequest On?(P&S Stats)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
        {
            this.m_enWebrequest = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
        }
        else if (strVariable.CompareTo("Webaddress") == 0)
        {
            this.m_webAddress = strValue;
        }
        else if (strVariable.CompareTo("Webrequest Intervall") == 0 && Int32.TryParse(strValue, out this.m_requestIntervall) == true)
        {
            this.m_requestIntervall = Convert.ToInt32(strValue);
        }
             */
            else if (strVariable.CompareTo("Stats Command:") == 0)
            {
                this.m_IngameCommands_stats = strValue;
            }
            else if (strVariable.CompareTo("ServerStats Command:") == 0)
            {
                this.m_IngameCommands_serverstats = strValue;
            }
            else if (strVariable.CompareTo("Session Command:") == 0)
            {
                this.m_IngameCommands_session = strValue;
            }
            else if (strVariable.CompareTo("Dogtags Command:") == 0)
            {
                this.m_IngameCommands_dogtags = strValue;
            }
            else if (strVariable.CompareTo("Top10 Command:") == 0)
            {
                this.m_IngameCommands_top10 = strValue;
            }
            else if (strVariable.CompareTo("Player Of The Day Command:") == 0)
            {
                this.m_IngameCommands_playerOfTheDay = strValue;
            }
            else if (strVariable.CompareTo("Top10 for period Command:") == 0)
            {
                this.m_IngameCommands_top10ForPeriod = strValue;
            }
            this.RegisterAllCommands();
        }

        private List<string> GetExcludedCommandStrings(string strAccountName)
        {
            List<string> lstReturnCommandStrings = new List<string>();
            List<MatchCommand> lstCommands = this.GetRegisteredCommands();
            CPrivileges privileges = this.GetAccountPrivileges(strAccountName);
            foreach (MatchCommand mtcCommand in lstCommands)
            {
                if (mtcCommand.Requirements.HasValidPermissions(privileges) == true && lstReturnCommandStrings.Contains(mtcCommand.Command) == false)
                {
                    lstReturnCommandStrings.Add(mtcCommand.Command);
                }
            }
            return lstReturnCommandStrings;
        }

        private List<string> GetCommandStrings()
        {
            List<string> lstReturnCommandStrings = new List<string>();
            List<MatchCommand> lstCommands = this.GetRegisteredCommands();
            foreach (MatchCommand mtcCommand in lstCommands)
            {
                if (lstReturnCommandStrings.Contains(mtcCommand.Command) == false)
                {
                    lstReturnCommandStrings.Add(mtcCommand.Command);
                }
            }
            return lstReturnCommandStrings;
        }

        private void UnregisterAllCommands()
        {
            this.setupIngameCommandDic();
            try
            {
                foreach (KeyValuePair<string, CStatsIngameCommands> kvp in this.dicIngameCommands)
                {
                    if (kvp.Value.commands != string.Empty)
                    {
                        foreach (string command in kvp.Value.commands.Split(','))
                        {
                            this.UnregisterCommand(new MatchCommand("CChatGUIDStatsLogger", kvp.Value.functioncall.ToString(), this.Listify<string>("@", "!", "#"), command.ToString(), this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), kvp.Value.description.ToString()));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.DebugInfo("Error", "Error in UnregisterAllCommands: " + e);
            }
        }

        private void SetupHelpCommands()
        {

        }

        private void RegisterAllCommands()
        {
            lock (this.registerallcomandslock)
            {
                this.setupIngameCommandDic();
                if (this.m_isPluginEnabled == true)
                {
                    if (this.m_enableInGameCommands == enumBoolYesNo.No)
                    {
                        this.UnregisterAllCommands();
                        return;
                    }
                    this.SetupHelpCommands();

                    try
                    {
                        foreach (KeyValuePair<string, CStatsIngameCommands> kvp in this.dicIngameCommands)
                        {
                            if (kvp.Value.commands != string.Empty)
                            {
                                foreach (string command in kvp.Value.commands.Split(','))
                                {
                                    if (kvp.Value.boolEnabled)
                                    {
                                        this.RegisterCommand(new MatchCommand("CChatGUIDStatsLogger", kvp.Value.functioncall, this.Listify<string>("@", "!", "#"), command, this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), kvp.Value.description));
                                    }
                                    else
                                    {
                                        this.UnregisterCommand(new MatchCommand("CChatGUIDStatsLogger", kvp.Value.functioncall, this.Listify<string>("@", "!", "#"), command, this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), kvp.Value.description));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.DebugInfo("Error", "Error in RegisterAllCommands: " + e);
                    }
                }
            }
        }

        private void setupIngameCommandDic()
        {
            lock (this.dicIngameCommands)
            {
                bool boolenable = false;
                this.dicIngameCommands.Clear();
                if (this.m_enLogSTATS == enumBoolYesNo.Yes && this.m_enableInGameCommands == enumBoolYesNo.Yes)
                {
                    boolenable = true;
                }
                this.dicIngameCommands.Add("playerstats", new CStatsIngameCommands(this.m_IngameCommands_stats, "OnCommandStats", boolenable, "Provides a player his personal serverstats"));
                this.dicIngameCommands.Add("serverstats", new CStatsIngameCommands(this.m_IngameCommands_serverstats, "OnCommandServerStats", boolenable, "Provides a player his personal serverstats"));
                this.dicIngameCommands.Add("dogtagstats", new CStatsIngameCommands(this.m_IngameCommands_dogtags, "OnCommandDogtags", boolenable, "Provides a player his personal dogtagstats"));
                this.dicIngameCommands.Add("session", new CStatsIngameCommands(this.m_IngameCommands_session, "OnCommandSession", boolenable, "Provides a player his personal sessiondata"));
                this.dicIngameCommands.Add("playeroftheday", new CStatsIngameCommands(this.m_IngameCommands_playerOfTheDay, "OnCommandPlayerOfTheDay", boolenable, "Provides the player of the day stats"));

                if (this.m_enLogSTATS == enumBoolYesNo.Yes && this.m_enTop10ingame == enumBoolYesNo.Yes && this.m_enableInGameCommands == enumBoolYesNo.Yes)
                {
                    this.dicIngameCommands.Add("top10", new CStatsIngameCommands(this.m_IngameCommands_top10, "OnCommandTop10", true, "Provides a player top10 Players"));
                    this.dicIngameCommands.Add("top10forperiode", new CStatsIngameCommands(this.m_IngameCommands_top10ForPeriod, "OnCommandTop10ForPeriod", true, "Provides a player top10 Players for a specific timeframe"));
                }
                else
                {
                    this.dicIngameCommands.Add("top10", new CStatsIngameCommands(this.m_IngameCommands_top10, "OnCommandTop10", false, "Provides a player top10 Players"));
                    this.dicIngameCommands.Add("top10forperiode", new CStatsIngameCommands(this.m_IngameCommands_top10ForPeriod, "OnCommandTop10ForPeriod", false, "Provides a player top10 Players for a specific timeframe"));
                }
            }
        }
        #endregion

        #region IPRoConPluginInterface
        /*=======ProCon Events========*/
        // Player events
        public override void OnPlayerJoin(string strSoldierName)
        {
            if (this.StatsTracker.ContainsKey(strSoldierName) == false)
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(strSoldierName, newEntry);
            }
            ThreadPool.QueueUserWorkItem(delegate { this.CreateSession(strSoldierName, 0, String.Empty); });

            if (bool_roundStarted == true && StatsTracker.ContainsKey(strSoldierName) == true)
            {
                if (StatsTracker[strSoldierName].PlayerOnServer == false)
                {
                    if (this.StatsTracker[strSoldierName].TimePlayerjoined == null)
                    {
                        this.StatsTracker[strSoldierName].TimePlayerjoined = MyDateTime.Now;
                    }
                    this.StatsTracker[strSoldierName].Playerjoined = MyDateTime.Now;
                    this.StatsTracker[strSoldierName].PlayerOnServer = true;
                }
            }
            //Mapstatscounter for Player who joined the server
            this.Mapstats.IntplayerjoinedServer++;

            if (this.m_enWelcomeStats == enumBoolYesNo.Yes)
            {
                if (this.welcomestatsDic.ContainsKey(strSoldierName))
                {
                    //Update jointime
                    this.welcomestatsDic[strSoldierName] = MyDateTime.Now;
                }
                else
                {
                    //Insert
                    this.DebugInfo("Trace", "Added Player: " + strSoldierName + " to welcomestatslist");
                    this.welcomestatsDic.Add(strSoldierName, MyDateTime.Now);
                }
            }
        }

        public override void OnPlayerAuthenticated(string soldierName, string guid)
        {
            if (this.StatsTracker.ContainsKey(soldierName) == false)
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(soldierName, newEntry);
                if (guid.Length > 0)
                {
                    StatsTracker[soldierName].EAGuid = guid;
                }
            }
        }

        // Will receive ALL chat global/team/squad in R3.
        public override void OnGlobalChat(string strSpeaker, string strMessage)
        {
            if (strMessage.Length > 0)
            {
                ThreadPool.QueueUserWorkItem(delegate { this.LogChat(strSpeaker, strMessage, "Global"); });
            }
        }

        // Place holder, non-functioning in R3.  It recieves the same data as OnGlobalChat though so look out for now.
        public override void OnTeamChat(string strSpeaker, string strMessage, int iTeamID)
        {
            if (strMessage.Length > 0)
            {
                ThreadPool.QueueUserWorkItem( delegate { this.LogChat(strSpeaker, strMessage, "Team"); });
            }
        }

        // Place holder, non-functioning in R3.  It recieves the same data as OnGlobalChat though so look out for now.
        public override void OnSquadChat(string strSpeaker, string strMessage, int iTeamID, int iSquadID)
        {
            if (strMessage.Length > 0)
            {
                ThreadPool.QueueUserWorkItem(delegate { this.LogChat(strSpeaker, strMessage, "Squad"); });
            }
        }

        public override void OnPunkbusterMessage(string strPunkbusterMessage)
        {
            try
            {
                // This piece of code gets the number of player out of Punkbustermessages
                string playercount = String.Empty;
                if (strPunkbusterMessage.Contains("End of Player List"))
                {
                    playercount = strPunkbusterMessage.Remove(0, 1 + strPunkbusterMessage.LastIndexOf("("));
                    playercount = playercount.Replace(" ", "");
                    playercount = playercount.Remove(playercount.LastIndexOf("P"), playercount.LastIndexOf(")"));
                    //this.DebugInfo("EoPl: "+playercount);
                    int players = Convert.ToInt32(playercount);
                    if (players >= intRoundStartCount && bool_roundStarted == false)
                    {
                        bool_roundStarted = true;
                        Time_RankingStarted = MyDateTime.Now;
                        //Mapstats Roundstarted
                        this.Mapstats.MapStarted();
                    }
                    else if (players >= intRoundStartCount && this.Mapstats.TimeMapStarted == DateTime.MinValue)
                    {
                        this.Mapstats.MapStarted();
                    }
                    //MapStats Playercount
                    this.Mapstats.ListADD(players);
                }
            }
            catch (Exception c)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error in OnPunkbusterMessage: " + c);
            }
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer)
        {
            this.RegisterAllCommands();
            if (this.m_enLogSTATS == enumBoolYesNo.Yes)
            {
                try
                {
                    this.AddPBInfoToStats(cpbiPlayer);
                    if (this.StatsTracker.ContainsKey(cpbiPlayer.SoldierName))
                    {
                        if (this.StatsTracker[cpbiPlayer.SoldierName].TimePlayerjoined == null)
                        {
                            this.StatsTracker[cpbiPlayer.SoldierName].TimePlayerjoined = MyDateTime.Now;
                        }
                        this.StatsTracker[cpbiPlayer.SoldierName].IP = cpbiPlayer.Ip;
                    }
                }
                catch (Exception c)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error in OnPunkbusterPlayerInfo: " + c);
                }
            }
        }

        // Query Events
        public override void OnServerInfo(CServerInfo csiServerInfo)
        {
            this.serverName = csiServerInfo.ServerName;
            this.Mapstats.StrGamemode = csiServerInfo.GameMode;
            this.Mapstats.ListADD(csiServerInfo.PlayerCount);
            //Mapstats
            if (csiServerInfo.PlayerCount >= intRoundStartCount && this.Mapstats.TimeMapStarted == DateTime.MinValue)
            {
                this.Mapstats.MapStarted();
            }
            this.Mapstats.StrMapname = csiServerInfo.Map;
            this.Mapstats.IntRound = csiServerInfo.CurrentRound;
            this.Mapstats.IntNumberOfRounds = csiServerInfo.TotalRounds;
            this.Mapstats.IntServerplayermax = csiServerInfo.MaxPlayerCount;

            if(this.ServerID == 0 || this.minIntervalllenght <= (DateTime.Now.Subtract(this.dtLastServerInfoEvent).TotalSeconds))
            {
                this.dtLastServerInfoEvent = DateTime.Now;
                try
                {
                    ThreadPool.QueueUserWorkItem(delegate { this.getUpdateServerID(csiServerInfo); });
                }
                catch { };
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            //List<CPlayerInfo> PlayerList = new List<CPlayerInfo>();
            //Mapstats Add Playercount to list
            this.Mapstats.ListADD(lstPlayers.Count);
            if (bool_roundStarted == false)
            {
                if (lstPlayers.Count >= intRoundStartCount)
                {
                    bool_roundStarted = true;
                    Time_RankingStarted = MyDateTime.Now;
                    this.DebugInfo("Trace", "OLP: roundstarted");
                    //Mapstats Roundstarted
                    this.Mapstats.MapStarted();
                }
            }
            if (lstPlayers.Count >= intRoundStartCount && this.Mapstats.TimeMapStarted == DateTime.MinValue)
            {
                this.Mapstats.MapStarted();
            }
            try
            {
                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    if (this.m_dicPlayers.ContainsKey(cpiPlayer.SoldierName) == true)
                    {
                        this.m_dicPlayers[cpiPlayer.SoldierName] = cpiPlayer;
                    }
                    else
                    {
                        this.m_dicPlayers.Add(cpiPlayer.SoldierName, cpiPlayer);
                    }
                    //Timelogging
                    if (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true)
                    {
                        if (this.StatsTracker[cpiPlayer.SoldierName].PlayerOnServer == false)
                        {
                            this.StatsTracker[cpiPlayer.SoldierName].Playerjoined = MyDateTime.Now;
                            this.StatsTracker[cpiPlayer.SoldierName].PlayerOnServer = true;
                        }
                        //EA-GUID, ClanTag, usw.
                        if (cpiPlayer.GUID.Length > 3)
                        {
                            this.StatsTracker[cpiPlayer.SoldierName].EAGuid = cpiPlayer.GUID;
                            //ID - Cache
                            if (this.m_ID_cache.ContainsKey(cpiPlayer.GUID))
                            {
                                this.m_ID_cache[cpiPlayer.GUID].PlayeronServer = true;
                            }
                        }
                        this.StatsTracker[cpiPlayer.SoldierName].ClanTag = cpiPlayer.ClanTag;
                        //TeamId
                        this.StatsTracker[cpiPlayer.SoldierName].TeamId = cpiPlayer.TeamID;
                        if (cpiPlayer.Score != 0)
                        {
                            this.StatsTracker[cpiPlayer.SoldierName].Score = cpiPlayer.Score;
                        }
                        //GlobalRank
                        if (cpiPlayer.Rank >= 0)
                        {
                            this.StatsTracker[cpiPlayer.SoldierName].GlobalRank = cpiPlayer.Rank;
                        }

                        //KDR Correction
                        if (this.m_kdrCorrection == enumBoolYesNo.Yes && ((cpiPlayer.Deaths == 0 && cpiPlayer.Kills == 0 && cpiPlayer.Score == 0) == false))
                        {
                            if (this.StatsTracker[cpiPlayer.SoldierName].Deaths > (cpiPlayer.Deaths + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftDeaths))
                            {
                                this.DebugInfo("Trace", "OnListPlayers Player: " + cpiPlayer.SoldierName + " has " + this.StatsTracker[cpiPlayer.SoldierName].Deaths + " deaths; correcting to " + cpiPlayer.Deaths + " deaths now");
                                this.StatsTracker[cpiPlayer.SoldierName].Deaths = (cpiPlayer.Deaths + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftDeaths);
                            }
                            if (this.StatsTracker[cpiPlayer.SoldierName].Kills > (cpiPlayer.Kills + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftKills))
                            {
                                this.StatsTracker[cpiPlayer.SoldierName].Kills = (cpiPlayer.Kills + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftKills);
                            }
                        }

                    }
                    //Session Score
                    if (this.m_sessionON == enumBoolYesNo.Yes)
                    {
                        lock (this.sessionlock)
                        {
                            if (this.m_dicSession.ContainsKey(cpiPlayer.SoldierName))
                            {
                                this.m_dicSession[cpiPlayer.SoldierName].AddScore(cpiPlayer.Score);
                                //KDR Correction
                                if (this.m_kdrCorrection == enumBoolYesNo.Yes && ((cpiPlayer.Deaths == 0 && cpiPlayer.Kills == 0 && cpiPlayer.Score == 0) == false))
                                {
                                    if (this.m_dicSession[cpiPlayer.SoldierName].Deaths > (cpiPlayer.Deaths + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftDeaths))
                                    {
                                        this.DebugInfo("Trace", "Player: " + cpiPlayer.SoldierName + " has " + this.m_dicSession[cpiPlayer.SoldierName].Deaths + " deaths; correcting to " + cpiPlayer.Deaths + " deaths now");
                                        this.m_dicSession[cpiPlayer.SoldierName].Deaths = (cpiPlayer.Deaths + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftDeaths);
                                    }
                                    if (this.m_dicSession[cpiPlayer.SoldierName].Kills > (cpiPlayer.Kills + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftKills))
                                    {
                                        this.m_dicSession[cpiPlayer.SoldierName].Kills = (cpiPlayer.Kills + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftKills);
                                    }
                                }
                                if (cpiPlayer.GUID.Length > 2)
                                {
                                    this.m_dicSession[cpiPlayer.SoldierName].EAGuid = cpiPlayer.GUID;
                                }
                                //TeamId
                                this.m_dicSession[cpiPlayer.SoldierName].TeamId = cpiPlayer.TeamID;
                            }
                            else
                            {
                                ThreadPool.QueueUserWorkItem(delegate { this.CreateSession(cpiPlayer.SoldierName, cpiPlayer.Score, cpiPlayer.GUID); });
                            }
                        }
                    }
                    //Checking the sessiondic
                    //ThreadPool.QueueUserWorkItem(delegate { this.CheckSessionDic(lstPlayers); });
                    //this.CreateSession(cpiPlayer.SoldierName, cpiPlayer.Score); 
                }

                if (this.m_enableCurrentPlayerstatsTable == enumBoolYesNo.Yes && this.ServerID > 0 && this.minIntervalllenght <= (DateTime.Now.Subtract(this.dtLastOnListPlayersEvent).TotalSeconds))
                {
                    ThreadPool.QueueUserWorkItem(delegate { this.UpdateCurrentPlayerTable(lstPlayers); });
                    this.dtLastOnListPlayersEvent = DateTime.Now;
                }
            }
            catch (Exception c)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error in OnListPlayers: " + c);
            }
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            bool_roundStarted = true;
            if (bool_roundStarted == true)
            {
                this.playerKilled(kKillerVictimDetails);
            }
        }

        public override void OnPlayerLeft(CPlayerInfo cpiPlayer)
        {
            this.playerLeftServer(cpiPlayer);
            this.RegisterAllCommands();
        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> lstPlayers)
        {
            this.DebugInfo("Trace", "OnRoundOverPlayers Event");
            foreach (CPlayerInfo cpiPlayer in lstPlayers)
            {
                if (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true)
                {
                    this.StatsTracker[cpiPlayer.SoldierName].Score = cpiPlayer.Score;
                    //EA-GUID, ClanTag, usw.
                    if (cpiPlayer.GUID.Length > 3)
                    {
                        this.StatsTracker[cpiPlayer.SoldierName].EAGuid = cpiPlayer.GUID;
                        //ID - Cache
                        if (this.m_ID_cache.ContainsKey(cpiPlayer.GUID))
                        {
                            this.m_ID_cache[cpiPlayer.GUID].PlayeronServer = true;
                        }
                    }
                    this.StatsTracker[cpiPlayer.SoldierName].ClanTag = cpiPlayer.ClanTag;
                    //TeamId
                    this.StatsTracker[cpiPlayer.SoldierName].TeamId = cpiPlayer.TeamID;

                    //KDR Correction
                    if (this.m_kdrCorrection == enumBoolYesNo.Yes && ((cpiPlayer.Deaths == 0 && cpiPlayer.Kills == 0 && cpiPlayer.Score == 0) == false))
                    {
                        if (this.StatsTracker[cpiPlayer.SoldierName].Deaths > (cpiPlayer.Deaths + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftDeaths))
                        {
                            this.DebugInfo("Trace", "Player: " + cpiPlayer.SoldierName + " has " + this.StatsTracker[cpiPlayer.SoldierName].Deaths + " deaths; correcting to " + cpiPlayer.Deaths + " deaths now");
                            this.StatsTracker[cpiPlayer.SoldierName].Deaths = (cpiPlayer.Deaths + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftDeaths);
                        }
                        if (this.StatsTracker[cpiPlayer.SoldierName].Kills > (cpiPlayer.Kills + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftKills))
                        {
                            this.StatsTracker[cpiPlayer.SoldierName].Kills = (cpiPlayer.Kills + this.StatsTracker[cpiPlayer.SoldierName].BeforeLeftKills);
                        }
                    }
                    //GlobalRank
                    if (cpiPlayer.Rank >= 0)
                    {
                        this.StatsTracker[cpiPlayer.SoldierName].GlobalRank = cpiPlayer.Rank;
                    }
                }
                //Session Score
                lock (this.sessionlock)
                {
                    if (this.m_dicSession.ContainsKey(cpiPlayer.SoldierName) && this.m_sessionON == enumBoolYesNo.Yes)
                    {
                        //KDR Correction
                        if (this.m_kdrCorrection == enumBoolYesNo.Yes && ((cpiPlayer.Deaths == 0 && cpiPlayer.Kills == 0 && cpiPlayer.Score == 0) == false))
                        {
                            if (this.m_dicSession[cpiPlayer.SoldierName].Deaths > (cpiPlayer.Deaths + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftDeaths))
                            {
                                this.DebugInfo("Trace", "Player: " + cpiPlayer.SoldierName + " has " + this.m_dicSession[cpiPlayer.SoldierName].Deaths + " deaths; correcting to " + cpiPlayer.Deaths + " deaths now");
                                this.m_dicSession[cpiPlayer.SoldierName].Deaths = (cpiPlayer.Deaths + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftDeaths);
                            }
                            if (this.m_dicSession[cpiPlayer.SoldierName].Kills > (cpiPlayer.Kills + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftKills))
                            {
                                this.m_dicSession[cpiPlayer.SoldierName].Kills = (cpiPlayer.Kills + this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftKills);
                            }
                        }
                        this.m_dicSession[cpiPlayer.SoldierName].AddScore(cpiPlayer.Score);
                        this.m_dicSession[cpiPlayer.SoldierName].LastScore = 0;
                        this.m_dicSession[cpiPlayer.SoldierName].Rounds++;
                        this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftKills += this.m_dicSession[cpiPlayer.SoldierName].Kills;
                        this.m_dicSession[cpiPlayer.SoldierName].BeforeLeftDeaths += this.m_dicSession[cpiPlayer.SoldierName].Deaths;
                        //TeamId
                        this.m_dicSession[cpiPlayer.SoldierName].TeamId = cpiPlayer.TeamID;
                        if (cpiPlayer.GUID.Length > 2)
                        {
                            this.m_dicSession[cpiPlayer.SoldierName].EAGuid = cpiPlayer.GUID;
                        }
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(delegate { this.CreateSession(cpiPlayer.SoldierName, cpiPlayer.Score, cpiPlayer.GUID); });
                    }
                }
            }
            this.Mapstats.MapEnd();
        }

        public override void OnRoundOver(int winningTeamId) 
        {
            this.DebugInfo("Trace", "OnRoundOver: TeamId -> " + winningTeamId);
            //StatsTracker
            foreach (KeyValuePair<string, CStats> kvp in this.StatsTracker)
            {
                if (kvp.Value.PlayerOnServer == true)
                {
                    if (kvp.Value.TeamId == winningTeamId)
                    {
                        this.StatsTracker[kvp.Key].Wins++;
                    }
                    else
                    {
                        this.StatsTracker[kvp.Key].Losses++;
                    }
                }
            }
            //Session
            lock (this.sessionlock)
            {
                foreach (KeyValuePair<string, CStats> kvp in this.m_dicSession)
                {
                    if (kvp.Value.PlayerOnServer == true)
                    {
                        if (kvp.Value.TeamId == winningTeamId)
                        {
                            this.m_dicSession[kvp.Key].Wins++;
                        }
                        else
                        {
                            this.m_dicSession[kvp.Key].Losses++;
                        }
                    }
                }
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            if (bool_roundStarted == true && StatsTracker.ContainsKey(soldierName) == true)
            {
                if (StatsTracker[soldierName].PlayerOnServer == false)
                {
                    this.StatsTracker[soldierName].Playerjoined = MyDateTime.Now;
                    this.StatsTracker[soldierName].PlayerOnServer = true;
                }
            }
            if (this.m_enWelcomeStats == enumBoolYesNo.Yes)
            {
                if(this.welcomestatsDic.ContainsKey(soldierName))
                {
                    //Call of the Welcomstatsfunction
                    ThreadPool.QueueUserWorkItem(delegate { this.WelcomeStats(soldierName); });
                    lock(this.welcomestatsDic)
                    {
                        this.welcomestatsDic.Remove(soldierName);
                    }
                }
            }
        }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            if ((DateTime.Now.Subtract(this.dtLastRoundendEvent)).TotalSeconds > 30)
            {
                this.dtLastRoundendEvent = DateTime.Now;
                this.DebugInfo("Info", "OnLevelLoaded: " + mapFileName + " Gamemode: " + Gamemode + " Round: " + (roundsPlayed + 1) + "/" + roundsTotal);
                this.DebugInfo("Info", "update sql server");
                this.Nextmapinfo = new CMapstats(MyDateTime.Now, mapFileName, (roundsPlayed + 1), roundsTotal, this.m_dTimeOffset);
                //Calculate Awards
                this.calculateAwards();
                new Thread(StartStreaming).Start();
                m_dicPlayers.Clear();
                this.Spamprotection.Reset();
            }
        }

        public override void OnRoundStartPlayerCount(int limit)
        {
            this.intRoundStartCount = limit;
        }

        public override void OnRoundRestartPlayerCount(int limit)
        {
            this.intRoundRestartCount = limit;
        }

        #endregion

        #region External Commands (ColColonCleaner)

        public void GetStatus(params String[] commands)
        {
            this.DebugInfo("Info", "GetStatus starting!");
            if (commands.Length < 1)
            {
                this.DebugInfo("Error", "Status fetch request canceled, no parameters provided.");
                return;
            }

            new Thread(new ParameterizedThreadStart(SendStatus)).Start(commands[0]);
            this.DebugInfo("Info", "GetStatus finished!");
        }

        private void SendStatus(Object clientInformation)
        {
            this.DebugInfo("Info", "SendStatus starting!");
            try
            {
                //Set current thread name
                Thread.CurrentThread.Name = "SendStatus";

                //Parse client plugin information
                Hashtable parsedClientInformation = (Hashtable)JSON.JsonDecode((String)clientInformation);
                String pluginName = String.Empty;
                String pluginMethod = String.Empty;
                if (!parsedClientInformation.ContainsKey("pluginName"))
                {
                    this.DebugInfo("Error", "Parsed command didn't contain a pluginName!");
                    return;
                }
                else
                {
                    pluginName = (String)parsedClientInformation["pluginName"];
                }

                if (!parsedClientInformation.ContainsKey("pluginMethod"))
                {
                    this.DebugInfo("Error", "Parsed command didn't contain a pluginMethod!");
                    return;
                }
                else
                {
                    pluginMethod = (String)parsedClientInformation["pluginMethod"];
                }

                //Check for active connection to the database using a simple query
                Boolean activeConnection = false;
                this.tablebuilder();
                if ((m_strHost != null) || (m_strDatabase != null) || (m_strDBPort != null) || (m_strUserName != null) || (m_strPassword != null))
                {
                    try
                    {
                        using (MySqlConnection Connection = new MySqlConnection(this.DBConnectionStringBuilder()))
                        {
                            Connection.Open();
                            if (Connection.State == ConnectionState.Open)
                            {
                                string query = "SELECT `ServerID` from `" + this.tbl_server + "` LIMIT 1";
                                using (MySqlCommand command = new MySqlCommand(query, Connection))
                                {
                                    DataTable resultTable = this.SQLquery(command);
                                    if (resultTable.Rows != null)
                                    {
                                        activeConnection = true;
                                    }
                                }
                            }
                            //Connection automatically closed by end of 'using' clause
                        }
                    }
                    catch (Exception e)
                    {
                        this.DebugInfo("Error", "Query could not be performed while sending plugin status.");
                    }
                }

                //Create response hashtable
                Hashtable response = new Hashtable();

                //Add Plugin General Settings
                response["pluginVersion"] = this.GetPluginVersion();
                response["pluginEnabled"] = this.m_isPluginEnabled.ToString();
                //Add Database connection info, without username and password
                response["DBHost"] = this.m_strHost;
                response["DBPort"] = this.m_strDBPort;
                response["DBName"] = this.m_strDatabase;
                //Add Database time offset
                response["DBTimeOffset"] = this.m_dTimeOffset.ToString();
                //Add Whether the connection is active
                response["DBConnectionActive"] = activeConnection.ToString();
                //Add Specific logging settings
                response["ChatloggingEnabled"] = (this.m_enChatloggingON == enumBoolYesNo.Yes).ToString();
                response["InstantChatLoggingEnabled"] = (this.m_enInstantChatlogging == enumBoolYesNo.Yes).ToString();
                response["StatsLoggingEnabled"] = (this.m_enLogSTATS == enumBoolYesNo.Yes).ToString();
                response["DBliveScoreboardEnabled"] = (this.m_enableCurrentPlayerstatsTable == enumBoolYesNo.Yes).ToString();
                //Add Plugin Debug Mode
                response["DebugMode"] = this.GlobalDebugMode;
                //Add Error as "no error"
                response["Error"] = false.ToString();

                //Encode JSON response 
                String JSONResponse = JSON.JsonEncode(response);

                //Send the response
                this.ExecuteCommand("procon.protected.plugins.call", pluginName, pluginMethod, JSONResponse);
            }
            catch (Exception e)
            {
                //Log the error in console
                this.DebugInfo("Error", e.ToString());
            }

            this.DebugInfo("Info", "SendStatus finished!");
        }

        #endregion

        #region In Game Commands

        public void OnCommandStats(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            /*
            this.DebugInfo("Trace", "MatchCommand:" + mtcCommand.Command);
            this.DebugInfo("Trace", "CapturedCommand:" + capCommand.Command);
            this.DebugInfo("Trace", "CapturedCommand:" + capCommand.ResposeScope);
            this.DebugInfo("Trace", "CapturedCommand:" + capCommand.ExtraArguments);
            */
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {                   
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }

                if (capCommand.ExtraArguments.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(delegate { this.GetWeaponStats(this.FindKeyword(capCommand.ExtraArguments.Trim().ToUpper()), strSpeaker, scope); });
                }
                else
                {
                    ThreadPool.QueueUserWorkItem( delegate { this.GetPlayerStats(strSpeaker, 0, scope); });
                }
            }
        }

        public void OnCommandTop10(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }

                if (capCommand.ExtraArguments.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(delegate { this.GetWeaponTop10(this.FindKeyword(capCommand.ExtraArguments.Trim().ToUpper()), strSpeaker, 2, scope); });
                }
                else
                {
                    ThreadPool.QueueUserWorkItem( delegate { this.GetTop10(strSpeaker, 2, scope); });
                }
            }
        }

        public void OnCommandDogtags(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }
                ThreadPool.QueueUserWorkItem(delegate { this.GetDogtags(strSpeaker, 1, scope); });
            }
        }

        public void OnCommandSession(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }
                ThreadPool.QueueUserWorkItem(delegate { this.GetSession(strSpeaker, 1, scope); });
            }
        }

        public void OnCommandServerStats(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }
                ThreadPool.QueueUserWorkItem(delegate { this.GetServerStats(strSpeaker, 1, scope); });
            }
        }

        public void OnCommandPlayerOfTheDay(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }
                ThreadPool.QueueUserWorkItem(delegate { this.GetPlayerOfTheDay(strSpeaker, 1, scope); });
            }
        }

        public void OnCommandTop10ForPeriod(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if ((this.m_enLogSTATS == enumBoolYesNo.Yes) && (this.Spamprotection.isAllowed(strSpeaker) == true))
            {
                string scope = String.Empty;
                if (capCommand.ResposeScope.Contains("!") == true)
                {
                    if (this.m_enSendStatsToAll == enumBoolYesNo.Yes)
                    {
                        scope = "all";
                    }
                    else
                    {
                        scope = "player";
                    }
                }
                else
                {
                    scope = "player";
                }
                ThreadPool.QueueUserWorkItem(delegate { this.GetTop10ForPeriod(strSpeaker, 2, scope, this.m_intDaysForPeriodTop10); });
            }
        }

        #endregion

        #region CChatGUIDStatsLogger Methodes

        private string DBConnectionStringBuilder()
        {
            string conString = String.Empty;
            lock (this.ConnectionStringBuilderlock)
            {
                uint uintport = 3306;
                uint.TryParse(m_strDBPort, out uintport);
                myCSB.Port = uintport;
                myCSB.Server = m_strHost;
                myCSB.UserID = m_strUserName;
                myCSB.Password = m_strPassword;
                myCSB.Database = m_strDatabase;
                //Connection Pool
                if (this.m_connectionPooling == enumBoolOnOff.On)
                {
                    myCSB.Pooling = true;
                    myCSB.MinimumPoolSize = Convert.ToUInt32(this.m_minPoolSize);
                    myCSB.MaximumPoolSize = Convert.ToUInt32(this.m_maxPoolSize);
                    myCSB.ConnectionLifeTime = 600;
                }
                else
                {
                    myCSB.Pooling = false;
                }
                //Compression
                if (this.m_Connectioncompression == enumBoolOnOff.On)
                {
                    myCSB.UseCompression = true;
                }
                else
                {
                    myCSB.UseCompression = false;
                }
                myCSB.AllowUserVariables = true;
                myCSB.DefaultCommandTimeout = 3600;
                myCSB.ConnectionTimeout = 50;
                conString = myCSB.ConnectionString;
            }
            return conString;
        }

        private int GetPlayerTeamID(string strSoldierName)
        {
            int iTeamID = 0; // Neutral Team ID
            if (this.m_dicPlayers.ContainsKey(strSoldierName) == true)
            {
                iTeamID = this.m_dicPlayers[strSoldierName].TeamID;
            }
            return iTeamID;
        }

        private void playerLeftServer(CPlayerInfo cpiPlayer)
        {
            try
            {
                this.DebugInfo("Trace", "playerLeftServer: " + cpiPlayer.SoldierName + " EAGUID: " + cpiPlayer.GUID);
                if (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true)
                {
                    this.StatsTracker[cpiPlayer.SoldierName].Score = cpiPlayer.Score;
                    this.StatsTracker[cpiPlayer.SoldierName].TimePlayerleft = MyDateTime.Now;
                    this.StatsTracker[cpiPlayer.SoldierName].playerleft();
                    //EA-GUID, ClanTag, usw.
                    if (cpiPlayer.GUID.Length > 2)
                    {
                        this.StatsTracker[cpiPlayer.SoldierName].EAGuid = cpiPlayer.GUID;   
                    }
                    //ID cache System
                    if (this.StatsTracker[cpiPlayer.SoldierName].EAGuid.Length > 2)
                    {
                        if (this.m_ID_cache.ContainsKey(this.StatsTracker[cpiPlayer.SoldierName].EAGuid) == true)
                        {
                            this.m_ID_cache[this.StatsTracker[cpiPlayer.SoldierName].EAGuid].PlayeronServer = false;
                        }
                    }
                    this.StatsTracker[cpiPlayer.SoldierName].ClanTag = cpiPlayer.ClanTag;
                }
                //Mapstats
                this.Mapstats.IntplayerleftServer++;
                //Session
                if (this.m_dicSession.ContainsKey(cpiPlayer.SoldierName) == true)
                {
                    if (cpiPlayer.Score > 0)
                    {
                        this.m_dicSession[cpiPlayer.SoldierName].AddScore(cpiPlayer.Score);
                    }
                    this.m_dicSession[cpiPlayer.SoldierName].TimePlayerleft = MyDateTime.Now;
                    this.m_dicSession[cpiPlayer.SoldierName].playerleft();
                    this.DebugInfo("Trace", "Score: " + this.m_dicSession[cpiPlayer.SoldierName].TotalScore.ToString() + " Playtime: " + this.m_dicSession[cpiPlayer.SoldierName].TotalPlaytime.ToString());
                    if (this.m_dicSession[cpiPlayer.SoldierName].TotalScore > 10 || this.m_dicSession[cpiPlayer.SoldierName].Kills > 0 || this.m_dicSession[cpiPlayer.SoldierName].Deaths > 0)
                    {
                        if ((this.m_dicSession[cpiPlayer.SoldierName].EAGuid.Length < 2) && (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true))
                        {
                            if (this.StatsTracker[cpiPlayer.SoldierName].EAGuid.Length > 2)
                            {
                                this.m_dicSession[cpiPlayer.SoldierName].EAGuid = this.StatsTracker[cpiPlayer.SoldierName].EAGuid;
                            }
                        }
                        this.DebugInfo("Trace", "Adding Session of Player " + cpiPlayer.SoldierName + " to passed sessions");
                        //Adding passed session to list if player has a Score greater than 0 or a Player greater than 120 sec
                        this.lstpassedSessions.Add(this.m_dicSession[cpiPlayer.SoldierName]);
                    }
                    //Removing old session
                    lock (this.sessionlock)
                    {
                        this.m_dicSession.Remove(cpiPlayer.SoldierName);
                    }
                }
                else
                {
                    this.DebugInfo("Trace", "playerLeftServer: " + cpiPlayer.SoldierName + " not in session dic");
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "playerLeftServer:" + c);
            }
        }

        private void playerKilled(Kill kKillerVictimDetails)
        {
            if (this.DamageClass.ContainsKey(kKillerVictimDetails.DamageType) == false && !kKillerVictimDetails.DamageType.Equals("Death"))
            {
                this.DebugInfo("Trace", "Weapon: " + kKillerVictimDetails.DamageType + " is missing in the " + this.strServerGameType +".def file!!!");
            }
            //this.DebugInfo("Trace","PlayerKilled Killer: "+ kKillerVictimDetails.Killer.SoldierName + "Victim: " + kKillerVictimDetails.Victim.SoldierName + "Weapon: " + kKillerVictimDetails.DamageType);
            //TEAMKILL OR SUICID
            if (String.Compare(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName) == 0)
            {		//  A Suicide
                this.AddSuicideToStats(kKillerVictimDetails.Killer.SoldierName, this.DamageClass[kKillerVictimDetails.DamageType], kKillerVictimDetails.DamageType);
            }
            else
            {
                if (this.GetPlayerTeamID(kKillerVictimDetails.Killer.SoldierName) == this.GetPlayerTeamID(kKillerVictimDetails.Victim.SoldierName))
                { 	//TeamKill
                    this.AddTeamKillToStats(kKillerVictimDetails.Killer.SoldierName);
                    this.AddDeathToStats(kKillerVictimDetails.Victim.SoldierName, this.DamageClass[kKillerVictimDetails.DamageType], kKillerVictimDetails.DamageType);
                }
                else
                {
                    //this.DebugInfo("Trace","PlayerKilled: Regular Kill");
                    //Regular Kill: Player killed an Enemy
                    this.AddKillToStats(kKillerVictimDetails.Killer.SoldierName, this.DamageClass[kKillerVictimDetails.DamageType], kKillerVictimDetails.DamageType, kKillerVictimDetails.Headshot);
                    this.AddDeathToStats(kKillerVictimDetails.Victim.SoldierName, this.DamageClass[kKillerVictimDetails.DamageType], kKillerVictimDetails.DamageType);
                    if (string.Equals(kKillerVictimDetails.DamageType, "Melee"))
                    {	//Dogtagstracking
                        CKillerVictim KnifeKill = new CKillerVictim(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName);
                        if (m_dicKnifeKills.ContainsKey(KnifeKill) == true)
                        {
                            m_dicKnifeKills[KnifeKill]++;
                        }
                        else
                        {
                            m_dicKnifeKills.Add(KnifeKill, 1);
                        }
                    }
                }
            }
        }

        private DataTable SQLquery(MySqlCommand selectQuery)
        {
            DataTable MyDataTable = new DataTable();
            try
            {
                this.tablebuilder();
                //this.DebugInfo("Trace", "SQLquery: " + selectQuery);                
                if (selectQuery == null)
                {
                    this.DebugInfo("Warning", "SQLquery: selectQuery is null");
                    return MyDataTable;
                }
                else if (selectQuery.CommandText.Equals(String.Empty) == true)
                {
                    this.DebugInfo("Warning", "SQLquery: CommandText is empty");
                    return MyDataTable;
                }

                if (this.m_highPerformanceConnectionMode == enumBoolOnOff.On)
                {
                    try
                    {
                        using (MySqlConnection Connection = new MySqlConnection(this.DBConnectionStringBuilder()))
                        {
                            selectQuery.Connection = Connection;
                            using (MySqlDataAdapter MyAdapter = new MySqlDataAdapter(selectQuery))
                            {
                                if (MyAdapter != null)
                                {
                                    MyAdapter.Fill(MyDataTable);
                                }
                                else
                                {
                                    this.DebugInfo("Warning", "SQLquery: MyAdapter is null");
                                }
                            }
                            Connection.Close();
                        }
                    }
                    catch (MySqlException me)
                    {
                        this.DebugInfo("Error", "SQLQuery:");
                        this.DisplayMySqlErrorCollection(me);
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "SQLQuery:" + c);
                    }
                }
                else
                {
                    lock (this.sqlquerylock)
                    {
                        if (this.MySqlCon == null)
                        {
                            this.MySqlCon = new MySqlConnection(this.DBConnectionStringBuilder());
                        }
                        try
                        {
                            selectQuery.Connection = this.MySqlCon;
                            using (MySqlDataAdapter MyAdapter = new MySqlDataAdapter(selectQuery))
                            {
                                if (MyAdapter != null)
                                {
                                    MyAdapter.Fill(MyDataTable);
                                }
                                else
                                {
                                    this.DebugInfo("Warning", "SQLquery: MyAdapter is null");
                                }
                            }
                        }
                        catch (MySqlException oe)
                        {
                            this.DebugInfo("Error", "SQLQuery:");
                            this.DisplayMySqlErrorCollection(oe);
                            if (MySqlCon.State == ConnectionState.Open)
                            {
                                MySqlCon.Close();
                                MySqlCon = null;
                            }
                        }
                        catch (Exception c)
                        {
                            this.DebugInfo("Error", "SQLQuery:" + c);
                            if (MySqlCon.State == ConnectionState.Open)
                            {
                                MySqlCon.Close();
                                MySqlCon = null;
                            }
                        }
                        finally
                        {
                            try
                            {
                                this.MySqlCon.Close();
                            }
                            catch
                            {
                                this.MySqlCon = null;
                            }
                        }
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "SQLQuery OuterException:" + c);
            }
            return MyDataTable;
        }

        // Updates database with player stats and chatlogs
        private void StartStreaming()
        {
            lock (this.streamlock)
            {
                bool success = false;
                int attemptCount = 0;
                try
                {
                    DateTime StartStreamingTime = MyDateTime.Now;
                    //Make a copy of Statstracker to prevent unwanted errors
                    Dictionary<string, CStats> StatsTrackerCopy = new Dictionary<string, CStats>(this.StatsTracker);
                    //C_ID_Cache id_cache;
                    List<string> lstEAGUIDs = new List<string>();
                    //Clearing the old Dictionary
                    StatsTracker.Clear();
                    if (isStreaming)
                    {
                        this.DebugInfo("Info", "Started streaming to the DB-Server");
                        // Uploads chat logs and Stats for round to database
                        if (ChatLog.Count > 0 || this.m_enLogSTATS == enumBoolYesNo.Yes)
                        {
                            this.tablebuilder(); //Build the tables if not exists
                            if ((m_strHost != null) && (m_strDatabase != null) && (m_strDBPort != null) && (m_strUserName != null) && (m_strPassword != null))
                            {
                                try
                                {
                                    this.OpenMySqlConnection(2);
                                    this.MySql_Connection_is_activ = true;

                                    if (ChatLog.Count > 0 && MySqlConn.State == ConnectionState.Open)
                                    {
                                        string ChatSQL = @"INSERT INTO " + this.tbl_chatlog + @" (logDate, ServerID, logSubset, logSoldierName, logMessage) VALUES ";
                                        lock (ChatLog)
                                        {
                                            int i = 0;
                                            foreach (CLogger log in ChatLog)
                                            {
                                                ChatSQL = string.Concat(ChatSQL, "(@logDate" + i + ", @ServerID" + i + ", @logSubset" + i + ", @logSoldierName" + i + ", @logMessage" + i + "),");
                                                i++;
                                            }
                                            ChatSQL = ChatSQL.Remove(ChatSQL.LastIndexOf(","));
                                            using (MySqlCommand OdbcCom = new MySqlCommand(ChatSQL, MySqlConn))
                                            {
                                                i = 0;
                                                foreach (CLogger log in ChatLog)
                                                {
                                                    OdbcCom.Parameters.AddWithValue("@logDate" + i, log.Time);
                                                    OdbcCom.Parameters.AddWithValue("@ServerID" + i, this.ServerID);
                                                    OdbcCom.Parameters.AddWithValue("@logSubset" + i, log.Subset);
                                                    OdbcCom.Parameters.AddWithValue("@logSoldierName" + i, log.Name);
                                                    OdbcCom.Parameters.AddWithValue("@logMessage" + i, log.Message);
                                                    i++;
                                                }
                                                OdbcCom.ExecuteNonQuery();
                                            }
                                            ChatLog.Clear();
                                        }
                                    }
                                    if (this.m_mapstatsON == enumBoolYesNo.Yes && MySqlConn.State == ConnectionState.Open)
                                    {
                                        this.DebugInfo("Trace", "Mapstats Write querys");
                                        this.Mapstats.calcMaxMinAvgPlayers();
                                        string MapSQL = @"INSERT INTO " + tbl_mapstats + @" (ServerID, TimeMapLoad, TimeRoundStarted, TimeRoundEnd, MapName, Gamemode, Roundcount, NumberofRounds, MinPlayers, AvgPlayers, MaxPlayers, PlayersJoinedServer, PlayersLeftServer)
													VALUES (@ServerID, @TimeMapLoad, @TimeRoundStarted, @TimeRoundEnd, @MapName, @Gamemode, @Roundcount, @NumberofRounds, @MinPlayers, @AvgPlayers, @MaxPlayers, @PlayersJoinedServer, @PlayersLeftServer)";
                                        using (MySqlCommand OdbcCom = new MySqlCommand(MapSQL, MySqlConn))
                                        {
                                            OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                            OdbcCom.Parameters.AddWithValue("@TimeMapLoad", this.Mapstats.TimeMaploaded);
                                            OdbcCom.Parameters.AddWithValue("@TimeRoundStarted", this.Mapstats.TimeMapStarted);
                                            OdbcCom.Parameters.AddWithValue("@TimeRoundEnd", this.Mapstats.TimeRoundEnd);
                                            OdbcCom.Parameters.AddWithValue("@MapName", this.Mapstats.StrMapname);
                                            OdbcCom.Parameters.AddWithValue("@Gamemode", this.Mapstats.StrGamemode);
                                            OdbcCom.Parameters.AddWithValue("@Roundcount", this.Mapstats.IntRound);
                                            OdbcCom.Parameters.AddWithValue("@NumberofRounds", this.Mapstats.IntNumberOfRounds);
                                            OdbcCom.Parameters.AddWithValue("@MinPlayers", this.Mapstats.IntMinPlayers);
                                            OdbcCom.Parameters.AddWithValue("@AvgPlayers", this.Mapstats.DoubleAvgPlayers);
                                            OdbcCom.Parameters.AddWithValue("@MaxPlayers", this.Mapstats.IntMaxPlayers);
                                            OdbcCom.Parameters.AddWithValue("@PlayersJoinedServer", this.Mapstats.IntplayerjoinedServer);
                                            OdbcCom.Parameters.AddWithValue("@PlayersLeftServer", this.Mapstats.IntplayerleftServer);
                                            OdbcCom.ExecuteNonQuery();
                                        }
                                    }
                                    if (this.m_enLogSTATS == enumBoolYesNo.Yes && MySqlConn.State == ConnectionState.Open)
                                    {
                                        this.DebugInfo("Trace", "PlayerStats Write querys");
                                        //Prepare EAGUID List
                                        foreach (KeyValuePair<string, CStats> kvp in StatsTrackerCopy)
                                        {
                                            if (kvp.Value.EAGuid.Length > 1)
                                            {
                                                if (GlobalDebugMode.Equals("Trace"))
                                                {
                                                    this.DebugInfo("Trace", "Adding EAGUID " + kvp.Value.EAGuid + " to searchlist");
                                                }
                                                lstEAGUIDs.Add(kvp.Value.EAGuid);
                                            }
                                        }
                                        //Perform Cache Update
                                        this.UpdateIDCache(lstEAGUIDs);

                                        while (!success)
                                        {
                                            attemptCount++;
                                            try
                                            {
                                                MySqlTrans = MySqlConn.BeginTransaction();
                                                foreach (KeyValuePair<string, CStats> kvp in StatsTrackerCopy)
                                                {
                                                    if (kvp.Key.Length > 0 && kvp.Value.EAGuid.Length > 1)
                                                    {
                                                        if (this.m_ID_cache.ContainsKey(kvp.Value.EAGuid) == false)
                                                        {
                                                            if (GlobalDebugMode.Equals("Trace"))
                                                            {
                                                                this.DebugInfo("Trace", kvp.Value.EAGuid + " is not in Cache!");
                                                            }
                                                            continue;
                                                        }
                                                        if (this.m_ID_cache[kvp.Value.EAGuid].Id >= 1)
                                                        {
                                                            string UpdatedataSQL = @"UPDATE " + this.tbl_playerdata + @" SET SoldierName = @SoldierName, ClanTag = @ClanTag, PBGUID = @PBGUID, IP_Address = @IP_Address, CountryCode = @CountryCode, GlobalRank = @GlobalRank  WHERE PlayerID = @PlayerID";
                                                            using (MySqlCommand OdbcCom = new MySqlCommand(UpdatedataSQL, MySqlConn, MySqlTrans))
                                                            {
                                                                //Update
                                                                if (GlobalDebugMode.Equals("Trace"))
                                                                {
                                                                    this.DebugInfo("Trace", "Update for Player " + kvp.Key);
                                                                    this.DebugInfo("Trace", "ClanTag " + kvp.Value.ClanTag);
                                                                    this.DebugInfo("Trace", "SoldierName " + kvp.Key);
                                                                    this.DebugInfo("Trace", "PBGUID " + kvp.Value.Guid);
                                                                    this.DebugInfo("Trace", "EAGUID " + kvp.Value.EAGuid);
                                                                    this.DebugInfo("Trace", "IP_Address " + kvp.Value.IP);
                                                                    this.DebugInfo("Trace", "CountryCode " + kvp.Value.PlayerCountryCode);
                                                                    this.DebugInfo("Trace", "GlobalRank " + kvp.Value.GlobalRank);
                                                                }
                                                                //OdbcCom.Parameters.AddWithValue("@pr", kvp.Value.ClanTag);
                                                                OdbcCom.Parameters.AddWithValue("@SoldierName", kvp.Key);
                                                                if (kvp.Value.ClanTag != null)
                                                                {
                                                                    if (kvp.Value.ClanTag.Length > 0)
                                                                    {
                                                                        OdbcCom.Parameters.AddWithValue("@ClanTag", kvp.Value.ClanTag);
                                                                    }
                                                                    else
                                                                    {
                                                                        OdbcCom.Parameters.AddWithValue("@ClanTag", Convert.DBNull);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@ClanTag", Convert.DBNull);
                                                                }
                                                                OdbcCom.Parameters.AddWithValue("@PBGUID", kvp.Value.Guid);
                                                                OdbcCom.Parameters.AddWithValue("@IP_Address", kvp.Value.IP);
                                                                OdbcCom.Parameters.AddWithValue("@CountryCode", kvp.Value.PlayerCountryCode);
                                                                OdbcCom.Parameters.AddWithValue("@PlayerID", this.m_ID_cache[kvp.Value.EAGuid].Id);
                                                                OdbcCom.Parameters.AddWithValue("@GlobalRank", kvp.Value.GlobalRank);
                                                                OdbcCom.ExecuteNonQuery();
                                                            }
                                                        }
                                                        else if (this.m_ID_cache[kvp.Value.EAGuid].Id <= 0)
                                                        {
                                                            string InsertdataSQL = @"INSERT INTO " + this.tbl_playerdata + @" (ClanTag, SoldierName, PBGUID, GameID, EAGUID, IP_Address, CountryCode, GlobalRank) VALUES(@ClanTag, @SoldierName, @PBGUID, @GameID, @EAGUID, @IP_Address, @CountryCode, @GlobalRank)";
                                                            using (MySqlCommand OdbcCom = new MySqlCommand(InsertdataSQL, MySqlConn, MySqlTrans))
                                                            {
                                                                //Insert
                                                                if (GlobalDebugMode.Equals("Trace"))
                                                                {
                                                                    this.DebugInfo("Trace", "Insert for Player " + kvp.Key);
                                                                    this.DebugInfo("Trace", "ClanTag " + kvp.Value.ClanTag);
                                                                    this.DebugInfo("Trace", "SoldierName " + kvp.Key);
                                                                    this.DebugInfo("Trace", "PBGUID " + kvp.Value.Guid);
                                                                    this.DebugInfo("Trace", "EAGUID " + kvp.Value.EAGuid);
                                                                    this.DebugInfo("Trace", "IP_Address " + kvp.Value.IP);
                                                                    this.DebugInfo("Trace", "CountryCode " + kvp.Value.PlayerCountryCode);
                                                                    this.DebugInfo("Trace", "GlobalRank " + kvp.Value.GlobalRank);
                                                                }
                                                                if (kvp.Value.ClanTag != null)
                                                                {
                                                                    if (kvp.Value.ClanTag.Length > 0)
                                                                    {
                                                                        OdbcCom.Parameters.AddWithValue("@ClanTag", kvp.Value.ClanTag);
                                                                    }
                                                                    else
                                                                    {
                                                                        OdbcCom.Parameters.AddWithValue("@ClanTag", Convert.DBNull);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@ClanTag", Convert.DBNull);
                                                                }
                                                                OdbcCom.Parameters.AddWithValue("@SoldierName", kvp.Key);
                                                                OdbcCom.Parameters.AddWithValue("@PBGUID", kvp.Value.Guid);
                                                                OdbcCom.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                                                                OdbcCom.Parameters.AddWithValue("@EAGUID", kvp.Value.EAGuid);
                                                                OdbcCom.Parameters.AddWithValue("@IP_Address", kvp.Value.IP);
                                                                OdbcCom.Parameters.AddWithValue("@CountryCode", kvp.Value.PlayerCountryCode);
                                                                OdbcCom.Parameters.AddWithValue("@GlobalRank", kvp.Value.GlobalRank);
                                                                OdbcCom.ExecuteNonQuery();
                                                            }
                                                        }
                                                    }
                                                }
                                                MySqlTrans.Commit();
                                                success = true;
                                            }
                                            catch (MySqlException ex)
                                            {
                                                switch (ex.Number)
                                                {
                                                    case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                    case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                        if (attemptCount < this.TransactionRetryCount)
                                                        {
                                                            this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(Playerdata). Attempt: " + attemptCount);
                                                            try
                                                            {
                                                                MySqlTrans.Rollback();
                                                            }
                                                            catch { }
                                                            Thread.Sleep(attemptCount * 1000);
                                                        }
                                                        else
                                                        {
                                                            this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction Playerdata)");
                                                            throw;
                                                        }
                                                        break;
                                                    default:
                                                        throw; //Other exceptions
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;

                                        //tbl_server_player

                                        this.DebugInfo("Trace", "tbl_server_player Write querys");
                                        this.UpdateIDCache(lstEAGUIDs);
                                        while (!success)
                                        {
                                            attemptCount++;
                                            try
                                            {
                                                //Start of the Transaction
                                                MySqlTrans = MySqlConn.BeginTransaction();
                                                foreach (KeyValuePair<string, CStats> kvp in StatsTrackerCopy)
                                                {
                                                    if (kvp.Value.EAGuid.Length > 0)
                                                    {
                                                        if (this.m_ID_cache.ContainsKey(kvp.Value.EAGuid) == false)
                                                        {
                                                            this.DebugInfo("Trace", kvp.Value.EAGuid + " is not in Cache!");
                                                            continue;
                                                        }
                                                        if (GlobalDebugMode.Equals("Trace"))
                                                        {
                                                            this.DebugInfo("Trace", "PlayerID: " + this.m_ID_cache[kvp.Value.EAGuid].Id);
                                                            this.DebugInfo("Trace", "StatsID: " + this.m_ID_cache[kvp.Value.EAGuid].StatsID);
                                                        }
                                                        if (this.m_ID_cache[kvp.Value.EAGuid].Id > 0 && this.m_ID_cache[kvp.Value.EAGuid].StatsID == 0)
                                                        {
                                                            string InsertdataSQL = @"INSERT INTO " + this.tbl_server_player + @" (ServerID, PlayerID) VALUES(@ServerID, @PlayerID)";
                                                            using (MySqlCommand OdbcCom = new MySqlCommand(InsertdataSQL, MySqlConn, MySqlTrans))
                                                            {
                                                                //Insert
                                                                this.DebugInfo("Trace", "Insert PlayerID " + this.m_ID_cache[kvp.Value.EAGuid].Id + "into tbl_server_player");
                                                                OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                                                OdbcCom.Parameters.AddWithValue("@PlayerID", this.m_ID_cache[kvp.Value.EAGuid].Id);
                                                                OdbcCom.ExecuteNonQuery();
                                                            }
                                                        }
                                                    }
                                                }
                                                MySqlTrans.Commit();
                                                success = true;
                                            }
                                            catch (MySqlException ex)
                                            {
                                                switch (ex.Number)
                                                {
                                                    case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                    case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                        if (attemptCount < this.TransactionRetryCount)
                                                        {
                                                            this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(server_player). Attempt: " + attemptCount);
                                                            try
                                                            {
                                                                MySqlTrans.Rollback();
                                                            }
                                                            catch { }
                                                            Thread.Sleep(attemptCount * 1000);
                                                        }
                                                        else
                                                        {
                                                            this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction server_player)");
                                                            throw;
                                                        }
                                                        break;
                                                    default:
                                                        throw; //Other exceptions
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;

                                        this.DebugInfo("Trace", "Combatstats Write querys");
                                        //Perform Cache Update
                                        this.UpdateIDCache(lstEAGUIDs);
                                        if (this.m_enLogPlayerDataOnly == enumBoolYesNo.No)
                                        {
                                            while (!success)
                                            {
                                                attemptCount++;
                                                try
                                                {
                                                    //Start of the Transaction
                                                    MySqlTrans = MySqlConn.BeginTransaction();

                                                    foreach (KeyValuePair<string, CStats> kvp in StatsTrackerCopy)
                                                    {
                                                        if (this.m_ID_cache.ContainsKey(kvp.Value.EAGuid) == false)
                                                        {
                                                            if (GlobalDebugMode.Equals("Trace"))
                                                            {
                                                                this.DebugInfo("Trace", kvp.Value.EAGuid + " is not in Cache!(empty GUID?)");
                                                            }
                                                            continue;
                                                        }
                                                        Dictionary<string, Dictionary<string, CStats.CUsedWeapon>> tempdic;
                                                        //tempdic = StatsTrackerCopy[kvp.Key].getWeaponKills();
                                                        tempdic = new Dictionary<string, Dictionary<string, CStats.CUsedWeapon>>(kvp.Value.getWeaponKills());
                                                        if (GlobalDebugMode.Equals("Trace"))
                                                        {
                                                            this.DebugInfo("Trace", "PlayerID: " + this.m_ID_cache[kvp.Value.EAGuid].Id);
                                                            this.DebugInfo("Trace", "StatsID: " + this.m_ID_cache[kvp.Value.EAGuid].StatsID);
                                                        }

                                                        if (kvp.Key.Length > 0 && StatsTrackerCopy[kvp.Key].Guid.Length > 0 && this.m_ID_cache[kvp.Value.EAGuid].StatsID >= 1)
                                                        {
                                                            string playerstatsSQL = @"INSERT INTO " + this.tbl_playerstats + @"(StatsID, Score, Kills, Headshots, Deaths, Suicide, TKs, Playtime, Rounds, FirstSeenOnServer, LastSeenOnServer, Killstreak, Deathstreak, HighScore , Wins, Losses)
																VALUES(@StatsID, @Score, @Kills, @Headshots, @Deaths, @Suicide, @TKs, @Playtime, @Rounds, @FirstSeenOnServer, @LastSeenOnServer, @Killstreak, @Deathstreak, @HighScore , @Wins, @Losses) 
                                                                ON DUPLICATE KEY UPDATE Score = Score + @Score, Kills = Kills + @Kills,Headshots = Headshots + @Headshots, Deaths = Deaths + @Deaths, Suicide = Suicide + @Suicide, TKs = TKs + @TKs, Playtime = Playtime + @Playtime, Rounds = Rounds + @Rounds, LastSeenOnServer = @LastSeenOnServer, Killstreak = GREATEST(Killstreak,@Killstreak),Deathstreak = GREATEST(Deathstreak, @Deathstreak) ,HighScore = GREATEST(HighScore, @HighScore), Wins = Wins + @Wins, Losses = Losses + @Losses ";

                                                            using (MySqlCommand OdbcCom = new MySqlCommand(playerstatsSQL, MySqlConn, MySqlTrans))
                                                            {
                                                                //Insert
                                                                OdbcCom.Parameters.AddWithValue("@StatsID", this.m_ID_cache[kvp.Value.EAGuid].StatsID);
                                                                OdbcCom.Parameters.AddWithValue("@Score", kvp.Value.TotalScore);
                                                                OdbcCom.Parameters.AddWithValue("@Kills", kvp.Value.Kills);
                                                                OdbcCom.Parameters.AddWithValue("@Headshots", kvp.Value.Headshots);
                                                                if (kvp.Value.Deaths >= 0)
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@Deaths", kvp.Value.Deaths);
                                                                }
                                                                else
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@Deaths", 0);
                                                                }
                                                                OdbcCom.Parameters.AddWithValue("@Suicide", kvp.Value.Suicides);
                                                                OdbcCom.Parameters.AddWithValue("@TKs", kvp.Value.Teamkills);
                                                                OdbcCom.Parameters.AddWithValue("@Playtime", kvp.Value.TotalPlaytime);
                                                                OdbcCom.Parameters.AddWithValue("@Rounds", 1);
                                                                OdbcCom.Parameters.AddWithValue("@FirstSeenOnServer", kvp.Value.TimePlayerjoined);
                                                                if (kvp.Value.TimePlayerleft != DateTime.MinValue)
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@LastSeenOnServer", kvp.Value.TimePlayerleft);
                                                                }
                                                                else
                                                                {
                                                                    OdbcCom.Parameters.AddWithValue("@LastSeenOnServer", MyDateTime.Now);
                                                                }
                                                                OdbcCom.Parameters.AddWithValue("@Killstreak", kvp.Value.Killstreak);
                                                                OdbcCom.Parameters.AddWithValue("@Deathstreak", kvp.Value.Deathstreak);
                                                                OdbcCom.Parameters.AddWithValue("@HighScore", kvp.Value.TotalScore);
                                                                OdbcCom.Parameters.AddWithValue("@Wins", kvp.Value.Wins);
                                                                OdbcCom.Parameters.AddWithValue("@Losses", kvp.Value.Losses);

                                                                OdbcCom.ExecuteNonQuery();
                                                            }

                                                            if (this.m_weaponstatsON == enumBoolYesNo.Yes)
                                                            {
                                                                this.DebugInfo("Trace", "Weaponstats Write querys");

                                                                string NewWeaponStatsSQL = @"INSERT INTO `" + this.tbl_weapons_stats + @"` (`StatsID`,`WeaponID`,`Kills`,`Headshots`,`Deaths`)
                                                                                         VALUES(@StatsID, @WeaponID, @Kills, @Headshots, @Deaths)
                                                                                         ON DUPLICATE KEY UPDATE  `Kills` = `Kills` + @Kills ,`Headshots` = `Headshots` + @Headshots,`Deaths` = `Deaths` + @Deaths";

                                                                foreach (KeyValuePair<string, Dictionary<string, CStats.CUsedWeapon>> branch in tempdic)
                                                                {
                                                                    //Build Query for Weaponstats
                                                                    if (tempdic != null)
                                                                    {
                                                                        foreach (KeyValuePair<string, CStats.CUsedWeapon> leaf in branch.Value)
                                                                        {
                                                                            if (leaf.Value.Kills != 0 || leaf.Value.Kills != 0 || leaf.Value.Deaths != 0)
                                                                            {
                                                                                if (this.WeaponMappingDic.ContainsKey(leaf.Value.Name))
                                                                                {
                                                                                    using (MySqlCommand OdbcCom = new MySqlCommand(NewWeaponStatsSQL, MySqlConn, MySqlTrans))
                                                                                    {
                                                                                        OdbcCom.Parameters.AddWithValue("@StatsID", this.m_ID_cache[kvp.Value.EAGuid].StatsID);
                                                                                        OdbcCom.Parameters.AddWithValue("@WeaponID", this.WeaponMappingDic[leaf.Value.Name]);
                                                                                        OdbcCom.Parameters.AddWithValue("@Kills", leaf.Value.Kills);
                                                                                        OdbcCom.Parameters.AddWithValue("@Headshots", leaf.Value.Headshots);
                                                                                        OdbcCom.Parameters.AddWithValue("@Deaths", leaf.Value.Deaths);
                                                                                        OdbcCom.ExecuteNonQuery();
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            //Awards
                                                            /*
                                                            if (this.m_awardsON == enumBoolYesNo.Yes && StatsTrackerCopy[kvp.Key].Awards.DicAwards.Count > 0)
                                                            {
                                                                string awardsInsert = "INSERT INTO " + tbl_awards + @" (`AwardID` ";
                                                                string awardsValues = ") VALUES (" + int_id;
                                                                string awardsUpdate = " ON DUPLICATE KEY UPDATE ";
                                                                foreach (KeyValuePair<string, int> award in StatsTrackerCopy[kvp.Key].Awards.DicAwards)
                                                                {
                                                                    awardsInsert = String.Concat(awardsInsert, ", `", award.Key, "`");
                                                                    awardsValues = String.Concat(awardsValues, ", ", award.Value.ToString());
                                                                    awardsUpdate = String.Concat(awardsUpdate, " `", award.Key, "` = `", award.Key, "` + ", award.Value.ToString(), ", ");
                                                                }
                                                                // Remove the last comma
                                                                int charindex2 = awardsUpdate.LastIndexOf(",");
                                                                if (charindex2 > 0)
                                                                {
                                                                    awardsUpdate = awardsUpdate.Remove(charindex2);
                                                                }
                                                                awardsInsert = String.Concat(awardsInsert, awardsValues, ") ", awardsUpdate);
                                                                //Sent Query to the Server
                                                                this.DebugInfo("Awardquery: " + awardsInsert);
                                                                using (MySqlCommand OdbcCom = new MySqlCommand(awardsInsert, OdbcConn, OdbcTrans))
                                                                {
                                                                    OdbcCom.ExecuteNonQuery();
                                                                }
                                                            }
                                                            */
                                                        }
                                                    }
                                                    MySqlTrans.Commit();
                                                    success = true;
                                                }
                                                catch (MySqlException ex)
                                                {
                                                    switch (ex.Number)
                                                    {
                                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                            if (attemptCount < this.TransactionRetryCount)
                                                            {
                                                                this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(Stats). Attempt: " + attemptCount);
                                                                try
                                                                {
                                                                    MySqlTrans.Rollback();
                                                                }
                                                                catch { }
                                                                Thread.Sleep(attemptCount * 1000);
                                                            }
                                                            else
                                                            {
                                                                this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction Stats)");
                                                                throw;
                                                            }
                                                            break;
                                                        default:
                                                            throw; //Other exceptions
                                                    }
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;
                                        if (this.m_enLogPlayerDataOnly == enumBoolYesNo.No)
                                        {
                                            while (!success)
                                            {
                                                attemptCount++;
                                                try
                                                {
                                                    //Start of the Transaction
                                                    MySqlTrans = MySqlConn.BeginTransaction();
                                                    this.DebugInfo("Trace", "Dogtagstats Write querys");
                                                    string KnifeSQL = String.Empty;
                                                    foreach (KeyValuePair<CKillerVictim, int> kvp in m_dicKnifeKills)
                                                    {
                                                        if (StatsTrackerCopy.ContainsKey(kvp.Key.Killer) == false || StatsTrackerCopy.ContainsKey(kvp.Key.Victim) == false)
                                                        {
                                                            continue;
                                                        }
                                                        if (this.m_ID_cache.ContainsKey(StatsTrackerCopy[kvp.Key.Killer].EAGuid) == false || this.m_ID_cache.ContainsKey(StatsTrackerCopy[kvp.Key.Victim].EAGuid) == false)
                                                        {
                                                            continue;
                                                        }
                                                        if (this.m_ID_cache[StatsTrackerCopy[kvp.Key.Killer].EAGuid].StatsID > 0 && this.m_ID_cache[StatsTrackerCopy[kvp.Key.Victim].EAGuid].StatsID > 0)
                                                        {
                                                            KnifeSQL = "INSERT INTO " + this.tbl_dogtags + @"(KillerID, VictimID, Count) VALUES(@KillerID, @VictimID, @Count)
                            						ON DUPLICATE KEY UPDATE Count = Count + @Count";
                                                            using (MySqlCommand OdbcCom = new MySqlCommand(KnifeSQL, MySqlConn, MySqlTrans))
                                                            {
                                                                OdbcCom.Parameters.AddWithValue("@KillerID", this.m_ID_cache[StatsTrackerCopy[kvp.Key.Killer].EAGuid].StatsID);
                                                                OdbcCom.Parameters.AddWithValue("@VictimID", this.m_ID_cache[StatsTrackerCopy[kvp.Key.Victim].EAGuid].StatsID);
                                                                OdbcCom.Parameters.AddWithValue("@Count", m_dicKnifeKills[kvp.Key]);
                                                                OdbcCom.ExecuteNonQuery();
                                                            }
                                                        }
                                                    }
                                                    MySqlTrans.Commit();
                                                    success = true;
                                                }
                                                catch (MySqlException ex)
                                                {
                                                    switch (ex.Number)
                                                    {
                                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                            if (attemptCount < this.TransactionRetryCount)
                                                            {
                                                                this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(Dogtags). Attempt: " + attemptCount);
                                                                try
                                                                {
                                                                    MySqlTrans.Rollback();
                                                                }
                                                                catch { }
                                                                Thread.Sleep(attemptCount * 1000);
                                                            }
                                                            else
                                                            {
                                                                this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction Dogtags)");
                                                                throw;
                                                            }
                                                            break;
                                                        default:
                                                            throw; //Other exceptions
                                                    }
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;

                                        while (!success)
                                        {
                                            attemptCount++;
                                            try
                                            {
                                                //Start of the Transaction
                                                MySqlTrans = MySqlConn.BeginTransaction();

                                                //Write the Player Sessions

                                                if (this.m_sessionON == enumBoolYesNo.Yes && this.m_enSessionTracking == enumBoolYesNo.Yes)
                                                {
                                                    bool containsvaildsessions = false;
                                                    StringBuilder InsertSQLSession = new StringBuilder(500);
                                                    InsertSQLSession.Append(@"INSERT INTO " + this.tbl_sessions + @" (`StatsID`, `StartTime`,`EndTime`, `Score`, `Kills`, `Headshots`, `Deaths`, `TKs`, `Suicide`,`RoundCount`, `Playtime`, `HighScore`, `Killstreak`, `Deathstreak`, `Wins`, `Losses`) VALUES");

                                                    this.DebugInfo("Trace", this.lstpassedSessions.Count + " Sessions to write to Sessiontable");
                                                    int i = 0;
                                                    foreach (CStats session in this.lstpassedSessions)
                                                    {
                                                        if (this.m_ID_cache.ContainsKey(session.EAGuid) == true)
                                                        {
                                                            if (this.m_ID_cache[session.EAGuid].StatsID > 0)
                                                            {
                                                                //write session
                                                                containsvaildsessions = true;
                                                                InsertSQLSession.Append(" (@StatsID" + i + ", @StartTime" + i + ", @EndTime" + i + ", @Score" + i + ", @Kills" + i + ", @Headshots" + i + ", @Deaths" + i + ", @TKs" + i + ", @Suicide" + i + ", @RoundCount" + i + ", @Playtime" + i + ", @HighScore" + i + ", @Killstreak" + i + ", @Deathstreak" + i + ", @Wins" + i + ", @Losses" + i + "),");
                                                                i++;
                                                            }
                                                        }
                                                    }
                                                    if (containsvaildsessions)
                                                    {
                                                        //remove last comma
                                                        InsertSQLSession.Length = InsertSQLSession.Length - 1;
                                                        using (MySqlCommand sessionInsert = new MySqlCommand(InsertSQLSession.ToString(), MySqlConn, MySqlTrans))
                                                        {
                                                            i = 0; 
                                                            foreach (CStats session in this.lstpassedSessions)
                                                            {
                                                                if (this.m_ID_cache.ContainsKey(session.EAGuid) == true)
                                                                {
                                                                    if (this.m_ID_cache[session.EAGuid].StatsID > 0)
                                                                    {
                                                                        sessionInsert.Parameters.AddWithValue("@StatsID" + i, this.m_ID_cache[session.EAGuid].StatsID);
                                                                        sessionInsert.Parameters.AddWithValue("@StartTime" + i, session.TimePlayerjoined);
                                                                        sessionInsert.Parameters.AddWithValue("@EndTime" + i, session.TimePlayerleft);
                                                                        sessionInsert.Parameters.AddWithValue("@Score" + i, session.TotalScore);
                                                                        sessionInsert.Parameters.AddWithValue("@Kills" + i, session.Kills);
                                                                        sessionInsert.Parameters.AddWithValue("@Headshots" + i, session.Headshots);
                                                                        if (session.Deaths >= 0)
                                                                        {
                                                                            sessionInsert.Parameters.AddWithValue("@Deaths" + i, session.Deaths);
                                                                        }
                                                                        else
                                                                        {
                                                                            sessionInsert.Parameters.AddWithValue("@Deaths" + i, 0);
                                                                        }
                                                                        sessionInsert.Parameters.AddWithValue("@TKs" + i, session.Teamkills);
                                                                        sessionInsert.Parameters.AddWithValue("@Suicide" + i, session.Suicides);
                                                                        sessionInsert.Parameters.AddWithValue("@RoundCount" + i, session.Rounds);
                                                                        sessionInsert.Parameters.AddWithValue("@Playtime" + i, session.TotalPlaytime);
                                                                        sessionInsert.Parameters.AddWithValue("@HighScore" + i, session.HighScore);
                                                                        sessionInsert.Parameters.AddWithValue("@Killstreak" + i, session.Killstreak);
                                                                        sessionInsert.Parameters.AddWithValue("@Deathstreak" + i, session.Deathstreak);
                                                                        sessionInsert.Parameters.AddWithValue("@Wins" + i, session.Wins);
                                                                        sessionInsert.Parameters.AddWithValue("@Losses" + i, session.Losses);
                                                                        i++;
                                                                    }
                                                                }
                                                            }
                                                            sessionInsert.ExecuteNonQuery();
                                                            this.lstpassedSessions.Clear();
                                                        }
                                                    }
                                                }
                                                //Commit the Transaction for the Playerstats
                                                MySqlTrans.Commit();
                                                success = true;
                                            }
                                            catch (MySqlException ex)
                                            {
                                                switch (ex.Number)
                                                {
                                                    case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                    case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                        if (attemptCount < this.TransactionRetryCount)
                                                        {
                                                            this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(Sessions). Attempt: " + attemptCount);
                                                            try
                                                            {
                                                                MySqlTrans.Rollback();
                                                            }
                                                            catch { }
                                                            Thread.Sleep(attemptCount * 1000);
                                                        }
                                                        else
                                                        {
                                                            this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction Sessions)");
                                                            throw;
                                                        }
                                                        break;
                                                    default:
                                                        throw; //Other exceptions
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;

                                        //Calculate ServerStats
                                        if (this.boolSkipServerStatsUpdate == false && this.m_enLogPlayerDataOnly == enumBoolYesNo.No)
                                        {
                                            this.DebugInfo("Trace", "Serverstats Write query");
                                            while (!success)
                                            {
                                                attemptCount++;
                                                try
                                                {
                                                    MySqlTrans = MySqlConn.BeginTransaction();
                                                    string serverstats = @"REPLACE INTO " + this.tbl_server_stats + @" SELECT tsp.ServerID, Count(*) AS CountPlayers, SUM(tps.Score) AS SumScore, AVG(tps.Score) AS AvgScore, SUM(tps.Kills) AS SumKills,  AVG(tps.Kills) AS AvgKills, SUM(tps.Headshots) AS SumHeadshots,
                                                        AVG(tps.Headshots) AS AvgHeadshots, SUM(tps.Deaths) AS SumDeaths, AVG(tps.Deaths) AS AvgDeaths, SUM(tps.Suicide) AS SumSuicide, AVG(tps.Suicide) AS AvgSuicide, SUM(tps.TKs) AS SumTKs, AVG(tps.TKs) AS AvgTKs,
                                                        SUM(tps.Playtime) AS SumPlaytime, AVG(tps.Playtime) AS AvgPlaytime, SUM(tps.Rounds) AS SumRounds, AVG(tps.Rounds) AS AvgRounds 
                                                        FROM " + this.tbl_playerstats + @" tps
                                                        INNER JOIN " + this.tbl_server_player + @" tsp ON tps.StatsID = tsp.StatsID
                                                        WHERE tsp.ServerID = @ServerID GROUP BY tsp.ServerID";
                                                    using (MySqlCommand OdbcCom = new MySqlCommand(serverstats, MySqlConn, MySqlTrans))
                                                    {
                                                        OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                                        OdbcCom.ExecuteNonQuery();
                                                    }
                                                    MySqlTrans.Commit();
                                                    success = true;
                                                }
                                                catch (MySqlException ex)
                                                {
                                                    switch (ex.Number)
                                                    {
                                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                                            if (attemptCount < this.TransactionRetryCount)
                                                            {
                                                                this.DebugInfo("Warning", "Warning in StartStreaming: Locktimeout or Deadlock occured restarting Transaction(Serverstats). Attempt: " + attemptCount);
                                                                try
                                                                {
                                                                    MySqlTrans.Rollback();
                                                                }
                                                                catch { }
                                                                Thread.Sleep(attemptCount * 1000);
                                                            }
                                                            else
                                                            {
                                                                this.DebugInfo("Error", "Error in StartStreaming: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction Serverstats)");
                                                                throw;
                                                            }
                                                            break;
                                                        default:
                                                            throw; //Other exceptions
                                                    }
                                                }
                                            }
                                        }
                                        //Reset bool and counter
                                        attemptCount = 0;
                                        success = false;

                                        StatsTrackerCopy.Clear();
                                        this.m_dicKnifeKills.Clear();

                                        List<string> leftplayerlist = new List<string>();

                                        foreach (KeyValuePair<string, C_ID_Cache> kvp in this.m_ID_cache)
                                        {
                                            if (this.m_ID_cache[kvp.Key].PlayeronServer == false)
                                            {
                                                leftplayerlist.Add(kvp.Key);
                                            }
                                            // Because so playerleft event seems not been reported by the server
                                            this.m_ID_cache[kvp.Key].PlayeronServer = false;
                                        }
                                        foreach (string player in leftplayerlist)
                                        {
                                            this.m_ID_cache.Remove(player);
                                            //this.DebugInfo("Removed " + player);
                                        }
                                        this.DebugInfo("Info", "Status ID-Cache: " + m_ID_cache.Count + " ID's in cache");
                                        if (this.m_ID_cache.Count > 500)
                                        {
                                            this.DebugInfo("Warning", "Forced Cache clear due the Nummber of cached IDs reached over 500 entries(overflowProtection)");
                                            this.m_ID_cache.Clear();
                                        }
                                    }
                                    else
                                    {
                                        StatsTracker.Clear();
                                    }
                                }
                                catch (MySqlException oe)
                                {
                                    this.DebugInfo("Error", "Error in Startstreaming: ");
                                    this.DisplayMySqlErrorCollection(oe);
                                    this.m_ID_cache.Clear();
                                    this.m_dicKnifeKills.Clear();
                                    try { MySqlTrans.Rollback(); }
                                    catch { }
                                }
                                catch (Exception c)
                                {
                                    this.DebugInfo("Error", "Error in Startstreaming: " + c);
                                    this.m_ID_cache.Clear();
                                    this.m_dicKnifeKills.Clear();
                                    try { MySqlTrans.Rollback(); }
                                    catch { }
                                }
                                finally
                                {
                                    StatsTrackerCopy = null;
                                    this.Mapstats = this.Nextmapinfo;
                                    this.MySql_Connection_is_activ = false;
                                    this.CloseMySqlConnection(2);

                                    //Update Serverranking
                                    this.UpdateRanking();
                                    //Welcomestats dic
                                    this.checkWelcomeStatsDic();
                                    if (GlobalDebugMode.Equals("Info"))
                                    {
                                        TimeSpan duration = MyDateTime.Now - StartStreamingTime;
                                        this.DebugInfo("Info", "Streamingprocess duration: " + Math.Round(duration.TotalSeconds, 3) + " seconds");
                                    }
                                }
                            }
                            else
                            {
                                this.DebugInfo("Error", "Streaming cancelled.  Please enter all database information");
                            }
                        }
                    }
                }
                catch (MySqlException oe)
                {
                    this.DebugInfo("Error", "Error in Startstreaming OuterException: ");
                    this.DisplayMySqlErrorCollection(oe);
                    this.m_ID_cache.Clear();
                    this.m_dicKnifeKills.Clear();
                }
                catch (Exception c)
                {
                    this.DebugInfo("Error", "Error in Startstreaming OuterException: " + c);
                    this.m_ID_cache.Clear();
                    this.m_dicKnifeKills.Clear();
                }
            }
        }

        private void UpdateCurrentPlayerTable(List<CPlayerInfo> lstPlayers)
        {
            if (this.boolTableEXISTS == false || ServerID <= 0)
            {
                return;
            }
            this.DebugInfo("Trace", "UpdateCurrentPlayerTable");
            bool success = false;
            int attemptCount = 0;
            try
            {
                using (MySqlConnection DBConnection = new MySqlConnection(this.DBConnectionStringBuilder()))
                {
                    string deleteSQL = "DELETE FROM " + this.tbl_currentplayers + " WHERE ServerID = @ServerID";
                    StringBuilder InsertSQL = new StringBuilder("INSERT INTO " + this.tbl_currentplayers + " (ServerID, SoldierName, ClanTag ,EA_GUID, PB_GUID, Score, Kills, Deaths, Headshots, Suicide, TeamID, SquadID, PlayerJoined, Ping, IP_aton, CountryCode, Killstreak, Deathstreak, GlobalRank ) VALUES ");
                    MySql.Data.MySqlClient.MySqlTransaction Tx = null;

                    DBConnection.Open();
                    while (!success)
                    {
                        attemptCount++;
                        try
                        {
                            //Start of the Transaction
                            Tx = DBConnection.BeginTransaction();

                            using (MySqlCommand OdbcCom = new MySqlCommand(deleteSQL, DBConnection, Tx))
                            {
                                OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                OdbcCom.ExecuteNonQuery();
                            }
                            if (lstPlayers.Count > 0)
                            {
                                int i = 0;
                                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                                {
                                    InsertSQL.Append("( @ServerID, @SoldierName" + i + ",@ClanTag" + i + ", @EA_GUID" + i + ", @PB_GUID" + i + ", @Score" + i + ", @Kills" + i + ", @Deaths" + i + ", @Suicide" + i + ", @Headshots" + i + ",@TeamID" + i + ",@SquadID" + i + " ,@PlayerJoined" + i + ",@Ping" + i + ", INET_ATON( @IP_aton" + i + ")" + ",@CountryCode" + i + ",@Killstreak" + i + ",@Deathstreak" + i + ",@GlobalRank" + i + "),");
                                    i++;
                                }
                                InsertSQL.Length = InsertSQL.Length - 1;
                                using (MySqlCommand OdbcCom = new MySqlCommand(InsertSQL.ToString(), DBConnection, Tx))
                                {
                                    i = 0;
                                    OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                    foreach (CPlayerInfo cpiPlayer in lstPlayers)
                                    {
                                        OdbcCom.Parameters.AddWithValue("@SoldierName" + i, cpiPlayer.SoldierName);
                                        OdbcCom.Parameters.AddWithValue("@ClanTag" + i, cpiPlayer.ClanTag + "");
                                        OdbcCom.Parameters.AddWithValue("@EA_GUID" + i, cpiPlayer.GUID);
                                        if (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true)
                                        {
                                            OdbcCom.Parameters.AddWithValue("@PB_GUID" + i, this.StatsTracker[cpiPlayer.SoldierName].Guid);
                                        }
                                        else
                                        {
                                            OdbcCom.Parameters.AddWithValue("@PB_GUID" + i, ""); //placeholder
                                        }
                                        OdbcCom.Parameters.AddWithValue("@Score" + i, cpiPlayer.Score);
                                        OdbcCom.Parameters.AddWithValue("@Kills" + i, cpiPlayer.Kills);
                                        OdbcCom.Parameters.AddWithValue("@Deaths" + i, cpiPlayer.Deaths);
                                        if (this.StatsTracker.ContainsKey(cpiPlayer.SoldierName) == true)
                                        {
                                            OdbcCom.Parameters.AddWithValue("@Headshots" + i, this.StatsTracker[cpiPlayer.SoldierName].Headshots);
                                            OdbcCom.Parameters.AddWithValue("@PlayerJoined" + i, this.StatsTracker[cpiPlayer.SoldierName].Playerjoined);
                                            OdbcCom.Parameters.AddWithValue("@CountryCode" + i, this.StatsTracker[cpiPlayer.SoldierName].PlayerCountryCode);
                                            OdbcCom.Parameters.AddWithValue("@Killstreak" + i, this.StatsTracker[cpiPlayer.SoldierName].Killstreak);
                                            OdbcCom.Parameters.AddWithValue("@Deathstreak" + i, this.StatsTracker[cpiPlayer.SoldierName].Deathstreak);
                                            OdbcCom.Parameters.AddWithValue("@Suicide" + i, this.StatsTracker[cpiPlayer.SoldierName].Suicides);

                                            // Check if string is empty or null. If it is then send a 0.0.0.0 instead for ip address
                                            if (string.IsNullOrEmpty(this.StatsTracker[cpiPlayer.SoldierName].IP.Trim()))
                                            {
                                                OdbcCom.Parameters.AddWithValue("@IP_aton" + i, "0.0.0.0");
                                            }
                                            else
                                            {
                                                OdbcCom.Parameters.AddWithValue("@IP_aton" + i, this.StatsTracker[cpiPlayer.SoldierName].IP.Trim());
                                            }
                                        }
                                        else
                                        {
                                            OdbcCom.Parameters.AddWithValue("@Headshots" + i, 0); //Headshot placeholder
                                            OdbcCom.Parameters.AddWithValue("@PlayerJoined" + i, MyDateTime.Now);
                                            OdbcCom.Parameters.AddWithValue("@CountryCode" + i, "");
                                            OdbcCom.Parameters.AddWithValue("@Killstreak" + i, 0);
                                            OdbcCom.Parameters.AddWithValue("@Deathstreak" + i, 0);
                                            OdbcCom.Parameters.AddWithValue("@Suicide" + i, 0);
                                            OdbcCom.Parameters.AddWithValue("@IP_aton" + i, "0.0.0.0");
                                        }
                                        OdbcCom.Parameters.AddWithValue("@TeamID" + i, cpiPlayer.TeamID);
                                        OdbcCom.Parameters.AddWithValue("@SquadID" + i, cpiPlayer.SquadID);
                                        if (cpiPlayer.Ping >= 0 && cpiPlayer.Ping < 65000)
                                        {
                                            OdbcCom.Parameters.AddWithValue("@Ping" + i, cpiPlayer.Ping);
                                        }
                                        else
                                        {
                                            OdbcCom.Parameters.AddWithValue("@Ping" + i, 0);
                                        }
                                        if (cpiPlayer.Rank >= 0 && cpiPlayer.Rank < 6500)
                                        {
                                            OdbcCom.Parameters.AddWithValue("@GlobalRank" + i, cpiPlayer.Rank);
                                        }
                                        else
                                        {
                                            OdbcCom.Parameters.AddWithValue("@GlobalRank" + i, 0);
                                        }
                                        //Increment Index
                                        i++;
                                    }
                                    OdbcCom.ExecuteNonQuery();
                                }
                            }
                            Tx.Commit();
                            success = true;
                        }
                        catch (MySqlException ex)
                        {
                            switch (ex.Number)
                            {
                                case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                    if (attemptCount < this.TransactionRetryCount)
                                    {
                                        this.DebugInfo("Warning", "Warning in UpdateCurrentPlayerTable: Locktimeout or Deadlock occured restarting Transaction(delete and Insert). Attempt: " + attemptCount);
                                        try
                                        {
                                            if (Tx.Connection != null)
                                            {
                                                Tx.Rollback();
                                            }
                                        }
                                        catch { }
                                        Thread.Sleep(attemptCount * 1000);
                                    }
                                    else
                                    {
                                        this.DebugInfo("Error", "Error in UpdateCurrentPlayerTable: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction delete und Insert)");
                                        throw;
                                    }
                                    break;
                                default:
                                    throw; //Other exceptions
                            }
                        }
                    }
                    //Reset bool and counter
                    attemptCount = 0;
                    success = false;
                    try
                    {
                        DBConnection.Close();
                    }
                    catch {};
                }
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "Error in UpdateCurrentPlayerTable: ");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in UpdateCurrentPlayerTable: " + c.ToString());
            }
           
        }

        //Fetch data from the db

        private C_ID_Cache GetID(string EAguid)
        {
            int playerID = 0;
            int StatsID = 0;
            if (GlobalDebugMode.Equals("Trace"))
            {
                this.DebugInfo("Trace", "Tying to get IDs form DB or cache for EAGuid: " + EAguid);
            }
            try
            {
                if (this.m_ID_cache.ContainsKey(EAguid) == true)
                {
                    if (this.m_ID_cache[EAguid].Id >= 1 && this.m_ID_cache[EAguid].StatsID >= 1)
                    {
                        //CacheHit
                        if (GlobalDebugMode.Equals("Trace"))
                        {
                            this.DebugInfo("Trace", "Status ID-Cache: used IDs(" + this.m_ID_cache[EAguid].Id + " | " + this.m_ID_cache[EAguid].StatsID + ") from cache for EAGuid " + EAguid);
                        }
                        return this.m_ID_cache[EAguid];
                    }
                    else
                    {
                        //Cachemiss
                        if (this.m_ID_cache[EAguid].Id <= 0)
                        {
                            using (MySqlCommand MyCommand = new MySqlCommand(@"SELECT `PlayerID` FROM `" + this.tbl_playerdata + "` WHERE `GameID` = @GameID AND `EAGUID` = @EAGUID "))
                            {
                                MyCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                                MyCommand.Parameters.AddWithValue("@EAGUID", EAguid);
                                DataTable resultTable = this.SQLquery(MyCommand);
                                if (resultTable.Rows != null)
                                {
                                    foreach (DataRow row in resultTable.Rows)
                                    {
                                        playerID = Convert.ToInt32(row["PlayerID"]);
                                        this.m_ID_cache[EAguid].Id = playerID;
                                    }
                                }
                            }
                        }
                        else
                        {
                            playerID = this.m_ID_cache[EAguid].Id;
                        }
                        if (playerID >= 1)
                        {
                            using (MySqlCommand MyCommand = new MySqlCommand(@"SELECT `StatsID` FROM `" + this.tbl_server_player + "` WHERE `PlayerID` = @PlayerID AND `ServerID`= @ServerID "))
                            {
                                MyCommand.Parameters.AddWithValue("@PlayerID", playerID);
                                MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                                DataTable resultTable = this.SQLquery(MyCommand);
                                if (resultTable.Rows != null)
                                {
                                    foreach (DataRow row in resultTable.Rows)
                                    {
                                        StatsID = Convert.ToInt32(row["StatsID"]);
                                        this.m_ID_cache[EAguid].StatsID = StatsID;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Cache has no entry
                    using (MySqlCommand MyCommand = new MySqlCommand(@"SELECT `PlayerID` FROM `" + this.tbl_playerdata + "` WHERE `GameID` = @GameID AND `EAGUID` = @EAGUID "))
                    {
                        MyCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                        MyCommand.Parameters.AddWithValue("@EAGUID", EAguid);
                        DataTable resultTable = this.SQLquery(MyCommand);
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                playerID = Convert.ToInt32(row["PlayerID"]);
                            }
                        }
                    }
                    if (playerID >= 1)
                    {
                        using (MySqlCommand MyCommand = new MySqlCommand(@"SELECT `StatsID` FROM `" + this.tbl_server_player + "` WHERE `PlayerID` = @PlayerID AND `ServerID`= @ServerID"))
                        {
                            MyCommand.Parameters.AddWithValue("@PlayerID", playerID);
                            MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                            DataTable resultTable = this.SQLquery(MyCommand);
                            if (resultTable.Rows != null)
                            {
                                foreach (DataRow row in resultTable.Rows)
                                {
                                    StatsID = Convert.ToInt32(row["StatsID"]);
                                }
                            }
                        }
                    }
                    this.m_ID_cache.Add(EAguid, new C_ID_Cache(StatsID, playerID, true));
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "GetID: " + c);
                return null;
            }
            if (GlobalDebugMode.Equals("Trace"))
            {
                this.DebugInfo("Trace", "Returning ID: PlayerID: " + playerID + " StatsID: " + StatsID);
            }
            return this.m_ID_cache[EAguid];
        }

        private void UpdateIDCache(List<string> lstEAGUID)
        {
            foreach (string EAGUID in lstEAGUID)
            {
                if (this.m_ID_cache.ContainsKey(EAGUID) == false)
                {
                    //Adding an empty entry
                    this.m_ID_cache.Add(EAGUID, new C_ID_Cache(0, 0, true));
                }
            }
            if (lstEAGUID.Count == 0)
            {
                this.DebugInfo("Trace", "UpdateIDCache: Empty List!");
                return;
            }

            StringBuilder SQL = new StringBuilder(@"SELECT EAGUID, tpd.PlayerID, tsp.StatsID
                          FROM " + this.tbl_playerdata + @" tpd
                          LEFT JOIN " + this.tbl_server_player + @" tsp ON tpd.PlayerID = tsp.PlayerID AND ServerID = @ServerID
                          WHERE tpd.GameID = @GameID AND tpd.EAGUID IN (");
            for (int i = 1; i <= lstEAGUID.Count; i++)
            {
                SQL.Append("@EAGUID" + i + ",");
            }
            SQL.Length = SQL.Length - 1;
            SQL.Append(')');
            try
            {
                using (MySqlCommand SelectCommand = new MySqlCommand(SQL.ToString()))
                {
                    SelectCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    SelectCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                    int i = 1;
                    foreach (string EAGUID in lstEAGUID)
                    {
                        SelectCommand.Parameters.AddWithValue("@EAGUID" + i, EAGUID);
                        i++;
                    }
                    DataTable result;
                    result = this.SQLquery(SelectCommand);
                    if (result != null)
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            if (row[1] == Convert.DBNull)
                            {
                                this.m_ID_cache.Add(row[0].ToString(), new C_ID_Cache(0, 0, true));
                                continue;
                            }
                            if (this.m_ID_cache.ContainsKey(row[0].ToString()))
                            {
                                this.m_ID_cache[row[0].ToString()].Id = Convert.ToInt32(row[1]);
                                if (row[2] == Convert.DBNull)
                                {
                                    this.m_ID_cache[row[0].ToString()].StatsID = 0;
                                }
                                else
                                {
                                    this.m_ID_cache[row[0].ToString()].StatsID = Convert.ToInt32(row[2]);
                                }
                            }
                            else
                            {
                                if (row[2] == Convert.DBNull)
                                {
                                    this.m_ID_cache.Add(row[0].ToString(), new C_ID_Cache(0, Convert.ToInt32(row[1]), true));
                                }
                                else
                                {
                                    this.m_ID_cache.Add(row[0].ToString(), new C_ID_Cache(Convert.ToInt32(row[2]), Convert.ToInt32(row[1]), true));
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "Error in UpdateCacheID: ");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "UpdateIDCache: " + c);
            }
        }

        private void WelcomeStats(string strSpeaker)
        {
            List<string> result = new List<string>();
            if (this.m_enWelcomeStats == enumBoolYesNo.Yes)
            {
                if (this.m_enLogSTATS == enumBoolYesNo.Yes)
                {
                    string SQL = String.Empty;
                    string strMSG = String.Empty;
                    //Statsquery with KDR
                    //Rankquery
                    if (m_enRankingByScore == enumBoolYesNo.Yes)
                    {
                        if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                        {
                            SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths, SUM(tps.Suicide) AS Suicide, SUM(tps.TKs) AS TKs, tpr.rankScore AS rank, (SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank , SUM(tps.Playtime) AS Playtime, SUM(tps.Headshots) AS Headshots, 
                                    SUM(tps.Rounds) AS Rounds, MAX(tps.Killstreak) AS Killstreak, MAX(tps.Deathstreak) AS Deathstreak   
                                    FROM " + this.tbl_playerdata + @" tpd
                                    INNER JOIN " + this.tbl_playerrank + @" tpr ON tpd.PlayerID = tpr.PlayerID AND tpr.ServerGroup = @ServerGroup 
                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                    INNER JOIN " + this.tbl_playerstats + @" tps ON tsp.StatsID = tps.StatsID
                                    WHERE tpd.SoldierName = @SoldierName
                                    GROUP BY tpd.PlayerID";
                        }
                        else
                        {
                            SQL = @"SELECT tpd.SoldierName AS SoldierName, tps.Score AS Score, tps.Kills AS Kills, tps.Deaths AS Deaths, tps.Suicide AS Suicide, tps.TKs AS TKs, tps.rankScore AS rank, (SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank ,
                                    tps.Playtime AS Playtime, tps.Headshots AS Headshots, tps.Rounds AS Rounds, tps.Killstreak AS Killstreak, tps.Deathstreak AS Deathstreak
                                    FROM " + this.tbl_playerdata + @" tpd
                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                    INNER JOIN " + this.tbl_playerstats + @" tps ON tps.StatsID = tsp.StatsID
                                    WHERE  tsp.ServerID = @ServerID AND tpd.SoldierName = @SoldierName";
                        }
                    }
                    else
                    {
                        if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                        {
                            SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths, SUM(tps.Suicide) AS Suicide, SUM(tps.TKs) AS TKs, tpr.rankKills AS rank, (SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank , SUM(tps.Playtime) AS Playtime, SUM(tps.Headshots) AS Headshots, 
                                    SUM(tps.Rounds) AS Rounds, MAX(tps.Killstreak) AS Killstreak, MAX(tps.Deathstreak) AS Deathstreak   
                                    FROM " + this.tbl_playerdata + @" tpd
                                    INNER JOIN " + this.tbl_playerrank + @" tpr ON tpd.PlayerID = tpr.PlayerID AND tpr.ServerGroup = @ServerGroup 
                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                    INNER JOIN " + this.tbl_playerstats + @" tps ON tsp.StatsID = tps.StatsID
                                    WHERE tpd.SoldierName = @SoldierName
                                    GROUP BY tpd.PlayerID";
                        }
                        else
                        {
                            SQL = @"SELECT tpd.SoldierName AS SoldierName, tps.Score AS Score, tps.Kills AS Kills, tps.Deaths AS Deaths, tps.Suicide AS Suicide, tps.TKs AS TKs, tps.rankKills AS rank, (SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank ,
                                    tps.Playtime AS Playtime, tps.Headshots AS Headshots, tps.Rounds AS Rounds, tps.Killstreak AS Killstreak, tps.Deathstreak AS Deathstreak
                                    FROM " + this.tbl_playerdata + @" tpd
                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                    INNER JOIN " + this.tbl_playerstats + @" tps ON tps.StatsID = tsp.StatsID
                                    WHERE  tsp.ServerID = @ServerID AND tpd.SoldierName = @SoldierName";
                        }
                    }
                    using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                    {
                        DataTable resultTable;
                        double kdr = 0;
                        if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                        {
                            MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                            MyCommand.Parameters.AddWithValue("@SoldierName", strSpeaker);
                        }
                        else
                        {
                            MyCommand.Parameters.AddWithValue("@ServerID", ServerID);
                            MyCommand.Parameters.AddWithValue("@SoldierName", strSpeaker);
                        }
                        try
                        {
                            resultTable = this.SQLquery(MyCommand);
                            if (resultTable.Rows != null)
                            {
                                foreach (DataRow row in resultTable.Rows)
                                {
                                    result = new List<string>(this.m_lstPlayerWelcomeStatsMessage);
                                    result = this.ListReplace(result, "%serverName%", this.serverName);
                                    result = this.ListReplace(result, "%playerName%", row["SoldierName"].ToString());
                                    result = this.ListReplace(result, "%playerScore%", row["Score"].ToString());
                                    result = this.ListReplace(result, "%playerKills%", row["Kills"].ToString());
                                    result = this.ListReplace(result, "%playerDeaths%", row["Deaths"].ToString());
                                    result = this.ListReplace(result, "%playerSuicide%", row["Suicide"].ToString());
                                    result = this.ListReplace(result, "%playerTKs%", row["TKs"].ToString());
                                    result = this.ListReplace(result, "%playerRank%", row["rank"].ToString());
                                    result = this.ListReplace(result, "%allRanks%", row["allrank"].ToString());
                                    result = this.ListReplace(result, "%playerHeadshots%", row["Headshots"].ToString());
                                    result = this.ListReplace(result, "%rounds%", row["Rounds"].ToString());
                                    result = this.ListReplace(result, "%killstreak%", row["Killstreak"].ToString());
                                    result = this.ListReplace(result, "%deathstreak%", row["Deathstreak"].ToString());
                                    //KDR
                                    if (Convert.ToInt32(row["Deaths"]) != 0)
                                    {
                                        kdr = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Deaths"]);
                                        kdr = Math.Round(kdr, 2);
                                        result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                    }
                                    else
                                    {
                                        kdr = Convert.ToDouble(row["Kills"]);
                                        result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                    }
                                    //Playtime
                                    TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(row["Playtime"]));
                                    result = this.ListReplace(result, "%playerPlaytime%", span.ToString());
                                    //SPM
                                    double SPM;
                                    if (Convert.ToDouble(row["Playtime"]) != 0)
                                    {
                                        SPM = (Convert.ToDouble(row["Score"]) / (Convert.ToDouble(row["Playtime"]) / 60));
                                        SPM = Math.Round(SPM, 2);
                                        result = this.ListReplace(result, "%SPM%", SPM.ToString());
                                    }
                                    else
                                    {
                                        result = this.ListReplace(result, "%SPM%", "0");
                                    }
                                }
                            }
                        }
                        catch (Exception c)
                        {
                            this.DebugInfo("Error", "WelcomeStats: " + c);
                        }
                    }
                    if (result.Count > 0)
                    {
                        //result.Insert(0, m_strPlayerWelcomeMsg.Replace("%serverName%", this.serverName).Replace("%playerName%", strSpeaker));
                    }
                    else
                    {
                        result.Clear();
                        result = new List<string>(this.m_lstNewPlayerWelcomeMsg);
                        result = this.ListReplace(result, "%serverName%", this.serverName);
                        result = this.ListReplace(result, "%playerName%", strSpeaker);
                        //result.Add(m_strNewPlayerWelcomeMsg.Replace("%serverName%", this.serverName).Replace("%playerName%", strSpeaker));
                    }
                    this.SendMultiLineChatMessage(result, int_welcomeStatsDelay, 0, "player", strSpeaker);
                }
            }
        }

        private void GetPlayerStats(string strSpeaker, int delay, string scope)
        {
            List<string> result = new List<string>();
            if (this.m_enLogSTATS == enumBoolYesNo.Yes)
            {
                string SQL = String.Empty;
                string strMSG = String.Empty;
                double kdr = 0;
                //Statsquery with KDR
                //Rankquery
                if (m_enRankingByScore == enumBoolYesNo.Yes)
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths, SUM(tps.Suicide) AS Suicide, SUM(tps.TKs) AS TKs, tpr.rankScore AS rank, (SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank , SUM(tps.Playtime) AS Playtime, SUM(tps.Headshots) AS Headshots, 
                                SUM(tps.Rounds) AS Rounds, MAX(tps.Killstreak) AS Killstreak, MAX(tps.Deathstreak) AS Deathstreak   
                                FROM " + this.tbl_playerdata + @" tpd
                                INNER JOIN " + this.tbl_playerrank + @" tpr ON tpd.PlayerID = tpr.PlayerID AND tpr.ServerGroup = @ServerGroup 
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                INNER JOIN " + this.tbl_playerstats + @" tps ON tsp.StatsID = tps.StatsID
                                WHERE tpd.SoldierName = @SoldierName
                                GROUP BY tpd.PlayerID";
                    }
                    else
                    {
                        SQL = @"SELECT tpd.SoldierName AS SoldierName, tps.Score AS Score, tps.Kills AS Kills, tps.Deaths AS Deaths, tps.Suicide AS Suicide, tps.TKs AS TKs, tps.rankScore AS rank, (SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank ,
                                tps.Playtime AS Playtime, tps.Headshots AS Headshots, tps.Rounds AS Rounds, tps.Killstreak AS Killstreak, tps.Deathstreak AS Deathstreak
                                FROM " + this.tbl_playerdata + @" tpd
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                INNER JOIN " + this.tbl_playerstats + @" tps ON tps.StatsID = tsp.StatsID
                                WHERE  tsp.ServerID = @ServerID AND tpd.SoldierName = @SoldierName";
                    }
                }
                else
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths, SUM(tps.Suicide) AS Suicide, SUM(tps.TKs) AS TKs, tpr.rankKills AS rank, (SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank , SUM(tps.Playtime) AS Playtime, SUM(tps.Headshots) AS Headshots, 
                                SUM(tps.Rounds) AS Rounds, MAX(tps.Killstreak) AS Killstreak, MAX(tps.Deathstreak) AS Deathstreak   
                                FROM " + this.tbl_playerdata + @" tpd
                                INNER JOIN " + this.tbl_playerrank + @" tpr ON tpd.PlayerID = tpr.PlayerID AND tpr.ServerGroup = @ServerGroup 
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                INNER JOIN " + this.tbl_playerstats + @" tps ON tsp.StatsID = tps.StatsID
                                WHERE tpd.SoldierName = @SoldierName
                                GROUP BY tpd.PlayerID";
                    }
                    else
                    {
                        SQL = @"SELECT tpd.SoldierName AS SoldierName, tps.Score AS Score, tps.Kills AS Kills, tps.Deaths AS Deaths, tps.Suicide AS Suicide, tps.TKs AS TKs, tps.rankKills AS rank, (SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank ,
                                tps.Playtime AS Playtime, tps.Headshots AS Headshots, tps.Rounds AS Rounds, tps.Killstreak AS Killstreak, tps.Deathstreak AS Deathstreak
                                FROM " + this.tbl_playerdata + @" tpd
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.PlayerID = tpd.PlayerID
                                INNER JOIN " + this.tbl_playerstats + @" tps ON tps.StatsID = tsp.StatsID
                                WHERE  tsp.ServerID = @ServerID AND tpd.SoldierName = @SoldierName";
                    }
                }
                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    DataTable resultTable;
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                        MyCommand.Parameters.AddWithValue("@SoldierName", strSpeaker);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", ServerID);
                        MyCommand.Parameters.AddWithValue("@SoldierName", strSpeaker);
                    }
                    try
                    {
                        resultTable = this.SQLquery(MyCommand);
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                result = new List<string>(m_lstPlayerStatsMessage);
                                result = this.ListReplace(result, "%playerName%", row["SoldierName"].ToString());
                                result = this.ListReplace(result, "%playerScore%", row["Score"].ToString());
                                result = this.ListReplace(result, "%playerKills%", row["Kills"].ToString());
                                result = this.ListReplace(result, "%playerDeaths%", row["Deaths"].ToString());
                                result = this.ListReplace(result, "%playerSuicide%", row["Suicide"].ToString());
                                result = this.ListReplace(result, "%playerTKs%", row["TKs"].ToString());
                                result = this.ListReplace(result, "%playerRank%", row["rank"].ToString());
                                result = this.ListReplace(result, "%allRanks%", row["allrank"].ToString());
                                result = this.ListReplace(result, "%playerHeadshots%", row["Headshots"].ToString());
                                result = this.ListReplace(result, "%rounds%", row["Rounds"].ToString());
                                result = this.ListReplace(result, "%killstreak%", row["Killstreak"].ToString());
                                result = this.ListReplace(result, "%deathstreak%", row["Deathstreak"].ToString());
                                //KDR
                                if (Convert.ToInt32(row["Deaths"]) != 0)
                                {
                                    kdr = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Deaths"]);
                                    kdr = Math.Round(kdr, 2);
                                    result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                }
                                else
                                {
                                    kdr = Convert.ToDouble(row["Kills"]);
                                    result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                }
                                //Playtime
                                TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(row["Playtime"]));
                                result = this.ListReplace(result, "%playerPlaytime%", span.ToString());
                                //SPM
                                double SPM;
                                if (Convert.ToDouble(row["Playtime"]) != 0)
                                {
                                    SPM = (Convert.ToDouble(row["Score"]) / (Convert.ToDouble(row["Playtime"]) / 60));
                                    SPM = Math.Round(SPM, 2);
                                    result = this.ListReplace(result, "%SPM%", SPM.ToString());
                                }
                                else
                                {
                                    result = this.ListReplace(result, "%SPM%", "0");
                                }
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetPlayerStats: " + c);
                    }
                }
                if (result.Count != 0)
                {
                    this.SendMultiLineChatMessage(result, delay,0 , scope, strSpeaker);
                }
                else
                {
                    result.Clear();
                    result.Add("No Stats are available yet! Please wait one Round!");
                    this.SendMultiLineChatMessage(result, delay,0 , scope, strSpeaker);
                }
            }
        }

        private void GetTop10(string strSpeaker, int delay, string scope)
        {
            List<string> result = new List<string>();
            if (this.m_enTop10ingame == enumBoolYesNo.Yes)
            {
                string SQL = String.Empty;
                int rank = 0;
                //Top10 Query
                if (this.m_enRankingByScore == enumBoolYesNo.Yes)
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths , SUM(tps.Headshots) AS Headshots  
                             FROM " + this.tbl_playerstats + @" tps
                             INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID
                             INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                             INNER JOIN " + this.tbl_playerrank + @" tpr ON tpr.PlayerID = tsp.PlayerID
                             WHERE tpr.ServerGroup = @ServerGroup AND tpr.rankScore BETWEEN 1 AND 10
                             GROUP BY tsp.PlayerID 
                             ORDER BY tpr.rankScore ASC";
                    }
                    else
                    {
                        SQL = @"SELECT tpd.SoldierName, tps.Score, tps.Kills, tps.Deaths, tps.Headshots 
                             FROM " + this.tbl_playerstats + @" tps
                             INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID
                             INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                             WHERE tsp.ServerID = @ServerID AND tps.rankScore BETWEEN 1 AND 10
                             ORDER BY tps.rankScore ASC";
                    }
                }
                else
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpd.SoldierName, SUM(tps.Score) AS Score, SUM(tps.Kills) AS Kills, SUM(tps.Deaths) AS Deaths , SUM(tps.Headshots) AS Headshots  
                             FROM " + this.tbl_playerstats + @" tps
                             INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID
                             INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                             INNER JOIN " + this.tbl_playerrank + @" tpr ON tpr.PlayerID = tsp.PlayerID
                             WHERE tpr.ServerGroup = @ServerGroup AND tpr.rankKills BETWEEN 1 AND 10
                             GROUP BY tsp.PlayerID 
                             ORDER BY tpr.rankKills ASC";
                    }
                    else
                    {
                        SQL = @"SELECT tpd.SoldierName, tps.Score, tps.Kills, tps.Deaths, tps.Headshots 
                             FROM " + this.tbl_playerstats + @" tps
                             INNER JOIN " + this.tbl_server_player + @" tsp ON  tsp.StatsID = tps.StatsID
                             INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                             WHERE tsp.ServerID = @ServerID AND tps.rankKills BETWEEN 1 AND 10 
                             ORDER BY tps.rankKills ASC";
                    }
                }
                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    }
                    DataTable resultTable;
                    try
                    {
                        resultTable = this.SQLquery(MyCommand);
                        result = new List<string>();
                        //Top 10 Header
                        result.Add(this.m_strTop10Header.Replace("%serverName%", this.serverName));
                        StringBuilder Top10Row = new StringBuilder();
                        double kdr1;
                        double khr;
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                Top10Row.Append(this.m_strTop10RowFormat);

                                if (Convert.ToDouble(row["Deaths"]) != 0)
                                {
                                    kdr1 = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Deaths"]);
                                    Top10Row.Replace("%playerKDR%", Math.Round(kdr1, 2).ToString());
                                }
                                else
                                {
                                    Top10Row.Replace("%playerKDR%", Convert.ToDouble(row["Kills"]).ToString());
                                }

                                if (Convert.ToDouble(row["Headshots"]) != 0)
                                {
                                    khr = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Headshots"]);
                                    khr = Math.Round(khr, 4);
                                    khr = khr * 100;
                                    Top10Row.Replace("%playerKHR%", khr.ToString());
                                }
                                else
                                {
                                    khr = 0;
                                }
                                rank = rank + 1;
                                Top10Row.Replace("%Rank%", rank.ToString()).Replace("%playerName%", row["SoldierName"].ToString()).Replace("%playerScore%", row["Score"].ToString()).Replace("%playerKills%", row["Kills"].ToString());
                                Top10Row.Replace("%playerDeaths%", row["Deaths"].ToString()).Replace("%playerHeadshots%", row["Headshots"].ToString());
                                result.Add(Top10Row.ToString());
                                Top10Row.Length = 0;
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetTop10: " + c);
                    }
                }
                if (result.Count > 0)
                {
                    this.SendMultiLineChatMessage(result, 0, delay, scope, strSpeaker);
                }
            }
        }

        private void GetWeaponStats(string strWeapon, string strPlayer, string scope)
        {

            this.DebugInfo("Trace", "GetWeaponStats: " + strPlayer + " " + strWeapon);
            int delay = 0;
            string SQL = String.Empty;
            List<string> result = new List<string>();

            if (this.DamageClass.ContainsKey(strWeapon) == true)
            //if (this.WeaponMappingDic.ContainsKey(strWeapon) == true)
            {
                if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                {
                    SQL = @"SELECT `Kills`, `Headshots`, `Deaths`, rank, (SELECT COUNT(DISTINCT PlayerID) FROM " + this.tbl_server_player + @" tsp INNER JOIN " + this.tbl_server + @" ts  ON ts.ServerID = tsp.ServerID AND ts.ServerGroup = @ServerGroup) AS allrank
                            FROM (SELECT sub.PlayerID, (@num := @num + 1) AS rank, `Kills`, `Headshots`, `Deaths`
                                 FROM
                                 (SELECT tsp.PlayerID, SUM(`Kills`) AS `Kills`, SUM(`Headshots`) AS `Headshots`, SUM(`Deaths`) AS `Deaths`
                                 FROM " + this.tbl_weapons_stats + @" tw
                                 INNER JOIN " + this.tbl_server_player + @" tsp ON tw.StatsID = tsp.StatsID 
                                 INNER JOIN " + this.tbl_server + @" tserver ON tsp.ServerID = tserver.ServerID AND tserver.ServerGroup = @ServerGroup ,(SELECT @num := 0) x 
                                 WHERE tw.WeaponID = @WeaponID
                                 GROUP BY tsp.PlayerID
                                 ORDER BY `Kills` DESC, `Headshots` DESC) sub )sub2
                            INNER JOIN " + this.tbl_playerdata + @" tpd ON tpd.PlayerID = sub2.PlayerID
                            WHERE tpd.SoldierName = @SoldierName AND tpd.GameID = @GameID LIMIT 1";
                }
                else
                {
                    SQL = @"SELECT 
                            Kills,
                            Headshots,
                            Deaths,
                            (SELECT 
                                    rank
                                FROM
                                    (SELECT 
                                        @rownum:=@rownum + 1 AS rank, sub.PlayerID
                                    FROM
                                        (SELECT @rownum:=0) r, (SELECT 
                                        tsp.PlayerID
                                    FROM
                                         " + this.tbl_weapons_stats + @" tw
                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.ServerID = @ServerID
                                        AND tw.StatsID = tsp.StatsID
                                    WHERE
                                        tw.WeaponID = @WeaponID
                                    ORDER BY tw.Kills DESC , tw.Headshots DESC) sub) sub2
                                WHERE
                                    PlayerID = tpd.PlayerID
                                LIMIT 1) AS rank,
                            (SELECT 
                                    COUNT(*)
                                FROM
                                    " + this.tbl_server_player + @"
                                WHERE
                                    ServerID = @ServerID) AS allrank
                        FROM
                            " + this.tbl_weapons_stats + @" tw
                                INNER JOIN
                            " + this.tbl_server_player + @" tsp ON tw.StatsID = tsp.StatsID
                                AND tsp.ServerID = @ServerID
                                INNER JOIN
                            " + this.tbl_playerdata + @" tpd ON tpd.PlayerID = tsp.PlayerID
                        WHERE
                            tpd.SoldierName = @SoldierName
                                AND tpd.GameID = @GameID
                                AND WeaponID = @WeaponID
                        LIMIT 1";
                }

                this.DebugInfo("Trace", "GetWeaponStats: Query:" + SQL);
                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    //MyCommand.Parameters.AddWithValue("@WeaponID", this.weaponDic[this.DamageClass[strWeapon]][strWeapon].Name);
                    MyCommand.Parameters.AddWithValue("@WeaponID", this.WeaponMappingDic[this.weaponDic[this.DamageClass[strWeapon]][strWeapon].Name]);
                    MyCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@SoldierName", strPlayer);
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                        MyCommand.Parameters.AddWithValue("@SoldierName", strPlayer);
                    }
                    try
                    {
                        DataTable resultTable = this.SQLquery(MyCommand);
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                result = new List<string>(this.m_lstWeaponstatsMsg);
                                if (row[0] != Convert.DBNull || row[1] != Convert.DBNull || row[2] != Convert.DBNull)
                                {
                                    result = this.ListReplace(result, "%playerKills%", row[0].ToString());
                                    result = this.ListReplace(result, "%playerHeadshots%", row[1].ToString());
                                    result = this.ListReplace(result, "%playerDeaths%", row[2].ToString());
                                    result = this.ListReplace(result, "%playerRank%", row[3].ToString());
                                    result = this.ListReplace(result, "%allRanks%", row[4].ToString());

                                    double khr = 0;
                                    if (Convert.ToDouble(row[0]) != 0)
                                    {
                                        khr = Convert.ToDouble(row[1]) / Convert.ToDouble(row[0]);
                                        khr = Math.Round(khr, 2);
                                        khr = khr * 100;
                                    }
                                    else
                                    {
                                        khr = 0;
                                    }
                                    double kdr = 0;
                                    if (Convert.ToDouble(row[2]) != 0)
                                    {
                                        kdr = Convert.ToDouble(row[0]) / Convert.ToDouble(row[2]);
                                        kdr = Math.Round(kdr, 2);
                                    }
                                    else
                                    {
                                        kdr = Convert.ToDouble(row[2]);
                                    }
                                    result = this.ListReplace(result, "%playerKHR%", khr.ToString());
                                    result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                }
                                else
                                {
                                    result.Clear();
                                }
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetWeaponStats: " + c);
                    }
                }
                result = this.ListReplace(result, "%playerName%", strPlayer);
                result = this.ListReplace(result, "%Weapon%", this.weaponDic[this.DamageClass[strWeapon]][strWeapon].FieldName);
                if (result.Count > 0)
                {
                    this.SendMultiLineChatMessage(result, delay, 0, scope, strPlayer);
                }
                else
                {
                    result.Clear();
                    result.Add("No Stats are available for this Weapon!!!");
                    this.SendMultiLineChatMessage(result, delay, 0, scope, strPlayer);
                }
            }
            else
            {
                result.Clear();
                result.Add("Specific Weapon not found!!");
                this.SendMultiLineChatMessage(result, delay, 0, scope, strPlayer);
            }
        }

        private void GetWeaponTop10(string strWeapon, string strPlayer, int delay, string scope)
        {
            this.DebugInfo("Trace", "GetWeaponTop10: strWeapon = " + strWeapon);
            int delaytop10 = 0;
            double kdr = 0;
            double khr = 0;
            int rank = 0;
            string SQL = String.Empty;
            List<string> result = new List<string>();
            if (this.DamageClass.ContainsKey(strWeapon) == true)
            {
                //string tbl_weapons = "tbl_weapons_" + this.DamageClass[strWeapon].ToLower() + this.tableSuffix;
                if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                {
                    SQL = @"SELECT tpd.SoldierName, SUM(`Kills`) AS `Kills`, SUM(`Headshots`) AS `Headshots`, SUM(`Deaths`) AS `Deaths`
							FROM " + this.tbl_weapons_stats + @" tw
							INNER JOIN " + this.tbl_server_player + @" tsp ON tw.StatsID = tsp.StatsID
							INNER JOIN " + this.tbl_playerdata + @" tpd ON tpd.PlayerID = tsp.PlayerID
                            INNER JOIN " + this.tbl_server + @" ts ON ts.ServerID = tsp.ServerID
                            WHERE ts.ServerGroup = @ServerGroup AND tw.WeaponID = @WeaponID
							GROUP BY tsp.PlayerID
							ORDER BY SUM(`Kills`) DESC, SUM(`Headshots`) DESC
							LIMIT 10";
                }
                else
                {
                    SQL = @"SELECT tpd.SoldierName, `Kills`, `Headshots`, `Deaths`
							FROM " + this.tbl_weapons_stats + @" tw
							INNER JOIN " + this.tbl_server_player + @" tsp ON tw.StatsID = tsp.StatsID
							INNER JOIN " + this.tbl_playerdata + @" tpd ON tpd.PlayerID = tsp.PlayerID
							WHERE tsp.ServerID = @ServerID AND tw.WeaponID = @WeaponID
							ORDER BY `Kills` DESC, `Headshots` DESC
							LIMIT 10";
                }

                //SQL = SQL.Replace("%Weapon%", this.weaponDic[this.DamageClass[strWeapon]][strWeapon].FieldName);

                this.DebugInfo("Trace", "GetWeaponTop10: Query:" + SQL);

                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    MyCommand.Parameters.AddWithValue("@WeaponID", this.WeaponMappingDic[this.weaponDic[this.DamageClass[strWeapon]][strWeapon].Name]);
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    }
                    DataTable resultTable;
                    try
                    {
                        result = new List<string>();
                        //result.Add("Top 10 Killers with %Weapon%");
                        result.Add(this.m_strWeaponTop10Header.Replace("%serverName%", this.serverName));
                        resultTable = this.SQLquery(MyCommand);
                        StringBuilder Top10Row = new StringBuilder(this.m_strWeaponTop10RowFormat);
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                if (Convert.ToDouble(row[3]) != 0)
                                {
                                    kdr = Convert.ToDouble(row[1]) / Convert.ToDouble(row[3]);
                                    kdr = Math.Round(kdr, 2);
                                }
                                else
                                {
                                    kdr = Convert.ToDouble(row[1]);
                                }
                                if (Convert.ToDouble(row[1]) != 0)
                                {
                                    khr = Convert.ToDouble(row[2]) / Convert.ToDouble(row[1]);
                                    khr = Math.Round(khr, 4);
                                    khr = khr * 100;
                                }
                                else
                                {
                                    khr = 0;
                                }
                                Top10Row.Length = 0;
                                Top10Row.Append(this.m_strWeaponTop10RowFormat);
                                rank = rank + 1;
                                Top10Row.Replace("%Rank%", rank.ToString()).Replace("%playerName%", row[0].ToString()).Replace("%playerKills%", row[1].ToString()).Replace("%playerHeadshots%", row[2].ToString()).Replace("%playerDeaths%", row[3].ToString()).Replace("%playerKHR%", khr.ToString());
                                result.Add(Top10Row.ToString());
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetWeaponTop10: " + c);
                    }
                }
                result = this.ListReplace(result, "%Player%", strPlayer);
                result = this.ListReplace(result, "%Weapon%", this.weaponDic[this.DamageClass[strWeapon]][strWeapon].FieldName);
                if (result.Count > 0)
                {
                    this.SendMultiLineChatMessage(result, 0, delay, scope, strPlayer);
                }
                else
                {
                    result.Clear();
                    result.Add("No Stats are available for this Weapon!!!");
                    this.SendMultiLineChatMessage(result, 0, delay, scope, strPlayer);
                }
            }
            else
            {
                result.Clear();
                result.Add("Specific Weapon not found!!");
                this.SendMultiLineChatMessage(result, 0, delay, scope, strPlayer);
            }
        }

        private void GetDogtags(string strPlayer, int delay, string scope)
        {
            int delaydogtags = 0;
            string SQL = String.Empty;
            string SQL2 = String.Empty;
            if (this.m_enOverallRanking == enumBoolYesNo.Yes)
            {
                SQL = @"SELECT pd.SoldierName, SUM(dt.Count) AS Count
                        FROM " + this.tbl_server_player + @" sp
                        INNER JOIN " + this.tbl_server + @" ts ON ts.ServerID = sp.ServerID AND ts.ServerGroup = @ServerGroup 
                        INNER JOIN " + this.tbl_dogtags + @" dt ON sp.StatsID = dt.VictimID 
                        INNER JOIN " + this.tbl_playerdata + @" pd ON sp.PlayerID = pd.PlayerID
                        WHERE KillerID IN (SELECT StatsID AS KillerID FROM " + this.tbl_server_player + @" WHERE PlayerID = @KillerID)
                        GROUP BY pd.PlayerID ORDER BY Count DESC Limit 3";

                SQL2 = @"SELECT pd.SoldierName, SUM(dt.Count) AS Count
                         FROM " + this.tbl_server_player + @"  sp
                         INNER JOIN " + this.tbl_server + @" ts ON ts.ServerID = sp.ServerID AND ts.ServerGroup = @ServerGroup 
                         INNER JOIN " + this.tbl_dogtags + @"  dt ON sp.StatsID = dt.KillerID 
                         INNER JOIN " + this.tbl_playerdata + @"  pd ON sp.PlayerID = pd.PlayerID
                         WHERE VictimID IN (SELECT StatsID AS VictimID FROM " + this.tbl_server_player + @"  WHERE PlayerID = @VictimID)
                         GROUP BY pd.PlayerID ORDER BY Count DESC Limit 3";
            }
            else
            {
                SQL = @"SELECT pd.SoldierName, dt.Count
                        FROM " + this.tbl_server_player + @" sp
                        INNER JOIN " + this.tbl_dogtags + @" dt ON sp.StatsID = dt.VictimID 
                        INNER JOIN " + this.tbl_playerdata + @" pd ON sp.PlayerID = pd.PlayerID
                        WHERE KillerID = @KillerID AND sp.ServerID = @ServerID
                        ORDER BY Count DESC Limit 3";

                SQL2 = @"SELECT pd.SoldierName, dt.Count
                         FROM " + this.tbl_server_player + @" sp
                         INNER JOIN " + this.tbl_dogtags + @" dt ON sp.StatsID = dt.KillerID 
                         INNER JOIN " + this.tbl_playerdata + @" pd ON sp.PlayerID = pd.PlayerID
                         WHERE VictimID = @VictimID AND sp.ServerID = @ServerID
                         ORDER BY Count DESC Limit 3";
            }

            List<string> result = new List<string>();
            List<string> result2 = new List<string>();

            if (this.StatsTracker.ContainsKey(strPlayer) == false)
            {
                return;
            }

            using (MySqlCommand MyCommand = new MySqlCommand(SQL))
            {
                if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                {
                    MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    MyCommand.Parameters.AddWithValue("@KillerID", this.GetID(this.StatsTracker[strPlayer].EAGuid).Id);
                }
                else
                {
                    MyCommand.Parameters.AddWithValue("@KillerID", this.GetID(this.StatsTracker[strPlayer].EAGuid).StatsID);
                    MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                }
                try
                {
                    result = new List<string>();
                    result.Add("Your favorite Victims:");
                    DataTable resultTable = this.SQLquery(MyCommand);
                    if (resultTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in resultTable.Rows)
                        {
                            result.Add(" " + row["Count"] + "x  " + row["SoldierName"]);
                        }
                    }
                    else
                    {
                        result.Add("None - Get some dogtags!!");
                    }
                    resultTable.Dispose();
                }
                catch (Exception c)
                {
                    this.DebugInfo("Error", "GetDogtags: " + c);
                }
            }
            using (MySqlCommand MyCommand = new MySqlCommand(SQL2))
            {
                if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                {
                    MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    MyCommand.Parameters.AddWithValue("@VictimID", this.GetID(this.StatsTracker[strPlayer].EAGuid).Id);
                }
                else
                {
                    MyCommand.Parameters.AddWithValue("@VictimID", this.GetID(this.StatsTracker[strPlayer].EAGuid).StatsID);
                    MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                }
                try
                {
                    result2 = new List<string>();
                    result2.Add("Your worst Enemies:");
                    DataTable resultTable = this.SQLquery(MyCommand);
                    if (resultTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in resultTable.Rows)
                        {
                            result2.Add(" " + row["Count"] + "x  " + row["SoldierName"]);
                        }
                    }
                    else
                    {
                        result2.Add("Nobody got your Tag yet!");
                    }
                    resultTable.Dispose();
                }
                catch (Exception c)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "Error in GetDogtags: " + c);
                }
            }
            this.CloseMySqlConnection(1);
            result.AddRange(result2);
            if (result[0].Equals("0") == false)
            {
                this.SendMultiLineChatMessage(result, delaydogtags, delay, scope, strPlayer);
            }
            else
            {
                result.Clear();
                result.Add("No Stats are available!!!");
                this.SendMultiLineChatMessage(result, delaydogtags, delay, scope, strPlayer);
            }
        }

        private void GetPlayerOfTheDay(string strSpeaker, int delay, string scope)
        {
            List<string> result = new List<string>();
            if (this.m_enLogSTATS == enumBoolYesNo.Yes)
            {
                string SQL = string.Empty;

                string SQL_SELECT = @"SELECT 
                                tpd.SoldierName AS SoldierName,
                                SUM(ts.Score) AS Score, 
                                SUM(ts.Kills) AS Kills,
                                SUM(ts.Headshots) AS Headshots,
                                SUM(ts.Deaths) AS Deaths, 
                                SUM(ts.TKs) AS TKs,
                                SUM(ts.Suicide) AS Suicide,
                                SUM(ts.RoundCount ) AS RoundCount,
                                SUM(ts.Playtime) AS Playtime,
                                MAX(ts.Killstreak) AS Killstreak,
                                MAX(ts.Deathstreak) AS Deathstreak,
                                MAX(ts.HighScore) AS HighScore,
                                SUM(ts.Wins ) AS Wins,
                                SUM(ts.Losses ) AS Losses, ";


                string SQL_JOINS = @" FROM " + this.tbl_sessions + @" ts 
                                      INNER JOIN " + this.tbl_server_player + @" tsp USING(StatsID)
                                      INNER JOIN " + this.tbl_playerdata + @" tpd USING(PlayerID) ";

                string SQL_CONDS = string.Empty;

                string strMSG = String.Empty;
                double kdr = 0;
                //Statsquery with KDR
                //Rankquery
                if (m_enRankingByScore == enumBoolYesNo.Yes)
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        //Ranking by Score overall Server
                        SQL_SELECT = SQL_SELECT + @"(SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank, tpr.rankScore AS rank ";

                        SQL_JOINS = SQL_JOINS + @" INNER JOIN " + this.tbl_server + @" ts2 USING(ServerID) 
                                                   INNER JOIN " + this.tbl_playerrank + @" tpr USING(PlayerID)";

                        SQL_CONDS = @" WHERE ts.StartTime >= CURRENT_DATE() AND ts2.ServerGroup = @ServerGroup
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Score) DESC ";
                    }
                    else
                    {
                        //Ranking by Score specfic Server
                        SQL_SELECT = SQL_SELECT + @"(SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank, tps.rankScore AS rank ";

                        SQL_JOINS = SQL_JOINS + @" INNER JOIN " + this.tbl_playerstats + @" tps USING(StatsID) ";

                        SQL_CONDS = @" WHERE ts.StartTime >= CURRENT_DATE() AND tsp.ServerID = @ServerID
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Score) DESC ";
                    }
                }
                else
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {

                        //Ranking by Kills overall Server
                        SQL_SELECT = SQL_SELECT + @"(SELECT SUM(tss.CountPlayers) FROM " + this.tbl_server_stats + @" tss INNER JOIN " + this.tbl_server + @" ts ON tss.ServerID = ts.ServerID AND ServerGroup = @ServerGroup GROUP BY ts.ServerGroup ) AS allrank, tpr.rankKills AS rank ";

                        SQL_JOINS = SQL_JOINS + @" INNER JOIN " + this.tbl_server + @" ts2 USING(ServerID)  
                                                   INNER JOIN " + this.tbl_playerrank + @" tpr USING(PlayerID)";

                        SQL_CONDS = @" WHERE ts.StartTime >= CURRENT_DATE() AND ts2.ServerGroup = @ServerGroup
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Kills) DESC, SUM(ts.Deaths) ASC ";
                    }
                    else
                    {
                        //Ranking by Kills specfic Server
                        SQL_SELECT = SQL_SELECT + @"(SELECT tss.CountPlayers FROM " + this.tbl_server_stats + @" tss WHERE ServerID = @ServerID ) AS allrank , tps.rankKills AS rank  ";

                        SQL_JOINS = SQL_JOINS + @" INNER JOIN " + this.tbl_playerstats + @" tps USING(StatsID) ";

                        SQL_CONDS = @" WHERE ts.StartTime >= CURRENT_DATE() AND tsp.ServerID = @ServerID
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Kills) DESC, SUM(ts.Deaths) ASC ";
                    }
                }
                //Add LIMIT
                SQL = SQL_SELECT + SQL_JOINS + SQL_CONDS + @" LIMIT 1";
                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    DataTable resultTable;
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    }
                    try
                    {
                        resultTable = this.SQLquery(MyCommand);
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                result = new List<string>(m_lstPlayerOfTheDayMessage);
                                result = this.ListReplace(result, "%playerName%", row["SoldierName"].ToString());
                                result = this.ListReplace(result, "%playerScore%", row["Score"].ToString());
                                result = this.ListReplace(result, "%playerKills%", row["Kills"].ToString());
                                result = this.ListReplace(result, "%playerDeaths%", row["Deaths"].ToString());
                                result = this.ListReplace(result, "%playerSuicide%", row["Suicide"].ToString());
                                result = this.ListReplace(result, "%playerTKs%", row["TKs"].ToString());
                                result = this.ListReplace(result, "%playerRank%", row["rank"].ToString());
                                result = this.ListReplace(result, "%allRanks%", row["allrank"].ToString());
                                result = this.ListReplace(result, "%playerHeadshots%", row["Headshots"].ToString());
                                result = this.ListReplace(result, "%rounds%", row["RoundCount"].ToString());
                                result = this.ListReplace(result, "%killstreak%", row["Killstreak"].ToString());
                                result = this.ListReplace(result, "%deathstreak%", row["Deathstreak"].ToString());
                                //KDR
                                if (Convert.ToInt32(row["Deaths"]) != 0)
                                {
                                    kdr = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Deaths"]);
                                    kdr = Math.Round(kdr, 2);
                                    result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                }
                                else
                                {
                                    kdr = Convert.ToDouble(row["Kills"]);
                                    result = this.ListReplace(result, "%playerKDR%", kdr.ToString());
                                }
                                //Playtime
                                TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(row["Playtime"]));
                                result = this.ListReplace(result, "%playerPlaytime%", span.ToString());
                                //SPM
                                double SPM;
                                if (Convert.ToDouble(row["Playtime"]) != 0)
                                {
                                    SPM = (Convert.ToDouble(row["Score"]) / (Convert.ToDouble(row["Playtime"]) / 60));
                                    SPM = Math.Round(SPM, 2);
                                    result = this.ListReplace(result, "%SPM%", SPM.ToString());
                                }
                                else
                                {
                                    result = this.ListReplace(result, "%SPM%", "0");
                                }
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetPlayerOfTheDay: " + c);
                    }
                }
                if (result.Count != 0)
                {
                    this.SendMultiLineChatMessage(result, delay, 0, scope, strSpeaker);
                }
                else
                {
                    result.Clear();
                    result.Add("No Stats are available yet! Please wait one Round!");
                    this.SendMultiLineChatMessage(result, delay, 0, scope, strSpeaker);
                }
            }
        }

        private void GetTop10ForPeriod(string strSpeaker, int delay, string scope, int intdays)
        {
            List<string> result = new List<string>();
            if (this.m_enTop10ingame == enumBoolYesNo.Yes)
            {
                string SQL = @"SELECT 
                                tpd.SoldierName AS SoldierName,
                                SUM(ts.Score) AS Score, 
                                SUM(ts.Kills) AS Kills,
                                SUM(ts.Headshots) AS Headshots,
                                SUM(ts.Deaths) AS Deaths, 
                                SUM(ts.TKs) AS TKs,
                                SUM(ts.Suicide) AS Suicide,
                                SUM(ts.RoundCount ) AS RoundCount,
                                SUM(ts.Playtime) AS Playtime
                                FROM " + this.tbl_sessions + @" ts 
                                INNER JOIN " + this.tbl_server_player + @" tsp USING(StatsID)
                                INNER JOIN " + this.tbl_playerdata + @" tpd USING(PlayerID) ";
                int rank = 0;
                //Top10 Query
                if (m_enRankingByScore == enumBoolYesNo.Yes)
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        //Ranking by Score overall Server
                        SQL = SQL + @" INNER JOIN " + this.tbl_server + @" ts2 USING(ServerID)
                                       WHERE ts.StartTime >= DATE_SUB(CURRENT_DATE(),INTERVAL @DAYS DAY) AND ts2.ServerGroup = @ServerGroup
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Score) DESC ";
                    }
                    else
                    {
                        //Ranking by Score specfic Server
                        SQL = SQL + @" WHERE ts.StartTime >= DATE_SUB(CURRENT_DATE(),INTERVAL @DAYS DAY) AND tsp.ServerID = @ServerID
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Score) DESC ";
                    }
                }
                else
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        //Ranking by Kills overall Server
                        SQL = SQL + @" INNER JOIN " + this.tbl_server + @" ts2 USING(ServerID)
                                       WHERE ts.StartTime >= DATE_SUB(CURRENT_DATE(),INTERVAL @DAYS DAY) AND ts2.ServerGroup = @ServerGroup
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Kills) DESC, SUM(ts.Deaths) ASC ";
                    }
                    else
                    {
                        //Ranking by Kills specfic Server
                        SQL = SQL + @" WHERE ts.StartTime >= DATE_SUB(CURRENT_DATE(),INTERVAL @DAYS DAY) AND tsp.ServerID = @ServerID
                                       Group BY tsp.StatsID
                                       ORDER BY SUM(ts.Kills) DESC, SUM(ts.Deaths) ASC ";
                    }
                }
                //Add LIMIT
                SQL = SQL + @" LIMIT 10";

                using (MySqlCommand MyCommand = new MySqlCommand(SQL))
                {
                    MyCommand.Parameters.AddWithValue("@DAYS", intdays);
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        MyCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        MyCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    }
                    DataTable resultTable;
                    try
                    {
                        resultTable = this.SQLquery(MyCommand);
                        result = new List<string>();
                        //Top 10 Header
                        result.Add(this.m_strTop10HeaderForPeriod.Replace("%serverName%", this.serverName).Replace("%intervaldays%", intdays.ToString()));
                        StringBuilder Top10Row = new StringBuilder();
                        double kdr1;
                        double khr;
                        if (resultTable.Rows != null)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                Top10Row.Append(this.m_strTop10RowFormat);

                                if (Convert.ToDouble(row["Deaths"]) != 0)
                                {
                                    kdr1 = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Deaths"]);
                                    Top10Row.Replace("%playerKDR%", Math.Round(kdr1, 2).ToString());
                                }
                                else
                                {
                                    Top10Row.Replace("%playerKDR%", Convert.ToDouble(row["Kills"]).ToString());
                                }

                                if (Convert.ToDouble(row["Headshots"]) != 0)
                                {
                                    khr = Convert.ToDouble(row["Kills"]) / Convert.ToDouble(row["Headshots"]);
                                    khr = Math.Round(khr, 4);
                                    khr = khr * 100;
                                    Top10Row.Replace("%playerKHR%", khr.ToString());
                                }
                                else
                                {
                                    khr = 0;
                                }
                                rank = rank + 1;
                                Top10Row.Replace("%Rank%", rank.ToString()).Replace("%playerName%", row["SoldierName"].ToString()).Replace("%playerScore%", row["Score"].ToString()).Replace("%playerKills%", row["Kills"].ToString());
                                Top10Row.Replace("%playerDeaths%", row["Deaths"].ToString()).Replace("%playerHeadshots%", row["Headshots"].ToString());
                                result.Add(Top10Row.ToString());
                                Top10Row.Length = 0;
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "GetTop10ForPeriod: " + c);
                    }
                }
                if (result.Count > 0)
                {
                    this.SendMultiLineChatMessage(result, 0, delay, scope, strSpeaker);
                }
            }
        }
        
        //Add to stats
        private void AddKillToStats(string strPlayerName, string DmgType, string weapon, bool headshot)
        {
            if (StatsTracker.ContainsKey(strPlayerName))
            {
                StatsTracker[strPlayerName].addKill(DmgType, weapon, headshot);
            }
            else
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(strPlayerName, newEntry);
                StatsTracker[strPlayerName].addKill(DmgType, weapon, headshot);
            }
            //Session
            if (m_dicSession.ContainsKey(strPlayerName) && this.m_sessionON == enumBoolYesNo.Yes)
            {
                m_dicSession[strPlayerName].addKill(DmgType, weapon, headshot);
            }
        }

        public void AddDeathToStats(string strPlayerName, string DmgType, string weapon)
        {
            if (StatsTracker.ContainsKey(strPlayerName))
            {
                StatsTracker[strPlayerName].addDeath(DmgType, weapon);
            }
            else
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(strPlayerName, newEntry);
                StatsTracker[strPlayerName].addDeath(DmgType, weapon);
            }

            //Session
            if (m_dicSession.ContainsKey(strPlayerName) && this.m_sessionON == enumBoolYesNo.Yes)
            {
                m_dicSession[strPlayerName].addDeath(DmgType, weapon);
            }
        }

        private void AddSuicideToStats(string strPlayerName, string DmgType, string weapon)
        {
            if (StatsTracker.ContainsKey(strPlayerName))
            {
                StatsTracker[strPlayerName].addDeath(DmgType, weapon);
                StatsTracker[strPlayerName].Suicides++;
            }
            else
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 1, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(strPlayerName, newEntry);
                StatsTracker[strPlayerName].addDeath(DmgType, weapon);
            }

            //Session
            if (m_dicSession.ContainsKey(strPlayerName) && this.m_sessionON == enumBoolYesNo.Yes)
            {
                m_dicSession[strPlayerName].addDeath(DmgType, weapon);
                m_dicSession[strPlayerName].Suicides++;
            }
        }

        private void AddTeamKillToStats(string strPlayerName)
        {
            if (StatsTracker.ContainsKey(strPlayerName))
            {
                StatsTracker[strPlayerName].Teamkills++;
            }
            else
            {
                CStats newEntry = new CStats(String.Empty, 0, 0, 0, 0, 0, 1, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(strPlayerName, newEntry);
            }

            //Session
            if (m_dicSession.ContainsKey(strPlayerName) && this.m_sessionON == enumBoolYesNo.Yes)
            {
                m_dicSession[strPlayerName].Teamkills++;
            }
        }
        //Misc
        private void AddPBInfoToStats(CPunkbusterInfo cpbiPlayer)
        {
            if (StatsTracker.ContainsKey(cpbiPlayer.SoldierName))
            {
                StatsTracker[cpbiPlayer.SoldierName].Guid = cpbiPlayer.GUID;
                if (cpbiPlayer.PlayerCountryCode.Length <= 2)
                {
                    StatsTracker[cpbiPlayer.SoldierName].PlayerCountryCode = cpbiPlayer.PlayerCountryCode;
                }
                else
                {
                    StatsTracker[cpbiPlayer.SoldierName].PlayerCountryCode = "--";
                }
                if (StatsTracker[cpbiPlayer.SoldierName].TimePlayerjoined == null)
                    StatsTracker[cpbiPlayer.SoldierName].TimePlayerjoined = MyDateTime.Now;
            }
            else
            {
                CStats newEntry = new CStats(cpbiPlayer.GUID, 0, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic);
                StatsTracker.Add(cpbiPlayer.SoldierName, newEntry);
                if (cpbiPlayer.PlayerCountryCode.Length <= 2)
                {
                    StatsTracker[cpbiPlayer.SoldierName].PlayerCountryCode = cpbiPlayer.PlayerCountryCode;
                }
                else
                {
                    StatsTracker[cpbiPlayer.SoldierName].PlayerCountryCode = "--";
                }
            }
        }

        private void OpenMySqlConnection(int type)
        {
            try
            {
                switch (type)
                {
                    //OdbcCon
                    case 1:

                        if (MySqlCon == null)
                        {
                            MySqlCon = new MySqlConnection(this.DBConnectionStringBuilder());
                            MySqlCon.Open();
                        }
                        if (MySqlCon.State == ConnectionState.Closed)
                        {
                            MySqlCon = new MySqlConnection(this.DBConnectionStringBuilder());
                            MySqlCon.Open();
                            //this.DebugInfo("Info", "MySqlCon was close Current State is open");
                        }

                        break;
                    //ODBCConn
                    case 2:
                        if (MySqlConn == null)
                        {
                            MySqlConn = new MySqlConnection(this.DBConnectionStringBuilder());
                            MySqlConn.Open();
                        }
                        if (MySqlConn.State == ConnectionState.Closed)
                        {
                            MySqlConn = new MySqlConnection(this.DBConnectionStringBuilder());
                            MySqlConn.Open();
                            //this.DebugInfo("Info", "MySqlConn was close, Reopen it, Current State is open");
                        }

                        break;

                    default:
                        break;
                }
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "OpenConnection:");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "OpenConnection: " + c);
            }
        }

        private void CloseMySqlConnection(int type)
        {
            if (this.MySql_Connection_is_activ == false)
            {
                try
                {
                    switch (type)
                    {
                        case 1:
                            //OdbcCon
                            if (this.MySqlCon != null)
                            {
                                this.MySqlCon.Close();
                                this.DebugInfo("Info", "Connection MySqlCon closed");
                            }
                            break;

                        case 2:
                            //ODBCConn
                            if (this.MySqlConn != null)
                            {
                                this.MySqlConn.Close();
                                this.DebugInfo("Info", "Connection MySqlConn closed");
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (MySqlException oe)
                {
                    this.DebugInfo("Error", "CloseMySqlConnection:");
                    this.DisplayMySqlErrorCollection(oe);
                }
                catch (Exception c)
                {
                    this.ExecuteCommand("Error", "CloseMySqlConnection: " + c);
                }
            }
        }

        private void tablebuilder()
        {
            if (boolTableEXISTS)
            {
                return;
            }
            lock (this.tablebuilderlock)
            {
                Thread.Sleep(3000);
                if ((m_strHost.Length == 0) || (m_strDatabase.Length == 0) || (m_strDBPort.Length == 0) || (m_strUserName.Length == 0))
                {
                    this.DebugInfo("Error", "Check you MySQL Server Details:, hostname, port, databasename and your login credentials!");
                    this.ExecuteCommand("procon.protected.plugins.enable", "CChatGUIDStatsLogger", "False");
                    return;
                }
                if ((m_strHost != null) && (m_strDatabase != null) && (m_strDBPort != null) && (m_strUserName != null) && (m_strPassword != null) && (boolTableEXISTS == false))
                {
                    this.DebugInfo("Info", "Start tablebuilder");
                    //new
                    this.generateWeaponList();

                    try
                    {
                        using (MySqlConnection TablebuilderCon = new MySqlConnection(this.DBConnectionStringBuilder()))
                        {
                            MySql.Data.MySqlClient.MySqlTransaction TableTransaction = null;
                            try
                            {
                                this.MySql_Connection_is_activ = true;
                                MySql.Data.MySqlClient.MySqlParameter param = new MySql.Data.MySqlClient.MySqlParameter();
                                TablebuilderCon.Open();
                                //Chatlog Table
                                string SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_chatlog + @"` (
                            					`ID` INT NOT NULL AUTO_INCREMENT ,
  												`logDate` DATETIME NULL DEFAULT NULL ,
  												`ServerID` SMALLINT UNSIGNED NOT NULL ,
  												`logSubset` VARCHAR(45) NULL DEFAULT NULL ,
  												`logSoldierName` VARCHAR(45) NULL DEFAULT NULL ,
  												`logMessage` TEXT NULL DEFAULT NULL ,
  													PRIMARY KEY (`ID`),
                                                    INDEX `INDEX_SERVERID` (`ServerID` ASC),
                                                    INDEX `INDEX_logDate` (`logDate` ASC))
													ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //MapStats Table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_mapstats + @"` (
  												      `ID` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
                                                      `ServerID` SMALLINT UNSIGNED NOT NULL DEFAULT '0' ,
                                                      `TimeMapLoad` DATETIME NULL DEFAULT NULL ,
                                                      `TimeRoundStarted` DATETIME NULL DEFAULT NULL ,
                                                      `TimeRoundEnd` DATETIME NULL DEFAULT NULL ,
                                                      `MapName` VARCHAR(45) NULL DEFAULT NULL ,
                                                      `Gamemode` VARCHAR(45) NULL DEFAULT NULL ,
                                                      `Roundcount` SMALLINT NOT NULL DEFAULT '0' ,
                                                      `NumberofRounds` SMALLINT NOT NULL DEFAULT '0' ,
                                                      `MinPlayers` SMALLINT NOT NULL DEFAULT '0' ,
                                                      `AvgPlayers` DOUBLE NOT NULL DEFAULT '0' ,
                                                      `MaxPlayers` SMALLINT NOT NULL DEFAULT '0' ,
                                                      `PlayersJoinedServer` SMALLINT NOT NULL DEFAULT '0' ,
                                                      `PlayersLeftServer` SMALLINT NOT NULL DEFAULT '0' ,
                                                      PRIMARY KEY (`ID`) ,
                                                      INDEX `ServerID_INDEX` (`ServerID` ASC) )
                                                    ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Start of the Transaction
                                TableTransaction = TablebuilderCon.BeginTransaction();


                                //Table tbl_games
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_games + @"` (
                                                   `GameID` tinyint(4) unsigned NOT NULL AUTO_INCREMENT,
                                                   `Name` varchar(45) DEFAULT NULL,
                                                   PRIMARY KEY (`GameID`),
                                                   UNIQUE KEY `name_unique` (`Name`)
                                                   ) ENGINE=InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Table playerdata
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_playerdata + @"` (
                                                  `PlayerID` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
                                                  `GameID` tinyint(4)unsigned NOT NULL DEFAULT '0',
												  `ClanTag` VARCHAR(10) NULL DEFAULT NULL ,
												  `SoldierName` VARCHAR(45) NULL DEFAULT NULL ,
                                                  `GlobalRank` SMALLINT UNSIGNED NOT NULL DEFAULT '0',
												  `PBGUID` VARCHAR(32) NULL DEFAULT NULL ,
												  `EAGUID` VARCHAR(35) NULL DEFAULT NULL ,
												  `IP_Address` VARCHAR(15) NULL DEFAULT NULL ,
                                                  `IPv6_Address` VARBINARY(16) NULL DEFAULT NULL ,
												  `CountryCode` VARCHAR(2) NULL DEFAULT NULL ,
												  PRIMARY KEY (`PlayerID`) ,
												  UNIQUE INDEX `UNIQUE_playerdata` (`GameID` ASC,`EAGUID` ASC) ,
												  INDEX `INDEX_SoldierName` (`SoldierName` ASC) )
												ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Server Table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_server + @"` (
  								      `ServerID` SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT ,
                                      `ServerGroup` TINYINT UNSIGNED NOT NULL DEFAULT 0 ,
									  `IP_Address` VARCHAR(45) NULL DEFAULT NULL ,
									  `ServerName` VARCHAR(200) NULL DEFAULT NULL ,
                                      `GameID` tinyint(4)unsigned NOT NULL DEFAULT '0',
									  `usedSlots` SMALLINT UNSIGNED NULL DEFAULT 0 ,
									  `maxSlots` SMALLINT UNSIGNED NULL DEFAULT 0 ,
									  `mapName` VARCHAR(45) NULL DEFAULT NULL ,
									  `fullMapName` TEXT NULL DEFAULT NULL ,
                                      
									  `Gamemode` VARCHAR(45) NULL DEFAULT NULL ,
									  `GameMod` VARCHAR(45) NULL DEFAULT NULL ,
									  `PBversion` VARCHAR(45) NULL DEFAULT NULL ,
									  `ConnectionState` VARCHAR(45) NULL DEFAULT NULL ,
									  PRIMARY KEY (`ServerID`) ,
                                      INDEX `INDEX_SERVERGROUP` (`ServerGroup` ASC) ,
									  UNIQUE INDEX `IP_Address_UNIQUE` (`IP_Address` ASC) )
									ENGINE = InnoDB";

                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Server Player Table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_server_player + @"` (
  								  `StatsID` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
								  `ServerID` SMALLINT UNSIGNED NOT NULL ,
								  `PlayerID` INT UNSIGNED NOT NULL ,
								  PRIMARY KEY (`StatsID`) ,
								  UNIQUE INDEX `UNIQUE_INDEX` (`ServerID` ASC, `PlayerID` ASC) ,
								  INDEX `fk_tbl_server_player_tbl_playerdata" + this.tableSuffix + @"` (`PlayerID` ASC) ,
								  INDEX `fk_tbl_server_player_tbl_server" + this.tableSuffix + @"` (`ServerID` ASC) ,
								  CONSTRAINT `fk_tbl_server_player_tbl_playerdata" + this.tableSuffix + @"`
								    FOREIGN KEY (`PlayerID` )
								    REFERENCES `" + this.tbl_playerdata + @"` (`PlayerID` )
								    ON DELETE CASCADE
								    ON UPDATE NO ACTION,
								  CONSTRAINT `fk_tbl_server_player_tbl_server" + this.tableSuffix + @"`
								    FOREIGN KEY (`ServerID` )
								    REFERENCES `" + this.tbl_server + @"` (`ServerID` )
								    ON DELETE CASCADE
								    ON UPDATE NO ACTION)
								ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }
                                //
                                //ServerStatistics Table
                                SQLTable = @"CREATE  TABLE IF NOT EXISTS `" + this.tbl_server_stats + @"` (
                                  `ServerID` SMALLINT(5) UNSIGNED NOT NULL ,
                                  `CountPlayers` BIGINT NOT NULL DEFAULT 0 ,
                                  `SumScore` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgScore` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumKills` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgKills` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumHeadshots` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgHeadshots` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumDeaths` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgDeaths` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumSuicide` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgSuicide` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumTKs` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgTKs` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumPlaytime` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgPlaytime` FLOAT NOT NULL DEFAULT 0 ,
                                  `SumRounds` BIGINT NOT NULL DEFAULT 0 ,
                                  `AvgRounds` FLOAT NOT NULL DEFAULT 0 ,
                                  PRIMARY KEY (`ServerID`) ,
                                  INDEX `fk_tbl_server_stats_tbl_server" + this.tableSuffix + @"` (`ServerID` ASC) ,
                                  CONSTRAINT `fk_tbl_server_stats_tbl_server" + this.tableSuffix + @"`
                                    FOREIGN KEY (`ServerID` )
                                    REFERENCES `" + this.tbl_server + @"` (`ServerID` )
                                    ON DELETE CASCADE
                                    ON UPDATE NO ACTION)
                                ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Stats Table
                                SQLTable = @"CREATE  TABLE IF NOT EXISTS `" + this.tbl_playerstats + @"` (
  								  `StatsID` INT UNSIGNED NOT NULL ,
								  `Score` INT NOT NULL DEFAULT '0' ,
								  `Kills` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Headshots` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Deaths` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Suicide` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `TKs` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Playtime` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Rounds` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  `FirstSeenOnServer` DATETIME NULL DEFAULT NULL ,
								  `LastSeenOnServer` DATETIME NULL DEFAULT NULL ,
								  `Killstreak` SMALLINT UNSIGNED NOT NULL DEFAULT '0' ,
								  `Deathstreak` SMALLINT UNSIGNED NOT NULL DEFAULT '0' ,
                                  `HighScore` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0' ,
                                  `rankScore` INT UNSIGNED NOT NULL DEFAULT '0' ,
                                  `rankKills` INT UNSIGNED NOT NULL DEFAULT '0' ,
                                  `Wins` INT UNSIGNED NOT NULL DEFAULT '0' ,
                                  `Losses` INT UNSIGNED NOT NULL DEFAULT '0' ,
								  PRIMARY KEY (`StatsID`) ,
                                  INDEX `INDEX_Score" + this.tableSuffix + @"` (`Score`),
                                  KEY `INDEX_RANK_SCORE" + this.tableSuffix + @"` (`rankScore`),
                                  KEY `INDEX_RANK_KILLS" + this.tableSuffix + @"` (`rankKills`),
								  CONSTRAINT `fk_tbl_playerstats_tbl_server_player1" + this.tableSuffix + @"`
								    FOREIGN KEY (`StatsID` )
								    REFERENCES `" + this.tbl_server_player + @"` (`StatsID` )
								    ON DELETE CASCADE
								    ON UPDATE NO ACTION)
								ENGINE = InnoDB";

                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Playerrank Table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_playerrank + @"` (
                                      `PlayerID` INT UNSIGNED NOT NULL DEFAULT 0 ,
                                      `ServerGroup` SMALLINT UNSIGNED NOT NULL DEFAULT 0 ,
                                      `rankScore` INT UNSIGNED NOT NULL DEFAULT 0 ,
                                      `rankKills` INT UNSIGNED NOT NULL DEFAULT 0 ,
                                      INDEX `INDEX_SCORERANKING" + this.tableSuffix + @"` (`rankScore` ASC) ,
                                      INDEX `INDEX_KILLSRANKING" + this.tableSuffix + @"` (`rankKills` ASC) ,
                                      PRIMARY KEY (`PlayerID`,`ServerGroup`) ,
                                      CONSTRAINT `fk_tbl_playerrank_tbl_playerdata" + this.tableSuffix + @"`
                                        FOREIGN KEY (`PlayerID` )
                                        REFERENCES `" + this.tbl_playerdata + @"` (`PlayerID` )
                                        ON DELETE CASCADE
                                        ON UPDATE NO ACTION)
                                    ENGINE = InnoDB";

                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Playersession Table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_sessions + @"` (
                                          `SessionID` INT UNSIGNED NOT NULL AUTO_INCREMENT,
                                          `StatsID` INT UNSIGNED NOT NULL,
                                          `StartTime` DATETIME NOT NULL,
                                          `EndTime` DATETIME NOT NULL,
                                          `Score` MEDIUMINT NOT NULL DEFAULT '0',
                                          `Kills` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0',
                                          `Headshots` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0',
                                          `Deaths` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0',
                                          `TKs` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0',
                                          `Suicide` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0',
                                          `RoundCount` TINYINT UNSIGNED NOT NULL DEFAULT '0',
                                          `Playtime` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0',
                                          `Killstreak` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' ,
								          `Deathstreak` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' ,
                                          `HighScore` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0' ,
                                          `Wins` TINYINT UNSIGNED NOT NULL DEFAULT '0' ,
                                          `Losses` TINYINT UNSIGNED NOT NULL DEFAULT '0' ,
                                          PRIMARY KEY (`SessionID`),
                                          INDEX `INDEX_STATSID" + this.tableSuffix + @"` (`StatsID` ASC),
                                          INDEX `INDEX_STARTTIME" + this.tableSuffix + @"` (`StartTime` ASC),
                                          CONSTRAINT `fk_tbl_sessions_tbl_server_player" + this.tableSuffix + @"` 
                                            FOREIGN KEY (`StatsID`) 
                                            REFERENCES `" + this.tbl_server_player + @"` (`StatsID`) 
                                            ON DELETE CASCADE 
                                            ON UPDATE NO ACTION) 
                                         ENGINE=InnoDB";

                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //currentplayers
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_currentplayers + @"` (
                                                  `ServerID` smallint(6) NOT NULL,
                                                  `Soldiername` varchar(45) NOT NULL,
                                                  `GlobalRank` SMALLINT UNSIGNED NOT NULL DEFAULT '0',
                                                  `ClanTag` varchar(45) DEFAULT NULL,
                                                  `Score` int(11) NOT NULL DEFAULT '0',
                                                  `Kills` int(11) NOT NULL DEFAULT '0',
                                                  `Headshots` int(11) NOT NULL DEFAULT '0',
                                                  `Deaths` int(11) NOT NULL DEFAULT '0',
                                                  `Suicide` int(11) DEFAULT NULL,
                                                  `Killstreak` smallint(6) DEFAULT '0',
                                                  `Deathstreak` smallint(6) DEFAULT '0',
                                                  `TeamID` tinyint(4) DEFAULT NULL,
                                                  `SquadID` tinyint(4) DEFAULT NULL,
                                                  `EA_GUID` varchar(45) NOT NULL DEFAULT '',
                                                  `PB_GUID` varchar(45) NOT NULL DEFAULT '',
                                                  `IP_aton` int(11) unsigned DEFAULT NULL,
                                                  `CountryCode` varchar(2) DEFAULT '',
                                                  `Ping` smallint(6) DEFAULT NULL,
                                                  `PlayerJoined` datetime DEFAULT NULL,
                                              PRIMARY KEY (`ServerID`,`Soldiername`)
                                            ) ENGINE=InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Awards
                                /*
                                SQLTable = @"CREATE  TABLE IF NOT EXISTS `" + this.tbl_awards + @"` (
                                                      `AwardID` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY ";
                                foreach (string strcolumn in this.m_lstAwardTable)
                                {
                                    SQLTable = String.Concat(SQLTable, ",`", strcolumn, "` mediumint(8) unsigned DEFAULT '0' ");
                                }
                                SQLTable = String.Concat(SQLTable, ")ENGINE = InnoDB DEFAULT CHARACTER SET = latin1");
                                if (this.m_awardsON == enumBoolYesNo.Yes)
                                {
                                    using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                }
                                */

                                //New Weapon table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_weapons + @"` (
                                              `WeaponID` int(11) unsigned NOT NULL AUTO_INCREMENT,
                                              `GameID` tinyint(4)unsigned NOT NULL,
                                              `Friendlyname` varchar(45) DEFAULT NULL,
                                              `Fullname` varchar(100) DEFAULT NULL,
                                              `Damagetype` varchar(45) DEFAULT NULL,
                                              `Slot` varchar(45) DEFAULT NULL,
                                              `Kitrestriction` varchar(45) DEFAULT NULL,
                                              PRIMARY KEY (`WeaponID`),
                                              UNIQUE KEY `unique` (`GameID`,`fullname`)
                                            ) ENGINE=InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //New Weapon stats table
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_weapons_stats + @"` (
                                                  `StatsID` INT unsigned NOT NULL,
                                                  `WeaponID` int(11) unsigned NOT NULL,
                                                  `Kills` int(11) unsigned NOT NULL DEFAULT '0',
                                                  `Headshots` int(11) unsigned NOT NULL DEFAULT '0',
                                                  `Deaths` int(11) unsigned NOT NULL DEFAULT '0',
                                                  PRIMARY KEY (`StatsID`,`WeaponID`),
                                                  KEY `Kills_Death_idx` (`Kills`,`Deaths`),
                                                  KEY `Kills_Head_idx` (`Kills`,`Headshots`),
                                                  CONSTRAINT `fk_tbl_weapons_stats_tbl_server_player_" + this.tableSuffix + @"`
								                    FOREIGN KEY (`StatsID` )
								                    REFERENCES `" + this.tbl_server_player + @"` (`StatsID` )
								                    ON DELETE CASCADE
								                    ON UPDATE NO ACTION
                                                ) ENGINE=InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Dogtagstable
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_dogtags + @"` (
                                      `KillerID` INT UNSIGNED NOT NULL ,
									  `VictimID` INT UNSIGNED NOT NULL ,
									  `Count` SMALLINT UNSIGNED NOT NULL DEFAULT '0' ,
									  PRIMARY KEY (`KillerID`, `VictimID`) ,
									  INDEX `fk_tbl_dogtags_tbl_server_player1" + this.tableSuffix + @"` (`KillerID` ASC) ,
									  INDEX `fk_tbl_dogtags_tbl_server_player2" + this.tableSuffix + @"` (`VictimID` ASC) ,
									  CONSTRAINT `fk_tbl_dogtags_tbl_server_player1" + this.tableSuffix + @"`
									    FOREIGN KEY (`KillerID` )
									    REFERENCES `" + this.tbl_server_player + @"` (`StatsID` )
									    ON DELETE CASCADE
									    ON UPDATE NO ACTION,
									  CONSTRAINT `fk_tbl_dogtags_tbl_server_player2" + this.tableSuffix + @"`
									    FOREIGN KEY (`VictimID` )
									    REFERENCES `" + this.tbl_server_player + @"` (`StatsID` )
									    ON DELETE CASCADE
									    ON UPDATE NO ACTION)
									ENGINE = InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Score and Tickettable
                                SQLTable = @"CREATE TABLE IF NOT EXISTS `" + this.tbl_teamscores + @"` (
                                              `ServerID` smallint(5) unsigned NOT NULL,
                                              `TeamID` smallint(5) unsigned NOT NULL,
                                              `Score` int(11) DEFAULT NULL,
                                              `WinningScore` int(11) DEFAULT NULL,
                                              PRIMARY KEY (`ServerID`,`TeamID` )
                                             ) ENGINE=InnoDB";
                                using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                {
                                    OdbcCom.ExecuteNonQuery();
                                }

                                //Commit the Transaction
                                TableTransaction.Commit();

                                this.boolTableEXISTS = true;

                                //fill weapon table
                                //get GameID
                                this.intServerGameType_ID = this.GetGameIDfromDB(this.strServerGameType);

                                List<string> addedWeaponList = new List<string>();

                                foreach (KeyValuePair<string, Dictionary<string, CStats.CUsedWeapon>> branch in this.weaponDic)
                                {
                                    string sqlCheckweapon = @"SELECT 
                                                                `GameID`,
                                                                `Friendlyname`,
                                                                `Fullname`,
                                                                `Damagetype`
                                                            FROM `" + this.tbl_weapons + @"`
                                                            WHERE `GameID` = @GameID
                                                              AND `Damagetype` = @Damagetype";
                                    using (MySqlCommand MyCommand = new MySqlCommand(sqlCheckweapon))
                                    {
                                        MyCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                                        MyCommand.Parameters.AddWithValue("@Damagetype", branch.Key.ToLower());

                                        using (DataTable result = this.SQLquery(MyCommand))
                                        {
                                            //this.DebugInfo("Info", "Rowcount:" + result.Rows.Count.ToString());
                                            if (result.Rows.Count >= 1)
                                            {
                                                result.PrimaryKey = new DataColumn[] { result.Columns["GameID"], result.Columns["Fullname"] };
                                            }
                                            TableTransaction = null;
                                            TableTransaction = TablebuilderCon.BeginTransaction();

                                            foreach (KeyValuePair<string, CStats.CUsedWeapon> leap in branch.Value)
                                            {
                                                if (result.Rows.Count == 0 || result.Rows.Contains(new object[] { this.intServerGameType_ID, leap.Value.Name }) == false || addedWeaponList.Contains(leap.Value.Name) == true)
                                                {
                                                    addedWeaponList.Add(leap.Value.Name);
                                                    //add weapon entry
                                                    string sqlInsertQuery = "INSERT INTO `" + this.tbl_weapons + @"` ( `GameID`, `Friendlyname`, `Fullname`,`Damagetype`,`Slot`,`Kitrestriction`) VALUES(@GameID, @Friendlyname, @Fullname, @Damagetype,@Slot,@Kitrestriction)
                                                                            ON DUPLICATE KEY UPDATE `Friendlyname` = @Friendlyname ,`Damagetype` =  @Damagetype,`Slot` = @Slot,`Kitrestriction` = @Kitrestriction";

                                                    using (MySqlCommand OdbcCom = new MySqlCommand(sqlInsertQuery, TablebuilderCon, TableTransaction))
                                                    {
                                                        OdbcCom.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                                                        OdbcCom.Parameters.AddWithValue("@Friendlyname", leap.Value.FieldName);
                                                        OdbcCom.Parameters.AddWithValue("@Fullname", leap.Value.Name);
                                                        OdbcCom.Parameters.AddWithValue("@Damagetype", branch.Key.ToLower());
                                                        OdbcCom.Parameters.AddWithValue("@Slot", leap.Value.Slot);
                                                        OdbcCom.Parameters.AddWithValue("@Kitrestriction", leap.Value.KitRestriction);
                                                        if (this.intServerGameType_ID != 0)
                                                        {
                                                            OdbcCom.ExecuteNonQuery();
                                                        }
                                                    }
                                                }
                                            }
                                            TableTransaction.Commit();
                                        }
                                    }
                                }

                                //Create WeaponMapping
                                this.WeaponMappingDic = new Dictionary<string, int>(this.GetWeaponMappingfromDB());


                                //TableCheck & Adjustemnts tbl_playerstats
                                /*
                                string sqlCheckplayerstats = "DESC `" + this.tbl_playerstats + "`";
                                string sqlAltertableplayerstats = "ALTER TABLE `" + this.tbl_playerstats + "` ";
                                string sqlIndex = "";
                                this.DebugInfo("Trace", "Tablecheck playerstats");
                                bool column_Missing = false;
                                using (DataTable result = this.SQLquery(new MySqlCommand(sqlCheckplayerstats)))
                                {
                                    DataColumn[] key = new DataColumn[1];
                                    key[0] = result.Columns[0];
                                    result.PrimaryKey = key;
                                    column_Missing = false;

                                    if (result.Rows.Contains("rankScore") == false)
                                    {
                                        this.DebugInfo("Trace", "Column rankScore is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `rankScore` INT(10) UNSIGNED NOT NULL DEFAULT '0', ");
                                        column_Missing = true;
                                        sqlIndex = string.Concat(sqlIndex, "ADD INDEX `INDEX_RANK_SCORE" + this.tableSuffix + @"` (`rankScore` ASC), ");
                                    }
                                    if (result.Rows.Contains("rankKills") == false)
                                    {
                                        this.DebugInfo("Trace", "Column rankScore is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `rankKills` INT(10) UNSIGNED NOT NULL DEFAULT '0', ");
                                        column_Missing = true;
                                        sqlIndex = string.Concat(sqlIndex, "ADD INDEX `INDEX_RANK_KILLS" + this.tableSuffix + @"` (`rankKills` ASC), ");
                                    }
                                    //Wins & Losses
                                    if (result.Rows.Contains("Wins") == false)
                                    {
                                        this.DebugInfo("Trace", "Column Wins is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `Wins` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0', ");
                                        column_Missing = true;
                                    }
                                    if (result.Rows.Contains("Losses") == false)
                                    {
                                        this.DebugInfo("Trace", "Column Losses is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `Losses` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0', ");
                                        column_Missing = true;
                                    }
                                    //HighScore
                                    if (result.Rows.Contains("HighScore") == false)
                                    {
                                        this.DebugInfo("Trace", "Column HighScore is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `HighScore` MEDIUMINT UNSIGNED NOT NULL DEFAULT '0' , ");
                                        column_Missing = true;
                                    }
                                }
                                if (column_Missing == true)
                                {
                                    TableTransaction = null;
                                    TableTransaction = TablebuilderCon.BeginTransaction();
                                    //Adding Columns
                                    sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, sqlIndex);
                                    int charindex = sqlAltertableplayerstats.LastIndexOf(",");
                                    if (charindex > 0)
                                    {
                                        sqlAltertableplayerstats = sqlAltertableplayerstats.Remove(charindex);
                                    }
                                    using (MySqlCommand OdbcCom = new MySqlCommand(sqlAltertableplayerstats, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                    TableTransaction.Commit();
                                }
                                

                                //TableCheck & Adjustemnts tbl_server
                                sqlCheckplayerstats = "DESC `" + this.tbl_server + "`";
                                sqlAltertableplayerstats = "ALTER TABLE `" + this.tbl_server + "` ";
                                sqlIndex = "";
                                this.DebugInfo("Trace", "Tablecheck tbl_server");
                                column_Missing = false;
                                using (DataTable result = this.SQLquery(new MySqlCommand(sqlCheckplayerstats)))
                                {
                                    DataColumn[] key = new DataColumn[1];
                                    key[0] = result.Columns[0];
                                    result.PrimaryKey = key;
                                    column_Missing = false;

                                    if (result.Rows.Contains("ServerGroup") == false)
                                    {
                                        this.DebugInfo("Trace", "Column ServerGroup is missing, Adding it to the table!");
                                        sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, "ADD COLUMN `ServerGroup` TINYINT UNSIGNED NOT NULL DEFAULT 0 ,");
                                        column_Missing = true;
                                        sqlIndex = string.Concat(sqlIndex, "ADD INDEX `INDEX_SERVERGROUP" + this.tableSuffix + @"` (`ServerGroup` ASC) ,");
                                    }
                                }
                                if (column_Missing == true)
                                {
                                    TableTransaction = null;
                                    TableTransaction = TablebuilderCon.BeginTransaction();
                                    //Adding Columns
                                    sqlAltertableplayerstats = string.Concat(sqlAltertableplayerstats, sqlIndex);
                                    int charindex = sqlAltertableplayerstats.LastIndexOf(",");
                                    if (charindex > 0)
                                    {
                                        sqlAltertableplayerstats = sqlAltertableplayerstats.Remove(charindex);
                                    }
                                    using (MySqlCommand OdbcCom = new MySqlCommand(sqlAltertableplayerstats, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                    TableTransaction.Commit();
                                }

                                */


                                //TableCheck  Adjustments playerstats
                                /*
                                //TableCheck Awards
                                sqlCheck = "DESC `" + this.tbl_awards + "`";
                                sqlAltertable = "ALTER TABLE `" + this.tbl_awards + "` ";
                                //result = new List<string>(this.SQLquery(sqlCheck,9));
                                this.DebugInfo("Tablecheck Awards");
                                using (DataTable result = this.SQLquery(new MySqlCommand(sqlCheck)))
                                {
                                    DataColumn[] key = new DataColumn[1];
                                    key[0] = result.Columns[0];
                                    result.PrimaryKey = key;
                                    fieldMissing = false;

                                    foreach (string strField in this.m_lstAwardTable)
                                    {
                                        if (result.Rows.Contains(strField) == false)
                                        {
                                            this.DebugInfo(strField + " is missing, Adding it to the table!");
                                            sqlAltertable = string.Concat(sqlAltertable, "ADD COLUMN `" + strField + "` mediumint(8) unsigned DEFAULT '0', ");
                                            fieldMissing = true;
                                        }
                                    }
                                }
                                if (fieldMissing == true)
                                {
                                    TableTransaction = null;
                                    TableTransaction = TablebuilderCon.BeginTransaction();
                                    SQLTable = "ALTER TABLE `" + this.tbl_awards + "` ENGINE = MyISAM";
                                    using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                    //Adding Columns
                                    int charindex = sqlAltertable.LastIndexOf(",");
                                    if (charindex > 0)
                                    {
                                        sqlAltertable = sqlAltertable.Remove(charindex);
                                    }
                                    using (MySqlCommand OdbcCom = new MySqlCommand(sqlAltertable, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                    SQLTable = "ALTER TABLE `" + this.tbl_awards + "` ENGINE = InnoDB";
                                    using (MySqlCommand OdbcCom = new MySqlCommand(SQLTable, TablebuilderCon, TableTransaction))
                                    {
                                        OdbcCom.ExecuteNonQuery();
                                    }
                                    TableTransaction.Commit();
                                }
                                */
                            }

                            catch (MySqlException oe)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error in Tablebuilder: ");
                                this.DisplayMySqlErrorCollection(oe);
                                TableTransaction.Rollback();
                            }
                            catch (Exception c)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error: " + c);
                                TableTransaction.Rollback();
                                this.boolTableEXISTS = false;
                                this.m_ID_cache.Clear();
                            }
                            finally
                            {
                                TablebuilderCon.Close();
                            }
                        }
                    }
                    catch (MySqlException oe)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error in Tablebuilder: ");
                        this.DisplayMySqlErrorCollection(oe);
                    }
                    catch (Exception c)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Error: " + c);
                    }
                }
            }
        }

        private void LogChat(string strSpeaker, string strMessage, string strType)
        {
            try
            {
                if (this.m_enChatloggingON == enumBoolYesNo.No)
                {
                    return;
                }
                if (this.m_enNoServerMsg == enumBoolYesNo.No && strSpeaker.CompareTo("Server") == 0)
                {
                    return;
                }
                if (this.m_enChatlogFilter == enumBoolYesNo.Yes)
                {
                    //Filter Messages
                    foreach (Regex FilterRule in this.lstChatFilterRules)
                    {
                        if (FilterRule.IsMatch(strMessage))
                        {
                            //dont log
                            this.DebugInfo("Trace", "Chatmessage: '" + strMessage + "' was filtered out by the Regex rule: " + FilterRule.ToString());
                            return;
                        }
                    }
                }
                if (m_enInstantChatlogging == enumBoolYesNo.Yes)
                {
                    string query = "INSERT INTO " + this.tbl_chatlog + @" (logDate, ServerID, logSubset, logSoldierName, logMessage) VALUES (@logDate, @ServerID, @logSubset, @logSoldierName, @logMessage)";
                    this.tablebuilder();
                    if ((m_strHost != null) || (m_strDatabase != null) || (m_strDBPort != null) || (m_strUserName != null) || (m_strPassword != null))
                    {
                        if (this.m_highPerformanceConnectionMode == enumBoolOnOff.On)
                        {
                            try
                            {
                                using (MySqlConnection Connection = new MySqlConnection(this.DBConnectionStringBuilder()))
                                {
                                    Connection.Open();
                                    if (Connection.State == ConnectionState.Open)
                                    {
                                        using (MySqlCommand OdbcCom = new MySqlCommand(query, Connection))
                                        {
                                            OdbcCom.Parameters.AddWithValue("@logDate", MyDateTime.Now);
                                            OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                            OdbcCom.Parameters.AddWithValue("@logSubset", strType);
                                            OdbcCom.Parameters.AddWithValue("@logSoldierName", strSpeaker);
                                            OdbcCom.Parameters.AddWithValue("@logMessage", strMessage);
                                            OdbcCom.ExecuteNonQuery();
                                        }
                                    }
                                    Connection.Close();
                                }
                            }
                            catch (MySqlException oe)
                            {
                                this.DebugInfo("Error", "LogChat: ");
                                this.DisplayMySqlErrorCollection(oe);
                            }
                            catch (Exception c)
                            {
                                this.DebugInfo("Error", "LogChat: " + c);
                            }
                        }
                        else
                        {
                            lock (this.chatloglock)
                            {
                                try
                                {
                                    if (this.MySqlChatCon == null)
                                    {
                                        this.MySqlChatCon = new MySqlConnection(this.DBConnectionStringBuilder());
                                    }
                                    if (MySqlChatCon.State != ConnectionState.Open)
                                    {
                                        this.MySqlChatCon.Open();
                                    }
                                    if (MySqlChatCon.State == ConnectionState.Open)
                                    {
                                        using (MySqlCommand OdbcCom = new MySqlCommand(query, MySqlChatCon))
                                        {
                                            OdbcCom.Parameters.AddWithValue("@logDate", MyDateTime.Now);
                                            OdbcCom.Parameters.AddWithValue("@ServerID", this.ServerID);
                                            OdbcCom.Parameters.AddWithValue("@logSubset", strType);
                                            OdbcCom.Parameters.AddWithValue("@logSoldierName", strSpeaker);
                                            OdbcCom.Parameters.AddWithValue("@logMessage", strMessage);
                                            OdbcCom.ExecuteNonQuery();
                                        }
                                    }
                                }
                                catch (MySqlException oe)
                                {
                                    this.DebugInfo("Error", "LogChat: ");
                                    this.DisplayMySqlErrorCollection(oe);
                                    try
                                    {
                                        if (MySqlChatCon.State == ConnectionState.Open)
                                        {
                                            MySqlChatCon.Dispose();
                                        }
                                    }
                                    catch { }
                                }
                                catch (Exception c)
                                {
                                    this.DebugInfo("Error", "LogChat: " + c);
                                    try
                                    {
                                        if (MySqlChatCon.State == ConnectionState.Open)
                                        {
                                            MySqlChatCon.Close();
                                        }
                                    }
                                    catch { }
                                }
                                finally
                                {
                                    try
                                    {
                                        if (MySqlChatCon != null)
                                        {
                                            MySqlChatCon.Close();
                                        }
                                    }
                                    catch { }

                                }
                            }
                        }
                    }
                }
                else
                {
                    CLogger chat = new CLogger(MyDateTime.Now, strSpeaker, strMessage, strType);
                    ChatLog.Add(chat);
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "LogChat_2: " + c);
            }
        }

        public void DebugInfo(string debuglevel, string DebugMessage)
        {
            switch (this.GlobalDebugMode)
            {
                case "Trace":
                    //Post every Message
                    break;
                case "Info":
                    if (String.Equals(debuglevel, "Trace") == true)
                    {
                        return;
                    }
                    break;
                case "Warning":
                    if (String.Equals(debuglevel, "Trace") == true || String.Equals(debuglevel, "Info") == true)
                    {
                        return;
                    }
                    break;
                case "Error":
                    if (String.Equals(debuglevel, "Error") == false)
                    {
                        return;
                    }
                    break;
            }
            // Post error Message in correct Format 
            if (String.Equals(debuglevel, "Trace"))
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "[Statslogger]Trace: " + DebugMessage);
            }
            else if (String.Equals(debuglevel, "Info"))
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^2" + "[Statslogger]Info: " + DebugMessage);
            }
            else if (String.Equals(debuglevel, "Warning"))
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^3" + "[Statslogger]Warning: " + DebugMessage);
            }
            else if (String.Equals(debuglevel, "Error"))
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^8" + "[Statslogger]Error: " + DebugMessage);
            }
        }

        private void PrepareKeywordDic()
        {
            if (boolKeywordDicReady == false)
            {
                this.DebugInfo("Trace", "PrepareKeywordDic: Preparing");
                this.m_dicKeywords.Clear();
                try
                {
                    foreach (KeyValuePair<string, string> kvp in this.DamageClass)
                    {
                        if (this.m_dicKeywords.ContainsKey(kvp.Key) == false)
                        {
                            this.m_dicKeywords.Add(kvp.Key, new List<string>());
                            this.m_dicKeywords[kvp.Key].Add(kvp.Key.ToUpper());

                            string[] weaponName = Regex.Replace(kvp.Key.Replace("Weapons/", "").Replace("Gadgets/", ""), @"XP\d_", "").Split('/');
                            string friendlyname = weaponName[0].Replace(' ', '_').Replace(".", "").Replace("U_", "").ToUpper();
                            if (this.m_dicKeywords.ContainsKey(friendlyname) == false)
                            {
                                this.m_dicKeywords[kvp.Key].Add(friendlyname);
                            }
                        }
                    }
                    string dicKey = String.Empty;
                    string dicValue = String.Empty;
                    foreach (string line in m_lstTableconfig)
                    {
                        if (line.Contains("{") && line.Contains("}"))
                        {
                            dicKey = line.Remove(line.IndexOf("{"));
                            dicValue = line.Replace("{", ",");
                            dicValue = dicValue.Replace("}", "").ToUpper();
                            string[] arrStrings = dicValue.Split(',');
                            if (this.m_dicKeywords.ContainsKey(dicKey))
                            {
                                //Prüfen
                                this.m_dicKeywords[dicKey].AddRange(arrStrings);
                                /*
                                foreach (string entry in this.m_dicKeywords[dicKey])
                                {
                                    this.DebugInfo("Trace", "PrepareKeywordDic: " + entry);
                                }
                                */
                            }
                            else
                            {
                                this.DebugInfo("Warning", "PrepareKeywordDic: Mainkey " + dicKey + " not found!");
                            }
                        }
                    }
                }
                catch (Exception c)
                {
                    this.DebugInfo("Error", "Error in PrepareKeywordDic: " + c);
                }
            }
        }

        public string FindKeyword(string strToFind)
        {
            try
            {
                this.DebugInfo("Trace", "FindKeyword: " + strToFind);
                foreach (KeyValuePair<string, List<string>> kvp in this.m_dicKeywords)
                {
                    if (kvp.Value.Contains(strToFind.Replace(" ", "")))
                    {
                        this.DebugInfo("Trace", "FindKeyword: Returning Key " + kvp.Key);
                        return kvp.Key;
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "FindKeyword: " + c);
            }
            return String.Empty;
        }

        public List<string> ListReplace(List<string> targetlist, string wordToReplace, string replacement)
        {
            List<string> lstResult = new List<string>();
            foreach (string substring in targetlist)
            {
                lstResult.Add(substring.Replace(wordToReplace, replacement));
            }
            return lstResult;
        }

        private void CheckMessageLength(string strMessage, int intMessagelength)
        {
            if (strMessage.Length > intMessagelength)
            {
                //Send Warning
                this.DebugInfo("Warning", strMessage);
                this.DebugInfo("Warning", "This Ingamemessage is too long and wont sent to Server!!!");
                this.DebugInfo("Warning", "The Message has a Length of " + strMessage.Length.ToString() + " Chars, Allow are 128 Chars");
            }
        }

        private void CreateSession(string SoldierName, int intScore, string EAGUID)
        {
            if (this.ServerID == 0)
            {
                return;
            }
            try
            {
                if (this.m_sessionON == enumBoolYesNo.Yes)
                {
                    //Session
                    lock (this.sessionlock)
                    {
                        if (this.m_dicSession.ContainsKey(SoldierName) == false)
                        {
                            //this.DebugInfo("Trace", "Session for Player: " + SoldierName + " created");
                            this.m_dicSession.Add(SoldierName, new CStats(String.Empty,intScore, 0, 0, 0, 0, 0, 0, this.m_dTimeOffset, this.weaponDic));
                            this.m_dicSession[SoldierName].Rank = this.GetRank(SoldierName);
                        }
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "CreateSession: " + c);
            }
            finally
            {
                lock (this.sessionlock)
                {
                    //Session Score
                    if (this.m_dicSession.ContainsKey(SoldierName) && this.m_sessionON == enumBoolYesNo.Yes)
                    {
                        this.m_dicSession[SoldierName].AddScore(intScore);
                        if (EAGUID.Length > 2)
                        {
                            this.m_dicSession[SoldierName].EAGuid = EAGUID;
                        }
                    }
                }
            }
        }
        /*
        private void RemoveSession(string SoldierName)
        {
            try
            {
                if (m_sessionON == enumBoolYesNo.Yes)
                {
                    if (this.m_dicSession.ContainsKey(SoldierName) == true)
                    {
                        //Passed seesion to list
                        this.lstpassedSessions.Add(m_dicSession[SoldierName]);
                        this.m_dicSession.Remove(SoldierName);
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "RemoveSession: " + c);
            }
        }
        */
        private void GetServerStats(string SoldierName, int delay, string scope)
        {
            string SQL = @"SELECT * FROM " + this.tbl_server_stats + @" WHERE ServerID = @ServerID";
            List<string> result = new List<string>(this.m_lstServerstatsMsg);
            try
            {
                using (MySqlCommand SelectCommand = new MySqlCommand(SQL))
                {
                    SelectCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    DataTable sqlresult = this.SQLquery(SelectCommand);
                    if (sqlresult != null)
                    {
                        foreach (DataRow row in sqlresult.Rows)
                        {
                            result = this.ListReplace(result, "%serverName%", this.serverName);
                            //COUNT
                            result = this.ListReplace(result, "%countPlayer%", Convert.ToInt64(row["CountPlayers"]).ToString());
                            //SUM
                            result = this.ListReplace(result, "%sumScore%", Convert.ToInt64(row["SumScore"]).ToString());
                            result = this.ListReplace(result, "%sumKills%", Convert.ToInt64(row["SumKills"]).ToString());
                            result = this.ListReplace(result, "%sumHeadshots%", Convert.ToInt64(row["SumHeadshots"]).ToString());
                            result = this.ListReplace(result, "%sumDeaths%", Convert.ToInt64(row["SumDeaths"]).ToString());
                            result = this.ListReplace(result, "%sumSuicide%", Convert.ToInt64(row["SumSuicide"]).ToString());
                            result = this.ListReplace(result, "%sumTKs%", Convert.ToInt64(row["SumTKs"]).ToString());
                            result = this.ListReplace(result, "%sumRounds%", Convert.ToInt64(row["SumRounds"]).ToString());

                            //AVG
                            result = this.ListReplace(result, "%avgScore%", Convert.ToInt64(row["AvgScore"]).ToString());
                            result = this.ListReplace(result, "%avgKills%", Convert.ToInt64(row["AvgKills"]).ToString());
                            result = this.ListReplace(result, "%avgHeadshots%", Convert.ToInt64(row["AvgHeadshots"]).ToString());
                            result = this.ListReplace(result, "%avgDeaths%", Convert.ToInt64(row["AvgDeaths"]).ToString());
                            result = this.ListReplace(result, "%avgSuicide%", Convert.ToInt64(row["AvgSuicide"]).ToString());
                            result = this.ListReplace(result, "%avgTKs%", Convert.ToInt64(row["AvgTKs"]).ToString());
                            result = this.ListReplace(result, "%avgRounds%", Convert.ToInt64(row["AvgRounds"]).ToString());
                            //MISC.
                            //SPM
                            result = this.ListReplace(result, "%avgSPM%", Math.Round(Convert.ToDouble(row["SumScore"]) / (Convert.ToDouble(row["SumPlaytime"]) / 60), 2).ToString());
                            //KPM
                            result = this.ListReplace(result, "%avgKPM%", Math.Round(Convert.ToDouble(row["SumKills"]) / (Convert.ToDouble(row["SumPlaytime"]) / 60), 2).ToString());
                            //HPM
                            result = this.ListReplace(result, "%avgHPM%", Math.Round(Convert.ToDouble(row["SumHeadshots"]) / (Convert.ToDouble(row["SumPlaytime"]) / 60), 2).ToString());
                            //HPK
                            result = this.ListReplace(result, "%avgHPK%", Math.Round(Convert.ToDouble(row["SumHeadshots"]) / (Convert.ToDouble(row["SumKills"])), 2).ToString());
                            //Playtime
                            TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(row["SumPlaytime"]), 0, 0);
                            result = this.ListReplace(result, "%sumPlaytime%", span.Days + "d:" + span.Hours + "h:" + span.Minutes + "m:" + span.Seconds + "s");
                            result = this.ListReplace(result, "%sumPlaytimeHours%", Math.Round(span.TotalHours, 2).ToString());
                            result = this.ListReplace(result, "%sumPlaytimeDays%", Math.Round(span.TotalDays, 2).ToString());
                            //avg. Playtime
                            span = new TimeSpan(0, 0, Convert.ToInt32(row["AvgPlaytime"]), 0, 0);
                            result = this.ListReplace(result, "%avgPlaytime%", span.Days + "d:" + span.Hours + "h:" + span.Minutes + "m:" + span.Seconds + "s");
                            result = this.ListReplace(result, "%avgPlaytimeHours%", Math.Round(span.TotalHours, 2).ToString());
                            result = this.ListReplace(result, "%avgPlaytimeDays%", Math.Round(span.TotalDays, 2).ToString());
                        }

                        if (result.Count != 0)
                        {
                            this.SendMultiLineChatMessage(result, delay, 0, scope, SoldierName);
                        }
                        else
                        {
                            result.Clear();
                            result.Add("No Serverdata available!");
                            this.SendMultiLineChatMessage(result, delay, 0, scope, SoldierName);
                        }
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "GetServerStats: " + c);
            }

        }

        private void GetSession(string SoldierName, int delay, string scope)
        {
            if (this.ServerID == 0)
            {
                return;
            }
            try
            {
                if (this.m_dicSession.ContainsKey(SoldierName) && this.m_sessionON == enumBoolYesNo.Yes)
                {
                    List<string> result = new List<string>();
                    result = m_lstSessionMessage;
                    result = ListReplace(result, "%playerName%", SoldierName);
                    result = ListReplace(result, "%playerScore%", this.m_dicSession[SoldierName].Score.ToString());
                    result = ListReplace(result, "%playerKills%", this.m_dicSession[SoldierName].Kills.ToString());
                    result = ListReplace(result, "%killstreak%", this.m_dicSession[SoldierName].Killstreak.ToString());
                    result = ListReplace(result, "%playerDeaths%", this.m_dicSession[SoldierName].Deaths.ToString());
                    result = ListReplace(result, "%deathstreak%", this.m_dicSession[SoldierName].Deathstreak.ToString());
                    result = ListReplace(result, "%playerKDR%", this.m_dicSession[SoldierName].KDR().ToString());
                    result = ListReplace(result, "%playerHeadshots%", this.m_dicSession[SoldierName].Headshots.ToString());
                    result = ListReplace(result, "%playerSuicide%", this.m_dicSession[SoldierName].Suicides.ToString());
                    result = ListReplace(result, "%playerTK%", this.m_dicSession[SoldierName].Teamkills.ToString());
                    result = ListReplace(result, "%startRank%", this.m_dicSession[SoldierName].Rank.ToString());
                    //Rankdiff
                    int playerRank = this.GetRank(SoldierName);
                    //int playerRank = 0;
                    result = ListReplace(result, "%playerRank%", playerRank.ToString());
                    int Rankdif = this.m_dicSession[SoldierName].Rank;
                    Rankdif = Rankdif - playerRank;
                    if (Rankdif == 0)
                    {
                        result = ListReplace(result, "%RankDif%", "0");
                    }
                    else if (Rankdif > 0)
                    {
                        result = ListReplace(result, "%RankDif%", "+" + Rankdif.ToString());
                    }
                    else
                    {
                        result = ListReplace(result, "%RankDif%", Rankdif.ToString());
                    }
                    result = ListReplace(result, "%SessionStarted%", this.m_dicSession[SoldierName].TimePlayerjoined.ToString());
                    TimeSpan duration = MyDateTime.Now - this.m_dicSession[SoldierName].TimePlayerjoined;
                    result = ListReplace(result, "%SessionDuration%", Math.Round(duration.TotalMinutes, 2).ToString());

                    if (result.Count != 0)
                    {
                        this.SendMultiLineChatMessage(result, delay, 0, scope, SoldierName);
                    }
                    else
                    {
                        result.Clear();
                        result.Add("No Sessiondata are available!");
                        this.SendMultiLineChatMessage(result, delay, 0, scope, SoldierName);
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "GetSession: " + c);
            }
        }

        private int GetRank(string SoldierName)
        {
            //this.DebugInfo("Trace", "GetRank: " + SoldierName);
            int rank = 0;
            try
            {
                string SQL = String.Empty;
                if (m_enRankingByScore == enumBoolYesNo.Yes)
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpr.rankScore AS rank
                                FROM " + this.tbl_playerrank + @" tpr
                                INNER JOIN " + this.tbl_playerdata + @" tpd ON tpr.PlayerID = tpd.PlayerID
                                WHERE tpd.SoldierName = @SoldierName AND tpr.ServerGroup = @ServerGroup";
                    }
                    else
                    {
                        SQL = @"SELECT tps.rankScore AS rank
                                FROM " + this.tbl_playerstats + @" tps
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tps.StatsID = tsp.StatsID
                                INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                                WHERE  tpd.SoldierName = @SoldierName AND tsp.ServerID = @ServerID";
                    }
                }
                else
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SQL = @"SELECT tpr.rankKills AS rank
                                FROM " + this.tbl_playerrank + @" tpr
                                INNER JOIN " + this.tbl_playerdata + @" tpd ON tpr.PlayerID = tpd.PlayerID
                                WHERE tpd.SoldierName = @SoldierName AND tpr.ServerGroup = @ServerGroup";
                    }
                    else
                    {
                        SQL = @"SELECT tps.rankKills AS rank
                                FROM " + this.tbl_playerstats + @" tps
                                INNER JOIN " + this.tbl_server_player + @" tsp ON tps.StatsID = tsp.StatsID
                                INNER JOIN " + this.tbl_playerdata + @" tpd ON tsp.PlayerID = tpd.PlayerID
                                WHERE  tpd.SoldierName = @SoldierName AND tsp.ServerID = @ServerID";
                    }
                }
                using (MySqlCommand SelectCommand = new MySqlCommand(SQL))
                {
                    if (this.m_enOverallRanking == enumBoolYesNo.Yes)
                    {
                        SelectCommand.Parameters.AddWithValue("@SoldierName", SoldierName);
                        SelectCommand.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                    }
                    else
                    {
                        SelectCommand.Parameters.AddWithValue("@SoldierName", SoldierName);
                        SelectCommand.Parameters.AddWithValue("@ServerID", this.ServerID);
                    }
                    DataTable result = this.SQLquery(SelectCommand);
                    if (result != null)
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            if (Convert.DBNull.Equals(row[0]) == false)
                            {
                                rank = Convert.ToInt32(row[0]);
                                this.DebugInfo("Trace", SoldierName + " Rank: " + row[0].ToString());
                            }
                        }
                    }
                }          
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "Error in GetRank: ");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in GetRank: " + c);
            }
            return rank;
        }

        public void PluginInfo(string strPlayer)
        {
            //this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger","0", "1", "1", "procon.protected.send", "admin.say","This Server running the PRoCon plugin "+this.GetPluginName+" "+this.GetPluginVersion+"running by "+ this.GetPluginAuthor,"player", strPlayer);
        }

        public void DisplayMySqlErrorCollection(MySqlException myException)
        {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Message: " + myException.Message);
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Native: " + myException.ErrorCode.ToString());
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Source: " + myException.Source.ToString());
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1StackTrace: " + myException.StackTrace.ToString());
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1InnerException: " + myException.InnerException.ToString());
               // this.ExecuteCommand("procon.protected.pluginconsole.write", "^1SQL: " + myException.);
        }

        private void prepareTablenames()
        {
            this.tbl_playerdata = "tbl_playerdata" + this.tableSuffix;
            this.tbl_playerstats = "tbl_playerstats" + this.tableSuffix;
            this.tbl_weaponstats = "tbl_weaponstats" + this.tableSuffix;
            this.tbl_dogtags = "tbl_dogtags" + this.tableSuffix;
            this.tbl_mapstats = "tbl_mapstats" + this.tableSuffix;
            this.tbl_chatlog = "tbl_chatlog" + this.tableSuffix;
            this.tbl_bfbcs = "tbl_bfbcs" + this.tableSuffix;
            this.tbl_awards = "tbl_awards" + this.tableSuffix;
            this.tbl_server = "tbl_server" + this.tableSuffix;
            this.tbl_server_player = "tbl_server_player" + this.tableSuffix;
            this.tbl_server_stats = "tbl_server_stats" + this.tableSuffix;
            this.tbl_playerrank = "tbl_playerrank" + this.tableSuffix;
            this.tbl_sessions = "tbl_sessions" + this.tableSuffix;
            this.tbl_currentplayers = "tbl_currentplayers" + this.tableSuffix;
            this.tbl_weapons = "tbl_weapons" + this.tableSuffix;
            this.tbl_weapons_stats = "tbl_weapons_stats" + this.tableSuffix;
            this.tbl_games = "tbl_games" + this.tableSuffix;
            this.tbl_teamscores = "tbl_teamscores" + this.tableSuffix;

        }

        private void setGameMod()
        {
            //this.PrepareKeywordDic();
            this.boolTableEXISTS = false;
        }

        /*private void getBFBCStats(List<CPlayerInfo> lstPlayers)
        {
            //Disabled temp
            return;
            try
            {
                List<string> lstSoldierName = new List<string>();
                foreach (CPlayerInfo Player in lstPlayers)
                {
                    DateTime lastUpdate = DateTime.MinValue;
                    if (this.m_getStatsfromBFBCS == enumBoolYesNo.Yes && Player.SoldierName != null && this.StatsTracker.ContainsKey(Player.SoldierName) == true && this.StatsTracker[Player.SoldierName].BFBCS_Stats.Updated == false && this.StatsTracker[Player.SoldierName].BFBCS_Stats.Fetching == false)
                    {
                        string SQL = @"SELECT b.LastUpdate, b.Rank, b.Kills, b.Deaths, b.Score, b.Time
	  								 FROM " + tbl_playerdata + @" a
	  								 INNER JOIN " + tbl_bfbcs + @" b ON a.PlayerID = b.bfbcsID
	               					 WHERE a.SoldierName = @SoldierName";

                        using (MySqlCommand SelectCommand = new MySqlCommand(SQL))
                        {
                            SelectCommand.Parameters.AddWithValue("@SoldierName", Player.SoldierName);
                            DataTable result = this.SQLquery(SelectCommand);

                            foreach (DataRow row in result.Rows)
                            {
                                //this.DebugInfo("Last Update: " + row[0].ToString());
                                lastUpdate = Convert.ToDateTime(row[0]);
                                TimeSpan TimeDifference = MyDateTime.Now.Subtract(lastUpdate);
                                //this.DebugInfo(TimeDifference.TotalHours.ToString());
                                if (TimeDifference.TotalHours >= this.BFBCS_UpdateInterval && this.StatsTracker[Player.SoldierName].BFBCS_Stats.Fetching == false)
                                {
                                    this.StatsTracker[Player.SoldierName].BFBCS_Stats.Fetching = true;
                                    lstSoldierName.Add(Player.SoldierName);
                                }
                                else if (this.StatsTracker.ContainsKey(Player.SoldierName) == true && this.StatsTracker[Player.SoldierName].BFBCS_Stats.Fetching == true)
                                {
                                    //Do nothing
                                }
                                else
                                {
                                    if (this.StatsTracker.ContainsKey(Player.SoldierName) == true)
                                    {
                                        //this.DebugInfo("No Update needed");
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Updated = true;
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Rank = Convert.ToInt32(row[1]);
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Kills = Convert.ToInt32(row[2]);
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Deaths = Convert.ToInt32(row[3]);
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Score = Convert.ToInt32(row[4]);
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.Time = Convert.ToDouble(row[5]);
                                        this.StatsTracker[Player.SoldierName].BFBCS_Stats.NoUpdate = true;
                                        this.checkPlayerStats(Player.SoldierName, this.m_strReasonMsg);
                                    }
                                }
                            }
                        }
                    }
                }
                if (lstSoldierName != null && lstSoldierName.Count > 0 && lstSoldierName.Count >= this.BFBCS_Min_Request)
                {
                    //Start Fetching
                    specialArrayObject ListObject = new specialArrayObject(lstSoldierName);
                    Thread newThread = new Thread(new ParameterizedThreadStart(this.DownloadBFBCS));
                    newThread.Start(ListObject);
                }
                else
                {
                    foreach (string player in lstSoldierName)
                    {
                        this.StatsTracker[player].BFBCS_Stats.Fetching = false;
                        this.StatsTracker[player].BFBCS_Stats.Updated = false;
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", " getBFBCStats: " + c);
            }
        }

        private void DownloadBFBCS(object ListObject)
        {
            specialArrayObject ListString = (specialArrayObject)ListObject;
            List<string> lstSoldierName = new List<string>();
            lstSoldierName = ListString.LstString;
            //Define a empty string for parameter
            string ParameterString = String.Empty;
            string result = String.Empty;
            foreach (string SoldierName in lstSoldierName)
            {
                if (this.StatsTracker[SoldierName].BFBCS_Stats.Updated == false)
                {
                    ParameterString = String.Concat(ParameterString, SoldierName, ",");
                    this.StatsTracker[SoldierName].BFBCS_Stats.Updated = true;
                }
            }
            ParameterString = ParameterString.Remove(ParameterString.LastIndexOf(","));
            try
            {
                this.DebugInfo("Trace", "Thread started and fetching Stats from BFBCS for Players: " + ParameterString);
                using (WebClient wc = new WebClient())
                {
                    //Thx to IIIAVIII
                    ParameterString = ParameterString.Replace("&", "%26");
                    ParameterString = ParameterString.Replace(" ", "%20");
                    ParameterString = ParameterString.Replace("$", "%24");
                    ParameterString = ParameterString.Replace("+", "%2B");
                    ParameterString = ParameterString.Replace("/", "%2F");
                    ParameterString = ParameterString.Replace("?", "%3F");
                    ParameterString = ParameterString.Replace("%", "%25");
                    ParameterString = ParameterString.Replace("#", "%23");
                    //ParameterString = ParameterString.Replace(",","%2C");
                    ParameterString = ParameterString.Replace(":", "%3A");
                    ParameterString = ParameterString.Replace(";", "%3B");
                    ParameterString = ParameterString.Replace("=", "%3D");
                    ParameterString = ParameterString.Replace("@", "%40");
                    ParameterString = ParameterString.Replace("<", "%3C");
                    ParameterString = ParameterString.Replace(">", "%3E");
                    ParameterString = ParameterString.Replace("{", "%7B");
                    ParameterString = ParameterString.Replace("}", "%7D");
                    ParameterString = ParameterString.Replace("|", "%7C");
                    ParameterString = ParameterString.Replace(@"\", @"%5C");
                    ParameterString = ParameterString.Replace("^", "%5E");
                    ParameterString = ParameterString.Replace("~", "%7E");
                    ParameterString = ParameterString.Replace("[", "%5B");
                    ParameterString = ParameterString.Replace("]", "%5D");
                    ParameterString = ParameterString.Replace("`", "%60");

                    result = wc.DownloadString("http://api.bfbcs.com/api/pc?players=" + ParameterString + "&fields=basic");
                }
                if (result == null || result.StartsWith("{") == false)
                {
                    this.DebugInfo("Trace", "the String returned by BFBCS was invalid");
                    this.DebugInfo("Trace", "Trying to repair the String...");
                    if (result != null)
                    {
                        //result = result.Remove(result.IndexOf("<"),(result.LastIndexOf(">")+1));
                        if (result.IndexOf("{") > 0)
                        {
                            result = result.Substring(result.IndexOf("{"));
                        }
                        if (result == null || result.StartsWith("{") == false)
                        {
                            this.DebugInfo("Trace", "Repair failed!!!");
                            return;
                        }
                        else
                        {
                            this.DebugInfo("Trace", "Repair (might be) successful");
                        }
                    }
                    else
                    {
                        this.DebugInfo("Trace", "Empty String...");
                        return;
                    }
                }
                //JSON DECODE
                Hashtable jsonHash = (Hashtable)JSON.JsonDecode(result);
                if (jsonHash["players"] != null)
                {
                    ArrayList jsonResults = (ArrayList)jsonHash["players"];
                    //Player with Stats
                    foreach (object objResult in jsonResults)
                    {
                        string stringvalue = String.Empty;
                        int intvalue = 0;
                        double doublevalue = 0;
                        Hashtable playerData = (Hashtable)objResult;
                        if (playerData != null && lstSoldierName.Contains(playerData["name"].ToString()) == true)
                        {
                            stringvalue = playerData["name"].ToString();
                            this.DebugInfo("Info", "Got BFBC2 stats for " + stringvalue);
                            int.TryParse(playerData["rank"].ToString(), out intvalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Rank = intvalue;
                            int.TryParse(playerData["kills"].ToString(), out intvalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Kills = intvalue;
                            int.TryParse(playerData["deaths"].ToString(), out intvalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Deaths = intvalue;
                            int.TryParse(playerData["score"].ToString(), out intvalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Score = intvalue;
                            double.TryParse(playerData["elo"].ToString(), out doublevalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Elo = doublevalue;
                            double.TryParse(playerData["level"].ToString(), out doublevalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Skilllevel = doublevalue;
                            double.TryParse(playerData["time"].ToString(), out doublevalue);
                            this.StatsTracker[stringvalue].BFBCS_Stats.Time = doublevalue;
                            this.StatsTracker[stringvalue].BFBCS_Stats.Updated = true;
                            // check Stats
                            this.checkPlayerStats(stringvalue, this.m_strReasonMsg);

                        }
                    }
                }
                if (jsonHash["players_unknown"] != null)
                {
                    //Player without Stats
                    ArrayList jsonResults_2 = (ArrayList)jsonHash["players_unknown"];
                    foreach (object objResult in jsonResults_2)
                    {
                        Hashtable playerData = (Hashtable)objResult;
                        if (playerData != null && lstSoldierName.Contains(playerData["name"].ToString()) == true)
                        {
                            this.DebugInfo("Info", "No Stats found for Player: " + playerData["name"].ToString());
                        }
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", " DownloadBFBCS: " + c);
                foreach (string SoldierName in lstSoldierName)
                {
                    this.StatsTracker[SoldierName].BFBCS_Stats.Updated = false;
                }
            }
        }


        public void RemovePlayerfromServer(string targetSoldierName, string strReason, string removeAction)
        {
            try
            {
                if (targetSoldierName == string.Empty)
                {
                    return;
                }
                switch (removeAction)
                {
                    case "Kick":
                        this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", targetSoldierName, strReason);
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Kicked Player: " + targetSoldierName + " - " + strReason);
                        break;

                    case "PBBan":
                        this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_ban \"{0}\" \"{1}\"", targetSoldierName, "BC2! " + strReason));
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1PB-Ban for Player: " + targetSoldierName + " - " + strReason);
                        break;

                    case "EAGUIDBan":
                        this.ExecuteCommand("procon.protected.send", "banList.add", "guid", this.StatsTracker[targetSoldierName].EAGuid, "perm", strReason);
                        this.ExecuteCommand("procon.protected.send", "banList.save");
                        this.ExecuteCommand("procon.protected.send", "banList.list");
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1EA-GUID Ban for Player: " + targetSoldierName + " - " + strReason);
                        break;
                    case "Nameban":
                        this.ExecuteCommand("procon.protected.send", "banList.add", "name", targetSoldierName, "perm", strReason);
                        this.ExecuteCommand("procon.protected.send", "banList.save");
                        this.ExecuteCommand("procon.protected.send", "banList.list");
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Nameban for Player: " + targetSoldierName + " - " + strReason);
                        break;
                    case "Warn":
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1Warning Player: " + targetSoldierName + " - " + strReason);
                        break;
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", " RemovePlayerfromServer: " + c);
            }
        }*/

        private void calculateAwards()
        {
            //Disabled temp
            return;
            /*
            string[] arrPlace = new string[] { "None", "None", "None" };
            int[] arrScores = new int[] { 0, 0, 0 };
            string BestCombat = "None";
            int BestCombat_kills = 0;
            // Place 1 to 3
            if (this.bool_roundStarted == true && this.StatsTracker.Count >= 4)
            {
                foreach (KeyValuePair<string, CStats> kvp in this.StatsTracker)
                {
                    //Place 1. to 3.
                    if (kvp.Value.Score > arrScores[0])
                    {
                        // 2. to 3.
                        arrScores[2] = arrScores[1];
                        arrPlace[2] = arrPlace[1];
                        // 1. to 2.
                        arrScores[1] = arrScores[0];
                        arrPlace[1] = arrPlace[0];
                        // New 1.
                        arrScores[0] = kvp.Value.Score;
                        arrPlace[0] = kvp.Key;
                    }
                    else if (kvp.Value.Score > arrScores[1])
                    {
                        // 2. to 3.
                        arrScores[2] = arrScores[1];
                        arrPlace[2] = arrPlace[1];
                        //New 2. 
                        arrScores[1] = kvp.Value.Score;
                        arrPlace[1] = kvp.Key;
                    }
                    else if (kvp.Value.Score > arrScores[2])
                    {
                        //New 3. 
                        arrScores[2] = kvp.Value.Score;
                        arrPlace[2] = kvp.Key;
                    }
                    //Most Kills - Best Combat
                    if (kvp.Value.Kills >= 5 && BestCombat_kills < kvp.Value.Kills)
                    {
                        BestCombat = kvp.Key;
                        BestCombat_kills = kvp.Value.Kills;
                    }
                    
                }
                //Set Awards
                //1.Place
                if (arrPlace[0] != null && String.Equals(arrPlace[0], "None") == false)
                {
                    this.StatsTracker[arrPlace[0]].Awards.dicAdd("First", 1);
                }
                //2.Place
                if (arrPlace[1] != null && String.Equals(arrPlace[1], "None") == false)
                {
                    this.StatsTracker[arrPlace[1]].Awards.dicAdd("Second", 1);
                }
                //3.Place
                if (arrPlace[1] != null && String.Equals(arrPlace[2], "None") == false)
                {
                    this.StatsTracker[arrPlace[2]].Awards.dicAdd("Third", 1);
                }
                //Best Combat
                if (BestCombat != null && String.Equals(BestCombat, "None") == false)
                {
                    this.StatsTracker[BestCombat].Awards.dicAdd("Best_Combat", 1);
                }
            }
            */
        }

        public void Threadstarter_Webrequest()
        {
            if (this.m_enWebrequest == enumBoolYesNo.Yes)
            {
                //Temp disabled
                // new Thread(Webrequest).Start();
            }
        }

        public void Webrequest()
        {
            /*
            try
            {
                this.DebugInfo("Info", "Thread started and calling the Website:  " + this.m_webAddress);
                using (WebClient wc = new WebClient())
                {
                    string result = wc.DownloadString(this.m_webAddress);
                    if (result.Length > 0)
                    {
                        this.DebugInfo("Info", "Got response from Webserver!");
                    }
                    else
                    {
                        this.DebugInfo("Warning", "Webrequest: Page(" + this.m_webAddress + ") not found!");
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Webrequest: " + c);
            }
             */
        }

        public void generateWeaponList()
        {
            this.DebugInfo("Trace", "generateWeaponList");
            List<string> weapList = new List<string>();
            this.weaponDic.Clear();
            this.DamageClass.Clear();
            try
            {
                WeaponDictionary weapons = this.GetWeaponDefines();
                foreach (PRoCon.Core.Players.Items.Weapon weapon in weapons)
                {
                    string[] weaponName = Regex.Replace(weapon.Name.Replace("Weapons/", "").Replace("Gadgets/", ""),@"XP\d_","").Split('/');
                    if (weapList.Contains(weaponName[0].Replace(' ', '_').Replace(".", "").Replace("U_", "")) == false)
                    {
                        weapList.Add(weaponName[0].Replace(' ', '_').Replace(".", "").Replace("U_", ""));
                    }
                    if (this.weaponDic.ContainsKey(weapon.Damage.ToString()) == false)
                    {
                        this.weaponDic.Add(weapon.Damage.ToString(), new Dictionary<string, CStats.CUsedWeapon>());
                    }
                    if (this.weaponDic[weapon.Damage.ToString()].ContainsKey(weapon.Name) == false)
                    {
                        this.weaponDic[weapon.Damage.ToString()].Add(weapon.Name, new CStats.CUsedWeapon(weapon.Name, weaponName[0].Replace(' ', '_').Replace(".", "").Replace("U_", ""),weapon.Slot.ToString(),weapon.KitRestriction.ToString()));
                    }
                    this.DamageClass.Add(weapon.Name, weapon.Damage.ToString());
                }
                this.PrepareKeywordDic();
            }
            catch (Exception e)
            {
                this.DebugInfo("Error", "generateWeaponList: " + e.ToString());
            }
            foreach (KeyValuePair<string, Dictionary<string, CStats.CUsedWeapon>> branch in this.weaponDic)
            {
                foreach (KeyValuePair<string, CStats.CUsedWeapon> leap in branch.Value)
                {
                    this.DebugInfo("Trace", "Weaponlist: DamageType: " + branch.Key + " Name: " + leap.Key);
                }
            }
        }

        private void getUpdateServerID(CServerInfo csiServerInfo)
        {
            try
            {
                //return;
                this.DebugInfo("Trace", "getUpdateServerID");
                /*
                this.DebugInfo("Trace", "ExternalGameIpandPort: " + this.m_strHostName + ":" + this.m_strPort);
                this.DebugInfo("Trace","ExternalGameIpandPort: "+ csiServerInfo.ExternalGameIpandPort);
                this.DebugInfo("Trace","ServerName: "+csiServerInfo.ServerName);
                this.DebugInfo("Trace","ConnectionState: "+csiServerInfo.ConnectionState);
                this.DebugInfo("Trace","CurrentRound: "+csiServerInfo.CurrentRound.ToString());
                //this.DebugInfo("Trace","GameMod: "+csiServerInfo.GameMod);
                this.DebugInfo("Trace","GameMode: "+csiServerInfo.GameMode);
                this.DebugInfo("Trace","JoinQueueEnabled: "+csiServerInfo.JoinQueueEnabled);
                this.DebugInfo("Trace","Map: "+csiServerInfo.Map);
                this.DebugInfo("Trace","Mappack: "+csiServerInfo.Mappack);
                this.DebugInfo("Trace","MaxPlayerCount: "+csiServerInfo.MaxPlayerCount);
                this.DebugInfo("Trace","Passworded: "+csiServerInfo.Passworded.ToString());
                this.DebugInfo("Trace","PlayerCount: "+csiServerInfo.PlayerCount);
                this.DebugInfo("Trace","Punkbuster: "+csiServerInfo.PunkBuster.ToString());
                this.DebugInfo("Trace","PunkBusterVersion: "+csiServerInfo.PunkBusterVersion);
                this.DebugInfo("Trace","Ranked: "+csiServerInfo.Ranked.ToString());
                this.DebugInfo("Trace","RoundTime: "+csiServerInfo.RoundTime.ToString());
                this.DebugInfo("Trace","ServerRegion: "+csiServerInfo.ServerRegion);
                this.DebugInfo("Trace","ServerUptime: "+csiServerInfo.ServerUptime.ToString());
                
                this.DebugInfo("Trace","TotalRounds: "+csiServerInfo.TotalRounds);
                
                */
                //this.DebugInfo("Trace", "TeamScores: " + csiServerInfo.TeamScores.Count.ToString());

                this.tablebuilder();
                DataTable resultTable;
                string SQL = String.Empty;
                int attemptCount = 0;
                bool success = false;
                using (MySqlConnection DBConnection = new MySqlConnection(this.DBConnectionStringBuilder()))
                {
                    MySql.Data.MySqlClient.MySqlTransaction Tx = null;
                    try
                    {
                        DBConnection.Open();
                        using (MySqlCommand MyCommand = new MySqlCommand("SELECT `ServerID` FROM " + this.tbl_server + @" WHERE IP_Address = @IP_Address"))
                        {
                            MyCommand.Parameters.AddWithValue("@IP_Address", this.m_strHostName + ":" + this.m_strPort);
                            resultTable = this.SQLquery(MyCommand);
                            if (resultTable.Rows != null)
                            {
                                foreach (DataRow row in resultTable.Rows)
                                {
                                    //this.ServerID = Convert.ToInt32(row[0]);
                                    int.TryParse(row[0].ToString(), out this.ServerID);
                                    this.DebugInfo("Trace", "DB returns ServerID = " + this.ServerID);
                                }
                            }
                        }
                        if (ServerID <= 0)
                        {
                            SQL = @"INSERT INTO " + tbl_server + @" (IP_Address, ServerName, ServerGroup, usedSlots, maxSlots, mapName, GameID, Gamemode) VALUES (@IP_Address, @ServerName, @ServerGroup, @usedSlots, @maxSlots, @mapName, @GameID, @Gamemode)";
                        }
                        else
                        {
                            SQL = @"UPDATE " + tbl_server + @" SET ServerName = @ServerName, ServerGroup = @ServerGroup , usedSlots = @usedSlots, maxSlots = @maxSlots, mapName = @mapName, GameID = @GameID, Gamemode = @Gamemode WHERE IP_Address = @IP_Address";
                        }
                        while (attemptCount < this.TransactionRetryCount && !success)
                        {
                            attemptCount++;
                            try
                            {
                                Tx = DBConnection.BeginTransaction();
                                using (MySqlCommand MySqlCom = new MySqlCommand(SQL, DBConnection, Tx))
                                {
                                    MySqlCom.Parameters.AddWithValue("@IP_Address", this.m_strHostName + ":" + this.m_strPort);
                                    MySqlCom.Parameters.AddWithValue("@ServerName", csiServerInfo.ServerName);
                                    MySqlCom.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                                    MySqlCom.Parameters.AddWithValue("@usedSlots", csiServerInfo.PlayerCount);
                                    MySqlCom.Parameters.AddWithValue("@maxSlots", csiServerInfo.MaxPlayerCount);
                                    MySqlCom.Parameters.AddWithValue("@mapName", csiServerInfo.Map);
                                    MySqlCom.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                                    MySqlCom.Parameters.AddWithValue("@Gamemode", csiServerInfo.GameMode);
                                    MySqlCom.ExecuteNonQuery();
                                    if (ServerID == 0)
                                    {
                                        int.TryParse(MySqlCom.LastInsertedId.ToString(), out this.ServerID);
                                    }
                                }
                                if (ServerID > 0 && this.m_enableCurrentPlayerstatsTable == enumBoolYesNo.Yes && csiServerInfo.TeamScores.Count > 0)
                                {
                                    string ScoreSQL = "DELETE FROM `" + this.tbl_teamscores + "` WHERE `ServerID` = @ServerID";
                                    using (MySqlCommand MySqlCom = new MySqlCommand(ScoreSQL, DBConnection, Tx))
                                    {
                                        MySqlCom.Parameters.AddWithValue("@ServerID", ServerID);
                                        MySqlCom.ExecuteNonQuery();
                                    }
                                    foreach (TeamScore teamscore in csiServerInfo.TeamScores)
                                    {
                                        //this.DebugInfo("Trace", "Update Score Table TeamID: " + teamscore.TeamID );
                                        ScoreSQL = "INSERT INTO `" + this.tbl_teamscores + "` (`ServerID`,`TeamID`,`Score`,`WinningScore`) VALUES(@ServerID, @TeamID, @Score, @WinningScore)";
                                        using (MySqlCommand MySqlCom = new MySqlCommand(ScoreSQL, DBConnection, Tx))
                                        {
                                            MySqlCom.Parameters.AddWithValue("@ServerID", ServerID);
                                            MySqlCom.Parameters.AddWithValue("@TeamID", teamscore.TeamID);
                                            MySqlCom.Parameters.AddWithValue("@Score", teamscore.Score);
                                            MySqlCom.Parameters.AddWithValue("@WinningScore", teamscore.WinningScore);
                                            MySqlCom.ExecuteNonQuery();
                                        }
                                    }
                                }
                                Tx.Commit();
                                success = true;
                            }
                            catch (MySqlException ex)
                            {
                                switch (ex.Number)
                                {
                                    case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                    case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                        this.DebugInfo("Warning", "Warning in getUpdateServer: Lock timeout or Deadlock occured restarting Transaction #1. Attempt: " + attemptCount);
                                        try
                                        {
                                            Tx.Rollback();
                                        }
                                        catch { }
                                        Thread.Sleep(attemptCount * 1000);
                                        break;
                                    default:
                                        throw; //Other exceptions
                                }
                            }
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "getUpdateServerID1: " + c);
                        try
                        {
                            Tx.Rollback();
                        }
                        catch { }
                    }
                    finally
                    {
                        try
                        {
                            DBConnection.Close();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "getUpdateServerID1: " + c);
            }
        }

        private void UpdateRanking()
        {
            try
            {
                //retrycount
                int attemptCount = 0;
                bool success = false;

                //ScoreRanking per server
                /*
                string sqlupdate1 = @"UPDATE " + this.tbl_playerstats + @" tps
                                    INNER JOIN (
                                                SELECT(@num := @num+1) AS rankScore, tsp.StatsID
                                                FROM " + this.tbl_playerstats + @" tps 
                                                STRAIGHT_JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID ,(SELECT @num := 0) x           
                                                WHERE tsp.ServerID = ?
                                                ORDER BY tps.Score DESC, tps.StatsID ASC 
                                                ) sub 
                                    ON sub.StatsID = tps.StatsID
                                    SET tps.rankScore = sub.rankScore
                                    WHERE sub.rankScore != tps.rankScore";
                 */
                string sqlupdate1 = @"UPDATE " + this.tbl_playerstats + @" tps
                                INNER JOIN (
                                            SELECT (@num := @num+1) AS rankScore, innersub.StatsID FROM
                                                (   
                                                    SELECT tsp.StatsID
                                                    FROM " + this.tbl_playerstats + @" tps 
                                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID ,(SELECT @num := 0) x           
                                                    WHERE tsp.ServerID = @ServerID
                                                    ORDER BY tps.Score DESC, tps.StatsID ASC 
                                                ) innersub
                                            ) sub 
                                ON sub.StatsID = tps.StatsID
                                SET tps.rankScore = sub.rankScore
                                WHERE sub.rankScore != tps.rankScore";


                //KillsRanking per server
                /*
                string sqlupdate2 = @"UPDATE " + this.tbl_playerstats + @" tps
                                      INNER JOIN (
                                                SELECT(@num := @num+1) AS rankKills, tsp.StatsID
                                                FROM " + this.tbl_playerstats + @" tps 
                                                STRAIGHT_JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID ,(SELECT @num := 0) y           
                                                WHERE tsp.ServerID = ?
                                                ORDER BY tps.Kills DESC, tps.Deaths ASC , tps.StatsID ASC 
                                                ) sub 
                                      ON sub.StatsID = tps.StatsID
                                      SET tps.rankKills = sub.rankKills
                                      WHERE tps.rankKills != sub.rankKills";
                 */

                string sqlupdate2 = @"UPDATE " + this.tbl_playerstats + @" tps
                                INNER JOIN (
                                            SELECT (@num := @num+1) AS rankKills, innersub.StatsID FROM
                                                (   
                                                    SELECT tsp.StatsID
                                                    FROM " + this.tbl_playerstats + @" tps 
                                                    INNER JOIN " + this.tbl_server_player + @" tsp ON tsp.StatsID = tps.StatsID ,(SELECT @num := 0) x           
                                                    WHERE tsp.ServerID = @ServerID
                                                    ORDER BY tps.Kills DESC, tps.Deaths ASC , tps.StatsID ASC 

                                                ) innersub
                                            ) sub 
                                ON sub.StatsID = tps.StatsID
                                SET tps.rankKills = sub.rankKills
                                WHERE sub.rankKills != tps.rankKills";

                // Global Updates
                string sqlInsert = @"INSERT INTO " + this.tbl_playerrank + @" (PlayerID, ServerGroup)
                                    SELECT PlayerID, (" + this.intServerGroup + @") AS ServerGroup
                                    FROM " + this.tbl_playerdata + @" 
                                    WHERE PlayerID NOT IN (SELECT PlayerID FROM " + this.tbl_playerrank + @" WHERE ServerGroup = @ServerGroup)";


                string sqlupdate3 = @"  UPDATE " + this.tbl_playerrank + @" tpr
                                    INNER JOIN (SELECT (@num := @num + 1) AS rankKills, sub1.PlayerID ,sub1.ServerGroup
                                                      FROM(SELECT tsp.PlayerID, ts.ServerGroup
                                                           FROM " + this.tbl_server_player + @" tsp
                                                           INNER JOIN " + this.tbl_server + @" ts ON tsp.ServerID = ts.ServerID
                                                           INNER JOIN " + this.tbl_playerstats + @" tps  ON  tsp.StatsID = tps.StatsID ,(SELECT @num := 0) x 
                                                           WHERE ts.ServerGroup = @ServerGroup
                                                           GROUP BY tsp.PlayerID, ts.ServerGroup 
                                                           ORDER BY SUM(tps.Kills) DESC, SUM(tps.Deaths) ASC, tsp.PlayerID ASC  
                                                     ) sub1 
                                                ) sub
                                    ON sub.PlayerID = tpr.PlayerID
                                    SET tpr.rankKills = sub.rankKills
                                    WHERE tpr.rankKills != sub.rankKills AND sub.ServerGroup = tpr.ServerGroup";


                string sqlupdate4 = @"  UPDATE " + this.tbl_playerrank + @" tpr
                                    INNER JOIN (SELECT (@num := @num + 1) AS rankScore, sub1.PlayerID ,sub1.ServerGroup
                                                      FROM(SELECT tsp.PlayerID, ts.ServerGroup
                                                           FROM " + this.tbl_server_player + @" tsp
                                                           INNER JOIN " + this.tbl_server + @" ts ON tsp.ServerID = ts.ServerID 
                                                           INNER JOIN " + this.tbl_playerstats + @" tps  ON  tsp.StatsID = tps.StatsID ,(SELECT @num := 0) y
                                                           WHERE ts.ServerGroup = @ServerGroup
                                                           GROUP BY tsp.PlayerID, ts.ServerGroup 
                                                           ORDER BY SUM(tps.Score) DESC, tsp.PlayerID ASC  
                                                     ) sub1 
                                                ) sub
                                    ON sub.PlayerID = tpr.PlayerID AND sub.ServerGroup = tpr.ServerGroup
                                    SET tpr.rankScore = sub.rankScore
                                    WHERE tpr.rankScore != sub.rankScore";

                MySql.Data.MySqlClient.MySqlTransaction Tx = null;
                using (MySqlConnection Con = new MySqlConnection(this.DBConnectionStringBuilder()))
                {
                    try
                    {
                        if (Con.State == ConnectionState.Closed)
                        {
                            Con.Open();
                        }

                        if (boolSkipServerUpdate == false)
                        {
                            while (attemptCount < this.TransactionRetryCount && !success)
                            {
                                attemptCount++;
                                try
                                {
                                    Tx = Con.BeginTransaction();
                                    using (MySqlCommand Command = new MySqlCommand(sqlupdate1, Con, Tx))
                                    {
                                        Command.Parameters.AddWithValue("@ServerID", this.ServerID);
                                        Command.ExecuteNonQuery();
                                    }
                                    //Commit
                                    Tx.Commit();
                                    success = true;
                                }
                                catch (MySqlException ex)
                                {
                                    switch (ex.Number)
                                    {
                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                            this.DebugInfo("Warning", "Warning in UpdateRanking: Lock timeout or Deadlock occured restarting Transaction #1. Attempt: " + attemptCount);
                                            try
                                            {
                                                Tx.Rollback();
                                            }
                                            catch { }
                                            Thread.Sleep(attemptCount * 1000);
                                            break;
                                        default:
                                            throw; //Other exceptions
                                    }
                                }
                            }
                            if (attemptCount > this.TransactionRetryCount)
                            {
                                this.DebugInfo("Error", "Error in UpdateRanking: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction #1)");
                            }
                            attemptCount = 0;
                            success = false;

                            //Next query
                            while (attemptCount < this.TransactionRetryCount && !success)
                            {
                                attemptCount++;
                                try
                                {
                                    //Start new Transaction
                                    Tx = Con.BeginTransaction();
                                    using (MySqlCommand Command = new MySqlCommand(sqlupdate2, Con, Tx))
                                    {
                                        Command.Parameters.AddWithValue("@ServerID", this.ServerID);
                                        Command.ExecuteNonQuery();
                                    }
                                    //Commit
                                    Tx.Commit();
                                    success = true;
                                }
                                catch (MySqlException ex)
                                {
                                    switch (ex.Number)
                                    {
                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                            this.DebugInfo("Warning", "Warning in UpdateRanking: Lock timeout or Deadlock occured restarting Transaction #2. Attempt: " + attemptCount);
                                            try
                                            {
                                                Tx.Rollback();
                                            }
                                            catch { }
                                            Thread.Sleep(attemptCount * 1000);
                                            break;
                                        default:
                                            throw; //Other exceptions
                                    }
                                }
                            }
                            if (attemptCount > this.TransactionRetryCount)
                            {
                                this.DebugInfo("Error", "Error in UpdateRanking: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction #2)");
                            }
                        }
                        attemptCount = 0;
                        success = false;

                        //Next query
                        if (boolSkipGlobalUpdate == false)
                        {
                            while (attemptCount < this.TransactionRetryCount && !success)
                            {
                                attemptCount++;
                                try
                                {
                                    //Start new Transaction
                                    Tx = Con.BeginTransaction();
                                    using (MySqlCommand Command = new MySqlCommand(sqlInsert, Con, Tx))
                                    {
                                        Command.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                                        Command.ExecuteNonQuery();
                                    }
                                    //Commit
                                    Tx.Commit();
                                    success = true;
                                }
                                catch (MySqlException ex)
                                {
                                    switch (ex.Number)
                                    {
                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                            this.DebugInfo("Warning", "Warning in UpdateRanking: Lock timeout or Deadlock occured restarting Transaction #3. Attempt: " + attemptCount);
                                            try
                                            {
                                                Tx.Rollback();
                                            }
                                            catch { }
                                            Thread.Sleep(attemptCount * 1000);
                                            break;
                                        default:
                                            throw; //Other exceptions
                                    }
                                }
                            }
                            if (attemptCount > this.TransactionRetryCount)
                            {
                                this.DebugInfo("Error", "Error in UpdateRanking: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction #3)");
                            }
                            attemptCount = 0;
                            success = false;

                            //Next query
                            while (attemptCount < this.TransactionRetryCount && !success)
                            {
                                attemptCount++;
                                try
                                {
                                    //Start new Transaction
                                    Tx = Con.BeginTransaction();
                                    using (MySqlCommand Command = new MySqlCommand(sqlupdate3, Con, Tx))
                                    {
                                        Command.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                                        Command.ExecuteNonQuery();
                                    }
                                    //Commit
                                    Tx.Commit();
                                    success = true;
                                }
                                catch (MySqlException ex)
                                {
                                    switch (ex.Number)
                                    {
                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                            this.DebugInfo("Warning", "Warning in UpdateRanking: Lock timeout or Deadlock occured restarting Transaction #4. Attempt: " + attemptCount);
                                            try
                                            {
                                                Tx.Rollback();
                                            }
                                            catch { }
                                            Thread.Sleep(attemptCount * 1000);
                                            break;
                                        default:
                                            throw; //Other exceptions
                                    }
                                }
                            }
                            if (attemptCount > this.TransactionRetryCount)
                            {
                                this.DebugInfo("Error", "Error in UpdateRanking: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction #4)");
                            }
                            attemptCount = 0;
                            success = false;

                            //Next query
                            while (attemptCount < this.TransactionRetryCount && !success)
                            {
                                attemptCount++;
                                try
                                {
                                    //Start new Transaction
                                    Tx = Con.BeginTransaction();
                                    using (MySqlCommand Command = new MySqlCommand(sqlupdate4, Con, Tx))
                                    {
                                        Command.Parameters.AddWithValue("@ServerGroup", this.intServerGroup);
                                        Command.ExecuteNonQuery();
                                    }
                                    Tx.Commit();
                                    success = true;
                                }
                                catch (MySqlException ex)
                                {
                                    switch (ex.Number)
                                    {
                                        case 1205: //(ER_LOCK_WAIT_TIMEOUT) Lock wait timeout exceeded
                                        case 1213: //(ER_LOCK_DEADLOCK) Deadlock found when trying to get lock
                                            this.DebugInfo("Warning", "Warning in UpdateRanking: Lock timeout or Deadlock occured restarting Transaction #5. Attempt: " + attemptCount);
                                            try
                                            {
                                                Tx.Rollback();
                                            }
                                            catch { }
                                            Thread.Sleep(attemptCount * 1000);
                                            break;
                                        default:
                                            throw; //Other exceptions
                                    }
                                }
                            }
                            if (attemptCount > this.TransactionRetryCount)
                            {
                                this.DebugInfo("Error", "Error in UpdateRanking: Maximum number of " + this.TransactionRetryCount + " transaction retrys exceeded (Transaction #5)");
                            }
                        }
                        attemptCount = 0;
                        success = false;
                    }
                    catch (MySqlException oe)
                    {
                        this.DebugInfo("Error", "Error in UpdateRanking: ");
                        this.DisplayMySqlErrorCollection(oe);
                        if (Tx != null)
                        {
                            try
                            {
                                Tx.Rollback();
                            }
                            catch { };
                        }
                    }
                    catch (Exception c)
                    {
                        this.DebugInfo("Error", "Error in UpdateRanking: " + c);
                        if (Tx != null)
                        {
                            try
                            {
                                Tx.Rollback();
                            }
                            catch { };
                        }
                    }
                    finally
                    {
                        try
                        {
                            Con.Close();
                        }
                        catch { };
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in UpdateRanking: " + c);
            }
        }

        private void checkWelcomeStatsDic()
        {
            try
            {
                lock (this.welcomestatsDic)
                {
                    TimeSpan duration = new TimeSpan(0, 10, 0);
                    List<string> entryToRemove = new List<string>();
                    foreach (KeyValuePair<string, DateTime> kvp in this.welcomestatsDic)
                    {
                        if (duration < (MyDateTime.Now - kvp.Value))
                        {
                            entryToRemove.Add(kvp.Key);
                        }
                    }
                    foreach (string entry in entryToRemove)
                    {
                        this.DebugInfo("Trace", "Removing Player " + entry + " from welcomestatslist  Timeoutlimit of 10 minutes was exceeded!");
                        this.welcomestatsDic.Remove(entry);
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in checkWelcomeStatsDic: " + c);
            }
        }

        private void BuildRegexRuleset()
        {
            try
            {
                this.lstChatFilterRules = new List<Regex>();
                foreach (string strRule in this.lstStrChatFilterRules)
                {
                    this.lstChatFilterRules.Add(new Regex(strRule.Replace("&#124", "|").Replace("&#124","+")));
                }

                if (this.GlobalDebugMode.Equals("Trace"))
                {
                    this.DebugInfo("Trace", "Active Regex-Ruleset:");
                    foreach (Regex regexrule in this.lstChatFilterRules)
                    {
                        this.DebugInfo("Trace", regexrule.ToString());
                    }
                }
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in BuildRegexRuleset: " + c);
            }
        }

        private void SendMultiLineChatMessage(List<string> lstMultiLineChatMSG, int intDelay, int delayIncreasePerLine , string strScope, string targetPlayerName)
        {
            int totalDelay = intDelay;
            int yellduration = 8;
            string duration = string.Empty;
            string yelltagwithduration = @"^\[[y|Y][e|E][l|L]{2,2},\d+\]";
            //string yelltag = @"^\[[y|Y][e|E][l|L]{2,2},";
            try
            {
                switch (strScope)
                {
                    case "all":
                        foreach (string line in lstMultiLineChatMSG)
                        {

                            if (Regex.IsMatch(line, yelltagwithduration))
                            {
	                            MatchCollection matches = Regex.Matches(line, yelltagwithduration);
	                            foreach (Match match in matches)
	                            {
	                                foreach (Capture capture in match.Captures)
	                                {
		                                if(int.TryParse(Regex.Replace(match.Value,@"\D",""), out yellduration) == false )
                                        {
                                            this.DebugInfo("Trace", "SendMultiLineChatMessage: Could not parse Duration, using default"); 
                                            yellduration = 8;
                                        }
	                                }
	                            }
                                //yell this!
                                this.CheckMessageLength(Regex.Replace(line,yelltagwithduration,"") , 255);
                                this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger", totalDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", Regex.Replace(line, yelltagwithduration, ""), yellduration.ToString(), strScope);
                                totalDelay += delayIncreasePerLine;
                                totalDelay += yellduration;
                            }
                            else
                            {
                                //default say
                                this.CheckMessageLength(line, 128);
                                this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger", totalDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", line, strScope);
                                totalDelay += delayIncreasePerLine;
                            }
                        }
                        break;

                    default:
                        foreach (string line in lstMultiLineChatMSG)
                        {
                            if (Regex.IsMatch(line, yelltagwithduration))
                            {
                                MatchCollection matches = Regex.Matches(line, yelltagwithduration);
                                foreach (Match match in matches)
                                {
                                    foreach (Capture capture in match.Captures)
                                    {
                                        if (int.TryParse(Regex.Replace(match.Value, @"\D", ""), out yellduration) == false)
                                        {
                                            this.DebugInfo("Trace", "SendMultiLineChatMessage: Could not parse Duration, using default"); 
                                            yellduration = 8;
                                        }
                                    }
                                }
                                //yell this!
                                this.CheckMessageLength(line, 255);
                                this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger", totalDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", Regex.Replace(line, yelltagwithduration, ""), yellduration.ToString(), "player", targetPlayerName);
                                totalDelay += delayIncreasePerLine;
                                totalDelay += yellduration;
                            }
                            else
                            {
                                //default say
                                this.CheckMessageLength(line, 128);
                                this.ExecuteCommand("procon.protected.tasks.add", "CChatGUIDStatsLogger", totalDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", line, "player", targetPlayerName);
                                totalDelay += delayIncreasePerLine;
                            }
                        }
                        break;
                }

            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in PostMultiLineChat: " + c);
            }
        }

        private Dictionary<string, int> GetWeaponMappingfromDB()
        {
            Dictionary<string,int> mappingDic = new Dictionary<string,int>();
            try
            {
                string sqlSelect = "SELECT `WeaponID`,`Fullname` FROM `" + this.tbl_weapons + @"` WHERE `GameID` = @GameID";
                using (MySqlCommand SelectCommand = new MySqlCommand(sqlSelect))
                {
                    SelectCommand.Parameters.AddWithValue("@GameID", this.intServerGameType_ID);
                   
                    DataTable result = this.SQLquery(SelectCommand);
                    if (result != null || result.Rows.Count != 0)
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            mappingDic.Add(row["Fullname"].ToString(), Convert.ToInt32(row["WeaponID"]));
                            this.DebugInfo("Trace", "WeaponMapping: ID: " + Convert.ToInt32(row["WeaponID"]).ToString() + " <--> Weapon:" + row["Fullname"].ToString());
                        }
                    }
                }
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "Error in GetWeaponMappingfromDB: ");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in GetWeaponMappingfromDB: " + c);
            }

            return mappingDic;
        }

        private int GetGameIDfromDB(string strGame)
        {
            this.DebugInfo("Trace", "GetGameIDfromDB Game: " + strGame);
            int intGameID = 0;
            try
            {
                string sqlSelect = "SELECT `GameID` FROM `" + this.tbl_games + @"` WHERE `Name` = @Name";
                using (MySqlCommand SelectCommand = new MySqlCommand(sqlSelect))
                {
                    SelectCommand.Parameters.AddWithValue("@Name", strGame);

                    DataTable result = this.SQLquery(SelectCommand);
                    if (result.Rows.Count != 0)
                    {
                        intGameID = Convert.ToInt32(result.Rows[0][0]);
                    }
                    else
                    {
                        this.DebugInfo("Trace", "GetGameIDfromDB Game:  no gameID found");
                        //Insert Game
                        using (MySqlConnection Con = new MySqlConnection(this.DBConnectionStringBuilder()))
                        {
                            Con.Open();
                            MySqlTransaction Transaction = null;
                            //Start of the Transaction
                            Transaction = Con.BeginTransaction();

                            string SQL = @"INSERT INTO `" + this.tbl_games + @"` (`Name`) VALUES (@Name)";
                            using (MySqlCommand MyCom = new MySqlCommand(SQL, Con, Transaction))
                            {
                                MyCom.Parameters.AddWithValue("@Name", this.strServerGameType);
                                MyCom.ExecuteNonQuery();
                                this.DebugInfo("Trace", "GetGameIDfromDB LastInsertedId: " + MyCom.LastInsertedId.ToString());
                                intGameID = Convert.ToInt32(MyCom.LastInsertedId);
                            }
                            Transaction.Commit();
                        }
                    }
                }
            }
            catch (MySqlException oe)
            {
                this.DebugInfo("Error", "Error in GetGameIDfromDB: ");
                this.DisplayMySqlErrorCollection(oe);
            }
            catch (Exception c)
            {
                this.DebugInfo("Error", "Error in GetGameIDfromDB: " + c);
            }
            this.DebugInfo("Trace", "GetGameIDfromDB GameID: " + intGameID);
            return intGameID;
        }
    }

        #endregion

    #region Classes
    /*==========Classes========*/
    class CLogger
    {
        private readonly string _Name;
        private string _Message = String.Empty;
        private string _Subset = String.Empty;
        private DateTime _Time;

        public string Name
        {
            get { return _Name; }
        }

        public string Message
        {
            get { return _Message; }
        }

        public string Subset
        {
            get { return _Subset; }
        }

        public DateTime Time
        {
            get { return _Time; }
        }

        public CLogger(DateTime time, string name, string message, string subset)
        {
            _Name = name;
            _Message = message;
            _Subset = subset;
            _Time = time;
        }
    }

    class CStats
    {
        private string _ClanTag;
        private string _Guid;
        private string _EAGuid;
        private string _IP;
        private string _PlayerCountryCode;
        private int _Score = 0;
        private int _HighScore = 0;
        private int _LastScore = 0;
        private int _Kills = 0;
        private int _Headshots = 0;
        private int _Deaths = 0;
        private int _Suicides = 0;
        private int _Teamkills = 0;
        private int _Playtime = 0;
        private int _Rounds = 0;
        private DateTime _Playerjoined;
        private DateTime _TimePlayerleft;
        private DateTime _TimePlayerjoined;
        private int _PlayerleftServerScore = 0;
        private bool _playerOnServer = true;
        private int _rank = 0;
        //KD Correction
        private int _beforeleftKills = 0;
        private int _beforeleftDeaths = 0;
        //Streaks
        private int _Killstreak;
        private int _Deathstreak;
        private int _Killcount;
        private int _Deathcount;
        //Wins&Loses
        private int _Wins = 0;
        private int _Losses = 0;
        //TeamID
        private int _TeamId = 0;
        //BFBCS
        private CBFBCS _BFBCS_Stats;
        private myDateTime MyDateTime = new myDateTime(0);
        public Dictionary<string, Dictionary<string, CStats.CUsedWeapon>> dicWeap = new Dictionary<string, Dictionary<string, CStats.CUsedWeapon>>();

        //Awards
        private CAwards _Awards;

        //global Rank
        private int _GlobalRank = 0;

        public string ClanTag
        {
            get { return _ClanTag; }
            set { _ClanTag = value; }
        }

        public string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }
        }

        public string EAGuid
        {
            get { return _EAGuid; }
            set { _EAGuid = value; }
        }

        public string IP
        {
            get { return _IP; }
            set { _IP = value.Remove(value.IndexOf(":")); }
        }

        public string PlayerCountryCode
        {
            get { return _PlayerCountryCode; }
            set { _PlayerCountryCode = value; }
        }

        public int Score
        {
            get { return _Score; }
            set { _Score = value; }
        }

        public int HighScore
        {
            get { return _HighScore; }
            set { _HighScore = value; }
        }

        public int LastScore
        {
            get { return _LastScore; }
            set { _LastScore = value; }
        }

        public int Kills
        {
            get { return _Kills; }
            set { _Kills = value; }
        }

        public int BeforeLeftKills
        {
            get { return _beforeleftKills; }
            set { _beforeleftKills = value; }
        }

        public int Headshots
        {
            get { return _Headshots; }
            set { _Headshots = value; }
        }

        public int Deaths
        {
            get { return _Deaths; }
            set { _Deaths = value; }
        }

        public int BeforeLeftDeaths
        {
            get { return _beforeleftDeaths; }
            set { _beforeleftDeaths = value; }
        }

        public int Suicides
        {
            get { return _Suicides; }
            set { _Suicides = value; }
        }

        public int Teamkills
        {
            get { return _Teamkills; }
            set { _Teamkills = value; }
        }

        public int Playtime
        {
            get { return _Playtime; }
            set { _Playtime = value; }
        }

        public int Rounds
        {
            get { return _Rounds; }
            set { _Rounds = value; }
        }

        public DateTime Playerjoined
        {
            get { return _Playerjoined; }
            set { _Playerjoined = value; }
        }

        public DateTime TimePlayerleft
        {
            get { return _TimePlayerleft; }
            set { _TimePlayerleft = value; }
        }

        public DateTime TimePlayerjoined
        {
            get { return _TimePlayerjoined; }
            set { _TimePlayerjoined = value; }
        }

        public int PlayerleftServerScore
        {
            get { return _PlayerleftServerScore; }
            set { _PlayerleftServerScore = value; }
        }

        public bool PlayerOnServer
        {
            get { return _playerOnServer; }
            set { _playerOnServer = value; }
        }

        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public int Killstreak
        {
            get { return _Killstreak; }
            set { _Killstreak = value; }
        }

        public int Deathstreak
        {
            get { return _Deathstreak; }
            set { _Deathstreak = value; }
        }

        public int Wins
        {
            get { return _Wins; }
            set { _Wins = value; }
        }

        public int Losses
        {
            get { return _Losses; }
            set { _Losses = value; }
        }

        public int TeamId
        {
            get { return _TeamId; }
            set { _TeamId = value; }
        }

        public int GlobalRank
        {
            get { return _GlobalRank; }
            set { _GlobalRank = value; }
        }

        //Methodes	
        public void AddScore(int intScore)
        {
            if (intScore != 0)
            {
                this._Score = this._Score + (intScore - this._LastScore);
                this._LastScore = intScore;
                if (intScore > this._HighScore)
                {
                    this._HighScore = intScore;
                }
            }
            else
            {
                this._LastScore = 0;
            }
        }

        public double KDR()
        {
            double ratio = 0;
            if (this._Deaths != 0)
            {
                ratio = Math.Round(Convert.ToDouble(this._Kills) / Convert.ToDouble(this._Deaths), 2);
            }
            else
            {
                ratio = this._Kills;
            }
            return ratio;
        }

        public Dictionary<string, Dictionary<string, CStats.CUsedWeapon>> getWeaponKills()
        {
            return this.dicWeap;
        }

        public void addKill(string strDmgType, string strweaponType, bool blheadshot)
        {
            this._Kills++;
            if (blheadshot)
            {
                if (this.dicWeap.ContainsKey(strDmgType))
                {
                    if (this.dicWeap[strDmgType].ContainsKey(strweaponType))
                    {
                        this.dicWeap[strDmgType][strweaponType].Kills++;
                        this.dicWeap[strDmgType][strweaponType].Headshots++;
                    }
                }
                this._Headshots++;
            }
            else
            {
                if (this.dicWeap.ContainsKey(strDmgType))
                {
                    if (this.dicWeap[strDmgType].ContainsKey(strweaponType))
                    {
                        this.dicWeap[strDmgType][strweaponType].Kills++;
                    }
                }
            }
            //Killstreaks
            this._Killcount++;
            this._Deathcount = 0;
            if (this._Killcount > this._Killstreak)
            {
                this._Killstreak = this._Killcount;
            }
            //Awardchecks
            this._Awards.CheckOnKill(_Kills, _Headshots, _Deaths, _Killcount, _Deathcount);
        }

        public void addDeath(string strDmgType, string strweaponType)
        {
            this._Deaths++;
            if (this.dicWeap.ContainsKey(strDmgType))
            {
                if (this.dicWeap[strDmgType].ContainsKey(strweaponType))
                {
                    this.dicWeap[strDmgType][strweaponType].Deaths++;
                }
            }
            //Deathstreak
            this._Deathcount++;
            this._Killcount = 0;
            if (this._Deathcount > this._Deathstreak)
            {
                this._Deathstreak = this._Deathcount;
            }
            //Awardchecks
            this._Awards.CheckOnDeath(_Kills, _Headshots, _Deaths, _Killcount, _Deathcount);
        }

        public void playerleft()
        {
            //Score
            this._PlayerleftServerScore += this._Score;
            this._Score = 0;
            //Kd Correction
            this._beforeleftKills += this._Kills;
            this._beforeleftDeaths += this._Deaths;

            //Time
            TimeSpan duration = MyDateTime.Now - this._Playerjoined;
            this._Playtime += Convert.ToInt32(duration.TotalSeconds);
            this._playerOnServer = false;
        }

        public int TotalScore
        {
            get { return (this._PlayerleftServerScore + this._Score); }
        }

        public int TotalPlaytime
        {
            get
            {
                if (this._playerOnServer)
                {
                    TimeSpan duration = MyDateTime.Now - this._Playerjoined;
                    return (this._Playtime + Convert.ToInt32(duration.TotalSeconds));
                }
                return this._Playtime;
            }
        }

        public CStats.CBFBCS BFBCS_Stats
        {
            get { return _BFBCS_Stats; }
            set { _BFBCS_Stats = value; }
        }

        public CStats.CAwards Awards
        {
            get { return _Awards; }
            set { _Awards = value; }
        }

        public class CUsedWeapon
        {
            private string _Name = "";
            private string _FieldName = "";
            private string _Slot = "";
            private string _KitRestriction = "";
            private int _Kills = 0;
            private int _Headshots = 0;
            private int _Deaths = 0;

            public int Kills
            {
                get { return _Kills; }
                set { _Kills = value; }
            }

            public int Headshots
            {
                get { return _Headshots; }
                set { _Headshots = value; }
            }

            public int Deaths
            {
                get { return _Deaths; }
                set { _Deaths = value; }
            }

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            public string FieldName
            {
                get { return _FieldName; }
                set { _FieldName = value; }
            }

            public string Slot
            {
                get { return _Slot; }
                set { _Slot = value; }
            }

            public string KitRestriction
            {
                get { return _KitRestriction; }
                set { _KitRestriction = value; }
            }

            public CUsedWeapon(string name, string fieldname, string slot, string kitrestriction)
            {
                this._Name = name;
                this._FieldName = fieldname;
                this._Slot = slot;
                this._KitRestriction = kitrestriction;
                this._Kills = 0;
                this._Headshots = 0;
                this._Deaths = 0;
            }
        }

        public class CBFBCS
        {
            private int _rank;
            private int _kills;
            private int _deaths;
            private int _score;
            private double _skilllevel;
            private double _time;
            private double _elo;
            private bool _Updated;
            private bool _fetching;
            private bool _noUpdate;

            public int Rank
            {
                get { return _rank; }
                set { _rank = value; }
            }

            public int Kills
            {
                get { return _kills; }
                set { _kills = value; }
            }

            public int Deaths
            {
                get { return _deaths; }
                set { _deaths = value; }
            }

            public double KDR
            {
                get
                {
                    double ratio = 0;
                    if (this._deaths != 0)
                    {
                        ratio = Math.Round(Convert.ToDouble(this._kills) / Convert.ToDouble(this._deaths), 2);
                    }
                    else
                    {
                        ratio = this._kills;
                    }
                    return ratio;
                }
            }
            public double SPM
            {
                get
                {
                    return Convert.ToDouble(this._score) / (this._time / 60);
                }
            }

            public int Score
            {
                get { return _score; }
                set { _score = value; }
            }

            public double Skilllevel
            {
                get { return _skilllevel; }
                set { _skilllevel = value; }
            }

            public double Time
            {
                get { return _time; }
                set { _time = value; }
            }

            public double Elo
            {
                get { return _elo; }
                set { _elo = value; }
            }

            public bool Updated
            {
                get { return _Updated; }
                set { _Updated = value; }
            }

            public bool Fetching
            {
                get { return _fetching; }
                set { _fetching = value; }
            }

            public bool NoUpdate
            {
                get { return _noUpdate; }
                set { _noUpdate = value; }
            }

            public CBFBCS()
            {
                this._rank = 0;
                this._kills = 0;
                this._deaths = 0;
                this._score = 0;
                this._skilllevel = 0;
                this._time = 0;
                this._elo = 0;
                this._Updated = false;
                this._fetching = false;
                this._noUpdate = false;
            }
        }

        public class CAwards
        {
            //Awards
            private Dictionary<string, int> _dicAwards = new Dictionary<string, int>();

            //Constructor
            public CAwards()
            {
                this._dicAwards = new Dictionary<string, int>();
            }

            //Get and Set
            public Dictionary<string, int> DicAwards
            {
                get { return _dicAwards; }
                set { _dicAwards = value; }
            }

            //Methodes
            public void dicAdd(string strAward, int count)
            {
                if (this._dicAwards.ContainsKey(strAward))
                {
                    this._dicAwards[strAward] = this._dicAwards[strAward] + count;
                }
                else
                {
                    this._dicAwards.Add(strAward, count);
                }
            }

            public void CheckOnKill(int kills, int hs, int deaths, int ks, int ds)
            {
                //Purple Heart
                if (kills >= 5 && deaths >= 20 && ((Double)kills / (Double)deaths) == 0.25)
                {
                    this.dicAdd("Purple_Heart", 1);
                }
                //Killstreaks
                if (ks == 5)
                {
                    //5 Kills in a row
                    this.dicAdd("Killstreak_5", 1);
                }
                else if (ks == 10)
                {
                    //10 kills in a row
                    this.dicAdd("Killstreak_10", 1);
                }
                else if (ks == 15)
                {
                    //15 kills in a row
                    this.dicAdd("Killstreak_15", 1);
                }
                else if (ks == 20)
                {
                    //20 kills in a row
                    this.dicAdd("Killstreak_20", 1);
                }
            }

            public void CheckOnDeath(int kills, int hs, int deaths, int ks, int ds)
            {
                //Purple Heart
                if (kills >= 5 && deaths >= 20 && ((Double)kills / (Double)deaths) == 0.25)
                {
                    this.dicAdd("Purple_Heart", 1);
                }
            }
        }

        public class myDateTime
        {
            private double _offset = 0;

            public DateTime Now
            {
                get
                {
                    DateTime dateValue = DateTime.Now;
                    return dateValue.AddHours(_offset);
                }
            }
            public myDateTime(double offset)
            {
                this._offset = offset;
            }
        }

        public CStats(string guid, int score, int kills, int headshots, int deaths, int suicides, int teamkills, int playtime, double timeoffset, Dictionary<string, Dictionary<string, CStats.CUsedWeapon>> _weaponDic)
        {
            this.MyDateTime = new myDateTime(timeoffset);
            this._ClanTag = String.Empty;
            this._Guid = guid;
            this._EAGuid = String.Empty;
            this._IP = String.Empty;
            this._Score = score;
            this._LastScore = 0;
            this._HighScore = score;
            this._Kills = kills;
            this._Headshots = headshots;
            this._Deaths = deaths;
            this._Suicides = suicides;
            this._Teamkills = teamkills;
            this._Playtime = playtime;
            this._Rounds = 0;
            this._PlayerleftServerScore = 0;
            this._PlayerCountryCode = String.Empty;
            this._Playerjoined = MyDateTime.Now;
            this._TimePlayerjoined = this._Playerjoined;
            this._TimePlayerleft = DateTime.MinValue;
            this._rank = 0;
            this._Killcount = 0;
            this._Killstreak = 0;
            this._Deathcount = 0;
            this._Deathstreak = 0;
            this._Wins = 0;
            this._Losses = 0;
            this.BFBCS_Stats = new CStats.CBFBCS();    
            this._Awards = new CAwards();
            //this.dicWeap = new Dictionary<string,Dictionary<string,CUsedWeapon>>(_weaponDic);
            foreach (KeyValuePair<string, Dictionary<string, CStats.CUsedWeapon>> pair in _weaponDic)
            {
                this.dicWeap.Add(pair.Key, new Dictionary<string, CStats.CUsedWeapon>());
                foreach (KeyValuePair<string, CStats.CUsedWeapon> subpair in pair.Value)
                {
                    this.dicWeap[pair.Key].Add(subpair.Key, new CStats.CUsedWeapon(subpair.Value.Name, subpair.Value.FieldName,subpair.Value.Slot,subpair.Value.KitRestriction));
                }
            }
        }
    }

    class C_ID_Cache
    {
        private int _Id;
        private int _StatsID;
        private bool _PlayeronServer;

        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public int StatsID
        {
            get { return _StatsID; }
            set { _StatsID = value; }
        }

        public bool PlayeronServer
        {
            get { return _PlayeronServer; }
            set { _PlayeronServer = value; }
        }
        //Constructor
        public C_ID_Cache(int statsid, int id, bool playeronServer)
        {
            this._Id = id;
            this._StatsID = statsid;
            this._PlayeronServer = playeronServer;
        }
    }

    class CKillerVictim
    {
        string _Killer = String.Empty;
        string _Victim = String.Empty;

        public string Killer
        {
            get { return _Killer; }
            set { _Killer = value; }
        }

        public string Victim
        {
            get { return _Victim; }
            set { _Victim = value; }
        }

        public CKillerVictim(string killer, string victim)
        {
            this._Killer = killer;
            this._Victim = victim;
        }
    }

    class CMapstats
    {
        private DateTime _timeMaploaded;
        private DateTime _timeMapStarted;
        private DateTime _timeRoundEnd;
        private string _strMapname = String.Empty;
        private string _strGamemode = String.Empty;
        private int _intRound;
        private int _intNumberOfRounds;
        private List<int> _lstPlayers;
        private int _intMinPlayers;
        private int _intMaxPlayers;
        private int _intServerplayermax;
        private double _doubleAvgPlayers;
        private int _intplayerleftServer;
        private int _intplayerjoinedServer;
        private myDateTime MyDateTime = new myDateTime(0);

        public DateTime TimeMaploaded
        {
            get { return _timeMaploaded; }
            set { _timeMaploaded = value; }
        }

        public DateTime TimeMapStarted
        {
            get { return _timeMapStarted; }
            set { _timeMapStarted = value; }
        }

        public DateTime TimeRoundEnd
        {
            get { return _timeRoundEnd; }
            set { _timeRoundEnd = value; }
        }

        public string StrMapname
        {
            get { return _strMapname; }
            set { _strMapname = value; }
        }

        public string StrGamemode
        {
            get { return _strGamemode; }
            set { _strGamemode = value; }
        }

        public int IntRound
        {
            get { return _intRound; }
            set { _intRound = value; }
        }

        public int IntNumberOfRounds
        {
            get { return _intNumberOfRounds; }
            set { _intNumberOfRounds = value; }
        }

        public List<int> LstPlayers
        {
            get { return _lstPlayers; }
            set { _lstPlayers = value; }
        }

        public int IntMinPlayers
        {
            get { return _intMinPlayers; }
            set { _intMinPlayers = value; }
        }

        public int IntMaxPlayers
        {
            get { return _intMaxPlayers; }
            set { _intMaxPlayers = value; }
        }

        public int IntServerplayermax
        {
            get { return _intServerplayermax; }
            set { _intServerplayermax = value; }
        }

        public double DoubleAvgPlayers
        {
            get { return _doubleAvgPlayers; }
            set { _doubleAvgPlayers = value; }
        }

        public int IntplayerleftServer
        {
            get { return _intplayerleftServer; }
            set { _intplayerleftServer = value; }
        }

        public int IntplayerjoinedServer
        {
            get { return _intplayerjoinedServer; }
            set { _intplayerjoinedServer = value; }
        }

        public void MapStarted()
        {
            this._timeMapStarted = MyDateTime.Now;
        }

        public void MapEnd()
        {
            this._timeRoundEnd = MyDateTime.Now;
        }

        public void ListADD(int entry)
        {
            this._lstPlayers.Add(entry);
        }

        public void calcMaxMinAvgPlayers()
        {
            this._intMaxPlayers = 0;
            this._intMinPlayers = _intServerplayermax;
            this._doubleAvgPlayers = 0;
            int entries = 0;
            foreach (int playercount in this._lstPlayers)
            {
                if (playercount >= this._intMaxPlayers)
                    this._intMaxPlayers = playercount;

                if (playercount <= this._intMinPlayers)
                    this._intMinPlayers = playercount;

                this._doubleAvgPlayers = this._doubleAvgPlayers + playercount;
                entries = entries + 1;

            }
            if (entries != 0)
            {
                this._doubleAvgPlayers = this._doubleAvgPlayers / (Convert.ToDouble(entries));
                this._doubleAvgPlayers = Math.Round(this._doubleAvgPlayers, 1);
            }
            else
            {
                this._doubleAvgPlayers = 0;
                this._intMaxPlayers = 0;
                this._intMinPlayers = 0;
            }
        }

        public class myDateTime
        {
            private double _offset = 0;

            public DateTime Now
            {
                get
                {
                    DateTime dateValue = DateTime.Now;
                    return dateValue.AddHours(_offset);
                }
            }
            public myDateTime(double offset)
            {
                this._offset = offset;
            }
        }

        public CMapstats(DateTime timeMaploaded, string strMapname, int intRound, int intNumberOfRounds, double timeoffset)
        {
            this._timeMaploaded = timeMaploaded;
            this._strMapname = strMapname;
            this._intRound = intRound;
            this._intNumberOfRounds = intNumberOfRounds;
            this._intMaxPlayers = 32;
            this._intServerplayermax = 32;
            this._intMinPlayers = 0;
            this._intplayerjoinedServer = 0;
            this._intplayerleftServer = 0;
            this._lstPlayers = new List<int>();
            this._timeMapStarted = DateTime.MinValue;
            this._timeRoundEnd = DateTime.MinValue;
            this._strGamemode = String.Empty;
            this.MyDateTime = new myDateTime(timeoffset);
        }
    }

    class CSpamprotection
    {
        private Dictionary<string, int> dicplayer;
        private int _allowedRequests;

        public CSpamprotection(int allowedRequests)
        {
            this._allowedRequests = allowedRequests;
            this.dicplayer = new Dictionary<string, int>();
        }

        public bool isAllowed(string strSpeaker)
        {
            bool result = false;
            if (this.dicplayer.ContainsKey(strSpeaker) == true)
            {
                int i = this.dicplayer[strSpeaker];
                if (0 >= i)
                {
                    //Player is blocked
                    result = false;
                    this.dicplayer[strSpeaker]--;
                }
                else
                {
                    //Player is not blocked
                    result = true;
                    this.dicplayer[strSpeaker]--;
                }
            }
            else
            {
                this.dicplayer.Add(strSpeaker, this._allowedRequests);
                result = true;
                this.dicplayer[strSpeaker]--;
            }
            return result;
        }

        public void Reset()
        {
            this.dicplayer.Clear();
        }
    }

    class myDateTime_W
    {
        private double _offset = 0;

        public DateTime Now
        {
            get
            {
                DateTime dateValue = DateTime.Now;
                return dateValue.AddHours(_offset);
            }
        }
        public myDateTime_W(double offset)
        {
            this._offset = offset;
        }
    }

    class CStatsIngameCommands
    {
        //Class variables
        private string _functioncall;
        private string _commands;
        private string _description;
        private bool _boolEnabled;


        public CStatsIngameCommands(string commands, string functioncall, bool boolEnabled, string description)
        {
            this._commands = commands;
            this._functioncall = functioncall;
            this._boolEnabled = boolEnabled;
            this._description = description;

        }

        public string commands
        {
            get { return this._commands; }
            set { this._commands = value; }
        }

        public string functioncall
        {
            get { return this._functioncall; }
            set { this._functioncall = value; }
        }

        public string description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        public bool boolEnabled
        {
            get { return this._boolEnabled; }
            set { this._boolEnabled = value; }
        }
    }

   #endregion
}
