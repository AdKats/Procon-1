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
    public class WeaponDictionary : KeyedCollection<string, Weapon>, ICloneable
    {

        protected override string GetKeyForItem(Weapon item)
        {
            return item.Name;
        }

        public object Clone()
        {
            // Only need a shallow copy since the Weapon objects are readonly themselves.
            return this.MemberwiseClone();
        }

        public new Weapon this[string key]
        {
            get
            {
                Weapon keyedWeapon = null;

                foreach (Weapon weapon in this)
                {
                    if (String.Compare(weapon.Name, key, true) == 0)
                    {
                        keyedWeapon = weapon;
                        break;
                    }
                }

                return keyedWeapon;
            }
        }

        public new bool Contains(string key)
        {

            bool isKeyed = false;

            foreach (Weapon weapon in this)
            {
                if (String.Compare(weapon.Name, key, true) == 0)
                {
                    isKeyed = true;
                    break;
                }
            }

            return isKeyed;
        }
    }
}

