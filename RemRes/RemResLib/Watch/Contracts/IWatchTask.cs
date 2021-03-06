﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;

namespace RemResLib.Watch.Contracts
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
        /// Gets the watch data.
        /// </summary>
        /// <value>
        /// The watch data.
        /// </value>
        /// <returns></returns>
        IList<WatchValue> WatchData { get; }

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
