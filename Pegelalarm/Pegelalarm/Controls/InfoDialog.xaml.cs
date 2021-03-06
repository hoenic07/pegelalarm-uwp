﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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
            this.DataContext = this;
        }



        public string Version
        {
            get
            {
                var v = Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
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

        private void Newtonsoft_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("http://www.newtonsoft.com/json"));
        }

        private void Caliburn_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("http://caliburnmicro.com/"));
        }

        private void Hockey_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://github.com/bitstadium/HockeySDK-Windows"));
        }
    }
}
