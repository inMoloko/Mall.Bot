using System;
using System.Collections.Generic;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class Schedule
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ScheduleID { get; set; }
        /// <summary>
        /// Расписания конкретных дней (исключения)
        /// </summary>
        public virtual ICollection<DaySchedule> DaySchedule { get; set; } = new HashSet<DaySchedule>();
        /// <summary>
        /// Филиалы
        /// </summary>
        public virtual ICollection<Office> Office { get; set; } = new HashSet<Office>();
        /// <summary>
        /// Расписание на неделю
        /// </summary>
        public virtual ICollection<WeekSchedule> WeekSchedule { get; set; } = new HashSet<WeekSchedule>();
        /// <summary>
        /// Дата последнего редактирования
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
