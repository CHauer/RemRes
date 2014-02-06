using System;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using RemResLib.DataService;
using RemResLib.Execution;
using RemResLib.Network;
using RemResLib.Network.XML;
using RemResLib.Settings;
using RemResLib.Watch;

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
        /// The execution system
        /// </summary>
        private ExecutionSystem executionSystem;

        /// <summary>
        /// The watch system
        /// </summary>
        private WatchSystem watchSystem;

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
            starterThread = new Thread(InitStartService);
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
            Task.Run(new Action(StopSystems));
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

            StartSystems();
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

            //load instance of networkConnect system
            networkSystem = NetworkConnectSystem.GetInstance();
            
            //Get port from SettingsManager
            servicePort = LoadServicePort();

            //initialize the dataservice to save the notification enpoints
            networkSystem.NotificationDataService = new XmlNotificationDataService();

            //add network XML Connector or Listener
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
                return Convert.ToInt32(settingsManager.GetSettingValue("standardServiceListenPort").Value);
            }
            catch(Exception ex)
            {
                log.Debug("Error during the load process of the service port from settings. Standard Value used.", ex);

                //Standard Value
                return 45510;
            }
        }

        /// <summary>
        /// Initializes the execution system.
        /// </summary>
        private void InitExecutionSystem()
        {
            executionSystem = ExecutionSystem.GetInstance();
        }

        /// <summary>
        /// Initializes the watch system.
        /// </summary>
        private void InitWatchSystem()
        {
            //load instance of watchsystem
            watchSystem = WatchSystem.GetInstance();

            //initialize the configdataservice to save the config in xml format
            watchSystem.ConfigDataService = new XmlConfigDataService();
        }

        /// <summary>
        /// Initializes the system cooperation and connectors.
        /// </summary>
        private void InitSystemCooperation()
        {
            //inject watchSystem instance in execution system to provide
            //the execution system a class to handle the incoming messages 
            executionSystem.WatchSystem = watchSystem;

            //incoming messages from the network system got forwarded to the execution system
            networkSystem.MessageReceived += executionSystem.AddMessageForExecution;

            //forward response messages to the network system to send them to the clients
            executionSystem.ResponseNetworkMessageOccured += networkSystem.SendMessage;

            //forward notification messages to the network system to send them to all notif entpoints
            watchSystem.NotificationOccured += networkSystem.SendNotification;

            //forward new notification endpoints to the network system
            watchSystem.NotificationEndpointReceived += networkSystem.AddNotificationEndpoint;
        }

#endregion 

        #region Start Stop Systems

        /// <summary>
        /// Starts the systems.
        /// </summary>
        private void StartSystems()
        {
            //first because to initialize the save config rules
            watchSystem.StartWatchSystem();
            //second to be prepared for incoming messages 
            executionSystem.Start();
            //third service ready for incoming messages
            networkSystem.Start();
        }

        /// <summary>
        /// Stops the systems.
        /// </summary>
        private void StopSystems()
        {
            //first - do not handle incoming messages any longer
            networkSystem.Stop();
            //second - handle all current messages and stop
            executionSystem.Stop();
            //third - save config and stop
            watchSystem.EndWatchSystem();
        }

        #endregion

        #region Debug

#if DEBUG

        /// <summary>
        /// Starts the service in a "Debug" Mode in a simple 
        /// console mode window with status messages printed to Console.Out.
        /// </summary>
        public void DebugStart()
        {
            Console.WriteLine("[Press ENTER for END]");

            OnStart(null);

            Console.ReadLine();

            OnStop();
        }

#endif

        #endregion

    }
}
