using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class SystemSettingHistory
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int SystemSettingHistoryID { get; set; }
        /// <summary>
        /// Значение, которое задал пользователь
        /// </summary>
        public string SettingValue { get; set; }

        public DateTime ModifiedDate { get; set; }
        public int SystemSettingID{ get; set; }

    }
}
