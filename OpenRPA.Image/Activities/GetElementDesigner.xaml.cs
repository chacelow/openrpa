using Microsoft.VisualBasic.Activities;
using OpenRPA.Interfaces;
using OpenRPA.Interfaces.win32;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenRPA.Image
{
    public partial class GetElementDesigner : INotifyPropertyChanged
    {
        public GetElementDesigner()
        {
            InitializeComponent();
            HighlightImage = Extensions.GetImageSourceFromResource("search.png");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public BitmapFrame HighlightImage { get; set; }
        private async void btn_Select(object sender, RoutedEventArgs e)
        {
            Interfaces.GenericTools.Minimize();

            var limit = ModelItem.GetValue<Rectangle>("Limit");
            Rectangle rect = Rectangle.Empty;
            Log.Information(limit.ToString());
            using (Interfaces.Overlay.OverlayWindow _overlayWindow = new Interfaces.Overlay.OverlayWindow(true))
            {
                _overlayWindow.BackColor = System.Drawing.Color.Blue;
                var tip = new Interfaces.Overlay.TooltipWindow("Select area to look for");
                if (limit != Rectangle.Empty)
                {
                    _overlayWindow.Visible = true;
                    _overlayWindow.Bounds = limit;
                    _overlayWindow.TopMost = true;
                    _overlayWindow.Opacity = 0.3;
                    tip.setText("Select area to look for within the blue area");
                }
                rect = await getrectangle.GetitAsync();
                tip.Close();
                tip = null;
            }

            if (limit != Rectangle.Empty)
            {
                if (!limit.Contains(rect))
                {
                    Log.Error(rect.ToString() + " is not within process limit of " + limit.ToString());
                    Interfaces.GenericTools.Restore();
                    return;
                }
            }

            var _image = new System.Drawing.Bitmap(rect.Width, rect.Height);
            var graphics = System.Drawing.Graphics.FromImage(_image as System.Drawing.Image);
            graphics.CopyFromScreen(rect.X, rect.Y, 0, 0, _image.Size);
            ModelItem.Properties["Image"].SetValue(Interfaces.Image.Util.Bitmap2Base64(_image));
            NotifyPropertyChanged("Image");
            Interfaces.GenericTools.Restore();

        }
        private void Open_Selector(object sender, RoutedEventArgs e)
        {
            string SelectorString = ModelItem.GetValue<string>("Selector");
            int maxresults = ModelItem.GetValue<int>("MaxResults");
            Interfaces.Selector.SelectorWindow selectors;
            if (!string.IsNullOrEmpty(SelectorString))
            {
                var selector = new OpenRPA.Windows.WindowsSelector(SelectorString);
                selectors = new Interfaces.Selector.SelectorWindow("Windows", selector, null, maxresults);
            }
            else
            {
                var selector = new OpenRPA.Windows.WindowsSelector("[{Selector: 'Windows'}]");
                selectors = new Interfaces.Selector.SelectorWindow("Windows", selector, null, maxresults);
            }
            if (selectors.ShowDialog() == true)
            {
                ModelItem.Properties["Selector"].SetValue(new InArgument<string>() { Expression = new Literal<string>(selectors.vm.json) });
                var l = selectors.vm.Selector.Last();
                if (l.Element != null)
                {
                    ModelItem.Properties["Image"].SetValue(l.Element.ImageString());
                    NotifyPropertyChanged("Image");
                }
            }
        }
        private void Highlight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = ImageString;
                Log.Information("Highlight: ImageString length=" + (image?.Length ?? 0));
                if (string.IsNullOrEmpty(image))
                {
                    Log.Error("Highlight: No image selected!");
                    System.Windows.MessageBox.Show("Please select an image region first by clicking [Select area].", "Highlight", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                Bitmap b = Task.Run(() =>
                {
                    return Interfaces.Image.Util.LoadBitmap(image);
                }).Result;
                using (b)
                {
                    if (b == null)
                    {
                        Log.Error("Highlight: Failed to load bitmap from image string");
                        System.Windows.MessageBox.Show("Failed to load the selected image.", "Highlight", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    var Threshold = ModelItem.GetValue<double>("Threshold");
                    var CompareGray = ModelItem.GetValue<bool>("CompareGray");
                    var Selector = ModelItem.GetValue<string>("Selector");
                    var MatchMode = ModelItem.GetValue<ImageMatchMode>("MatchMode");
                    if (Threshold < 0.5) Threshold = 0.8;
                    Log.Information(string.Format("Highlight: M={0} T={1} G={2} S={3}", MatchMode, Threshold, CompareGray, string.IsNullOrEmpty(Selector) ? "(fullscreen)" : "selector"));
                    var matches = ImageEvent.waitFor(b, Threshold, Selector, TimeSpan.FromMilliseconds(0), CompareGray, MatchMode);
                    Log.Information("Highlight: found " + matches.Length + " match(es)");
                    foreach (var r in matches)
                    {
                        var element = new ImageElement(r);
                        element.Highlight(false, System.Drawing.Color.PaleGreen, TimeSpan.FromSeconds(1));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Highlight_Click exception: " + ex.ToString());
                System.Windows.MessageBox.Show("Highlight error: " + ex.Message, "Highlight", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public string ImageString
        {
            get
            {
                string result = string.Empty;
                result = ModelItem.GetValue<string>("Image");
                return result;
            }
        }
        public BitmapImage Image
        {
            get
            {
                var image = ImageString;
                System.Drawing.Bitmap b = Task.Run(() =>
                {
                    return Interfaces.Image.Util.LoadBitmap(image);
                }).Result;
                using (b)
                {
                    if (b == null) return null;
                    return Interfaces.Image.Util.BitmapToImageSource(b, Interfaces.Image.Util.ActivityPreviewImageWidth, Interfaces.Image.Util.ActivityPreviewImageHeight);
                }
            }
        }
    }
}