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

namespace PRoCon.Controls.Battlemap
{
    public class MapTextBlockString : MapObject
    {

        public string Text
        {
            get;
            private set;
        }

        public Color TextColor
        {
            get;
            private set;
        }

        public bool NewLine
        {
            get;
            set;
        }

        public MapTextBlockString(string text, Color textColor, bool newLine)
        {

            GraphicsPath gpText = new GraphicsPath();
            gpText.AddString(text, new FontFamily("Arial"), 0, 12.0F, new Point(0, 0), StringFormat.GenericTypographic);
            //gpText.Widen(this.m_pOneWidth);
            this.ObjectPath = gpText;

            this.TextColor = textColor;
            this.NewLine = newLine;

            this.HotSpot = this.ObjectPath.GetBounds();

            this.m_pntDrawOffset = new PointF();
        }


        protected override void MouseOver(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

        protected override void MouseLeave(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

        protected override void MouseDown(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

        protected override void MouseClicked(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

        protected override void NormalPaint(Graphics g)
        {
            this.DrawBwShape(g, 1.0F, 4.0F, Color.Black, this.TextColor);
        }

    }
}
