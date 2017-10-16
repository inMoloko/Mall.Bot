using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.MallHelpers.Models;
using Moloko.Utils;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Bot.Common.MallHelpers
{
    public class AnswerWithPhotoHelper
    {
        private SocialNetworkType type;
        private FindedInformation answer;
        private ApiRouter sender;
        private BotUser botUser;
        private BotTextHelper texter;
        private CachedDataModel dataOfBot;


        public AnswerWithPhotoHelper(SocialNetworkType _type, FindedInformation _answer, ApiRouter _sender, BotUser _botUser, BotTextHelper _texter, CachedDataModel _dataOfBot)
        {
            answer = _answer;
            sender = _sender;
            botUser = _botUser;
            texter = _texter;
            type = _type;
            dataOfBot = _dataOfBot;
        }

        public AnswerWithPhotoHelper()
        {
        }

        private async Task<int> SendAllFindedInformation()
        {
            var groupes = (List<GroupedOrganization>)answer.GroopedResult;
            groupes.OrderByDescending(x => x.AverageRating).ToList();
            string p = texter.GetMessage("%srchsuccess%") + "\\r\\n\\r\\n";
            int i = 0;
            foreach (Floor f in dataOfBot.Floors)
            {
                var groupsFromFloor = groupes.Where(x => x.FloorID == f.FloorID).ToList();
                if (groupsFromFloor.Count != 0)
                {
                    char index = 'A';
                    foreach (var group in groupsFromFloor)
                    {
                        if (index != 'F')
                        {
                            p += $"{index}: ";
                            index++;
                        }
                        foreach (var org in group.Orgs.DistinctBy(x => x.Name))
                        {
                            p += org.Name + ", ";
                        }
                        p = p.Remove(p.Length - 2, 2);
                        p += "\\r\\n\\r\\n";
                    }
                    if (type == SocialNetworkType.VK)
                    {
                        await sender.SendPhoto(answer.FloorsPictures[i].Bmp, p);
                    }
                    else
                    {
                        await sender.SendText(p);
                        await sender.SendPhoto(answer.FloorsPictures[i].Bmp);
                    }
                    p = "";
                    i++;
                }
            }
            return 1;
        }

        private int GetFullFloorID()
        {
            var findedOrganizations = GetDataHelper.GetOrganizationFromFuzzySearchResult(answer.Result.QueryResults, dataOfBot);

            int max = 0; int index = 0;
            for (int i = 0; i < answer.FloorsPictures.Count; i++)
            {
                var count = dataOfBot.GetMapObjects(findedOrganizations).Where(x => x.FloorID == answer.FloorsPictures[i].FloorID).Count();
                if (max < count)
                {
                    max = count;
                    index = answer.FloorsPictures[i].FloorID;
                }
            }
            return index;
        }

        /// <summary>
        /// Организации с одинановыми Name - "одинаковые".
        /// Метод вернет кол-во "уникальных" организаций.
        /// </summary>
        /// <returns></returns>
        public int CountFindedOrganizations()
        {
            if (answer.Result.QueryResults.Count == 0) return 0;
            int res = 1;
            int temp = 0;
            answer.Result.QueryResults = answer.Result.QueryResults.OrderBy(x => x.Name).ToList();
            for (int i = 0; i < answer.Result.QueryResults.Count - 1; i++)
            {
                if (answer.Result.QueryResults[i].Name == answer.Result.QueryResults[i + 1].Name)
                {
                    temp++;
                }
                else
                {
                    res++;
                    temp = 0;
                }
            }
            return res;
        }

        private string Beauty(int FullFloorID)
        {
            var groupedOrgs = (List<GroupedOrganization>)answer.GroopedResult;
            groupedOrgs.OrderByDescending(x => x.AverageRating).ToList();
            string p = texter.GetMessage("%srchsuccess%") + "\\r\\n\\r\\n";
            int i = 0;
            var groupsFromFloor = groupedOrgs.Where(x => x.FloorID == FullFloorID).ToList();
            if (groupsFromFloor.Count != 0)
            {
                char index = 'A';
                foreach (var group in groupsFromFloor)
                {
                    p += $"{index}: ";
                    index++;
                    foreach (var org in group.Orgs.DistinctBy(x => x.Name))
                    {
                        p += org.Name + ", ";
                    }
                    p = p.Remove(p.Length - 2, 2);
                    p += "\\r\\n\\r\\n";
                    if (index == 'F') break;
                }
                i++;
            }
            p += "...";
            return p;
        }

        public async Task<int> AnalyseSearchOrganizationResult(bool botUserWantAllFindedInformation = false)
        {
            switch (answer.Result.QueryResults.DistinctBy(x => x.Name).Count())
            {
                case 0:
                    await sender.SendText(texter.GetMessage("%orgserchfail%"));
                    return 1;
                case 1:
                    await SendAllFindedInformation();
                    var cacher = new CacheHelper();
                    cacher.Set($"FINDEDFIRSTORG{botUser.BotUserID}", answer, 35);
                    botUser.NowIs = MallBotWhatIsHappeningNow.SearchingWay;
                    await sender.SendText(texter.GetMessage("%srchwaystart%", "%org%", answer.Result.QueryResults[0].Name));
                    return 1;
                default:
                    var groups = (List<GroupedOrganization>)answer.GroopedResult;
                    if ((answer.FloorsPictures.Count == 1 && groups.Count <= 5) || answer.Result.QueryResults.Count <= 5 || botUserWantAllFindedInformation)
                    {
                        await SendAllFindedInformation();
                        await sender.SendText(texter.GetMessage("%ready%", "%mall%", dataOfBot.Customers[0].Name, dataOfBot.Customers[0].LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                        return 1;
                    }
                    var FullFloorID = GetFullFloorID();
                    string p = Beauty(FullFloorID);

                    var PictFloor = answer.FloorsPictures.FirstOrDefault(x => x.FloorID == FullFloorID);

                    if (type == SocialNetworkType.VK)
                    {
                        await sender.SendPhoto(PictFloor.Bmp, p);
                    }
                    else
                    {
                        await sender.SendText(p);
                        await sender.SendPhoto(PictFloor.Bmp);
                    }
                    await sender.SendText(texter.GetMessage("%orgsearchviewall%", "%count%", answer.Result.QueryResults.Count.ToString()));
                    cacher = new CacheHelper();
                    cacher.Set($"VIEWALLORG{botUser.BotUserID}", answer, 35);
                    botUser.NowIs = MallBotWhatIsHappeningNow.GettingAllOrganizations;
                    return 1;
            }
        }
        public async Task<int> AnalyseSearchOrganizationForWayResult()
        {
            var cacher = new CacheHelper();
            switch (answer.Result.QueryResults.Count)
            {
                case 0:
                    await sender.SendText(texter.GetMessage("%wayserchfail%", "%query%", answer.Result.QueryText));
                    return 1;
                case 1:
                    if (dataOfBot.Organizations.FirstOrDefault(x => x.OrganizationID == answer.Result.QueryResults[0].ID).OrganizationMapObject.Where(z => z.MapObject.Params == null).Count() > 1)
                    {
                        await sender.SendText(texter.GetMessage("%getwayerror%"));
                        return 1;
                    }

                    object previousObjAnswer = cacher.Get($"FINDEDFIRSTORG{botUser.BotUserID}");
                    if (previousObjAnswer == null)
                    {
                        cacher.Clear(botUser.BotUserID);
                        botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                        await sender.SendText(texter.GetMessage("%ready%", "%mall%", dataOfBot.Customers[0].Name, dataOfBot.Customers[0].LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                        return 1;
                    }

                    var previousAnswer = (FindedInformation)previousObjAnswer;
                    if (previousAnswer.Result.QueryResults[0].ID == answer.Result.QueryResults[0].ID)
                    {
                        await sender.SendText(texter.GetMessage("%srchwaysameorgs%", "%org%", answer.Result.QueryResults[0].Name));
                        return 1;
                    }

                    var mapHelper = new BotMapHelper();
                    var way = mapHelper.GetClosestWay(answer.Result.QueryResults[0], previousAnswer.Result.QueryResults, dataOfBot);
                    if (way.Way == null)
                    {
                        cacher.Clear(botUser.BotUserID);
                        botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
                        await sender.SendText(texter.GetMessage("%getwayfail%") + "\\r\\n\\r\\n" + texter.GetMessage("%ready%", "%mall%", dataOfBot.Customers[0].Name, dataOfBot.Customers[0].LocaleCity)
                              + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
                        return 1;
                    }
                    await SendWayWithPhoto(way.From, way.To, way.Way);

                    return 1;
                default:
                    if (answer.Result.QueryResults.Count != answer.Result.QueryResults.DistinctBy(x => x.Name).Count())
                    {
                        await sender.SendText(texter.GetMessage("%getwayerror%"));
                        return 1;
                    }

                    string p = "";
                    for (int i = 0; i < answer.Result.QueryResults.Count; i++)
                    {
                        p += $"{BotTextHelper.GetEmojiNumber(i + 1)} {answer.Result.QueryResults[i].Name} \\r\\n";
                    }
                    await sender.SendText(texter.GetMessage("%getwaymanyres%", "%shops%", p));

                    string orgIDs = "";
                    foreach (var item in answer.Result.QueryResults)
                    {
                        orgIDs += item.ID + ";";
                    }
                    cacher.Set($"SEARCHWAY{botUser.BotUserID}", orgIDs, 35);
                    return 1;
            }
        }

        /// <summary>
        /// Непосредственная отправка маршрута
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Way"></param>
        /// <param name="IsFromTerminal">Терминалы хранятся отдельно, поэтому, если From - терминал, то тру</param>
        /// <returns></returns>
        public async Task<int> SendWayWithPhoto(MapObject From, MapObject To, List<MapHelper.Vertex> Way, bool IsFromTerminal = false)
        {
            DrawHelper drawer;
            if (botUser.Locale == "ru_RU")
            {
                drawer = new DrawHelper(dataOfBot, answer, $"Этаж %floornumber%   {dataOfBot.Customers[0].Name} {dataOfBot.Customers[0].LocaleCity[0]}");
            }
            else
            {
                if (dataOfBot.Customers[0].LocaleCity.Length == 1) drawer = new DrawHelper(dataOfBot, answer, $"Floor %floornumber%   {dataOfBot.Customers[0].Name} {dataOfBot.Customers[0].LocaleCity[0]}");
                else drawer = new DrawHelper(dataOfBot, answer, $"Floor %floornumber%   {dataOfBot.Customers[0].Name} {dataOfBot.Customers[0].LocaleCity[1]}");
            }

            drawer.DrawWay(From, To, Way);

            string FromName;
            if (IsFromTerminal) FromName = dataOfBot.GetTerminal(From).Name;
            else FromName = dataOfBot.GetOrganization(From).Name;

            if (botUser.Locale == "ru_RU") await sender.SendText(texter.GetMessage("%sendway%", "%shops%", $"Начало: A - {FromName} \\r\\n Конец: B - {dataOfBot.GetOrganization(To).Name}"));
            else await sender.SendText(texter.GetMessage("%sendway%", "%shops%", $"Start: A - {FromName} \\r\\n Finish: B - {dataOfBot.GetOrganization(To).Name}"));
            foreach (var item in answer.FloorsPictures)
            {
                await sender.SendPhoto(item.Bmp);
            }

            botUser.NowIs = MallBotWhatIsHappeningNow.SearchingOrganization;
            await sender.SendText(texter.GetMessage("%adv%") + "\\r\\n\\r\\n" + texter.GetMessage("%ready%", "%mall%", dataOfBot.Customers[0].Name, dataOfBot.Customers[0].LocaleCity) + "\\r\\n\\r\\n" + texter.GetMessage("%orgsrchstartback%"));
            return 1;
        }
    }
}
