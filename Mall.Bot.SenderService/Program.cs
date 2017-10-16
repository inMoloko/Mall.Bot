using System;
using System.ServiceProcess;
using System.Threading;

namespace Mall.Bot.SenderService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
#if DEBUG
                (new BotSendService()).Start();
                Thread.Sleep(Timeout.Infinite);
#else
                            //MessageBox.Show("Приложение должно быть установлено в виде службы Windows и не может быть запущено интерактивно.");
#endif
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new BotSendService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
