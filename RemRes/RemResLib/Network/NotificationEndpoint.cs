using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResLib.Network
{
    /// <summary>
    /// Contains the data for one notification endpoint.
    /// </summary>
    public class NotificationEndpoint
    {
        /// <summary>
        /// Gets or sets the date the notification endpoint was received.
        /// </summary>
        /// <value>
        /// The date received.
        /// </value>
        public DateTime DateReceived { get; set; }

        /// <summary>
        /// Gets or sets the endpoint address.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the failed send operations.
        /// </summary>
        /// <value>
        /// The failed send operations.
        /// </value>
        public int FailedSendOperations { get; set; }
    }
}
