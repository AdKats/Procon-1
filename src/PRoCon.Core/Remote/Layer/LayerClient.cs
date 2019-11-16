using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using PRoCon.Core.Remote.Layer.PacketDispatchers;

namespace PRoCon.Core.Remote.Layer {
    using Core;
    using Core.Plugin;
    using Core.Accounts;
    using Core.Battlemap;
    using Core.Remote;
    using Core.Players;

    public class LayerClient : ILayerClient {

        public static String ResponseOk = "OK";

        public static String ResponseInvalidPasswordHash = "InvalidPasswordHash";
        public static String ResponseInvalidPassword = "InvalidPassword";
        public static String ResponseInvalidUsername = "InvalidUsername";
        public static String ResponseLoginRequired = "LogInRequired";
        public static String ResponseInsufficientPrivileges = "InsufficientPrivileges";
        public static String ResponseInvalidArguments = "InvalidArguments";
        public static String ResponseUnknownCommand = "UnknownCommand";

        public event Action<ILayerClient> ClientShutdown;
        public event Action<ILayerClient> Login;
        public event Action<ILayerClient> Logout;
        public event Action<ILayerClient> Quit;
        public event Action<ILayerClient> UidRegistered;

        /// <summary>
        /// The owner of this client
        /// </summary>
        protected ILayerInstance Layer { get; set; }

        protected PRoConApplication Application { get; set; }
        protected PRoConClient Client { get; set; }

        /// <summary>
        /// Handle Procon specific requests.
        /// </summary>
        protected Dictionary<String, Action<ILayerPacketDispatcher, Packet>> RequestDelegates { get; set; }

        /// <summary>
        /// The game dependant packet dispatcher to use
        /// </summary>
        public ILayerPacketDispatcher PacketDispatcher { get; set; }

        /// <summary>
        /// If the client has authenticated yet
        /// </summary>
        protected bool IsLoggedIn { get; set; }

        /// <summary>
        /// If the client has events enabled (wants to recieve events)
        /// </summary>
        protected bool EventsEnabled { get; set; }

        /// <summary>
        /// Uid the client has chosen to identify them selves as
        /// </summary>
        public String ProconEventsUid { get; protected set; }

        /// <summary>
        /// The username the client has authenticated with. See IsLoggedIn.
        /// </summary>
        public String Username { get; private set; }

        /// <summary>
        /// The privileges of the authenticated user
        /// </summary>
        public CPrivileges Privileges { get; private set; }

        /// <summary>
        /// If gzip compression should be used during transport
        /// </summary>
        public bool GzipCompression { get; private set; }

        /// <summary>
        /// The salt issued to the client for authentication
        /// </summary>
        protected String Salt { get; set; }

        /// <summary>
        /// Records the sequence number of a server info request so we may edit the respond before dispatching.
        /// </summary>
        protected UInt32 ServerInfoSequenceNumber { get; set; }

        public LayerClient(ILayerInstance layer, ILayerConnection connection, PRoConApplication application, PRoConClient client) {
            if (layer == null) throw new ArgumentNullException("layer");
            if (connection == null) throw new ArgumentNullException("connection");
            if (application == null) throw new ArgumentNullException("application");
            if (client == null) throw new ArgumentNullException("client");

            this.Layer = layer;
            this.Application = application;
            this.Client = client;

            Privileges = new CPrivileges();
            Username = String.Empty;

            // This is just a default value so we never accidently pass through an empty
            // String for authentication. We generate a better salt later on.
            this.Salt = DateTime.Now.ToString("HH:mm:ss ff");

            this.IsLoggedIn = false;
            this.GzipCompression = false;

            this.ProconEventsUid = String.Empty;

            if (client.Game != null) {

                if (client.Game is BFBC2Client) {
                    this.PacketDispatcher = new Bfbc2PacketDispatcher(connection);
                }
                else if (client.Game is MoHClient) {
                    this.PacketDispatcher = new MohPacketDispatcher(connection);
                }
                else if (client.Game is BF3Client) {
                    this.PacketDispatcher = new Bf3PacketDispatcher(connection);
                }
                else if (client.Game is BFHLClient) {
                    this.PacketDispatcher = new BfhlPacketDispatcher(connection);
                }
                else if (client.Game is BF4Client) {
                    this.PacketDispatcher = new Bf4PacketDispatcher(connection);
                }
                else if (client.Game is MOHWClient) {
                    this.PacketDispatcher = new MohwPacketDispatcher(connection);
                }

                this.RequestDelegates = new Dictionary<String, Action<ILayerPacketDispatcher, Packet>>() {
                    { "procon.application.shutdown", this.DispatchProconApplicationShutdownRequest  },

                    { "procon.login.username", this.DispatchProconLoginUsernameRequest  },
                    { "procon.registerUid", this.DispatchProconRegisterUidRequest  },
                    { "procon.version", this.DispatchProconVersionRequest  },
                    { "procon.vars", this.DispatchProconVarsRequest  },
                    { "procon.privileges", this.DispatchProconPrivilegesRequest  },
                    { "procon.compression", this.DispatchProconCompressionRequest  },

                    { "procon.account.listAccounts", this.DispatchProconAccountListAccountsRequest  },
                    { "procon.account.listLoggedIn", this.DispatchProconAccountListLoggedInRequest  },
                    { "procon.account.create", this.DispatchProconAccountCreateRequest  },
                    { "procon.account.delete", this.DispatchProconAccountDeleteRequest  },
                    { "procon.account.setPassword", this.DispatchProconAccountSetPasswordRequest  },

                    { "procon.battlemap.deleteZone", this.DispatchProconBattlemapDeleteZoneRequest  },
                    { "procon.battlemap.createZone", this.DispatchProconBattlemapCreateZoneRequest  },
                    { "procon.battlemap.modifyZoneTags", this.DispatchProconBattlemapModifyZoneTagsRequest  },
                    { "procon.battlemap.modifyZonePoints", this.DispatchProconBattlemapModifyZonePointsRequest  },
                    { "procon.battlemap.listZones", this.DispatchProconBattlemapListZonesRequest  },

                    { "procon.layer.setPrivileges", this.DispatchProconLayerSetPrivilegesRequest  },

                    { "procon.plugin.listLoaded", this.DispatchProconPluginListLoadedRequest  },
                    { "procon.plugin.listEnabled", this.DispatchProconPluginListEnabledRequest  },
                    { "procon.plugin.enable", this.DispatchProconPluginEnableRequest  },
                    { "procon.plugin.setVariable", this.DispatchProconPluginSetVariableRequest  },

                    { "procon.exec", this.DispatchProconExecRequest },

                    { "procon.admin.say", this.DispatchProconAdminSayRequest },
                    { "procon.admin.yell", this.DispatchProconAdminYellRequest }

                    ,{ "procon.player.syncPlayTimes", this.DispatchProconPlayerSyncPlayTimesRequest }
                };

                this.RegisterEvents();
            }
        }

