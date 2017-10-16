using Newtonsoft.Json;

namespace Mall.Bot.Common.VKApi.Models
{
    public class VKMessage
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("date")]
        public int Date { get; set; }
        [JsonProperty("_out")]
        public int Out { get; set; }
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }
        [JsonProperty("read_state")]
        public int ReadState { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        public VKGeo geo { get; set; }
        public VKAttachment [] attachments { get; set; }
    }
}
