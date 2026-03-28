using Microsoft.Extensions.Logging;
using PRoCon.Core.Logging;
using PRoCon.Core.Remote.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
        /// This may be a plain NetworkStream or an SslStream wrapping it.
        /// </summary>
        protected Stream NetworkStream;

        /// <summary>
        /// The underlying SslStream when TLS is active, null otherwise.
        /// Kept separately so it can be properly disposed.
        /// </summary>
        protected SslStream SslStream;

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

        /// <summary>
        /// Whether to use TLS when connecting to the remote server.
        /// Default is false for backward compatibility.
        /// </summary>
        public bool UseTls { get; set; }

        /// <summary>
        /// When true, the TLS handshake will accept self-signed or otherwise
        /// invalid server certificates. Default is false (strict validation).
        /// </summary>
        public bool AllowSelfSignedCertificates { get; set; }

        /// <summary>
        /// When true and TLS handshake fails, fall back to a plain TCP connection
        /// instead of treating the failure as fatal. Default is false.
        /// </summary>
        // AllowTlsFallback removed — TLS downgrade is a security risk

        /// <summary>
        /// Cancellation token source for the receive loop and other async operations.
        /// Cancelled on shutdown.
        /// </summary>
        protected CancellationTokenSource ConnectionCts;

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

        private static readonly ILogger _logger = PRoConLog.CreateLogger("PRoCon.FrostbiteConnection");

        /// <summary>
        /// Logs an error with packet context. The original signature is preserved for
        /// backward compatibility, but logging now delegates to
        /// <see cref="Microsoft.Extensions.Logging.ILogger"/> via <see cref="PRoConLog"/>.
        /// A fallback to DEBUG.txt is kept so that errors are never silently lost when
        /// the logging subsystem has not been initialised.
        /// </summary>
        public static void LogError(string strPacket, string strAdditional, Exception e)
        {
            try
            {
                // Build the structured detail string (kept for file fallback as well).
                string strOutput = "=======================================" + Environment.NewLine + Environment.NewLine;

                StackTrace stTracer = new StackTrace(e, true);
                if (stTracer.FrameCount > 0)
                {
                    var frame = stTracer.GetFrame(stTracer.FrameCount - 1);
                    strOutput += "Exception caught at: " + Environment.NewLine;
                    strOutput += String.Format("{0}{1}", frame.GetFileName(), Environment.NewLine);
                    strOutput += String.Format("Line {0}{1}", frame.GetFileLineNumber(), Environment.NewLine);
                    strOutput += String.Format("Method {0}{1}", frame.GetMethod()?.Name, Environment.NewLine);
                }

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

                // ---- Primary path: ILogger ----
                _logger.LogError(e, "Packet={Packet} Additional={Additional}", strPacket, strAdditional);

                // ---- Fallback path: DEBUG.txt (kept for environments without logging config) ----
                try
                {
                    string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEBUG.txt");
                    using (StreamWriter sw = File.AppendText(debugPath))
                    {
                        sw.Write(strOutput);
                    }
                }
                catch (Exception)
                {
                    // Swallow file-write errors — the ILogger call above already recorded it.
                }
            }
            catch (Exception)
            {
                // It'd be too ironic to happen, surely?
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

        // Send straight away ignoring the queue
        private async Task SendAsync(Packet cpPacket, CancellationToken cancellationToken = default)
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

                    await this.NetworkStream.WriteAsync(bytePacket, 0, bytePacket.Length, cancellationToken).ConfigureAwait(false);

                    if (this.PacketSent != null)
                    {
                        this.LastPacketSent = cpPacket;

                        this.PacketSent(this, false, cpPacket);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested, do not treat as error
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
                    _ = this.SendAsync(cpPacket);
                }
                else
                {
                    // Null return because we're not popping a packet, just checking to see if this one needs to be queued.
                    if (this.QueueUnqueuePacket(true, cpPacket, out cpNullPacket) == false)
                    {
                        // No need to queue, queue is empty.  Send away..
                        _ = this.SendAsync(cpPacket);
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

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (this.NetworkStream == null)
                    {
                        return;
                    }

                    int iBytesRead = await this.NetworkStream.ReadAsync(this.ReceivedBuffer, 0, this.ReceivedBuffer.Length, cancellationToken).ConfigureAwait(false);

                    if (iBytesRead == 0)
                    {
                        this.Shutdown();
                        return;
                    }

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
                                await this.SendAsync(new Packet(true, true, packet.SequenceNumber, "OK"), cancellationToken).ConfigureAwait(false);
                            }

                            Packet cpNextPacket = null;
                            if (this.QueueUnqueuePacket(false, packet, out cpNextPacket) == true)
                            {
                                await this.SendAsync(cpNextPacket, cancellationToken).ConfigureAwait(false);
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
                                await this.SendAsync(cpNextPacket, cancellationToken).ConfigureAwait(false);
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
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested, normal shutdown path
            }
            catch (Exception e)
            {
                this.Shutdown(e);
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

        /// <summary>
        /// Certificate validation callback for TLS connections.
        /// When AllowSelfSignedCertificates is true, all certificates are accepted.
        /// </summary>
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (this.AllowSelfSignedCertificates)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets up the stream after TCP connection, optionally wrapping in TLS.
        /// Returns the stream to use for communication.
        /// </summary>
        private async System.Threading.Tasks.Task<Stream> SetupStreamAsync(NetworkStream networkStream, CancellationToken cancellationToken)
        {
            if (!this.UseTls)
            {
                return networkStream;
            }

            try
            {
                var sslStream = new SslStream(
                    networkStream,
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: this.ValidateServerCertificate
                );

                await sslStream.AuthenticateAsClientAsync(this.Hostname).ConfigureAwait(false);

                this.SslStream = sslStream;
                return sslStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TLS handshake failed for {Hostname}:{Port}. Connection aborted.", this.Hostname, this.Port);
                throw;
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.Client.ConnectAsync(this.Hostname, this.Port).ConfigureAwait(false);
                this.Client.NoDelay = true;

                if (this.ConnectSuccess != null)
                {
                    this.ConnectSuccess(this);
                }

                NetworkStream rawStream = this.Client.GetStream();
                this.NetworkStream = await this.SetupStreamAsync(rawStream, cancellationToken).ConfigureAwait(false);

                if (this.ConnectionReady != null)
                {
                    this.ConnectionReady(this);
                }

                // Start the receive loop (fire-and-forget, errors handled internally)
                _ = this.ReceiveLoopAsync(cancellationToken);
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

            if (IPAddress.TryParse(hostName, out ipReturn) != false)
            {
                return ipReturn;
            }

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

                this.ConnectionCts = new CancellationTokenSource();

                this.Client = new TcpClient();
                this.Client.NoDelay = true;

                if (this.ConnectAttempt != null)
                {
                    this.ConnectAttempt(this);
                }

                // Fire-and-forget the async connection; errors are handled internally
                _ = this.ConnectAsync(this.ConnectionCts.Token);
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
            if (this.Client == null)
            {
                return;
            }

            lock (this.ShutdownConnectionLock)
            {
                try
                {
                    // Cancel any in-flight async operations
                    if (this.ConnectionCts != null)
                    {
                        this.ConnectionCts?.Cancel();
                        // Don't dispose immediately - let async operations observe the cancellation
                        var oldCts = this.ConnectionCts;
                        this.ConnectionCts = null;
                        // Dispose after a delay to allow pending operations to complete
                        System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => oldCts?.Dispose());
                    }

                    this.ClearConnection();

                    if (this.SslStream != null)
                    {
                        // SslStream disposes the inner NetworkStream automatically
                        this.SslStream.Close();
                        this.SslStream.Dispose();
                        this.SslStream = null;
                        this.NetworkStream = null;
                    }
                    else if (this.NetworkStream != null)
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
