using Moloko.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class PathPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
    /// <summary>
    /// Этаж торгового центра
    /// </summary>
    [DisplayName("Этаж")]
    public partial class Floor : BaseObject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int FloorID { get; set; }
        /// <summary>
        /// Номер этажа (порядок)
        /// </summary>
        public int Number { get; set; }
        
        
        /// <summary>
        /// Описание путей на этаже, по которым могут перемещаться люди (Json)
        /// </summary>

        public string Paths { get; set; }

        [NotMapped]
        public PathPoint[][] PathsFixed { get; set; }

        /// <summary>
        /// Идентификатор торгового центра
        /// </summary>
        public Nullable<int> CustomerID { get; set; }

        public string ImportMetadata { get; set; }
        public string FileExtension { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Type { get; set; }

        public void FixPaths ()
        {
            var paths = JsonConvert.DeserializeObject<PathPoint[][]>(Paths);
            PathsFixed = new PathPoint[paths.Count()][];
            string[] param = ImportMetadata.Split(';');
            var floorParams = param.Select(p => Convert.ToDouble(p, CultureInfo.InvariantCulture)).ToList();

            int i = 0;
            foreach (var item in paths)
            {
                PathsFixed[i] = new PathPoint[2];
                PathsFixed[i][0] = new PathPoint
                {
                    X = (double)((-item[0].X + floorParams[0]) / (floorParams[0] - floorParams[2]) * Width - Width / 2),
                    Y = (double)((-item[0].Y + floorParams[1]) / (floorParams[1] - floorParams[3]) * Height - Height / 2)
                };

                PathsFixed[i][1] = new PathPoint
                {
                    X = (double)((-item[1].X + floorParams[0]) / (floorParams[0] - floorParams[2]) * Width - Width / 2),
                    Y = (double)((-item[1].Y + floorParams[1]) / (floorParams[1] - floorParams[3]) * Height - Height / 2)
                };
                i++;
            }

        }

    }
}
