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
using System.Globalization;
using System.IO;

namespace PRoCon.Controls.Battlemap.MapImagePacks
{
    using Core;

    public class MapImagePack
    {
        public delegate void MapLoadedHandler();
        public event MapLoadedHandler MapLoaded;

        public CLocalization MapImagePackDataFile
        {
            get;
            private set;
        }

        public string MapImagePackPath
        {
            get;
            private set;
        }

        public Point MapOrigin
        {
            get;
            set;
        }

        public PointF MapScale
        {
            get;
            set;
        }

        /// <summary>
        /// Defaults to 2048 for backwards compatability.  This value can be left out (MapFileName.PixelResolution)
        /// or included if a map is larger than 2048px.
        /// </summary>
        public int MapPixelResolution
        {
            get;
            private set;
        }

        public bool Readonly
        {
            get;
            private set;
        }

        private float m_flMapRotation;
        public float MapRotation
        {
            get
            {
                return this.m_flMapRotation;
            }
            set
            {
                this.m_flMapRotation = value;

                //if (this.DeathIconImage != null) {
                //    this.DeathIconImage = this.LoadImage("Death");
                //    this.DeathIconImage = this.CompensateImageRotation(this.DeathIconImage);
                //}

                /*
                Dictionary<string, Image> dicCompensatedIcons = new Dictionary<string, Image>();

                foreach (KeyValuePair<string, Image> kvpIcon in this.m_dicLoadedIcons) {
                    if (dicCompensatedIcons.ContainsKey(kvpIcon.Key) == false) {
                        dicCompensatedIcons.Add(kvpIcon.Key, this.CompensateImageRotation(this.LoadImage(kvpIcon.Key)));
                    }

                    kvpIcon.Value.Dispose();
                }

                this.m_dicLoadedIcons = dicCompensatedIcons;
                */
            }
        }

        public Image MapImage
        {
            get;
            private set;
        }

        //public Image DeathIconImage {
        //    get;
        //    private set;
        //}

        public string LoadedMapFileName
        {
            get;
            private set;
        }

        private Dictionary<string, Image> m_dicLoadedIcons;

        public MapImagePack(string strMapImagePackPath, CLocalization clocMapImagePack)
        {
            this.m_dicLoadedIcons = new Dictionary<string, Image>();

            this.MapImagePackPath = strMapImagePackPath;
            this.MapImagePackDataFile = clocMapImagePack;
            this.LoadedMapFileName = String.Empty;

            this.MapOrigin = new Point(0, 0);
            this.MapScale = new PointF(1.0F, 1.0F);
            this.MapRotation = 0.0F;
            this.MapPixelResolution = 2048;

            //this.DeathIconImage = this.LoadImage("Death");
        }

        public Image GetIcon(string strImageKey)
        {

            Image returnIcon = null;

            if (this.m_dicLoadedIcons.ContainsKey(strImageKey) == true)
            {
                returnIcon = this.m_dicLoadedIcons[strImageKey];
            }
            else
            {
                // Create an entry, even if it is null.
                returnIcon = this.LoadImage(strImageKey, "Image");
                this.m_dicLoadedIcons.Add(strImageKey, returnIcon);
            }

            return returnIcon;
        }

        private Image LoadImage(string strImageKey, string type)
        {

            Image imgReturn = null;

            string strImagePath = Path.Combine(this.MapImagePackPath, MapImagePackDataFile.GetLocalized(String.Format("{0}.{1}", strImageKey, type)));
            if (File.Exists(strImagePath) == true)
            {
                imgReturn = new Bitmap(strImagePath);
            }
            else
            {
                imgReturn = null;
            }

            return imgReturn;
        }

        public void UnloadMapImage()
        {

            Image mapImage = this.MapImage;

            this.MapImage = new Bitmap(1, 1);

            if (mapImage != null)
            {
                mapImage.Dispose();
                mapImage = null;
                GC.Collect();
            }
        }

