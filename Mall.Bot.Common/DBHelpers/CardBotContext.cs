using Mall.Bot.Common.DBHelpers.Models.CardModels;
using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using System;
using System.Data.Entity;

namespace Mall.Bot.Common.DBHelpers
{
    public class CardBotContext:DbContext
    {
        public CardBotContext(string connStringName) : base(connStringName)
        {
            Database.SetInitializer<MallBotContext>(null);
        }

        public DbSet<Models.CardModels.BotUser> BotUser { get; set; }
        public DbSet<Models.BotUserRequest> BotUserRequest { get; set; }
        public DbSet<Models.BotText> BotText { get; set; }
        public DbSet<BotCard> BotCards { get; set; }

        /// <summary>
        /// Добавление пользователя
        /// </summary>
        /// <param name="recipientID"></param>
        /// <param name="isJoinGroup"></param>
        /// <returns></returns>
        public CardBotContext AddBotUser(string recipientID)
        {
            try
            {
                var btusr = new Models.CardModels.BotUser();

                btusr.BotUserVKID = recipientID;
                btusr.Locale = "ru_RU";
                btusr.LastActivityDate = DateTime.Now;
                btusr.NowIs = Helpers.CardBotWhatIsHappeningNow.Start;

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
        public void AddBotRequest(Models.BotUserRequest btrequest, BotUser botUser, DateTime TimeToStartAnswer)
        {
            var TimeToAnswer = DateTime.Now.Subtract(TimeToStartAnswer);
            btrequest.TimeToAnswer = TimeToAnswer.Seconds.ToString() + ":" + TimeToAnswer.Milliseconds.ToString();
            btrequest.DateTime = DateTime.Now;
            btrequest.BotUserID = botUser.BotUserID;
            btrequest.NowIs = (MallBotWhatIsHappeningNow)botUser.NowIs;
            botUser.LastActivityDate = DateTime.Now;
            BotUserRequest.Add(btrequest);
            SaveChanges();
        }

        public bool AddNewCard(Models.CardModels.BotUser botUser, byte[] buffer, string cardName, bool IsShared)
        {
            try
            {
                BotCards.Add(new Models.CardModels.BotCard { BotUserID = botUser.BotUserID, IsShare = IsShared, Name = cardName, Photo = buffer });
                this.SaveChanges();
                return true;
            }
            catch(Exception exc)
            {
                Logging.Logger.Error(exc);
                return false;
            }
        }
    }
}
