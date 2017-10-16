using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;

namespace Mall.Portal.Tasks
{
    public class PixelToGeoTransformation
    {
        public MallBotContext _context { get; set; }
        public PixelToGeoTransformation(MallBotContext context)
        {
            _context = context;
        }

        public void FixMapObjects(int customerID)
        {
            var map = _context.MapObject.Include("Floor").Where(i => i.Floor.CustomerID == customerID).ToList();
            foreach (MapObject mapObject in map)
            {
                var floor = mapObject.Floor;
                var par = floor.ImportMetadata.Split(';').Select(i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToArray();

                var res = Fix(par[0], par[1], par[2], par[3], (double)floor.Width, (double)floor.Height, mapObject.Longitude, mapObject.Latitude);

                mapObject.Longitude = res[0];
                mapObject.Latitude = res[1];
            }
            _context.SaveChanges();
        }

        /// <summary>
        /// returns fixed longitude and latitude
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private double[] Fix(double longNE, double latNE, double longSW, double latSW, double width, double height, double OLDlong, double OLDlat)
        {
            var kw = (longNE - longSW) / width;
            var kh = (latNE - latSW) / height;

            return new double[] {
            longNE - (OLDlong + width / 2) * kw,
            latNE - (OLDlat + height / 2) * kh };
        }

        public void FixFloorPaths(int customerID)
        {
            var floors = _context.Floor.Where(i => i.CustomerID == customerID).ToList();
            foreach (var floor in floors)
            {
                var paths = JsonConvert.DeserializeObject<PathPoint[][]>(floor.Paths);
                var PathsFixed = new PathPoint[paths.Count()][];
                var par = floor.ImportMetadata.Split(';').Select(p => Convert.ToDouble(p, CultureInfo.InvariantCulture)).ToArray();

                int i = 0;
                foreach (var item in paths)
                {

                    PathsFixed[i] = new PathPoint[2];
                    var res = Fix(par[0], par[1], par[2], par[3], (double)floor.Width, (double)floor.Height, item[0].X, item[0].Y);
                    PathsFixed[i][0] = new PathPoint
                    {
                        X = res[0],
                        Y = res[1]
                    };
                    res = Fix(par[0], par[1], par[2], par[3], (double)floor.Width, (double)floor.Height, item[1].X, item[1].Y);
                    PathsFixed[i][1] = new PathPoint
                    {
                        X = res[0],
                        Y = res[1]
                    };
                    i++;

                    floor.Paths = JsonConvert.SerializeObject(PathsFixed);
                }
            }
            _context.SaveChanges();
        }
    }
}