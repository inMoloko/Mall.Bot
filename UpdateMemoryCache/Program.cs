using System;
using System.ServiceProcess;
using System.Threading;

namespace UpdateMemoryCache
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
                (new ServiceByYan()).Start();
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
                new ServiceByYan()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
