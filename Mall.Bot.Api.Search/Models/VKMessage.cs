using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Bot.Api.Search.Models
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
    }
}
