/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */


using PRoCon.Core.Consoles.Chat;
using PRoCon.Core.Logging;
using PRoCon.Core.Players;
using PRoCon.Core.Remote;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

// This class will move to .Core once ProConClient is in .Core.

namespace PRoCon.Core.Consoles
{
    public class ChatConsole : Loggable
    {
        public delegate void IndexChangedHandler(int index);

        public delegate void IsEnabledHandler(bool isEnabled);

        protected PRoConClient Client;

        private bool _logJoinLeaving;

        private bool _logKills;

        private bool _scrolling;

        private bool _logComRoseMessages;

        private bool _logPlayerDisconnected;

        private int _displayTimeIndex;

        private int _displayTypeIndex;

        public ChatConsole(PRoConClient prcClient) : base()
        {
            Client = prcClient;

            FileHostNamePort = Client.FileHostNamePort;
            LoggingStartedPrefix = "Chat logging started";
            LoggingStoppedPrefix = "Chat logging stopped";
            FileNameSuffix = "chat";

            LogJoinLeaving = false;
            LogKills = false;
            Scrolling = true;
            DisplayTypeIndex = 0;
            DisplayTimeIndex = 0;

            MessageHistory = new Queue<ChatMessage>();

            Client.Game.Chat += new FrostbiteClient.RawChatHandler(m_prcClient_Chat);

            Client.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);
            Client.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
            Client.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
            Client.Game.PlayerDisconnected += new FrostbiteClient.PlayerDisconnectedHandler(m_prcClient_PlayerDisconnected);

            Client.ProconAdminSaying += new PRoConClient.ProconAdminSayingHandler(m_prcClient_ProconAdminSaying);
            Client.ProconAdminYelling += new PRoConClient.ProconAdminYellingHandler(m_prcClient_ProconAdminYelling);

            Client.ReadRemoteChatConsole += new PRoConClient.ReadRemoteConsoleHandler(m_prcClient_ReadRemoteChatConsole);
        }

        public Queue<ChatMessage> MessageHistory { get; private set; }

        public bool LogJoinLeaving
        {
            get { return _logJoinLeaving; }
            set
            {
                if (LogJoinLeavingChanged != null)
                {
                    this.LogJoinLeavingChanged(value);
                }

                _logJoinLeaving = value;
            }
        }

        public bool LogKills
        {
            get { return _logKills; }
            set
            {
                if (LogKillsChanged != null)
                {
                    this.LogKillsChanged(value);
                }

                _logKills = value;
            }
        }

        public bool Scrolling
        {
            get { return _scrolling; }
            set
            {
                if (ScrollingChanged != null)
                {
                    this.ScrollingChanged(value);
                }

                _scrolling = value;
            }
        }

        public bool LogComRoseMessages
        {
            get { return _logComRoseMessages; }
            set
            {
                if (LogComRoseMessagesChanged != null)
                {
                    this.LogComRoseMessagesChanged(value);
                }

                _logComRoseMessages = value;
            }
        }

        public bool LogPlayerDisconnected
        {
            get { return _logPlayerDisconnected; }
            set
            {
                if (LogPlayerDisconnectedChanged != null)
                {
                    this.LogPlayerDisconnectedChanged(value);
                }

                _logPlayerDisconnected = value;
            }
        }

        public int DisplayTypeIndex
        {
            get { return _displayTypeIndex; }
            set
            {
                _displayTypeIndex = value;

                if (DisplayTypeChanged != null)
                {
                    this.DisplayTypeChanged(_displayTypeIndex);
                }
            }
        }

        public int DisplayTimeIndex
        {
            get { return _displayTimeIndex; }
            set
            {
                _displayTimeIndex = value;

                if (DisplayTimeChanged != null)
                {
                    this.DisplayTimeChanged(_displayTimeIndex);
                }
            }
        }

