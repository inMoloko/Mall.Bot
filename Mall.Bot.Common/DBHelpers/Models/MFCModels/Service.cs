using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class Service
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int AisMFCID { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Отображаемое наименование
        /// </summary>
        [Required]
        public string DisplayName { get; set; }
        /// <summary>
        /// Дата последнего редактирования
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Идентификатор заказчика
        /// </summary>
        public Nullable<int> CustomerID { get; set; }
        /// <summary>
        /// Уровень популярности
        /// </summary>
        public Nullable<int> Priority { get; set; }
    }
}
