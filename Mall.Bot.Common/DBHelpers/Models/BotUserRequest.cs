using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotUserRequest : BaseObject
    {
        /// <summary>
        /// ID запроса
        /// </summary>
        [Key]
        public int BotUserRequestID { get; set; }
        /// <summary>
        /// ID пользователя, что послал запрос
        /// </summary>
        public int BotUserID { get; set; }
        /// <summary>
        /// Флаг-идентификатор того, на каком этапе сейчас находится пользователь
        /// </summary>
        public MallBotWhatIsHappeningNow NowIs { get; set; }
        /// <summary>
        /// Текст запроса // или другая полезная инфа из запроса
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Текст, который бот отправил пользователю
        /// </summary>
        public string Answer { get; set; }
        /// <summary>
        /// Время на обработку запроса
        /// </summary>
        public string TimeToAnswer { get; set; }
        /// <summary>
        /// Составной ключ ТЦ
        /// </summary>
        public string CustomerCompositeID { get; set; }
        /// <summary>
        /// Дата и время запроса
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// ID офиса, относительно которого проводился запрос. 0 - офис не был установлен
        /// </summary>
        public int? OfficeID { get; set; }
        /// <summary>
        /// ID услуги, относитьно которой проводился запрос. 0 - услуга не была выбрана
        /// </summary>
        public int? ServiceID { get; set; }
        /// <summary>
        /// Флаг. В случае ошибки при отправке запроса - 1
        /// </summary>
        public int IsSendingError { get; set; }
    }
}
