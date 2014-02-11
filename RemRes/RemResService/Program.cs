using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RemResService
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
#if !DEBUG
            ServiceBase[] servicesToRun =
            { 
                new RemResService() 
            };
            ServiceBase.Run(servicesToRun);

#else
            new RemResService().DebugStart();
#endif
        }
    }
}
