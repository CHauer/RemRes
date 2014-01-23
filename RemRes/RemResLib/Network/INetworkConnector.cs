using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;

namespace RemResLib.Network
{
    /// <summary>
    /// The INetwornConnector INterfaces describes which functions a Network Connector 
    /// has to offer to run in the Network Connect System.
    /// </summary>
    public interface INetworkConnector
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Occurs when message received.
        /// </summary>
        event NetworkMessage MessageReceived;

        /// <summary>
        /// Determines if a connection for a client is registered or available.
        /// </summary>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        bool IsClientRegistered(Guid clientID);

        /// <summary>
        /// Sends the given message to given client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        bool SendMessage(RemResMessage message, Guid clientID);

        /// <summary>
        /// Send a notification message to the endpoint.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        bool SendNotification(RemResMessage message, string endpoint, int port);
    }
}
