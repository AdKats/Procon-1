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
using System.Net.Sockets;

namespace PRoCon.Core.HttpServer
{
    public class HttpWebServer
    {
        public delegate void ProcessResponseHandler(HttpWebServerRequest sender);

        public delegate void StateChangeHandler(HttpWebServer sender);

        protected readonly Dictionary<string, HttpWebServerResponseData> CachedResponses;
        protected readonly List<HttpWebServerRequest> HttpClients;
        protected TcpListener Listener;

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
                    // ELSE return IPAddress.None..
                }
                catch (Exception)
                {
                } // Returns IPAddress.None..
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

                Listener.BeginAcceptTcpClient(ListenIncommingWebRequests, null);
                IsOnline = true;

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

        // private AsyncCallback m_asyncAcceptCallback = new AsyncCallback(PRoConLayer.ListenIncommingLayerConnections);
        private void ListenIncommingWebRequests(IAsyncResult ar)
        {
            TcpClient tcpNewConnection = null;
            try
            {
                tcpNewConnection = Listener.EndAcceptTcpClient(ar);

                var newClient = new HttpWebServerRequest(tcpNewConnection.GetStream());
                newClient.ProcessRequest += new ProcessResponseHandler(newClient_ProcessRequest);
                newClient.ResponseSent += new HttpWebServerRequest.ResponseSentHandler(newClient_ResponseSent);
                newClient.ClientShutdown += new HttpWebServerRequest.ClientShutdownHandler(newClient_ClientShutdown);

                HttpClients.Add(newClient);

                //if (this.m_tcpListener != null) {
                //    this.m_tcpListener.BeginAcceptTcpClient(this.ListenIncommingWebRequests, null);
                //}
            }
            catch (Exception)
            {
                if (tcpNewConnection != null)
                {
                    tcpNewConnection.Close();
                }
            }

            try
            {
                if (Listener != null)
                {
                    Listener.BeginAcceptTcpClient(ListenIncommingWebRequests, null);
                }
            }
            catch (Exception)
            {
                Shutdown();
            }
        }

        private void newClient_ResponseSent(HttpWebServerRequest request, HttpWebServerResponseData response)
        {
            if (response.Cache.CacheType == HttpWebServerCacheType.Cache && CachedResponses.ContainsKey(request.ToString()) == false)
            {
                CachedResponses.Add(request.ToString(), response);
            }
        }

        private void newClient_ProcessRequest(HttpWebServerRequest sender)
        {
            // Scrub the cache for old responses
            foreach (string key in new List<string>(CachedResponses.Keys))
            {
                if (CachedResponses[key].Cache.TrashTime >= DateTime.Now)
                {
                    CachedResponses.Remove(key);
                }
            }

            if (CachedResponses.ContainsKey(sender.ToString()) == true)
            {
                sender.Respond(CachedResponses[sender.ToString()]);
            }
            else if (ProcessRequest != null)
            {
                this.ProcessRequest(sender);
            }
        }

        private void newClient_ClientShutdown(HttpWebServerRequest sender)
        {
            if (HttpClients.Contains(sender) == true)
            {
                HttpClients.Remove(sender);
            }
        }

        public void Shutdown()
        {
            try
            {
                IsOnline = false;

                foreach (HttpWebServerRequest client in new List<HttpWebServerRequest>(HttpClients))
                {
                    client.Shutdown();
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