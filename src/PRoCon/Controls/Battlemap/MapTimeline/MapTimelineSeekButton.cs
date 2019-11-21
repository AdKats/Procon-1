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

namespace PRoCon.Controls.Battlemap.MapTimeline
{
    public class MapTimelineSeekButton : MapTimelineControlButton
    {

        public float SeekerPosition
        {
            get;
            set;
        }

        public bool IsSeekButtonSelected
        {
            get
            {
                return this.m_isMouseDown;
            }
        }

        public RectangleF SeekerBounds
        {
            get;
            set;
        }

        public MapTimelineSeekButton()
            : base()
        {

            this.ObjectPath = this.GetRoundRect(0, 0, 12, 4, 2);
            this.HotSpot = this.ObjectPath.GetBounds();

            this.SeekerPosition = 1.0F;
        }

        public GraphicsPath GetRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            //gp.AddLine(x + radius, y, x + width - (radius * 2), y); // Line
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90); // Corner
            //gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2)); // Line
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
            //gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height); // Line
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90); // Corner
            //gp.AddLine(x, y + height - (radius * 2), x, y + radius); // Line
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Corner
            gp.CloseFigure();

            return gp;
        }

        public new void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons)
        {

            GraphicsPath gpSeekPosition = new GraphicsPath();
            gpSeekPosition.AddLine(new PointF(6.0F, 2), new PointF(this.SeekerBounds.Width - this.SeekerPosition * (this.SeekerBounds.Width - 15) - 5, 2));
            gpSeekPosition.Widen(this.m_pOneWidth);
            this.DrawBwShape(g, gpSeekPosition, this.ButtonOpacity, 4.0F, Color.Black, ControlPaint.LightLight(Color.LightSeaGreen));

            if (this.m_isMouseDown == true)
            {

                if (pntMouseLocation.X < pntDrawOffset.X)
                {
                    this.SeekerPosition = 0.0F;
                }
                else if (pntMouseLocation.X > pntDrawOffset.X + this.SeekerBounds.Width - 15)
                {
                    this.SeekerPosition = 1.0F;
                }
                else
                {
                    this.SeekerPosition = (pntMouseLocation.X - pntDrawOffset.X - 6) / (this.SeekerBounds.Width - 15);
                }
            }

            float xBeginningOffset = pntDrawOffset.X;

            pntDrawOffset.X += this.SeekerPosition * (this.SeekerBounds.Width - 15);

            this.HotSpot = new RectangleF(-pntDrawOffset.X + xBeginningOffset, -15, this.SeekerBounds.Width, 20);

            base.Draw(g, pntDrawOffset, pntMouseLocation, mbButtons);

            gpSeekPosition.Dispose();
        }

        protected override void MouseOver(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, Color.White);
            this.DrawTime(g);
        }

        protected override void MouseLeave(Graphics g)
        {
            //this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, Color.White);
        }

        protected override void MouseDown(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 8.0F, Color.Black, Color.White);
            this.DrawTime(g);
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, Color.White);
        }

        protected override void MouseClicked(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, Color.White);
        }

        protected override void NormalPaint(Graphics g)
        {
            if (this.m_isMouseDown == true)
            {
                this.DrawBwShape(g, this.ButtonOpacity, 8.0F, Color.Black, Color.White);
                this.DrawTime(g);
            }
        }

        private void DrawTime(Graphics g)
        {
            TimeSpan tsSpan = DateTime.Now.AddHours(Math.Abs(this.SeekerPosition - 1.0F)) - DateTime.Now;

            GraphicsPath gpSeekTime = new GraphicsPath();
            string strText = String.Empty;
            Color clForecolor = Color.White;

            if (tsSpan.TotalSeconds == 0)
            {
                strText = "Live";
                clForecolor = ControlPaint.Light(ControlPaint.LightLight(Color.LightSeaGreen));
            }
            else
            {
                strText = String.Format("-{0:00}:{1:00}:{2:00}", tsSpan.Hours, tsSpan.Minutes, tsSpan.Seconds);
                clForecolor = Color.White;
            }

            SizeF szText = g.MeasureString(strText, new Font("Arial", 12));
            gpSeekTime.AddString(strText, new FontFamily("Arial"), 0, 12, new PointF(2, -szText.Height + 4), StringFormat.GenericTypographic);
            this.DrawBwShape(g, gpSeekTime, this.ButtonOpacity, 4.0F, Color.Black, clForecolor);

            gpSeekTime.Dispose();
        }
    }
}
