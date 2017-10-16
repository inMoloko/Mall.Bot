using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class BotSession
    {
        [Key]
        public int BotSessionID { get; set; }
        public int TalonID{ get; set; }
        public int BotUserID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public int? idState { get; set; }
        public int AisMFCOfficeID { get; set; }
        public string ServiceName { get; set; }
    }
}
