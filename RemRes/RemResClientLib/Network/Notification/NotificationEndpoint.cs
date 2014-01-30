using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemResClientLib.Network.Notification.Contracts;

namespace RemResClientLib.Network.Notification
{
    public class NotificationEndpoint
    {
        /// <summary>
        /// Occurs when [notification occured handler].
        /// </summary>
        private event NotificationMessage notificationOccuredHandler;

        /// <summary>
        /// The connectors
        /// </summary>
        private List<INotificationNetworkConnector> connectors;

        /// <summary>
        /// The network system run flag indicates if the "network" system is running
        /// </summary>
        private bool networkSystemRun;

        /// <summary>
        /// The port
        /// </summary>
        private int port;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationEndpoint"/> class.
        /// </summary>
        public NotificationEndpoint(int port)
        {
            this.port = port;
            InitializeObjects();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitializeObjects()
        {
            this.networkSystemRun = false;
            connectors = new List<INotificationNetworkConnector>();
        }

        #endregion

        #region Events

        public event NotificationMessage NotificationOccured
        {
            add
            {
                notificationOccuredHandler += value;
            }
            remove
            {
                notificationOccuredHandler -= value;
            }
        }

        #endregion

        #region Start Stop 

        public void StartNotificationEndpoint()
        {
            if (networkSystemRun)
            {
                return;
            }

            foreach (INotificationNetworkConnector connector in connectors)
            {
                connector.NotificationReceived += notificationOccuredHandler;
                connector.Start();
            }
        }

        public void StopNotificationEndpoint()
        {
            foreach (INotificationNetworkConnector connector in connectors)
            {
                connector.NotificationReceived -= notificationOccuredHandler;
                connector.Stop();
            }

            networkSystemRun = false;
        }

        #endregion

        #region Manage Connectors

        /// <summary>
        /// Adds the network connector.
        /// </summary>
        /// <param name="connector">The connector.</param>
        public void AddNetworkConnector(INotificationNetworkConnector connector)
        {
            connector.Port = port;
            connectors.Add(connector);
        }

        /// <summary>
        /// Adds the network connector.
        /// </summary>
        /// <param name="rangeConnectors">The range of connectors.</param>
        public void AddNetworkConnector(IEnumerable<INotificationNetworkConnector> rangeConnectors)
        {
            foreach (var con in rangeConnectors)
            {
                con.Port = port;
            }

            this.connectors.AddRange(rangeConnectors);
        }

        /// <summary>
        /// Clears the network connectors.
        /// </summary>
        public void ClearNetworkConnectors()
        {
            foreach (INotificationNetworkConnector connector in connectors)
            {
                connector.Stop();
            }
            connectors.Clear();
        }

        #endregion

    }
}
