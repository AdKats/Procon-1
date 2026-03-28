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
    public class CAdminIn_and_SpawnMsg : PRoConPluginAPI, IPRoConPluginInterface {

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private enumBoolYesNo m_enDoAdminAnnounce;
        private enumBoolYesNo m_enDoAdminAnnounceName;
        private enumBoolYesNo m_enDoAnnounceYell;
        private enumBoolYesNo m_enDoAnnounceOnChat;
        private enumBoolYesNo m_enDoCheckAdminPrivs;
        //
        private enumBoolYesNo m_enDoVipAnnounce;
        private enumBoolYesNo m_enDoVipAnnounceName;
        private enumBoolYesNo m_enDoVipAnnounceYell;
        private enumBoolYesNo m_enDoVipAnnounceOnChat;
        private enumBoolYesNo m_enDoCheckVipList;
        //
        private enumBoolYesNo m_enDoBanAnnounce;
        private enumBoolYesNo m_enDoBanOnlyName;
        private enumBoolYesNo m_enDoBanBC2;
        private enumBoolYesNo m_enDoBanPB;
        private enumBoolYesNo m_enDoBanYell;
        private enumBoolYesNo m_enDoBanOnChat;
        private enumBoolYesNo m_enDoKickAnnounce;
        private enumBoolYesNo m_enDoKickYell;
        private enumBoolYesNo m_enDoKickOnChat;
        private enumBoolYesNo m_enDoKickPB;
        private enumBoolYesNo m_enDoSpawnMessage;
        private enumBoolYesNo m_enDoSpawnYell;
        private enumBoolYesNo m_enDoSpawnInterval;
        private enumBoolYesNo m_enDoSpawnFirst;
        private enumBoolYesNo m_enDoSpawnOnChat;
        private enumBoolYesNo m_enDoUptimeDisplay;
        //private enumBoolYesNo m_enDoUptimeYell;
        private enumBoolYesNo m_enDoRoundTimeDisplay;
        private enumBoolYesNo m_enDoRoundTimeYell;
        private enumBoolYesNo m_enDoTimesOneLine;
        private enumBoolYesNo m_enDoTimesInGame;
        private enumBoolYesNo m_enDoTimesChatTab;
        private enumBoolYesNo m_enDoPlayerSpot;
        private enumBoolYesNo m_enDoPlayerSpotExtend;
        private enumBoolYesNo m_enDoPlayerChatTab;
        private enumBoolYesNo m_enPluginOnLayer;
        private enumBoolYesNo m_enDoConsoleOutput;
        private enumBoolYesNo m_enDoDebugOutput;
        private enumBoolYesNo m_enDoQuietMode;

        private string m_strChatTabPrefix;
        private string m_strServerType;
        private string[] m_astrTimeDescription;

        private string m_strAdminAnnounceMsg;
        private string m_strAdminAnnounceNameMsg;
        private int m_iDisplayTimeAdmin;
        private List<string> m_lstAdminList;

        private string m_strVipAnnounceMsg;
        private string m_strVipAnnounceNameMsg;
        private int m_iDisplayTimeVip;
        private List<string> m_lstVipList;
        private List<string> m_lstSrvVipList;

        private CBanInfo m_bannedPlayer;
        private bool m_bCheckBan;
        private string m_strBanAnnounceMsg;
        private int m_iDisplayTimeBan;
        
        private bool m_bCheckKick;
        private string m_strKickAnnounceMsg;
        private string m_strVIPKickMsg;
        private int m_iDisplayTimeKick;

        private string m_strOnSpawnMsg;
        private int m_iDisplayTimeSpawn;
        private int m_iSpawnMsgInterval;
        private int m_iSpawnStart;

        private string m_strPlayerSpotMsg;

        private int m_iUptimeDisplayInterval;
        private int m_iRoundTimeInterval;
        private int m_iDisplayTimeRound;
        private int m_iSrvInfoUptime;
        private int m_iOldUptime;
        private int m_iSrvInfoRoundTime;
        private int m_iOldRoundTime;

        private int m_iYellDivider;

        private bool m_blIsPreRound;

        private Dictionary<string, bool> m_playerJoined = new Dictionary<string, bool>();
        private Dictionary<string, int> m_playerSpawns = new Dictionary<string, int>();
        private Dictionary<string, string> m_playerBanned = new Dictionary<string, string>();
        private Dictionary<string, string> m_playerKicked = new Dictionary<string, string>();

        private bool m_isPluginEnabled;

        public CAdminIn_and_SpawnMsg() {

            this.m_enPluginOnLayer          = enumBoolYesNo.Yes;
            this.m_enDoAdminAnnounce 	    = enumBoolYesNo.No;
            this.m_enDoAdminAnnounceName 	= enumBoolYesNo.No;
            this.m_enDoAnnounceYell		    = enumBoolYesNo.No;
            this.m_enDoCheckAdminPrivs      = enumBoolYesNo.No;
            this.m_enDoAnnounceOnChat       = enumBoolYesNo.Yes;

            this.m_enDoVipAnnounce          = enumBoolYesNo.No;
            this.m_enDoVipAnnounceName      = enumBoolYesNo.No;
            this.m_enDoVipAnnounceYell      = enumBoolYesNo.No;
            this.m_enDoCheckVipList         = enumBoolYesNo.No;
            this.m_enDoVipAnnounceOnChat    = enumBoolYesNo.Yes;

            this.m_enDoBanAnnounce = enumBoolYesNo.No;
            this.m_enDoBanOnlyName          = enumBoolYesNo.No;
            this.m_enDoBanBC2               = enumBoolYesNo.Yes;
            this.m_enDoBanPB                = enumBoolYesNo.Yes;
            this.m_enDoBanYell              = enumBoolYesNo.No;
            this.m_enDoBanOnChat            = enumBoolYesNo.Yes;
            this.m_enDoKickAnnounce         = enumBoolYesNo.No;
            this.m_enDoKickYell             = enumBoolYesNo.No;
            this.m_enDoKickOnChat           = enumBoolYesNo.Yes;
            this.m_enDoKickPB               = enumBoolYesNo.Yes;
            this.m_enDoSpawnMessage         = enumBoolYesNo.No;
            this.m_enDoSpawnYell		    = enumBoolYesNo.No;
            this.m_enDoSpawnInterval        = enumBoolYesNo.No;
            this.m_enDoSpawnFirst           = enumBoolYesNo.No;
            this.m_enDoSpawnOnChat          = enumBoolYesNo.Yes;

            this.m_enDoUptimeDisplay        = enumBoolYesNo.No;
            this.m_enDoRoundTimeDisplay     = enumBoolYesNo.No;
            this.m_enDoRoundTimeYell        = enumBoolYesNo.No;
            this.m_enDoTimesOneLine         = enumBoolYesNo.No; 
            this.m_enDoTimesInGame          = enumBoolYesNo.Yes;
            this.m_enDoTimesChatTab         = enumBoolYesNo.Yes;

            this.m_enDoPlayerSpot           = enumBoolYesNo.No;
            this.m_enDoPlayerSpotExtend     = enumBoolYesNo.No; 
            this.m_enDoPlayerChatTab        = enumBoolYesNo.No;


            this.m_enDoConsoleOutput 	    = enumBoolYesNo.No;
            this.m_enDoDebugOutput 		    = enumBoolYesNo.No;
            this.m_enDoQuietMode            = enumBoolYesNo.No;

            this.m_strChatTabPrefix         = "AdminIn_and_SpawnMsg";
            this.m_strServerType            = "none";
            this.m_astrTimeDescription      = new string[] { "y ", "y ", "M ", "M ", "w ", "w ", "d ", "d ", "h ", "h ", "m ", "m ", "s ", "s " };

            this.m_strAdminAnnounceMsg 	    = "Admin joined the game!";
            this.m_strAdminAnnounceNameMsg 	= "Admin (%name%) joined the game!";
            this.m_lstAdminList             = new List<string>();
            this.m_iDisplayTimeAdmin        = 4000;

            this.m_strVipAnnounceMsg        = "One of our VIPs joined the game!";
            this.m_strVipAnnounceNameMsg    = "VIP (%name%) joined the game!";
            this.m_lstVipList               = new List<string>();
            this.m_iDisplayTimeVip          = 4000;

            this.m_bCheckBan = false;
            this.m_strBanAnnounceMsg        = "{0} was banned for: {1}.";
            this.m_iDisplayTimeBan          = 4000;

            this.m_bCheckKick               = false;
            this.m_strKickAnnounceMsg       = "{0} was kicked for: {1}.";
            this.m_strVIPKickMsg            = "Random slot was needed for VIP user joining.";
            this.m_iDisplayTimeKick         = 4000;

            this.m_strOnSpawnMsg		    = "Stand up and fight!";
            this.m_iDisplayTimeSpawn        = 4000;
            this.m_iSpawnMsgInterval        = 2;
            this.m_iSpawnStart              = 0;

            this.m_strPlayerSpotMsg         = "Another player ({0}) is joining...";

            this.m_iUptimeDisplayInterval   = 5;
            this.m_iRoundTimeInterval       = 5;
            this.m_iDisplayTimeRound        = 4000;
            
            this.m_iSrvInfoUptime           = 0;
            this.m_iOldUptime               = 0;
            this.m_iSrvInfoRoundTime        = 0;
            this.m_iOldRoundTime            = 0;

            this.m_iYellDivider             = 1;

            this.m_blIsPreRound             = true;

            this.m_isPluginEnabled          = false;
        }

        public string GetPluginName() {
            return "AdminIn & Spawn Message";
        }

        public string GetPluginVersion() {
            return "1.3.6.0";
        }

        public string GetPluginAuthor() {
            return "Phil_K";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net/forumvb/showthread.php?1637-Admin-Announcer-amp-onSpawn-Message";
        }

        public string GetPluginDescription() {
            return @"<p>If you find my plugins useful, please feel free to donate</p>
<blockquote>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""encrypted"" value=""-----BEGIN PKCS7-----MIIHbwYJKoZIhvcNAQcEoIIHYDCCB1wCAQExggEwMIIBLAIBADCBlDCBjjELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAkNBMRYwFAYDVQQHEw1Nb3VudGFpbiBWaWV3MRQwEgYDVQQKEwtQYXlQYWwgSW5jLjETMBEGA1UECxQKbGl2ZV9jZXJ0czERMA8GA1UEAxQIbGl2ZV9hcGkxHDAaBgkqhkiG9w0BCQEWDXJlQHBheXBhbC5jb20CAQAwDQYJKoZIhvcNAQEBBQAEgYC7Ni/Qumgsg8eY2XLtVbiVIhtyImqr9EjQr9TA+PqAwr3G4hWidwPQIQXksR40lRsSh/yWOhc3s2UEjbrpc8mLejr8M/qvwmMcXR9BjkNi12aow/ZN22KIzX+je695xqnjATH+P+eS/HOj9L7bUHYQiNXcJXgQh7iYdW5m8iSn0jELMAkGBSsOAwIaBQAwgewGCSqGSIb3DQEHATAUBggqhkiG9w0DBwQIZE2o3niiix+AgcgZQy0CszPd4aluc46B2kNh1oz1XNK0cdcgHGKPYaqzCZnlfed89irXTE6EMhkviuGSlcfEw9vhpA9pMY/KBv++eindnbrSkkOsPD7jjR1I7JMUaFf8PS548Od9kNDpdbqvFwNrjAY1xk+FwXIw3GlCGZHRK2AYacQ/gasqdrlJTfQd47NaVPi1nzaTLjViucfUq3IoRS0gUYhVjjOVGrgNzU0esEm0XNCDd+imVAinTkKcYRfgjRB1Vh0mW1AesSffmm2DQ08bS6CCA4cwggODMIIC7KADAgECAgEAMA0GCSqGSIb3DQEBBQUAMIGOMQswCQYDVQQGEwJVUzELMAkGA1UECBMCQ0ExFjAUBgNVBAcTDU1vdW50YWluIFZpZXcxFDASBgNVBAoTC1BheVBhbCBJbmMuMRMwEQYDVQQLFApsaXZlX2NlcnRzMREwDwYDVQQDFAhsaXZlX2FwaTEcMBoGCSqGSIb3DQEJARYNcmVAcGF5cGFsLmNvbTAeFw0wNDAyMTMxMDEzMTVaFw0zNTAyMTMxMDEzMTVaMIGOMQswCQYDVQQGEwJVUzELMAkGA1UECBMCQ0ExFjAUBgNVBAcTDU1vdW50YWluIFZpZXcxFDASBgNVBAoTC1BheVBhbCBJbmMuMRMwEQYDVQQLFApsaXZlX2NlcnRzMREwDwYDVQQDFAhsaXZlX2FwaTEcMBoGCSqGSIb3DQEJARYNcmVAcGF5cGFsLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAwUdO3fxEzEtcnI7ZKZL412XvZPugoni7i7D7prCe0AtaHTc97CYgm7NsAtJyxNLixmhLV8pyIEaiHXWAh8fPKW+R017+EmXrr9EaquPmsVvTywAAE1PMNOKqo2kl4Gxiz9zZqIajOm1fZGWcGS0f5JQ2kBqNbvbg2/Za+GJ/qwUCAwEAAaOB7jCB6zAdBgNVHQ4EFgQUlp98u8ZvF71ZP1LXChvsENZklGswgbsGA1UdIwSBszCBsIAUlp98u8ZvF71ZP1LXChvsENZklGuhgZSkgZEwgY4xCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJDQTEWMBQGA1UEBxMNTW91bnRhaW4gVmlldzEUMBIGA1UEChMLUGF5UGFsIEluYy4xEzARBgNVBAsUCmxpdmVfY2VydHMxETAPBgNVBAMUCGxpdmVfYXBpMRwwGgYJKoZIhvcNAQkBFg1yZUBwYXlwYWwuY29tggEAMAwGA1UdEwQFMAMBAf8wDQYJKoZIhvcNAQEFBQADgYEAgV86VpqAWuXvX6Oro4qJ1tYVIT5DgWpE692Ag422H7yRIr/9j/iKG4Thia/Oflx4TdL+IFJBAyPK9v6zZNZtBgPBynXb048hsP16l2vi0k5Q2JKiPDsEfBhGI+HnxLXEaUWAcVfCsQFvd2A1sxRr67ip5y2wwBelUecP3AjJ+YcxggGaMIIBlgIBATCBlDCBjjELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAkNBMRYwFAYDVQQHEw1Nb3VudGFpbiBWaWV3MRQwEgYDVQQKEwtQYXlQYWwgSW5jLjETMBEGA1UECxQKbGl2ZV9jZXJ0czERMA8GA1UEAxQIbGl2ZV9hcGkxHDAaBgkqhkiG9w0BCQEWDXJlQHBheXBhbC5jb20CAQAwCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTExMTExMjE0MzIyMVowIwYJKoZIhvcNAQkEMRYEFOqcZ/PhgcD6FjPGU6EDmM5q7DFPMA0GCSqGSIb3DQEBAQUABIGAeIbspBNon0tzt+4qSK1XjoiTaRs444sm/uECX3by57T+5BJ1R2nYj5Clm5uQrmmDmz1fNqj3dtLvyiCxAraqvz5Mx7WV8i8/mAAM/qKvopGv6+hOaQa0CDtgleZQVhoN6HWTHkooqbvrIFnf1/xUrjuYx6WcwUkHwpJO3dE6fjU=-----END PKCS7-----"">
<input type=""image"" src=""https://www.paypalobjects.com/de_DE/DE/i/btn/btn_donate_SM.gif"" border=""0"" name=""submit"" alt=""Jetzt einfach, schnell und sicher online bezahlen – mit PayPal."">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>
</blockquote>

<h2>Description</h2>
    <p>Spams an 'admin joined the game' message every time an admin joins the game.
    The message can name the admin or do it anonymously. In addition you can set it to
    spam a message to a player after he spawns in the game. It also can announce bans & kicks to all players.</p>

    <p>You have also the ability to let the plug-in interprete the gameservers uptime and round times send with
    the serverInfo response and displayed in game via chat messages. While one of server uptime or round duration
    is enabled you can choose to have it displayed in-game, on the chat-tab or both. If you only want this information
    on the plug-in console enable 'Console Output'.</p>

<h2>Commands</h2>
    This plug-in has no in-game command.

<h2>Settings</h2>
    <h3>Miscellaneous</h3>
        <blockquote><h4>Announce admin</h4>Enabled or disables the announcement of an admin joining (global).</blockquote>
        <blockquote><h4>Announce admins name</h4>Enabled or disables if the admin should be identified by name.<br/>
                        Use %name% as placeholder in your message.</blockquote>
        <blockquote><h4>Check admin privileges</h4>Enabled or disables to check if a joining player is on the account list and<br/>
                        has the right to do temp. bans. Therefor he is treaded as admin.</blockquote>
        <blockquote><h4>Announce bans</h4>Enabled or disables the announcement of bans to all players.<br/>
                        Will only announce bans of players in the game. Banning offline players will not be send to chat.<br/>
                        Use {0} as placeholder for the playername and {1} for the reason in the message.<br/>
                        Note your message is limited to 30 chars. If your reason and message is above 100 only 100 chars are<br/>
                        send to all players.</blockquote>
        <blockquote><h4>Announce kicks</h4>Enabled or disables the announcement of kicks to all players.<br/>
                        Use {0} as placeholder for the playername and {1} for the reason in the message.<br/>
                        Note your message is limited to 30 chars. If your reason and message is above 100 only 100 chars are<br/>
                        send to all players.</blockquote>
        <blockquote><h4>Only named bans</h4>Enabled or disables the announcement of only bans on player names.<br/>
                        If disabled also EA_GUID and PB_GUID bans will be announce with the players name.</blockquote>
        <blockquote><h4>Enable onSpawn message</h4>Enabled or disables that a message is send to a player every time he spawns</blockquote>
    <h3>Time</h3>
        <blockquote><h4>Show round duration</h4> The plug-in will announce the duration the actual round is running.</blockquote>
        <blockquote><h4>Round Interval (min)</h4> Sets the interval in minutes to show the round duration information.</blockquote>
        <blockquote><h4>Show server uptime</h4> The plug-in will announce the actual server uptime.</blockquote>
        <blockquote><h4>Uptime Interval (min)</h4> Sets the interval in minutes to show the server uptime.</blockquote>
        <blockquote><h4>Show In-Game</h4> Select Yes to have the above information send as chat message to all players.</blockquote>
        <blockquote><h4>Show on Chat-Tab</h4> Select Yes to have the above information displayed on Procons chat tab. This will also show up in an enabled chat log.</blockquote>
    <h3>Spotting</h3>
        <blockquote><h4>Spot players</h4> Will enable chat messages spotting players joining during pre-round mode (BF3, BF4, BFHL, MoHW) to inform waiting players.</blockquote>
        <blockquote><h4>Spot on Chat-Tab</h4> Will show the spotting also on the chat tab if you do not want to enable join/leave messages there.</blockquote>
    <h3>Extra</h3>
        <blockquote><h4>On layer</h4> Select YES if the plug-in runs on a layer server. This prevents it from raising info windows.</blockquote>
        <blockquote><h4>Console output</h4>Enabled or disables monitoring of messages send ingame on plugin console</blockquote>
        <blockquote><h4>Debug output</h4>Enabled or disables some more debuging output.</blockquote>
        <blockquote><h4>Quiet mode</h4>Enable or disable in-game output. If enabled Console output is enabled too.<br/> 
                        Console or Debug output must stay enabled while this is enabled.</blockquote>
<br/><br/>
<h2>Development</h2>
    <h3>Changelog</h3>
        <blockquote><h4>1.3.6.0 (2015-03-31)</h4>
            - added Battlefield Hardline compatibility <br/>
        </blockquote>
        <blockquote><h4>1.3.5.2 (2014-02-10)</h4>
            - added check to not multi announce on quick tries <br/>
        </blockquote>
        <blockquote><h4>1.3.5.1 (2014-02-08)</h4>
            - fixed kick announcement for BF4 <br/>
            - made spotting message editable <br/>
        </blockquote>
        <blockquote><h4>1.3.5.0 (2014-02-05)</h4>
            - added switch for reservedSlotsList vip announcement <br/>
            - added player spoting for BF4<br/>
            -- this announces every player who is not admin or vip in preround or always based on the spotting settings<br/>
            - added BF4 yell compatibility <br/>
        </blockquote>
        <blockquote><h4>1.3.3.0 (2013-02-06)</h4>
            - added compatibility for MoHW <br/>
            -- player spotting now work <br/>
            -- messages can be yelled, except onSpawnMessage that goes to the fireteam <br/>
            - added ability to have joining players announced even after the first kill <br/>
        </blockquote>
        <blockquote><h4>1.3.2.0 (2012-07-09)</h4>
            - added announcing of VIPs based on reservedSlotsList / server based VIP list<br/>
        </blockquote>
        <blockquote><h4>1.3.1.0 (2012-04-02)</h4>
            - added yell to round duration<br/>
            - added announcing of VIPs<br/>
        </blockquote>
        <blockquote><h4>1.3.0.0 (2012-04-02)</h4>
            - fixed BF3 kick/ban detection again<br/>
            - added customizable chat-tab message prefix<br/>
        </blockquote>
        <blockquote><h4>1.2.3.0 (2012-04-01)</h4>
            - enabled all yell functions for BF3 again<br/>
        </blockquote>
        <blockquote><h4>1.2.2.1 (2011-11-30)</h4>
            - added workaround to have kicks announced for BF3<br/>
        </blockquote>
        <blockquote><h4>1.2.2.0 (2011-11-23)</h4>
            - added player spotting in pre-round mode<br/>
        </blockquote>
        <blockquote><h4>1.2.1.1 (2011-11-19)</h4>
            - fixed not to be able to totally disable time display<br/>
        </blockquote>
        <blockquote><h4>1.2.0.2 -> 1.2.1.0 (2011-11-18)</h4>
            - added option to announce server uptime and RoundTime.<br/>
            - fixed constant Game discover output. Now checks debug / console output setting.<br/>
        </blockquote>
        <blockquote><h4>1.2.0.1 -> 1.2.0.2 (2011-11-14)</h4>
            - packages update to include lost bf3 file. sorry.<br/>
        </blockquote>
        <blockquote><h4>1.2.0.0 -> 1.2.0.1 (2011-11-13)</h4>
            - added switch for layer hosted plugin which disables info popups<br/>
        </blockquote>
        <blockquote><h4>1.1.3.2 -> 1.2.0.0 (2011-11-10)</h4>
            - fixed compatibility for BF-3<br/>
        </blockquote>
        <blockquote><h4>1.1.3.1 -> 1.1.3.2 (2011-06-16)</h4>
            - added rewrite for PC_DIALOG_TEXT_VIPKICK string send by the server<br/>
            - added option to define the replacement text<br/>
        </blockquote>
        <blockquote><h4>1.1.3.0 -> 1.1.3.1 (2011-03-04)</h4>
            - bug fix on yell bans<br/>
        </blockquote>
        <blockquote><h4>1.1.2.0 -> 1.1.3.0 (2011-03-03)</h4>
            - added switches for detailed settings to report which type of ban / kick<br/>
        </blockquote>
        <blockquote><h4>1.1.1.0 -> 1.1.2.0 (2011-02-21)</h4>
            - added switches to enable/disable plugin chat messages on chat tab<br/>
        </blockquote>
        <blockquote><h4>1.1.0.0 -> 1.1.1.0 (2011-01-20)</h4>
            - added kick announcement<br/>
            - added changeable messages vor kick and ban announcements<br/>
            - added automated cut of chat message length to 100 chars if entered message combinded with username and reason exceeds this limit<br/></blockquote>
        <blockquote><h4>1.0.0.0 -> 1.1.0.0 (2011-01-18)</h4>
            - added spawn message interval<br/>
            - added output of messages on the chat console for better following<br/>
            - added announcement of bans<br/>
        </blockquote>
";
        }

        public void OnPluginLoadingEnv(List<string> lstPluginEnv) {
            Version PRoConVersion = new Version(lstPluginEnv[0]);
            string strPRoConVersion = PRoConVersion.ToString();
            string strGameType = lstPluginEnv[1].ToLower();
            string strGameMod = lstPluginEnv[2];
            string strServerVersion = lstPluginEnv[3];
            
            //WritePluginConsole("OnPluginLoadingEnv - Procon " + strPRoConVersion + " - GameType " + strGameType + " - GameMod " + strGameMod + " - ServerVersion " + strServerVersion, 2);
            this.m_strServerType = strGameType;

            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Discovered GameType: " + this.m_strServerType);
            }
            if (this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl") {
                this.m_iYellDivider = 1000;
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: set yellDivider for " + this.m_strServerType);
                }
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
	        
            WritePluginConsole("PreCollectedData - GameType " + this.m_strServerType, 2);
            WritePluginConsole("PreCollectedData - HostName " + this.m_strHostName, 2);
            WritePluginConsole("PreCollectedData - HostPort " + this.m_strPort, 2);

            if (string.Compare(this.m_strServerType, "none", true) == 0) {
                this.RegisterEvents(this.GetType().Name, "OnVersion", "OnServerInfo", "OnPlayerJoin", "OnPlayerLeft", "OnPlayerSpawned", "OnPlayerKicked", "OnPlayerKickedByAdmin", "OnBanAdded", "OnPunkbusterBanInfo", "OnPlayerKilled", "OnLevelLoaded", "OnReservedSlotsList");
                this.ExecuteCommand("procon.protected.send", "version");
            } else {
                this.RegisterEvents(this.GetType().Name, "OnServerInfo", "OnPlayerJoin", "OnPlayerLeft", "OnPlayerSpawned", "OnPlayerKicked", "OnPlayerKickedByAdmin", "OnBanAdded", "OnPunkbusterBanInfo", "OnPlayerKilled", "OnLevelLoaded", "OnReservedSlotsList");
            }
        }

        public void OnPluginEnable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdminIn_and_SpawnMsg: ^2Enabled!");
            this.m_isPluginEnabled = true;

            string VipListCmd = "reservedSlotsList.list";

            if (string.Compare(this.m_strServerType, "none", true) == 0) {
                this.ExecuteCommand("procon.protected.send", "version");
            }
            
            if (string.Compare(this.m_strServerType, "bfbc2", true) == 0) {
                VipListCmd = "reservedSlots.list";
            }
            if (string.Compare(this.m_strServerType, "moh", true) == 0) {
                VipListCmd = "reservedSpectateSlots.list";
            }
            this.ExecuteCommand("procon.protected.send", VipListCmd);

        }

        public void OnPluginDisable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bAdminIn_and_SpawnMsg: ^1Disabled =(" );
            this.m_playerJoined.Clear();
            this.m_playerSpawns.Clear();
            this.m_playerKicked.Clear();
            this.m_playerBanned.Clear();
            this.m_lstSrvVipList.Clear();
            this.m_isPluginEnabled = false;
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            if (this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl")
            {
                this.m_iYellDivider = 1000;
            }

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Admin announcement|Announce admin", typeof(enumBoolYesNo), this.m_enDoAdminAnnounce));
            if (this.m_enDoAdminAnnounce == enumBoolYesNo.Yes) {
                lstReturn.Add(new CPluginVariable("Admin announcement|Yell admin announcement", typeof(enumBoolYesNo), this.m_enDoAnnounceYell));
                if (this.m_enDoAnnounceYell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Admin announcement|Show announcement (seconds)", this.m_iDisplayTimeAdmin.GetType(), this.m_iDisplayTimeAdmin / 1000));
                }
                lstReturn.Add(new CPluginVariable("Admin announcement|Announce admin name", typeof(enumBoolYesNo), this.m_enDoAdminAnnounceName));
                if (this.m_enDoAdminAnnounceName == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Admin announcement|Announcement with name", typeof(string), this.m_strAdminAnnounceNameMsg));
                }
                else
                {
                    lstReturn.Add(new CPluginVariable("Admin announcement|Announcement", typeof(string), this.m_strAdminAnnounceMsg));
                }
                lstReturn.Add(new CPluginVariable("Admin announcement|Show announcement on chat tab", typeof(enumBoolYesNo), this.m_enDoAnnounceOnChat));
            }
            lstReturn.Add(new CPluginVariable("Admin announcement|Admin list", typeof(string[]), this.m_lstAdminList.ToArray()));
            lstReturn.Add(new CPluginVariable("Admin announcement|Check admin privileges", typeof(enumBoolYesNo), this.m_enDoCheckAdminPrivs));

            lstReturn.Add(new CPluginVariable("Announcement VIP|Announce vip", typeof(enumBoolYesNo), this.m_enDoVipAnnounce));
            if (this.m_enDoVipAnnounce == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Announcement VIP|Yell vip announcement", typeof(enumBoolYesNo), this.m_enDoVipAnnounceYell));
                if (this.m_enDoVipAnnounceYell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Announcement VIP|Show vip announcement (seconds)", this.m_iDisplayTimeVip.GetType(), this.m_iDisplayTimeAdmin / 1000));
                }
                lstReturn.Add(new CPluginVariable("Announcement VIP|Announce vip name", typeof(enumBoolYesNo), this.m_enDoVipAnnounceName));
                if (this.m_enDoVipAnnounceName == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Announcement VIP|Vip announcement with name", typeof(string), this.m_strVipAnnounceNameMsg));
                }
                else
                {
                    lstReturn.Add(new CPluginVariable("Announcement VIP|Vip announcement", typeof(string), this.m_strVipAnnounceMsg));
                }
                lstReturn.Add(new CPluginVariable("Announcement VIP|Show vip announcement on chat tab", typeof(enumBoolYesNo), this.m_enDoVipAnnounceOnChat));
            }
            lstReturn.Add(new CPluginVariable("Announcement VIP|Vip list", typeof(string[]), this.m_lstVipList.ToArray()));
            lstReturn.Add(new CPluginVariable("Announcement VIP|Check server vip list", typeof(enumBoolYesNo), this.m_enDoCheckVipList));

            lstReturn.Add(new CPluginVariable("Ban announcement|Announce bans", typeof(enumBoolYesNo), this.m_enDoBanAnnounce));
            if (this.m_enDoBanAnnounce == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Ban announcement|Named bans only", typeof(enumBoolYesNo), this.m_enDoBanOnlyName));
                if (this.m_enDoBanOnlyName == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("Ban announcement|Show BC2-bans", typeof(enumBoolYesNo), this.m_enDoBanBC2));
                    lstReturn.Add(new CPluginVariable("Ban announcement|Show PB-bans", typeof(enumBoolYesNo), this.m_enDoBanPB));
                }
                lstReturn.Add(new CPluginVariable("Ban announcement|Yell bans", typeof(enumBoolYesNo), this.m_enDoBanYell));
                lstReturn.Add(new CPluginVariable("Ban announcement|Show bans (seconds)", this.m_iDisplayTimeBan.GetType(), this.m_iDisplayTimeBan / 1000));
                lstReturn.Add(new CPluginVariable("Ban announcement|Msg on bans", typeof(string), this.m_strBanAnnounceMsg));
                lstReturn.Add(new CPluginVariable("Ban announcement|Show bans on chat tab", typeof(enumBoolYesNo), this.m_enDoBanOnChat));
            }

            lstReturn.Add(new CPluginVariable("Kick announcement|Announce kicks", typeof(enumBoolYesNo), this.m_enDoKickAnnounce));
            if (this.m_enDoKickAnnounce == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Kick announcement|Show PB-kicks", typeof(enumBoolYesNo), this.m_enDoKickPB));
                lstReturn.Add(new CPluginVariable("Kick announcement|Yell kicks", typeof(enumBoolYesNo), this.m_enDoKickYell));
                lstReturn.Add(new CPluginVariable("Kick announcement|Show kicks (seconds)", this.m_iDisplayTimeKick.GetType(), this.m_iDisplayTimeKick / 1000));
                lstReturn.Add(new CPluginVariable("Kick announcement|Msg on kicks", typeof(string), this.m_strKickAnnounceMsg));
                lstReturn.Add(new CPluginVariable("Kick announcement|Msg on VIPKick", typeof(string), this.m_strVIPKickMsg));
                lstReturn.Add(new CPluginVariable("Kick announcement|Show kicks on chat tab", typeof(enumBoolYesNo), this.m_enDoKickOnChat));
            }

            lstReturn.Add(new CPluginVariable("SpawnMsg|Enable on spawn message", typeof(enumBoolYesNo), this.m_enDoSpawnMessage));
            if (this.m_enDoSpawnMessage == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("SpawnMsg|Yell spawn message", typeof(enumBoolYesNo), this.m_enDoSpawnYell));
                if (this.m_enDoSpawnYell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("SpawnMsg|Show spanMsg (seconds)", this.m_iDisplayTimeSpawn.GetType(), this.m_iDisplayTimeSpawn / 1000));
                }
                lstReturn.Add(new CPluginVariable("SpawnMsg|OnSpawn message", typeof(string), this.m_strOnSpawnMsg));
                lstReturn.Add(new CPluginVariable("SpawnMsg|Send in interval", typeof(enumBoolYesNo), this.m_enDoSpawnInterval));
                if (this.m_enDoSpawnInterval == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("SpawnMsg|Show on first spawn", typeof(enumBoolYesNo), this.m_enDoSpawnFirst));
                    lstReturn.Add(new CPluginVariable("SpawnMsg|Interval", typeof(int), this.m_iSpawnMsgInterval));
                }
                lstReturn.Add(new CPluginVariable("SpawnMsg|Show spawn message on chat tab", typeof(enumBoolYesNo), this.m_enDoSpawnOnChat));
            }
            lstReturn.Add(new CPluginVariable("Time|Show round duration", typeof(enumBoolYesNo), this.m_enDoRoundTimeDisplay));
            if (this.m_enDoRoundTimeDisplay == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Time|Round Interval (min)", this.m_iRoundTimeInterval.GetType(), this.m_iRoundTimeInterval));
                lstReturn.Add(new CPluginVariable("Time|Yell round duration", typeof(enumBoolYesNo), this.m_enDoRoundTimeYell));
                if (this.m_enDoRoundTimeYell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("Time|Show round duration (seconds)", this.m_iDisplayTimeRound.GetType(), this.m_iDisplayTimeRound / 1000));
                }
            } 
            lstReturn.Add(new CPluginVariable("Time|Show server uptime", typeof(enumBoolYesNo), this.m_enDoUptimeDisplay));
            if (this.m_enDoUptimeDisplay == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Time|Uptime Interval (min)", this.m_iUptimeDisplayInterval.GetType(), this.m_iUptimeDisplayInterval));
            }
            /* not implemented 
            if (this.m_enDoRoundTimeDisplay == enumBoolYesNo.Yes && this.m_enDoUptimeDisplay == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Time|Use one line", typeof(enumBoolYesNo), this.m_enDoTimesOneLine));
            } */
            if (this.m_enDoRoundTimeDisplay == enumBoolYesNo.Yes || this.m_enDoUptimeDisplay == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Time|Show In-Game", typeof(enumBoolYesNo), this.m_enDoTimesInGame));
                lstReturn.Add(new CPluginVariable("Time|Show on Chat-Tab", typeof(enumBoolYesNo), this.m_enDoTimesChatTab));
            }
            lstReturn.Add(new CPluginVariable("Spotting|Spot players", typeof(enumBoolYesNo), this.m_enDoPlayerSpot));
            if (this.m_enDoPlayerSpot == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Spotting|Spot after 1. kill", typeof(enumBoolYesNo), this.m_enDoPlayerSpotExtend));
                lstReturn.Add(new CPluginVariable("Spotting|Spot on Chat-Tab", typeof(enumBoolYesNo), this.m_enDoPlayerChatTab));
                lstReturn.Add(new CPluginVariable("Spotting|Spot message", typeof(string), this.m_strPlayerSpotMsg));
            }
            lstReturn.Add(new CPluginVariable("Xtras|ChatTab prefix", typeof(string), this.m_strChatTabPrefix));
            lstReturn.Add(new CPluginVariable("Xtras|On layer", typeof(enumBoolYesNo), this.m_enPluginOnLayer)); 
            lstReturn.Add(new CPluginVariable("Xtras|Console output", typeof(enumBoolYesNo), this.m_enDoConsoleOutput));
            lstReturn.Add(new CPluginVariable("Xtras|Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            lstReturn.Add(new CPluginVariable("Xtras|Quiet mode", typeof(enumBoolYesNo), this.m_enDoQuietMode));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {

            if (this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl")
            {
                this.m_iYellDivider = 1000;
            }

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            lstReturn.Add(new CPluginVariable("Announce admin", typeof(enumBoolYesNo), this.m_enDoAdminAnnounce));
            lstReturn.Add(new CPluginVariable("Yell admin announcement", typeof(enumBoolYesNo), this.m_enDoAnnounceYell));
            lstReturn.Add(new CPluginVariable("Show announcement (seconds)", this.m_iDisplayTimeAdmin.GetType(), this.m_iDisplayTimeAdmin / 1000));
            lstReturn.Add(new CPluginVariable("Announce admin name", typeof(enumBoolYesNo), this.m_enDoAdminAnnounceName));
            lstReturn.Add(new CPluginVariable("Announcement with name", typeof(string), this.m_strAdminAnnounceNameMsg));
            lstReturn.Add(new CPluginVariable("Announcement", typeof(string), this.m_strAdminAnnounceMsg));
            lstReturn.Add(new CPluginVariable("Show announcement on chat tab", typeof(enumBoolYesNo), this.m_enDoAnnounceOnChat));
            lstReturn.Add(new CPluginVariable("Admin list", typeof(string[]), this.m_lstAdminList.ToArray()));
            lstReturn.Add(new CPluginVariable("Check admin privileges", typeof(enumBoolYesNo), this.m_enDoCheckAdminPrivs));

            lstReturn.Add(new CPluginVariable("Announce vip", typeof(enumBoolYesNo), this.m_enDoVipAnnounce));
            lstReturn.Add(new CPluginVariable("Yell vip announcement", typeof(enumBoolYesNo), this.m_enDoVipAnnounceYell));
            lstReturn.Add(new CPluginVariable("Show vip announcement (seconds)", this.m_iDisplayTimeVip.GetType(), this.m_iDisplayTimeAdmin / 1000));
            lstReturn.Add(new CPluginVariable("Announce vip name", typeof(enumBoolYesNo), this.m_enDoVipAnnounceName));
            lstReturn.Add(new CPluginVariable("Vip announcement with name", typeof(string), this.m_strVipAnnounceNameMsg));
            lstReturn.Add(new CPluginVariable("Vip announcement", typeof(string), this.m_strVipAnnounceMsg));
            lstReturn.Add(new CPluginVariable("Show vip announcement on chat tab", typeof(enumBoolYesNo), this.m_enDoVipAnnounceOnChat));
            lstReturn.Add(new CPluginVariable("Vip list", typeof(string[]), this.m_lstVipList.ToArray()));
            lstReturn.Add(new CPluginVariable("Check server vip list", typeof(enumBoolYesNo), this.m_enDoCheckVipList));

            lstReturn.Add(new CPluginVariable("Announce bans", typeof(enumBoolYesNo), this.m_enDoBanAnnounce));
            lstReturn.Add(new CPluginVariable("Named bans only", typeof(enumBoolYesNo), this.m_enDoBanOnlyName));
            lstReturn.Add(new CPluginVariable("Show BC2-bans", typeof(enumBoolYesNo), this.m_enDoBanBC2));
            lstReturn.Add(new CPluginVariable("Show PB-bans", typeof(enumBoolYesNo), this.m_enDoBanPB));
            lstReturn.Add(new CPluginVariable("Yell bans", typeof(enumBoolYesNo), this.m_enDoBanYell));
            lstReturn.Add(new CPluginVariable("Show bans (seconds)", this.m_iDisplayTimeBan.GetType(), this.m_iDisplayTimeBan / 1000));
            lstReturn.Add(new CPluginVariable("Msg on bans", typeof(string), this.m_strBanAnnounceMsg));
            lstReturn.Add(new CPluginVariable("Show bans on chat tab", typeof(enumBoolYesNo), this.m_enDoBanOnChat));

            lstReturn.Add(new CPluginVariable("Announce kicks", typeof(enumBoolYesNo), this.m_enDoKickAnnounce));
            lstReturn.Add(new CPluginVariable("Show PB-kicks", typeof(enumBoolYesNo), this.m_enDoKickPB));
            lstReturn.Add(new CPluginVariable("Yell kicks", typeof(enumBoolYesNo), this.m_enDoKickYell));
            lstReturn.Add(new CPluginVariable("Show kicks (seconds)", this.m_iDisplayTimeKick.GetType(), this.m_iDisplayTimeKick / 1000));
            lstReturn.Add(new CPluginVariable("Msg on kicks", typeof(string), this.m_strKickAnnounceMsg));
            lstReturn.Add(new CPluginVariable("Msg on VIPKick", typeof(string), this.m_strVIPKickMsg));
            lstReturn.Add(new CPluginVariable("Show kicks on chat tab", typeof(enumBoolYesNo), this.m_enDoKickOnChat));

            lstReturn.Add(new CPluginVariable("Enable on spawn message", typeof(enumBoolYesNo), this.m_enDoSpawnMessage));
            lstReturn.Add(new CPluginVariable("Yell spawn message", typeof(enumBoolYesNo), this.m_enDoSpawnYell));
            lstReturn.Add(new CPluginVariable("Show spanMsg (seconds)", this.m_iDisplayTimeSpawn.GetType(), this.m_iDisplayTimeSpawn / 1000));
            lstReturn.Add(new CPluginVariable("OnSpawn message", typeof(string), this.m_strOnSpawnMsg));
            lstReturn.Add(new CPluginVariable("Send in interval", typeof(enumBoolYesNo), this.m_enDoSpawnInterval));
            lstReturn.Add(new CPluginVariable("Show on first spawn", typeof(enumBoolYesNo), this.m_enDoSpawnFirst));
            lstReturn.Add(new CPluginVariable("Interval", typeof(int), this.m_iSpawnMsgInterval));
            lstReturn.Add(new CPluginVariable("Show spawn message on chat tab", typeof(enumBoolYesNo), this.m_enDoSpawnOnChat));

            lstReturn.Add(new CPluginVariable("Show round duration", typeof(enumBoolYesNo), this.m_enDoRoundTimeDisplay));
            lstReturn.Add(new CPluginVariable("Round Interval (min)", this.m_iRoundTimeInterval.GetType(), this.m_iRoundTimeInterval)); 
            lstReturn.Add(new CPluginVariable("Yell round duration", typeof(enumBoolYesNo), this.m_enDoRoundTimeYell));
            lstReturn.Add(new CPluginVariable("Show round duration (seconds)", this.m_iDisplayTimeRound.GetType(), this.m_iDisplayTimeRound / 1000));
            lstReturn.Add(new CPluginVariable("Show server uptime", typeof(enumBoolYesNo), this.m_enDoUptimeDisplay));
            lstReturn.Add(new CPluginVariable("Uptime Interval (min)", this.m_iUptimeDisplayInterval.GetType(), this.m_iUptimeDisplayInterval));
            // not implemented lstReturn.Add(new CPluginVariable("Use one line", typeof(enumBoolYesNo), this.m_enDoTimesOneLine));
            lstReturn.Add(new CPluginVariable("Show In-Game", typeof(enumBoolYesNo), this.m_enDoTimesInGame));
            lstReturn.Add(new CPluginVariable("Show on Chat-Tab", typeof(enumBoolYesNo), this.m_enDoTimesChatTab));

            lstReturn.Add(new CPluginVariable("Spot players", typeof(enumBoolYesNo), this.m_enDoPlayerSpot));
            lstReturn.Add(new CPluginVariable("Spot after 1. kill", typeof(enumBoolYesNo), this.m_enDoPlayerSpotExtend));
            lstReturn.Add(new CPluginVariable("Spot on Chat-Tab", typeof(enumBoolYesNo), this.m_enDoPlayerChatTab));
            lstReturn.Add(new CPluginVariable("Spot message", typeof(string), this.m_strPlayerSpotMsg));

            lstReturn.Add(new CPluginVariable("ChatTab prefix", typeof(string), this.m_strChatTabPrefix));
            lstReturn.Add(new CPluginVariable("On layer", typeof(enumBoolYesNo), this.m_enPluginOnLayer)); 
            lstReturn.Add(new CPluginVariable("Console output", typeof(enumBoolYesNo), this.m_enDoConsoleOutput));
            lstReturn.Add(new CPluginVariable("Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            lstReturn.Add(new CPluginVariable("Quiet mode", typeof(enumBoolYesNo), this.m_enDoQuietMode));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) 
        {
            int iTimeSeconds = 4;
            int iInterval = 2;

            if (strVariable.CompareTo("Announce admin") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) 
            {
                this.m_enDoAdminAnnounce = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell admin announcement") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoAnnounceYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show announcement (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeAdmin = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeAdmin = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeAdmin = 59999;
                }
            }
            else if (strVariable.CompareTo("Announce admin name") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoAdminAnnounceName = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Announcement with name") == 0)
            {
                if (strValue.Length > 84)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 84 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 84 chars.", 2);
                }
                else
                {
                    this.m_strAdminAnnounceNameMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Announcement") == 0)
            {
                if (strValue.Length > 100)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 100 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 100 chars.", 2);
                }
                else
                {
                    this.m_strAdminAnnounceMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Admin list") == 0)
            {
                this.m_lstAdminList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Check admin privileges") == 0)
            {
                this.m_enDoCheckAdminPrivs = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show announcement on chat tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoAnnounceOnChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            // ...
            else if (strVariable.CompareTo("Announce vip") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoVipAnnounce = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell vip announcement") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoVipAnnounceYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show vip announcement (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeVip = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeVip = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeVip = 59999;
                }
            }
            else if (strVariable.CompareTo("Announce vip name") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoVipAnnounceName = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Vip announcement with name") == 0)
            {
                if (strValue.Length > 84)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 84 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 84 chars.", 2);
                }
                else
                {
                    this.m_strVipAnnounceNameMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Vip announcement") == 0)
            {
                if (strValue.Length > 100)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 100 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 100 chars.", 2);
                }
                else
                {
                    this.m_strVipAnnounceMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Vip list") == 0)
            {
                this.m_lstVipList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Check server vip list") == 0)
            {
                this.m_enDoCheckVipList = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show vip announcement on chat tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoVipAnnounceOnChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            // ---
            else if (strVariable.CompareTo("Announce bans") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanAnnounce = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Named bans only") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanOnlyName = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell bans") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show BC2-bans") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanBC2 = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show PB-bans") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanPB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show bans (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeBan = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeBan = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeBan = 59999;
                }
            }
            else if (strVariable.CompareTo("Msg on bans") == 0)
            {
                if (strValue.Length > 30)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No) 
                    {
                        MessageBox.Show("Your message is to long. Please only use 30 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 30 chars.", 2);
                }
                else
                {
                    this.m_strBanAnnounceMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Show bans on chat tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoBanOnChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Announce kicks") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoKickAnnounce = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show PB-kicks") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoKickPB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell kicks") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoKickYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show kicks (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeKick = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeKick = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeKick = 59999;
                }
            }
            else if (strVariable.CompareTo("Msg on kicks") == 0)
            {
                if (strValue.Length > 30)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No) 
                    {
                        MessageBox.Show("Your message is to long. Please only use 30 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 30 chars.", 2);
                }
                else
                {
                    this.m_strKickAnnounceMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Msg on VIPKick") == 0)
            {
                if (strValue.Length > 80)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No) 
                    {
                        MessageBox.Show("Your message is to long. Please only use 80 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 80 chars.", 2);
                }
                else
                {
                    this.m_strVIPKickMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Show kicks on chat tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoKickOnChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable on spawn message") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoSpawnMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell spawn message") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enDoSpawnYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show spanMsg (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeSpawn = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeSpawn = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeSpawn = 59999;
                }
            }
            else if (strVariable.CompareTo("OnSpawn message") == 0)
            {
                if (strValue.Length > 100)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 100 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 100 chars.", 2);
                }
                else
                {
                    this.m_strOnSpawnMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("Send in interval") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoSpawnInterval = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show on first spawn") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoSpawnFirst = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoSpawnFirst == enumBoolYesNo.Yes)
                {
                    this.m_iSpawnStart = this.m_iSpawnMsgInterval - 1;
                }
            }
            else if (strVariable.CompareTo("Interval") == 0 && int.TryParse(strValue, out iInterval) == true)
            {
                if (iInterval <= 2 ) { iInterval = 2; }
                this.m_iSpawnMsgInterval = iInterval;
                if (this.m_enDoSpawnFirst == enumBoolYesNo.Yes)
                {
                    this.m_iSpawnStart = iInterval - 1;
                }
            }
            else if (strVariable.CompareTo("Show spawn message on chat tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoSpawnOnChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            // server uptime
            else if (strVariable.CompareTo("Show server uptime") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoUptimeDisplay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Uptime Interval (min)") == 0 && int.TryParse(strValue, out iInterval) == true)
            {
                if (iInterval <= 1) { iInterval = 1; }
                if (iInterval >= 720) { iInterval = 720; }
                this.m_iUptimeDisplayInterval = iInterval;
            }
            // round duration
            else if (strVariable.CompareTo("Show round duration") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoRoundTimeDisplay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Round Interval (min)") == 0 && int.TryParse(strValue, out iInterval) == true)
            {
                if (iInterval <= 1) { iInterval = 1; }
                if (iInterval >= 720) { iInterval = 720; }
                this.m_iRoundTimeInterval = iInterval;
            }
            else if (strVariable.CompareTo("Yell round duration") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoRoundTimeYell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show round duration (seconds)") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDisplayTimeRound = iTimeSeconds * 1000;

                if (iTimeSeconds <= 0)
                {
                    this.m_iDisplayTimeRound = 1000;
                }
                else if (iTimeSeconds > 60)
                {
                    this.m_iDisplayTimeRound = 59999;
                }
            }
                //m_enDoTimesOneLine m_enDoTimesInGame m_enDoTimesChatTab
            else if (strVariable.CompareTo("Use one line") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoTimesOneLine = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show In-Game") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoTimesInGame = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show on Chat-Tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoTimesChatTab = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Spot players") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoPlayerSpot = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Spot after 1. kill") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoPlayerSpotExtend = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Spot on Chat-Tab") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoPlayerChatTab = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Spot message") == 0)
            {
                if (strValue.Length > 84)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 84 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 84 chars.", 2);
                }
                else
                {
                    this.m_strPlayerSpotMsg = strValue;
                }
            }
            else if (strVariable.CompareTo("ChatTab prefix") == 0)
            {
                if (strValue.Length > 20)
                {
                    if (this.m_enPluginOnLayer == enumBoolYesNo.No)
                    {
                        MessageBox.Show("Your message is to long. Please only use 20 chars.", "Input Error", MessageBoxButtons.OK);
                    }
                    this.WritePluginConsole("Your message is to long. Please only use 20 chars.", 2);
                }
                else
                {
                    this.m_strChatTabPrefix = strValue;
                }
            }
            else if (strVariable.CompareTo("On layer") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enPluginOnLayer = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Console output") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoConsoleOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoConsoleOutput == enumBoolYesNo.No && this.m_enDoQuietMode == enumBoolYesNo.Yes && this.m_enDoDebugOutput == enumBoolYesNo.No)
                {
                    this.m_enDoConsoleOutput = enumBoolYesNo.Yes;
                }
            }
            else if (strVariable.CompareTo("Debug output") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoDebugOutput == enumBoolYesNo.No && this.m_enDoQuietMode == enumBoolYesNo.Yes)
                {
                    this.m_enDoConsoleOutput = enumBoolYesNo.Yes;
                }
            }
            else if (strVariable.CompareTo("Quiet mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoQuietMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoQuietMode == enumBoolYesNo.Yes) this.m_enDoConsoleOutput = enumBoolYesNo.Yes;
            }
        }

        private void UnregisterAllCommands() {
        }

        private void SetupHelpCommands() {
        }

        private void RegisterAllCommands() {
        }

#region Events

        /* only debug
        public override void OnGlobalChat(string speaker, string message)
        {
            WritePluginConsole("G - " + speaker + " said: " + message, 2);
        }

        public override void OnTeamChat(string speaker, string message, int teamID)
        {
            WritePluginConsole("T - " + speaker + " said for team " + teamID.ToString() + " : " + message, 2);
        }
        */

        public override void OnVersion(string serverType, string version) {
            this.m_strServerType = serverType.ToLower();

            if (this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl")
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Discovered GameType: " + this.m_strServerType);
                }
                this.m_iYellDivider = 1000;
            }
        }

        public override void OnServerInfo(CServerInfo csiServerInfo)
        {
            this.m_iSrvInfoUptime = csiServerInfo.ServerUptime;
            this.m_iSrvInfoRoundTime = csiServerInfo.RoundTime;

            if (this.m_enDoUptimeDisplay == enumBoolYesNo.Yes) { this.m_iOldUptime = this.CheckDisplayTimes("server"); }
            if (this.m_enDoRoundTimeDisplay == enumBoolYesNo.Yes) { this.m_iOldRoundTime = this.CheckDisplayTimes("round"); }
        }

        public override void OnPlayerJoin(string playerName) {
            bool m_isAdmin = false;
            bool m_isVip = false;
            string strMessage = "";
            CPrivileges cpPlayerPrivs = this.GetAccountPrivileges(playerName);

            //add player to dict.
            if (this.m_playerJoined.ContainsKey(playerName) == false)
            {
                this.m_playerJoined.Add(playerName, false);
            } 
            if (this.m_playerSpawns.ContainsKey(playerName) == false)
            {
                this.m_playerSpawns.Add(playerName, this.m_iSpawnStart);
            }

            if (this.m_playerJoined[playerName] == false)
            {
                #region onJoinAnnounce
                #region AnnounceAdmin
                if (this.m_enDoAdminAnnounce == enumBoolYesNo.Yes)
                {
                    if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Player (" + playerName + ") joined...");
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... comparing with admin list...");
                    }
                    foreach (string admin in this.m_lstAdminList)
                    {
                        if (admin.CompareTo(playerName) == 0)
                        {
                            m_isAdmin = true;
                        }
                    }
                    if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                    {
                        if (m_isAdmin == true)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Admin found!");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... no hit.");
                        }
                    }
                    if (m_isAdmin == false && this.m_enDoCheckAdminPrivs == enumBoolYesNo.Yes)
                    {
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... checking account for temp.ban privileges ...");
                        }
                        if (cpPlayerPrivs.CanTemporaryBanPlayers == true)
                        {
                            m_isAdmin = true;
                            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... found privileges to do temp.ban!");
                            }
                        }
                    }
                    if (m_isAdmin == true)
                    {
                        strMessage = this.m_strAdminAnnounceMsg;
                        if (this.m_enDoAdminAnnounceName == enumBoolYesNo.Yes)
                        {
                            strMessage = this.m_strAdminAnnounceNameMsg.Replace("%name%", playerName);
                        }
                        if (this.m_enDoConsoleOutput == enumBoolYesNo.Yes || this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: " + playerName + " triggered: " + strMessage);
                        }
                        if (this.m_enDoQuietMode == enumBoolYesNo.No)
                        {
                            string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 > ^4" + strMessage + "^0";
                            if (this.m_enDoAnnounceYell == enumBoolYesNo.Yes)
                            {
                                if (this.m_strServerType == "mohw")
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, "all");
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, (this.m_iDisplayTimeAdmin / this.m_iYellDivider).ToString(), "all");
                                }
                                if (this.m_enDoAnnounceOnChat == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.chat.write", strTmpMsg.ToUpper());
                                }
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strMessage, "all");
                                if (this.m_enDoAnnounceOnChat == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
                                }
                            }
                        }
                    }
                }
                #endregion // AnnounceAdmin
                #region AnnounceVIP
                if (this.m_enDoVipAnnounce == enumBoolYesNo.Yes && ((this.m_enDoAdminAnnounce == enumBoolYesNo.Yes && m_isAdmin == false) || this.m_enDoAdminAnnounce == enumBoolYesNo.No))
                {
                    //
                    if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... comparing with VIP/reservedSlots list...");
                    }
                    foreach (string vip in this.m_lstVipList)
                    {
                        if (vip.CompareTo(playerName) == 0)
                        {
                            m_isVip = true;
                        }
                    }

                    if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                    {
                        if (m_isVip == true)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Vip found!");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... no hit.");
                        }
                    }
                    if (m_isVip == false && this.m_enDoCheckVipList == enumBoolYesNo.Yes)
                    {
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... checking account on reservedSlots list ...");
                        }
                        foreach (string vip in this.m_lstSrvVipList)
                        {
                            if (vip.CompareTo(playerName) == 0)
                            {
                                m_isVip = true;
                                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... found ...");
                                }
                            }
                        }
                    }
                    if (m_isVip == true)
                    {
                        strMessage = this.m_strVipAnnounceMsg;
                        if (this.m_enDoVipAnnounceName == enumBoolYesNo.Yes)
                        {
                            strMessage = this.m_strVipAnnounceNameMsg.Replace("%name%", playerName);
                        }
                        if (this.m_enDoConsoleOutput == enumBoolYesNo.Yes || this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: " + playerName + " triggered: " + strMessage);
                        }
                        if (this.m_enDoQuietMode == enumBoolYesNo.No)
                        {
                            string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 > ^4" + strMessage + "^0";
                            if (this.m_enDoVipAnnounceYell == enumBoolYesNo.Yes)
                            {
                                if (this.m_strServerType == "mohw")
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, "all");
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, (this.m_iDisplayTimeAdmin / this.m_iYellDivider).ToString(), "all");
                                }
                                if (this.m_enDoVipAnnounceOnChat == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.chat.write", strTmpMsg.ToUpper());
                                }
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strMessage, "all");
                                if (this.m_enDoVipAnnounceOnChat == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
                                }
                            }
                        }
                    }
                }
                #endregion // AnnounceVIP
                #endregion // onJoinAnnounce

                #region PlayerSpotingBF3
                if ((this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl" || this.m_strServerType == "mohw") &&
                    this.m_enDoPlayerSpot == enumBoolYesNo.Yes && (this.m_blIsPreRound == true || this.m_enDoPlayerSpotExtend == enumBoolYesNo.Yes) &&
                    (m_isVip == false && m_isAdmin == false))
                {
                    strMessage = string.Format(this.m_strPlayerSpotMsg, playerName);
                    this.WritePluginConsole(strMessage, 0);
                    this.WriteChatTab(strMessage, this.m_enDoPlayerChatTab);
                    this.WriteMessage(strMessage, enumBoolYesNo.Yes);
                }
                #endregion

                this.m_playerJoined[playerName] = true;
            }
            else
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Player (" + playerName + ") already joined... doing nothing.");
                }
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            bool bSendMsg = true;

            if (this.m_playerBanned.ContainsKey(soldierName) == true)
            {
                this.m_playerBanned.Remove(soldierName);
            }
            
            if (this.m_enDoSpawnMessage == enumBoolYesNo.Yes)
            {
                if (this.m_playerSpawns.ContainsKey(soldierName) == false)
                {
                    this.m_playerSpawns.Add(soldierName, this.m_iSpawnStart);
                }
                this.m_playerSpawns[soldierName]++;

                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    if (this.m_enDoSpawnInterval == enumBoolYesNo.No)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Player (" + soldierName + ") spawned...");
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... sending message...");
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Player (" + soldierName + ") spawned...");
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... checking spawn counter ...");
                    }
                }

                if (this.m_enDoSpawnInterval == enumBoolYesNo.Yes)
                {
                    if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Spawn/Interval: " + this.m_playerSpawns[soldierName] + "/" + this.m_iSpawnMsgInterval + " ...");
                    }
                    if (this.m_iSpawnMsgInterval == this.m_playerSpawns[soldierName])
                    {
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... sending message...");
                        }
                        this.m_playerSpawns[soldierName] = 0;
                    }
                    else
                    {
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... no message.");
                        }
                        bSendMsg = false;
                    }
                }
                
                if ((this.m_enDoConsoleOutput == enumBoolYesNo.Yes || this.m_enDoDebugOutput == enumBoolYesNo.Yes) && bSendMsg == true)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: " + soldierName + " <- " + this.m_strOnSpawnMsg);
                }
                if (this.m_enDoQuietMode == enumBoolYesNo.No && bSendMsg == true)
                {
                    string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 - ^4" + soldierName + "^0 > ^4" + this.m_strOnSpawnMsg + "^0";
                    if (this.m_enDoSpawnYell == enumBoolYesNo.Yes)
                    {
                        if (this.m_strServerType == "mohw") {
                            // not possible for MoHW to addess an individual player, but Procon will send it to his fireteam
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.m_strOnSpawnMsg, "player", soldierName);
                        } else {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.m_strOnSpawnMsg, (this.m_iDisplayTimeSpawn / this.m_iYellDivider).ToString(), "player", soldierName);
                        }
                        if (this.m_enDoSpawnOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", strTmpMsg.ToUpper());
                        }
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", this.m_strOnSpawnMsg, "player", soldierName);
                        if (this.m_enDoSpawnOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
                        }
                    }
                }
            }
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails) {
            this.m_blIsPreRound = false;
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo) {
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Player left ({0}) ...", playerInfo.SoldierName));
            }

            if (this.m_playerJoined.ContainsKey(playerInfo.SoldierName) == true)
            {
                this.m_playerJoined.Remove(playerInfo.SoldierName);
            }

            if (this.m_playerSpawns.ContainsKey(playerInfo.SoldierName) == true)
            {
                this.m_playerSpawns.Remove(playerInfo.SoldierName);
            }

            if (this.m_enDoBanAnnounce == enumBoolYesNo.Yes && this.m_bCheckBan == true)
            {
                this.DoCompareBan(playerInfo);
            }
            
            if (this.m_enDoKickAnnounce == enumBoolYesNo.Yes && this.m_bCheckKick == true)
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Checking kick event ...");
                }
                if (this.m_playerKicked.ContainsKey(playerInfo.SoldierName) == true && this.m_playerBanned.ContainsKey(playerInfo.SoldierName) == false)
                { 
                    this.DoBanKickOutput(playerInfo.SoldierName, this.m_playerKicked[playerInfo.SoldierName], true);
                    this.m_playerKicked.Remove(playerInfo.SoldierName);
                }
            }

        }

        public override void OnPlayerKicked(string soldierName, string reason) {
            if (this.m_playerKicked.ContainsKey(soldierName) == false)
            {
                this.m_playerKicked.Add(soldierName, reason);
            }
            this.m_bCheckKick = true;
            if (this.m_enDoKickAnnounce == enumBoolYesNo.Yes)
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Kick event noticed ({0}) ...", soldierName));
                }
            }
        }

        public override void OnPlayerKickedByAdmin(string soldierName, string reason) {
            if (this.m_strServerType == "bf3" || this.m_strServerType == "bf4" || this.m_strServerType == "bfhl")
            {
                this.OnPlayerKicked(soldierName, reason);
            }
        }

        public override void OnBanAdded(CBanInfo ban) {
            this.m_bannedPlayer = ban;
            this.m_bCheckBan = true;

            if (this.m_enDoBanAnnounce == enumBoolYesNo.Yes)
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Ban event noticed ({0}) ...", ban.IdType));
                    if (ban.IdType == "pbguid") {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... PB_GUID {0} ...", ban.Guid));
                    }
                }
            }
        }

        public override void OnPunkbusterBanInfo(CBanInfo ban) {
            this.OnBanAdded(ban);
        }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) {
            this.m_blIsPreRound = true;
        }

        public override void OnReservedSlotsList(List<string> soldierNames) {
            this.m_lstSrvVipList = soldierNames;

            this.WritePluginConsole(String.Format("Got {0} VIPs from the server.", this.m_lstSrvVipList.Count.ToString()), 1);
        }

