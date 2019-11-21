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

namespace PRoCon.Controls.Battlemap
{
    using Core;
    public class BattlemapRoundChange
    {

        public CMap Map
        {
            get;
            private set;
        }

        public DateTime ChangeTime
        {
            get;
            private set;
        }

        public BattlemapRoundChange(CMap cmMap)
        {
            this.Map = cmMap;
            this.ChangeTime = DateTime.Now;
        }

        public BattlemapRoundChange(CMap cmMap, DateTime dtChangeTime)
        {
            this.Map = cmMap;
            this.ChangeTime = dtChangeTime;
        }
    }
}
