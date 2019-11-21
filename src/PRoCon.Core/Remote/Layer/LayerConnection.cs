using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PRoCon.Core.Remote.Layer
{
    public class LayerConnection : ILayerConnection
    {
        protected const UInt32 MaxGarbageBytes = 4194304;

        /// <summary>
        ///     Lock used when aquiring a sequence #
        /// </summary>
        protected readonly Object AcquireSequenceNumberLock = new Object();

        /// <summary>
        /// The underlying tcp client
        /// </summary>
        protected TcpClient Client;

        /// <summary>
        /// Buffer to hold chunks of recieved data in the order they were recieved
        /// </summary>
        protected byte[] PacketStream;

        /// <summary>
        /// Buffer to accept recieved data into (chunk)
        /// </summary>
        protected byte[] ReceivedBuffer;
        /// <summary>
        /// The current sequence number (or last sequence number sent)
        /// </summary>
        protected UInt32 SequenceNumber;

        /// <summary>
        /// Lock used when shutting down the connection
        /// </summary>
        protected Object ShutdownConnectionLock = new Object();

        /// <summary>
        /// The stream from Client
        /// </summary>
        protected NetworkStream NetworkStream;

        public Action<ILayerConnection> ConnectionClosed { get; set; }

        public Action<ILayerConnection, Packet> PacketSent { get; set; }

        public Action<ILayerConnection, Packet> PacketReceived { get; set; }

        public LayerConnection(TcpClient acceptedConnection)
        {
            ReceivedBuffer = new byte[4096];
            PacketStream = null;

            Client = acceptedConnection;

            NetworkStream = Client.GetStream();

            NetworkStream.BeginRead(ReceivedBuffer, 0, ReceivedBuffer.Length, ReceiveCallback, this);
        }

        /// <summary>
        ///     The last packet that was receieved by this connection.
        /// </summary>
        public Packet LastPacketReceived { get; protected set; }

        /// <summary>
        ///     The last packet that was sent by this connection.
        /// </summary>
        public Packet LastPacketSent { get; protected set; }

        public UInt32 AcquireSequenceNumber
        {
            get
            {
                lock (AcquireSequenceNumberLock)
                {
                    return ++SequenceNumber;
                }
            }
        }

        public String IPPort
        {
            get
            {
                string strClientIPPort = String.Empty;

                // However if the connection is open just get it straight from the horses mouth.
                if (Client != null && Client.Connected == true)
                {
                    strClientIPPort = ((IPEndPoint)Client.Client.RemoteEndPoint).Address + ":" + ((IPEndPoint)Client.Client.RemoteEndPoint).Port.ToString(CultureInfo.InvariantCulture);
                }

                return strClientIPPort;
            }
        }

        private void SendAsyncCallback(IAsyncResult ar)
        {
            try
            {
                if (NetworkStream != null)
                {
                    NetworkStream.EndWrite(ar);

                    this.OnPacketSent((Packet)ar.AsyncState);
                }
            }
            catch (SocketException)
            {
                Shutdown();
            }
            catch (Exception)
            {
                Shutdown();
            }
        }

        public void Send(Packet packet)
        {
            try
            {
                if (NetworkStream != null)
                {
                    byte[] bytePacket = packet.EncodePacket();

                    LastPacketSent = packet;

                    NetworkStream.BeginWrite(bytePacket, 0, bytePacket.Length, SendAsyncCallback, packet);
                }
            }
            catch (SocketException)
            {
                // TO DO: Error reporting, possibly in a log file.
                Shutdown();
            }
            catch (Exception)
            {
                Shutdown();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (NetworkStream != null)
            {
                try
                {
                    int iBytesRead = NetworkStream.EndRead(ar);
                    iBytesRead = ServeCrossDomainPolicy(iBytesRead);

                    if (iBytesRead > 0)
                    {
                        // Create or resize our packet stream to hold the new data.
                        if (PacketStream == null)
                        {
                            PacketStream = new byte[iBytesRead];
                        }
                        else
                        {
                            Array.Resize(ref PacketStream, PacketStream.Length + iBytesRead);
                        }

                        Array.Copy(ReceivedBuffer, 0, PacketStream, PacketStream.Length - iBytesRead, iBytesRead);

                        UInt32 packetSize = Packet.DecodePacketSize(PacketStream);

                        while (this.PacketStream != null && PacketStream.Length >= packetSize && PacketStream.Length > Packet.PacketHeaderSize)
                        {
                            // Copy the complete packet from the beginning of the stream.
                            var completePacket = new byte[packetSize];
                            Array.Copy(PacketStream, completePacket, packetSize);

                            var deserializedPacket = new Packet(completePacket);
                            SequenceNumber = Math.Max(SequenceNumber, deserializedPacket.SequenceNumber);

                            LastPacketReceived = deserializedPacket;

                            // Dispatch the completed packet.
                            this.OnPacketReceived(deserializedPacket);

                            // Now remove the completed packet from the beginning of the stream
                            var updatedSteam = new byte[PacketStream.Length - packetSize];
                            Array.Copy(PacketStream, packetSize, updatedSteam, 0, PacketStream.Length - packetSize);
                            PacketStream = updatedSteam;

                            packetSize = Packet.DecodePacketSize(PacketStream);
                        }

                        // If we've recieved 16 kb's and still don't have a full command then shutdown the connection.
                        if (ReceivedBuffer.Length >= MaxGarbageBytes)
                        {
                            Shutdown();
                        }

                        if (this.NetworkStream != null)
                        {
                            this.NetworkStream.BeginRead(this.ReceivedBuffer, 0, this.ReceivedBuffer.Length, this.ReceiveCallback, this);
                        }
                    }
                    else
                    {
                        Shutdown();
                    }
                }
                catch (Exception)
                {
                    Shutdown();
                }
            }
            else
            {
                Shutdown();
            }
        }

        public virtual void Poke()
        {
            bool downstreamDead = this.LastPacketReceived != null && this.LastPacketReceived.Stamp < DateTime.Now.AddMinutes(-2);
            bool upstreamDead = this.LastPacketSent != null && this.LastPacketSent.Stamp < DateTime.Now.AddMinutes(-2);

            if (downstreamDead && upstreamDead)
            {
                // Prevent these from raising another poke-shutdown.
                this.LastPacketReceived = null;
                this.LastPacketSent = null;

                Shutdown();
            }
        }

        protected void OnPacketSent(Packet packet)
        {
            var handler = this.PacketSent;
            if (handler != null)
            {
                handler(this, packet);
            }
        }

        protected void OnPacketReceived(Packet packet)
        {
            var handler = this.PacketReceived;
            if (handler != null)
            {
                handler(this, packet);
            }
        }

        protected void OnConnectionClosed()
        {
            var handler = this.ConnectionClosed;
            if (handler != null)
            {
                handler(this);
            }
        }

        /// <summary>
        /// Nulls out all of the actions, 
        /// </summary>
        protected void NullActions()
        {
            this.ConnectionClosed = null;
            this.PacketReceived = null;
            this.PacketSent = null;
        }

        // TO DO: Better error reporting on this method.
        public void Shutdown()
        {
            try
            {
                lock (ShutdownConnectionLock)
                {
                    if (NetworkStream != null)
                    {
                        NetworkStream.Close();
                        NetworkStream.Dispose();
                        NetworkStream = null;
                    }
                    if (Client != null)
                    {
                        Client.Close();
                        Client = null;
                    }

                    this.OnConnectionClosed();
                }
            }
            catch (SocketException)
            {
                // TO DO: Error reporting, possibly in a log file.
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError("FrostbiteLayerConnection.Shutdown", "catch (Exception e)", e);
            }
            finally
            {
                this.NullActions();
            }
        }

        #region CrossDomainCode

        // Represents NULL-terminated "<policy-file-request/>"  
        protected static readonly byte[] PolicyRequest = new byte[] { 0x3c, 0x70, 0x6f, 0x6c, 0x69, 0x63, 0x79, 0x2d, 0x66, 0x69, 0x6c, 0x65, 0x2d, 0x72, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x2f, 0x3e, 0x00 };

        protected int ServeCrossDomainPolicy(int iBytesRead)
        {
            // Cross domain policy is only served once, at the begining of the TCP connection
            if (PacketStream != null)
                return iBytesRead;

            if (iBytesRead >= PolicyRequest.Length)
            {
                // Compare buffers, to see if policy request was received
                int i = 0;
                for (; i < PolicyRequest.Length; i++)
                    if (ReceivedBuffer[i] != PolicyRequest[i])
                        break;

                if (i == PolicyRequest.Length)
                {
                    // Comparison succeeded
                    int iLocalPort = ((IPEndPoint)Client.Client.LocalEndPoint).Port;

                    String sPolicyResponse = "<?xml version=\"1.0\"?>" + "<!DOCTYPE cross-domain-policy " + "SYSTEM \"http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd\">" + "<cross-domain-policy>" + "<allow-access-from domain=\"*\" to-ports=\"" + iLocalPort + "\" />" + "</cross-domain-policy>";

                    byte[] response = Encoding.GetEncoding(1252).GetBytes(sPolicyResponse + Convert.ToChar(0x00));
                    NetworkStream.Write(response, 0, response.Length);

                    // Remove the policy request from the begining of the receive buffer
                    iBytesRead -= PolicyRequest.Length;
                    Array.Copy(ReceivedBuffer, PolicyRequest.Length, ReceivedBuffer, 0, iBytesRead);
                }
            }

            return iBytesRead;
        }

        #endregion
    }
}