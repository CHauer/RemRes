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
using RemResLib.Watch.Contracts;

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
        /// Occurs when a new notification endpoint is received.
        /// </summary>
        private event NotificationEndpointMessage NotificationEndpointReceivedHandler;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog _log;

        /// <summary>
        /// The settings manager object
        /// </summary>
        private SettingsManager settingsManagerObj;

        /// <summary>
        /// The lock object for the save watch task config 
        /// </summary>
        private object lockSaveWatchTask;

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
            lockSaveWatchTask = new object();
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

        /// <summary>
        /// Occurs when a new notification endpoint is received.
        /// </summary>
        public event NotificationEndpointMessage NotificationEndpointReceived
        {
            add
            {
                NotificationEndpointReceivedHandler += value;
            }
            remove
            {
                NotificationEndpointReceivedHandler -= value;
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
            AddWatchRule convertedMessage;
            WatchTask tempTask;

            if (!(message is AddWatchRule))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            //convert message to goal type
            convertedMessage = message as AddWatchRule;

            //Validate the watchRule
            ValidateAddWatchRule(convertedMessage);

            //init the watch task
            CreateWatchTask(convertedMessage.WatchRule, true);

            return new OperationStatus()
            {
                Command = convertedMessage.GetType().Name,
                Status = StatusType.OK,
                Message = String.Format("The watchrule {0} has been sucessfully created and is now running.", convertedMessage.WatchRule.Name)
            };
        }

        /// <summary>
        /// Validates the add watch rule.
        /// </summary>
        /// <param name="convertedMessage">The converted message.</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        private void ValidateAddWatchRule(AddWatchRule convertedMessage)
        {
            if (convertedMessage.WatchRule == null)
            {
                throw new ArgumentNullException("message.AddWatchRule.WatchRule");
            }

            if (string.IsNullOrEmpty(convertedMessage.WatchRule.Name))
            {
                throw new ArgumentNullException("message.AddWatchRule.WatchRule.Name");
            }

            if (lstAktivWatchTasks.Any(t => t.Rule.Name.Equals(convertedMessage.WatchRule.Name)))
            {
                throw new ArgumentException(String.Format("The name for the watchRule {0} " +
                                    "ist already in use. Please use another name.",
                                    convertedMessage.WatchRule.Name));
            }

            if (convertedMessage.WatchRule.WatchField == null)
            {
                throw new ArgumentNullException("message.AddWatchRule.WatchRule.WatchField");
            }

            if (string.IsNullOrEmpty(convertedMessage.WatchRule.WatchField.WatchObject))
            {
                throw new ArgumentNullException("message.AddWatchRule.WatchRule.WatchField.WatchObject");
            }

            if (string.IsNullOrEmpty(convertedMessage.WatchRule.WatchField.WatchProperty))
            {
                throw new ArgumentNullException("message.AddWatchRule.WatchRule.WatchField.WatchProperty");
            }

            //NOTE no check if watchobject and watchproperty returns value or causes error at wmi
            //NOTE no check if given type is ok

            if (convertedMessage.WatchRule.Notify)
            {
                if (convertedMessage.WatchRule.WatchParameter == null)
                {
                    throw new ArgumentException(
                        "If the notification is activated for this rule the watch parameter has to specified.");
                }

                var para = convertedMessage.WatchRule.WatchParameter;

                foreach (var item in new List<WatchParameterDetail>() {para.Max, para.Min, para.Value})
                {
                    if (item != null)
                    {
                        if (item.WatchField == null && item.SingleValue == null)
                        {
                            throw new ArgumentException("If the WatchParameter section is specified, there has to " +
                                                        "be a watchField/singlevalue specified in the max/min/value section.");
                        }

                        if (item.SingleValue != null && item.SingleValue.Type != convertedMessage.WatchRule.WatchField.Type)
                        {
                            throw new ArgumentException(
                                "The notification can not work with two differnt types. Please use the " +
                                "same Types for WatchField and WatchParameter sections " +
                                "max/min/value fields or single values.");
                        }

                        if (item.WatchField != null)
                        {
                            if (string.IsNullOrEmpty(item.WatchField.WatchObject))
                            {
                                throw new ArgumentNullException("The WatchField specification is invalid in the " +
                                                                "watchparameter min/max/value section.Missing WatchObject.");
                            }

                            if (string.IsNullOrEmpty(item.WatchField.WatchProperty))
                            {
                                throw new ArgumentNullException("The WatchField specification is invalid in the " +
                                                                "watchparameter min/max/value section. Missing WatchProperty.");
                            }

                            if (item.WatchField.Type != convertedMessage.WatchRule.WatchField.Type)
                            {
                                throw new ArgumentException(
                                    "The notification can not work with two differnt types. Please use the " +
                                    "same Types for WatchField and WatchParameter sections " +
                                    "max/min/value fields or single values.");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the watch rule.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// The message type for this messagehandler method is invalid.
        /// or
        /// </exception>
        /// <exception cref="System.ArgumentNullException">message.DeleteWatchRule.Name</exception>
        [RemResMessageHandler(typeof(DeleteWatchRule))]
        public RemResMessage DeleteWatchRule(RemResMessage message)
        {
            DeleteWatchRule convertedMessage;
            WatchTask tempTask;

            if (!(message is DeleteWatchRule))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            //convert message to goal type
            convertedMessage = message as DeleteWatchRule;

            if (string.IsNullOrEmpty(convertedMessage.Name))
            {
                throw new ArgumentNullException("message.DeleteWatchRule.Name");
            }

            if (lstAktivWatchTasks.Any(t => t.Rule.Name.Equals(convertedMessage.Name)))
            {
                var item = lstAktivWatchTasks.FirstOrDefault(t => t.Rule.Name.Equals(convertedMessage.Name));

                if (item != null)
                {
                    item.EndWatchTask();

                    lstAktivWatchTasks.Remove(item);

                    SaveConfigRules();

                    return new OperationStatus()
                    {
                        Status = StatusType.OK,
                        Command = "DeleteWatchRule",
                        Message = String.Format("The watchrule {0} has been successfully deleted.", convertedMessage.Name)
                    };
                }

                throw new InvalidOperationException(String.Format("There is no activ watchrule {0} running " +
                                            "on this service", convertedMessage.Name));
            }
            else
            {
                throw new InvalidOperationException(String.Format("There is no activ watchrule {0} running "+
                                            "on this service", convertedMessage.Name));
            }
        }

        /// <summary>
        /// Clears the watch rules.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(ClearWatchRules))]
        public RemResMessage ClearWatchRules(RemResMessage message)
        {
            if (!(message is ClearWatchRules))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            foreach (var item in lstAktivWatchTasks)
            {
                item.EndWatchTask();
            }

            lstAktivWatchTasks.Clear();

            SaveConfigRules();

            return new OperationStatus()
            {
                Status = StatusType.OK,
                Command = "ClearWatchRules",
                Message = "The watchrule set has been successfully cleared."
            };
        }

        /// <summary>
        /// Gets the watch data.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(GetWatchData))]
        public RemResMessage GetWatchData(RemResMessage message)
        {
            GetWatchData convertedMessage;
            WatchDataSet resultSet = new WatchDataSet();
            IList<IWatchTask> tempTaskList = new List<IWatchTask>();

            if (!(message is GetWatchData))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            convertedMessage = (message as GetWatchData);

            if (string.IsNullOrEmpty(convertedMessage.Name) && convertedMessage.WatchField == null)
            {
                //return all data
                tempTaskList = lstAktivWatchTasks;
            }

            if (!string.IsNullOrEmpty(convertedMessage.Name))
            {
                //return data for watch rule name
                tempTaskList = lstAktivWatchTasks.Where(t => t.Rule.Name.Equals(convertedMessage.Name)).ToList();
            }

            if (convertedMessage.WatchField != null)
            {
                //return data for all rules with given field
                tempTaskList = lstAktivWatchTasks.Where(t => t.Rule.WatchField.WatchObject.Equals(convertedMessage.WatchField.WatchObject) &&
                                                             t.Rule.WatchField.WatchProperty.Equals(convertedMessage.WatchField.WatchProperty)).ToList();
            }

            foreach (var task in tempTaskList)
            {
                WatchField tempField = task.Rule.WatchField;

                tempField.WatchFieldValues = task.WatchData.ToList();

                resultSet.Add(tempField);
            }

            return new GetWatchDataResult()
            {
                WatchDataSet = resultSet
            };
        }

        /// <summary>
        /// Gets the watch rules.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(GetWatchRules))]
        public RemResMessage GetWatchRules(RemResMessage message)
        {
            if (!(message is GetWatchRules))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            //no possible in one line because of problem with convert from list<watchrule> to WatchRuleSet
            var ruleset = new WatchRuleSet();
            ruleset.AddRange(lstAktivWatchTasks.Select(t => t.Rule).ToList());

            return new GetWatchRuleResult()
            {
                WatchRuleSet = ruleset
            };
        }

        #endregion

        #region Process 

        /// <summary>
        /// Gets the process list.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(GetProcessList))]
        public RemResMessage GetProcessList(RemResMessage message)
        {
            ProcessSet resultList = new ProcessSet();
            GetProcessList convertedMessage;
            int minRamBytes;

            List<System.Diagnostics.Process> lstProcess;

            if (!(message is GetProcessList))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            convertedMessage = message as GetProcessList;

            minRamBytes = convertedMessage.RAMOver;

            if (minRamBytes == 0)
            {
                lstProcess = System.Diagnostics.Process.GetProcesses().ToList();
            }
            else
            {
                lstProcess = System.Diagnostics.Process.GetProcesses().Where(p => p.WorkingSet64 >= minRamBytes).ToList();
            }

            foreach (var proc in lstProcess)
            {
                try
                {
                    resultList.Add(new Process
                    {
                        PID = proc.Id,
                        ProcessName = proc.ProcessName,
                        ProcessTitle = proc.MainWindowTitle,
                        Responding = proc.Responding,
                        RAM = (int)(proc.WorkingSet64)
                    });
                }
                catch (Exception ex)
                {
                    _log.Debug("Error during accesing the proccess list - " + 
                                ex.Message);
                }
            }

            return new GetProcessListResult() { ProcessSet = resultList };
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
            try
            {
                return new GetSettingResult
                {
                    Settings = new RemResDataLib.BaseTypes.Settings(){
                    settingsManagerObj.GetSettingValue(key)
                }
                };
            }
            catch (Exception ex)
            {
                return new OperationStatus(){
                    Command = message.GetType().Name,
                    Message = ex.Message,
                    Status = StatusType.ERROR
                };
            }
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

        #region Notification

        /// <summary>
        /// Handles the new notification received.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The message type for this messagehandler method is invalid.</exception>
        [RemResMessageHandler(typeof(NotifyMe))]
        public RemResMessage HandleNewNotificationEndPointReceived(RemResMessage message)
        {
            if (!(message is NotifyMe))
            {
                throw new InvalidOperationException("The message type for this messagehandler method is invalid.");
            }

            OperationStatus errorStatus = new OperationStatus()
                {
                    Command = message.GetType().Name,
                    Status = StatusType.OK,
                    Message = "The enpoint is in the wrong format. Please use 'hostname:port'."
                };

            string endpoint = (message as NotifyMe).Endpoint;
            string hostname = null;
            int port = -1; 

            if (string.IsNullOrEmpty(endpoint))
            {
                return errorStatus; 
            }

            string[] endpointParts = endpoint.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (endpointParts.Length == 2)
            {
                hostname = endpointParts[0];
                try
                {
                    port = Convert.ToInt32(endpointParts[1]);
                }
                catch
                {
                    return errorStatus;
                }
            }
            else
            {
                return errorStatus;
            }

            //raise event enpoint recieved
            NotificationEndpointReceivedHandler(hostname, port);

            return new OperationStatus()
            {
                Command = message.GetType().Name,
                Status = StatusType.OK,
                Message = string.Format("The enpoint {0} was successfully added to the notification endpoints.", (message as NotifyMe).Endpoint)
            };
        }

        #endregion

        #endregion

        #region Watch System

        /// <summary>
        /// Starts the watch system.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The ConfigDataService has to be initialized.</exception>
        public void StartWatchSystem()
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
                CreateWatchTask(rule, false);              
            }
        }

        /// <summary>
        /// Ends the watch system.
        /// </summary>
        public void EndWatchSystem()
        {
            foreach (WatchTask task in lstAktivWatchTasks)
            {
                task.EndWatchTask();
            }

            SaveConfigRules();
        }

        /// <summary>
        /// Creates a new watch task.
        /// Add it to the current activ list.
        /// Starts the watch task.
        /// Saves all watch tasks to config file.
        /// </summary>
        /// <param name="rule">The rule.</param>
        private void CreateWatchTask(WatchRule rule, bool save)
        {
            //create new watchtask
            var task = new WatchTask(rule);

            //add watchtaskt to list
            lstAktivWatchTasks.Add(task);

            //handle created notifications and redirect
            task.WatchNotificationOccured += (message) => NotificationOccuredHandler(message);

            //start task
            task.StartWatchTask();

            if (save)
            {
                SaveConfigRules();
            }
        }

        /// <summary>
        /// Saves the configuration rules.
        /// </summary>
        private void SaveConfigRules()
        {
            lock (lockSaveWatchTask)
            {
                var rules = lstAktivWatchTasks.Select(t => t.Rule).ToList();
                var ruleset = new WatchRuleSet();

                foreach (var rule in rules)
                {
                    //dont save the collected values - only temporay watching
                    rule.WatchField.WatchFieldValues = null;
                    ruleset.Add(rule);
                }

                configDataServiceObj.SaveWatchRules(ruleset);

                //no use of direct cast due  problem with the convert from List<WatchRule> to WatchRuleSet
                //configDataServiceObj.SaveWatchRules(lstAktivWatchTasks.Select(t => t.Rule).ToList() as WatchRuleSet);
            }
        }

        #endregion

    }
}
