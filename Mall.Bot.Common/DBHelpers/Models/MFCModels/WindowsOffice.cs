using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class WindowsOffice
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int WindowID { get; set; }
        public int Number { get; set; }
    }
}
