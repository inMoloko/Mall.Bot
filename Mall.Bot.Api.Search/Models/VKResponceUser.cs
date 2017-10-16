using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Bot.Api.Search.Models
{
    public class VKResponceUser
    {
        [JsonProperty("response")]
        public VKUser[] response { get; set; }
    }
}