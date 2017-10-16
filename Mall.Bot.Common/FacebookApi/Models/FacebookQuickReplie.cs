using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Mall.Bot.Common.Helpers;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookQuickReplie
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentType content_type { get; set; }
        public string title { get; set; }
        public string payload { get; set; }
    }
}