using PRoCon.Core.Logging;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;

// This class will move to .Core once ProConClient is in .Core.

namespace PRoCon.Core.Consoles
{
    public class ConnectionConsole : Loggable
    {
        public delegate void IsEnabledHandler(bool isEnabled);

        protected PRoConClient Client;
        private bool _conoleScrolling;
        private bool _displayConnection;
        private bool _displayPunkbuster;
        private bool _logDebugDetails;
        private bool _logEventsConnection;
        private bool _punkbusterScrolling;

        public ConnectionConsole(PRoConClient prcClient) : base()
        {
            Client = prcClient;

            FileHostNamePort = Client.FileHostNamePort;
            LoggingStartedPrefix = "Console logging started";
            LoggingStoppedPrefix = "Console logging stopped";
            FileNameSuffix = "console";

            LogDebugDetails = false;
            LogEventsConnection = false;
            DisplayConnection = true;
            DisplayPunkbuster = true;
            ConScrolling = true;
            PBScrolling = true;

            Client.Game.Connection.PacketQueued += new FrostbiteConnection.PacketQueuedHandler(m_prcClient_PacketQueued);
            Client.Game.Connection.PacketDequeued += new FrostbiteConnection.PacketQueuedHandler(m_prcClient_PacketDequeued);
            Client.Game.Connection.PacketSent += new FrostbiteConnection.PacketDispatchHandler(m_prcClient_PacketSent);
            Client.Game.Connection.PacketReceived += new FrostbiteConnection.PacketDispatchHandler(m_prcClient_PacketRecieved);
            Client.Game.Connection.PacketCacheIntercept += new FrostbiteConnection.PacketCacheDispatchHandler(Connection_PacketCacheIntercept);

            Client.ConnectAttempt += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandConnectAttempt);
            Client.ConnectSuccess += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandConnectSuccess);

