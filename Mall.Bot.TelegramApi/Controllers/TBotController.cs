using Mall.Bot.Common;
using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DrawHelper;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.TelegramApi.Helpers;
using Moloko.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Mall.Bot.TelegramApi.Controllers
{
    public class TBotController : ApiController
    {
        //string _token = "252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg"; // токен тестового бота
        string _token = "241831232:AAEC9Gke3lOrwPBsh24fcqCWWjkkOROeQIU"; // токен боевого бота

        [HttpGet] // setting webhook
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                var _Bot = new Telegram.Bot.Api(_token);
                //await _Bot.SetWebhookAsync("https:/ /server.inmoloko.ru//Mall.Bot.Test/api/tbot"); // установка webHook
                await _Bot.SetWebhookAsync("https://server.inmoloko.ru/api/tbot"); // установка webHook
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok"),
                };
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            DateTime TimeToStartAnswer = DateTime.Now;
            var telegramResponce = jsonResponce.ToObject<Update>();

            Logging.Logger.Debug($"PostMessage message={jsonResponce}");

            if (telegramResponce == null)
            {
                Logging.Logger.Error("Пустой запрос");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }


            if (telegramResponce.Type == UpdateType.MessageUpdate)
            {
                var _Bot = new Telegram.Bot.Api(_token);
                
                await _Bot.SendChatActionAsync(telegramResponce.Message.Chat.Id, ChatAction.Typing);

                if (telegramResponce.Message.Text == null)
                {
                    await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, "Простите, но я не поняла вас \U0001F614");
                }
                else
                {
                    try
                    {
                        // кэшируем
                        #region
                        string trimmedLoweredQuery = telegramResponce.Message.Text.ToLower().Trim(QueryAnaliser.splitters);
                        var thisQuery = new BotUserQuery { IsError = 0 };

                        var mbFunctional = new MallBotFunctional();
                        var mbDBHelper = new MallBotDBHelper();
                        var mbApiFunctional = new MallBotApiTelegramFunctional();

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
                            List<int?> ids = new List<int?>{ 3, 5 };
                            MainDataOfBot = new MallBotModel(dbMainContext, ids);

                            ids = new List<int?> {1};
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
                        var botUser = botUsers.FirstOrDefault(x => x.BotUserTelegramID == telegramResponce.Message.Chat.Id.ToString());
                        QueryAnaliserResult answer = null;
                        MallBotModel DataOfBot = null;
                        var customers = MainDataOfBot.Customers;
                        customers.AddRange(RadugaDataOfBot.Customers);

                        var IsTutorial = true;
                        if (
                        #region
                                botUser == null ||
                                trimmedLoweredQuery == "place" ||
                                trimmedLoweredQuery == "место" ||
                                trimmedLoweredQuery == "сменить тц" ||
                                trimmedLoweredQuery == "тц" ||
                                botUser.CustomerName == "newuser" ||
                                trimmedLoweredQuery == "help" && (botUser.CustomerName == "newuser" || botUser.CustomerName == "empty") ||
                                trimmedLoweredQuery == "помощь" && (botUser.CustomerName == "newuser" || botUser.CustomerName == "empty")
                        #endregion
                            )
                        {
                            if (trimmedLoweredQuery == "help" || trimmedLoweredQuery == "помощь")
                            {
                                var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%priorityhelp%").Text;
                                message = message.Replace("%place%", "/place");
                                message = BotTextHelper.SmileCodesReplace(message);
                                await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                            }
                            else
                            {
                                if (botUser == null)
                                {
                                    dbMainContext = mbDBHelper.AddBotUser(ulong.Parse(telegramResponce.Message.Chat.Id.ToString()), 2, null, telegramResponce.Message.From.FirstName, telegramResponce.Message.From.LastName, 0, null,"ru_RU", false, dbMainContext);
                                    botUser = dbMainContext.BotUser.FirstOrDefault(x => x.BotUserTelegramID == telegramResponce.Message.Chat.Id.ToString());
                                }

                                botUser.CustomerName = "empty";
                                if (botUser.LevelTutorial != 4) botUser.LevelTutorial = 0;
                                botUser.ModifiedDate = DateTime.Now;
                                dbMainContext.SaveChanges();

                                var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselect%").Text;
                                message = message.Replace("%mall1%", "1. " + customers[0].Name + "  " + customers[0].City);
                                message = message.Replace("%mall2%", "2. " + customers[1].Name + "  " + customers[1].City);
                                message = message.Replace("%mall3%", "3. " + customers[2].Name + "  " + customers[2].City);
                                message = BotTextHelper.SmileCodesReplace(message);
                                await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                            }
                        }
                        else
                        {
                            if (botUser.CustomerName == "empty")
                            {
                                var qa = new QueryAnaliser();
                                var res = mbFunctional.SearchCustomer(qa.NormalizeQuery(telegramResponce.Message.Text), customers);

                                if (res.Count != 0)
                                {
                                    var findedCustomer = customers.FirstOrDefault(x => x.CustomerID == res[0].ID);

                                    var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselectsucces%").Text;
                                    message = message.Replace("%findedmall%", findedCustomer.Name + "  " + findedCustomer.City);
                                    message = message.Replace("%place%", "/place");
                                    message = BotTextHelper.SmileCodesReplace(message);
                                    await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);

                                    botUser.CustomerName = res[0].Name;
                                    dbMainContext.SaveChanges();
                                    if (botUser.LevelTutorial != 4) answer = await mbApiFunctional.doTutorial(botUser, telegramResponce.Message.Text, _Bot, telegramResponce.Message.Chat.Id, dbMainContext, MainDataOfBot);
                                }
                                else
                                {
                                    var message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%mallselectfail%").Text;
                                    message = message.Replace("%notfindedmall%", telegramResponce.Message.Text);
                                    message = BotTextHelper.SmileCodesReplace(message);
                                    await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
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

                                // Блок для описания команд
                                if (trimmedLoweredQuery.Contains("testfunc")) // команды для разработчиков
                                {
                                    await mbApiFunctional.doDebelopersCommands(trimmedLoweredQuery, DataOfBot, _Bot, telegramResponce.Message.Chat.Id);
                                }
                                else // команды для пользователей
                                {
                                    var message = "";
                                    switch (trimmedLoweredQuery) 
                                    {
                                        case "start":
                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%start%").Text;
                                            message = message.Replace("%help%", "/help");
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "update":
                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%cacheupdate%").Text;
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "clear":
                                            botUser.LevelTutorial = 0;
                                            botUser.CustomerName = "newuser";
                                            botUser.ModifiedDate = DateTime.Now;
                                            dbMainContext.SaveChanges();

                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%clear%").Text;
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "skip":
                                            botUser.LevelTutorial = 4;
                                            botUser.ModifiedDate = DateTime.Now;
                                            dbMainContext.SaveChanges();

                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%skip%").Text;
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "привет":
                                        case "hello":
                                        case "здравствуйте":
                                        case "пряффки":
                                        case "hi":

                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%hello%").Text;
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "пока":
                                        case "до свидания":
                                        case "спасибо":
                                        case "покеда":
                                        case "досвидули":
                                        case "большоеспасибо":
                                        case "прощай":
                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%thkx%").Text;
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "помощь":
                                        case "help":
                                        case "как это работает":
                                        case "хелп":
                                            message = MainDataOfBot.Texts.FirstOrDefault(x => x.Locale == botUser.Locale && x.Key == "%help%").Text;
                                            message = message.Replace("%place%", "/place");
                                            message = message.Replace("%tutorial%", "/tutorial");
                                            message = message.Replace("%help%", "/help");
                                            message = BotTextHelper.SmileCodesReplace(message);
                                            await _Bot.SendTextMessageAsync(telegramResponce.Message.Chat.Id, message);
                                            break;

                                        case "что делать":
                                        case "tutorial":
                                        case "обучение":
                                        case "туториал":
                                            answer = await mbApiFunctional.doTutorial(botUser, telegramResponce.Message.Text, _Bot, telegramResponce.Message.Chat.Id, dbMainContext, DataOfBot, true);
                                            break;

                                        default:
                                            if (botUser.LevelTutorial != 4)
                                            {
                                                answer = await mbApiFunctional.doTutorial(botUser, telegramResponce.Message.Text, _Bot, telegramResponce.Message.Chat.Id, dbMainContext, DataOfBot);
                                            }
                                            else
                                            {
                                                var mbFunctionalAnalizer = new MallBotFunctionalAnalizeTelegramHelper();
                                                answer = await mbFunctionalAnalizer.AnaliseDoWorkResult(telegramResponce.Message.Text, telegramResponce.Message.Chat.Id, DataOfBot, _Bot, botUser);
                                                IsTutorial = false;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        //пишем  в базу запрос
                        if (botUser != null)
                        {
                            if (DataOfBot == null)
                            {
                                mbDBHelper.AddBotQuery(botUser, ulong.Parse(telegramResponce.Message.Chat.Id.ToString()), 2, answer, telegramResponce.Message.Text, IsTutorial, TimeToStartAnswer, thisQuery);
                            }
                            else
                            {
                                mbDBHelper.AddBotQuery(botUser, ulong.Parse(telegramResponce.Message.Chat.Id.ToString()), 2, answer, telegramResponce.Message.Text, IsTutorial, TimeToStartAnswer, thisQuery);
                            }
                        }

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("ok"),
                        };
                    }
                    catch (Exception exc)
                    {
                        Logging.Logger.Error(exc);
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok"),
            };
        } 
    }
}
