using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;

namespace RemResTestClient.Logging
{

    public class MemoryAppenderWithEvents : MemoryAppender
    {
        public event Action<LoggingEvent> LogEventOccured;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            // Append the event as usual
            base.Append(loggingEvent);

            // Then alert the Updated event that an event has occurred
            var handler = LogEventOccured;
            if (handler != null)
            {
                handler(loggingEvent);
            }
        }
    }

}
