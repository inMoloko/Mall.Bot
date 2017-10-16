using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotText
    {
        [Key]
        public int BotTextID { get; set; }
        public string Key { get; set; }
        public string Locale { get; set; }
        public string Text { get; set; }
    }
}
