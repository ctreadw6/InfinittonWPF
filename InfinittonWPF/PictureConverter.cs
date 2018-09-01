using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace InfinittonWPF
{
    public  class PictureConverter
    {
        public static byte[] Header1 =
        {
            0x02, 0x00, 0x00, 0x00, 0x00, 0x40, 0x1F, 0x00, 0x00, 0x55, 0xAA, 0xAA, 0x55, 0x11, 0x22, 0x33,
            0x44, 0x42, 0x4D, 0xF6, 0x3C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28,
            0x00, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00,
            0x00, 0x00, 0x00, 0xC0, 0x3C, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        public static byte[] Header2 =
        {
            0x02, 0x40, 0x1F, 0x00, 0x00, 0xB6, 0x1D, 0x00, 0x00, 0x55, 0xAA, 0xAA, 0x55, 0x11, 0x22, 0x33, 0x44
        };

        public static Bitmap Superimpose(Bitmap smallBmp, Color backgroundColor)
        {
            if (smallBmp == null)
            {
                var largeBmp = GetSolidBitmap(256,256, backgroundColor);
                var finalImage = new Bitmap(largeBmp.Width, largeBmp.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(finalImage);
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(largeBmp, 0, 0, largeBmp.Width, largeBmp.Height);
                return finalImage;
            }
            else
            {
                var largeBmp = GetSolidBitmap(smallBmp.Width, smallBmp.Height, backgroundColor);
                var finalImage = new Bitmap(largeBmp.Width, largeBmp.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(finalImage);
                g.CompositingMode = CompositingMode.SourceOver;
                smallBmp.MakeTransparent();
                g.DrawImage(largeBmp, 0, 0, largeBmp.Width, largeBmp.Height);
                g.DrawImage(smallBmp, 0, 0, smallBmp.Width, smallBmp.Height);
                return finalImage;
            }
        }

        public static Image GetSolidBitmap(int width, int height, Color color)
        {
            Bitmap Bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(Bmp))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(color.R, color.G, color.B)))
            {
                gfx.FillRectangle(brush, 0, 0, width, height);
                gfx.Flush();
            }

            return Bmp;
        }
        
        public static byte[] GetBuffer(IButtonPressAction action)
        {
            Image image = action.Icon;
            string title = action.Title;

            if (image == null)
            {
                byte[] buf = new byte[8017*2];
                Array.Clear(buf, 0, buf.Length);
                Header1.CopyTo(buf, 0);
                Header2.CopyTo(buf, 8017);
                return buf;
            }

            Bitmap bmp = ResizeImage(image, 72, 72);
            RectangleF rectf = new RectangleF(5, 50, 62, 15);

            if (!string.IsNullOrEmpty(title) && action.ShowTitleLabel)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawString(title, new Font(action.Titlefont.Family?.Source, (float)action.Titlefont.Size), 
                        new SolidBrush(Color.FromArgb(action.Titlefont.Color.Brush.Color.R, action.Titlefont.Color.Brush.Color.G, action.Titlefont.Color.Brush.Color.B)), rectf);

                    g.Flush();
                }
            }

            

            List<byte> imageBuf = new List<byte>(72 * 72 * 3);

            for (int i = 0; i < 72 * 72; i++)
            {
                System.Drawing.Color color = bmp.GetPixel(i / 72, i % 72);

                imageBuf.Add(color.B);
                imageBuf.Add(color.G);
                imageBuf.Add(color.R);

            }

            List<byte> bytes = Header1.Concat(imageBuf.Take(8017 - Header1.Length)).Concat(Header2).Concat(imageBuf.Skip(8017 - Header1.Length)).ToList();
            while (bytes.Count() < 8017 * 2) bytes.Add(0);
            return bytes.ToArray();
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
