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

namespace PRoCon.Controls.Battlemap.KillDisplay
{
    using Core;
    public class KillDisplayPopup : MapObject
    {

        // MapObject is only used in this class as a reference point, nothing in it is really used per se.
        public void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons, Kill kMouseOveredKill, KillDisplayDetails kddDisplayDetails, Image imgDamageType, string strLocalizedDamageType, KillDisplayColours colours, Dictionary<int, Color> teamColours)
        {

            //string killerName = kMouseOveredKill.Killer.ClanTag.Length > 0 ? String.Format("[{0}] {1}", kMouseOveredKill.Killer.ClanTag, kMouseOveredKill.Killer.SoldierName) : kMouseOveredKill.Killer.SoldierName;
            //string victimName = kMouseOveredKill.Victim.ClanTag.Length > 0 ? String.Format("[{0}] {1}", kMouseOveredKill.Victim.ClanTag, kMouseOveredKill.Victim.SoldierName) : kMouseOveredKill.Victim.SoldierName;

            GraphicsPath gpBackground = new GraphicsPath();
            RectangleF recBackground;

            MapTextBlock textBlock = new MapTextBlock();

            Color killerColour = Color.White;
            Color victimColour = Color.White;

            if (colours == KillDisplayColours.EnemyColours)
            {
                killerColour = ControlPaint.LightLight(Color.SeaGreen);
                victimColour = ControlPaint.LightLight(Color.Red);
            }
            else if (colours == KillDisplayColours.TeamColours)
            {
                if (teamColours.ContainsKey(kMouseOveredKill.Killer.TeamID) == true && teamColours.ContainsKey(kMouseOveredKill.Victim.TeamID) == true)
                {

                    killerColour = ControlPaint.LightLight(teamColours[kMouseOveredKill.Killer.TeamID]);
                    victimColour = ControlPaint.LightLight(teamColours[kMouseOveredKill.Victim.TeamID]);
                }
            }

            if (kMouseOveredKill.Killer.ClanTag.Length > 0)
            {
                textBlock.Strings.Add(new MapTextBlockString(String.Format("Killer: [{0}] ", kMouseOveredKill.Killer.ClanTag), Color.WhiteSmoke, false));
            }
            else
            {
                textBlock.Strings.Add(new MapTextBlockString("Killer: ", Color.WhiteSmoke, false));
            }
            textBlock.Strings.Add(new MapTextBlockString(kMouseOveredKill.Killer.SoldierName, killerColour, true));

            if (kMouseOveredKill.Victim.ClanTag.Length > 0)
            {
                textBlock.Strings.Add(new MapTextBlockString(String.Format("Victim: [{0}] ", kMouseOveredKill.Victim.ClanTag), Color.WhiteSmoke, false));
            }
            else
            {
                textBlock.Strings.Add(new MapTextBlockString("Victim: ", Color.WhiteSmoke, false));
            }
            textBlock.Strings.Add(new MapTextBlockString(kMouseOveredKill.Victim.SoldierName, victimColour, true));

            textBlock.Strings.Add(new MapTextBlockString(String.Format("Weapon: {0}", strLocalizedDamageType), Color.WhiteSmoke, true));
            textBlock.Strings.Add(new MapTextBlockString(String.Format("Distance: {0:0.0} m, {1:0.0} yd", kMouseOveredKill.Distance, kMouseOveredKill.Distance * 1.0936133D), Color.WhiteSmoke, true));

            if (kMouseOveredKill.Headshot == true)
            {
                textBlock.Strings.Add(new MapTextBlockString("... HEADSHOT ...", killerColour, true));
            }

            RectangleF recTextSize = textBlock.GetBounds();

            if (imgDamageType != null)
            {
                recBackground = new RectangleF(new PointF(pntDrawOffset.X, pntDrawOffset.Y - (imgDamageType.Height + recTextSize.Height)), new SizeF(Math.Max(imgDamageType.Width, recTextSize.Width) + 10.0F, imgDamageType.Height + recTextSize.Height));
                gpBackground.AddRectangle(recBackground);

                // DRAWBLOCK: new PointF(recBackground.X + 10.0F, recBackground.Y + imgDamageType.Height)

                //gpKillPopupText.AddString(strText, new FontFamily("Arial"), 0, 12, new PointF(recBackground.X + 10.0F, recBackground.Y + imgDamageType.Height), StringFormat.GenericTypographic);
                //this.DrawText(g, strText, new Point(pntMouseLocation.X + 10, pntMouseLocation.Y - 100 + imgDamageType.Height), 12, 1.0F);
            }
            else
            {
                recBackground = new RectangleF(new PointF(pntDrawOffset.X, pntDrawOffset.Y - recTextSize.Height), new SizeF(recTextSize.Width + 10.0F, recTextSize.Height));

                gpBackground.AddRectangle(recBackground);

                // DRAW BLOCK : new PointF(recBackground.X + 10.0F, recBackground.Y + 1.0F)

                //gpKillPopupText.AddString(strText, new FontFamily("Arial"), 0, 12, new PointF(recBackground.X + 10.0F, recBackground.Y + 1.0F), StringFormat.GenericTypographic);
                //this.DrawText(g, strText, new Point(pntMouseLocation.X + 10, pntMouseLocation.Y - 100), 12, 1.0F);
            }

            // Give it a little bit of a border.

            if (pntDrawOffset.X + recBackground.Width > g.ClipBounds.Width)
            {
                this.m_pntDrawOffset.X = -1 * ((pntDrawOffset.X + recBackground.Width) - g.ClipBounds.Width);
            }

            if (pntDrawOffset.Y - recBackground.Height < 0)
            {
                this.m_pntDrawOffset.Y = -1 * (pntDrawOffset.Y - recBackground.Height);
            }

            this.DrawBwShape(g, gpBackground, 0.8F, 4.0F, Color.Black, Color.White);
            //this.DrawBwShape(g, gpKillPopupText, 1.0F, 4.0F, Color.Black, Color.White);
            textBlock.Draw(g, new PointF(this.m_pntDrawOffset.X + recBackground.X + 5.0F, this.m_pntDrawOffset.Y + recBackground.Y + 10.0F + (imgDamageType != null ? imgDamageType.Height : 0)), pntMouseLocation, mbButtons);
            //textBlock.Draw(g, new PointF(pntDrawOffset.X + 5.0F, pntDrawOffset.Y - recTextSize.Height + 10.0F), pntMouseLocation, mbButtons);

            if (imgDamageType != null)
            {

                g.DrawImage(imgDamageType, new RectangleF((recBackground.X + recBackground.Width / 2) - imgDamageType.Width / 2 + this.m_pntDrawOffset.X, recBackground.Y + this.m_pntDrawOffset.Y + 5.0F, imgDamageType.Width, imgDamageType.Height));
            }

            textBlock.Dispose();
            gpBackground.Dispose();

            /*

            string strText = String.Format("Killer: {0}\nVictim: {1}\nWeapon: {2}\nDistance: {3:0.0} m, {4:0.0} yd", killerName, victimName, strLocalizedDamageType, kMouseOveredKill.Distance, kMouseOveredKill.Distance * 1.0936133D);
            SizeF szTextSize = g.MeasureString(strText, new Font("Arial", 10));

            GraphicsPath gpBackground = new GraphicsPath();
            GraphicsPath gpKillPopupText = new GraphicsPath();
            RectangleF recBackground;

            if (imgDamageType != null) {
                recBackground = new RectangleF(new PointF(pntDrawOffset.X, pntDrawOffset.Y - (imgDamageType.Height + szTextSize.Height)), new SizeF(Math.Max(imgDamageType.Width, szTextSize.Width), imgDamageType.Height + szTextSize.Height));
                gpBackground.AddRectangle(recBackground);

                gpKillPopupText.AddString(strText, new FontFamily("Arial"), 0, 12, new PointF(recBackground.X + 10.0F, recBackground.Y + imgDamageType.Height), StringFormat.GenericTypographic);
                //this.DrawText(g, strText, new Point(pntMouseLocation.X + 10, pntMouseLocation.Y - 100 + imgDamageType.Height), 12, 1.0F);
            }
            else {
                recBackground = new RectangleF(new PointF(pntDrawOffset.X, pntDrawOffset.Y - szTextSize.Height), new SizeF(szTextSize.Width, szTextSize.Height));

                gpBackground.AddRectangle(recBackground);

                gpKillPopupText.AddString(strText, new FontFamily("Arial"), 0, 12, new PointF(recBackground.X + 10.0F, recBackground.Y + 1.0F), StringFormat.GenericTypographic);
                //this.DrawText(g, strText, new Point(pntMouseLocation.X + 10, pntMouseLocation.Y - 100), 12, 1.0F);
            }

            //gpBackground.Widen(this.m_pOneWidth);
            //gpKillPopupText.Widen(this.m_pOneWidth);

            if (pntDrawOffset.X + recBackground.Width > g.ClipBounds.Width) {
                this.m_pntDrawOffset.X = -1 * ((pntDrawOffset.X + recBackground.Width) - g.ClipBounds.Width);
            }

            if (pntDrawOffset.Y - recBackground.Height < 0) {
                this.m_pntDrawOffset.Y = -1 * (pntDrawOffset.Y - recBackground.Height);
            }

            this.DrawBwShape(g, gpBackground, 0.8F, 4.0F, Color.Black, Color.White);
            this.DrawBwShape(g, gpKillPopupText, 1.0F, 4.0F, Color.Black, Color.White);

            if (imgDamageType != null) {
                g.DrawImage(imgDamageType, new PointF((recBackground.X + recBackground.Width / 2) - imgDamageType.Width / 2 + this.m_pntDrawOffset.X, recBackground.Y + this.m_pntDrawOffset.Y));
            }

            gpBackground.Dispose();
            gpKillPopupText.Dispose();
            */
        }

    }
}
