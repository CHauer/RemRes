using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;

namespace RemResLib.Network
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="clientID">The client identifier.</param>
    public delegate void NetworkMessage(RemResMessage message, Guid clientID);

    public class NetworkConnectSystem
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
        /// The network system run
        /// </summary>
        private bool networkSystemRun;

        /// <summary>
        /// The connectors
        /// </summary>
        private List<INetworkConnector> connectors;

        /// <summary>
        /// Occurs when [network message occured handler].
        /// </summary>
        private event NetworkMessage messageReceivedHandler;

        #region Constrcutor 

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConnectSystem"/> class.
        /// </summary>
        private NetworkConnectSystem()
        {
            this.networkSystemRun = false;
            connectors = new List<INetworkConnector>();
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
        }

        /// <summary>
        /// Adds the network connector.
        /// </summary>
        /// <param name="connectors">The connectors.</param>
        public void AddNetworkConnector(IEnumerable<INetworkConnector> connectors)
        {
            this.connectors.AddRange(connectors);
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
                messageReceivedHandler += value;
            }
            remove
            {
                messageReceivedHandler -= value;
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
                connector.MessageReceived += messageReceivedHandler;
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
                connector.MessageReceived -= messageReceivedHandler;
                connector.Stop();
            }
        }

        #endregion

        #region Send

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        public bool SendMessage(RemResMessage message, Guid clientID)
        {
            foreach (INetworkConnector connector in connectors)
            {
                if (connector.IsClientRegistered(clientID))
                {
                    return connector.SendMessage(message, clientID);
                }
            }

            return false;
        }

        #endregion

        #region Notification


        #endregion

    }
}
