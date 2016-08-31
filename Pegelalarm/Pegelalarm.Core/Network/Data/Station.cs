using Caliburn.Micro;
using Pegelalarm.Core.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Pegelalarm.Core.Utils;
using System.Runtime.Serialization;
using Pegelalarm.Core.Data;

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

        

    }

    public class Datum
    {
        public string type { get; set; }

        public string TypeString
        {
            get { return type.Contains("height") ? "cm" : "m³/s"; }
        }

        public MetricKind Metric
        {
            get { return type.Contains("height") ? MetricKind.Height:MetricKind.Flow; }
        }


        public string SourceDateString
        {
            get
            {
                if (sourceDate == null) return null;
                var date = sourceDate.DateFromString();

                var diff = DateTime.Now - date;

                if(date.Date == DateTime.Today)
                {
                    return date.ToString("t");
                }
                else if(date.Year==DateTime.Today.Year)
                {
                    return date.ToString("m");
                }
                else
                {
                    return date.ToString("d");
                }
            }
        }

        public float value { get; set; }
        public string requestDate { get; set; }
        public string sourceDate { get; set; }
    }
}