#endregion

#region own_functions

        private void WritePluginConsole(string message, int debug)
        {
            string line = String.Format("AdminIn_and_SpawnMsg: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes || (this.m_enDoConsoleOutput == enumBoolYesNo.Yes && debug == 0) || debug == 2)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        private void WriteMessage(string message, enumBoolYesNo DoWrite)
        {
            if (DoWrite == enumBoolYesNo.Yes && this.m_enDoQuietMode == enumBoolYesNo.No)
            {
                List<string> wordWrappedLines = this.WordWrap(message, 100);
                foreach (string line in wordWrappedLines)
                {
                    string formattedLine = String.Format("{0}", line);
                    this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "all");
                }
            }
        }

        private void YellMessageToAll(string strMessage, int iDisplayTime, enumBoolYesNo DoYell, enumBoolYesNo DoWriteInGame, enumBoolYesNo DoWriteChatTab)
        {
            string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 - " + strMessage + "^0";
            if (DoYell == enumBoolYesNo.Yes)
            {
                strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 > ^4" + strMessage + "^0";
            }
            if (DoWriteInGame == enumBoolYesNo.Yes)
            {
                if (DoYell == enumBoolYesNo.Yes && this.m_enDoQuietMode == enumBoolYesNo.No)
                {
                    if (this.m_strServerType == "mohw") {
                        this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, "all");
                    } else {
                        this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, (iDisplayTime / this.m_iYellDivider).ToString(), "all");
                    }
                    if (this.m_enDoAnnounceOnChat == enumBoolYesNo.Yes)
                    {
                        strTmpMsg = strTmpMsg.ToUpper();
                    }
                }
                if (DoYell == enumBoolYesNo.No && this.m_enDoQuietMode == enumBoolYesNo.No)
                {
                    this.WriteMessage(strMessage, DoWriteInGame);
                }
            }
            if (DoWriteChatTab == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
            }
        }

        private void WriteChatTab(string message, enumBoolYesNo DoWrite)
        {
            string strTmpMsg = "^4" + this.m_strChatTabPrefix +"^0 - " + message + "^0";
            if (DoWrite == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
            }
        }

        private int CheckDisplayTimes(string strSource)
        {
            int OldTime = 0;
            int NewTime = 0;
            int TimeDif = 0;
            int Interval = 0;
            string msgStart = "";
            string tmpMsg = "";

            if (strSource == "server")
            {
                OldTime = this.m_iOldUptime;
                NewTime = this.m_iSrvInfoUptime;
                TimeDif = NewTime - OldTime;
                Interval = this.m_iUptimeDisplayInterval * 60; 
                msgStart = "Server Uptime: ";
            }
            if (strSource == "round")
            {
                OldTime = this.m_iOldRoundTime;
                NewTime = this.m_iSrvInfoRoundTime;
                TimeDif = NewTime - OldTime;
                Interval = this.m_iRoundTimeInterval * 60;
                msgStart = "Round duration: ";
            }
            if (strSource == "round" || strSource == "server")
            {
                if (OldTime > NewTime) { OldTime = 0; }

                this.WritePluginConsole("Old: " + OldTime + " New: " + NewTime + " Dif: " + TimeDif + " Interval: " + Interval, 1);
                if (TimeDif >= Interval)
                {
                    string strTimeOutput = this.SecondsToText((UInt32)NewTime, this.m_astrTimeDescription);
                    tmpMsg = msgStart + strTimeOutput;

                    this.WritePluginConsole(msgStart + strTimeOutput, 0);
                    if (strSource == "server")
                    {
                        this.WriteChatTab(tmpMsg, this.m_enDoTimesChatTab);
                        this.WriteMessage(tmpMsg, this.m_enDoTimesInGame);
                    } else {
                        this.YellMessageToAll(tmpMsg, this.m_iDisplayTimeRound, this.m_enDoRoundTimeYell, this.m_enDoTimesInGame, this.m_enDoTimesChatTab);
                    }
                    
                    return NewTime;
                }
            }
            return OldTime;
        }

        private string SecondsToText(UInt32 iSeconds, string[] a_strTimeDescriptions)
        {
            string strReturn = String.Empty;

            double dblSeconds = iSeconds;
            double dblMinutes = (iSeconds / 60);
            double dblHours = (dblMinutes / 60);
            double dblDays = (dblHours / 24);
            double dblWeeks = (dblDays / 7);
            double dblMonths = (dblWeeks / 4);
            double dblYears = (dblMonths / 12);

            if ((Int32)dblYears > 0)
            {
                strReturn += String.Empty + ((Int32)dblYears) + (((Int32)dblYears) == 1 ? a_strTimeDescriptions[0] : a_strTimeDescriptions[1]);
            }
            if ((Int32)dblMonths % 12 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMonths) % 12 + (((Int32)dblMonths % 12) == 1 ? a_strTimeDescriptions[2] : a_strTimeDescriptions[3]);
            }
            if ((Int32)dblWeeks % 4 > 0)
            {
                strReturn += String.Empty + ((Int32)dblWeeks) % 4 + (((Int32)dblWeeks % 4) == 1 ? a_strTimeDescriptions[4] : a_strTimeDescriptions[5]);
            }
            if ((Int32)dblDays % 7 > 0)
            {
                strReturn += String.Empty + ((Int32)dblDays) % 7 + (((Int32)dblDays % 7) == 1 ? a_strTimeDescriptions[6] : a_strTimeDescriptions[7]);
            }
            if ((Int32)dblHours % 24 > 0)
            {
                strReturn += String.Empty + ((Int32)dblHours) % 24 + (((Int32)dblHours % 24) == 1 ? a_strTimeDescriptions[8] : a_strTimeDescriptions[9]);
            }
            if ((Int32)dblMinutes % 60 > 0)
            {
                strReturn += String.Empty + ((Int32)dblMinutes) % 60 + (((Int32)dblMinutes % 60) == 1 ? a_strTimeDescriptions[10] : a_strTimeDescriptions[11]);
            }

            if (iSeconds < 60)
            {
                if ((Int32)dblSeconds % 60 > 0)
                {
                    strReturn += String.Empty + ((Int32)dblSeconds) % 60 + (((Int32)dblSeconds % 60) == 1 ? a_strTimeDescriptions[12] : a_strTimeDescriptions[13]);
                }
            }

            return strReturn;
        }

        private void DoBanKickOutput(string soldierName, string Reason, bool kicked)
        {
            if (Reason.CompareTo("PC_DIALOG_TEXT_VIPKICK") == 0) {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Rewriting PC_DIALOG_TEXT_VIPKICK...");
                }
                Reason = this.m_strVIPKickMsg;
            }
            
            if (this.m_enDoConsoleOutput == enumBoolYesNo.Yes || this.m_enDoDebugOutput == enumBoolYesNo.Yes)
            {
                if (kicked == false)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: " + soldierName + " was banned. Reason: " + Reason);
                }
                else
                {
                    if (Reason.Contains("PunkBuster kicked player") && this.m_enDoKickPB == enumBoolYesNo.No)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: Display of PB kicks disabled.");
                        return;
                    } 
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: " + soldierName + " was kicked. Reason: " + Reason);
                }
            }
            if (this.m_enDoQuietMode == enumBoolYesNo.No)
            {
                string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 > ^4" + string.Format(this.m_strBanAnnounceMsg, soldierName, Reason);
                string strTmpMsgKick = "^4" + this.m_strChatTabPrefix + "^0 > ^4" + string.Format(this.m_strKickAnnounceMsg, soldierName, Reason);
                if (kicked == false)
                {
                    string strTmpBan = string.Format(this.m_strBanAnnounceMsg, soldierName, Reason);
                    if (strTmpBan.Length > 100) { strTmpBan = strTmpBan.Remove(100); }
                    if (this.m_enDoBanYell == enumBoolYesNo.Yes)
                    {
                        if (this.m_strServerType == "mohw") {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", strTmpBan, "all");
                        } else {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", strTmpBan, (this.m_iDisplayTimeBan / this.m_iYellDivider).ToString(), "all");
                        }
                        if (this.m_enDoBanOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", (strTmpMsg.Length > 100) ? strTmpMsg.ToUpper().Remove(100) : strTmpMsg.ToUpper());
                        }
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTmpBan, "all");
                        if (this.m_enDoBanOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
                        }
                    }
                } else {
                    if (Reason.Contains("PunkBuster kicked player") && this.m_enDoKickPB == enumBoolYesNo.No)
                    {
                        return;
                    }
                    string strTmpKick = string.Format(this.m_strKickAnnounceMsg, soldierName, Reason);
                    if (strTmpKick.Length > 100) { strTmpKick = strTmpKick.Remove(100); }
                    if (this.m_enDoKickYell == enumBoolYesNo.Yes)
                    {
                        if (this.m_strServerType == "mohw") {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", strTmpKick, "all");
                        } else {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", strTmpKick, (this.m_iDisplayTimeKick / this.m_iYellDivider).ToString(), "all");
                        }
                        if (this.m_enDoKickOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", (strTmpMsgKick.Length > 100) ? strTmpMsgKick.ToUpper().Remove(100) : strTmpMsgKick.ToUpper());
                        }
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTmpKick, "all");
                        if (this.m_enDoKickOnChat == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.chat.write", strTmpMsgKick);
                        }
                    }
                }
            }
        }

        private void DoCompareBan(CPlayerInfo playerInfo)
        {
            if (this.m_enDoBanAnnounce == enumBoolYesNo.Yes && this.m_bCheckBan == true)
            {
                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Checking type of ban ...");
                }
                switch (this.m_bannedPlayer.IdType)
                {
                    case "name":
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Name ban found ...");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Comparing on left player ...");
                        }
                        if (string.Compare(playerInfo.SoldierName, this.m_bannedPlayer.SoldierName) == 0)
                        {
                            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Player ({0}) found.", playerInfo.SoldierName));
                            }
                            this.DoBanKickOutput(playerInfo.SoldierName, this.m_bannedPlayer.Reason, false);
                            if (this.m_playerBanned.ContainsKey(playerInfo.SoldierName) == false)
                            {
                                this.m_playerBanned.Add(playerInfo.SoldierName, this.m_bannedPlayer.Reason);
                                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Added player to the banned list.");
                                }
                            }
                            this.m_bCheckBan = false;
                            this.m_bCheckKick = false;
                        }
                        this.m_bCheckBan = false;
                        break;
                    case "ip":
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes && this.m_enDoBanOnlyName == enumBoolYesNo.No)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... IP ban found ...");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... can not be compared to playername.");
                        }
                        this.m_bCheckBan = false;
                        this.m_bCheckKick = true;
                        break;
                    case "guid":
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes && this.m_enDoBanOnlyName == enumBoolYesNo.No && this.m_enDoBanBC2 == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... EA_GUID ban found ...");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Comparing on left player ...");
                        }
                        if (string.Compare(playerInfo.GUID, this.m_bannedPlayer.Guid) == 0 && this.m_enDoBanOnlyName == enumBoolYesNo.No && this.m_enDoBanBC2 == enumBoolYesNo.Yes)
                        {
                            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Player ({0}) found.", playerInfo.SoldierName));
                            }
                            this.DoBanKickOutput(playerInfo.SoldierName, this.m_bannedPlayer.Reason, false);
                            if (this.m_playerBanned.ContainsKey(playerInfo.SoldierName) == false)
                            {
                                this.m_playerBanned.Add(playerInfo.SoldierName, this.m_bannedPlayer.Reason);
                                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Added player to the banned list.");
                                }
                            }
                            this.m_bCheckBan = false;
                            this.m_bCheckKick = false;
                        }
                        this.m_bCheckBan = false;
                        break;
                    case "pbguid":
                        if (this.m_enDoDebugOutput == enumBoolYesNo.Yes && this.m_enDoBanOnlyName == enumBoolYesNo.No && this.m_enDoBanPB == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... PB_GUID ban found ...");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Comparing on left player ...");
                        }
                        if (string.Compare(playerInfo.SoldierName, this.m_bannedPlayer.SoldierName) == 0 && this.m_enDoBanOnlyName == enumBoolYesNo.No && this.m_enDoBanPB == enumBoolYesNo.Yes)
                        {
                            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", string.Format("AdminIn_and_SpawnMsg: ... Player ({0}) found.", playerInfo.SoldierName));
                            }
                            this.DoBanKickOutput(playerInfo.SoldierName, this.m_bannedPlayer.Reason, false);
                            if (this.m_playerBanned.ContainsKey(playerInfo.SoldierName) == false)
                            {
                                this.m_playerBanned.Add(playerInfo.SoldierName, this.m_bannedPlayer.Reason);
                                if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "AdminIn_and_SpawnMsg: ... Added player to the banned list.");
                                }
                            }
                            this.m_bCheckBan = false;
                            this.m_bCheckKick = false;
                        }
                        this.m_bCheckBan = false;
                        break;
                    default:
                        break;
                }
            }
        }
#endregion
    }
}
