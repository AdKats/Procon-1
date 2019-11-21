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

using System.Collections.ObjectModel;

namespace PRoCon.Controls.Battlemap.MapImagePacks
{
    public class MapImagePackDictionary : KeyedCollection<string, MapImagePack>
    {

        public delegate void ImagePackAlteredHandler(MapImagePack item);
        public event ImagePackAlteredHandler ImagePackAdded;
        public event ImagePackAlteredHandler ImagePackRemoved;

        protected override string GetKeyForItem(MapImagePack item)
        {
            return item.MapImagePackPath;
        }

        protected override void InsertItem(int index, MapImagePack item)
        {
            base.InsertItem(index, item);

            if (this.ImagePackAdded != null)
            {
                this.ImagePackAdded(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            MapImagePack clocRemoved = this[index];

            base.RemoveItem(index);

            if (this.ImagePackRemoved != null)
            {
                this.ImagePackRemoved(clocRemoved);
            }
        }
    }

}
