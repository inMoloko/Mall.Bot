using Newtonsoft.Json;

namespace Mall.Bot.Common.VKApi.Models
{
    public class VKResponce
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }


    }
}
