using PRoCon.Core.Accounts;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PRoCon.Core.Remote.Layer
{
    public interface ILayerInstance
    {
        /// <summary>
        /// Dictionary of connected clients
        /// </summary>
        Dictionary<String, ILayerClient> Clients { get; }

        /// <summary>
        /// The accounts we are working from to determine privileges and authentication
        /// </summary>
        AccountPrivilegeDictionary AccountPrivileges { get; }

        /// <summary>
        /// The address to bind the listener to
        /// </summary>
        String BindingAddress { get; set; }

        /// <summary>
        /// The port to bind the listener to
        /// </summary>
        UInt16 ListeningPort { get; set; }

        /// <summary>
        /// The format of the layer name. %servername% will be replaced with whatever
        /// was received from the game server
        /// </summary>
        String NameFormat { get; set; }

        /// <summary>
        /// If the layer is enabled or not
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// If the layer is currently online
        /// </summary>
        bool IsOnline { get; }

        /// <summary>
        /// Called when the layer begins accepting incoming connections 
        /// </summary>
        event Action LayerStarted;

        /// <summary>
        /// Fired when the layer is no longer accepting connections
        /// </summary>
        event Action LayerShutdown;

        /// <summary>
        /// Fired when a socket error occurs when setting up the listener
        /// </summary>
        event Action<SocketException> SocketError;

        /// <summary>
        /// Called whenever a client has connected to the layer
        /// </summary>
        event Action<ILayerClient> ClientConnected;

        /// <summary>
        /// Initializes the layer with the parent application and connected client (to the game server)
        /// </summary>
        /// <param name="application">The main application controller</param>
        /// <param name="client">The connected client for the game server</param>
        void Initialize(PRoConApplication application, PRoConClient client);

        /// <summary>
        /// Fetch a list of usernames for all currently logged in accounts.
        /// </summary>
        /// <returns></returns>
        List<String> GetLoggedInAccountUsernames();

        /// <summary>
        /// Fetch a list of usernames for all currently logged in accounts and optionally zip
        /// the account uid's in with the list [ [Name, Uid], [Name, Uid] ... ]
        /// </summary>
        /// <param name="listUids"></param>
        /// <returns></returns>
        List<String> GetLoggedInAccountUsernamesWithUids(bool listUids);

        /// <summary>
        /// Poke all of the connect clients and the listener.
        /// </summary>
        void Poke();

        /// <summary>
        /// Start listening/accepting incoming layer connectings
        /// </summary>
        void Start();

        /// <summary>
        /// Shutdown the listener, no longer accepting incoming connections
        /// </summary>
        void Shutdown();
    }
}
