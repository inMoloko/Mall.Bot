using System;
using System.Data.Entity;
using System.Linq;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.VKApi;
using Moloko.Utils;
using Mall.Bot.Common.FacebookApi.Helpers;
using Mall.Bot.Common.Helpers;
using Moloko.Utils.Base;
using Newtonsoft.Json;
using Mall.Bot.Common.VKApi.Models;
using Mall.Bot.Common.FacebookApi.Models;
using Mall.Bot.Common.MallHelpers.Models;

namespace Mall.Bot.Common.DBHelpers
{
    public class MallBotContext : DbContext
    {
        //<add key="DatabaseInitializerForType Mall.Bot.Common.DBHelpers.MallBotContext,Mall.Bot.Common" value="Disabled" />
        public MallBotContext(string connStringName) : base(connStringName)
        {
            Database.SetInitializer<MallBotContext>(null);
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<BotCustomersSetting> BotCustomersSetting { get; set; }
        public DbSet<Floor> Floor { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Organization> Organization { get; set; }
        public DbSet<OrganizationImage> OrganizationImage { get; set; }
        public DbSet<BotUser> BotUser { get; set; }
        public DbSet<BotUserQuery> BotUserQuery { get; set; }
        public DbSet<BotUserRequest> BotUserRequest { get; set; }
        public DbSet<OrganizationSynonym> OrganizationSynonym { get; set; }
        public DbSet<BotText> BotText{ get; set; }
        public DbSet<SystemSettingHistory> SystemSettingHistory { get; set; }
        public DbSet<SystemSettingType> SystemSettingType { get; set; }
        public DbSet<SystemSetting> SystemSetting { get; set; }

        public DbSet<MapObject> MapObject { get; set; }
        public DbSet<MapObjectLink> MapObjectLink { get; set; }
        public DbSet<OrganizationMapObject> OrganizationMapObject { get; set; }
        public DbSet<MTerminal> MTerminal { get; set; }
        public DbSet<TerminalMapObject> TerminalMapObject { get; set; }

        /// <summary>
        /// Добавление пользователя в базу
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Bot"></param>
        /// <param name="recepientID"></param>
        /// <param name="first_name"></param>
        /// <param name="last_name"></param>
        /// <returns></returns>
        public MallBotContext AddBotUser(SocialNetworkType type, object Bot, string recepientID, string first_name = "", string last_name = "")
        {
            try
            {
                var btusr = new BotUser();

                byte sex = 0;
                string bdate = "";
                string locale = "ru_RU";
                VK vk = null;
                
                FacebookApiHelper facebook = null;

                if (type == SocialNetworkType.VK)
                {
                    //Запрашиваем данные о юзере в vk
                    vk = (VK)Bot;
                    var obj = AsyncHelper.RunSync(() => vk.GetUsersInformation(ulong.Parse(recepientID)));
                    if (obj.ToString()[0] == '¡') Logging.Logger.Error("VK. GetUsersInformation", obj.ToString());
                    else
                    {
                        var VKuser = (VKResponceUser)obj;

                        sex = VKuser.response[0].sex;
                        first_name = VKuser.response[0].first_name;
                        last_name = VKuser.response[0].last_name;
                        bdate = VKuser.response[0].bdate;
                    }
                    btusr.BotUserVKID = recepientID;
                }

                if (type == SocialNetworkType.Telegram)
                {
                    btusr.BotUserTelegramID = recepientID;
                }

                if (type == SocialNetworkType.Facebook)
                {
                    facebook = (FacebookApiHelper)Bot;

                    object obj = AsyncHelper.RunSync(() => facebook.GetUsersInformation(recepientID.ToString()));
                    if (obj.ToString()[0] == '¡') Logging.Logger.Error("Facebook. GetUsersInformation", obj.ToString());
                    else
                    {
                        var facebookUser = (FacebookUser)obj;
                        if (facebookUser.gender == "male") sex = 2;
                        else sex = 1;
                        first_name = facebookUser.first_name;
                        last_name = facebookUser.last_name;

                        if (facebookUser.locale != "ru_RU") locale = "en_EN";
                    }
                    btusr.BotUserFacebookID = recepientID;
                }

                btusr.LevelTutorial = 0;

                if (sex != 0) btusr.Male = sex.ToString();

                if (!string.IsNullOrWhiteSpace(first_name) || !string.IsNullOrWhiteSpace(last_name)) btusr.Name = first_name + " " + last_name;

                if (!string.IsNullOrWhiteSpace(bdate))
                {
                    string[] data = bdate.Split('.');
                    if (data.Count() == 3) btusr.DateOfBirth = new DateTime(int.Parse(data[2]), int.Parse(data[1]), int.Parse(data[0]));
                    else if (data.Count() == 2) btusr.DateOfBirth = new DateTime(2096, int.Parse(data[1]), int.Parse(data[0]));
                }

                btusr.Locale = locale;
                btusr.IsTestMode = 0;
                btusr.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                btusr.CustomerCompositeID = "newuser";
                btusr.LastActivityDate = DateTime.Now;

                BotUser.Add(btusr);
                SaveChanges();

                return this;
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return this;
            }
        }
        /// <summary>
        /// Добавление запроса пользователя
        /// </summary>
        /// <param name="botUser"></param>
        /// <param name="type"></param>
        /// <param name="answer"></param>
        /// <param name="query"></param>
        /// <param name="TimeToStartAnswer"></param>
        /// <param name="buRequest"></param>
        public void AddBotQuery(BotUser botUser, FindedInformation answer, DateTime TimeToStartAnswer, BotUserRequest buRequest)
        {
            try
            {
                buRequest.BotUserID = botUser.BotUserID;
                buRequest.NowIs = (MallBotWhatIsHappeningNow)botUser.NowIs;
                if (answer != null && answer.Result != null)
                {
                    buRequest.Answer = JsonConvert.SerializeObject(answer.Result);
                }
                var TimeToAnswer = DateTime.Now.Subtract(TimeToStartAnswer);
                buRequest.TimeToAnswer = TimeToAnswer.Days.ToString() + ":" + TimeToAnswer.Hours.ToString() + ":" + TimeToAnswer.Minutes.ToString() + ":" + TimeToAnswer.Seconds.ToString() + ":" + TimeToAnswer.Milliseconds.ToString();
                buRequest.DateTime = DateTime.Now;
                buRequest.BotUserID = botUser.BotUserID;
                buRequest.CustomerCompositeID = botUser.CustomerCompositeID;
                buRequest.Name = botUser.Name;

                botUser.LastActivityDate = DateTime.Now;

                BotUserRequest.Add(buRequest);
                SaveChanges();
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
            }
        }
    }
}

