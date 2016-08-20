using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Network.Data
{

    public class ListResponse
    {
        public Status status { get; set; }
        public PayloadList payload { get; set; }
    }

    public class Status
    {
        public int code { get; set; }
    }

    public class PayloadList
    {
        public Station[] stations { get; set; }
    }

    

}
