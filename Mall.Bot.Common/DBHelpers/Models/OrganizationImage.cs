using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class OrganizationImage
    {
        [Key, Column(Order = 1)]
        public int OrganizationID { get; set; }
        [Key, Column(Order = 2)]
        public string Type { get; set; }
        public string Extension { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
