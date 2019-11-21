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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace PRoCon.Controls.Battlemap.MapZoneDisplay
{
    using Core;

    public class MapZonePointControl : MapObject, IMapZonePoints
    {

        public delegate void MapZonePointControlDragHandler(MapZonePointControl sender);
        public event MapZonePointControlDragHandler MapZonePointDropped;

        public bool IsPointSelected
        {
            get
            {
                return this.m_isMouseDown;
            }
        }

        private PointF m_zonePoint;
        public PointF ZonePoint
        {
            get
            {
                return this.m_zonePoint;
            }
            set
            {
                this.m_zonePoint = value;
                this.ObjectPath = new GraphicsPath();

                RectangleF recPoint = new RectangleF(value.X - 3.0F, value.Y - 3.0F, 6.0F, 6.0F);
                this.ObjectPath.AddRectangle(recPoint);
                this.HotSpot = this.ObjectPath.GetBounds();

                this.HotSpot = new RectangleF(this.HotSpot.X - 10.0F, this.HotSpot.Y - 10.0F, this.HotSpot.Width + 20.0F, this.HotSpot.Height + 20.0F);
            }
        }

        public Point3D ToPoint3D()
        {
            return new Point3D((int)this.ZonePoint.X, (int)this.ZonePoint.Y, 0);
        }

        public MapZonePointControl(PointF point)
        {
            this.ZonePoint = point;

        }

        protected override void MouseOver(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.Blue);
        }

        protected override void MouseLeave(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.WhiteSmoke);
        }

        protected override void MouseDown(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.WhiteSmoke);
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.WhiteSmoke);
        }

        protected override void MouseClicked(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.WhiteSmoke);
        }

        protected override void NormalPaint(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.WhiteSmoke);
        }

        protected override void EndDrag()
        {
            if (this.MapZonePointDropped != null)
            {
                this.MapZonePointDropped(this);
            }
        }

        #region IMapZonePoints Members

        public void translate(float x, float y)
        {
            this.ZonePoint = new PointF(this.ZonePoint.X + x, this.ZonePoint.Y + y);
        }

        #endregion
    }
}
