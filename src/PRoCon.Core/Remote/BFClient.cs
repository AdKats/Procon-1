// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.f
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
    public class BFClient : FrostbiteClient
    {

        public override string GameType
        {
            get
            {
                return "BF";
            }
        }

        public BFClient(FrostbiteConnection connection)
            : base(connection)
        {

            this.ResponseDelegates.Add("vars.rankLimit", this.DispatchVarsRankLimitResponse);
            this.ResponseDelegates.Add("vars.teamBalance", this.DispatchVarsTeamBalanceResponse);
            this.ResponseDelegates.Add("vars.killCam", this.DispatchVarsKillCamResponse);
            this.ResponseDelegates.Add("vars.miniMap", this.DispatchVarsMiniMapResponse);
            this.ResponseDelegates.Add("vars.crossHair", this.DispatchVarsCrossHairResponse);
            this.ResponseDelegates.Add("vars.3dSpotting", this.DispatchVars3dSpottingResponse);
            this.ResponseDelegates.Add("vars.miniMapSpotting", this.DispatchVarsMiniMapSpottingResponse);
            this.ResponseDelegates.Add("vars.thirdPersonVehicleCameras", this.DispatchVarsThirdPersonVehicleCamerasResponse);

            this.ResponseDelegates.Add("admin.password", this.DispatchVarsAdminPasswordResponse);

            this.GetPacketsPattern = new Regex(this.GetPacketsPattern.ToString() + "|^admin.getPlaylist|^reservedSlots.list", RegexOptions.Compiled);
        }

        public override void FetchStartupVariables()
        {
            base.FetchStartupVariables();

            this.SendGetVarsTeamBalancePacket();
            this.SendGetVarsKillCamPacket();
            this.SendGetVarsMiniMapPacket();
            this.SendGetVars3dSpottingPacket();
            this.SendGetVarsMiniMapSpottingPacket();
            this.SendGetVarsThirdPersonVehicleCamerasPacket();
        }

        public virtual void FetchStartupVariablesBase()
        {
            base.FetchStartupVariables();
        }

        #region Overridden Events

        public override event FrostbiteClient.LimitHandler RankLimit;
        public override event FrostbiteClient.IsEnabledHandler KillCam;
        public override event FrostbiteClient.IsEnabledHandler MiniMap;
        public override event FrostbiteClient.IsEnabledHandler CrossHair;
        public override event FrostbiteClient.IsEnabledHandler ThreeDSpotting;
        public override event FrostbiteClient.IsEnabledHandler MiniMapSpotting;
        public override event FrostbiteClient.IsEnabledHandler ThirdPersonVehicleCameras;
        public override event FrostbiteClient.IsEnabledHandler TeamBalance;

        public override event FrostbiteClient.PlaylistSetHandler PlaylistSet;

        public override event FrostbiteClient.PlayerSpawnedHandler PlayerSpawned;

        #endregion

        #region Overridden Response Handlers

        protected override void DispatchPlayerOnSpawnRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 9)
            {
                if (this.PlayerSpawned != null)
                {
                    this.PlayerSpawned(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words.GetRange(3, 3), cpRequestPacket.Words.GetRange(6, 3)); // new Inventory(cpRequestPacket.Words[3], cpRequestPacket.Words[4], cpRequestPacket.Words[5], cpRequestPacket.Words[6], cpRequestPacket.Words[7], cpRequestPacket.Words[8]));
                }
            }
        }

        protected virtual void DispatchAdminSetPlaylistResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (this.PlaylistSet != null)
                {
                    this.PlaylistSet(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchAdminGetPlaylistResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1 && cpRecievedPacket.Words.Count >= 2)
            {
                if (this.PlaylistSet != null)
                {
                    this.PlaylistSet(this, cpRecievedPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchVarsRankLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.RankLimit != null)
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

        protected virtual void DispatchVarsTeamBalanceResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.TeamBalance != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamBalance(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamBalance(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }


        protected virtual void DispatchVarsKillCamResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.KillCam != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.KillCam(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.KillCam(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsMiniMapResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.MiniMap != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.MiniMap(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.MiniMap(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsCrossHairResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.CrossHair != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.CrossHair(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.CrossHair(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVars3dSpottingResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.ThreeDSpotting != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ThreeDSpotting(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ThreeDSpotting(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsMiniMapSpottingResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.MiniMapSpotting != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.MiniMapSpotting(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.MiniMapSpotting(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsThirdPersonVehicleCamerasResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.ThirdPersonVehicleCameras != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ThirdPersonVehicleCameras(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ThirdPersonVehicleCameras(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        //public override void DispatchResponsePacket(FrostbiteConnection connection, Packet cpRecievedPacket, Packet cpRequestPacket) {
        //    base.DispatchResponsePacket(connection, cpRecievedPacket, cpRequestPacket);

        //}

        #endregion

    }
}
