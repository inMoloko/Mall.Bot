namespace Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels
{
    public class WeekDaySchedule
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int WeekDayScheduleID { get; set; }

        /// <summary>
        /// День недели
        /// </summary>
        public byte DayOfWeek { get; set; }
        /// <summary>
        /// Идентификатор элемента расписания
        /// </summary>
        public int ScheduleItemID { get; set; }
        /// <summary>
        /// Элемент расписания
        /// </summary>
        public virtual ScheduleItem ScheduleItem { get; set; }
        /// <summary>
        /// Идентификатор расписания на неделю
        /// </summary>
        public int WeekScheduleID { get; set; }
        /// <summary>
        /// Расписание на неделю
        /// </summary>
        public virtual WeekSchedule WeekSchedule { get; set; }
    }
}
