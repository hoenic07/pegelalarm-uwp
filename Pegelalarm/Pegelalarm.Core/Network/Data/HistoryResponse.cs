using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Network.Data
{
    public class HistoryResponse
    {
        public Status status { get; set; }
        public PayloadHistory payload { get; set; }
    }

    public class PayloadHistory
    {
        public Sample[] history { get; set; }
    }

    public class Sample
    {
        public double value { get; set; }
        public string sourceDate { get; set; }
    }

}
