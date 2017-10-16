using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Bot.Api.Search.Models
{
    public class VKJoin
    {
        [JsonProperty("user_id")]
        public ulong User_ID { get; set; }

        [JsonProperty("join_type")]
        public string JoinType { get; set; }
    }
}