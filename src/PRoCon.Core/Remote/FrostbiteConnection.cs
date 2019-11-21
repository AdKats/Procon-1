using PRoCon.Core.Remote.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace PRoCon.Core.Remote
{
    public class FrostbiteConnection
    {

        /// <summary>
        /// The open client connection.
        /// </summary>
        protected System.Net.Sockets.TcpClient Client;

        /// <summary>
        /// The stream to read and write data to.
        /// </summary>
        protected NetworkStream NetworkStream;

        // Maximum amount of data to accept before scrapping the whole lot and trying again.
        // Test maximizing this to see if plugin descriptions are causing some problems.
        private const UInt32 MaxGarbageBytes = 4194304;
        private const UInt16 BufferSize = 16384;

        /// <summary>
        /// A list of packets currently sent to the server and awaiting a response
        /// </summary>
        protected Dictionary<UInt32?, Packet> OutgoingPackets;

        /// <summary>
        /// A queue of packets to send to the server (waiting until the outgoing packets list is clear)
        /// </summary>
        protected Queue<Packet> QueuedPackets;

        /// <summary>
        /// Data collected so far for a packet.
        /// </summary>
        protected byte[] ReceivedBuffer;

        /// <summary>
        /// Buffer for the data currently being read from the stream. This is appended to the received buffer.
        /// </summary>
        protected byte[] PacketStream;

        /// <summary>
        /// Lock used when aquiring a sequence #
        /// </summary>
        protected readonly Object AcquireSequenceNumberLock = new Object();

        /// <summary>
        /// The last packet that was receieved by this connection.
        /// </summary>
        public Packet LastPacketReceived { get; protected set; }

        /// <summary>
        /// The last packet that was sent by this connection.
        /// </summary>
        public Packet LastPacketSent { get; protected set; }

        /// <summary>
        /// Holds packet cache to avoid doubling up on calls to the server.
        /// </summary>
        public ICacheManager Cache { get; set; }

        /// <summary>
        /// Why is this here?
        /// </summary>
        protected UInt32 SequenceNumber;
        public UInt32 AcquireSequenceNumber
        {
            get
            {
                lock (this.AcquireSequenceNumberLock)
                {
                    return ++this.SequenceNumber;
                }
            }
        }

        protected Object ShutdownConnectionLock = new Object();

        public string Hostname
        {
            get;
            private set;
        }

        public UInt16 Port
        {
            get;
            private set;
        }

        public bool IsConnected
        {
            get
            {
                return this.Client != null && this.Client.Connected;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return this.Client != null && true ^ this.Client.Connected;
            }
        }

        /// <summary>
        /// If the current shutdown was requested with this.Shutdown(), or if it's a result of
        /// an error.
        /// </summary>
        public bool IsRequestedShutdown { get; set; }

        /// <summary>
        /// Lock for processing new queue items
        /// </summary>
        protected readonly Object QueueUnqueuePacketLock = new Object();

        #region Events

        public delegate void PrePacketDispatchedHandler(FrostbiteConnection sender, Packet packetBeforeDispatch, out bool isProcessed);
        public event PrePacketDispatchedHandler BeforePacketDispatch;
        public event PrePacketDispatchedHandler BeforePacketSend;

        public delegate void PacketDispatchHandler(FrostbiteConnection sender, bool isHandled, Packet packet);
        public event PacketDispatchHandler PacketSent;
        public event PacketDispatchHandler PacketReceived;

        public delegate void PacketCacheDispatchHandler(FrostbiteConnection sender, Packet request, Packet response);
        /// <summary>
        /// A packet response has been pulled from cache, instead of being sent to the server.
        /// </summary>
        public event PacketCacheDispatchHandler PacketCacheIntercept;

        public delegate void SocketExceptionHandler(FrostbiteConnection sender, SocketException se);
        public event SocketExceptionHandler SocketException;

        public delegate void FailureHandler(FrostbiteConnection sender, Exception exception);
        public event FailureHandler ConnectionFailure;

        public delegate void PacketQueuedHandler(FrostbiteConnection sender, Packet cpPacket, int iThreadId);

        public event PacketQueuedHandler PacketQueued;
        public event PacketQueuedHandler PacketDequeued;

        public delegate void EmptyParamterHandler(FrostbiteConnection sender);
        public event EmptyParamterHandler ConnectAttempt;
        public event EmptyParamterHandler ConnectSuccess;
        public event EmptyParamterHandler ConnectionClosed;
        public event EmptyParamterHandler ConnectionReady;

        #endregion

        public FrostbiteConnection(string hostname, UInt16 port)
        {
            this.ClearConnection();

            this.Hostname = hostname;
            this.Port = port;

            this.Cache = new CacheManager()
            {
                Configurations = new List<IPacketCacheConfiguration>() {
                    // Cache all ping values for 30 seconds.
                    new PacketCacheConfiguration() {
                        Matching = new Regex(@"^player\.ping .*$", RegexOptions.Compiled),
                        Ttl = new TimeSpan(0, 0, 0, 30)
                    },
                    // Cache all banlist responses for two minutes
                    new PacketCacheConfiguration() {
                        Matching = new Regex(@"^banList\.list[ 0-9]*$", RegexOptions.Compiled),
                        Ttl = new TimeSpan(0, 0, 1, 0)
                    },
                    // Cache all reserved slit responses for one minute
                    new PacketCacheConfiguration() {
                        Matching = new Regex(@"^reservedSlotsList\.list [0-9]*$", RegexOptions.Compiled),
                        Ttl = new TimeSpan(0, 0, 1, 0)
                    },
                    // Only initiate the punkbuster plist update everyminute (max)
                    new PacketCacheConfiguration() {
                        Matching = new Regex(@"^punkBuster\.pb_sv_command pb_sv_plist$", RegexOptions.Compiled),
                        Ttl = new TimeSpan(0, 0, 1, 0)
                    }
                }
            };
        }

        private void ClearConnection()
        {
            this.SequenceNumber = 0;

            this.OutgoingPackets = new Dictionary<uint?, Packet>();
            this.QueuedPackets = new Queue<Packet>();

            this.ReceivedBuffer = new byte[FrostbiteConnection.BufferSize];
            this.PacketStream = null;
        }

        public static void LogError(string strPacket, string strAdditional, Exception e)
        {
            try
            {
                string strOutput = "=======================================" + Environment.NewLine + Environment.NewLine;

                StackTrace stTracer = new StackTrace(e, true);
                strOutput += "Exception caught at: " + Environment.NewLine;
                strOutput += String.Format("{0}{1}", stTracer.GetFrame((stTracer.FrameCount - 1)).GetFileName(), Environment.NewLine);
                strOutput += String.Format("Line {0}{1}", stTracer.GetFrame((stTracer.FrameCount - 1)).GetFileLineNumber(), Environment.NewLine);
                strOutput += String.Format("Method {0}{1}", stTracer.GetFrame((stTracer.FrameCount - 1)).GetMethod().Name, Environment.NewLine);

                strOutput += "DateTime: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine;
                strOutput += "Version: " + Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine;

                strOutput += "Packet: " + Environment.NewLine;
                strOutput += strPacket + Environment.NewLine;

                strOutput += "Additional: " + Environment.NewLine;
                strOutput += strAdditional + Environment.NewLine;

                strOutput += Environment.NewLine;
                strOutput += e.Message + Environment.NewLine;

                strOutput += Environment.NewLine;
                strOutput += stTracer.ToString();

                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEBUG.txt")))
                {
                    using (StreamWriter sw = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEBUG.txt")))
                    {
                        sw.Write(strOutput);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEBUG.txt")))
                    {
                        sw.Write(strOutput);
                    }
                }
            }
            catch (Exception)
            {
                // It'd be to ironic to happen, surely?
            }
        }

        private bool QueueUnqueuePacket(bool isSending, Packet packet, out Packet nextPacket)
        {

            nextPacket = null;
            bool response = false;

            lock (this.QueueUnqueuePacketLock)
            {

                if (isSending == true)
                {
                    // If we have something that has been sent and is awaiting a response
                    if (this.OutgoingPackets.Count > 0)
                    {
                        // Add the packet to our queue to be sent at a later time.
                        this.QueuedPackets.Enqueue(packet);

                        response = true;

                        if (this.PacketQueued != null)
                        {
                            this.PacketQueued(this, packet, Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                    // else - response = false
                }
                else
                {
                    // Else it's being called from recv and cpPacket holds the processed RequestPacket.

                    // Remove the packet 
                    if (packet != null)
                    {
                        if (this.OutgoingPackets.ContainsKey(packet.SequenceNumber) == true)
                        {
                            this.OutgoingPackets.Remove(packet.SequenceNumber);
                        }
                    }

                    if (this.QueuedPackets.Count > 0)
                    {
                        nextPacket = this.QueuedPackets.Dequeue();

                        response = true;

                        if (this.PacketDequeued != null)
                        {
                            this.PacketDequeued(this, nextPacket, Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                    else
                    {
                        response = false;
                    }

                }
            }

            return response;
        }

        private void SendAsyncCallback(IAsyncResult ar)
        {

            Packet packet = (Packet)ar.AsyncState;

            try
            {
                if (this.NetworkStream != null)
                {
                    this.NetworkStream.EndWrite(ar);
                    if (this.PacketSent != null)
                    {

                        this.LastPacketSent = packet;

                        this.PacketSent(this, false, packet);
                    }
                }
            }
            catch (SocketException se)
            {
                this.Shutdown(se);
            }
            catch (Exception e)
            {
                this.Shutdown(e);
            }
        }

        // Send straight away ignoring the queue
        private void SendAsync(Packet cpPacket)
        {
            try
            {

                bool isProcessed = false;

                if (this.BeforePacketSend != null)
                {
                    this.BeforePacketSend(this, cpPacket, out isProcessed);
                }

                if (isProcessed == false && this.NetworkStream != null)
                {

                    byte[] bytePacket = cpPacket.EncodePacket();

                    lock (this.QueueUnqueuePacketLock)
                    {
                        if (cpPacket.OriginatedFromServer == false && cpPacket.IsResponse == false && this.OutgoingPackets.ContainsKey(cpPacket.SequenceNumber) == false)
                        {
                            this.OutgoingPackets.Add(cpPacket.SequenceNumber, cpPacket);
                        }
                    }

                    this.NetworkStream.BeginWrite(bytePacket, 0, bytePacket.Length, this.SendAsyncCallback, cpPacket);

                }
            }
            catch (SocketException se)
            {
                this.Shutdown(se);
            }
            catch (Exception e)
            {
                this.Shutdown(e);
            }
        }

        // Queue for sending.
        public void SendQueued(Packet cpPacket)
        {
            IPacketCache cache = this.Cache.Request(cpPacket);

            if (cache == null)
            {
                // QueueUnqueuePacket
                Packet cpNullPacket = null;

                if (cpPacket.OriginatedFromServer == true && cpPacket.IsResponse == true)
                {
                    this.SendAsync(cpPacket);
                }
                else
                {
                    // Null return because we're not popping a packet, just checking to see if this one needs to be queued.
                    if (this.QueueUnqueuePacket(true, cpPacket, out cpNullPacket) == false)
                    {
                        // No need to queue, queue is empty.  Send away..
                        this.SendAsync(cpPacket);
                    }

                    // Shutdown if we're just waiting for a response to an old packet.
                    this.RestartConnectionOnQueueFailure();
                }
            }
            else if (this.PacketCacheIntercept != null)
            {
                Packet cloned = (Packet)cache.Response.Clone();
                cloned.SequenceNumber = cpPacket.SequenceNumber;

                // Fake a response to this packet
                this.PacketCacheIntercept(this, cpPacket, cloned);
            }
        }

        public Packet GetRequestPacket(Packet cpRecievedPacket)
        {

            Packet cpRequestPacket = null;

            lock (this.QueueUnqueuePacketLock)
            {
                if (this.OutgoingPackets.ContainsKey(cpRecievedPacket.SequenceNumber) == true)
                {
                    cpRequestPacket = this.OutgoingPackets[cpRecievedPacket.SequenceNumber];
                }
            }

            return cpRequestPacket;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {

            if (this.NetworkStream != null)
            {
                try
                {
                    int iBytesRead = this.NetworkStream.EndRead(ar);

                    if (iBytesRead > 0)
                    {

                        // Create or resize our packet stream to hold the new data.
                        if (this.PacketStream == null)
                        {
                            this.PacketStream = new byte[iBytesRead];
                        }
                        else
                        {
                            Array.Resize(ref this.PacketStream, this.PacketStream.Length + iBytesRead);
                        }

                        Array.Copy(this.ReceivedBuffer, 0, this.PacketStream, this.PacketStream.Length - iBytesRead, iBytesRead);

                        UInt32 ui32PacketSize = Packet.DecodePacketSize(this.PacketStream);

                        while (this.PacketStream != null && this.PacketStream.Length >= ui32PacketSize && this.PacketStream.Length > Packet.PacketHeaderSize)
                        {

                            // Copy the complete packet from the beginning of the stream.
                            byte[] completePacket = new byte[ui32PacketSize];
                            Array.Copy(this.PacketStream, completePacket, ui32PacketSize);

                            Packet packet = new Packet(completePacket);
                            //cbfConnection.m_ui32SequenceNumber = Math.Max(cbfConnection.m_ui32SequenceNumber, cpCompletePacket.SequenceNumber) + 1;

                            // Dispatch the completed packet.
                            try
                            {
                                bool isProcessed = false;

                                if (this.BeforePacketDispatch != null)
                                {
                                    this.BeforePacketDispatch(this, packet, out isProcessed);
                                }

                                if (this.PacketReceived != null)
                                {
                                    this.LastPacketReceived = packet;

                                    this.Cache.Response(packet);

                                    this.PacketReceived(this, isProcessed, packet);
                                }

                                if (packet.OriginatedFromServer == true && packet.IsResponse == false)
                                {
                                    this.SendAsync(new Packet(true, true, packet.SequenceNumber, "OK"));
                                }

                                Packet cpNextPacket = null;
                                if (this.QueueUnqueuePacket(false, packet, out cpNextPacket) == true)
                                {
                                    this.SendAsync(cpNextPacket);
                                }

                                // Shutdown if we're just waiting for a response to an old packet.
                                this.RestartConnectionOnQueueFailure();
                            }
                            catch (Exception e)
                            {

                                Packet cpRequest = this.GetRequestPacket(packet);

                                if (cpRequest != null)
                                {
                                    LogError(packet.ToDebugString(), cpRequest.ToDebugString(), e);
                                }
                                else
                                {
                                    LogError(packet.ToDebugString(), String.Empty, e);
                                }

                                // Now try to recover..
                                Packet cpNextPacket = null;
                                if (this.QueueUnqueuePacket(false, packet, out cpNextPacket) == true)
                                {
                                    this.SendAsync(cpNextPacket);
                                }

                                // Shutdown if we're just waiting for a response to an old packet.
                                this.RestartConnectionOnQueueFailure();
                            }

                            // Now remove the completed packet from the beginning of the stream
                            if (this.PacketStream != null)
                            {
                                byte[] updatedSteam = new byte[this.PacketStream.Length - ui32PacketSize];
                                Array.Copy(this.PacketStream, ui32PacketSize, updatedSteam, 0, this.PacketStream.Length - ui32PacketSize);
                                this.PacketStream = updatedSteam;

                                ui32PacketSize = Packet.DecodePacketSize(this.PacketStream);
                            }
                        }

                        // If we've recieved the maxmimum garbage, scrap it all and shutdown the connection.
                        // We went really wrong somewhere =)
                        if (this.ReceivedBuffer.Length >= FrostbiteConnection.MaxGarbageBytes)
                        {
                            this.ReceivedBuffer = null; // GC.collect()
                            this.Shutdown(new Exception("Exceeded maximum garbage packet"));
                        }

                        if (this.NetworkStream != null)
                        {
                            this.NetworkStream.BeginRead(this.ReceivedBuffer, 0, this.ReceivedBuffer.Length, this.ReceiveCallback, this);
                        }
                    }
                    else if (iBytesRead == 0)
                    {
                        this.Shutdown();
                    }
                }
                catch (SocketException se)
                {
                    this.Shutdown(se);
                }
                catch (Exception e)
                {
                    this.Shutdown(e);
                }
            }
        }

        /// <summary>
        /// Validates that packets are not 'lost' after being sent. If this is the case then the connection is shutdown
        /// to then be rebooted at a later time.
        /// 
        /// If a packet exists in our outgoing "SentPackets"
        /// </summary>
        protected void RestartConnectionOnQueueFailure()
        {
            bool restart = false;

            lock (this.QueueUnqueuePacketLock)
            {
                restart = this.OutgoingPackets.Any(outgoingPacket => outgoingPacket.Value.Stamp < DateTime.Now.AddMinutes(-2));

                if (restart == true)
                {
                    this.OutgoingPackets.Clear();
                    this.QueuedPackets.Clear();
                }
            }

            // We do this outside of the lock to ensure calls outside this method won't result in a deadlock elsewhere.
            if (restart == true)
            {
                this.Shutdown(new Exception("Failed to hear response to packet within two minutes, forced shutdown."));
            }
        }

        /// <summary>
        /// Pokes the connection, ensuring that the connection is still alive. If
        /// this method determines that the connection is dead then it will call for
        /// a shutdown.
        /// </summary>
        /// <remarks>
        ///     <para>
        /// This method is a final check to make sure communications are proceeding in both directions in
        /// the last five minutes. If nothing has been sent and received in the last five minutes then the connection is assumed
        /// dead and a shutdown is initiated.
        /// </para>
        /// </remarks>
        public virtual void Poke()
        {
            bool downstreamDead = this.LastPacketReceived != null && this.LastPacketReceived.Stamp < DateTime.Now.AddMinutes(-2);
            bool upstreamDead = this.LastPacketSent != null && this.LastPacketSent.Stamp < DateTime.Now.AddMinutes(-2);

            if (downstreamDead && upstreamDead)
            {
                // Clear these out so we don't pick it up again on the next connection attempt.
                this.LastPacketReceived = null;
                this.LastPacketSent = null;

                // Now shutdown the connection, it's dead jim.
                this.Shutdown();

                // Alert the ProconClient of the error, explaining why the connection has been shut down.
                if (this.ConnectionFailure != null)
                {
                    this.ConnectionFailure(this, new Exception("Connection timed out with two minutes of inactivity."));
                }
            }
        }

        private void ConnectedCallback(IAsyncResult ar)
        {

            try
            {
                this.Client.EndConnect(ar);
                this.Client.NoDelay = true;

                if (this.ConnectSuccess != null)
                {
                    this.ConnectSuccess(this);
                }

                this.NetworkStream = this.Client.GetStream();
                this.NetworkStream.BeginRead(this.ReceivedBuffer, 0, this.ReceivedBuffer.Length, this.ReceiveCallback, this);

                if (this.ConnectionReady != null)
                {
                    this.ConnectionReady(this);
                }
            }
            catch (SocketException se)
            {
                this.Shutdown(se);
            }
            catch (Exception e)
            {
                this.Shutdown(e);
            }
        }

        public static IPAddress ResolveHostName(string hostName)
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

        public void AttemptConnection()
        {
            try
            {
                // Clear this, everything from now on will throw an error.
                this.IsRequestedShutdown = false;

                lock (this.QueueUnqueuePacketLock)
                {
                    this.QueuedPackets.Clear();
                    this.OutgoingPackets.Clear();
                }
                this.SequenceNumber = 0;

                this.Client = new TcpClient();
                this.Client.NoDelay = true;
                this.Client.BeginConnect(this.Hostname, this.Port, this.ConnectedCallback, this);

                if (this.ConnectAttempt != null)
                {
                    this.ConnectAttempt(this);
                }
            }
            catch (SocketException se)
            {
                this.Shutdown(se);
            }
            catch (Exception e)
            {
                this.Shutdown(e);
            }
        }

        public void Shutdown(Exception e)
        {
            this.ShutdownConnection();

            // If we're not currently shutdown from an external request
            if (this.IsRequestedShutdown == false && this.ConnectionFailure != null)
            {
                this.ConnectionFailure(this, e);
            }
        }

        public void Shutdown(SocketException se)
        {
            this.ShutdownConnection();

            // If we're not currently shutdown from an external request
            if (this.IsRequestedShutdown == false && this.SocketException != null)
            {
                this.SocketException(this, se);
            }
        }

        public void Shutdown()
        {
            // We've been asked to shutdown gracefully. We'll do so and supress any errors
            // that occur during the shutdown.
            this.IsRequestedShutdown = true;

            this.ShutdownConnection();
        }

        protected void ShutdownConnection()
        {

            if (this.Client != null)
            {
                lock (this.ShutdownConnectionLock)
                {
                    try
                    {

                        this.ClearConnection();

                        if (this.NetworkStream != null)
                        {
                            this.NetworkStream.Close();
                            this.NetworkStream.Dispose();
                            this.NetworkStream = null;
                        }

                        this.Client.Close();
                        this.Client = null;

                        if (this.ConnectionClosed != null)
                        {
                            this.ConnectionClosed(this);
                        }
                    }
                    catch (SocketException se)
                    {
                        if (this.SocketException != null)
                        {
                            this.SocketException(this, se);
                            //this.SocketException(se);
                        }
                    }
                    catch (Exception e)
                    {
                        if (this.ConnectionFailure != null)
                        {
                            this.ConnectionFailure(this, e);
                        }
                    }
                }
            }
        }
    }
}
