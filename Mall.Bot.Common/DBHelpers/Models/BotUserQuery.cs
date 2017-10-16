using Moloko.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Bot.Common.DBHelpers.Models
{
    public class BotUserQuery : BaseObject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int BotUserQueryID { get; set; }
        /// <summary>
        /// Текст запроса пользователя
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Дата и время запроса
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Результат поиска первой организации 
        /// </summary>
        public string OrganizationFromAnswer { get; set; }
        /// <summary>
        /// Результат поиска второй организации 
        /// </summary>
        public string OrganizationToAnswer { get; set; }
        /// <summary>
        /// Коэффициент "distance" первой организации
        /// </summary>
        public Nullable<double> OrganizationFromDistance { get; set; }
        /// <summary>
        /// Коэффициент "distance" второй организации
        /// </summary>
        public Nullable<double> OrganizationToDistance { get; set; }
        /// <summary>
        /// Идентификатор того, кто совершил запрос
        /// </summary>
        public int BotUserID { get; set; }
        /// <summary>
        /// флаг
        /// </summary>
        public int? IsTutorial { get; set; }
        /// <summary>
        /// ID ТЦ
        /// </summary>
        public string ThisRequestCustomerName{ get; set; }
        /// <summary>
        /// Логи по отправке сообщение в ВК
        /// </summary>
        public string LoggingRequestsResult { get; set; }
        /// <summary>
        /// Флаг для сигнализации об ошибке со стороны Вконтакте
        /// </summary>
        public int? IsError { get; set; }
        /// <summary>
        /// Время ответа
        /// </summary>
        public string TimeToAnswer { get; set; }

    }
}
