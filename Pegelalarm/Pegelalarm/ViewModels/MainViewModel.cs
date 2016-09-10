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
using Windows.ApplicationModel.Background;
using Pegelalarm.Core.Persistance;
using Windows.UI.Popups;
using System.Diagnostics;

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
        private Delayer alarmSliderDelayer;
        private bool isLoaded;

        #endregion

        #region Properties

        public ObservableCollection<MapItem> MapItems { get; set; }

        public ObservableCollection<UIStation> DisplayedStations { get; set; }

        private UIStation displayedStation;

        public UIStation DisplayedStation
        {
            get { return displayedStation; }
            set { displayedStation = value; NotifyOfPropertyChange(); }
        }

        public List<UIMonitoredStation> MonitoredStations { get; set; }

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

                location = value;

                MapItems.Add(value);
                SaveLocationData();
            }
        }

        private int displayRange;

        public int DisplayRange
        {
            get { return displayRange; }
            set
            {
                if (displayRange == value) return;
                displayRange = value;
                NotifyOfPropertyChange();
                sliderDelayer.ResetAndTick();
            }
        }

        private int alarmRange;

        public int AlarmRange
        {
            get { return alarmRange; }
            set
            {
                if (alarmRange == value) return;
                alarmRange = value;
                NotifyOfPropertyChange();
                alarmSliderDelayer.ResetAndTick();
            }
        }

        private bool isStationInfoDisplayed;

        public bool IsStationInfoDisplayed
        {
            get { return isStationInfoDisplayed; }
            set { isStationInfoDisplayed = value; NotifyOfPropertyChange(); }
        }

        private bool isMapDisplayed;

        public bool IsMapDisplayed
        {
            get { return isMapDisplayed; }
            set { isMapDisplayed = value; NotifyOfPropertyChange(); }
        }

        private bool isListDisplayed;

        public bool IsListDisplayed
        {
            get { return isListDisplayed; }
            set { isListDisplayed = value; NotifyOfPropertyChange(); }
        }

        private bool isMonitoredListDisplayed;

        public bool IsMonitoredListDisplayed
        {
            get { return isMonitoredListDisplayed; }
            set { isMonitoredListDisplayed = value; NotifyOfPropertyChange(); }
        }
        
        public bool AlarmNotificationsOn
        {
            get { return GlobalSettings.Instance.AlarmRangeNotificationsOn; }
            set { GlobalSettings.Instance.AlarmRangeNotificationsOn = value; NotifyOfPropertyChange(); }
        }

        #endregion

        #region Ctor
        public MainViewModel(INavigationService navService, WebService webService)
        {
            this.navService = navService;
            this.webService = webService;
            locator = new Geolocator();
            MapItems = new ObservableCollection<MapItem>();
            DisplayedStations = new ObservableCollection<UIStation>();
            sliderDelayer = new Delayer(TimeSpan.FromSeconds(1));
            sliderDelayer.Action += (a, b) => UpdateDisplayedStations();
            alarmSliderDelayer = new Delayer(TimeSpan.FromSeconds(1));
            alarmSliderDelayer.Action += (a, b) => SaveLocationData();
            IsMapDisplayed = true;
            RegisterBackgroundTask();
        }
        #endregion

        #region Events

        protected async override void OnActivate()
        {
            base.OnActivate();
            if (view != null)
            {
                Loaded();
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            this.view = view as IMainView;

            if (IsActive)
            {
                Loaded();
            }
        }

        private void Loaded()
        {
            LoadMonitoredStations();

            if (isLoaded) return;

            MapItems.CollectionChanged -= this.view.MapItemsChanged;
            MapItems.CollectionChanged += this.view.MapItemsChanged;

            

            LoadLocationData();
            if (Location == null)
            {
                //Show Austria at beginning
                this.view.ShowMapAt(47.5759594, 13.7267207, 8);
                GetMyLocation();
            }
            else
            {
                var p = Location.Icon.Location.Position;
                view.ShowMapAt(p.Latitude, p.Longitude);
            }

            isLoaded = true;
        }

        #endregion

        #region Actions

        public async void GetMyLocation()
        {
            try
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
                else
                {
                    view.SetPositionOnMap();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Location > Error getting location. " + ex.Message);
                view.SetPositionOnMap();
            }
        }

        public void IconClick(MapElementClickEventArgs o)
        {
            var icon = o.MapElements.FirstOrDefault(d => d is MapIcon);
            var item = MapItems.FirstOrDefault(i => i.Icon == icon);
            if (item == null || item == location) return;
            DisplayedStation = DisplayedStations.FirstOrDefault(s => s.Data.commonid == item.ID);
            IsStationInfoDisplayed = true;
        }

        public void HideStationInfo()
        {
            IsStationInfoDisplayed = false;
        }

        public void Show(int id)
        {
            IsMapDisplayed = false;
            IsListDisplayed = false;
            IsMonitoredListDisplayed = false;

            switch (id)
            {
                case 0:
                    IsMapDisplayed = true; break;
                case 1:
                    IsListDisplayed = true; break;
                case 2:
                    IsMonitoredListDisplayed = true; break;
            }

        }

        public void ShowDetail(UIStation st)
        {
            var id = st.Data.commonid;
            navService.For<DetailViewModel>().WithParam<string>(d => d.StationId, id).Navigate();
        }

        public void ShowDetail()
        {
            ShowDetail(DisplayedStation);
        }

        #endregion

        #region Methods

        public async void UpdateDisplayedStations()
        {
            SaveLocationData();
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
                    var dist = loc.GetDistanceInMetersBetween(d);
                    if (dist > DisplayRange * 1000) continue;
                    var mi = new MapItem(st.latitude, st.longitude, MapItemKind.Station, st);
                    MapItems.Add(mi);
                    DisplayedStations.Add(new UIStation { Data = st });
                }
            }
            else
            {
                new MessageDialog("Daten konnten nicht abgerufen werden.", "Fehler").ShowAsync();
            }
            NotifyOfPropertyChange(() => DisplayedStations);

            LoadMonitoredStations();
        }

        public async void LoadMonitoredStations()
        {
            if (DisplayedStations == null)
            {
                MonitoredStations = null;
                NotifyOfPropertyChange(() => MonitoredStations);
                return;
            }

            var stations = await GlobalSettings.Instance.GetMonitoredStations();
            var list = new List<UIMonitoredStation>();
            foreach (var st in stations)
            {
                var sta = DisplayedStations.FirstOrDefault(d => d.Data.commonid == st.StationId);
                if (sta != null)
                {
                    var uimst = new UIMonitoredStation();
                    uimst.Data = sta.Data;
                    uimst.WaterKindStringPlain = st.WaterKindStringPlain;
                    uimst.AlarmValue = st.AlarmValue;
                    uimst.MonitoredValueTypeString = st.MetricKindString;
                    uimst.MonitoredValue = sta.Data.data.FirstOrDefault(d => d.Metric == st.MetricKind)?.value ?? 0;
                    list.Add(uimst);
                }
            }
            MonitoredStations = list;
            NotifyOfPropertyChange(()=> MonitoredStations);
        }

        public async Task RegisterBackgroundTask()
        {
            await Task.Delay(1000);
            var taskRegistered = false;
            var exampleTaskName = "PeriodicStationTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = "PeriodicStationTask";
                builder.TaskEntryPoint = "BackgroundTask.PeriodicStationTask";
                builder.SetTrigger(new TimeTrigger(30, false));

                BackgroundTaskRegistration task = builder.Register();
                var a = await BackgroundExecutionManager.RequestAccessAsync();
                
            }

        }

        private void SaveLocationData()
        {
            var loc = new LocationRange();
            loc.AlarmRadius = AlarmRange;
            loc.DisplayRadius = DisplayRange;
            loc.Longitude = Location?.Icon.Location.Position.Longitude ?? 0;
            loc.Latitude = Location?.Icon.Location.Position.Latitude ?? 0;
            GlobalSettings.Instance.LocationRange = loc;
        }

        private void LoadLocationData()
        {
            var locationData = GlobalSettings.Instance.LocationRange;
            AlarmRange = locationData.AlarmRadius;
            DisplayRange = locationData.DisplayRadius;
            if (locationData.Latitude != 0 && locationData.Longitude != 0)
            {
                Location = new MapItem(locationData.Latitude, locationData.Longitude, MapItemKind.Location, null);
            }
        }

        #endregion
    }
}
