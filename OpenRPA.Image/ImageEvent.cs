using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Image
{
    class ImageEvent
    {
        private ImageEvent() { }
        private Bitmap template;
        private System.Threading.AutoResetEvent waitHandle;
        private System.Timers.Timer timer;
        private Rectangle[] results = new Rectangle[] { };
        private bool CompareGray = false;
        private bool running = false;
        private Double threshold;
        private ImageMatchMode matchMode = ImageMatchMode.SIFT;
        private string selector = null;
        private Interfaces.Selector.SelectorItem selItem = null;

        // Backward-compatible: old 6-param signature (Processname/Limit ignored)
        public static Rectangle[] waitFor(Bitmap Image, Double Threshold, String Processname, TimeSpan TimeOut, bool CompareGray, Rectangle Limit)
        {
            return waitFor(Image, Threshold, Processname, TimeOut, CompareGray, Limit, ImageMatchMode.TemplateMatching);
        }
        // Backward-compatible: old 7-param signature (Processname/Limit ignored)
        public static Rectangle[] waitFor(Bitmap Image, Double Threshold, String Processname, TimeSpan TimeOut, bool CompareGray, Rectangle Limit, ImageMatchMode MatchMode)
        {
            return waitFor(Image, Threshold, (string)null, TimeOut, CompareGray, MatchMode);
        }
        // New signature with Selector
        public static Rectangle[] waitFor(Bitmap Image, Double Threshold, String Selector, TimeSpan TimeOut, bool CompareGray, ImageMatchMode MatchMode)
        {
            var me = new ImageEvent();
            try
            {
                me.matchMode = MatchMode;
                me.selector = Selector;
                me.template = Image;
                try
                {
                    var W = me.template.Width;
                    var H = me.template.Height;
                }
                catch (Exception)
                {
                    return me.results;
                }

                me.CompareGray = CompareGray;
                me.threshold = Threshold;
                me.running = true;

                // Pre-resolve selector to cache the window rectangle
                if (!string.IsNullOrEmpty(me.selector))
                {
                    me.resolveSelector();
                }

                if (me.findMatch()) return me.results;

                if (TimeOut.TotalMilliseconds < 100) return me.results;
                me.timer = new System.Timers.Timer();
                me.timer.Elapsed += me.onElapsed;
                me.timer.AutoReset = false;
                me.timer.Interval = 100;
                me.timer.Start();

                me.waitHandle = new System.Threading.AutoResetEvent(false);
                me.waitHandle.WaitOne(TimeOut);
                me.template = null;
                me.running = false;
                me.timer.Stop();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (me.timer != null)
                {
                    me.timer.Elapsed -= me.onElapsed;
                    me.timer.Dispose();
                    me.timer = null;
                }
            }
            return me.results;
        }

        private void resolveSelector()
        {
            try
            {
                var sel = new OpenRPA.Windows.WindowsSelector(selector);
                var items = sel.ToArray();
                if (items.Length > 0)
                {
                    selItem = items[0];
                    Log.Debug(string.Format("ImageEvent: selector resolved, item={0}", selItem));
                }
            }
            catch (Exception ex)
            {
                Log.Error("ImageEvent: failed to resolve selector: " + ex.ToString());
                selItem = null;
            }
        }

        private void onElapsed(object sender, EventArgs e)
        {
            timer.Stop();
            if (!running) return;
            if (findMatch())
            {
                running = false;
                waitHandle.Set();
            }
            if (timer != null)
            {
                if (running) timer.Start();
            }
        }

        private bool findMatch()
        {
            try
            {
                if (!running) return false;
                if (this.template == null) return false;

                Rectangle searchRect;
                if (!string.IsNullOrEmpty(selector) && selItem != null)
                {
                    // Search within selector-resolved window rectangle
                    var elements = OpenRPA.Windows.WindowsSelector.GetElementsWithuiSelector(
                        new OpenRPA.Windows.WindowsSelector(selector), null, 1, null);
                    if (elements == null || elements.Length == 0)
                    {
                        Log.Information("ImageEvent.findMatch: selector resolved but window not found");
                        return false;
                    }
                    searchRect = elements[0].Rectangle;
                    // Ensure minimum size for matching
                    if (searchRect.Width < template.Width || searchRect.Height < template.Height)
                    {
                        Log.Information(string.Format("ImageEvent.findMatch: window {0}x{1} too small for template {2}x{3}",
                            searchRect.Width, searchRect.Height, template.Width, template.Height));
                        return false;
                    }
                }
                else
                {
                    // Full-screen search
                    searchRect = new Rectangle(0, 0,
                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
                }

                var desktop = Interfaces.Image.Util.Screenshot(searchRect);
                GC.KeepAlive(template);
                        var results = matchMode == ImageMatchMode.TemplateMatching
                                ? Matches.FindMatchesMultiScale(desktop, template, threshold, 10, CompareGray)
                                : Matches.FindCvMatches(desktop, template, threshold, 10, matchMode);

                if (results.Length > 0)
                {
                    // Offset results to absolute screen coordinates
                    for (var i = 0; i < results.Length; i++)
                    {
                        results[i].X += searchRect.X;
                        results[i].Y += searchRect.Y;
                    }
                    this.results = results;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ImageEvent.findMatch: " + ex.ToString());
            }
            return false;
        }
    }
}
