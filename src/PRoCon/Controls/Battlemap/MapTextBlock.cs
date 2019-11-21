using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PRoCon.Controls.Battlemap
{
    public class MapTextBlock : IDisposable
    {

        public float HorizontalSpacing
        {
            get
            {
                return 5.0F;
            }
        }

        public float VerticalSpacing
        {
            get
            {
                return 5.0F;
            }
        }

        public List<MapTextBlockString> Strings
        {
            get;
            private set;
        }

        public MapTextBlock()
        {
            this.Strings = new List<MapTextBlockString>();
        }

        public void Draw(Graphics g, PointF pntDrawOffset, Point pntMouseLocation, MouseButtons mbButtons)
        {

            PointF BlockStringDrawOffset = new PointF(pntDrawOffset.X, pntDrawOffset.Y);

            foreach (MapTextBlockString blockString in this.Strings)
            {
                blockString.Draw(g, BlockStringDrawOffset, pntMouseLocation, mbButtons);

                if (blockString.NewLine == true)
                {
                    BlockStringDrawOffset.X = pntDrawOffset.X;
                    BlockStringDrawOffset.Y += blockString.HotSpot.Height + VerticalSpacing;
                }
                else
                {
                    BlockStringDrawOffset.X += blockString.HotSpot.Width + HorizontalSpacing;
                }
            }

        }

        public RectangleF GetBounds()
        {

            RectangleF returnRec = new RectangleF();

            float lineWidth = 0.0F;
            float totalHeight = 0.0F;

            foreach (MapTextBlockString blockString in this.Strings)
            {

                lineWidth += blockString.HotSpot.Width + HorizontalSpacing;

                if (blockString.NewLine == true)
                {
                    returnRec.Width = Math.Max(returnRec.Width, lineWidth);
                    lineWidth = 0;

                    totalHeight += blockString.HotSpot.Height + VerticalSpacing;
                }
                else if (totalHeight == 0.0F)
                {
                    totalHeight = blockString.HotSpot.Height + VerticalSpacing;
                }
            }

            returnRec.Height = totalHeight;

            return returnRec;
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (MapTextBlockString blockString in this.Strings)
            {
                blockString.Dispose();
            }
        }

        #endregion
    }
}
