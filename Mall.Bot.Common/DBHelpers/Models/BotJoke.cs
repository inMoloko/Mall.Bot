using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotJoke
    {
        [Key]
        public int BotJokeID { get; set; }
        /// <summary>
        /// текст шутки
        /// </summary>
        public string Text { get; set; }
    }
}
