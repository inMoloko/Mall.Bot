using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class OfficeQueue
    {
        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        [Key]
        public int OfficeID { get; set; }
        /// <summary>
        /// Длина очереди
        /// </summary>
        public int QueueSize { get; set; }
        /// <summary>
        /// Дата последнего редактирования
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

    }
}
