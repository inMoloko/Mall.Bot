using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.VKApi;
using Mall.Bot.Search.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mall.Bot.Common.MallHelpers
{
    public class AnalyseHelper
    {
        public static char[] splitters = { '~', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '-', '+', '=', '!', '"', '№', ';', '%', ':', '?', '.', ',', '/', '<', '|', '>', '`', '\\', '\'', ' ' };

        public List<FuzzySearchResult> DeleteBadResults(List<FuzzySearchResult> queryResults)
        {
            List<FuzzySearchResult> res = new List<FuzzySearchResult>();
            res.Add(queryResults[0]);
            for (int i = 0; i < queryResults.Count - 1; i++)
            {
                if (queryResults[i].Distinction == queryResults[i + 1].Distinction)
                    res.Add(queryResults[i + 1]);
                else break;
            }
            return res;
        }

        public async Task<int> AnalyseBadRequest(BotUser botUser, SocialNetworkType type, object bot, List<BotText> botTexts, List<VKApiRequestModel> Requests = null)
        {
            var texter = new BotTextHelper(botUser.Locale, type, botTexts);
            var sender = new ApiRouter(type, bot, botUser, Requests);

            switch (botUser.NowIs)
            {
                case MallBotWhatIsHappeningNow.SettingCustomer:
                    return await sender.SendText(texter.GetMessage("%badrequest1%"));
                case MallBotWhatIsHappeningNow.SearchingOrganization:
                    return await sender.SendText(texter.GetMessage("%badrequest2%"));
                case MallBotWhatIsHappeningNow.SearchingWay:
                    return await sender.SendText(texter.GetMessage("%badrequest3%"));
                case MallBotWhatIsHappeningNow.GettingAllOrganizations:
                    return await sender.SendText(texter.GetMessage("%badrequest4%"));
                default:
                    return 0;
            }
        }
    }
}