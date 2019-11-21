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
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote
{
    public class BFBC2Client : BFClient
    {
        public BFBC2Client(FrostbiteConnection connection) : base(connection)
        {
            #region Map list functions

            ResponseDelegates.Add("admin.getPlaylist", DispatchAdminGetPlaylistResponse);
            ResponseDelegates.Add("admin.setPlaylist", DispatchAdminSetPlaylistResponse);

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("admin.runNextLevel", DispatchAdminRunNextRoundResponse);
            ResponseDelegates.Add("admin.currentLevel", DispatchAdminCurrentLevelResponse);

            #endregion

            //this.m_responseDelegates.Add("vars.rankLimit", this.DispatchVarsRankLimitResponse);

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("reservedSlots.configFile", DispatchReservedSlotsConfigFileResponse);
            ResponseDelegates.Add("reservedSlots.load", DispatchReservedSlotsLoadResponse);
            ResponseDelegates.Add("reservedSlots.save", DispatchReservedSlotsSaveResponse);
            ResponseDelegates.Add("reservedSlots.addPlayer", DispatchReservedSlotsAddPlayerResponse);
            ResponseDelegates.Add("reservedSlots.removePlayer", DispatchReservedSlotsRemovePlayerResponse);
            ResponseDelegates.Add("reservedSlots.clear", DispatchReservedSlotsClearResponse);
            ResponseDelegates.Add("reservedSlots.list", DispatchReservedSlotsListResponse);

            GetPacketsPattern = new Regex(GetPacketsPattern + "|^admin.getPlaylist|^reservedSlots.list", RegexOptions.Compiled);
        }

        public override string GameType
        {
            get { return "BFBC2"; }
        }

        public override void FetchStartupVariables()
        {
            base.FetchStartupVariables();

            SendGetVarsBannerUrlPacket();

            SendGetVarsRankLimitPacket();
            SendGetVarsCrossHairPacket();

            SendTextChatModerationListListPacket();

            SendGetVarsHardCorePacket();
            SendGetVarsProfanityFilterPacket();

            SendGetVarsRankedPacket();
            SendGetVarsPunkBusterPacket();

            SendGetVarsMaxPlayerLimitPacket();

            SendGetVarsCurrentPlayerLimitPacket();
            SendGetVarsPlayerLimitPacket();

            SendAdminGetPlaylistPacket();

            SendGetVarsRankLimitPacket();


            // Text Chat Moderation
            SendGetVarsTextChatModerationModePacket();
            SendGetVarsTextChatSpamCoolDownTimePacket();
            SendGetVarsTextChatSpamDetectionTimePacket();
            SendGetVarsTextChatSpamTriggerCountPacket();
        }

        #region Overridden Events

        public override event LimitHandler RankLimit;

        public override event PlaylistSetHandler PlaylistSet;

        public override event PlayerSpawnedHandler PlayerSpawned;

        #endregion

        #region Overridden Response Handlers

        protected override void DispatchPlayerOnSpawnRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 9)
            {
                if (PlayerSpawned != null)
                {
                    this.PlayerSpawned(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words.GetRange(3, 3), cpRequestPacket.Words.GetRange(6, 3)); // new Inventory(cpRequestPacket.Words[3], cpRequestPacket.Words[4], cpRequestPacket.Words[5], cpRequestPacket.Words[6], cpRequestPacket.Words[7], cpRequestPacket.Words[8]));
                }
            }
        }

        protected override void DispatchAdminSetPlaylistResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (PlaylistSet != null)
                {
                    this.PlaylistSet(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected override void DispatchAdminGetPlaylistResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1 && cpRecievedPacket.Words.Count >= 2)
            {
                if (PlaylistSet != null)
                {
                    this.PlaylistSet(this, cpRecievedPacket.Words[1]);
                }
            }
        }

        protected override void DispatchVarsRankLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RankLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RankLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RankLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion
    }
}