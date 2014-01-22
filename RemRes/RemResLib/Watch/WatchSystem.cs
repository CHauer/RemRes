using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;
using RemResLib.Settings;
using RemResLib.Watch.Contract;

namespace RemResLib.Watch
{
    public class WatchSystem : IWatchSystem
    {
        /// <summary>
        /// The WatchSystem singelton instance object
        /// </summary>
        private static WatchSystem _watchSystemObj;

        /// <summary>
        /// The watch thread
        /// </summary>
        private Thread watchThread;

        /// <summary>
        /// The configuration data service object
        /// </summary>
        private IConfigDataService configDataServiceObj;

        /// <summary>
        /// The current list of watch rules
        /// </summary>
        private IList<IWatchTask> lstAktivWatchTasks;

        /// <summary>
        /// Occurs when a notification occured during watch process.
        /// </summary>
        private event NotificationMessage NotificationOccuredHandler;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog _log;

        /// <summary>
        /// The settings manager object
        /// </summary>
        private SettingsManager settingsManagerObj;

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="WatchSystem"/> class from being created.
        /// </summary>
        private WatchSystem()
        {
            InitializeWatchSystem();
        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static WatchSystem GetInstance()
        {
            if (_watchSystemObj == null)
            {
                _watchSystemObj = new WatchSystem();
            }

            return _watchSystemObj;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the watch system.
        /// </summary>
        private void InitializeWatchSystem()
        {
            InitObjects();
            InitLog();
            InitWatchThread(); 
        }

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitObjects()
        {
            lstAktivWatchTasks = new List<IWatchTask>();
            settingsManagerObj = SettingsManager.GetInstance();
        }

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitLog()
        {
            _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// Initializes the watch thread.
        /// </summary>
        private void InitWatchThread()
        {
            watchThread = new Thread(RunStartWatchSystem);
        }

        #endregion

        #region Injection Properties

        /// <summary>
        /// Gets or sets the configuration data service.
        /// </summary>
        /// <value>
        /// The configuration data service.
        /// </value>
        public IConfigDataService ConfigDataService
        {
            get
            {
                return this.configDataServiceObj;
            }
            set
            {
                this.configDataServiceObj = value;
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Occurs when a notification occured during watch process.
        /// </summary>
        public event NotificationMessage NotificationOccured
        {
            add
            {
                NotificationOccuredHandler += value;
            }
            remove
            {
                NotificationOccuredHandler -= value;
            }
        }

        #endregion

        #region Message Handler Methods

        #region Watch System

        /// <summary>
        /// Adds a new watch rule.
        /// </summary>
        /// <returns></returns>
        [RemResMessageHandler(typeof(AddWatchRule))]
        public RemResMessage AddWatchRule(RemResMessage message)
        {
            return null;
        }

        [RemResMessageHandler(typeof(DeleteWatchRule))]
        public RemResMessage DeleteWatchRule(RemResMessage message)
        {
            return null;
        }

        [RemResMessageHandler(typeof(ClearWatchRules))]
        public RemResMessage ClearWatchRules(RemResMessage message)
        {
            return null;
        }

        [RemResMessageHandler(typeof(GetWatchData))]
        public RemResMessage GetWatchData(RemResMessage message)
        {
            return null;
        }

        [RemResMessageHandler(typeof(GetWatchRules))]
        public RemResMessage GetWatchRules(RemResMessage message)
        {
            return null;
        }

        #endregion

        #region Process 

        [RemResMessageHandler(typeof(GetProcessList))]
        public RemResMessage GetProcessList(RemResMessage message)
        {
            return null;
        }

        #endregion

        #region Setting

        /// <summary>
        /// Return the settings value with the give key.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(GetSetting))]
        public RemResMessage GetSetting(RemResMessage message)
        {
            if(!(message is GetSetting))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            var key = ((GetSetting)message).Key;

            return new GetSettingResult
            {
                Settings = new RemResDataLib.BaseTypes.Settings(){
                    settingsManagerObj.GetSettingValue(key)
                }
            };
        }

        /// <summary>
        /// Sets the setting.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
         [RemResMessageHandler(typeof(SetSetting))]
        public RemResMessage SetSetting(RemResMessage message)
        {
            if (!(message is SetSetting))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            var key = ((SetSetting)message).Key;
            var value = ((SetSetting)message).Value;

            if (string.IsNullOrEmpty(key))
            {
                return new OperationStatus()
                {
                    Command = "SetSetting",
                    Message = "The key value has to be specified.",
                    Status = StatusType.INVALIDINPUT
                };
            }

            var status = settingsManagerObj.SetSettingValue(key, value);

            if (status)
            {
                return new OperationStatus()
                {
                    Command = "SetSetting",
                    Message = "The settings value was set.",
                    Status = StatusType.OK
                };
            }
            else
            {
                return new OperationStatus()
                {
                    Command = "SetSetting",
                    Message = "The settings value can't be set.",
                    Status = StatusType.ERROR
                };
            }
        }

        #endregion

        #region Keep Alive

         /// <summary>
         /// Gets the keep alive response.
         /// </summary>
         /// <param name="message">The message.</param>
         /// <returns></returns>
         /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(KeepAliveRequest))]
        public RemResMessage GetKeepAliveResponse(RemResMessage message)
        {
            if (!(message is KeepAliveRequest))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            return new KeepAliveResponse();
        }

        #endregion

        #endregion

        #region Watch System

        /// <summary>
        /// Starts the watch system.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The ConfigDataService has to be initialized.</exception>
        private void StartWatchSystem()
        {
            if (configDataServiceObj == null)
            {
                throw new InvalidOperationException("The ConfigDataService has to be initialized.");
            }

            try
            {
                watchThread.Start();
            }
            catch (Exception ex)
            {
                _log.Debug("Error during the start process of the Watch System.", ex);
            }
        }

        /// <summary>
        /// Runs the start watch system.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The ConfigDataService has to be initialized.</exception>
        private void RunStartWatchSystem()
        {
            IList<WatchRule> rules;

            if (configDataServiceObj == null)
            {
                throw new InvalidOperationException("The ConfigDataService has to be initialized.");
            }

            //load the save watch rules
            rules = configDataServiceObj.LoadWatchRules();

            foreach (WatchRule rule in rules)
            {
                //create new watchtask
                var task = new WatchTask(rule);

                //add watchtaskt to list
                lstAktivWatchTasks.Add(task);

                //handle created notifications and redirect
                task.WatchNotificationOccured += (message) => NotificationOccuredHandler(message);

                //start task
                task.StartWatchTask();                
            }
        }

        /// <summary>
        /// Ends the watch system.
        /// </summary>
        private void EndWatchSystem()
        {
            foreach (WatchTask task in lstAktivWatchTasks)
            {
                task.EndWatchTask();
            }

            configDataServiceObj.SaveWatchRules(lstAktivWatchTasks.Select(t => t.Rule).ToList());
        }

        #endregion

    }
}
