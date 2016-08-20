using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Data
{
    public class Timeframe
    {
        public string Name { get; set; }
        public DateTime FromDate { get; set; }

        public Granularity Granularity { get; set; }
    }
}
