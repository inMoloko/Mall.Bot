namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookEntry
    {
        public string id { get; set; }
        public  ulong time { get; set; }
        public FacebookMessaging [] messaging { get; set; }
    }
}