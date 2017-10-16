using Mall.Bot.Common.DBHelpers.Models.Common;
using Mall.Bot.Common.Helpers;

namespace Mall.Bot.Common.DBHelpers.Models.VodModels
{
    public class BotUser: BaseBotUser
    {
        /// <summary>
        /// Флаг. Показывает какое действи запрашивает пользователь
        /// </summary>
        public VodBotWhatIsHappeningNow? NowIs { get; set; }
    }
}
