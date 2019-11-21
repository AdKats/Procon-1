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

namespace PRoCon.Controls.Battlemap.MapTimeline
{
    using Controls.Battlemap;
    using Controls.Battlemap.KillDisplay;
    using Core;

    public class MapTimelineControl : MapObject
    {

        private DateTime m_dtMouseLeftFadeout;

        private MapTimelineSeekButton m_mtsSeek;

        private float m_flControlChangeSpeed;

        protected List<MapObject> TimelineButtons
        {
            get;
            private set;
        }

        public bool IsSeekerSelected
        {
            get
            {
                return this.m_mtsSeek.IsSeekButtonSelected;
            }
        }

        public bool IsSeekerMouseOvered
        {
            get
            {
                return this.m_mtsSeek.IsMouseOver;
            }
        }

        public float SeekerPosition
        {
            get
            {
                return this.m_mtsSeek.SeekerPosition;
            }
        }

        public MapTimelineControlButtonType SelectedButtonType
        {
            get;
            private set;
        }

        public new bool IsMouseOver
        {
            get
            {
                bool blMouseOverButton = false;

                foreach (MapTimelineControlButton mtbButton in this.TimelineButtons)
                {
                    if ((blMouseOverButton = mtbButton.IsMouseOver) == true)
                    {
                        break;
                    }
                }

                return blMouseOverButton || this.m_mtsSeek.IsMouseOver;
            }
        }

