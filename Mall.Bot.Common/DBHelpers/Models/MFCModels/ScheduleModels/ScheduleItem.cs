using System;
using System.Collections.Generic;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class ScheduleItem
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ScheduleItemID { get; set; }
        /// <summary>
        /// Признак выходного дня
        /// </summary>
        public bool IsDayOff { get; set; }
        /// <summary>
        /// Начало работы
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        /// <summary>
        /// Конец работы
        /// </summary>
        public TimeSpan? EndTime { get; set; }
        /// <summary>
        /// Расписание на конкретный день
        /// </summary>
        public virtual ICollection<DaySchedule> DaySchedule { get; set; } = new HashSet<DaySchedule>();
        /// <summary>
        /// 
        /// </summary>        
        public virtual ICollection<ScheduleWorktime> ScheduleWorktime { get; set; } = new HashSet<ScheduleWorktime>();
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<WeekDaySchedule> WeekDaySchedule { get; set; } = new HashSet<WeekDaySchedule>();
    }
}
