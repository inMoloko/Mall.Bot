using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class OrganizationMapObject
    {
        [Key, Column(Order = 1)]
        public int MapObjectID { get; set; }
        public MapObject MapObject { get; set; }
        [Key, Column(Order = 2)]
        public int OrganizationID { get; set; }
        public Organization Organization { get; set; }
    }
}
