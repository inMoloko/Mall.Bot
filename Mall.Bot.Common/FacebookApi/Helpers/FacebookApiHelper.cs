using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.FacebookApi.Models;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moloko.Utils;
using System.Configuration;

namespace Mall.Bot.Common.FacebookApi.Helpers
{
    public class FacebookApiHelper
    {
        private string _token;

        public FacebookApiHelper(string token)
        {
            _token = token;
        }

        /// <summary>
        /// Генерит jsonчик и отправляет его фейсбуку
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<int> Send(SendMessageModel model)
        {
            string jsonContent = JsonConvert.SerializeObject(model,
                                Formatting.None,
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                });
            
            jsonContent = BotTextHelper.SmileCodesReplace(jsonContent, SocialNetworkType.Facebook);

            HttpResponseMessage responce;
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            string url = $"https://graph.facebook.com/v2.6/me/messages?access_token={_token}";
            using (var client = new HttpClient())
            {
                using (var r = await client.PostAsync(new Uri(url), content))
                {
                    responce = r;
                    string responceString = await r.Content.ReadAsStringAsync();
                    if (responceString.ToLower().Contains("error"))
                    {
                        Logging.Logger.Error($"Facebook Api Send: NOT OK!! {responceString}");
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        /// <summary>
        /// Отправка текстового сообщения и/или кнопочек "Быстрые ответы"
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="message"></param>
        /// <param name="quick_replies"></param>
        /// <returns></returns>
        public async Task<int> SendMessage(string toID, string message, string[] quick_replies = null)
        {
            SendMessageModel toMessage = new SendMessageModel();
            toMessage.sender_action = null;
            toMessage.recipient = new FacebookSenderOrRecipient { Id = toID };
            toMessage.message = new FacebookMessage { text = message };

            if (quick_replies != null && quick_replies.Length != 0)
            {
                toMessage.message.quick_replies = new FacebookQuickReplie[quick_replies.Length];
                for (int i = 0; i < quick_replies.Length; i++)
                {
                    toMessage.message.quick_replies[i] = new FacebookQuickReplie { content_type = ContentType.text, title = quick_replies[i], payload = "HaveChoosen:" + quick_replies[i] };
                }
            }
            return await Send(toMessage);
        }
        /// <summary>
        /// Кэширование фото. Отправка URL на это фото
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public async Task<int> UrlSendPhoto(string toID, Bitmap bmp)
        {
            //object ImageFromCache = MemoryCache.Default.Get($"Image{toID}");
            //if (ImageFromCache != null)
            //{
            //    MemoryCache.Default.Remove($"Image{toID}");
            //    ImageFromCache = null;
            //}

            //CacheItemPolicy cip = new CacheItemPolicy()
            //{
            //    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(20))
            //};
            //MemoryCache.Default.Set(new CacheItem(, ), cip);

            var cacher = new CacheHelper();
            cacher.Set($"Image{toID}", bmp, 1);

            SendMessageModel toMessage = new SendMessageModel();
            toMessage.sender_action = null;
            toMessage.recipient = new FacebookSenderOrRecipient { Id = toID };
            toMessage.message = new FacebookMessage
            {
                attachment = new FacebookAttachment { type = AttachmentType.image, payload = new FacebookPayLoad { url = $"{ConfigurationManager.AppSettings["ServerAdress"]}?key=Image{toID}" } }
            };

            return await Send(toMessage);
        }

        public async Task<int> SendAction(string toID, SenderActionType type)
        {
            SendMessageModel toMessage = new SendMessageModel();
            toMessage.recipient = new FacebookSenderOrRecipient { Id = toID };
            toMessage.sender_action = type;

            return await Send(toMessage);
        }

        public async Task<object> GetUsersInformation(string id)
        {
            string responceString = "";
            string url = $"https://graph.facebook.com/v2.6/{id}?access_token={_token}";

            using (var client = new HttpClient())
            {
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    responceString = r.Content.ReadAsStringAsync().Result;
                    if (responceString.ToLower().Contains("error"))
                    {
                        return '¡' + responceString;
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<FacebookUser>(responceString);
                    }
                }
            }
        }
    }
}