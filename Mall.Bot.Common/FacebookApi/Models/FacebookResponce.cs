using Newtonsoft.Json;
using Mall.Bot.Common.Helpers;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookResponce
    {
        [JsonProperty ("object") ]
        public ObjectTypes Object { get; set; }
        public FacebookEntry [] entry { get; set; }
    }
}