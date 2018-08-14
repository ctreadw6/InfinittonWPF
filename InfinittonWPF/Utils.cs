using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace InfinittonWPF
{
    public static class Utils
    {
        public static Bitmap GetBitmap(BitmapSource source)
        {
            if (source == null) return null;
            Bitmap bmp = null;

            source.Dispatcher.Invoke((Action)delegate
            {
                bmp = new Bitmap
                (
                  source.PixelWidth,
                  source.PixelHeight,
                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                );
            

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);
            });

            return bmp;
        }

        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bitmapSource;
        }
    }
}
