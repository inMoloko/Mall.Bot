using Newtonsoft.Json;

namespace Mall.Bot.Common.VKApi.Models
{
    public class VKResponceUser
    {
        [JsonProperty("response")]
        public VKUser[] response { get; set; }
        /// <summary>
        /// Нужен для обнаружения ошибки при вызове методово VK API. 
        /// VK.GetUsersInformation(userID) - вернет VKResponceUser либо специальный символ, сигнализирующий об ошибке.
        /// для результата метода будет вызван ToString() который вернет ok, если все ок)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ok";
        }
    }
}