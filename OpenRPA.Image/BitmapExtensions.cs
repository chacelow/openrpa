using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenRPA.Image
{
    public static class BitmapExtensions
    {
        public static Image<TColor, TDepth> ToImage<TColor, TDepth>(this Bitmap bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var mat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 3, data.Scan0, data.Stride);
                var img = mat.ToImage<TColor, TDepth>();
                return img;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
