using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotCustomersSetting
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int BotCustomersSettingID { get; set; }
        /// <summary>
        /// ID ТЦ
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        /// Признак о том, что ТЦ в продакшене
        /// </summary>
        public int IsPublish { get; set; }
        /// <summary>
        /// Географическая долгота
        /// </summary>
        public string GeoLongitude { get; set; }
        /// <summary>
        /// Географическая широта
        /// </summary>
        public string GeoLatitude { get; set; }
    }
}
