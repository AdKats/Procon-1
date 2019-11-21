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
using System.Windows.Forms;


namespace PRoCon.Controls.Battlemap.MapTimeline
{
    public class MapTimelineControlButton : MapObject
    {

        public delegate void TimelineControlButtonClickedHandler(MapTimelineControlButton sender, MapTimelineControlButtonType ButtonType);
        public event TimelineControlButtonClickedHandler TimelineControlButtonClicked;

        public MapTimelineControlButtonType ButtonType
        {
            get;
            private set;
        }

        public float ButtonOpacity
        {
            get;
            set;
        }

        public Color ForegroundColour
        {
            get;
            set;
        }

        public MapTimelineControlButton(GraphicsPath gpButtonPath, MapTimelineControlButtonType mtbtButtonType)
            : base(gpButtonPath)
        {
            this.ButtonOpacity = 0.0F;
            this.ButtonType = mtbtButtonType;
            this.ForegroundColour = Color.White;
        }

        public MapTimelineControlButton()
            : base()
        {
            this.ButtonOpacity = 0.0F;
            this.ButtonType = MapTimelineControlButtonType.None;
        }

        protected override void MouseOver(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, ControlPaint.Light(Color.RoyalBlue));
        }

        protected override void MouseLeave(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, this.ForegroundColour);
        }

        protected override void MouseDown(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 8.0F, Color.Black, ControlPaint.Light(Color.RoyalBlue));
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, this.ForegroundColour);
        }

        protected override void MouseClicked(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 8.0F, Color.Black, ControlPaint.Light(Color.RoyalBlue));

            if (this.TimelineControlButtonClicked != null)
            {
                this.TimelineControlButtonClicked(this, this.ButtonType);
                //this.TimelineControlButtonClicked(this.ButtonType);
            }
        }

        protected override void NormalPaint(Graphics g)
        {
            this.DrawBwShape(g, this.ButtonOpacity, 4.0F, Color.Black, this.ForegroundColour);
        }
    }
}
