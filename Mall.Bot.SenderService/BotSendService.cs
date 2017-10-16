using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.VKApi;
using Moloko.Utils.Base;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Mall.Bot.SenderService
{
    public partial class BotSendService : ServiceBase
    {
        public BotSendService()
        {
            InitializeComponent(); 
        }
        public void Start()
        {
            OnStart(null);
        }
        Thread thread;
        //string token = "157c278b4e80a8bcade8eab4f4c0a99e2d6bc3f6fb9f0736763e8600e3682e3a0471f126a34a52e37534e";
        VK vk = new VK(ConfigurationManager.AppSettings["token"]);

        protected override void OnStart(string[] args)
        {
            thread = new Thread(DoWork);
            thread.Start();
        }

        protected override void OnStop()
        {
            thread.Abort();
        }

        public void DoWork()
        {
            var context = new SenderContext("database");
            var Requests = new List<VKApiRequestModel>();

            while (true)
            {
                try
                {
                    var list = context.Message.Where(x => !x.IsSended && x.BotUserVKID != null).ToArray();
                    int count = 0;
                    //неотправленные запросы пользователям Вконтакте
                    foreach (var item in list)
                    {
                        count++;
                        if (item.Photo != null) Requests.Add(new VKApiRequestModel(ulong.Parse(item.BotUserVKID), item.Text, RequestType.SendMessageWithPhoto, item.Photo));
                        else Requests.Add(new VKApiRequestModel(ulong.Parse(item.BotUserVKID), item.Text));
                        item.IsSended = true;

                        if (count >= 20 && count % 20 == 0)
                        {
                            var res = AsyncHelper.RunSync(() => vk.SendAllRequests(Requests));
                            if (res == 0) context.SaveChanges();
                            else context.UndoChanges();
                            Requests.Clear();
                        }
                    }
                    if (Requests.Count > 0)
                    {
                        var res = AsyncHelper.RunSync(() => vk.SendAllRequests(Requests));
                        if (res == 0) context.SaveChanges();
                        else context.UndoChanges();
                        Requests.Clear();
                        Thread.Sleep(500);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception esc)
                {
                    Logging.Logger.Error(esc);
                }
            }
        }
        private int Send(List<VKApiRequestModel> Requests, SenderContext context)
        {
            var res = AsyncHelper.RunSync(() => vk.SendAllRequests(Requests));
            if (res == 0) context.SaveChanges();
            else context.UndoChanges();
            Requests.Clear();
            return res;
        }
    }
    public static class Logging
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
