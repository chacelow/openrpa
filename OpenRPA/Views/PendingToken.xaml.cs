using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenRPA.Views
{
    /// <summary>
    /// Not logged in dialog — user clicks link to open browser login, or cancels.
    /// </summary>
    public partial class PendingToken : Window
    {
        private readonly string _loginUrl;

        public PendingToken(string loginUrl)
        {
            InitializeComponent();
            DataContext = this;
            _loginUrl = loginUrl;
            lnkLogin.Text = loginUrl;
        }

        // Backward compatibility constructor (no URL)
        public PendingToken() : this(null)
        {
        }

        public bool result = true;
        public bool dontremind = false;

        private void LnkLogin_Click(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(_loginUrl))
            {
                GenericTools.OpenUrl(_loginUrl);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            dontremind = chkDontRemind.IsChecked == true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            result = false;
            dontremind = chkDontRemind.IsChecked == true;
        }
    }
}
