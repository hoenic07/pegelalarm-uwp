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

        public MainView()
        {
            this.InitializeComponent();
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
            if (radiusDisplay != null)
            {
                Map.MapElements.Remove(radiusDisplay);
                radiusDisplay = CreateCircle(ViewModel.Location.Icon.Location, ViewModel.DisplayRange * 1000);
                radiusDisplay.FillColor = (Color)App.Current.Resources["TransparentGreyColor"];
                radiusDisplay.StrokeColor = (Color)App.Current.Resources["DarkGreyColor"];
                Map.MapElements.Add(radiusDisplay);
            }
        }

        public void ShowMapAt(double lat, double lon, int zoom = 12)
        {
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
                Map.MapElements.Add(item.Icon);
                if (item.Kind == MapItemKind.Location)
                {
                    var disRange = (DataContext as MainViewModel).DisplayRange;
                    radiusDisplay = CreateCircle(item.Icon.Location, disRange * 1000);
                    radiusDisplay.FillColor = (Color)App.Current.Resources["TransparentGreyColor"];
                    radiusDisplay.StrokeColor = (Color)App.Current.Resources["DarkGreyColor"];
                    Map.MapElements.Add(radiusDisplay);
                }
                else
                {
                    stations.Add(item);
                }
            }
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
    }
}
