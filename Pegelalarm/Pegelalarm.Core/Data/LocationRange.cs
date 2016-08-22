using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Data
{
    public class LocationRange
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public int DisplayRadius { get; set; }
        public int AlarmRadius { get; set; }
    }
}
