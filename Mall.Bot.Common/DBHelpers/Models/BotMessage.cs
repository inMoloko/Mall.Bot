using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotMessage
    {
        [Key]
        public int BotMessageID { get; set; }
        public string BotUserVKID { get; set; }
        public string Text { get; set; }
        public byte[] Photo { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsSended { get; set; }
    }
}
