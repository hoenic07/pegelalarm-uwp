using Caliburn.Micro;
using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network;
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
                alarm = alarms.First(d => d.MetricKind == selectedMetric.Kind);
                LoadGraphData();
            }
        }


        private Alarm alarm;

        public Alarm Alarm
        {
            get { return alarm; }
            set { alarm = value; NotifyOfPropertyChange(); }
        }

        private bool canSetAlarm;

        public bool CanSetAlarm
        {
            get { return canSetAlarm; }
            set { canSetAlarm = value; NotifyOfPropertyChange(); }
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
            SelectedTimeFrame = TimeFrames[0];
            Metrics = new ObservableCollection<Metric>();
            Metrics.Add(new Metric { Kind = MetricKind.Height, Name = "cm" });
            Metrics.Add(new Metric { Kind = MetricKind.Flow, Name = "m³/s" });
            SelectedMetric = Metrics[0];

            alarms = new List<Alarm>
            {
                new Alarm {HasAlarm=false, MetricKind = MetricKind.Height },
                new Alarm {HasAlarm=false, MetricKind = MetricKind.Flow }
            };

            Alarm = alarms[0];
        }

        #endregion

        #region Events

        protected override async void OnActivate()
        {
            base.OnActivate();
            Station = IoC.Get<MainViewModel>().DisplayedStations.FirstOrDefault(st => st.Data.commonid == StationId);
            
            LoadGraphData();
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

        private async Task LoadGraphData()
        {
            if (SelectedMetric == null || SelectedTimeFrame == null || StationId == null) return;
            
            var res = await webService.GetStationHistory(StationId, SelectedMetric.Kind.ToString().ToLower(), 
                SelectedTimeFrame.Granularity, SelectedTimeFrame.FromDate, DateTime.Now);
            
            if (res.IsSuccessful)
            {
                CanSetAlarm = res.Payload.Length > 0;

                var d = Station.Data;
                var alarmValue = SelectedMetric.Kind == MetricKind.Height ? d.defaultAlarmValueCm : d.defaultAlarmValueM3s;
                var warnValue = SelectedMetric.Kind == MetricKind.Height ? d.defaultWarnValueCm : d.defaultWarnValueM3s;

                view.ConfigChart(0, 0, res.Payload.ToList());
            }
        }

        #endregion

    }
}
