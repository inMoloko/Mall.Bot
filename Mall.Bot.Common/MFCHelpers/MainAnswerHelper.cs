using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.MFCHelpers.Models;
using Mall.Bot.Common.VKApi;
using Mall.Bot.Search.Mall;
using Mall.Bot.Search.Models;
using MFC.Bot.WebService;
using MFC.Bot.WebService.Contracts;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Bot.Common.MFCHelpers
{
    public class MainAnswerHelper
    {
        private MFCBotContext context;
        private DBHelpers.Models.MFCModels.BotUser botUser;
        private BotUserRequest thisRequest;
        private List<VKApiRequestModel> requests;

        private ApiRouter sendHelper = null;
        private MFCBotModel mfcDataOfBot = null;
        private BotTextHelper textHelper = null;
        private CacheHelper cacheHelper = new CacheHelper();
        public static MfcService mfcservice = new MfcService(ConfigurationManager.AppSettings["AppUrl"]);


        public MainAnswerHelper(DBHelpers.Models.MFCModels.BotUser _botUser, MFCBotContext _context, BotUserRequest _thisRequest,  List<VKApiRequestModel> _requests)
        {
            context = _context;
            botUser = _botUser;
            thisRequest = _thisRequest;
            requests = _requests;

            string key = "MFCDATAOFBOT";
            var datafromCache = cacheHelper.Get(key);
            if (datafromCache == null)
            {
                mfcDataOfBot = cacheHelper.Update(key, context);
            }
            else mfcDataOfBot = (MFCBotModel)datafromCache;

            if(botUser.NowIs != MFCBotWhatIsHappeningNow.SettingOffice)
            {
                var usersOffice = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                if (usersOffice.Name == "Ярмарка") usersOffice.OfficeID = 326;
                //получаем все секции, которые относятся к выбранному филиалу
                mfcDataOfBot.Sections = mfcDataOfBot.AllSections.
                    Where(x => mfcDataOfBot.SectionOffices.
                    Where(z => z.OfficeID == usersOffice.OfficeID).
                    Select(y => y.SectionID).Contains(x.SectionID)).DistinctBy(x => x.Name).ToList();
                //сохраняем из них только листья
                mfcDataOfBot.RetainLeafs();
            }

            sendHelper = new ApiRouter(SocialNetworkType.VK, null, botUser, requests);
            textHelper = new BotTextHelper(botUser.Locale, SocialNetworkType.VK, mfcDataOfBot.Texts);
        }

        public async Task<BotUserRequest> Main(object usefulData)
        {
            string query = "";
            if (botUser.InputDataType == InputDataType.Text)
            {
                //добавлена возможность задать вопрос пользователю. isQuestion - кэшированный флаг. Говорит о том, что текущее сообщение это вопрос от пользователя
                #region Question
                query = usefulData.ToString();
                var isQuestion = cacheHelper.Get($"QUESTION{botUser.BotUserVKID}"); //isQuestion - кэшированный флаг. Говорит о том, что текущее сообщение это вопрос от пользователя
                if (isQuestion != null && query != "назад" && query != "вопрос")
                {
                    cacheHelper.Remove($"QUESTION{botUser.BotUserVKID}");
                    // пересылаем оператору, ID которого указано в конфиг файле
                    thisRequest.IsSendingError = await VK.SendMessage(ulong.Parse(ConfigurationManager.AppSettings["OperatorVKID"]), $"Вопрос:\r\n{query}\U00002753\r\n\r\nКто задал -> https://vk.com/id{botUser.BotUserVKID} \U00002709");
                    if (thisRequest.IsSendingError == 0) 
                    {// все ок
                        await sendHelper.SendText(textHelper.GetMessage("%questionsuccss%"));
                        return thisRequest;
                    }
                    else
                    {// произошла ошибка при отправке сообщения оператору
                        await sendHelper.SendText(textHelper.GetMessage("%questionfail%"));
                        return thisRequest;
                    }
                }
                #endregion
                if (await AnaliseCommands(query) == 1) return thisRequest;
            }

            var office = new DBHelpers.Models.MFCModels.Office();
            var searchHelper = new SearchHelper(mfcDataOfBot);
            object alreadyFinded;
            switch (botUser.NowIs)
            {
                case MFCBotWhatIsHappeningNow.SettingOffice:
                    #region SetOffice     
                    // включение/отключение расписаний филиалов
                    if (ConfigurationManager.AppSettings["Schedules"] == "Enable") mfcDataOfBot.Offices = SetSchedules(mfcDataOfBot.Offices);
                    List<FuzzySearchResult> result = null;
                    switch (botUser.InputDataType)
                    {
                        case InputDataType.Image:
                            //хз ваще
                            break;
                        case InputDataType.GeoLocation:
                            result = searchHelper.SearchOfficeByGeocode((DbGeography)usefulData);
                            thisRequest.Answer = GetAnswer(result);
                            break;
                        case InputDataType.Text:
                            alreadyFinded = cacheHelper.Get($"SETOFFICES{botUser.BotUserVKID}");
                            if (alreadyFinded == null)
                            {
                                result = searchHelper.SearchOfficeByName(query);
                                thisRequest.Answer = GetAnswer(result);
                            }
                            else
                            {
                                result = searchHelper.SearchOfficeByName(query, (string)alreadyFinded);
                                thisRequest.Answer = GetAnswer(result);
                            }
                            break;
                    }
                    
                    if (result.Count == 1 && ConfigurationManager.AppSettings["IsTestMode"] != "off" ) result = DoDummy();

                    thisRequest.Answer = GetAnswer(result);
                    await AnaliseSearchOfficeResult(result);
                    #endregion
                    break;
                case MFCBotWhatIsHappeningNow.SettingOpportunity: //новый флаг! нужен для идентификации выбора (проверить статус/записаться на услугу)
                    #region SetOpp
                    int numFromList;
                    if (int.TryParse(query, out numFromList) && (numFromList == 1 || numFromList == 2))
                    {
                        if(numFromList == 1)// выбрана проверка статуса
                        {
                            botUser.NowIs = MFCBotWhatIsHappeningNow.GetingTicketInformation;
                            office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            await sendHelper.SendText(textHelper.GetMessage("%gtinfstart%",
                                    new string[] { "%officename%", "%adress%", "%business%" },
                                    new string[] { office.DisplayName, office.DisplayAddress, GetBusynessOffice(office.AisMFCID) }));
                        }
                        if (numFromList == 2)// выбрана запись на услугу
                        {
                            botUser.NowIs = MFCBotWhatIsHappeningNow.SettingService;
                            office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            await SelectSendSetTenPopularServices(office.DisplayName, office.AisMFCID, office.DisplayAddress);
                        }
                    }
                    else
                    {// что-то другое
                        await sendHelper.SendText(textHelper.GetMessage("%slctopfail%"));
                    }
                    #endregion
                    break;
                case MFCBotWhatIsHappeningNow.SettingService:
                    #region SetService
                    alreadyFinded = cacheHelper.Get($"SETSERVICE{botUser.BotUserVKID}"); 
                    if (alreadyFinded == null)
                    {
                        alreadyFinded = cacheHelper.Get($"SETSERVICES{botUser.BotUserVKID}");
                        if (alreadyFinded == null)
                        {
                            office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            await SelectSendSetTenPopularServices(office.DisplayName, office.AisMFCID, office.DisplayAddress);
                            return thisRequest;
                        }
                        //если есть информация о множестве услуг, значит ведем поиск по ним
                        if (int.TryParse(query, out numFromList))
                        {
                            //теперь поиск ведется по секциям, а не сервисам
                            result = searchHelper.SearchSectionByName(query, (string)alreadyFinded);
                            thisRequest.Answer = GetAnswer(result);
                            if (result.Count == 0)
                            {
                                await sendHelper.SendText(textHelper.GetMessage("%slctsrvicefail%"));
                                return thisRequest;
                            }
                        }
                        else
                        {
                            if (alreadyFinded.ToString().Last() == '¡' || alreadyFinded.ToString().Last() == '!')//в данном случае, это флаг, который говорит, что кэшированные данные относятся к списку 10-ти самых популярных услуг
                            {
                                //теперь поиск ведется по секциям, а не сервисам
                                result = searchHelper.SearchSectionByName(query);
                                thisRequest.Answer = GetAnswer(result);
                            }
                            else
                            {
                                await sendHelper.SendText(textHelper.GetMessage("%slctsrvicesucs%"));
                                return thisRequest;
                            }
                        }

                        await AnaliseSearchServiceResult(result);
                    }
                    else// если есть информация об услуге, значит ждем подтвержденеия
                    {
                        if (!string.IsNullOrWhiteSpace(query) && query == "да")
                        {
                            var service = mfcDataOfBot.Sections.FirstOrDefault(x => x.SectionID == (int)alreadyFinded);
                            office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            var message = textHelper.GetMessage("%talon%");
                            var talon = mfcservice.Enqueue(service.SectionID, botUser.OfficeID);

                            if (talon == null || talon?.ID == 0) //аис не выдал талон => не поставил пользователя в очередь
                            {
                                await sendHelper.SendText(textHelper.GetMessage("%talonerr%"));
                            }
                            else
                            {
                                message = Analiser.AnaliseTalon(talon, message, service.Name, office);

                                botUser.TalonID = talon.ID;
                                botUser.ServiceID = service.SectionID;
                                botUser.NowIs = MFCBotWhatIsHappeningNow.QueueWaiting;

                                await sendHelper.SendText(message);
                            }
                            cacheHelper.Clear(botUser.BotUserVKID);
                        }
                        else
                        {
                            office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            await SelectSendSetTenPopularServices(office.DisplayName, office.AisMFCID, office.DisplayAddress);
                        }
                        cacheHelper.Remove($"SETSERVICE{botUser.BotUserVKID}");
                    }
                    #endregion
                    break;
                case MFCBotWhatIsHappeningNow.GetingTicketInformation:
                    #region GetTicketInfo
                    office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                    if (int.TryParse(query, out numFromList))
                    {
                        //проверяем введенный номер
                        await sendHelper.SendText(Analiser.GetAnalysedAnswer(botUser, office, numFromList, textHelper, mfcDataOfBot.WindowsOffices));
                    }
                    else await sendHelper.SendText(textHelper.GetMessage("%gtinfstart%", //начальное сообщение
                                    new string[] { "%officename%", "%adress%", "%business%" },
                                    new string[] { office.DisplayName, office.DisplayAddress, GetBusynessOffice(office.AisMFCID) }));
                    #endregion
                    break;
                case MFCBotWhatIsHappeningNow.QueueWaiting:
                    #region QueueWait
                    //var rnd = new Random();
                    //var joke = mfcDataOfBot.Jokes[rnd.Next(mfcDataOfBot.Jokes.Count)];
                    //await sendHelper.BotSendText(joke.Text);
                    //thisRequest.Answer = "joke";
                    await sendHelper.SendText(textHelper.GetMessage("%waitinghelp%", "%number%", BotTextHelper.GetEmojiNumber( (int)botUser.TalonID)));
                    #endregion
                    break;
            }
            return thisRequest;
        }
        /// <summary>
        /// Устанавливает тестовый филиал в качестве выбранного вне зависимости от желания полоьзователя.
        /// В тестовом режиме работы естественно
        /// </summary>
        /// <returns></returns>
        private List<FuzzySearchResult> DoDummy()
        {
            var result = new List<FuzzySearchResult>();
            var tmp = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == 23);
            result.Add(new FuzzySearchResult(tmp.DisplayName, tmp.AisMFCID, 0.0, 0, FuzzySearchResultDataType.Customer));
            return result;
        }

        private string GetAnswer(List<FuzzySearchResult> result)
        {
            return JsonConvert.SerializeObject(result);
        }

        public async Task<int> AnaliseSearchServiceResult(List<FuzzySearchResult> result)
        {
            string message = "";
            switch (result.Count)
            {
                case 0:
                    await sendHelper.SendText(textHelper.GetMessage("%srchservicefail%"));
                    return 1;
                case 1:
                    cacheHelper.Remove($"SETSERVICES{botUser.BotUserVKID}");

                    message = textHelper.GetMessage("%srchserviceoneres%", "%findedservicename%", result[0].Name);

                    var res = mfcservice.GetQueue(result[0].ID, botUser.OfficeID);
                    if (res == null)
                    {
                        await sendHelper.SendText(textHelper.GetMessage("%getQueueError%"));
                    }
                    else
                    {
                        cacheHelper.Set($"SETSERVICE{botUser.BotUserVKID}", result[0].ID, 35);

                        message = message.Replace("%queuecount%", res.Waiting.ToString());
                        message = message.Replace("%windowcount%", res.Working.ToString());
                        await sendHelper.SendText(message);
                    }
                    return 1;
                default:
                    message = textHelper.GetMessage("%srchservicemanyres%");
                    var partsOfmessage = message.Split(';');
                    var p = partsOfmessage.First();

                    for (int i = 0; i < result.Count; i++)
                    {
                        p += BotTextHelper.GetVKSmileNumber(i + 1) + result[i].Name + "\\r\\n\\r\\n";

                        if (i >= 10 && i % 10 == 0 || i == result.Count-1)
                        {
                            if (i != result.Count-1)
                            {
                                await sendHelper.SendText(p);
                                p = "";
                            }
                            else
                            {
                                await sendHelper.SendText(p+partsOfmessage.Last());
                            }
                        }
                    }

                    if (requests.Count > 20)
                    {
                        requests.Clear();
                        await sendHelper.SendText(textHelper.GetMessage("%toomuch%"));
                        return 1;
                    }

                    string Services = "";
                    foreach (var item in result)
                    {
                        Services += item.ID + ";";
                    }
                    cacheHelper.Set($"SETSERVICES{botUser.BotUserVKID}", Services, 35);
                    return 1;
            }
        }
        /// <summary>
        /// Выбирает 10 самых популярных услуг и кэширует их
        /// </summary>
        /// <param name="officeName"></param>
        /// <param name="AisMFCID"></param>
        /// <returns></returns>
        private async Task<int> SelectSendSetTenPopularServices(string officeName, int AisMFCID, string officeAdress)
        {
            try
            {
                string p = "";
                //выбираем 10 самых популярных услуг и отправляем результат юзеру
                for (int i = 0; i < 10; i++)
                {
                    if (i == 9) p += $"&#128287;  " + mfcDataOfBot.Sections[i].Name + "\\r\\n\\r\\n";
                    else p += $"{i + 1}&#8419;  " + mfcDataOfBot.Sections[i].Name + "\\r\\n\\r\\n";
                }
                var message = textHelper.GetMessage("%srchofficesuccess%", 
                    new string[] { "%services%", "%officename%", "%adress%", "%busyness%" },
                    new string[] { p, officeName, officeAdress, GetBusynessOffice(AisMFCID) });
                await sendHelper.SendText(message);

                SetTenPopularServices();
                return 1;
            }
            catch(Exception exc)
            {
                Logging.Logger.Error(exc);
                return 0;
            }
        }
        /// <summary>
        /// Кэширует 10 популярных усдуг. 
        /// isTen - помечает, относятся ли кэшированные услуги к началу выбора услу, либо они были закешированы в процессе поиска. 
        /// В последнем случае, команда назад вернет к началу поиска услуг, а не к поиску филиала
        /// </summary>
        /// <param name="isTen"></param>
        private void SetTenPopularServices(bool isTen = true)
        {
            string Services = "";
            for (int i = 0; i < 10; i++)
            {
                Services += mfcDataOfBot.Sections[i].SectionID + ";";
            }

            if(isTen) cacheHelper.Set($"SETSERVICES{botUser.BotUserVKID}", Services + "¡", 35);
            else cacheHelper.Set($"SETSERVICES{botUser.BotUserVKID}", Services + "!", 35);
        }

        public async Task<int> AnaliseSearchOfficeResult(List<FuzzySearchResult> result)
        {
            string p = "";
            switch (result.Count)
            {
                case 0:
                    if (botUser.IsNewUser)
                    {
                        await sendHelper.SendText(textHelper.GetMessage("%setofficehelp%"));
                    }
                    else
                    {
                        await sendHelper.SendText(textHelper.GetMessage("%srchofficefail%"));
                    }
                    return 1;
                case 1:
                    cacheHelper.Remove($"SETOFFICES{botUser.BotUserVKID}");
                    // изменям информацию по пользователю
                    botUser.OfficeID = result[0].ID;
                    var office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == result[0].ID);

                    if (office.IsOpen == true || result[0].Name == "Ярмарка") // если офис открыт или он ярмарка
                    {
                        //(если офис не центральный и не индустриальный или он ярмарка) и при этом запись в очередь работает
                        if ((result[0].ID != 21 && result[0].ID != 49 || result[0].Name == "Ярмарка") && context.AISMFCServiceStatus.FirstOrDefault(x => x.ServiceName == "enqueue").Status == "ok") 
                        {//то даем возможность записать
                            botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOpportunity;
                            await sendHelper.SendText(textHelper.GetMessage("%slctopstart%",
                                    new string[] { "%officename%", "%adress%", "%business%" },
                                    new string[] { office.DisplayName, office.DisplayAddress, GetBusynessOffice(office.AisMFCID) }));
                        }
                        else
                        {//иначе отправляем проверять статус
                            //если сервисы проверки статуса талона работают
                            if(context.AISMFCServiceStatus.FirstOrDefault(x => x.ServiceName == "getTicketInformation").Status == "ok" && context.AISMFCServiceStatus.FirstOrDefault(x => x.ServiceName == "getQueueInformation").Status == "ok")
                            {
                                botUser.NowIs = MFCBotWhatIsHappeningNow.GetingTicketInformation;
                                await sendHelper.SendText(textHelper.GetMessage("%gtinfstart%",
                                        new string[] { "%officename%", "%adress%", "%business%" },
                                        new string[] { office.DisplayName, office.DisplayAddress, GetBusynessOffice(office.AisMFCID) }));
                            }
                            else
                            {
                                botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                                await sendHelper.SendText(textHelper.GetMessage("%getQueueError%"));//Произошла серьезная неполадка \U0001F631 \r\n\U00002705 Пожалуйста повторите запрос позже
                            }
                            
                        }
                    }
                    else
                    {//если офис зарыт
                        await sendHelper.SendText(textHelper.GetMessage("%officeclosed%",
                                    new string[] { "%officename%", "%adress%" },
                                    new string[] { office.DisplayName, office.DisplayAddress }));
                    }
                    return 1;
                default:
                    for (int i = 0; i < result.Count; i++)
                    {
                        office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == result[i].ID);
                        //офис закрыт или открыт
                        if (office.IsOpen == true) p += BotTextHelper.GetVKSmileNumber(i + 1) + office.DisplayName + $" \U00002600 Открыт\\r\\n\U0001F449 {office.DisplayAddress}" + "\\r\\n" + GetBusynessOffice(office.AisMFCID) + "\\r\\n\\r\\n";
                        else p += BotTextHelper.GetVKSmileNumber(i + 1) + office.DisplayName + $" \U0001F4A4 Закрыт\\r\\n\U0001F449 {office.DisplayAddress}" + "\\r\\n\\r\\n";
                    }
                    await sendHelper.SendText(textHelper.GetMessage("%srchofficemanyres%", "%offices%", p));

                    string Offices = "";
                    foreach (var item in result)
                    {
                        Offices += item.ID + ";";
                    }
                    cacheHelper.Set($"SETOFFICES{botUser.BotUserVKID}", Offices, 35);
                    return 1;
            }
        }

        public async Task<int> AnaliseCommands(string query)
        {
            switch(query)
            {
                case "milk":
                    switch (botUser.NowIs)
                    {
                        case MFCBotWhatIsHappeningNow.GetingTicketInformation:
                            var office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);

                            botUser.NowIs = MFCBotWhatIsHappeningNow.SettingService;
                            botUser.TalonID = 0;
                            botUser.ServiceID = 0;
                            await SelectSendSetTenPopularServices(office.DisplayName, office.AisMFCID, office.DisplayAddress);
                            return 1;
                        default:
                            return 0;
                    }
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
                    await sendHelper.SendText(textHelper.GetMessage("%hello%"));
                    goto case "help";
                case "спасибо":
                case "благодарю":
                case "thanks":
                    await sendHelper.SendText(textHelper.GetMessage("%thanks%"));
                    return 1;
                case "прости":
                case "простите":
                case "сорян":
                case "извините":
                case "sorry":
                    await sendHelper.SendText(textHelper.GetMessage("%sorry%"));
                    return 1;
                case "пока":
                case "до свидания":
                case "досвидули":
                case "goodbye":
                case "bye":
                case "bye bye":
                    await sendHelper.SendText(textHelper.GetMessage("%bye%"));
                    return 1;
                case "вопрос": //добавлена команда "вопрос"
                    await sendHelper.SendText(textHelper.GetMessage("%question%"));
                    cacheHelper.Set($"QUESTION{botUser.BotUserVKID}", 1, 35);
                    return 1;
                case "отмена":
                case "start":
                    botUser.OfficeID = 0;
                    botUser.TalonID = 0;
                    botUser.ServiceID = 0;
                    botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                    botUser.ModifiedDate = DateTime.Now;
                    cacheHelper.Clear(botUser.BotUserVKID);

                    await sendHelper.SendText(textHelper.GetMessage("%setofficehelp%"));
                    return 1;
                case "помощь":
                case "help":
                    cacheHelper.Clear(botUser.BotUserVKID);
                    switch (botUser.NowIs)
                    {
                        case MFCBotWhatIsHappeningNow.SettingOffice:
                            await sendHelper.SendText(textHelper.GetMessage("%setofficerealhelp%"));
                            return 1;
                        case MFCBotWhatIsHappeningNow.SettingService:
                            SetTenPopularServices(false);
                            await sendHelper.SendText(textHelper.GetMessage("%setservicehelp%"));
                            return 1;
                        case MFCBotWhatIsHappeningNow.SettingOpportunity:
                            await sendHelper.SendText(textHelper.GetMessage("%slctophelp%"));
                            return 1;
                        case MFCBotWhatIsHappeningNow.GetingTicketInformation:
                            await sendHelper.SendText(textHelper.GetMessage("%gtinfhelp%"));
                            return 1;
                        default:
                            return 0;
                    }
                case "назад":
                case "back":
                    switch (botUser.NowIs)
                    {
                        case MFCBotWhatIsHappeningNow.SettingOffice:
                            cacheHelper.Clear(botUser.BotUserVKID);
                            await sendHelper.SendText(textHelper.GetMessage("%setofficehelp%"));
                            return 1;
                        case MFCBotWhatIsHappeningNow.SettingService:
                            var data = cacheHelper.Get($"SETSERVICES{botUser.BotUserVKID}");

                            if (data == null || data.ToString().Last() == '¡') //если кэш пуст, либо в кэше хранятся данные, которые относятся к 10ки популярных услуг
                            {
                                cacheHelper.Clear(botUser.BotUserVKID);
                                if (context.AISMFCServiceStatus.FirstOrDefault(x => x.ServiceName == "enqueue").Status == "ok")
                                {
                                    botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOpportunity;
                                    var office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                                    await sendHelper.SendText(textHelper.GetMessage("%slctopstart%",
                                        new string[] { "%officename%", "%adress%", "%business%" },
                                        new string[] { office.DisplayName, office.DisplayAddress, GetBusynessOffice(office.AisMFCID) }));
                                }
                                else
                                {
                                    goto case MFCBotWhatIsHappeningNow.SettingOpportunity;
                                }
                            }
                            else
                            {//возвращаем к началу этапа SettingService
                                var office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                                await SelectSendSetTenPopularServices(office.DisplayName, office.AisMFCID, office.DisplayAddress);
                            }
                            return 1;
                        case MFCBotWhatIsHappeningNow.SettingOpportunity:
                            cacheHelper.Clear(botUser.BotUserVKID);
                            botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                            botUser.OfficeID = 0;
                            botUser.ServiceID = 0;
                            botUser.TalonID = 0;
                            await sendHelper.SendText(textHelper.GetMessage("%setofficehelp%"));
                            return 1;
                        case MFCBotWhatIsHappeningNow.GetingTicketInformation:
                            cacheHelper.Clear(botUser.BotUserVKID);
                            var usersOffice = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            if ((usersOffice.AisMFCID != 21 && usersOffice.AisMFCID != 49 || usersOffice.DisplayName == "Ярмарка") && context.AISMFCServiceStatus.FirstOrDefault(x => x.ServiceName == "enqueue").Status == "ok")
                            {//если были в филиале, где разрешена запись
                                botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOpportunity;
                                await sendHelper.SendText(textHelper.GetMessage("%slctopstart%", 
                                    new string[] { "%officename%", "%adress%", "%business%" }, 
                                    new string[] { usersOffice.DisplayName, usersOffice.DisplayAddress, GetBusynessOffice(usersOffice.AisMFCID) }));
                            }
                            else
                            {
                                botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                                botUser.OfficeID = 0;
                                botUser.ServiceID = 0;
                                botUser.TalonID = 0;
                                await sendHelper.SendText(textHelper.GetMessage("%setofficehelp%"));
                            }
                            return 1;
                        default:
                            return 0;
                    }
                case "update":
                    mfcDataOfBot = cacheHelper.Update("MFCDATAOFBOT", context);
                    if (mfcDataOfBot != null)
                    {
                        await sendHelper.SendText(textHelper.GetMessage("%updatecachesuccess%"));
                    }
                    else
                    {
                        await sendHelper.SendText(textHelper.GetMessage("%updatecachefail%"));
                    }
                    return 1;
                case "getinfo":
                case "статус":
                    switch (botUser.NowIs)
                    {
                        case MFCBotWhatIsHappeningNow.QueueWaiting:
                            Logging.Logger.Debug("qwaiting  status");
                            var office = mfcDataOfBot.Offices.FirstOrDefault(x => x.AisMFCID == botUser.OfficeID);
                            var message = Analiser.GetAnalysedAnswer(botUser, office, (int)botUser.TalonID, textHelper, mfcDataOfBot.WindowsOffices);
                            await sendHelper.SendText(message);
                            return 1;
                        default:
                            return 0;
                    }
                case "finish":
                    botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                    botUser.ModifiedDate = DateTime.Now;
                    botUser.TalonID = 0;
                    botUser.ServiceID = 0;
                    botUser.OfficeID = 0;
                    cacheHelper.Clear(botUser.BotUserVKID);

                    await sendHelper.SendText(textHelper.GetMessage("%finish%", "%window%","номер окна"));
                    return 1;
            }
            return 0;
        }

        public static string GetBusynessOffice(int AisMFCID)
        {
            
            QueueParams qp = mfcservice.GetQueue(1, AisMFCID) ?? new QueueParams { Queue = 0};
            if (qp.Queue == 0)
            {
                return "";
            } 
            if (qp.Queue < 5)
            {
                if (qp.Queue < 3)
                {
                    return "\U00002733 Малое количество людей в очереди";
                }
                else
                {
                    return "\U000026A0 Среднее количество людей в очереди";
                }
            }
            else
            {
                return "\U0001F198 Большое количество людей в очереди";
            }
        }

        private List<DBHelpers.Models.MFCModels.Office> SetSchedules(List<DBHelpers.Models.MFCModels.Office> offices)
        {
            foreach (var item in offices)
            {
                item.IsOpen = Analiser.OfficeIsOpen(item.Schedule);
            }
            return offices;
        }
    }
}
