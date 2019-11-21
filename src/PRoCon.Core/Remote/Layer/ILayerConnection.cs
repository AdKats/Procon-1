using System;

namespace PRoCon.Core.Remote.Layer
{
    public interface ILayerConnection
    {
        /// <summary>
        /// The unique incoming ip:port of the remote point
        /// </summary>
        String IPPort { get; }

        /// <summary>
        /// Called when the underlying connection has been closed.
        /// </summary>
        Action<ILayerConnection> ConnectionClosed { get; set; }

        /// <summary>
        /// Called whenever a packet has been successfully sent.
        /// </summary>
        Action<ILayerConnection, Packet> PacketSent { get; set; }

        /// <summary>
        /// Called whenever the connection recieves a completed packet.
        /// </summary>
        Action<ILayerConnection, Packet> PacketReceived { get; set; }

        /// <summary>
        /// Safely increments the sequence number 
        /// </summary>
        UInt32 AcquireSequenceNumber { get; }

        /// <summary>
        /// Sends/Queues a packet for sending asynchronously
        /// </summary>
        /// <param name="packet">The packet to be sent</param>
        void Send(Packet packet);

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
        /// Closes the current connection
        /// </summary>
        void Shutdown();
    }
}
