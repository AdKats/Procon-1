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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PRoCon.Controls
{
    using Controls.Battlemap;
    using Controls.Battlemap.KillDisplay;
    using Controls.Battlemap.MapImagePacks;
    using Controls.Battlemap.MapTimeline;
    using Controls.Battlemap.MapZoneDisplay;
    using Controls.Battlemap.TeamNamesDisplay;
    using Core;
    using Core.Battlemap;

    public partial class BattlemapView : UserControl
    {

        //public static readonly Size MAP_SIZE = new Size(2048, 2048);

        public delegate void CreateMapZoneHandler(string mapFileName, Point3D[] zonePoints);
        public event CreateMapZoneHandler CreateMapZone;

        public delegate void DeleteMapZoneHandler(string strUid);
        public event DeleteMapZoneHandler DeleteMapZone;

        public delegate void ModifyMapZoneHandler(string strUid, Point3D[] zonePoints);
        public event ModifyMapZoneHandler ModifyMapZone;

        public delegate void MapZoneSelectedHandler(MapZoneDrawing zone);
        public event MapZoneSelectedHandler MapZoneSelected;

        // Dragging / Measuring
        private bool m_isBeingDragged;
        private PointF m_pntStart;

        // Measuring
        private bool m_isMeasuringNow;
        private PointF m_pntEnd;

        // Shown
        private PointF m_pntOrigin = new System.Drawing.PointF(0.0F, 0.0F);
        private RectangleF m_recSource;
        private RectangleF m_recDestination;

        // Font
        private FontFamily family = new FontFamily("Arial");
        private int fontStyle = (int)FontStyle.Regular;
        private int emSize = 12;
        private StringFormat format = StringFormat.GenericDefault;

        //private Size m_ApparentImageSize = new Size(0, 0);

        private int m_DrawWidth;

        private int m_DrawHeight;

        private PointF m_centerpoint;

        private bool m_blCheckingBounds = false;

        private readonly object objKillDictionaryLocker = new object();
        private Dictionary<Kill, KillDisplayDetails> m_dicKills;
        private List<BattlemapRoundChange> m_lstRoundChanges;

        public bool FullyLoadMap { get; set; }

        public List<Kill> CalibrationMarkers
        {
            get;
            private set;
        }

        public bool DisplayCalibrationGrid { get; set; }

        public delegate void KillColoursChangedHandler(KillDisplayColours newKillColour);
        public event KillColoursChangedHandler KillColoursChanged;
        private KillDisplayColours m_KillColours;
        public KillDisplayColours KillColours
        {
            get
            {
                return this.m_KillColours;
            }
            set
            {
                if (this.m_KillColours != value)
                {
                    this.m_KillColours = value;

                    if (this.KillColoursChanged != null)
                    {
                        this.KillColoursChanged(value);
                    }
                }
            }
        }
        //public Dictionary<Kill, float> Kills {
        //    get;
        //    set;
        //}

        /*
        private string GetImageFilePath(string strImagePackKey) {
            string strReturnPath = String.Empty;

            if (this.m_mipMapImagePack != null && this.m_mipMapImagePack.MapImagePackDataFile.LocalizedExists(strImagePackKey) == true) {
                strReturnPath = Path.Combine(this.m_mipMapImagePack.MapImagePackPath, this.m_mipMapImagePack.MapImagePackDataFile.GetLocalized(strImagePackKey));
            }

            return strReturnPath;
        }
        */
        public void AddKill(Kill kill)
        {
            lock (this.objKillDictionaryLocker)
            {
                if (this.LoadedMapImagePack != null && this.m_dicKills.ContainsKey(kill) == false)
                {

                    this.m_dicKills.Add(kill, new KillDisplayDetails(kill.TimeOfDeath));
                }
            }
        }

        public void AddRoundChange(CMap map)
        {
            this.m_lstRoundChanges.Add(new BattlemapRoundChange(map));
        }

        private MapImagePack m_mipMapImagePack;
        private string m_strInitialMap;
        public MapImagePack LoadedMapImagePack
        {
            get
            {
                return this.m_mipMapImagePack;
            }
            set
            {
                if (this.m_mipMapImagePack == null && value != null)
                {
                    this.m_strInitialMap = value.LoadedMapFileName;
                }

                this.m_mipMapImagePack = value;

                if (this.m_mipMapImagePack != null)
                {
                    this.m_mipMapImagePack.MapLoaded += new MapImagePack.MapLoadedHandler(m_mipMapImagePack_MapLoaded);
                }
                // Like this, later this week I will make it wipe the current kill details and reload all displayed images.
            }
        }

        private void m_mipMapImagePack_MapLoaded()
        {
            //this.Map = this.m_mipMapImagePack.MapImage;

            this.m_recDestination = new System.Drawing.RectangleF(0, 0, ClientSize.Width, ClientSize.Height);

            this.m_centerpoint.X = (this.m_pntOrigin.X + (this.m_mipMapImagePack.MapImage.Width / 2));
            this.m_centerpoint.Y = (this.m_pntOrigin.Y + (this.m_mipMapImagePack.MapImage.Height / 2));

            this.CheckBounds();
            this.Invalidate();
        }

        public int ErrorRadius
        {
            get;
            set;
        }

        // In milliseconds
        public int KillEventFadeout
        {
            get;
            set;
        }

        /*
        // Background map
        private Bitmap m_bitSourceImage;
        private Image Map {
            get {
                return this.m_bitSourceImage;
            }
            set {
                if (this.m_bitSourceImage != null) {
                    this.m_bitSourceImage.Dispose();
                    this.m_bitSourceImage = null;
                    GC.Collect();
                }

                if (value == null) {
                    this.m_bitSourceImage = null;
                    this.Invalidate();
                }
                else {
                    // Resize the image so it is 2048x2048 by default.
                    //Size newImageSize = new Size();

                    //if (this.LoadedMapImagePack != null) {
                    //    newImageSize.Width = (int)(((double)this.LoadedMapImagePack.MapPixelResolution / Math.Max((double)value.Height, (double)value.Width)) * (double)value.Width);
                    //    newImageSize.Height = (int)(((double)this.LoadedMapImagePack.MapPixelResolution / Math.Max((double)value.Height, (double)value.Width)) * (double)value.Height);
                    //}
                    //else {
                    //    newImageSize.Width = (int)((2048.0D / Math.Max((double)value.Height, (double)value.Width)) * (double)value.Width);
                    //    newImageSize.Height = (int)((2048.0D / Math.Max((double)value.Height, (double)value.Width)) * (double)value.Height);
                    //}
                    
                    this.m_bitSourceImage = new Bitmap(value);//, newImageSize);
                    this.m_recDestination = new System.Drawing.RectangleF(0, 0, ClientSize.Width, ClientSize.Height);

                    this.m_centerpoint.X = (this.m_pntOrigin.X + (this.m_bitSourceImage.Width / 2));
                    this.m_centerpoint.Y = (this.m_pntOrigin.Y + (this.m_bitSourceImage.Height / 2));

                    this.CheckBounds();
                    this.Invalidate();
                }
            }
        }
        */
        private float m_flZoomFactor = 1;
        public float ZoomFactor
        {
            get
            {
                return m_flZoomFactor;
            }
            set
            {
                this.m_flZoomFactor = value;

                if (this.m_flZoomFactor > 15.0F)
                {
                    this.m_flZoomFactor = 15.0F;
                }

                if (this.m_flZoomFactor < 0.05F)
                {
                    this.m_flZoomFactor = 0.05F;
                }

                if (this.m_mipMapImagePack != null && this.m_mipMapImagePack.MapImage != null)
                {
                    //this.m_ApparentImageSize.Height = (int)(this.m_bitSourceImage.Height * this.m_flZoomFactor);
                    //this.m_ApparentImageSize.Width = (int)(this.m_bitSourceImage.Width * this.m_flZoomFactor);
                    this.ComputeDrawingArea();
                    this.CheckBounds();
                }

                this.Invalidate();
            }
        }

        public BattlemapViewTools SelectedTool
        {
            get;
            set;
        }

        private MapTimelineControl Timeline
        {
            get;
            set;
        }

        private Dictionary<string, MapZoneControl> MapZoneControls
        {
            get;
            set;
        }

        private Dictionary<string, TeamNamesDisplayLegend> m_dicTeamLegends;
        public List<CMap> MapDetails
        {
            set
            {
                this.m_dicTeamLegends.Clear();

                foreach (CMap map in value)
                {
                    if (this.m_dicTeamLegends.ContainsKey(map.FileName.ToLower()) == false)
                    {
                        this.m_dicTeamLegends.Add(map.PlayList.ToLower() + map.FileName.ToLower() + KillDisplayColours.TeamColours.ToString(), new TeamNamesDisplayLegend(KillDisplayColours.TeamColours, this.m_dicTeamColours, this.m_clocLanguage, map));
                        this.m_dicTeamLegends.Add(map.PlayList.ToLower() + map.FileName.ToLower() + KillDisplayColours.EnemyColours.ToString(), new TeamNamesDisplayLegend(KillDisplayColours.EnemyColours, this.m_dicTeamColours, this.m_clocLanguage, map));
                    }
                }
            }
        }

        private readonly Pen m_pOneWidth = new Pen(Brushes.Black, 1.0F);
        private readonly Pen m_pTwoWidth = new Pen(Brushes.Black, 2.0F);

        private Dictionary<int, Color> m_dicTeamColours = new Dictionary<int, Color>() { { 0, Color.WhiteSmoke }, { 1, Color.RoyalBlue }, { 2, Color.OrangeRed }, { 3, Color.YellowGreen }, { 4, Color.LightSeaGreen } };

        public BattlemapView()
        {

            // TODO change these to value types
            this.m_recSource = new RectangleF(0.0F, 0.0F, 0.0F, 0.0F);
            this.m_recDestination = new RectangleF(0.0F, 0.0F, 0.0F, 0.0F);

            this.m_dicTeamLegends = new Dictionary<string, TeamNamesDisplayLegend>();

            //this.MapOrigin = new Point(0, 0);
            //this.MapScale = 1.0F;
            //this.MapRotation = 0.0F;

            this.KillColours = KillDisplayColours.EnemyColours;

            this.ErrorRadius = 14;
            this.KillEventFadeout = 5000;
            this.SelectedTool = BattlemapViewTools.Pointer;
            this.m_isBeingDragged = false;

            this.m_dicKills = new Dictionary<Kill, KillDisplayDetails>();
            this.m_lstRoundChanges = new List<BattlemapRoundChange>();
            this.CalibrationMarkers = new List<Kill>();
            this.m_pntEnd = new PointF(0.0F, 0.0F);

            this.MouseMove += new MouseEventHandler(ImageViewer_MouseMove);
            this.MouseWheel += new MouseEventHandler(ImageViewer_MouseWheel);
            this.MouseDown += new MouseEventHandler(ImageViewer_MouseDown);
            this.MouseUp += new MouseEventHandler(DrawingBoard_MouseUp);
            this.Resize += new EventHandler(DrawingBoard_Resize);

            this.Timeline = new MapTimelineControl();
            this.MapZoneControls = new Dictionary<string, MapZoneControl>();

            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
        }

        public CLocalization m_clocLanguage;

        public void SetLocalization(CLocalization clocLanguage)
        {
            if ((this.m_clocLanguage = clocLanguage) != null)
            {
                // Anything?
            }
        }

        private void DrawCalibrationGrid(Graphics g)
        {
            Pen calibrationAxis = new Pen(Brushes.Purple, 0.5F);
            Pen calibrationGrid = new Pen(Brushes.Purple, 0.2F);

            g.DrawLine(calibrationAxis, new Point(0, 0), new Point(0, 500));
            g.DrawLine(calibrationAxis, new Point(0, 0), new Point(500, 0));

            g.DrawString("X", this.Font, Brushes.HotPink, new Point(5, -15));
            g.DrawString("Y", this.Font, Brushes.HotPink, new Point(-15, 5));
            g.DrawString("(0,0)", this.Font, Brushes.HotPink, new Point(-25, -15));

            for (int x = 10; x < 300; x = x + 10)
            {
                g.DrawLine(calibrationGrid, new Point(x, 0), new Point(x, 500));
            }

            for (int y = 10; y < 300; y = y + 10)
            {
                g.DrawLine(calibrationGrid, new Point(0, y), new Point(500, y));
            }

            calibrationGrid.Dispose();
            calibrationAxis.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            //g.Clear(SystemColors.Window);

            //Point mousePosition = Cursor.Position;
            MouseButtons mouseButtonsPressed = MouseButtons;
            if (this.Focused == false)
            {
                //mousePosition = new Point(int.MaxValue, int.MaxValue);
                mouseButtonsPressed = MouseButtons.None;
            }

            if (this.LoadedMapImagePack != null)
            {

                if (this.m_mipMapImagePack.MapImage != null)
                {

                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    this.m_recSource = new System.Drawing.RectangleF(this.m_pntOrigin.X, this.m_pntOrigin.Y, this.m_DrawWidth, this.m_DrawHeight);

                    g.DrawImage(this.m_mipMapImagePack.MapImage, this.m_recDestination, this.m_recSource, GraphicsUnit.Pixel);
                }

                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                Matrix mOriginal = g.Transform;

                Matrix m = new Matrix();

                m.Scale(this.ZoomFactor, this.ZoomFactor);
                m.Translate(-m_pntOrigin.X, -m_pntOrigin.Y);

                m.Translate(this.LoadedMapImagePack.MapOrigin.X, this.LoadedMapImagePack.MapOrigin.Y);
                m.Scale(this.LoadedMapImagePack.MapScale.X, this.LoadedMapImagePack.MapScale.Y);
                m.Rotate(this.LoadedMapImagePack.MapRotation);

                try
                {
                    g.Transform = m;
                }
                catch (ArgumentException)
                {

                }

                Kill kMouseOveredKill = null;
                KillDisplayDetails kddDisplayDetails = null;

                foreach (MapZoneControl zone in new List<MapZoneControl>(this.MapZoneControls.Values))
                {
                    if (String.Compare(this.LoadedMapImagePack.LoadedMapFileName, zone.ZoneDetails.LevelFileName, true) == 0)
                    {
                        zone.Draw(g, new Point(0, 0), this.ClientPointToGame(this.PointToClient(Cursor.Position)), mouseButtonsPressed, this.SelectedTool);
                    }
                }

                lock (this.objKillDictionaryLocker)
                {

                    if (this.DisplayCalibrationGrid == true)
                    {
                        this.DrawCalibrationGrid(g);
                    }

                    Pen calibrationOutline = new Pen(Brushes.Pink, 1.0F);

                    foreach (Kill kKill in this.CalibrationMarkers)
                    {

                        g.DrawEllipse(calibrationOutline, new Rectangle(kKill.KillerLocation.X - this.ErrorRadius, kKill.KillerLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
                        g.DrawEllipse(calibrationOutline, new Rectangle(kKill.VictimLocation.X - this.ErrorRadius, kKill.VictimLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
                    }

                    calibrationOutline.Dispose();

                    foreach (KeyValuePair<Kill, KillDisplayDetails> kvpKill in this.m_dicKills)
                    {

                        if (kvpKill.Value.Opacity > 0.0F)
                        {
                            this.DrawKillCircles(g, kvpKill.Key, kvpKill.Value);

                            if (kvpKill.Value.IsMouseOver == true)
                            {
                                kMouseOveredKill = kvpKill.Key;
                                kddDisplayDetails = kvpKill.Value;
                            }
                        }
                    }
                }

                try
                {
                    g.Transform = mOriginal;
                }
                catch (ArgumentException)
                {

                }

                this.DrawAdditionalDetails(g, kMouseOveredKill, kddDisplayDetails);
                this.DrawScale(g);
                this.DrawCoordinates(g);

                if (this.m_isBeingDragged == false)
                {
                    this.Timeline.Draw(g, new Point(235, (int)g.ClipBounds.Height - 55), this.PointToClient(Cursor.Position), mouseButtonsPressed, this.m_dicKills, this.m_lstRoundChanges, this.KillColours, this.m_dicTeamColours);
                }
                else
                {
                    this.Timeline.Draw(g, new Point(235, (int)g.ClipBounds.Height - 55), new Point(0, 0), mouseButtonsPressed, this.m_dicKills, this.m_lstRoundChanges, this.KillColours, this.m_dicTeamColours);
                }

                if (this.m_dicTeamLegends.ContainsKey(this.m_mipMapImagePack.LoadedMapFileName.ToLower() + this.m_KillColours.ToString()) == true)
                {

                    TeamNamesDisplayLegend teamLegend = this.m_dicTeamLegends[this.m_mipMapImagePack.LoadedMapFileName.ToLower() + this.m_KillColours.ToString()];
                    RectangleF teamLegendBounds = teamLegend.GetBounds();

                    teamLegend.Draw(g, new PointF(10.0F, g.ClipBounds.Height - teamLegendBounds.Height - 70.0F), new Point(0, 0), mouseButtonsPressed);
                }

                if (this.SelectedTool == BattlemapViewTools.Measuring && this.m_isMeasuringNow == true)
                {
                    this.DrawMeasuringResults(g);
                }
            }

            base.OnPaint(e);
        }

        private void DrawAdditionalDetails(Graphics g, Kill kMouseOveredKill, KillDisplayDetails kddDisplayDetails)
        {
            if (kMouseOveredKill != null && kddDisplayDetails != null)
            {

                KillDisplayPopup kdp = new KillDisplayPopup();

                string strDamageType = String.Empty;
                if (this.m_clocLanguage.TryGetLocalized(out strDamageType, "global.Weapons." + kMouseOveredKill.DamageType.ToLower()) == false)
                {
                    strDamageType = kMouseOveredKill.DamageType;
                }

                kdp.Draw(g, this.PointToClient(Cursor.Position), this.PointToClient(Cursor.Position), MouseButtons, kMouseOveredKill, kddDisplayDetails, this.LoadedMapImagePack.GetIcon(kMouseOveredKill.DamageType.ToLower()), strDamageType, this.KillColours, this.m_dicTeamColours);
            }
        }

        private void DrawBwShape(Graphics g, GraphicsPath gp, float flOpacity)
        {

            Pen pen = new Pen(Color.FromArgb((int)(255.0F * flOpacity), Color.Black), 4);
            pen.LineJoin = LineJoin.Round;
            g.DrawPath(pen, gp);
            SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255.0F * flOpacity), Color.White));
            g.FillPath(brush, gp);

            brush.Dispose();
            pen.Dispose();
            gp.Dispose();
        }

        protected void DrawBwShape(Graphics g, GraphicsPath gp, float flOpacity, float flOutlineWidth, Color clBackground, Color clForecolour)
        {
            Pen pen = new Pen(Color.FromArgb((int)(255.0F * flOpacity), clBackground), flOutlineWidth);
            pen.LineJoin = LineJoin.Round;
            g.DrawPath(pen, gp);
            SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255.0F * flOpacity), clForecolour));
            g.FillPath(brush, gp);

            brush.Dispose();
            pen.Dispose();
            gp.Dispose();
        }


        private void DrawCoordinates(Graphics g)
        {
            // Move to class so they are not declared every paint.
            /* FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Regular;
            int emSize = 12;
            StringFormat format = StringFormat.GenericDefault;
            */
            GraphicsPath gpCoordinates = new GraphicsPath();

            Point pntCoords = this.ClientPointToGame(this.PointToClient(Cursor.Position));

            gpCoordinates.AddString(String.Format("{0}, {1}", pntCoords.X, pntCoords.Y), this.family, this.fontStyle, this.emSize, new Point((int)g.ClipBounds.Width - 75, (int)g.ClipBounds.Height - 20), this.format);
            this.DrawBwShape(g, gpCoordinates, 1.0F);

            gpCoordinates.Dispose();
        }

        private void DrawMeasuringResults(Graphics g)
        {

            if (this.LoadedMapImagePack != null)
            {

                // Move to class so they are not declared every paint.
                /* FontFamily family = new FontFamily("Arial");
                int fontStyle = (int)FontStyle.Regular;
                int emSize = 12;
                StringFormat format = StringFormat.GenericDefault;
                */
                //double dblMetrePixels = this.ZoomFactor;

                if (this.LoadedMapImagePack.MapScale.X != 0.0F && this.LoadedMapImagePack.MapScale.Y != 0.0F && this.ZoomFactor != 0.0F && (this.m_pntStart.X != this.m_pntEnd.X || this.m_pntStart.Y != this.m_pntEnd.Y))
                {
                    double dx = (this.m_pntStart.X - this.m_pntEnd.X) / (this.LoadedMapImagePack.MapScale.X * this.ZoomFactor);
                    double dy = (this.m_pntStart.Y - this.m_pntEnd.Y) / (this.LoadedMapImagePack.MapScale.Y * this.ZoomFactor);
                    double dblMetresDistance = Math.Sqrt(dx * dx + dy * dy);
                    //double dblMetresDistance = dblPixelDistance / dblMetrePixels;

                    GraphicsPath gpMeasuringResultsLine = new GraphicsPath();
                    gpMeasuringResultsLine.AddLine(this.m_pntStart, this.m_pntEnd);
                    gpMeasuringResultsLine.Widen(this.m_pOneWidth);
                    this.DrawBwShape(g, gpMeasuringResultsLine, 1.0F);
                    gpMeasuringResultsLine.Dispose();

                    GraphicsPath gpMeasuringResults = new GraphicsPath();
                    gpMeasuringResults.AddString(String.Format("{0:0.0} m\n{1:0.0} yd", dblMetresDistance, dblMetresDistance * 1.0936133D), this.family, this.fontStyle, this.emSize, new PointF(this.m_pntEnd.X, this.m_pntEnd.Y - 25), this.format);
                    this.DrawBwShape(g, gpMeasuringResults, 1.0F);
                    gpMeasuringResults.Dispose();
                }
                //family.Dispose();
            }
        }

        private void DrawScale(Graphics g)
        {

            if (this.LoadedMapImagePack != null)
            {

                // Move to class so they are not declared every paint.
                /* FontFamily family = new FontFamily("Arial");
                int fontStyle = (int)FontStyle.Regular;
                int emSize = 12;
                StringFormat format = StringFormat.GenericDefault;
                */
                GraphicsPath gpScale = new GraphicsPath();
                GraphicsPath gpScaleUnits = new GraphicsPath();

                gpScale.AddLine(new Point(10, (int)g.ClipBounds.Height - 50), new Point(10, (int)g.ClipBounds.Height - 30));
                gpScale.AddLine(new Point(10, (int)g.ClipBounds.Height - 40), new Point(200, (int)g.ClipBounds.Height - 40));
                gpScale.Widen(this.m_pOneWidth);

                gpScaleUnits.AddString("m", this.family, this.fontStyle, this.emSize, new Point(4, (int)g.ClipBounds.Height - 65), this.format);
                gpScaleUnits.AddString("yd", this.family, this.fontStyle, this.emSize, new Point(1, (int)g.ClipBounds.Height - 30), this.format);

                // Only interested in horizontal
                double dblMetrePixels = this.LoadedMapImagePack.MapScale.X * this.ZoomFactor;

                for (double i = 0.0D; i < 16.0D; i++)
                {

                    double dblMetres = Math.Pow(2, i);

                    // Metres

                    int iOffset = (int)Math.Round(dblMetres * dblMetrePixels);

                    if (iOffset >= 15 && 10 + iOffset <= 200)
                    {

                        GraphicsPath gpScaleMarkerLine = new GraphicsPath();
                        GraphicsPath gpScaleMarker = new GraphicsPath();

                        gpScaleMarker.AddString(String.Format("{0:0}", dblMetres), this.family, this.fontStyle, this.emSize, new Point(4 + iOffset, (int)g.ClipBounds.Height - 60), this.format);
                        gpScaleMarkerLine.AddLine(new Point(10 + iOffset, (int)g.ClipBounds.Height - 45), new Point(10 + iOffset, (int)g.ClipBounds.Height - 40));
                        gpScaleMarkerLine.Widen(this.m_pOneWidth);

                        this.DrawBwShape(g, gpScaleMarkerLine, 1.0F);
                        this.DrawBwShape(g, gpScaleMarker, 1.0F);

                        gpScaleMarkerLine.Dispose();
                        gpScaleMarker.Dispose();
                    }

                    // Yards

                    iOffset = (int)Math.Round(dblMetres * (dblMetrePixels / 1.0936133D));

                    if (iOffset >= 15 && 10 + iOffset <= 200)
                    {

                        GraphicsPath gpScaleMarkerLine = new GraphicsPath();
                        GraphicsPath gpScaleMarker = new GraphicsPath();

                        gpScaleMarker.AddString(String.Format("{0:0}", dblMetres), this.family, this.fontStyle, this.emSize, new Point(4 + iOffset, (int)g.ClipBounds.Height - 32), this.format);
                        gpScaleMarkerLine.AddLine(new Point(10 + iOffset, (int)g.ClipBounds.Height - 35), new Point(10 + iOffset, (int)g.ClipBounds.Height - 40));
                        gpScaleMarkerLine.Widen(this.m_pOneWidth);

                        this.DrawBwShape(g, gpScaleMarkerLine, 1.0F);
                        this.DrawBwShape(g, gpScaleMarker, 1.0F);

                        gpScaleMarkerLine.Dispose();
                        gpScaleMarker.Dispose();
                    }
                }

                //gpScale.AddLine(new Point(10, (int)g.ClipBounds.Height - 20), new Point(10, (int)g.ClipBounds.Height - 10));
                //gpScale.AddLine(new Point(10, (int)g.ClipBounds.Height - 10), new Point(60, (int)g.ClipBounds.Height - 10));
                //gpScale.AddLine(new Point(10, (int)g.ClipBounds.Height - 20), new Point(60, (int)g.ClipBounds.Height - 20));

                this.DrawBwShape(g, gpScale, 1.0F);
                this.DrawBwShape(g, gpScaleUnits, 1.0F);

                gpScale.Dispose();
                gpScaleUnits.Dispose();

                //g.DrawLine(Pens.White, new Point(10, (int)g.ClipBounds.Height - 10), new Point(60, (int)g.ClipBounds.Height - 10));
            }
        }

        // Move to KillDisplayKillCircles : MapObject once completed.
        private LinearGradientBrush GetKillColour(KillDisplayColours colours, Kill kKill, KillDisplayDetails killDetails)
        {

            LinearGradientBrush returnBrush = null;

            double angle = Math.Atan2((double)kKill.KillerLocation.X - (double)kKill.VictimLocation.X, (double)kKill.KillerLocation.Y - (double)kKill.VictimLocation.Y);
            double adjext = Math.Cos(angle) * 14.4F;
            double oppext = Math.Sin(angle) * 14.4F;

            PointF pntStart = new PointF((float)kKill.KillerLocation.X + (float)oppext, (float)kKill.KillerLocation.Y + (float)adjext);
            PointF pntEnd = new PointF((float)kKill.VictimLocation.X - (float)oppext, (float)kKill.VictimLocation.Y - (float)adjext);

            if (colours == KillDisplayColours.EnemyColours)
            {
                returnBrush = new LinearGradientBrush(pntStart, pntEnd, Color.FromArgb((int)(192.0F * killDetails.Opacity), Color.LightSeaGreen), Color.FromArgb((int)(192.0F * killDetails.Opacity), Color.Red));
            }
            else if (colours == KillDisplayColours.TeamColours)
            {
                if (this.m_dicTeamColours.ContainsKey(kKill.Killer.TeamID) == true && this.m_dicTeamColours.ContainsKey(kKill.Victim.TeamID) == true)
                {
                    returnBrush = new LinearGradientBrush(pntStart, pntEnd, Color.FromArgb((int)(192.0F * killDetails.Opacity), this.m_dicTeamColours[kKill.Killer.TeamID]), Color.FromArgb((int)(192.0F * killDetails.Opacity), this.m_dicTeamColours[kKill.Victim.TeamID]));
                }
            }

            return returnBrush;
        }

        // While still in design this function draws directly to the component.
        // Once design is complete it will paint to a image, then the image painted to the component for a little speed boost.
        private void DrawKillCircles(Graphics g, Kill kKill, KillDisplayDetails kddKillDetails)
        {

            PointF pntLineStart = new PointF((float)kKill.KillerLocation.X, (float)kKill.KillerLocation.Y);
            PointF pntLineEnd = new PointF((float)kKill.VictimLocation.X, (float)kKill.VictimLocation.Y);
            PointF pntLineHalfway = new PointF(pntLineStart.X - (pntLineStart.X - pntLineEnd.X) / 2, pntLineStart.Y - (pntLineStart.Y - pntLineEnd.Y) / 2 - 3);
            PointF pntLineHalfway2 = new PointF(pntLineStart.X - (pntLineStart.X - pntLineEnd.X) / 2, pntLineStart.Y - (pntLineStart.Y - pntLineEnd.Y) / 2 - 4);

            LinearGradientBrush killBrush = this.GetKillColour(this.KillColours, kKill, kddKillDetails);

            GraphicsPath gpKillCircles = new GraphicsPath();
            gpKillCircles.AddEllipse(new Rectangle(kKill.KillerLocation.X - this.ErrorRadius, kKill.KillerLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
            gpKillCircles.AddEllipse(new Rectangle(kKill.VictimLocation.X - this.ErrorRadius, kKill.VictimLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
            gpKillCircles.FillMode = FillMode.Winding;

            //GraphicsPath gpKill = new GraphicsPath();
            GraphicsPath gpKill = (GraphicsPath)gpKillCircles.Clone();
            gpKill.AddClosedCurve(new PointF[] { pntLineStart, pntLineHalfway, pntLineEnd, pntLineHalfway2 });
            //gpKill.AddEllipse(new Rectangle(kKill.KillerLocation.X - this.ErrorRadius, kKill.KillerLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
            //gpKill.AddEllipse(new Rectangle(kKill.VictimLocation.X - this.ErrorRadius, kKill.VictimLocation.Y - this.ErrorRadius, this.ErrorRadius * 2, this.ErrorRadius * 2));
            gpKill.FillMode = FillMode.Winding;

            GraphicsPath gpKillOutline = (GraphicsPath)gpKill.Clone();
            //GraphicsPath gpKillOutline = new GraphicsPath(gpKill.PathPoints, gpKill.PathTypes);
            Matrix gpKillMatrix = new Matrix();
            gpKillOutline.Widen(this.m_pTwoWidth, gpKillMatrix, 0.01F);

            Region reKillOutline = new Region(gpKillOutline);
            reKillOutline.Exclude(gpKill);
            reKillOutline.Exclude(gpKillCircles);

            Region reKill = new Region(gpKill);
            reKill.Union(gpKillCircles);

            //Region reKillDropshadow = new Region(gpKill);
            //reKillDropshadow.Union(gpKillCircles);
            //reKillDropshadow.Union(reKillOutline);
            //reKillDropshadow.Translate(0.4F, 1.0F);

            if (reKill.IsVisible(this.ClientPointToGame(this.PointToClient(Cursor.Position))) == true)
            {
                kddKillDetails.IsMouseOver = true;
                kddKillDetails.Opacity = 1.0F;
            }
            else
            {
                kddKillDetails.IsMouseOver = false;
            }

            //g.FillRegion(new SolidBrush(Color.FromArgb((int)(64.0F * kddKillDetails.Opacity), Color.Black)), reKillDropshadow);
            g.FillRegion(killBrush, reKill);
            g.FillRegion(new SolidBrush(Color.FromArgb((int)(255.0F * kddKillDetails.Opacity), Color.Black)), reKillOutline);

            if (this.LoadedMapImagePack != null)
            {

                Image imgDeathIcon = null;
                if (kKill.Headshot == true)
                {
                    imgDeathIcon = this.LoadedMapImagePack.CompensateImageRotation(this.LoadedMapImagePack.GetIcon("Headshot"));
                }
                else
                {
                    imgDeathIcon = this.LoadedMapImagePack.CompensateImageRotation(this.LoadedMapImagePack.GetIcon("Death"));
                }

                if (imgDeathIcon != null)
                {
                    ColorMatrix colormatrix = new ColorMatrix();
                    colormatrix.Matrix00 = 1.0F;
                    colormatrix.Matrix11 = 1.0F;
                    colormatrix.Matrix22 = 1.0F;
                    colormatrix.Matrix33 = kddKillDetails.Opacity;
                    colormatrix.Matrix44 = 1.0F;
                    ImageAttributes imgattr = new ImageAttributes();
                    imgattr.SetColorMatrix(colormatrix);

                    Rectangle destRect = new Rectangle((int)pntLineEnd.X - 12, (int)pntLineEnd.Y - 12, 24, 24);
                    g.DrawImage(imgDeathIcon, destRect, 0, 0, imgDeathIcon.Width, imgDeathIcon.Height, GraphicsUnit.Pixel, imgattr);

                    imgattr.Dispose();
                    imgDeathIcon.Dispose();
                }
            }

            this.DrawMapText(g, kKill.Victim.SoldierName, kKill.VictimLocation, 16, kddKillDetails.Opacity);
            this.DrawMapText(g, kKill.Killer.SoldierName, kKill.KillerLocation, 16, kddKillDetails.Opacity);

            killBrush.Dispose();
            gpKillCircles.Dispose();
            gpKill.Dispose();
            gpKillOutline.Dispose();
            gpKillMatrix.Dispose();
            reKill.Dispose();
        }

        private void DrawText(Graphics g, string strText, Point pntLocation, int iSize, float flOpacity)
        {
            GraphicsPath gpKillerName = new GraphicsPath();
            gpKillerName.AddString(strText, this.family, this.fontStyle, iSize, new PointF(pntLocation.X, pntLocation.Y), this.format);
            this.DrawBwShape(g, gpKillerName, flOpacity);
            gpKillerName.Dispose();
        }

        //private void DrawMapText(Graphics g, string strText, Point3D pntLocation, int iSize, float flOpacity, float flScaleChange) {

        //}

        private void DrawMapText(Graphics g, string strText, Point3D pntLocation, int iSize, float flOpacity)
        {
            GraphicsPath gpKillerName = new GraphicsPath();

            float textZoom = this.ZoomFactor * ((this.LoadedMapImagePack.MapScale.X + this.LoadedMapImagePack.MapScale.Y) / 2.0F);

            Font fontText = new Font("Arial", Math.Max(Math.Min(iSize / textZoom, iSize), 1.0F), FontStyle.Regular);
            SizeF sText = g.MeasureString(strText, fontText);

            float outlineWidth = Math.Min(4.0F / textZoom, 4.0F);
            gpKillerName.AddString(strText, fontText.FontFamily, (int)fontText.Style, fontText.Size, new PointF((float)pntLocation.X, (float)pntLocation.Y), StringFormat.GenericDefault);

            Matrix m = new Matrix();
            m.RotateAt(-this.LoadedMapImagePack.MapRotation, new PointF((float)pntLocation.X, (float)pntLocation.Y));
            m.Translate(gpKillerName.GetBounds().Width / -2.0F + outlineWidth / -2.0F, gpKillerName.GetBounds().Height / -2.0F + outlineWidth / -2.0F);
            gpKillerName.Transform(m);

            this.DrawBwShape(g, gpKillerName, flOpacity, outlineWidth, Color.Black, Color.WhiteSmoke);

            fontText.Dispose();
            m.Dispose();
            gpKillerName.Dispose();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.m_recDestination = new RectangleF(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            this.CheckBounds();
            this.ComputeDrawingArea();
            base.OnSizeChanged(e);
        }

        private void ProgressiveZoomImage(bool ZoomIn)
        {
            if (ZoomIn == true)
            {
                this.ZoomImage((float)Math.Round((this.ZoomFactor * 1.18), 2));
            }
            else
            {
                this.ZoomImage((float)Math.Round((this.ZoomFactor * 0.82), 2));
            }
        }

        private void ZoomImage(float flNewZoomFactor)
        {

            // Phogue, code to allow zooming while maintaining a target.
            // Where ever the cursor is 
            Point pntCursorPoint = this.PointToClient(Cursor.Position);
            float flCursorOffsetX = (float)((pntCursorPoint.X - this.ClientSize.Width / 2) / this.m_flZoomFactor);
            float flCursorOffsetY = (float)((pntCursorPoint.Y - this.ClientSize.Height / 2) / this.m_flZoomFactor);

            // Set new zoomfactor
            this.ZoomFactor = flNewZoomFactor;

            this.m_centerpoint.X -= (float)((pntCursorPoint.X - this.ClientSize.Width / 2) / this.m_flZoomFactor) - flCursorOffsetX;
            this.m_centerpoint.Y -= (float)((pntCursorPoint.Y - this.ClientSize.Height / 2) / this.m_flZoomFactor) - flCursorOffsetY;

            // Reset the origin to maintain center point
            this.m_pntOrigin.X = (this.m_centerpoint.X - (float)(this.ClientSize.Width / this.m_flZoomFactor / 2));
            this.m_pntOrigin.Y = (this.m_centerpoint.Y - (float)(this.ClientSize.Height / this.m_flZoomFactor / 2));

            this.CheckBounds();
        }

        public void FitOnScreen()
        {

            this.m_pntOrigin = new PointF(0, 0);

            if (this.m_mipMapImagePack.MapImage != null)
            {
                float flZoomFactor = (float)Math.Min((double)this.ClientSize.Width / (double)this.m_mipMapImagePack.MapImage.Width, (double)this.ClientSize.Height / (double)this.m_mipMapImagePack.MapImage.Height);

                this.ZoomFactor = flZoomFactor;

                this.m_centerpoint.X = (this.m_pntOrigin.X + (this.m_mipMapImagePack.MapImage.Width / 2));
                this.m_centerpoint.Y = (this.m_pntOrigin.Y + (this.m_mipMapImagePack.MapImage.Height / 2));
            }
        }

        private Point ClientPointToZoom(Point pntClient)
        {
            Point pntReturn = new Point(pntClient.X, pntClient.Y);

            pntReturn.X = (int)(this.m_pntOrigin.X + (float)pntReturn.X / this.ZoomFactor);
            pntReturn.Y = (int)(this.m_pntOrigin.Y + (float)pntReturn.Y / this.ZoomFactor);

            return pntReturn;
        }

        private Point ClientPointToGame(Point pntClient)
        {
            Point[] pntReturn = new Point[] { this.ClientPointToZoom(pntClient) };

            if (this.LoadedMapImagePack != null)
            {
                Matrix m = new Matrix();

                pntReturn[0].X = (int)((float)pntReturn[0].X - this.LoadedMapImagePack.MapOrigin.X);
                pntReturn[0].Y = (int)((float)pntReturn[0].Y - this.LoadedMapImagePack.MapOrigin.Y);

                pntReturn[0].X = (int)((float)pntReturn[0].X / this.LoadedMapImagePack.MapScale.X);
                pntReturn[0].Y = (int)((float)pntReturn[0].Y / this.LoadedMapImagePack.MapScale.Y);

                m.Rotate(360.0F - this.LoadedMapImagePack.MapRotation);

                m.TransformPoints(pntReturn);
                m.Dispose();
            }
            else
            {
                pntReturn[0].X = 0;
                pntReturn[0].Y = 0;
            }

            return pntReturn[0];
        }

        private Rectangle RecFtoRec(RectangleF recOriginal)
        {
            return new Rectangle((int)recOriginal.X, (int)recOriginal.Y, (int)recOriginal.Width, (int)recOriginal.Height);
        }

        private void ComputeDrawingArea()
        {
            this.m_DrawHeight = (int)(this.ClientSize.Height / this.m_flZoomFactor);
            this.m_DrawWidth = (int)(this.ClientSize.Width / this.m_flZoomFactor);
        }

        private bool IsMouseOverZone()
        {
            bool isMouseOverZone = false;

            foreach (KeyValuePair<string, MapZoneControl> mapZone in this.MapZoneControls)
            {

                if (mapZone.Value.IsMapZonePointSelected == true || mapZone.Value.IsMapZonePointMouseOvered == true || mapZone.Value.IsMouseOver == true)
                {
                    isMouseOverZone = true;
                    break;
                }
            }

            return isMouseOverZone;
        }

        private void ImageViewer_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (this.m_mipMapImagePack != null && this.m_mipMapImagePack.MapImage != null)
            {

                if (this.SelectedTool == BattlemapViewTools.Pointer && e.Button == MouseButtons.Left ||
                    ((this.SelectedTool == BattlemapViewTools.Measuring || this.SelectedTool == BattlemapViewTools.Zones) && e.Button == MouseButtons.Right))
                {
                    this.Cursor = Cursor.Current;

                    if (this.Timeline.IsMouseOver == false)
                    {
                        this.m_isBeingDragged = true;
                    }

                    this.m_pntStart = new PointF(e.X, e.Y);
                    this.Focus();
                }

                if (this.SelectedTool == BattlemapViewTools.Pointer && e.Button == MouseButtons.Right)
                {
                    this.Cursor = Cursor.Current;

                    this.m_pntStart = new PointF(e.X, e.Y);
                    this.Focus();

                    this.ctxPointer.Show(this, e.X, e.Y);
                }

                if (this.SelectedTool == BattlemapViewTools.Zones && e.Button == MouseButtons.Left)
                {

                    bool isMouseOverZone = this.IsMouseOverZone();

                    if (this.Timeline.IsMouseOver == false && isMouseOverZone == false)
                    {
                        this.m_isBeingDragged = true;

                        if (this.CreateMapZone != null && this.Focused == true)
                        {
                            //MapZoneControl newZone = new MapZoneControl(this.ClientPointToGame(new Point(e.X, e.Y)));

                            this.CreateMapZone(this.LoadedMapImagePack.LoadedMapFileName, MapZoneControl.CreateInitialZone(this.ClientPointToGame(new Point(e.X, e.Y))));
                        }


                        // newZone.MapZoneSelected += new MapZoneControl.MapZoneSelectedHandler(newZone_MapZoneSelected);
                        // this.MapZoneControls.Add(newZone);
                    }
                }

                if (this.SelectedTool == BattlemapViewTools.Zones && e.Button == MouseButtons.Right)
                {
                    if (this.IsMouseOverZone() == true)
                    {
                        this.ctxZones.Show(this, e.X, e.Y);
                    }
                }

                if (this.SelectedTool == BattlemapViewTools.Measuring && e.Button == MouseButtons.Left)
                {
                    this.Cursor = Cursor.Current;

                    this.m_isMeasuringNow = true;
                    this.m_pntStart = new PointF(e.X, e.Y);
                    this.m_pntEnd = new PointF(e.X, e.Y);
                }

            }
        }

        private void ImageViewer_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (this.m_mipMapImagePack != null && this.m_mipMapImagePack.MapImage != null)
            {

                if (this.SelectedTool == BattlemapViewTools.Pointer && e.Button == MouseButtons.Left ||
                    ((this.SelectedTool == BattlemapViewTools.Measuring || this.SelectedTool == BattlemapViewTools.Zones) && e.Button == MouseButtons.Right))
                {

                    // If buttons not selected..
                    if (this.Timeline.IsMouseOver == false && this.Timeline.IsSeekerSelected == false)
                    {
                        float DeltaX = this.m_pntStart.X - e.X;
                        float DeltaY = this.m_pntStart.Y - e.Y;

                        // Set the origin of the new image
                        this.m_pntOrigin.X = (this.m_pntOrigin.X + (float)(DeltaX / this.ZoomFactor));
                        this.m_pntOrigin.Y = (this.m_pntOrigin.Y + (float)(DeltaY / this.ZoomFactor));

                        // Phogue, centrepoint should not be variable in a zoom.  Moved to here.
                        this.m_centerpoint.X = (this.m_pntOrigin.X + (this.m_recSource.Width / 2));
                        this.m_centerpoint.Y = (this.m_pntOrigin.Y + (this.m_recSource.Height / 2));

                        this.CheckBounds();

                        // reset the startpoints
                        this.m_pntStart.X = e.X;
                        this.m_pntStart.Y = e.Y;

                        // Force a paint
                        this.Invalidate();
                    }
                    else if (this.Timeline.IsSeekerSelected == true)
                    {
                        this.Invalidate();
                    }
                }

                if (this.SelectedTool == BattlemapViewTools.Measuring && e.Button == MouseButtons.Left)
                {
                    this.m_isMeasuringNow = true;

                    this.m_pntEnd.X = e.X;
                    this.m_pntEnd.Y = e.Y;

                    this.Invalidate();
                }

                if (this.SelectedTool == BattlemapViewTools.Zones && e.Button == MouseButtons.Left)
                {

                    this.Invalidate();
                }

                if (this.Timeline.IsSeekerMouseOvered == true)
                {
                    this.Cursor = Cursors.Hand;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void CheckBounds()
        {
            if (this.m_mipMapImagePack != null && this.m_mipMapImagePack.MapImage != null && this.m_blCheckingBounds == false)
            {

                // Make sure we don't go out of bounds
                if (this.m_pntOrigin.X < 0)
                {
                    this.m_pntOrigin.X = 0;
                }

                if (m_pntOrigin.Y < 0)
                {
                    this.m_pntOrigin.Y = 0;
                }

                // Alter the zoom if they have zoomed out to far.
                if (this.ZoomFactor < (float)ClientSize.Height / (float)this.m_mipMapImagePack.MapImage.Height && this.ZoomFactor < (float)ClientSize.Width / (float)this.m_mipMapImagePack.MapImage.Width)
                {
                    this.m_blCheckingBounds = true;
                    this.FitOnScreen();
                    this.m_blCheckingBounds = false;
                }

                // Centers the image when smaller than view window
                if (this.m_pntOrigin.X == 0 && this.m_pntOrigin.X > this.m_mipMapImagePack.MapImage.Width - (this.ClientSize.Width / this.m_flZoomFactor))
                {
                    this.m_pntOrigin.X = (float)(this.m_mipMapImagePack.MapImage.Width - (this.ClientSize.Width / this.m_flZoomFactor)) / 2;
                }
                // Stop the image from leaving bottom right corner
                else if (this.m_pntOrigin.X + (this.ClientSize.Width / this.m_flZoomFactor) > this.m_mipMapImagePack.MapImage.Width)
                {
                    this.m_pntOrigin.X = (float)(this.m_mipMapImagePack.MapImage.Width - (this.ClientSize.Width / this.m_flZoomFactor)); //Stick to bottom right 
                }

                //if (this.m_pntOrigin.Y > this.m_bitSourceImage.Height - (this.ClientSize.Height / this.m_flZoomFactor)) {
                //    this.m_pntOrigin.Y = (this.m_bitSourceImage.Height - (float)(this.ClientSize.Height / this.m_flZoomFactor));
                //}



                // Centers the image when smaller than view window
                if (this.m_pntOrigin.Y == 0 && this.m_pntOrigin.Y > this.m_mipMapImagePack.MapImage.Height - (this.ClientSize.Height / this.m_flZoomFactor))
                {
                    this.m_pntOrigin.Y = (float)(this.m_mipMapImagePack.MapImage.Height - (this.ClientSize.Height / this.m_flZoomFactor)) / 2;
                }
                // Stop the image from leaving bottom right corner
                else if (this.m_pntOrigin.Y + (this.ClientSize.Height / this.m_flZoomFactor) > this.m_mipMapImagePack.MapImage.Height)
                {
                    this.m_pntOrigin.Y = (float)(this.m_mipMapImagePack.MapImage.Height - (this.ClientSize.Height / this.m_flZoomFactor)); //Stick to bottom right 
                }
            }
        }

        private void DrawingBoard_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.m_mipMapImagePack.MapImage != null)
            {
                this.Cursor = Cursor.Current;

                this.m_isBeingDragged = false;

                if (this.SelectedTool == BattlemapViewTools.Measuring)
                {
                    this.m_isMeasuringNow = false;

                    this.m_pntStart.X = e.X;
                    this.m_pntStart.Y = e.Y;

                    this.m_pntEnd.X = e.X;
                    this.m_pntEnd.Y = e.Y;
                }
            }
        }

        private void ImageViewer_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // set new zoomfactor
            //if (this.SelectedTool == BattlemapViewTools.Pointer) {
            if (e.Delta > 0)
            {
                this.ProgressiveZoomImage(true);
            }
            else if (e.Delta < 0)
            {
                this.ProgressiveZoomImage(false);
            }
            //}
        }

        private void DrawingBoard_Resize(object sender, System.EventArgs e)
        {
            this.ComputeDrawingArea();
        }

        //int iNextKill = 0, iTimeCount = 0;

        private void tmrKillFadeout_Tick(object sender, EventArgs e)
        {

            lock (this.objKillDictionaryLocker)
            {
                List<Kill> lstKills = new List<Kill>(this.m_dicKills.Keys);

                this.Timeline.SeekerPositionTick();

                DateTime dtCurrent = DateTime.Now.AddSeconds((this.Timeline.SeekerPosition - 1.0F) * 3600);

                string strCurrentMap = this.m_strInitialMap;

                for (int i = 0; i < this.m_lstRoundChanges.Count; i++)
                {
                    if (dtCurrent.Ticks >= this.m_lstRoundChanges[i].ChangeTime.Ticks)
                    {
                        strCurrentMap = this.m_lstRoundChanges[i].Map.FileName;
                    }
                }

                // If the current map is incorrect at this time.
                if (this.m_mipMapImagePack != null && String.Compare(this.m_mipMapImagePack.LoadedMapFileName, strCurrentMap, true) != 0)
                {
                    this.m_mipMapImagePack.LoadMap(strCurrentMap, this.FullyLoadMap);
                    //this.Map = this.m_mipMapImagePack.MapImage;
                }

                for (int i = 0; i < lstKills.Count; i++)
                {
                    if (this.m_dicKills[lstKills[i]].IsMouseOver == false)
                    {
                        TimeSpan tsDifference = dtCurrent - this.m_dicKills[lstKills[i]].TimeOfFadeoutStart;

                        if (tsDifference.TotalMilliseconds > 0.0F)
                        {
                            this.m_dicKills[lstKills[i]].Opacity = (this.KillEventFadeout - (float)tsDifference.TotalMilliseconds) / this.KillEventFadeout;
                        }
                        else
                        {
                            this.m_dicKills[lstKills[i]].TimeOfFadeoutStart = lstKills[i].TimeOfDeath;
                            this.m_dicKills[lstKills[i]].Opacity = 0.0F;
                        }
                        //if (this.m_dicKills[lstKills[i]].Opacity < 0) {
                        //    this.m_dicKills.Remove(lstKills[i]);
                        //}
                    }
                    else
                    {
                        this.m_dicKills[lstKills[i]].TimeOfFadeoutStart = dtCurrent;//DateTime.Now;
                    }

                    if (DateTime.Now.AddHours(-1.0D) > this.m_dicKills[lstKills[i]].TimeOfFadeoutStart)
                    {
                        this.m_dicKills.Remove(lstKills[i]);
                    }

                }


                this.Invalidate();
            }


        }

        private void fitOnScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.FitOnScreen();
        }

        private void actualPixelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ZoomImage(1.0F);
        }

        private void OverheadMapView_MouseUp(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        private void OverheadMapView_MouseDown(object sender, MouseEventArgs e)
        {
            this.Invalidate();
        }

        #region Map Zone Manipulation

        private void BattlemapView_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.SelectedTool == BattlemapViewTools.Zones)
            {

                foreach (MapZoneControl mapZone in new List<MapZoneControl>(this.MapZoneControls.Values))
                {
                    if (mapZone.IsSelected == true)
                    {

                        if (e.KeyCode == Keys.Delete)
                        {

                            if (this.DeleteMapZone != null)
                            {
                                this.DeleteMapZone(mapZone.ZoneDetails.UID);
                            }
                        }

                        break;
                    }
                }

            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (MapZoneControl mapZone in new List<MapZoneControl>(this.MapZoneControls.Values))
            {
                if (mapZone.IsSelected == true)
                {

                    if (this.DeleteMapZone != null)
                    {
                        this.DeleteMapZone(mapZone.ZoneDetails.UID);
                    }

                    // Throw event.

                    //this.MapZoneControls.Remove(mapZone);
                    break;
                }
            }
        }

        public void AddMapZone(MapZoneDrawing zone)
        {

            if (this.MapZoneControls.ContainsKey(zone.UID) == false)
            {
                MapZoneControl newZone = new MapZoneControl(zone);

                newZone.MapZoneSelected += new MapZoneControl.MapZoneHandler(newZone_MapZoneSelected);
                newZone.MapZoneModified += new MapZoneControl.MapZoneHandler(newZone_MapZoneModified);
                this.MapZoneControls.Add(zone.UID, newZone);
            }
            else
            {
                this.MapZoneControls[zone.UID].SetZonePoints(zone.ZonePolygon);
                this.MapZoneControls[zone.UID].ZoneDetails = zone;
            }
        }

        public void SetMapZoneTags(MapZoneDrawing zone)
        {
            if (this.MapZoneControls.ContainsKey(zone.UID) == true)
            {
                this.MapZoneControls[zone.UID].SetZoneTags(zone.Tags);
                //this.MapZoneControls[zone.UID].ZoneDetails.ZonePolygon = zone.ZonePolygon;
                //this.MapZoneControls[zone.UID].ZoneDetails = zone;
            }
        }

        public void SetMapZonePoints(MapZoneDrawing zone)
        {
            if (this.MapZoneControls.ContainsKey(zone.UID) == true)
            {
                this.MapZoneControls[zone.UID].SetZonePoints(zone.ZonePolygon);
            }
        }

        public void RemoveMapZone(MapZoneDrawing zone)
        {

            if (this.MapZoneControls.ContainsKey(zone.UID) == true)
            {

                this.MapZoneControls[zone.UID].MapZoneSelected -= new MapZoneControl.MapZoneHandler(newZone_MapZoneSelected);
                this.MapZoneControls[zone.UID].MapZoneModified -= new MapZoneControl.MapZoneHandler(newZone_MapZoneModified);

                if (this.MapZoneControls[zone.UID].IsSelected == true && this.MapZoneSelected != null)
                {
                    this.MapZoneSelected(null);
                }

                this.MapZoneControls.Remove(zone.UID);
            }
        }

        private void newZone_MapZoneModified(MapZoneControl sender)
        {
            if (this.ModifyMapZone != null)
            {
                this.ModifyMapZone(sender.ZoneDetails.UID, sender.ToPoint3DArray());
            }
        }

        private void newZone_MapZoneSelected(MapZoneControl sender)
        {
            foreach (KeyValuePair<string, MapZoneControl> mapZone in this.MapZoneControls)
            {
                if (mapZone.Value != sender)
                {
                    mapZone.Value.IsSelected = false;
                }
            }

            if (this.MapZoneSelected != null)
            {
                this.MapZoneSelected(sender.ZoneDetails);
            }
        }

        #endregion
    }

}
