using Mall.Bot.Common.VKApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Moloko.Utils.Base;
using System.Configuration;
using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.VKApi.Models;
using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using Mall.Bot.Common.MallHelpers;
using Mall.Bot.Common.MallHelpers.Models;
using System.Data.Entity.Spatial;

namespace Mall.Bot.Api.Controllers
{

    public class BotController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok"),
            };

            try
            {
                DateTime TimeToStartAnswer = DateTime.Now;

                var vkResponce = jsonResponce.ToObject<VKResponce>();

                Logging.Logger.Debug($"VK PostMessage message={jsonResponce}");

                if (vkResponce == null)
                {
                    Logging.Logger.Error("VK Empry request");
                    return result;
                }
                if (vkResponce.GroupId != 120366480 && vkResponce.GroupId != 127789119)
                {
                    Logging.Logger.Error($"VK group with ID   {vkResponce.GroupId}  is not supporting by MOLOKO");
                    result.Content =
                        new StringContent($"VK group with ID   {vkResponce.GroupId}  is not supporting by MOLOKO");
                    return result;
                }

                if (vkResponce.Type == "confirmation")
                {
                    if (vkResponce.GroupId == 120366480)
                    {
                        return new HttpResponseMessage
                        {
                            Content = new StringContent("da3aa7a7"),
                        };
                    }
                    if (vkResponce.GroupId == 127789119)
                    {
                        return new HttpResponseMessage
                        {
                            Content = new StringContent("38ffe9fe"),
                        };
                    }
                }

                if (vkResponce.Type == "message_new")
                {
                    var vkMessage = jsonResponce["object"].ToObject<VKMessage>();

                    // создание объекта - Бота
                    VK vk = null;
                    if (vkResponce.GroupId == 120366480)
                    {
                        var token = "af48a9fdfdb50e827c09799047c71bcb1ac8ef0f874000d1e2ba30e416735e535badacec4f0f48af3fed4";
                        vk = new VK(token);
                    }
                    else
                    {
                        var token = "157c278b4e80a8bcade8eab4f4c0a99e2d6bc3f6fb9f0736763e8600e3682e3a0471f126a34a52e37534e";
                        vk = new VK(token);
                    }

                    // создание объекта для логирования запроса
                    var thisRequest = new BotUserRequest();
                    thisRequest.IsSendingError = 0;
                    // помечаем сообщение как прочитанное. 
                    thisRequest.IsSendingError = AsyncHelper.RunSync(() => vk.markAsRead(vkMessage.Id));// сообщение помечено как прочитанное
                    // подключаем главную базу
                    var dbContextes = new List<MallBotContext>();
                    dbContextes.Add(new MallBotContext($"A{ConfigurationManager.AppSettings["dbTest"]}"));
                    // Находим пользователя
                    var botUsers = dbContextes[0].BotUser.ToList();
                    var botUser = botUsers.FirstOrDefault(x => x.BotUserVKID == vkMessage.UserId.ToString());

                    if (botUser == null)
                    {
                        var temp = vkMessage.UserId.ToString();
                        dbContextes[0] = dbContextes[0].AddBotUser(SocialNetworkType.VK, vk, temp);
                        botUser = dbContextes[0].BotUser.FirstOrDefault(x => x.BotUserVKID == temp);
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
                    if (ConfigurationManager.AppSettings["IgnoreOldEvents"] == "Enabled" && !TimeHelper.IsNewEvent(vkMessage.Date, botUser.BotUserID))
                        return result;

                    // сборщик запросов к апи
                    var Requests = new List<VKApiRequestModel>();
                    //проверяем, что сообщение не пусто
                    var trimmedLoweredQuery = vkMessage.Body.ToLower().Trim(AnalyseHelper.splitters);
                    if (string.IsNullOrWhiteSpace(trimmedLoweredQuery) && vkMessage.geo == null)
                    {
                        var simpleAnaliser = new AnalyseHelper();
                        thisRequest.IsSendingError = await simpleAnaliser.AnalyseBadRequest(botUser, SocialNetworkType.VK, vk, dbContextes[0].BotText.ToList(), Requests);
                        // отправка данных во вконтаке
                        thisRequest.IsSendingError = AsyncHelper.RunSync(() => vk.SendAllRequests(Requests));
                        dbContextes[0].AddBotQuery(botUser, null, TimeToStartAnswer, thisRequest);
                        return result;
                    }
                    

                    var analyser = new MainAnswerHelper(thisRequest, vk, SocialNetworkType.VK, botUser, dbContextes, Requests);
                    FindedInformation answer;
                    // если была прислана геолокация, то ищем по ней. Иначе по тексту
                    if (vkMessage.geo != null)
                    {
                        botUser.InputDataType = InputDataType.GeoLocation;
                        botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                        thisRequest.Text = vkMessage.geo.coordinates;

                        var geo = vkMessage.geo.coordinates.Split(' ');
                        var temp = $"POINT({geo[1]} {geo[0]})";
                        temp = temp.Replace(',', '.');
                        answer = await analyser.Main(DbGeography.FromText(temp));
                    }
                    else
                    {
                        botUser.InputDataType = InputDataType.Text;
                        thisRequest.Text = vkMessage.Body;
                        answer = await analyser.Main(trimmedLoweredQuery);
                    }

                    // отправка данных во вконтаке
                    thisRequest.IsSendingError = AsyncHelper.RunSync(() => vk.SendAllRequests(Requests));

                    //сохраняем полезные данные по юзеру
                    botUser.LastActivityDate = DateTime.Now;
                    dbContextes[0].SaveChanges();

                    //пишем в базу запрос
                    dbContextes[0].AddBotQuery(botUser, answer, TimeToStartAnswer, thisRequest);
                    return result;
                }
                if (vkResponce.Type == "group_join")
                {
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logging.Logger.Error(ex, "VK");
                return result;
            }
        }
    }
}
