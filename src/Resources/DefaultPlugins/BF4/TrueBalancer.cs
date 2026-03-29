/*  Copyright 2011 Panther

    This plugin is made for PRoCon.

    BFBC2 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BF3 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BF3 PRoCon.  If not, see <http://www.gnu.org/licenses/>.
    
    Use this script at your own risk!
 */

/* support: 
 * https://forum.myrcon.com/showthread.php?7169-TrueBalancer-BF3-BF4-0-5-RC
 * 
 * grizzlybeer
 * https://forum.myrcon.com/member.php?13930-grizzlybeer
 * 
 * TODO:
 * - remove tb-move cmds (use procons !move)
 * ok - exclude commanders, spectators
 * ok - persona id fix
 * 
 * 
 * 
 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;


using System.Web;
using System.Net;
using System.Threading;
using System.Diagnostics;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class TrueBalancer : PRoConPluginAPI, IPRoConPluginInterface {

        #region Variables and Constructors
        
        //Proconvariables
        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private string Servertype;

        private DateTime lastupdatecheck=DateTime.Now.AddHours(-4);

        //TrueBalancer Variables    

        BattlelogClient bclient;
        
        private Dictionary<string, CPlayerJoinInf> dicPlayerCache = new Dictionary<string, CPlayerJoinInf>();
        private Dictionary<int, CPlayerScoreInf> dicPlayerScore = new Dictionary<int, CPlayerScoreInf>();
        private Dictionary<int, CSquadScoreInf> dicSquadScore = new Dictionary<int, CSquadScoreInf>();
        private Dictionary<string, int> dicSquadList= new Dictionary<string, int>();
        private Dictionary<string, bool> OnCommandMove = new Dictionary<string, bool>();
        private List<string> OnCommandMoveDone;

        private string[] strAWhitelist;
        private string[] strAClantagWhitelist;
        private string[] strAClantagWhitelistScrambler;
        private string[] strAWhitelistComplete;
        private List<string> teamswitcher;
        private List<string> BalancedPlayers;
        //private List<string> PlayersOnServer;
        private string strWarning;
        private string strLastWarning;
        private string strBeenMoved;
        private string strMovedPlayer;
        private string strJoinedPlayerName;
        private string strShameMessage;
        
        private string strScrambleDoneMsg;
        private string strScrambleNowMsg;
        private string strScrambleRoundMsg;
        
        private string strFinalSquad;
        private string strErrorMsg;
        private string strdeadplayer;   
            
        private int intInterval;
        private int intWarnings;
        private int intTimerWait;
        private int intI;
        
        private int TeamA;
        private int TeamB;
        private int intPlayerDif;
        private int intToTeam;
        private int intFromTeam;
        private int intNewTeam;
        private int intcountWarnings;
        private int intWaitSeconds;
        private int intScoreTeamA;
        private int intScoreTeamB;
        private int intTicketcount;
        
        private int bestSquadTeamID;
        private int intTicketsdif;
        private int intCurrentRoundCount;
        private int intMaxSlots;
        private int intScrambledPlayers;
        private int intSquadA;
        private int intSquadB;
        private int intScrambleCount;
        private int intPlayerCache;
        private double dblValueDiffRUSH;
        private double dblValueDiffCONQUEST;
        private double dblValueDiffDOM;
        private double dblValueDiffTDM;
        private double dblValueDiffGM;
        private int intScoreWTS;
        
        private int intAllowDif;
        private int intminScore;
        
        private int intminScoreRUSH;
        private int intTreshRUSH;       
        private int intAllowDif1RUSH;
        private int intAllowDif2RUSH;

        private int intTreshGM;
        private int intAllowDif1GM;
        private int intAllowDif2GM;

        private int intTreshDF;
        private int intAllowDif1DF;
        private int intAllowDif2DF;

        private int intminScoreCONQUEST;
        private int intTreshCONQUEST;
        private int intAllowDif1CONQUEST;
        private int intAllowDif2CONQUEST;

        private int intminScoreDOM;
        private int intTreshDOM;
        private int intAllowDif1DOM;
        private int intAllowDif2DOM;

        private int intminScoreOB;
        private int intTreshOB;
        private int intAllowDif1OB;
        private int intAllowDif2OB;

        private int intminScoreTDM;
        private int intTreshTDM;
        private int intAllowDif1TDM;
        private int intAllowDif2TDM;
        
        private double rankA;
        private double rankB;
        private double skillA;
        private double skillB;
        private double spmA;
        private double spmB;
        private double spmcombatA;
        private double spmcombatB;
        private double kdrA;
        private double kdrB;
        private double TBvalueA;
        private double TBvalueB;
        
        private enumBoolYesNo ynbDebugMode;
        private enumBoolYesNo ynbDebugModeSkill;
        private enumBoolYesNo ynbDebugModeGuard;
        private enumBoolYesNo ynbWhitelist;
        private enumBoolYesNo ynbShowWarnings;
        private enumBoolYesNo ynbShowBallancing;
        private enumBoolYesNo ynbShowPlayermessage;
        private enumBoolYesNo ynbLoneWolf;
        private enumBoolYesNo ynbincludeVIPlist;
        private enumBoolYesNo ynbBalancingGuard;
        private enumBoolYesNo ynbShameMessage;
        private enumBoolYesNo ynbScrambleMessage;
        private enumBoolYesNo ynbEnableScrambleNow;
        private enumBoolYesNo ynbEnableScrambleRound;
        private enumBoolYesNo ynbYellScrambleManuall;
        private enumBoolYesNo ynbYellScrambleMessage;

        private enumBoolYesNo ynbScrambleRoundViaPRoCon;
        private enumBoolYesNo ynbScrambleRoundViaPRoConConf;

        private string strScrambleMode;

        private enumBoolYesNo ynbenableSkillRUSH;
        private enumBoolYesNo ynbScrambleMapRUSH;
        private string ScrambleByRUSH;

        private enumBoolYesNo ynbenableSkillGM;
        private enumBoolYesNo ynbScrambleMapGM;
        private string ScrambleByGM;

        private enumBoolYesNo ynbenableSkillDF;
        private enumBoolYesNo ynbScrambleMapDF;
        private string ScrambleByDF;
        
        private enumBoolYesNo ynbenableSkillCONQUEST;
        private enumBoolYesNo ynbScrambleMapCONQUEST;
        private enumBoolYesNo ynbScrambleEveryRoundCONQUEST;
        private int intwonTicketsCONQUEST;
        private string strScrambleMessageCONQUEST;
        private string ScrambleByCONQUEST;

        private enumBoolYesNo ynbenableSkillDOM;
        private enumBoolYesNo ynbScrambleMapDOM;
        private enumBoolYesNo ynbScrambleEveryRoundDOM;
        private int intwonTicketsDOM;
        private string strScrambleMessageDOM;
        private string ScrambleByDOM;

        //private enumBoolYesNo ynbenableSkillOB;
        //private enumBoolYesNo ynbScrambleMapOB;
        //private string ScrambleByOB;

        private enumBoolYesNo ynbenableSkillOB;
        private enumBoolYesNo ynbScrambleMapOB;
        private enumBoolYesNo ynbScrambleEveryRoundOB;
        private int intwonTicketsOB;
        private string strScrambleMessageOB;
        private string ScrambleByOB;
 
        private enumBoolYesNo ynbenableSkillTDM;
        private enumBoolYesNo ynbScrambleMapTDM;
        private enumBoolYesNo ynbScrambleEveryRoundTDM;
        private int intwonTicketsTDM;
        private int intshowTicketsTDM;
        private string strScrambleMessageTDM;
        private string ScrambleByTDM;
        
        private bool m_isPluginEnabled;
        private bool boolplayerexists;
        private bool boolneedbalance;
        private bool booltimer;
        private bool boolstartBalance;
        private bool boolFirstOP;
        private bool boolLevelStart;
        private bool boolLevelLoaded;
        private bool boolRoundOver;
        private bool boolgametype;
        private bool boolafterBalance;
        private bool boolfirstwarningWL;
        private bool boolmanuellchange;
        private bool boolnoplayer;
        private bool boolscrambleActive;
        private bool boolscrambleNow; 
        private bool boolTeamsScrambled;
        private bool boolRunOnList;
        private bool boolplayerleft;
        private bool boolticketdif;
        private bool boolwaitfordeath;
        private bool boolwaitdead;
        private bool boolfirstscrambler;
        private bool boolscramblefailed;
        private bool boolmessagesent;
        private bool backswitcher;
        private bool boolbalanced;
        private bool boolscramblebyadminroundend;
        private bool showfirstmove;
                
        private string strcurrentGametype;      
        private TimeSpan TSWait;
        private DateTime DTScramblestarted;
        
        private DateTime DTLevelStart;
        private TimeSpan TSLevelStartWait;

        private TimeSpan TSForceMove;
        private DateTime DTForceMove;

        private TimeSpan EndRoundSeconds;
        private DateTime EndRoundTime;
        private DateTime DTLevelLoaded;
        
        //private Timer myTimer = new Timer();
        //private TimeSpan TSWaitforOP;
        // private bool boolOnLogin;        
        //private DateTime DTRoundOver;        

        private bool boolVirtual;
        private enumBoolYesNo ynbVirtualMode;
        private int intMaxPlayersToFetch;

        private enumBoolYesNo showMoves;
        private enumBoolYesNo Check4Update;
        
        public TrueBalancer() {

            //lastupdatecheck = DateTime.Now.AddHours(-4);

             this.bclient = new BattlelogClient(this);

            this.dicPlayerCache = new Dictionary<string, CPlayerJoinInf>(); 
            this.dicPlayerScore = new Dictionary<int, CPlayerScoreInf>();
            this.dicSquadScore = new Dictionary<int, CSquadScoreInf>();
            this.dicSquadList = new Dictionary<string, int>();
            this.OnCommandMove = new Dictionary<string,bool>();
            this.OnCommandMoveDone = new List<string>();

            this.Servertype = "AUTOMATIC";

            //this.strAWhitelist = new string[] { "HRPanter", "Name two", "Name three" };
            //this.strAClantagWhitelist = new string[] { "FoC", "ClanTag2", "ClanTag3" };
            //this.strAClantagWhitelistScrambler = new string[] { "FoC", "FoCr", "ClanTag3" }; ;
            this.strAWhitelist = new string[] { "HRPanter" };
            this.strAClantagWhitelist = new string[] { "FoC" };
            this.strAClantagWhitelistScrambler = new string[] { "FoC", "FoCr" }; ;
            this.strAWhitelistComplete = new string[] { };
            this.teamswitcher = new List<string>();
            this.BalancedPlayers = new List<string>();
            
            //this.PlayersOnServer = new List<string>(new string[] {"Z1", "Z2", "HRPanter"}); //panter
            this.strWarning = "EVEN TEAMS! Autobalancing teams shortly.";
            this.strLastWarning = "AUTOBALANCING [Player: %MovedPlayer%]";
            this.strBeenMoved = "You got balanced, because you have a low score and died.";
            this.strMovedPlayer = "";
            this.strJoinedPlayerName = "";
            this.strShameMessage = "%TeamSwitcher% tried to switch into the winning team. SHAME ON YOU!";
            
            this.strScrambleDoneMsg = "Teams are scrambled now. Good luck all!";
            this.strScrambleNowMsg = "Teams are going to be scrambled now. This may take up to 20 seconds!";
            this.strScrambleRoundMsg = "Teams are going to be scrambled on next round!";
            
            this.strFinalSquad = "";
            this.strErrorMsg = "";
            this.strdeadplayer = "";
            
            this.intInterval = 15;
            this.intWarnings = 1;
            this.intTimerWait = 15;
            this.intI = 0;
            this.TeamA = 0;
            this.TeamB = 0;
            this.intPlayerDif = 0;
            this.intToTeam = 0;
            this.intFromTeam = 0;
            this.intNewTeam = 0;
            this.intcountWarnings = 0;
            this.intWaitSeconds = 0;
            this.intScoreTeamA = 0; 
            this.intScoreTeamB = 0;     
            this.bestSquadTeamID = 0;
            this.intCurrentRoundCount = 100;
            this.intTicketsdif = -1;
            this.intMaxSlots = 0;
            this.intScrambledPlayers = 0;
            this.intSquadA = 0;
            this.intSquadB = 0;
            this.intScrambleCount = 0;
            this.intPlayerCache = 15;
            this.dblValueDiffRUSH = 10;
            this.dblValueDiffCONQUEST = 10;
            this.dblValueDiffDOM = 10;
            this.dblValueDiffTDM = 10;
            this.dblValueDiffGM = 10;
            this.intScoreWTS = 50;
            this.intTicketcount = 123987123;
            
            this.rankA = 0;
            this.rankB = 0;
            this.skillA = 0;
            this.skillB = 0;
            this.spmA = 0;
            this.spmB = 0;
            this.spmcombatA = 0;
            this.spmcombatB = 0;
            this.kdrA = 0;
            this.kdrB = 0;
            this.TBvalueA = 0;
            this.TBvalueB = 0;
            
            this.ynbDebugMode = enumBoolYesNo.No;
            this.ynbDebugModeSkill = enumBoolYesNo.No;
            this.ynbDebugModeGuard = enumBoolYesNo.No;
            //this.ynbWhitelist = enumBoolYesNo.No;
            this.ynbWhitelist = enumBoolYesNo.Yes;
            this.ynbShowWarnings = enumBoolYesNo.Yes;
            this.ynbShowBallancing = enumBoolYesNo.Yes;
            this.ynbShowPlayermessage = enumBoolYesNo.Yes;
            this.ynbLoneWolf = enumBoolYesNo.Yes;
            //this.ynbincludeVIPlist = enumBoolYesNo.No;
            this.ynbincludeVIPlist = enumBoolYesNo.Yes;
            //this.ynbShameMessage = enumBoolYesNo.Yes;
            //this.ynbBalancingGuard = enumBoolYesNo.No;
            this.ynbShameMessage = enumBoolYesNo.No;
            this.ynbBalancingGuard = enumBoolYesNo.Yes;
            this.ynbScrambleMessage = enumBoolYesNo.Yes;
            this.ynbEnableScrambleNow = enumBoolYesNo.No;
            this.ynbEnableScrambleRound = enumBoolYesNo.Yes;
            this.ynbYellScrambleManuall = enumBoolYesNo.Yes;
            this.ynbYellScrambleMessage = enumBoolYesNo.Yes;

            this.ynbScrambleRoundViaPRoCon = enumBoolYesNo.No;
            this.ynbScrambleRoundViaPRoConConf = enumBoolYesNo.No;
                
            this.backswitcher = false;
            this.boolplayerleft = false;
            this.m_isPluginEnabled = false;
            this.boolplayerexists = false;
            this.boolneedbalance = false;
            this.booltimer = false;
            this.boolstartBalance = false;
            this.boolLevelStart = false;
            this.boolLevelLoaded = true;
            this.boolRoundOver = false;
            this.boolgametype = false;
            this.boolafterBalance = false;
            this.boolFirstOP = false;
            this.boolfirstwarningWL = false;
            this.boolmanuellchange = false;
            this.boolnoplayer = false;
            this.boolscrambleActive = false;
            this.boolscrambleNow = false;
            this.boolTeamsScrambled = false;
            this.DTScramblestarted = new DateTime();
            this.boolRunOnList = false;
            this.boolticketdif = false;
            this.boolwaitfordeath = false;
            this.boolwaitdead = false;
            this.boolfirstscrambler = false;
            this.boolscramblefailed = false;
            this.boolmessagesent = false;
            this.boolbalanced = true;
            this.boolscramblebyadminroundend = false;
            this.showfirstmove = true;

            this.TSLevelStartWait = new TimeSpan(0);
            this.DTLevelStart = new DateTime();
            this.DTLevelLoaded = new DateTime();
            
            this.intAllowDif = 500;
            this.intminScore = 500;
            
            this.intminScoreRUSH = 15;
            this.intTreshRUSH = 24;
            this.intAllowDif1RUSH = 1;
            this.intAllowDif2RUSH = 2;

            this.intTreshGM = 16;
            this.intAllowDif1GM = 1;
            this.intAllowDif2GM = 2;

            this.intTreshDF = 16;
            this.intAllowDif1DF = 1;
            this.intAllowDif2DF = 2;

            this.intminScoreCONQUEST = 100;
            this.intTreshCONQUEST = 24;
            this.intAllowDif1CONQUEST = 1;
            this.intAllowDif2CONQUEST = 2;

            this.intminScoreDOM = 100;
            this.intTreshDOM = 24;
            this.intAllowDif1DOM = 1;
            this.intAllowDif2DOM = 2;

            this.intminScoreOB = 1;
            this.intTreshOB = 24;
            this.intAllowDif1OB = 1;
            this.intAllowDif2OB = 2;
            
            this.intminScoreTDM = 25;
            this.intTreshTDM = 24;
            this.intAllowDif1TDM = 1;
            this.intAllowDif2TDM = 2;

            //this.strScrambleMode = "Keep squads with two or more clanmates";
            this.strScrambleMode = "Keep all Squads";
            
            this.ynbenableSkillRUSH = enumBoolYesNo.No;
            this.ynbScrambleMapRUSH = enumBoolYesNo.No;
            this.ScrambleByRUSH = "TB-Value";

            this.ynbenableSkillGM = enumBoolYesNo.No;
            this.ynbScrambleMapGM = enumBoolYesNo.No;
            this.ScrambleByGM = "TB-Value";

            this.ynbenableSkillDF = enumBoolYesNo.No;
            this.ynbScrambleMapDF = enumBoolYesNo.No;
            this.ScrambleByDF = "TB-Value";

            //this.ynbenableSkillOB = enumBoolYesNo.No;
            //this.ynbScrambleMapOB = enumBoolYesNo.No;
            //this.ScrambleByOB = "TB-Value";

            //this.ynbenableSkillCONQUEST = enumBoolYesNo.No;
            this.ynbenableSkillCONQUEST = enumBoolYesNo.Yes;
            this.ynbScrambleMapCONQUEST = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundCONQUEST = enumBoolYesNo.No;
            this.intwonTicketsCONQUEST = 50;
            this.strScrambleMessageCONQUEST = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByCONQUEST = "TB-Value";

            this.ynbenableSkillDOM = enumBoolYesNo.Yes;
            this.ynbScrambleMapDOM = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundDOM = enumBoolYesNo.No;
            this.intwonTicketsDOM = 50;
            this.strScrambleMessageDOM = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByDOM = "TB-Value";

            //this.ynbenableSkillOB = enumBoolYesNo.No;
            this.ynbenableSkillOB = enumBoolYesNo.Yes;
            this.ynbScrambleMapOB = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundOB = enumBoolYesNo.No;
            this.intwonTicketsOB = 50;
            this.strScrambleMessageOB = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByOB = "TB-Value";

            //this.ynbenableSkillTDM = enumBoolYesNo.No;
            this.ynbenableSkillTDM = enumBoolYesNo.Yes;
            this.ynbScrambleMapTDM = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundTDM = enumBoolYesNo.No;
            this.intwonTicketsTDM = 50;
            this.intshowTicketsTDM = 50;
            this.strScrambleMessageTDM = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByTDM = "TB-Value";


            this.TSForceMove = new TimeSpan(0);
            this.DTForceMove = new DateTime();

            this.EndRoundSeconds = new TimeSpan(0);
            this.EndRoundTime = new DateTime();

            this.boolVirtual = false;
            this.ynbVirtualMode = enumBoolYesNo.No;
            this.intMaxPlayersToFetch = 2;

            showMoves = enumBoolYesNo.No;
            Check4Update = enumBoolYesNo.Yes;
        }

        #endregion

        #region PluginSetup

        public string GetPluginName() {
            return "TrueBalancer";
        }

        public string GetPluginVersion() {
            return "0.5.3.0";
        }

        public string GetPluginAuthor() {
            return "onegrizzlybeer, versions < 0.5 by Panther";
        }

        public string GetPluginWebsite() {
            return "forum.myrcon.com/showthread.php?7169-TrueBalancer-0-5-0-0";
        }

        // A note to plugin authors: DO NOT change how a tag works, instead make a whole new tag.
        public string GetPluginDescription() {
            return @"<p> ... and contributors from the Procon community.</p>
<p>If you like my plugin, please feel free to donate</p>
<blockquote>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""VVZUWJJ8UFA3Q"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>

</form>


</blockquote>
<br><br>
<h2>Description</h2>
    <p>This plugin is used to autobalance Teams in BF3 (All modes except SquadDeathMatch)<br>
    There are 3 major parts within TrueBalancer:<br><br>
    <u>1. PlayerNumber - Balancer</u> <br>
    <i>- The PN-Balancer try to keep the teams even by playernumber. Eg: 30vs30</i><br><br>
    <u>2. Skill- Scrambler</u><br>
    <i>- The Scrambler trys to even out the teams at roundend by skill. Eg: Even out the teams by SPM, Skill, K/D...</i><br><br>
    <u>3. Balancing Guard</u><br>
    <i>- Balancing Guard trys to keep teams even by skill during the game. He sorts new players to the teams they fit best and doesn't allow players to switch
    into the other team if they would unbalance the teams by PlayerNumber or Skill. In all Conquest Modes and TDM - Modes Balancing Guard doesn't allow winning team switching.</i><br><br>
    
    <b><u>IMPORTANT:</b></u><br>
    If you are running the plugins in sandboxmode, make sure you have these ports open:<br>
    https://battlelog.battlefield.com 443<br>
    http://battlelog.battlefield.com 80
    <br><br>
    This script is in Beta-Phase. Test at your own Risk. Report back please!</p>
<br><br>
<h2>Tutorials and recommended Settings</h2><br>
<p><center><font size=""+2""><a href=""http://youtu.be/hYG0NqjW5p0"" target=""_blank"" filter:Glow(color=#ff0000, strength=12);>TrueBalancer Part 1 of 3 - PlayerNumber Balancer</a><br>
<a href=""http://youtu.be/c5BhRIZYoFM"" target=""_blank"">TrueBalancer Part 2 of 3 - SkillScrambler</a><br>
<a href=""http://youtu.be/73a8_6jTHGU"" target=""_blank"">TrueBalancer Part 3 of 3 - BalancingGuard</a><br><br>
<a href=""http://www.phogue.net/forumvb/showthread.php?2866-TrueBalancer-for-BF3-(CQ-Rush-TDM)-0-2-7-(30-05-2012)&p=25828&viewfull=1#post25828"" target=""_blank"">RECOMMENDED SETTINGS</a><br>
</font></center></p>
<br><br>
<h2>InGame Commands</h2><br>
<p>@scramblenow: Will scramble the teams now. Use with caution. This will make players angry.<br>
@scrambleround: Will scramble the teams at roundend.<br><br>
Due to BalacingGuard the standart move/fmove are not working. TB has it's own commands:<br>
@tb-fmove: Force move a player into the other team.<br>
@tb-move: Move a player into the other team upon death.<br>
</p>    
<br><br>
<h2>Settings</h2><br>
<p>Here is a link for the recommended settings:<br>
<a href=""http://www.phogue.net/forumvb/showthread.php?2866-TrueBalancer-for-BF3-(CQ-Rush-TDM)-0-2-7-(30-05-2012)&p=25828&viewfull=1#post25828"" target=""_blank"">RECOMMENDED SETTINGS</a></p>
<br><br><br>
<h2>Things you need to know:</h2>
<h3>Minimum values for the settings</h3>
<table border ='1'>
    <tr><th>Setting</th><th>Minimum Value</th></tr>
    <tr><td>How many Warnings?</td><td>1</td></tr>
    <tr><td>Allow Player Difference of</td><td>1</td></tr>
    <tr><td>Time between Warnings in sec</td><td>1</td></tr>
</table>

<h3>Prefixes for TrueBalance</h3>
    <table border ='1'>
    <tr><th>Prefix</th><th>Effect</th></tr>
    <tr><td>%MovedPlayer%</td><td>Will be replaced by the moved player's name. Usable for: 'Message for moved Player' and 'Balancing Message'</td></tr>
    <tr><td>%Warning%</td><td>Will be replaced by the number of the current warning. Usable for: 'Warning Message'</td></tr>
    <tr><td>%maxWarnings%</td><td>Will be replaced by the number of maximal warnings. Usable for: 'Warning Message'</td></tr>
    </table>
    <br>
    
<h3>Changelog</h3><br>
<b>0.5.3.0</b><br>
- NEW: Added Gamemode Chainlink. Chainlink is treated just like Conquest.<br><br>
<b>0.5.2.0</b><br>
- Fix: Balancing was not working for Carrier Assault. Carrier Assault will use Conquest settings. On Carrier Assault its not possible to stop balancing once a certain point is reached (no proper way to detect for plugins)<br><br>
<b>0.5.1.1</b><br>
- Fix: Commanders and Spectators are now excluded from balancing<br>
- Fix: Persona ID could not be found for some players<br><br>
<b>0.5.1.0</b><br>
- NEW: Automatic Update Check<br>
- Fix: Domination settings block<br>
- Fix: Confirmation msg when scrambling at roundend requested by admin via procon<br>
- Fix: Settings for game mode Obliteration could not be altered.<br><br>
<b>0.5.0.0</b><br>
- NEW: BF4 compatibility<br><br>
<b>0.4.0.7 (Edits by EBastard)</b><br>
- Fix: Added Capture The Flag to plugin settings<br>
- Fix: scrambling for CTF<br><br>
<b>0.4.0.6 (Edits by EBastard)</b><br>
- ADD: Support for Air Superiority. Conquest settings also apply to this gamemodde.<br>
- ADD: Support for Capture the Flag. Gunmaster settings also apply to this gamemode.<br><br>
<b>0.4.0.2 - 0.4.0.5 (Edits by PapaCharlie9)</b><br>
- (0.4.0.5) Restores the tb-move and tb-fmove commands that were lost in the 0.4.0.4 patch<br>
- Reduces lag/delays due to stats fetching (but does not eliminate lag, see https://forum.myrcon.com/showthread.php?5623)<br>
- Reduces Procon panic errors, where the plugin seems to disable itself or disappear<br>
- Adds player name to StatsException messages<br>
- Adds a new plugin setting (see https://forum.myrcon.com/showthread.php?5623)<br><br>
<b>0.4.0.1 (Edit by EBastard)</b><br>
- ADD: Support for Scavenger. Conquest settings also apply to Scavenger.<br><br>
<b>0.4.0.0</b><br>
- ADD: Support for TankSuperiority. Conquest settings also apply to TankSuperiority.<br><br>
<b>0.3.5.5</b><br>
- Small Fix: When ticket till end was reached, the message to balance to server was shown, causing it to spam on high ticket servers. Removed this message.<br>
- Small Fix: Threshold adjusting to make it work as threshold is defined. Playerdifference below and equal to/above threshold.<br><br>
<b>0.3.5.0</b><br>
- HOTFIX: Workaround for ingame bug with 5 players in a squad.<br><br>
<b>0.3.4.0</b><br>
- CHANGE: Changing the scrambling mechanism for 'keep no squads' and 'keep squads with two or more clanmates'.<br><br>
<b>0.3.3.2</b><br>
- Fix: Problems with negativ Skills fixed. Negativ Skills are set to 0.<br>
- FIX: Problem with StatsReset feature. This got fixed by calculating all values by TrueBalancer.<br>
- FIX: Fixing a bug with Rush/GM and the BalancingGuard. Wrong values have been used.<br><br>
<b>0.3.3.1</b><br>
- ADDED: ClanTagWhitelist for the SkillScrambler, when using 'Keep squads with two or more clanmates'. Squads with at least one player, who contains one of the whitelisted ClanTags will not be teared apart. They will still be scrambled<br>
- ADDED: New Value 'TB-Value'. This is a combination of all the information/skills of a player.<br>
- FIX: Fixing a small bug in the scrambler. If a player can't be put into the squad of his mates, TB tries to assign a complete new squad to all playmates.<br><br>
<b>0.3.3</b><br>
- ADDED: TrueBalancer's own move/fmove - commands, to be able to move players, even if BalancingGuard is on.<br>
- ADDED: Possiblity to split squads while scrambling. Admins can choose what they want.<br><br>
<b>0.3.2</b><br>
- CHANGE: Changing form tickets to % in settings: Sramble at % ticketsdiff, stop winning teamswitching at x %.<br>
- CHANGE: TDM settings no need for 'show message at x tickets' anymore <br>
- TWEAK: BalancinGuard tweaked to automatically detect what team is better by all skills.<br>
- FIX: Fixing minor bug for moving dead players only (A player was still marked as dead upon revive)<br>
- FIX: Fixing TrueBalancer, which could get crazy if a manual roundrestart/nextround was requested.<br>
- ADDED: Possiblitly to execute manual command '!scrambleround' via PRoCon<br><br>
<b>0.3.1</b><br>
- FIXING: Domination bug fixed. TB didn't balance at all.<br>
- ADDED: Seperating GunMaster from Rush<br>
- FIXING: Minor issues with settings, not able to be set below a certain number<br><br>
<b>0.3.0</b><br>
- Adding support for CQC<br>
- NEW: Choose what your server should be scrambled/sorted by<br><br>
<b>0.2.8</b><br>
- Tweaking Balancing Guard to let the server breath more (not so tight settings).<br>
- Fixing some minor bugs with Rush<br><br>
<b>0.2.7</b><br>
- Fixing Balancing Guard: Moving Good players to the winning team and bad players to the loosing team on join. (When winning team had a lower SPMAverage than loosing team).<br>
- Fixing Balancing Guard: Definition for 'good' and 'bad' players was missing.<br>
Definition Before:<br>
- If SPM of a player is higher than the winning Teams SPMAverage, he was declared as good player. But a player with a SPM of 250 is far away from being good ;).<br>
Definition Now:<br>
- Good Player SPM >= 450<br>
- Bad Player SPM <= 250<br><br>
<b>0.2.6</b><br>
- Manual commands added to scramble teams now or at the end of a round.<br><br>
<b>0.2.5</b><br>
- Fixing Balancing Guard for Rush<br>
- Fixing TB for the newest Serverversion that could have gone to a critical stage at roundstart.<br><br>
<b>0.2.4</b><br>
- NEW FEATURE: Balancing Guard!<br>
- Does not allow players to teamswtich and unbalance the teams by playernumber.<br>
- Does not allow players to teamswitch to the winning team.<br>
- Sorting new players by SPM and adds them to the team they should be.<br><br>
<b>0.2.2</b><br>
- Added Threshold for all gamemodes: (Playernumberbalancer above and below that threshold can be set sepperate.)<br>
- Seperate settings for all GameModes (SquadDeathMatch is not supported)<br>
- Made the scrambler abit more safe against strane lags that can happen during scrambling.<br><br>
<b>0.2.0</b><br>
- Now scrambling by SPM (Battlelog).<br>
- Scrambling Message is yelled at roundend for 30 seconds.<br>
- NEW method to balance players. Now players are only going to be balanced when they are dead.<br><br>
<b>0.1.6</b><br>
- RUSH: Only way to scramble teams in rush is on every new map.<br><br>
<b>0.1.5</b><br>
- RUSHMode Support<br><br>
<b>0.1.4</b><br>
- FIX: Showing message at roundend when SkillScrambler is deactivated. This bug is fixed.<br><br>
<b>0.1.3</b><br>
- FIX: If the server is full, TB can't scramble the teams. Checking this 40 seconds later gives the player the chance to leave the server.<br>
- TB works now for all modes except SquadDeathMatch <br>
- RUSH: Teams will not be scrambled anymore if the defenders win. (Ticketdifference > 1000)<br>
- NEW: Message near roundend, if the teams will be scrambled next round.<br><br>
<b>0.1.2</b><br>
- VIP-List can now be included into the whitelist.<br>
- Players are not slayed anymore at roundstart. Players get scrambled before the next round starts loading.<br>
(small side effect: The scoretable gets messed during the last 20 seconds at roundend. This has no effect on scores or anyhting else. This is way better than players getting slayed at roundstart.)<br><br>
<b>0.1.0</b><br>
- SquadBug fixed<br>
- Added support for conquestsmall1<br>
- Added a scrambler, which will scrambler the teams at a new map if nessesary.<br><br>
<b>0.0.6</b><br>
Changed the lone wolf option. Now it is lone wolfs first. If there are no lone wolf, than somebody from a squad.<br><br>
<b>0.0.5</b><br>
Added possibility to move lone wolfs only.<br><br>
<b>0.0.4</b><br>
Added more possibilities to edit the ingame messages.<br><br>
<b>0.0.3</b><br>
Added 'Stop balancing, when tickets till end'<br>
Important bug fix, where the jointimes could be deleted at roundstart.<br><br>
<b>0.0.2</b><br>
Added Teamdeathmatch<br><br>
<b>0.0.1</b><br>
First Release!
        
If you have any Idears for the autobalancer contact me on the Procon - Forums. I'll try to implement every good idea.
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
            this.boolRunOnList = false;
            this.boolLevelLoaded = true;
            this.boolnoplayer = false;
            this.boolscrambleNow = false;
            this.boolscrambleActive = false;
            this.intTicketsdif = -1;
            this.boolTeamsScrambled  = false;
            this.boolticketdif = false;
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
            this.RegisterEvents(this.GetType().Name,    "OnReservedSlotsList", 
                                                        "OnListPlayers", 
                                                        "OnPlayerSquadChange", 
                                                        "OnPlayerMovedByAdmin", 
                                                        "OnRoundOverPlayers", 
                                                        "OnRoundOverTeamScores", 
                                                        "OnPlayerSpawned", 
                                                        "OnLevelLoaded", 
                                                        "OnResponseError", 
                                                        "OnLogin", 
                                                        "OnServerInfo", 
                                                        "OnPlayerTeamChange", 
                                                        "OnPlayerLeft", 
                                                        "OnRoundOver", 
                                                        //"BalancingTimer",
                                                        "OnPlayerKilled",
                                                        "OnRestartLevel",
                                                        "OnRunNextLevel");
        }

        public void OnPluginEnable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bTrueBalancer ^2Enabled! " + GetPluginVersion());
            this.boolfirstwarningWL = false;
            this.m_isPluginEnabled = true; 
            this.RegisterAllCommands();
            //this.boolOnLogin = true;
            this.boolLevelStart = false;
            this.boolLevelLoaded = true;
            this.boolFirstOP = false;
            this.boolgametype = false; 
            this.DTLevelStart = new DateTime();
            this.boolnoplayer = false;
            this.intScrambledPlayers = 0;
            this.boolscrambleNow = false;
            this.boolscrambleActive = false;
            this.intTicketsdif = -1;
            this.boolTeamsScrambled = false;
            this.boolticketdif = false;
            this.DTScramblestarted = new DateTime();
            this.boolRunOnList = false;
            
        }

        public void OnPluginDisable() {
            this.UnregisterAllCommands();

            this.boolfirstwarningWL = false;
            this.m_isPluginEnabled = false; 
            this.boolneedbalance = false; 

            if (this.boolscrambleActive){
                this.DebugInfoSkill("^8^bScrambler active at Disabled. STOP! Teams partly scrambled!");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }
            this.dicPlayerCache.Clear(); 
            this.dicPlayerScore.Clear();
            this.dicSquadScore.Clear();
            this.dicSquadList.Clear();
            this.OnCommandMove.Clear();
            this.OnCommandMoveDone.Clear();
            this.BalancedPlayers.Clear();
            
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bTrueBalancer ^1Disabled =(");
        }

        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("0. Battlelog Stats|Maximum number of players to fetch at each interval", this.intMaxPlayersToFetch.GetType(), this.intMaxPlayersToFetch));
            lstReturn.Add(new CPluginVariable("0. Battlelog Stats|Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));

            lstReturn.Add(new CPluginVariable("0. Manual Commands via PRoCon|PRoCon - Scramble Teams on Roundend?", this.ynbScrambleRoundViaPRoCon.GetType(), this.ynbScrambleRoundViaPRoCon));
            if (this.ynbScrambleRoundViaPRoCon == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("0. Manual Commands via PRoCon|PRoCon - Scramble Teams on Roundend? Are you sure?", this.ynbScrambleRoundViaPRoConConf.GetType(), this.ynbScrambleRoundViaPRoConConf));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|How many warnings?", this.intWarnings.GetType(), this.intWarnings));
            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Time between Warnings in sec", this.intInterval.GetType(), this.intInterval));
            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show ingame warnings?", this.ynbShowWarnings.GetType(), this.ynbShowWarnings));
            if (this.ynbShowWarnings == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Warning Message", this.strWarning.GetType(), this.strWarning));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show ingame balancing message?", this.ynbShowBallancing.GetType(), this.ynbShowBallancing));
            if (this.ynbShowBallancing == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Balancing Message", this.strLastWarning.GetType(), this.strLastWarning));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show private message to moved player?", this.ynbShowPlayermessage.GetType(), this.ynbShowPlayermessage));
            if (this.ynbShowPlayermessage == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Message for moved Player", this.strBeenMoved.GetType(), this.strBeenMoved));
            }
            
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Player Threshold", this.intTreshRUSH.GetType(), this.intTreshRUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Allowing Player Difference below Threshold", this.intAllowDif1RUSH.GetType(), this.intAllowDif1RUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Allowing Player Difference equal to/above Threshold", this.intAllowDif2RUSH.GetType(), this.intAllowDif2RUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Stop balancing, when tickets till end", this.intminScoreRUSH.GetType(), this.intminScoreRUSH));

            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Player Threshold", this.intTreshCONQUEST.GetType(), this.intTreshCONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Allowing Player Difference below Threshold", this.intAllowDif1CONQUEST.GetType(), this.intAllowDif1CONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Allowing Player Difference equal to/above Threshold", this.intAllowDif2CONQUEST.GetType(), this.intAllowDif2CONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Stop balancing, when tickets till end", this.intminScoreCONQUEST.GetType(), this.intminScoreCONQUEST));

            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Player Threshold", this.intTreshTDM.GetType(), this.intTreshTDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Allowing Player Difference below Threshold", this.intAllowDif1TDM.GetType(), this.intAllowDif1TDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2TDM.GetType(), this.intAllowDif2TDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Stop balancing, when tickets till end", this.intminScoreTDM.GetType(), this.intminScoreTDM));

            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Player Threshold", this.intTreshGM.GetType(), this.intTreshGM));
            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Allowing Player Difference below Threshold", this.intAllowDif1GM.GetType(), this.intAllowDif1GM));
            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2GM.GetType(), this.intAllowDif2GM));

            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Player Threshold", this.intTreshDF.GetType(), this.intTreshDF));
            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Allowing Player Difference below Threshold", this.intAllowDif1DF.GetType(), this.intAllowDif1DF));
            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DF.GetType(), this.intAllowDif2DF));

            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Player Threshold", this.intTreshOB.GetType(), this.intTreshOB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Allowing Player Difference below Threshold", this.intAllowDif1OB.GetType(), this.intAllowDif1OB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Allowing Player Difference equal to/above Threshold", this.intAllowDif2OB.GetType(), this.intAllowDif2OB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Stop balancing, when tickets till end", this.intminScoreOB.GetType(), this.intminScoreOB));

            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Player Threshold", this.intTreshDOM.GetType(), this.intTreshDOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Allowing Player Difference below Threshold", this.intAllowDif1DOM.GetType(), this.intAllowDif1DOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DOM.GetType(), this.intAllowDif2DOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Stop balancing, when tickets till end", this.intminScoreDOM.GetType(), this.intminScoreDOM));

            lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|RUSH-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillRUSH));
            
            if (this.ynbenableSkillRUSH == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                /*
                
                if(this.ynbScrambleEveryRound == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scramble on every new map no matter what score(Rush and Conquest)", typeof(enumBoolYesNo), this.ynbScrambleMap));
                }
                if(this.ynbScrambleMap == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Check balance on every new Round (else on new Map only) (Conquest)", typeof(enumBoolYesNo), this.ynbScrambleEveryRound));
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scramble if won with over x Tickets (Coquest)", this.intwonTickets.GetType(), this.intwonTickets));
                }
                lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scrambling Message at roundend (Coquest)", this.strScrambleMessage.GetType(), this.strScrambleMessage));
                */
            }

            lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillCONQUEST));
            if (this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapCONQUEST));
                }
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundCONQUEST));
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsCONQUEST.GetType(), this.intwonTicketsCONQUEST));
                }
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scrambling Message at roundend", this.strScrambleMessageCONQUEST.GetType(), this.strScrambleMessageCONQUEST));
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }
                                 
            lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillTDM));
            if (this.ynbenableSkillTDM == enumBoolYesNo.Yes)
            {
                if(this.ynbScrambleEveryRoundTDM == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapTDM));
                }
                if(this.ynbScrambleMapTDM == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundTDM));
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsTDM.GetType(), this.intwonTicketsTDM));    
                }
                //lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Show Scramble message at roundend when x Tickets are reached", this.intwonTicketsTDM.GetType(), this.intshowTicketsTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scrambling Message at roundend", this.strScrambleMessageTDM.GetType(), this.strScrambleMessageTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
                
            }

            lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|GM/CTF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillGM));

            if (this.ynbenableSkillGM == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
            }

            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));

            //if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes || this.ynbEnableScrambleNow == enumBoolYesNo.Yes)
            //{
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            //    if (this.strScrambleMode == "Keep squads with two or more clanmates")
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
            //    }
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
            //    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));

            //    }
            //}

            lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            if (this.ynbenableSkillOB == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundOB == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapOB));
                }
                if (this.ynbScrambleMapOB == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundOB));
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsOB.GetType(), this.intwonTicketsOB));
                }
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scrambling Message at roundend", this.strScrambleMessageOB.GetType(), this.strScrambleMessageOB));
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }

            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillOB));

            //if (this.ynbenableSkillOB == enumBoolYesNo.Yes)
            //{
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            //    if (this.strScrambleMode == "Keep squads with two or more clanmates")
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
            //    }
            //}

            lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|DF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillDF));

            if (this.ynbenableSkillDF == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));
                lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
            }

            lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillDOM));
            if (this.ynbenableSkillDOM == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundDOM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapDOM));
                }
                if (this.ynbScrambleMapDOM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundDOM));
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsDOM.GetType(), this.intwonTicketsDOM));
                }
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDOM));
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scrambling Message at roundend", this.strScrambleMessageDOM.GetType(), this.strScrambleMessageDOM));
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }

            lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));

            if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes || this.ynbEnableScrambleNow == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
                if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));

                }
            }

            lstReturn.Add(new CPluginVariable("3. Balancing Guard|Enable Balancing Guard?", this.ynbBalancingGuard.GetType(), this.ynbBalancingGuard));
            if (this.ynbBalancingGuard == enumBoolYesNo.Yes){
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Rush - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|Rush - Start sorting at Difference of", this.dblValueDiffRUSH.GetType(), this.dblValueDiffRUSH));
                
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Conquest - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|Conquest - Start sorting at Difference of", this.dblValueDiffCONQUEST.GetType(), this.dblValueDiffCONQUEST));
                
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|TDM - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|TDM - Start sorting at Difference of", this.dblValueDiffTDM.GetType(), this.dblValueDiffTDM));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|GM/CTF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|GM - Start sorting at Difference of", this.dblValueDiffGM.GetType(), this.dblValueDiffGM));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Obliteration - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|DF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)", this.intScoreWTS.GetType(), this.intScoreWTS));
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Enable Shame Message?", this.ynbShameMessage.GetType(), this.ynbShameMessage));
                if (this.ynbShameMessage == enumBoolYesNo.Yes){
                    lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Shame Message", this.strShameMessage.GetType(), this.strShameMessage));
                }
            }

            lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Enable Whitelist", typeof(enumBoolYesNo), this.ynbWhitelist));
            if (this.ynbWhitelist == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Whitelist ClanTags", this.strAClantagWhitelist.GetType(), this.strAClantagWhitelist));
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Whitelist Names", this.strAWhitelist.GetType(), this.strAWhitelist));
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Include VIP/Reserved Slots List into the whitelist?", typeof(enumBoolYesNo), this.ynbincludeVIPlist));
            }

            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for normal balancing", this.ynbDebugMode.GetType(), this.ynbDebugMode));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for skill balancing", this.ynbDebugModeSkill.GetType(), this.ynbDebugModeSkill));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for balancing guard", this.ynbDebugModeGuard.GetType(), this.ynbDebugModeGuard));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Show Balancing Moves (PlayerNumber Skill and Guard)", this.showMoves.GetType(), this.showMoves));

            lstReturn.Add(new CPluginVariable("6. Automatic Update Check settings|Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            lstReturn.Add(new CPluginVariable("7. Testing settings|Enable Virtual Mode?", this.ynbVirtualMode.GetType(), this.ynbVirtualMode));
            
            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Maximum number of players to fetch at each interval", this.intMaxPlayersToFetch.GetType(), this.intMaxPlayersToFetch));
            lstReturn.Add(new CPluginVariable("Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));

            lstReturn.Add(new CPluginVariable("PRoCon - Scramble Teams on Roundend?", this.ynbScrambleRoundViaPRoCon.GetType(), this.ynbScrambleRoundViaPRoCon));
            lstReturn.Add(new CPluginVariable("PRoCon - Scramble Teams on Roundend? Are you sure?", this.ynbScrambleRoundViaPRoConConf.GetType(), this.ynbScrambleRoundViaPRoConConf));
            

            lstReturn.Add(new CPluginVariable("How many warnings?", this.intWarnings.GetType(), this.intWarnings));
            lstReturn.Add(new CPluginVariable("Time between Warnings in sec", this.intInterval.GetType(), this.intInterval));

            lstReturn.Add(new CPluginVariable("RUSH-Player Threshold", this.intTreshRUSH.GetType(), this.intTreshRUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Allowing Player Difference below Threshold", this.intAllowDif1RUSH.GetType(), this.intAllowDif1RUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Allowing Player Difference equal to/above Threshold", this.intAllowDif2RUSH.GetType(), this.intAllowDif2RUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Stop balancing, when tickets till end", this.intminScoreRUSH.GetType(), this.intminScoreRUSH));

            lstReturn.Add(new CPluginVariable("CQ-Player Threshold", this.intTreshCONQUEST.GetType(), this.intTreshCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Allowing Player Difference below Threshold", this.intAllowDif1CONQUEST.GetType(), this.intAllowDif1CONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Allowing Player Difference equal to/above Threshold", this.intAllowDif2CONQUEST.GetType(), this.intAllowDif2CONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Stop balancing, when tickets till end", this.intminScoreCONQUEST.GetType(), this.intminScoreCONQUEST));

            lstReturn.Add(new CPluginVariable("TDM-Player Threshold", this.intTreshTDM.GetType(), this.intTreshTDM));
            lstReturn.Add(new CPluginVariable("TDM-Allowing Player Difference below Threshold", this.intAllowDif1TDM.GetType(), this.intAllowDif1TDM));
            lstReturn.Add(new CPluginVariable("TDM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2TDM.GetType(), this.intAllowDif2TDM));
            lstReturn.Add(new CPluginVariable("TDM-Stop balancing, when tickets till end", this.intminScoreTDM.GetType(), this.intminScoreTDM));

            lstReturn.Add(new CPluginVariable("GM/CTF-Player Threshold", this.intTreshGM.GetType(), this.intTreshGM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Allowing Player Difference below Threshold", this.intAllowDif1GM.GetType(), this.intAllowDif1GM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2GM.GetType(), this.intAllowDif2GM));

            lstReturn.Add(new CPluginVariable("DF-Player Threshold", this.intTreshDF.GetType(), this.intTreshDF));
            lstReturn.Add(new CPluginVariable("DF-Allowing Player Difference below Threshold", this.intAllowDif1DF.GetType(), this.intAllowDif1DF));
            lstReturn.Add(new CPluginVariable("DF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DF.GetType(), this.intAllowDif2DF));

            lstReturn.Add(new CPluginVariable("OB-Player Threshold", this.intTreshOB.GetType(), this.intTreshOB));
            lstReturn.Add(new CPluginVariable("OB-Allowing Player Difference below Threshold", this.intAllowDif1OB.GetType(), this.intAllowDif1OB));
            lstReturn.Add(new CPluginVariable("OB-Allowing Player Difference equal to/above Threshold", this.intAllowDif2OB.GetType(), this.intAllowDif2OB));
            lstReturn.Add(new CPluginVariable("OB-Stop balancing, when tickets till end", this.intminScoreOB.GetType(), this.intminScoreOB));

            lstReturn.Add(new CPluginVariable("DOM-Player Threshold", this.intTreshDOM.GetType(), this.intTreshDOM));
            lstReturn.Add(new CPluginVariable("DOM-Allowing Player Difference below Threshold", this.intAllowDif1DOM.GetType(), this.intAllowDif1DOM));
            lstReturn.Add(new CPluginVariable("DOM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DOM.GetType(), this.intAllowDif2DOM));
            lstReturn.Add(new CPluginVariable("DOM-Stop balancing, when tickets till end", this.intminScoreDOM.GetType(), this.intminScoreDOM));

            lstReturn.Add(new CPluginVariable("Show ingame warnings?", this.ynbShowWarnings.GetType(), this.ynbShowWarnings));
            lstReturn.Add(new CPluginVariable("Warning Message", this.strWarning.GetType(), this.strWarning));
            lstReturn.Add(new CPluginVariable("Show ingame balancing message?", this.ynbShowBallancing.GetType(), this.ynbShowBallancing));
            lstReturn.Add(new CPluginVariable("Balancing Message", this.strLastWarning.GetType(), this.strLastWarning));
            lstReturn.Add(new CPluginVariable("Show private message to moved player?", this.ynbShowPlayermessage.GetType(), this.ynbShowPlayermessage));
            lstReturn.Add(new CPluginVariable("Message for moved Player", this.strBeenMoved.GetType(), this.strBeenMoved));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for normal balancing", this.ynbDebugMode.GetType(), this.ynbDebugMode));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for skill balancing", this.ynbDebugModeSkill.GetType(), this.ynbDebugModeSkill));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for balancing guard", this.ynbDebugModeGuard.GetType(), this.ynbDebugModeGuard));
            lstReturn.Add(new CPluginVariable("Enable Whitelist", typeof(enumBoolYesNo), this.ynbWhitelist));
            lstReturn.Add(new CPluginVariable("Whitelist ClanTags", this.strAClantagWhitelist.GetType(), this.strAClantagWhitelist));
            lstReturn.Add(new CPluginVariable("Whitelist Names", this.strAWhitelist.GetType(), this.strAWhitelist));
            lstReturn.Add(new CPluginVariable("Include VIP/Reserved Slots List into the whitelist?",typeof(enumBoolYesNo), this.ynbincludeVIPlist));
            
            lstReturn.Add(new CPluginVariable("Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            lstReturn.Add(new CPluginVariable("Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));
            
            lstReturn.Add(new CPluginVariable("Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
            lstReturn.Add(new CPluginVariable("Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
            lstReturn.Add(new CPluginVariable("Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
            lstReturn.Add(new CPluginVariable("Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
            lstReturn.Add(new CPluginVariable("Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));
            
            lstReturn.Add(new CPluginVariable("Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            lstReturn.Add(new CPluginVariable("What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            lstReturn.Add(new CPluginVariable("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));


            lstReturn.Add(new CPluginVariable("RUSH-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillRUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));

            lstReturn.Add(new CPluginVariable("GM/CTF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillGM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));

            lstReturn.Add(new CPluginVariable("DF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillDF));
            lstReturn.Add(new CPluginVariable("DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));
            
            lstReturn.Add(new CPluginVariable("CQ-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsCONQUEST.GetType(), this.intwonTicketsCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scrambling Message at roundend", this.strScrambleMessageCONQUEST.GetType(), this.strScrambleMessageCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));

            //lstReturn.Add(new CPluginVariable("OB-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            //lstReturn.Add(new CPluginVariable("OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

            lstReturn.Add(new CPluginVariable("OB-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            lstReturn.Add(new CPluginVariable("OB-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapOB));
            lstReturn.Add(new CPluginVariable("OB-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundOB));
            lstReturn.Add(new CPluginVariable("OB-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsOB.GetType(), this.intwonTicketsOB));

            lstReturn.Add(new CPluginVariable("OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
            lstReturn.Add(new CPluginVariable("What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            lstReturn.Add(new CPluginVariable("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));

            lstReturn.Add(new CPluginVariable("OB-Scrambling Message at roundend", this.strScrambleMessageOB.GetType(), this.strScrambleMessageOB));
            lstReturn.Add(new CPluginVariable("Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));            

            lstReturn.Add(new CPluginVariable("TDM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapTDM));
            lstReturn.Add(new CPluginVariable("TDM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsTDM.GetType(), this.intwonTicketsTDM));
            lstReturn.Add(new CPluginVariable("TDM-Show Scramble message at roundend when x Tickets are reached", this.intwonTicketsTDM.GetType(), this.intshowTicketsTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scrambling Message at roundend", this.strScrambleMessageTDM.GetType(), this.strScrambleMessageTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));

            lstReturn.Add(new CPluginVariable("DOM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapDOM));
            lstReturn.Add(new CPluginVariable("DOM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsDOM.GetType(), this.intwonTicketsDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scrambling Message at roundend", this.strScrambleMessageDOM.GetType(), this.strScrambleMessageDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDOM));

            lstReturn.Add(new CPluginVariable("Enable Balancing Guard?", this.ynbBalancingGuard.GetType(), this.ynbBalancingGuard));
            lstReturn.Add(new CPluginVariable("Rush - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
            lstReturn.Add(new CPluginVariable("Conquest - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
            lstReturn.Add(new CPluginVariable("TDM - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
            lstReturn.Add(new CPluginVariable("GM/CTF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));

            lstReturn.Add(new CPluginVariable("DF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));

            lstReturn.Add(new CPluginVariable("Obliteration - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

            lstReturn.Add(new CPluginVariable("Rush - Start sorting at Difference of", this.dblValueDiffRUSH.GetType(), this.dblValueDiffRUSH));
            lstReturn.Add(new CPluginVariable("Conquest - Start sorting at Difference of", this.dblValueDiffCONQUEST.GetType(), this.dblValueDiffCONQUEST));
            lstReturn.Add(new CPluginVariable("TDM - Start sorting at Difference of", this.dblValueDiffTDM.GetType(), this.dblValueDiffTDM));
            lstReturn.Add(new CPluginVariable("GM/CTF - Start sorting at Difference of", this.dblValueDiffGM.GetType(), this.dblValueDiffGM));
            
            lstReturn.Add(new CPluginVariable("CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)", this.intScoreWTS.GetType(), this.intScoreWTS));
            lstReturn.Add(new CPluginVariable("CQ/TDM - Enable Shame Message?", this.ynbShameMessage.GetType(), this.ynbShameMessage));
            lstReturn.Add(new CPluginVariable("CQ/TDM - Shame Message", this.strShameMessage.GetType(), this.strShameMessage));
            
            //lstReturn.Add(new CPluginVariable("Scramble if won with over x Tickets (Coquest)", this.intwonTickets.GetType(), this.intwonTickets));
            //lstReturn.Add(new CPluginVariable("Scrambling Message at roundend (Coquest)", this.strScrambleMessage.GetType(), this.strScrambleMessage));
            //lstReturn.Add(new CPluginVariable("Scramble on every new map no matter what score(Rush and Conquest)", typeof(enumBoolYesNo), this.ynbScrambleMap));
            //lstReturn.Add(new CPluginVariable("Check balance on every new Round (else on new Map only) (Conquest)", typeof(enumBoolYesNo), this.ynbScrambleEveryRound));

            lstReturn.Add(new CPluginVariable("Show Balancing Moves (PlayerNumber Skill and Guard)", this.showMoves.GetType(), this.showMoves));

            lstReturn.Add(new CPluginVariable("Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            lstReturn.Add(new CPluginVariable("Enable Virtual Mode?", this.ynbVirtualMode.GetType(), this.ynbVirtualMode));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse

        public void SetPluginVariable(string strVariable, string strValue) {
            if (strVariable.CompareTo("Servertype") == 0)
            {
                this.Servertype = strValue;
            }

            if (strVariable == "Maximum number of players to fetch at each interval")
            {
                int numPlayers = 1;
                Int32.TryParse(strValue, out numPlayers);
                if (numPlayers < 0 || numPlayers > 3)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^8^bMaximum number of players to fetch must be between 0 and 3, inclusive!");
                    numPlayers = 1;
                } 
                else if (numPlayers == 0)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^1^bMaximum number of players to fetch set to 0, stats fetching is disabled!");
                }
                else if (numPlayers != this.intMaxPlayersToFetch)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^0^bMaximum number of players to fetch set to " + numPlayers);
                }
                this.intMaxPlayersToFetch = numPlayers;
            }
            
            if (strVariable.CompareTo("PRoCon - Scramble Teams on Roundend?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleRoundViaPRoCon = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("PRoCon - Scramble Teams on Roundend? Are you sure?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleRoundViaPRoConConf = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.ynbScrambleRoundViaPRoConConf == enumBoolYesNo.Yes)
                {
                    OnScrambleViaPRoCon();
                }
            }
            else if (strVariable.CompareTo("How many warnings?") == 0 && Int32.TryParse(strValue, out  this.intWarnings) == true)
            {
                if (this.intWarnings > 0)
                {
                    this.intWarnings = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intWarnings = 1;
                }
            }
            else if (strVariable.CompareTo("Enable Debug Mode for balancing guard") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugModeGuard = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Balancing Guard?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbBalancingGuard = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ/TDM - Enable Shame Message?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShameMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ/TDM - Shame Message") == 0)
            {
                this.strShameMessage = strValue;
            }
            else if (strVariable.CompareTo("CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)") == 0 && Int32.TryParse(strValue, out  this.intScoreWTS) == true)
            {
                if (this.intScoreWTS > 0)
                {
                    this.intScoreWTS = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intScoreWTS = 1;
                }
            }
            else if (strVariable.CompareTo("Rush - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out  this.dblValueDiffRUSH) == true)
            {
                if (this.dblValueDiffRUSH >= 0.01)
                {
                    this.dblValueDiffRUSH = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffRUSH = 0.01;
                }
            }
            else if (strVariable.CompareTo("GM/CTF - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out  this.dblValueDiffGM) == true)
            {
                if (this.dblValueDiffGM >= 0.01)
                {
                    this.dblValueDiffGM = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffGM = 0.01;
                }
            }
            else if (strVariable.CompareTo("Conquest - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out  this.dblValueDiffCONQUEST) == true)
            {
                if (this.dblValueDiffCONQUEST >= 0.01)
                {
                    this.dblValueDiffCONQUEST = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffCONQUEST = 0.01;
                }
            }
            else if (strVariable.CompareTo("TDM - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out  this.dblValueDiffTDM) == true)
            {
                if (this.dblValueDiffTDM >= 0.01)
                {
                    this.dblValueDiffTDM = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffTDM = 0.01;
                }
            }
            else if (strVariable.CompareTo("Rush - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByRUSH = strValue;
            }
            else if (strVariable.CompareTo("GM/CTF - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByGM = strValue;
            }
            else if (strVariable.CompareTo("DF - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByDF = strValue;
            }
            else if (strVariable.CompareTo("Conquest - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByCONQUEST = strValue;
            }
            else if (strVariable.CompareTo("TDM - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByTDM = strValue;
            }
            else if (strVariable.CompareTo("Obliteration - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByOB = strValue;
            }
            else if (strVariable.CompareTo("Time between Warnings in sec") == 0 && Int32.TryParse(strValue, out  this.intInterval) == true)
            {
                if (this.intInterval > 0)
                {
                    this.intInterval = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intInterval = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshRUSH) == true)
            {
                if (this.intTreshRUSH > 0)
                {
                    this.intTreshRUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshRUSH = 0;
                }
            }
            else if (strVariable.CompareTo("RUSH-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1RUSH) == true)
            {
                if (this.intAllowDif1RUSH > 0)
                {
                    this.intAllowDif1RUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1RUSH = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2RUSH) == true)
            {
                if (this.intAllowDif2RUSH > 0)
                {
                    this.intAllowDif2RUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2RUSH = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out  this.intminScoreRUSH) == true)
            {
                    this.intminScoreRUSH = Convert.ToInt32(strValue);

            }


            else if (strVariable.CompareTo("GM/CTF-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshGM) == true)
            {
                if (this.intTreshGM > 0)
                {
                    this.intTreshGM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshGM = 0;
                }
            }
            else if (strVariable.CompareTo("GM/CTF-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1GM) == true)
            {
                if (this.intAllowDif1GM > 0)
                {
                    this.intAllowDif1GM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1GM = 1;
                }
            }
            else if (strVariable.CompareTo("GM/CTF-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2GM) == true)
            {
                if (this.intAllowDif2GM > 0)
                {
                    this.intAllowDif2GM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2GM = 1;
                }
            }

            else if (strVariable.CompareTo("DF-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshDF) == true)
            {
                if (this.intTreshDF > 0)
                {
                    this.intTreshDF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshDF = 0;
                }
            }
            else if (strVariable.CompareTo("DF-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1DF) == true)
            {
                if (this.intAllowDif1DF > 0)
                {
                    this.intAllowDif1DF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1DF = 1;
                }
            }
            else if (strVariable.CompareTo("DF-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2DF) == true)
            {
                if (this.intAllowDif2DF > 0)
                {
                    this.intAllowDif2DF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2DF = 1;
                }
            }

            else if (strVariable.CompareTo("CQ-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshCONQUEST) == true)
            {
                if (this.intTreshCONQUEST > 0)
                {
                    this.intTreshCONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshCONQUEST = 0;
                }
            }
            else if (strVariable.CompareTo("CQ-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1CONQUEST) == true)
            {
                if (this.intAllowDif1CONQUEST > 0)
                {
                    this.intAllowDif1CONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1CONQUEST = 1;
                }
            }
             else if (strVariable.CompareTo("CQ-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2CONQUEST) == true)
            {
                if (this.intAllowDif2CONQUEST > 0)
                {
                    this.intAllowDif2CONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2CONQUEST = 1;
                }
            }
             else if (strVariable.CompareTo("CQ-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out  this.intminScoreCONQUEST) == true)
            {
                    this.intminScoreCONQUEST = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("DOM-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshDOM) == true)
            {
                if (this.intTreshDOM > 0)
                {
                    this.intTreshDOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshDOM = 0;
                }
            }
            else if (strVariable.CompareTo("DOM-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1DOM) == true)
            {
                if (this.intAllowDif1DOM > 0)
                {
                    this.intAllowDif1DOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1DOM = 1;
                }
            }
            else if (strVariable.CompareTo("DOM-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2DOM) == true)
            {
                if (this.intAllowDif2DOM > 0)
                {
                    this.intAllowDif2DOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2DOM = 1;
                }
            }
            else if (strVariable.CompareTo("DOM-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out  this.intminScoreDOM) == true)
            {
                this.intminScoreDOM = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("TDM-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshTDM) == true)
            {
                if (this.intTreshTDM> 0)
                {
                    this.intTreshTDM= Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshTDM= 0;
                }
            }
            else if (strVariable.CompareTo("TDM-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1TDM) == true)
            {
                if (this.intAllowDif1TDM> 0)
                {
                    this.intAllowDif1TDM= Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1TDM= 1;
                }
            }
             else if (strVariable.CompareTo("TDM-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2TDM) == true)
            {
                if (this.intAllowDif2TDM> 0)
                {
                    this.intAllowDif2TDM= Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2TDM= 1;
                }
            }
             else if (strVariable.CompareTo("TDM-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out  this.intminScoreTDM) == true)
            {
                    this.intminScoreTDM = Convert.ToInt32(strValue);

            }
            
            else if (strVariable.CompareTo("OB-Player Threshold") == 0 && Int32.TryParse(strValue, out  this.intTreshOB) == true)
            {
                if (this.intTreshOB > 0)
                {
                    this.intTreshOB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshOB = 0;
                }
            }
            else if (strVariable.CompareTo("OB-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif1OB) == true)
            {
                if (this.intAllowDif1OB > 0)
                {
                    this.intAllowDif1OB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1OB = 1;
                }
            }
            else if (strVariable.CompareTo("OB-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out  this.intAllowDif2OB) == true)
            {
                if (this.intAllowDif2OB > 0)
                {
                    this.intAllowDif2OB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2OB = 1;
                }
            }
            else if (strVariable.CompareTo("OB-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out  this.intminScoreOB) == true)
            {
                this.intminScoreOB = Convert.ToInt32(strValue);

            }
                            
            else if (strVariable.CompareTo("Show ingame warnings?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowWarnings = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Warning Message") == 0)
            {
                this.strWarning = strValue;
            }
            else if (strVariable.CompareTo("Show ingame balancing message?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowBallancing = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Balancing Message") == 0)
            {
                this.strLastWarning = strValue;
            }
            else if (strVariable.CompareTo("Show private message to moved player?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowPlayermessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Message for moved Player") == 0)
            {
                this.strBeenMoved = strValue;
            }
            
            
            else if (strVariable.CompareTo("Enable Command: !scrambleround") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbEnableScrambleRound = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Command: !scramblenow") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbEnableScrambleNow = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show Scrambling messages to the server?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell messages to the server?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbYellScrambleManuall = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Message when admin requests a scramble at roundend") == 0)
            {
                this.strScrambleRoundMsg = strValue;
            }
            else if (strVariable.CompareTo("Message when admin requests a scramble now") == 0)
            {
                this.strScrambleNowMsg = strValue;
            }
            else if (strVariable.CompareTo("Message when scrambling is done") == 0)
            {
                this.strScrambleDoneMsg = strValue;
            }
            
            else if (strVariable.CompareTo("Yell scramble message at roundend?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbYellScrambleMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("What to do with squads?") == 0)
            {
                this.strScrambleMode = strValue;
            }
            else if (strVariable.CompareTo("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags") == 0)
            {
                this.strAClantagWhitelistScrambler = CPluginVariable.DecodeStringArray(strValue);
            }

            else if (strVariable.CompareTo("RUSH-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillRUSH = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("RUSH-Scramble by") == 0)
            {
                this.ScrambleByRUSH = strValue;
            }

            else if (strVariable.CompareTo("GM/CTF-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillGM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("GM/CTF-Scramble by") == 0)
            {
                this.ScrambleByGM = strValue;
            }

            else if (strVariable.CompareTo("DF-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillDF = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DF-Scramble by") == 0)
            {
                this.ScrambleByDF = strValue;
            }

            //else if (strVariable.CompareTo("OB-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            //{
            //    this.ynbenableSkillOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            //}
            //else if (strVariable.CompareTo("OB-Scramble by") == 0)
            //{
            //    this.ScrambleByOB = strValue;
            //}

            else if (strVariable.CompareTo("CQ-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("CQ-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out  this.intwonTicketsCONQUEST) == true)
            {
                if (this.intwonTicketsCONQUEST >= 0)
                {
                    this.intwonTicketsCONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsCONQUEST = 0;
                }
            }
            else if (strVariable.CompareTo("CQ-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageCONQUEST = strValue;
            }
            else if (strVariable.CompareTo("CQ-Scramble by") == 0)
            {
                this.ScrambleByCONQUEST = strValue;
            }

            else if (strVariable.CompareTo("DOM-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DOM-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DOM-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("DOM-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out  this.intwonTicketsDOM) == true)
            {
                if (this.intwonTicketsDOM >= 0)
                {
                    this.intwonTicketsDOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsDOM = 0;
                }
            }
            else if (strVariable.CompareTo("DOM-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageDOM = strValue;
            }
            else if (strVariable.CompareTo("DOM-Scramble by") == 0)
            {
                this.ScrambleByDOM = strValue;
            }

            else if (strVariable.CompareTo("OB-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("OB-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("OB-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("OB-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out  this.intwonTicketsOB) == true)
            {
                if (this.intwonTicketsOB >= 0)
                {
                    this.intwonTicketsOB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsOB = 0;
                }
            }
            else if (strVariable.CompareTo("OB-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageOB = strValue;
            }
            else if (strVariable.CompareTo("OB-Scramble by") == 0)
            {
                this.ScrambleByOB = strValue;
            }
            
            else if (strVariable.CompareTo("TDM-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("TDM-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("TDM-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }   
            if (strVariable.CompareTo("TDM-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out  this.intwonTicketsTDM) == true)
            {
                if (this.intwonTicketsTDM >= 0)
                {
                    this.intwonTicketsTDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsTDM = 0;
                }
            }
            if (strVariable.CompareTo("TDM-Show Scramble message at roundend when x Tickets are reached") == 0 && Int32.TryParse(strValue, out  this.intshowTicketsTDM) == true)
            {
                if (this.intshowTicketsTDM >= 0)
                {
                    this.intshowTicketsTDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intshowTicketsTDM = 5;
                }
            }
            else if (strVariable.CompareTo("TDM-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageTDM = strValue;
            }
            else if (strVariable.CompareTo("TDM-Scramble by") == 0)
            {
                this.ScrambleByTDM = strValue;
            }
            

            else if (strVariable.CompareTo("Enable Debug Mode for normal balancing") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Debug Mode for skill balancing") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugModeSkill = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Whitelist") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbWhitelist = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Whitelist ClanTags") == 0)
            {
                this.strAClantagWhitelist = CPluginVariable.DecodeStringArray(strValue);
            }
            else if (strVariable.CompareTo("Whitelist Names") == 0)
            {
                this.strAWhitelist = CPluginVariable.DecodeStringArray(strValue);
            }
            else if (strVariable.CompareTo("Include VIP/Reserved Slots List into the whitelist?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbincludeVIPlist = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show Balancing Moves (PlayerNumber Skill and Guard)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.showMoves = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Check for Update?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.Check4Update = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

            if (strVariable == "Enable Virtual Mode?" && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbVirtualMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                this.boolVirtual = (this.ynbVirtualMode == enumBoolYesNo.Yes);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^1^bVirtual mode is ENABLED!^n^0 No player moves or say/yells will be sent to the game server.");
                }
            }

        }

        private void UnregisterAllCommands() {
        
            List<string> emptyList = new List<string>();
            
            this.UnregisterCommand(
                new MatchCommand(
                    emptyList, 
                    "scramblenow",
                    this.Listify<MatchArgumentFormat>()
                )
            );
                                                    
            this.UnregisterCommand(
                new MatchCommand(
                    emptyList, 
                    "scrambleround",
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "tb-move",
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "tb-fmove",
                    this.Listify<MatchArgumentFormat>()
                )
            );
            
        }

        private void RegisterAllCommands(){

            if (this.m_isPluginEnabled == true)
            {
                MatchCommand confirmationCommand = new MatchCommand(this.Listify<string>("@", "!", "#"), "yes" , this.Listify<MatchArgumentFormat>());
                
                this.RegisterCommand(
                    new MatchCommand(
                        "TrueBalancer", 
                        "OnCommandScrambleNow", 
                        this.Listify<string>("@", "!", "#"), 
                        "scramblenow",
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Privileges,
                            Privileges.CanMovePlayers,
                            0,
                            confirmationCommand,
                            "You do not have enough privileges to scramble players"), 
                        "Scrambling teams now! Use with caution."
                    )
                );
                                                        
                this.RegisterCommand(
                    new MatchCommand (
                        "TrueBalancer",
                        "OnCommandScrambleRound", 
                        this.Listify<string>("@", "!", "#"), 
                        "scrambleround",
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Privileges,
                            Privileges.CanMovePlayers,
                            0,
                            confirmationCommand,
                            "You do not have enough privileges to scramble players"),
                        "Scrambling teams next round!"
                    )
                );

                this.RegisterCommand(
                        new MatchCommand(
                            "TrueBalancer",
                            "OnCommandTBMove",
                            this.Listify<string>("@", "!", "#"), 
                            "tb-move",
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    GetSoldierNames()
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Moves the player to the other team on death!"
                        )
                    );

                this.RegisterCommand(
                        new MatchCommand(
                            "TrueBalancer",
                            "OnCommandTBForceMove",
                            this.Listify<string>("@", "!", "#"),
                            "tb-fmove",
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    GetSoldierNames()
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Kills a player and moves the player to the other team."
                        )
                    );
            }
            
        }
        #endregion
                    
        #region ProconPluginInterface

        public override void OnRestartLevel()
        {
            DebugInfoSkill("^9Restart Round");
            this.intTicketcount = 123987123;
            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }
        }

        public override void OnRunNextLevel()
        {
            DebugInfoSkill("^9Next Round");
            this.intTicketcount = 123987123;
            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }
        }

        public virtual void OnReservedSlotsList(List<string> soldierNames) { 
            
            
            this.strAWhitelistComplete = this.strAWhitelist;
            List<string> listWL = new List<string>(this.strAWhitelist);
            
            if (this.ynbWhitelist == enumBoolYesNo.Yes && this.ynbincludeVIPlist == enumBoolYesNo.Yes) {
                
                //this.DebugInfo ("reservedSlots");
                foreach(string name in soldierNames){
                    listWL.Add(name);
                }
                this.strAWhitelistComplete = listWL.ToArray();
                string whitelist = "";
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache){
                    if (((IList<string>)this.strAWhitelistComplete).Contains(kvp.Key)){
                    whitelist = whitelist + kvp.Key + ", ";
                    }
                }
                
                //this.DebugInfo("Online VIPs: " + whitelist);  
            }
        }
        
        public virtual void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled) {
            this.boolscramblefailed = true;
            this.boolbalanced = true;
            // if (this.boolscrambleNow){
                // foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
                    // if (this.dicPlayerScore[kvp.Key].playerName == soldierName){
                        // this.dicPlayerScore[kvp.Key].scrambled = true;
                        // DebugInfoSkill (soldierName + " --- MOVED: team= " + destinationTeamId.ToString() + ", squad= " + destinationSquadId.ToString());
                        // break;
                    // }
                // }
                // ScrambleNow();
            // }
        }
        
        public override void OnResponseError(List<string> lstRequestWords, string strError) {

            //DebugInfoSkill ("^3W: ^9" + strError);
            if (this.boolscrambleNow && this.strErrorMsg == ""){
                this.boolscramblefailed = true;
                this.strErrorMsg = strError;
                if (strError == "SetSquadFailed"){
                    DebugInfoSkill ("^b^8ScrambleNow: SetSquadFailed");             
                }
            }
        }
            
        public virtual void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal){
            
            
            this.DTLevelLoaded= DateTime.Now;
            this.DebugInfo("^9New Level loaded");
            this.EndRoundSeconds = DateTime.Now - this.EndRoundTime;
            this.DebugInfoSkill("^9New Level loaded, after " + this.EndRoundSeconds.TotalSeconds.ToString("F2") + " seconds loading screen.");
            this.intTicketcount = 123987123;
            //this.boolRoundOver = false;
            this.strcurrentGametype = Gamemode;
            this.DebugInfo("^9Gamemode: " + strcurrentGametype);
            this.boolLevelLoaded = true;
            this.intCurrentRoundCount = roundsTotal - roundsPlayed; 
            this.boolticketdif = false;
            this.boolbalanced = true;
            this.boolscramblebyadminroundend = false;
            this.OnCommandMove.Clear();
            this.OnCommandMoveDone.Clear();
            this.BalancedPlayers.Clear();
            
            if (this.boolscrambleActive){
                this.DebugInfoSkill("^8^bScrambler active at Maploaded. STOP! Teams partly scrambled!");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }
        }
        
        public override void OnRoundOver(int iWinningTeamID) {

        }
        
        public virtual void OnRoundOverTeamScores(List<TeamScore> teamScores) { 
            DebugInfoSkill ("^9^bONROUNDOVERTEAMSCORES");

            this.EndRoundSeconds = new TimeSpan(0);
            this.EndRoundTime = DateTime.Now.AddSeconds(60);
            this.showfirstmove = false;
            
            int TeamAScore = 0;
            int TeamBScore = 0;
            
            this.intTicketsdif = 0;
            
            if (teamScores.Count == 1){
                TeamAScore = teamScores[0].Score;
                TeamBScore = 0;
            } else if (teamScores.Count == 2) {
                TeamAScore = teamScores[0].Score;
                TeamBScore = teamScores[1].Score;
            } else {
                DebugInfoSkill("^8^bGamemode not Supported! Not scrambling.");
            }
            
            if (TeamAScore > TeamBScore) this.intTicketsdif = TeamAScore - TeamBScore;
            else if (TeamBScore > TeamAScore) this.intTicketsdif = TeamBScore - TeamAScore;
            else this.intTicketsdif = 0;
            
            /*
            if(this.ynbScrambleMap == enumBoolYesNo.Yes){
                this.boolticketdif = true;
                DebugInfoSkill("Scrambling on every new Map.");
            } */

            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false; 
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();
            
            /*
            this.skillA = 0;
            this.skillB = 0;
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerCache){
                if (dicPlayerCache[kvp.Key].teamID == 1){
                    skillA = skillA + dicPlayerCache[kvp.Key].spm;
                }else if (dicPlayerCache[kvp.Key].teamID == 2){
                    skillB = skillB + dicPlayerCache[kvp.Key].spm;
                }
            }
            
            this.skillA = Math.Round(this.skillA/this.TeamA, 2);
            this.skillB = Math.Round(this.skillB/this.TeamB, 2);
            
            string strSKILL = "TEAM1: " + skillA.ToString() + " --- " + "TEAM2: " + skillB.ToString();
            
            DebugInfoSkill("Ticketdifference at roundend: " + this.intTicketsdif.ToString());
            this.DebugInfo("Round Over, Roundnumber: " + this.intCurrentRoundCount.ToString());  
            DebugInfoSkill("SKILL: " + strSKILL);
            
            */
            
            this.DebugInfo("^9Round Over, Roundnumber: ^b" + this.intCurrentRoundCount.ToString()); 
            this.DebugInfoSkill("^bTicketdifference at roundend: ^3" + this.intTicketsdif.ToString());
            this.DebugInfoSkill("^9Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
            this.DebugInfoSkill("^9Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));

            
            
            if(this.boolscramblebyadminroundend && this.strcurrentGametype != "SquadDeathMatch0"){
                DebugInfoSkill("^9" + this.strcurrentGametype + ": ^b^2Scrambling on roundend requested by an admin!");
                this.boolscramblebyadminroundend = false;
                this.boolfirstscrambler = true;
                this.boolscramblefailed = false;
                this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
            }
            else if ((this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByCONQUEST);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsCONQUEST * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByCONQUEST);
                    if (this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Domination0") && this.ynbenableSkillDOM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapDOM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByDOM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsDOM * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByDOM);
                    if (this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9OB - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapOB == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByOB);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsOB * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByOB);
                    if (this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if ((this.strcurrentGametype.Contains("Rush")) && this.ynbenableSkillRUSH == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("R - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1){
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByRUSH);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                } else {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if ((this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag")) && this.ynbenableSkillGM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("GM/CTF - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByGM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Elimination") && this.ynbenableSkillDF == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("DF - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByDF);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            //else if (this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes)
            //{
            //    DebugInfoSkill("OB - Gamemode: ^b" + this.strcurrentGametype);
            //    if (this.intCurrentRoundCount == 1)
            //    {
            //        DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByOB);
            //        //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
            //        this.boolfirstscrambler = true;
            //        this.boolscramblefailed = false;
            //        this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
            //        // StartScrambler();
            //    }
            //    else
            //    {
            //        DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
            //    }
            //}
            else if (this.strcurrentGametype.Contains("TeamDeathMatch") && this.ynbenableSkillTDM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("TDM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapTDM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1){
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByTDM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                } else if ((this.intwonTicketsTDM * this.intTicketcount/100) <= this.intTicketsdif && ( this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes)){
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByTDM);
                    if (this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes){
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                } else {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }else
            {
                DebugInfoSkill(this.strcurrentGametype + ": ^8^bGamemode not supported or EndRound-Scrambler turned off for this gamemode.");
            }
            
            
//          if (!this.boolTeamsScrambled && this.intCurrentRoundCount == 1 && this.boolticketdif == true  && this.ynbenableSkill==enumBoolYesNo.Yes){
/*          if (!this.boolTeamsScrambled && this.boolticketdif == true  && this.ynbenableSkill==enumBoolYesNo.Yes){
                if(this.ynbScrambleMap == enumBoolYesNo.No && this.ynbScrambleEveryRound == enumBoolYesNo.Yes && this.strcurrentGametype != "squadrush0" && this.strcurrentGametype != "rushlarge0"){
                    this.boolticketdif = false;
                    DebugInfoSkill("Won with to much tickets --> Starting scrambler!");
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    if (strcurrentGametype != "squaddeathmatch0"){
                        this.boolfirstscrambler = true;
                        this.boolscramblefailed = false;
                        this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                        // StartScrambler();
                    }else if (strcurrentGametype != ""){
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "SkillBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );
                    }
                }else if (this.intCurrentRoundCount == 1) {
                    this.boolticketdif = false;
                    DebugInfoSkill("Scramble needed NEW ROUND --> Starting scrambler!");
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    if (strcurrentGametype != "squaddeathmatch0"){
                        this.boolfirstscrambler = true;
                        this.boolscramblefailed = false;
                        this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                        // StartScrambler();
                    }else if (strcurrentGametype != ""){
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "SkillBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );
                    }
                }else{
                    DebugInfoSkill("Won with to much tickets BUT same map again.");
                }
            }
            */
            
            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            
        }
        
        public virtual void OnRoundOverPlayers(List<CPlayerInfo> lstPlayers) { 
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache){
                if (this.dicPlayerCache[kvp.Key].playerWL != 1 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }
            
            int i = 1;
            this.dicPlayerScore.Clear();
            this.dicSquadScore.Clear();
            this.bestSquadTeamID = 0;
            
            foreach (CPlayerInfo cpiPlayer in lstPlayers){
                double value = 0;
                string tag = "";
                if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName)) {   
                    value = this.dicPlayerCache[cpiPlayer.SoldierName].playerValue;
                    tag = this.dicPlayerCache[cpiPlayer.SoldierName].tag;
                }
                CPlayerScoreInf newEntry = new CPlayerScoreInf(cpiPlayer.SoldierName, cpiPlayer.TeamID, cpiPlayer.SquadID, value, false, false, tag);
                this.dicPlayerScore.Add(i, newEntry);
                i++;
            }
            
            bool Sortiert;
            do{
                Sortiert = true; 
                for (int j = 1; j < this.dicPlayerScore.Count; j++) {
                    if (this.dicPlayerScore[j].playerValue < this.dicPlayerScore[j+1].playerValue){
                        CPlayerScoreInf temp = new CPlayerScoreInf(this.dicPlayerScore[j].playerName, this.dicPlayerScore[j].teamID, this.dicPlayerScore[j].playerSquad, this.dicPlayerScore[j].playerValue, false, false, this.dicPlayerScore[j].tag);
                        this.dicPlayerScore[j] = this.dicPlayerScore[j+1];
                        this.dicPlayerScore[j+1] = temp;
                    Sortiert = false;
                    }
                }  
            } while (!Sortiert);
            
            
            // bool Sortiert;
            // do{
                // Sortiert = true; 
                // for (int j = 1; j < this.dicPlayerScore.Count; j++) {
                    // if (this.dicPlayerScore[j].playerScore < this.dicPlayerScore[j+1].playerScore){ 
                        // CPlayerScoreInf temp = new CPlayerScoreInf(this.dicPlayerScore[j].playerName, this.dicPlayerScore[j].teamID, this.dicPlayerScore[j].playerSquad, this.dicPlayerScore[j].playerScore, false, false);
                        // this.dicPlayerScore[j] = this.dicPlayerScore[j+1];
                        // this.dicPlayerScore[j+1] = temp;
                    // Sortiert = false;
                    // }
                // }  
            // } while (!Sortiert);
            
        }       
            
        public virtual void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) { 
         
            if (this.boolLevelStart == false && this.boolLevelLoaded){
                this.boolmessagesent = false;
                this.boolfirstscrambler = false;
                this.boolLevelLoaded = false;
                this.boolLevelStart = true;
                this.DebugInfo("First Spawner: ^bRound started!");
                this.DebugInfoGuard("First Spawner: ^bRound started!");
                this.DTLevelStart = DateTime.Now;
            }
            
            if (this.dicPlayerCache.ContainsKey(soldierName)) {
                this.dicPlayerCache[soldierName].tobebalanced = false;
            }
            //string msg = soldierName + " spawned.";
            //this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }
        
        public override void OnLogin() {
             this.DebugInfo("=================OnLogin=============");
            //this.boolOnLogin = true;
            this.boolfirstwarningWL = false;
            this.m_isPluginEnabled = true; 
            this.RegisterAllCommands();
            //this.boolOnLogin = true;
            this.boolLevelStart = false;
            this.boolLevelLoaded = true;
            this.boolFirstOP = false;
            this.boolgametype = false; 
            this.DTLevelStart = new DateTime();
            this.boolnoplayer = false;
            this.intScrambledPlayers = 0;
            this.boolscrambleNow = false;
            this.boolscrambleActive = false;
            this.intTicketsdif = -1;
            this.boolTeamsScrambled = false;
            this.boolticketdif = false;
            this.DTScramblestarted = new DateTime();
            this.boolRunOnList = false;

        }

        public override void OnServerInfo(CServerInfo csiServerInfo)
        {

            //DebugInfo(csiServerInfo.GameMod.ToString());
            //DebugInfo(csiServerInfo.GameMode.ToString());

            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + csiServerInfo.BlazeGameState);
            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + csiServerInfo.BlazePlayerCount);

            if (Servertype == "AUTOMATIC")
            {
                if (string.IsNullOrEmpty(csiServerInfo.BlazeGameState))
                {
                    DebugInfoSkill("BF3 detected");
                    Servertype = "BF3";
                }
                else
                {
                    DebugInfoSkill("BF4 detected");
                    Servertype = "BF4";
                }
            }

            this.strcurrentGametype = csiServerInfo.GameMode;
            this.intMaxSlots = csiServerInfo.MaxPlayerCount;
            this.intCurrentRoundCount = csiServerInfo.TotalRounds - csiServerInfo.CurrentRound;

            if (intCurrentRoundCount < 1)
            {
                intCurrentRoundCount = 1;
            }

            this.intScoreTeamA = 0;
            this.intScoreTeamB = 0;
            //DebugInfoSkill("Before Check: " + this.intTicketcount + ", GameMode: " + this.strcurrentGametype);

            if (this.strcurrentGametype.Contains("Elimination"))
            {
                this.intScoreTeamA = 10000;
                this.intScoreTeamB = 10000;
            }
            else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
            {
                this.intScoreTeamA = 10000;
                this.intScoreTeamB = 10000;
            }
            else
            {
                foreach (TeamScore score in csiServerInfo.TeamScores)
                {
                    if (score.TeamID == 1)
                    {
                        this.intScoreTeamA = score.Score;
                        if (this.intTicketcount == 123987123 && this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            this.intTicketcount = score.WinningScore;
                        }
                    }
                    else if (score.TeamID == 2)
                    {
                        this.intScoreTeamB = score.Score;
                    }
                }
            }


            if ((this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreCONQUEST;
                if ((this.TeamA + this.TeamB) < this.intTreshCONQUEST)
                {
                    this.intAllowDif = this.intAllowDif1CONQUEST;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2CONQUEST;
                }
            }

            if (this.strcurrentGametype.Contains("Domination0") && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreDOM;
                if ((this.TeamA + this.TeamB) < this.intTreshDOM)
                {
                    this.intAllowDif = this.intAllowDif1DOM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2DOM;
                }
            }

            else if (this.strcurrentGametype.Contains("Obliteration") && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreOB;
                if ((this.TeamA + this.TeamB) < this.intTreshOB)
                {
                    this.intAllowDif = this.intAllowDif1OB;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2OB;
                }
            }

            else if ((this.strcurrentGametype.Contains("Rush")) && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreRUSH;

                if ((this.TeamA + this.TeamB) < this.intTreshRUSH)
                {
                    this.intAllowDif = this.intAllowDif1RUSH;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2RUSH;
                }
            }
            //else if (this.strcurrentGametype.Contains("GunMaster") && this.boolLevelStart)
            else if ((this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0")) && this.boolLevelStart)
            {
                this.intminScore = 0;
                if ((this.TeamA + this.TeamB) < this.intTreshGM)
                {
                    this.intAllowDif = this.intAllowDif1GM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2GM;
                }
            }
            else if ((this.strcurrentGametype.Contains("Elimination")) && this.boolLevelStart)
            {
                this.intminScore = 0;
                if ((this.TeamA + this.TeamB) < this.intTreshDF)
                {
                    this.intAllowDif = this.intAllowDif1DF;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2DF;
                }
            }
            else if (this.strcurrentGametype.Contains("TeamDeathMatch") && this.boolLevelStart)
            {
                this.intminScore = this.intTicketcount - intminScoreTDM;
                if ((this.TeamA + this.TeamB) < this.intTreshTDM)
                {
                    this.intAllowDif = this.intAllowDif1TDM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2TDM;
                }
            }

            int intticketdiffrence = 0;
            //DebugInfoSkill("TicketCount: " + this.intTicketcount);
            if (this.intScoreTeamA > this.intScoreTeamB) intticketdiffrence = this.intScoreTeamA - this.intScoreTeamB;
            else if (this.intScoreTeamA < this.intScoreTeamB) intticketdiffrence = this.intScoreTeamB - this.intScoreTeamA;

            if (!this.boolmessagesent && (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes && (this.intScoreTeamA < 40 || this.intScoreTeamB < 40))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageCONQUEST != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageCONQUEST);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageCONQUEST, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageCONQUEST, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsCONQUEST * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageCONQUEST != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageCONQUEST);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageCONQUEST, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageCONQUEST, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            if (!this.boolmessagesent && this.strcurrentGametype.Contains("Domination0") && this.ynbenableSkillDOM == enumBoolYesNo.Yes && (this.intScoreTeamA < 40 || this.intScoreTeamB < 40))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - DOM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapDOM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageDOM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageDOM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageDOM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageDOM, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsDOM * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageDOM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageDOM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageDOM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageDOM, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            else if (!this.boolmessagesent && this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes && (this.intScoreTeamA < 2 || this.intScoreTeamB < 2))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - OB - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapOB == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageOB != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageOB);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageOB, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageOB, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsOB * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageOB != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageOB);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageOB, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageOB, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            else if (!this.boolmessagesent && this.strcurrentGametype.Contains("TeamDeathMatch") && this.ynbenableSkillTDM == enumBoolYesNo.Yes && (this.intScoreTeamA > (this.intTicketcount - 25) || this.intScoreTeamB > (this.intTicketcount - 25)))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - TDM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapTDM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageTDM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageTDM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageTDM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageTDM, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsTDM * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageTDM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageTDM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageTDM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageTDM, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ": ^b^1No reason to scramble!");
                }
            }

            /*
                if ((this.intScoreTeamA < 40 || this.intScoreTeamB < 40) && this.ynbenableSkill==enumBoolYesNo.Yes && this.strcurrentGametype != "squadrush0" && this.strcurrentGametype != "rushlarge0"){
    //              if ((this.intwonTickets <= intticketdiffrence || this.ynbScrambleMap == enumBoolYesNo.Yes)&& this.intCurrentRoundCount == 1){
                    if (this.intwonTickets <= intticketdiffrence || this.ynbScrambleMap == enumBoolYesNo.Yes){
                    
                        if (this.strScrambleMessage !="" && this.boolticketdif == false){
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessage, "30");
                        }
                        this.boolticketdif = true;      
                    }
                }
            
            */


            this.DebugInfo("Serverinfo: ^1ScoreA: ^i" + this.intScoreTeamA.ToString() + "^n, ^2ScoreB: ^i" + this.intScoreTeamB.ToString() + "^n, ^9RoundCount: ^i " + this.intCurrentRoundCount.ToString());


            UpdateCheck();

        }

        private void UpdateCheck()
        {
            if (Check4Update == enumBoolYesNo.Yes)
            {
                try
                {
                    DateTime updatehelper = lastupdatecheck.AddHours(3);
                    if (DateTime.Compare(updatehelper, DateTime.Now) <= 0)
                    {
                        WebClient wc = new WebClient();
                        string latestversion = wc.DownloadString("https://forum.myrcon.com/showthread.php?7169");

                        latestversion = latestversion.Substring(latestversion.IndexOf("<title>") + 7);
                        latestversion = latestversion.Substring(0, latestversion.IndexOf("</title>"));
                        latestversion = latestversion.Substring(latestversion.IndexOf("TrueBalancer") + 13);

                        if (GetPluginVersion() != latestversion)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: ^b^2UPDATE " + latestversion + " AVAILABLE");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: your version: " + GetPluginVersion());
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: latest version " + latestversion);
                        }
                        lastupdatecheck = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "[TB] ERROR checking for Update: " + ex);
                }
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {
            int numStatsFetch = this.intMaxPlayersToFetch;
            int numRemaining = lstPlayers.Count;

            if (lstPlayers.Count == 0 && this.boolnoplayer == false){           
                this.boolnoplayer = true;
                this.dicPlayerCache.Clear();
                this.DebugInfo("^3^b" + lstPlayers.Count.ToString() + " Players on the server.");
                return;
            } else if (lstPlayers.Count != 0 && this.boolnoplayer) {
                this.boolnoplayer = false;
            } else if (lstPlayers.Count == 0) {
                return; // suppress debugging messages for empty server
            }

            TimeSpan ScrambleTime  = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;
            if (ScrambleTime.TotalSeconds > 20 && this.boolscrambleActive){
                this.DebugInfoSkill("^b^8Was not able to scramble teams in 15 seconds! Teams partly scrambled.");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }
            
            if (!this.boolscrambleActive){
                List<String> removeFromCache = new List<String>();
                
                int numWithStats = 0;
                foreach (String k in this.dicPlayerCache.Keys) {
                    if (this.dicPlayerCache[k].statsFetched) ++numWithStats;
                }
                DebugInfoSkill("OnListPlayers: Players = " + lstPlayers.Count + " vs Known with stats = " + numWithStats);
            
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                {
                    if (this.m_isPluginEnabled == false) break;

                    this.boolplayerexists = false;
                    foreach (CPlayerInfo cpiPlayer in lstPlayers)
                    {
                        if (this.m_isPluginEnabled == false) break;

                        if (cpiPlayer.SoldierName == kvp.Key)
                        {
                            boolplayerexists = true;
                            if (this.dicPlayerCache[kvp.Key].teamID != cpiPlayer.TeamID){
                                if(this.boolmanuellchange && this.dicPlayerCache[kvp.Key].teamID != 0 && kvp.Key != this.strMovedPlayer){
                                    
                                    this.dicPlayerCache[kvp.Key].Playerjoined = DateTime.Now;
                                    this.dicPlayerCache[kvp.Key].playerWL = 0;
                                    this.DebugInfo("Switched Manually!");
                                    
                                }
                                this.dicPlayerCache[kvp.Key].teamID = cpiPlayer.TeamID;
                                
                            }
                            this.dicPlayerCache[kvp.Key].playerSquad = cpiPlayer.SquadID;
                            this.dicPlayerCache[kvp.Key].score = cpiPlayer.Score;


                            if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                            {
                                if (this.ScrambleByCONQUEST == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByCONQUEST == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByCONQUEST == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByCONQUEST == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByCONQUEST == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByCONQUEST == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            if (this.strcurrentGametype.Contains("Domination"))
                            {
                                if (this.ScrambleByDOM == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByDOM == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByDOM == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByDOM == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByDOM == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByDOM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Obliteration"))
                            {
                                if (this.ScrambleByOB == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByOB == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByOB == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByOB == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByOB == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByOB == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Rush"))
                            {
                                if (this.ScrambleByRUSH == "Rank"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }else if (this.ScrambleByRUSH == "Skill"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }else if (this.ScrambleByRUSH == "SPM"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }else if (this.ScrambleByRUSH == "SPMcombat"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }else if (this.ScrambleByRUSH == "K/D"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByRUSH == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                            {
                                if (this.ScrambleByGM == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByGM == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByGM == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByGM == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByGM == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByGM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Elimination"))
                            {
                                if (this.ScrambleByDF == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByDF == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByDF == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByDF == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByDF == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByDF == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                            {
                                if (this.ScrambleByTDM == "Rank"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }else if (this.ScrambleByTDM == "Skill"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }else if (this.ScrambleByTDM == "SPM"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }else if (this.ScrambleByTDM == "SPMcombat"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }else if (this.ScrambleByTDM == "K/D"){
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByTDM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            
                            
                            
                            if ( (  ((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || ( ((IList<string>)this.strAClantagWhitelist).Contains(dicPlayerCache[cpiPlayer.SoldierName].tag) && dicPlayerCache[cpiPlayer.SoldierName].tag != String.Empty )  )   && this.ynbWhitelist == enumBoolYesNo.Yes)
                            {
                                this.dicPlayerCache[kvp.Key].playerWL = 1;
                            }
                            
                            break;
                        }
                    }
                    if (boolplayerexists == false)
                    {
                        if (!removeFromCache.Contains(kvp.Key))
                        {
                            removeFromCache.Add(kvp.Key);
                        }
                        // FIXME this.dicPlayerCache.Remove(kvp.Key);
                        //PlayersOnServer.Remove(kvp.Key);
                    }
                }
                
                foreach (String s in removeFromCache)
                {
                    if (this.dicPlayerCache.ContainsKey(s))
                    {
                        this.dicPlayerCache.Remove(s);
                    }
                }


                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    if (this.m_isPluginEnabled == false) break;
                    
                    --numRemaining;

                    if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName) == true && this.dicPlayerCache[cpiPlayer.SoldierName].statsFetched)
                    {
                        if (this.dicPlayerCache[cpiPlayer.SoldierName].teamID != cpiPlayer.TeamID){
                                if(this.boolmanuellchange && this.dicPlayerCache[cpiPlayer.SoldierName].teamID != 0 && cpiPlayer.SoldierName != this.strMovedPlayer){
                                    
                                    this.dicPlayerCache[cpiPlayer.SoldierName].Playerjoined = DateTime.Now;
                                    this.dicPlayerCache[cpiPlayer.SoldierName].playerWL = 0;
                                    this.DebugInfo("Switched Manually!(2)");
                                    
                                }
                                this.dicPlayerCache[cpiPlayer.SoldierName].teamID = cpiPlayer.TeamID;
                                
                        }
                        this.dicPlayerCache[cpiPlayer.SoldierName].playerSquad = cpiPlayer.SquadID;
                        this.dicPlayerCache[cpiPlayer.SoldierName].score = cpiPlayer.Score;

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.ScrambleByDOM == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByDOM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.ScrambleByOB == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByOB == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByOB == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByOB == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByOB == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByOB == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            if (this.ScrambleByRUSH == "Rank"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }else if (this.ScrambleByRUSH == "Skill"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }else if (this.ScrambleByRUSH == "SPM"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }else if (this.ScrambleByRUSH == "SPMcombat"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }else if (this.ScrambleByRUSH == "K/D"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByRUSH == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            if (this.ScrambleByGM == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByGM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            if (this.ScrambleByDF == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByDF == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.ScrambleByTDM == "Rank"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }else if (this.ScrambleByTDM == "Skill"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }else if (this.ScrambleByTDM == "SPM"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }else if (this.ScrambleByTDM == "SPMcombat"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }else if (this.ScrambleByTDM == "K/D"){
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByTDM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        
                        
                        
                        if ( (  ((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || ( ((IList<string>)this.strAClantagWhitelist).Contains(dicPlayerCache[cpiPlayer.SoldierName].tag) && dicPlayerCache[cpiPlayer.SoldierName].tag != String.Empty )  )   && this.ynbWhitelist == enumBoolYesNo.Yes)
                        {
                            this.dicPlayerCache[cpiPlayer.SoldierName].playerWL = 1;
                        }
                    }
                    else
                    {
                        PlayerStats stats = new PlayerStats();
                        stats.reset();

                        /* If stats fetching is taking too long, skip for the rest of the current players list */

                        if (numStatsFetch > 0)
                        {
                            DebugInfoSkill("Starting Battlelog stats fetch for: ^b^5" + cpiPlayer.SoldierName);
                            DateTime startTime = DateTime.Now;

                            if (Servertype == "BF3")
                            {
                                stats = this.bclient.getPlayerStats(cpiPlayer.SoldierName, BattlelogClient.ServerType.BF3);
                            }
                            if (Servertype == "BF4")
                            {
                                stats = this.bclient.getPlayerStats(cpiPlayer.SoldierName, BattlelogClient.ServerType.BF4);
                            }

                            stats.statsFetched = true;

                            String name = (stats.tag != String.Empty) ? "[" + stats.tag + "]" + cpiPlayer.SoldierName : cpiPlayer.SoldierName;
                            DebugInfoSkill("^5^b" + name + "^n^9 stats fetched, rank is " + stats.rank + ", spm is " + stats.spm.ToString("F0") + ", ^b" + numRemaining + "^n players still need stats. ^2ELAPSED TIME: " + DateTime.Now.Subtract(startTime).TotalSeconds.ToString("F1") + " seconds");

                            --numStatsFetch;
                        }

                        double ValueTemp = 0;
                        
                        double tempskill = stats.skill;
                        
                        if (tempskill == 0)
                            tempskill = (this.skillA + this.skillB) / 2;
                        else if (tempskill < 0)
                            tempskill = 0;

                        double tempspm = stats.spm;
                        if (tempspm == 0)
                            tempspm = (this.spmA + this.spmB) / 2;

                        double tempspmcombat = stats.spmcombat;
                        if (tempspmcombat == 0)
                        {
                            tempspmcombat = (this.spmcombatA + this.spmcombatB) / 2;
                        }

                        double tempkdr = stats.kdr;
                        if (tempkdr == 0)
                            tempkdr = (this.kdrA + this.kdrB) / 2;


                        double TBvalueTemp = TBValue(stats.rank, tempskill, tempspm, tempspmcombat, tempkdr);

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.ScrambleByDOM == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByDOM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByOB == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByOB == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByOB == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByOB == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByOB == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            if (this.ScrambleByRUSH == "Rank"){
                                ValueTemp = stats.rank;
                            }else if (this.ScrambleByRUSH == "Skill"){
                                ValueTemp = tempskill;
                            }else if (this.ScrambleByRUSH == "SPM"){
                                ValueTemp = tempspm;
                            }else if (this.ScrambleByRUSH == "SPMcombat"){
                                ValueTemp = tempspmcombat;
                            }else if (this.ScrambleByRUSH == "K/D"){
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByRUSH == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            if (this.ScrambleByGM == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByGM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            if (this.ScrambleByDF == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByDF == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.ScrambleByTDM == "Rank"){
                                ValueTemp = stats.rank;
                            }else if (this.ScrambleByTDM == "Skill"){
                                ValueTemp = tempskill;
                            }else if (this.ScrambleByTDM == "SPM"){
                                ValueTemp = tempspm;
                            }else if (this.ScrambleByTDM == "SPMcombat"){
                                ValueTemp = tempspmcombat;
                            }else if (this.ScrambleByTDM == "K/D"){
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByTDM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }

                        bool commander = cpiPlayer.Type == 1 || cpiPlayer.Type == 2 ? true : false;
                        bool spectator = cpiPlayer.Type == 3 ? true : false;
                        if ((((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || (((IList<string>)this.strAClantagWhitelist).Contains(stats.tag) && stats.tag != String.Empty)) && this.ynbWhitelist == enumBoolYesNo.Yes)
                        {

                            CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 1, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, false, commander, spectator);
                            newEntry.statsFetched = stats.statsFetched;
                            this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                            //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                            //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            //this.ExecuteCommand("procon.protected.pluginconsole.write", spm.ToString());
                        }
                        else
                        {
                            if (cpiPlayer.TeamID == 0)
                            {
                                CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 0, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, true, commander, spectator);
                                newEntry.statsFetched = stats.statsFetched;
                                this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                                //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                                //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            }
                            else
                            {
                                CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 0, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, false, commander, spectator);
                                newEntry.statsFetched = stats.statsFetched;
                                this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                                //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                                //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            }
                            //this.ExecuteCommand("procon.protected.pluginconsole.write", spm.ToString());
                        }
                        
                    }
                }
                 
                if (this.ynbWhitelist == enumBoolYesNo.Yes && this.ynbincludeVIPlist == enumBoolYesNo.Yes) {
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
                }
                 
                 
                this.strMovedPlayer = "";
                this.boolmanuellchange = false;
                this.DebugInfo("-------OP-------");
                
                
                string printSoldier = "";
                string strPlayerlist = "";
                string strPLTeam1 = "";
                string strPLTeam2 = "";
                string strPLNeutral = "";
                
                
                /*  Dictionary<string, CPlayerJoinInf> dicSorted = new Dictionary<string, CPlayerJoinInf>();
                
                dicSorted = from k in dicPlayerCache.Keys
                            orderby dicPlayerCache[k].Playerjoined ascending
                            select k; */

                Dictionary<string, CPlayerJoinInf> dicPlayerSorted = new Dictionary<string, CPlayerJoinInf>();

                string whitelisttemp = "^2";

                foreach (KeyValuePair<string, CPlayerJoinInf> kvp1 in this.dicPlayerCache)
                {
                    if (this.dicPlayerCache[kvp1.Key].playerWL == 1 || dicPlayerCache[kvp1.Key].IsCommander || dicPlayerCache[kvp1.Key].IsSpectator)
                    {
                        whitelisttemp = whitelisttemp + "^b" + kvp1.Key + "^n" + ", ";
                    }
                    // DateTime maxValueJoined = new DateTime();
                    double minpoints = 100000000;
                    KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();
                    
                    foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in this.dicPlayerCache)
                    {
                        if (this.dicPlayerCache[kvp2.Key].score <= minpoints && dicPlayerSorted.ContainsKey(kvp2.Key) == false)
                        {
                            minpoints = this.dicPlayerCache[kvp2.Key].score;
                            kvplastjoiner = kvp2;
                        }
                    }
                    dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);    
                }
                
                
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerSorted)
                {
                    printSoldier = kvp.Key.Replace("{", "(");
                    printSoldier = printSoldier.Replace("}", ")");
                    if (dicPlayerSorted[kvp.Key].teamID == 1){
                        strPLTeam1 = strPLTeam1 + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }else if (dicPlayerSorted[kvp.Key].teamID == 2){
                        strPLTeam2 = strPLTeam2 + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }else{
                        strPLNeutral = strPLNeutral + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }
                }
            
    /*          foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                {
                    printSoldier = kvp.Key.Replace("{", "(");
                    printSoldier = printSoldier.Replace("}", ")");
                    if (this.dicPlayerCache[kvp.Key].teamID == 1){
                        strPLTeam1 = strPLTeam1 + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                        "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                        this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                    }else if (this.dicPlayerCache[kvp.Key].teamID == 2){
                        strPLTeam2 = strPLTeam2 + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                        "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                        this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                    }else{
                        strPLNeutral = strPLNeutral + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                        "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                        this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                    }
                } */
                            
                strPlayerlist = "\n^b^4TEAM 1:^n " + strPLTeam1 + "\n\n^bTEAM 2:^n " + strPLTeam2 + "\n\n^bNeutral:^n " + strPLNeutral;
                
                this.DebugInfo(strPlayerlist);
                this.DebugInfo("Online whitelisted players: " + whitelisttemp);
                
                //this.DebugInfo("WaitSeconds: " + this.intWaitSeconds.ToString());

            
                this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
                if (this.boolLevelStart && this.TSLevelStartWait.TotalSeconds > 30)
                {
                        if (this.boolFirstOP == false)
                        {
                            this.boolFirstOP = true;                    
                        }   

                        if (strcurrentGametype != "squaddeathmatch0")
                        {
                            // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                                CompareTeams();
                            // } else {
                                // this.DebugInfo("Not comparing teams. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB  );
                            // }
                        }
                        else if (strcurrentGametype != "")
                        {
                            if (this.boolgametype == false)
                            {
                                this.boolgametype = true;
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );
                                
                            }
                        }   
                    } 
            }else {

                DebugInfoSkill("^3OnListPlayers, skill scramble in progress");

                double afterscrambleValueA = 0;
                double afterscrambleTeamSizeA = 0;
                double afterscrambleValueB = 0;
                double afterscrambleTeamSizeB = 0;
                if (this.boolscrambleNow && this.boolRunOnList){
                    this.boolTeamsScrambled = true;
                    foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
                        if (this.m_isPluginEnabled == false) break;
                        foreach (CPlayerInfo cpiPlayer in lstPlayers){
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerScore[kvp.Key].playerName == cpiPlayer.SoldierName){
                                if (cpiPlayer.TeamID == this.dicPlayerScore[kvp.Key].teamID && cpiPlayer.SquadID == this.dicPlayerScore[kvp.Key].playerSquad){
                                    this.dicPlayerScore[kvp.Key].scrambled = true;
                                    if (cpiPlayer.TeamID == 1)
                                    {
                                        afterscrambleValueA = afterscrambleValueA + this.dicPlayerScore[kvp.Key].playerValue;
                                        afterscrambleTeamSizeA++;
                                    }
                                    else
                                    {
                                        afterscrambleValueB = afterscrambleValueB + this.dicPlayerScore[kvp.Key].playerValue;
                                        afterscrambleTeamSizeB++;
                                    }
                                }else{
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    this.boolTeamsScrambled = false;
                                    this.DebugInfoSkill("^3Not scrambled: ^b" + cpiPlayer.SoldierName + "^n. Is in: ^b" + cpiPlayer.TeamID.ToString() +"."+cpiPlayer.SquadID +"^n - Goal: ^b" + this.dicPlayerScore[kvp.Key].teamID .ToString() +  "." + this.dicPlayerScore[kvp.Key].playerSquad.ToString());
                                }
                                break;
                            }
                        }
                    }
                    if (this.boolTeamsScrambled) {
                        this.boolRunOnList = false;
                        this.DebugInfoSkill("^b^2Teams are scrambled now!");
                        this.DebugInfoSkill("Team 1 Value: ^b^2" + afterscrambleValueA / afterscrambleTeamSizeA + "^9*^7" + afterscrambleTeamSizeA + "^n^9 --- Team 2 Value: ^b^2" + afterscrambleValueB / afterscrambleTeamSizeB + "^9*^7" + afterscrambleTeamSizeB);
                        double valuediffendround = afterscrambleValueA / afterscrambleTeamSizeA - afterscrambleValueB / afterscrambleTeamSizeB;
                        this.DebugInfoSkill("ValueDifference: ^b^2" + valuediffendround);
                        TimeSpan ScrambleDuration = DateTime.Now - this.DTScramblestarted;
                        this.DebugInfoSkill("ScrambleDuration: " + ScrambleDuration.TotalSeconds.ToString("F2") + " seconds");
                        this.boolscrambleNow = false;
                        this.boolscrambleActive = false;
                        this.intScrambleCount = 0;
                        if (this.boolLevelStart){
                            if (this.ynbScrambleMessage == enumBoolYesNo.Yes){
                                if (this.strScrambleDoneMsg !=""){
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strScrambleDoneMsg);
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.say", strScrambleDoneMsg , "all");
                                    }
                                    if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual){
                                        this.ExecuteCommand("procon.protected.send", "admin.yell", strScrambleDoneMsg , "30");
                                    }
                                }
                            }
                        }                   
                    } else{
                        this.boolRunOnList = false;
                        ScrambleNow();
                    }
                    
                } else  {
                    DebugInfoSkill("^3Scrambler Active OnList!");
                }
            }

            /* Since the playername matching feature requires the lastest list of player names, we have
             * to re-register commands after every player list update, bleah!
             */
            this.RegisterAllCommands();
            DebugInfoSkill("OnListPlayers handler returning");
        }
        
        public virtual void OnPlayerSquadChange(string soldierName, int teamId, int squadId) {
        
            TimeSpan ScrambleTime  = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;
            
            if (!this.boolscrambleActive && ScrambleTime.TotalSeconds > 30){
                if (this.boolLevelStart == true && !this.boolplayerleft)
                {   
                    this.DebugInfo("player squad change: ^b" + soldierName + " ^n" + this.boolLevelStart.ToString());
                    this.dicPlayerCache[soldierName].playerSquad = squadId;
                    //this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                } 
            }
            if (this.boolplayerleft)
                this.boolplayerleft = false;
        }
        
        public override void OnPlayerTeamChange(string strSoldierName, int iTeamID, int iSquadID) {


            if (DateTime.Now > this.EndRoundTime && this.showfirstmove == false && this.dicPlayerCache[strSoldierName].teamID != 0)
            {
                TimeSpan firstmovetime = DateTime.Now - this.DTLevelLoaded;
                this.showfirstmove = true;
                DebugInfoSkill("First Player moved by the game: ^n" + firstmovetime.TotalSeconds.ToString("F2") + " ^nseconds after Level Loaded");
            }

            this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
            if (!this.boolscrambleActive){

                if (this.boolLevelStart == true && this.boolFirstOP == true)
                {
                    if (!this.teamswitcher.Contains(strSoldierName))
                    {
                        if (this.dicPlayerCache[strSoldierName].teamID != 0)
                        {
                            this.DebugInfoGuard("^1^b*** TEAMSWITCHER ***: " + "^0^n[" + this.dicPlayerCache[strSoldierName].tag + "]^b" + strSoldierName + "^n^9--- From: ^0" + this.dicPlayerCache[strSoldierName].teamID.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + "^9, to: ^0" + iTeamID.ToString());
                            // this.DebugInfoGuard("Rank: " + this.dicPlayerCache[strSoldierName].rank + " - Skill: " + this.dicPlayerCache[strSoldierName].skill + " - SPM: " + this.dicPlayerCache[strSoldierName].spm + " - SPMcombat: " + this.dicPlayerCache[strSoldierName].spmcombat + " - K/D: " + this.dicPlayerCache[strSoldierName].kdr);
                        }
                        else
                        {
                            this.DebugInfoGuard("^0*** New Player ***: " + "^0^n[" + this.dicPlayerCache[strSoldierName].tag + "]^b" + strSoldierName + "^n^9--- From: ^0" + this.dicPlayerCache[strSoldierName].teamID.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + "^9, to: ^0" + iTeamID.ToString());
                            // this.DebugInfoGuard("Rank: " + this.dicPlayerCache[strSoldierName].rank + " - Skill: " + this.dicPlayerCache[strSoldierName].skill + " - SPM: " + this.dicPlayerCache[strSoldierName].spm + " - SPMcombat: " + this.dicPlayerCache[strSoldierName].spmcombat + " - K/D: " + this.dicPlayerCache[strSoldierName].kdr);
                        }
                    }

                    if (this.boolFirstOP == true)
                    {
                        this.boolmanuellchange = true;
                    }

                    if (this.ynbBalancingGuard == enumBoolYesNo.Yes && this.boolFirstOP && this.OnCommandMove.ContainsKey(strSoldierName) == false)
                    {

                        string sortBy = "";
                        double sortValueA = 0;
                        double sortValueB = 0;
                        double goodplayer = 0;
                        double badplayer = 0;
                        double dblValueDiff = 0;

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            dblValueDiff = this.dblValueDiffCONQUEST;

                            if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByCONQUEST == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            dblValueDiff = this.dblValueDiffDOM;

                            if (this.ScrambleByDOM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByDOM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            dblValueDiff = this.dblValueDiffCONQUEST;

                            if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByCONQUEST == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            dblValueDiff = this.dblValueDiffRUSH;

                            if (this.ScrambleByRUSH == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByRUSH == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByRUSH == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByRUSH == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByRUSH == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByRUSH == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            dblValueDiff = this.dblValueDiffGM;

                            if (this.ScrambleByGM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByGM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            dblValueDiff = this.dblValueDiffRUSH;

                            if (this.ScrambleByDF == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByDF == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            dblValueDiff = this.dblValueDiffTDM;

                            if (this.ScrambleByTDM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByTDM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByTDM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByTDM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByTDM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByTDM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }


                        double serveraverage = (sortValueA * this.TeamA + sortValueB * this.TeamB) / (this.TeamA + this.TeamB);
                        goodplayer = 1.25 * serveraverage;
                        badplayer = 0.75 * serveraverage;


                        //bool TeamAbetter = false;
                        //bool TeamBbetter = false;



                        int Maxdifference = this.intAllowDif - 1;
                        double goodvalue = 0;
                        double badvalue = 0;
                        double valuediff = 0;
                        int betterteam = 0;
                        int otherteam = 0;
                        int scorediff = 0;
                        int playerdiff = 0;

                        if (sortValueA > sortValueB)
                        {
                            betterteam = 1;
                            otherteam = 2;
                            goodvalue = sortValueA;
                            badvalue = sortValueB;
                            valuediff = sortValueA - sortValueB;

                        }
                        else if (sortValueA < sortValueB)
                        {
                            betterteam = 2;
                            otherteam = 1;
                            goodvalue = sortValueB;
                            badvalue = sortValueA;
                            valuediff = sortValueB - sortValueA;

                        }

                        if (this.dicPlayerCache[strSoldierName].teamID == 0 && this.dicPlayerCache[strSoldierName].playerWL != 1 && !dicPlayerCache[strSoldierName].IsCommander && !dicPlayerCache[strSoldierName].IsSpectator)
                        {

                            if (!this.teamswitcher.Contains(strSoldierName))
                            {
                                int newdiff = 0;
                                int teamsizemax = 0;
                                if (iTeamID == 1)
                                {
                                    newdiff = this.TeamA - this.TeamB;
                                    scorediff = this.intScoreTeamA - this.intScoreTeamB;
                                    teamsizemax = this.intMaxSlots / 2 - this.TeamB;
                                }
                                else
                                {
                                    newdiff = this.TeamB - this.TeamA;
                                    scorediff = this.intScoreTeamB - this.intScoreTeamA;
                                    teamsizemax = this.intMaxSlots / 2 - this.TeamA;
                                }
                                if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer)
                                {
                                    this.DebugInfoGuard("^2Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer)
                                {
                                    this.DebugInfoGuard("^1Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                else
                                {
                                    this.DebugInfoGuard("^0Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                this.DebugInfoGuard("Declared good, if " + sortBy + " >^2 " + goodplayer.ToString("F2") + "^9 --- Declared bad, if " + sortBy + " < ^1" + badplayer.ToString("F2"));
                                this.DebugInfoGuard("PlayerNumber --- Team 1: ^0" + this.TeamA + "^9, Team 2: ^0" + this.TeamB);
                                this.DebugInfoGuard("Score --- Team 1: ^0" + this.intScoreTeamA + "^9, Team 2: ^0" + this.intScoreTeamB);

                                if (sortValueA > sortValueB)
                                {
                                    this.DebugInfoGuard("Team ^0^b1^9^n is the better team.");

                                    if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    if (this.strcurrentGametype.Contains("Domination"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Obliteration"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }
                                    else if (this.strcurrentGametype.Contains("Elimination"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }

                                }
                                else if (sortValueB > sortValueA)
                                {
                                    this.DebugInfoGuard("Team ^0^b2^9^n is the better team.");

                                    if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    if (this.strcurrentGametype.Contains("Domination"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Obliteration"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }
                                    else if (this.strcurrentGametype.Contains("Elimination"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    } 

                                }
                                else this.DebugInfoGuard("\n");

                                //this.DebugInfoGuard(sortBy + " 1: " + sortValueA.ToString("F2") + " --- " + sortBy + " 2: " + sortValueB.ToString("F2") + "\n");

                                this.DebugInfoGuard("Player - Data: ^7TBValue: ^b" + this.dicPlayerCache[strSoldierName].TBvalue.ToString("F1") + "^n^0 - Rank: ^b" + this.dicPlayerCache[strSoldierName].rank.ToString("F2") + "^n^1 - Skill: ^b" + this.dicPlayerCache[strSoldierName].skill.ToString("F2") + "^n^2 - SPM: ^b" + this.dicPlayerCache[strSoldierName].spm.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.dicPlayerCache[strSoldierName].spmcombat.ToString("F2") + "^n^4 - K/D: ^b" + this.dicPlayerCache[strSoldierName].kdr.ToString("F2"));
                                this.DebugInfoGuard("Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
                                this.DebugInfoGuard("Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));
                                this.DebugInfoGuard("Team - Diff: ^7TBValue: ^b" + (this.TBvalueA - this.TBvalueB).ToString("F1") + "^n^0 - Rank: ^b" + (this.rankA - this.rankB).ToString("F2") + "^n^1 - Skill: ^b" + (this.skillA - this.skillB).ToString("F2") + "^n^2 - SPM: ^b" + (this.spmA - this.spmB).ToString("F2") + "^n^3 - SPMcombat: ^b" + (this.spmcombatA - this.spmcombatB).ToString("F2") + "^n^4 - K/D: ^b" + (this.kdrA - this.kdrB).ToString("F2"));

                                if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Domination"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Obliteration"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Elimination"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    this.DebugInfoGuard("^8^bGameMode not supported.");
                                }

                            }
                            else
                            {
                                this.teamswitcher.Remove(strSoldierName);
                                //this.DebugInfoGuard("New player moved: " + strSoldierName);
                            }

                        }
                        else if (this.dicPlayerCache[strSoldierName].playerWL != 1 && this.TSLevelStartWait.TotalSeconds > 90 && !dicPlayerCache[strSoldierName].IsCommander && !dicPlayerCache[strSoldierName].IsSpectator)
                        {

                            // if (iTeamID == 1 && this.dicPlayerCache[strSoldierName].teamID != 0){
                            // this.TeamA = this.TeamA + 1;
                            // this.TeamB = this.TeamB - 1;
                            // }else if(iTeamID == 2 && this.dicPlayerCache[strSoldierName].teamID != 0){
                            // this.TeamA = this.TeamA - 1;
                            // this.TeamB = this.TeamB + 1;
                            // }

                            if (!this.teamswitcher.Contains(strSoldierName))
                            {

                                int toteam = iTeamID;
                                double switchingvalue = 0;
                                int fromteam = this.dicPlayerCache[strSoldierName].teamID;

                                if (this.BalancedPlayers.Contains(strSoldierName) == false)
                                {

                                    if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer)
                                    {
                                        this.DebugInfoGuard("^2Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer)
                                    {
                                        this.DebugInfoGuard("^1Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    else
                                    {
                                        this.DebugInfoGuard("^0Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    this.DebugInfoGuard("Declared good, if " + sortBy + " >^2 " + goodplayer.ToString("F2") + "^9 --- Declared bad, if " + sortBy + " < ^1" + badplayer.ToString("F2"));
                                    this.DebugInfoGuard("PlayerNumber --- Team 1: ^0" + this.TeamA + "^9, Team 2: ^0" + this.TeamB);
                                    this.DebugInfoGuard("Score --- Team 1: ^0" + this.intScoreTeamA + "^9, Team 2: ^0" + this.intScoreTeamB);

                                    if (sortValueA > sortValueB) this.DebugInfoGuard("Team ^0^b1^9^n is the better team." + "\n");
                                    else if (sortValueB > sortValueA) this.DebugInfoGuard("Team ^0^b2^9^n is the better team." + "\n");
                                    else this.DebugInfoGuard("\n");

                                    //this.DebugInfoGuard(sortBy + " 1: " + sortValueA.ToString("F2") + " --- " + sortBy + " 2: " + sortValueB.ToString("F2") + "\n");

                                    this.DebugInfoGuard("Player - Data: ^7TBValue: ^b" + this.dicPlayerCache[strSoldierName].TBvalue.ToString("F1") + "^n^0 - Rank: ^b" + this.dicPlayerCache[strSoldierName].rank.ToString("F2") + "^n^1 - Skill: ^b" + this.dicPlayerCache[strSoldierName].skill.ToString("F2") + "^n^2 - SPM: ^b" + this.dicPlayerCache[strSoldierName].spm.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.dicPlayerCache[strSoldierName].spmcombat.ToString("F2") + "^n^4 - K/D: ^b" + this.dicPlayerCache[strSoldierName].kdr.ToString("F2"));
                                    this.DebugInfoGuard("Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
                                    this.DebugInfoGuard("Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));
                                    this.DebugInfoGuard("Team - Diff: ^7TBValue: ^b" + (this.TBvalueA - this.TBvalueB).ToString("F1") + "^n^0 - Rank: ^b" + (this.rankA - this.rankB).ToString("F2") + "^n^1 - Skill: ^b" + (this.skillA - this.skillB).ToString("F2") + "^n^2 - SPM: ^b" + (this.spmA - this.spmB).ToString("F2") + "^n^3 - SPMcombat: ^b" + (this.spmcombatA - this.spmcombatB).ToString("F2") + "^n^4 - K/D: ^b" + (this.kdrA - this.kdrB).ToString("F2"));
                                }
                                else
                                {
                                    this.DebugInfoGuard("^5-= TB Automatically =- Team 1: ^b" + this.TeamA + "^n --- Team 2: ^b" + this.TeamB);
                                    this.DebugInfoGuard("^5-= TB Automatically =- PlayerScore: ^b" + this.dicPlayerCache[strSoldierName].score);
                                    // this.ExecuteCommand("procon.protected.chat.write", "AutoBalancing Teams: " + strSoldierName);
                                }

                                if (toteam == 1)
                                {
                                    scorediff = this.intScoreTeamA - this.intScoreTeamB;
                                    playerdiff = this.TeamA - this.TeamB;
                                    switchingvalue = sortValueA;
                                }
                                else
                                {
                                    scorediff = this.intScoreTeamB - this.intScoreTeamA;
                                    playerdiff = this.TeamB - this.TeamA;
                                    switchingvalue = sortValueB;
                                }

                                TimeSpan balancetimer = new TimeSpan(0);
                                balancetimer = DateTime.Now - this.dicPlayerCache[strSoldierName].Playerjoined;

                                if (!this.BalancedPlayers.Contains(strSoldierName) && this.dicPlayerCache[strSoldierName].playerValue >= badplayer && scorediff >= (intScoreWTS * this.intTicketcount / 100) && playerdiff >= -1 && (this.strcurrentGametype.Contains("Rush") == false && this.strcurrentGametype.Contains("GunMaster") == false && this.strcurrentGametype.Contains("CaptureTheFlag0") == false && this.strcurrentGametype.Contains("Elimination") == false))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " true");                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "true");
                                    }

                                    if (this.ynbShameMessage == enumBoolYesNo.Yes)
                                    {
                                        string strTEMP = this.strShameMessage.Replace("%TeamSwitcher%", strSoldierName);
                                        if (this.boolVirtual)
                                        {
                                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say all - " + strTEMP);
                                        }
                                        else
                                        {
                                            this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                        }
                                    }

                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into the winning team.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into the winning team.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^1You are not allowed to switch into the winning team. ^3SHAME ON YOU!");
                                    this.DebugInfoGuard("^1You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into the winning team. SHAME ON YOU!");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && (this.strcurrentGametype.Contains("Rush") == false && this.strcurrentGametype.Contains("GunMaster") == false && this.strcurrentGametype.Contains("Elimination") == false && this.strcurrentGametype.Contains("CaptureTheFlag0") == false) && playerdiff >= -1 /*&& valuediff >= dblValueDiff*/ && ((this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && toteam == betterteam && scorediff > 5) || (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && toteam == otherteam && scorediff < -5)))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^5^bYou are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    this.DebugInfoGuard("^5You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && (this.strcurrentGametype.Contains("Rush") == true || this.strcurrentGametype.Contains("GunMaster") == true || this.strcurrentGametype.Contains("Elimination") == true || this.strcurrentGametype.Contains("CaptureTheFlag0") == true) && playerdiff >= -1 /*&& valuediff >= dblValueDiff*/ && ((this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && toteam == betterteam) || (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && toteam == otherteam)))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^5You are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    this.DebugInfoGuard("^5You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && playerdiff >= Maxdifference)
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^4You are not allowed to switch into this team, because you would unbalance the teams.");
                                    this.DebugInfoGuard("^4You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams.");
                                    }
                                }
                                else if (this.BalancedPlayers.Contains(strSoldierName))
                                {
                                    this.BalancedPlayers.Remove(strSoldierName);
                                }
                            }
                            else
                            {

                                this.teamswitcher.Remove(strSoldierName);
                                //this.DebugInfoGuard("TEAMSWITCHER moved: " + strSoldierName);
                            }
                        }
                        else if (this.dicPlayerCache[strSoldierName].playerWL == 1 || dicPlayerCache[strSoldierName].IsCommander || dicPlayerCache[strSoldierName].IsSpectator)
                        {
                            this.DebugInfoGuard("^2^b" + strSoldierName + "^n is VIP, Commander or Spectator.");
                        }
                    }
                    else if (this.OnCommandMove.ContainsKey(strSoldierName))
                    {
                        this.DebugInfoGuard("^6BG: Player moved by ^bTB-AdminCommand");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You have been moved to the other team by an admin.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "You have been moved to the other team by an admin.", "player", strSoldierName);
                        }
                        this.OnCommandMove.Remove(strSoldierName);
                        if (this.OnCommandMoveDone.Contains(strSoldierName))
                            this.OnCommandMoveDone.Remove(strSoldierName);
                    }

                    this.DebugInfoGuard("^0^b**************************************************************");
                    if (this.dicPlayerCache[strSoldierName].teamID == 0)
                    {
                        this.dicPlayerCache[strSoldierName].teamID = iTeamID;
                    }
                    this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");

                }
                else
                {
                    if (this.OnCommandMove.ContainsKey(strSoldierName))
                    {
                        this.DebugInfoGuard("^6Player moved by ^bTB-AdminCommand");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You have been moved to the other team by an admin.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "You have been moved to the other team by an admin.", "player", strSoldierName);
                        }
                        this.OnCommandMove.Remove(strSoldierName);
                        if (this.OnCommandMoveDone.Contains(strSoldierName))
                            this.OnCommandMoveDone.Remove(strSoldierName);
                    }
                }
            }
        } 

        public override void OnPlayerLeft(CPlayerInfo cpiPlayer) {
        
            this.boolplayerleft = true;
            // int WaitforLevel = 0;

            
            // WaitforLevel = this.TSLevelStartWait.Hours * 3600 + this.TSLevelStartWait.Minutes * 60 + this.TSLevelStartWait.Seconds;
            
            this.DebugInfo("player left: ^0^b" + cpiPlayer.SoldierName + "^n^0 " + this.boolLevelStart.ToString());

            // if (this.boolLevelStart == true)
            // {        
                // this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            // } 
            
        string printSoldier = "";
        printSoldier = cpiPlayer.SoldierName.Replace("{", "(");
        printSoldier = printSoldier.Replace("}", ")");
        
        if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName))
            {
                this.DebugInfo("In PlayerDic. Player Deleted.");
                this.dicPlayerCache.Remove(cpiPlayer.SoldierName);
            }  
        
        int toberomved = -1;
        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
            if (this.dicPlayerScore[kvp.Key].playerName== cpiPlayer.SoldierName){
                toberomved = kvp.Key;
                break;
            }
        }
        if (toberomved > -1){
            this.dicPlayerScore.Remove(toberomved);
        }
        
        this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
        if (this.boolFirstOP && this.boolLevelStart)
            {
                if (strcurrentGametype != "squaddeathmatch0")
                    {
                        // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                            CompareTeams();
                        // } else {
                            // this.DebugInfo("Not comparing teams. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB);
                        // }
                    }
                    else if (strcurrentGametype != "")
                    {
                        if (this.boolgametype == false)
                        {
                            this.boolgametype = true;
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );  
                        }
                    }
            }
            else
            {
                this.DebugInfo("Waiting for FirstOP (plyrLeft)");
            } 
        }

        public void OnPlayerKilled(Kill killInfo) {

            
            
            if (killInfo == null)
                    return;
            
            if(this.OnCommandMove.ContainsKey(killInfo.Victim.SoldierName))

            {            
                if (this.OnCommandMove[killInfo.Victim.SoldierName] == false)
                {
                    //normal move
                    if (this.OnCommandMoveDone.Contains(killInfo.Victim.SoldierName))
                    {
                        this.OnCommandMoveDone.Remove(killInfo.Victim.SoldierName);
                        this.OnCommandMove.Remove(killInfo.Victim.SoldierName);
                        this.DebugInfoGuard("^3^bMoving player to the other team went wrong on his last death. Not trying again! - ^8" + killInfo.Victim.SoldierName);
                    }
                    else
                    {
                        this.DebugInfoGuard("^2Trying to move dead player to the other side, due to TB-MoveCommand: ^b" + killInfo.Victim.SoldierName);
                        int team = 0;

                        if (this.dicPlayerCache[killInfo.Victim.SoldierName].teamID == 1)
                            team = 2;
                        else
                            team = 1;

                        this.OnCommandMoveDone.Add(killInfo.Victim.SoldierName);
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + killInfo.Victim.SoldierName + " " + team.ToString() + " 0 false");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.movePlayer", killInfo.Victim.SoldierName, team.ToString(), "0", "false");
                        }
                    }
                }
                
                else if(this.OnCommandMove[killInfo.Victim.SoldierName] == true)
                {
                    //force move  
                    this.TSForceMove = DateTime.Now - this.DTForceMove;
                    if (this.TSForceMove.TotalMilliseconds > 10000)
                    {
                        this.DebugInfoGuard("^3^bForce Moving player to the other team went wrong! - ^8" + killInfo.Victim.SoldierName);
                        this.OnCommandMove.Remove(killInfo.Victim.SoldierName);
                    }

                }
                

            }
            else if (this.boolwaitfordeath)
            {
                CPlayerInfo victim = killInfo.Victim;
                if (dicPlayerCache[victim.SoldierName].teamID == intFromTeam) {
                    this.DebugInfo("Player ^b^0" + victim.SoldierName + "^n^9 died.");
                    if (dicPlayerCache[victim.SoldierName].playerWL == 0 && !dicPlayerCache[victim.SoldierName].IsCommander && !dicPlayerCache[victim.SoldierName].IsSpectator)
                    {
                        if(this.boolwaitdead == false){
                        this.DebugInfo("^4Player ^b" + victim.SoldierName + "^n needs to be balanced.");
                        this.strdeadplayer = victim.SoldierName;
                        //this.dicPlayerCache[victim.SoldierName].tobebalanced = true;
                        //CompareTeams();
                        this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                        }
                    } else {
                        this.DebugInfo("^1Player ^b" + victim.SoldierName + "^n is VIP, Commander, Spectator or has been allready moved once.");
                    }
                }   
            }
        }
        
        
        #endregion      
      
        #region TrueBalancer Functions
        
        public void DebugInfo(string DebugMessage){
            if (ynbDebugMode == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write","^b^9TrueBalancer:^n " + DebugMessage);
            }
        }

        public void CompareTeams() {
            //int WaitforOPSec = 0;

            //this.TSWaitforOP = DateTime.Now - this.DTLevelStart;
            //WaitforOPSec = this.TSWaitforOP.Hours * 3600 + this.TSWaitforOP.Minutes * 60 + this.TSWaitforOP.Seconds;
            //if (WaitforOPSec >= 10)
            //{
                //this.DebugInfo("WaitforOPSec: " + WaitforOPSec.ToString());

            this.TeamA = 0;
            this.TeamB = 0;
            
            this.rankA = 0;
            this.rankB = 0;
            this.skillA = 0;
            this.skillB = 0;
            this.spmA = 0;
            this.spmB = 0;
            this.spmcombatA = 0;
            this.spmcombatB = 0;
            this.kdrA = 0;
            this.kdrB = 0;
            this.TBvalueA = 0;
            this.TBvalueB = 0;
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (!dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    if (this.dicPlayerCache[kvp.Key].teamID == 1)
                    {
                        this.TeamA++;
                        this.rankA = this.rankA + dicPlayerCache[kvp.Key].rank;
                        this.skillA = this.skillA + dicPlayerCache[kvp.Key].skill;
                        this.spmA = this.spmA + dicPlayerCache[kvp.Key].spm;
                        this.spmcombatA = this.spmcombatA + dicPlayerCache[kvp.Key].spmcombat;
                        this.kdrA = this.kdrA + dicPlayerCache[kvp.Key].kdr;
                        this.TBvalueA = this.TBvalueA + dicPlayerCache[kvp.Key].TBvalue;
                    }
                    if (this.dicPlayerCache[kvp.Key].teamID == 2)
                    {
                        this.TeamB++;
                        this.rankB = this.rankB + dicPlayerCache[kvp.Key].rank;
                        this.skillB = this.skillB + dicPlayerCache[kvp.Key].skill;
                        this.spmB = this.spmB + dicPlayerCache[kvp.Key].spm;
                        this.spmcombatB = this.spmcombatB + dicPlayerCache[kvp.Key].spmcombat;
                        this.kdrB = this.kdrB + dicPlayerCache[kvp.Key].kdr;
                        this.TBvalueB = this.TBvalueB + dicPlayerCache[kvp.Key].TBvalue;
                    }
                }
            }
            
            this.rankA = Math.Round(this.rankA/this.TeamA, 2);
            this.rankB = Math.Round(this.rankB/this.TeamB, 2);
            
            this.skillA = Math.Round(this.skillA/this.TeamA, 2);
            this.skillB = Math.Round(this.skillB/this.TeamB, 2);
            
            this.spmA = Math.Round(this.spmA/this.TeamA, 2);
            this.spmB = Math.Round(this.spmB/this.TeamB, 2);
            
            this.spmcombatA = Math.Round(this.spmcombatA/this.TeamA, 2);
            this.spmcombatB = Math.Round(this.spmcombatB/this.TeamB, 2);
            
            this.kdrA = Math.Round(this.kdrA/this.TeamA, 2);
            this.kdrB = Math.Round(this.kdrB/this.TeamB, 2);

            this.TBvalueA = Math.Round(this.TBvalueA / this.TeamA, 1);
            this.TBvalueB = Math.Round(this.TBvalueB / this.TeamB, 1);

            this.DebugInfo("Team A: ^6Size: ^b" + this.TeamA + "^n,^7 TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n,^0 Rank: ^b" + this.rankA.ToString("F2") + "^n,^1 Skill: ^b" + this.skillA.ToString("F2") + "^n,^2 SPM: ^b" + this.spmA.ToString("F2") + "^n,^3 SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n,^4 KDR: ^b" + this.kdrA.ToString("F2"));
            this.DebugInfo("Team B: ^6Size: ^b" + this.TeamB + "^n,^7 TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n,^0 Rank: ^b" + this.rankB.ToString("F2") + "^n,^1 Skill: ^b" + this.skillB.ToString("F2") + "^n,^2 SPM: ^b" + this.spmB.ToString("F2") + "^n,^3 SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n,^4 KDR: ^b" + this.kdrB.ToString("F2"));
            
            if (this.TeamA > this.TeamB)
            {
                this.intToTeam = 2;
                this.intFromTeam = 1;
                this.intPlayerDif = this.TeamA - this.TeamB;

            }
            else if (this.TeamB > this.TeamA)
            {
                this.intToTeam = 1;
                this.intFromTeam = 2;
                this.intPlayerDif = this.TeamB - this.TeamA;
            }
            else
            {
                this.intPlayerDif = 0;
            }
            this.DebugInfo("^4PlayerDiff: ^b" + this.intPlayerDif);
            
            if (this.intPlayerDif < 2)
            {
                this.DebugInfo("^2Teams are balanced.");
                this.boolfirstwarningWL = false;
                this.boolneedbalance = false;
                this.booltimer = false;
                this.intcountWarnings = 0;
                this.boolbalanced = true;
                
                if(this.boolstartBalance == true)
                {
                    this.boolstartBalance = false;
                    this.boolafterBalance = true;
                }
                this.intToTeam = 0;
                this.intFromTeam = 0;
                this.strdeadplayer = "";
                this.boolwaitfordeath = false;
                this.boolwaitdead = false;
            }
            
            
            
            
            
            if (!this.boolscrambleActive){

                if (this.intPlayerDif > this.intAllowDif)
                {
                    this.boolneedbalance = true;
                    this.DebugInfo("^4^bNeedBalance = true");
                    this.ExecuteCommand("procon.protected.send", "serverInfo");
                }


                if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CaptureTheFlag0") || this.strcurrentGametype.Contains("Elimination") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("Domination"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("Obliteration"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                {
                    if (this.intScoreTeamA < this.intminScore && this.intScoreTeamB < this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }


                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }


            }else {
                DebugInfoSkill("^3Scrambler Active Comparing teams!");
            }
            
        //}
        //else
        //{
        //    this.DebugInfo("Waiting for new round! WaitforOPSec: " + WaitforOPSec.ToString());
        //}
        }

        public void BalancingTimer() {
                   
            this.DebugInfo("------Timer-----");

            if (this.intPlayerDif < 2)
            {
                this.DebugInfo("^2Teams are balanced.(timer)");
                this.boolfirstwarningWL = false;
                this.boolneedbalance = false;
                this.intcountWarnings = 0;
                this.boolwaitfordeath = false;
                this.strdeadplayer = "";
                this.boolbalanced = true;
            
                if (this.boolstartBalance == true)
                {
                    this.boolstartBalance = false;
                    this.boolafterBalance = true;
                }
                this.intToTeam = 0;
                this.intFromTeam = 0;
                this.boolwaitdead = false;
            }

            if (this.boolneedbalance)
            {
                this.intcountWarnings++;
                if (this.intcountWarnings <= this.intWarnings)
                {
                    // Warnungen ausgeben. strWarning
                    string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                    strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                    if (ynbShowWarnings == enumBoolYesNo.Yes){
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP , "all");
                        }
                    }
                    this.DebugInfo(strTEMP);

                }
                if (this.intcountWarnings > this.intWarnings)
                {
                    this.boolstartBalance = true;
                    this.boolwaitfordeath = true;
                    //AUSFÜHREN von SERVERINFO
                    //this.ExecuteCommand("procon.protected.send", "serverInfo");
                    if(/*this.strdeadplayer != "" && this.boolwaitfordeath && */this.boolwaitdead == false){
                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CaptureTheFlag0") || this.strcurrentGametype.Contains("Elimination") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.intScoreTeamA < this.intminScore && this.intScoreTeamB < this.intminScore)
                            {
                                this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                startBalancing();
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else
                        {
                            this.DebugInfo("^8^b" + this.strcurrentGametype + " not supported!");
                        }
                    }
                    
                    //Start Balance
                    // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                        // startBalancing();
                    // } else {
                        // this.DebugInfo("Not starting Balance. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB);
                    // }
                }
            }

            this.booltimer = false;
            // if (this.boolLevelStart)
                // CompareTeams();
            this.DebugInfo("Timer End");    
        }
                
        public void startBalancing() {
        
            this.DebugInfo("startBalancing");
            this.boolstartBalance = true;
            this.boolwaitdead = true;
            
            //DateTime maxValue = new DateTime();
            this.strMovedPlayer = "";
                        
            
            Dictionary<string, CPlayerJoinInf> dicPlayerSorted = new Dictionary<string, CPlayerJoinInf>();
            Dictionary<string, CPlayerJoinInf> dicPlayerSortedTEMP = new Dictionary<string, CPlayerJoinInf>();
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].teamID == this.intFromTeam && this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    dicPlayerSortedTEMP.Add(kvp.Key, kvp.Value);    
                }
            }
            
            /*      
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp1 in dicPlayerSortedTEMP)
            {
                DateTime maxValueJoined = new DateTime();
                int minpoints = 100000000;
                KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();
                
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in dicPlayerSortedTEMP)
                {
                    if (dicPlayerSortedTEMP[kvp2.Key].score <= minpoints && dicPlayerSorted.ContainsKey(kvp2.Key) == false)
                    {
                        minpoints = dicPlayerSortedTEMP[kvp2.Key].score;
                        kvplastjoiner = kvp2;
                    }
                }
                dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);    
            }
            */
            
            bool Sortiert = true;
            do{
                Sortiert = true; 
                DateTime maxValueJoined = new DateTime();
                int minscore = 100000000;
                KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();
                
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in dicPlayerSortedTEMP){
                    
                    if (dicPlayerSorted.ContainsKey(kvp2.Key) == false && dicPlayerSortedTEMP[kvp2.Key].score < minscore){
                        minscore = dicPlayerSortedTEMP[kvp2.Key].score;
                        maxValueJoined = dicPlayerSortedTEMP[kvp2.Key].Playerjoined;
                        kvplastjoiner = kvp2;
                        Sortiert = false;
                    }
                    else if (dicPlayerSorted.ContainsKey(kvp2.Key) == false && dicPlayerSortedTEMP[kvp2.Key].score == minscore && maxValueJoined < dicPlayerSortedTEMP[kvp2.Key].Playerjoined){
                        maxValueJoined = dicPlayerSortedTEMP[kvp2.Key].Playerjoined;
                        kvplastjoiner = kvp2;
                        Sortiert = false;
                    }
                }
                if (Sortiert == false){
                    dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);
                }
                if (Sortiert)
                    this.DebugInfo("sorted");
                  
            } while (!Sortiert);
            
            
            string strsorted = "";
            string printSoldier = "";
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerSorted)
            {
                printSoldier = kvp.Key.Replace("{", "(");
                printSoldier = printSoldier.Replace("}", ")");

                strsorted = strsorted + "^4" + Convert.ToString(dicPlayerSorted[kvp.Key].score) + 
                "^9 / ^5" + dicPlayerSorted[kvp.Key].Playerjoined.ToString("HH:mm:ss")+ "^9 - ^0^b" + printSoldier + "^n^3 -=- ^9";
            }
            
            this.DebugInfo(strsorted);
            
            
            List<string> ToBeMovedList = new List<string>(dicPlayerSorted.Keys);
            int itemCount = ToBeMovedList.Count;
            
            int dblcutoff = 5;
            if (this.intFromTeam == 1) {
                if (this.TeamA/2 > 5)
                    dblcutoff = this.TeamA/2;
            }else if (this.intFromTeam == 2){
                if (this.TeamB/2 > 5)
                    dblcutoff = this.TeamB/2;
            }
            this.DebugInfo("Cutoff: ^b" + dblcutoff.ToString());
            for (int k=itemCount-1; k >= dblcutoff; k--){
            //this.DebugInfo("removed");
            ToBeMovedList.RemoveAt(k);
            //itemCount = ToBeMovedList.Count;
            }
            
            string completelist = "";
            itemCount = ToBeMovedList.Count;
            for (int k=0; k<itemCount; k++){
                completelist = completelist + "^0^b" + ToBeMovedList[k] + "^9^n: ^4" + this.dicPlayerCache[ToBeMovedList[k]].score + "^9 -=- ";
            }
            this.DebugInfo(completelist);
            
            bool willbebalanced = false;
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].tobebalanced && this.dicPlayerCache[kvp.Key].teamID == this.intFromTeam)
                {
                    this.strMovedPlayer = kvp.Key;
                    willbebalanced = true;
                    break;
                }
            }

            if (!willbebalanced && ToBeMovedList.Contains(this.strdeadplayer))
            {
                this.strMovedPlayer = this.strdeadplayer;
                willbebalanced = true;
            }

            if (!willbebalanced){
                this.DebugInfo("^3No Player dead and/or marked to be moved.");
                this.strdeadplayer = "";
            }else{
                string strTEMP = this.strLastWarning.Replace("%MovedPlayer%", this.strMovedPlayer);
                if (ynbShowBallancing == enumBoolYesNo.Yes){
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                    }
                }
                this.boolbalanced = false;
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.strMovedPlayer + " " + this.intToTeam.ToString() + " 0 false");
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.strMovedPlayer, this.intToTeam.ToString(), "0", "false");
                    LogMove("AUTOBALANCING [Player: " + this.strMovedPlayer + "]");
                }
                this.DebugInfo("playermoved");
                this.ExecuteCommand("procon.protected.chat.write", "AUTOBALANCING [Player: " + this.strMovedPlayer + "]");
                this.BalancedPlayers.Add(this.strMovedPlayer);
                
                this.dicPlayerCache[this.strMovedPlayer].teamID = intToTeam;
                this.dicPlayerCache[this.strMovedPlayer].playerWL = 2;
                this.dicPlayerCache[this.strMovedPlayer].tobebalanced = false;
                dicPlayerCache[this.strMovedPlayer].Playerjoined = DateTime.Now;
                string strTEMP2 = this.strBeenMoved.Replace("%MovedPlayer%", this.strMovedPlayer);
                
                if (ynbShowPlayermessage == enumBoolYesNo.Yes){
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + this.strMovedPlayer + " - " + strTEMP2);
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP2 , "player", this.strMovedPlayer);
                    }
                }

                this.DebugInfo("^b^0" + this.strMovedPlayer + "^n:^4 " + dicPlayerCache[this.strMovedPlayer].score + "^9 - ^4" + dicPlayerCache[this.strMovedPlayer].Playerjoined.ToString("HH:mm:ss") + "^9 -=- ^4" + 
                this.intFromTeam.ToString() + "." + dicPlayerCache[this.strMovedPlayer].playerSquad + " ---> "  + dicPlayerCache[this.strMovedPlayer].teamID.ToString());
                this.DebugInfo(strTEMP);
                this.DebugInfo(strTEMP2);
                this.strdeadplayer = "";    
            }
            
        this.DebugInfo("startBalancing end");
        this.boolwaitdead = false;
        }
        #endregion
    
        #region SkillScrambler

        public void DebugInfoSkill(string DebugMessage){
            if (ynbDebugModeSkill == enumBoolYesNo.Yes){
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
                //TextWriter tw = new StreamWriter("TrueBalancer.txt",true);
                //tw.WriteLine(DateTime.Now.ToString() + ": " + DebugMessage);
                //tw.Close();
            }
        }
        
        public void StartScrambler() {
            //this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            // NEU - Die Squadsortierung und das scramblen hier nach unten verschieben, um sicherzustellen, dass auch nur die gescrambled werden, die auch auf dem Server sind.
            DebugInfoSkill("^4Starting Scrambler now!");

            int CompareSlots = 0;

            if (this.intMaxSlots >= 48)
                CompareSlots = intMaxSlots - 1;
            else
                CompareSlots = intMaxSlots;


            if (this.dicPlayerCache.Count < CompareSlots){

                this.dicSquadScore.Clear();
                this.DTScramblestarted = DateTime.Now;
                this.boolscrambleActive = true;
                this.boolTeamsScrambled = false;
                this.boolFirstOP = false;
                this.intScrambleCount = 0;
                this.strErrorMsg = "";



                if (this.strScrambleMode == "Keep all Squads")
                {
                    KeepAllSquads();
                }
                else if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    KeepClanMates();
                }
                else if (this.strScrambleMode == "Keep no squads")
                {
                    KeepNoSquads();
                }

                
                
                //move them all out of squads
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache){
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + kvp.Key + " " + this.dicPlayerCache[kvp.Key].teamID.ToString() + " 0 true");
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", kvp.Key, this.dicPlayerCache[kvp.Key].teamID.ToString(), "0", "true");
                    }
                }
                ScrambleNow();
                
                
                //this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "1", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "ScrambleNow");
            }else{
                this.DebugInfoSkill("^3^bWas not able to scramble, because the server is full.");
            }
        
        }
        
        public void ScrambleNow() {
            
            this.boolscrambleNow = true;
            this.intScrambleCount ++;
            bool boolScrambledall = true;           
            this.DebugInfoSkill("^4Scrambling now!");
            
            TimeSpan ScrambleTime  = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;
            if (ScrambleTime.TotalSeconds > 20 || this.boolTeamsScrambled){
                this.DebugInfoSkill("^3^bWas not able to scramble teams in 15 seconds! Teams partly scrambled. TeamsScrambled? " + this.boolTeamsScrambled.ToString());
                if (this.boolLevelStart){
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes){
                        if (this.strScrambleDoneMsg !=""){
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strScrambleDoneMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strScrambleDoneMsg , "all");
                            }
                                if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual){
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", strScrambleDoneMsg , "30");
                                }
                        }
                    }
                }
                
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }   
            
            if (this.boolscrambleActive){
            
                if (this.boolfirstscrambler || this.boolscramblefailed){
                    this.boolfirstscrambler = false;
                    this.boolscramblefailed = false;
                    
                    if (this.strErrorMsg == "SetSquadFailed"){
                        string strfailedSquad = "";
                        int SSFTeamID = 0;
                        int intnewSquad = 0;
                        
                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && !this.dicPlayerScore[kvp.Key].scrambled){
                                this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                                if (!this.dicSquadList.ContainsKey(this.strFinalSquad)){
                                    this.dicSquadList.Add(this.strFinalSquad, this.dicPlayerScore[kvp.Key].playerSquad);
                                }
                                if (strfailedSquad == ""){
                                    SSFTeamID = this.dicPlayerScore[kvp.Key].teamID;
                                    strfailedSquad = this.strFinalSquad;
                                    /*for (int j = 1; j < 17; j++) {
                                        if (j<17 && !this.dicSquadList.ContainsValue(j)){
                                            intnewSquad = j;
                                            this.dicPlayerScore[kvp.Key].playerSquad = j;
                                            this.dicSquadList.Add(this.dicPlayerScore[kvp.Key].teamID.ToString() + "." + j.ToString(), j);
                                            break;
                                        }else if (j==17){
                                            intnewSquad = 0;
                                            this.dicPlayerScore[kvp.Key].playerSquad = 0;
                                            break;
                                        }
                                    }*/
                                }

                                if (strfailedSquad != this.strFinalSquad)
                                {
                                    this.DebugInfoSkill("^1" + this.strFinalSquad + ":" + "(SSF[noChange])Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }
                                }

                                /*if (strfailedSquad == this.strFinalSquad && intnewSquad != -1){
                                    this.dicPlayerScore[kvp.Key].playerSquad = intnewSquad;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b" + intnewSquad.ToString() + "^n^8(SSF[Change])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    
                                }else{
                                    this.DebugInfoSkill("^1" + this.strFinalSquad + ":" +  "(SSF[noChange])Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    
                                }*/
                            }
                        }
                        
                        for (int j = 1; j <= 20; j++) 
                        {
                            if (!this.dicSquadList.ContainsKey(SSFTeamID + "." + j))
                            {
                                intnewSquad = j;
                                this.dicSquadList.Add(SSFTeamID + "." + j, j);
                                break;
                            }

                        }

                        int playersinsquad = 0;

                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                        {
                            if (this.m_isPluginEnabled == false) break;
                            this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && this.strFinalSquad == strfailedSquad)
                            {
                                if (playersinsquad < 4)
                                {
                                    playersinsquad++;
                                    this.dicPlayerScore[kvp.Key].playerSquad = intnewSquad;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b" + intnewSquad.ToString() + "^n^8(SSF[Change])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }
                                    
                                }
                                else
                                {
                                    this.dicPlayerScore[kvp.Key].playerSquad = 0;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b 0 ^n^8(SSF[4+ players in Squad])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }
                                }
                            }
                        }


                                                                
                    }else {
                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && !this.dicPlayerScore[kvp.Key].scrambled){
                                this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                                if (!this.dicSquadList.ContainsKey(this.strFinalSquad)){
                                    this.dicSquadList.Add(this.strFinalSquad, this.dicPlayerScore[kvp.Key].playerSquad);
                                }       
                                //this.DebugInfoSkill("^4" + this.strFinalSquad + ":" +  "Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                boolScrambledall = false;
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                }
                                
                            }   
                        }
                    }
                    
                    this.strErrorMsg = "";  
                }else{
                    this.DebugInfoSkill("^1Waiting for players to be moved");
                    boolScrambledall = false;
                }
            }
            
            if (!boolScrambledall){
                this.boolRunOnList = true;
                this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            }else{
                this.boolRunOnList = false;
                this.DebugInfoSkill("^4^bTeams are scrambled now! boolScrambledall");
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                if (this.boolLevelStart){
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes){
                        if (this.strScrambleDoneMsg !=""){
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strScrambleDoneMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strScrambleDoneMsg , "all");
                            }
                            if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual){
                                this.ExecuteCommand("procon.protected.send", "admin.yell", strScrambleDoneMsg , "30");
                            }
                        }
                    }
                }
            }
        }
        
        public void OnCommandScrambleNow(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope){
            bool blIsAdmin = false;
            CPrivileges cpAccount = this.GetAccountPrivileges(strSpeaker);
            if (cpAccount != null && cpAccount.PrivilegesFlags > 0) { blIsAdmin = true; }
            
            if(blIsAdmin){
                this.DebugInfoSkill("^1Admin requested a scramble now!");
                
                this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
                if (this.boolLevelStart && this.boolFirstOP){

                    if (this.ynbEnableScrambleNow == enumBoolYesNo.Yes){
                        if (!this.boolscrambleActive){
                    
                            if (this.ynbScrambleMessage == enumBoolYesNo.Yes){
                                    if (this.strScrambleNowMsg != ""){
                                        if (this.boolVirtual)
                                        {
                                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleNowMsg);
                                        }
                                        else
                                        {
                                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleNowMsg , "all");
                                        }
                                        if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual){
                                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleNowMsg , "30");
                                        }
                                    }
                                }
                                
                            int i = 1;
                            this.dicPlayerScore.Clear();
                            this.dicSquadScore.Clear();
                            this.bestSquadTeamID = 0;
                            
                            
                            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache) {
                                double value = this.dicPlayerCache[kvp.Key].playerValue;
                                string tag = this.dicPlayerCache[kvp.Key].tag;
                                CPlayerScoreInf newEntry = new CPlayerScoreInf(kvp.Key, this.dicPlayerCache[kvp.Key].teamID, this.dicPlayerCache[kvp.Key].playerSquad, value, false, false, tag);
                                this.dicPlayerScore.Add(i, newEntry);
                                i++;
                            
                            }
                            
                            
                            bool Sortiert;
                            do{
                                Sortiert = true; 
                                for (int j = 1; j < this.dicPlayerScore.Count; j++) {
                                    if (this.dicPlayerScore[j].playerValue < this.dicPlayerScore[j+1].playerValue){
                                        CPlayerScoreInf temp = new CPlayerScoreInf(this.dicPlayerScore[j].playerName, this.dicPlayerScore[j].teamID, this.dicPlayerScore[j].playerSquad, this.dicPlayerScore[j].playerValue, false, false, this.dicPlayerScore[j].tag);
                                        this.dicPlayerScore[j] = this.dicPlayerScore[j+1];
                                        this.dicPlayerScore[j+1] = temp;
                                        Sortiert = false;
                                    }
                                }  
                            } while (!Sortiert);
                            
                            this.boolFirstOP = false;
                            this.boolwaitfordeath = false;
                            this.boolscrambleActive = true;
                            this.boolTeamsScrambled = false;
                            this.intScrambledPlayers = 0;
                            this.teamswitcher.Clear();
                            
                            
                            this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "3", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                            //StartScrambler();
                            
                        }else{
                            this.DebugInfoSkill("^3A scrambing has allready been requested.");
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "A scrambing has allready been requested.");
                            }
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", "A scrambing has allready been requested." , "player", strSpeaker);
                            }
                        }
                        
                    }else{
                        this.DebugInfoSkill("^3This command is deactivated at the moment. Activate in in PRoCon.");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "This command is not activated for your server in PRoCon.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "This command is not activated for your server in PRoCon." , "player", strSpeaker);
                        }
                    }
                    
                }
            }           
        }
        
        public void OnCommandScrambleRound(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope){
            bool blIsAdmin = false;
            CPrivileges cpAccount = this.GetAccountPrivileges(strSpeaker);
            if (cpAccount != null && cpAccount.PrivilegesFlags > 0) { blIsAdmin = true; }
            if(blIsAdmin){
                this.DebugInfoSkill("^1Admin requested a scramble next round!");
                
                if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes){
                    this.boolscramblebyadminroundend = true;
                    if (this.boolLevelStart){
                        if (this.ynbScrambleMessage == enumBoolYesNo.Yes){
                            if (this.strScrambleRoundMsg != ""){
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleRoundMsg);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleRoundMsg , "all");
                                }
                                if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual){
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleRoundMsg , "30");
                                }
                            }
                        }
                    }
                }else{
                    this.DebugInfoSkill("^3This command is deactivated at the moment. Activate it in PRoCon.");
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "This command is not activated for your server in PRoCon.");
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "This command is not activated for your server in PRoCon." , "player", strSpeaker);
                    }
                }
            }           
        }

        public void OnScrambleViaPRoCon()
        {
            this.ynbScrambleRoundViaPRoConConf = enumBoolYesNo.No;
            this.ynbScrambleRoundViaPRoCon = enumBoolYesNo.No;

            if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes)
            {
                if (this.boolLevelStart)
                {
                    this.DebugInfoSkill("^1Admin requested a scramble for next round!");
                    this.boolscramblebyadminroundend = true;
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                    {
                        if (this.strScrambleRoundMsg != "")
                        {
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleRoundMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleRoundMsg, "all");
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n Teams will be scrambled at RoundEnd");
                            }
                            if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleRoundMsg, "30");
                            }
                        }
                    }
                }
                else
                {
                    this.DebugInfoSkill("^3You can only request for a scramble, if a round is running.");
                }
            }
            else 
            {
                this.DebugInfoSkill("You need to activate and setup !scrambleround at '2.5 Skill-Scrambler: Manual Commands'");
            }
        }

        public void LogMove(string MoveMsg)
        {
            if (showMoves == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + MoveMsg);
            }
        }

        public void KeepAllSquads()
        {

            //KEEP ALL SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";
            bool squadexists = false;
            int squadIDnew = 0;

            int intTeamA = 0;
            int intTeamB = 0;

            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }

                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }
            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }


            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                
                if (this.dicPlayerScore[kvp.Key].playerSquad != 0)
                {
                    squadexists = false;
                    foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
                    {
                        if (this.dicSquadScore[kvpsquad.Key].teamID == this.dicPlayerScore[kvp.Key].teamID &&
                                    this.dicSquadScore[kvpsquad.Key].squadID == this.dicPlayerScore[kvp.Key].playerSquad)
                        {
                            this.dicSquadScore[kvpsquad.Key].squadScore = this.dicSquadScore[kvpsquad.Key].squadScore + this.dicPlayerScore[kvp.Key].playerValue;
                            this.dicSquadScore[kvpsquad.Key].squadsize++;
                            squadexists = true;
                            break;
                        }
                    }
                    if (!squadexists)
                    {
                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvp.Key].teamID, this.dicPlayerScore[kvp.Key].playerSquad, 1, this.dicPlayerScore[kvp.Key].playerValue, false);
                        squadIDnew++;
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                    }
                }

            }


            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                this.dicSquadScore[kvpsquad.Key].squadScore = this.dicSquadScore[kvpsquad.Key].squadScore / this.dicSquadScore[kvpsquad.Key].squadsize;
            }
            
            bool Sortiert = true;

            do
            {
                Sortiert = true;
                for (int j = 1; j < this.dicSquadScore.Count; j++)
                {
                    if (this.dicSquadScore[j].squadsize < this.dicSquadScore[j + 1].squadsize)
                    {
                        CSquadScoreInf tempsquad = new CSquadScoreInf(this.dicSquadScore[j].teamID, this.dicSquadScore[j].squadID, this.dicSquadScore[j].squadsize, this.dicSquadScore[j].squadScore, false);
                        this.dicSquadScore[j] = this.dicSquadScore[j + 1];
                        this.dicSquadScore[j + 1] = tempsquad;
                        Sortiert = false;
                    }
                    else if (this.dicSquadScore[j].squadsize == this.dicSquadScore[j + 1].squadsize && this.dicSquadScore[j].squadScore < this.dicSquadScore[j + 1].squadScore) 
                    {
                        CSquadScoreInf tempsquad = new CSquadScoreInf(this.dicSquadScore[j].teamID, this.dicSquadScore[j].squadID, this.dicSquadScore[j].squadsize, this.dicSquadScore[j].squadScore, false);
                        this.dicSquadScore[j] = this.dicSquadScore[j + 1];
                        this.dicSquadScore[j + 1] = tempsquad;
                        Sortiert = false;
                    }
                }

            } while (!Sortiert);

            string DebugSquadSorted = "";
            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                DebugSquadSorted = DebugSquadSorted + "^0" + this.dicSquadScore[kvpsquad.Key].teamID + "." + this.dicSquadScore[kvpsquad.Key].squadID +
                                    ": ^7" + this.dicSquadScore[kvpsquad.Key].squadsize + "^9*^5" + this.dicSquadScore[kvpsquad.Key].squadScore + "^9 --- ";
            }


            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);

            bestSquadTeamID = this.dicSquadScore[1].teamID;
            this.DebugInfoSkill("Squads sorted: " + DebugSquadSorted);

            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();
            double TeamValueA = 0;
            int TeamSizeA = 0;
            double TeamValueB = 0;
            int TeamSizeB = 0;

            int n = 0;

            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                if (n == 0 && kvpsquad.Key == 1)
                    SquadsTeamA.Add(kvpsquad.Key);
                else if (n <= 2)
                    SquadsTeamB.Add(kvpsquad.Key);
                else if (n > 2)
                    SquadsTeamA.Add(kvpsquad.Key);

                n++;

                if (n == 5)
                    n = 1;
            }

            foreach (int squadID in SquadsTeamA)
            {
                TeamValueA = TeamValueA + this.dicSquadScore[squadID].squadScore * this.dicSquadScore[squadID].squadsize;
                TeamSizeA = TeamSizeA + this.dicSquadScore[squadID].squadsize;
            }

            foreach (int squadID in SquadsTeamB)
            {
                TeamValueB = TeamValueB + this.dicSquadScore[squadID].squadScore * this.dicSquadScore[squadID].squadsize;
                TeamSizeB = TeamSizeB + this.dicSquadScore[squadID].squadsize;
            }


            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);

            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int squadIDA in SquadsTeamA)
                {
                    foreach (int squadIDB in SquadsTeamB)
                    {
                        if (adjustvalue > 0 && this.dicSquadScore[squadIDA].squadScore > this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicSquadScore[squadIDA].squadScore < this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }

                    }
                }



                if (!adjusted)
                {
                    SquadsTeamA.Remove(moveIDA);
                    SquadsTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize +
                                this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    SquadsTeamB.Remove(moveIDB);
                    SquadsTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize +
                                this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: ^b^2 1." + moveIDA + " ^9<-->^2 2." + moveIDB);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nadjustment: ^b^2" + AverageDiff);




            int squadscrambledA = 0;
            int squadscrambledB = 0;
            n = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            n = 0;
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].playerSquad == 0)
                {
                    if (n == 0 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 0 && n <= 2 && dicPlayerScore[kvp.Key].teamID != 1)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 2 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    n++;
                    if (n == 5) n = 1;
                }
            }

            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping ALL Squads");
            // KEEP ALL SQUADS END
 
        }

        public void KeepClanMates()
        {
            //KEEP CLAN SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";

            int intTeamA = 0;
            int intTeamB = 0;
            int squadless = 0;
            double squadvalue = 0;
            int squadsize = 0;
            int squadIDnew = 0;
            List<string> KeepSquads = new List<string>();
            List<string> squadTags = new List<string>();

            List<int> PlayerTeamA = new List<int>();
            List<int> PlayerTeamB = new List<int>();
            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();
            List<int> squadplayers = new List<int>();

            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }

                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }


            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }


            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);     



            bool SWITCHsquad = false;
            bool SWITCHplayer = false;

            foreach(KeyValuePair<int, CPlayerScoreInf> kvpCheck in this.dicPlayerScore)
            {
                squadTags.Clear();

                if (this.dicPlayerScore[kvpCheck.Key].playerSquad == 0)
                {
                    squadless++;
                    
                    if (!SWITCHplayer)
                    {
                        SWITCHplayer = true;
                        PlayerTeamA.Add(kvpCheck.Key);
                    }
                    else if (SWITCHplayer)
                    {
                        SWITCHplayer = false;
                        PlayerTeamB.Add(kvpCheck.Key);
                    }

                }
                else
                {
                    squadvalue = this.dicPlayerScore[kvpCheck.Key].playerValue;
                    squadsize = 1;
                    squadplayers = new List<int>();
                    squadplayers.Add(kvpCheck.Key);
                    string CheckSquad = this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad;
                       
                    if (this.dicPlayerScore[kvpCheck.Key].tag != "")
                    {
                        squadTags.Add(this.dicPlayerScore[kvpCheck.Key].tag);
                    }

                    if (KeepSquads.Contains(CheckSquad) == false)
                    {
                        bool deletesquad = true;
                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp2 in this.dicPlayerScore)
                        {
                            if (kvpCheck.Key != kvp2.Key && this.dicPlayerScore[kvpCheck.Key].teamID == this.dicPlayerScore[kvp2.Key].teamID && this.dicPlayerScore[kvpCheck.Key].playerSquad == this.dicPlayerScore[kvp2.Key].playerSquad)
                            {
                                squadvalue = squadvalue + this.dicPlayerScore[kvp2.Key].playerValue;
                                squadsize++;
                                squadplayers.Add(kvp2.Key);
                                if ((  ( ((IList<string>)this.strAClantagWhitelistScrambler).Contains(this.dicPlayerScore[kvp2.Key].tag) && this.dicPlayerScore[kvp2.Key].tag != "" ) || ( ((IList<string>)this.strAClantagWhitelistScrambler).Contains(this.dicPlayerScore[kvpCheck.Key].tag) && this.dicPlayerScore[kvpCheck.Key].tag != "" ) ) && !KeepSquads.Contains(CheckSquad))
                                {
                                    KeepSquads.Add(this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad);
                                    deletesquad = false;
                                    CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvpCheck.Key].teamID, this.dicPlayerScore[kvpCheck.Key].playerSquad, 0, 0, false);
                                    squadIDnew++;
                                    this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                                    DebugInfoSkill(string.Format("WL-Clantag detected, Clantag: ^b^2{0}^n^9 or Clantag: ^b^2{1}^n^9. Best Player: ^2{2}", this.dicPlayerScore[kvp2.Key].tag, this.dicPlayerScore[kvpCheck.Key].tag, this.dicPlayerScore[kvp2.Key].playerName)); 
                                    //BLEIBT BESTEHEN!
                                }
                                else if (squadTags.Contains(this.dicPlayerScore[kvp2.Key].tag) && this.dicPlayerScore[kvp2.Key].tag != "" && !KeepSquads.Contains(CheckSquad))
                                {

                                    KeepSquads.Add(this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad);
                                    deletesquad = false;
                                    CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvpCheck.Key].teamID, this.dicPlayerScore[kvpCheck.Key].playerSquad, 0, 0, false);
                                    squadIDnew++;
                                    this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                                    DebugInfoSkill(string.Format("Clanmates detected, Clantag: ^b^2{0}^n^9. Best Player: ^2{1}", this.dicPlayerScore[kvp2.Key].tag, this.dicPlayerScore[kvpCheck.Key].playerName));
                                    //BLEIBT BESTEHEN!
                                }
                                else if (!KeepSquads.Contains(CheckSquad) && this.dicPlayerScore[kvp2.Key].tag != "")
                                {
                                    squadTags.Add(this.dicPlayerScore[kvp2.Key].tag);     
                                }
                            }
                        }

                        if (deletesquad)
                        {

                            this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                            squadless++;

                            if (!SWITCHplayer)
                            {
                                SWITCHplayer = true;
                                PlayerTeamA.Add(kvpCheck.Key);
                            }
                            else if (SWITCHplayer)
                            {
                                SWITCHplayer = false;
                                PlayerTeamB.Add(kvpCheck.Key);
                            }

                        }
                        else
                        {
                            this.dicSquadScore[squadIDnew].squadScore = squadvalue;
                            this.dicSquadScore[squadIDnew].squadsize = squadsize;
                            
                            if (!SWITCHsquad)
                            {
                                SWITCHsquad = true;
                                PlayerTeamA.AddRange(squadplayers);
                                SquadsTeamA.Add(squadIDnew);
                            }
                            else if (SWITCHsquad)
                            {
                                SWITCHsquad = false;
                                PlayerTeamB.AddRange(squadplayers);
                                SquadsTeamB.Add(squadIDnew);
                            }
                            
                        }
                    }
                    

                }

            }

            int teamsizedifference = PlayerTeamA.Count - PlayerTeamB.Count;

            if (teamsizedifference >= 2)
            {
                DebugInfoSkill("Before. ^bTeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);
                int fromsize = PlayerTeamA.Count - 1;
                for (int j = fromsize; j >= 0; j--)
                {
                    if (this.dicPlayerScore[PlayerTeamA[j]].playerSquad == 0 && (PlayerTeamA.Count - PlayerTeamB.Count) >= 2)
                    {
                        DebugInfoSkill("Player moved to even Teams, 1->2: " + "[" + this.dicPlayerScore[PlayerTeamA[j]].tag +"]^b" + this.dicPlayerScore[PlayerTeamA[j]].playerName);
                        PlayerTeamB.Add(PlayerTeamA[j]);
                        PlayerTeamA.RemoveAt(j);
                    }

                    if ((PlayerTeamA.Count - PlayerTeamB.Count) < 2)
                    {
                        DebugInfoSkill("Done!");
                        break;
                    }
                }

                DebugInfoSkill("^2After. TeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);

            }
            else if (teamsizedifference <= -2)
            {
                DebugInfoSkill("Before. TeamSizeA: " + PlayerTeamA.Count + "^n --- ^bTeamSizeB: " + PlayerTeamB.Count);
                int fromsize = PlayerTeamB.Count - 1;
                for (int j = fromsize; j >= 0; j--)
                {
                    if (this.dicPlayerScore[PlayerTeamB[j]].playerSquad == 0 && (PlayerTeamA.Count - PlayerTeamB.Count) <= -2)
                    {
                        DebugInfoSkill("Player moved to even Teams, 2->1: " + "[" + this.dicPlayerScore[PlayerTeamB[j]].tag + "]^b" + this.dicPlayerScore[PlayerTeamB[j]].playerName);
                        PlayerTeamA.Add(PlayerTeamB[j]);
                        PlayerTeamB.RemoveAt(j);
                    }

                    if ((PlayerTeamA.Count - PlayerTeamB.Count) > -2)
                    {
                        DebugInfoSkill("Done!");
                        break;
                    }
                }

                DebugInfoSkill("^2After. TeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);

            }
            else
            {
                DebugInfoSkill("^2TeamSize A: " + PlayerTeamA.Count + "TeamSizeB: " + PlayerTeamB.Count);
            }



            double TeamValueA = 0;
            int TeamSizeA = PlayerTeamA.Count;
            double TeamValueB = 0;
            int TeamSizeB = PlayerTeamB.Count;

            foreach (int PlayerIDa in PlayerTeamA)
            {
                TeamValueA = TeamValueA + this.dicPlayerScore[PlayerIDa].playerValue;
            }
            foreach (int PlayerIDb in PlayerTeamB)
            {
                TeamValueB = TeamValueB + this.dicPlayerScore[PlayerIDb].playerValue;
            }

            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);


            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int playerIDa in PlayerTeamA)
                {
                    foreach (int playerIDb in PlayerTeamB)
                    {
                        if (adjustvalue > 0 && this.dicPlayerScore[playerIDa].playerValue > this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicPlayerScore[playerIDa].playerValue < this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }

                    }
                }

                if (!adjusted)
                {
                    PlayerTeamA.Remove(moveIDA);
                    PlayerTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicPlayerScore[moveIDA].playerValue + this.dicPlayerScore[moveIDB].playerValue;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    PlayerTeamB.Remove(moveIDB);
                    PlayerTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicPlayerScore[moveIDB].playerValue + this.dicPlayerScore[moveIDA].playerValue;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: " + "^0[" + this.dicPlayerScore[moveIDA].tag + "]^b" + this.dicPlayerScore[moveIDA].playerName + "^n^9/^2" + this.dicPlayerScore[moveIDA].playerValue + " ^9 <--> " + "^0[" + this.dicPlayerScore[moveIDB].tag + "]^b" + this.dicPlayerScore[moveIDB].playerName + "n^9/^2" + this.dicPlayerScore[moveIDB].playerValue);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n PLAYER adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nPLAYER adjustment: ^b^2" + AverageDiff);


            bool Sortiert;
            do
            {
                Sortiert = true;
                for (int j = 0; j < (PlayerTeamA.Count -1); j++)
                {
                    if (this.dicPlayerScore[PlayerTeamA[j]].playerValue < this.dicPlayerScore[PlayerTeamA[j+1]].playerValue)
                    {
                        int temp = PlayerTeamA[j];
                        PlayerTeamA[j] = PlayerTeamA[j + 1];
                        PlayerTeamA[j + 1] = temp;
                        Sortiert = false;
                    }
                }
            } while (!Sortiert);

            do
            {
                Sortiert = true;
                for (int j = 0; j < (PlayerTeamB.Count - 1); j++)
                {
                    if (this.dicPlayerScore[PlayerTeamB[j]].playerValue < this.dicPlayerScore[PlayerTeamB[j + 1]].playerValue)
                    {
                        int temp = PlayerTeamB[j];
                        PlayerTeamB[j] = PlayerTeamB[j + 1];
                        PlayerTeamB[j + 1] = temp;
                        Sortiert = false;
                    }
                }
            } while (!Sortiert);


            int count1 = 1;
            foreach (int playerIDa in PlayerTeamA)
            {
                if (this.dicPlayerScore[playerIDa].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDa].teamID, this.dicPlayerScore[playerIDa].playerSquad, 1, this.dicPlayerScore[playerIDa].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamA.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDa].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }


            count1 = 1;
            foreach (int playerIDb in PlayerTeamB)
            {
                if (this.dicPlayerScore[playerIDb].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDb].teamID, this.dicPlayerScore[playerIDb].playerSquad, 1, this.dicPlayerScore[playerIDb].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamB.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDb].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }



            adjusted = false;
            bool squadadjust = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int squadIDA in SquadsTeamA)
                {
                    foreach (int squadIDB in SquadsTeamB)
                    {
                        if (adjustvalue > 0 && this.dicSquadScore[squadIDA].squadScore > this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicSquadScore[squadIDA].squadScore < this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }

                    }
                }



                if (!adjusted)
                {
                    squadadjust = true;
                    SquadsTeamA.Remove(moveIDA);
                    SquadsTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize +
                                this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    SquadsTeamB.Remove(moveIDB);
                    SquadsTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize +
                                this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("SQUAD Adjustment: ^b^2 1." + moveIDA + " ^9<-->^2 2." + moveIDB);
                }

            } while (!adjusted);

            if (squadadjust)
            {
                this.DebugInfoSkill("SortValue ^bafter^n SQUAD adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                    "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
                this.DebugInfoSkill("Average Difference ^bafter ^nSQUAD adjustment: ^b^2" + AverageDiff);
            }



            int squadscrambledA = 0;
            int squadscrambledB = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }
            
            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping only CLAN-Squads");
            
            // KEEP CLAN SQUADS END
 

        }

        public void KeepNoSquads()
        {
            //KEEP NO SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";

            int intTeamA = 0;
            int intTeamB = 0;
            int squadIDnew = 0;
            List<string> KeepSquads = new List<string>();
            List<string> squadTags = new List<string>();

            List<int> PlayerTeamA = new List<int>();
            List<int> PlayerTeamB = new List<int>();
            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();


            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }


                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }


            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }

            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);  




            bool SWITCHplayer = false;

            foreach (KeyValuePair<int, CPlayerScoreInf> kvpCheck in this.dicPlayerScore)
            {
                if (!SWITCHplayer)
                {
                    SWITCHplayer = true;
                    this.dicPlayerScore[kvpCheck.Key].teamID = 100;
                    this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                    PlayerTeamA.Add(kvpCheck.Key);
                }
                else if (SWITCHplayer)
                {
                    SWITCHplayer = false;
                    this.dicPlayerScore[kvpCheck.Key].teamID = 200;
                    this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                    PlayerTeamB.Add(kvpCheck.Key);
                }
                
            }

            int teamsizedifference = PlayerTeamA.Count - PlayerTeamB.Count;

            double TeamValueA = 0;
            int TeamSizeA = PlayerTeamA.Count;
            double TeamValueB = 0;
            int TeamSizeB = PlayerTeamB.Count;

            foreach (int PlayerIDa in PlayerTeamA)
            {
                TeamValueA = TeamValueA + this.dicPlayerScore[PlayerIDa].playerValue;
            }
            foreach (int PlayerIDb in PlayerTeamB)
            {
                TeamValueB = TeamValueB + this.dicPlayerScore[PlayerIDb].playerValue;
            }

            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);


            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int playerIDa in PlayerTeamA)
                {
                    foreach (int playerIDb in PlayerTeamB)
                    {
                        if (adjustvalue > 0 && this.dicPlayerScore[playerIDa].playerValue > this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicPlayerScore[playerIDa].playerValue < this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }

                    }
                }

                if (!adjusted)
                {
                    PlayerTeamA.Remove(moveIDA);
                    PlayerTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicPlayerScore[moveIDA].playerValue + this.dicPlayerScore[moveIDB].playerValue;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    PlayerTeamB.Remove(moveIDB);
                    PlayerTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicPlayerScore[moveIDB].playerValue + this.dicPlayerScore[moveIDA].playerValue;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: ^b^2" + this.dicPlayerScore[moveIDA].playerName + "/" + this.dicPlayerScore[moveIDA].playerValue + " ^9 <--> ^2" + this.dicPlayerScore[moveIDB].playerName + "/" + this.dicPlayerScore[moveIDB].playerValue);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nadjustment: ^b^2" + AverageDiff);

            int count1 = 1;
            foreach (int playerIDa in PlayerTeamA)
            {
                if (this.dicPlayerScore[playerIDa].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDa].teamID, this.dicPlayerScore[playerIDa].playerSquad, 1, this.dicPlayerScore[playerIDa].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamA.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDa].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }


            count1 = 1;
            foreach (int playerIDb in PlayerTeamB)
            {
                if (this.dicPlayerScore[playerIDb].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDb].teamID, this.dicPlayerScore[playerIDb].playerSquad, 1, this.dicPlayerScore[playerIDb].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamB.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDb].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }
     

            int squadscrambledA = 0;
            int squadscrambledB = 0;
            int n = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }


            n = 0;
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].playerSquad == 0)
                {
                    if (n == 0 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 0 && n <= 2 && dicPlayerScore[kvp.Key].teamID != 1)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 2 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    n++;
                    if (n == 5) n = 1;
                }
            }

            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping NO Squads");

            // KEEP NO SQUADS END
        }

        public double TBValue(double TBrank, double TBskill, double TBspm, double TBspmcombat, double TBkdr) 
        {
            double _TBValue = TBrank * 5 + TBskill * 4 + TBspm + TBspmcombat * 8 + TBkdr * 500;

            return _TBValue;
        }


        #endregion
        
        #region Guard

        public void DebugInfoGuard(string DebugMessage){
            if (ynbDebugModeGuard == enumBoolYesNo.Yes){
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
            }
        }

        public List<string> GetSoldierNames()
        {
            List<string> soldierNames = new List<string>();

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                soldierNames.Add(kvp.Key);
            }

            return soldierNames;
        }

        public void OnCommandTBForceMove(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (capCommand.MatchedArguments.Count == 0) {
                this.DebugInfoGuard("^4Malformed tb-fmove command: " + strText);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "Malformed command: " + strText);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Malformed command: " + strText, "player", strSpeaker);
                }
                return;
            }

            string player = capCommand.MatchedArguments[0].Argument;
            this.DebugInfoGuard("^4Force Moving Player: ^b" + player);

            this.DTForceMove = DateTime.Now;

            if (this.OnCommandMove.ContainsKey(player) == false)
            {
                this.OnCommandMove.Add(player, true);
            }
            else 
            {
                this.OnCommandMove[player] = true;
            }

            int team = 0;

            if (this.dicPlayerCache[player].teamID == 1)
                team = 2;
            else
                team = 1;

            if (this.boolVirtual)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + player + " " + team.ToString() + " 0 false");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", player, team.ToString(), "0", "true");
            }
            this.DebugInfoGuard("^4Trying to force player to the other side, due to TB-ForceMoveCommand: ^b" + player);

        }

        public void OnCommandTBMove(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (capCommand.MatchedArguments.Count == 0) {
                this.DebugInfoGuard("^4Malformed tb-move command: " + strText);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "Malformed command: " + strText);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Malformed command: " + strText, "player", strSpeaker);
                }
                return;
            }
            string player = capCommand.MatchedArguments[0].Argument;
            this.DebugInfoGuard("^4Move Player when dead: ^b" + player);
            if (this.boolVirtual)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + player + " - " + "You will be moved to the other team upon death.");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", "You will be moved to the other team upon death.", "player", player);
            }
            if (this.OnCommandMoveDone.Contains(player))
            {
                this.OnCommandMoveDone.Remove(player);
            }

            if (this.OnCommandMove.ContainsKey(player) == false)
            {
                this.OnCommandMove.Add(player, false);
            }
            else
            {
                this.OnCommandMove[player] = false;
            }
        }
        
        
        #endregion
        
        #region Battlelog

        public class BattlelogClient
        {

            private TrueBalancer plugin = null;

            public BattlelogClient(TrueBalancer plugin)
            {
                this.plugin = plugin;
            }



            //private HttpWebRequest req = null;

            WebClient client = null;



            private void fetchWebPage(ref String html_data, String url)
            {
                try
                {
                
                    // Create a request for the URL.        
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    // Set Timeout
                    //plugin.DebugInfoSkill("Default timeout: " + request.Timeout);
                    request.Timeout = 12*1000; // 12 seconds
                    request.ReadWriteTimeout = 2*1000; // 2 seconds
                    request.KeepAlive = false;
                    /*
                    String h = "Headers: ";
                    for (int k = 0; k < request.Headers.Count; k++) {
                        h = h + request.Headers.GetKey(k) + ":" + request.Headers.Get(k) + ";";
                    }
                    plugin.DebugInfoSkill(h);
                    */
                    //plugin.DebugInfoSkill("New timeout: " + request.Timeout);
                    // Get the response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    // Display the status.
                    //plugin.DebugInfoSkill("HTTP Response: " + response.StatusDescription);
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    html_data = reader.ReadToEnd();
                    // Cleanup the streams and the response.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                

                /*
                    if (client == null)
                        client = new WebClient();

                    html_data = client.DownloadString(url);
                    //return html_data;
                */

                }
                catch (WebException e)
                {
                    if (e.Status.Equals(WebExceptionStatus.Timeout))
                        throw new Exception("HTTP request timed-out");
                    else
                        throw;

                }
            }

            public enum ServerType { BF3, BF4 };
            
            public class StatsException : Exception
            {
                public StatsException(String message)
                    : base(message)
                {
                }
            }
            
            public PlayerStats getPlayerStats(String player, ServerType st)
            {
                try
                {
                    /* First fetch the player's main page to get the persona id */
                    String result = "";

                    if (st == ServerType.BF3)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf3/user/" + player);
                    }
                    if (st == ServerType.BF4)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/user/" + player);
                    }
                    

                    /* Extract the persona id */
                    MatchCollection pid = Regex.Matches(result, @"/soldier/" + player + @"/stats/(\d+)(/\w*)?/", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    String personaId = "";

                    foreach(Match m in pid)
                    {
                        if (m.Success && m.Groups[2].Value.Trim() != "/ps3" && m.Groups[2].Value.Trim() != "/xbox" && m.Groups[2].Value.Trim() != "/xbox360" && m.Groups[2].Value.Trim() != "/xboxone" && m.Groups[2].Value.Trim() != "/ps4")
                        {
                            personaId = m.Groups[1].Value.Trim();
                        }
                    }

                    if (personaId == "")
                        throw new Exception("could not find persona-id for ^b" + player);

                    PlayerStats ps = new PlayerStats();                 
                    ps.tag = extractClanTag(result, player);
                    if (st == ServerType.BF3)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf3/overviewPopulateStats/" + personaId + "/bf3-us-engineer/1/");
                    }
                    if (st == ServerType.BF4)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/warsawoverviewpopulate/" + personaId + "/1/");
                    }

                    Hashtable json = (Hashtable)JSON.JsonDecode(result);
                    
                    // check we got a valid response
                    if (!(json.ContainsKey("type") && json.ContainsKey("message")))
                        throw new Exception("JSON response does not contain \"type\" or \"message\" fields");

                    String type = (String)json["type"];
                    String message = (String)json["message"];

                    /* verify we got a success message */
                    if (!(type.StartsWith("success") && message.StartsWith("OK")))
                        throw new Exception("JSON response was type=" + type + ", message=" + message);


                    /* verify there is data structure */
                    Hashtable data = null;
                    if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                        throw new Exception("JSON response was does not contain a data field");
                    
                    Hashtable stats = null;
                    if (!data.ContainsKey("overviewStats") || (stats = (Hashtable)data["overviewStats"]) == null)
                        throw new StatsException("^1^bERROR^0^n: JSON response ^bdata^n does not contain ^boverviewStats^n");
                    
                    
                    // get the data fields

                    if (st == ServerType.BF3)
                    {
                        if (stats.ContainsKey("rank"))
                            Double.TryParse(stats["rank"].ToString(), out ps.rank);


                        if (stats.ContainsKey("elo"))
                            Double.TryParse(stats["elo"].ToString(), out ps.skill);

                        double combatScore = 0;
                        double score = 0;
                        double timePlayed = 0;
                        double kills = 0;
                        double deaths = 0;


                        if (stats.ContainsKey("combatScore"))
                            Double.TryParse(stats["combatScore"].ToString(), out combatScore);

                        if (stats.ContainsKey("score"))
                            Double.TryParse(stats["score"].ToString(), out score);

                        if (stats.ContainsKey("timePlayed"))
                            Double.TryParse(stats["timePlayed"].ToString(), out timePlayed);

                        if (stats.ContainsKey("kills"))
                            Double.TryParse(stats["kills"].ToString(), out kills);

                        if (stats.ContainsKey("deaths"))
                            Double.TryParse(stats["deaths"].ToString(), out deaths);

                        if (combatScore != 0 && timePlayed != 0)
                            ps.spmcombat = Math.Round(combatScore / (timePlayed / 60), 2);
                        else
                            ps.spmcombat = 0;

                        if (score != 0 && timePlayed != 0)
                            ps.spm = Math.Round(score / (timePlayed / 60), 2);
                        else
                            ps.spm = 0;

                        if (kills != 0 && deaths != 0)
                            ps.kdr = Math.Round(kills / deaths, 2);
                        else
                            ps.kdr = 0;
                    }
                    if (st == ServerType.BF4)
                    {
                        if (stats.ContainsKey("rank"))
                            Double.TryParse(stats["rank"].ToString(), out ps.rank);


                        if (stats.ContainsKey("skill"))
                            Double.TryParse(stats["skill"].ToString(), out ps.skill);

                        double combatScore = 0;
                        double score = 0;
                        double timePlayed = 0;
                        double kills = 0;
                        double deaths = 0;


                        if (stats.ContainsKey("combatScore"))
                            Double.TryParse(stats["combatScore"].ToString(), out combatScore);

                        if (stats.ContainsKey("score"))
                            Double.TryParse(stats["score"].ToString(), out score);

                        if (stats.ContainsKey("timePlayed"))
                            Double.TryParse(stats["timePlayed"].ToString(), out timePlayed);

                        if (stats.ContainsKey("kills"))
                            Double.TryParse(stats["kills"].ToString(), out kills);

                        if (stats.ContainsKey("deaths"))
                            Double.TryParse(stats["deaths"].ToString(), out deaths);

                        if (combatScore != 0 && timePlayed != 0)
                            ps.spmcombat = Math.Round(combatScore / (timePlayed / 60), 2);
                        else
                            ps.spmcombat = 0;

                        if (score != 0 && timePlayed != 0)
                            ps.spm = Math.Round(score / (timePlayed / 60), 2);
                        else
                            ps.spm = 0;

                        if (kills != 0 && deaths != 0)
                            ps.kdr = Math.Round(kills / deaths, 2);
                        else
                            ps.kdr = 0;
                    }
                    //ps.statsFetched = true;
                    return ps;
                }
                catch (StatsException e)
                {
                    this.plugin.DebugInfoSkill("^8 StatsException (^b" + player + "^n): " + e.Message);
                }
                catch (Exception e)
                {
                    this.plugin.DebugInfoSkill("^8 Exception (^b" + player + "^n): " + e.Message);
                }
                

                return new PlayerStats();
            }

            public String extractClanTag(String result, String player)
            {
                /* Extract the player tag */
                Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + player, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (tag.Success)
                    return tag.Groups[1].Value;
                return String.Empty;
            }


        }

        public class PlayerStats
        {
            public double rank;
            public double skill;
            public double spm;
            public double spmcombat;
            public double kdr;
            public string tag = String.Empty;
            public bool statsFetched = false;

            public void reset()
            {
                rank = 0;
                skill = 0;
                spm = 0;
                spmcombat = 0;
                kdr = 0;
                tag = String.Empty;
                
            }

        }
        
        
        #endregion
    
    }
    
    
    #region Classes

    
    
        class CPlayerJoinInf
        {
            private int _teamID = 0;
            private int _playerWL = 0;
            private int _playerSquad = 0;
            private DateTime _Playerjoined;
            private int _score = 0;
            private bool _tobebalanced = false;
            private double _rank = 0;
            private double _skill = 0;
            private double _spm = 0;
            private double _spmcombat = 0;
            private double _kdr = 0;
            private double _TBvalue = 0;
            private double _playerValue = 0;
            private string _tag = "";

            public bool statsFetched = false;

            private bool _IsCommander = false;
            private bool _IsSpectator = false;
            
            public int teamID {
                get { return _teamID; }
                set { _teamID = value; }
            }

            public int playerWL
            {
                get { return _playerWL; }
                set { _playerWL = value; }
            }
            
            public int playerSquad
            {
                get { return _playerSquad; }
                set { _playerSquad = value; }
            }
            
            public DateTime Playerjoined {
            get { return _Playerjoined; }
            set { _Playerjoined = value; }
            }
            
            public int score {
            get { return _score; }
            set { _score = value; }
            }
            
            public bool tobebalanced {
            get { return _tobebalanced; }
            set { _tobebalanced = value; }
            }
            
            public double rank {
                get { return _rank; }
                set { _rank = value; }
            }

            public double TBvalue
            {
                get { return _TBvalue; }
                set { _TBvalue = value; }
            }
            
            public double skill {
                get { return _skill; }
                set { _skill= value; }
            }
            
            public double spm {
                get { return _spm; }
                set { _spm = value; }
            }
            
            public double spmcombat {
                get { return _spmcombat; }
                set { _spmcombat = value; }
            }
            
            public double kdr {
                get { return _kdr; }
                set { _kdr = value; }
            }
            
            public double playerValue {
                get { return _playerValue; }
                set { _playerValue = value; }
            }
            
            public string tag {
                get { return _tag; }
                set { _tag = value; }
            }

            public bool IsCommander
            {
                get { return _IsCommander; }
                set { _IsCommander = value; }
            }

            public bool IsSpectator
            {
                get { return _IsSpectator; }
                set { _IsSpectator = value; }
            }

            public CPlayerJoinInf(int teamID, int playerWL, int playerSquad, DateTime Playerjoined, int score, double rank, double skill, double spm, double spmcombat, double kdr, double TBvalue, double playerValue, string tag, bool tobebalanced, bool commander, bool spectator)
            {
                _TBvalue = TBvalue;
                _teamID = teamID;
                _playerWL = playerWL;
                _playerSquad = playerSquad;
                _Playerjoined = Playerjoined;
                _score = score;
                _rank = rank;
                _skill = skill;
                _spm = spm;
                _spmcombat = spmcombat;
                _kdr = kdr;
                _tag = tag;
                _tobebalanced = tobebalanced;
                _playerValue = playerValue;
                _IsCommander = commander;
                _IsSpectator = spectator;
            }
        }

        class CPlayerScoreInf
        {
            private string _playerName = "";
            private int _teamID = 0;
            private int _playerSquad = 0;
            private double _playerValue = 0;
            private bool _balanced = false;
            private bool _scrambled = false;
            private string _tag = String.Empty;

            public string tag
            {
                get { return _tag; }
                set { _tag = value; }
            }

            public int teamID {
            get { return _teamID; }
            set { _teamID = value; }
            }
            
            public int playerSquad
            {
                get { return _playerSquad; }
                set { _playerSquad = value; }
            }
            
            public double playerValue
            {
                get { return _playerValue; }
                set { _playerValue= value; }
            }
            public string playerName
            {
                get { return _playerName; }
                set { _playerName = value; }
            }
            
            public bool balanced
            {
                get { return _balanced; }
                set { _balanced = value; }
            }
            public bool scrambled
            {
                get { return _scrambled; }
                set { _scrambled = value; }
            }
            
            public CPlayerScoreInf(string playerName, int teamID, int playerSquad, double playerValue, bool balanced, bool scrambled, string tag)
            {
                _tag = tag;
                _playerName = playerName;
                _teamID = teamID;
                _playerSquad = playerSquad;
                _playerValue = playerValue;
                _balanced = balanced;
                _scrambled = scrambled;

            }
        }       
        
        class CSquadScoreInf
        {
            private int _teamID = 0;
            private int _squadID = 0;
            private double _squadScore = 0;
            private int _squadsize = 0;
            private bool _assigned = false;

            public bool assigned {
            get { return _assigned; }
            set { _assigned = value; }
            }

            public int teamID {
            get { return _teamID; }
            set { _teamID = value; }
            }

            public int squadsize
            {
                get { return _squadsize; }
                set { _squadsize = value; }
            }

            public int squadID
            {
                get { return _squadID; }
                set { _squadID = value; }
            }
            
            public double squadScore
            {
                get { return _squadScore; }
                set { _squadScore = value; }
            }

            public CSquadScoreInf(int teamID, int squadID, int squadsize, double squadScore, bool assigned)
            {
                _assigned = assigned;
                _teamID = teamID;
                _squadID = squadID;
                _squadScore = squadScore;
                _squadsize = squadsize;
                
            }
        }
    
    #endregion

}