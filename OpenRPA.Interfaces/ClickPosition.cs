using System;
using System.Drawing;

namespace OpenRPA.Interfaces
{
    public static class ClickPosition
    {
        public static bool TryCreateRatios(Rectangle rectangle, int offsetX, int offsetY, out double clickRatioX, out double clickRatioY)
        {
            clickRatioX = 0;
            clickRatioY = 0;
            if (rectangle.Width <= 0 || rectangle.Height <= 0) return false;

            clickRatioX = ToRatio(offsetX, rectangle.Width);
            clickRatioY = ToRatio(offsetY, rectangle.Height);
            return true;
        }

        public static double ToRatio(int offset, int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            return Clamp((double)offset / length);
        }

        public static int ToOffset(double ratio, int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            var boundedRatio = Clamp(ratio);
            var offset = (int)Math.Round(boundedRatio * length, MidpointRounding.AwayFromZero);
            return Math.Max(0, Math.Min(length - 1, offset));
        }

        private static double Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }
    }
}
