using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Pegelalarm.Core.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pegelalarm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailView : Page, IDetailView
    {
        public DetailView()
        {
            this.InitializeComponent();
            this.Chart.ShowSlider(false, 0);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void Chart_AlarmValueChanged(object sender, double e)
        {
            ViewModel.Alarm.AlarmValue = e;
            ViewModel.AlarmChanged();
        }

        public DetailViewModel ViewModel => DataContext as DetailViewModel;
            

        public void ConfigChart(double warnValue, double alarmValue, List<Sample> samples)
        {
            if (samples.Count == 0) Chart.ShowSlider(false, 0);
            Chart.SetValues(warnValue, alarmValue, samples);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            this.Chart.AlarmValueChanged -= Chart_AlarmValueChanged;
            this.Chart.AlarmValueChanged += Chart_AlarmValueChanged;
            var kind = (sender as MenuFlyoutItem).CommandParameter == "high" ? WaterKind.Highwater : WaterKind.Lowwater;

            ViewModel.AlarmActivated(kind);
            Chart.ShowSlider(true, 0);
        }

        public void ShowAlarmSlider()
        {
            this.Chart.AlarmValueChanged -= Chart_AlarmValueChanged;
            this.Chart.AlarmValueChanged += Chart_AlarmValueChanged;
            Chart.ShowSlider(true, ViewModel.Alarm.AlarmValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Alarm.HasAlarm = false;
            ViewModel.Alarm.AlarmValue = 0;
            Chart.ShowSlider(false,0);
            this.Chart.AlarmValueChanged -= Chart_AlarmValueChanged;
            ViewModel.RemoveAlarm();
        }
    }
}
