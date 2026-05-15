using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenRPA.Views
{
    /// <summary>
    /// First-run welcome wizard - lets the user choose between cloud (OpenFlow) or local mode.
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private bool _cloudSelected = false;
        public bool CloudModeSelected { get; private set; } = false;

        public WelcomeWindow()
        {
            InitializeComponent();
            DataContext = this;
            KeyDown += WelcomeWindow_KeyDown;
        }

        private void WelcomeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // ESC = local mode (default)
                CloudModeSelected = false;
                DialogResult = true;
                Close();
            }
        }

        private void CloudCard_Click(object sender, MouseButtonEventArgs e)
        {
            _cloudSelected = true;
            CloudCard.BorderBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5));
            CloudCard.Background = new SolidColorBrush(Color.FromRgb(0xE3, 0xF2, 0xFD));
            LocalCard.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            LocalCard.Background = Brushes.White;
            UrlPanel.Visibility = Visibility.Visible;
            btnStart.Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5));
        }

        private void LocalCard_Click(object sender, MouseButtonEventArgs e)
        {
            _cloudSelected = false;
            LocalCard.BorderBrush = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47));
            LocalCard.Background = new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9));
            CloudCard.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            CloudCard.Background = Brushes.White;
            UrlPanel.Visibility = Visibility.Collapsed;
            btnStart.Background = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47));
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ApplyChoice();
        }

        private void ApplyChoice()
        {
            if (_cloudSelected)
            {
                var url = txtWsUrl.Text?.Trim();
                if (string.IsNullOrEmpty(url))
                {
                    MessageBox.Show("Please enter an OpenFlow server URL.", "URL Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Config.local.wsurl = url;
                CloudModeSelected = true;
            }
            else
            {
                Config.local.wsurl = "";
                CloudModeSelected = false;
            }

            Config.local.firstrun = false;
            Config.Save();

            DialogResult = true;
            Close();
        }
    }
}
