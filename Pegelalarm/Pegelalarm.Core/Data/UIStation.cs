using Caliburn.Micro;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Core.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Pegelalarm.Core.Data
{
    public class UIStation : Screen
    {
        public Station Data { get; set; }

        private bool isFavorit;

        public bool IsFavorite
        {
            get { return isFavorit; }
            set { isFavorit = value; NotifyOfPropertyChange(); }
        }


        public string SituationString
        {
            get
            {
                switch (Data.situation)
                {
                    case 50: return "Nationale Überflutungen";
                    case 40: return "Regionale Überflutungen";
                    case 30: return "Alarmgrenze";
                    case 20: return "Warngrenze";
                    case 10: return "Normalwasser";
                    case -10: return "Niedrigwasser";
                    case 100: return "Unbekannt";
                }
                return "Unbekannt";
            }
        }

        public string TrendString
        {
            get
            {
                switch (Data.trend)
                {
                    case 10: return "steigend";
                    case 0: return "gleichbleibend";
                    case -10: return "fallend";
                    case 100: return "Unbekannt";
                }
                return "Unbekannt";
            }
        }

        public SolidColorBrush SituationColor
        {
            get
            {
                var c = Colors.Gray;
                switch (Data.situation)
                {
                    case 50:
                    case 40:
                        c = Colors.Red; break;
                    case 30:
                        c = Colors.Yellow; break;
                    case 20:
                    case 10:
                    case -10:
                        c = Colors.Green; break;
                    case 100:
                    case -100:
                        c = Colors.Gray; break;
                        //TODO: Out of date?
                }
                return new SolidColorBrush(c);
            }
        }
    }

    public class UIMonitoredStation : UIStation
    {
        public double MonitoredValue { get; set; }
        public string MonitoredValueTypeString { get; set; }
        public double AlarmValue { get; set; }
        public string WaterKindStringPlain { get; set; }
    }
}
