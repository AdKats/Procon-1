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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PRoCon.Controls.Battlemap
{
    public class MapObject : IDisposable
    {

        protected PointF m_pntDrawOffset;
        protected Point m_pntMousePosition;
        protected MouseButtons m_MouseButtons;

        protected bool m_isMouseOverLeaveCheck;
        protected bool m_isMouseDown;

        public bool IsMouseOver
        {
            get;
            protected set;
        }

        public bool IsDragging
        {
            get;
            protected set;
        }

        public GraphicsPath ObjectPath
        {
            get;
            protected set;
        }

        public RectangleF HotSpot
        {
            get;
            protected set;
        }

        protected void DrawBwShape(Graphics g, float flOpacity, float flOutlineWidth, Color clBackground, Color clForecolour)
        {
            this.DrawBwShape(g, this.ObjectPath, flOpacity, flOutlineWidth, clBackground, clForecolour);
        }

        protected void DrawBwShape(Graphics g, float flOpacity, float flOutlineWidth, Color clBackground, Brush foreColourBrush)
        {
            this.DrawBwShape(g, this.ObjectPath, flOpacity, flOutlineWidth, clBackground, foreColourBrush);
        }

        protected readonly Pen m_pOneWidth = new Pen(Brushes.Black, 1.0F);

        protected void DrawBwShape(Graphics g, GraphicsPath gpPass, float flOpacity, float flOutlineWidth, Color clBackground, Color clForecolour)
        {
            if (flOpacity > 0.0F)
            {
                GraphicsPath gp = (GraphicsPath)gpPass.Clone();

                Matrix m = new Matrix();
                m.Translate(this.m_pntDrawOffset.X, this.m_pntDrawOffset.Y);
                gp.Transform(m);

                Pen pen = new Pen(Color.FromArgb((int)(255.0F * flOpacity), clBackground), flOutlineWidth);
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, gp);
                SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255.0F * flOpacity), clForecolour));
                g.FillPath(brush, gp);

                brush.Dispose();
                pen.Dispose();
                m.Dispose();
                gp.Dispose();
            }
        }

        protected void DrawBwShape(Graphics g, GraphicsPath gpPass, float flOpacity, float flOutlineWidth, Color clBackground, Brush foreColourBrush)
        {
            if (flOpacity > 0.0F)
            {
                GraphicsPath gp = (GraphicsPath)gpPass.Clone();

                Matrix m = new Matrix();
                m.Translate(this.m_pntDrawOffset.X, this.m_pntDrawOffset.Y);
                gp.Transform(m);

                Pen pen = new Pen(Color.FromArgb((int)(255.0F * flOpacity), clBackground), flOutlineWidth);
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, gp);
                //SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255.0F * flOpacity), clForecolour));
                g.FillPath(foreColourBrush, gp);

                //brush.Dispose();
                pen.Dispose();
                m.Dispose();
                gp.Dispose();
            }
        }

        public MapObject()
        {
            this.m_pntDrawOffset = new PointF();
        }

        public MapObject(GraphicsPath gpButtonPath)
        {
            this.ObjectPath = gpButtonPath;
            this.HotSpot = this.ObjectPath.GetBounds();

            this.m_pntDrawOffset = new PointF();
        }

        protected virtual void MouseOver(Graphics g)
        {

        }

        protected virtual void MouseLeave(Graphics g)
        {

        }

        protected virtual void MouseDown(Graphics g)
        {

        }

        protected virtual void MouseUp(Graphics g)
        {

        }

        protected virtual void MouseClicked(Graphics g)
        {

        }

        protected virtual void NormalPaint(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, Color.White);
        }

        protected virtual void BeginDrag()
        {

        }

        protected virtual void EndDrag()
        {

        }

        protected virtual void CheckMouseOver(PointF pntDrawOffset, Point pntMouseLocation)
        {
            this.IsMouseOver = (new RectangleF(pntDrawOffset.X + this.HotSpot.X, pntDrawOffset.Y + this.HotSpot.Y, this.HotSpot.Width, this.HotSpot.Height)).Contains(pntMouseLocation.X, pntMouseLocation.Y);
        }

        public void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons)
        {

            //this.IsMouseOver = (new RectangleF(pntDrawOffset.X + this.HotSpot.X, pntDrawOffset.Y + this.HotSpot.Y, this.HotSpot.Width, this.HotSpot.Height)).Contains(pntMouseLocation.X, pntMouseLocation.Y);

            //Region bah = new Region(this.ObjectPath);
            //this.IsMouseOver = bah.IsVisible(new PointF(pntMouseLocation.X - pntDrawOffset.X, pntMouseLocation.Y - pntDrawOffset.Y));
            //bah.Dispose();

            this.CheckMouseOver(pntDrawOffset, pntMouseLocation);

            if (this.m_pntMousePosition != null)
            {
                if (this.IsDragging == false && this.IsMouseOver == true && this.m_isMouseDown == true && (this.m_pntMousePosition.X != pntMouseLocation.X || this.m_pntMousePosition.Y != pntMouseLocation.Y))
                {
                    this.IsDragging = true;
                    this.BeginDrag();
                }
                else if (this.IsDragging == true && (this.IsMouseOver == false || this.m_isMouseDown == false))
                {
                    this.IsDragging = false;
                    this.EndDrag();
                }
            }

            this.m_pntMousePosition = pntMouseLocation;
            this.m_pntDrawOffset = pntDrawOffset;
            this.m_MouseButtons = mbButtons;

            if (this.IsMouseOver == true && (mbButtons == MouseButtons.Left || mbButtons == MouseButtons.Right))
            {
                this.m_isMouseDown = true;
                this.MouseDown(g);
            }
            else if (this.m_isMouseDown == true && (mbButtons != MouseButtons.Left && mbButtons != MouseButtons.Right))
            {
                this.m_isMouseDown = false;

                if (this.IsMouseOver == true)
                {
                    this.MouseClicked(g);
                }
                else
                {
                    this.MouseUp(g);
                }
            }
            //else if ( && this.m_isMouseDown == true && (mbButtons != MouseButtons.Left && mbButtons != MouseButtons.Right)) {
            //    this.m_isMouseDown = false;

            //}
            else if (this.IsMouseOver == true && this.m_isMouseDown == false)
            {
                this.m_isMouseOverLeaveCheck = true;
                this.MouseOver(g);
            }
            else if (this.IsMouseOver == false && this.m_isMouseOverLeaveCheck == true && this.m_isMouseDown == false)
            {
                this.m_isMouseOverLeaveCheck = false;
                this.MouseLeave(g);
            }
            else
            {
                this.m_isMouseOverLeaveCheck = false;
                //this.m_isMouseDown = false;

                this.NormalPaint(g);
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            this.ObjectPath.Dispose();
        }

        #endregion
    }
}
