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
using System.Drawing;
using System.Drawing.Drawing2D;

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

        public GraphicsPath ZoneGraphicsPath
        {
            get
            {
                var gpReturn = new GraphicsPath();
                var pntPolygon = new PointF[ZonePolygon.Length];
                for (int i = 0; i < ZonePolygon.Length; i++)
                {
                    pntPolygon[i] = new PointF(ZonePolygon[i].X, ZonePolygon[i].Y);
                }
                gpReturn.AddPolygon(pntPolygon);
                gpReturn.CloseFigure();

                return gpReturn;
            }
        }

        public event TagsEditedHandler TagsEdited;

        // Returns a percentage of the ErrorArea circle trespassing on the zone.
        // If anyone knows calculus better than me I'd welcome you to clean up this function =)
        public float TrespassArea(Point3D pntLocation, float flErrorRadius)
        {
            float returnPercentage = 0.0F;
            var errorArea = (float)(flErrorRadius * flErrorRadius * Math.PI);

            var gpLocationError = new GraphicsPath();
            gpLocationError.AddEllipse(new RectangleF(pntLocation.X - flErrorRadius, pntLocation.Y - flErrorRadius, flErrorRadius * 2, flErrorRadius * 2));
            gpLocationError.CloseAllFigures();

            var regZone = new Region(ZoneGraphicsPath);
            regZone.Intersect(gpLocationError);
            RectangleF[] scans = regZone.GetRegionScans(new Matrix());
            var recIntersection = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);

            int iPixelCount = 0;

            if (scans.Length > 0)
            {
                for (int i = 0; i < scans.Length; i++)
                {
                    recIntersection.X = scans[i].X < recIntersection.X ? (int)scans[i].X : recIntersection.X;
                    recIntersection.Y = scans[i].Y < recIntersection.Y ? (int)scans[i].Y : recIntersection.Y;

                    recIntersection.Width = scans[i].Right > recIntersection.Right ? (int)scans[i].Right - recIntersection.X : recIntersection.Width;
                    recIntersection.Height = scans[i].Bottom > recIntersection.Bottom ? (int)scans[i].Bottom - recIntersection.Y : recIntersection.Height;
                }

                var pntVisible = new Point(recIntersection.X, recIntersection.Y);

                for (pntVisible.X = recIntersection.X; pntVisible.X <= recIntersection.Right; pntVisible.X++)
                {
                    for (pntVisible.Y = recIntersection.Y; pntVisible.Y <= recIntersection.Bottom; pntVisible.Y++)
                    {
                        if (regZone.IsVisible(pntVisible) == true)
                        {
                            iPixelCount++;
                        }
                    }
                }
            }

            returnPercentage = iPixelCount / errorArea;

            // Accounts for low error when using this method. (98.4% should be 100%)
            // but using regZone.GetRegionScans is slightly lossy.
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