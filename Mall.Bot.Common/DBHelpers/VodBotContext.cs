using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DBHelpers.Models.VodModels;
using Moloko.Utils;
using System;
using System.Data.Entity;

namespace Mall.Bot.Common.DBHelpers
{
    public class VodBotContext: DbContext
    {
        public VodBotContext(string connStringName) : base(connStringName)
        {
            Database.SetInitializer<MallBotContext>(null);
        }

        public DbSet<Models.VodModels.BotUser> BotUser { get; set; }
        public DbSet<BotUserRequest> BotUserRequest { get; set; }
        public DbSet<BotText> BotText { get; set; }
        public DbSet<RecognizeStatistic> RecognizeStatistic { get; set; }

        /// <summary>
        /// Добавление пользователя
        /// </summary>
        /// <param name="recipientID"></param>
        /// <param name="isJoinGroup"></param>
        /// <returns></returns>
        public VodBotContext AddBotUser(string recipientID)
        {
            try
            {
                var btusr = new Models.VodModels.BotUser();

                btusr.BotUserVKID = recipientID;
                btusr.Locale = "ru_RU";
                btusr.LastActivityDate = DateTime.Now;
                btusr.NowIs = Helpers.VodBotWhatIsHappeningNow.AddPhoto;

                BotUser.Add(btusr);
                SaveChanges();
                return this;
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return null;
            }
        }

        /// <summary>
        /// Добавление запроса
        /// </summary>
        /// <param name="btrequest"></param>
        /// <param name="TimeToStartAnswer"></param>
        public void AddBotRequest(BotUserRequest btrequest,int BotUserID, DateTime TimeToStartAnswer)
        {
            var TimeToAnswer = DateTime.Now.Subtract(TimeToStartAnswer);
            btrequest.TimeToAnswer = TimeToAnswer.Seconds.ToString() + ":" + TimeToAnswer.Milliseconds.ToString();
            btrequest.DateTime = DateTime.Now;
            btrequest.BotUserID = BotUserID;

            BotUserRequest.Add(btrequest);
            SaveChanges();
        }
    }
}
