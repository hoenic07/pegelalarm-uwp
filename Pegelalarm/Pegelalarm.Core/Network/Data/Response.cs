using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Network.Data
{
    public class Response<T>
    {
        public T Payload { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccessful
        {
            get
            {
                int code = (int)StatusCode;
                return code >= 200 && code < 300;
            }
        }
    }
}
