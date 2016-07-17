using Newtonsoft.Json;
using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace Pegelalarm.Core.Network
{
    public class WebService
    {

        /// <summary>
        /// Please visit pegelalarm.at to get this url
        /// </summary>
        private const string SERVER = ApiConstants.BASE_URI;
        private const string LIST_ENDPOINT = "station/1.0/list?";
        private const string HISTORY_ENDPOINT = "";

        public async Task<Response<Station[]>> GetStationsBy(GeoboundingBox boundingBox = null, string nameFilter = null)
        {
            var uri = SERVER + LIST_ENDPOINT;

            if (boundingBox != null) uri += string.Format("bBoxLon1={0}&bBoxLat1={1}&bBoxLon2={2}&bBoxLat2={3}&", boundingBox.NorthwestCorner.Longitude, boundingBox.NorthwestCorner.Latitude, boundingBox.SoutheastCorner.Longitude, boundingBox.SoutheastCorner.Latitude);
            if (!string.IsNullOrWhiteSpace(nameFilter)) uri += string.Format("q={0}", nameFilter);

            var obj = await GetJsonUri<ListResponse>(uri);

            return new Response<Station[]>
            {
                Payload = obj.Item1.payload.stations,
                StatusCode = obj.Item2
            };
        }

        private async Task<Tuple<T,HttpStatusCode>> GetJsonUri<T>(string uri)
        {
            var res = await (new HttpClient().GetAsync(uri));
            Debug.WriteLine("API > Request " + uri);
            var str = await res.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(str);
            Debug.WriteLine("API > Response: " + res.StatusCode);
            return new Tuple<T, HttpStatusCode>(obj, res.StatusCode);
        }

    }
}
