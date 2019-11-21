namespace PRoCon.Core.Players
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class CPlayerInfo
    {
        public CPlayerInfo()
        {
            this.SoldierName = String.Empty;
            this.ClanTag = String.Empty;
            this.GUID = String.Empty;

            this.TeamID = -1;
            this.SquadID = -1;

            this.JoinTime = 0;
            this.SessionTime = 0;
        }

        public CPlayerInfo(IList<string> lstParameters, IList<string> lstVariables)
        {
            if (lstParameters.Count != lstVariables.Count) return;

            int iValue = 0;

            for (int i = 0; i < lstParameters.Count; i++)
            {
                if (String.Compare(lstParameters[i], "name", true) == 0)
                {
                    this.SoldierName = lstVariables[i];
                }
                if (String.Compare(lstParameters[i], "clanTag", true) == 0)
                {
                    this.ClanTag = lstVariables[i];
                }
                else if (String.Compare(lstParameters[i], "guid", true) == 0)
                {
                    this.GUID = lstVariables[i];
                }
                else if (String.Compare(lstParameters[i], "teamId", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.TeamID = iValue;
                }
                else if (String.Compare(lstParameters[i], "squadId", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.SquadID = iValue;
                }
                else if (String.Compare(lstParameters[i], "kills", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.Kills = iValue;
                }
                else if (String.Compare(lstParameters[i], "deaths", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.Deaths = iValue;
                }
                else if (String.Compare(lstParameters[i], "score", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.Score = iValue;
                }
                else if (String.Compare(lstParameters[i], "ping", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    if (iValue > 5000)
                    {
                        iValue = -1;
                    }

                    this.Ping = iValue;
                }
                else if (String.Compare(lstParameters[i], "rank", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.Rank = iValue;
                }
                else if (String.Compare(lstParameters[i], "type", true) == 0 && int.TryParse(lstVariables[i], out iValue))
                {
                    this.Type = iValue;
                }
            }

            if (this.Deaths > 0)
            {
                this.Kdr = (float)this.Kills / (float)this.Deaths;
            }
            else
            {
                this.Kdr = this.Kills;
            }

        }

        public CPlayerInfo(string strSoldierName, string strClanTag, int iTeamId, int iSquadId)
        {
            this.SoldierName = strSoldierName;
            this.ClanTag = strClanTag;

            this.TeamID = iTeamId;
            this.SquadID = iSquadId;
        }

        public CPlayerInfo(string strSoldierName, string strClanTag, int iTeamId, int iSquadId, int iJoinTime, int iSessionTime)
        {
            this.SoldierName = strSoldierName;
            this.ClanTag = strClanTag;

            this.TeamID = iTeamId;
            this.SquadID = iSquadId;

            this.JoinTime = iJoinTime;
            this.SessionTime = iSessionTime;
        }

        public string ClanTag { get; set; }

        public string SoldierName { get; set; }

        public string GUID { get; set; }

        public int TeamID { get; set; }

        public int SquadID { get; set; }

        public int Score { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Ping { get; set; }

        public int Rank { get; set; }

        public int Type { get; set; }

        public float Kdr { get; set; }

        public int JoinTime { get; set; }

        public int SessionTime { get; set; }

        //  Player list is needed in OnPlayerList, OnPlayerLeave and server.onRoundOverPlayers
        public static List<CPlayerInfo> GetPlayerList(List<string> lstWords)
        {
            List<CPlayerInfo> lstReturnList = new List<CPlayerInfo>();

            int iCurrentOffset = 0;
            int iParameters = 0;
            int iPlayers = 0;

            if (lstWords.Count > 0 && int.TryParse(lstWords[iCurrentOffset++], out iParameters) == true)
            {
                List<string> lstParameters = lstWords.GetRange(1, iParameters);
                iCurrentOffset += iParameters;

                if (lstWords.Count > iCurrentOffset && int.TryParse(lstWords[iCurrentOffset++], out iPlayers) == true)
                {
                    for (int i = 0; i < iPlayers; i++)
                    {
                        if (lstWords.Count > iCurrentOffset + (i * iParameters))
                        {
                            lstReturnList.Add(new CPlayerInfo(lstParameters, lstWords.GetRange(iCurrentOffset + i * iParameters, iParameters)));
                        }
                    }
                }
            }

            return lstReturnList;
        }
    }
}