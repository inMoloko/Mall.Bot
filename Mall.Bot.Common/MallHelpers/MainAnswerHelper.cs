using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Common.VKApi;
using Mall.Bot.Search.Models;
using Moloko.Utils;
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Bot.Common.MallHelpers
{
    public class MainAnswerHelper
    {
        /// <summary>
        /// Сборщик запросов ВК
        /// </summary>
        private List<VKApiRequestModel> Requests; // для VK
        /// <summary>
        /// API помошник
        /// </summary>
        private object Bot;
        /// <summary>
        /// telegram/facebook/vk
        /// </summary>
        private SocialNetworkType type;
        /// <summary>
        /// текущий запрос. сбор инфы
        /// </summary>
        private BotUserRequest thisRequest;
        /// <summary>
        /// текущий пользователь
        /// </summary>
        private BotUser botUser;
        /// <summary>
        /// Множество контекстов баз данных. По умолчанию dbContextes[0] - главный
        /// </summary>
        private List<MallBotContext> dbContextes;
        
        /// <summary>
        /// Множество данных из соответствующих контекстов. По умолчанию datasOfBot[0] - главный
        /// </summary>
        private List<CachedDataModel> datasOfBot = null;
        private ICustomer currentCustomer = null;
        private BotTextHelper texter = null;
        private ApiRouter sender = null;
        private GetDataHelper dataGetter = null;
        private CacheHelper cacher = new CacheHelper();

        public MainAnswerHelper(BotUserRequest _thisRequest, object _Bot, SocialNetworkType _type, BotUser _botUser, List<MallBotContext> _dbContextes, List<VKApiRequestModel> _Requests = null)
        {
            botUser = _botUser;
            dbContextes = _dbContextes;
            Requests = _Requests;
            Bot = _Bot;
            type = _type;
            thisRequest = _thisRequest;

            #region Кэшируем
            string CachedItemKey = "MallBotData";
            var datafromCache = cacher.Get(CachedItemKey);
            if (datafromCache == null)
            {
                datasOfBot = cacher.Update(CachedItemKey, dbContextes);
            }
            else datasOfBot = (List<CachedDataModel>)datafromCache;
            #endregion

            texter = new BotTextHelper(botUser.Locale, type, datasOfBot[0].Texts);
            sender = new ApiRouter(type, Bot, botUser, Requests);
            dataGetter = new GetDataHelper(datasOfBot);

            if (botUser.NowIs != MallBotWhatIsHappeningNow.SettingCustomer)
            {
                char usersdbID = botUser.CustomerCompositeID[0];
                var customerID = int.Parse(botUser.CustomerCompositeID.Remove(0, 1));
                currentCustomer = dataGetter.GetStructuredCustomers(true).FirstOrDefault(x => x.DBaseID == usersdbID).Customers.FirstOrDefault(x => x.CustomerID == customerID); //дает возможность работы в тц не из тестового режима
            }
        }

        public async Task<FindedInformation> Main(object usefulData)
        {
            string query = "";
            if (botUser.InputDataType == InputDataType.Text)
            {
                query = usefulData.ToString();
                if (await AnaliseCommands(query) == 1) return null;
            }

            FindedInformation answer;
            var searcher = new Search.Mall.SearchHelper();
            List<FuzzySearchResult> res;
            object alreadyFinded;
            AnswerWithPhotoHelper answererWithPhoto;
            switch (botUser.NowIs)
            {
                case MallBotWhatIsHappeningNow.SettingCustomer:

                    switch(botUser.InputDataType)
                    {
                        case InputDataType.GeoLocation:
                            res = searcher.SearchCustomerByGeocode((DbGeography)usefulData, dataGetter.GetStructuredCustomers(Convert.ToBoolean(botUser.IsTestMode)));
                            break;
                        case InputDataType.Text:
                            alreadyFinded = cacher.Get($"SETCUSTOMER{botUser.BotUserID}");
                            if (alreadyFinded == null) res = searcher.SearchCustomerByName(query, dataGetter.GetStructuredCustomers(Convert.ToBoolean(botUser.IsTestMode)));
                            else res = searcher.SearchCustomerByName(query, dataGetter.GetStructuredCustomers(Convert.ToBoolean(botUser.IsTestMode)), (string)alreadyFinded);
                            break;
                        default:
                            res = new List<FuzzySearchResult>();
                            break;
                    }
                    answer = new FindedInformation { Result = new SearchResult(query, res) };
                    await AnaliseSearchCustomerResult(res);
                    break;
                case MallBotWhatIsHappeningNow.SearchingOrganization:
                    cacher.Clear(botUser.BotUserID);
                    var dataOfbot = dataGetter.GetDataForOneCustomer(currentCustomer.CustomerID, botUser.CustomerCompositeID);
                    res = searcher.SearchOrganization(query, dataOfbot.Organizations.OfType<IOrganization>(), dataOfbot.Categories.OfType<ICategory>(), dataOfbot.Synonyms.OfType<IOrganizationSynonym>());
                    answer = new FindedInformation { Result = new SearchResult(query, res), GroopedResult = BotMapHelper.GroupFuzzySearchResult(res, dataOfbot) };

                    DrawHelper drawer;
                    if (botUser.Locale == "ru_RU")
                    {
                        drawer = new DrawHelper(dataOfbot, answer, $"Этаж %floornumber%   {currentCustomer.Name} {currentCustomer.LocaleCity[0]}");
                    }
                    else
                    {
                        if (currentCustomer.LocaleCity.Length == 1) drawer = new DrawHelper(dataOfbot, answer, $"Floor %floornumber%   {currentCustomer.Name} {currentCustomer.LocaleCity[0]}");
                        else drawer = new DrawHelper(dataOfbot, answer, $"Floor %floornumber%   {currentCustomer.Name} {currentCustomer.LocaleCity[1]}");
                    }
                    answer = drawer.DrawFindedShops();

                    answererWithPhoto = new AnswerWithPhotoHelper(type, answer, sender, botUser, texter, dataOfbot);
                    await answererWithPhoto.AnalyseSearchOrganizationResult();
                    break;
                case MallBotWhatIsHappeningNow.SearchingWay:
                    if (cacher.Get($"FINDEDFIRSTORG{botUser.BotUserID}") == null || query == "нет" || query == "no" || query == "не хочу" || query == "неа" || query == "не надо" || query == "но")
                    {
                        cacher.Clear(botUser.BotUserID);
                        botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                        await sender.SendText(texter.GetMessage("%ready%", "%mall%", currentCustomer.Name, currentCustomer.LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                        return null;
                    }

                    dataOfbot = dataGetter.GetDataForOneCustomer(currentCustomer.CustomerID, botUser.CustomerCompositeID);
                    alreadyFinded = cacher.Get($"SEARCHWAY{botUser.BotUserID}");
                    if (alreadyFinded == null) res = searcher.SearchOrganization(query, dataOfbot.Organizations.OfType<IOrganization>(), dataOfbot.Categories.OfType<ICategory>(), dataOfbot.Synonyms.OfType<IOrganizationSynonym>());
                    else res = searcher.SearchOrganization(query, dataOfbot.Organizations.OfType<IOrganization>().ToList(), dataOfbot.Categories.OfType<ICategory>().ToList(), dataOfbot.Synonyms.OfType<IOrganizationSynonym>().ToList(), (string)alreadyFinded);

                    answer = new FindedInformation { Result = new SearchResult(query, res) };
                    answererWithPhoto = new AnswerWithPhotoHelper(type, answer, sender, botUser, texter, dataOfbot);
                    await answererWithPhoto.AnalyseSearchOrganizationForWayResult();
                    break;
                case MallBotWhatIsHappeningNow.GettingAllOrganizations:
                    alreadyFinded = cacher.Get($"VIEWALLORG{botUser.BotUserID}");
                    dataOfbot = dataGetter.GetDataForOneCustomer(currentCustomer.CustomerID, botUser.CustomerCompositeID);
                    botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;

                    if (alreadyFinded != null && ( query == "да" || query == "хочу" || query == "конечно" || query == "yes" || query == "of corse" || query == "ага"))
                    {
                        answererWithPhoto = new AnswerWithPhotoHelper(type, (FindedInformation)alreadyFinded, sender, botUser, texter, dataOfbot);
                        await answererWithPhoto.AnalyseSearchOrganizationResult(true);
                        return (FindedInformation)alreadyFinded;
                    }
                    await sender.SendText(texter.GetMessage("%ready%", "%mall%", currentCustomer.Name, currentCustomer.LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                    if (alreadyFinded != null) answer = (FindedInformation)alreadyFinded;
                    else answer = null;
                    cacher.Remove($"VIEWALLORG{botUser.BotUserID}");
                    break;
                default:
                    answer = null;
                    break;
            }
            return answer;
        }

        private async Task<int> AnaliseSearchCustomerResult(List<FuzzySearchResult> result)
        {
            string message;
            switch (result.Count)
            {
                case 0:
                    message = GetBeauteCustomersString();
                    if (botUser.IsNewUser)
                    {
                        message = texter.GetMessage("%start%") + "\\r\\n\\r\\n" + texter.GetMessage("%mallsearchstart%", "%malls%", message);
                        await sender.SendText(message);
                    }
                    else
                    {
                        message = texter.GetMessage("%ctmrsearchfail%") + "\\r\\n\\r\\n" + texter.GetMessage("%mallsearchstart%", "%malls%", message);
                        await sender.SendText(message);
                    }
                    return 1;
                case 1:
                    cacher.Remove($"SETCUSTOMER{botUser.BotUserID}");
                    // изменям информацию по пользователю
                    botUser.CustomerCompositeID = result[0].CustomersKey;
                    botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                    botUser.ModifiedDate = DateTime.Now;

                    message = texter.GetMessage("%ctmrsearchoneres%", "%mall%", result[0].Name , result[0].LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsearchstart%", "%mall%", result[0].Name + " " + result[0].LocaleCity);
                    await sender.SendText(message);

                    return 1;
                default:
                    string p = "";
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (botUser.Locale == "ru_RU" || result[i].LocaleCity.Length == 1) p += $"{BotTextHelper.GetEmojiNumber(i + 1)} " + result[i].Name + "  " + result[i].LocaleCity[0] + "\\r\\n";
                        else p += $"{BotTextHelper.GetEmojiNumber(i+1)} " + result[i].Name + "  " + result[i].LocaleCity[1] + "\\r\\n";
                    }
                    await sender.SendText(texter.GetMessage("%ctmrsearchmanyres%", "%malls%",p));

                    string customersIDs = "";
                    foreach (var item in result)
                    {
                        customersIDs += item.CustomersKey + ";";
                    }
                    cacher.Set($"SETCUSTOMER{botUser.BotUserID}", customersIDs, 35);
                    return 1;
            }
        }


        private async Task<int> AnaliseCommands(string query)
        {
            switch (query)
            {
                case "start":
                    botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                    goto case "help";
                case "привет":
                case "здарова":
                case "привяу":
                case "здравствуй":
                case "здравствуйте":
                case "добрый день":
                case "добрый вечер":
                case "доброе утро":
                case "hi":
                case "hello":
                case "hey":
                    await sender.SendText(texter.GetMessage("%hello%"));
                    goto case "help";
                case "спасибо":
                case "благодарю":
                case "thanks":
                    await sender.SendText(texter.GetMessage("%thanks%"));
                    return 1;
                case "прости":
                case "простите":
                case "сорян":
                case "извините":
                case "sorry":
                    await sender.SendText(texter.GetMessage("%sorry%"));
                    return 1;
                case "пока":
                case "до свидания":
                case "досвидули":
                case "goodbye":
                case "bye":
                case "bye bye":
                    await sender.SendText(texter.GetMessage("%bye%"));
                    return 1;
                case "place":
                case "место":
                case "тц":
                    string p = GetBeauteCustomersString();
                    cacher.Clear(botUser.BotUserID);
                    botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                    await sender.SendText(texter.GetMessage("%mallsearchstart%", "%malls%", p));
                    return 1;
                case "clang":
                    if (botUser.Locale == "ru_RU") botUser.Locale = "en_EN";
                    else botUser.Locale = "ru_RU";
                    texter.Locale = botUser.Locale;
                    await sender.SendText(texter.GetMessage("%clang%"));
                    return 1;
                case "testmodeon":
                    botUser.IsTestMode = 1;
                    p = GetBeauteCustomersString();
                    cacher.Clear(botUser.BotUserID);
                    botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                    await sender.SendText(texter.GetMessage("%testmodeon%") + "\\r\\n\\r\\n" +texter.GetMessage("%mallsearchstart%", "%malls%", p));
                    return 1;
                case "testmodeoff":
                    botUser.IsTestMode = 0;
                    p = GetBeauteCustomersString();
                    cacher.Clear(botUser.BotUserID);
                    botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                    await sender.SendText(texter.GetMessage("%testmodeoff%") + "\\r\\n\\r\\n" + texter.GetMessage("%mallsearchstart%", "%malls%", p));
                    return 1;
                case "update":
                    var datasOfBot = cacher.Update("MallBotData", dbContextes);
                    if (datasOfBot != null) await sender.SendText(texter.GetMessage("%update%"));
                    else await sender.SendText(texter.GetMessage("%updatefail%"));
                    return 1;
                case "help":
                case "помощь":
                    switch (botUser.NowIs)
                    {
                        case MallBotWhatIsHappeningNow.SettingCustomer:
                            cacher.Clear(botUser.BotUserID);

                            p = GetBeauteCustomersString();
                            await sender.SendText(texter.GetMessage("%start%") + "\\r\\n\\r\\n" + texter.GetMessage("%mallsearchstart%", "%malls%", p));
                            return 1;
                        case MallBotWhatIsHappeningNow.SearchingOrganization:
                            await sender.SendText(texter.GetMessage("%helpsrchorg%"));
                            return 1;
                        case MallBotWhatIsHappeningNow.SearchingWay:

                            object previousObjAnswer = cacher.Get($"FINDEDFIRSTORG{botUser.BotUserID}");
                            if (previousObjAnswer == null)
                            {
                                botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                                await sender.SendText(texter.GetMessage("%ready%", "%mall%", currentCustomer.Name, currentCustomer.LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                                return 1;
                            }
                            var tmp = (FindedInformation)previousObjAnswer;
                            await sender.SendText(texter.GetMessage("%helpsrchway%", "%shop%", tmp.Result.QueryResults[0].Name));
                            return 1;
                        case MallBotWhatIsHappeningNow.GettingAllOrganizations:
                            await sender.SendText(texter.GetMessage("%helpgetallorg%"));
                            return 1;
                    }
                    return 0;

                case "back":
                case "назад":
                    cacher.Clear(botUser.BotUserID);
                    switch (botUser.NowIs)
                    {
                        case MallBotWhatIsHappeningNow.SettingCustomer:
                            p = GetBeauteCustomersString();
                            await sender.SendText(texter.GetMessage("%mallsearchstart%", "%malls%", p));
                            return 1;
                        case MallBotWhatIsHappeningNow.SearchingOrganization:
                            botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                            p = GetBeauteCustomersString();
                            await sender.SendText(texter.GetMessage("%mallsearchstart%", "%malls%", p));
                            return 1;
                        case MallBotWhatIsHappeningNow.SearchingWay:
                        case MallBotWhatIsHappeningNow.GettingAllOrganizations:
                            botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                            await sender.SendText(texter.GetMessage("%ready%", "%mall%", currentCustomer.Name, currentCustomer.LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                            return 1;
                    }
                    return 0;
                default: //тут попытаемся распознать код
                    var code = new CodeModel(query);
                    if (code.IsError) return 0; 
                    else //распознали без ошибок
                    {
                        #region Определяем, что за ТЦ
                        var customers = dataGetter.GetStructuredCustomers(true);
                        char usersdbID = new char();
                        foreach (var item in customers)
                        {
                            currentCustomer = item.Customers.FirstOrDefault(x => x.Synonym?.ToLower() == code.Synonym);
                            if (currentCustomer != null)
                            {
                                usersdbID = item.DBaseID;
                                break;
                            }
                        }
                        #endregion
                        if (currentCustomer == null) // если из кода не понятно что за ТЦ, то игнорим
                        {
                            botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                            await sender.SendText(texter.GetMessage("%parsecodeerr%"));
                            return 1;
                        }

                        var dataOfbot = dataGetter.GetDataForOneCustomer(currentCustomer.CustomerID, $"{usersdbID}{currentCustomer.CustomerID}");

                        var tmobj = dataOfbot.MTerminals.FirstOrDefault(x => x.MTerminalID == code.TerminalID).TerminalMapObject.FirstOrDefault().MapObject;
                        var orgmobj = dataOfbot.MapObjects.FirstOrDefault(x => x.MapObjectID == code.MabObjectID);

                        var mapHelper = new BotMapHelper();

                        List<MapHelper.Vertex> way = null;
                        if (tmobj == null || orgmobj == null)
                        {
                            botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                            await sender.SendText(texter.GetMessage("%parsecodeerr%"));
                            return 1;
                        }
                        else
                        {
                            way = mapHelper.GetWay(tmobj, orgmobj, dataOfbot);
                            if (way == null)
                            {
                                botUser.NowIs = MallBotWhatIsHappeningNow.SettingCustomer;
                                await sender.SendText(texter.GetMessage("%parsecodeerr%"));
                                return 1;
                            }
                        }


                        botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization; // если все ок, то устанавливаем юзера в текущем тц (даже если он в тестовом режиме)
                        botUser.CustomerCompositeID = $"{usersdbID}{currentCustomer.CustomerID}";
                        var answerer = new AnswerWithPhotoHelper(type, new FindedInformation(), sender, botUser, texter, dataOfbot);
                        await answerer.SendWayWithPhoto(tmobj, orgmobj, way, true);
                        return 1;
                    }
            }
        }

        private string GetBeauteCustomersString()
        {
            var customers = dataGetter.GetStructuredCustomers(Convert.ToBoolean(botUser.IsTestMode));

            string message = "";
            int number = 1;
            foreach (var dbData in customers)
            {
                foreach (var customer in dbData.Customers)
                {
                    if(botUser.Locale == "ru_RU" || customer.LocaleCity.Length == 1) message += $"{BotTextHelper.GetEmojiNumber(number)} " + customer.Name + "  " + customer.LocaleCity[0] + "\\r\\n";
                    else message += $"{BotTextHelper.GetEmojiNumber(number)} " + customer.Name + "  " + customer.LocaleCity[1] + "\\r\\n";
                    number++;
                }
            }
            return message;
        }
    }
}
