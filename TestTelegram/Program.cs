namespace TestTelegram
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}

//        static void Main(string[] args)
//        {
//    class Program
//    {
//        static void Main(string[] args)
//        {


//            Run().Wait();
//        }

//        private static async Task Run()
//        {
//            var token = "252156027:AAEwcfBNyngaR7FhGjH38JaFTyQ14M4hKhg";
//            var Bot = new Telegram.Bot.Api(token);
//            var me = Bot.GetMe().Result;
//            Console.WriteLine("My name is " + me.FirstName);

//            var dbContext = new MallBotContext("A");
//            var data =  new MallBotModel(dbContext);

//            var offset = 0;
//            while (true)
//            {
//                var updates = await Bot.GetUpdates(offset);
//                var metartext = "Привет, Антон!";
//                foreach (var update in updates)
//                {
//                    var mallBotFunctional = new MallBotFunctional();
//                    QueryAnaliserResult answer = mallBotFunctional.DoWork(update.Message.Text, data);
//                    Analise(answer);


//                    var textRes = await Bot.SendTextMessage(update.Message.Chat.Id, metartext);
//                    var photoRes = SendPhoto(update.Message.Chat.Id.ToString(), "C:\\image.png", token);
//                    offset = update.Id + 1;
//                }
//                await Task.Delay(500);
//            }
//        }

//        public async static Task SendPhoto(string chatId, string filePath, string token)
//        {
//            var url = string.Format("https://api.telegram.org/bot{0}/sendPhoto", token);
//            var fileName = filePath.Split('\\').Last();

//            using (var form = new MultipartFormDataContent())
//            {
//                form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");

//                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//                {
//                    form.Add(new StreamContent(fileStream), "photo", fileName);

//                    using (var client = new HttpClient())
//                    {
//                        await client.PostAsync(url, form);
//                    }
//                }
//            }
//        }

//        public static void Analise(QueryAnaliserResult answer)
//        {
//            byte flag = 10; // 0 - нет запросов, 1 - приветствие, 2 - местоположение одного, 3 - путь, 4 - множественная выдача
//            string badQuery = "";
//            string textForUser = "";
//            if (answer.IsCategorySearch)
//            {
//                textForUser = "Найдена категория - " + answer.FindedCategory;
//                textForUser = textForUser.Remove(textForUser.Length - 2);
//                textForUser += " \\r\\n  \\r\\n ";
//            }

//            var HasBeenFinded = new List<QueryResult>();
//            var HasBeenFindedWithErrors = new List<QueryResult>();

//            if (answer != null)
//            {
//                if (answer.Queries.Count == 0) // нет запросов
//                {
//                    flag = 0;
//                }
//                else
//                {
//                    foreach (Query _query in answer.Queries)
//                    {
//                        if (_query._Query == "приветствие") // если в запросе было приветсвие
//                        {
//                            flag = 1;
//                            break;
//                        }
//                        if (_query.QueryResults != null) // по запросу ничего не нашлось
//                        {
//                            var temp = _query.QueryResults.FindAll(x => x.Log == LogType.OrganizationFindedWithErrors).ToList();
//                            if (temp.Count != 0)
//                                HasBeenFindedWithErrors.AddRange(temp);
//                            temp = null;

//                            temp = _query.QueryResults.FindAll(x => x.Log == LogType.OrganizationFinded).ToList(); // выбираем только супернайденные  организации
//                            if (temp.Count > 1) // найденных организаций больше одной
//                            {
//                                flag = 4;
//                                HasBeenFinded.AddRange(temp);
//                            }
//                            else
//                            {
//                                if (temp.Count == 1 && flag == 2 && answer.Queries.Count == 2 && flag != 4) // если 2 запроса и  у каждого 1 реузьтат
//                                {
//                                    flag = 3;
//                                }
//                                else // флаг для 1го найденного магазина
//                                {
//                                    flag = 2;
//                                    HasBeenFinded.AddRange(temp);
//                                }
//                            }
//                        }
//                        else badQuery += _query._Query + ", ";
//                    }
//                }

//                if (flag == 4 || flag == 2) flag = 2; // множественная выдача идентична нахождению одного магазина. Отдельный флаг нужен для идентификации того, что был запрошел путь

//            }
//        }
//    }
//}
