using Mall.Bot.Common.DBHelpers.Models.Common;
using Mall.Bot.Common.Helpers;

namespace Mall.Bot.Common.DBHelpers.Models.MFCModels
{
    public class BotUser : BaseBotUser
    {
        /// <summary>
        /// Идентификатор филиала МФЦ
        /// </summary>
        public int OfficeID { get; set; }
        /// <summary>
        /// Идентификатор услуги МФЦ
        /// </summary>
        public int? ServiceID { get; set; }
        /// <summary>
        /// Флаг. Показывает какое действи запрашивает пользователь
        /// </summary>
        public MFCBotWhatIsHappeningNow? NowIs { get; set; }
        /// <summary>
        /// номер талона МФЦ
        /// </summary>
        public int? TalonID { get; set; }
        /// <summary>
        /// Флаг. Показывает появлялся ли этот пользователь раньше
        /// </summary>
    }
}
