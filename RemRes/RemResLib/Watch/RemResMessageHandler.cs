using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResLib.Watch
{
    /// <summary>
    /// 
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method)]
    public class RemResMessageHandler : System.Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RemResMessageHandler"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        public RemResMessageHandler(Type messageType)
        {
            this.MessageType = messageType;
            this.OverrideExistingHandler = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemResMessageHandler" /> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="overrideExistingHandler">if set to <c>true</c> [override existing handler].</param>
        public RemResMessageHandler(Type messageType, bool overrideExistingHandler)
        {
            this.MessageType = messageType;
            this.OverrideExistingHandler = overrideExistingHandler;
        }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public Type MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [override existing handler].
        /// </summary>
        /// <value>
        /// <c>true</c> if [override existing handler]; otherwise, <c>false</c>.
        /// </value>
        public bool OverrideExistingHandler { get; set; }
    }
}
