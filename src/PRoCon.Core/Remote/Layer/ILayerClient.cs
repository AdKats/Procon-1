using System;

namespace PRoCon.Core.Remote.Layer
{
    public interface ILayerClient
    {
        /// <summary>
        /// The username the client has authenticated with. See IsLoggedIn.
        /// </summary>
        String Username { get; }

        /// <summary>
        /// The username the client has authenticated with. See IsLoggedIn.
        /// </summary>
        String IPPort { get; }

        /// <summary>
        /// The privileges of the authenticated user
        /// </summary>
        CPrivileges Privileges { get; }

        /// <summary>
        /// Uid the client has chosen to identify them selves as
        /// </summary>
        String ProconEventsUid { get; }

        /// <summary>
        /// Fired when the client is shutdown completely
        /// </summary>
        event Action<ILayerClient> ClientShutdown;

        /// <summary>
        /// Fired once the client has successfully authenticated
        /// </summary>
        event Action<ILayerClient> Login;

        /// <summary>
        /// Fired when the client logs out (specifically calls logout)
        /// </summary>
        event Action<ILayerClient> Logout;

        /// <summary>
        /// Fired when the client issues a quit request
        /// </summary>
        event Action<ILayerClient> Quit;

        /// <summary>
        /// Fired when a valid guid is registered by a client
        /// </summary>
        event Action<ILayerClient> UidRegistered;

        /// <summary>
        /// Send an event alerting the client to a new logged in user
        /// </summary>
        /// <param name="username">The user that has logged in</param>
        /// <param name="privileges">The privileges of the logged in user (at the time of login)</param>
        void SendAccountLogin(String username, CPrivileges privileges);

        /// <summary>
        /// Send an event alerting the client that a user has logged out or been disconnected.
        /// </summary>
        /// <param name="username">The username that has been removed</param>
        void SendAccountLogout(String username);

        /// <summary>
        /// Send an event alerting clients that a uid has been registered for a username
        /// </summary>
        /// <param name="uid">The uid registered</param>
        /// <param name="username">The username to attach the uid to</param>
        void SendRegisteredUid(String uid, String username);

        /// <summary>
        /// Forward (send) a packet to the client, but do not alter it in anyway. It's straight from
        /// another source (game server) and should be sent to the client as-is.
        /// </summary>
        /// <param name="packet">The packet to forward on to the client</param>
        void Forward(Packet packet);

        /// <summary>
        /// Pokes the underlying packet dispatcher.
        /// </summary>
        void Poke();

        /// <summary>
        /// Shutdown the underlying connection and packet dispatcher
        /// </summary>
        void Shutdown();
    }
}
