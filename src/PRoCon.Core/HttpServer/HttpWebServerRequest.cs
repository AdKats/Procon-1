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

using System;
using System.Net.Sockets;
using System.Text;

namespace PRoCon.Core.HttpServer
{
    public class HttpWebServerRequest
    {
        public delegate void ClientShutdownHandler(HttpWebServerRequest sender);

        public delegate void ResponseSentHandler(HttpWebServerRequest request, HttpWebServerResponseData response);

        protected readonly byte[] RecievedPacket;
        protected byte[] CompletedPacket;

        public HttpWebServerRequest(NetworkStream stream)
        {
            Stream = stream;
            RecievedPacket = new byte[4096];

            Stream.BeginRead(RecievedPacket, 0, RecievedPacket.Length, ReadWebRequests, null);
        }

        public NetworkStream Stream { get; private set; }

        public HttpWebServerRequestData Data { get; private set; }
        public event ResponseSentHandler ResponseSent;
        public event ClientShutdownHandler ClientShutdown;

        public event HttpWebServer.ProcessResponseHandler ProcessRequest;

        public void ProcessPacket()
        {
            if (CompletedPacket != null)
            {
                string packet = Encoding.ASCII.GetString(CompletedPacket);

                Data = new HttpWebServerRequestData(packet);

                if (ProcessRequest != null)
                {
                    this.ProcessRequest(this);
                }
            }
        }

        public override string ToString()
        {
            return Data.Request;
        }

        #region Reading packet

        private void CompilePacket(int recievedData)
        {
            if (CompletedPacket == null)
            {
                CompletedPacket = new byte[recievedData];
            }
            else
            {
                Array.Resize(ref CompletedPacket, CompletedPacket.Length + recievedData);
            }

            Array.Copy(RecievedPacket, 0, CompletedPacket, CompletedPacket.Length - recievedData, recievedData);
        }

        private void ReadWebRequests(IAsyncResult ar)
        {
            try
            {
                //HttpWebServerRequest client = (HttpWebServerRequest)ar.AsyncState;

                int iBytesRead = Stream.EndRead(ar);

                if (iBytesRead > 0)
                {
                    CompilePacket(iBytesRead);

                    if (Stream.DataAvailable == true)
                    {
                        Stream.BeginRead(RecievedPacket, 0, RecievedPacket.Length, ReadWebRequests, null);
                    }
                    else
                    {
                        ProcessPacket();
                    }
                }
            }
            catch (Exception)
            {
            }

            Shutdown();
        }

        public void Shutdown()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;

                if (ClientShutdown != null)
                {
                    this.ClientShutdown(this);
                }
            }
        }

        #endregion

        #region Sending Packet

        public void Respond(HttpWebServerResponseData response)
        {
            try
            {
                if (Stream != null)
                {
                    byte[] bData = Encoding.UTF8.GetBytes(response.ToString());

                    Stream.Write(bData, 0, bData.Length);

                    if (ResponseSent != null)
                    {
                        this.ResponseSent(this, response);
                    }

                    Shutdown();
                }
            }
            catch (Exception e)
            {
            }
        }

        #endregion
    }
}