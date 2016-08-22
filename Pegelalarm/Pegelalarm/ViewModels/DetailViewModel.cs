using Caliburn.Micro;
using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network;
using Pegelalarm.Core.Persistance;
using Pegelalarm.Core.Utils;
using Pegelalarm.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.ViewModels
{
    public class DetailViewModel : Screen
    {
        #region Members

        private WebService webService;
        private IDetailView view;
        private List<Alarm> alarms;
        private Delayer alarmStorageDelayer;

        #endregion

        #region Properties

        private UIStation station;

        public UIStation Station
        {
            get { return station; }
            set { station = value; NotifyOfPropertyChange(); }
        }
        
        public string StationId { get; set; }

        public string GraphType { get; set; }

        public List<Timeframe> TimeFrames { get; set; }

        private Timeframe selectedTimeFrame;

        public Timeframe SelectedTimeFrame
        {
            get { return selectedTimeFrame; }
            set {
                selectedTimeFrame = value;
                NotifyOfPropertyChange();
                LoadGraphData();
            }
        }
        
        public ObservableCollection<Metric> Metrics { get; set; }

        private Metric selectedMetric;

        public Metric SelectedMetric
        {
            get { return selectedMetric; }
            set {
                selectedMetric = value;
                NotifyOfPropertyChange();
                Alarm = alarms.First(d => d.MetricKind == selectedMetric.Kind);
                LoadGraphData();
            }
        }


        private Alarm alarm;

        public Alarm Alarm
        {
            get { return alarm; }
            set
            {
                alarm = value; NotifyOfPropertyChange();
                if (alarm.HasAlarm)
                {
                    view.ShowAlarmSlider();
                }
            }
        }

        #endregion

        #region Ctor

        public DetailViewModel(WebService webService)
        {
            this.webService = webService;
            TimeFrames = new List<Timeframe>()
            {
                new Timeframe {Name="24 Stunden", FromDate = DateTime.Now.AddDays(-1), Granularity=Granularity.Hour },
                new Timeframe {Name="7 Tage", FromDate = DateTime.Now.AddDays(-7), Granularity= Granularity.Hour },
                new Timeframe {Name="30 Tage", FromDate = DateTime.Now.AddDays(-30), Granularity=Granularity.Day },
            };

            alarms = new List<Alarm>
            {
                new Alarm {HasAlarm=false, MetricKind = MetricKind.Height },
                new Alarm {HasAlarm=false, MetricKind = MetricKind.Flow }
            };

            Alarm = alarms[0];

            SelectedTimeFrame = TimeFrames[0];
            Metrics = new ObservableCollection<Metric>();
            Metrics.Add(new Metric { Kind = MetricKind.Height, Name = "cm" });
            Metrics.Add(new Metric { Kind = MetricKind.Flow, Name = "m³/s" });
            SelectedMetric = Metrics[0];
            alarmStorageDelayer = new Delayer(TimeSpan.FromSeconds(1.5));
            alarmStorageDelayer.Action += (a,b) => SaveAlarm();
        }

        #endregion

        #region Events

        protected override async void OnActivate()
        {
            base.OnActivate();
            Station = IoC.Get<MainViewModel>().DisplayedStations.FirstOrDefault(st => st.Data.commonid == StationId);

            var stations = await GlobalSettings.Instance.GetMonitoredStations();
            var heightAlarm = stations.FirstOrDefault(a => a.StationId == StationId && a.MetricKind == MetricKind.Height);
            var flowAlarm = stations.FirstOrDefault(a => a.StationId == StationId && a.MetricKind == MetricKind.Flow);

            if (heightAlarm != null)
            {
                heightAlarm.HasAlarm = true;
                alarms[0] = heightAlarm;
            }
            if (flowAlarm != null)
            {
                flowAlarm.HasAlarm = true;
                alarms[1] = flowAlarm;
            }

            if (heightAlarm != null || flowAlarm != null)
            {
                Alarm = alarms.First(d => d.MetricKind == selectedMetric.Kind);
            }

            LoadGraphData(true);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.view = view as IDetailView;
        }

        #endregion

        #region Actions

        public void AlarmActivated(WaterKind waterKind)
        {
            alarm.HasAlarm = true;
            alarm.WaterKind = waterKind;
        }

        #endregion

        #region Methods

        private async Task LoadGraphData(bool isInit=false)
        {
            if (SelectedMetric == null || SelectedTimeFrame == null || StationId == null) return;
            
            var res = await webService.GetStationHistory(StationId, SelectedMetric.Kind.ToString().ToLower(), 
                SelectedTimeFrame.Granularity, SelectedTimeFrame.FromDate, DateTime.Now);
            
            if (res.IsSuccessful)
            {
                Alarm.CanSetAlarm = res.Payload.Length > 0;

                var d = Station.Data;
                var alarmValue = SelectedMetric.Kind == MetricKind.Height ? d.defaultAlarmValueCm : d.defaultAlarmValueM3s;
                var warnValue = SelectedMetric.Kind == MetricKind.Height ? d.defaultWarnValueCm : d.defaultWarnValueM3s;

                view.ConfigChart(warnValue, alarmValue, res.Payload.ToList());

                if (res.Payload.Length == 0 && isInit)
                {
                    SelectedMetric = Metrics[1];
                }
            }
        }

        public void AlarmChanged()
        {
            alarmStorageDelayer.ResetAndTick();
        }

        private async void SaveAlarm()
        {
            var stations = await GlobalSettings.Instance.GetMonitoredStations();
            Alarm.StationId = StationId;
            var al = stations.FirstOrDefault(st => st.StationId == StationId && st.MetricKind == Alarm.MetricKind);
            if (al != null) stations.Remove(al);
            stations.Add(Alarm);

            await GlobalSettings.Instance.SaveMonitoredStations(stations);
        }

        public async void RemoveAlarm()
        {
            var stations = await GlobalSettings.Instance.GetMonitoredStations();
            Alarm.StationId = StationId;
            var al = stations.FirstOrDefault(st => st.StationId == StationId && st.MetricKind == Alarm.MetricKind);
            if (al != null) stations.Remove(al);
            
            await GlobalSettings.Instance.SaveMonitoredStations(stations);
        }

        #endregion

    }
}
