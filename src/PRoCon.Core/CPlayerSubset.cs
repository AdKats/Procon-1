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
using System.Collections;
using System.Collections.Generic;

namespace PRoCon.Core
{
    [Serializable]
    public class CPlayerSubset
    {

        private PlayerSubsetType m_enPlayerSubset;
        private string m_strSoldierName;
        private int m_iTeamID, m_iSquadID;

        public enum PlayerSubsetType
        {
            None,
            All,
            Player,
            Server,
            Squad,
            Team,
        }

        public CPlayerSubset(List<string> lstWordSubset)
        {

            this.m_enPlayerSubset = PlayerSubsetType.None;

            if (lstWordSubset.Count == 1)
            { // All, Server
                if (String.Compare(lstWordSubset[0], "all", true) == 0)
                {
                    this.m_enPlayerSubset = PlayerSubsetType.All;
                }
                else if (String.Compare(lstWordSubset[0], "server", true) == 0)
                {
                    this.m_enPlayerSubset = PlayerSubsetType.Server;
                }
            }
            else if (lstWordSubset.Count == 2)
            { // Team, Player
                if (String.Compare(lstWordSubset[0], "team", true) == 0 && int.TryParse(lstWordSubset[1], out this.m_iTeamID) == true)
                {
                    this.m_enPlayerSubset = PlayerSubsetType.Team;
                }
                else if (String.Compare(lstWordSubset[0], "player", true) == 0)
                {
                    this.m_enPlayerSubset = PlayerSubsetType.Player;
                    this.m_strSoldierName = lstWordSubset[1];
                }
            }
            else if (lstWordSubset.Count >= 3)
            { // Squad
                if (String.Compare(lstWordSubset[0], "squad", true) == 0 && int.TryParse(lstWordSubset[1], out this.m_iTeamID) == true && int.TryParse(lstWordSubset[2], out this.m_iSquadID) == true)
                {
                    this.m_enPlayerSubset = PlayerSubsetType.Squad;
                }
            }
        }

        public CPlayerSubset(PlayerSubsetType enSubset)
        {
            this.m_enPlayerSubset = enSubset;
        }

        public CPlayerSubset(PlayerSubsetType enSubset, string strSoldierName)
        {
            this.m_enPlayerSubset = enSubset;
            this.m_strSoldierName = strSoldierName;
        }

        public CPlayerSubset(PlayerSubsetType enSubset, int iTeamID)
        {
            this.m_enPlayerSubset = enSubset;
            this.m_iTeamID = iTeamID;
        }

        public CPlayerSubset(PlayerSubsetType enSubset, int iTeamID, int iSquadID)
        {
            this.m_enPlayerSubset = enSubset;
            this.m_iTeamID = iTeamID;
            this.m_iSquadID = iSquadID;
        }

        public PlayerSubsetType Subset
        {
            get
            {
                return this.m_enPlayerSubset;
            }
        }

        public string SoldierName
        {
            get
            {
                return this.m_strSoldierName;
            }
        }

        public int TeamID
        {
            get
            {
                return this.m_iTeamID;
            }
        }

        public int SquadID
        {
            get
            {
                return this.m_iSquadID;
            }
        }

        public Hashtable ToHashtable()
        {

            Hashtable subset = new Hashtable();

            subset.Add("type", this.m_enPlayerSubset.ToString());
            subset.Add("target", this.SoldierName);
            subset.Add("team_id", this.TeamID);
            subset.Add("squad_id", this.SquadID);

            return subset;
        }
    }
}
