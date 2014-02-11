using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;
using RemResLib.Network.Contracts;
using RemResLib.Settings;
using RemResLib.Watch.Contracts;

namespace RemResLib.Network
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="clientID">The client identifier.</param>
    public delegate void NetworkMessage(RemResMessage message, Guid clientID);

    public class NetworkConnectSystem : IWatchNotificationHandler
    {

        /// <summary>
        /// The notification data service object
        /// </summary>
        private INotificationDataService notificationDataServiceObj;

        /// <summary>
        /// The singelton instance object.
        /// </summary>
        private static NetworkConnectSystem ncsObj;

        /// <summary>
        /// The settings manager object
        /// </summary>
        private SettingsManager settingsManagerObj;

        /// <summary>
        /// The network system run flag indicates if the "network" system is running
        /// </summary>
        private bool networkSystemRun;

        /// <summary>
        /// The connectors with the implementation of the network access.
        /// </summary>
        private List<INetworkConnector> connectors;

        /// <summary>
        /// Occurs when [network message occured handler].
        /// </summary>
        private event NetworkMessage MessageReceivedHandler;

        /// <summary>
        /// The list of notification endpoints
        /// </summary>
        private List<NotificationEndpoint> lstNotificationEndpoints;

        /// <summary>
        /// The lock object for the notification endpoints list.
        /// </summary>
        private object lockEndpoints;

        #region Constrcutor 

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConnectSystem"/> class.
        /// </summary>
        private NetworkConnectSystem()
        {
            InitializeObjects();
            InitializeSettingsManager();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitializeObjects()
        {
            this.networkSystemRun = false;
            connectors = new List<INetworkConnector>();
            lstNotificationEndpoints = new List<NotificationEndpoint>();
            lockEndpoints = new object();
        }

        /// <summary>
        /// Initializes the settings manager.
        /// </summary>
        private void InitializeSettingsManager()
        {
            settingsManagerObj = SettingsManager.GetInstance();
        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static NetworkConnectSystem GetInstance()
        {
            if (ncsObj == null)
            {
                ncsObj = new NetworkConnectSystem();
            }

            return ncsObj;
        }

        #endregion

        #region Injection Properties

        /// <summary>
        /// Gets or sets the notification data service.
        /// </summary>
        /// <value>
        /// The notification data service.
        /// </value>
        public INotificationDataService NotificationDataService
        {
            get
            {
                return notificationDataServiceObj;
            }
            set
            {
                this.notificationDataServiceObj = value;
                LoadNotificationEndpoints();
            }
        }

        #endregion

        #region Manage Connectors

        /// <summary>
        /// Adds the network connector.
        /// </summary>
        /// <param name="connector">The connector.</param>
        public void AddNetworkConnector(INetworkConnector connector)
        {
            connectors.Add(connector);

            if (networkSystemRun)
            {
                connector.Start();
            }
        }

        /// <summary>
        /// Adds the network connector.
        /// </summary>
        /// <param name="addConnectors">The connectors.</param>
        public void AddNetworkConnector(IEnumerable<INetworkConnector> addConnectors)
        {
            if (addConnectors == null)
            {
                return;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            connectors.AddRange(addConnectors);

            if (networkSystemRun)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var con in addConnectors)
                {
                    con.Start();
                }
            }
        }

        /// <summary>
        /// Clears the network connectors.
        /// </summary>
        public void ClearNetworkConnectors()
        {
            foreach (INetworkConnector connector in connectors)
            {
                connector.Stop();
            }
            connectors.Clear();
        }

        #endregion

        #region Event

        /// <summary>
        /// Occurs when message over network input occured.
        /// </summary>
        public event NetworkMessage MessageReceived
        {
            add
            {
                MessageReceivedHandler += value;
            }
            remove
            {
                MessageReceivedHandler -= value;
            }
        }

        #endregion

        #region Start / Stop

        /// <summary>
        /// Starts this network system instance network connectors.
        /// </summary>
        public void Start()
        {
            if(networkSystemRun)
            {
                return;
            }

            foreach (INetworkConnector connector in connectors)
            {
                connector.MessageReceived += MessageReceivedHandler;
                connector.Start();
            }

        }

        /// <summary>
        /// Stops this network system instance network connectors.
        /// </summary>
        public void Stop()
        {
            foreach (INetworkConnector connector in connectors)
            {
                connector.MessageReceived -= MessageReceivedHandler;
                connector.Stop();
            }

            //End the Notification handling - clean and save endpoints
            EndNotificationHandling();

            networkSystemRun = false;
        }

        #endregion

        #region Send

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        public void SendMessage(RemResMessage message, Guid clientID)
        {
            foreach (INetworkConnector connector in connectors)
            {
                if (connector.IsClientRegistered(clientID))
                {
                    connector.SendMessage(message, clientID);
                    return;
                }
            }
        }

        #endregion

        #region Notification

        /// <summary>
        /// Loads the notification endpoints.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The Injection Property NotificationData Service has to be set.</exception>
        private void LoadNotificationEndpoints()
        {
            if (notificationDataServiceObj == null)
            {
                throw new InvalidOperationException("The Injection Property NotificationData Service has to be set.");
            }

            lstNotificationEndpoints = notificationDataServiceObj.LoadNotificationEndpoints();

            CleanUpNotificationEndpoints();
        }

        /// <summary>
        /// Adds the notification endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="port">The port.</param>
        public void AddNotificationEndpoint(string endpoint, int port)
        {
            lock (lockEndpoints)
            {
                //is the notification endpoint exists 
                if (lstNotificationEndpoints.Any(ne => ne.Endpoint.Equals(endpoint) && ne.Port.Equals(port)))
                {
                    //update the exisiting notification endpoint
                    var elem = lstNotificationEndpoints.First(ne => ne.Endpoint.Equals(endpoint) && ne.Port.Equals(port));

                    elem.FailedSendOperations = 0;
                    elem.DateReceived = DateTime.Now;
                }
                else
                {
                    //endpoint doesnt exist - add a new endpoint
                    lstNotificationEndpoints.Add(new NotificationEndpoint
                    {
                        FailedSendOperations = 0,
                        DateReceived = DateTime.Now,
                        Endpoint = endpoint,
                        Port = port
                    });
                }

                //Check for clean up notification enpoints
                CleanUpNotificationEndpoints();

                //Save Notification endpoints
                notificationDataServiceObj.SaveNotificationEndpoints(lstNotificationEndpoints);
            }
        }

        /// <summary>
        /// Cleans up notification endpoints.
        /// Deletes all Notification Endpoints the are older than "keepDays" Setting
        /// </summary>
        private void CleanUpNotificationEndpoints()
        {
            int keepDays = 0;
            int maxFailedSends = 0;

            try
            {
                keepDays = Convert.ToInt32(settingsManagerObj.GetSettingValue("notificationDuration").Value);
                maxFailedSends = Convert.ToInt32(settingsManagerObj.GetSettingValue("notificationMaxFailSendOperations").Value);
            }
            catch
            {
                keepDays = 5;
                maxFailedSends = 20;
            }

            //remove all items older than xx days 
            foreach (var item in lstNotificationEndpoints
                        .Where(e => (Math.Abs((e.DateReceived - DateTime.Now).Days) > keepDays) ||
                                    (e.FailedSendOperations >= maxFailedSends))
                        .ToList())
            {
                lstNotificationEndpoints.Remove(item);
            }
        }

        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendNotification(RemResMessage message)
        {
            //run through all endpoints and forward notification message
            lock (lockEndpoints)
            {
                foreach (var endpoint in lstNotificationEndpoints)
                {
                    foreach (var connector in connectors)
                    {
                        if (!connector.SendNotification(message, endpoint.Endpoint, endpoint.Port))
                        {
                            endpoint.FailedSendOperations++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ends the notification handling.
        /// </summary>
        private void EndNotificationHandling()
        {
            lock (lockEndpoints)
            {
                //Check for clean up notification enpoints
                CleanUpNotificationEndpoints();

                if (notificationDataServiceObj != null)
                {
                    //Save Notification endpoints
                    notificationDataServiceObj.SaveNotificationEndpoints(lstNotificationEndpoints);
                }
            }
        }

        #endregion

    }
}
