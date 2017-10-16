using Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class Office
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int OfficeID { get; set; }
        public int AisMFCID { get; set; }
        /// <summary>
        /// Отображаемое наименование
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>
        /// Наименование
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; }
        /// <summary>
        /// Отображаемый адресс
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string DisplayAddress { get; set; }
        /// <summary>
        /// Географические координаты
        /// </summary>
        public DbGeography Geo { get; set; }
        /// <summary>
        /// Признак, что филиал работает, его необходимо отображать
        /// </summary>
        public Nullable<bool> IsActive { get; set; }
        /// <summary>
        /// Дата последнего редактирования
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Идентификатор расписания
        /// </summary>
        public Nullable<int> ScheduleID { get; set; }
        /// <summary>
        /// Расписание
        /// </summary>
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Привязка к муниципалитету
        /// </summary>
        public int? TerritoryID { get; set; }

        [NotMapped]
        public bool? IsOpen = true;
    }
}