            Client.ConnectionFailure += new PRoConClient.FailureHandler(m_prcClient_ConnectionFailure);
            Client.ConnectionClosed += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);

            Client.LoginAttempt += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLoginAttempt);
            Client.Login += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogin);
            Client.LoginFailure += new PRoConClient.AuthenticationFailureHandler(m_prcClient_CommandLoginFailure);
            Client.Logout += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogout);
        }


        public UInt32 BytesRecieved { get; private set; }

        public UInt32 BytesSent { get; private set; }

        public bool LogEventsConnection
        {
            get { return _logEventsConnection; }
            set
            {
                if (LogEventsConnectionChanged != null)
                {
                    this.LogEventsConnectionChanged(value);
                }

                _logEventsConnection = value;
            }
        }

        public bool LogDebugDetails
        {
            get { return _logDebugDetails; }
            set
            {
                if (LogDebugDetailsChanged != null)
                {
                    this.LogDebugDetailsChanged(value);
                }

                _logDebugDetails = value;
            }
        }

        public bool DisplayConnection
        {
            get { return _displayConnection; }
            set
            {
                if (DisplayConnectionChanged != null)
                {
                    this.DisplayConnectionChanged(value);
                }

                _displayConnection = value;
            }
        }

        public bool DisplayPunkbuster
        {
            get { return _displayPunkbuster; }
            set
            {
                if (DisplayPunkbusterChanged != null)
                {
                    this.DisplayPunkbusterChanged(value);
                }

                _displayPunkbuster = value;
            }
        }

        public bool ConScrolling
        {
            get { return _conoleScrolling; }
            set
            {
                if (ConScrollingChanged != null)
                {
                    this.ConScrollingChanged(value);
                }

                _conoleScrolling = value;
            }
        }

        public bool PBScrolling
        {
            get { return _punkbusterScrolling; }
            set
            {
                if (PBScrollingChanged != null)
                {
                    this.PBScrollingChanged(value);
                }

                _punkbusterScrolling = value;
            }
        }

        public List<string> Settings
        {
            set
            {
                bool isEnabled = true;

                if (value.Count >= 1 && bool.TryParse(value[0], out isEnabled) == true)
                {
                    DisplayConnection = isEnabled;
                }

                if (value.Count >= 2 && bool.TryParse(value[1], out isEnabled) == true)
                {
                    LogEventsConnection = isEnabled;
                }

                if (value.Count >= 3 && bool.TryParse(value[2], out isEnabled) == true)
                {
                    LogDebugDetails = isEnabled;
                }

                if (value.Count >= 4 && bool.TryParse(value[3], out isEnabled) == true)
                {
                    DisplayPunkbuster = isEnabled;
                }

                if (value.Count >= 5 && bool.TryParse(value[4], out isEnabled) == true)
                {
                    ConScrolling = isEnabled;
                }

                if (value.Count >= 6 && bool.TryParse(value[5], out isEnabled) == true)
                {
                    PBScrolling = isEnabled;
                }
            }
            get
            {
                return new List<string>() {
                    DisplayConnection.ToString(),
                    LogEventsConnection.ToString(),
                    LogDebugDetails.ToString(),
                    DisplayPunkbuster.ToString(),
                    ConScrolling.ToString(),
                    PBScrolling.ToString()
                };
            }
        }

        public event WriteConsoleHandler WriteConsole;
        public event IsEnabledHandler LogEventsConnectionChanged;
        public event IsEnabledHandler LogDebugDetailsChanged;
        public event IsEnabledHandler DisplayConnectionChanged;
        public event IsEnabledHandler DisplayPunkbusterChanged;
        public event IsEnabledHandler ConScrollingChanged;
        public event IsEnabledHandler PBScrollingChanged;

        private void Connection_PacketCacheIntercept(FrostbiteConnection sender, Packet request, Packet response)
        {
            if (LogDebugDetails == true)
            {
                if (request.OriginatedFromServer == false)
                {
                    Write(GetDebugPacket("^7Cache", "^4", response, request));
                }
                else
                {
                    if (LogEventsConnection == true)
                    {
                        Write(GetDebugPacket("^7Cache", "^4", response, request));
                    }
                }
            }
        }

        private void m_prcClient_PacketRecieved(FrostbiteConnection sender, bool isHandled, Packet packetBeforeDispatch)
        {
            Packet cpRequestPacket = Client.Game.Connection.GetRequestPacket(packetBeforeDispatch);

            if (packetBeforeDispatch.OriginatedFromServer == false && packetBeforeDispatch.IsResponse == true)
            {
                if (LogDebugDetails == true && cpRequestPacket != null)
                {
                    if (cpRequestPacket.OriginatedFromServer == false)
                    {
                        Write(GetDebugPacket("^6Client", "^4", packetBeforeDispatch, cpRequestPacket));
                    }
                    else
                    {
                        if (LogEventsConnection == true)
                        {
                            Write(GetDebugPacket("^8Server", "^4", packetBeforeDispatch, cpRequestPacket));
                        }
                    }
                }
                else
                {
                    if ((cpRequestPacket != null && cpRequestPacket.OriginatedFromServer == false) || LogEventsConnection == true)
                    {
                        Write("^b^4{0}", packetBeforeDispatch.ToString().TrimEnd('\r', '\n').Replace("{", "{{").Replace("}", "}}"));
                    }
                }
            }
            // ELSE IF it's an event initiated by the server (OnJoin, OnLeave, OnChat etc)
            else if (packetBeforeDispatch.OriginatedFromServer == true && packetBeforeDispatch.IsResponse == false)
            {
                if (LogDebugDetails == true)
                {
                    if (cpRequestPacket != null && cpRequestPacket.OriginatedFromServer == false)
                    {
                        Write(GetDebugPacket("^6Client", "^4", packetBeforeDispatch, null));
                    }
                    else
                    {
                        if (LogEventsConnection == true)
                        {
                            Write(GetDebugPacket("^8Server", "^4", packetBeforeDispatch, null));
                        }
                    }
                }
                else
                {
                    if ((cpRequestPacket != null && cpRequestPacket.OriginatedFromServer == false) || LogEventsConnection == true)
                    {
                        Write("^b^4{0}", packetBeforeDispatch.ToString().TrimEnd('\r', '\n').Replace("{", "{{").Replace("}", "}}"));
                    }
                }
            }

            BytesRecieved += packetBeforeDispatch.PacketSize;
        }

        private void m_prcClient_CommandLogout(PRoConClient sender)
        {
            Write(Client.Language.GetLocalized("uscServerConnection.OnLogoutSuccess"));
        }

        private void m_prcClient_CommandLoginFailure(PRoConClient sender, string strError)
        {
            Write("^1" + Client.Language.GetLocalized("uscServerConnection.OnLoginAuthenticationFailure"));
        }

        private void m_prcClient_CommandLogin(PRoConClient sender)
        {
            Write("^b^3" + Client.Language.GetLocalized("uscServerConnection.OnLoginSuccess"));
        }

        private void m_prcClient_CommandLoginAttempt(PRoConClient sender)
        {
            Write(Client.Language.GetLocalized("uscServerConnection.OnLoginAttempt"));
        }

        private void m_prcClient_ConnectionFailure(PRoConClient sender, Exception exception)
        {
            Write("^b^1" + Client.Language.GetLocalized("uscServerConnection.OnServerConnectionFailure", exception.Message));
        }

        private void m_prcClient_ConnectionClosed(PRoConClient sender)
        {
            Write(Client.Language.GetLocalized("uscServerConnection.OnServerConnectionClosed", Client.HostNamePort));
        }

        protected void m_prcClient_CommandConnectAttempt(PRoConClient sender)
        {
            Write(Client.Language.GetLocalized("uscServerConnection.OnServerCommandConnectionAttempt", Client.HostNamePort));
        }

        protected void m_prcClient_CommandConnectSuccess(PRoConClient sender)
        {
            Write("^b^3" + Client.Language.GetLocalized("uscServerConnection.OnServerCommandConnectionSuccess", Client.HostNamePort));
        }

        protected void m_prcClient_PacketSent(FrostbiteConnection sender, bool isHandled, Packet packetBeforeDispatch)
        {
            if (LogDebugDetails == true)
            {
                if (packetBeforeDispatch.OriginatedFromServer == false)
                {
                    Write(GetDebugPacket("^6Client", "^2", packetBeforeDispatch, null));
                }
                else
                {
                    if (LogEventsConnection == true)
                    {
                        Write(GetDebugPacket("^8Server", "^2", packetBeforeDispatch, null));
                    }
                }
            }
            else
            {
                if (packetBeforeDispatch.OriginatedFromServer == false || LogEventsConnection == true)
                {
                    Write("^b^2{0}", packetBeforeDispatch.ToString().TrimEnd('\r', '\n'));
                }
            }

            BytesSent += packetBeforeDispatch.PacketSize;
        }

        protected void m_prcClient_PacketDequeued(FrostbiteConnection sender, Packet cpPacket, int iThreadId)
        {
            if (LogDebugDetails == true)
            {
                Write(GetDebugPacket("^7Dequeued", "^2", cpPacket, null));
            }
        }

        protected void m_prcClient_PacketQueued(FrostbiteConnection sender, Packet cpPacket, int iThreadId)
        {
            if (LogDebugDetails == true)
            {
                Write(GetDebugPacket("^7Queued", "^2", cpPacket, null));
            }
        }

        protected static string GetDebugPacket(string connectionPrefix, string packetColour, Packet packet, Packet requestPacket)
        {
            string debugString = String.Empty;

            try
            {
                debugString = string.Format("{0,10}: {1,-12} S: {2,-6} {3}{4}", connectionPrefix, GetRequestResponseColour(packet), packet.SequenceNumber, packetColour, packet.ToDebugString().Replace("\r", "").Replace("\n", ""));

                if (requestPacket != null)
                {
                    debugString = String.Format("{0} ^0(RE: ^2{1}^0)", debugString, requestPacket.ToDebugString().TrimEnd('\r', '\n'));
                }

                debugString = debugString.Replace("{", "{{").Replace("}", "}}");
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError(String.Join(", ", new[] { connectionPrefix, packetColour, packet.ToString(), requestPacket != null ? requestPacket.ToString() : "" }), "", e);
                debugString = "";
            }

            return debugString;
        }

        protected static string GetRequestResponseColour(Packet packet)
        {
            return packet.IsResponse == true ? "^2response^0" : "^1request^0";
        }

        public void Write(string strFormat, params object[] arguments)
        {
            DateTime dtLoggedTime = DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime();
            string text = "";

            try
            {
                text = String.Format(strFormat, arguments);
            }
            catch (Exception e)
            {
                FrostbiteConnection.LogError(String.Join(", ", new[] { strFormat, }), "", e);
                text = "";
            }

            WriteLogLine(String.Format("[{0}] {1}", dtLoggedTime.ToString("HH:mm:ss"), text.Replace("{", "{{").Replace("}", "}}")));

            if (WriteConsole != null)
            {
                this.WriteConsole(dtLoggedTime, text);
            }
        }
    }
}