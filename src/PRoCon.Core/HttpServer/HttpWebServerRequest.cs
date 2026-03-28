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
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PRoCon.Core.HttpServer
{
    public class HttpWebServerRequest
    {
        public delegate void ClientShutdownHandler(HttpWebServerRequest sender);

        public delegate void ResponseSentHandler(HttpWebServerRequest request, HttpWebServerResponseData response);

        protected readonly byte[] RecievedPacket;
        protected byte[] CompletedPacket;

        public HttpWebServerRequest(Stream stream)
        {
            Stream = stream;
            RecievedPacket = new byte[4096];

        }

        public void Start()
        {
            _ = ReadRequestAsync();
        }

        public Stream Stream { get; private set; }

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

        private async System.Threading.Tasks.Task ReadRequestAsync()
        {
            try
            {
                int bytesRead = await Stream.ReadAsync(RecievedPacket, 0, RecievedPacket.Length);

                if (bytesRead > 0)
                {
                    CompilePacket(bytesRead);

                    // For NetworkStream we could check DataAvailable, but for generic Stream
                    // we read what we got and process it. HTTP requests are typically sent in one packet.
                    ProcessPacket();
                }
            }
            catch (Exception)
            {
            }

            Shutdown();
        }

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
            catch (Exception)
            {
            }
        }
    }
}
