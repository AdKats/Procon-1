/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */

using PRoCon.Core.Accounts;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Consoles;
using PRoCon.Core.Events;
using PRoCon.Core.Lists;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote.Layer;
using PRoCon.Core.Settings;
using PRoCon.Core.TextChatModeration;
using PRoCon.Core.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PRoCon.Core.Remote
{
    public class PRoConClient
    {
        protected readonly object ConfigSavingLocker = new object();

        /// <summary>
        /// The task timer to fire at a one second interval.
        /// </summary>
        protected Timer TaskTimer;

        /// <summary>
        /// True if the connection config is currently loading or savings.
        /// </summary>
        protected bool IsLoadingSavingConnectionConfig;
        //private Thread m_thTasks;
        //private bool m_isTasksRunning;

        /// <summary>
        /// What is this and what does it do?
        /// </summary>
        private FrostbiteConnection _connection;

        /// <summary>
        /// Bool to specify if the game is running a modification (BFBC2: Vietnam)
        /// </summary>
        protected bool IsGameModModified;

        /// <summary>
        /// The Uid for this procon events.
        /// </summary>
        protected string ProconEventsUid;

        /// <summary>
        /// The maximum number of seconds/minutes that a plugin can execute a single command before it is disabled.
        /// </summary>
        public TimeSpan PluginMaxRuntimeSpan = new TimeSpan(0, 0, 59);

        private bool PlayTimesSynced = false;

        #region Event handlers

        public delegate void AuthenticationFailureHandler(PRoConClient sender, string strError);

        public delegate void AutomaticallyConnectHandler(PRoConClient sender, bool isEnabled);

        public delegate void EmptyParamterHandler(PRoConClient sender);

        public delegate void FailureHandler(PRoConClient sender, Exception exception);

        public delegate void FullBanListListHandler(PRoConClient sender, List<CBanInfo> lstBans);

        public delegate void FullTextChatModerationListListHandler(PRoConClient sender, TextChatModerationDictionary moderationList);

        public delegate void ListMapZonesHandler(PRoConClient sender, List<MapZoneDrawing> zones);

        public delegate void MapZoneEditedHandler(PRoConClient sender, MapZoneDrawing zone);

        public delegate void PassLayerEventHandler(PRoConClient sender, Packet packet);

        public delegate void PlayerKilledHandler(PRoConClient sender, Kill kKillerVictimDetails);

        public delegate void PlayerSpawnedHandler(PRoConClient sender, string soldierName, Inventory spawnedInventory);

        public delegate void ProconAdminPlayerPinged(PRoConClient sender, string strSoldierName, int iPing);

        public delegate void ProconAdminSayingHandler(PRoConClient sender, string strAdminStack, string strMessage, CPlayerSubset spsAudience);

        public delegate void ProconAdminYellingHandler(PRoConClient sender, string strAdminStack, string strMessage, int iMessageDuration, CPlayerSubset spsAudience);

        public delegate void ProconPrivilegesHandler(PRoConClient sender, CPrivileges spPrivs);

        public delegate void ProconVersionHandler(PRoConClient sender, Version version);

        public delegate void PunkbusterBanHandler(PRoConClient sender, CBanInfo cbiUnbannedPlayer);

        public delegate void PunkbusterPlayerInfoHandler(PRoConClient sender, CPunkbusterInfo pbInfo);

        public delegate void ReadRemoteConsoleHandler(PRoConClient sender, DateTime loggedTime, string text);

        public delegate void ReceiveProconVariableHandler(PRoConClient sender, string strVariable, string strValue);

        public delegate void RemoteAccountAlteredHandler(PRoConClient sender, string accountName, CPrivileges accountPrivileges);

        public delegate void RemoteAccountHandler(PRoConClient sender, string accountName);

        public delegate void RemoteAccountLoginStatusHandler(PRoConClient sender, string accountName, bool isOnline);

        public delegate void RemoteEnabledPluginsHandler(PRoConClient sender, List<string> enabledPluginClasses);

        public delegate void RemoteLoadedPluginsHandler(PRoConClient sender, Dictionary<string, PluginDetails> loadedPlugins);

        public delegate void RemotePluginEnabledHandler(PRoConClient sender, string strClassName, bool isEnabled);

        public delegate void RemotePluginLoadedHandler(PRoConClient sender, PluginDetails spdDetails);

        public delegate void RemotePluginVariablesHandler(PRoConClient sender, string strClassName, List<CPluginVariable> lstVariables);

        public delegate void SocketExceptionHandler(PRoConClient sender, SocketException se);

        public event EmptyParamterHandler ConnectAttempt;
        public event EmptyParamterHandler ConnectSuccess;
        public event EmptyParamterHandler ConnectionClosed;

        public event EmptyParamterHandler LoginAttempt;
        public event EmptyParamterHandler Login;
        public event EmptyParamterHandler Logout;

        public event EmptyParamterHandler GameTypeDiscovered;

        public event FailureHandler ConnectionFailure;

        public event SocketExceptionHandler SocketException;

        public event PassLayerEventHandler PassLayerEvent;

        public event AuthenticationFailureHandler LoginFailure;

        public event PunkbusterPlayerInfoHandler PunkbusterPlayerInfo;
        public event EmptyParamterHandler PunkbusterBeginPlayerInfo;
        public event EmptyParamterHandler PunkbusterEndPlayerInfo;

        public event PunkbusterBanHandler PunkbusterPlayerBanned;
        public event PunkbusterBanHandler PunkbusterPlayerUnbanned;

        public event FullBanListListHandler FullBanListList;

        public event FullTextChatModerationListListHandler FullTextChatModerationListList;

        // These events are in PRoConClient to determine if it's a recompile or not.
        public event EmptyParamterHandler CompilingPlugins;
        public event EmptyParamterHandler RecompilingPlugins;
        public event EmptyParamterHandler PluginsCompiled;

        public event ProconAdminSayingHandler ProconAdminSaying;

        public event ProconAdminYellingHandler ProconAdminYelling;

        public event ProconAdminPlayerPinged ProconAdminPinging;

        public event ProconPrivilegesHandler ProconPrivileges;

        public event ProconVersionHandler ProconVersion;

        public event ReceiveProconVariableHandler ReceiveProconVariable;

        // List/Edit/Create/Delete Accounts through the Parent Layer Control.

        public event RemoteAccountLoginStatusHandler RemoteAccountLoggedIn;

        public event EmptyParamterHandler RemoteAccountChangePassword;

        public event RemoteAccountAlteredHandler RemoteAccountAltered;

        public event RemoteAccountHandler RemoteAccountCreated;
        public event RemoteAccountHandler RemoteAccountDeleted;

        // List/Enable/Disable remote plugins via PLC.

        public event RemoteEnabledPluginsHandler RemoteEnabledPlugins;

        public event RemoteLoadedPluginsHandler RemoteLoadedPlugins;

        public event RemotePluginLoadedHandler RemotePluginLoaded;

        public event RemotePluginEnabledHandler RemotePluginEnabled;

        public event RemotePluginVariablesHandler RemotePluginVariables;

        public event AutomaticallyConnectHandler AutomaticallyConnectChanged;

        public event PlayerSpawnedHandler PlayerSpawned;

        public event PlayerKilledHandler PlayerKilled;

        public event MapZoneEditedHandler MapZoneCreated;
        public event MapZoneEditedHandler MapZoneDeleted;
        public event MapZoneEditedHandler MapZoneModified;

        public event ListMapZonesHandler ListMapZones;

        public event ReadRemoteConsoleHandler ReadRemotePluginConsole;
        public event ReadRemoteConsoleHandler ReadRemoteChatConsole;

        #region Packages

        public delegate void RemotePackagesInstallErrorHandler(PRoConClient sender, string uid, string error);

        public delegate void RemotePackagesInstallHandler(PRoConClient sender, string uid);

        public delegate void RemotePackagesInstalledHandler(PRoConClient sender, string uid, bool restart);

        public event RemotePackagesInstallHandler PackageDownloading;
        public event RemotePackagesInstallHandler PackageDownloaded;
        public event RemotePackagesInstallHandler PackageInstalling;

        public event RemotePackagesInstallErrorHandler PackageDownloadError;

        public event RemotePackagesInstalledHandler PackageInstalled;

        #endregion

        #endregion

        #region Attributes

        protected bool IsAutomaticallyConnectEnabled;
        public FrostbiteClient Game { get; private set; }

        public string InstigatingAccountName { get; private set; }

        public string Password { get; set; }

        public bool IsPRoConConnection { get; private set; }

        public EventCaptures EventsLogging { get; private set; }

        public PluginManager PluginsManager { get; private set; }

        public PluginConsole PluginConsole { get; private set; }

        public ConnectionConsole Console { get; private set; }

        public PunkbusterConsole PunkbusterConsole { get; private set; }

        public ChatConsole ChatConsole { get; private set; }

        public PlayerDictionary PlayerList { get; private set; }

        public PlayerListSettings PlayerListSettings { get; private set; }

        public ListsSettings ListSettings { get; private set; }

        public ServerSettings ServerSettings { get; private set; }

        public CServerInfo CurrentServerInfo { get; private set; }

        public List<CTeamName> TeamNameList { get; private set; }

        public NotificationList<CMap> MapListPool { get; private set; }

        public NotificationList<string> ReservedSlotList { get; private set; }

        public NotificationList<string> SpectatorList { get; private set; }

        public CLocalization Language
        {
            get { return Parent.CurrentLanguage; }
        }

        public CPrivileges Privileges { get; private set; }

        public Version ConnectedLayerVersion { get; private set; }

        public ILayerInstance Layer { get; private set; }

        public NotificationList<string> Reasons { get; private set; }

        public VariableDictionary Variables { get; private set; }

        public List<CBanInfo> FullVanillaBanList { get; private set; }


        public TextChatModerationDictionary FullTextChatModerationList { get; private set; }

        public MapGeometry MapGeometry { get; private set; }

        public bool AutomaticallyConnect
        {
            get { return IsAutomaticallyConnectEnabled; }
            set
            {
                IsAutomaticallyConnectEnabled = value;

                if (AutomaticallyConnectChanged != null)
                {
                    this.AutomaticallyConnectChanged(this, value);
                }
            }
        }

        /*
        public bool ManuallyDisconnected {
            get;
            private set;
        }
        */
        private List<Task> Tasks { get; set; }

        // Variables received by the server.
        //private Dictionary<string, string> m_dicSvLayerVariables;
        public VariableDictionary SV_Variables { get; private set; }

        public WeaponDictionary Weapons { get; private set; }

        public SpecializationDictionary Specializations { get; private set; }

        public string HostName { get; private set; }

        public ushort Port { get; private set; }

        public string ConnectionServerName
        {
            get { return _connectionServerName; }
            set { _connectionServerName = value; }
        }
        private string _connectionServerName = String.Empty;

        public string Username
        {
            get { return _username; }
            set
            {
                if (Game != null && Game.IsLoggedIn == false)
                {
                    _username = value;
                }
            }
        }
        private string _username = String.Empty;

        public string HostNamePort
        {
            get { return HostName + ":" + Port.ToString(); }
        }

        public string FileHostNamePort
        {
            get { return HostName + "_" + Port.ToString(); }
        }

        /*
        public bool ConnectionError {
            get;
            private set;
        }
        */

        public string GameType
        {
            get
            {
                string gameType = String.Empty;

                if (Game != null)
                {
                    gameType = Game.GameType;
                }

                return gameType;
            }
        }

        public string VersionNumber { get; private set; }

        #region Connection State

        private bool IsConnected
        {
            get
            {
                bool isConnected = false;

                if (Game != null && Game.Connection != null)
                {
                    isConnected = Game.Connection.IsConnected;
                }
                else if (_connection != null)
                {
                    isConnected = _connection.IsConnected;
                }

                return isConnected;
            }
        }

        private bool IsConnecting
        {
            get
            {
                bool isConnecting = false;

                if (Game != null && Game.Connection != null)
                {
                    isConnecting = Game.Connection.IsConnecting;
                }
                else if (_connection != null)
                {
                    isConnecting = _connection.IsConnecting;
                }

                return isConnecting;
            }
        }

        public ConnectionState State
        {
            get
            {
                ConnectionState currentState = _currentState;

                if (IsConnected == true)
                {
                    currentState = ConnectionState.Connected;
                }
                else if (IsConnecting == true)
                {
                    currentState = ConnectionState.Connecting;
                }

                return currentState;
            }
            set { _currentState = value; }
        }
        private ConnectionState _currentState;

        public bool IsLoggedIn
        {
            get
            {
                bool isLoggedIn = false;

                if (Game != null)
                {
                    isLoggedIn = Game.IsLoggedIn;
                }

                return isLoggedIn;
            }
        }

        #endregion

        #endregion

        #region Layer Packet Passing

        // <UInt32, - Sequence number sent to the BFBC2 server
        // Packet> - Original sequence number to be used when passing it back to the PRoCon layer.
        private readonly Dictionary<UInt32, SOriginalForwardedPacket> m_dicForwardedPackets;

        private readonly Dictionary<string, string> m_dicUsernamesToUids;

        internal struct SOriginalForwardedPacket
        {
            public List<string> m_lstWords;
            public ILayerClient m_sender;
            public UInt32 m_ui32OriginalSequence;
        }

        #endregion

        public PRoConClient(PRoConApplication praApplication, string hostName, ushort port, string username, string password)
        {
            HostName = hostName;
            Port = port;
            Password = password;
            _username = username;
            //this.m_uscParent = frmParent;
            Parent = praApplication;

            ProconEventsUid = String.Empty;

            m_dicForwardedPackets = new Dictionary<UInt32, SOriginalForwardedPacket>();

            Tasks = new List<Task>();
            VersionNumber = String.Empty;

            Layer = new LayerInstance();

            m_dicUsernamesToUids = new Dictionary<string, string>() {
                {"SYSOP", ""}
            };
            InstigatingAccountName = String.Empty;

            PluginMaxRuntimeSpan = new TimeSpan(0, Parent.OptionsSettings.PluginMaxRuntime_m, Parent.OptionsSettings.PluginMaxRuntime_s);
        }

        public PRoConApplication Parent { get; private set; }

        private void AssignEventHandlers()
        {
            if (Game != null)
            {
                Game.Login += new FrostbiteClient.EmptyParamterHandler(Game_Login);
                Game.LoginFailure += new FrostbiteClient.AuthenticationFailureHandler(Game_LoginFailure);
                Game.Logout += new FrostbiteClient.EmptyParamterHandler(Game_Logout);

                Game.ListPlayers += OnListPlayers;
                Game.PlayerLeft += OnPlayerLeft;
                Game.PlayerDisconnected += OnPlayerDisconnected;
                Game.PunkbusterMessage += OnPunkbusterMessage;

                // this.Game.ServerInfo += this.OnServerInfo;

                Game.BanListList += new FrostbiteClient.BanListListHandler(PRoConClient_BanListList);
                Game.TextChatModerationListAddPlayer += new FrostbiteClient.TextChatModerationListAddPlayerHandler(Game_TextChatModerationListAddPlayer);
                Game.TextChatModerationListRemovePlayer += new FrostbiteClient.TextChatModerationListRemovePlayerHandler(Game_TextChatModerationListRemovePlayer);
                Game.TextChatModerationListClear += new FrostbiteClient.EmptyParamterHandler(Game_TextChatModerationListClear);
                Game.TextChatModerationListList += new FrostbiteClient.TextChatModerationListListHandler(Game_TextChatModerationListList);
                Game.PlayerLimit += OnPlayerLimit;

                Game.ReservedSlotsList += new FrostbiteClient.ReservedSlotsListHandler(PRoConClient_ReservedSlotsList);
                Game.ReservedSlotsPlayerAdded += new FrostbiteClient.ReservedSlotsPlayerHandler(PRoConClient_ReservedSlotsPlayerAdded);
                Game.ReservedSlotsPlayerRemoved += new FrostbiteClient.ReservedSlotsPlayerHandler(PRoConClient_ReservedSlotsPlayerRemoved);

                Game.SpectatorListList += new FrostbiteClient.SpectatorListListHandler(PRoConClient_SpectatorListList);
                Game.SpectatorListPlayerAdded += new FrostbiteClient.SpectatorListPlayerHandler(PRoConClient_SpectatorListPlayerAdded);
                Game.SpectatorListPlayerRemoved += new FrostbiteClient.SpectatorListPlayerHandler(PRoConClient_SpectatorListPlayerRemoved);

                Game.ResponseError += new FrostbiteClient.ResponseErrorHandler(PRoConClient_ResponseError);

                PluginsCompiled += new EmptyParamterHandler(ProConClient_PluginsCompiled);

                Game.PlayerSpawned += new FrostbiteClient.PlayerSpawnedHandler(PRoConClient_PlayerSpawned);
                Game.PlayerKilled += new FrostbiteClient.PlayerKilledHandler(PRoConClient_PlayerKilled);
            }
        }

        private void Connection_ConnectionClosed(FrostbiteConnection sender)
        {
            State = ConnectionState.Disconnected;
            //sender.PacketReceived -= new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);

            if (ConnectionClosed != null)
            {
                this.ConnectionClosed(this);
            }
        }

        private void Connection_ConnectionFailure(FrostbiteConnection sender, Exception exception)
        {
            State = ConnectionState.Error;

            //sender.PacketReceived -= new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);

            if (ConnectionFailure != null)
            {
                this.ConnectionFailure(this, exception);
            }
        }

        private void Connection_SocketException(FrostbiteConnection sender, SocketException se)
        {
            State = ConnectionState.Error;

            //sender.PacketReceived -= new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);

            if (SocketException != null)
            {
                this.SocketException(this, se);
            }
        }

        private void InitialSetup()
        {
            if (Game != null)
            {
                IsLoadingSavingConnectionConfig = true;

                AssignEventHandlers();

                // Assume full access until we're told otherwise.
                Layer.Initialize(Parent, this);

                // I may move these events to within Layer, depends on the end of the restructure.
                Layer.LayerStarted += Layer_LayerOnline;
                Layer.LayerShutdown += Layer_LayerOffline;
                Layer.AccountPrivileges.AccountPrivilegeAdded += new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeAdded);
                Layer.AccountPrivileges.AccountPrivilegeRemoved += new AccountPrivilegeDictionary.AccountPrivilegeAlteredHandler(AccountPrivileges_AccountPrivilegeRemoved);

                foreach (AccountPrivilege apPriv in Layer.AccountPrivileges)
                {
                    apPriv.AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(item_AccountPrivilegesChanged);
                }

                Privileges = new CPrivileges(CPrivileges.FullPrivilegesFlags);
                EventsLogging = new EventCaptures(this);
                Console = new ConnectionConsole(this);
                PunkbusterConsole = new PunkbusterConsole(this);
                ChatConsole = new ChatConsole(this);
                PluginConsole = new PluginConsole(this);
                MapGeometry = new MapGeometry(this);
                MapGeometry.MapZones.MapZoneAdded += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneAdded);
                MapGeometry.MapZones.MapZoneChanged += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneChanged);
                MapGeometry.MapZones.MapZoneRemoved += new MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneRemoved);

                if (CurrentServerInfo == null)
                {
                    CurrentServerInfo = new CServerInfo();
                }

                ListSettings = new ListsSettings(this);
                ServerSettings = new ServerSettings(this);
                PlayerListSettings = new PlayerListSettings();
                PlayerList = new PlayerDictionary();
                TeamNameList = new List<CTeamName>();
                MapListPool = new NotificationList<CMap>();
                ReservedSlotList = new NotificationList<string>();
                SpectatorList = new NotificationList<string>();
                Variables = new VariableDictionary();
                SV_Variables = new VariableDictionary();
                Reasons = new NotificationList<string>();
                FullVanillaBanList = new List<CBanInfo>();
                FullTextChatModerationList = new TextChatModerationDictionary();
                Weapons = new WeaponDictionary();
                Specializations = new SpecializationDictionary();

                if (Regex.Match(HostName, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$").Success == true)
                {
                    Variables.SetVariable("SERVER_COUNTRY", Parent.GetCountryName(HostName));
                    Variables.SetVariable("SERVER_COUNTRY_CODE", Parent.GetCountryCode(HostName));
                }
                else
                {
                    IPAddress ipServer = FrostbiteConnection.ResolveHostName(HostName);
                    Variables.SetVariable("SERVER_COUNTRY", Parent.GetCountryName(ipServer.ToString()));
                    Variables.SetVariable("SERVER_COUNTRY_CODE", Parent.GetCountryCode(ipServer.ToString()));
                }

                Console.Logging = Parent.OptionsSettings.ConsoleLogging;
                EventsLogging.Logging = Parent.OptionsSettings.EventsLogging;
                ChatConsole.Logging = Parent.OptionsSettings.ChatLogging;
                PluginConsole.Logging = Parent.OptionsSettings.PluginLogging;

                //this.m_blLoadingSavingConnectionConfig = true;

                if (CurrentServerInfo.GameMod == GameMods.None)
                {
                    ExecuteConnectionConfig(Game.GameType + ".def", 0, null, false);
                }
                else
                {
                    ExecuteConnectionConfig(Game.GameType + "." + CurrentServerInfo.GameMod + ".def", 0, null, false);
                }

                // load override global_vars.def
                ExecuteGlobalVarsConfig("global_vars.def", 0, null);

                ExecuteConnectionConfig("reasons.cfg", 0, null, false);

                lock (Parent)
                {
                    if (Username.Length == 0 || Parent.OptionsSettings.LayerHideLocalPlugins == false)
                    {
                        CompilePlugins(Parent.OptionsSettings.PluginPermissions);
                    }
                }

                string configDirectoryPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), FileHostNamePort);
                string oldConfigFilePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), string.Format("{0}.cfg", FileHostNamePort));

                if (File.Exists(oldConfigFilePath) == false && Directory.Exists(configDirectoryPath) == true)
                {
                    string[] pluginConfigPaths = Directory.GetFiles(configDirectoryPath, "*.cfg");

                    if (Parent.OptionsSettings.UsePluginOldStyleLoad == true)
                    {
                        foreach (string pluginConfigPath in pluginConfigPaths)
                        {
                            ExecuteConnectionConfig(pluginConfigPath, 0, null, false);
                        }
                    }

                    BeginLoginSequence();

                    if (Parent.OptionsSettings.UsePluginOldStyleLoad == false)
                    {
                        foreach (string pluginConfigPath in pluginConfigPaths)
                        {
                            ExecuteConnectionConfig(pluginConfigPath, 0, null, true);
                        }
                    }

                    IsLoadingSavingConnectionConfig = false;
                }
                else
                {
                    if (Parent.OptionsSettings.UsePluginOldStyleLoad == true)
                    {
                        ExecuteConnectionConfig(FileHostNamePort + ".cfg", 0, null, false);
                    }

                    BeginLoginSequence();

                    if (Parent.OptionsSettings.UsePluginOldStyleLoad == false)
                    {
                        ExecuteConnectionConfig(FileHostNamePort + ".cfg", 0, null, true);
                    }

                    IsLoadingSavingConnectionConfig = false;
                    SaveConnectionConfig();

                    try
                    {
                        if (Directory.Exists(configDirectoryPath) == true && File.Exists(Path.Combine(configDirectoryPath, string.Format("{0}.cfg", FileHostNamePort))) == true)
                        {
                            try
                            {
                                if (File.Exists(oldConfigFilePath) == true)
                                {
                                    File.Delete(oldConfigFilePath);
                                }
                            }
                            catch (Exception e)
                            {
                                FrostbiteConnection.LogError("RemoveOldConfig", String.Empty, e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        FrostbiteConnection.LogError("MigrateConfig", "Error writing new config structure during migration", e);
                    }
                }

            }
        }

        private void BeginLoginSequence()
        {
            if (Username.Length > 0)
            {
                SendProconLoginUsernamePacket(Username);
            }
            else
            {
                Game.SendLoginHashedPacket(Password);
            }
        }

        private void Connection_PacketRecieved(FrostbiteConnection sender, bool isHandled, Packet packetBeforeDispatch)
        {
            if (packetBeforeDispatch.OriginatedFromServer == false)
            {
                Packet request = sender.GetRequestPacket(packetBeforeDispatch);

                if (request != null && String.Compare(request.Words[0], "version", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (Game == null)
                    {
                        if (String.Compare(packetBeforeDispatch.Words[1], "BFBC2", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new BFBC2Client(sender);
                            _connection = null;
                        }
                        else if (String.Compare(packetBeforeDispatch.Words[1], "MOH", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new MoHClient(sender);
                            _connection = null;
                        }
                        else if (String.Compare(packetBeforeDispatch.Words[1], "BF3", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new BF3Client(sender);
                            _connection = null;
                        }
                        else if (String.Compare(packetBeforeDispatch.Words[1], "BFHL", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new BFHLClient(sender);
                            _connection = null;
                        }
                        else if (String.Compare(packetBeforeDispatch.Words[1], "BF4", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new BF4Client(sender);
                            _connection = null;
                        }
                        else if (String.Compare(packetBeforeDispatch.Words[1], "MOHW", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Game = new MOHWClient(sender);
                            _connection = null;
                        }

                        if (Game != null)
                        {
                            VersionNumber = packetBeforeDispatch.Words[2];

                            Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(OnServerInfo);
                            Game.SendServerinfoPacket();

                            //sender.SendQueued(new Packet(false, false, sender.AcquireSequenceNumber, "serverInfo"));
                        }
                    }
                    else if (Game.Connection != null)
                    {
                        BeginLoginSequence();
                    }

                    sender.PacketReceived -= new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);
                }
            }
        }

        private void Connection_ConnectSuccess(FrostbiteConnection sender)
        {
            if (ConnectSuccess != null)
            {
                this.ConnectSuccess(this);
            }

            sender.PacketReceived -= new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);
            sender.PacketReceived += new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);
        }

        private void m_connection_ConnectionReady(FrostbiteConnection sender)
        {
            // Sleep the thread so the server has enough time to setup the listener.
            // This is generally only problematic when connecting to local layers.
            Thread.Sleep(50);

            sender.SendQueued(new Packet(false, false, sender.AcquireSequenceNumber, "version"));
        }

        private void Connection_ConnectAttempt(FrostbiteConnection sender)
        {
            if (ConnectAttempt != null)
            {
                this.ConnectAttempt(this);
            }
        }

        private void Game_LoginFailure(FrostbiteClient sender, string strError)
        {
            if (LoginFailure != null)
            {
                State = ConnectionState.Error;

                this.LoginFailure(this, strError);
            }
        }

        private void Game_Logout(FrostbiteClient sender)
        {
            if (Logout != null)
            {
                this.Logout(this);
            }
        }

        private void Game_Login(FrostbiteClient sender)
        {
            if (IsGameModModified == true)
            {
                if (CurrentServerInfo.GameMod == GameMods.None)
                {
                    ExecuteConnectionConfig(Game.GameType + ".def", 0, null, false);
                }
                else
                {
                    ExecuteConnectionConfig(Game.GameType + "." + CurrentServerInfo.GameMod + ".def", 0, null, false);
                }

                ExecuteGlobalVarsConfig("global_vars.def", 0, null);

                lock (Parent)
                {
                    if ((Parent.OptionsSettings.LayerHideLocalPlugins == false && IsPRoConConnection == true) || IsPRoConConnection == false)
                    {
                        CompilePlugins(Parent.OptionsSettings.PluginPermissions);
                    }
                }
            }

            if (IsPRoConConnection == true)
            {
                SendRequest(new List<string>() {
                    "procon.privileges"
                });
                SendRequest(new List<string>() {
                    "procon.registerUid",
                    "true",
                    FrostbiteClient.GeneratePasswordHash(Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss ff")), _username)
                });
                SendRequest(new List<string>() {
                    "version"
                });
                SendRequest(new List<string>() {
                    "procon.version"
                });
            }

            Game.FetchStartupVariables();

            // Occurs when they disconnect then reconnect a connection.
            if (PluginsManager == null)
            {
                if ((Parent.OptionsSettings.LayerHideLocalPlugins == false && IsPRoConConnection == true) || IsPRoConConnection == false)
                {
                    CompilePlugins(Parent.OptionsSettings.PluginPermissions);
                }
            }

            // This saves about 1.7 mb's per connection.  I'd prefer the plugins never compiled though if its connecting to a layer.
            //if (this.IsPRoConConnection == true) { this.PluginsManager.Unload(); GC.Collect();  }
            //if (this.Parent.OptionsSettings.LayerHideLocalPlugins == true && this.IsPRoConConnection == true) { this.PluginsManager.Unload(); GC.Collect(); }

            ExecuteConnectionConfig("connection_onlogin.cfg", 0, null, false);

            if (Login != null)
            {
                this.Login(this);
            }

            // Tasks

            if (TaskTimer == null)
            {
                TaskTimer = new Timer(1000);
                TaskTimer.Elapsed += new ElapsedEventHandler(TaskTimer_Elapsed);
                TaskTimer.Start();
            }
        }

        public void Shutdown()
        {
            if (Game != null)
            {
                Game.Shutdown();
            }
            else if (_connection != null)
            {
                _connection.Shutdown();
            }
            else if (ConnectionClosed != null)
            {
                this.ConnectionClosed(this);
            }
        }

        public void Poke()
        {
            // Poke the connection every second to make sure it's still alive, provided the state is not set to disconnected.
            if (Game != null && Game.Connection != null && State != ConnectionState.Disconnected)
            {
                Game.Connection.Poke();
            }

            if (Layer != null)
            {
                // Poke all of the layer clients to make sure they are still connected.
                this.Layer.Poke();
            }
        }

        public void Connect()
        {
            if (State != ConnectionState.Connecting && State != ConnectionState.Connected || (State == ConnectionState.Connected && IsLoggedIn == false))
            {
                if (TaskTimer != null)
                {
                    TaskTimer.Stop();
                    TaskTimer = null;
                }
                /*
                if (this.m_thTasks != null) {

                    try {
                        this.m_thTasks.Abort();
                    }
                    catch (Exception) { }

                    this.m_thTasks = null;
                }
                */
                if (Game == null)
                {
                    if (_connection == null)
                    {
                        _connection = new FrostbiteConnection(HostName, Port);
                        _connection.ConnectAttempt += new FrostbiteConnection.EmptyParamterHandler(Connection_ConnectAttempt);
                        _connection.ConnectionReady += new FrostbiteConnection.EmptyParamterHandler(m_connection_ConnectionReady);
                        _connection.ConnectSuccess += new FrostbiteConnection.EmptyParamterHandler(Connection_ConnectSuccess);
                        //this.m_connection.PacketReceived += new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);
                        _connection.SocketException += new FrostbiteConnection.SocketExceptionHandler(Connection_SocketException);
                        _connection.ConnectionFailure += new FrostbiteConnection.FailureHandler(Connection_ConnectionFailure);
                        _connection.ConnectionClosed += new FrostbiteConnection.EmptyParamterHandler(Connection_ConnectionClosed);
                        _connection.BeforePacketDispatch += new FrostbiteConnection.PrePacketDispatchedHandler(Connection_BeforePacketDispatch);
                        _connection.PacketCacheIntercept += new FrostbiteConnection.PacketCacheDispatchHandler(_connection_PacketCacheIntercept);
                    }

                    _connection.AttemptConnection();
                }
                else if (Game.Connection != null && Game.Connection.IsConnected == false)
                {
                    Game.Connection.AttemptConnection();
                }
            }
        }

        private void ProConClient_PluginsCompiled(PRoConClient sender)
        {
            if (PluginsManager != null)
            {
                PluginsManager.PluginVariableAltered += new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
                PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
                PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);

                PluginsManager.PluginPanic += new PluginManager.PluginEventHandler(PluginsManager_PluginPanic);
            }
        }

        private void Plugins_PluginDisabled(string strClassName)
        {
            SaveConnectionConfig();
        }

        private void Plugins_PluginEnabled(string strClassName)
        {
            SaveConnectionConfig();
        }

        private void Plugins_PluginVariableAltered(PluginDetails spdNewDetails)
        {
            SaveConnectionConfig();
        }

        private void _connection_PacketCacheIntercept(FrostbiteConnection sender, Packet request, Packet response)
        {
            InstigatingAccountName = String.Empty;

            HandleResponsePacket(response, false, false, request);
        }

        private void Connection_BeforePacketDispatch(FrostbiteConnection sender, Packet packetBeforeDispatch, out bool isProcessed)
        {
            bool blCancelPacket = false;
            bool blCancelUpdateEvent = false;

            InstigatingAccountName = String.Empty;

            // IF it's a response to a packet we sent..
            if (packetBeforeDispatch.OriginatedFromServer == false && packetBeforeDispatch.IsResponse == true)
            {
                blCancelPacket = HandleResponsePacket(packetBeforeDispatch, blCancelUpdateEvent, blCancelPacket);
            }
            // ELSE IF it's an event initiated by the server (OnJoin, OnLeave, OnChat etc)
            else if (packetBeforeDispatch.OriginatedFromServer == true && packetBeforeDispatch.IsResponse == false)
            {
                blCancelPacket = HandleEventPacket(packetBeforeDispatch, blCancelPacket);
            }

            isProcessed = blCancelPacket;
        }

        private bool HandleEventPacket(Packet cpBeforePacketDispatch, bool blCancelPacket)
        {
            if (cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.shutdown", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                State = ConnectionState.Error;
                Connection_ConnectionFailure(Game.Connection, new Exception("The PRoCon layer has been shutdown by the host"));

                Shutdown();
                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onLogin", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                // Also able to get their privs as well if needed?
                //this.m_uscParent.OnRemoteAccountLoggedIn(cpBeforePacketDispatch.Words[1], true);

                if (RemoteAccountLoggedIn != null)
                {
                    this.RemoteAccountLoggedIn(this, cpBeforePacketDispatch.Words[1], true);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onLogout", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                if (RemoteAccountLoggedIn != null)
                {
                    this.RemoteAccountLoggedIn(this, cpBeforePacketDispatch.Words[1], false);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onUidRegistered", true) == 0)
            {
                if (m_dicUsernamesToUids.ContainsKey(cpBeforePacketDispatch.Words[2]) == true)
                {
                    m_dicUsernamesToUids[cpBeforePacketDispatch.Words[2]] = cpBeforePacketDispatch.Words[1];
                }
                else
                {
                    m_dicUsernamesToUids.Add(cpBeforePacketDispatch.Words[2], cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onCreated", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                if (RemoteAccountCreated != null)
                {
                    this.RemoteAccountCreated(this, cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onDeleted", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                if (RemoteAccountDeleted != null)
                {
                    this.RemoteAccountDeleted(this, cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.account.onAltered", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                UInt32 ui32Privileges = 0;
                if (UInt32.TryParse(cpBeforePacketDispatch.Words[2], out ui32Privileges) == true)
                {
                    var spPrivs = new CPrivileges();
                    spPrivs.PrivilegesFlags = ui32Privileges;

                    if (ProconPrivileges != null && String.Compare(cpBeforePacketDispatch.Words[1], Username) == 0)
                    {
                        Privileges = spPrivs;

                        this.ProconPrivileges(this, spPrivs);
                    }

                    if (RemoteAccountAltered != null)
                    {
                        this.RemoteAccountAltered(this, cpBeforePacketDispatch.Words[1], spPrivs);
                    }
                }

                blCancelPacket = true;
            }

            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.packages.onDownloading", true) == 0)
            {
                if (PackageDownloading != null)
                {
                    this.PackageDownloading(this, cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.packages.onDownloaded", true) == 0)
            {
                if (PackageDownloaded != null)
                {
                    this.PackageDownloaded(this, cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.packages.onInstalling", true) == 0)
            {
                if (PackageInstalling != null)
                {
                    this.PackageInstalling(this, cpBeforePacketDispatch.Words[1]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.packages.onDownloadError", true) == 0)
            {
                if (PackageDownloadError != null)
                {
                    this.PackageDownloadError(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2]);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.packages.onInstalled", true) == 0)
            {
                bool restartRequired = false;

                if (bool.TryParse(cpBeforePacketDispatch.Words[2], out restartRequired) == true)
                {
                    if (PackageInstalled != null)
                    {
                        this.PackageInstalled(this, cpBeforePacketDispatch.Words[1], restartRequired);
                    }
                }

                blCancelPacket = true;
            }


            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.chat.onConsole", true) == 0)
            {
                long logTime = 0L;

                if (long.TryParse(cpBeforePacketDispatch.Words[1], out logTime) == true)
                {
                    if (ReadRemoteChatConsole != null)
                    {
                        this.ReadRemoteChatConsole(this, DateTime.FromBinary(logTime), cpBeforePacketDispatch.Words[2]);
                    }
                }
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.plugin.onConsole", true) == 0)
            {
                long logTime = 0L;

                if (long.TryParse(cpBeforePacketDispatch.Words[1], out logTime) == true)
                {
                    if (ReadRemotePluginConsole != null)
                    {
                        this.ReadRemotePluginConsole(this, DateTime.FromBinary(logTime), cpBeforePacketDispatch.Words[2]);
                    }
                }
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.plugin.onVariablesAltered", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int i = 1;
                var lstVariables = new List<CPluginVariable>();
                string strClassName = cpBeforePacketDispatch.Words[i++];

                int iTotalVariables = 0;
                if (int.TryParse(cpBeforePacketDispatch.Words[i++], out iTotalVariables) == true && i + (iTotalVariables * 3) <= cpBeforePacketDispatch.Words.Count)
                {
                    for (int x = 0; x < (iTotalVariables * 3); x += 3)
                    {
                        lstVariables.Add(new CPluginVariable(cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++]));
                    }
                }

                if (RemotePluginVariables != null)
                {
                    this.RemotePluginVariables(this, strClassName, lstVariables);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.plugin.onLoaded", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int i = 1;
                if (i + 6 <= cpBeforePacketDispatch.Words.Count)
                {
                    var spdLoaded = new PluginDetails();

                    spdLoaded.ClassName = cpBeforePacketDispatch.Words[i++];
                    spdLoaded.Name = cpBeforePacketDispatch.Words[i++];
                    spdLoaded.Author = cpBeforePacketDispatch.Words[i++];
                    spdLoaded.Website = cpBeforePacketDispatch.Words[i++];
                    spdLoaded.Version = cpBeforePacketDispatch.Words[i++];
                    spdLoaded.Description = cpBeforePacketDispatch.Words[i++];

                    spdLoaded.DisplayPluginVariables = new List<CPluginVariable>();
                    spdLoaded.PluginVariables = new List<CPluginVariable>(); // Not used here.
                    int iTotalVariables = 0;
                    if (int.TryParse(cpBeforePacketDispatch.Words[i++], out iTotalVariables) == true && i + (iTotalVariables * 3) <= cpBeforePacketDispatch.Words.Count)
                    {
                        for (int x = 0; x < (iTotalVariables * 3); x += 3)
                        {
                            spdLoaded.DisplayPluginVariables.Add(new CPluginVariable(cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++]));
                        }
                    }

                    if (RemotePluginLoaded != null)
                    {
                        this.RemotePluginLoaded(this, spdLoaded);
                    }
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.plugin.onEnabled", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                bool blEnabled = false;

                if (bool.TryParse(cpBeforePacketDispatch.Words[2], out blEnabled) == true && RemotePluginEnabled != null)
                {
                    this.RemotePluginEnabled(this, cpBeforePacketDispatch.Words[1], blEnabled);
                }

                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 4 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.admin.onSay", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                if (ProconAdminSaying != null)
                {
                    this.ProconAdminSaying(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2], new CPlayerSubset(cpBeforePacketDispatch.Words.GetRange(3, cpBeforePacketDispatch.Words.Count - 3)));
                }

                if (PassLayerEvent != null)
                {
                    this.PassLayerEvent(this, cpBeforePacketDispatch);
                }

                blCancelPacket = true;
            }
            // MoHW R-6 hack
            else if (cpBeforePacketDispatch.Words.Count >= 4 && Game.GameType == "MOHW" && String.Compare(cpBeforePacketDispatch.Words[0], "procon.admin.onYell", true) == 0)
            {
                // this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int iDisplayDuration = 0;

                if (ProconAdminYelling != null)
                {
                    this.ProconAdminYelling(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2], iDisplayDuration, new CPlayerSubset(cpBeforePacketDispatch.Words.GetRange(4, cpBeforePacketDispatch.Words.Count - 4)));
                }
                if (PassLayerEvent != null)
                {
                    this.PassLayerEvent(this, cpBeforePacketDispatch);
                }

                blCancelPacket = true;
            }
            // hack end
            else if (cpBeforePacketDispatch.Words.Count >= 5 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.admin.onYell", true) == 0)
            {
                // this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int iDisplayDuration = 0;

                if (int.TryParse(cpBeforePacketDispatch.Words[3], out iDisplayDuration) == true)
                {
                    if (ProconAdminYelling != null)
                    {
                        this.ProconAdminYelling(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2], iDisplayDuration, new CPlayerSubset(cpBeforePacketDispatch.Words.GetRange(4, cpBeforePacketDispatch.Words.Count - 4)));
                    }

                    if (PassLayerEvent != null)
                    {
                        this.PassLayerEvent(this, cpBeforePacketDispatch);
                    }
                }

                blCancelPacket = true;
            }
            // bf3 player.ping
            else if (Game.GameType == "BF3" && cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.admin.onPlayerPinged", true) == 0)
            {
                int iPing = 0;
                string strSoldierName = cpBeforePacketDispatch.Words[1];

                if (int.TryParse(cpBeforePacketDispatch.Words[2], out iPing) == true)
                {
                    if (iPing == 65535)
                    {
                        iPing = -1;
                    }

                    if (ProconAdminPinging != null)
                    {
                        this.ProconAdminPinging(this, strSoldierName, iPing);
                    }
                    if (PassLayerEvent != null)
                    {
                        this.PassLayerEvent(this, cpBeforePacketDispatch);
                    }
                }
                blCancelPacket = true;
            }
            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.updated", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                foreach (var registerUid in m_dicUsernamesToUids)
                {
                    if (String.Compare(cpBeforePacketDispatch.Words[1], registerUid.Value) == 0)
                    {
                        InstigatingAccountName = registerUid.Key;
                        break;
                    }
                }

                // Only parse it if the UID is different from this connections registered UID.
                if (String.Compare(cpBeforePacketDispatch.Words[1], ProconEventsUid) != 0)
                {
                    var lstAssumedRequestPacket = new List<string>(cpBeforePacketDispatch.Words);
                    lstAssumedRequestPacket.RemoveRange(0, 2);

                    var cpAssumedRequestPacket = new Packet(false, false, 0, lstAssumedRequestPacket);
                    var cpAssumedResponsePacket = new Packet(false, true, 0, new List<string>() {
                        "OK"
                    });

                    Game.DispatchResponsePacket(Game.Connection, cpAssumedResponsePacket, cpAssumedRequestPacket);

                    cpAssumedRequestPacket = null;
                    cpAssumedResponsePacket = null;

                    if (PassLayerEvent != null)
                    {
                        this.PassLayerEvent(this, cpBeforePacketDispatch);
                    }
                }

                blCancelPacket = true;
            }

            else if (cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.battlemap.onZoneRemoved", true) == 0)
            {
                if (MapZoneDeleted != null)
                {
                    this.MapZoneDeleted(this, new MapZoneDrawing(cpBeforePacketDispatch.Words[1], String.Empty, String.Empty, null, true));
                }
            }

            else if (cpBeforePacketDispatch.Words.Count >= 4 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.battlemap.onZoneCreated", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int iPoints = 0;

                if (int.TryParse(cpBeforePacketDispatch.Words[3], out iPoints) == true)
                {
                    var points = new Point3D[iPoints];

                    for (int i = 0; i < iPoints; i++)
                    {
                        points[i] = new Point3D(cpBeforePacketDispatch.Words[3 + i * 3 + 1], cpBeforePacketDispatch.Words[3 + i * 3 + 2], cpBeforePacketDispatch.Words[3 + i * 3 + 3]);
                    }

                    if (MapZoneCreated != null)
                    {
                        this.MapZoneCreated(this, new MapZoneDrawing(cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2], String.Empty, points, true));
                    }
                }

                blCancelPacket = true;
            }

            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.battlemap.onZoneModified", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                int iPoints = 0;

                if (int.TryParse(cpBeforePacketDispatch.Words[3], out iPoints) == true)
                {
                    var points = new Point3D[iPoints];

                    for (int i = 0; i < iPoints; i++)
                    {
                        points[i] = new Point3D(cpBeforePacketDispatch.Words[3 + i * 3 + 1], cpBeforePacketDispatch.Words[3 + i * 3 + 2], cpBeforePacketDispatch.Words[3 + i * 3 + 3]);
                    }

                    if (MapZoneModified != null)
                    {
                        this.MapZoneModified(this, new MapZoneDrawing(cpBeforePacketDispatch.Words[1], String.Empty, cpBeforePacketDispatch.Words[2], points, true));
                    }
                }

                blCancelPacket = true;
            }


            else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpBeforePacketDispatch.Words[0], "procon.vars.onAltered", true) == 0)
            {
                //this.SendPacket(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "OK" }));

                SV_Variables.SetVariable(cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2]);

                if (ReceiveProconVariable != null)
                {
                    this.ReceiveProconVariable(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2]);
                }

                blCancelPacket = true;
            }

            else
            {
                //  if (blCommandConnection == false) Pass everything else onto any connected clients..
                if (PassLayerEvent != null)
                {
                    this.PassLayerEvent(this, cpBeforePacketDispatch);
                }
            }

            return blCancelPacket;
        }

        private bool HandleResponsePacket(Packet cpBeforePacketDispatch, bool blCancelUpdateEvent, bool blCancelPacket, Packet request = null)
        {
            if (Game != null)
            {
                Packet cpRequestPacket = request ?? Game.Connection.GetRequestPacket(cpBeforePacketDispatch);

                if (cpRequestPacket != null)
                {
                    if (cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "InvalidUsername", true) == 0)
                    {
                        if (LoginFailure != null)
                        {
                            this.LoginFailure(this, cpBeforePacketDispatch.Words[0]);
                        }

                        Shutdown();
                        State = ConnectionState.Error;

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.version", true) == 0 && cpBeforePacketDispatch.Words.Count >= 2)
                    {
                        try
                        {
                            ConnectedLayerVersion = new Version(cpBeforePacketDispatch.Words[1]);

                            if (ProconVersion != null)
                            {
                                this.ProconVersion(this, ConnectedLayerVersion);
                            }
                        }
                        catch (Exception)
                        {
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.privileges", true) == 0 && cpBeforePacketDispatch.Words.Count >= 2)
                    {
                        UInt32 ui32Privileges = 0;
                        if (UInt32.TryParse(cpBeforePacketDispatch.Words[1], out ui32Privileges) == true)
                        {
                            var spPrivs = new CPrivileges();
                            spPrivs.PrivilegesFlags = ui32Privileges;

                            Privileges = spPrivs;

                            if (ProconPrivileges != null)
                            {
                                this.ProconPrivileges(this, spPrivs);
                            }
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[0], "procon.registerUid", true) == 0 && cpBeforePacketDispatch.Words.Count >= 1)
                    {
                        if (String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0 && cpRequestPacket.Words.Count >= 3)
                        {
                            ProconEventsUid = cpRequestPacket.Words[2];
                        }
                        else if (String.Compare(cpBeforePacketDispatch.Words[0], "ProconUidConflict", true) == 0)
                        {
                            // Conflict in our UID, just hash and send another one.
                            // Then go to vegas.
                            SendRequest(new List<string>() {
                                "procon.registerUid",
                                "true",
                                FrostbiteClient.GeneratePasswordHash(Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss ff")), _username)
                            });
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.account.listAccounts", true) == 0)
                    {
                        UInt32 ui32Privileges = 0;

                        for (int i = 1; i < cpBeforePacketDispatch.Words.Count; i += 2)
                        {
                            if (UInt32.TryParse(cpBeforePacketDispatch.Words[i + 1], out ui32Privileges) == true)
                            {
                                var spPrivs = new CPrivileges();
                                spPrivs.PrivilegesFlags = ui32Privileges;

                                if (RemoteAccountCreated != null && RemoteAccountAltered != null)
                                {
                                    this.RemoteAccountCreated(this, cpBeforePacketDispatch.Words[i]);
                                    this.RemoteAccountAltered(this, cpBeforePacketDispatch.Words[i], spPrivs);
                                }
                            }
                        }

                        blCancelPacket = true;
                    }

                    // syncPlayTimes coming in
                    else if (cpBeforePacketDispatch.Words.Count >= 3 && String.Compare(cpRequestPacket.Words[0], "procon.player.syncPlayTimes", true) == 0)
                    {
                        int iJoinTime = 0;
                        var vSoldier = new CPlayerInfo();
                        var oPlayer = new CPlayerInfo();

                        for (int i = 1; i < cpBeforePacketDispatch.Words.Count; i += 2)
                        {
                            vSoldier.SoldierName = cpBeforePacketDispatch.Words[i];
                            if (int.TryParse(cpBeforePacketDispatch.Words[i + 1], out iJoinTime) == true)
                            {
                                vSoldier.JoinTime = iJoinTime;
                            }
                            if (PlayerList.Contains(vSoldier.SoldierName) == true)
                            {
                                oPlayer = PlayerList[vSoldier.SoldierName];
                                oPlayer.JoinTime = iJoinTime;

                                PlayerList[PlayerList.IndexOf(PlayerList[vSoldier.SoldierName])] = oPlayer;
                            }
                        }
                        blCancelPacket = false;
                    }

                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.battlemap.listZones", true) == 0 && cpBeforePacketDispatch.Words.Count >= 2 && String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0)
                    {
                        var zones = new List<MapZoneDrawing>();

                        int iZones = 0;
                        int iOffset = 1;

                        if (int.TryParse(cpBeforePacketDispatch.Words[iOffset++], out iZones) == true)
                        {
                            for (int iZoneCount = 0; iZoneCount < iZones; iZoneCount++)
                            {
                                string uid = String.Empty;
                                string level = String.Empty;
                                string tags = String.Empty;
                                var points = new List<Point3D>();

                                if (iOffset + 4 < cpBeforePacketDispatch.Words.Count)
                                {
                                    uid = cpBeforePacketDispatch.Words[iOffset++];
                                    level = cpBeforePacketDispatch.Words[iOffset++];
                                    tags = cpBeforePacketDispatch.Words[iOffset++];

                                    int iZonePoints = 0;
                                    if (int.TryParse(cpBeforePacketDispatch.Words[iOffset++], out iZonePoints) == true && iOffset + iZonePoints * 3 <= cpBeforePacketDispatch.Words.Count)
                                    {
                                        for (int iZonePointCount = 0; iZonePointCount < iZonePoints && iOffset + 3 <= cpBeforePacketDispatch.Words.Count; iZonePointCount++)
                                        {
                                            points.Add(new Point3D(cpBeforePacketDispatch.Words[iOffset++], cpBeforePacketDispatch.Words[iOffset++], cpBeforePacketDispatch.Words[iOffset++]));
                                        }
                                    }
                                }

                                zones.Add(new MapZoneDrawing(uid, level, tags, points.ToArray(), true));
                            }
                        }

                        if (ListMapZones != null)
                        {
                            this.ListMapZones(this, zones);
                        }

                        blCancelPacket = true;
                    }


                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.account.setPassword", true) == 0 && cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0)
                    {
                        if (RemoteAccountChangePassword != null)
                        {
                            this.RemoteAccountChangePassword(this);
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.account.listLoggedIn", true) == 0)
                    {
                        bool containsUids = (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[1], "uids") == 0);

                        if (RemoteAccountLoggedIn != null)
                        {
                            for (int i = 1; i < cpBeforePacketDispatch.Words.Count; i++)
                            {
                                this.RemoteAccountLoggedIn(this, cpBeforePacketDispatch.Words[i], true);

                                if (containsUids == true && i + 1 < cpBeforePacketDispatch.Words.Count)
                                {
                                    if (m_dicUsernamesToUids.ContainsKey(cpBeforePacketDispatch.Words[i]) == true)
                                    {
                                        m_dicUsernamesToUids[cpBeforePacketDispatch.Words[i]] = cpBeforePacketDispatch.Words[i + 1];
                                    }
                                    else
                                    {
                                        m_dicUsernamesToUids.Add(cpBeforePacketDispatch.Words[i], cpBeforePacketDispatch.Words[i + 1]);
                                    }

                                    i++;
                                }
                            }
                        }

                        blCancelPacket = true;
                    }

                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.plugin.listEnabled", true) == 0)
                    {
                        if (RemoteEnabledPlugins != null)
                        {
                            var lstEnabledPlugins = new List<string>(cpBeforePacketDispatch.Words);
                            lstEnabledPlugins.RemoveAt(0);

                            this.RemoteEnabledPlugins(this, lstEnabledPlugins);
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.vars", true) == 0 && cpBeforePacketDispatch.Words.Count >= 3)
                    {
                        SV_Variables.SetVariable(cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2]);

                        if (ReceiveProconVariable != null)
                        {
                            this.ReceiveProconVariable(this, cpBeforePacketDispatch.Words[1], cpBeforePacketDispatch.Words[2]);
                        }
                        // Dispatch to plugins.
                    }
                    else if (cpRequestPacket.Words.Count >= 1 && String.Compare(cpRequestPacket.Words[0], "procon.plugin.listLoaded", true) == 0)
                    {
                        if (RemoteLoadedPlugins != null)
                        {
                            int i = 0;
                            if (cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[i++], "OK", true) == 0)
                            {
                                var dicLoadedPlugins = new Dictionary<string, PluginDetails>();

                                while (i + 6 <= cpBeforePacketDispatch.Words.Count)
                                {
                                    var spdLoaded = new PluginDetails();

                                    spdLoaded.ClassName = cpBeforePacketDispatch.Words[i++];
                                    spdLoaded.Name = cpBeforePacketDispatch.Words[i++];
                                    spdLoaded.Author = cpBeforePacketDispatch.Words[i++];
                                    spdLoaded.Website = cpBeforePacketDispatch.Words[i++];
                                    spdLoaded.Version = cpBeforePacketDispatch.Words[i++];
                                    spdLoaded.Description = cpBeforePacketDispatch.Words[i++];

                                    spdLoaded.DisplayPluginVariables = new List<CPluginVariable>();
                                    spdLoaded.PluginVariables = new List<CPluginVariable>(); // Not used here.
                                    int iTotalVariables = 0;
                                    if (int.TryParse(cpBeforePacketDispatch.Words[i++], out iTotalVariables) == true && i + (iTotalVariables * 3) <= cpBeforePacketDispatch.Words.Count)
                                    {
                                        for (int x = 0; x < (iTotalVariables * 3); x += 3)
                                        {
                                            spdLoaded.DisplayPluginVariables.Add(new CPluginVariable(cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++], cpBeforePacketDispatch.Words[i++]));
                                        }
                                    }

                                    if (dicLoadedPlugins.ContainsKey(spdLoaded.ClassName) == false)
                                    {
                                        dicLoadedPlugins.Add(spdLoaded.ClassName, spdLoaded);
                                    }
                                }

                                this.RemoteLoadedPlugins(this, dicLoadedPlugins);
                            }
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[0], "login.hashed", true) == 0 && cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "InsufficientPrivileges", true) == 0)
                    {
                        if (LoginFailure != null)
                        {
                            this.LoginFailure(this, cpBeforePacketDispatch.Words[0]);
                        }

                        Shutdown();
                        State = ConnectionState.Error;

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[0], "procon.login.username", true) == 0 && cpBeforePacketDispatch.Words.Count >= 1 && (String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0 || String.Compare(cpBeforePacketDispatch.Words[0], "UnknownCommand", true) == 0))
                    {
                        //this.send(new Packet(true, true, cpBeforePacketDispatch.SequenceNumber, new List<string>() { "procon.login.requestUsername", this.m_strUsername }));

                        // This is the first command we would recieve so now we know we're connected through a PRoCon layer.
                        if (LoginAttempt != null)
                        {
                            this.LoginAttempt(this);
                        }

                        Game.SendLoginHashedPacket(Password);

                        if (String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0)
                        {
                            IsPRoConConnection = true;
                            Game.IsLayered = true;
                        }
                        else
                        {
                            IsPRoConConnection = false;
                            Username = "";
                        }

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[0], "procon.login.username", true) == 0 && cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "InsufficientPrivileges", true) == 0)
                    {
                        // The servers just told us off, try and login normally.
                        if (LoginFailure != null)
                        {
                            this.LoginFailure(this, cpBeforePacketDispatch.Words[0]);
                        }

                        Shutdown();
                        State = ConnectionState.Error;

                        blCancelPacket = true;
                    }
                    else if (cpRequestPacket.Words.Count >= 3 && String.Compare(cpRequestPacket.Words[0], "admin.say", true) == 0 && m_dicForwardedPackets.ContainsKey(cpBeforePacketDispatch.SequenceNumber) == true)
                    {
                        if (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords.Count >= 4 && String.Compare(m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0], "procon.admin.say", true) == 0)
                        {
                            m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0] = "procon.admin.onSay";

                            if (IsPRoConConnection == false)
                            {
                                List<string> lstWords = m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords;

                                if (ProconAdminSaying != null)
                                {
                                    this.ProconAdminSaying(this, lstWords[1], lstWords[2], new CPlayerSubset(lstWords.GetRange(3, lstWords.Count - 3)));
                                }
                            }

                            if (PassLayerEvent != null)
                            {
                                this.PassLayerEvent(this, new Packet(true, false, cpRequestPacket.SequenceNumber, m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords));
                            }

                            // Send to all logged in layer clients
                            m_dicForwardedPackets.Remove(cpBeforePacketDispatch.SequenceNumber);
                            blCancelPacket = true;
                            blCancelUpdateEvent = true;
                        }
                    }
                    else if (cpRequestPacket.Words.Count >= 4 && String.Compare(cpRequestPacket.Words[0], "admin.yell", true) == 0 && m_dicForwardedPackets.ContainsKey(cpBeforePacketDispatch.SequenceNumber) == true)
                    {
                        if (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords.Count >= 5 && String.Compare(m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0], "procon.admin.yell", true) == 0)
                        {
                            m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0] = "procon.admin.onYell";

                            // If we're at the top of the tree, simulate the event coming from a layer above.
                            if (IsPRoConConnection == false)
                            {
                                List<string> lstWords = m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords;

                                int iDisplayDuration = 0;

                                if (int.TryParse(lstWords[3], out iDisplayDuration) == true)
                                {
                                    if (ProconAdminYelling != null)
                                    {
                                        this.ProconAdminYelling(this, lstWords[1], lstWords[2], iDisplayDuration, new CPlayerSubset(lstWords.GetRange(4, lstWords.Count - 4)));
                                    }
                                }
                            }

                            // Send to all logged in layer clients
                            if (PassLayerEvent != null)
                            {
                                this.PassLayerEvent(this, new Packet(true, false, cpRequestPacket.SequenceNumber, m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords));
                            }

                            m_dicForwardedPackets.Remove(cpBeforePacketDispatch.SequenceNumber);
                            blCancelPacket = true;
                            blCancelUpdateEvent = true;
                        }
                    }
                    // MoHW R-6 hack
                    else if (cpRequestPacket.Words.Count >= 3 && Game.GameType == "MOHW" && String.Compare(cpRequestPacket.Words[0], "admin.yell", true) == 0 && m_dicForwardedPackets.ContainsKey(cpBeforePacketDispatch.SequenceNumber) == true)
                    {
                        if (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords.Count >= 4 && String.Compare(m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0], "procon.admin.yell", true) == 0)
                        {
                            m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords[0] = "procon.admin.onYell";

                            // If we're at the top of the tree, simulate the event coming from a layer above.
                            if (IsPRoConConnection == false)
                            {
                                List<string> lstWords = m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords;

                                int iDisplayDuration = 0;
                                if (int.TryParse(lstWords[3], out iDisplayDuration) == true)
                                {
                                    iDisplayDuration = 0;
                                    if (ProconAdminYelling != null)
                                    {
                                        this.ProconAdminYelling(this, lstWords[1], lstWords[2], iDisplayDuration, new CPlayerSubset(lstWords.GetRange(4, lstWords.Count - 4)));
                                    }
                                }
                            }

                            // Send to all logged in layer clients
                            if (PassLayerEvent != null)
                            {
                                this.PassLayerEvent(this, new Packet(true, false, cpRequestPacket.SequenceNumber, m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_lstWords));
                            }

                            m_dicForwardedPackets.Remove(cpBeforePacketDispatch.SequenceNumber);
                            blCancelPacket = true;
                            blCancelUpdateEvent = true;
                        }
                    }
                    // end hack
                    // BF3 player.ping
                    else if (cpRequestPacket.Words.Count >= 2 && Game.GameType == "BF3" && String.Compare(cpRequestPacket.Words[0], "player.ping", true) == 0 && String.Compare(cpBeforePacketDispatch.Words[0], "OK", true) == 0)
                    {
                        string strProconEventsUid = String.Empty;

                        var lstProconUpdatedWords = new List<string>(cpRequestPacket.Words);
                        lstProconUpdatedWords.Insert(0, "procon.admin.onPlayerPinged");
                        lstProconUpdatedWords.RemoveAt(1);
                        lstProconUpdatedWords.Add(cpBeforePacketDispatch.Words[1]);
                        // Now we pass on the packet to all the clients as an event so they can remain in sync.

                        // Don't pass on anything regarding login
                        if ((lstProconUpdatedWords.Count >= 4 && (String.Compare(lstProconUpdatedWords[2], "login.plainText", true) == 0 || String.Compare(lstProconUpdatedWords[2], "login.hashed", true) == 0)) == false)
                        {
                            if (PassLayerEvent != null)
                            {
                                this.PassLayerEvent(this, new Packet(true, false, cpRequestPacket.SequenceNumber, lstProconUpdatedWords));
                            }
                        }
                    }
                    if (blCancelUpdateEvent == false)
                    {
                        string strProconEventsUid = String.Empty;

                        // If a layer client sent this packet..
                        if (m_dicForwardedPackets.ContainsKey(cpBeforePacketDispatch.SequenceNumber) == true)
                        {
                            if (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_sender != null)
                            {
                                (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_sender).Forward(new Packet(false, true, m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_ui32OriginalSequence, new List<string>(cpBeforePacketDispatch.Words)));

                                strProconEventsUid = (m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_sender).ProconEventsUid;

                                InstigatingAccountName = m_dicForwardedPackets[cpBeforePacketDispatch.SequenceNumber].m_sender.Username;
                            }

                            // Unregister the sequence and packet.
                            m_dicForwardedPackets.Remove(cpBeforePacketDispatch.SequenceNumber);
                        }

                        // IF the command was not a request for a list (it's a GET operation only,
                        // as in it only lists or retrieves information and will never be set.)

                        if (Game != null && Game.GetPacketsPattern.IsMatch(cpRequestPacket.ToString()) == false)
                        {
                            // && cpBeforePacketDispatch.Words.Count >= 1 && String.Compare(cpBeforePacketDispatch.Words[0], "OK") == 0) {
                            var lstProconUpdatedWords = new List<string>(cpRequestPacket.Words);
                            lstProconUpdatedWords.Insert(0, "procon.updated");
                            lstProconUpdatedWords.Insert(1, strProconEventsUid);
                            // Now we pass on the packet to all the clients as an event so they can remain in sync.

                            // Don't pass on anything regarding login
                            if ((lstProconUpdatedWords.Count >= 4 && (String.Compare(lstProconUpdatedWords[2], "login.plainText", true) == 0 || String.Compare(lstProconUpdatedWords[2], "login.hashed", true) == 0)) == false)
                            {
                                if (PassLayerEvent != null)
                                {
                                    this.PassLayerEvent(this, new Packet(true, false, cpRequestPacket.SequenceNumber, lstProconUpdatedWords));
                                }
                            }
                        }
                    }
                }
            }

            return blCancelPacket;
        }

        // TO DO: With events most of these can be removed.

        public void Disconnect()
        {
            State = ConnectionState.Disconnected;
            ForceDisconnect();
        }

        public void ForceDisconnect()
        {
            SaveConnectionConfig();

            IsLoadingSavingConnectionConfig = true;

            if (Console != null)
                Console.Logging = false;
            if (EventsLogging != null)
                EventsLogging.Logging = false;
            if (PunkbusterConsole != null)
                PunkbusterConsole.Logging = false;
            if (PluginConsole != null)
                PluginConsole.Logging = false;
            if (ChatConsole != null)
                ChatConsole.Logging = false;

            SendRequest(new List<string>() {
                "quit"
            });

            if (TaskTimer != null)
            {
                TaskTimer.Stop();
            }

            /*
            this.m_isTasksRunning = false;
            if (this.m_thTasks != null) {

                try {
                    this.m_thTasks.Abort();
                }
                catch (Exception) { }

                this.m_thTasks = null;
            }
            */
            if (Layer != null)
            {
                Layer.Shutdown();
            }

            Shutdown();

            IsLoadingSavingConnectionConfig = false;
        }

        public void Destroy()
        {
            //this.SaveConnectionConfig();

            IsLoadingSavingConnectionConfig = true;

            lock (ConfigSavingLocker)
            {
                if (PluginsManager != null)
                {
                    PluginsManager.Unload();
                    PluginsManager = null;
                }
            }

            IsLoadingSavingConnectionConfig = false;
        }

        #region General methods

        public int GetLocalizedTeamNameCount(string mapFilename, string playlist)
        {
            int teamCount = 0;

            foreach (CTeamName team in TeamNameList)
            {
                if (String.IsNullOrEmpty(playlist) == false && String.IsNullOrEmpty(team.Playlist) == false)
                {
                    if (String.Compare(team.MapFilename, mapFilename, true) == 0 && String.Compare(team.Playlist, playlist, true) == 0)
                    {
                        teamCount++;
                    }
                }
                else
                {
                    if (String.Compare(team.MapFilename, mapFilename, true) == 0)
                    {
                        teamCount++;
                    }
                }
            }

            return teamCount;
        }

        public string GetLocalizedTeamName(int teamId, string mapFilename, string playlist)
        {
            string returnName = String.Empty;

            foreach (CTeamName team in TeamNameList)
            {
                if (String.IsNullOrEmpty(playlist) == false && String.IsNullOrEmpty(team.Playlist) == false)
                {
                    if (String.Compare(team.MapFilename, mapFilename, true) == 0 && team.TeamID == teamId && String.Compare(team.Playlist, playlist, true) == 0)
                    {
                        returnName = Language.GetLocalized(team.LocalizationKey, null);
                        break;
                    }
                }
                else
                {
                    if (String.Compare(team.MapFilename, mapFilename, true) == 0 && team.TeamID == teamId)
                    {
                        returnName = Language.GetLocalized(team.LocalizationKey, null);
                        break;
                    }
                }
            }

            return returnName;
        }

        public CMap GetFriendlyMapByFilenamePlayList(string strMapFileName, string strMapPlayList)
        {
            CMap cmReturn = null;
            List<CMap> mapDefines = GetMapDefines();

            if (mapDefines != null)
            {
                foreach (CMap cmMap in mapDefines)
                {
                    if (String.Compare(cmMap.FileName, strMapFileName, true) == 0 && String.Compare(cmMap.PlayList, strMapPlayList, true) == 0)
                    {
                        cmReturn = cmMap;
                        break;
                    }
                }
            }

            return cmReturn;
        }

        public string GetFriendlyGamemodeByMap(string strMapFileName)
        {
            string strFriendlyName = String.Empty;

            if (MapListPool != null)
            {
                foreach (CMap cmMap in MapListPool)
                {
                    if (string.Compare(cmMap.FileName, strMapFileName, true) == 0)
                    {
                        strFriendlyName = cmMap.GameMode;
                        break;
                    }
                }
            }

            return strFriendlyName;
        }

        public string GetFriendlyGamemode(string strPlaylistName)
        {
            string strFriendlyName = String.Empty;

            if (MapListPool != null)
            {
                foreach (CMap cmMap in MapListPool)
                {
                    if (string.Compare(cmMap.PlayList, strPlaylistName, true) == 0)
                    {
                        strFriendlyName = cmMap.GameMode;
                        break;
                    }
                }
            }

            return strFriendlyName;
        }

        public string GetFriendlyMapname(string strMapFileName)
        {
            string strFriendlyName = String.Empty;

            if (MapListPool != null)
            {
                foreach (CMap cmMap in MapListPool)
                {
                    if (string.Compare(cmMap.FileName, strMapFileName, true) == 0)
                    {
                        strFriendlyName = cmMap.PublicLevelName;
                        break;
                    }
                }
            }

            return strFriendlyName;
        }

        public string GetPlaylistByMapname(string strMapFileName)
        {
            string strFriendlyName = String.Empty;

            foreach (CMap cmMap in MapListPool)
            {
                if (string.Compare(cmMap.FileName, strMapFileName, true) == 0)
                {
                    strFriendlyName = cmMap.PlayList;
                    break;
                }
            }

            return strFriendlyName;
        }

        public int GetDefaultSquadIDByMapname(string strMapFileName)
        {
            int iDefaultSquadID = 0;

            foreach (CMap cmMap in MapListPool)
            {
                if (string.Compare(cmMap.FileName, strMapFileName, true) == 0)
                {
                    iDefaultSquadID = cmMap.DefaultSquadID;
                    break;
                }
            }

            return iDefaultSquadID;
        }

        /// <summary>
        ///     Return an arbitrary list of CMap objects in this.MapListPool
        ///     with unique .Gamemode
        /// </summary>
        /// <returns></returns>
        public List<CMap> GetGamemodeList()
        {
            var returnList = new List<CMap>();

            foreach (CMap map in MapListPool)
            {
                bool isGamemodeAdded = false;

                foreach (CMap gamemodeMap in returnList)
                {
                    if (String.Compare(map.GameMode, gamemodeMap.GameMode, true) == 0)
                    {
                        isGamemodeAdded = true;
                    }
                }

                if (isGamemodeAdded == false)
                {
                    returnList.Add(map);
                }
            }

            return returnList;
        }

        #endregion

        #region Playing Sounds

        private readonly SoundPlayer m_spPlayer = new SoundPlayer();
        private bool m_blPlaySound;

        private Thread m_thSound;
        private Thread m_thStopSound;

        public void PlaySound(string strSoundFile, int iRepeat)
        {
            var spsSound = new SPlaySound();
            spsSound.m_iRepeat = iRepeat;
            spsSound.m_strSoundFile = strSoundFile;
            //spsSound.m_spPlayer = this.m_spPlayer;

            if (m_thSound != null)
            {
                StopSound(spsSound);
            }
            else
            {
                m_thSound = new Thread(new ParameterizedThreadStart(PlaySound));
                m_blPlaySound = true;
                m_thSound.Start(spsSound);
            }
        }

        public void StopSound(SPlaySound spsSound)
        {
            m_blPlaySound = false;
            //this.m_spPlayer.Stop();

            if (m_thSound != null)
            {
                m_thStopSound = new Thread(new ParameterizedThreadStart(StopSound));
                m_thStopSound.Start(spsSound);
            }
        }

        private void StopSound(object obj)
        {
            try
            {
                if (m_thSound != null)
                {
                    //this.m_spPlayer.Stop();
                    m_blPlaySound = false;
                    m_thSound.Join();

                    if (obj != null)
                    {
                        m_thSound = new Thread(new ParameterizedThreadStart(PlaySound));
                        m_blPlaySound = true;
                        m_thSound.Start((SPlaySound)obj);
                    }
                    else
                    {
                        m_thSound = null;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private void PlaySound(object obj)
        {
            var spsSound = (SPlaySound)obj;

            try
            {
                using (var brFormatCheck = new BinaryReader(File.Open(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), spsSound.m_strSoundFile), FileMode.Open)))
                {
                    brFormatCheck.BaseStream.Position = 20;
                    Int16 i16Format = brFormatCheck.ReadInt16();

                    if (i16Format == 1 || i16Format == 2)
                    {
                        brFormatCheck.Close();

                        // Load it in this thread in case the file is big.
                        m_spPlayer.SoundLocation = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), spsSound.m_strSoundFile);

                        for (int i = 0; i < spsSound.m_iRepeat && m_blPlaySound == true; i++)
                        {
                            m_spPlayer.PlaySync();
                        }
                    }
                    else
                    {
                        brFormatCheck.Close();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public struct SPlaySound
        {
            public int m_iRepeat;
            public string m_strSoundFile;
        }

        #endregion

        #region Internal Commands

        /*
        public void ProconProtectedWeaponsClear() {
            this.Weapons.Clear();
        }

        public void ProconProtectedWeaponsAdd(Kits restriction, string name, WeaponSlots slot, DamageTypes damage) {
            this.Weapons.Add(new Weapon(restriction, name, slot, damage);
        }

        public void ProconProtectedSpecializationClear() {
            this.Specializations.Clear();
        }

        public void ProconProtectedSpecializationAdd(SpecializationSlots slot, string name) {
            this.Specializations.Add(new Specialization(slot, name));
        }
        */

        public void ProconProtectedTeamNamesClear()
        {
            TeamNameList.Clear();

            foreach (CMap cmMap in MapListPool)
            {
                cmMap.TeamNames.Clear();
            }
        }

        public void ProconProtectedTeamNamesAdd(string strFileName, int iTeamID, string strLocalizationKey, string strImageKey, string playlist = "")
        {
            foreach (CMap cmMap in MapListPool)
            {
                if (String.Compare(cmMap.FileName, strFileName, true) == 0)
                {
                    cmMap.TeamNames.Add(new CTeamName(strFileName, iTeamID, strLocalizationKey, strImageKey, playlist));
                }
            }

            TeamNameList.Add(new CTeamName(strFileName, iTeamID, strLocalizationKey, strImageKey, playlist));
        }

        public void ProconProtectedMapsClear()
        {
            MapListPool.Clear();
        }

        public void ProconProtectedMapsAdd(string strPlaylist, string strFileName, string strGamemode, string strPublicLevelName, int iDefaultSquadID)
        {
            MapListPool.Add(new CMap(strPlaylist, strFileName, strGamemode, strPublicLevelName, iDefaultSquadID));
        }

        public void ProconProtectedReasonsClear()
        {
            Reasons.Clear();
        }

        public void ProconProtectedReasonsAdd(string strReason)
        {
            Reasons.Add(strReason);
        }

        public void ProconProtectedServerVersionsClear(string strType)
        {
            Game.VersionNumberToFriendlyName.Clear();
        }

        public void ProconProtectedServerVersionsAdd(string strType, string strVerNr, string strVerRel)
        {
            Game.VersionNumberToFriendlyName.Add(strVerNr, strVerRel);
            /* Geoff 08/10/2011 - What were these checks for?
            if (strType == "BFBC2")
            {
                this.Game.VersionNumberToFriendlyName.Add(strVerNr, strVerRel);
            }
            if (strType == "MoH")
            {
                this.Game.VersionNumberToFriendlyName.Add(strVerNr, strVerRel);
            }
            */
        }

        public void ProconProtectedPluginEnable(string strClassName, bool blEnabled)
        {
            if (PluginsManager != null && PluginsManager.Plugins.LoadedClassNames.Contains(strClassName) == true)
            {
                if (blEnabled == true)
                {
                    PluginsManager.EnablePlugin(strClassName);
                }
                else
                {
                    PluginsManager.DisablePlugin(strClassName);
                }
            }
        }


        public void ProconProtectedPluginSetVariable(string strClassName, string strVariable, string strValue)
        {
            if (PluginsManager != null)
            {
                PluginsManager.SetPluginVariable(strClassName, strVariable, strValue);
            }
        }

        public void ProconProtectedPluginSetVariableCon(string strClassName, string strVariable, string strValue)
        {
            if (PluginsManager != null)
            {
                PluginsManager.SetPluginVariableCon(strClassName, strVariable, strValue, false);
            }
        }

        public void ProconProtectedLayerSetPrivileges(Account account, CPrivileges sprvPrivileges)
        {
            if (Layer.AccountPrivileges.Contains(account.Name) == true)
            {
                Layer.AccountPrivileges[account.Name].SetPrivileges(sprvPrivileges);
            }
            else
            {
                Layer.AccountPrivileges.Add(new AccountPrivilege(account, sprvPrivileges));
            }
        }

        public void ProconProtectedLayerEnable(bool blEnabled, UInt16 ui16Port, string strBindingAddress, string strLayerName)
        {
            if (Layer != null)
            {
                Layer.IsEnabled = blEnabled;
                //if (this.Layer.LayerEnabled == true) {
                Layer.ListeningPort = ui16Port;
                Layer.BindingAddress = strBindingAddress;
                Layer.NameFormat = strLayerName;
                //}

                // Start it up if we've logged into the bfbc2 server..
                if (Game != null && Game.IsLoggedIn == true)
                {
                    if (Layer.IsEnabled == true && Layer.IsOnline == false)
                    {
                        Layer.Start();
                    }
                    else if (Layer.IsEnabled == false && Layer.IsOnline == true)
                    {
                        Layer.Shutdown();
                    }
                }
            }
            else
            {
                // Magic!!!
            }
        }

        public void ProconPrivateServerConnect()
        {
            Connect();
        }

        #region Tasks

        // Quick and dirty thread safe tasks.
        // private readonly object m_taskLocker = new object();

        public void ProconProtectedTasksList()
        {
            Console.Write("Running Tasks: [Name] [Delay] [Interval] [Repeat] [Command]");

            //lock (this.m_taskLocker) {
            foreach (Task ctTask in new List<Task>(Tasks))
            {
                Console.Write(ctTask.ToString());
            }
            //}

            Console.Write(String.Format("End of Tasks List ({0} Tasks)", Tasks.Count));
        }

        public void ProconProtectedTasksAdd(string strTaskname, List<string> lstCommandWords, int iDelay, int iInterval, int iRepeat)
        {
            if (iDelay >= 0 && iInterval > 0 && iRepeat != 0)
            {
                //lock (this.m_taskLocker) {
                Tasks.Add(new Task(strTaskname, lstCommandWords, iDelay, iInterval, iRepeat));
                //}
            }
        }

        public void ProconProtectedTasksClear()
        {
            //lock (this.m_taskLocker) {
            Tasks.Clear();
            //}
        }

        public void ProconProtectedTasksRemove(string strTaskName)
        {
            //lock (this.m_taskLocker) {
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (String.Compare(Tasks[i].TaskName, strTaskName) == 0)
                {
                    Tasks.RemoveAt(i);
                    i--;
                }
            }
            //}
        }

        private void TaskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (Task ctExecute in new List<Task>(Tasks).Where(ctExecute => Game != null && Game.IsLoggedIn == true && ctExecute.ExecuteCommand == true))
                {
                    Parent.ExecutePRoConCommand(this, ctExecute.Command, 0);
                }

                Tasks.RemoveAll(RepeatTaskDisabled);
            }
            catch (Exception ex)
            {
                // Debug for 0.3.1.0 to make sure this bug is gone.
                FrostbiteConnection.LogError("TaskExecuterThread", String.Empty, ex);
            }
        }

        private static bool RepeatTaskDisabled(Task ctRemoveAll)
        {
            return ctRemoveAll.RemoveTask;
        }

        #endregion

        #endregion

        #region Send Packet Helpers

        public void SendResponse(params string[] words)
        {
            SendResponse(new List<string>(words));
        }

        public void SendResponse(List<string> lstWords)
        {
            if (lstWords.Count > 0)
            {
                if (Game != null && Game.Connection != null)
                {
                    Game.Connection.SendQueued(new Packet(true, true, Game.Connection.AcquireSequenceNumber, lstWords));
                }
            }
        }

        private void SendRequest(params string[] words)
        {
            SendRequest(new List<string>(words));
        }

        public void SendRequest(List<string> words)
        {
            if (IsLoggedIn == true && words.Count > 0)
            {
                if (words.Count >= 4 && String.Compare(words[0], "procon.admin.yell", true) == 0)
                {
                    SendProconAdminYell(words[1], words[2], words[3], words.Count > 4 ? words[4] : String.Empty);
                }
                // obsolete since R-20 and hopefully stays so.
                #region Quick BF3 Hack, now MoHW

                else if (Game is MOHWClient && words.Count >= 4 && String.Compare(words[0], "admin.say", true) == 0 && String.Compare(words[2], "player", true) == 0)
                {
                    if (PlayerList.Contains(words[3]) == true)
                    {
                        CPlayerInfo player = PlayerList[words[3]];

                        words[2] = "squad";
                        words[3] = player.TeamID.ToString();
                        words.Add(player.SquadID.ToString());

                        var pwords = new List<string> {
                            "admin.say",
                            "@" + player.SoldierName,
                            words[2],
                            words[3],
                            words[4]
                        }; // new

                        if (Game != null && Game.Connection != null)
                        {
                            Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, pwords)); // new
                            Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, words));
                        }
                    }
                }
                // MoHW yell hack
                else if (Game is MOHWClient && words.Count >= 4 && String.Compare(words[0], "admin.yell", true) == 0 && String.Compare(words[2], "player", true) == 0)
                {
                    if (PlayerList.Contains(words[3]) == true)
                    {
                        CPlayerInfo player = PlayerList[words[3]];

                        words[2] = "squad";
                        words[3] = player.TeamID.ToString();
                        words.Add(player.SquadID.ToString());

                        var pwords = new List<string> {
                            "admin.yell",
                            "@" + player.SoldierName,
                            words[2],
                            words[3],
                            words[4]
                        };

                        if (Game != null && Game.Connection != null)
                        {
                            Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, pwords));
                            Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, words));
                        }
                    }
                }

                #endregion // END Quick BF3 Hack

                else
                {
                    if (Game != null && Game.Connection != null)
                    {
                        Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, words));
                    }
                }
            }
            else
            {
                int i = 0;
            }
        }

        public void SendPacket(Packet packet)
        {
            Game.Connection.SendQueued(packet);
        }

        #region Procon Extensions

        public void SendProconAdminSay(string strText, string strPlayerSubset, string strTarget)
        {
            if (strTarget.Length > 0)
            {
                SendProconLayerPacket(null, new Packet(false, false, Game.Connection.AcquireSequenceNumber, new List<string>() {
                    "procon.admin.say",
                    String.Empty,
                    strText,
                    strPlayerSubset,
                    strTarget
                }));
            }
            else
            {
                SendProconLayerPacket(null, new Packet(false, false, Game.Connection.AcquireSequenceNumber, new List<string>() {
                    "procon.admin.say",
                    String.Empty,
                    strText,
                    strPlayerSubset
                }));
            }
        }

        public void SendProconAdminYell(string strText, string strDisplayTime, string strPlayerSubset, string strTarget)
        {
            if (strTarget.Length > 0)
            {
                SendProconLayerPacket(null, new Packet(false, false, Game.Connection.AcquireSequenceNumber, new List<string>() {
                    "procon.admin.yell",
                    String.Empty,
                    strText,
                    strDisplayTime,
                    strPlayerSubset,
                    strTarget
                }));
            }
            else
            {
                SendProconLayerPacket(null, new Packet(false, false, Game.Connection.AcquireSequenceNumber, new List<string>() {
                    "procon.admin.yell",
                    String.Empty,
                    strText,
                    strDisplayTime,
                    strPlayerSubset
                }));
            }
        }

        public void SendProconLayerPacket(ILayerClient sender, Packet cpPassOn)
        {
            lock (new object())
            {
                UInt32 ui32MainConnSequence = Game.Connection.AcquireSequenceNumber;

                if (m_dicForwardedPackets.ContainsKey(ui32MainConnSequence) == false)
                {
                    var spopForwardedPacket = new SOriginalForwardedPacket();
                    spopForwardedPacket.m_ui32OriginalSequence = cpPassOn.SequenceNumber;
                    spopForwardedPacket.m_sender = sender;
                    spopForwardedPacket.m_lstWords = new List<string>(cpPassOn.Words);

                    // Register the packet as forwared. 
                    m_dicForwardedPackets.Add(ui32MainConnSequence, spopForwardedPacket);

                    if (cpPassOn.Words.Count >= 5 && String.Compare(cpPassOn.Words[0], "procon.admin.yell") == 0)
                    {
                        if (IsPRoConConnection == false)
                        {
                            if (Game is MOHWClient)
                            {
                                cpPassOn.Words.RemoveAt(3);
                            }
                            // Just yell it, we'll capture it and process the return in OnBeforePacketRecv
                            cpPassOn.Words.RemoveAt(1);
                            cpPassOn.Words[0] = "admin.yell";
                        }
                        // Else forward the packet as is so the layer above can append its username.
                    }
                    else if (cpPassOn.Words.Count >= 4 && String.Compare(cpPassOn.Words[0], "procon.admin.say") == 0)
                    {
                        if (IsPRoConConnection == false)
                        {
                            // Just yell it, we'll capture it and process the return in OnBeforePacketRecv
                            cpPassOn.Words.RemoveAt(1);
                            cpPassOn.Words[0] = "admin.say";
                        }
                        // Else forward the packet as is so the layer above can append its username.
                    }

                    // Now forward the packet.
                    SendPacket(new Packet(false, false, ui32MainConnSequence, cpPassOn.Words));
                }
            }
        }

        public virtual void SendProconLoginUsernamePacket(string username)
        {
            // By pass all usual login checks.
            if (Game != null && Game.Connection != null)
            {
                Game.Connection.SendQueued(new Packet(false, false, Game.Connection.AcquireSequenceNumber, new List<string>() {
                    "procon.login.username",
                    username
                }));
            }
        }

        public virtual void SendProconBattlemapListZonesPacket()
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.battlemap.listZones");
            }
        }

        public virtual void SendGetProconVarsPacket(string variable)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.vars", variable);
            }
        }

        public virtual void SendProconPluginSetVariablePacket(string strClassName, string strVariable, string strValue)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.plugin.setVariable", strClassName, strVariable, strValue);
            }
        }

        public virtual void SendProconPluginEnablePacket(string strClassName, bool blEnabled)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.plugin.enable", strClassName, Packet.Bltos(blEnabled));
            }
        }

        #region Map Zones

        public virtual void SendProconBattlemapModifyZonePointsPacket(string uid, Point3D[] zonePoints)
        {
            if (IsLoggedIn == true)
            {
                var list = new List<string>() {
                    "procon.battlemap.modifyZonePoints",
                    uid
                };
                list.Add(zonePoints.Length.ToString());
                list.AddRange(Point3D.ToStringList(zonePoints));

                SendRequest(list);
            }
        }

        public virtual void SendProconBattlemapDeleteZonePacket(string uid)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.battlemap.deleteZone", uid);
            }
        }

        public virtual void SendProconBattlemapCreateZonePacket(string mapFileName, Point3D[] zonePoints)
        {
            if (IsLoggedIn == true)
            {
                var list = new List<string>() {
                    "procon.battlemap.createZone",
                    mapFileName
                };
                list.Add(zonePoints.Length.ToString());
                list.AddRange(Point3D.ToStringList(zonePoints));

                SendRequest(list);
            }
        }

        public virtual void SendProconBattlemapModifyZoneTagsPacket(string uid, string tagList)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.battlemap.modifyZoneTags", uid, tagList);
            }
        }

        #endregion

        #region Layer

        public virtual void SendProconLayerSetPrivilegesPacket(string username, UInt32 privileges)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.layer.setPrivileges", username, privileges.ToString());
            }
        }

        #endregion

        #region Accounts

        public virtual void SendProconAccountListAccountsPacket()
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.account.listAccounts");
            }
        }

        public virtual void SendProconAccountListLoggedInPacket()
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.account.listLoggedIn", "uids");
            }
        }

        public virtual void SendProconAccountSetPasswordPacket(string username, string password)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.account.setPassword", username, password);
            }
        }

        public virtual void SendProconAccountCreatePacket(string username, string password)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.account.create", username, password);
            }
        }

        public virtual void SendProconAccountDeletePacket(string username)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.account.delete", username);
            }
        }

        #endregion

        #region Plugins

        public virtual void SendProconPluginListLoadedPacket()
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.plugin.listLoaded");
            }
        }

        public virtual void SendProconPluginListEnabledPacket()
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.plugin.listEnabled");
            }
        }

        #endregion

        #region Packages

        public virtual void SendProconPackagesInstallPacket(string uid, string version, string md5)
        {
            if (IsLoggedIn == true)
            {
                SendRequest("procon.packages.install", uid, version, md5);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Game events

        private void PRoConClient_PlayerKilled(FrostbiteClient sender, string strKiller, string strVictim, string strDamageType, bool blHeadshot, Point3D pntKiller, Point3D pntVictim)
        {
            if (PlayerKilled != null)
            {
                CPlayerInfo cpKiller = null, cpVictim = null;

                if (PlayerList.Contains(strKiller) == true)
                {
                    cpKiller = PlayerList[strKiller];
                }
                else
                {
                    cpKiller = new CPlayerInfo(strKiller, String.Empty, 0, 0);
                }

                if (PlayerList.Contains(strVictim) == true)
                {
                    cpVictim = PlayerList[strVictim];
                }
                else
                {
                    cpVictim = new CPlayerInfo(strVictim, String.Empty, 0, 0);
                }

                this.PlayerKilled(this, new Kill(cpKiller, cpVictim, strDamageType, blHeadshot, pntKiller, pntVictim));
            }
        }

        private void PRoConClient_PlayerSpawned(FrostbiteClient sender, string soldierName, string strKit, List<string> lstWeapons, List<string> lstSpecializations)
        {
            if (PlayerSpawned != null)
            {
                if (Enum.IsDefined(typeof(Kits), strKit) == true)
                {
                    var inv = new Inventory((Kits)Enum.Parse(typeof(Kits), strKit));

                    foreach (string strWeapon in lstWeapons)
                    {
                        if (Weapons.Contains(strWeapon) == true)
                        {
                            inv.Weapons.Add(Weapons[strWeapon]);
                        }
                    }

                    foreach (string strSpecialization in lstSpecializations)
                    {
                        if (Specializations.Contains(strSpecialization) == true)
                        {
                            inv.Specializations.Add(Specializations[strSpecialization]);
                        }
                    }

                    this.PlayerSpawned(this, soldierName, inv);
                }
                else
                {
                    this.PlayerSpawned(this, soldierName, new Inventory(Kits.None));
                }
            }
        }

        private void PRoConClient_ReservedSlotsPlayerRemoved(FrostbiteClient sender, string strSoldierName)
        {
            if (ReservedSlotList.Contains(strSoldierName) == true)
            {
                ReservedSlotList.Remove(strSoldierName);
            }
        }

        private void PRoConClient_ReservedSlotsPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            if (ReservedSlotList.Contains(strSoldierName) == false)
            {
                ReservedSlotList.Add(strSoldierName);
            }
        }

        private void PRoConClient_ReservedSlotsList(FrostbiteClient sender, List<string> soldierNames)
        {
            if (sender is BFHLClient || sender is BF4Client || sender is BF3Client || sender is MOHWClient)
            {
                ReservedSlotList.Clear();
            }

            if (soldierNames.Count != 0)
            {
                foreach (string strSoldierName in soldierNames)
                {
                    if (ReservedSlotList.Contains(strSoldierName) == false)
                    {
                        ReservedSlotList.Add(strSoldierName);
                    }
                }

                foreach (string strSoldierName in ReservedSlotList)
                {
                    if (soldierNames.Contains(strSoldierName) == false)
                    {
                        ReservedSlotList.Remove(strSoldierName);
                    }
                }
            }
        }

        private void PRoConClient_SpectatorListPlayerRemoved(FrostbiteClient sender, string strSoldierName)
        {
            if (SpectatorList.Contains(strSoldierName) == true)
            {
                SpectatorList.Remove(strSoldierName);
            }
        }

        private void PRoConClient_SpectatorListPlayerAdded(FrostbiteClient sender, string strSoldierName)
        {
            if (SpectatorList.Contains(strSoldierName) == false)
            {
                SpectatorList.Add(strSoldierName);
            }
        }

        private void PRoConClient_SpectatorListList(FrostbiteClient sender, List<string> soldierNames)
        {
            if (sender is BFHLClient || sender is BF4Client)
            {
                SpectatorList.Clear();
            }

            if (soldierNames.Count != 0)
            {
                foreach (string strSoldierName in soldierNames)
                {
                    if (SpectatorList.Contains(strSoldierName) == false)
                    {
                        SpectatorList.Add(strSoldierName);
                    }
                }

                foreach (string strSoldierName in ReservedSlotList)
                {
                    if (soldierNames.Contains(strSoldierName) == false)
                    {
                        SpectatorList.Remove(strSoldierName);
                    }
                }
            }
        }

        protected void OnServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            GameMods oldGameMod = CurrentServerInfo != null ? CurrentServerInfo.GameMod : GameMods.None;

            // Initial loading..
            if (CurrentServerInfo == null)
            {
                CurrentServerInfo = csiServerInfo;

                InitialSetup();

                if (GameTypeDiscovered != null)
                {
                    this.GameTypeDiscovered(this);
                }
            }
            else if (CurrentServerInfo != null && oldGameMod != csiServerInfo.GameMod)
            {
                IsGameModModified = true;
            }
            else
            {
                IsGameModModified = false;
            }

            CurrentServerInfo = csiServerInfo;
        }

        protected void OnLogout()
        {
            Shutdown();
        }

        protected void OnPlayerLeft(FrostbiteClient sender, string strSoldierName, CPlayerInfo cpiPlayer)
        {
            if (PlayerList.Contains(strSoldierName) == true)
            {
                PlayerList.Remove(strSoldierName);
            }
        }

        protected void OnPlayerDisconnected(FrostbiteClient sender, string strSoldierName, string reason)
        {
            if (PlayerList.Contains(strSoldierName) == true)
            {
                PlayerList.Remove(strSoldierName);
            }
        }

        protected void OnPunkbusterMessage(FrostbiteClient sender, string strPunkbusterMessage)
        {
            strPunkbusterMessage = strPunkbusterMessage.TrimEnd('\r', '\n');

            // PunkBuster Server: ([0-9]+)[ ]? ([A-Za-z0-9]+)\(.*?\) ([0-9\.:]+).*?\(.*?\) "(.*?)"
            // PunkBuster Server: 1  2c90591ce08a5f799622705d7ba1155c(-) 192.168.1.3:52460 OK   1 3.0 0 (W) "(U3)Phogue"
            //Match mMatch = Regex.Match(strPunkbusterMessage, @":[ ]+?(?<slotid>[0-9]+)[ ]+?(?<guid>[A-Za-z0-9]+)\(.*?\)[ ]+?(?<ip>[0-9\.:]+).*?\(.*?\)[ ]+?""(?<name>.*?)\""", RegexOptions.IgnoreCase);
            Match mMatch = Parent.RegexMatchPunkbusterPlist.Match(strPunkbusterMessage);
            // If it is a punkbuster pb_plist update
            if (mMatch.Success == true && mMatch.Groups.Count >= 5)
            {
                string pbip = mMatch.Groups["ip"].Value;
                var newPbInfo = new CPunkbusterInfo(mMatch.Groups["slotid"].Value, mMatch.Groups["name"].Value, mMatch.Groups["guid"].Value, pbip, Parent.GetCountryName(pbip), Parent.GetCountryCode(pbip));

                // if the proxy checker doesn't have the record
                // call for fetch, pass in the client and new pb info
                // when the check returns, update the pbinfo object and call the below line again to update the clients
                
                if (PunkbusterPlayerInfo != null)
                {
                    this.PunkbusterPlayerInfo(this, newPbInfo);
                }
            }

            mMatch = Parent.RegexMatchPunkbusterBeginPlist.Match(strPunkbusterMessage);
            if (mMatch.Success == true && PunkbusterBeginPlayerInfo != null)
            {
                this.PunkbusterBeginPlayerInfo(this);
            }

            mMatch = Parent.RegexMatchPunkbusterEndPlist.Match(strPunkbusterMessage);
            if (mMatch.Success == true && PunkbusterEndPlayerInfo != null)
            {
                this.PunkbusterEndPlayerInfo(this);
            }

            // PunkBuster Server: Player Guid Computed ([A-Za-z0-9]+)\(.*?\) \(slot #([0-9]+)\) ([0-9\.:]+) (.*)
            // PunkBuster Server: Player Guid Computed 2c90591ce08a5f799622705d7ba1155c(-) (slot #1) 192.168.1.3:52581 (U3)Phogue
            //mMatch = Regex.Match(strPunkbusterMessage, @": Player Guid Computed[ ]+?(?<guid>[A-Za-z0-9]+)\(.*?\)[ ]+?\(slot #(?<slotid>[0-9]+)\)[ ]+?(?<ip>[0-9\.:]+)[ ]+?(?<name>.*)", RegexOptions.IgnoreCase);
            mMatch = Parent.RegexMatchPunkbusterGuidComputed.Match(strPunkbusterMessage);
            // If it is a new connection, technically its a resolved guid type command but stil..
            if (mMatch.Success == true && mMatch.Groups.Count >= 5)
            {
                var newPbInfo = new CPunkbusterInfo(mMatch.Groups["slotid"].Value, mMatch.Groups["name"].Value, mMatch.Groups["guid"].Value, mMatch.Groups["ip"].Value, Parent.GetCountryName(mMatch.Groups["ip"].Value), Parent.GetCountryCode(mMatch.Groups["ip"].Value));

                if (PunkbusterPlayerInfo != null)
                {
                    this.PunkbusterPlayerInfo(this, newPbInfo);
                }
            }

            //mMatch = Regex.Match(strPunkbusterMessage, @":[ ]+?(?<banid>[0-9]+)[ ]+?(?<guid>[A-Za-z0-9]+)[ ]+?{(?<remaining>[0-9\-]+)/(?<banlength>[0-9\-]+)}[ ]+?""(?<name>.+?)""[ ]+?""(?<ip>.+?)""[ ]+?(?<reason>.*)", RegexOptions.IgnoreCase);
            mMatch = Parent.RegexMatchPunkbusterBanlist.Match(strPunkbusterMessage);
            if (mMatch.Success == true && mMatch.Groups.Count >= 5)
            {
                //IPAddress ipOut;
                string strIP = String.Empty;
                string[] a_strIP;

                if (mMatch.Groups["ip"].Value.Length > 0 && (a_strIP = mMatch.Groups["ip"].Value.Split(':')).Length > 0)
                {
                    strIP = a_strIP[0];
                }

                var newPbBanInfo = new CBanInfo(mMatch.Groups["name"].Value, mMatch.Groups["guid"].Value, mMatch.Groups["ip"].Value, new TimeoutSubset(mMatch.Groups["banlength"].Value, mMatch.Groups["remaining"].Value), mMatch.Groups["reason"].Value);

                if (PunkbusterPlayerBanned != null)
                {
                    this.PunkbusterPlayerBanned(this, newPbBanInfo);
                }
            }

            //PunkBuster Server: Kick/Ban Command Issued (testing) for (slot#1) xxx.xxx.xxx.xxx:yyyy GUID name
            mMatch = Parent.RegexMatchPunkbusterKickBanCmd.Match(strPunkbusterMessage);
            if (mMatch.Success == true && mMatch.Groups.Count >= 5)
            {
                //IPAddress ipOut;
                string strIP = String.Empty;
                string[] a_strIP;

                if (mMatch.Groups["ip"].Value.Length > 0 && (a_strIP = mMatch.Groups["ip"].Value.Split(':')).Length > 0)
                {
                    strIP = a_strIP[0];
                }

                TimeoutSubset kb_timeoutSubset;
                if (String.Compare(mMatch.Groups["kb_type"].ToString(), "Kick/Ban", true) == 0)
                {
                    kb_timeoutSubset = new TimeoutSubset(new List<string>() {
                        "perm",
                        ""
                    });
                }
                else
                {
                    kb_timeoutSubset = new TimeoutSubset(new List<string>() {
                        "seconds",
                        "120"
                    });
                }

                //CBanInfo newPbBanInfo = new CBanInfo(mMatch.Groups["name"].Value, mMatch.Groups["guid"].Value, mMatch.Groups["ip"].Value, new TimeoutSubset("perm",""), mMatch.Groups["reason"].Value);
                var newPbBanInfo = new CBanInfo(mMatch.Groups["name"].Value, mMatch.Groups["guid"].Value, mMatch.Groups["ip"].Value, kb_timeoutSubset, mMatch.Groups["reason"].Value);

                if (PunkbusterPlayerBanned != null)
                {
                    this.PunkbusterPlayerBanned(this, newPbBanInfo);
                }
            }

            //mMatch = Regex.Match(strPunkbusterMessage, @":[ ]+?Guid[ ]+?(?<guid>[A-Za-z0-9]+)[ ]+?has been Unbanned", RegexOptions.IgnoreCase);
            mMatch = Parent.RegexMatchPunkbusterUnban.Match(strPunkbusterMessage);
            // If it is a new connection, technically its a resolved guid type command but stil..
            if (mMatch.Success == true && mMatch.Groups.Count >= 2)
            {
                var cbiUnbannedPlayer = new CBanInfo(String.Empty, mMatch.Groups["guid"].Value, String.Empty, new TimeoutSubset(TimeoutSubset.TimeoutSubsetType.None), String.Empty);

                if (PunkbusterPlayerUnbanned != null)
                {
                    this.PunkbusterPlayerUnbanned(this, cbiUnbannedPlayer);
                }
            }

            //mMatch = Regex.Match(strPunkbusterMessage, @": Ban Added to Ban List", RegexOptions.IgnoreCase);
            mMatch = Parent.RegexMatchPunkbusterBanAdded.Match(strPunkbusterMessage);
            if (mMatch.Success == true && mMatch.Groups.Count >= 5)
            {
                SendRequest(new List<string>() {
                    "punkBuster.pb_sv_command",
                    Variables.GetVariable("PUNKBUSTER_BANLIST_REFRESH", "pb_sv_banlist BC2! ")
                });
            }
        }

        protected void OnListPlayers(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                // Add or update players.
                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    if (PlayerList.Contains(cpiPlayer.SoldierName) == true)
                    {
                        CPlayerInfo storedPlayer = PlayerList[PlayerList.IndexOf(PlayerList[cpiPlayer.SoldierName])];
                        cpiPlayer.JoinTime = CurrentServerInfo.ServerUptime;
                        if (storedPlayer.JoinTime == 0) { storedPlayer.JoinTime = CurrentServerInfo.ServerUptime; }
                        if (storedPlayer.JoinTime <= CurrentServerInfo.ServerUptime)
                        {
                            cpiPlayer.SessionTime = CurrentServerInfo.ServerUptime - storedPlayer.JoinTime;
                            cpiPlayer.JoinTime = storedPlayer.JoinTime;
                        }

                        PlayerList[PlayerList.IndexOf(PlayerList[cpiPlayer.SoldierName])] = cpiPlayer;
                    }
                    else
                    {
                        cpiPlayer.JoinTime = CurrentServerInfo.ServerUptime;
                        PlayerList.Add(cpiPlayer);
                    }
                }

                var recievedPlayerList = new PlayerDictionary(lstPlayers);
                foreach (CPlayerInfo storedPlayer in new List<CPlayerInfo>(PlayerList))
                {
                    // If the stored player is not in the list we recieved
                    if (recievedPlayerList.Contains(storedPlayer.SoldierName) == false)
                    {
                        // They have left the server, remove them from the master stored list.
                        PlayerList.Remove(storedPlayer.SoldierName);
                    }
                }

                if (this.IsPRoConConnection == true && this.PlayTimesSynced == false)
                {
                    SendRequest(new List<string>() {
                        "procon.player.syncPlayTimes"
                    });
                    this.PlayTimesSynced = true;
                }
            }
        }

        private void PRoConClient_BanListList(FrostbiteClient sender, int iStartOffset, List<CBanInfo> lstBans)
        {
            if (iStartOffset == 0)
            {
                FullVanillaBanList.Clear();
            }

            if (lstBans.Count > 0)
            {
                FullVanillaBanList.AddRange(lstBans);

                Game.SendBanListListPacket(iStartOffset + 100);
                //this.SendRequest(new List<string>() { "banList.list", (iStartOffset + 100).ToString() });
            }
            else
            {
                // We have recieved the whole banlist in 100 ban increments.. throw event.
                if (FullBanListList != null)
                {
                    this.FullBanListList(this, FullVanillaBanList);
                }
            }
        }

        private void PRoConClient_ResponseError(FrostbiteClient sender, Packet originalRequest, string errorMessage)
        {
            // Banlist backwards compatability with R11 (Will attempt to get "banList.list 100" which will throw this error)
            if (originalRequest.Words.Count > 0 && String.Compare(originalRequest.Words[0], "banList.list", true) == 0 && String.Compare(errorMessage, "InvalidArguments", true) == 0)
            {
                if (FullBanListList != null)
                {
                    this.FullBanListList(this, FullVanillaBanList);
                }
            }
        }

        protected void OnPlayerLimit(FrostbiteClient sender, int iPlayerLimit)
        {
            // Quick 'hack' because BF3 does not have these and it's generating confusion on the forums.
            if (!(Game is BFHLClient) && !(Game is BF4Client) && !(Game is BF3Client) && !(Game is MOHWClient))
            {
                SendRequest(new List<string>() {
                    "vars.currentPlayerLimit"
                });
                SendRequest(new List<string>() {
                    "vars.maxPlayerLimit"
                });
            }

            SendRequest(new List<string>() {
                "serverInfo"
            });
        }

        #region Text Chat Moderation

        // FullTextChatModerationList
        private void Game_TextChatModerationListList(FrostbiteClient sender, int startOffset, List<TextChatModerationEntry> textChatModerationList)
        {
            if (startOffset == 0)
            {
                FullTextChatModerationList.Clear();
            }

            if (textChatModerationList.Count > 0)
            {
                FullTextChatModerationList.AddRange(textChatModerationList);

                Game.SendTextChatModerationListListPacket(startOffset + 100);
                //this.SendRequest(new List<string>() { "banList.list", (startOffset + 100).ToString() });
            }
            else
            {
                // We have recieved the whole banlist in 100 ban increments.. throw event.
                if (FullTextChatModerationListList != null)
                {
                    this.FullTextChatModerationListList(this, FullTextChatModerationList);
                }
            }
        }

        private void Game_TextChatModerationListClear(FrostbiteClient sender)
        {
            FullTextChatModerationList.Clear();
        }

        private void Game_TextChatModerationListRemovePlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            FullTextChatModerationList.RemoveEntry(playerEntry);
        }

        private void Game_TextChatModerationListAddPlayer(FrostbiteClient sender, TextChatModerationEntry playerEntry)
        {
            FullTextChatModerationList.AddEntry(playerEntry);
        }

        #endregion

        #endregion

        #region Mapzones

        private void MapZones_MapZoneRemoved(MapZoneDrawing item)
        {
            SaveConnectionConfig();
        }

        private void MapZones_MapZoneChanged(MapZoneDrawing item)
        {
            SaveConnectionConfig();
        }

        private void MapZones_MapZoneAdded(MapZoneDrawing item)
        {
            SaveConnectionConfig();
        }

        #endregion

        #region Layer Events

        private void Layer_LayerOffline()
        {
            SaveConnectionConfig();
        }

        private void Layer_LayerOnline()
        {
            SaveConnectionConfig();
        }

        private void AccountPrivileges_AccountPrivilegeRemoved(AccountPrivilege item)
        {
            item.AccountPrivilegesChanged -= new AccountPrivilege.AccountPrivilegesChangedHandler(item_AccountPrivilegesChanged);

            SaveConnectionConfig();
        }

        private void AccountPrivileges_AccountPrivilegeAdded(AccountPrivilege item)
        {
            item.AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(item_AccountPrivilegesChanged);

            SaveConnectionConfig();
        }

        private void item_AccountPrivilegesChanged(AccountPrivilege item)
        {
            SaveConnectionConfig();
        }

        #endregion

        #region Saving/Loading Settings

        // Set to true just before the config is open so events thrown during the loading process
        // don't trigger a save.
        public void SaveConnectionConfig()
        {
            if (IsLoadingSavingConnectionConfig == false && Layer != null && Layer.AccountPrivileges != null && (PluginsManager != null || (PluginsManager == null && IsPRoConConnection == true && Parent.OptionsSettings.LayerHideLocalPlugins == true)) && MapGeometry != null && MapGeometry.MapZones != null)
            {
                lock (ConfigSavingLocker)
                {
                    FileStream stmConnectionConfigFile = null;
                    string configDirectoryPath = null;

                    try
                    {
                        configDirectoryPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), FileHostNamePort);

                        if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs")) == false)
                        {
                            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"));
                        }

                        if (Directory.Exists(configDirectoryPath) == false)
                        {
                            Directory.CreateDirectory(configDirectoryPath);
                        }

                        string strSaveFile = Path.Combine(configDirectoryPath, string.Format("{0}.cfg", FileHostNamePort));

                        stmConnectionConfigFile = new FileStream(strSaveFile + ".temp", FileMode.Create);

                        if (stmConnectionConfigFile.CanWrite == true)
                        {
                            var stwConfig = new StreamWriter(stmConnectionConfigFile, Encoding.UTF8);

                            stwConfig.WriteLine("/////////////////////////////////////////////");
                            stwConfig.WriteLine("// This config will be overwritten by procon.");
                            stwConfig.WriteLine("/////////////////////////////////////////////");

                            foreach (AccountPrivilege apPrivs in Layer.AccountPrivileges)
                            {
                                stwConfig.WriteLine("procon.protected.layer.setPrivileges \"{0}\" {1}", apPrivs.Owner.Name, apPrivs.Privileges.PrivilegesFlags);
                            }

                            stwConfig.WriteLine("procon.protected.layer.enable {0} {1} \"{2}\" \"{3}\"", Layer.IsEnabled, Layer.ListeningPort, Layer.BindingAddress, Layer.NameFormat);

                            stwConfig.WriteLine("procon.protected.playerlist.settings " + String.Join(" ", PlayerListSettings.Settings.ToArray()));
                            stwConfig.WriteLine("procon.protected.chat.settings " + String.Join(" ", ChatConsole.Settings.ToArray()));
                            stwConfig.WriteLine("procon.protected.events.captures " + String.Join(" ", EventsLogging.Settings.ToArray()));
                            stwConfig.WriteLine("procon.protected.lists.settings " + String.Join(" ", ListSettings.Settings.ToArray()));
                            stwConfig.WriteLine("procon.protected.console.settings " + String.Join(" ", Console.Settings.ToArray()));
                            stwConfig.WriteLine("procon.protected.timezone_UTCoffset " + Game.UtcOffset);

                            foreach (MapZoneDrawing zone in MapGeometry.MapZones)
                            {
                                stwConfig.WriteLine("procon.protected.zones.add \"{0}\" \"{1}\" \"{2}\" {3} {4}", zone.UID, zone.LevelFileName, zone.Tags, zone.ZonePolygon.Length, String.Join(" ", Point3D.ToStringList(zone.ZonePolygon).ToArray()));
                            }

                            stwConfig.Flush();
                            stwConfig.Close();

                            File.Copy(strSaveFile + ".temp", strSaveFile, true);
                            File.Delete(strSaveFile + ".temp");
                        }
                    }
                    catch (Exception e)
                    {
                        FrostbiteConnection.LogError("SaveConnectionConfig", String.Empty, e);
                    }
                    finally
                    {
                        if (stmConnectionConfigFile != null)
                        {
                            stmConnectionConfigFile.Close();
                            stmConnectionConfigFile.Dispose();
                        }
                    }

                    if (PluginsManager != null)
                    {
                        foreach (Plugin.Plugin plugin in PluginsManager.Plugins)
                        {
                            FileStream pluginConfigFileStream = null;
                            string pluginConfigPath = Path.Combine(configDirectoryPath, string.Format("{0}.cfg", plugin.ClassName));

                            try
                            {
                                pluginConfigFileStream = new FileStream(string.Format("{0}.temp", pluginConfigPath), FileMode.Create);

                                if (pluginConfigFileStream.CanWrite == true)
                                {
                                    StreamWriter pluginConfigWriter = new StreamWriter(pluginConfigFileStream, Encoding.UTF8);

                                    pluginConfigWriter.WriteLine("/////////////////////////////////////////////");
                                    pluginConfigWriter.WriteLine("// This config will be overwritten by procon.");
                                    pluginConfigWriter.WriteLine("/////////////////////////////////////////////");

                                    pluginConfigWriter.WriteLine("procon.protected.plugins.enable \"{0}\" {1}", plugin.ClassName, plugin.IsEnabled);

                                    if (plugin.IsLoaded == true)
                                    {
                                        PluginDetails pluginDetails = PluginsManager.GetPluginDetails(plugin.ClassName);

                                        foreach (CPluginVariable pluginVariable in pluginDetails.PluginVariables)
                                        {
                                            string escapedNewlines = CPluginVariable.Decode(pluginVariable.Value).Replace("\n", @"\n").Replace("\r", @"\r").Replace("\"", @"\""");

                                            pluginConfigWriter.WriteLine("procon.protected.plugins.setVariable \"{0}\" \"{1}\" \"{2}\"", plugin.ClassName, pluginVariable.Name, escapedNewlines);
                                        }
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, string> cachedPluginVariable in plugin.CacheFailCompiledPluginVariables)
                                        {
                                            pluginConfigWriter.WriteLine("procon.protected.plugins.setVariable \"{0}\" \"{1}\" \"{2}\"", plugin.ClassName, cachedPluginVariable.Key, cachedPluginVariable.Value);
                                        }
                                    }

                                    pluginConfigWriter.Flush();
                                    pluginConfigWriter.Close();

                                    File.Copy(string.Format("{0}.temp", pluginConfigPath), pluginConfigPath, true);
                                    File.Delete(string.Format("{0}.temp", pluginConfigPath));
                                }
                            }
                            catch (Exception e)
                            {
                                FrostbiteConnection.LogError("SaveConnectionConfig", plugin.ClassName, e);
                            }
                            finally
                            {
                                if (pluginConfigFileStream != null)
                                {
                                    pluginConfigFileStream.Close();
                                    pluginConfigFileStream.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ExecuteConnectionConfig(string strConfigFile, int iRecursion, List<string> lstArguments, bool blIncPlugin)
        {
            //FileStream stmConfigFile = null;
            try
            {
                if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile)) == true)
                {
                    //stmConfigFile = new FileStream(String.Format(@"{0}Configs\{1}", AppDomain.CurrentDomain.BaseDirectory, strConfigFile), FileMode.Open);

                    string[] a_strConfigData = File.ReadAllLines(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile));

                    if (a_strConfigData != null)
                    {
                        foreach (string strLine in a_strConfigData)
                        {
                            if (strLine.Length > 0 && Regex.Match(strLine, "^[ ]+//.*").Success == false)
                            {
                                // AND not a comment..

                                if (lstArguments != null)
                                {
                                    string strReplacedLine = strLine;

                                    for (int i = 0; i < lstArguments.Count; i++)
                                    {
                                        strReplacedLine = strReplacedLine.Replace(String.Format("%arg:{0}%", i), lstArguments[i]);
                                    }

                                    List<string> lstWordifiedCommand = Packet.Wordify(strReplacedLine);
                                    // procon.protected.config.demand 48543 CanKickPlayers procon.protected.send admin.say "You're not allowed to kick" player phogue
                                    if (lstWordifiedCommand.Count >= 3 && String.Compare(lstWordifiedCommand[0], "procon.protected.config.demand", true) == 0)
                                    {
                                        UInt32 ui32PrivilegesFlags = 0;

                                        if (UInt32.TryParse(lstWordifiedCommand[1], out ui32PrivilegesFlags) == true)
                                        {
                                            var cpConfigPrivs = new CPrivileges(ui32PrivilegesFlags);
                                            bool blHasPrivileges = false;

                                            try
                                            {
                                                PropertyInfo[] a_piAllProperties = cpConfigPrivs.GetType().GetProperties();

                                                foreach (PropertyInfo pInfo in a_piAllProperties)
                                                {
                                                    if (String.Compare(pInfo.GetGetMethod().Name, "get_" + lstWordifiedCommand[2]) == 0)
                                                    {
                                                        blHasPrivileges = (bool)pInfo.GetValue(cpConfigPrivs, null);
                                                        break;
                                                    }
                                                }

                                                if (blHasPrivileges == false)
                                                {
                                                    // If they have asked for a command on failure..
                                                    if (lstWordifiedCommand.Count > 3)
                                                    {
                                                        if (blIncPlugin == true)
                                                        {
                                                            Parent.ExecutePRoConCommandCon(this, lstWordifiedCommand.GetRange(3, lstWordifiedCommand.Count - 3), iRecursion++);
                                                        }
                                                        else
                                                        {
                                                            Parent.ExecutePRoConCommand(this, lstWordifiedCommand.GetRange(3, lstWordifiedCommand.Count - 3), iRecursion++);
                                                        }
                                                    }

                                                    // Cancel execution of the config file, they don't have the demanded privileges.
                                                    break;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                FrostbiteConnection.LogError("Parsing a config.", String.Empty, e);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // Cancel execution of the config file, wrong format for demand.
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (blIncPlugin == true)
                                        {
                                            Parent.ExecutePRoConCommandCon(this, lstWordifiedCommand, iRecursion++);
                                        }
                                        else
                                        {
                                            Parent.ExecutePRoConCommand(this, lstWordifiedCommand, iRecursion++);
                                        }
                                    }
                                }
                                else
                                {
                                    if (blIncPlugin == true)
                                    {
                                        Parent.ExecutePRoConCommandCon(this, Packet.Wordify(strLine), iRecursion++);
                                    }
                                    else
                                    {
                                        Parent.ExecutePRoConCommand(this, Packet.Wordify(strLine), iRecursion++);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError("ExecuteConnectionConfig", String.Empty, e);
            }
        }

        public void ExecuteGlobalVarsConfig(string strConfigFile, int iRecursion, List<string> lstArguments)
        {
            //FileStream stmConfigFile = null;
            try
            {
                if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile)) == true)
                {
                    string[] a_strConfigData = File.ReadAllLines(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile));

                    if (a_strConfigData != null)
                    {
                        foreach (string strLine in a_strConfigData)
                        {
                            if (strLine.Length > 0 && Regex.Match(strLine, "^[ ]+//.*").Success == false && Regex.Match(strLine, "^procon.protected.vars.set .*").Success)
                            {
                                // AND not a comment..
                                Parent.ExecutePRoConCommand(this, Packet.Wordify(strLine), iRecursion++);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError("ExecuteConnectionConfig", String.Empty, e);
            }
        }

        #endregion

        #region Plugin setup & events

        public void CompilePlugins(PermissionSet prmPluginSandbox)
        {
            var dicClassSavedVariables = new Dictionary<string, List<CPluginVariable>>();
            List<string> lstEnabledPlugins = null;
            List<String> ignoredPluginClassNames = null;

            // If it's a recompile save all the current variables.
            if (PluginsManager != null)
            {
                foreach (String strClassName in PluginsManager.Plugins.LoadedClassNames)
                {
                    if (dicClassSavedVariables.ContainsKey(strClassName) == false)
                    {
                        dicClassSavedVariables.Add(strClassName, PluginsManager.GetPluginVariables(strClassName));
                        //dicClassSavedVariables.Add(strClassName, ProConClient.GetSvVariables(this.m_cpPlugins.InvokeOnLoaded(strClassName, "GetPluginVariables", null)));
                    }
                }

                lstEnabledPlugins = PluginsManager.Plugins.EnabledClassNames;
                ignoredPluginClassNames = PluginsManager.IgnoredPluginClassNames;

                PluginsManager.Unload();

                PluginsManager = new PluginManager(this);
                if (RecompilingPlugins != null)
                {
                    this.RecompilingPlugins(this);
                }
            }
            else
            {
                PluginsManager = new PluginManager(this);
                if (CompilingPlugins != null)
                {
                    this.CompilingPlugins(this);
                }
            }

            PluginsManager.CompilePlugins(prmPluginSandbox, ignoredPluginClassNames);

            if (PluginsCompiled != null)
            {
                this.PluginsCompiled(this);
            }

            // Reload all the variables if it's a recompile
            foreach (var kvpPluginVariables in dicClassSavedVariables)
            {
                foreach (CPluginVariable cpvVariable in kvpPluginVariables.Value)
                {
                    PluginsManager.SetPluginVariable(kvpPluginVariables.Key, cpvVariable.Name, cpvVariable.Value);
                    //this.Plugins.InvokeOnLoaded(kvpPluginVariables.Key, "SetPluginVariable", new object[] { cpvVariable.Name, cpvVariable.Value });
                }
            }

            if (lstEnabledPlugins != null)
            {
                foreach (string strEnabledClass in lstEnabledPlugins)
                {
                    PluginsManager.EnablePlugin(strEnabledClass);
                }
            }
        }

        private void PluginsManager_PluginPanic()
        {
            CompilePlugins(Parent.OptionsSettings.PluginPermissions);
        }

        public List<CMap> GetMapDefines()
        {
            return new List<CMap>(MapListPool);
        }

        public bool TryGetLocalized(string languageCode, out string localizedText, string variable, string[] arguements)
        {
            bool isSuccess = false;
            localizedText = String.Empty;

            if (languageCode == null)
            {
                isSuccess = Language.TryGetLocalized(out localizedText, variable, arguements);
            }
            else
            {
                string strCountryCode = String.Empty;

                if (Parent.Languages.Any(languageFile => languageFile.TryGetLocalized(out strCountryCode, "file.countrycode") == true && String.Compare(strCountryCode, languageCode, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    isSuccess = Language.TryGetLocalized(out localizedText, variable, arguements);
                }
            }

            return isSuccess;
        }

        public string GetVariable(string strVariable)
        {
            return Variables.GetVariable(strVariable, String.Empty);
        }

        public string GetSvVariable(string strVariable)
        {
            return SV_Variables.GetVariable(strVariable, String.Empty);
        }

        public CPrivileges GetAccountPrivileges(string strAccountName)
        {
            CPrivileges spReturn = default(CPrivileges);

            if (Layer.AccountPrivileges.Contains(strAccountName) == true)
            {
                spReturn = Layer.AccountPrivileges[strAccountName].Privileges;
            }

            return spReturn;
        }

        public void ExecuteCommand(List<string> lstCommand)
        {
            if (lstCommand != null)
            {
                Parent.ExecutePRoConCommand(this, lstCommand, 0);
            }
        }

        public WeaponDictionary GetWeaponDefines()
        {
            return (WeaponDictionary)Weapons.Clone();
        }

        public SpecializationDictionary GetSpecializationDefines()
        {
            return (SpecializationDictionary)Specializations.Clone();
        }

        #endregion
    }
}