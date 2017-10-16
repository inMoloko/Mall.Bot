using System.Net;
using System.Net.Http;

namespace Mall.Bot.AutomaticTest
{
    public class Test
    {
        public string Query { get; set; }
        public string Answer { get; set; }
        public override string ToString()
        {
            return $"Query: {Query}, Answer: {Answer}";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            var httpClient = new HttpClient(handler);

            ///string code = "return [ API.messages.send({\"user_id\": " + userID.ToString() + ", \"message\": \" " + message.ToString() + "\"}) ];";   !!!!!!
                
            /// var url = $"https:/ /api.vk.com/method/execute?code={code}&access_token={_token}&v=5.52";!!!!!!!


            ///string code = "return [ API.messages.send({\"user_id\": 35890850, \"message\": \"hello syka\"}) ];";

            ///var url = String.Format("https:/ /api.vk.com/method/execute?code={0}", code);


            //Task<HttpResponseMessage> temp = httpClient.GetAsync(url);
            //string res = temp.Content.ReadAsStringAsync().Result;



            //int customerID = 1;

            ////string tests = Mall.Bot.AutomaticTest.Properties.Resources.tests;
            //string tests = Mall.Bot.AutomaticTest.Properties.Resources.ТРК_Радуга;

            //Test[][] Tests = JsonConvert.DeserializeObject<Test[][]>(tests);
            //var dbContext = new MallBotContext();
            //var collection = dbContext.CuttedOrganization.Where(x => x.CustomerID == customerID).ToList();
            //collection = collection.OrderByDescending(x => x.Rating).ToList();

            //var mallBotFunctional = new MallBotFunctional();

            //var synonyms = dbContext.OrganizationSynonym.ToList();
            //mallBotFunctional.GetSynonyms(collection, synonyms);

            //int k = 0; int er = 0; int df = 0;

            //while (k < Tests.Length)
            //{
            //    //while (true)
            //    //{ 
            //    Console.WriteLine("Напишите название одного или двух магазинов, и я пришлю где находится магазин или маршрут к нему");

            //    string s = Tests[k][0].Query;
            //    //string s = Console.ReadLine();
            //    Console.WriteLine(s);

            //    QueryAnaliserResult answer = mallBotFunctional.DoWork(s, customerID, collection);

            //    if (answer != null)
            //    {
            //        if (answer.Message.Count != 0)
            //        {
            //            if (answer.Message[answer.Message.Count - 1] != "TheWayIsNotExist")
            //            {
            //                bool flag = true;
            //                for (int i = 0; i < answer.Message.Count; i++)
            //                {
            //                    if (answer.Message[i] != "OrganizationFinded") Console.WriteLine("К сожалению по запросу «" + answer.Message[i] + "» магазина не нашлось");
            //                    else
            //                    if (flag)
            //                    {
            //                        Console.WriteLine("A -  " + answer.First.Name);
            //                        flag = false;
            //                    }
            //                    else
            //                    {
            //                        Console.WriteLine("B -  " + answer.Second.Name);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("К сожалению, по запросу « " + answer.First.Name + " " + answer.Second.Name + " » пути не нашлось");
            //            }
            //        }
            //    }
            //    Console.WriteLine(Tests[k][0].Answer);
            //    if (answer.First != null)
            //    {
            //        if (Tests[k][0].Answer != null)
            //        {
            //            if (answer.First.Name.ToLower() == Tests[k][0].Answer.ToLower()) Console.WriteLine("Ok");
            //            else
            //            {
            //                Console.WriteLine("NOT OK!!!!!");
            //                er++;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (Tests[k][0].Answer != null) df++;
            //    }

            //    k++;
            //}
            //Console.WriteLine(er + " - ошибок, " + df + " - не нашел, " + Tests.Length.ToString() + " - всего тестов");
            //Console.ReadLine();
        }
    }
}
