using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;

namespace RemResLib.Watch.Contract
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="notification">The notification.</param>
    public  delegate void NotificationMessage(Notification notification);

    public interface IWatchSystem
    {
        /// <summary>
        /// Gets or sets the configuration data service.
        /// </summary>
        /// <value>
        /// The configuration data service.
        /// </value>
        IConfigDataService ConfigDataService { get; set; }

        /// <summary>
        /// Occurs when [notification occured].
        /// </summary>
        event NotificationMessage NotificationOccured;
    }
}