        private float TimelineOpacity
        {
            get
            {
                float flOpacity = 0.0F;
                //TimeSpan tsDifference = DateTime.Now - this.m_dtMouseLeftFadeout;

                //double dblTotalMilliseconds = tsDifference.TotalMilliseconds;
                double dblTotalMilliseconds = (DateTime.Now.Ticks - this.m_dtMouseLeftFadeout.Ticks) / TimeSpan.TicksPerMillisecond;

                if (this.m_dtMouseLeftFadeout > DateTime.Now)
                {
                    dblTotalMilliseconds = Math.Abs((this.m_dtMouseLeftFadeout.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond);
                    //tsDifference = this.m_dtMouseLeftFadeout - DateTime.Now;
                    //dblTotalMilliseconds = Math.Abs(tsDifference.TotalMilliseconds);
                }

                if (dblTotalMilliseconds < 1000)
                {
                    flOpacity = 1.0F;
                }
                else if (dblTotalMilliseconds < 2000)
                {
                    flOpacity = (2000 - (float)dblTotalMilliseconds) / 2000 * 2.0F;
                }

                return flOpacity;
            }
        }

        public MapTimelineControl()
        {

            this.m_mtsSeek = new MapTimelineSeekButton();

            this.TimelineButtons = new List<MapObject>();

            GraphicsPath gpButtonPath = new GraphicsPath();
            gpButtonPath.AddLines(new Point[] { new Point(0, 6), new Point(6, 12), new Point(6, 6), new Point(12, 12), new Point(12, 0), new Point(6, 6), new Point(6, 0), new Point(0, 6) });
            gpButtonPath.CloseFigure();
            MapTimelineControlButton mtbButton = new MapTimelineControlButton(gpButtonPath, MapTimelineControlButtonType.Rewind);
            mtbButton.TimelineControlButtonClicked += new MapTimelineControlButton.TimelineControlButtonClickedHandler(mtbButton_TimelineControlButtonClicked);
            this.TimelineButtons.Add(mtbButton);

            gpButtonPath = new GraphicsPath();
            gpButtonPath.AddRectangles(new Rectangle[] { new Rectangle(0, 0, 4, 12), new Rectangle(8, 0, 4, 12) });
            mtbButton = new MapTimelineControlButton(gpButtonPath, MapTimelineControlButtonType.Pause);
            mtbButton.TimelineControlButtonClicked += new MapTimelineControlButton.TimelineControlButtonClickedHandler(mtbButton_TimelineControlButtonClicked);
            this.TimelineButtons.Add(mtbButton);

            gpButtonPath = new GraphicsPath();
            gpButtonPath.AddLines(new Point[] { new Point(1, 0), new Point(1, 12), new Point(9, 6), new Point(1, 0) });
            gpButtonPath.CloseFigure();
            mtbButton = new MapTimelineControlButton(gpButtonPath, MapTimelineControlButtonType.Play);
            mtbButton.ForegroundColour = Color.LightSeaGreen;
            mtbButton.TimelineControlButtonClicked += new MapTimelineControlButton.TimelineControlButtonClickedHandler(mtbButton_TimelineControlButtonClicked);
            this.TimelineButtons.Add(mtbButton);

            gpButtonPath = new GraphicsPath();
            gpButtonPath.AddLines(new Point[] { new Point(0, 0), new Point(0, 12), new Point(6, 6), new Point(6, 12), new Point(12, 6), new Point(6, 0), new Point(6, 6), new Point(0, 0) });
            gpButtonPath.CloseFigure();
            mtbButton = new MapTimelineControlButton(gpButtonPath, MapTimelineControlButtonType.FastForward);
            mtbButton.TimelineControlButtonClicked += new MapTimelineControlButton.TimelineControlButtonClickedHandler(mtbButton_TimelineControlButtonClicked);
            this.TimelineButtons.Add(mtbButton);

            this.SelectedButtonType = MapTimelineControlButtonType.Play;
            this.m_flControlChangeSpeed = 2.0F;
        }

        private void mtbButton_TimelineControlButtonClicked(MapTimelineControlButton sender, MapTimelineControlButtonType ButtonType)
        {

            if (this.SelectedButtonType == ButtonType && ButtonType == MapTimelineControlButtonType.FastForward)
            {
                if (this.m_flControlChangeSpeed < 512.0F)
                {
                    this.m_flControlChangeSpeed = this.m_flControlChangeSpeed * 2.0F;
                }
            }
            else if (this.SelectedButtonType == ButtonType && ButtonType == MapTimelineControlButtonType.Rewind)
            {
                if (this.m_flControlChangeSpeed < 512.0F)
                {
                    this.m_flControlChangeSpeed = this.m_flControlChangeSpeed * 2.0F;
                }
            }
            else if (ButtonType == MapTimelineControlButtonType.FastForward || ButtonType == MapTimelineControlButtonType.Rewind)
            {
                this.m_flControlChangeSpeed = 2.0F;
            }

            foreach (MapTimelineControlButton mtbButton in this.TimelineButtons)
            {
                if (mtbButton == sender)
                {
                    mtbButton.ForegroundColour = Color.LightSeaGreen;
                }
                else
                {
                    mtbButton.ForegroundColour = Color.White;
                }
            }

            this.SelectedButtonType = ButtonType;
        }

        private DateTime m_dtLastTick = DateTime.MinValue;
        public void SeekerPositionTick()
        {

            if (this.m_dtLastTick != DateTime.MinValue)
            {

                TimeSpan tsLastTick = DateTime.Now - this.m_dtLastTick;

                if (this.SelectedButtonType == MapTimelineControlButtonType.Pause)
                {
                    // Go back however much time has past since last tick.
                    this.m_mtsSeek.SeekerPosition -= (float)(tsLastTick.TotalSeconds / 3600.0D);
                }
                else if (this.SelectedButtonType == MapTimelineControlButtonType.Rewind)
                {
                    // Go back 2x however much time has past since last tick.
                    this.m_mtsSeek.SeekerPosition -= this.m_flControlChangeSpeed * (float)(tsLastTick.TotalSeconds / 3600.0D);
                }
                else if (this.SelectedButtonType == MapTimelineControlButtonType.FastForward)
                {
                    // Go forward 2x however much time has past since last tick.
                    this.m_mtsSeek.SeekerPosition += (this.m_flControlChangeSpeed / 2.0F) * (float)(tsLastTick.TotalSeconds / 3600.0D);
                }

                if (this.m_mtsSeek.SeekerPosition > 1.0F)
                {
                    this.m_mtsSeek.SeekerPosition = 1.0F;
                    this.SelectedButtonType = MapTimelineControlButtonType.Play;

                    foreach (MapTimelineControlButton mtbButton in this.TimelineButtons)
                    {
                        if (mtbButton.ButtonType == MapTimelineControlButtonType.Play)
                        {
                            mtbButton.ForegroundColour = Color.LightSeaGreen;
                        }
                        else
                        {
                            mtbButton.ForegroundColour = Color.White;
                        }
                    }
                }
                else if (this.m_mtsSeek.SeekerPosition < 0.0F)
                {
                    this.m_mtsSeek.SeekerPosition = 0.0F;
                    this.SelectedButtonType = MapTimelineControlButtonType.Play;

                    foreach (MapTimelineControlButton mtbButton in this.TimelineButtons)
                    {
                        if (mtbButton.ButtonType == MapTimelineControlButtonType.Play)
                        {
                            mtbButton.ForegroundColour = Color.LightSeaGreen;
                        }
                        else
                        {
                            mtbButton.ForegroundColour = Color.White;
                        }
                    }
                }
            }

            this.m_dtLastTick = DateTime.Now;
        }

        public void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons, Dictionary<Kill, KillDisplayDetails> dicKills, List<BattlemapRoundChange> lstRounds, KillDisplayColours colours, Dictionary<int, Color> teamColours)
        {
            GraphicsPath gpTimelineOutline = new GraphicsPath();

            gpTimelineOutline.AddLines(new Point[] { new Point(5, 0), new Point(0, 5), new Point(0, 15), new Point((int)g.ClipBounds.Width - 280, 15), new Point((int)g.ClipBounds.Width - 280, 5), new Point((int)g.ClipBounds.Width - 275, 0) });
            //gpTimelineOutline.AddLine(new Point(this.m_mtsSeek.SeekerPosition, 15), new Point((int)g.ClipBounds.Width - 280, 15));
            //gpTimelineOutline.AddLines(new Point[] { new Point(235, (int)g.ClipBounds.Height - 55), new Point(230, (int)g.ClipBounds.Height - 50), new Point(230, (int)g.ClipBounds.Height - 40), new Point((int)g.ClipBounds.Width - 50, (int)g.ClipBounds.Height - 40), new Point((int)g.ClipBounds.Width - 50, (int)g.ClipBounds.Height - 50), new Point((int)g.ClipBounds.Width - 45, (int)g.ClipBounds.Height - 55) });
            gpTimelineOutline.Widen(this.m_pOneWidth);

            this.ObjectPath = gpTimelineOutline;
            RectangleF recBounds = gpTimelineOutline.GetBounds();
            recBounds.Height += 50.0F;
            this.HotSpot = recBounds;

            //string strMouseOverKillList = String.Empty;
            float flMouseOffsetX = 0.0F;

            bool blRoundChanged = false;

            MapTextBlock timeList = new MapTextBlock();

            foreach (BattlemapRoundChange RoundChange in new List<BattlemapRoundChange>(lstRounds))
            {
                float flOffsetXs = (this.HotSpot.Width - 5.0F) - ((float)((DateTime.Now.Ticks - RoundChange.ChangeTime.Ticks) / TimeSpan.TicksPerSecond) / 3600.0F) * (this.HotSpot.Width - 5.0F);
                RectangleF recChangePosition = new RectangleF(flOffsetXs + this.m_pntDrawOffset.X - 2.0F, this.m_pntDrawOffset.Y, 4.0F, 20.0F);

                if (flOffsetXs >= 0.0F)
                {
                    GraphicsPath gpChangeTime = new GraphicsPath();
                    gpChangeTime.AddLine(new PointF(flOffsetXs, 5), new PointF(flOffsetXs, 12));
                    gpChangeTime.Widen(this.m_pOneWidth);

                    this.DrawBwShape(g, gpChangeTime, this.TimelineOpacity, 4.0F, Color.Black, Color.RoyalBlue);
                    gpChangeTime.Dispose();

                    if (recChangePosition.Contains(new PointF(pntMouseLocation.X, pntMouseLocation.Y)) == true)
                    {
                        //strMouseOverKillList += String.Format("Round change {0}\r\n", RoundChange.Map.PublicLevelName);

                        timeList.Strings.Add(new MapTextBlockString(String.Format("Round change {0}", RoundChange.Map.PublicLevelName), Color.Pink, true));

                        blRoundChanged = true;
                        flMouseOffsetX = flOffsetXs;
                        //flMouseOffsetX = flOffsetXs;
                    }
                }
            }

            foreach (KeyValuePair<Kill, KillDisplayDetails> kvpKill in new Dictionary<Kill, KillDisplayDetails>(dicKills))
            {

                float flOffsetXs = (this.HotSpot.Width - 5.0F) - ((float)((DateTime.Now.Ticks - kvpKill.Key.TimeOfDeath.Ticks) / TimeSpan.TicksPerSecond) / 3600.0F) * (this.HotSpot.Width - 5.0F);
                RectangleF recKillPosition = new RectangleF(flOffsetXs + this.m_pntDrawOffset.X - 2.0F, this.m_pntDrawOffset.Y, 4.0F, 20.0F);

                if (recKillPosition.Contains(new PointF(pntMouseLocation.X + 5.0F, pntMouseLocation.Y)) == true)
                {
                    GraphicsPath gpKillTime = new GraphicsPath();
                    gpKillTime.AddLine(new PointF(flOffsetXs, 10), new PointF(flOffsetXs, 12));
                    gpKillTime.Widen(this.m_pOneWidth);

                    this.DrawBwShape(g, gpKillTime, this.TimelineOpacity, 4.0F, Color.Black, Color.RoyalBlue);
                    gpKillTime.Dispose();

                    Color killerColour = Color.White;
                    Color victimColour = Color.White;

                    if (colours == KillDisplayColours.EnemyColours)
                    {
                        killerColour = ControlPaint.Light(Color.SeaGreen);
                        victimColour = ControlPaint.LightLight(Color.Black);
                    }
                    else if (colours == KillDisplayColours.TeamColours)
                    {
                        if (teamColours.ContainsKey(kvpKill.Key.Killer.TeamID) == true && teamColours.ContainsKey(kvpKill.Key.Victim.TeamID) == true)
                        {
                            killerColour = ControlPaint.Light(teamColours[kvpKill.Key.Killer.TeamID]);
                            victimColour = ControlPaint.Light(teamColours[kvpKill.Key.Victim.TeamID]);
                        }
                    }

                    if (kvpKill.Key.Killer.ClanTag.Length > 0)
                    {
                        timeList.Strings.Add(new MapTextBlockString(String.Format("[{0}] ", kvpKill.Key.Killer.ClanTag), killerColour, false));
                    }

                    timeList.Strings.Add(new MapTextBlockString(kvpKill.Key.Killer.SoldierName, killerColour, false));

                    timeList.Strings.Add(new MapTextBlockString(String.Format("[{0}] ", kvpKill.Key.DamageType), Color.WhiteSmoke, false));

                    if (kvpKill.Key.Victim.ClanTag.Length > 0)
                    {
                        timeList.Strings.Add(new MapTextBlockString(String.Format("[{0}] ", kvpKill.Key.Victim.ClanTag), victimColour, false));
                    }
                    timeList.Strings.Add(new MapTextBlockString(kvpKill.Key.Victim.SoldierName, victimColour, true));

                    flMouseOffsetX = flOffsetXs;
                }
            }

            if (timeList.Strings.Count > 0)
            {

                RectangleF recText = timeList.GetBounds();

                PointF timeListOffset = new PointF(pntDrawOffset.X + flMouseOffsetX - recText.Width / 2.0F, pntDrawOffset.Y - recText.Height);

                if (timeListOffset.X + recText.Width > g.ClipBounds.Width)
                {
                    timeListOffset.X = g.ClipBounds.Width - recText.Width;
                }

                timeList.Draw(g, timeListOffset, pntMouseLocation, mbButtons);
            }

            base.Draw(g, pntDrawOffset, pntMouseLocation, mbButtons);

            this.m_mtsSeek.ButtonOpacity = this.TimelineOpacity;
            this.m_mtsSeek.SeekerBounds = recBounds;
            this.m_mtsSeek.Draw(g, new PointF(pntDrawOffset.X, pntDrawOffset.Y + 13.0F), pntMouseLocation, mbButtons);

            timeList.Dispose();
        }

