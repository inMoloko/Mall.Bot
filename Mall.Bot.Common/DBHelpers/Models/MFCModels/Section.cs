using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class Section
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int SectionID { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Флаг. Показывает, что на услугу можно записываться
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Уровень популярности
        /// </summary>
        public int? Rating { get; set; }
        /// <summary>
        /// ID родительской кнопки
        /// </summary>
        public int? ParentID { get; set; }
        /// <summary>
        /// Идентификатор заказчика
        /// </summary>
        public int CustomerID { get; set; }
    }
}
