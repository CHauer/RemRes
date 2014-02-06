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
using RemResClientLib.Network.Notification.Contracts;
using RemResDataLib.Messages;

namespace RemResClientLib.Network.Notification.XML
{
    public class XmlNotificationNetworkConnector : INotificationNetworkConnector
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
        private event NotificationMessage NotificationReceivedHandler;

        /// <summary>
        /// The TCP listener
        /// </summary>
        private TcpListener tcpListener;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNotificationNetworkConnector"/> class.
        /// </summary>
        /// <exception cref="System.ArgumentException">Port number has to be between 0 and 65535.;port</exception>
        public XmlNotificationNetworkConnector()
        {
            InitLog();
            
            this.runService = false;
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

        #region Properties

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        /// <exception cref="System.ArgumentException">Port number has to be between 0 and 65535.;Service Port</exception>
        public int Port
        {
            get
            {
                return serviceListenPort;
            }
            set
            {
                if (value <= 0 && value > 65535)
                {
                    throw new ArgumentException("Port number has to be between 0 and 65535.", "Service Port");
                }

                this.serviceListenPort = value;

            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Occurs when message over network input occured.
        /// </summary>
        public event NotificationMessage NotificationReceived
        {
            add
            {
                NotificationReceivedHandler += value;
            }
            remove
            {
                NotificationReceivedHandler -= value;
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

            //if (currentClients != null)
            //{
            //    foreach(Socket cClient in currentClients.Values)
            //    {
            //        cClient.Close(5);
            //        cClient.Dispose();
            //    }
            //}
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
                    tcpListener.BeginAcceptSocket(HandleDataConnection, null);
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
            RemResMessage inputMessage = null;
            XmlSerializer xmlFormatter = new XmlSerializer(typeof(RemResDataLib.Messages.Notification));
            byte[] buffer;

            try
            {
                client = tcpListener.EndAcceptSocket(result);
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

                if (client.Connected && networkStream != null)
                {
                    try
                    {
                        //direct deserializing from networkstream ends in endless loop
                        //because networkStream does not support seek/readtoend 
                        buffer = new byte[client.Available];
                        networkStream.Read(buffer, 0, client.Available);
                        memoryStream = new MemoryStream(buffer);

                        inputMessage = (RemResMessage)xmlFormatter.Deserialize(memoryStream);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Problem with receiving the notification xml data message from the client.", ex);
                    }

                    if (inputMessage != null)
                    {
                        if (NotificationReceivedHandler != null)
                        {
                            NotificationReceivedHandler(inputMessage);
                        }
                    }
                }

                //clean up 
                if (networkStream != null)
                {
                    networkStream.Close();
                    networkStream.Dispose();
                }

                client.Close();
                client.Dispose();
            }
        }

        #endregion

    }
}
