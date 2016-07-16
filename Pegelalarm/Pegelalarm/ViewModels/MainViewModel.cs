using Caliburn.Micro;
using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Pegelalarm.Core.Utils;
using System.Collections.Specialized;
using Windows.UI.Xaml.Controls.Maps;

namespace Pegelalarm.ViewModels
{
    public class MainViewModel : Screen
    {
        #region Members
        private INavigationService navService;
        private WebService webService;
        private Geolocator locator;
        private IMainView view;

        private Delayer sliderDelayer;

        #endregion

        #region Properties

        public ObservableCollection<MapItem> MapItems { get; set; }

        public ObservableCollection<Station> DisplayedStations { get; set; }

        private Station displayedStation;

        public Station DisplayedStation
        {
            get { return displayedStation; }
            set { displayedStation = value; NotifyOfPropertyChange(); }
        }

        private MapItem location;

        public MapItem Location
        {
            get { return location; }
            set
            {

                if (location != value && location != null)
                {
                    var loc = MapItems.FirstOrDefault(d => d.Kind == MapItemKind.Location);
                    if (loc != null) MapItems.Remove(loc);
                }

                MapItems.Add(value);

                location = value;
            }
        }

        private int displayRange;

        public int DisplayRange
        {
            get { return displayRange; }
            set { displayRange = value; NotifyOfPropertyChange(); sliderDelayer.ResetAndTick(); }
        }


        private bool isStationInfoDisplayed;

        public bool IsStationInfoDisplayed
        {
            get { return isStationInfoDisplayed; }
            set { isStationInfoDisplayed = value; NotifyOfPropertyChange(); }
        }


        #endregion

        #region Ctor
        public MainViewModel(INavigationService navService, WebService webService)
        {
            this.navService = navService;
            this.webService = webService;
            locator = new Geolocator();
            MapItems = new ObservableCollection<MapItem>();
            DisplayedStations = new ObservableCollection<Station>();
            sliderDelayer = new Delayer(TimeSpan.FromSeconds(1));
            sliderDelayer.Action += (a, b) => UpdateDisplayedStations();
            DisplayRange = 100;
        }
        #endregion

        #region Events

        protected async override void OnActivate()
        {
            base.OnActivate();
            GetMyLocation();
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            this.view = view as IMainView;

            MapItems.CollectionChanged += this.view.MapItemsChanged;

            //Show Austria at beginning
            this.view.ShowMapAt(47.5759594, 13.7267207, 8);
        }

        #endregion

        #region Actions

        public async void GetMyLocation()
        {
            var res = await Geolocator.RequestAccessAsync();

            if (res == GeolocationAccessStatus.Allowed)
            {
                var pos = await locator.GetGeopositionAsync();
                var p = pos.Coordinate;
                view.ShowMapAt(p.Latitude, p.Longitude);
                Location = new MapItem(p.Latitude, p.Longitude, MapItemKind.Location, null);
                UpdateDisplayedStations();
            }
        }

        public void IconClick(MapElementClickEventArgs o)
        {
            var icon = o.MapElements.FirstOrDefault(d => d is MapIcon);
            var item = MapItems.FirstOrDefault(i => i.Icon == icon);
            if (item == null) return;
            DisplayedStation = DisplayedStations.FirstOrDefault(s => s.commonid == item.ID);
            IsStationInfoDisplayed = true;
        }

        public void HideStationInfo()
        {
            IsStationInfoDisplayed = false;
        }

        #endregion

        #region Methods
        
        private async void UpdateDisplayedStations()
        {
            if (Location == null) return;
            var loc = Location.Icon.Location;
            var bb = loc.CalculateBoundingBoxAroundPosition(DisplayRange * 1000);
            DisplayedStations.Clear();
            view.MapItemsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            var resSt = await webService.GetStationsBy(bb);

            if (resSt.IsSuccessful)
            {
                foreach (var st in resSt.Payload)
                {
                    Geopoint d = new Geopoint(new BasicGeoposition { Latitude = st.latitude, Longitude = st.longitude });
                    if (loc.GetDistanceInMetersBetween(d) > DisplayRange * 1000) continue;
                    var mi = new MapItem(st.latitude, st.longitude, MapItemKind.Station, st);
                    MapItems.Add(mi);
                    DisplayedStations.Add(st);
                }
            }
        }

        #endregion
    }
}
