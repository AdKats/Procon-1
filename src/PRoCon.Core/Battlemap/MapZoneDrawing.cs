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

namespace PRoCon.Core.Battlemap
{
    [Serializable]
    public class MapZoneDrawing : MapZone
    {
        public delegate void TagsEditedHandler(MapZoneDrawing sender);

        public MapZoneDrawing(string strUid, string strLevelFileName, string strTagList, Point3D[] zonePolygon, bool blInclusive) : base(strUid, strLevelFileName, strTagList, zonePolygon, blInclusive)
        {
            Tags.TagsEdited += new ZoneTagList.TagsEditedHandler(Tags_TagsEdited);
        }

        public event TagsEditedHandler TagsEdited;

        /// <summary>
        /// Tests whether a point is inside the zone polygon using the ray-casting algorithm.
        /// Cross-platform replacement for System.Drawing.Drawing2D.GraphicsPath.IsVisible.
        /// </summary>
        private bool IsPointInPolygon(float testX, float testY)
        {
            if (ZonePolygon == null || ZonePolygon.Length < 3)
                return false;

            bool inside = false;
            int j = ZonePolygon.Length - 1;

            for (int i = 0; i < ZonePolygon.Length; i++)
            {
                float xi = ZonePolygon[i].X, yi = ZonePolygon[i].Y;
                float xj = ZonePolygon[j].X, yj = ZonePolygon[j].Y;

                if (((yi > testY) != (yj > testY)) &&
                    (testX < (xj - xi) * (testY - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }

        /// <summary>
        /// Returns a percentage of the error-radius circle that overlaps with the zone polygon.
        /// Uses point-in-polygon sampling instead of System.Drawing Region/GraphicsPath.
        /// </summary>
        public float TrespassArea(Point3D pntLocation, float flErrorRadius)
        {
            float returnPercentage = 0.0F;
            var errorArea = (float)(flErrorRadius * flErrorRadius * Math.PI);

            // Determine the bounding box of the error circle
            float minX = pntLocation.X - flErrorRadius;
            float minY = pntLocation.Y - flErrorRadius;
            float maxX = pntLocation.X + flErrorRadius;
            float maxY = pntLocation.Y + flErrorRadius;

            float radiusSquared = flErrorRadius * flErrorRadius;
            int iPixelCount = 0;

            // Sample integer grid points within the bounding box, counting those
            // inside both the circle and the polygon (same approach as the original).
            for (int x = (int)minX; x <= (int)maxX; x++)
            {
                for (int y = (int)minY; y <= (int)maxY; y++)
                {
                    float dx = x - pntLocation.X;
                    float dy = y - pntLocation.Y;

                    // Point must be inside the error circle AND inside the polygon
                    if (dx * dx + dy * dy <= radiusSquared && IsPointInPolygon(x, y))
                    {
                        iPixelCount++;
                    }
                }
            }

            returnPercentage = iPixelCount / errorArea;

            // Accounts for low error when using this method. (98.4% should be 100%)
            // but using pixel sampling is slightly lossy.
            if (returnPercentage > 0.0F)
            {
                returnPercentage = (float)Math.Min(1.0F, returnPercentage + 0.02);
            }

            return returnPercentage;
        }

        private void Tags_TagsEdited(ZoneTagList sender)
        {
            if (TagsEdited != null)
            {
                this.TagsEdited(this);
            }
        }
    }
}
