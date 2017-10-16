using Mall.Bot.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mall.Bot.Common.FacebookApi.Models
{
    public class SendMessageModel
    {
        public FacebookSenderOrRecipient recipient { get; set; }
        public FacebookMessage message { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Nullable< SenderActionType> sender_action { get; set; }
    }
}