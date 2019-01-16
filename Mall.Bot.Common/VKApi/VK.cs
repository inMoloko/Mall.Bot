using Mall.Bot.Common.Helpers;
using Mall.Bot.Common.VKApi.Models;
using Moloko.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mall.Bot.Common.VKApi
{
    public class VK
    {
        static string _token; // сделал статическим для более гибкого использования
        string _apptoken;
        public VK(string token)
        {
            _token = token;
        }
        public VK(string token, string appToken)
        {
            _token = token;
            _apptoken = appToken;
        }

    public async Task<int> markAsRead(int message_id)
        {
            var url = $"https://api.vk.com/method/messages.markAsRead?message_ids={message_id}&access_token={_token}&v=5.56";
            HttpResponseMessage responce = null;
            using (var client = new HttpClient())
            {
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    responce = r;
                    string responceString = await r.Content.ReadAsStringAsync();
                    if (responceString.ToLower().Contains("error"))
                    {
                        Logging.Logger.Error($"VK Api MarkAsRead: NOT OK {responceString}");
                        return 1;
                    }
                    else
                        return 0;
                }
            }
        }

        public async Task<int> Wall_Post (string groupID, string message, byte[] image = null)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();

                using (var client = new HttpClient(handler))
                {
                    message = BotTextHelper.SmileCodesReplace(message, SocialNetworkType.VK);
                    message = message.Replace("\\n", "\n");
                    message = HttpUtility.UrlEncode(message);
                    string responceString;

                    if (image != null)
                    {
                        var url = $"https://api.vk.com/method/photos.getWallUploadServer?access_token={_apptoken}&group_id={groupID}";
                        using (var responce = await client.GetAsync(url))
                        {
                            responceString = responce.Content.ReadAsStringAsync().Result;
                            if (responceString.Contains("error") || !responce.IsSuccessStatusCode)
                            {
                                Logging.Logger.Error($"VK Api Wall Post GetUrl: NOT OK {responceString}");
                                return 1;
                            }
                        }

                        JObject json = JObject.Parse(responceString);
                        var uploadImageUrl = json["response"]["upload_url"].ToString();

                        //Загружаем изображение
                        MultipartFormDataContent form = new MultipartFormDataContent();
                        form.Add(new ByteArrayContent(image), "photo", "photo.jpg");
                        using (var responce = await client.PostAsync(uploadImageUrl, form))
                        {
                            responceString = responce.Content.ReadAsStringAsync().Result;
                            if (responceString.Contains("error") || !responce.IsSuccessStatusCode)
                            {
                                Logging.Logger.Error($"VK Api Wall Post LoadPhotoToVkServ: NOT OK {responceString}");
                                return 1;
                            }
                        }
                        json = JObject.Parse(responceString);

                        var photo = json["photo"];
                        var server = json["server"];
                        var hash = json["hash"];

                        url = $"https://api.vk.com/method/photos.saveWallPhoto?access_token={_apptoken}&group_id={groupID}&server={server}&photo={photo}&hash={hash}";
                        using (var responce = await client.GetAsync(url))
                        {
                            responceString = responce.Content.ReadAsStringAsync().Result;

                            if (responceString.Contains("error") || !responce.IsSuccessStatusCode)
                            {
                                Logging.Logger.Error($"VK Api SavePhotoToVkServ: NOT OK {responceString}");
                                return 1;
                            }
                        }
                        json = JObject.Parse(responceString);
                        var attachment = json["response"][0]["id"].ToString();//photo{owner_id}_{pid}


                        url = $"https://api.vk.com/method/wall.post?owner_id=-{groupID}&message={message}&attachments={attachment}&access_token={_apptoken}";

                        using (var res = await client.GetAsync(url))
                        {
                            responceString = res.Content.ReadAsStringAsync().Result;

                            if (responceString.Contains("error") || !res.IsSuccessStatusCode)
                            {
                                Logging.Logger.Error($"Vk Api SendMessageWithPhoto: NOT OK {responceString}");
                                return 1;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        var url = $"https://api.vk.com/method/wall.post?owner_id=-{groupID}&message={message}&access_token={_apptoken}";
                        using (var res = await client.GetAsync(url))
                        {
                            responceString = res.Content.ReadAsStringAsync().Result;
                            if (responceString.Contains("error") || !res.IsSuccessStatusCode)
                            {
                                Logging.Logger.Error($"Vk Api SendMessageWithPhoto: NOT OK {responceString}");
                                return 1;
                            }
                        }
                        return 0;

                    }
                }
            }

            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return 1;
            }
        }

        public async Task<object> GetUsersInformation(ulong userID)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            var httpClient = new HttpClient(handler);
            var url = $"https://api.vk.com/method/users.get?user_ids={userID}&fields=sex,bdate&v=5.53";

            var responceString = "";
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
                        return JsonConvert.DeserializeObject<VKResponceUser>(responceString);
                    }
                }
            }
        }
        // сделал статическим для более гибкого использования. Для удобства отправки одного сообщения
        public static async Task<int> SendMessage(ulong userID, string message)
        {
            message = HttpUtility.UrlEncode(message);
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            var httpClient = new HttpClient(handler);

            var url = $"https://api.vk.com/method/messages.send?user_id={userID}&message={message}&access_token={_token}&v=5.53";

            HttpResponseMessage res = await httpClient.GetAsync(url);
            string resString = res.Content.ReadAsStringAsync().Result;
            if (!res.IsSuccessStatusCode || resString.ToLower().Contains("error"))
            {
                Logging.Logger.Error($"VK Api SendMessage: NOT OK!!! {resString}");
                return 1;
            }
            else
            {
                return 0;
            }
        }


        public async Task<int> SendMessageWithPhoto(ulong userID, string message, byte[] image)
        {
            int IsError = 0;
            try
            {
                message = HttpUtility.UrlEncode(message);
                var handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                var httpClient = new HttpClient(handler);


                //photos.getMessagesUploadServer - получаем url сервиса для загрузки изображения
                var url = $"https://api.vk.com/method/photos.getMessagesUploadServer?access_token={_token}&v=5.56";
                var responce = await httpClient.GetAsync(url);
                var responceString = responce.Content.ReadAsStringAsync().Result;

                    if (responceString.Contains("error"))
                    {
                        IsError = 1;
                        Logging.Logger.Error($"VK Api GetUrl: NOT OK {responceString}" );
                    }

                JObject json = JObject.Parse(responceString);
                var uploadImageUrl = json["response"]["upload_url"].ToString();

                //Загружаем изображение
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new ByteArrayContent(image), "photo", "photo.jpg");
                responce = await httpClient.PostAsync(uploadImageUrl, form);
                responceString = responce.Content.ReadAsStringAsync().Result;

                    if (responceString.Contains("error"))
                    {
                        IsError = 1;
                        Logging.Logger.Error($"VK Api LoadPhotoToVkServ: NOT OK {responceString}");
                    }

                json = JObject.Parse(responceString);

                var photo = json["photo"];
                var server = json["server"];
                var hash = json["hash"];

                url = $"https://api.vk.com/method/photos.saveMessagesPhoto?photo={photo}&hash={hash}&server={server}&access_token={_token}&v=5.56";
                responce = await httpClient.GetAsync(url);
                responceString = responce.Content.ReadAsStringAsync().Result;

                    if (responceString.Contains("error"))
                    {
                        IsError = 1;
                        Logging.Logger.Error($"VK Api SavePhotoToVkServ: NOT OK {responceString}");
                    }

                json = JObject.Parse(responceString);
                var attachment = "photo" + json["response"][0]["owner_id"] + "_" + json["response"][0]["id"];


                url = $"https://api.vk.com/method/messages.send?attachment={attachment}&user_id={userID}&message={message}&access_token={_token}&v=5.52";

                HttpResponseMessage res = await httpClient.GetAsync(url);
                responceString = res.Content.ReadAsStringAsync().Result;

                    if (responceString.Contains("error"))
                    {
                        IsError = 1;
                        Logging.Logger.Error($"Vk Api SendMessageWithPhoto: NOT OK {responceString}");
                    }

                return IsError;
            }
            catch (Exception exc)
            {
                return IsError;
            }
        }


        /// <summary>
        /// Не больше 25 сообщений за раз.
        /// </summary>
        /// <param name="Requests"></param>
        /// <returns></returns>
        public async Task<int> SendAllRequests(List<VKApiRequestModel> Requests)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            var httpClient = new HttpClient(handler);

            string uploadImageUrl = "";
            var IsError = 0;
            string url = "";
            HttpResponseMessage responce = null;
            var responceString = "";
            JObject json = null;
            if (Requests.Where(x => x.Type == RequestType.SendMessageWithPhoto).Count() > 0)
            {
                // получаем url для загрузки изображений на сервак
                url = $"https://api.vk.com/method/photos.getMessagesUploadServer?access_token={_token}&v=5.57";
                responce = await httpClient.GetAsync(url);
                responceString = responce.Content.ReadAsStringAsync().Result;

                if (responceString.Contains("error"))
                {
                    IsError = 1;
                    Logging.Logger.Error($"VK Api Get Url: NOT OK {responceString}");
                }

                json = JObject.Parse(responceString);
                uploadImageUrl = json["response"]["upload_url"].ToString();
            }
            string ids = "";
            string messages = "";
            string types = "";
            string photos = "";
            string hashs = "";
            string servers = "";
            
            foreach (var item in Requests)
            {
                ids += item.User_ID.ToString() + ", ";
                messages += "\"" + item.Message.Replace("\"", "\\\"") + "\", ";
                types += ((int)item.Type).ToString() + ", ";

                if (item.Type == RequestType.SendMessageWithPhoto)
                {
                    //Загружаем изображение
                    MultipartFormDataContent form = new MultipartFormDataContent();
                    form.Add(new ByteArrayContent(item.Photo), "photo", "photo.jpg");
                    responce = await httpClient.PostAsync(uploadImageUrl, form);
                    responceString = responce.Content.ReadAsStringAsync().Result;

                        if (responceString.Contains("error"))
                        {
                            IsError = 1;
                            Logging.Logger.Error($"VK Api Load: NOT OK {responceString}" );
                        }

                    json = JObject.Parse(responceString);
                    var photo = json["photo"];
                    var server = json["server"];
                    var hash = json["hash"];

                        if (photo.ToString() == "[]")
                        {
                            await SendMessage(item.User_ID, "Опаньки! Мы попали на неудачный сервер. Мы уже на пути к тому, чтобы устранить эту проблему, но пока я не смогу ответить на ваши вопросы 3(");
                            IsError = 1;
                            return IsError;
                        }

                    photos += "\"" + photo.ToString().Replace("\"", "\\\"") + "\", ";
                    servers += "\"" + server.ToString()+ "\", ";
                    hashs += "\"" + hash.ToString() + "\", ";
                }
                else
                {
                    photos += "\"empty\", ";
                    servers += "\"empty\", ";
                    hashs += "\"empty\", ";
                }
            }
            // передает в скрип "code" параметры
            var code = Properties.Resources.SendMessageWithPhotoSkript;
            code = code.Replace("Pids", ids.Remove(ids.Length-2));
            code = code.Replace("Pmessages", messages.Remove(messages.Length - 2));
            code = code.Replace("Phashs", hashs.Remove(hashs.Length - 2));
            code = code.Replace("Pphotos", photos.Remove(photos.Length - 2));
            code = code.Replace("Pservers", servers.Remove(servers.Length - 2));
            code = code.Replace("Ptypes", types.Remove(types.Length - 2));
            code = code.Replace("Pcount", Requests.Count.ToString());
            code = BotTextHelper.SmileCodesReplace(code, SocialNetworkType.VK);
            code = HttpUtility.UrlEncode(code);
            //генерим контент для пост запроса
            string data = "code=" + code + "&access_token=" + _token + "&v=5.53";
            StringContent parametrs = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            //вызываем метод, параметром которого является скрипт, который выполнится на строне vk.com без лимитов (почти)
            url = $"https://api.vk.com/method/execute";

            using (var client = new HttpClient())
            {
                using (var r = await client.PostAsync(new Uri(url), parametrs))
                {
                    responce = r;
                    responceString = await r.Content.ReadAsStringAsync();


                    if (responceString.Contains("error"))
                    {
                        IsError = 1;
                        Logging.Logger.Error($"VK Api Execute: NOT OK {responceString}, data {data}");
                    }
                }
            }
            return IsError;
        }
    }
}