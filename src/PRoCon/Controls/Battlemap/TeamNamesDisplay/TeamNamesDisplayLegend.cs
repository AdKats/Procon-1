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

namespace PRoCon.Controls.Battlemap.TeamNamesDisplay
{
    using Controls.Battlemap.KillDisplay;
    using PRoCon.Core;
    public class TeamNamesDisplayLegend : MapObject, IDisposable
    {

        private MapTextBlock TeamNamesText
        {
            get;
            set;
        }

        public KillDisplayColours LegendType
        {
            get;
            private set;
        }

        public TeamNamesDisplayLegend(KillDisplayColours legendType, Dictionary<int, Color> dicTeamColours, CLocalization language, CMap mapDetails)
        {

            this.TeamNamesText = new MapTextBlock();
            this.LegendType = legendType;

            if (legendType == KillDisplayColours.TeamColours)
            {

                foreach (CTeamName teamName in mapDetails.TeamNames)
                {
                    if (teamName.TeamID > 0 && dicTeamColours.ContainsKey(teamName.TeamID))
                    {
                        this.TeamNamesText.Strings.Add(new MapTextBlockString(language.GetLocalized(teamName.LocalizationKey), ControlPaint.LightLight(dicTeamColours[teamName.TeamID]), true));
                    }
                }
            }
            else
            {
                this.TeamNamesText.Strings.Add(new MapTextBlockString("Killer", Color.LightSeaGreen, true));
                this.TeamNamesText.Strings.Add(new MapTextBlockString("Victim", ControlPaint.Light(Color.Red), true));
            }

        }

        public new void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons)
        {

            if (this.TeamNamesText != null)
            {
                this.TeamNamesText.Draw(g, new PointF(pntDrawOffset.X + this.TeamNamesText.HorizontalSpacing + 10.0F, pntDrawOffset.Y), pntMouseLocation, mbButtons);
            }

            RectangleF blockBounds = this.TeamNamesText.GetBounds();

            this.m_pntDrawOffset = pntDrawOffset;

            for (int i = 0; i < this.TeamNamesText.Strings.Count; i++)
            {
                //foreach (MapTextBlockString teamName in this.TeamNamesText.Strings) {
                GraphicsPath gpTeamColour = new GraphicsPath();

                gpTeamColour.AddRectangle(new RectangleF(0.0F, i * (blockBounds.Height / this.TeamNamesText.Strings.Count) + (this.TeamNamesText.VerticalSpacing / 2.0F), 10.0F, 10.0F));

                this.DrawBwShape(g, gpTeamColour, 1.0F, 4.0F, Color.Black, this.TeamNamesText.Strings[i].TextColor);

                gpTeamColour.Dispose();
            }
        }

        public RectangleF GetBounds()
        {

            RectangleF bounds = this.TeamNamesText.GetBounds();

            bounds.Width += 10.0F + this.TeamNamesText.HorizontalSpacing;

            return bounds;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {

            this.TeamNamesText.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
