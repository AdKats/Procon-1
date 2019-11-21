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

namespace PRoCon.Core
{

    [Serializable]
    public class LevelVariableContext : IComparable
    {
        public LevelVariableContextType ContextType
        {
            get;
            private set;
        }

        public string ContextTarget
        {
            get;
            private set;
        }

        public LevelVariableContext(LevelVariableContextType lvctContextType)
        {
            this.ContextType = lvctContextType;
            this.ContextTarget = String.Empty;
        }

        public LevelVariableContext(LevelVariableContextType lvctContextType, string strContextTarget)
        {
            this.ContextType = lvctContextType;
            this.ContextTarget = strContextTarget;
        }

        public LevelVariableContext(string strContextType, string strContextTarget)
        {

            if (String.Compare(strContextType, "all", true) == 0)
            {
                this.ContextType = LevelVariableContextType.All;
            }
            else if (String.Compare(strContextType, "gamemode", true) == 0)
            {
                this.ContextType = LevelVariableContextType.GameMode;
            }
            else if (String.Compare(strContextType, "level", true) == 0)
            {
                this.ContextType = LevelVariableContextType.Level;
            }
            else
            {
                this.ContextType = LevelVariableContextType.None;
            }

            this.ContextTarget = strContextTarget;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            LevelVariableContext compareObject = (LevelVariableContext)obj;

            //int returnCompare = String.Compare(this.ContextType.ToString(), compareObject.ContextType.ToString(), true);
            //returnCompare += String.Compare(this.ContextTarget, compareObject.ContextTarget, true);

            return String.Compare(this.ToString(), compareObject.ToString(), true);
        }

        #endregion

        public override string ToString()
        {
            if (this.ContextTarget.Length > 0)
            {
                return String.Format("{0} - {1}", this.ContextType.ToString(), this.ContextTarget);
            }
            else
            {
                return String.Format("{0}", this.ContextType.ToString());
            }

        }
    }
}
