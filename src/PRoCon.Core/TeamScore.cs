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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace PRoCon.Core
{

    [Serializable]
    public class TeamScore
    {

        /// <summary>
        /// The team ID (offset from array of scores +1?)
        /// </summary>
        public int TeamID
        {
            get;
            private set;
        }

        /// <summary>
        /// Tickets the team has.
        /// </summary>
        public int Score
        {
            get;
            private set;
        }

        /// <summary>
        /// The score required to win a round.
        /// </summary>
        public int WinningScore
        {
            get;
            private set;
        }

        [Obsolete]
        public TeamScore(int iTeamID, int iScore)
        {
            this.TeamID = iTeamID;
            this.Score = iScore;
        }

        public TeamScore(int iTeamID, int iScore, int iWinningScore)
        {
            this.TeamID = iTeamID;
            this.Score = iScore;
            this.WinningScore = iWinningScore;
        }

        public static List<TeamScore> GetTeamScores(List<string> lstWords)
        {
            List<TeamScore> lstReturnScores = new List<TeamScore>();

            int iTotalScores = 0;
            float flScore = 0;
            int iWinningScore = 0;

            if (lstWords.Count >= 1 && int.TryParse(lstWords[0], out iTotalScores) == true && lstWords.Count >= iTotalScores + 1)
            {

                int.TryParse(lstWords[lstWords.Count - 1], out iWinningScore);

                for (int i = 0; i < iTotalScores; i++)
                {

                    if (float.TryParse(lstWords[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out flScore) == true)
                    {
                        lstReturnScores.Add(new TeamScore(i + 1, Convert.ToInt32(flScore), iWinningScore));
                    }
                }
            }

            return lstReturnScores;
        }
    }
}
