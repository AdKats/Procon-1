using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PRoCon.Core.Remote.Layer
{
    using Core;
    using Core.Accounts;
    using Core.Remote;

    public class LayerInstance : ILayerInstance
    {

        private TcpListener _layerListener;
        protected readonly Object LayerListenerLock = new Object();

        private PRoConApplication _application;
        private PRoConClient _client;

        public event Action LayerStarted;
        public event Action LayerShutdown;
        public event Action<SocketException> SocketError;
        public event Action<ILayerClient> ClientConnected;

        public AccountPrivilegeDictionary AccountPrivileges { get; protected set; }

        public Dictionary<String, ILayerClient> Clients { get; protected set; }

        public string BindingAddress { get; set; }

        public UInt16 ListeningPort { get; set; }

        public string NameFormat { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsOnline
        {
            get
            {
                return (this._layerListener != null);
            }
        }

        public LayerInstance()
        {
            this.AccountPrivileges = new AccountPrivilegeDictionary();

            this.ListeningPort = 27260;
            this.BindingAddress = "0.0.0.0";
            this.NameFormat = "PRoCon[%servername%]";
            this._layerListener = null;
            this.Clients = new Dictionary<String, ILayerClient>();

            this.IsEnabled = false;

        }

        public void Initialize(PRoConApplication praApplication, PRoConClient prcClient)
        {
            this._application = praApplication;
            foreach (Account accAccount in this._application.AccountsList)
            {

                if (this.AccountPrivileges.Contains(accAccount.Name) == false)
                {
                    AccountPrivilege apPrivs = new AccountPrivilege(accAccount, new CPrivileges());
                    apPrivs.AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(apPrivs_AccountPrivilegesChanged);
                    this.AccountPrivileges.Add(apPrivs);
                }
                else
                {
                    this.AccountPrivileges[accAccount.Name].AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(apPrivs_AccountPrivilegesChanged);
                }

            }
            this._application.AccountsList.AccountAdded += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
            this._application.AccountsList.AccountRemoved += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);

            this._client = prcClient;

            this._client.SocketException += new PRoConClient.SocketExceptionHandler(m_prcClient_SocketException);
            this._client.ConnectionFailure += new PRoConClient.FailureHandler(m_prcClient_ConnectionFailure);
            this._client.ConnectionClosed += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);
            this._client.Game.Login += new FrostbiteClient.EmptyParamterHandler(m_prcClient_CommandLogin);

            this.ClientConnected += PRoConLayer_ClientConnected;

            if (this.IsEnabled == true && this.IsOnline == false)
            {
                this.Start();
            }
        }

        protected void OnLayerStarted()
        {
            var handler = this.LayerStarted;
            if (handler != null)
            {
                handler();
            }
        }

        protected void OnLayerShutdown()
        {
            var handler = this.LayerShutdown;
            if (handler != null)
            {
                handler();
            }
        }

        protected void OnSocketError(SocketException exception)
        {
            var handler = this.SocketError;
            if (handler != null)
            {
                handler(exception);
            }
        }

        protected void OnClientConnected(ILayerClient client)
        {
            var handler = this.ClientConnected;
            if (handler != null)
            {
                handler(client);
            }
        }

        private void AccountsList_AccountRemoved(Account item)
        {
            item.AccountPasswordChanged -= new Account.AccountPasswordChangedHandler(item_AccountPasswordChanged);
            this.AccountPrivileges[item.Name].AccountPrivilegesChanged -= new AccountPrivilege.AccountPrivilegesChangedHandler(apPrivs_AccountPrivilegesChanged);

            this.AccountPrivileges.Remove(item.Name);

            this.ForcefullyDisconnectAccount(item.Name);
        }

        private void AccountsList_AccountAdded(Account item)
        {
            AccountPrivilege apPrivs = new AccountPrivilege(item, new CPrivileges());

            this.AccountPrivileges.Add(apPrivs);

            item.AccountPasswordChanged += new Account.AccountPasswordChangedHandler(item_AccountPasswordChanged);
            apPrivs.AccountPrivilegesChanged += new AccountPrivilege.AccountPrivilegesChangedHandler(apPrivs_AccountPrivilegesChanged);
        }

        private void item_AccountPasswordChanged(Account item)
        {
            this.ForcefullyDisconnectAccount(item.Name);
        }

        private void apPrivs_AccountPrivilegesChanged(AccountPrivilege item)
        {
            if (item.Privileges.CanLogin == false)
            {
                this.ForcefullyDisconnectAccount(item.Owner.Name);
            }
        }

        private void m_prcClient_CommandLogin(FrostbiteClient sender)
        {
            // Start the layer if it's been enabled to on startup.
            if (this.IsEnabled == true && this.IsOnline == false)
            {
                this.Start();
            }
        }

        private void m_prcClient_ConnectionClosed(PRoConClient sender)
        {
            this.Shutdown();
        }

        private void m_prcClient_ConnectionFailure(PRoConClient sender, Exception exception)
        {
            this.Shutdown();
        }

        private void m_prcClient_SocketException(PRoConClient sender, SocketException se)
        {
            this.Shutdown();
        }

        private void PRoConLayer_ClientConnected(ILayerClient sender)
        {
            sender.Login += client_LayerClientLogin;
            sender.Logout += client_LayerClientLogout;
            sender.Quit += client_LayerClientQuit;
            sender.ClientShutdown += client_LayerClientShutdown;
            sender.UidRegistered += client_UidRegistered;
        }

        private void client_LayerClientShutdown(ILayerClient sender)
        {
            sender.Login -= client_LayerClientLogin;
            sender.Logout -= client_LayerClientLogout;
            sender.Quit -= client_LayerClientQuit;
            sender.ClientShutdown -= client_LayerClientShutdown;
            sender.UidRegistered -= client_UidRegistered;

            this.Clients.Remove(sender.IPPort);

            this.BroadcastAccountLogout(sender.Username);
        }

        private void BroadcastAccountLogout(string username)
        {
            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                client.SendAccountLogout(username);
            }
        }

        private void client_LayerClientQuit(ILayerClient sender)
        {
            if (this.Clients.ContainsKey(sender.IPPort) == true)
            {
                this.Clients.Remove(sender.IPPort);
                this.BroadcastAccountLogout(sender.Username);
            }
        }

        private void client_LayerClientLogout(ILayerClient sender)
        {
            this.BroadcastAccountLogout(sender.Username);
        }

        private void client_LayerClientLogin(ILayerClient sender)
        {
            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                client.SendAccountLogin(sender.Username, sender.Privileges);
            }
        }

        private void client_UidRegistered(ILayerClient sender)
        {
            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                client.SendRegisteredUid(sender.ProconEventsUid, sender.Username);
            }
        }

        private void ForcefullyDisconnectAccount(string accountName)
        {
            List<ILayerClient> shutDownClients = new List<ILayerClient>();

            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                if (String.CompareOrdinal(client.Username, accountName) == 0)
                {
                    shutDownClients.Add(client);
                }
            }

            foreach (ILayerClient cplcShutdown in shutDownClients)
            {
                cplcShutdown.Shutdown();
            }

            this.BroadcastAccountLogout(accountName);
        }

        public List<String> GetLoggedInAccountUsernames()
        {
            List<String> loggedInAccountUsernames = new List<String>();

            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                if (loggedInAccountUsernames.Contains(client.Username) == false)
                {
                    loggedInAccountUsernames.Add(client.Username);
                }
            }

            return loggedInAccountUsernames;
        }

        public List<String> GetLoggedInAccountUsernamesWithUids(bool listUids)
        {
            List<String> loggedInAccounts = new List<String>();

            foreach (ILayerClient plcConnection in new List<ILayerClient>(this.Clients.Values))
            {
                if (loggedInAccounts.Contains(plcConnection.Username) == false)
                {
                    loggedInAccounts.Add(plcConnection.Username);

                    if (listUids == true)
                    {
                        loggedInAccounts.Add(plcConnection.ProconEventsUid);
                    }
                }
            }

            return loggedInAccounts;
        }

        private void ListenIncommingLayerConnections(IAsyncResult ar)
        {
            lock (this.LayerListenerLock)
            {
                if (this._layerListener != null)
                {

                    try
                    {
                        TcpClient tcpClient = this._layerListener.EndAcceptTcpClient(ar);

                        ILayerClient client = new LayerClient(this, new LayerConnection(tcpClient), this._application, this._client);

                        // Issue #24. Somewhere the end port connection+port isn't being removed.
                        if (this.Clients.ContainsKey(client.IPPort) == true)
                        {
                            this.Clients[client.IPPort].Shutdown();

                            // If, for some reason, the client wasn't removed during shutdown..
                            if (this.Clients.ContainsKey(client.IPPort) == true)
                            {
                                this.Clients.Remove(client.IPPort);
                            }
                        }

                        this.Clients.Add(client.IPPort, client);


                        this.OnClientConnected(client);

                        this._layerListener.BeginAcceptTcpClient(this.ListenIncommingLayerConnections, this);
                    }
                    catch (SocketException exception)
                    {
                        this.OnSocketError(exception);

                        this.Shutdown();

                        //cbfAccountsPanel.OnLayerServerSocketError(skeError);
                    }
                    catch (Exception e)
                    {
                        FrostbiteConnection.LogError("ListenIncommingLayerConnections", "catch (Exception e)", e);
                    }
                }
            }
        }

        /// <summary>
        /// Pokes all connections, making sure they are still alive and well. Shuts them down if no traffic has occured in
        /// the last five minutes.
        /// </summary>
        public void Poke()
        {
            foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
            {
                client.Poke();
            }
        }

        private IPAddress ResolveHostName(string hostName)
        {
            IPAddress ipReturn = IPAddress.None;

            if (IPAddress.TryParse(hostName, out ipReturn) == false)
            {

                ipReturn = IPAddress.None;

                try
                {
                    IPHostEntry iphHost = Dns.GetHostEntry(hostName);

                    if (iphHost.AddressList.Length > 0)
                    {
                        ipReturn = iphHost.AddressList[0];
                    }
                    // ELSE return IPAddress.None..
                }
                catch (Exception) { } // Returns IPAddress.None..
            }

            return ipReturn;
        }

        public void Start()
        {

            try
            {
                IPAddress ipBinding = this.ResolveHostName(this.BindingAddress);

                lock (this.LayerListenerLock)
                {
                    this._layerListener = new TcpListener(ipBinding, this.ListeningPort);

                    this._layerListener.Start();
                }

                this.OnLayerStarted();

                this._layerListener.BeginAcceptTcpClient(this.ListenIncommingLayerConnections, this);
            }
            catch (SocketException exception)
            {
                this.OnSocketError(exception);

                this.Shutdown();
            }
        }

        public void Shutdown()
        {
            lock (this.LayerListenerLock)
            {
                if (this._layerListener != null)
                {

                    try
                    {
                        foreach (ILayerClient client in new List<ILayerClient>(this.Clients.Values))
                        {
                            client.Shutdown();
                        }

                        this._layerListener.Stop();
                        this._layerListener = null;
                    }
                    catch (Exception)
                    {
                    }

                    this.OnLayerShutdown();
                }
            }
        }

    }

}
