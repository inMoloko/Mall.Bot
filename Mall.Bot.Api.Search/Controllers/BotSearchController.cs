using Mall.Bot.Api.Search.Models;
using Mall.Bot.Common;
using Mall.Bot.Common.VKApi;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Runtime.Caching;
using System.Configuration;
using Mall.Bot.Api.Search.Helpers;
using Moloko.Utils.Base;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DBHelpers;

namespace Mall.Bot.Api.Search.Controllers
{
    public class BotSearchController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            var vkResponce = jsonResponce.ToObject<VKResponce>();

            Logging.Logger.Debug($"PostMessage message={jsonResponce}");

            if (vkResponce == null)
            {
                Logging.Logger.Error("Пустой запрос");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            if (vkResponce.GroupId != 127789119)
            {
                Logging.Logger.Error($"Группа с идентификатором {vkResponce.GroupId} не поддерживается ботом MOLOKO");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            if (vkResponce.Type == "confirmation")
            {
                if (vkResponce.GroupId == 127789119)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("38ffe9fe", Encoding.UTF8, "text/html")
                    };
                }
            }

            if (vkResponce.Type == "message_new")
            {
                try
                {
                    var vk = new VK("9a50a582a63ff80cb9d03a5333984e3932dc13cc6ec150a1cf0026bef7c273084b3e0a55c7ebbc42f08f4");
                    var vkMessage = jsonResponce["object"].ToObject<VKMessage>();
                    List<BotCustomer> customers = null;
                    string CachedItemKey = "Customers";
                    var dbContext = new MallBotSearchContext(); 
                    object dataFromCache = MemoryCache.Default.Get(CachedItemKey, null);

                    if (dataFromCache == null)
                    {
                        customers = dbContext.BotCustomer.ToList();

                        string[] TimeOfExpiration = ConfigurationManager.AppSettings["TimeOfExpiration"].ToString().Split(':');
                        CacheItemPolicy cip = new CacheItemPolicy()
                        {
                            AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(int.Parse(TimeOfExpiration[0])).AddMinutes(int.Parse(TimeOfExpiration[1])).AddSeconds(int.Parse(TimeOfExpiration[2])))
                        };
                        MemoryCache.Default.Set(new CacheItem(CachedItemKey, customers), cip);
                    }
                    else
                    {
                        customers =  (List<BotCustomer>)dataFromCache;
                    }
                    var searchHerper = new SearchHelper();
                    var analizer = new QueryAnaliser();
                    var findedCustomers = searchHerper.GetCustomer(analizer.NormalizeQuery(vkMessage.Body), customers);
                    string message = "Ура! :-) Я нашла: \r\n \r\n";
                    if (findedCustomers.Count != 0)
                    {
                        foreach (var item in findedCustomers)
                        {
                            var temp = customers.FirstOrDefault(x => x.BotCustomerID == item.ID);
                            if (!string.IsNullOrWhiteSpace(temp.VKGroupName) && !string.IsNullOrWhiteSpace(temp.Name))
                            {
                                message += "« "+ temp.Name + " » \r\n vk.me/"+temp.VKGroupName + "\r\n \r\n";
                            }
                        }
                    }
                    else message = "к сожалению я ничего не нашла 3(";
                    AsyncHelper.RunSync(() => vk.SendMessage(vkMessage.UserId, message));
                }
                catch (Exception exc)
                {
                    Logging.Logger.Error(exc);
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("ВСЕ ОЧЕНЬ ПЛОХО!"),
                    };
                }
            }
            if (vkResponce.Type == "group_join")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok"),
                };
            }
            Logging.Logger.Error(jsonResponce.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
    public static class Logging
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
