using Moloko.Utils;
using System;
using System.Configuration;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace UpdateMemoryCache
{
    public partial class ServiceByYan : ServiceBase
    {
        private Updater _updater = new Updater();
        public ServiceByYan()
        {
            InitializeComponent();
        }

        public void Start()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            _updater.HandleTimerElapsed(null, null);
            _updater.Start();
        }

        protected override void OnStop()
        {
            _updater.Stop();
        }
    }
    public class Updater
    {
        private string BotUrl;
        readonly Timer _timer;

        public Updater()
        {
            _timer = new Timer(60000 * int.Parse(ConfigurationManager.AppSettings["CacheUpdateInterval"])) { AutoReset = true }; 
            _timer.Elapsed += HandleTimerElapsed;
            BotUrl = ConfigurationManager.AppSettings["BotUrl"];
        }
        public void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonContent = Properties.Resources.ContentVK;
                    jsonContent = jsonContent.Replace("%date%", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
                    HttpResponseMessage res = client.PostAsync(BotUrl + "/api/bot", new StringContent(jsonContent, Encoding.UTF8, "application/json")).Result; // запрос на веб сервис
                    var resStringVK = res.Content.ReadAsStringAsync().Result;
                    
                    if (resStringVK.Contains("error"))
                    {
                        Logging.Logger.Error(resStringVK);
                    }
                    else
                    {
                        Logging.Logger.Debug("OK" + Environment.NewLine);
                    }
                }
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
            }
        }

        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }
    }
}
