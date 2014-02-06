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
        /// Occurs when a notification message is received.
        /// </summary>
        private event NotificationMessage NotificationOccuredHandler;

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
        /// Initializes a new instance of the <see cref="NotificationEndpoint" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">port</exception>
        public NotificationEndpoint(int port)
        {
            if (port <= 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException("port");
            }

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

        /// <summary>
        /// Occurs when a notification message is received
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

        #region Start Stop 

        /// <summary>
        /// Starts the notification endpoint.
        /// </summary>
        public void StartNotificationEndpoint()
        {
            if (networkSystemRun)
            {
                return;
            }

            foreach (INotificationNetworkConnector connector in connectors)
            {
                connector.NotificationReceived += NotificationOccuredHandler;
                connector.Start();
            }
        }

        /// <summary>
        /// Stops the notification endpoint.
        /// </summary>
        public void StopNotificationEndpoint()
        {
            foreach (INotificationNetworkConnector connector in connectors)
            {
                connector.NotificationReceived -= NotificationOccuredHandler;
                connector.Stop();
            }

            networkSystemRun = false;
        }

        #endregion

        #region Manage Connectors

        /// <summary>
        /// Adds the network connector.
        /// If the notificationEndpoint is running - the conector ist started.
        /// </summary>
        /// <param name="connector">The connector.</param>
        public void AddNetworkConnector(INotificationNetworkConnector connector)
        {
            connector.Port = port;
            connectors.Add(connector);

            if (networkSystemRun)
            {
                connector.Start();
            }
        }

        /// <summary>
        /// Adds the network connector.
        /// If the notificationEndpoint is running - the conector ist started.
        /// </summary>
        /// <param name="rangeConnectors">The range of connectors.</param>
        public void AddNetworkConnector(IEnumerable<INotificationNetworkConnector> rangeConnectors)
        {
            foreach (var con in rangeConnectors)
            {
                con.Port = port;

                if (networkSystemRun)
                {
                    con.Start();
                }
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
