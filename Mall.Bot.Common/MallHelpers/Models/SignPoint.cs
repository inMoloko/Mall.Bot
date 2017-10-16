using Mall.Bot.Common.DBHelpers.Models;
using Moloko.Utils;
using System;

namespace Mall.Bot.Common.MallHelpers.Models
{
    public class SignPoint
    {
        public double SignPointRadius { get; set; }
        public string SignText { get; set; }
        public double SignPointRadiusFixed { get; set; }

        public void FixSignPointRadius(MapObject latlng, Floor f)
        {
            int R = 6371000; //earth’s radius in metres
            double brng = Math.PI / 2;
            double d = SignPointRadius; //Distance in m

            double lat1 = latlng.Latitude * Math.PI / 180;
            double lng1 = latlng.Longitude * Math.PI / 180;
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) + Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(brng));
            double lng2 = (lng1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(lat1), Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2))) * 180 / Math.PI;

            var mo = new MapObject { Latitude = latlng.Latitude, Longitude = lng2 };

            latlng.FixCoords(f);
            mo.FixCoords(f);

            SignPointRadiusFixed = MapHelper.Distance(new System.Windows.Point(latlng.LatitudeFixed, latlng.LongitudeFixed), new System.Windows.Point(mo.LatitudeFixed, mo.LongitudeFixed));
        }
    }
}
