using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.CardModels
{
    public class BotCard
    {
        [Key]
        public int BotCardID { get; set; }
        public int BotUserID { get; set; }
        public byte[] Photo { get; set; }
        public string Name { get; set; }
        public bool IsShare { get; set; }
    }
}
