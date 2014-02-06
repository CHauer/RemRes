using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;

namespace RemResLib.Watch.Contracts
{
    public interface IWatchNotificationHandler
    {
        /// <summary>
        /// Gets or sets the notification data service.
        /// </summary>
        /// <value>
        /// The notification data service.
        /// </value>
        INotificationDataService NotificationDataService { get; set; }
        
        /// <summary>
        /// Adds a new notification endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="port">The port.</param>
        void AddNotificationEndpoint(string endpoint, int port);

        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendNotification(RemResMessage message);
    }
}
