using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class SystemSettingType
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int SystemSettingTypeID { get; set; }
        public string Name { get; set; }
    }
}
