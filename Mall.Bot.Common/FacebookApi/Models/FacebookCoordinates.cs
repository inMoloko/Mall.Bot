using Newtonsoft.Json;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookCoordinates
    {
        public float lat { get; set; }
        [JsonProperty("long")]
        public float Long { get; set; }
    }
}
