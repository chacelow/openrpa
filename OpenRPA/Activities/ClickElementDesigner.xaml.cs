using System;
using System.Activities.Presentation.Model;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace OpenRPA.Activities
{
    public partial class ClickElementDesigner
    {
        public ClickElementDesigner() { InitializeComponent(); }

        private async void Highlight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Walk up to find the parent GetElement's selector
                var parent = ModelItem.Parent;
                string selectorString = null;
                while (parent != null)
                {
                    selectorString = parent.GetValue<string>("Selector");
                    if (!string.IsNullOrEmpty(selectorString)) break;
                    parent = parent.Parent;
                }
                if (string.IsNullOrEmpty(selectorString)) { MessageBox.Show("No Selector found"); return; }

                var selector = new Interfaces.Selector.Selector(selectorString);
                var pluginName = selector.First().Selector;
                var plugin = Interfaces.Plugins.recordPlugins.Where(x => x.Name == pluginName).First();
                var elements = plugin.GetElementsWithSelector(selector, null, 1);
                if (elements.Length == 0) { MessageBox.Show("Element not found"); return; }
                var el = elements[0];

                var rect = el.Rectangle;
                var offsetX = ModelItem.GetValue<int>("OffsetX");
                var offsetY = ModelItem.GetValue<int>("OffsetY");
                var clickRatioX = ModelItem.Properties["ClickRatioX"]?.ComputedValue as double?;
                var clickRatioY = ModelItem.Properties["ClickRatioY"]?.ComputedValue as double?;
                if (clickRatioX.HasValue && clickRatioY.HasValue)
                {
                    offsetX = (int)Math.Round(clickRatioX.Value * rect.Width);
                    offsetY = (int)Math.Round(clickRatioY.Value * rect.Height);
                }
                int cx = rect.X + offsetX;
                int cy = rect.Y + offsetY;

                // Show crosshair overlay (outline + crosshair) on UI thread
                var overlay = new ClickHighlightOverlay(rect, cx, cy);
                overlay.Show();
                // Flash 1s + solid 2s = 3s total
                await Task.Delay(3000);
                overlay.BeginInvoke((Action)(() => { try { overlay.Close(); overlay.Dispose(); } catch { } }));
            }
            catch (Exception ex) { MessageBox.Show("Highlight failed: " + ex.Message); }
        }
    }

    internal class ClickHighlightOverlay : Form
    {
        private readonly Rectangle _elementRect;
        private readonly int _crossX, _crossY;
        private readonly System.Windows.Forms.Timer _flashTimer;
        private bool _solid = false;
        private const int MARGIN = 20;

        public ClickHighlightOverlay(Rectangle elementRect, int crossX, int crossY)
        {
            _elementRect = elementRect;
            _crossX = crossX;
            _crossY = crossY;

            this.AutoScaleMode = AutoScaleMode.None;
            this.StartPosition = FormStartPosition.Manual;
            // Expand by margin so crosshair lines extend beyond element bounds
            this.Location = new System.Drawing.Point(elementRect.X - MARGIN, elementRect.Y - MARGIN);
            this.Size = new System.Drawing.Size(elementRect.Width + MARGIN * 2, elementRect.Height + MARGIN * 2);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = System.Drawing.Color.Lime;
            this.TransparencyKey = System.Drawing.Color.Lime;
            this.DoubleBuffered = true;
            this.Paint += OnPaint;

            // Flash: outline + crosshair together for 1s, then solid 2s
            _flashTimer = new System.Windows.Forms.Timer { Interval = 200 };
            int ticks = 0;
            _flashTimer.Tick += (s, e) =>
            {
                ticks++;
                if (ticks >= 5) { _solid = true; _flashTimer.Stop(); }
                this.Invalidate();
            };
            _flashTimer.Start();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3))
            {
                bool visible = _solid || (_flashTimer.Enabled && DateTime.Now.Millisecond < 500);
                if (!visible) return;

                // Element outline
                g.DrawRectangle(pen, MARGIN, MARGIN, _elementRect.Width, _elementRect.Height);

                // Crosshair inside element bounds
                int localX = MARGIN + (_crossX - _elementRect.X);
                int localY = MARGIN + (_crossY - _elementRect.Y);
                int elemTop = MARGIN, elemBottom = MARGIN + _elementRect.Height;
                int elemLeft = MARGIN, elemRight = MARGIN + _elementRect.Width;
                g.DrawLine(pen, localX, elemTop, localX, elemBottom);
                g.DrawLine(pen, elemLeft, localY, elemRight, localY);
            }
        }
    }
}
