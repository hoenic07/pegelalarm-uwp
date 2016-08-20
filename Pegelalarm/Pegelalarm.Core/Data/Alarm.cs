using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Data
{
    public class Alarm : Screen
    {
        
        private double alarmValue;

        public double AlarmValue
        {
            get { return alarmValue; }
            set { alarmValue = value; NotifyOfPropertyChange(); }
        }

        private bool hasAlarm;

        public bool HasAlarm
        {
            get { return hasAlarm; }
            set { hasAlarm = value; NotifyOfPropertyChange(); }
        }

        private MetricKind metricKind;

        public MetricKind MetricKind
        {
            get { return metricKind; }
            set { metricKind = value; NotifyOfPropertyChange(); }
        }

        private WaterKind waterKind;

        public WaterKind WaterKind
        {
            get { return waterKind; }
            set
            {
                waterKind = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => WaterKindString);
            }
        }

        public string WaterKindString => WaterKind == WaterKind.Highwater ? "Hochwasser" : "Niedrigwasser";
    }
}
