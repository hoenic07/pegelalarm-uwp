using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pegelalarm.Controls
{
    public sealed partial class InfoDialog : ContentDialog
    {
        public InfoDialog()
        {
            this.InitializeComponent();
        }

        private void DataSource_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("http://www.pegelalarm.at"));
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("mailto:nik.hoesl@hotmail.com"));
        }

        private void Source_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://github.com/hoenic07/pegelalarm-uwp"));
        }
    }
}
