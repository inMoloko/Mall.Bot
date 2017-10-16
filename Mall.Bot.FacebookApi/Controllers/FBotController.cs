using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;
using Mall.Bot.Common.VKApi;
using Newtonsoft.Json.Linq;
using Moloko.Utils;
using Mall.Bot.Common.FacebookApi.Models;
using Mall.Bot.Common.FacebookApi.Helpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common;
using Mall.Bot.Common.DBHelpers;
using System.Runtime.Caching;
using System.Configuration;
using System.Drawing;
using System.Net.Http.Headers;

namespace Mall.Bot.FacebookApi.Controllers
{
    public class FBotController : ApiController
    {
        string _token = "EAAC3tteq77IBAMmMwgkfy5vLZBEEULAZC22ZAO9TpoOMr6b0a1l1zViJZChFqmmHb8OhZAbuiGZCFAYz6kJp7WYzweWGLEpf0Hj4uVrfl1BbVzsh51HTZCypOhJuZB7WZBUTnoozUZBmyicUhNRPQSNlirqGwt49SKk1mugTdrAouOcgZDZD";

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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(challenge, Encoding.UTF8, "text/html")
            };
        }


        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject Responce)
        {
            DateTime TimeToStartAnswer = DateTime.Now;
            FacebookResponce facebookeResponce = Responce.ToObject<FacebookResponce>();

            if (facebookeResponce == null)
            {
                Logging.Logger.Error("Facebook. Empty request!");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (facebookeResponce.Object != ObjectTypes.page)
            {
                Logging.Logger.Error($"Facebook. It's not a text message!");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                var FacebookBot = new FacebookApiHelper(_token);
                var thisQuery = new BotUserQuery { IsError = 0 };
                await FacebookBot.SendAction(facebookeResponce.entry[0].messaging[0].sender.Id, SenderActionType.typing_on, thisQuery);
                if (facebookeResponce.entry[0].messaging[0].message.text != null) // пришло тектовое сообщение
                {
                    try
                    {
                        string trimmedLoweredQuery = facebookeResponce.entry[0].messaging[0].message.text.ToLower().Trim(QueryAnaliser.splitters);

                        var mbFunctional = new MallBotFunctional();
                        var mbDBHelper = new MallBotDBHelper();
                        var mbApiFunctional = new MallBotFacebookApiFunctional();

                        // Кэширование
                        #region
                        string MainCachedItemKey = "MainDataOfBot";
                        string RadugaCachedItemKey = "RadugaDataOfBot";
                        MallBotModel MainDataOfBot = null;
                        MallBotModel RadugaDataOfBot = null;

                        var dbMainContext = new MallBotContext(); dbMainContext.Configuration.ProxyCreationEnabled = false;
                        var dbRadugaContext = new MallBotContext(1); dbMainContext.Configuration.ProxyCreationEnabled = false;

                        object MaindataFromCache = MemoryCache.Default.Get(MainCachedItemKey, null);
                        object RadugadataFromCache = MemoryCache.Default.Get(RadugaCachedItemKey, null);

                        if (trimmedLoweredQuery == "update")
                        {
                            if (MaindataFromCache != null) MemoryCache.Default.Remove(MainCachedItemKey, null);
                            if (RadugadataFromCache != null) MemoryCache.Default.Remove(RadugaCachedItemKey, null);

                            MaindataFromCache = null;
                            RadugadataFromCache = null;
                        }

                        if (MaindataFromCache == null || RadugadataFromCache == null)
                        {
                            List<int?> ids = new List<int?> { 3, 5 };
                            MainDataOfBot = new MallBotModel(dbMainContext, ids);

                            ids = new List<int?> { 1 };
                            RadugaDataOfBot = new MallBotModel(dbRadugaContext, ids);

                            string[] TimeOfExpiration = ConfigurationManager.AppSettings["TimeOfExpiration"].ToString().Split(':');
                            CacheItemPolicy cip = new CacheItemPolicy()
                            {
                                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(int.Parse(TimeOfExpiration[0])).AddMinutes(int.Parse(TimeOfExpiration[1])).AddSeconds(int.Parse(TimeOfExpiration[2])))
                            };
                            MemoryCache.Default.Set(new CacheItem(MainCachedItemKey, (object)MainDataOfBot), cip);
                            MemoryCache.Default.Set(new CacheItem(RadugaCachedItemKey, (object)RadugaDataOfBot), cip);
                        }
                        else
                        {
                            MainDataOfBot = (MallBotModel)MaindataFromCache;
                            RadugaDataOfBot = (MallBotModel)RadugadataFromCache;
                        }
                        #endregion

                        var botUsers = dbMainContext.BotUser.ToList();
                        var botUser = botUsers.FirstOrDefault(x => x.BotUserFacebookID == facebookeResponce.entry[0].messaging[0].sender.Id);
                        QueryAnaliserResult answer = null;
                        MallBotModel DataOfBot = null;
                        var customers = MainDataOfBot.Customers;
                        customers.AddRange(RadugaDataOfBot.Customers);

                        var IsTutorial = true;

                        // Блок для описания команд. Приоритет 1. Наивысший
                        if (
                        #region
                                botUser == null ||
                                trimmedLoweredQuery == "place" ||
                                trimmedLoweredQuery == "место" ||
                                trimmedLoweredQuery == "сменить тц" ||
                                trimmedLoweredQuery == "тц" ||
                                trimmedLoweredQuery == "mall" ||
                                botUser.CustomerName == "newuser" ||
                                trimmedLoweredQuery == "help" && (botUser.CustomerName == "newuser" || botUser.CustomerName == "empty") ||
                                trimmedLoweredQuery == "помощь" && (botUser.CustomerName == "newuser" || botUser.CustomerName == "empty")
                        #endregion
                            )
                        {
                            string message = "";
                            switch (trimmedLoweredQuery)
                            {
                                case "help":
                                case "помощь":
                                    message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%priorityhelp%").Text;
                                    if (botUser.Locale == "ru_RU")
                                    {
                                        message = message.Replace("%place%", "место");
                                    }
                                    else
                                    {
                                        message = message.Replace("%place%", "place");
                                    }

                                    await FacebookBot.SendMessage(facebookeResponce.entry[0].messaging[0].sender.Id, message, thisQuery);
                                    break;

                                default:
                                    if (botUser == null)
                                    {
                                        FacebookUser facebookUser = await FacebookBot.GetUsersInformation(facebookeResponce.entry[0].messaging[0].sender.Id, thisQuery);
                                        byte gender = 0;
                                        if (facebookUser.gender == "male") gender = 2;
                                        else gender = 1;

                                        dbMainContext = mbDBHelper.AddBotUser(ulong.Parse(facebookeResponce.entry[0].messaging[0].sender.Id), 3, null, facebookUser.first_name, facebookUser.last_name, gender, null, facebookUser.locale, false, dbMainContext);
                                        string senderId = facebookeResponce.entry[0].messaging[0].sender.Id;
                                        botUser = dbMainContext.BotUser.FirstOrDefault(x => x.BotUserFacebookID == senderId);
                                    }
                                    message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselect%").Text;
                                    message = message.Replace("%mall1%", "1. " + customers[0].Name + "  " + customers[0].City);
                                    message = message.Replace("%mall2%", "2. " + customers[1].Name + "  " + customers[1].City);
                                    message = message.Replace("%mall3%", "3. " + customers[2].Name + "  " + customers[2].City);

                                    string[] quickReplies = { customers[0].Name, customers[1].Name, customers[2].Name };

                                    await FacebookBot.SendMessage(facebookeResponce.entry[0].messaging[0].sender.Id, message, thisQuery, quickReplies);

                                    botUser.CustomerName = "empty";
                                    if (botUser.LevelTutorial != 4) botUser.LevelTutorial = 0;
                                    botUser.ModifiedDate = DateTime.Now;
                                    dbMainContext.SaveChanges();
                                    break;
                            }
                        }
                        else
                        {
                            if (botUser.CustomerName == "empty")
                            {
                                var qa = new QueryAnaliser();
                                var res = mbFunctional.SearchCustomer(qa.NormalizeQuery(facebookeResponce.entry[0].messaging[0].message.text), customers);

                                if (res.Count != 0)
                                {
                                    var findedCustomer = customers.FirstOrDefault(x => x.CustomerID == res[0].ID);

                                    var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselectsucces%").Text;
                                    message = message.Replace("%findedmall%", findedCustomer.Name + ", " + findedCustomer.City);
                                    if (botUser.Locale == "ru_RU")
                                    {
                                        message = message.Replace("%place%", "место");
                                    }
                                    else
                                    {
                                        message = message.Replace("%place%", "place");
                                    }

                                    await FacebookBot.SendMessage(facebookeResponce.entry[0].messaging[0].sender.Id, message, thisQuery);
                                    botUser.CustomerName = res[0].Name;
                                    dbMainContext.SaveChanges();
                                    if (botUser.LevelTutorial != 4) answer = await mbApiFunctional.doTutorial(botUser, facebookeResponce.entry[0].messaging[0].message.text, FacebookBot, facebookeResponce.entry[0].messaging[0].sender.Id, dbMainContext, MainDataOfBot, thisQuery);
                                }
                                else
                                {

                                    var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselectfail%").Text;
                                    message = message.Replace("%notfindedmall%", facebookeResponce.entry[0].messaging[0].message.text);

                                    await FacebookBot.SendMessage(facebookeResponce.entry[0].messaging[0].sender.Id,message, thisQuery);
                                }
                            }
                            else
                            {
                                if (botUser.CustomerName == RadugaDataOfBot.Customers[0].Name)
                                {
                                    DataOfBot = RadugaDataOfBot;
                                    DataOfBot.Texts = MainDataOfBot.Texts;
                                }
                                else
                                {
                                    DataOfBot = mbDBHelper.SelectData(MainDataOfBot, botUser, customers);
                                }

                                if (trimmedLoweredQuery.Contains("testfunc")) // команды для разработчиков
                                {
                                    await mbApiFunctional.doDebelopersCommands(trimmedLoweredQuery, DataOfBot, FacebookBot, facebookeResponce.entry[0].messaging[0].sender.Id, thisQuery);
                                }
                                else
                                {
                                    // Блок для описания команд. Приоритет 0
                                    // команды для пользователей
                                    await mbApiFunctional.doUsersCommands(trimmedLoweredQuery, DataOfBot, FacebookBot, facebookeResponce.entry[0].messaging[0].sender.Id, botUser, thisQuery, dbMainContext);
                                }
                            }
                        }
                        if (botUser != null)
                        {
                            if (DataOfBot == null)
                            {
                                mbDBHelper.AddBotQuery(botUser, ulong.Parse(facebookeResponce.entry[0].messaging[0].sender.Id.ToString()), 3, answer, facebookeResponce.entry[0].messaging[0].message.text, IsTutorial, TimeToStartAnswer, thisQuery);
                            }
                            else
                            {
                                mbDBHelper.AddBotQuery(botUser, ulong.Parse(facebookeResponce.entry[0].messaging[0].sender.Id.ToString()), 3, answer, facebookeResponce.entry[0].messaging[0].message.text, IsTutorial, TimeToStartAnswer, thisQuery);
                            }
                        }

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("ok"),
                        };
                    }
                    catch (Exception exc)
                    {
                        Logging.Logger.Error("Facebook"+exc);
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
                else
                {
                    await FacebookBot.SendMessage(facebookeResponce.entry[0].messaging[0].sender.Id, "Sorry, but i didn't understand you \U0001F614", thisQuery);
                }

            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
