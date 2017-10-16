using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Bot.Common.DBHelpers.Models.Common
{
    public class BaseBotUser : BaseObject
    {
        public BaseBotUser()
        {
            DateOfBirth = null;
            JoinGroupDate = null;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int BotUserID { get; set; }
        /// <summary>
        /// Локализация
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// ID Вконтакте
        /// </summary>
        public string BotUserVKID { get; set; }
        /// <summary>
        /// ID Telegram
        /// </summary>
        public string BotUserTelegramID { get; set; }
        /// <summary>
        /// ID Facebook
        /// </summary>
        public string BotUserFacebookID { get; set; }
        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Пол
        /// </summary>
        public string Male { get; set; }
        /// <summary>
        /// дата рождения
        /// </summary>
        public Nullable<DateTime> DateOfBirth { get; set; }

        /// <summary>
        /// Дата вступления в группу
        /// </summary>
        public Nullable<DateTime> JoinGroupDate { get; set; }
        /// <summary>
        /// Дата последней активности пользователя
        /// </summary>
        public Nullable<DateTime> LastActivityDate { get; set; }

        [NotMapped]
        public bool IsNewUser { get; set; }
        /// <summary>
        /// Флаг. Определяет тип поиска местопожения
        /// </summary>
        [NotMapped]
        public InputDataType InputDataType { get; set; }

    }
}
