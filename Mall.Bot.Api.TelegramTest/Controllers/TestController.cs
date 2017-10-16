using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot.Types;

namespace Mall.Bot.Api.TelegramTest.Controllers
{

    public class TestController : ApiController
    {
        string _token = "252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg";

        //public TestController()
        //{
        //    _Bot = new Telegram.Bot.Api("252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg");
        //    _Bot.GetMe();
        //    _Bot.SetWebhook("https://server.inmoloko.ru/Mall.Bot.Telegram.Test/api/test");
        //}
        [HttpPost]
        public async Task<HttpResponseMessage> PostMessage(JObject jsonResponce)
        {
            var TelegramResponce = jsonResponce.ToObject<Update>();

            var _Bot = new Telegram.Bot.Api(_token);
            await _Bot.SendTextMessageAsync(TelegramResponce.Message.Chat.Id, "Привет  " + TelegramResponce.Message.From.FirstName + "  im BOTTTTTT");

            Image img = Image.FromFile("C:\\image.png");
            var buff = new byte[100]; // ImagingHelper.ImageToByteArray(img);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);

            await _Bot.SendPhotoAsync(TelegramResponce.Message.Chat.Id, new FileToSend("photo.png", ms));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok"),
            };
        }
    }
}
