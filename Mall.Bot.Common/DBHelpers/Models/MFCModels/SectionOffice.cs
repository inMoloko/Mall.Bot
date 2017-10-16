using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class SectionOffice
    {
        [Key, Column(Order = 1)]
        public int SectionID { get; set; }

        [Key, Column(Order = 2)]
        public int OfficeID { get; set; }
    }
}