        private void RegisterEvents() {
            if (this.PacketDispatcher != null) {
                this.PacketDispatcher.ConnectionClosed =PacketDispatcher_ConnectionClosed;

                this.PacketDispatcher.RequestPacketUnknownRecieved = PacketDispatcher_RequestPacketUnknownRecieved;
                this.PacketDispatcher.RequestLoginHashed = PacketDispatcher_RequestLoginHashed;
                this.PacketDispatcher.RequestLoginHashedPassword = PacketDispatcher_RequestLoginHashedPassword;
                this.PacketDispatcher.RequestLoginPlainText = PacketDispatcher_RequestLoginPlainText;
                this.PacketDispatcher.RequestLogout = PacketDispatcher_RequestLogout;
                this.PacketDispatcher.RequestQuit = PacketDispatcher_RequestQuit;
                this.PacketDispatcher.RequestHelp = PacketDispatcher_RequestHelp;
                this.PacketDispatcher.RequestPacketAdminShutdown = PacketDispatcher_RequestPacketAdminShutdown;

                this.PacketDispatcher.RequestEventsEnabled = PacketDispatcher_RequestEventsEnabled;

                this.PacketDispatcher.RequestPacketSecureSafeListedRecieved = PacketDispatcher_RequestPacketSecureSafeListedRecieved;
                this.PacketDispatcher.RequestPacketUnsecureSafeListedRecieved = PacketDispatcher_RequestPacketUnsecureSafeListedRecieved;

                this.PacketDispatcher.RequestPacketPunkbusterRecieved = PacketDispatcher_RequestPacketPunkbusterRecieved;
                this.PacketDispatcher.RequestPacketUseMapFunctionRecieved = PacketDispatcher_RequestPacketUseMapFunctionRecieved;
                this.PacketDispatcher.RequestPacketAlterMaplistRecieved = PacketDispatcher_RequestPacketAlterMaplistRecieved;
                this.PacketDispatcher.RequestPacketAdminPlayerMoveRecieved = PacketDispatcher_RequestPacketAdminPlayerMoveRecieved;
                this.PacketDispatcher.RequestPacketAdminPlayerKillRecieved = PacketDispatcher_RequestPacketAdminPlayerKillRecieved;
                this.PacketDispatcher.RequestPacketAdminKickPlayerRecieved = PacketDispatcher_RequestPacketAdminKickPlayerRecieved;
                this.PacketDispatcher.RequestBanListAddRecieved = PacketDispatcher_RequestBanListAddRecieved;
                this.PacketDispatcher.RequestPacketAlterBanListRecieved = PacketDispatcher_RequestPacketAlterBanListRecieved;
                this.PacketDispatcher.RequestPacketAlterReservedSlotsListRecieved = PacketDispatcher_RequestPacketAlterReservedSlotsListRecieved;
                this.PacketDispatcher.RequestPacketAlterTextMonderationListRecieved = PacketDispatcher_RequestPacketAlterTextMonderationListRecieved;
                this.PacketDispatcher.RequestPacketVarsRecieved = PacketDispatcher_RequestPacketVarsRecieved;

                this.PacketDispatcher.RequestPacketSquadLeaderRecieved = PacketDispatcher_RequestPacketSquadLeaderRecieved;
                this.PacketDispatcher.RequestPacketSquadIsPrivateReceived = PacketDispatcher_RequestPacketSquadIsPrivateReceived;
                
            }

            this.Application.AccountsList.AccountAdded += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
            this.Application.AccountsList.AccountRemoved += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);

            foreach (Account acAccount in this.Application.AccountsList) {
                this.Layer.AccountPrivileges[acAccount.Name].AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(CPRoConLayerClient_AccountPrivilegesChanged);
            }

