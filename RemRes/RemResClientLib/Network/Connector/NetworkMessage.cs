using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemResDataLib.Messages;

namespace RemResClientLib.Network.Connector
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceID">The service identifier.</param>
    /// <param name="message">The message.</param>
    public delegate void NetworkMessage(Guid serviceID, RemResMessage message);
}
