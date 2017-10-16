using Mall.Bot.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookAttachment
    {
        public string title { get; set; }
        public string url { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AttachmentType type { get; set; }
        public FacebookPayLoad payload { get; set; }
    }
}