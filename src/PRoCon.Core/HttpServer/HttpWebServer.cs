// Copyright 2010 Geoffrey 'Phogue' Green
//
// http://www.phogue.net
//
// This file is part of PRoCon Frostbite.
//
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.


using PRoCon.Core.HttpServer.Cache;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PRoCon.Core.HttpServer
{
    public class HttpWebServer
    {
        public delegate void ProcessResponseHandler(HttpWebServerRequest sender);

        public delegate void StateChangeHandler(HttpWebServer sender);

        protected readonly Dictionary<string, HttpWebServerResponseData> CachedResponses;
        protected readonly List<HttpWebServerRequest> HttpClients;
        protected TcpListener Listener;
        private CancellationTokenSource _cts;

        public HttpWebServer(string bindingAddress, UInt16 port)
        {
            HttpClients = new List<HttpWebServerRequest>();
            CachedResponses = new Dictionary<string, HttpWebServerResponseData>();

            BindingAddress = bindingAddress;
            ListeningPort = port;
        }

        public string BindingAddress { get; set; }

        public UInt16 ListeningPort { get; set; }

        public bool IsOnline { get; private set; }

        public X509Certificate2 TlsCertificate { get; set; }

        public bool UseTls => TlsCertificate != null;

        public event ProcessResponseHandler ProcessRequest;
        public event StateChangeHandler HttpServerOnline;
        public event StateChangeHandler HttpServerOffline;

        private IPAddress ResolveHostName(string strHostName)
        {
            IPAddress ipReturn = IPAddress.None;

            if (IPAddress.TryParse(strHostName, out ipReturn) == false)
            {
                ipReturn = IPAddress.None;

                try
                {
                    IPHostEntry iphHost = Dns.GetHostEntry(strHostName);

                    if (iphHost.AddressList.Length > 0)
                    {
                        ipReturn = iphHost.AddressList[0];
                    }
                }
                catch (Exception)
                {
                }
            }

            return ipReturn;
        }

        public void Start()
        {
            try
            {
                IPAddress ipBinding = ResolveHostName(BindingAddress);

                Listener = new TcpListener(ipBinding, ListeningPort);
                Listener.Start();

                _cts = new CancellationTokenSource();
                IsOnline = true;

                // Start accepting connections asynchronously
                _ = AcceptConnectionsAsync(_cts.Token);

                if (HttpServerOnline != null)
                {
                    this.HttpServerOnline(this);
                }
            }
            catch (SocketException)
            {
                Shutdown();
            }
        }

        private async System.Threading.Tasks.Task AcceptConnectionsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Listener != null)
            {
                try
                {
                    TcpClient tcpClient = await Listener.AcceptTcpClientAsync(token);
                    _ = HandleClientAsync(tcpClient);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Continue accepting on transient errors
                }
            }
        }

        private async System.Threading.Tasks.Task HandleClientAsync(TcpClient tcpClient)
        {
            try
            {
                System.IO.Stream stream = tcpClient.GetStream();

                if (UseTls)
                {
                    var sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsServerAsync(TlsCertificate);
                    stream = sslStream;
                }

                var newClient = new HttpWebServerRequest(stream);
                newClient.ProcessRequest += new ProcessResponseHandler(newClient_ProcessRequest);
                newClient.ResponseSent += new HttpWebServerRequest.ResponseSentHandler(newClient_ResponseSent);
                newClient.ClientShutdown += new HttpWebServerRequest.ClientShutdownHandler(newClient_ClientShutdown);

                lock (HttpClients)
                {
                    HttpClients.Add(newClient);
                }

                newClient.Start();
            }
            catch (Exception)
            {
                tcpClient?.Close();
            }
        }

        private void newClient_ResponseSent(HttpWebServerRequest request, HttpWebServerResponseData response)
        {
            if (response.Cache.CacheType == HttpWebServerCacheType.Cache)
            {
                lock (CachedResponses)
                {
                    if (!CachedResponses.ContainsKey(request.ToString()))
                    {
                        CachedResponses.Add(request.ToString(), response);
                    }
                }
            }
        }

        private void newClient_ProcessRequest(HttpWebServerRequest sender)
        {
            // Scrub the cache for old responses
            lock (CachedResponses)
            {
                foreach (string key in new List<string>(CachedResponses.Keys))
                {
                    if (CachedResponses[key].Cache.TrashTime <= DateTime.Now)
                    {
                        CachedResponses.Remove(key);
                    }
                }

                if (CachedResponses.ContainsKey(sender.ToString()))
                {
                    sender.Respond(CachedResponses[sender.ToString()]);
                    return;
                }
            }

            if (ProcessRequest != null)
            {
                this.ProcessRequest(sender);
            }
        }

        private void newClient_ClientShutdown(HttpWebServerRequest sender)
        {
            lock (HttpClients)
            {
                HttpClients.Remove(sender);
            }
        }

        public void Shutdown()
        {
            try
            {
                IsOnline = false;
                _cts?.Cancel();

                lock (HttpClients)
                {
                    foreach (HttpWebServerRequest client in new List<HttpWebServerRequest>(HttpClients))
                    {
                        client.Shutdown();
                    }
                }

                if (Listener != null)
                {
                    Listener.Stop();
                    Listener = null;
                }

                if (HttpServerOffline != null)
                {
                    this.HttpServerOffline(this);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