            this.Client.RecompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);
            this.Client.CompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);

            this.Client.PassLayerEvent += new PRoConClient.PassLayerEventHandler(m_prcClient_PassLayerEvent);

            if (this.Client.PluginsManager != null) {
                this.Client.PluginsManager.PluginLoaded += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginLoaded);
                this.Client.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
                this.Client.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
                this.Client.PluginsManager.PluginVariableAltered += new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
            }

            this.Client.MapGeometry.MapZones.MapZoneAdded += new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneAdded);
            this.Client.MapGeometry.MapZones.MapZoneChanged += new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneChanged);
            this.Client.MapGeometry.MapZones.MapZoneRemoved += new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneRemoved);

            this.Client.PluginConsole.WriteConsole += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(PluginConsole_WriteConsole);
            this.Client.ChatConsole.WriteConsoleViaCommand += new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(ChatConsole_WriteConsoleViaCommand);

            this.Client.Variables.VariableAdded += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableAdded);
            this.Client.Variables.VariableUpdated += new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableUpdated);
        }

        private void UnregisterEvents() {
            this.Application.AccountsList.AccountAdded -= new AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
            this.Application.AccountsList.AccountRemoved -= new AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);

            foreach (Account acAccount in this.Application.AccountsList) {
                this.Layer.AccountPrivileges[acAccount.Name].AccountPrivilegesChanged -= new AccountPrivilege.AccountPrivilegesChangedHandler(CPRoConLayerClient_AccountPrivilegesChanged);
            }

            this.Client.RecompilingPlugins -= new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);
            this.Client.CompilingPlugins -= new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);

            this.Client.PassLayerEvent -= new PRoConClient.PassLayerEventHandler(m_prcClient_PassLayerEvent);

            if (this.Client.PluginsManager != null) {
                this.Client.PluginsManager.PluginLoaded -= new PluginManager.PluginEmptyParameterHandler(Plugins_PluginLoaded);
                this.Client.PluginsManager.PluginEnabled -= new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
                this.Client.PluginsManager.PluginDisabled -= new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
                this.Client.PluginsManager.PluginVariableAltered -= new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
            }

            this.Client.MapGeometry.MapZones.MapZoneAdded -= new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneAdded);
            this.Client.MapGeometry.MapZones.MapZoneChanged -= new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneChanged);
            this.Client.MapGeometry.MapZones.MapZoneRemoved -= new PRoCon.Core.Battlemap.MapZoneDictionary.MapZoneAlteredHandler(MapZones_MapZoneRemoved);

            this.Client.PluginConsole.WriteConsole -= new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(PluginConsole_WriteConsole);
            this.Client.ChatConsole.WriteConsoleViaCommand -= new PRoCon.Core.Logging.Loggable.WriteConsoleHandler(ChatConsole_WriteConsoleViaCommand);

            this.Client.Variables.VariableAdded -= new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableAdded);
            this.Client.Variables.VariableUpdated -= new PRoCon.Core.Variables.VariableDictionary.PlayerAlteredHandler(Variables_VariableUpdated);


        }

        public String IPPort {
            get {
                String ipPort = String.Empty;

                // However if the connection is open just get it straight from the horses mouth.
                if (this.PacketDispatcher != null) {
                    ipPort = this.PacketDispatcher.IPPort;
                }

                return ipPort;
            }
        }

        #region Account Authentication

        protected String GeneratePasswordHash(byte[] salt, String data) {
            MD5 md5Hasher = MD5.Create();

            byte[] combined = new byte[salt.Length + data.Length];
            salt.CopyTo(combined, 0);
            Encoding.Default.GetBytes(data).CopyTo(combined, salt.Length);

            byte[] hash = md5Hasher.ComputeHash(combined);

            return hash.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b);
        }

        protected byte[] HashToByteArray(String hex) {
            byte[] array = new byte[hex.Length / 2];

            for (int i = 0; i < array.Length; i++) {
                array[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return array;
        }

        protected bool AuthenticatePlaintextAccount(String username, String password) {
            bool authenticated = false;

            if (String.CompareOrdinal(this.GetAccountPassword(username), String.Empty) != 0) {
                authenticated = String.CompareOrdinal(this.GetAccountPassword(username), password) == 0;
            }

            return authenticated;
        }

        protected bool AuthenticateHashedAccount(String username, String hashedPassword) {
            bool authenticated = false;

            if (String.CompareOrdinal(this.GetAccountPassword(username), String.Empty) != 0) {
                authenticated = String.CompareOrdinal(GeneratePasswordHash(HashToByteArray(this.Salt), this.GetAccountPassword(username)), hashedPassword) == 0;
            }

            return authenticated;
        }

        protected String GenerateSalt() {
            var provider = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[1024];
            provider.GetBytes(buffer);

            // Note that frostbite sends back a md5 for salt, so we must too.
            this.Salt = this.GeneratePasswordHash(buffer, Convert.ToBase64String(buffer));

            return this.Salt;
        }

        #endregion

        #region Packet Forwarding

        // What we got back from the BFBC2 server..
        public void Forward(Packet packet) {
            if (this.PacketDispatcher != null) {
                if (this.ServerInfoSequenceNumber == packet.SequenceNumber && packet.Words.Count >= 2) {
                    packet.Words[1] = this.Layer.NameFormat.Replace("%servername%", packet.Words[1]);
                }

                this.PacketDispatcher.SendResponse(packet, packet.Words);
            }
        }

        public void Poke() {
            if (this.PacketDispatcher != null) {
                this.PacketDispatcher.Poke();
            }
        }

        private void m_prcClient_PassLayerEvent(PRoConClient sender, Packet packet) {

            if (this.PacketDispatcher != null && this.IsLoggedIn == true && this.EventsEnabled == true) {
                this.PacketDispatcher.SendPacket(packet);
            }
            /*
            if (this.m_connection != null && this.m_blEventsEnabled == true) {
                this.m_connection.SendAsync(packet);
            }*/
        }

        #endregion

        #region Packet Handling

        #region Extended Protocol Handling

        #region Procon.Application.Shutdown
        
        // DispatchProconApplicationShutdownRequest
        private void DispatchProconApplicationShutdownRequest(ILayerPacketDispatcher sender, Packet packet)
        {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanShutdownServer == true) {
                    sender.SendResponse(packet, LayerClient.ResponseOk, "but nothing will happen");
                    // shutdowns only the connection not the whole procon... this.m_praApplication.Shutdown();
                } else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            } else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        private void DispatchProconLoginUsernameRequest(ILayerPacketDispatcher sender, Packet packet) {
            this.Username = packet.Words[1];

            // We send back any errors in the login process after they attempt to login.
            if (this.Application.AccountsList.Contains(this.Username) == true) {
                this.Privileges = this.GetAccountPrivileges(this.Username);

                this.Privileges.SetLowestPrivileges(this.Client.Privileges);

                sender.SendResponse(packet, this.Privileges.CanLogin == true ? LayerClient.ResponseOk : LayerClient.ResponseInsufficientPrivileges);
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseInvalidUsername);
            }
        }

        private void DispatchProconRegisterUidRequest(ILayerPacketDispatcher sender, Packet packet) {
            
            if (this.IsLoggedIn == true) {
            
                bool blEnabled = true;

                if (bool.TryParse(packet.Words[1], out blEnabled) == true) {

                    if (blEnabled == false) {
                        sender.SendResponse(packet, LayerClient.ResponseOk);

                        this.ProconEventsUid = String.Empty;
                    }
                    else if (packet.Words.Count >= 3) {

                        if (this.Layer.Clients.Any(client => client.Value.ProconEventsUid == packet.Words[2]) == false) {
                            sender.SendResponse(packet, LayerClient.ResponseOk);

                            this.ProconEventsUid = packet.Words[2];

                            var handler = this.UidRegistered;
                            if (handler != null) {
                                handler(this);
                            }
                        }
                        else {
                            sender.SendResponse(packet, "ProconUidConflict");
                        }
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconVersionRequest(ILayerPacketDispatcher sender, Packet packet) {
            sender.SendResponse(packet, LayerClient.ResponseOk, Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void DispatchProconVarsRequest(ILayerPacketDispatcher sender, Packet packet) {

            if (this.IsLoggedIn == true) {

                if (packet.Words.Count == 2) {
                    sender.SendResponse(packet, LayerClient.ResponseOk, packet.Words[1], this.Client.Variables.GetVariable(packet.Words[1], ""));
                }
                else if (packet.Words.Count > 2) {

                    if (this.Privileges.CanIssueLimitedProconCommands == true) {

                        this.Client.Variables.SetVariable(packet.Words[1], packet.Words[2]);

                        sender.SendResponse(packet, LayerClient.ResponseOk, packet.Words[1], this.Client.Variables.GetVariable(packet.Words[1], ""));
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconPrivilegesRequest(ILayerPacketDispatcher sender, Packet packet) {

            if (this.IsLoggedIn == true) {
                sender.SendResponse(packet, LayerClient.ResponseOk, this.Privileges.PrivilegesFlags.ToString(CultureInfo.InvariantCulture));
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconCompressionRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {

                bool enableCompress = false;

                if (packet.Words.Count == 2 && bool.TryParse(packet.Words[1], out enableCompress) == true) {
                    this.GzipCompression = enableCompress;
                    
                    sender.SendResponse(packet, LayerClient.ResponseOk);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconExecRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueAllProconCommands == true) {
                    sender.SendResponse(packet, LayerClient.ResponseOk);

                    packet.Words.RemoveAt(0);
                    this.Application.ExecutePRoConCommand(this.Client, packet.Words, 0);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #region Accounts

        private void DispatchProconAccountListAccountsRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconCommands == true) {

                    List<String> lstAccounts = new List<String> {
                        LayerClient.ResponseOk
                    };

                    foreach (String strAccountName in this.Application.AccountsList.ListAccountNames()) {
                        if (this.Layer.AccountPrivileges.Contains(strAccountName) == true) {
                            lstAccounts.Add(strAccountName);
                            lstAccounts.Add(this.Layer.AccountPrivileges[strAccountName].Privileges.PrivilegesFlags.ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    sender.SendResponse(packet, lstAccounts);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconAccountListLoggedInRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.Privileges.CanIssueLimitedProconCommands == true) {
                
                List<String> lstLoggedInAccounts = this.Layer.GetLoggedInAccountUsernamesWithUids((packet.Words.Count >= 2 && String.CompareOrdinal(packet.Words[1], "uids") == 0));

                //List<String> lstLoggedInAccounts = this.m_prcClient.Layer.GetLoggedInAccounts();
                lstLoggedInAccounts.Insert(0, LayerClient.ResponseOk);

                sender.SendResponse(packet, lstLoggedInAccounts);
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
            }
        }

        private void DispatchProconAccountCreateRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconCommands == true) {

                    if (this.Application.AccountsList.Contains(packet.Words[1]) == false) {
                        if (packet.Words[2].Length > 0) {
                            sender.SendResponse(packet, LayerClient.ResponseOk);
                            this.Application.AccountsList.CreateAccount(packet.Words[1], packet.Words[2]);
                            //this.m_uscParent.LayerCreateAccount(
                        }
                        else {
                            sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                        }
                    }
                    else {
                        sender.SendResponse(packet, "AccountAlreadyExists");
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconAccountDeleteRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconCommands == true) {
                    if (packet.Words.Count >= 2) {

                        if (this.Application.AccountsList.Contains(packet.Words[1]) == true) {
                            sender.SendResponse(packet, LayerClient.ResponseOk);

                            this.Application.AccountsList.Remove(packet.Words[1]);
                            //this.m_uscParent.LayerDeleteAccount(cpPacket.Words[1]);
                        }
                        else {
                            sender.SendResponse(packet, "AccountDoesNotExists");
                        }
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconAccountSetPasswordRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconCommands == true) {

                    if (packet.Words.Count >= 3 && packet.Words[2].Length > 0) {

                        if (this.Application.AccountsList.Contains(packet.Words[1]) == true) {
                            sender.SendResponse(packet, LayerClient.ResponseOk);

                            this.Application.AccountsList[packet.Words[1]].Password = packet.Words[2];
                        }
                        else {
                            sender.SendResponse(packet, "AccountDoesNotExists");
                        }
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region Battlemap

        private void DispatchProconBattlemapDeleteZoneRequest(ILayerPacketDispatcher sender, Packet packet) {

            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditMapZones == true) {
                    if (this.Client.MapGeometry.MapZones.Contains(packet.Words[1]) == true) {
                        this.Client.MapGeometry.MapZones.Remove(packet.Words[1]);
                    }

                    sender.SendResponse(packet, LayerClient.ResponseOk);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconBattlemapCreateZoneRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditMapZones == true) {

                    if (packet.Words.Count >= 3) {

                        int iPoints = 0;

                        if (int.TryParse(packet.Words[2], out iPoints) == true) {

                            Point3D[] points = new Point3D[iPoints];

                            for (int i = 0; i < iPoints && i + 3 < packet.Words.Count; i++) {
                                points[i] = new Point3D(packet.Words[2 + i * 3 + 1], packet.Words[2 + i * 3 + 2], packet.Words[2 + i * 3 + 3]);
                            }

                            this.Client.MapGeometry.MapZones.CreateMapZone(packet.Words[1], points);
                        }

                        sender.SendResponse(packet, LayerClient.ResponseOk);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconBattlemapModifyZoneTagsRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditMapZones == true) {

                    if (packet.Words.Count >= 3) {

                        if (this.Client.MapGeometry.MapZones.Contains(packet.Words[1]) == true) {
                            this.Client.MapGeometry.MapZones[packet.Words[1]].Tags.FromString(packet.Words[2]);
                        }

                        sender.SendResponse(packet, LayerClient.ResponseOk);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconBattlemapModifyZonePointsRequest(ILayerPacketDispatcher sender, Packet packet) {
            
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditMapZones == true) {

                    if (packet.Words.Count >= 3) {

                        int iPoints = 0;

                        if (int.TryParse(packet.Words[2], out iPoints) == true) {

                            Point3D[] points = new Point3D[iPoints];

                            for (int i = 0; i < iPoints && i + 3 < packet.Words.Count; i++) {
                                points[i] = new Point3D(packet.Words[2 + i * 3 + 1], packet.Words[2 + i * 3 + 2], packet.Words[2 + i * 3 + 3]);
                            }

                            if (this.Client.MapGeometry.MapZones.Contains(packet.Words[1]) == true) {
                                this.Client.MapGeometry.MapZones.ModifyMapZonePoints(packet.Words[1], points);
                            }
                        }

                        sender.SendResponse(packet, LayerClient.ResponseOk);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconBattlemapListZonesRequest(ILayerPacketDispatcher sender, Packet packet) {

            if (this.IsLoggedIn == true) {

                List<String> listPacket = new List<String> {
                    LayerClient.ResponseOk,
                    this.Client.MapGeometry.MapZones.Count.ToString(CultureInfo.InvariantCulture)
                };

                foreach (MapZoneDrawing zone in this.Client.MapGeometry.MapZones) {
                    listPacket.Add(zone.UID);
                    listPacket.Add(zone.LevelFileName);
                    listPacket.Add(zone.Tags.ToString());

                    listPacket.Add(zone.ZonePolygon.Length.ToString(CultureInfo.InvariantCulture));
                    listPacket.AddRange(Point3D.ToStringList(zone.ZonePolygon));
                }

                sender.SendResponse(packet, listPacket);
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region Layer

        private void DispatchProconLayerSetPrivilegesRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconCommands == true) {

                    UInt32 ui32Privileges = 0;

                    if (packet.Words.Count >= 3 && UInt32.TryParse(packet.Words[2], out ui32Privileges) == true) {

                        if (this.Application.AccountsList.Contains(packet.Words[1]) == true) {

                            CPrivileges sprvPrivs = new CPrivileges();

                            sender.SendResponse(packet, LayerClient.ResponseOk);

                            sprvPrivs.PrivilegesFlags = ui32Privileges;
                            this.Layer.AccountPrivileges[packet.Words[1]].SetPrivileges(sprvPrivs);
                        }
                        else {
                            sender.SendResponse(packet, "AccountDoesNotExists");
                        }
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region Plugin

        private void DispatchProconPluginListLoadedRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconPluginCommands == true) {

                    if (packet.Words.Count == 1) {
                        List<String> lstLoadedPlugins = this.GetListLoadedPlugins();

                        lstLoadedPlugins.Insert(0, LayerClient.ResponseOk);

                        sender.SendResponse(packet, lstLoadedPlugins);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconPluginListEnabledRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconPluginCommands == true) {
                    List<String> lstEnabledPlugins = this.Client.PluginsManager.Plugins.EnabledClassNames;
                    lstEnabledPlugins.Insert(0, LayerClient.ResponseOk);

                    sender.SendResponse(packet, lstEnabledPlugins);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconPluginEnableRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconPluginCommands == true) {
                    bool blEnabled = false;

                    if (packet.Words.Count >= 3 && bool.TryParse(packet.Words[2], out blEnabled) == true) {
                        sender.SendResponse(packet, LayerClient.ResponseOk);

                        if (blEnabled == true) {
                            this.Client.PluginsManager.EnablePlugin(packet.Words[1]);
                        }
                        else {
                            this.Client.PluginsManager.DisablePlugin(packet.Words[1]);
                        }
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconPluginSetVariableRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanIssueLimitedProconPluginCommands == true) {

                    if (packet.Words.Count >= 4) {

                        sender.SendResponse(packet, LayerClient.ResponseOk);

                        this.Client.PluginsManager.SetPluginVariable(packet.Words[1], packet.Words[2], packet.Words[3]);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region Communication

        private void DispatchProconAdminSayRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                
                if (packet.Words.Count >= 4) {

                    // Append the admin to the adminstack and send it on its way..
                    if (packet.Words[1].Length > 0) {
                        packet.Words[1] = String.Format("{0}|{1}", packet.Words[1], CPluginVariable.Encode(this.Username));
                    }
                    else {
                        packet.Words[1] = CPluginVariable.Encode(this.Username);
                    }

                    sender.SendResponse(packet, LayerClient.ResponseOk);

                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void DispatchProconAdminYellRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                
                if (packet.Words.Count >= 5) {
                    // Append the admin to the adminstack and send it on its way..
                    if (packet.Words[1].Length > 0) {
                        packet.Words[1] = String.Format("{0}|{1}", packet.Words[1], CPluginVariable.Encode(this.Username));
                    }
                    else {
                        packet.Words[1] = CPluginVariable.Encode(this.Username);
                    }

                    sender.SendResponse(packet, LayerClient.ResponseOk);

                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region procon.player.syncPlayTimes

        private void DispatchProconPlayerSyncPlayTimesRequest(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                List<String> lstPlayerJoinTimes = new List<String> {
                    LayerClient.ResponseOk
                };
                
                foreach ( CPlayerInfo player in Client.PlayerList) {
                    lstPlayerJoinTimes.Add(player.SoldierName);
                    lstPlayerJoinTimes.Add(player.JoinTime.ToString());
                }
                
                sender.SendResponse(packet, lstPlayerJoinTimes);
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        private void PacketDispatcher_RequestPacketUnknownRecieved(ILayerPacketDispatcher sender, Packet packet) {

            if (packet.Words.Count >= 1) {
                if (this.RequestDelegates.ContainsKey(packet.Words[0]) == true) {
                    this.RequestDelegates[packet.Words[0]](sender, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseUnknownCommand);
                }
            }
        }

        #endregion

        #region Overridden Protocol Handling

        private void PacketDispatcher_RequestLoginHashed(ILayerPacketDispatcher sender, Packet packet) {
            sender.SendResponse(packet, LayerClient.ResponseOk, this.GenerateSalt());
        }

        private void PacketDispatcher_RequestLoginHashedPassword(ILayerPacketDispatcher sender, Packet packet, String hashedPassword) {

            if (this.Application.AccountsList.Contains(this.Username) == false) {
                sender.SendResponse(packet, LayerClient.ResponseInvalidUsername);
            }
            else {
                if (this.AuthenticateHashedAccount(this.Username, hashedPassword) == true) {

                    this.Privileges = this.GetAccountPrivileges(this.Username);
                    this.Privileges.SetLowestPrivileges(this.Client.Privileges);

                    if (this.Privileges.CanLogin == true) {
                        this.IsLoggedIn = true;
                        sender.SendResponse(packet, LayerClient.ResponseOk);

                        var handler = this.Login;
                        if (handler != null) {
                            handler(this);
                        }
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidPasswordHash);
                }
            }
        }

        private void PacketDispatcher_RequestLoginPlainText(ILayerPacketDispatcher sender, Packet packet, String password) {

            if (this.Application.AccountsList.Contains(this.Username) == false) {
                sender.SendResponse(packet, LayerClient.ResponseInvalidUsername);
            }
            else {

                if (this.AuthenticatePlaintextAccount(this.Username, password) == true) {

                    this.IsLoggedIn = true;
                    sender.SendResponse(packet, LayerClient.ResponseOk);

                    var handler = this.Login;
                    if (handler != null) {
                        handler(this);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidPassword);
                }
            } 
        }

        private void PacketDispatcher_RequestLogout(ILayerPacketDispatcher sender, Packet packet) {
            sender.SendResponse(packet, LayerClient.ResponseOk);
            
            this.IsLoggedIn = false;

            var handler = this.Logout;
            if (handler != null) {
                handler(this);
            }
        }

        private void PacketDispatcher_RequestQuit(ILayerPacketDispatcher sender, Packet packet) {
            sender.SendResponse(packet, LayerClient.ResponseOk);

            var handler = this.Logout;
            if (handler != null) {
                handler(this);
            }

            handler = this.Quit;
            if (handler != null) {
                handler(this);
            }

            this.Shutdown();
        }

        private void PacketDispatcher_RequestEventsEnabled(ILayerPacketDispatcher sender, Packet packet, bool eventsEnabled) {
            if (this.IsLoggedIn == true) {
                sender.SendResponse(packet, LayerClient.ResponseOk);

                this.EventsEnabled = eventsEnabled;
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestHelp(ILayerPacketDispatcher sender, Packet packet) {
            // TO DO: Edit on way back with additional commands IF NOT PRESENT.
            this.Client.SendProconLayerPacket(this, packet);
        }

        private void PacketDispatcher_RequestPacketAdminShutdown(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanShutdownServer == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #region PacketDispatcher Protocol Handling

        private void PacketDispatcher_RequestPacketSecureSafeListedRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                this.Client.SendProconLayerPacket(this, packet);
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketUnsecureSafeListedRecieved(ILayerPacketDispatcher sender, Packet packet) {
            
            if (packet.Words.Count >= 1 && String.Compare(packet.Words[0], "serverInfo", StringComparison.OrdinalIgnoreCase) == 0) {
                this.ServerInfoSequenceNumber = packet.SequenceNumber;
            }
            
            this.Client.SendProconLayerPacket(this, packet);
        }

        private void PacketDispatcher_RequestPacketPunkbusterRecieved(ILayerPacketDispatcher sender, Packet packet) {
 	        if (this.IsLoggedIn == true) {

                if (packet.Words.Count >= 2) {
                    
                    bool blCommandProcessed = false;
                    
                    if (this.Privileges.CannotIssuePunkbusterCommands == true) {
                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);

                        blCommandProcessed = true;
                    }
                    else {
                        Match mtcMatch = Regex.Match(packet.Words[1], "^(?=(?<pb_sv_command>pb_sv_plist))|(?=(?<pb_sv_command>pb_sv_ban))|(?=(?<pb_sv_command>pb_sv_banguid))|(?=(?<pb_sv_command>pb_sv_banlist))|(?=(?<pb_sv_command>pb_sv_getss))|(?=(?<pb_sv_command>pb_sv_kick)[ ]+?.*?[ ]+?(?<pb_sv_kick_time>[0-9]+)[ ]+)|(?=(?<pb_sv_command>pb_sv_unban))|(?=(?<pb_sv_command>pb_sv_unbanguid))|(?=(?<pb_sv_command>pb_sv_reban))", RegexOptions.IgnoreCase);

                        // IF they tried to issue a pb_sv_command that isn't on the safe list AND they don't have full access.
                        if (mtcMatch.Success == false && this.Privileges.CanIssueAllPunkbusterCommands == false) {
                            sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                            blCommandProcessed = true;
                        }
                        else {

                            if (this.Privileges.CanPermanentlyBanPlayers == false && (System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_ban", System.StringComparison.OrdinalIgnoreCase) == 0 || System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_banguid", System.StringComparison.OrdinalIgnoreCase) == 0 || System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_reban", System.StringComparison.OrdinalIgnoreCase) == 0)) {
                                sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                                blCommandProcessed = true;
                            }
                            else if (this.Privileges.CanEditBanList == false && (System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_unban", System.StringComparison.OrdinalIgnoreCase) == 0 || System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_unbanguid", System.StringComparison.OrdinalIgnoreCase) == 0)) {
                                sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                                blCommandProcessed = true;
                            }
                            else if (System.String.Compare(mtcMatch.Groups["pb_sv_command"].Value, "pb_sv_kick", System.StringComparison.OrdinalIgnoreCase) == 0) {

                                int iBanLength = 0;

                                // NOTE* Punkbuster uses minutes not seconds.
                                if (int.TryParse(mtcMatch.Groups["pb_sv_kick_time"].Value, out iBanLength) == true) {

                                    // If they cannot punish players at all..
                                    if (this.Privileges.CannotPunishPlayers == true) {
                                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                                        blCommandProcessed = true;
                                    }
                                    // If they can temporary ban but not permanently ban BUT the banlength is over an hour (default)
                                    else if (this.Privileges.CanTemporaryBanPlayers == true && this.Privileges.CanPermanentlyBanPlayers == false && iBanLength > (this.Client.Variables.GetVariable("TEMP_BAN_CEILING", 3600) / 60)) {
                                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                                        blCommandProcessed = true;
                                    }
                                    // If they can kick but not temp or perm ban players AND the banlength is over 0 (no ban time)
                                    else if (this.Privileges.CanKickPlayers == true && this.Privileges.CanTemporaryBanPlayers == false && this.Privileges.CanPermanentlyBanPlayers == false && iBanLength > 0) {
                                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                                        blCommandProcessed = true;
                                    }
                                    // ELSE they have punkbuster access and full ban privs.. issue the command.
                                }
                                else { // Would rather stop it here than pass it on
                                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);

                                    blCommandProcessed = true;
                                }
                            }
                            // ELSE they have permission to issue this command (full or partial)
                        }
                    }

                    // Was not denied above, send it on to the PacketDispatcher server.
                    if (blCommandProcessed == false) {
                        this.Client.SendProconLayerPacket(this, packet);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInvalidArguments);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketUseMapFunctionRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanUseMapFunctions == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAlterMaplistRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditMapList == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAdminPlayerMoveRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanMovePlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAdminPlayerKillRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanKillPlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAdminKickPlayerRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanKickPlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestBanListAddRecieved(ILayerPacketDispatcher sender, Packet packet, CBanInfo newBan) {
            if (this.IsLoggedIn == true) {

                if (newBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Permanent && this.Privileges.CanPermanentlyBanPlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else if (newBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Round && this.Privileges.CanTemporaryBanPlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else if (newBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds && this.Privileges.CanPermanentlyBanPlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else if (newBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds && this.Privileges.CanTemporaryBanPlayers == true) {
                    
                    if (newBan.BanLength.Timeout <= this.Client.Variables.GetVariable("TEMP_BAN_CEILING", 3600)) {
                        this.Client.SendProconLayerPacket(this, packet);
                    }
                    else {
                        sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                    }
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAlterBanListRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditBanList == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAlterTextMonderationListRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditTextChatModerationList == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketAlterReservedSlotsListRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanEditReservedSlotsList == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketVarsRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanAlterServerSettings == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                }
                else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            }
            else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        #endregion

        #endregion

        #region player/squad cmds

        private void PacketDispatcher_RequestPacketSquadLeaderRecieved(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanMovePlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                } else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            } else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }

        private void PacketDispatcher_RequestPacketSquadIsPrivateReceived(ILayerPacketDispatcher sender, Packet packet) {
            if (this.IsLoggedIn == true) {
                if (this.Privileges.CanMovePlayers == true) {
                    this.Client.SendProconLayerPacket(this, packet);
                } else {
                    sender.SendResponse(packet, LayerClient.ResponseInsufficientPrivileges);
                }
            } else {
                sender.SendResponse(packet, LayerClient.ResponseLoginRequired);
            }
        }


        #endregion

        #region Accounts


        private String GetAccountPassword(String strUsername) {

            String strReturnPassword = String.Empty;

            if (this.Application.AccountsList.Contains(strUsername) == true) {
                strReturnPassword = this.Application.AccountsList[strUsername].Password;
            }

            if (String.IsNullOrEmpty(strUsername) == true) {
                strReturnPassword = this.Client.Variables.GetVariable("GUEST_PASSWORD", "");
            }

            return strReturnPassword;
        }

        private CPrivileges GetAccountPrivileges(String username) {

            CPrivileges privileges = new CPrivileges {
                PrivilegesFlags = 0
            };

            if (this.Layer.AccountPrivileges.Contains(username) == true) {
                privileges = this.Layer.AccountPrivileges[username].Privileges;
            }

            if (String.IsNullOrEmpty(username) == true && this.Client.Variables.IsVariableNullOrEmpty("GUEST_PRIVILEGES") == false) {
                privileges.PrivilegesFlags = this.Client.Variables.GetVariable<UInt32>("GUEST_PRIVILEGES", 0);
            }

            return privileges;
        }

        // TO DO: Implement event once available
        public void SendAccountLogin(String username, CPrivileges privileges) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onLogin", username, privileges.PrivilegesFlags.ToString(CultureInfo.InvariantCulture));
            }
        }

        // TO DO: Implement event once available
        public void SendAccountLogout(String username) {
            if (this.IsLoggedIn == true && System.String.CompareOrdinal(username, this.Username) != 0 && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onLogout", username);
            }
        }

        public void SendRegisteredUid(String uid, String username) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onUidRegistered", uid, username);
            }
        }

        private void CPRoConLayerClient_AccountPrivilegesChanged(AccountPrivilege item) {

            CPrivileges cpPrivs = new CPrivileges(item.Privileges.PrivilegesFlags);

            cpPrivs.SetLowestPrivileges(this.Client.Privileges);

            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onAltered", item.Owner.Name, cpPrivs.PrivilegesFlags.ToString(CultureInfo.InvariantCulture));
            }

            if (System.String.CompareOrdinal(this.Username, item.Owner.Name) == 0) {
                this.Privileges = cpPrivs;
            }
        }

        private void AccountsList_AccountRemoved(Account item) {

            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onDeleted", item.Name);
                //this.send(new Packet(true, false, this.AcquireSequenceNumber, new List<String>() { "procon.account.onDeleted", item.Name }));
            }
        }

        private void AccountsList_AccountAdded(Account item) {

            this.Layer.AccountPrivileges[item.Name].AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(CPRoConLayerClient_AccountPrivilegesChanged);

            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.account.onCreated", item.Name);
                //this.send(new Packet(true, false, this.AcquireSequenceNumber, new List<String>() { "procon.account.onCreated", item.Name }));
            }
        }

        #endregion

        #region Plugins

        private List<String> GetListLoadedPlugins() {
            List<String> plugins = new List<String>();

            foreach (String strPluginClassName in this.Client.PluginsManager.Plugins.LoadedClassNames) {
                // Get some updated plugin details..
                PluginDetails spDetails = this.Client.PluginsManager.GetPluginDetails(strPluginClassName);

                plugins.Add(spDetails.ClassName);

                plugins.Add(spDetails.Name);
                plugins.Add(spDetails.Author);
                plugins.Add(spDetails.Website);
                plugins.Add(spDetails.Version);
                plugins.Add(this.GzipCompression == true ? Packet.Compress(spDetails.Description) : spDetails.Description);
                plugins.Add(spDetails.DisplayPluginVariables.Count.ToString(CultureInfo.InvariantCulture));

                foreach (CPluginVariable cpvVariable in spDetails.DisplayPluginVariables) {
                    plugins.Add(cpvVariable.Name);
                    plugins.Add(cpvVariable.Type);
                    plugins.Add(cpvVariable.Value);
                }
            }

            return plugins;
        }

        private void m_prcClient_CompilingPlugins(PRoConClient sender) {
            this.Client.PluginsManager.PluginLoaded += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginLoaded);
            this.Client.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
            this.Client.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
            this.Client.PluginsManager.PluginVariableAltered += new PluginManager.PluginVariableAlteredHandler(Plugins_PluginVariableAltered);
        }

        private void Plugins_PluginLoaded(String strClassName) {
            PluginDetails spdDetails = this.Client.PluginsManager.GetPluginDetails(strClassName);

            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {

                List<String> lstOnPluginLoaded = new List<String>() { "procon.plugin.onLoaded", spdDetails.ClassName, spdDetails.Name, spdDetails.Author, spdDetails.Website, spdDetails.Version, spdDetails.Description, spdDetails.DisplayPluginVariables.Count.ToString(CultureInfo.InvariantCulture) };

                foreach (CPluginVariable cpvVariable in spdDetails.DisplayPluginVariables) {
                    lstOnPluginLoaded.AddRange(new List<String> { cpvVariable.Name, cpvVariable.Type, cpvVariable.Value });
                }

                this.PacketDispatcher.SendRequest(lstOnPluginLoaded);
            }
        }

        private void Plugins_PluginVariableAltered(PluginDetails spdNewDetails) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {

                List<String> lstWords = new List<String>() { "procon.plugin.onVariablesAltered", spdNewDetails.ClassName, (spdNewDetails.DisplayPluginVariables.Count).ToString(CultureInfo.InvariantCulture) };

                foreach (CPluginVariable cpvVariable in spdNewDetails.DisplayPluginVariables) {
                    lstWords.AddRange(new[] {
                        cpvVariable.Name, 
                        cpvVariable.Type, 
                        cpvVariable.Value
                    });
                }

                this.PacketDispatcher.SendRequest(lstWords);
            }
        }

        private void Plugins_PluginEnabled(String strClassName) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.plugin.onEnabled", strClassName, Packet.Bltos(true));
            }
        }

        private void Plugins_PluginDisabled(String strClassName) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.plugin.onEnabled", strClassName, Packet.Bltos(false));
            }
        }

        public void PluginConsole_WriteConsole(DateTime dtLoggedTime, String strLoggedText) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.plugin.onConsole", dtLoggedTime.ToBinary().ToString(CultureInfo.InvariantCulture), strLoggedText);
            }
        }

        #endregion

        #region Chat

        public void ChatConsole_WriteConsoleViaCommand(DateTime dtLoggedTime, String strLoggedText) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.chat.onConsole", dtLoggedTime.ToBinary().ToString(CultureInfo.InvariantCulture), strLoggedText);
            }
        }

        #endregion

        #region Map Zones

        private void MapZones_MapZoneRemoved(PRoCon.Core.Battlemap.MapZoneDrawing item) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.battlemap.onZoneRemoved", item.UID);
            }
        }

        private void MapZones_MapZoneChanged(PRoCon.Core.Battlemap.MapZoneDrawing item) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                List<String> packet = new List<String>() { "procon.battlemap.onZoneModified", item.UID, item.Tags.ToString(), item.ZonePolygon.Length.ToString(CultureInfo.InvariantCulture) };

                packet.AddRange(Point3D.ToStringList(item.ZonePolygon));

                this.PacketDispatcher.SendRequest(packet);
                //this.send(new Packet(true, false, this.AcquireSequenceNumber, packet));
            }
        }

        private void MapZones_MapZoneAdded(PRoCon.Core.Battlemap.MapZoneDrawing item) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                List<String> packet = new List<String>() { "procon.battlemap.onZoneCreated", item.UID, item.LevelFileName, item.ZonePolygon.Length.ToString(CultureInfo.InvariantCulture) };

                packet.AddRange(Point3D.ToStringList(item.ZonePolygon));

                this.PacketDispatcher.SendRequest(packet);
                //this.send(new Packet(true, false, this.AcquireSequenceNumber, packet));
            }
        }

        #endregion

        #region Variables

        private void Variables_VariableUpdated(PRoCon.Core.Variables.Variable item) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.vars.onAltered", item.Name, item.Value);
            }
        }

        private void Variables_VariableAdded(PRoCon.Core.Variables.Variable item) {
            if (this.IsLoggedIn == true && this.EventsEnabled == true && this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.vars.onAltered", item.Name, item.Value);
            }
        }

        #endregion

        private void PacketDispatcher_ConnectionClosed(ILayerPacketDispatcher sender) {
            var handler = this.ClientShutdown;

            if (handler != null) {
                handler(this);
            }

            this.UnregisterEvents();
        }

        public void Shutdown() {
            if (this.PacketDispatcher != null) {
                this.PacketDispatcher.SendRequest("procon.shutdown");

                this.PacketDispatcher.Shutdown();
            }
        }
    }
}
