using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResDataLib.Messages;

namespace RemResLib.Network.XML
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceXmlListener : INetworkConnector
    {
        /// <summary>
        /// The run service
        /// </summary>
        private bool runService;

        /// <summary>
        /// The network connection thread
        /// </summary>
        private Thread networkConnectionThread;

        /// <summary>
        /// The network port
        /// </summary>
        private int serviceListenPort;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// Occurs when a message occured.
        /// </summary>
        private event NetworkMessage messageReceivedHandler;

        /// <summary>
        /// The TCP listener
        /// </summary>
        private TcpListener tcpListener;

        /// <summary>
        /// The current client sockets
        /// </summary>
        private Dictionary<Guid, Socket> currentClients;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceXmlListener"/> class.
        /// </summary>
        public ServiceXmlListener(int port)
        {
            InitLog();
            
            this.currentClients = new Dictionary<Guid, Socket>();
            this.runService = false;

            if (port <= 0 && port > 65535)
            {
                throw new ArgumentException("Port number has to be between 0 and 65535.", "port");
            }

            this.serviceListenPort = port;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// Inits the TCP listener.
        /// </summary>
        private void InitTcpListener()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, serviceListenPort);
                tcpListener.Start();

                log.Debug("The port listener was started successfully on port " + serviceListenPort + ".");
            }
            catch (Exception ex)
            {
                log.Error("The port listener could not be started. The service can not accept any messages!", ex);
                runService = false;
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Occurs when message over network input occured.
        /// </summary>
        public event NetworkMessage MessageReceived
        {
            add
            {
                messageReceivedHandler += value;
            }
            remove
            {
                messageReceivedHandler -= value;
            }
        }

        #endregion

        #region Start Stop

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (!runService)
            {
                runService = true;

                networkConnectionThread = new Thread(new ThreadStart(NetworkConnectionRun));

                networkConnectionThread.Start();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            runService = false;

            if (currentClients != null)
            {
                foreach(Socket cClient in currentClients.Values)
                {
                    cClient.Close(5);
                    cClient.Dispose();
                }
            }
        }

        #endregion

        #region Run

        /// <summary>
        /// Networks the connection run.
        /// </summary>
        private void NetworkConnectionRun()
        {
            InitTcpListener();

            while (runService)
            {
                try
                {
                    while (!tcpListener.Pending())
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error waiting for incoming connection requests.", ex);
                }

                try
                {
                    tcpListener.BeginAcceptSocket(new AsyncCallback(HandleDataConnection), null);
                }
                catch (Exception ex)
                {
                    log.Error("During creating the connection between client and service, an error has occurred!", ex);
                }
            }

            tcpListener.Stop();
        }

        #endregion

        #region Handle Connection

        /// <summary>
        /// Handles the data connection.
        /// </summary>
        /// <param name="result">The result.</param>
        private void HandleDataConnection(IAsyncResult result)
        {
            Socket client = null;
            NetworkStream networkStream;
            RemResMessage inputMessage = null;
            Guid clientKey = Guid.NewGuid();
            XmlSerializer xmlFormatter = new XmlSerializer(typeof(RemResMessage));

            try
            {
                client = tcpListener.EndAcceptSocket(result);

                currentClients.Add(clientKey, client);
            }
            catch(Exception ex)
            {
                log.Debug("Error during connection establishment to the client.", ex);
            }

            if (client != null)
            {
                try
                {
                    networkStream = new NetworkStream(client);
                }
                catch (Exception ex)
                {
                    try
                    {
                        log.Debug("Problem with data connection establishment to the client " + client.RemoteEndPoint + ".", ex);
                    }
                    catch (Exception exi)
                    {
                        log.Debug("Problem with data connection establishment to the client.", exi);
                    }
                    networkStream = null;
                }

                while (client.Connected)
                {
                    try
                    {
                        inputMessage = (RemResMessage)xmlFormatter.Deserialize(networkStream);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Problem with receiving the xml data message from the client.", ex);
                    }

                    if (inputMessage != null)
                    {
                        if (messageReceivedHandler != null)
                        {
                            messageReceivedHandler(inputMessage, clientKey);
                        }
                    }
                }
            }

            if (currentClients.ContainsKey(clientKey))
            {
                currentClients.Remove(clientKey);
            }

        }

        #endregion

        #region Send

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        public bool SendMessage(RemResMessage message, Guid clientID)
        {
            XmlSerializer xmlFormatter;
            NetworkStream networkStream;

            if (!currentClients.ContainsKey(clientID))
            {
                throw new InvalidOperationException("The given client is no longer available. Connection closed.");
            }

            xmlFormatter = new XmlSerializer(message.GetType());

            try
            {
                networkStream = new NetworkStream(currentClients[clientID]);
            }
            catch (Exception ex)
            {
                try
                {
                    log.Debug("Problem with data connection establishment to the client " + currentClients[clientID].RemoteEndPoint + ".", ex);
                }
                catch (Exception exi)
                {
                    log.Debug("Problem with data connection establishment to the client.", exi);
                }
                networkStream = null;
                return false;
            }

            try
            {
                xmlFormatter.Serialize(networkStream, message);
            }
            catch (Exception ex)
            {
                log.Debug("Problem with sending the xml data message from the client.", ex);
            }

            return true;
        }

        /// <summary>
        /// Send a notification message to the endpoint.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        public bool SendNotification(RemResMessage message, string endpoint, int port)
        {
            XmlSerializer xmlFormatter;
            NetworkStream networkStream;
            TcpClient client; 

            xmlFormatter = new XmlSerializer(message.GetType());

            try
            {
                client = new TcpClient(endpoint, port);
                networkStream = client.GetStream();
            }
            catch (Exception ex)
            {
                try
                {
                    log.Debug(String.Format("Problem with data connection establishment to the client {0}:{1}.",endpoint, port), ex);
                }
                catch (Exception exi)
                {
                    log.Debug("Problem with data connection establishment to the client.", exi);
                }

                networkStream = null;
                return false;
            }

            try
            {
                xmlFormatter.Serialize(networkStream, message);
            }
            catch (Exception ex)
            {
                log.Debug("Problem with sending the xml data message from the client.", ex);
            }

            return true;
        }

        #endregion

        #region Client Registered

        /// <summary>
        /// Determines whether [is client registered] [the specified client identifier].
        /// </summary>
        /// <param name="clientID">The client identifier.</param>
        /// <returns></returns>
        public bool IsClientRegistered(Guid clientID)
        {
            return currentClients.ContainsKey(clientID);
        }

        #endregion

    }

}
