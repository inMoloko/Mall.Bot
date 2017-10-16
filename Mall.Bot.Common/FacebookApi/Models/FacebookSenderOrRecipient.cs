using Newtonsoft.Json;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookSenderOrRecipient
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}