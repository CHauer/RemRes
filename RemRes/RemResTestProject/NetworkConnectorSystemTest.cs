using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemResDataLib.Messages;
using RemResLib.Network;
using RemResLib.Network.Fakes;

namespace RemResTestProject
{
    [TestClass]
    public class NetworkConnectorSystemTest
    {
        [TestMethod]
        public void AddNetworkConnector_StartUp_Test()
        {
            bool eventFired = false;
            Guid clientGuid = new Guid();
            
            //Arrange
            NetworkConnectSystem system = NetworkConnectSystem.GetInstance();
            var connector = new StubINetworkConnector();
            system.AddNetworkConnector(connector);
            system.MessageReceived += (message, clientID) => { eventFired = true; };

            //network system start
            system.Start();

            //receive message from network connector
            connector.MessageReceivedEvent(new AddWatchRule(), clientGuid);

            //Assert
            Assert.IsTrue(eventFired);

            system.Stop();
        }

        [TestMethod]
        public void AddNetworkConnector_SendMessage_Test()
        {
            bool eventFired = false;
            Guid clientGuid = new Guid();
            
            //Arrange
            NetworkConnectSystem system = NetworkConnectSystem.GetInstance();
            var connector = new StubINetworkConnector()
            {
                SendMessageRemResMessageGuid = (message, clientID) => { return true; },
                IsClientRegisteredGuid = (clientID) =>
                {
                    return clientGuid == clientID ? true : false;
                }
            };
            system.AddNetworkConnector(connector);
            system.MessageReceived += (message, clientID) => { eventFired = true; };

            //network system start
            system.Start();

            //receive message from network connector
            connector.MessageReceivedEvent(new AddWatchRule(), clientGuid);

            //Assert
            //Send message Back
            Assert.IsTrue(eventFired);
            Assert.IsTrue(system.SendMessage(new OperationStatus(), clientGuid));

            system.Stop();
        }
    }
}
