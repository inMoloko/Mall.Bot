using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models.VodModels
{
    public class RecognizeStatistic
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int StatisticID { get; set; }
        /// <summary>
        /// ID юзера
        /// </summary>
        public int BotUserID { get; set; }
        /// <summary>
        /// То что было распознано по фотке
        /// </summary>
        public string Recognized { get; set; }
        /// <summary>
        /// Ссылка на фото счетчика в файловой системе
        /// </summary>
        public string PhotoLinq { get; set; }
    }
}
