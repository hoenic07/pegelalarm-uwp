using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Pegelalarm.Core.Utils
{
    public static class GeoUtils
    {
        /// <summary>
        /// Calculates the end-point from a given source at a given range (meters) and bearing (degrees).
        /// This methods uses simple geometry equations to calculate the end-point.
        /// </summary>
        /// <param name="source">Point of origin</param>
        /// <param name="range">Range in meters</param>
        /// <param name="bearing">Bearing in degrees</param>
        /// <returns>End-point from the source given the desired range and bearing.</returns>
        public static Geopoint CalculateDerivedPosition(this Geopoint source, double range, double bearing)
        {
            var latA = source.Position.Latitude * DegreesToRadians;
            var lonA = source.Position.Longitude * DegreesToRadians;
            var angularDistance = range / EarthRadius;
            var trueCourse = bearing * DegreesToRadians;

            var lat = Math.Asin(
                Math.Sin(latA) * Math.Cos(angularDistance) +
                Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            var dlon = Math.Atan2(
                Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA),
                Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));

            var lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;

            return new Geopoint(new BasicGeoposition
            {
                Latitude = lat * RadiansToDegrees,
                Longitude = lon * RadiansToDegrees
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range">meters</param>
        /// <returns></returns>
        public static GeoboundingBox CalculateBoundingBoxAroundPosition(this Geopoint source, double range)
        {
            var rootTwo = Math.Sqrt(2);
            var nw = source.CalculateDerivedPosition(range * rootTwo, 360 - 45);
            var se = source.CalculateDerivedPosition(range * rootTwo, 90 + 45);
            return GeoboundingBox.TryCompute(new List<BasicGeoposition> { nw.Position, se.Position });
        }

        public static double GetDistanceInMetersBetween(this Geopoint from, Geopoint to )
        {
            double lat1 = from.Position.Latitude, lat2 = to.Position.Latitude;
            double lon1 = from.Position.Longitude, lon2 = to.Position.Longitude;

            var dist = DistanceTo(lat1, lon1, lat2, lon2);

            return (dist * 1000.0);
        }

        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }



        private const double DegreesToRadians = Math.PI / 180.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;
        private const double EarthRadius = 6378137.0;
    }
}
