using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RemResClientLib.Network.Connector.Contracts;
using RemResDataLib.Messages;

namespace RemResClientLib.Network.Connector
{
    public class RemResServiceConnector
    {
        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The list of connectors
        /// </summary>
        private Dictionary<Guid, IServiceConnector> lstConnectors;

        /// <summary>
        /// The rem resource service connector object
        /// </summary>
        private static RemResServiceConnector remResServiceConnectorObj;

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="RemResServiceConnector"/> class from being created.
        /// </summary>
        private RemResServiceConnector()
        {
            InitializeObjects();
            InitializeLog();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitializeObjects()
        {
            lstConnectors = new Dictionary<Guid, IServiceConnector>();
        }

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitializeLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static RemResServiceConnector GetInstance()
        {
            if (remResServiceConnectorObj == null)
            {
                remResServiceConnectorObj = new RemResServiceConnector();
            }

            return remResServiceConnectorObj;
        }

        #endregion

        #region Manage Connectors

        /// <summary>
        /// Adds the service connector.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="connector">The connector.</param>
        public void AddServiceConnector(Guid serviceID, IServiceConnector connector)
        {
            if (lstConnectors.ContainsKey(serviceID))
            {
                throw new InvalidOperationException("This service ID is already in use.");
            }

            lstConnectors.Add(serviceID, connector);
        }

        /// <summary>
        /// Clears the service connectors.
        /// </summary>
        public void ClearServiceConnectors()
        {
            lstConnectors.Clear();
        }

        #endregion

        #region Send Message

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public RemResMessage SendMessage(Guid serviceID, RemResMessage message)
        {
            return lstConnectors[serviceID].SendMessage(message);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string SendMessage(Guid serviceID, string message)
        {
            return lstConnectors[serviceID].SendMessage(message);
        }

        /// <summary>
        /// Sends the message asynchronous.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task<RemResMessage> SendMessageAsync(Guid serviceID, RemResMessage message)
        {
            return Task.Run<RemResMessage>(() => lstConnectors[serviceID].SendMessage(message));
        }

        /// <summary>
        /// Sends the message asynchronous.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task<string> SendMessageAsync(Guid serviceID, string message)
        {
            return Task.Run<string>(() => lstConnectors[serviceID].SendMessage(message));
        }

        #endregion 

    }
}
