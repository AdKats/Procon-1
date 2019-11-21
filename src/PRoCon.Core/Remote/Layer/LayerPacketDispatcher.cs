using System;
using System.Collections.Generic;

namespace PRoCon.Core.Remote.Layer
{
    public abstract class LayerPacketDispatcher : ILayerPacketDispatcher
    {
        /// <summary>
        /// The connection to dispatch incoming packets from
        /// </summary>
        public ILayerConnection Connection { get; set; }

        /// <summary>
        /// A list of handlers to handle requests from the connection
        /// </summary>
        protected Dictionary<String, Action<ILayerConnection, Packet>> RequestDelegates = new Dictionary<String, Action<ILayerConnection, Packet>>();

        public Action<ILayerPacketDispatcher> ConnectionClosed { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketUnsecureSafeListedRecieved { get; set; } // Harmless packets that do not require login.
        public Action<ILayerPacketDispatcher, Packet> RequestPacketSecureSafeListedRecieved { get; set; } // Harmless recieved packets for logged in users

        public Action<ILayerPacketDispatcher, Packet> RequestPacketPunkbusterRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketAlterTextMonderationListRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketAlterBanListRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketAlterReservedSlotsListRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketAlterMaplistRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketUseMapFunctionRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketVarsRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketAdminShutdown { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketAdminPlayerKillRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketAdminKickPlayerRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketAdminPlayerMoveRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketUnknownRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet, CBanInfo> RequestBanListAddRecieved { get; set; }

        public Action<ILayerPacketDispatcher, Packet, String> RequestLoginPlainText { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestQuit { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestLogout { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestHelp { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestLoginHashed { get; set; }

        public Action<ILayerPacketDispatcher, Packet, bool> RequestEventsEnabled { get; set; }

        public Action<ILayerPacketDispatcher, Packet, String> RequestLoginHashedPassword { get; set; }

        public Action<ILayerPacketDispatcher, Packet> RequestPacketSquadLeaderRecieved { get; set; }
        public Action<ILayerPacketDispatcher, Packet> RequestPacketSquadIsPrivateReceived { get; set; }

        public String IPPort
        {
            get
            {
                String ipPort = String.Empty;

                if (this.Connection != null)
                {
                    ipPort = this.Connection.IPPort;
                }

                return ipPort;
            }
        }

        protected LayerPacketDispatcher(ILayerConnection connection)
        {
            this.Connection = connection;

            this.RequestDelegates = new Dictionary<String, Action<ILayerConnection, Packet>>() {
                { "login.plainText", this.DispatchLoginPlainTextRequest },
                { "login.hashed", this.DispatchLoginHashedRequest },
                { "logout", this.DispatchLogoutRequest },
                { "quit", this.DispatchQuitRequest },
                { "version", this.DispatchUnsecureSafeListedRequest },
                { "eventsEnabled", this.DispatchEventsEnabledRequest },
                { "help", this.DispatchHelpRequest },

                { "admin.runScript", this.DispatchSecureSafeListedRequest },
                { "punkBuster.pb_sv_command", this.DispatchPunkbusterRequest },
                { "serverInfo", this.DispatchUnsecureSafeListedRequest },
                { "admin.say", this.DispatchSecureSafeListedRequest },
                { "admin.yell", this.DispatchSecureSafeListedRequest },

                { "admin.runNextLevel", this.DispatchUseMapFunctionRequest },
                { "admin.currentLevel", this.DispatchSecureSafeListedRequest },
                { "admin.restartMap", this.DispatchUseMapFunctionRequest },
                { "admin.supportedMaps", this.DispatchSecureSafeListedRequest },
                { "admin.setPlaylist", this.DispatchAlterMaplistRequest },
                { "admin.getPlaylist", this.DispatchSecureSafeListedRequest },
                { "admin.getPlaylists", this.DispatchSecureSafeListedRequest },
                { "admin.listPlayers", this.DispatchSecureSafeListedRequest },
                { "listPlayers", this.DispatchSecureSafeListedRequest },
                { "admin.endRound", this.DispatchUseMapFunctionRequest },

                { "admin.runNextRound", this.DispatchUseMapFunctionRequest },
                { "admin.restartRound", this.DispatchUseMapFunctionRequest },

                { "banList.add", this.DispatchBanListAddRequest },
                { "banList.remove", this.DispatchAlterBanListRequest },
                { "banList.clear", this.DispatchAlterBanListRequest },
                { "banList.save", this.DispatchAlterBanListRequest },
                { "banList.load", this.DispatchAlterBanListRequest },
                { "banList.list", this.DispatchSecureSafeListedRequest },

                { "textChatModerationList.addPlayer", this.DispatchAlterTextMonderationListRequest },
                { "textChatModerationList.removePlayer", this.DispatchAlterTextMonderationListRequest },
                { "textChatModerationList.clear", this.DispatchAlterTextMonderationListRequest },
                { "textChatModerationList.save", this.DispatchAlterTextMonderationListRequest },
                { "textChatModerationList.load", this.DispatchAlterTextMonderationListRequest },
                { "textChatModerationList.list", this.DispatchSecureSafeListedRequest },

                #region Maplist

                { "mapList.configFile", this.DispatchAlterMaplistRequest },
                { "mapList.load", this.DispatchAlterMaplistRequest },
                { "mapList.save", this.DispatchAlterMaplistRequest },
                { "mapList.list", this.DispatchSecureSafeListedRequest },
                { "mapList.clear", this.DispatchAlterMaplistRequest },
                { "mapList.append", this.DispatchAlterMaplistRequest },
                { "mapList.nextLevelIndex", this.DispatchUseMapFunctionRequest },
                { "mapList.remove", this.DispatchAlterMaplistRequest },
                { "mapList.insert", this.DispatchAlterMaplistRequest },

                #endregion

                #region Configuration

                { "vars.adminPassword", this.DispatchVarsAdminPasswordRequest },
                { "vars.gamePassword", this.DispatchVarsRequest },
                { "vars.punkBuster", this.DispatchVarsRequest },
                { "vars.ranked", this.DispatchVarsRequest },
                { "vars.rankLimit", this.DispatchVarsRequest },
                { "vars.profanityFilter", this.DispatchVarsRequest },
                { "vars.idleTimeout", this.DispatchVarsRequest },
                { "vars.playerLimit", this.DispatchVarsRequest },
                { "vars.currentPlayerLimit", this.DispatchVarsRequest },
                { "vars.maxPlayerLimit", this.DispatchVarsRequest },
                { "vars.teamFactionOverride", this.DispatchVarsRequest },

                #endregion

                #region Details

                { "vars.serverName", this.DispatchVarsRequest },
                { "vars.bannerUrl", this.DispatchVarsRequest },
                { "vars.serverDescription", this.DispatchVarsRequest },

                #endregion

                #region Gameplay

                { "vars.hardCore", this.DispatchVarsRequest },
                { "vars.friendlyFire", this.DispatchVarsRequest },

                #endregion

                #region Team Killing

                { "vars.teamKillCountForKick", this.DispatchVarsRequest },
                { "vars.teamKillKickForBan", this.DispatchVarsRequest },
                { "vars.teamKillValueForKick", this.DispatchVarsRequest },
                { "vars.teamKillValueIncrease", this.DispatchVarsRequest },
                { "vars.teamKillValueDecreasePerSecond", this.DispatchVarsRequest },

                #endregion

                #region Text Chat Moderation

                { "vars.textChatModerationMode", this.DispatchVarsRequest },
                { "vars.textChatSpamTriggerCount", this.DispatchVarsRequest },
                { "vars.textChatSpamDetectionTime", this.DispatchVarsRequest },
                { "vars.textChatSpamCoolDownTime", this.DispatchVarsRequest },

                #endregion

                #region Level Variables

                { "levelVars.set", this.DispatchVarsRequest },
                { "levelVars.get", this.DispatchVarsRequest },
                { "levelVars.evaluate", this.DispatchVarsRequest },
                { "levelVars.clear", this.DispatchVarsRequest },
                { "levelVars.list", this.DispatchSecureSafeListedRequest },

                #endregion

                { "admin.kickPlayer", this.DispatchAdminKickPlayerRequest },
                { "admin.killPlayer", this.DispatchAdminKillPlayerRequest },
                { "admin.movePlayer", this.DispatchAdminMovePlayerRequest },

                { "admin.shutDown", this.DispatchAdminShutDownRequest },
            };

            this.Connection.PacketReceived = Connection_PacketReceived;
            this.Connection.ConnectionClosed = Connection_ConnectionClosed;
        }


        protected virtual void DispatchPunkbusterRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketPunkbusterRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchUseMapFunctionRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketUseMapFunctionRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchBanListAddRequest(ILayerConnection sender, Packet request)
        {
            if (request.Words.Count >= 4)
            {
                CBanInfo newBan = new CBanInfo(request.Words[1], request.Words[2], new TimeoutSubset(request.Words.GetRange(3, TimeoutSubset.RequiredLength(request.Words[3]))), request.Words.Count >= (4 + TimeoutSubset.RequiredLength(request.Words[3])) ? request.Words[(3 + TimeoutSubset.RequiredLength(request.Words[3]))] : "");

                var handler = this.RequestBanListAddRecieved;
                if (handler != null)
                {
                    handler(this, request, newBan);
                }
            }
        }

        protected virtual void DispatchAlterTextMonderationListRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAlterTextMonderationListRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAlterBanListRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAlterBanListRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAlterReservedSlotsListRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAlterReservedSlotsListRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAlterMaplistRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAlterMaplistRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchVarsAdminPasswordRequest(ILayerConnection sender, Packet request)
        {
            this.SendResponse(request, "UnknownCommand");
        }

        protected virtual void DispatchUnsecureSafeListedRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketUnsecureSafeListedRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchVarsRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketVarsRecieved;

            if (request.Words.Count == 1)
            {
                handler = this.RequestPacketSecureSafeListedRecieved;
            }

            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchSecureSafeListedRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketSecureSafeListedRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAdminKickPlayerRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAdminKickPlayerRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAdminMovePlayerRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAdminPlayerMoveRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAdminKillPlayerRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAdminPlayerKillRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchSquadLeaderRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketSquadLeaderRecieved;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchSquadIsPrivateRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketSquadIsPrivateReceived;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchLoginPlainTextRequest(ILayerConnection sender, Packet request)
        {
            if (request.Words.Count >= 2)
            {
                var handler = this.RequestLoginPlainText;
                if (handler != null)
                {
                    handler(this, request, request.Words[1]);
                }
            }
            else
            {
                this.SendResponse(request, "InvalidArguments");
            }
        }

        protected virtual void DispatchLoginHashedRequest(ILayerConnection sender, Packet request)
        {
            if (request.Words.Count == 1)
            {
                var handler = this.RequestLoginHashed;
                if (handler != null)
                {
                    handler(this, request);
                }
            }
            else if (request.Words.Count >= 2)
            {
                var handler = this.RequestLoginHashedPassword;
                if (handler != null)
                {
                    handler(this, request, request.Words[1]);
                }
            }
            else
            {
                this.SendResponse(request, "InvalidArguments");
            }
        }

        protected virtual void DispatchLogoutRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestLogout;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchQuitRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestQuit;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchEventsEnabledRequest(ILayerConnection sender, Packet request)
        {
            if (this.RequestEventsEnabled != null)
            {
                bool blEnabled = true;

                if (request.Words.Count == 2 && bool.TryParse(request.Words[1], out blEnabled) == true)
                {
                    var handler = this.RequestEventsEnabled;
                    if (handler != null)
                    {
                        handler(this, request, blEnabled);
                    }
                }
                else
                {
                    this.SendResponse(request, "InvalidArguments");
                }
            }
        }

        protected virtual void DispatchHelpRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestHelp;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        protected virtual void DispatchAdminShutDownRequest(ILayerConnection sender, Packet request)
        {
            var handler = this.RequestPacketAdminShutdown;
            if (handler != null)
            {
                handler(this, request);
            }
        }

        public virtual void DispatchRequestPacket(ILayerConnection sender, Packet request)
        {
            if (request.Words.Count >= 1)
            {
                if (this.RequestDelegates.ContainsKey(request.Words[0]) == true)
                {
                    this.RequestDelegates[request.Words[0]](sender, request);
                }
                else
                {
                    var handler = this.RequestPacketUnknownRecieved;
                    if (handler != null)
                    {
                        handler(this, request);
                    }
                }
            }
        }

        private void Connection_PacketReceived(ILayerConnection sender, Packet packet)
        {
            if (packet.OriginatedFromServer == false && packet.IsResponse == false)
            {
                this.DispatchRequestPacket(sender, packet);
            }
            //else if (packet.OriginatedFromServer == true && packet.IsResponse == true) {
            //  Response to an event we sent.  We just accept these without processing them.  Should always be "OK".  
            //}
        }

        private void Connection_ConnectionClosed(ILayerConnection sender)
        {
            var handler = this.ConnectionClosed;
            if (handler != null)
            {
                handler(this);
            }

            this.NullActions();
        }

        public void Poke()
        {
            if (this.Connection != null)
            {
                this.Connection.Poke();
            }
        }

        public void SendPacket(Packet packet)
        {
            if (this.Connection != null)
            {
                this.Connection.Send(packet);
            }
        }

        public void SendRequest(List<String> words)
        {
            if (this.Connection != null)
            {
                this.Connection.Send(new Packet(true, false, this.Connection.AcquireSequenceNumber, words));
            }
        }

        public void SendRequest(params String[] words)
        {
            this.SendRequest(new List<String>(words));
        }

        public void SendResponse(Packet packet, List<String> words)
        {
            if (this.Connection != null)
            {
                this.Connection.Send(new Packet(false, true, packet.SequenceNumber, words));
            }
        }

        public void SendResponse(Packet packet, params String[] words)
        {
            this.SendResponse(packet, new List<String>(words));
        }

        /// <summary>
        /// Nulls out all of the actions, 
        /// </summary>
        protected void NullActions()
        {
            this.ConnectionClosed = null;

            this.RequestPacketUnsecureSafeListedRecieved = null;
            this.RequestPacketSecureSafeListedRecieved = null;

            this.RequestPacketPunkbusterRecieved = null;

            this.RequestPacketAlterTextMonderationListRecieved = null;
            this.RequestPacketAlterBanListRecieved = null;
            this.RequestPacketAlterReservedSlotsListRecieved = null;
            this.RequestPacketAlterMaplistRecieved = null;

            this.RequestPacketUseMapFunctionRecieved = null;

            this.RequestPacketVarsRecieved = null;

            this.RequestPacketAdminShutdown = null;

            this.RequestPacketAdminPlayerKillRecieved = null;
            this.RequestPacketAdminKickPlayerRecieved = null;
            this.RequestPacketAdminPlayerMoveRecieved = null;

            this.RequestPacketUnknownRecieved = null;

            this.RequestBanListAddRecieved = null;

            this.RequestLoginPlainText = null;

            this.RequestQuit = null;
            this.RequestLogout = null;
            this.RequestHelp = null;
            this.RequestLoginHashed = null;

            this.RequestEventsEnabled = null;

            this.RequestLoginHashedPassword = null;

            this.RequestPacketSquadLeaderRecieved = null;
            this.RequestPacketSquadIsPrivateReceived = null;
        }

        public void Shutdown()
        {
            if (this.Connection != null)
            {
                this.Connection.Shutdown();
                this.Connection = null;
            }

            this.NullActions();
        }
    }
}
