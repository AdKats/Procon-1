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

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace PRoCon.Core
{
    using System;

    [Serializable]
    public class CServerInfo
    {
        public CServerInfo()
        {
            this.ServerName = String.Empty;
            this.Map = String.Empty;
            this.GameMode = String.Empty;
            this.ConnectionState = String.Empty;

            this.PlayerCount = 0;
            this.MaxPlayerCount = 0;
            this.CurrentRound = 0;
            this.TotalRounds = 0;
        }
        /*
        // Legacy constructor for R8 and below.
        public CServerInfo(string strServerName, string strMap, string strGameMode, int iPlayerCount,
                           int iMaxPlayerCount) {
            this.ServerName = strServerName;
            this.Map = strMap;
            this.GameMode = strGameMode;

            this.PlayerCount = iPlayerCount;
            this.MaxPlayerCount = iMaxPlayerCount;
        }

        // Legacy constructor for R11 and below
        public CServerInfo(string strServerName, string strMap, string strGameMode, int iPlayerCount,
                           int iMaxPlayerCount, int iCurrentRound, int iTotalRounds) {
            this.ServerName = strServerName;
            this.Map = strMap;
            this.GameMode = strGameMode;

            this.PlayerCount = iPlayerCount;
            this.MaxPlayerCount = iMaxPlayerCount;
            this.CurrentRound = iCurrentRound;
            this.TotalRounds = iTotalRounds;
        }

        // Legacy constructor for R16 and below
        public CServerInfo(string strServerName, string strMap, string strGameMode, int iPlayerCount,
                           int iMaxPlayerCount, int iCurrentRound, int iTotalRounds, List<TeamScore> lstTeamScores) {
            this.ServerName = strServerName;
            this.Map = strMap;
            this.GameMode = strGameMode;

            this.PlayerCount = iPlayerCount;
            this.MaxPlayerCount = iMaxPlayerCount;
            this.CurrentRound = iCurrentRound;
            this.TotalRounds = iTotalRounds;
            this.TeamScores = lstTeamScores;
        }
        */
        // Legacy constructor for R17 and below
        // Legacy constructor for backwards plugin compatability
        [Obsolete]
        public CServerInfo(string strServerName, string strMap, string strGameMode, int iPlayerCount,
                           int iMaxPlayerCount, int iCurrentRound, int iTotalRounds, List<TeamScore> lstTeamScores, string ConnectionState)
        {
            this.ServerName = strServerName;
            this.Map = strMap;
            this.GameMode = strGameMode;
            this.ConnectionState = ConnectionState;

            this.PlayerCount = iPlayerCount;
            this.MaxPlayerCount = iMaxPlayerCount;
            this.CurrentRound = iCurrentRound;
            this.TotalRounds = iTotalRounds;
            this.TeamScores = lstTeamScores;
        }
        public string ServerName { get; set; }

        public string Map { get; set; }

        public string GameMode { get; set; }

        public string ConnectionState { get; set; }

        public int PlayerCount { get; set; }

        public int MaxPlayerCount { get; set; }

        public int CurrentRound { get; set; }

        public int TotalRounds { get; set; }

        // R27
        // <ranked: boolean> <punkBuster: boolean> <hasGamePassword: boolean> <serverUpTime: seconds> <roundTime: seconds> <gameMod: GameModId> <mapPack: integer>
        public bool Ranked { get; set; }
        public bool PunkBuster { get; set; }
        public bool Passworded { get; set; }
        public int ServerUptime { get; set; }
        public int RoundTime { get; set; }
        public GameMods GameMod { get; set; }
        public int Mappack { get; set; }

        // R28
        // <externalGameIpAndPort: IpPortPair>
        public string ExternalGameIpandPort { get; set; }

        // R32
        // <punkBusterVersion: PunkBusterVersion> <joinQueueEnabled: boolean> <region: Region>
        public string PunkBusterVersion { get; set; }
        public bool JoinQueueEnabled { get; set; }
        public string ServerRegion { get; set; }
        public string PingSite { get; set; }
        public string ServerCountry { get; set; }
        public bool QuickMatch { get; set; }

        // BF4
        public int BlazePlayerCount { get; set; }
        public string BlazeGameState { get; set; }

        public List<TeamScore> TeamScores
        {
            get;
            private set;
        }

        public CServerInfo(List<string> parameters, List<string> variables)
        {

            this.RoundTime = this.ServerUptime = -1;

            for (int paramCount = 0, varCount = 0; paramCount < parameters.Count && varCount < variables.Count; paramCount++, varCount++)
            {

                switch (parameters[paramCount])
                {
                    case "TeamScores":

                        int scoresCount = 0;

                        if (int.TryParse(variables[varCount], out scoresCount) == true)
                        {
                            scoresCount++;

                            this.TeamScores = TeamScore.GetTeamScores(variables.GetRange(varCount, scoresCount + 1));

                            varCount += scoresCount;
                        }
                        else
                        {
                            varCount--;
                        }

                        break;
                    case "GameMod":

                        if (Enum.IsDefined(typeof(GameMods), variables[varCount]) == true)
                        {
                            this.GameMod = (GameMods)Enum.Parse(typeof(GameMods), variables[varCount]);
                        }

                        break;
                    default:
                        PropertyInfo property = null;
                        if ((property = this.GetType().GetProperty(parameters[paramCount])) != null)
                        {

                            try
                            {
                                // Geoff Green 08/10/2011 to account for a bug in BF3 release 872601 F sending back blank "" instead of a valid int.
                                if (variables[varCount].Length == 0 && property.PropertyType == typeof(int))
                                {
                                    variables[varCount] = "0";
                                }

                                object value = TypeDescriptor.GetConverter(property.PropertyType).ConvertFrom(variables[varCount]);

                                if (value != null)
                                {
                                    property.SetValue(this, value, null);
                                }

                            }
                            catch (Exception) { }
                        }

                        break;
                }
            }
        }
    }
}