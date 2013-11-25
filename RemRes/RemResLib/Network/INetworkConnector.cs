using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;

namespace RemResLib.Network
{
    public interface INetworkConnector
    {
        void Start();

        void Stop();

        event NetworkMessage MessageReceived;

        bool IsClientRegistered(Guid clientID); 

        bool SendMessage(RemResMessage message, Guid clientID);
    }
}
