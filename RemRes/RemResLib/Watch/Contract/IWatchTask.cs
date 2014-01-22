using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;

namespace RemResLib.Watch.Contract
{
    interface IWatchTask
    {
        /// <summary>
        /// Starts the watch task.
        /// </summary>
        void StartWatchTask();

        /// <summary>
        /// Ends the watch task.
        /// </summary>
        void EndWatchTask();

        /// <summary>
        /// Occurs when the watch thread raises a notification.
        /// </summary>
        event NotificationMessage WatchNotificationOccured;

        /// <summary>
        /// Gets the rule.
        /// </summary>
        /// <value>
        /// The rule.
        /// </value>
        WatchRule Rule { get; }

    }
}
