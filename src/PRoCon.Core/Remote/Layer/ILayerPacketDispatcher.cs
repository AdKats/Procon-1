using System;
using System.Collections.Generic;

namespace PRoCon.Core.Remote.Layer
{
    public interface ILayerPacketDispatcher
    {
        /// <summary>
        /// The unique incoming ip:port of the remote point
        /// </summary>
        String IPPort { get; }

        /// <summary>
        /// THe underlying connection has been closed.
        /// </summary>
        Action<ILayerPacketDispatcher> ConnectionClosed { get; set; }

        Action<ILayerPacketDispatcher, Packet> RequestPacketUnsecureSafeListedRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketSecureSafeListedRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketPunkbusterRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAlterTextMonderationListRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAlterBanListRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAlterReservedSlotsListRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAlterMaplistRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketUseMapFunctionRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketVarsRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAdminShutdown { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAdminPlayerKillRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAdminKickPlayerRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketAdminPlayerMoveRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketUnknownRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet, CBanInfo> RequestBanListAddRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet, String> RequestLoginPlainText { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestQuit { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestLogout { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestHelp { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestLoginHashed { get; set; }
        Action<ILayerPacketDispatcher, Packet, bool> RequestEventsEnabled { get; set; }
        Action<ILayerPacketDispatcher, Packet, String> RequestLoginHashedPassword { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketSquadLeaderRecieved { get; set; }
        Action<ILayerPacketDispatcher, Packet> RequestPacketSquadIsPrivateReceived { get; set; }

        /// <summary>
        ///     Pokes the connection, ensuring that the connection is still alive. If
        ///     this method determines that the connection is dead then it will call for
        ///     a shutdown.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method is a final check to make sure communications are proceeding in both directions in
        ///         the last five minutes. If nothing has been sent and received in the last five minutes then the connection is assumed
        ///         dead and a shutdown is initiated.
        ///     </para>
        /// </remarks>
        void Poke();

        /// <summary>
        /// Pass on a raw packet to the underlying connection
        /// </summary>
        /// <param name="packet">The packet to send through</param>
        void SendPacket(Packet packet);

        /// <summary>
        /// Create a new request to the underlying connection from a list of words
        /// </summary>
        /// <param name="words">The words to use when building the new packet</param>
        void SendRequest(List<String> words);

        /// <summary>
        /// Alias of SendRequest(List&lt;String&gt;)
        /// </summary>
        void SendRequest(params String[] words);

        /// <summary>
        /// Send a response to a packet by building a new packet, copying the sequence number
        /// and adjusting the origin/direction to compensate.
        /// </summary>
        /// <param name="packet">The packet to respond to</param>
        /// <param name="words">The words of the packet to respond with</param>
        void SendResponse(Packet packet, List<String> words);

        /// <summary>
        /// Alias of SendResponse(Packet, List&lt;String&gt;)
        /// </summary>
        void SendResponse(Packet packet, params String[] words);

        /// <summary>
        /// Shutdown the underlying connection and stop dispatching incoming requests.
        /// </summary>
        void Shutdown();
    }
}
