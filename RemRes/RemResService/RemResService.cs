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
using RemResLib.DataService;
using RemResLib.Network;
using RemResLib.Network.XML;
using RemResLib.Settings;

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
        /// The settings manager
        /// </summary>
        private SettingsManager settingsManager;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemResService"/> class.
        /// </summary>
        public RemResService()
        {
            InitializeComponent();

            InitLog();
            starterThread = new Thread(new ThreadStart(InitStartService));
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region Start Stop Service

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
            log.Info("Service RemRes is starting.");

            try
            {
                starterThread.Start();
            }
            catch (Exception ex)
            {
                //Error - if this thread doesnt start - the service won't start
                log.Error("Error during the start process of the service", ex);
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
            InitSettingsManager();
            InitNetworkConnectSystem();
            InitExecutionSystem();
            InitWatchSystem();
            InitSystemCooperation();
        }

        /// <summary>
        /// Initializes the settings manager.
        /// </summary>
        private void InitSettingsManager()
        {
            settingsManager = SettingsManager.GetInstance();

            settingsManager.SettingsDataService = new AppConfigSettingsDataService();
        }

        /// <summary>
        /// Initializes the network connect system.
        /// </summary>
        private void InitNetworkConnectSystem()
        {
            int servicePort = -1;

            networkSystem = NetworkConnectSystem.GetInstance();
            
            //Get port from SettingsManager
            servicePort = LoadServicePort();

            networkSystem.AddNetworkConnector(new ServiceXmlListener(servicePort));
        }

        /// <summary>
        /// Loads the service port from settings.
        /// </summary>
        /// <returns></returns>
        private int LoadServicePort()
        {
            try
            {
                return Convert.ToInt32(settingsManager.GetSettingValue("standardServiceListenPort"));
            }
            catch(Exception ex)
            {
                log.Debug("Error during the load process of the service port from settings. Standard Value used.", ex);

                //Standard Value
                return 45510;
            }
        }

        private void InitExecutionSystem()
        {
        }

        private void InitWatchSystem()
        {
        }

        private void InitSystemCooperation()
        {
        }

        #endregion


#if DEBUG

        /// <summary>
        /// Starts the service in a "Debug" Mode in a simple 
        /// console mode window with status messages printed to Console.Out.
        /// </summary>
        public void DebugStart()
        {
            OnStart(null);

            Console.ReadLine();
        }

#endif

    }
}
