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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PRoCon.Controls.Battlemap.MapZoneDisplay
{
    using Core;
    using Core.Battlemap;

    public interface IMapZonePoints
    {
        void translate(float x, float y);
    }

    public class MapZoneControl : MapObject, IMapZonePoints, IDisposable
    {

        public delegate void MapZoneHandler(MapZoneControl sender);
        public event MapZoneHandler MapZoneSelected;
        public event MapZoneHandler MapZoneModified;

        private List<MapZonePointControl> lstPoints;

        private LinearGradientBrush m_zoneBackground;
        private LinearGradientBrush m_zoneFadedBackground;

        private bool m_isSelected;
        public bool IsSelected
        {
            get
            {
                return this.m_isSelected;
            }
            set
            {

                if (this.m_isSelected != value)
                {
                    this.m_isSelected = value;

                    if (this.m_isSelected == true && this.MapZoneSelected != null)
                    {
                        this.MapZoneSelected(this);
                    }
                }
            }
        }

        public bool IsMapZonePointSelected
        {
            get
            {
                bool isMouseSelected = false;

                foreach (MapZonePointControl zonePoint in this.lstPoints)
                {
                    if ((isMouseSelected = zonePoint.IsPointSelected) == true)
                    {
                        break;
                    }
                }

                return isMouseSelected;
            }
        }

        public bool IsMapZonePointMouseOvered
        {
            get
            {
                bool isMouseOvered = false;

                foreach (MapZonePointControl zonePoint in this.lstPoints)
                {
                    if ((isMouseOvered = zonePoint.IsMouseOver) == true)
                    {
                        break;
                    }
                }

                return isMouseOvered;
            }
        }

        /// <summary>
        /// This only holds a copy for drawing/editing.  It could be local or remote.
        /// </summary>
        public MapZoneDrawing ZoneDetails
        {
            get;
            set;
        }

        public MapZoneControl(MapZoneDrawing zone)
        {

            this.lstPoints = new List<MapZonePointControl>();

            this.ZoneDetails = zone;

            this.SetZonePoints(zone.ZonePolygon);
        }

        public void SetZoneTags(ZoneTagList tags)
        {

            if (this.ZoneDetails != null)
            {
                this.ZoneDetails.Tags.FromString(tags.ToString());
            }
        }

        public void SetZonePoints(Point3D[] points)
        {

            this.lstPoints.Clear();

            foreach (Point3D point in points)
            {
                MapZonePointControl newPoint = new MapZonePointControl(new PointF(point.X, point.Y));
                newPoint.MapZonePointDropped += new MapZonePointControl.MapZonePointControlDragHandler(newPoint_MapZonePointDropped);
                this.lstPoints.Add(newPoint);
            }
        }

        private void newPoint_MapZonePointDropped(MapZonePointControl sender)
        {
            if (this.MapZoneModified != null)
            {
                this.MapZoneModified(this);
            }
        }

        public static Point3D[] CreateInitialZone(Point centre)
        {

            List<Point3D> returnArray = new List<Point3D>();

            float R = 50.0F;
            float R2 = (float)(R / Math.Sqrt(2.0D));

            returnArray.Add(new Point3D(centre.X, centre.Y - (int)R, 0));
            returnArray.Add(new Point3D(centre.X + (int)R2, centre.Y - (int)R2, 0));

            returnArray.Add(new Point3D(centre.X + (int)R, centre.Y, 0));
            returnArray.Add(new Point3D(centre.X + (int)R2, centre.Y + (int)R2, 0));

            returnArray.Add(new Point3D(centre.X, centre.Y + (int)R, 0));
            returnArray.Add(new Point3D(centre.X - (int)R2, centre.Y + (int)R2, 0));

            returnArray.Add(new Point3D(centre.X - (int)R, centre.Y, 0));
            returnArray.Add(new Point3D(centre.X - (int)R2, centre.Y - (int)R2, 0));

            return returnArray.ToArray();
        }

        /*
        // Not needed, only used to pull the initial centre octogon from..
        public MapZoneControl(PointF centre) {
            this.lstPoints = new List<MapZonePointControl>();

            float R = 50.0F;
            float R2 = (float)(R / Math.Sqrt(2.0D));

            lstPoints.Add(new MapZonePointControl(new PointF(centre.X, centre.Y - R)));
            lstPoints.Add(new MapZonePointControl(new PointF(centre.X + R2, centre.Y - R2)));

            lstPoints.Add(new MapZonePointControl(new PointF(centre.X + R, centre.Y)));
            lstPoints.Add(new MapZonePointControl(new PointF(centre.X + R2, centre.Y + R2)));

            lstPoints.Add(new MapZonePointControl(new PointF(centre.X, centre.Y + R)));
            lstPoints.Add(new MapZonePointControl(new PointF(centre.X - R2, centre.Y + R2)));

            lstPoints.Add(new MapZonePointControl(new PointF(centre.X - R, centre.Y)));
            lstPoints.Add(new MapZonePointControl(new PointF(centre.X - R2, centre.Y - R2)));

        }
    */
        protected override void CheckMouseOver(PointF pntDrawOffset, Point pntMouseLocation)
        {
            if (this.ObjectPath != null)
            {
                this.IsMouseOver = this.IsMapZonePointMouseOvered || this.ObjectPath.IsVisible(new PointF(pntMouseLocation.X - pntDrawOffset.X, pntMouseLocation.Y - pntDrawOffset.Y));
            }
        }

        private BattlemapViewTools m_SelectedTool;

        private Point m_pntPreviousMouseLocation;

        public void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons, BattlemapViewTools SelectedTool)
        {

            this.m_SelectedTool = SelectedTool;

            if (SelectedTool == BattlemapViewTools.Zones && this.IsMapZonePointSelected == false && this.m_isMouseDown == true && this.m_pntPreviousMouseLocation != null && mbButtons == MouseButtons.Left)
            {
                this.translate(pntMouseLocation.X - this.m_pntPreviousMouseLocation.X, pntMouseLocation.Y - this.m_pntPreviousMouseLocation.Y);
            }

            if (this.m_zoneBackground == null)
            {
                this.m_zoneBackground = new LinearGradientBrush(new RectangleF(0.0F, 0.0F, 30.0F, 30.0F), Color.RoyalBlue, Color.FromArgb(64, Color.WhiteSmoke), LinearGradientMode.ForwardDiagonal);
                this.m_zoneBackground.SetSigmaBellShape(0.5f, 1.0f);
            }

            if (this.m_zoneFadedBackground == null)
            {
                this.m_zoneFadedBackground = new LinearGradientBrush(new RectangleF(0.0F, 0.0F, 30.0F, 30.0F), Color.FromArgb(64, Color.RoyalBlue), Color.FromArgb(64, Color.WhiteSmoke), LinearGradientMode.ForwardDiagonal);
                this.m_zoneFadedBackground.SetSigmaBellShape(0.5f, 1.0f);
            }

            //g.FillRectangle(this.m_zoneBackground, g.ClipBounds);

            this.ObjectPath = new GraphicsPath();
            this.ObjectPath.AddPolygon(this.ToPointFArray());
            this.ObjectPath.CloseAllFigures();
            //this.DrawBwShape(g, gpZoneBackground, 0.2F, 4.0F, Color.Black, Color.Honeydew);

            base.Draw(g, pntDrawOffset, pntMouseLocation, mbButtons);

            if (SelectedTool == BattlemapViewTools.Zones && this.IsSelected == true)
            {

                foreach (MapZonePointControl zonePoint in new List<MapZonePointControl>(this.lstPoints))
                {

                    if (zonePoint.IsPointSelected == true && mbButtons == MouseButtons.Left)
                    {
                        zonePoint.ZonePoint = new PointF(pntMouseLocation.X, pntMouseLocation.Y);

                        zonePoint.Draw(g, pntDrawOffset, pntMouseLocation, mbButtons);
                    }
                    else
                    {
                        if (this.IsMapZonePointSelected == false)
                        {
                            zonePoint.Draw(g, pntDrawOffset, pntMouseLocation, mbButtons);
                        }
                        else
                        {
                            zonePoint.Draw(g, pntDrawOffset, new Point(int.MaxValue, int.MaxValue), mbButtons);
                        }
                    }
                }
            }

            this.m_pntPreviousMouseLocation = pntMouseLocation;
        }

        public PointF[] ToPointFArray()
        {
            PointF[] returnArray = new PointF[this.lstPoints.Count];

            for (int i = 0; i < this.lstPoints.Count; i++)
            {
                returnArray[i] = this.lstPoints[i].ZonePoint;
            }

            return returnArray;
        }

        public Point3D[] ToPoint3DArray()
        {
            Point3D[] returnArray = new Point3D[this.lstPoints.Count];

            for (int i = 0; i < this.lstPoints.Count; i++)
            {
                returnArray[i] = this.lstPoints[i].ToPoint3D();
            }

            return returnArray;
        }

        private float GetDefaultOpacity()
        {
            if (this.IsSelected == true && this.m_SelectedTool == BattlemapViewTools.Zones)
            {
                return 1.0F;
            }
            else
            {
                return 0.2F;
            }
        }

        private Brush GetDefaultBackgroundBrush()
        {
            if (this.IsSelected == true && this.m_SelectedTool == BattlemapViewTools.Zones)
            {
                return this.m_zoneBackground;
            }
            else
            {
                return this.m_zoneFadedBackground;
            }
        }

        protected override void MouseOver(Graphics g)
        {
            if (this.m_SelectedTool == BattlemapViewTools.Zones)
            {

                this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
            }
            else
            {
                this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
            }
        }

        protected override void MouseLeave(Graphics g)
        {
            this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
        }

        protected override void MouseDown(Graphics g)
        {
            if (this.m_SelectedTool == BattlemapViewTools.Zones)
            {
                this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
                this.IsSelected = true;
            }
            else
            {
                this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
            }
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
        }

        protected override void MouseClicked(Graphics g)
        {
            if (this.m_SelectedTool == BattlemapViewTools.Zones)
            {
                this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
                this.IsSelected = true;
            }
            else
            {
                this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
            }
        }

        protected override void NormalPaint(Graphics g)
        {
            this.DrawBwShape(g, this.GetDefaultOpacity(), 4.0F, Color.Black, this.GetDefaultBackgroundBrush());
        }

        protected override void EndDrag()
        {
            if (this.MapZoneModified != null)
            {
                this.MapZoneModified(this);
            }
        }

        #region IMapZonePoints Members

        public void translate(float x, float y)
        {

            foreach (MapZonePointControl zonePoint in this.lstPoints)
            {
                zonePoint.translate(x, y);
            }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {

            if (this.m_zoneBackground != null)
            {
                this.m_zoneBackground.Dispose();
            }

            if (this.m_zoneFadedBackground != null)
            {
                this.m_zoneFadedBackground.Dispose();
            }

            base.Dispose();
        }

        #endregion
    }
}
