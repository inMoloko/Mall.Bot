using Newtonsoft.Json;

namespace Mall.Bot.Common.VKApi.Models
{
    public class VKJoin
    {
        [JsonProperty("user_id")]
        public ulong User_ID { get; set; }

        [JsonProperty("join_type")]
        public string JoinType { get; set; }
    }
}