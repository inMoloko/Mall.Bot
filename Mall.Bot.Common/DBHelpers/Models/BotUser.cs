using Mall.Bot.Common.DBHelpers.Models.Common;
using Mall.Bot.Common.Helpers;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotUser : BaseBotUser
    {
        /// <summary>
        /// Уровень прохождения туториалки
        /// </summary>
        public int LevelTutorial { get; set; }
        /// <summary>
        /// Идентификатор филиала МФЦ
        /// </summary>
        /// <summary>
        /// Включен ли тестовый режим у пользователя
        /// </summary>
        public int IsTestMode { get; set; }
        /// <summary>
        /// ID ТЦ d в котором находтся юзер
        /// </summary>
        public string CustomerCompositeID { get; set; }
        /// <summary>
        /// Флаг. Показывает какое действи запрашивает пользователь
        /// </summary>
        public MallBotWhatIsHappeningNow? NowIs { get; set; }
    }
}
