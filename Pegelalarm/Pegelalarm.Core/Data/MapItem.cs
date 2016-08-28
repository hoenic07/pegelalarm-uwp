using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Graphics.Display;
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
                    name = "my_home"; break;
                case MapItemKind.Station:

                    switch (stationSituation)
                    {
                        case 50:
                        case 40:
                            name = "red"; break;
                        case 30:
                            name = "yellow"; break;
                        case 20:
                        case 10:
                        case -10:
                            name = "green"; break;
                        case 100:
                        case -100:
                            name = "gray"; break;
                            //TODO: Out of date?
                    }
                    break;
            }

            var px = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = px <= 2 ? 1 : 2;
            var uri = "ms-appx:///Pegelalarm.Core/Assets/Pinpoints/" + name + "_" + size + ".png";

            return RandomAccessStreamReference.CreateFromUri(new Uri(uri, UriKind.RelativeOrAbsolute));
        }


    }
}
