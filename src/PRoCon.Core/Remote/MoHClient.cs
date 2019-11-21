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
using System.Globalization;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote
{
    public class MoHClient : FrostbiteClient
    {
        public MoHClient(FrostbiteConnection connection) : base(connection)
        {
            ResponseDelegates.Add("vars.clanTeams", DispatchVarsClanTeamsResponse);
            ResponseDelegates.Add("vars.noCrosshairs", DispatchVarsNoCrosshairsResponse);
            ResponseDelegates.Add("vars.realisticHealth", DispatchVarsRealisticHealthResponse);
            ResponseDelegates.Add("vars.noUnlocks", DispatchVarsNoUnlocksResponse);
            ResponseDelegates.Add("vars.skillLimit", DispatchVarsSkillLimitResponse);
            ResponseDelegates.Add("vars.noAmmoPickups", DispatchVarsNoAmmoPickupsResponse);
            ResponseDelegates.Add("vars.tdmScoreCounterMaxScore", DispatchVarsTdmScoreCounterMaxScoreResponse);

            // Preround vars
            ResponseDelegates.Add("vars.preRoundLimit", DispatchVarsPreRoundLimitResponse);
            ResponseDelegates.Add("admin.roundStartTimerEnabled", DispatchAdminRoundStartTimerEnabledResponse);
            ResponseDelegates.Add("vars.roundStartTimerDelay", DispatchVarsRoundStartTimerDelayResponse);
            ResponseDelegates.Add("vars.roundStartTimerPlayersLimit", DispatchVarsRoundStartTimerPlayersLimitResponse);

            // New map functions?
            ResponseDelegates.Add("admin.stopPreRound", DispatchAdminStopPreRoundResponse);

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("reservedSpectateSlots.configFile", DispatchReservedSlotsConfigFileResponse);
            ResponseDelegates.Add("reservedSpectateSlots.load", DispatchReservedSlotsLoadResponse);
            ResponseDelegates.Add("reservedSpectateSlots.save", DispatchReservedSlotsSaveResponse);
            ResponseDelegates.Add("reservedSpectateSlots.addPlayer", DispatchReservedSlotsAddPlayerResponse);
            ResponseDelegates.Add("reservedSpectateSlots.removePlayer", DispatchReservedSlotsRemovePlayerResponse);
            ResponseDelegates.Add("reservedSpectateSlots.clear", DispatchReservedSlotsClearResponse);
            ResponseDelegates.Add("reservedSpectateSlots.list", DispatchReservedSlotsListResponse);

            GetPacketsPattern = new Regex(GetPacketsPattern + "|^reservedSpectateSlots.list|^admin.roundStartTimerEnabled$|^admin.tdmScoreCounterMaxScore$", RegexOptions.Compiled);
        }

        public override string GameType
        {
            get { return "MOH"; }
        }

        public override bool HasSquads
        {
            get { return false; }
        }

        public override bool HasOpenMaplist
        {
            get
            {
                // true, does not lock maplist to a playlist
                return true;
            }
        }

        public override void FetchStartupVariables()
        {
            base.FetchStartupVariables();

            SendGetVarsBannerUrlPacket();

            SendTextChatModerationListListPacket();

            SendGetVarsHardCorePacket();
            SendGetVarsProfanityFilterPacket();

            SendGetVarsRankedPacket();
            SendGetVarsPunkBusterPacket();

            SendGetVarsMaxPlayerLimitPacket();

            SendGetVarsCurrentPlayerLimitPacket();
            SendGetVarsPlayerLimitPacket();

            SendGetVarsClanTeamsPacket();
            SendGetVarsNoCrosshairsPacket();
            SendGetVarsRealisticHealthPacket();
            SendGetVarsNoUnlocksPacket();
            SendGetVarsTdmScoreCounterMaxScorePacket();
            SendGetVarsNoAmmoPickupsPacket();
            SendGetVarsSkillLimitPacket();

            SendGetAdminRoundStartTimerEnabledPacket();
            SendGetVarsRoundStartTimerPlayersLimitPacket();
            SendGetVarsRoundStartTimerDelayPacket();
            SendGetVarsPreRoundLimitPacket();


            // Text Chat Moderation
            SendGetVarsTextChatModerationModePacket();
            SendGetVarsTextChatSpamCoolDownTimePacket();
            SendGetVarsTextChatSpamDetectionTimePacket();
            SendGetVarsTextChatSpamTriggerCountPacket();
            // vars.skillLimit
            // vars.preRoundLimit
        }

        #region Overridden Events

        public override event PlayerTeamChangeHandler PlayerChangedTeam;
        public override event PlayerMovedByAdminHandler PlayerMovedByAdmin;

        // Vars
        public override event IsEnabledHandler ClanTeams;
        public override event UpperLowerLimitHandler SkillLimit;
        public override event IsEnabledHandler NoAmmoPickups;
        public override event IsEnabledHandler NoCrosshairs;
        public override event IsEnabledHandler NoSpotting;
        public override event IsEnabledHandler NoUnlocks;
        public override event IsEnabledHandler RealisticHealth;
        public override event LimitHandler TdmScoreCounterMaxScore;

        // Preround
        public override event EmptyParamterHandler StopPreRound;
        public override event UpperLowerLimitHandler PreRoundLimit;
        public override event IsEnabledHandler RoundStartTimer;
        public override event LimitHandler RoundStartTimerDelay;
        public override event LimitHandler RoundStartTimerPlayerLimit;

        public override event PlayerSpawnedHandler PlayerSpawned;

        #endregion

        #region Packet Helpers

        public override void SendAdminMovePlayerPacket(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKill)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.movePlayer", soldierName, destinationTeamId.ToString(CultureInfo.InvariantCulture), Packet.Bltos(forceKill));
            }
        }

        #region Reserved Slot List

        public override void SendReservedSlotsListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSpectateSlots.list");
            }
        }

        public override void SendReservedSlotsAddPlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSpectateSlots.addPlayer", soldierName);
            }
        }

        public override void SendReservedSlotsRemovePlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSpectateSlots.removePlayer", soldierName);
            }
        }

        public override void SendReservedSlotsSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSpectateSlots.save");
            }
        }

        #endregion

        #endregion

        #region Implemented/Overridden Response Handlers

        protected virtual void DispatchAdminStopPreRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (StopPreRound != null)
                {
                    this.StopPreRound(this);
                }
            }
        }

        #region Vars

        protected virtual void DispatchVarsClanTeamsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ClanTeams != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ClanTeams(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ClanTeams(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsNoCrosshairsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (NoCrosshairs != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.NoCrosshairs(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.NoCrosshairs(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsNoSpottingResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (NoSpotting != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.NoSpotting(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.NoSpotting(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsNoUnlocksResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (NoUnlocks != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.NoUnlocks(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.NoUnlocks(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRealisticHealthResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RealisticHealth != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RealisticHealth(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RealisticHealth(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsSkillLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SkillLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 3)
                    {
                        this.SkillLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]), Convert.ToInt32(cpRecievedPacket.Words[2]));
                    }
                    else if (cpRequestPacket.Words.Count >= 3)
                    {
                        this.SkillLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRequestPacket.Words[2]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsNoAmmoPickupsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (NoAmmoPickups != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.NoAmmoPickups(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.NoAmmoPickups(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }


        protected virtual void DispatchVarsTdmScoreCounterMaxScoreResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TdmScoreCounterMaxScore != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TdmScoreCounterMaxScore(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TdmScoreCounterMaxScore(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #region Preround

        protected virtual void DispatchVarsPreRoundLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PreRoundLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 3)
                    {
                        this.PreRoundLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]), Convert.ToInt32(cpRecievedPacket.Words[2]));
                    }
                    else if (cpRequestPacket.Words.Count >= 3)
                    {
                        this.PreRoundLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRequestPacket.Words[2]));
                    }
                }
            }
        }

        protected virtual void DispatchAdminRoundStartTimerEnabledResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundStartTimer != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundStartTimer(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundStartTimer(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundStartTimerDelayResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundStartTimerDelay != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundStartTimerDelay(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundStartTimerDelay(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundStartTimerPlayersLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundStartTimerPlayerLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundStartTimerPlayerLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundStartTimerPlayerLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

        #endregion

        //public override void DispatchResponsePacket(FrostbiteConnection connection, Packet cpRecievedPacket, Packet cpRequestPacket) {
        //    base.DispatchResponsePacket(connection, cpRecievedPacket, cpRequestPacket);
        //}

        #endregion

        #region Overridden Request Handlers

        protected override void DispatchPlayerOnSpawnRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 7)
            {
                if (PlayerSpawned != null)
                {
                    this.PlayerSpawned(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words.GetRange(3, 1), cpRequestPacket.Words.GetRange(4, 3)); // new Inventory(cpRequestPacket.Words[3], cpRequestPacket.Words[4], cpRequestPacket.Words[5], cpRequestPacket.Words[6], cpRequestPacket.Words[7], cpRequestPacket.Words[8]));
                }
            }
        }

        protected override void DispatchPlayerOnTeamChangeRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                int iTeamID = 0;

                if (int.TryParse(cpRequestPacket.Words[2], out iTeamID) == true)
                {
                    if (PlayerChangedTeam != null)
                    {
                        this.PlayerChangedTeam(this, cpRequestPacket.Words[1], iTeamID, 0);
                    }
                }
            }
        }

        protected override void DispatchAdminMovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (PlayerMovedByAdmin != null)
            {
                int desinationTeamId;
                bool forceKilled;

                if (cpRequestPacket.Words.Count >= 4)
                {
                    if (int.TryParse(cpRequestPacket.Words[2], out desinationTeamId) == true && bool.TryParse(cpRequestPacket.Words[3], out forceKilled) == true)
                    {
                        this.PlayerMovedByAdmin(this, cpRequestPacket.Words[1], desinationTeamId, 0, forceKilled);
                    }
                }
            }
        }

        //public override void DispatchRequestPacket(FrostbiteConnection connection, Packet cpRequestPacket) {
        //    base.DispatchRequestPacket(connection, cpRequestPacket);
        //}

        #endregion
    }
}