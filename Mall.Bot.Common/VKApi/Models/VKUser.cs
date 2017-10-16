using Newtonsoft.Json;

namespace Mall.Bot.Common.VKApi.Models
{
    public class VKUser
    {
        [JsonProperty("id")]
        public ulong id { get; set; }

        [JsonProperty("first_name")]
        public string first_name { get; set; }

        [JsonProperty("last_name")]
        public string last_name { get; set; }

        [JsonProperty("sex")]
        public byte sex { get; set; }

        [JsonProperty("bdate")]
        public string bdate { get; set; }
    }
}