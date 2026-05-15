using System;
using System.Drawing;
using OpenRPA.Interfaces;

namespace OpenRPA.Interfaces.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            RecordsClickRatiosWithinElement();
            ClampsRatiosAtElementBounds();
            ReplaysRatiosAgainstCurrentElementSize();
            return 0;
        }

        private static void RecordsClickRatiosWithinElement()
        {
            var ok = ClickPosition.TryCreateRatios(new Rectangle(20, 30, 200, 100), 50, 25, out var ratioX, out var ratioY);

            AssertTrue(ok, "Expected valid rectangle to create ratios.");
            AssertEqual(0.25, ratioX, "ClickRatioX");
            AssertEqual(0.25, ratioY, "ClickRatioY");
        }

        private static void ClampsRatiosAtElementBounds()
        {
            ClickPosition.TryCreateRatios(new Rectangle(0, 0, 100, 100), -10, 150, out var ratioX, out var ratioY);

            AssertEqual(0, ratioX, "Clamped ClickRatioX");
            AssertEqual(1, ratioY, "Clamped ClickRatioY");
        }

        private static void ReplaysRatiosAgainstCurrentElementSize()
        {
            var offsetX = ClickPosition.ToOffset(0.25, 400);
            var offsetY = ClickPosition.ToOffset(0.5, 80);

            AssertEqual(100, offsetX, "Replayed OffsetX");
            AssertEqual(40, offsetY, "Replayed OffsetY");
        }

        private static void AssertTrue(bool value, string message)
        {
            if (!value) throw new Exception(message);
        }

        private static void AssertEqual(double expected, double actual, string name)
        {
            if (Math.Abs(expected - actual) > 0.000001)
            {
                throw new Exception($"{name}: expected {expected}, got {actual}.");
            }
        }

        private static void AssertEqual(int expected, int actual, string name)
        {
            if (expected != actual)
            {
                throw new Exception($"{name}: expected {expected}, got {actual}.");
            }
        }
    }
}
