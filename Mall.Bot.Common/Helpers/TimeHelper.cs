using Moloko.Utils;
using System;

namespace Mall.Bot.Common.Helpers
{
    public class TimeHelper
    {
        public static ulong GetMinutes(DateTime lastActivity)
        {
            var subTime = DateTime.Now.Subtract(lastActivity);
            return (ulong)(subTime.Days * 1440 + subTime.Hours * 60 + subTime.Minutes);
        }
        public static bool IsNewEvent(int unixTimeStampOfEvent, int someIdentifier = 1)
        {
            var cache = new CacheHelper();
            var unixTimestampobj = cache.Get($"{someIdentifier}UNXTMSTMP");

            if (unixTimeStampOfEvent <= (int)(unixTimestampobj ?? 0))
            {
                Logging.Logger.Info($"An old event occurred {(int)(unixTimestampobj)}");
                return false;
            }
            cache.Set($"{someIdentifier}UNXTMSTMP", unixTimeStampOfEvent, 5);
            return true;
        }
    }
}