        public List<string> Settings
        {
            get
            {
                return new List<string>() {
                    LogJoinLeaving.ToString(),
                    LogKills.ToString(),
                    Scrolling.ToString(),
                    DisplayTypeIndex.ToString(CultureInfo.InvariantCulture),
                    DisplayTimeIndex.ToString(CultureInfo.InvariantCulture),
                    LogComRoseMessages.ToString(),
                    LogPlayerDisconnected.ToString()
                };
            }
            set
            {
                if (value.Count > 0)
                {
                    bool isEnabled = true;
                    int iIndex = 0;

                    if (value.Count >= 1 && bool.TryParse(value[0], out isEnabled) == true)
                    {
                        LogJoinLeaving = isEnabled;
                    }

                    if (value.Count >= 2 && bool.TryParse(value[1], out isEnabled) == true)
                    {
                        LogKills = isEnabled;
                    }

                    if (value.Count >= 3 && bool.TryParse(value[2], out isEnabled) == true)
                    {
                        Scrolling = isEnabled;
                    }

                    if (value.Count >= 4 && int.TryParse(value[3], out iIndex) == true)
                    {
                        DisplayTypeIndex = iIndex;
                    }

                    if (value.Count >= 5 && int.TryParse(value[4], out iIndex) == true)
                    {
                        DisplayTimeIndex = iIndex;
                    }

                    if (value.Count >= 6 && bool.TryParse(value[5], out isEnabled) == true)
                    {
                        LogComRoseMessages = isEnabled;
                    }

                    if (value.Count >= 7 && bool.TryParse(value[6], out isEnabled) == true)
                    {
                        LogPlayerDisconnected = isEnabled;
                    }
                }
            }
        }

        public event WriteConsoleHandler WriteConsole;
        public event WriteConsoleHandler WriteConsoleViaCommand;
        public event IsEnabledHandler LogJoinLeavingChanged;
        public event IsEnabledHandler LogKillsChanged;
        public event IsEnabledHandler ScrollingChanged;
        public event IsEnabledHandler LogComRoseMessagesChanged;
        public event IsEnabledHandler LogPlayerDisconnectedChanged;
        public event IndexChangedHandler DisplayTypeChanged;
        public event IndexChangedHandler DisplayTimeChanged;

        private void EnqueueMessage(ChatMessage message)
        {
            MessageHistory.Enqueue(message);

            while (MessageHistory.Count > 100)
            {
                MessageHistory.Dequeue();
            }
        }

        public string ToJsonString(int historyLength)
        {
            var messages = new Hashtable();
            var messageList = new ArrayList();

            var chatHistory = new List<ChatMessage>(MessageHistory);
            //chatHistory.Reverse();

            for (int i = chatHistory.Count - 1; i > chatHistory.Count - 1 - historyLength && i >= 0; i--)
            {
                messageList.Add(chatHistory[i].ToHashtable());
            }

            messages.Add("messages", messageList);

            return JSON.JsonEncode(messages);
        }

        public string ToJsonString(DateTime newerThan)
        {
            var messages = new Hashtable();
            var messageList = new ArrayList();

            foreach (ChatMessage message in MessageHistory)
            {
                if (message.LoggedTime >= newerThan)
                {
                    messageList.Add(message.ToHashtable());
                }
            }

            messages.Add("messages", messageList);

            return JSON.JsonEncode(messages);
        }

