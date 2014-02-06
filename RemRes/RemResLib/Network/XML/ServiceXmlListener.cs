using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResDataLib.Messages;
using RemResLib.Network.Contracts;

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
            MemoryStream memoryStream;
            StreamReader reader;
            RemResMessage inputMessage = null;
            Guid clientKey = Guid.NewGuid();
            XmlSerializer xmlFormatter;
            byte[] buffer;

            OperationStatus osInvalidInput = new OperationStatus()
                                                {
                                                    Command = "Unknown",
                                                    Message = "Invalid Input Data - The input " +
                                                                "is not a valid RemRes Message or Command.",
                                                    Status = StatusType.INVALIDINPUT
                                                };

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
                    memoryStream = null;
                }

                while (client.Connected && networkStream != null)
                {

                    try
                    {
                        //direct deserializing from networkstream ends in endless loop
                        //because networkStream does not support seek/readtoend 

                        //read data in buffer
                        buffer = new byte[client.Available];
                        networkStream.Read(buffer, 0, client.Available);
                        memoryStream = new MemoryStream(buffer);

                        reader = new StreamReader(memoryStream);
                        var message = reader.ReadToEnd();

                        //get the right formatter for the incomming message
                        xmlFormatter = GetXmlSerializer(message);

                        memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(message));

                        if (xmlFormatter != null)
                        {
                            inputMessage = (RemResMessage)xmlFormatter.Deserialize(memoryStream);
                        }
                        else
                        {
                            log.Debug("Problem with receiving the xml data message from the client. Unknown message format!");

                            //Send back an error operation status
                            SendMessage(osInvalidInput, clientKey);

                            //end connection
                            client.Disconnect(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Problem with receiving the xml data message from the client.", ex);

                        //Send back an error operation status
                        SendMessage(osInvalidInput, clientKey);

                        //end connection
                        client.Disconnect(false);
                    }

                    if (inputMessage != null)
                    {
                        if (messageReceivedHandler != null)
                        {
                            messageReceivedHandler(inputMessage, clientKey);
                        }
                    }

                    //while no data on network stream available and connection not closed
                    while (client.Available == 0 && client.Connected)
                    {
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
                    }
                }

                if (networkStream != null)
                {
                    networkStream.Close();
                    networkStream.Dispose();
                }

                client.Close();
                client.Dispose(); 
            }

            if (currentClients.ContainsKey(clientKey))
            {
                currentClients.Remove(clientKey);
            }

        }

        /// <summary>
        /// Gets the XML serializer.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private XmlSerializer GetXmlSerializer(string message)
        {
            Type baseType = typeof(RemResMessage);
            Assembly messageAssembly = Assembly.GetAssembly(baseType);

            foreach(Type t in messageAssembly.GetTypes())
            {
                if (t.BaseType != null && t.BaseType == baseType)
                {
                    if(message.Contains(t.Name))
                    {
                        return new XmlSerializer(t);
                    }
                }
            }
            return null;
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
            finally
            {
                //Clean up 
                networkStream.Close();
                networkStream.Dispose();

                //end connection to client - response was send - clean up in incomming data handler
                if (currentClients.ContainsKey(clientID))
                {
                    currentClients[clientID].Disconnect(false);
                }
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
                return false;
            }
            finally
            {
                //clean up

                if (networkStream != null)
                {
                    networkStream.Close();
                    networkStream.Dispose();
                }

                client.Close();
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
