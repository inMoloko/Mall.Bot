using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class MapObjectLink
    {
        [Key, Column(Order = 1)]
        public int MapObjectFromID { get; set; }
        [ForeignKey("MapObjectFromID")]
        public MapObject MapObjectFrom { get; set; }

        [Key, Column(Order = 2)]
        public int MapObjectToID { get; set; }
        [ForeignKey("MapObjectToID")]
        public MapObject MapObjectTo { get; set; }
    }
}
