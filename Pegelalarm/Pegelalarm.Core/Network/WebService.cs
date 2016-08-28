using Newtonsoft.Json;
using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private const string LIST_ENDPOINT = "station/1.0/list?";
        private const string HISTORY_ENDPOINT = "station/1.0/";

        ///api/station/1.0/211458-at/threshold/stats
        ///api/webcam/1.0/list

        public async Task<Response<Station[]>> GetStationsBy(GeoboundingBox boundingBox = null, string nameFilter = null)
        {
            var uri = ApiConstants.BASE_URI + LIST_ENDPOINT;

            if (boundingBox != null) uri += string.Format(CultureInfo.InvariantCulture,"bBoxLon1={0}&bBoxLat1={1}&bBoxLon2={2}&bBoxLat2={3}&", boundingBox.NorthwestCorner.Longitude, boundingBox.NorthwestCorner.Latitude, boundingBox.SoutheastCorner.Longitude, boundingBox.SoutheastCorner.Latitude);
            if (!string.IsNullOrWhiteSpace(nameFilter)) uri += string.Format("q={0}", nameFilter);

            var obj = await GetJsonUri<ListResponse>(uri);

            return new Response<Station[]>
            {
                Payload = obj.Item1?.payload?.stations,
                StatusCode = obj.Item2
            };
        }

        public async Task<Response<Sample[]>> GetStationHistory(string id, string type, Granularity granularity, DateTime start, DateTime end)
        {
            var uri = ApiConstants.BASE_URI + HISTORY_ENDPOINT + $"{type}/{id}/history?granularity={granularity.ToString().ToLower()}&loadStartDate={DateToUri(start)}&loadEndDate={DateToUri(end)}";
            var obj = await GetJsonUri<HistoryResponse>(uri);

            return new Response<Sample[]>()
            {
                Payload = obj.Item1?.payload?.history,
                StatusCode = obj.Item2
            };
        }

        private async Task<Tuple<T,HttpStatusCode>> GetJsonUri<T>(string uri)
        {
            try
            {
                var res = await (new HttpClient().GetAsync(uri));
                Debug.WriteLine("API > Request " + uri);
                var str = await res.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(str);
                Debug.WriteLine("API > Response: " + res.StatusCode);
                return new Tuple<T, HttpStatusCode>(obj, res.StatusCode);
            }
            catch(Exception ex)
            {
                return new Tuple<T, HttpStatusCode>(default(T), HttpStatusCode.NotFound);
            }
            
        }

        private string DateToUri(DateTime date)
        {
            var firstPart = date.ToString("dd.MM.yyyyTHH:mm:ss");

            var second = Uri.EscapeDataString(date.ToString("zzz").Replace(":", ""));

            return firstPart + second;
        }

    }
}
