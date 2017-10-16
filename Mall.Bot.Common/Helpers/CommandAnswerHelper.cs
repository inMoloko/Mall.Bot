using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.MallHelpers.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;

namespace Mall.Bot.Common.Helpers
{
    public class CommandAnswerHelper
    {
        /// <summary>
        /// Парсер команды /testfunc_route_=номер этажа=
        /// </summary>
        /// <param name="command"></param>
        /// <param name="dataOfBot"></param>
        /// <param name="floorNumber"></param>
        /// <returns></returns>
        public byte RouteAnalise(string command, CachedDataModel dataOfBot, out int floorNumber)
        {
            var parse = command.Split('_');
            if (parse[0] == "testfunc" && parse[1] == "route" && int.TryParse(parse[2], out floorNumber))
            {
                int temp = floorNumber;
                if (dataOfBot.Floors.FirstOrDefault(x => x.Number == temp) != null)
                {
                    return 1;
                }
                return 2; // этажа с таким номером нет
            }
            floorNumber = -1;
            return 3; // синтаксическая ошибка
        }
        /// <summary>
        /// Парсер команды /mallset_place_=БДID==ТЦID=
        /// </summary>
        /// <param name="command"></param>
        /// <param name="dataOfBot"></param>
        /// <param name="floorNumber"></param>
        /// <returns></returns>
        public byte PlaceAnalise(string command, out Customer thisCustomer)
        {
            var parse = command.Split('_');
            thisCustomer = null;

            if (parse[0] == "mallset" && parse[1] == "place" && !string.IsNullOrWhiteSpace(parse[2]))
            {
                int parsedCustomerID = 0;

                char dbID = 'a';
                for (int i = 0; i < int.Parse(ConfigurationManager.AppSettings["dbCount"]); i++)
                {
                    if (dbID == parse[2][0])
                    {
                        if (int.TryParse(parse[2].Remove(0, 1), out parsedCustomerID))
                        {
                            object dataFromCache = MemoryCache.Default.Get("DataOfBot", null);
                            if (dataFromCache == null) return 4;//На данный момент кэш пуст
                            else
                            {
                                List<CachedDataModel> temp = (List<CachedDataModel>)dataFromCache;
                                thisCustomer = temp[i].Customers.FirstOrDefault(x => x.CustomerID == parsedCustomerID);
                                if (thisCustomer == null) return 2;//Кастомера с таким ID нет
                                else return 1;
                            }
                        }
                    }
                    dbID++;
                }
            }
            return 3; // синтаксическая ошибка
        }
    }
}
