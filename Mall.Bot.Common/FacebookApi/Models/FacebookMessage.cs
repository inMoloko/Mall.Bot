namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookMessage
    {
        public string mid { get; set; }
        public string seq { get; set; }
        public string text { get; set; }
        public FacebookAttachment[] attachments { get; set; }
        public FacebookAttachment attachment { get; set; }
        public FacebookQuickReplie[] quick_replies { get; set; }
    }
}