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
using System.Collections.ObjectModel;

namespace PRoCon.Core.Players.Items
{

    [Serializable]
    public class SpecializationDictionary : KeyedCollection<string, Specialization>, ICloneable
    {

        protected override string GetKeyForItem(Specialization item)
        {
            return item.Name;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public new Specialization this[string key]
        {
            get
            {
                Specialization keyedSpecialization = null;

                foreach (Specialization spec in this)
                {
                    if (String.Compare(spec.Name, key, true) == 0)
                    {
                        keyedSpecialization = spec;
                        break;
                    }
                }

                return keyedSpecialization;
            }
        }

        public new bool Contains(string key)
        {

            bool isKeyed = false;

            foreach (Specialization spec in this)
            {
                if (String.Compare(spec.Name, key, true) == 0)
                {
                    isKeyed = true;
                    break;
                }
            }

            return isKeyed;
        }
    }
}
