using Mall.Bot.Common.DBHelpers.Models.Common;
using Mall.Bot.Common.FacebookApi.Helpers;
using Mall.Bot.Common.VKApi;
using Moloko.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mall.Bot.Common.Helpers
{
    public class ApiRouter
    {
        private string botSendDBName = "Z_Messages";
        BaseBotUser botUser;
        SocialNetworkType type;
        object Bot;
        List<VKApiRequestModel> Requests;

        public ApiRouter(SocialNetworkType _type, object _Bot, BaseBotUser _botUser, List<VKApiRequestModel> _Requests = null)
        {
            type = _type;
            Bot = _Bot;
            botUser = _botUser;
            Requests = _Requests;
        }

        public async Task<int> Post(string groupID, string text, byte [] photo = null)
        {
            if (SocialNetworkType.VK == type)
            {
                return await (Bot as VK).Wall_Post(groupID, text, photo);
            }
            return 0;
        }

        /// <summary>
        /// Маршрутизатор отправки изображений
        /// </summary>
        /// <param name="image"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public async Task<int> SendPhoto(Bitmap image, string caption = "")
        {
            return await SendPhoto(ImagingHelper.ImageToByteArray(image), caption);
        }
        public async Task<int> SendPhoto(byte [] image, string caption = "")
        {
            TelegramBotClient telegram = null;
            FacebookApiHelper facebook = null;
            int IsError = 0;

            if (type == SocialNetworkType.Telegram) telegram = (TelegramBotClient)Bot;
            if (type == SocialNetworkType.Facebook) facebook = (FacebookApiHelper)Bot;

            if (type == SocialNetworkType.VK)
            {
                //var cont = new SenderContext("Z_Messages");
                //var mes = new BotMessage { BotUserVKID = botUser.BotUserVKID, Text = caption, DateTime = DateTime.Now, IsSended = false, Photo = image };
                //cont.Message.Add(mes);
                //cont.SaveChanges();
                //TODO Убрать
//#if DEBUG
//                System.IO.File.WriteAllBytes(@"C:\Temp\vk.png", image);
//#endif
                Requests.Add(new VKApiRequestModel(ulong.Parse(botUser.BotUserVKID), caption, RequestType.SendMessageWithPhoto, image));
            }
            if (type == SocialNetworkType.Telegram) await telegram.SendPhotoAsync(botUser.BotUserTelegramID, new FileToSend("photo.jpg", new MemoryStream(image)), caption);
            if (type == SocialNetworkType.Facebook) IsError = await facebook.UrlSendPhoto(botUser.BotUserFacebookID, (Bitmap)Image.FromStream(new MemoryStream(image)));

            return IsError;
        }

        /// <summary>
        /// Маршрутизатор текстовых сообщений
        /// </summary>
        /// <param name="thisQuery"></param>
        /// <param name="type"></param>
        /// <param name="Bot"></param>
        /// <param name="botUser"></param>
        /// <param name="text"></param>
        /// <param name="Requests"></param>
        /// <returns></returns>
        public async Task<int> SendText(string text)
        {
            TelegramBotClient telegram = null;
            FacebookApiHelper facebook = null;
            int IsError = 0;

            if (type == SocialNetworkType.Telegram) telegram = (TelegramBotClient)Bot;
            if (type == SocialNetworkType.Facebook) facebook = (FacebookApiHelper)Bot;

            if (type == SocialNetworkType.VK)
            {
                //var cont = new SenderContext("Z_Messages");
                //cont.Message.Add(new BotMessage { BotUserVKID = botUser.BotUserVKID, Text = text, DateTime = DateTime.Now, IsSended = false });
                //cont.SaveChanges();
                Requests.Add(new VKApiRequestModel(ulong.Parse(botUser.BotUserVKID), text));
            }
            if (type == SocialNetworkType.Telegram)
            {
                text = BotTextHelper.SmileCodesReplace(text);
                await telegram.SendTextMessageAsync(botUser.BotUserTelegramID, text);
            }
            if (type == SocialNetworkType.Facebook) IsError = await facebook.SendMessage(botUser.BotUserFacebookID, text);

            return IsError;
        }
    }
}
