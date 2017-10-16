using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using Moloko.Utils;
using Mall.Bot.Common.FacebookApi.Models;
using Mall.Bot.Common.FacebookApi.Helpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DBHelpers;
using System.Runtime.Caching;
using System.Drawing;
using System.Net.Http.Headers;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.MallHelpers;
using Mall.Bot.Common.MallHelpers.Models;
using System.Configuration;
using System.Data.Entity.Spatial;

namespace Mall.Bot.Api.Controllers
{
    public class FBotController : ApiController
    {
        string _token = "EAAC3tteq77IBAMmMwgkfy5vLZBEEULAZC22ZAO9TpoOMr6b0a1l1zViJZChFqmmHb8OhZAbuiGZCFAYz6kJp7WYzweWGLEpf0Hj4uVrfl1BbVzsh51HTZCypOhJuZB7WZBUTnoozUZBmyicUhNRPQSNlirqGwt49SKk1mugTdrAouOcgZDZD";
        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

        [HttpGet]
        public HttpResponseMessage Get(string key)
        {
            string CachedItemKey = key;
            Image img = null;

            object ImageFromCache = MemoryCache.Default.Get(CachedItemKey, null);

            if (ImageFromCache == null)
            {
                Logging.Logger.Error("There is no cached image!!!");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("There is no cached image"),
                };
            }
            else
            {
                img = (Image)ImageFromCache;
            }

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(ms.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return result;
        }
        [HttpGet] // setting webhook
        public async Task<HttpResponseMessage> Get()
        {
            string s = Url.Request.ToString();

            int i = 0;
            for (i = 0; true; i++)
            {
                if (s[i] == '&') break;
            }
            i += 15;

            string challenge = "";
            for (int j = i; true; j++)
            {
                if (s[j] != '&')
                    challenge += s[j];
                else break;
            }
            return new HttpResponseMessage
            {
                Content = new StringContent(challenge.ToString()),
            };
        }


        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject Responce)
        {
            try
            {
                DateTime TimeToStartAnswer = DateTime.Now;

                FacebookResponce facebookeResponce = Responce.ToObject<FacebookResponce>();

                Logging.Logger.Debug($"Facebook PostMessage message={Responce}");

                if (facebookeResponce == null)
                {
                    Logging.Logger.Error("Facebook. Empty request!");
                    return result;
                }

                if (facebookeResponce.Object != ObjectTypes.page)
                {
                    Logging.Logger.Error($"Facebook. It's not a message!");
                    return result;
                }

                //long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                //if (unixTimestamp - facebookTimestamp > long.Parse(ConfigurationManager.AppSettings["TimeToNotice"]))
                //{
                //    Logging.Logger.Debug($"Faceboooook  !!!!  An old message occurred {unixTimestamp}  sub = {unixTimestamp - facebookTimestamp}");
                //    return result;
                //}

                

                // создание объекта - Бота
                var FacebookBot = new FacebookApiHelper(_token);
                // создание объекта для логирования запроса
                var thisRequest = new BotUserRequest();
                
                // подключаем главную базу
                var dbContextes = new List<MallBotContext>();
                dbContextes.Add(new MallBotContext($"A{ConfigurationManager.AppSettings["dbTest"]}"));
                // Находим пользователя
                var botUsers = dbContextes[0].BotUser.ToList();
                var stringID = facebookeResponce.entry[0].messaging[0].sender.Id.ToString();
                var botUser = botUsers.FirstOrDefault(x => x.BotUserFacebookID == stringID);
                
                if (botUser == null)
                {
                    dbContextes[0] = dbContextes[0].AddBotUser(SocialNetworkType.Facebook, FacebookBot, stringID);
                    botUser = dbContextes[0].BotUser.FirstOrDefault(x => x.BotUserFacebookID == stringID);
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
                long facebookTimestamp = (long)facebookeResponce.entry[0].messaging[0].timestamp / 1000;
                if (ConfigurationManager.AppSettings["IgnoreOldEvents"] == "Enabled" && !TimeHelper.IsNewEvent((int)facebookTimestamp, botUser.BotUserID))
                    return result;

                // помечаем сообщение как прочитанное. 
                thisRequest.IsSendingError = await FacebookBot.SendAction(facebookeResponce.entry[0].messaging[0].sender.Id, SenderActionType.typing_on);

                //проверяем, что сообщение не пусто
                var trimmedLoweredQuery = "";
                if (!string.IsNullOrWhiteSpace(facebookeResponce.entry[0].messaging[0].message?.text)) trimmedLoweredQuery = facebookeResponce.entry[0].messaging[0].message.text.ToLower().Trim(AnalyseHelper.splitters);
                else
                {
                    if (facebookeResponce.entry[0].messaging[0].postback != null)
                    {
                        trimmedLoweredQuery = facebookeResponce.entry[0].messaging[0].postback.payload;
                        facebookeResponce.entry[0].messaging[0].message = new FacebookMessage { text = trimmedLoweredQuery };
                    }
                }

                if (string.IsNullOrWhiteSpace(trimmedLoweredQuery) || (facebookeResponce.entry[0].messaging[0].message?.attachments != null && facebookeResponce.entry[0].messaging[0].message?.attachments[0].type != AttachmentType.location)) //что-то непонятное пришло
                {
                    var simpleAnaliser = new AnalyseHelper();
                    thisRequest.IsSendingError = await simpleAnaliser.AnalyseBadRequest(botUser, SocialNetworkType.Facebook, FacebookBot, dbContextes[0].BotText.ToList());
                    dbContextes[0].AddBotQuery(botUser, null, TimeToStartAnswer, thisRequest);
                    return result;
                }
                
                var analyser = new MainAnswerHelper(thisRequest, FacebookBot, SocialNetworkType.Facebook, botUser, dbContextes);
                FindedInformation answer;
                // если была прислана геолокация, то ищем по ней. Иначе по тексту
                if (facebookeResponce.entry[0].messaging[0].message?.attachments != null && facebookeResponce.entry[0].messaging[0].message?.attachments[0].type == AttachmentType.location)
                {
                    botUser.InputDataType = InputDataType.GeoLocation;
                    botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;

                    var temp = $"POINT({facebookeResponce.entry[0].messaging[0].message.attachments[0].payload.coordinates.Long} {facebookeResponce.entry[0].messaging[0].message.attachments[0].payload.coordinates.lat})";
                    temp = temp.Replace(',', '.');
                    thisRequest.Text = temp;

                    answer = await analyser.Main(DbGeography.FromText(temp));
                }
                else
                {
                    botUser.InputDataType = InputDataType.Text;
                    thisRequest.Text = facebookeResponce.entry[0].messaging[0].message.text;
                    answer = await analyser.Main(trimmedLoweredQuery);
                }

                //сохраняем полезные данные по юзеру
                botUser.LastActivityDate = DateTime.Now;
                dbContextes[0].SaveChanges();

                //пишем в базу запрос
                dbContextes[0].AddBotQuery(botUser, answer, TimeToStartAnswer, thisRequest);
                return result;
            }
            catch (Exception ex)
            {
                Logging.Logger.Error(ex, "Facebook");
                return result;
            }
        }
    }
}
