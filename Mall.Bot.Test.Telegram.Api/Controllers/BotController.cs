using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot.Args;
using Telegram.Bot.Helpers;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace Mall.Bot.Test.Telegram.Api.Controllers
{
    public class BotController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
            };
        }
        static async void testApiAsync()
        {
            var Bot = new Telegram.Bot.Api("your API access Token");
            var me = await Bot.GetMeAsync();
            System.Console.WriteLine("Hello my name is " + me.FirstName);
        }
    }
}
