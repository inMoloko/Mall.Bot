namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookMessaging
    {
        public FacebookSenderOrRecipient sender { get; set; }
        public FacebookSenderOrRecipient recipient { get; set; }
        public ulong timestamp { get; set; }
        public FacebookMessage message { get; set; }
        public FacebookPostBack postback { get; set; }
    }
}