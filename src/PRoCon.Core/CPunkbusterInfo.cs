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

    [Serializable]
    public class CPunkbusterInfo
    {
        public CPunkbusterInfo()
        {
            this.SlotID = String.Empty;
            this.SoldierName = String.Empty;
            this.GUID = String.Empty;
            this.Ip = String.Empty;
            this.PlayerCountry = String.Empty;
            this.PlayerCountryCode = String.Empty;
        }

        public CPunkbusterInfo(string strSlotID, string strSoldierName, string strGUID, string strIP,
                               string strPlayerCountry, string strPlayerCountryCode)
        {
            this.SlotID = strSlotID;
            this.SoldierName = strSoldierName;
            this.GUID = strGUID;
            this.Ip = strIP;
            this.PlayerCountry = strPlayerCountry;
            this.PlayerCountryCode = strPlayerCountryCode;
        }

        public string SlotID { get; private set; }

        public string SoldierName { get; private set; }

        public string GUID { get; private set; }

        public string Ip { get; private set; }

        public string PlayerCountry { get; private set; }

        public string PlayerCountryCode { get; private set; }
    }
}