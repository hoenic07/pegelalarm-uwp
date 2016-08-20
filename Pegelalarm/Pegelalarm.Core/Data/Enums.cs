using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Data
{
    public enum Granularity
    {
        Raw,
        Hour,
        Day,
        Month,
        Year
    }

    public enum MetricKind
    {
        Height,
        Flow
    }

    public enum WaterKind
    {
        Highwater,
        Lowwater
    }
}
