using Caliburn.Micro;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Core.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
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
                if (Data.data.All(d => d.IsOutOfDate)) return "Keine aktuellen Daten";

                switch (Data.situation)
                {
                    case 50: return "Alarmgrenze erreicht";
                    case 40: return "Alarmgrenze erreicht";
                    case 30: return "Frühwarngrenze erreicht";
                    case 20:
                    case 10: 
                    case -10:
                        return "Normalbereich";
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
                    case 100: return "unbekannt";
                }
                return "unbekannt";
            }
        }

        public Brush SituationColor
        {
            get
            {
                var c = "Gray";
                switch (Data.situation)
                {
                    case 50:
                    case 40:
                        c = "Red"; break;
                    case 30:
                        c = "Yellow"; break;
                    case 20:
                    case 10:
                    case -10:
                        c = "Green"; break;
                    case 100:
                        c = "Blue"; break;
                }

                if (Data.data.All(d => d.IsOutOfDate)) c = "Gray";

                return (Brush)Application.Current.Resources[c+"ColorBrush"];
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
