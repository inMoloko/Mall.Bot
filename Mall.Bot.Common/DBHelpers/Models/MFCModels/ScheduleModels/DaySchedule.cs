using System;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class DaySchedule
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int DayScheduleID { get; set; }
        /// <summary>
        /// Идентификатор расписания
        /// </summary>
        public Nullable<int> ScheduleID { get; set; }
        /// <summary>
        /// День (дата)
        /// </summary>
        public DateTime Day { get; set; }
        /// <summary>
        /// Идентификатор элемента расписания
        /// </summary>
        public int ScheduleItemID { get; set; }
        /// <summary>
        /// Расписание
        /// </summary>
        public virtual Schedule Schedule { get; set; }
        /// <summary>
        /// Элемент расписания
        /// </summary>
        public virtual ScheduleItem ScheduleItem { get; set; }
    }
}
