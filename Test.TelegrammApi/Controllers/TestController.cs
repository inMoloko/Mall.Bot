using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Test.TelegrammApi.Controllers
{
    public class TestController : ApiController
    {
        [HttpPost]
        public void PostMessage()
        {
            testApi();
        }

        static void testApi()
        {
            var Bot = new Telegram.Bot.Api("252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg");
            var me = Bot.GetMeAsync().Result;
            var waaaat = Bot.GetUpdatesAsync().Result;
            //var res = Bot.SendTextMessageAsync().Result;
            System.Console.WriteLine("Hello my name is " + me.FirstName);
        }
    }
}