        private void m_prcClient_ProconAdminYelling(PRoConClient sender, string strAdminStack, string strMessage, int iMessageDuration, CPlayerSubset spsAudience)
        {
            string strAdminName = Client.Language.GetLocalized("uscChatPanel.rtbChatBox.Admin", null);
            string formattedMessage = String.Empty;

            if (strAdminStack.Length > 0)
            {
                strAdminName = String.Join(" via ", CPluginVariable.DecodeStringArray(strAdminStack));
            }

            if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                formattedMessage = String.Format("^b^2{0}^0 > ^2{1}", strAdminName.ToUpper(), strMessage.ToUpper());
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Player)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 > ^2{2}", strAdminName.ToUpper(), spsAudience.SoldierName.ToUpper(), strMessage.ToUpper());
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Team)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 >^2 {2}", strAdminName.ToUpper(), Client.GetLocalizedTeamName(spsAudience.TeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode).ToUpper(), strMessage.ToUpper());
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Squad)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 - ^2{2}^0 >^2 {3}", strAdminName.ToUpper(), Client.GetLocalizedTeamName(spsAudience.TeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode).ToUpper(), Client.Language.GetLocalized("global.Squad" + spsAudience.SquadID.ToString(CultureInfo.InvariantCulture), null), strMessage).ToUpper();
            }

            if (formattedMessage.Length > 0)
            {
                //this.EnqueueMessage(new ChatMessage(DateTime.Now, strAdminName, strMessage, true, true, spsAudience));
                //this.Write(DateTime.Now, formattedMessage);
                EnqueueMessage(new ChatMessage(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), strAdminName, strMessage, true, true, spsAudience));
                Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), formattedMessage);
            }
        }

        private void m_prcClient_ProconAdminSaying(PRoConClient sender, string strAdminStack, string strMessage, CPlayerSubset spsAudience)
        {
            string strAdminName = Client.Language.GetLocalized("uscChatPanel.rtbChatBox.Admin", null);
            string formattedMessage = String.Empty;

            if (strAdminStack.Length > 0)
            {
                strAdminName = String.Join(" via ", CPluginVariable.DecodeStringArray(strAdminStack));
            }

            if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                formattedMessage = String.Format("^b^2{0}^0 > ^2{1}", strAdminName, strMessage);
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Player)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 > ^2{2}", strAdminName, spsAudience.SoldierName, strMessage);
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Team)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 >^2 {2}", strAdminName, Client.GetLocalizedTeamName(spsAudience.TeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode), strMessage);
            }
            else if (spsAudience.Subset == CPlayerSubset.PlayerSubsetType.Squad)
            {
                formattedMessage = String.Format("^b^2{0}^0 -^2 {1}^0 - ^2{2}^0 >^2 {3}", strAdminName, Client.GetLocalizedTeamName(spsAudience.TeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode), Client.Language.GetLocalized("global.Squad" + spsAudience.SquadID.ToString(CultureInfo.InvariantCulture), null), strMessage);
            }

            if (formattedMessage.Length > 0)
            {
                EnqueueMessage(new ChatMessage(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), strAdminName, strMessage, true, false, spsAudience));
                Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), formattedMessage);
            }
        }

        private void m_prcClient_Chat(FrostbiteClient sender, List<string> rawChat)
        {
            int iTeamID = 0;
            string formattedMessage = String.Empty;
            bool commoroseMessage = false;

            /*
            ID_CHAT_ATTACK/DEFEND
            ID_CHAT_REQUEST_MEDIC
            ID_CHAT_REQUEST_ORDER
            ID_CHAT_REQUEST_REPAIRS
            ID_CHAT_GOGOGO
            ID_CHAT_AFFIRMATIVE
            ID_CHAT_THANKS
            ID_CHAT_REQUEST_AMMO
            ID_CHAT_REQUEST_RIDE
            ID_CHAT_GET_OUT
            ID_CHAT_GET_IN
            ID_CHAT_NEGATIVE
            ID_CHAT_SORRY
            */
            if (System.String.Compare(rawChat[1], "ID_CHAT_ATTACK/DEFEND", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_ATTACK/DEFEND", "uscChatPanel.ID_CHAT_ATTACK_DEFEND");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_REQUEST_MEDIC", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_REQUEST_MEDIC", "uscChatPanel.ID_CHAT_REQUEST_MEDIC");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_REQUEST_ORDER", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_REQUEST_ORDER", "uscChatPanel.ID_CHAT_REQUEST_ORDER");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_REQUEST_REPAIRS", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_REQUEST_REPAIRS", "uscChatPanel.ID_CHAT_REQUEST_REPAIRS");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_GOGOGO", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_GOGOGO", "uscChatPanel.ID_CHAT_GOGOGO");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_AFFIRMATIVE", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_AFFIRMATIVE", "uscChatPanel.ID_CHAT_AFFIRMATIVE");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_THANKS", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_THANKS", "uscChatPanel.ID_CHAT_THANKS");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_REQUEST_AMMO", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_REQUEST_AMMO", "uscChatPanel.ID_CHAT_REQUEST_AMMO");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_REQUEST_RIDE", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_REQUEST_RIDE", "uscChatPanel.ID_CHAT_REQUEST_RIDE");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_GET_OUT", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_GET_OUT", "uscChatPanel.ID_CHAT_GET_OUT");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_GET_IN", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_GET_IN", "uscChatPanel.ID_CHAT_GET_IN");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_NEGATIVE", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_NEGATIVE", "uscChatPanel.ID_CHAT_NEGATIVE");
                commoroseMessage = true;
            }
            else if (System.String.Compare(rawChat[1], "ID_CHAT_SORRY", System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                rawChat[1] = Client.Language.GetDefaultLocalized("ID_CHAT_SORRY", "uscChatPanel.ID_CHAT_SORRY");
                commoroseMessage = true;
            }

            if (commoroseMessage == true && this.LogComRoseMessages == false)
            {
                // Commo Rose logging has been disabled
                return;
            }

            if (System.String.Compare(rawChat[0], "server", System.StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (rawChat.Count == 2)
                {
                    // < R9 Support.
                    formattedMessage = String.Format("^b^4{0}^0 > ^4{1}", rawChat[0], rawChat[1]);
                }
                else if (rawChat.Count == 3 && (System.String.Compare(rawChat[2], "all", System.StringComparison.OrdinalIgnoreCase) == 0 || System.String.Compare(rawChat[2], "unknown", System.StringComparison.OrdinalIgnoreCase) == 0))
                {
                    // "unknown" because of BF4 beta
                    formattedMessage = String.Format(commoroseMessage ? "^6{0}^0 > ^6{1}" : "^b^4{0}^0 > ^4{1}", rawChat[0], rawChat[1]);
                }
                else if (rawChat.Count >= 4 && System.String.Compare(rawChat[2], "team", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (int.TryParse(rawChat[3], out iTeamID) == true)
                    {
                        formattedMessage = String.Format(commoroseMessage ? "^6{0}^0 - ^6{1}^0 > ^6{2}^0" : "^b^4{0}^0 - ^4{1}^0 >^4 {2}", rawChat[0], Client.GetLocalizedTeamName(iTeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode), rawChat[1]);
                    }
                }
                else if (rawChat.Count >= 5 && System.String.Compare(rawChat[2], "squad", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (int.TryParse(rawChat[3], out iTeamID) == true)
                    {
                        if (System.String.CompareOrdinal(rawChat[4], "0") != 0)
                        {
                            formattedMessage = String.Format(commoroseMessage ? "^6{0}^0 - ^6{1}^0 - ^6{2}^0 > ^6{3}" : "^b^4{0}^0 - ^4{1}^0 - ^4{2}^0 >^4 {3}", rawChat[0], Client.GetLocalizedTeamName(iTeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode), Client.Language.GetLocalized("global.Squad" + rawChat[4], null), rawChat[1]);
                        }
                        else
                        {
                            // TO DO: Localize and change uscPlayerListPanel.lsvPlayers.colSquad 
                            formattedMessage = String.Format(commoroseMessage ? "^6{0}^0 - ^6{1}^0 - ^6{2}^0 > ^6{3}" : "^b^4{0}^0 - ^4{1}^0 - ^4{2}^0 >^4 {3}", rawChat[0], Client.GetLocalizedTeamName(iTeamID, Client.CurrentServerInfo.Map, Client.CurrentServerInfo.GameMode), Client.Language.GetLocalized("uscPlayerListPanel.lsvPlayers.colSquad", null), rawChat[1]);
                        }
                    }
                }

                if (rawChat.Count >= 3)
                {
                    EnqueueMessage(new ChatMessage(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), rawChat[0], rawChat[1], false, false, new CPlayerSubset(rawChat.GetRange(2, rawChat.Count - 2))));
                }

                if (formattedMessage.Length > 0)
                {
                    Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), formattedMessage);
                }
            }
        }

        private void m_prcClient_PlayerJoin(FrostbiteClient sender, string playerName)
        {
            if (LogJoinLeaving == true)
            {
                Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("^4{0}", Client.Language.GetLocalized("uscChatPanel.chkDisplayOnJoinLeaveEvents.Joined", playerName)));
            }
        }

        private void m_prcClient_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer)
        {
            if (LogJoinLeaving == true && LogPlayerDisconnected == false)
            {
                if (cpiPlayer != null && cpiPlayer.ClanTag != null)
                {
                    Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("^1{0}", Client.Language.GetLocalized("uscChatPanel.chkDisplayOnJoinLeaveEvents.Left", string.Format("{0} {1}", cpiPlayer.ClanTag, cpiPlayer.SoldierName))));
                }
                else
                {
                    Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("^1{0}", Client.Language.GetLocalized("uscChatPanel.chkDisplayOnJoinLeaveEvents.Left", playerName)));
                }
            }
        }

        private void m_prcClient_PlayerDisconnected(FrostbiteClient sender, string playerName, string reason)
        {
            if (LogPlayerDisconnected == true)
            {
                Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.IsNullOrEmpty(reason) ? String.Format("^1{0}", Client.Language.GetLocalized("uscChatPanel.chkDisplayPlayerDisconnected.Disconnected", playerName)) : String.Format("^1{0}", Client.Language.GetLocalized("uscChatPanel.chkDisplayPlayerDisconnected.DisconnectedReason", playerName, Client.Language.GetDefaultLocalized(reason, String.Format("uscChatPanel.{0}", reason)))));
            }
        }

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kKillerVictimDetails)
        {
            if (LogKills == true)
            {
                string strKillerName = kKillerVictimDetails.Killer.SoldierName, strVictimName = kKillerVictimDetails.Victim.SoldierName;

                bool isTk = kKillerVictimDetails.Killer.TeamID == kKillerVictimDetails.Victim.TeamID && kKillerVictimDetails.Killer.SoldierName != kKillerVictimDetails.Victim.SoldierName;

                if (Client.PlayerList.Contains(kKillerVictimDetails.Killer) == true && Client.PlayerList[kKillerVictimDetails.Killer.SoldierName].ClanTag != null)
                {
                    strKillerName = String.Format("{0} {1}", Client.PlayerList[kKillerVictimDetails.Killer.SoldierName].ClanTag, kKillerVictimDetails.Killer.SoldierName);
                }

                if (Client.PlayerList.Contains(kKillerVictimDetails.Victim) == true && Client.PlayerList[kKillerVictimDetails.Victim.SoldierName].ClanTag != null)
                {
                    strVictimName = String.Format("{0} {1}", Client.PlayerList[kKillerVictimDetails.Victim.SoldierName].ClanTag, kKillerVictimDetails.Victim.SoldierName);
                }

                if (kKillerVictimDetails.Headshot == false)
                {
                    if (kKillerVictimDetails.DamageType.Length > 0)
                    {
                        if (isTk == true)
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^8{3}^0]", strKillerName, Client.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.TeamKill")));
                        }
                        else
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2}", strKillerName, Client.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())), strVictimName));
                        }
                    }
                    else
                    {
                        if (isTk == true)
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^8{3}^0]", strKillerName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.Killed"), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.TeamKill")));
                        }
                        else
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2}", strKillerName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.Killed"), strVictimName));
                        }
                    }
                }
                else
                {
                    // show headshot
                    if (kKillerVictimDetails.DamageType.Length > 0)
                    {
                        if (isTk == true)
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^2{3}^0] [^8{4}^0]", strKillerName, Client.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.HeadShot"), Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.TeamKill")));
                        }
                        else
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^2{3}^0]", strKillerName, Client.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.HeadShot")));
                        }
                    }
                    else
                    {
                        if (isTk == true)
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^2{3}^0] [^8{4}^0]", strKillerName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.Killed"), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.HeadShot"), Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.TeamKill")));
                        }
                        else
                        {
                            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), String.Format("{0} [^3{1}^0] {2} [^2{3}^0]", strKillerName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.Killed"), strVictimName, Client.Language.GetLocalized("uscChatPanel.chkDisplayOnKilledEvents.HeadShot")));
                        }
                    }
                }
            }
        }

        private void m_prcClient_ReadRemoteChatConsole(PRoConClient sender, DateTime loggedTime, string text)
        {
            Write(loggedTime, text);
        }

        /// <summary>
        ///     This public method is used whenever the chat console has been written to via
        ///     the procon.protected.chat.write command (basically plugin output)
        /// </summary>
        /// <param name="strText"></param>
        public void WriteViaCommand(string strText)
        {
            Write(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), strText);

            if (WriteConsoleViaCommand != null)
            {
                this.WriteConsoleViaCommand(DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime(), strText);
            }
        }

        private void Write(DateTime dtLoggedTime, string strText)
        {
            WriteLogLine(String.Format("[{0}] {1}", dtLoggedTime.ToString("HH:mm:ss"), strText.Replace("{", "{{").Replace("}", "}}")));

            if (WriteConsole != null)
            {
                this.WriteConsole(dtLoggedTime, strText);
            }
        }
    }
}