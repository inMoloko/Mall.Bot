using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class SystemSetting
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int SystemSettingID { get; set; }
        public int CustomerID { get; set; }
        public int SystemSettingTypeID { get; set; }
    }
}
