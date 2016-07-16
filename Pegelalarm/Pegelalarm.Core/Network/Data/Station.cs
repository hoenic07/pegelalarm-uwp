using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Network.Data
{
    public class Station
    {
        public string name { get; set; }
        public string commonid { get; set; }
        public string country { get; set; }
        public string stationName { get; set; }
        public string water { get; set; }
        public string region { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public double defaultWarnValueM3s { get; set; }
        public double defaultAlarmValueM3s { get; set; }
        public Datum[] data { get; set; }
        public int trend { get; set; }
        public int situation { get; set; }
        public string visibility { get; set; }
        public float positionKm { get; set; }
        public float altitudeM { get; set; }
        public double defaultWarnValueCm { get; set; }
        public double defaultAlarmValueCm { get; set; }


        public string SituationString
        {
            get
            {
                switch (situation)
                {
                    case 50: return "Nationale Überflutungen";
                    case 40: return "Regionale Überflutungen";
                    case 30: return "Alarmgrenze";
                    case 20: return "Warngrenze";
                    case 10: return "Normalwasser";
                    case -10:return "Niedrigwasser";
                    case 100:return "Unbekoannt";
                }
                return "Unbekannt";
            }
        }

        public string TrendString
        {
            get
            {
                switch (trend)
                {
                    case 10: return "steigend";
                    case 0: return "gleichbleibend";
                    case -10: return "fallend";
                    case 100:return "Unbekannt";
                }
                return "Unbekannt";
            }
        }
    }

    public class Datum
    {
        public string type { get; set; }
        public float value { get; set; }
        public string requestDate { get; set; }
        public string sourceDate { get; set; }
    }
}
