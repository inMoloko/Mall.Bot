using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class MapObject
    {
        [Key]
        public int MapObjectID { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int FloorID { get; set; }
        public string Params { get; set; }
        public Floor Floor { get; set; }

        [NotMapped]
        public double LongitudeFixed { get; set; }
        [NotMapped]
        public double LatitudeFixed { get; set; }
        public void FixCoords(Floor f)
        {
            if (f!= null && f.ImportMetadata != null && f.Width != null && f.Height != null)
            {
                string[] param = f.ImportMetadata.Split(';');
                var floorParams = param.Select(p => Convert.ToDouble(p, CultureInfo.InvariantCulture)).ToList();

                LongitudeFixed = (double)(((-Longitude + floorParams[0]) / (floorParams[0] - floorParams[2])) * f.Width - f.Width / 2);
                LatitudeFixed = (double)(((-Latitude + floorParams[1]) / (floorParams[1] - floorParams[3])) * f.Height - f.Height / 2);
            }
        }
    }
}
