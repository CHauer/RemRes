using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;
using RemResDataLib.Messages;
using RemResLib.Watch.Contract;


namespace RemResLib.Watch
{
    public class WatchTask : IWatchTask
    {

        /// <summary>
        /// The activ watch rule
        /// </summary>
        private WatchRule activWatchRule;

        /// <summary>
        /// The watch task thread
        /// </summary>
        private Thread watchTaskThread;

        /// <summary>
        /// Indicates if the current Task runs or not.
        /// </summary>
        private bool runTask;

        /// <summary>
        /// Occurs when [watch notification occured handler].
        /// </summary>
        private event NotificationMessage WatchNotificationOccuredHandler;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The list of measured values
        /// </summary>
        private IList<WatchValue> lstMeasuredValues;

        /// <summary>
        /// The thread period
        /// </summary>
        private TimeSpan threadPeriod;

        /// <summary>
        /// The maximum logged values
        /// </summary>
        private int maxLoggedValues;

        /// <summary>
        /// A access lock object.
        /// </summary>
        private object lockObject;

        /// <summary>
        /// The last sent notification;
        /// </summary>
        private DateTime lastNotification;

        #region Constuctor

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchTask"/> class.
        /// </summary>
        /// <param name="watchRule">The watch rule.</param>
        public WatchTask(WatchRule watchRule)
        {
            if (watchRule == null)
            {
                throw new ArgumentNullException("watchRule");
            }

            activWatchRule = watchRule;

            InitializeObjects();
            InitLog();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitializeObjects()
        {
            watchTaskThread = new Thread(RunWatchTask);
            lstMeasuredValues = new List<WatchValue>();

            //use peroid defined for rule : otherwise standard 60 sek
            threadPeriod = new TimeSpan(0, 0, activWatchRule.Period >= 1 ? activWatchRule.Period : 60);

            //use cacheValues as maxLoggedvaues : otherwise standard 50 log watchvalues
            maxLoggedValues = activWatchRule.CacheValues >= 1 ? activWatchRule.CacheValues : 50;

            lockObject = new object();

            //init the value for "the last notifciaotn sent time" to some "random" value
            lastNotification = DateTime.Now.AddHours(-10);
        }

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region Event

        /// <summary>
        /// Occurs when the watch thread raises a notification.
        /// </summary>
        public event NotificationMessage WatchNotificationOccured
        {
            add
            {
                WatchNotificationOccuredHandler += value;
            }
            remove
            {
                WatchNotificationOccuredHandler -= value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current watched rule.
        /// </summary>
        /// <value>
        /// The rule.
        /// </value>
        public WatchRule Rule
        {
            get { return activWatchRule; }
        }

        #endregion

        #region Start Stop

        /// <summary>
        /// Starts the watch task.
        /// </summary>
        public void StartWatchTask()
        {
            runTask = true; 
        }
        
        /// <summary>
        /// Ends the watch task.
        /// </summary>
        public void EndWatchTask()
        {
            runTask = false;
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs the watch task.
        /// </summary>
        private void RunWatchTask()
        {
            WatchValue tempValue = null;
            object currentValue = null;

            while (runTask)
            {
                try
                {
                    currentValue = ReadValue(activWatchRule.WatchField).ToString(), 
                    //Watch Field read value
                    tempValue = new WatchValue
                    { 
                        Value = currentValue.ToString(),
                        MomentOfMeasure = DateTime.Now 
                    };

                    //lock this section
                    lock (lockObject)
                    {
                        //a user could not request alle logged values in this timespan
                        lstMeasuredValues.Add(tempValue);

                        if (lstMeasuredValues.Count > maxLoggedValues)
                        {
                            //delete oldest value
                            lstMeasuredValues.RemoveAt(0);
                        }
                    }
                }
                catch (InvalidOperationException ioex)
                {
                    log.Debug(String.Format("Error during the logging process of a Watch Value. Details: {0}", ioex.Message), ioex);
                    tempValue = null;
                }

                //Check for notification
                if (activWatchRule.Notifiy)
                {
                    //only if a value is available
                    if (tempValue != null)
                    {
                        ExecuteNotificationCheck(currentValue);
                    }
                }

                //Wait for next read
                Thread.Sleep(threadPeriod);
            }
        }

        #endregion

        #region Notification

        /// <summary>
        /// Executes the notification check.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        private void ExecuteNotificationCheck(object currentValue)
        {
            bool isNotify = false;
            Notification notifObj = null;


            //Check if notification is configures with watch parameter
            if (activWatchRule.WatchParameter != null)
            {
                var ruleType = activWatchRule.WatchField.Type;

                if (ruleType == WatchFieldType.DateTime || ruleType == WatchFieldType.Integer || 
                    ruleType == WatchFieldType.Double)
                {
                    WatchFieldType compareType = WatchFieldType.Integer;

                    #region Max

                    //check if currentValue is max
                    if (activWatchRule.WatchParameter.Max != null)
                    {
                        object compareValue;

                        //shorter
                        var max = activWatchRule.WatchParameter.Max;

                        //check if a max watchfield value is defined otherwise use single value
                        if (max.WatchField != null)
                        {
                            try
                            {
                                compareValue = ReadValue(max.WatchField);
                            }
                            catch (InvalidOperationException ioex)
                            {
                                log.Debug(String.Format("Error during the logging process of a Watch Value. Details: {0}", ioex.Message), ioex);
                                return;
                            }

                            compareType = max.WatchField.Type;
                        }
                        else if (max.SingleValue != null)
                        {
                            compareValue = max.SingleValue.Value;
                            compareType = max.SingleValue.Type;
                        }
                        else
                        {
                            return;
                        }

                        // currentValue >= compareValue -> NOTIFY
                        isNotify = Compare(currentValue, compareValue, compareType, CompareOperation.More);

                        if (isNotify)
                        {
                            //TODO Notification Message
                            notifObj = new Notification
                                        {
                                            LastValue = currentValue.ToString(),
                                            Message = "",
                                            Type = "MaxReached",
                                            WatchField = activWatchRule.WatchField,
                                            WatchRuleName = activWatchRule.Name
                                        };
                        }

                    }

                    #endregion

                    #region Min

                    //check if currentValue is min
                    if (activWatchRule.WatchParameter.Min != null)
                    {
                        object compareValue;

                        var min = activWatchRule.WatchParameter.Min;

                        if (min.WatchField != null)
                        {
                            try
                            {
                                compareValue = ReadValue(min.WatchField);
                            }
                            catch (InvalidOperationException ioex)
                            {
                                log.Debug(String.Format("Error during the logging process of a Watch Value. Details: {0}", ioex.Message), ioex);
                                return;
                            }

                            compareType = min.WatchField.Type;
                        }
                        else if (min.SingleValue != null)
                        {
                            compareValue = min.SingleValue.Value;
                            compareType = min.SingleValue.Type;
                        }
                        else
                        {
                            return;
                        }

                        // currentValue <= compareValue -> NOTIFIY
                        isNotify = Compare(currentValue, compareValue, compareType, CompareOperation.Less);

                        if (isNotify)
                        {
                            //TODO Notification Message
                            notifObj = new Notification
                            {
                                LastValue = currentValue.ToString(),
                                Message = "",
                                Type = "MinReached",
                                WatchField = activWatchRule.WatchField,
                                WatchRuleName = activWatchRule.Name
                            };
                        }
                    }

                    #endregion

                }
                else
                {
                    log.Debug("The Max and Min Compare function can only be used with numeric compareable datatypes.");
                }

                #region Value

                //check if currentValue is value
                if (activWatchRule.WatchParameter.Value != null)
                {
                    WatchFieldType compareType = WatchFieldType.String;
                    object compareValue;

                    var value = activWatchRule.WatchParameter.Value; 

                    //check if a watchfield value is defined otherwise use single value
                    if (value.WatchField != null)
                    {
                        try
                        {
                            compareValue = ReadValue(value.WatchField);
                        }
                        catch (InvalidOperationException ioex)
                        {
                            log.Debug(String.Format("Error during the logging process of a Watch Value. Details: {0}", ioex.Message), ioex);
                            return;
                        }

                        compareType = value.WatchField.Type;
                    }
                    else if (value.SingleValue != null)
                    {
                        compareValue = value.SingleValue.Value;
                        compareType = value.SingleValue.Type;
                    }
                    else
                    {
                        return;
                    }

                    // currentValue == compareValue -> NOTIFY
                    isNotify = Compare(currentValue, compareValue, compareType, CompareOperation.Equal);

                    if (isNotify)
                    {
                        //TODO Notification Message
                        notifObj = new Notification
                        {
                            LastValue = currentValue.ToString(),
                            Message = "",
                            Type = "ValueReached",
                            WatchField = activWatchRule.WatchField,
                            WatchRuleName = activWatchRule.Name
                        };
                    }
                }

                #endregion

                if(isNotify && lastNotification.AddMinutes(5) <= DateTime.Now)
                {
                    WatchNotificationOccuredHandler(notifObj);
                    lastNotification = DateTime.Now;
                }
            }
            else
            {
                log.DebugFormat("For the rule {0} the notification is activated but no WatchParamaters are configured.", activWatchRule.Name);
            }
        }

        /// <summary>
        /// The Compare Operations for the compare funtion more, less, equal
        /// </summary>
        private enum CompareOperation
        {
            /// <summary>
            /// The more
            /// </summary>
            More,
            /// <summary>
            /// The less
            /// </summary>
            Less,
            /// <summary>
            /// The equal
            /// </summary>
            Equal
        }

        /// <summary>
        /// Compares the specified current value.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="compareType">Type of the compare.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        private bool Compare(object currentValue, object compareValue, WatchFieldType compareType, CompareOperation operation)
        {
            switch (compareType)
            {
                case WatchFieldType.DateTime:

                    if (currentValue.GetType() == compareValue.GetType())
                    {
                        try
                        {
                            if (operation == CompareOperation.More)
                            {
                                return (DateTime)currentValue >= (DateTime)compareValue;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return (DateTime)currentValue <= (DateTime)compareValue;
                            }
                            else
                            {
                                return ((DateTime)currentValue).Equals((DateTime)compareValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Datetime compare.", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var d1 = Convert.ToDateTime(currentValue);
                            var d2 = Convert.ToDateTime(compareValue);

                            if (operation == CompareOperation.More)
                            {
                                return d1 >= d2;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return d1 <= d2;
                            }
                            else
                            {
                                return d1.Equals(d2);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Datetime compare.", ex);
                        }
                    }

                    break;
                case WatchFieldType.Double:

                    if (currentValue.GetType() == compareValue.GetType())
                    {
                        try
                        {
                            if (operation == CompareOperation.More)
                            {
                                return (Double)currentValue >= (Double)compareValue;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return (Double)currentValue <= (Double)compareValue;
                            }
                            else
                            {
                                return ((Double)currentValue).Equals((Double)compareValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Double compare.", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var d1 = Convert.ToDouble(currentValue);
                            var d2 = Convert.ToDouble(compareValue);

                            if (operation == CompareOperation.More)
                            {
                                return d1 >= d2;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return d1 <= d2;
                            }
                            else
                            {
                                return d1.Equals(d2);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Double compare.", ex);
                        }
                    }
                    break;
                case WatchFieldType.Integer:

                    if (currentValue.GetType() == compareValue.GetType())
                    {
                        try
                        {
                            if (operation == CompareOperation.More)
                            {
                                return (int)currentValue >= (int)compareValue;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return (int)currentValue <= (int)compareValue;
                            }
                            else
                            {
                                return ((Double)currentValue).Equals((Double)compareValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Integer compare.", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var i1 = Convert.ToInt32(currentValue);
                            var i2 = Convert.ToInt32(compareValue);

                            if (operation == CompareOperation.More)
                            {
                                return i1 >= i2;
                            }
                            else if (operation == CompareOperation.Less)
                            {
                                return i1 <= i2;
                            }
                            else
                            {
                                return i1.Equals(i2);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Error during Integer compare.", ex);
                        }
                    }
                    break;
                case WatchFieldType.Boolean:
                    return currentValue.Equals(compareValue);
                case WatchFieldType.String:
                    return currentValue.Equals(compareValue);
                default:
                    break;
            }

            return false;
        }

        #endregion

        #region Read Watch Value

        /// <summary>
        /// Reads the value definded by the watch Rule
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The WatchField Query returns more then one collection result. Please check the Where Clause</exception>
        private object ReadValue(WatchField field)
        {
            ManagementObjectSearcher mosObj;
            ManagementObjectCollection resultCollection;

            //Generate Object Searcher with build WQL query
            mosObj = new ManagementObjectSearcher(BuildQuery(field));
            
            //Exec Query for result collection
            resultCollection = mosObj.Get();

            //check if only one instance is in collection
            if (resultCollection.Count == 1)
            {
                var obj = (resultCollection.GetEnumerator().Current as ManagementObject);

                return ExtractValue(obj, activWatchRule.WatchField.WatchProperty);
            }
            else
            {
                //otherwise throw exception 
                throw new InvalidOperationException("The WatchField Query returns more then one "+
                        "collection result. Please check the Where Clause");
            }
        }

        /// <summary>
        /// Extracts the given property value of the management object.
        /// </summary>
        /// <param name="mo">The mo.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private object ExtractValue(ManagementObject mo, String propertyName)
        {
            PropertyData propertyData;
            
            try
            {
                propertyData = mo.Properties[propertyName];
            }
            catch (Exception ex)
            {
                log.Debug("Error during read of wmi property data." , ex);
                return null;
            }

            if (propertyData.Value != null && !string.IsNullOrEmpty(propertyData.Value.ToString()))
            {
                if (propertyData.IsArray)
                {
                    return String.Join(", ", propertyData.Value);
                }
                else
                {
                    return propertyData.Value;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Build Query

        /// <summary>
        /// Builds the query for the given Watch Field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private ObjectQuery BuildQuery(WatchField field)
        {
            if (string.IsNullOrEmpty(activWatchRule.WatchField.WhereClause))
            {
                return new ObjectQuery(
                     String.Format("Select {0} from {1}",
                         field.WatchProperty,
                         field.WatchObject));
            }
            else
            {
                return new ObjectQuery(
                   String.Format("Select {0} from {1} where {2}",
                        field.WatchProperty,
                        field.WatchObject,
                        field.WhereClause));
            }
        }

        #endregion

    }
}
