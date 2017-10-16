using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers.Models;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Search.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Mall.Bot.Common.MallHelpers
{
    public class GetDataHelper
    {
        private List<CachedDataModel> datasOfBot;

        public GetDataHelper(List<CachedDataModel> _datasOfBot)
        {
            datasOfBot = _datasOfBot;
        }

        public static List<Organization> GetOrganizationFromFuzzySearchResult(List<FuzzySearchResult> result, CachedDataModel dataOfbot)
        {
            var findedOrganizations = new List<Organization>();
            foreach (var item in result)
            {
                findedOrganizations.Add(dataOfbot.Organizations.FirstOrDefault(x => item.ID == x.OrganizationID));
            }
            return findedOrganizations;
        }



        /// <summary>
        /// Возвращает структурированное множество всет ТЦ из всех баз, поддерживаемых ботом
        /// </summary>
        /// <param name="IsTestMode"> == true - вернет абсолютно все поддерживаемые ТЦ независимо от того опубликованы ли они</param>
        /// <returns></returns>
        public List<DBasesCustomersHolder> GetStructuredCustomers(bool IsTestMode)
        {
            // Собираем всех кастомеров
            var customers = new List<DBasesCustomersHolder>();
            char dbID = 'A';
            int i;
            for (i = 0; i < int.Parse(ConfigurationManager.AppSettings["dbCount"]); i++)
            {
                customers.Add(new DBasesCustomersHolder { DBaseID = dbID, Customers = new List<ICustomer>() });
                dbID++;
            }

            i = 0;
            foreach (var item in customers)
            {
                if (!IsTestMode)
                {
                    item.Customers.AddRange(datasOfBot[i].Customers.Where(x => x.IsPublish == 1));
                }
                else
                {
                    item.Customers.AddRange(datasOfBot[i].Customers);
                }
                item.Customers = item.Customers.OrderBy(x => x.Name).ToList();
                i++;
            }
            return customers;
        }

        public CachedDataModel GetDataForOneCustomer(int customerID, string CustomerCompositeID)
        {
            CachedDataModel DataOfBot = null;
            char dbID = 'A';
            for (int i = 0; i < int.Parse(ConfigurationManager.AppSettings["dbCount"]); i++)
            {
                if (dbID == CustomerCompositeID[0])
                {
                    DataOfBot = SelectData(datasOfBot[i], customerID);
                    DataOfBot.Texts = datasOfBot[0].Texts;
                }
                dbID++;
            }
            return DataOfBot;
        }
        private CachedDataModel SelectData(CachedDataModel MainDataOfBot, int customerID)
        {
            CachedDataModel DataOfBot = null;

            DataOfBot = new CachedDataModel();
            DataOfBot.Customers = MainDataOfBot.Customers.Where(x => x.CustomerID == customerID).ToList();
            DataOfBot.Organizations = MainDataOfBot.Organizations.Where(x => x.CustomerID == customerID).ToList();
            DataOfBot.Floors = MainDataOfBot.Floors.Where(x => x.CustomerID == customerID).ToList();
            DataOfBot.Categories = MainDataOfBot.Categories.Where(x => x.CustomerID == customerID).ToList();
            DataOfBot.Synonyms = MainDataOfBot.Synonyms;
            DataOfBot.OrganizationMapObjects = MainDataOfBot.OrganizationMapObjects;
            DataOfBot.MapObjects = MainDataOfBot.MapObjects;
            DataOfBot.MapObjectLinks = MainDataOfBot.MapObjectLinks;
            DataOfBot.MTerminals = MainDataOfBot.MTerminals;
            DataOfBot.TerminalMapObjects = MainDataOfBot.TerminalMapObjects;

            return DataOfBot;
        }


    }
}
