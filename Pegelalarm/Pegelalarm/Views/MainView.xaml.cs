using Pegelalarm.Core.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Pegelalarm.Core.Utils;
using Windows.UI;
using Pegelalarm.ViewModels;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Pegelalarm.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pegelalarm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page, IMainView
    {
        private List<MapItem> stations;

        private MapPolygon radiusDisplay;
        private MapPolygon alarmDisplay;

        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            stations = new List<MapItem>();
        }

        public MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (DataContext as MainViewModel).PropertyChanged += MainView_PropertyChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            (DataContext as MainViewModel).PropertyChanged -= MainView_PropertyChanged;

        }

        private void MainView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null || e.PropertyName == nameof(ViewModel.DisplayRange) && radiusDisplay != null)
            {
                UpdateDisplayRadiusCircle();
            }

            if (e.PropertyName == nameof(ViewModel.AlarmRange))
            {
                UpdateAlarmCircle();
            }
        }

        public async void ShowMapAt(double lat, double lon, int zoom = 10)
        {
            await Task.Delay(1);
            Map.TrySetViewAsync(new Geopoint(new BasicGeoposition { Latitude = lat, Longitude = lon }), zoom);
        }

        public void MapItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var st in stations)
                {
                    Map.MapElements.Remove(st.Icon);
                }
                stations.Clear();
                return;
            }

            foreach (var oldItem in e.OldItems ?? new List<object>())
            {
                var item = oldItem as MapItem;
                
                Map.MapElements.Remove(item.Icon);
                if (radiusDisplay != null && item.Kind == MapItemKind.Location) Map.MapElements.Remove(radiusDisplay);
            }

            foreach (var newItem in e.NewItems ?? new List<object>())
            {
                var item = newItem as MapItem;
                item.Icon.ZIndex = 2;
                Map.MapElements.Add(item.Icon);
                if (item.Kind == MapItemKind.Location)
                {
                    UpdateDisplayRadiusCircle();
                }
                else
                {
                    stations.Add(item);
                }
            }

            UpdateAlarmCircle();

        }

        private MapPolygon CreateCircle(Geopoint point, int radius)
        {
            MapPolygon mp = new MapPolygon();
            
            var list = new List<BasicGeoposition>();
            for (int i = 0; i < 360; i++)
            {
                var p = point.CalculateDerivedPosition(radius, i);
                list.Add(p.Position);
            }
            
            mp.Path = new Geopath(list);
            return mp;
        }


        private void SplitViewOpenClose(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = SplitView.IsPaneOpen ? false : true;
        }

        private void SwitchMapStyle(object sender, RoutedEventArgs e)
        {
            if (Map.Style == MapStyle.AerialWithRoads)
            {
                Map.Style = MapStyle.Road;
            }
            else
            {
                Map.Style = MapStyle.AerialWithRoads;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Map_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            Map.MapTapped -= Map_MapTapped;
            var p = args.Location.Position;
            ViewModel.Location = new MapItem(p.Latitude, p.Longitude, MapItemKind.Location, null);
            ViewModel.UpdateDisplayedStations();
        }

        private void SetPositionOnMap(object sender, RoutedEventArgs e)
        {
            new MessageDialog("Klicke nun auf die Karte um deine Position zu festzulegen!").ShowAsync();
            Map.MapTapped += Map_MapTapped;
        }

        private void UpdateDisplayRadiusCircle()
        {
            if (radiusDisplay != null && Map.MapElements.Contains(radiusDisplay))
            {
                Map.MapElements.Remove(radiusDisplay);
            }
            radiusDisplay = CreateCircle(ViewModel.Location.Icon.Location, ViewModel.DisplayRange * 1000);
            radiusDisplay.FillColor = (Color)App.Current.Resources["TransparentGreyColor"];
            radiusDisplay.StrokeColor = (Color)App.Current.Resources["DarkGreyColor"];
            radiusDisplay.ZIndex = 0;
            Map.MapElements.Add(radiusDisplay);
        }

        private void UpdateAlarmCircle()
        {
            if (alarmDisplay != null && Map.MapElements.Contains(alarmDisplay))
            {
                Map.MapElements.Remove(alarmDisplay);
            }
            alarmDisplay = CreateCircle(ViewModel.Location.Icon.Location, ViewModel.AlarmRange * 1000);
            alarmDisplay.FillColor = (Color)App.Current.Resources["TransparentRedColor"];
            alarmDisplay.StrokeColor = (Color)App.Current.Resources["DarkGreyColor"];
            alarmDisplay.ZIndex = 1;
            Map.MapElements.Add(alarmDisplay);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SplitView.IsPaneOpen = false;
            ViewModel.Show((sender as ListView).SelectedIndex);
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = true;
            new InfoDialog().ShowAsync();
        }
    }
}
