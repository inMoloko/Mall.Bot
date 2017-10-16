using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Common.MFCHelpers.Models;
using Moloko.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;

namespace Mall.Bot.Common.Helpers
{
    public class CacheHelper
    {
        public void Set(string key, object data, int? minutes = null)
        {
            Remove(key);

            Logging.Logger.Debug($"Caching with a KEY = {key}");
            CacheItemPolicy cip = null;

            if (minutes != null)
            {
                cip = new CacheItemPolicy()
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes((double)minutes))
                };
            }
            else
            {
                string[] TimeOfExpiration = ConfigurationManager.AppSettings["TimeOfExpiration"].ToString().Split(':');
                cip = new CacheItemPolicy()
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(int.Parse(TimeOfExpiration[0])).AddMinutes(int.Parse(TimeOfExpiration[1])).AddSeconds(int.Parse(TimeOfExpiration[2])))
                };
            }
            MemoryCache.Default.Set(new CacheItem(key, data), cip);

        }
        public object Get(string key)
        {
            return MemoryCache.Default.Get(key, null);
        }
        public void Remove(string key)
        {
            if (Get(key) != null)
            {
                MemoryCache.Default.Remove(key);
            }
        }
        public MFCBotModel Update(string key, MFCBotContext dbContext)
        {
            Remove(key);
            var dataOfBot = new MFCBotModel(dbContext);
            Set(key, dataOfBot);
            return dataOfBot;
        }

        public List<CachedDataModel> Update(string key, List<MallBotContext> dbContextes)
        {
            Remove(key);

            char dbID = 'B';
            for (int i = 1; i < int.Parse(ConfigurationManager.AppSettings["dbCount"]); i++)
            {
                dbContextes.Add(new MallBotContext(dbID.ToString()+ConfigurationManager.AppSettings["dbTest"]));
                dbContextes[i].Configuration.ProxyCreationEnabled = false;
                dbID++;
            }

            var datasOfBot = new List<CachedDataModel>();
            for (int i = 0; i < int.Parse(ConfigurationManager.AppSettings["dbCount"]); i++)
            {
                datasOfBot.Add(new CachedDataModel(dbContextes[i]));
            }
            
            Set(key, datasOfBot);
            return datasOfBot;
        }
        public void Clear(int BotUserID, string [] keys)
        {
            foreach (var item in keys)
            {
                Remove($"{item}{BotUserID}");
            }
        }
        public void Clear(string BotUserVKID)
        {
            Remove($"SETSERVICE{BotUserVKID}");
            Remove($"SETSERVICES{BotUserVKID}");
            Remove($"SETOFFICES{BotUserVKID}");
            Remove($"QUESTION{BotUserVKID}");

        }
        public void Clear(int BotUserID)
        {
            Remove($"SEARCHWAY{BotUserID}");
            Remove($"SETCUSTOMER{BotUserID}");
            Remove($"VIEWALLORG{BotUserID}");
            Remove($"FINDEDFIRSTORG{BotUserID}");

            Remove($"FINDEDCARDS{BotUserID}");
            Remove($"CACHEDCARDNAME{BotUserID}");
            Remove($"CACHEDIMAGE{BotUserID}");
        }
    }
}
