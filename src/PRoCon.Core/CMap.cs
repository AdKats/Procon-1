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

namespace PRoCon.Core
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class CMap
    {
        public CMap()
        {
            this.PlayList = String.Empty;
            this.FileName = String.Empty;
            this.GameMode = String.Empty;
            this.PublicLevelName = String.Empty;
            this.TeamNames = new List<CTeamName>();
            this.DefaultSquadID = 0;
        }

        public CMap(string strPlaylist, string strFileName, string strGamemode, string strPublicLevelName, int iDefaultSquadID)
        {
            this.PlayList = strPlaylist;
            this.FileName = strFileName;
            this.GameMode = strGamemode;
            this.PublicLevelName = strPublicLevelName;
            this.TeamNames = new List<CTeamName>();
            this.DefaultSquadID = iDefaultSquadID;
        }

        public string PlayList { get; private set; }

        public string FileName { get; private set; }

        public string GameMode { get; private set; }

        public string PublicLevelName { get; private set; }

        public int DefaultSquadID
        {
            get;
            private set;
        }

        public List<CTeamName> TeamNames { get; set; }
    }
}