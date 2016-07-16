using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls.Maps;

namespace Pegelalarm.Core.Data
{

    public enum MapItemKind { Station, Location, Home }

    public class MapItem
    {
        public MapIcon Icon { get; set; }

        public MapItemKind Kind { get; set; }

        public string ID { get; set; }

        public MapItem(double lat, double lon, MapItemKind kind, Station st)
        {
            Icon = new MapIcon();
            Icon.Location = new Geopoint(new BasicGeoposition { Latitude = lat, Longitude = lon });
            Icon.Image = ImageForKind(kind, st != null ? st.situation : 0);
            Icon.NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1);
            this.Kind = kind;
            this.ID = st != null ? st.commonid : "";
        }


        private IRandomAccessStreamReference ImageForKind(MapItemKind kind, int stationSituation)
        {
            var name = "";

            switch (kind)
            {
                case MapItemKind.Location:
                    name = "my_location"; break;
                case MapItemKind.Home:
                    name = "my_home"; break;
                case MapItemKind.Station:
                    if(stationSituation== -100)
                    {
                        //blue = out of date
                        name = "out_of_date"; break;
                    }
                    else if (stationSituation == 100)
                    {
                        //gray
                        name = "unknown"; break;
                    }
                    else if (stationSituation >= 40)
                    {
                        //red
                        name = "alarm_limit_reached"; break;
                    }
                    else if (stationSituation >= 30)
                    {
                        //yellow
                        name = "report_limit_reached"; break;
                    }
                    else
                    {
                        //normal
                        name = "normal"; break;
                    }
                    break;
            }


            var uri = "ms-appx:///Pegelalarm.Core/Assets/Pinpoints/" + name + ".png";

            return RandomAccessStreamReference.CreateFromUri(new Uri(uri, UriKind.RelativeOrAbsolute));
        }


    }
}
