using System;
using System.Collections.Generic;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class WeekSchedule
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int WeekScheduleID { get; set; }
        /// <summary>
        /// Дата начала действия расписания
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Дата окончания действия расписания. Null - бессрочно
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Идентификатор расписания
        /// </summary>
        public int ScheduleID { get; set; }
        /// <summary>
        /// Расписание
        /// </summary>
        public virtual Schedule Schedule { get; set; }
        /// <summary>
        /// Расписания дней недели
        /// </summary>
        public virtual ICollection<WeekDaySchedule> WeekDaySchedule { get; set; } = new HashSet<WeekDaySchedule>();
    }
}