        protected override void MouseOver(Graphics g)
        {

            if (this.m_dtMouseLeftFadeout.AddSeconds(1.4D) < DateTime.Now)
            {
                this.m_dtMouseLeftFadeout = DateTime.Now.AddSeconds(1.4D);
            }
            else
            {
                this.m_dtMouseLeftFadeout = DateTime.Now;
            }

            this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
            this.DrawButtons(g);
        }

        protected override void MouseLeave(Graphics g)
        {
            this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
            this.DrawButtons(g);
        }

        protected override void MouseDown(Graphics g)
        {
            if (base.IsMouseOver == true)
            {
                this.MouseOver(g);
            }
            else
            {
                this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
                this.DrawButtons(g);
            }
        }

        protected override void MouseUp(Graphics g)
        {
            this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
        }

        protected override void MouseClicked(Graphics g)
        {
            this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
            this.DrawButtons(g);
        }

        protected override void NormalPaint(Graphics g)
        {
            if (this.IsSeekerSelected == true)
            {
                this.m_dtMouseLeftFadeout = DateTime.Now;
            }

            this.DrawBwShape(g, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
            this.DrawButtons(g);
        }

        private void DrawButtons(Graphics g)
        {
            int iTimelineControlsWidth = this.TimelineButtons.Count * 20;
            int iTimelineOffsetX = (int)this.m_pntDrawOffset.X + (int)this.HotSpot.Width / 2 - iTimelineControlsWidth / 2;

            foreach (MapTimelineControlButton mtbButton in this.TimelineButtons)
            {
                mtbButton.ButtonOpacity = this.TimelineOpacity;
                mtbButton.Draw(g, new Point(iTimelineOffsetX, (int)g.ClipBounds.Height - 28), this.m_pntMousePosition, this.m_MouseButtons);
                iTimelineOffsetX += 20;
            }

            GraphicsPath gpPlaySpeed = new GraphicsPath();
            string strPlaySpeedText = String.Empty;

            if (this.SelectedButtonType == MapTimelineControlButtonType.Pause)
            {
                strPlaySpeedText = "0.0x";
            }
            else if (this.SelectedButtonType == MapTimelineControlButtonType.Play)
            {
                strPlaySpeedText = "1.0x";
            }
            else if (this.SelectedButtonType == MapTimelineControlButtonType.Rewind)
            {
                strPlaySpeedText = String.Format("-{0:0.0}x", this.m_flControlChangeSpeed);
            }
            else if (this.SelectedButtonType == MapTimelineControlButtonType.FastForward)
            {
                strPlaySpeedText = String.Format("{0:0.0}x", this.m_flControlChangeSpeed);
            }

            if (strPlaySpeedText.Length > 0)
            {
                gpPlaySpeed.AddString(strPlaySpeedText, new FontFamily("Arial"), 0, 12, new Point(iTimelineOffsetX - (int)this.m_pntDrawOffset.X, 26), StringFormat.GenericTypographic);
                this.DrawBwShape(g, gpPlaySpeed, this.TimelineOpacity, 4.0F, Color.Black, Color.White);
            }

            gpPlaySpeed.Dispose();
        }
    }
}
