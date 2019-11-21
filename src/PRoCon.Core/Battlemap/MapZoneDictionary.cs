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
using System.Collections.ObjectModel;
using System.Text;

namespace PRoCon.Core.Battlemap
{
    [Serializable]
    public class MapZoneDictionary : KeyedCollection<string, MapZoneDrawing>
    {
        public delegate void MapZoneAlteredHandler(MapZoneDrawing item);

        public event MapZoneAlteredHandler MapZoneAdded;
        public event MapZoneAlteredHandler MapZoneChanged;
        public event MapZoneAlteredHandler MapZoneRemoved;

        protected override string GetKeyForItem(MapZoneDrawing item)
        {
            return item.UID;
        }

        protected override void InsertItem(int index, MapZoneDrawing item)
        {
            base.InsertItem(index, item);
            item.TagsEdited += new MapZoneDrawing.TagsEditedHandler(item_TagsEdited);

            if (MapZoneAdded != null)
            {
                this.MapZoneAdded(item);
            }
        }

        private void item_TagsEdited(MapZoneDrawing sender)
        {
            if (MapZoneChanged != null)
            {
                this.MapZoneChanged(sender);
            }
        }

        protected override void RemoveItem(int index)
        {
            MapZoneDrawing apRemoved = this[index];
            apRemoved.TagsEdited -= new MapZoneDrawing.TagsEditedHandler(item_TagsEdited);

            base.RemoveItem(index);

            if (MapZoneRemoved != null)
            {
                this.MapZoneRemoved(apRemoved);
            }
        }

        protected override void SetItem(int index, MapZoneDrawing item)
        {
            if (MapZoneChanged != null)
            {
                this.MapZoneChanged(item);
            }

            base.SetItem(index, item);
            item.TagsEdited += new MapZoneDrawing.TagsEditedHandler(item_TagsEdited);
        }

        public void CreateMapZone(string mapFileName, Point3D[] points)
        {
            var random = new Random();
            string strUid = String.Empty;

            do
            {
                strUid = String.Format("{0}{1}", mapFileName, random.Next());
                strUid = Convert.ToBase64String(Encoding.ASCII.GetBytes(strUid));
            } while (Contains(strUid) == true);

            Add(new MapZoneDrawing(strUid, mapFileName, "", points, true));
        }

        public void ModifyMapZonePoints(string strUid, Point3D[] points)
        {
            if (Contains(strUid) == true)
            {
                // this[strUid].LevelFileName = mapFileName;
                this[strUid].ZonePolygon = points;

                if (MapZoneChanged != null)
                {
                    this.MapZoneChanged(this[strUid]);
                }
            }
        }
    }
}