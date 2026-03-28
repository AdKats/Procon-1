/* CBattlelogCache.cs
 * written by Nick 'MorpheusX(AUT)' Mueller
 * This plugin should be used by plugin-authors to query
 * for Battlelog-stats. It uses a MySQL-Database to cache
 * requests and thus allows reducing the load on Battlelog
 * 
 * Procon Plugin Template by PapaCharlie9
 * Statsfetching part taken from Insane Limits
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

using MySql.Data.MySqlClient;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;


namespace PRoConEvents
{
    //Aliases
    using EventType = PRoCon.Core.Events.EventType;
    using CapturableEvent = PRoCon.Core.Events.CapturableEvents;

    public class CBattlelogCache : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region Variables

        // Enumerations
        public enum MessageTypeEnum { Warning, Error, Exception, Normal };
        public enum RequestTypeEnum { Overview, Weapon, Vehicle, ClanTag };

        // MatchCommands
        private MatchCommand matchCommandLookupRequest;

        private List<LookupRequest> lookupRequests;
        private List<LookupResponse> lookupResponses;

        // General settings
        private bool isEnabled;
        private int debugLevel;

        // MySQL login details
        private bool mySqlPromptDetails;
        private String mySqlHostname;
        private String mySqlPort;
        private String mySqlDatabase;
        private String mySqlTable;
        private String mySqlUsername;
        private String mySqlPassword;

        // Threading
        private readonly Object lookupRequestLock = new Object();
        private readonly Object lookupResponseLock = new Object();
        private EventWaitHandle requestLoopHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private EventWaitHandle responseLoopHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Thread requestLoopThread;
        private Thread responseLoopThread;

        #endregion

        public CBattlelogCache()
        {
            matchCommandLookupRequest = new MatchCommand("CBattlelogCache", "PlayerLookup", new List<string>(), "CBattlelogCache_LookupRequest", new List<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.None), "LookupRequest method for the Battlelog statscaching plugin. Cannot be called ingame");

            lookupRequests = new List<LookupRequest>();
            lookupResponses = new List<LookupResponse>();

            isEnabled = false;
            debugLevel = 2;
        }

        #region Plugin details

        public string GetPluginName()
        {
            return "Battlelog Cache";
        }

        public string GetPluginVersion()
        {
            return "1.0.1.0";
        }

        public string GetPluginAuthor()
        {
            return "MorpheusX(AUT)";
        }

        public string GetPluginWebsite()
        {
            return "http://www.phogue.net/forumvb/member.php?565-MorpheusX(AUT)";
        }

        public string GetPluginDescription()
        {
            return @"
        <h1>Battlelog Cache</h1>
        <p>Caching Plugin for Battlelog-Statsrequests</p>

        <h2>Description</h2>
        <p>This plugin should be used by plugin-authors to query for Battlelog-stats. It uses a MySQL-Database to cache requests and thus allows reducing the load on Battlelog.<br />
        This version if the very first public release and thus may contain bugs, however it has proven stable over a few days of testing on multiple servers. If you encounter any bugs, please inform the plugin author.<br />
        Whilst this caching-plugin will reduce the amount of requests made towards Battlelog, it doesn't solve the 'Too Many Requests' problem completely. The first version does not contain dynamic throtteling and re-fetching of failed requests yet.</p>

        <h2>Commands</h2>
        <p><i>Battlelog Cache</i> does not include any ingame-commands or additional functionality, but just provides an interface for other plugins to query. In order to communicate with <i>Battlelog Cache</i>, a plugin must implement the following mechanisms:<br />
        <ol>
        <li>Provide a public method following the given scheme: <i>public void METHODNAME(params String[] response) { }</i></li>
        <li>Check whether <i>Battlelog Cache</i> is installed by issueing <i>this.GetRegisteredCommands();</i> and parsing the list for a MatchCommand with <i>RegisteredClassname == CBattlelogCache</i> and <i>RegisteredMethodName == PlayerLookup</i></li>
        <li>Create a Hashtable with the following keys: <i>playerName</i>, <i>pluginName</i>, <i>pluginMethod</i>, <i>requestType</i></li>
        <li>Execute a query calling the following command: <i>this.ExecuteCommand(" + "\"" + "procon.protected.plugins.call" + "\"" + ", " + "\"" + "CBattlelogCache" + "\"" + ", " + "\"" + "PlayerLookup" + "\"" + @", JSON.JsonEncode(HASHTABLE));</i></li>
        </ol>
        <ul>
        <li><i>METHODNAME</i> would be the name of the plugin's public method for returning stats</li>
        <li><i>playerName</i> should have the name of the player as a content</li>
        <li><i>pluginName</i> should have the Class-Name of a plugin as a content (e.g. CBattlelogCache)</li>
        <li><i>pluginMethod</i> should have METHODNAME as a content</li>
        <li><i>requestType</i> defines the type of the statsrequest. Possible requests: 'overview', 'weapon', 'vehicle', 'clanTag'</li>
        <li><i>HASHTABE</i> should be replaced with the hashtable created in the step before</li>
        </ul></p>

        <h2>Settings</h2>
        <blockquote><h4>Debug level</h4>
        <p><i>Indicates how much debug-output is printed to the plugin-console. 0 turns off debug messages (just shows important warnings/exceptions), 6 documents nearly every step.</i></p>
        </blockquote>
        <blockquote><h4>MySQL Hostname</h4>
        <p><i>Hostname of the MySQL-Server <i>Battlelog Cache</i> should connect to.</i></p>
        </blockquote>
        <blockquote><h4>MySQL Port</h4>
        <p><i>Port of the MySQL-Server <i>Battlelog Cache</i> should connect to. (Default: 3306)</i></p>
        </blockquote>
        <blockquote><h4>MySQL Database</h4>
        <p><i>Database <i>Battlelog Cache</i> should use to cache stats.</i></p>
        </blockquote>
        <blockquote><h4>MySQL Table</h4>
        <p><i>Table <i>Battlelog Cache</i> should use to cache stats. (Default: playerstats)</i></p>
        </blockquote>
        <blockquote><h4>MySQL Username</h4>
        <p><i>Username <i>Battlelog Cache</i> should use to authenticate with the MySQL-Server.</i></p>
        </blockquote>
        <blockquote><h4>MySQL Password</h4>
        <p><i>Password <i>Battlelog Cache</i> should use to authenticate with the MySQL-Server.</i></p>
        </blockquote>
        <p>If you don't see the MySQL settings, the MySQL details have been 'hardcoded' in the <i>Configs/gsp_settings/battlelogcache_mysql.cfg</i> file. Hosters can use this to 'enforce' all users to query a centralized database, allowing more cache-hits.<br />
        To disable the MySQL settings, create a subfolder <i>gsp_settings</i> within the <i>Configs</i> folder in Procon. Within this subfolder, create a file called <i>battlelogcache_mysql.cfg</i>. This file should have the following structure:
        <blockquote>
        hostname HOSTNAME<br />
        port PORT<br />
        database DATABASE<br />
        table TABLE<br />
        username USERNAME<br />
        password PASSWORD<br />
        </blockquote>        
        </p>

        <h2>MySQL Setup</h2>
        <p>In order for <i>Battlelog Cache</i> to work, it needs to be connected to a MySQL-Database. The needed CREATE-script for the table can either be found in the plugin's zip-File or manually downloaded at <a href='http://morpheusx.at/procon/battlelogcache/battlelogcache_table_create.sql'><b>this page</b></a>.<br />
        After the table has been created, no further setup will be required to run <i>Battlelog Cache</i>.</p>

        <h3>Changelog</h3>
        <blockquote><h4>1.0.0.0 (18.12.2012)</h4>
            - initial (public) version<br/>
        </blockquote>
        ";
        }

        #endregion

        #region Plugin settings

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Debugging|Debug level", typeof(int), debugLevel));

            if (mySqlPromptDetails)
            {
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Hostname", typeof(string), mySqlHostname));
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Port", typeof(string), mySqlPort));
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Database", typeof(string), mySqlDatabase));
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Table", typeof(string), mySqlTable));
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Username", typeof(string), mySqlUsername));
                lstReturn.Add(new CPluginVariable("MySQL Settings|MySQL Password", typeof(string), mySqlPassword));
            }

            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            if (Regex.Match(strVariable, @"Debug level").Success)
            {
                int tmp = 2;
                int.TryParse(strValue, out tmp);
                debugLevel = tmp;
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Hostname").Success)
            {
                mySqlHostname = strValue;
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Port").Success)
            {
                int tmp = 3306;
                int.TryParse(strValue, out tmp);
                if (tmp > 0 && tmp < 65536)
                {
                    mySqlPort = strValue;
                }
                else
                {
                    ConsoleException("Invalid value for MySQL Port: '" + strValue + "'. Must be number between 1 and 65535!");
                }
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Database").Success)
            {
                mySqlDatabase = strValue;
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Table").Success)
            {
                mySqlTable = strValue;
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Username").Success)
            {
                mySqlUsername = strValue;
            }
            else if (mySqlPromptDetails && Regex.Match(strVariable, @"MySQL Password").Success)
            {
                mySqlPassword = strValue;
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            String configFile = Path.Combine(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Configs"), "gsp_settings"),"battlelogcache_mysql.cfg");

            if (File.Exists(configFile))
            {
                DebugWrite("Reading MySQL-Details from config-file...", 4);

                StreamReader configFileReader = new StreamReader(configFile);
                String line = String.Empty;

                while ((line = configFileReader.ReadLine()) != null)
                {
                    if (line.Contains("hostname"))
                    {
                        mySqlHostname = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.Contains("port"))
                    {
                        mySqlPort = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.Contains("database"))
                    {
                        mySqlDatabase = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.Contains("table"))
                    {
                        mySqlTable = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.Contains("username"))
                    {
                        mySqlUsername = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.Contains("password"))
                    {
                        mySqlPassword = line.Substring(line.IndexOf(' ') + 1);
                    }
                }

                mySqlPromptDetails = false;
            }
            else
            {
                mySqlPromptDetails = true;
            }

            this.RegisterEvents(this.GetType().Name, "OnServerInfo", "OnListPlayers", "OnPlayerLeft");
        }

        public void OnPluginEnable()
        {
            isEnabled = true;

            // register a command do indicate availibility to other plugins
            this.RegisterCommand(matchCommandLookupRequest);

            lookupRequests = new List<LookupRequest>();
            lookupResponses = new List<LookupResponse>();

            InitializeThreads();

            requestLoopThread.Start();
            responseLoopThread.Start();

            ConsoleWrite("^b^2Enabled!^n^0 Version: " + GetPluginVersion());
        }

        public void OnPluginDisable()
        {
            isEnabled = false;
            // deregister the command again to remove availibility-indicator
            this.UnregisterCommand(matchCommandLookupRequest);

            requestLoopHandle.Set();

            ConsoleWrite("^b^1Disabled! =(^n^0");
        }

        #endregion

        #region Procon events

        public override void OnServerInfo(CServerInfo serverInfo)
        {
            // TODO: Check if database holds old stats?
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            // TODO: Check if database holds old stats?
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            // TODO: Check if database holds old stats?
        }

        #endregion

        private void AddRequest(Object jsonRequest)
        {
            DebugWrite("AddRequest starting!", 6);

            Thread.CurrentThread.Name = "AddRequest";

            Hashtable parsedRequest = (Hashtable)JSON.JsonDecode((String)jsonRequest);
            String pluginName = String.Empty;
            String pluginMethod = String.Empty;
            String playerName = String.Empty;
            RequestTypeEnum requestType = RequestTypeEnum.ClanTag;

            if (!parsedRequest.ContainsKey("pluginName"))
            {
                ConsoleError("Parsed commands didn't contain a pluginName!");
                return;
            }
            else
            {
                pluginName = (String)parsedRequest["pluginName"];
            }

            if (!parsedRequest.ContainsKey("pluginMethod"))
            {
                ConsoleError("Parsed commands didn't contain a pluginMethod!");
                return;
            }
            else
            {
                pluginMethod = (String)parsedRequest["pluginMethod"];
            }

            if (!parsedRequest.ContainsKey("playerName"))
            {
                ConsoleError("Parsed commands didn't contain a playerName!");
                return;
            }
            else
            {
                playerName = (String)parsedRequest["playerName"];
            }

            if (!parsedRequest.ContainsKey("requestType"))
            {
                ConsoleError("Parsed commands didn't contain a requestType!");
                return;
            }
            else
            {
                switch (parsedRequest["requestType"].ToString().ToLower())
                {
                    case "overview":
                        requestType = RequestTypeEnum.Overview;
                        break;
                    case "weapon":
                        requestType = RequestTypeEnum.Weapon;
                        break;
                    case "vehicle":
                        requestType = RequestTypeEnum.Vehicle;
                        break;
                    case "clantag":
                        requestType = RequestTypeEnum.ClanTag;
                        break;
                    default:
                        ConsoleWarn("The parsed requestType '" + parsedRequest["requestType"].ToString() + "' was not recognized. Using default type 'clanTag'");
                        break;
                }
            }

            LookupRequest request = new LookupRequest(pluginName, pluginMethod, playerName, requestType);

            DebugWrite("AddRequest locking lookupRequests (adding new request)", 6);
            lock (lookupRequestLock)
            {
                lookupRequests.Add(request);
            }
            DebugWrite("AddRequest releasing lookupRequests (adding new request)", 6);

            requestLoopHandle.Set();

            DebugWrite("AddRequest finished!", 6);
        }

        private void RequestLoop()
        {
            DebugWrite("RequestLoop starting!", 6);

            DateTime startTime = DateTime.Now;
            double minWaitTime = 2.0D;
            List<LookupRequest> requests = new List<LookupRequest>(); ;

            while (true)
            {
                try
                {
                    if (RequestQueueCount() == 0)
                    {
                        requestLoopHandle.Reset();
                        DebugWrite("RequestQueue empty, waiting...", 6);
                        requestLoopHandle.WaitOne();
                    }

                    if (!isEnabled)
                    {
                        DebugWrite("Plugin disabled, RequestLoop aborting...", 6);
                        break;
                    }

                    DebugWrite("RequestLoop active...", 6);

                    requests.Clear();
                    DebugWrite("RequestLoop locking lookupRequests (creating local copy)", 6);
                    lock (lookupRequestLock)
                    {
                        requests = new List<LookupRequest>(lookupRequests);
                    }
                    DebugWrite("RequestLoop releasing lookupRequests (creating local copy)", 6);

                    DebugWrite(lookupRequests.Count + " lookupRequests in queue...", 5);
                    for (int i = 0; i < requests.Count; i++)
                    {
                        DebugWrite("Performing lookupRequest #" + i + "...", 5);

                        String jsonData = PerformLookup(requests[i].PlayerName, requests[i].RequestType, startTime, minWaitTime);

                        startTime = DateTime.Now;

                        if (!isEnabled)
                        {
                            DebugWrite("Plugin disabled, RequestLoop aborting...", 6);
                            return;
                        }

                        LookupResponse response = new LookupResponse(requests[i].PluginName, requests[i].PluginMethod, requests[i].PlayerName, jsonData);

                        DebugWrite("RequestLoop locking lookupResponses (adding new response)", 6);
                        lock (lookupResponseLock)
                        {
                            lookupResponses.Add(response);
                        }
                        DebugWrite("RequestLoop releasing lookupResponses (adding new response)", 6);

                        DebugWrite("RequestLoop locking lookupRequests (deleting old request)", 6);
                        lock (lookupRequestLock)
                        {
                            for (int j = 0; j < lookupRequests.Count; j++)
                            {
                                if (lookupRequests[j].Equals(requests[i]))
                                {
                                    lookupRequests.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                        DebugWrite("RequestLoop releasing lookupRequests (deleting old request)", 6);

                        //requests.Remove(requests[i]);
                        responseLoopHandle.Set();

                        DebugWrite("Finished lookupRequest #" + i + "...", 5);
                    }
                    DebugWrite("Finished all lookupRequests...", 5);
                }
                catch (Exception e)
                {
                    if (typeof(ThreadAbortException).Equals(e.GetType()))
                    {
                        Thread.ResetAbort();
                        return;
                    }

                    DebugWrite("Exception in RequestLoop! " + e.ToString(), 3);
                }
            }

            responseLoopHandle.Set();
            DebugWrite("RequestLoop finished!", 6);
        }

        private void ResponseLoop()
        {
            DebugWrite("ResponseLoop starting!", 6);

            List<LookupResponse> responses = new List<LookupResponse>();

            while (true)
            {
                try
                {
                    if (ResponseQueueCount() == 0)
                    {
                        responseLoopHandle.Reset();
                        DebugWrite("ResponseQueue empty, waiting...", 6);
                        responseLoopHandle.WaitOne();
                    }

                    if (!isEnabled)
                    {
                        DebugWrite("Plugin disabled, ResponseLoop aborting...", 6);
                        break;
                    }

                    DebugWrite("ResponseLoop active...", 6);

                    responses.Clear();
                    DebugWrite("ResponseLoop locking lookupResponses (creating local copy)", 6);
                    lock (lookupResponseLock)
                    {
                        responses = new List<LookupResponse>(lookupResponses);
                    }
                    DebugWrite("ResponseLoop releasing lookupResponses (creating local copy)", 6);

                    DebugWrite(responses.Count + " lookupResponses in queue...", 5);
                    for (int i = 0; i < responses.Count; i++)
                    {
                        DebugWrite("Performing lookupResponse #" + i + "...", 5);

                        this.ExecuteCommand("procon.protected.plugins.call", responses[i].PluginName, responses[i].PluginMethod, responses[i].PlayerName, responses[i].JsonData);

                        DebugWrite("ResponseLoop locking lookupResponses (removing old response)", 6);
                        lock (lookupResponseLock)
                        {
                            for (int j = 0; j < lookupResponses.Count; j++)
                            {
                                if (lookupResponses[j].Equals(responses[i]))
                                {
                                    lookupResponses.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                        DebugWrite("ResponseLoop releasing lookupResponses (removing old response)", 6);

                        //responses.Remove(responses[i]);

                        DebugWrite("Finished lookupResponse #" + i + "...", 5);
                    }
                    DebugWrite("Finished all lookupResponses...", 5);
                }
                catch (Exception e)
                {
                    if (typeof(ThreadAbortException).Equals(e.GetType()))
                    {
                        Thread.ResetAbort();
                        return;
                    }

                    DebugWrite("Exception in ResponseLoop!" + e.ToString(), 3);
                }
            } 

            DebugWrite("ResponseLoop finished!", 6);
        }

        private String PerformLookup(String name, RequestTypeEnum requestType, DateTime startTime, double minWaitTime)
        {
            DebugWrite("Performing lookup for player '" + name + "' with requestType '" + requestType + "'...", 4);

            PlayerStats stats = MySqlLookup(name, requestType);

            if (!isEnabled)
            {
                DebugWrite("Plugin disabled, PerformLookup aborting...", 6);
                return String.Empty;
            }

            if (!stats.ValidStats(requestType))
            {
                /*
                Don't fetch stats if there is a persisted error for the request type
                */
                String messageValue = null;
                switch (requestType)
                {
                    case RequestTypeEnum.Overview:
                        if (stats.OverviewStatsError != String.Empty)
                        {
                            messageValue = stats.OverviewStatsError;
                        }
                        break;
                    case RequestTypeEnum.Weapon:
                        if (stats.WeaponStatsError != String.Empty)
                        {
                            messageValue = stats.WeaponStatsError;
                        }
                        break;
                    case RequestTypeEnum.Vehicle:
                        if (stats.VehicleStatsError != String.Empty)
                        {
                            messageValue = stats.VehicleStatsError;
                        }
                        break;
                    case RequestTypeEnum.ClanTag:
                    default:
                        break;
                }
                
                if (messageValue != null)
                {
                    DebugWrite("No valid stats for player '" + name + "' with requestType '" + requestType + "'", 2);
                    DebugWrite("    due to cached error: " + messageValue, 3);
                    Hashtable response = new Hashtable();
                    response["type"] = "Error";
                    response["message"] = messageValue;
                    return JSON.JsonEncode(response);
                }
                
                DebugWrite("No valid stats for player '" + name + "' with requestType '" + requestType + "' found in MySQL-database. Fetching stats...", 5);

                TimeSpan timeElapsed = DateTime.Now.Subtract(startTime);

                if (timeElapsed.TotalSeconds < minWaitTime)
                {
                    DebugWrite("Waiting for " + (minWaitTime - timeElapsed.TotalSeconds) + " seconds to avoid Battlelog throtteling...", 5);
                    Thread.Sleep((int)(minWaitTime - timeElapsed.TotalSeconds) * 1000);
                }

                try
                {
                    stats = BattlelogLookup(stats, requestType);
                }
                catch (Exception e)
                {
                    DebugWrite("BattlelogLookup for player '" + name + "' with requestType '" + requestType + "' failed!", 2);
                    DebugWrite("    EXCEPTION: " + e.ToString(), 3);
                    Hashtable response = new Hashtable();
                    response["type"] = "Error";
                    response["message"] = e.Message;
                    return JSON.JsonEncode(response);
                    // No insert into database
                }
            }
            else
            {
                return stats.GetStats(requestType);
            }

            if (!isEnabled)
            {
                DebugWrite("Plugin disabled, PerformLookup aborting...", 6);
                return String.Empty;
            }

            Object[] insertData = new Object[2];
            insertData[0] = stats;
            insertData[1] = requestType;
            new Thread(new ParameterizedThreadStart(MySqlInsert)).Start(insertData);

            if (stats.ValidStats(requestType))
            {
                return stats.GetStats(requestType);
            }
            else
            {
                DebugWrite("Fetching stats for player '" + name + "' with requestType '" + requestType + "' failed!", 2);
                DebugWrite("    General Status: " + stats.GeneralStatus, 3);
                Hashtable response = new Hashtable();
                response["type"] = "Error";
                response["message"] = stats.GeneralStatus;
                return JSON.JsonEncode(response);
            }
        }

        private PlayerStats MySqlLookup(String name, RequestTypeEnum requestType)
        {
            DebugWrite("MySqlLookup starting!", 6);

            PlayerStats stats = new PlayerStats(String.Empty, name);
            stats.FetchTime = 0.0D;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareMySqlConnectionString()))
                {
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        //command.CommandText = "SELECT * FROM `" + mySqlDatabase + "`.`" + mySqlTable + "` WHERE `playerName` = '" + name + "' AND ";

                        switch (requestType)
                        {
                            case RequestTypeEnum.Overview:
                                command.CommandText = "SELECT `personaId`, `playerName`, `generalStatus`, `overviewStats`, `overviewStatsError`, `timestampOverview` FROM `" + mySqlDatabase + "`.`" + mySqlTable + "` WHERE `playerName` = '" + name + "' AND DATEDIFF(CURRENT_TIMESTAMP, `timestampOverview`) = 0;";
                                break;
                            case RequestTypeEnum.Weapon:
                                command.CommandText = "SELECT `personaId`, `playerName`, `generalStatus`, `weaponStats`, `weaponStatsError`, `timestampWeapon` FROM `" + mySqlDatabase + "`.`" + mySqlTable + "` WHERE `playerName` = '" + name + "' AND DATEDIFF(CURRENT_TIMESTAMP, `timestampWeapon`) = 0;";
                                break;
                            case RequestTypeEnum.Vehicle:
                                command.CommandText = "SELECT `personaId`, `playerName`, `generalStatus`, `vehicleStats`, `vehicleStatsError`, `timestampVehicle` FROM `" + mySqlDatabase + "`.`" + mySqlTable + "` WHERE `playerName` = '" + name + "' AND DATEDIFF(CURRENT_TIMESTAMP, `timestampVehicle`) = 0;";
                                break;
                            case RequestTypeEnum.ClanTag:
                                command.CommandText = "SELECT `personaId`, `playerName`, `generalStatus`, `clanTag`, `timestampClanTag` FROM `" + mySqlDatabase + "`.`" + mySqlTable + "` WHERE `playerName` = '" + name + "' AND DATEDIFF(CURRENT_TIMESTAMP, `timestampClanTag`) = 0;";
                                break;
                        }

                        connection.Open();

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DebugWrite("MySqlLookup found stats for player " + stats.PlayerName + "!", 6);
                                stats.PersonaId = reader.GetString("personaId");

                                if (!reader.IsDBNull(2))
                                {
                                    stats.GeneralStatus = reader.GetString("generalStatus");
                                }

                                switch (requestType)
                                {
                                    case RequestTypeEnum.Overview:
                                        if (!reader.IsDBNull(3))
                                        {
                                            stats.OverviewStats = reader.GetString("overviewStats");
                                        }
                                        if (!reader.IsDBNull(4))
                                        {
                                            stats.OverviewStatsError = reader.GetString("overviewStatsError");
                                        }
                                        if (!reader.IsDBNull(5))
                                        {
                                            DateTime timeStamp = reader.GetDateTime("timestampOverview");
                                            stats.Age = DateTime.Now.Subtract(timeStamp).TotalSeconds;
                                        }
                                        break;
                                    case RequestTypeEnum.Weapon:
                                        if (!reader.IsDBNull(3))
                                        {
                                            stats.WeaponStats = reader.GetString("weaponStats");
                                        }
                                        if (!reader.IsDBNull(4))
                                        {
                                            stats.OverviewStatsError = reader.GetString("weaponStatsError");
                                        }
                                        if (!reader.IsDBNull(5))
                                        {
                                            DateTime timeStamp = reader.GetDateTime("timestampWeapon");
                                            stats.Age = DateTime.Now.Subtract(timeStamp).TotalSeconds;
                                        }
                                        break;
                                    case RequestTypeEnum.Vehicle:
                                        if (!reader.IsDBNull(3))
                                        {
                                            stats.VehicleStats = reader.GetString("vehicleStats");
                                        }
                                        if (!reader.IsDBNull(4))
                                        {
                                            stats.OverviewStatsError = reader.GetString("vehicleStatsError");
                                        }
                                        if (!reader.IsDBNull(5))
                                        {
                                            DateTime timeStamp = reader.GetDateTime("timestampVehicle");
                                            stats.Age = DateTime.Now.Subtract(timeStamp).TotalSeconds;
                                        }
                                        break;
                                    case RequestTypeEnum.ClanTag:
                                        if (!reader.IsDBNull(3))
                                        {
                                            stats.ClanTag = reader.GetString("clanTag");
                                        }
                                        if (!reader.IsDBNull(4))
                                        {
                                            DateTime timeStamp = reader.GetDateTime("timestampClanTag");
                                            stats.Age = DateTime.Now.Subtract(timeStamp).TotalSeconds;
                                        }
                                        break;
                                    default:
                                        // Whud? o.O
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugWrite(e.ToString(), 3);
            }

            DebugWrite("MySqlLookup finished!", 6);
            return stats;
        }

        private PlayerStats BattlelogLookup(PlayerStats stats, RequestTypeEnum requestType)
        {
            String name = stats.PlayerName;
            String result = String.Empty;
            String webClientResponse = String.Empty;

            WebClient webClient = new WebClient();
            String userAgent = "Mozilla/5.0 (compatible; Procon 1; CBattlelogCache)";
            webClient.Headers.Add("user-agent", userAgent);

            DebugWrite("BattlelogLookup created WebClient...", 6);

            if (stats.GeneralStatus.Contains("Player not found"))
            {
                stats.GeneralStatus = "Player not found, additional BattlelogLookup aborted!";
                return stats;
            }

            stats.Age = 0.0D;
            DateTime startTime = DateTime.Now;

            if (String.IsNullOrEmpty(stats.PersonaId) || requestType == RequestTypeEnum.ClanTag)
            {
                try
                {
                    DebugWrite("No personaId for player or requestType was clanTag...", 6);

                    webClientResponse = DownloadWebPage(webClient, "http://battlelog.battlefield.com/bf3/user/" + name, ref result);
                    if (webClientResponse != String.Empty)
                    {
                        throw new BattlelogLookupException("DownloadWebPage(New Persona or ClanTag) failed! " + webClientResponse);
                    }

                    if (!isEnabled)
                    {
                        DebugWrite("Plugin disabled, BattlelogLookup aborting...", 6);
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        return stats;
                    }

                    MatchCollection pid = Regex.Matches(result, @"bf3/soldier/" + name + @"/stats/(\d+)(['""]|/\s*['""]|/[^/'""]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    String personaId = String.Empty;

                    DebugWrite("Extracting personaId...", 6);
                    foreach (Match match in pid)
                    {
                        Match nonPC = Regex.Match(match.Groups[2].Value.Trim(), @"(ps3|xbox)", RegexOptions.IgnoreCase);
                        if (match.Success && !nonPC.Success)
                        {
                            personaId = match.Groups[1].Value.Trim();
                            break;
                        }
                        else if (nonPC.Success)
                        {
                            DebugWrite("Ignoring non-PC (" + match.Groups[2].Value.Trim() + ") personaId of " + match.Groups[1].Value.Trim(), 4);
                        }
                    }
                    stats.PersonaId = personaId;

                    if (stats.PersonaId.Length == 0)
                    {
                        DebugWrite("Could not find personaId for player '" + stats.PlayerName + "'", 5);
                        stats.GeneralStatus = "Could not find personaId for player '" + stats.PlayerName + "'";
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        return stats;
                    }

                    Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + name, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    DebugWrite("Extracting clanTag...", 6);
                    if (tag.Success)
                    {
                        DebugWrite("clanTag is '" + tag.Groups[1].Value + "'", 6);
                        stats.ClanTag = tag.Groups[1].Value;
                    }
                    else
                    {
                        stats.ClanTag = String.Empty;
                        DebugWrite("clanTag is empty", 6);
                    }
                }
                catch (Exception e)
                {
                    DebugWrite("Exception while grabbing personaId and clanTag: " + e.ToString(), 5);
                    stats.GeneralStatus = e.Message;
                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    return stats;
                }
            }

            DebugWrite("Performing requestType-lookup for personaId " + stats.PersonaId + "...", 6);

            Hashtable response = null;
            Hashtable responseData = null;
            Hashtable shrinkedResponse = null;
            Hashtable shrinkedResponseData = null;

            switch (requestType)
            {
                case RequestTypeEnum.ClanTag:
                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    DebugWrite("BattlelogLookup returning clanTag...", 6);
                    return stats;
                case RequestTypeEnum.Overview:
                    webClientResponse = DownloadWebPage(webClient, "http://battlelog.battlefield.com/bf3/overviewPopulateStats/" + stats.PersonaId + "/bf3-us-engineer/1", ref result);

                    if (webClientResponse != String.Empty)
                    {
                        throw new BattlelogLookupException("DownloadWebPage(Overview) failed! " + webClientResponse);
                    }

                    response = (Hashtable)JSON.JsonDecode(result);
                    shrinkedResponse = new Hashtable();
                    shrinkedResponse["type"] = response["type"];
                    shrinkedResponse["message"] = response["message"];
                    shrinkedResponseData = new Hashtable();
                    responseData = (Hashtable)response["data"];

                    if (responseData == null)
                    {
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        stats.GeneralStatus = "BattlelogLookup(Overview) failed!";
                        stats.OverviewStatsError = "JSON missing 'data'";
                        DebugWrite(stats.GeneralStatus + " " + stats.OverviewStatsError, 5);
                        return stats;
                    }

                    Hashtable responseDataOverviewStats = (Hashtable)responseData["overviewStats"];

                    if (responseDataOverviewStats == null)
                    {
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        stats.GeneralStatus = "BattlelogLookup(Overview) failed!";
                        stats.OverviewStatsError = "JSON missing 'overviewStats'";
                        DebugWrite(stats.GeneralStatus + " " + stats.OverviewStatsError, 5);
                        return stats;
                    }

                    Hashtable shrinkedResponseDataOverviewStats = new Hashtable();

                    shrinkedResponseDataOverviewStats["vehiclesDestroyed"] = responseDataOverviewStats["vehiclesDestroyed"];
                    shrinkedResponseDataOverviewStats["deaths"] = responseDataOverviewStats["deaths"];
                    shrinkedResponseDataOverviewStats["revives"] = responseDataOverviewStats["revives"];
                    shrinkedResponseDataOverviewStats["killAssists"] = responseDataOverviewStats["killAssists"];
                    shrinkedResponseDataOverviewStats["sc_vehicle"] = responseDataOverviewStats["sc_vehicle"];
                    shrinkedResponseDataOverviewStats["sc_team"] = responseDataOverviewStats["sc_team"];
                    shrinkedResponseDataOverviewStats["totalScore"] = responseDataOverviewStats["totalScore"];
                    shrinkedResponseDataOverviewStats["heals"] = responseDataOverviewStats["heals"];
                    shrinkedResponseDataOverviewStats["longestHeadshot"] = responseDataOverviewStats["longestHeadshot"];
                    shrinkedResponseDataOverviewStats["headshots"] = responseDataOverviewStats["headshots"];
                    shrinkedResponseDataOverviewStats["repairs"] = responseDataOverviewStats["repairs"];
                    shrinkedResponseDataOverviewStats["shotsFired"] = responseDataOverviewStats["shotsFired"];
                    shrinkedResponseDataOverviewStats["elo"] = responseDataOverviewStats["elo"];
                    shrinkedResponseDataOverviewStats["longestWinStreak"] = responseDataOverviewStats["longestWinStreak"];
                    shrinkedResponseDataOverviewStats["resupplies"] = responseDataOverviewStats["resupplies"];
                    shrinkedResponseDataOverviewStats["sc_unlock"] = responseDataOverviewStats["sc_unlock"];
                    shrinkedResponseDataOverviewStats["rsNumWins"] = responseDataOverviewStats["rsNumWins"];
                    shrinkedResponseDataOverviewStats["wlRatio"] = responseDataOverviewStats["wlRatio"];
                    shrinkedResponseDataOverviewStats["sc_award"] = responseDataOverviewStats["sc_award"];
                    shrinkedResponseDataOverviewStats["score"] = responseDataOverviewStats["score"];
                    shrinkedResponseDataOverviewStats["vehiclesDestroyedAssists"] = responseDataOverviewStats["vehiclesDestroyedAssists"];
                    shrinkedResponseDataOverviewStats["kitTimes"] = responseDataOverviewStats["kitTimes"];
                    shrinkedResponseDataOverviewStats["kdRatio"] = responseDataOverviewStats["kdRatio"];
                    shrinkedResponseDataOverviewStats["kitTimesInPercentage"] = responseDataOverviewStats["kitTimesInPercentage"];
                    shrinkedResponseDataOverviewStats["timePlayed"] = responseDataOverviewStats["timePlayed"];
                    shrinkedResponseDataOverviewStats["rank"] = responseDataOverviewStats["rank"];
                    shrinkedResponseDataOverviewStats["kitScores"] = responseDataOverviewStats["kitScores"];
                    shrinkedResponseDataOverviewStats["kills"] = responseDataOverviewStats["kills"];
                    shrinkedResponseDataOverviewStats["rsScorePerMinute"] = responseDataOverviewStats["rsScorePerMinute"];
                    shrinkedResponseDataOverviewStats["rsDeaths"] = responseDataOverviewStats["rsDeaths"];
                    shrinkedResponseDataOverviewStats["sc_bonus"] = responseDataOverviewStats["sc_bonus"];
                    shrinkedResponseDataOverviewStats["rsNumLosses"] = responseDataOverviewStats["rsNumLosses"];
                    shrinkedResponseDataOverviewStats["scorePerMinute"] = responseDataOverviewStats["scorePerMinute"];
                    shrinkedResponseDataOverviewStats["timePlayedSinceLastReset"] = responseDataOverviewStats["timePlayedSinceLastReset"];
                    shrinkedResponseDataOverviewStats["numWins"] = responseDataOverviewStats["numWins"];
                    shrinkedResponseDataOverviewStats["combatScore"] = responseDataOverviewStats["combatScore"];
                    shrinkedResponseDataOverviewStats["rsScore"] = responseDataOverviewStats["rsScore"];
                    shrinkedResponseDataOverviewStats["meleeKills"] = responseDataOverviewStats["meleeKills"];
                    shrinkedResponseDataOverviewStats["numRounds"] = responseDataOverviewStats["numRounds"];
                    shrinkedResponseDataOverviewStats["rsKills"] = responseDataOverviewStats["rsKills"];
                    shrinkedResponseDataOverviewStats["sc_objective"] = responseDataOverviewStats["sc_objective"];
                    shrinkedResponseDataOverviewStats["rsShotsHit"] = responseDataOverviewStats["rsShotsHit"];
                    shrinkedResponseDataOverviewStats["rsTimePlayed"] = responseDataOverviewStats["rsTimePlayed"];
                    shrinkedResponseDataOverviewStats["lastReset"] = responseDataOverviewStats["lastReset"];
                    shrinkedResponseDataOverviewStats["rsShotsFired"] = responseDataOverviewStats["rsShotsFired"];
                    shrinkedResponseDataOverviewStats["killStreakBonus"] = responseDataOverviewStats["killStreakBonus"];
                    shrinkedResponseDataOverviewStats["shotsHit"] = responseDataOverviewStats["shotsHit"];
                    shrinkedResponseDataOverviewStats["quitPercentage"] = responseDataOverviewStats["quitPercentage"];
                    shrinkedResponseDataOverviewStats["numLosses"] = responseDataOverviewStats["numLosses"];
                    shrinkedResponseDataOverviewStats["sc_squad"] = responseDataOverviewStats["sc_squad"];
                    shrinkedResponseDataOverviewStats["sc_general"] = responseDataOverviewStats["sc_general"];
                    shrinkedResponseDataOverviewStats["accuracy"] = responseDataOverviewStats["accuracy"];
                    shrinkedResponseDataOverviewStats["maxScoreInRound"] = responseDataOverviewStats["maxScoreInRound"];

                    Hashtable responseDataKitMap = (Hashtable)((Hashtable)response["data"])["kitMap"];
                    Hashtable shrinkedResponseDataKitMap = new Hashtable();

                    foreach (DictionaryEntry kitMap in responseDataKitMap)
                    {
                        shrinkedResponseDataKitMap[kitMap.Key] = kitMap.Value;
                    }

                    shrinkedResponseData["overviewStats"] = shrinkedResponseDataOverviewStats;
                    shrinkedResponseData["kitMap"] = shrinkedResponseDataKitMap;
                    shrinkedResponse["data"] = shrinkedResponseData;

                    stats.OverviewStats = JSON.JsonEncode(shrinkedResponse);

                    /* Moved up to immediately after download call
                    if (webClientResponse != String.Empty)
                    {
                        DebugWrite("WebClientResponse in BattlelogLookup was not empty: " + webClientResponse, 5);
                        stats.GeneralStatus = "overviewStats failed";
                        stats.OverviewStatsError = webClientResponse;
                    }
                    */

                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    DebugWrite("BattlelogLookup returning overviewStats...", 6);
                    return stats;
                case RequestTypeEnum.Weapon:
                    webClientResponse = DownloadWebPage(webClient, "http://battlelog.battlefield.com/bf3/weaponsPopulateStats/" + stats.PersonaId + "/1", ref result);

                    if (webClientResponse != String.Empty)
                    {
                        throw new BattlelogLookupException("DownloadWebPage(Weapon) failed! " + webClientResponse);
                    }

                    response = (Hashtable)JSON.JsonDecode(result);
                    shrinkedResponse = new Hashtable();
                    shrinkedResponse["type"] = response["type"];
                    shrinkedResponse["message"] = response["message"];
                    shrinkedResponseData = new Hashtable();
                    responseData = (Hashtable)response["data"];

                    if (responseData == null)
                    {
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        stats.GeneralStatus = "BattlelogLookup(Weapon) failed!";
                        stats.WeaponStatsError = "JSON missing 'data'";
                        DebugWrite(stats.GeneralStatus + " " + stats.WeaponStatsError, 5);
                        return stats;
                    }
                    
                    ArrayList responseDataMainWeaponStats = (ArrayList)responseData["mainWeaponStats"];
                    
                    if (responseDataMainWeaponStats == null)
                    {
                        stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                        stats.GeneralStatus = "BattlelogLookup(Weapon) failed!";
                        stats.WeaponStatsError = "JSON missing 'mainWeaponStats'";
                        DebugWrite(stats.GeneralStatus + " " + stats.WeaponStatsError, 5);
                        return stats;
                    }

                    ArrayList shrinkedResponseDataMainWeaponStats = new ArrayList();

                    foreach (Hashtable weaponStats in responseDataMainWeaponStats)
                    {
                        Hashtable shrinkedWeaponStats = new Hashtable();
                        shrinkedWeaponStats["code"] = weaponStats["code"];
                        shrinkedWeaponStats["category"] = weaponStats["category"];
                        shrinkedWeaponStats["shotsFired"] = weaponStats["shotsFired"];
                        shrinkedWeaponStats["accuracy"] = weaponStats["accuracy"];
                        shrinkedWeaponStats["headshots"] = weaponStats["headshots"];
                        shrinkedWeaponStats["kills"] = weaponStats["kills"];
                        shrinkedWeaponStats["slug"] = weaponStats["slug"];
                        shrinkedWeaponStats["timeEquipped"] = weaponStats["timeEquipped"];
                        shrinkedWeaponStats["name"] = weaponStats["name"];
                        shrinkedWeaponStats["shotsHit"] = weaponStats["shotsHit"];
                        shrinkedResponseDataMainWeaponStats.Add(shrinkedWeaponStats);
                    }

                    shrinkedResponseData["mainWeaponStats"] = shrinkedResponseDataMainWeaponStats;
                    shrinkedResponse["data"] = shrinkedResponseData;

                    stats.WeaponStats = JSON.JsonEncode(shrinkedResponse);

                    /* Moved up to immediately after download call
                    if (webClientResponse != String.Empty)
                    {
                        DebugWrite("WebClientResponse in BattlelogLookup was not empty: " + webClientResponse, 5);
                        stats.GeneralStatus = "weaponStats failed";
                        stats.WeaponStatsError = webClientResponse;
                    }
                    */

                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    DebugWrite("BattlelogLookup returning weaponStats...", 6);
                    return stats;
                case RequestTypeEnum.Vehicle:
                    webClientResponse = DownloadWebPage(webClient, "http://battlelog.battlefield.com/bf3/vehiclesPopulateStats/" + stats.PersonaId + "/1", ref result);
                    
                    if (webClientResponse != String.Empty)
                    {
                        throw new BattlelogLookupException("DownloadWebPage(Vehicle) failed! " + webClientResponse);
                    }

                    stats.VehicleStats = result;

                    /* Moved up to immediately after download call
                    if (webClientResponse != String.Empty)
                    {
                        DebugWrite("WebClientResponse in BattlelogLookup was not empty: " + webClientResponse, 5);
                        stats.GeneralStatus = "vehicleStats failed";
                        stats.VehicleStatsError = webClientResponse;
                    }
                    */

                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    DebugWrite("BattlelogLookup returning vehicleStats...", 6);
                    return stats;
                default:
                    stats.GeneralStatus = "Unknown requestType";
                    stats.FetchTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                    return stats;
            }
        }

        private void MySqlInsert(object insertData)
        {
            DebugWrite("MySqlInsert starting!", 6);

            PlayerStats stats = (PlayerStats)((Object[])insertData)[0];
            RequestTypeEnum requestType = (RequestTypeEnum)((Object[])insertData)[1];

            Thread.CurrentThread.Name = "MySqlInsert";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareMySqlConnectionString()))
                {
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO `" + mySqlDatabase + "`.`" + mySqlTable + "` (`personaId`, `playerName`, `";

                        switch (requestType)
                        {
                            case RequestTypeEnum.ClanTag:
                                command.CommandText = command.CommandText + "clanTag`, `generalStatus`, `timestampClanTag`) VALUES (@personaId, @playerName, @clanTag, @generalStatus, @timestamp) ON DUPLICATE KEY UPDATE `clanTag` = @clanTag, `generalStatus` = @generalStatus, `timestampClanTag` = @timestamp;";
                                command.Parameters.AddWithValue("@personaId", stats.PersonaId);
                                command.Parameters.AddWithValue("@playerName", stats.PlayerName);
                                if (stats.ClanTag.CompareTo("@NotAClanTag@") == 0)
                                {
                                    command.Parameters.AddWithValue("@clanTag", DBNull.Value);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@clanTag", stats.ClanTag);
                                }
                                if (stats.GeneralStatus == String.Empty)
                                {
                                    command.Parameters.AddWithValue("generalStatus", "Success");
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("generalStatus", stats.GeneralStatus);
                                }
                                command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                                break;
                            case RequestTypeEnum.Overview:
                                if (stats.ClanTag == String.Empty)
                                {
                                    command.CommandText = command.CommandText + "overviewStats`, `generalStatus`, `overviewStatsError`, `timestampOverview`) VALUES (@personaId, @playerName, @overviewStats, @generalStatus, @overviewStatsError, @timestamp) ON DUPLICATE KEY UPDATE `overviewStats` = @overviewStats, `generalStatus` = @generalStatus, `overviewStatsError` = @overviewStatsError, `timestampOverview` = @timestamp;";
                                }
                                else
                                {
                                    command.CommandText = command.CommandText + "clanTag`, `overviewStats`, `generalStatus`, `overviewStatsError`, `timestampOverview`, `timestampClanTag`) VALUES (@personaId, @playerName, @clanTag, @overviewStats, @generalStatus, @overviewStatsError, @timestamp, @timestamp) ON DUPLICATE KEY UPDATE `clanTag` = @clanTag, `overviewStats` = @overviewStats, `generalStatus` = @generalStatus, `overviewStatsError` = @overviewStatsError, `timestampOverview` = @timestamp, `timestampClanTag` = @timestamp;";
                                    if (stats.ClanTag.CompareTo("@NotAClanTag@") == 0)
                                    {
                                        command.Parameters.AddWithValue("@clanTag", DBNull.Value);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@clanTag", stats.ClanTag);
                                    }
                                }
                                command.Parameters.AddWithValue("@personaId", stats.PersonaId);
                                command.Parameters.AddWithValue("@playerName", stats.PlayerName);
                                command.Parameters.AddWithValue("@overviewStats", stats.OverviewStats);
                                if (stats.GeneralStatus == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@generalStatus", "Success");
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@generalStatus", stats.GeneralStatus);
                                }
                                if (stats.OverviewStatsError == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@overviewStatsError", DBNull.Value);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@overviewStatsError", stats.OverviewStatsError);
                                }
                                command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                                break;
                            case RequestTypeEnum.Weapon:
                                if (stats.ClanTag == String.Empty)
                                {
                                    command.CommandText = command.CommandText + "weaponStats`, `generalStatus`, `weaponStatsError`, `timestampWeapon`) VALUES (@personaId, @playerName, @weaponStats, @generalStatus, @weaponStatsError, @timestamp) ON DUPLICATE KEY UPDATE `weaponStats` = @weaponStats, `generalStatus` = @generalStatus, `weaponStatsError` = @weaponStatsError, `timestampWeapon` = @timestamp;";
                                }
                                else
                                {
                                    command.CommandText = command.CommandText + "clanTag`, `weaponStats`, `generalStatus`, `weaponStatsError`, `timestampWeapon`, `timestampClanTag`) VALUES (@personaId, @playerName, @clanTag, @weaponStats, @generalStatus, @weaponStatsError, @timestamp, @timestamp) ON DUPLICATE KEY UPDATE `clanTag` = @clanTag, `weaponStats` = @weaponStats, `generalStatus` = @generalStatus, `weaponStatsError` = @weaponStatsError, `timestampWeapon` = @timestamp, `timestampClanTag` = @timestamp;";
                                    if (stats.ClanTag.CompareTo("@NotAClanTag@") == 0)
                                    {
                                        command.Parameters.AddWithValue("@clanTag", DBNull.Value);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@clanTag", stats.ClanTag);
                                    }
                                }
                                command.Parameters.AddWithValue("@personaId", stats.PersonaId);
                                command.Parameters.AddWithValue("@playerName", stats.PlayerName);
                                command.Parameters.AddWithValue("@weaponStats", stats.WeaponStats);
                                if (stats.GeneralStatus == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@generalStatus", "Success");
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@generalStatus", stats.GeneralStatus);
                                }
                                if (stats.WeaponStatsError == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@weaponStatsError", DBNull.Value);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@weaponStatsError", stats.WeaponStatsError);
                                }
                                command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                                break;
                            case RequestTypeEnum.Vehicle:
                                if (stats.ClanTag == String.Empty)
                                {
                                    command.CommandText = command.CommandText + "vehicleStats`, `generalStatus`, `vehicleStatsError`, `timestampVehicle`) VALUES (@personaId, @playerName, @vehicleStats, @generalStatus, @vehicleStatsError, @timestamp) ON DUPLICATE KEY UPDATE `vehicleStats` = @vehicleStats, `generalStatus` = @generalStatus, `vehicleStatsError` = @vehicleStatsError, `timestampVehicle` = @timestamp;";
                                }
                                else
                                {
                                    command.CommandText = command.CommandText + "clanTag`, `vehicleStats`, `generalStatus`, `vehicleStatsError`, `timestampVehicle`, `timestampClanTag`) VALUES (@personaId, @playerName, @clanTag, @vehicleStats, @generalStatus, @vehicleStatsError, @timestamp, @timestamp) ON DUPLICATE KEY UPDATE `clanTag` = @clanTag, `vehicleStats` = @vehicleStats, `generalStatus` = @generalStatus, `vehicleStatsError` = @vehicleStatsError, `timestampVehicle` = @timestamp, `timestampClanTag` = @timestamp;";
                                    if (stats.ClanTag.CompareTo("@NotAClanTag@") == 0)
                                    {
                                        command.Parameters.AddWithValue("@clanTag", DBNull.Value);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@clanTag", stats.ClanTag);
                                    }
                                }
                                command.Parameters.AddWithValue("@personaId", stats.PersonaId);
                                command.Parameters.AddWithValue("@playerName", stats.PlayerName);
                                command.Parameters.AddWithValue("@vehicleStats", stats.VehicleStats);
                                if (stats.GeneralStatus == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@generalStatus", "Success");
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@generalStatus", stats.GeneralStatus);
                                }
                                if (stats.VehicleStatsError == String.Empty)
                                {
                                    command.Parameters.AddWithValue("@vehicleStatsError", DBNull.Value);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@vehicleStatsError", stats.VehicleStatsError);
                                }
                                command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                                break;
                            default:
                                return;
                        }

                        connection.Open();

                        if (command.ExecuteNonQuery() > 0)
                        {
                            DebugWrite("MySqlInsert for player '" + stats.PlayerName + "' with requestType '" + requestType + "' successful!", 5);
                        }
                        else
                        {
                            DebugWrite("MySqlInsert for player '" + stats.PlayerName + "' with requestType '" + requestType + "' failed!", 2);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugWrite(FormatMessage(e.ToString(), MessageTypeEnum.Exception), 3);
            }

            DebugWrite("MySqlInsert finished!", 6);
        }

        public void PlayerLookup(params String[] commands)
        {
            if (commands.Length < 1)
            {
                ConsoleError("PlayerLookup failed. No commands given.");
                return;
            }

            new Thread(new ParameterizedThreadStart(AddRequest)).Start(commands[0]);
        }

        

        public void PlayerLookupReturn(params String[] returnMessage)
        {
            StreamWriter sw = new StreamWriter("response.txt");
            foreach (String message in returnMessage)
            {
                sw.WriteLine(message);
            }
            sw.Flush();
            sw.Close();
        }

        #region Classes

        private class LookupRequest
        {
            private String _pluginName;
            private String _pluginMethod;
            private String _playerName;
            private RequestTypeEnum _requestType;
            private DateTime _requestTimestamp;

            public LookupRequest(String pluginName, String pluginMethod, String playerName, RequestTypeEnum requestType)
            {
                _pluginName = pluginName;
                _pluginMethod = pluginMethod;
                _playerName = playerName;
                _requestType = requestType;
                _requestTimestamp = DateTime.UtcNow;
            }

            public String PluginName { get { return _pluginName; } }
            public String PluginMethod { get { return _pluginMethod; } }
            public String PlayerName { get { return _playerName; } }
            public RequestTypeEnum RequestType { get { return _requestType; } }

            public override String ToString()
            {
                return ">LookupRequest< pluginName: '" + _pluginName + "'; pluginMethod: '" + _pluginMethod + "'; playerName: '" + _playerName + "'; requestType: '" + _requestType + "'; requestTimestamp: '" + _requestTimestamp.ToString() + "' >LookupRequest<";
            }
        }

        private class LookupResponse
        {
            private String _pluginName;
            private String _pluginMethod;
            private String _playerName;
            private String _jsonData;

            public LookupResponse(String pluginName, String pluginMethod, String playerName, String jsonData)
            {
                _pluginName = pluginName;
                _pluginMethod = pluginMethod;
                _playerName = playerName;
                _jsonData = jsonData;
            }

            public String PluginName { get { return _pluginName; } }
            public String PluginMethod { get { return _pluginMethod; } }
            public String PlayerName { get { return _playerName; } }
            public String JsonData { get { return _jsonData; } }
        }

        private class PlayerStats
        {
            private String _personaId;
            private String _name;
            private String _clanTag;
            private String _overviewStats;
            private String _weaponStats;
            private String _vehicleStats;
            private String _generalStatus;
            private String _overviewStatsError;
            private String _weaponStatsError;
            private String _vehicleStatsError;
            private double _fetchTime;
            private double _age;

            public PlayerStats(String personaId, String name)
            {
                _personaId = personaId;
                _name = name;
                _clanTag = "@NotAClanTag@";
                _overviewStats = String.Empty;
                _weaponStats = String.Empty;
                _vehicleStats = String.Empty;
                _generalStatus = String.Empty;
                _overviewStatsError = String.Empty;
                _weaponStatsError = String.Empty;
                _vehicleStatsError = String.Empty;
                _fetchTime = 0.0D;
                _age = 0.0D;
            }

            public String PersonaId { get { return _personaId; } set { _personaId = value; } }
            public String PlayerName { get { return _name; } set { _name = value; } }
            public String ClanTag { get { return _clanTag; } set { _clanTag = value; } }
            public String OverviewStats { get { return _overviewStats; } set { _overviewStats = value; } }
            public String WeaponStats { get { return _weaponStats; } set { _weaponStats = value; } }
            public String VehicleStats { get { return _vehicleStats; } set { _vehicleStats = value; } }
            public String GeneralStatus { get { return _generalStatus; } set { _generalStatus = value; } }
            public String OverviewStatsError 
            {
                get { return _overviewStatsError; } 
                set 
                { 
                    if (_overviewStatsError.Length == 0) { _overviewStatsError = value; }
                    else if (!_overviewStatsError.Contains(value)) { _overviewStatsError = _overviewStatsError + "\n=====\n" + value; }
                }
            }
            public String WeaponStatsError 
            { 
                get { return _weaponStatsError; } 
                set
                { 
                    if (_weaponStatsError.Length == 0) { _weaponStatsError = value; }
                    else if (!_weaponStatsError.Contains(value)) { _weaponStatsError = _weaponStatsError + "\n=====\n" + value; }
                }
            }
            public String VehicleStatsError 
            {
                get { return _vehicleStatsError; }
                set
                {
                    if (_vehicleStatsError.Length == 0) { _vehicleStatsError = value; }
                    else if (!_vehicleStatsError.Contains(value)) { _vehicleStatsError = _vehicleStatsError + "\n=====\n"  + value; }
                }
            }
            public double FetchTime { get { return _fetchTime; } set { _fetchTime = value; } }
            public double Age { get { return _age; } set { _age = value; } }

            public bool ValidStats(RequestTypeEnum requestType)
            {
                if (String.IsNullOrEmpty(_personaId) && String.IsNullOrEmpty(_name))
                {
                    GeneralStatus = "PersonaId and Name empty!";
                    return false;
                }

                /*
                ClanTag request may still succeed even if generalStatus is not Success,
                so ignore generalStatus on ClanTag requests
                */
                if (requestType != RequestTypeEnum.ClanTag && _generalStatus != String.Empty && !_generalStatus.Contains("Success"))
                {
                    return false;
                }

                switch (requestType)
                {
                    case RequestTypeEnum.ClanTag:
                        if (_clanTag.CompareTo("@NotAClanTag@") == 0)
                        {
                            return false;
                        }
                        break;
                    case RequestTypeEnum.Overview:
                        if (_overviewStats == String.Empty || _overviewStatsError != String.Empty)
                        {
                            if (_overviewStatsError != String.Empty) GeneralStatus = "Error, details in OverviewStatsError";
                            return false;
                        }
                        break;
                    case RequestTypeEnum.Weapon:
                        if (_weaponStats == String.Empty || _weaponStatsError != String.Empty)
                        {
                            if (_weaponStatsError != String.Empty) GeneralStatus = "Error, details in WeaponStatsError";
                            return false;
                        }
                        break;
                    case RequestTypeEnum.Vehicle:
                        if (_vehicleStats == String.Empty || _vehicleStatsError != String.Empty)
                        {
                            if (_vehicleStatsError != String.Empty) GeneralStatus = "Error, details in VehicleStatsError";
                            return false;
                        }
                        break;
                    default:
                        return false;
                }

                return true;
            }

            public String GetStats(RequestTypeEnum requestType)
            {
                Hashtable stats = null;

                switch (requestType)
                {
                    case RequestTypeEnum.ClanTag:
                        stats = new Hashtable();
                        stats["type"] = "Success";
                        stats["message"] = "OK";
                        stats["fetchTime"] = _fetchTime.ToString("F3");
                        stats["age"] = _age.ToString("F1");
                        Hashtable data = new Hashtable();
                        data["clanTag"] = _clanTag;
                        stats["data"] = data;
                        return JSON.JsonEncode(stats);
                    case RequestTypeEnum.Overview:
                        stats = (Hashtable)JSON.JsonDecode(_overviewStats);
                        stats["fetchTime"] = _fetchTime.ToString("F3");
                        stats["age"] = _age.ToString("F1");
                        return JSON.JsonEncode(stats);
                    case RequestTypeEnum.Weapon:
                        stats = (Hashtable)JSON.JsonDecode(_weaponStats);
                        stats["fetchTime"] = _fetchTime.ToString("F3");
                        stats["age"] = _age.ToString("F1");
                        return JSON.JsonEncode(stats);
                    case RequestTypeEnum.Vehicle:
                        stats = (Hashtable)JSON.JsonDecode(_vehicleStats);
                        stats["fetchTime"] = _fetchTime.ToString("F3");
                        stats["age"] = _age.ToString("F1");
                        return JSON.JsonEncode(stats);
                    default:
                        return String.Empty;
                }
            }
        }

        private class BattlelogLookupException: System.Exception
        {
           public BattlelogLookupException()
           {
           }

           public BattlelogLookupException(string message): base(message)
           {
           }

           public BattlelogLookupException(string message, Exception innerException): base(message, innerException)
           {
           }
        }

        #endregion

        #region Helper methods

        #region Logging/writing

        public String FormatMessage(String msg, MessageTypeEnum type)
        {
            String prefix = "[^bBattlelog Cache^n] ";

            if (type.Equals(MessageTypeEnum.Warning))
            {
                prefix += "^1^bWARNING^0^n: ";
            }
            else if (type.Equals(MessageTypeEnum.Error))
            {
                prefix += "^1^bERROR^0^n: ";
            }
            else if (type.Equals(MessageTypeEnum.Exception))
            {
                prefix += "^1^bEXCEPTION^0^n: ";
            }

            return prefix + msg;
        }

        public void LogWrite(String msg)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }

        public void ConsoleWrite(string msg, MessageTypeEnum type)
        {
            LogWrite(FormatMessage(msg, type));
        }

        public void ConsoleWrite(string msg)
        {
            ConsoleWrite(msg, MessageTypeEnum.Normal);
        }

        public void ConsoleWarn(String msg)
        {
            ConsoleWrite(msg, MessageTypeEnum.Warning);
        }

        public void ConsoleError(String msg)
        {
            ConsoleWrite(msg, MessageTypeEnum.Error);
        }

        public void ConsoleException(String msg)
        {
            ConsoleWrite(msg, MessageTypeEnum.Exception);
        }

        public void DebugWrite(string msg, int level)
        {
            if (debugLevel >= level)
            {
                ConsoleWrite(msg, MessageTypeEnum.Normal);
            }
        }

        #endregion

        public void ServerCommand(params String[] args)
        {
            List<string> list = new List<string>();
            list.Add("procon.protected.send");
            list.AddRange(args);
            this.ExecuteCommand(list.ToArray());
        }

        private void InitializeThreads()
        {
            requestLoopThread = new Thread(RequestLoop);
            requestLoopThread.Name = "LookupLoop";
            requestLoopThread.IsBackground = true;
            responseLoopThread = new Thread(ResponseLoop);
            responseLoopThread.Name = "ResponseLoop";
            responseLoopThread.IsBackground = true;
        }

        private String PrepareMySqlConnectionString()
        {
            return "Server=" + mySqlHostname + ";Port=" + mySqlPort + ";Database=" + mySqlDatabase + ";Uid=" + mySqlUsername + ";Pwd=" + mySqlPassword + ";";
        }

        private String DownloadWebPage(WebClient webClient, String url, ref String result)
        {
            try
            {
                if (webClient == null)
                {
                    webClient = new WebClient();
                    String userAgent = "Mozilla/5.0 (compatible; Procon 1; CBattlelogCache)";
                    webClient.Headers.Add("user-agent", userAgent);
                }

                DateTime start = DateTime.Now;
                result = webClient.DownloadString(url);
                DebugWrite("DownloadWebPage for URL '" + url + "' took " + DateTime.Now.Subtract(start).TotalSeconds.ToString("F2") + " seconds", 5);

                if (Regex.Match(result, @"that\s+page\s+doesn't\s+exist", RegexOptions.IgnoreCase | RegexOptions.Singleline).Success)
                {
                    return "Player not found";
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private int RequestQueueCount()
        {
            int count = 0;

            lock (lookupRequestLock)
            {
                count = lookupRequests.Count;
            }

            return count;
        }

        private int ResponseQueueCount()
        {
            int count = 0;

            lock (lookupResponseLock)
            {
                count = lookupResponses.Count;
            }

            return count;
        }

        #endregion

    } // end BattlelogCache
} // end namespace PRoConEvents



