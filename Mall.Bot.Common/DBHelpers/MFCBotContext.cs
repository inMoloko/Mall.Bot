using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DBHelpers.Models.MFCModels;
using Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels;
using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using System;
using System.Data.Entity;

namespace Mall.Bot.Common.DBHelpers
{
    public class MFCBotContext : DbContext
    {
        public MFCBotContext(string connStringName) : base(connStringName)
        {
            Database.SetInitializer<MallBotContext>(null);
        }

        public MFCBotContext(string connStringName, bool IsTest) : base(connStringName+"Test")
        {
            Database.SetInitializer<MallBotContext>(null);
        }

        public DbSet<Models.MFCModels.Customer> Customer { get; set; }
        public DbSet<Office> Office { get; set; }
        public DbSet<OfficeQueue> OfficeQueue { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Section> Section { get; set; }
        public DbSet<SectionOffice> SectionOffice { get; set; }
        public DbSet<WindowsOffice> WindowsOffice { get; set; }
        public DbSet<BotSession> BotSession { get; set; }


        // common models
        public DbSet<Models.MFCModels.BotUser> BotUser{ get; set; }
        public DbSet<BotText> BotText { get; set; }
        public DbSet<BotJoke> BotJoke { get; set; }
        public DbSet<BotUserRequest> BotUserRequest { get; set; }
        public DbSet<AISMFCServiceStatus> AISMFCServiceStatus { get; set; }

        ////schedule
        public DbSet<DaySchedule> DaySchedule { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<ScheduleItem> ScheduleItem { get; set; }
        public DbSet<ScheduleWorktime> ScheduleWorktime { get; set; }
        public DbSet<WeekDaySchedule> WeekDaySchedule { get; set; }
        public DbSet<WeekSchedule> WeekSchedule { get; set; }

        /// <summary>
        /// Добавление пользователя
        /// </summary>
        /// <param name="recipientID"></param>
        /// <param name="isJoinGroup"></param>
        /// <returns></returns>
        public MFCBotContext AddBotUser(string recipientID, bool isJoinGroup = false)
        {
            try
            {
                var btusr = new Models.MFCModels.BotUser();

                if (isJoinGroup) btusr.JoinGroupDate = DateTime.Now;

                btusr.BotUserVKID = recipientID;
                btusr.Locale = "ru_RU";
                btusr.LastActivityDate = DateTime.Now;

                //данные для мфц
                btusr.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                btusr.TalonID = 0;
                btusr.OfficeID = 0;
                btusr.ServiceID = 0;

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
        public void AddBotRequest(BotUserRequest btrequest, DateTime TimeToStartAnswer)
        {
            var TimeToAnswer = DateTime.Now.Subtract(TimeToStartAnswer);
            btrequest.TimeToAnswer = TimeToAnswer.Seconds.ToString() + ":" + TimeToAnswer.Milliseconds.ToString();

            BotUserRequest.Add(btrequest);
            SaveChanges();
        }

        public void AddBotSession(int BotUserID,int TalonID, int AisMFCOfficeID, string ServiceName)
        {
            var session = new BotSession();
            session.TalonID = TalonID;
            session.BotUserID = BotUserID;
            session.AisMFCOfficeID = AisMFCOfficeID;
            session.ServiceName  = ServiceName;
            session.StartTime = DateTime.Now;
            BotSession.Add(session);
        }
    }
}
