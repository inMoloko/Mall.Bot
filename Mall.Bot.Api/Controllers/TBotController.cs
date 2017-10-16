using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.MallHelpers;
using Mall.Bot.Common.MallHelpers.Models;
using Moloko.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Mall.Bot.Api.Controllers
{
    public class TBotController : ApiController
    {
        //string _token = "252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg"; // токен тестового бота
        string _token = "241831232:AAEC9Gke3lOrwPBsh24fcqCWWjkkOROeQIU"; // токен боевого бота
        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok"),
        };

        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            try
            {
                DateTime TimeToStartAnswer = DateTime.Now;

                var telegramResponce = jsonResponce.ToObject<Update>();

                Logging.Logger.Debug($"Telegram PostMessage message={jsonResponce}");

                if (telegramResponce == null)
                {
                    Logging.Logger.Error("Telegram  Empty request");
                    return result;
                }
                if (telegramResponce.Type == UpdateType.MessageUpdate)
                {
                    // создание объекта - Бота
                    var TelegramBot = new Telegram.Bot.TelegramBotClient(_token);
                    // создание объекта для логирования запроса
                    var thisRequest = new BotUserRequest();
                    
                    
                    // подключаем главную базу
                    var dbContextes = new List<MallBotContext>();
                    dbContextes.Add(new MallBotContext($"A{ConfigurationManager.AppSettings["dbTest"]}"));
                    // Находим пользователя
                    var botUsers = dbContextes[0].BotUser.ToList();
                    var stringID = telegramResponce.Message.Chat.Id.ToString();
                    var botUser = botUsers.FirstOrDefault(x => x.BotUserTelegramID == stringID);

                    if (botUser == null)
                    {
                        dbContextes[0] = dbContextes[0].AddBotUser(SocialNetworkType.Telegram, TelegramBot, stringID, telegramResponce.Message.From.FirstName, telegramResponce.Message.From.LastName);
                        botUser = dbContextes[0].BotUser.FirstOrDefault(x => x.BotUserTelegramID == stringID);
                        botUser.IsNewUser = true;
                    }
                    else
                    {
                        if (botUser.NowIs == MallBotWhatIsHappeningNow.SettingCustomer && TimeHelper.GetMinutes((DateTime)botUser.LastActivityDate) > 240)
                        {
                            botUser.IsNewUser = true;
                        }
                        else botUser.IsNewUser = false;
                        // если пользователь не завершил диалог с ботом и вернулся больше, чем через пол часа, то данные сбросятся к поиску организации. // кэш почистится в MainAnswerHelper
                        if ((botUser.NowIs == MallBotWhatIsHappeningNow.SearchingWay || botUser.NowIs == MallBotWhatIsHappeningNow.GettingAllOrganizations) && TimeHelper.GetMinutes((DateTime)botUser.LastActivityDate) > 30)
                        {
                            botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                        }
                    }

                    //проверка на актуальность сообщения
                    long thisTimestamp = (int)(telegramResponce.Message.Date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    if (ConfigurationManager.AppSettings["IgnoreOldEvents"] == "Enabled" && !TimeHelper.IsNewEvent((int)thisTimestamp, botUser.BotUserID))
                        return result;

                    thisRequest.IsSendingError = 0;
                    await TelegramBot.SendChatActionAsync(telegramResponce.Message.Chat.Id, ChatAction.Typing); // бот сделает вид что набирает сообщение

                    //проверяем, что сообщение не пусто
                    var trimmedLoweredQuery = "";
                    if (!string.IsNullOrWhiteSpace(telegramResponce.Message.Text)) trimmedLoweredQuery = telegramResponce.Message.Text.ToLower().Trim(AnalyseHelper.splitters);

                    if (telegramResponce.Message.Text == null && telegramResponce.Message.Location == null)
                    {
                        var simpleAnaliser = new AnalyseHelper();
                        thisRequest.IsSendingError = await simpleAnaliser.AnalyseBadRequest(botUser, SocialNetworkType.Telegram, TelegramBot, dbContextes[0].BotText.ToList());
                        dbContextes[0].AddBotQuery(botUser, null, TimeToStartAnswer, thisRequest);
                        return result;
                    }

                    var analyser = new MainAnswerHelper(thisRequest, TelegramBot, SocialNetworkType.Telegram, botUser, dbContextes);
                    FindedInformation answer;
                    // если была прислана геолокация, то ищем по ней. Иначе по тексту
                    if (telegramResponce.Message.Location != null)
                    {
                        botUser.InputDataType = InputDataType.GeoLocation;
                        botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                        var temp = $"POINT({telegramResponce.Message.Location.Longitude} {telegramResponce.Message.Location.Latitude})";
                        temp = temp.Replace(',', '.');
                        thisRequest.Text = temp;

                        answer = await analyser.Main(DbGeography.FromText(temp));
                    }
                    else
                    {
                        botUser.InputDataType = InputDataType.Text;
                        thisRequest.Text = telegramResponce.Message.Text;
                        answer = await analyser.Main(trimmedLoweredQuery);
                    }

                    //сохраняем полезные данные по юзеру
                    botUser.LastActivityDate = DateTime.Now;
                    dbContextes[0].SaveChanges();

                    //пишем в базу запрос
                    dbContextes[0].AddBotQuery(botUser, answer, TimeToStartAnswer, thisRequest);
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logging.Logger.Error(ex, "Telegram");
                return result;
            }
        }
    }
}
