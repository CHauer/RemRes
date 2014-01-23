using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResLib.Network;

namespace RemResLib.DataService.Contracts
{
    public interface INotificationDataService
    {
        /// <summary>
        /// Loads the notification endpoints.
        /// </summary>
        /// <returns></returns>
        List<NotificationEndpoint> LoadNotificationEndpoints();

        /// <summary>
        /// Saves the notification endpoints.
        /// </summary>
        /// <param name="endpoints">The endpoints.</param>
        void SaveNotificationEndpoints(List<NotificationEndpoint> endpoints);
    }
}
