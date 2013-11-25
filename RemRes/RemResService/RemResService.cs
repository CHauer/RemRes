using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net.Core;
using RemResDataLib.Messages;
using RemResDataLib.BaseTypes;
using RemResLib.Network;
using RemResLib.Network.XML;

namespace RemResService
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RemResService : ServiceBase
    {
        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The starter thread
        /// </summary>
        private Thread starterThread;

        /// <summary>
        /// The network system
        /// </summary>
        private NetworkConnectSystem networkSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemResService"/> class.
        /// </summary>
        public RemResService()
        {
            InitializeComponent();
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            starterThread = new Thread(new ThreadStart(InitStartService));
        }

        #region Start Stop

        /// <summary>
        /// Wird bei der Implementierung in einer abgeleiteten Klasse ausgeführt,
        ///  wenn der Dienststeuerungs-Manager einen Befehl zum Starten an den Dienst
        ///  sendet oder wenn das Betriebssystem gestartet wird (diese gilt für Dienste,
        ///  die automatisch gestartet werden). Gibt Aktionen an, die beim Starten
        ///  des Diensts ausgeführt werden müssen.
        /// </summary>
        /// <param name="args">Vom Befehl zum Starten übergebene Daten.</param>
        protected override void OnStart(string[] args)
        {
            log.Debug("Service RemRes is starting.");

            try
            {
                starterThread.Start();
            }
            catch (Exception ex)
            {
                log.Debug("Error during the start process of the service", ex);
            }
            
        }

        /// <summary>
        /// Wird bei der Implementierung in einer abgeleiteten Klasse ausgeführt,
        ///  wenn der Dienststeuerungs-Manager einen Befehl zum Beenden an den
        ///  Dienst sendet. Gibt Aktionen an, die beim Beenden eines Diensts 
        /// auszuführen sind.
        /// </summary>
        protected override void OnStop()
        {
            
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the start service.
        /// </summary>
        private void InitStartService()
        {
            InitNetworkConnectSystem();
        }

        private void InitNetworkConnectSystem()
        {
            networkSystem = NetworkConnectSystem.GetInstance();
            networkSystem.AddNetworkConnector(new ServiceXmlListener(45510));
        }

        #endregion


#if DEBUG

        public void DebugStart()
        {
            OnStart(null);

            Console.ReadLine();
        }

#endif

    }
}
