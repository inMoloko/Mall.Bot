using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class AISMFCServiceStatus
    {
        [Key]
        public int AISMFCServiceStatusID { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; }
    }
}