        public void LoadMap(string strMapFileName, bool loadImage)
        {

            int iOriginX = 0, iOriginY = 0, pixelResolution = 2048;
            float flScaleX = 1.0F, flScaleY = 1.0F, flRotation = 0.0F;
            bool isReadonly;
            // bool isLegacyUpgrade;

            bool.TryParse(this.MapImagePackDataFile.GetLocalized("file.readonly"), out isReadonly);
            int.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.Translate.X", strMapFileName.ToLower())), NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out iOriginX);
            int.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.Translate.Y", strMapFileName.ToLower())), NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out iOriginY);
            if (float.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.ScaleX", strMapFileName.ToLower())), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out flScaleX) == false)
            {
                flScaleX = 1.0F;
            }
            if (float.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.ScaleY", strMapFileName.ToLower())), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out flScaleY) == false)
            {
                flScaleY = 1.0F;
            }

            // REMOVE AFTER LEGACY UPDATE
            // bool.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.Legacy", strMapFileName.ToLower())), out isLegacyUpgrade);
            // END REMOVE AFTER LEGACY UPDATE

            float.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.Rotation", strMapFileName.ToLower())), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out flRotation);

            if (int.TryParse(this.MapImagePackDataFile.GetLocalized(String.Format("{0}.PixelResolution", strMapFileName.ToLower())), NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out pixelResolution) == true)
            {
                this.MapPixelResolution = pixelResolution;
            }
            // else keep default of 2048
            else
            {
                this.MapPixelResolution = 2048;
            }

            this.Readonly = isReadonly;
            this.MapOrigin = new Point(iOriginX, iOriginY);
            this.MapScale = new PointF(flScaleX, flScaleY);
            this.MapRotation = flRotation;

            /*
            string strMapImagePath = Path.Combine(this.MapImagePackPath, MapImagePackDataFile.GetLocalized(String.Format("{0}.Image", strMapFileName.ToLower())));
            if (File.Exists(strMapImagePath) == true) {
                this.MapImage = new Bitmap(strMapImagePath);
            }
            else {
                this.MapImage = null;
            }
            */
            //if (this.MapImage != null) {
            //    this.MapImage.Dispose();
            //    this.MapImage = null;
            //}

            this.UnloadMapImage();

            if (loadImage == true)
            {
                this.MapImage = this.LoadImage(strMapFileName.ToLower(), "Image");

                Image overlay = this.LoadImage(strMapFileName.ToLower(), "Overlay");

                if (overlay != null)
                {
                    Bitmap newMapImage = new Bitmap(this.MapImage);

                    Graphics g = Graphics.FromImage(newMapImage);
                    //g.DrawImage(this.MapImage, new Point(0, 0));
                    g.DrawImage(overlay, new Point(0, 0));

                    overlay.Dispose();
                    overlay = null;
                    this.MapImage.Dispose();

                    this.MapImage = newMapImage;
                }
            }
            //else {
            //    this.MapImage = new Bitmap(1, 1);
            //}

            this.LoadedMapFileName = strMapFileName;

            // REMOVE AFTER LEGACY UPDATE

            //if (isLegacyUpgrade == true) {

            //    if (this.MapImage.Width <= 2048 && this.MapImage.Height <= 2048) {

            //        float zoomRatio = Math.Max(this.MapImage.Width, this.MapImage.Height) / 2048.0F;

            //        this.MapScale = new PointF(flScaleX * zoomRatio, flScaleY * zoomRatio);

            //        this.MapOrigin = new Point((int)((float)iOriginX * zoomRatio), (int)((float)iOriginY * zoomRatio));
            //    }
            //}

            // END REMOVE AFTER LEGACY UPDATE

            if (this.MapLoaded != null)
            {
                this.MapLoaded();
            }
        }

        public Image CompensateImageRotation(Image imgSource)
        {
            Bitmap bmRotatedImage = null;

            if (imgSource != null)
            {
                bmRotatedImage = new Bitmap(imgSource.Width, imgSource.Height);
                Graphics g = Graphics.FromImage(bmRotatedImage);
                Matrix m = new Matrix();
                m.RotateAt(-this.MapRotation, new PointF(imgSource.Width / 2, imgSource.Height / 2));
                g.Transform = m;

                g.DrawImage(imgSource, 0, 0);
                g.Dispose();
            }

            return bmRotatedImage;
        }

        public override string ToString()
        {
            string strText = String.Empty;

            if (this.MapImagePackDataFile != null)
            {
                if (this.MapImagePackDataFile.LocalizedExists("file.name") == true)
                {
                    strText = this.MapImagePackDataFile.GetLocalized("file.name");
                }
                else
                {
                    strText = Path.Combine(this.MapImagePackDataFile.FilePath, this.MapImagePackDataFile.FileName);
                }
            }

            return strText;
        }
    }
}
