using System;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class ScheduleWorktime
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ScheduleWorktimeID { get; set; }

        /// <summary>
        /// Время начала
        /// </summary>
        public TimeSpan StartTime { get; set; }
        /// <summary>
        /// Время конца
        /// </summary>
        public TimeSpan EndTime { get; set; }
        /// <summary>
        /// Идентификатор элемента расписания
        /// </summary>
        public int ScheduleItemID { get; set; }
        /// <summary>
        /// Элемент расписания
        /// </summary>
        public virtual ScheduleItem ScheduleItem { get; set; }
    }
}
