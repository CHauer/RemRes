using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResClientLib.Network.Connector.Contracts;
using RemResDataLib.Messages;

namespace RemResClientLib.Network.Connector.XML
{
    public class XmlServiceConnector : IServiceConnector
    {
        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The service identifier
        /// </summary>
        private Guid serviceID;

        /// <summary>
        /// The endpoint
        /// </summary>
        private string endpoint;

        /// <summary>
        /// The port
        /// </summary>
        private int port;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlServiceConnector"/> class.
        /// </summary>
        /// <param name="serviceID">The service identifier.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="port">The port.</param>
        public XmlServiceConnector(Guid serviceID, string endpoint, int port)
        {
            this.serviceID = serviceID;
            this.endpoint = endpoint;
            this.port = port;

            InitializeLog();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitializeLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        /// <value>
        /// The service identifier.
        /// </value>
        public Guid ServiceID
        {
            get { return serviceID; }
            set { serviceID = value; }
        }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public string Endpoint
        {
            get { return endpoint; }
            set { endpoint = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        #endregion

        #region Send Message

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public RemResMessage SendMessage(RemResMessage message)
        {
            XmlSerializer xmlFormatter;
            NetworkStream networkStream;
            MemoryStream memoryStream;
            StreamReader reader;
            TcpClient client;
            Byte[] buffer;

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
                    log.Debug(String.Format("Problem with data connection establishment" +
                                            " to the service {0}:{1}.", endpoint, port), ex);
                }
                catch (Exception exi)
                {
                    log.Debug("Problem with data connection establishment" +
                              " to the service.", exi);
                }
                networkStream = null;
                return null;
            }

            try
            {
                xmlFormatter.Serialize(networkStream, message);
            }
            catch (Exception ex)
            {
                log.Debug("Problem with sending the xml data message to the service.", ex);
            }

            try
            {
                buffer = new byte[client.Available];
                networkStream.Read(buffer, 0, client.Available);
                memoryStream = new MemoryStream(buffer);

                reader = new StreamReader(memoryStream);
                var messageStr = reader.ReadToEnd();
                xmlFormatter = GetXmlSerializer(messageStr);

                memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(messageStr));

                if (xmlFormatter != null)
                {
                    return (RemResMessage)xmlFormatter.Deserialize(memoryStream);
                }
                else
                {
                    log.Debug("Problem with receiving the xml data message from the client. Unknown message format!");
                }
            }
            catch (Exception ex)
            {
                log.Debug("Problem while receiving the the xml response data message from the service.", ex);
            }
            finally
            {
                networkStream.Close();
                networkStream.Dispose();
                client.Close();
            }

            return null;
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

            foreach (Type t in messageAssembly.GetTypes())
            {
                if (t.BaseType != null && t.BaseType == baseType)
                {
                    if (message.Contains(t.Name))
                    {
                        return new XmlSerializer(t);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string SendMessage(String message)
        {
            //XmlSerializer xmlFormatter;
            NetworkStream networkStream;
            TcpClient client;
            StreamWriter writer;
            StreamReader reader;
            MemoryStream memoryStream;
            Byte[] buffer;

            //xmlFormatter = new XmlSerializer(message.GetType());

            try
            {
                client = new TcpClient(endpoint, port);
                networkStream = client.GetStream();
                writer = new StreamWriter(networkStream);
            }
            catch (Exception ex)
            {
                try
                {
                    log.Debug(String.Format("Problem with data connection establishment" +
                                            " to the service {0}:{1}.", endpoint, port), ex);
                }
                catch (Exception exi)
                {
                    log.Debug("Problem with data connection establishment" +
                              " to the service.", exi);
                }
                networkStream = null;
                return null;
            }

            try
            {
                writer.Write(message);
                writer.Flush();
            }
            catch (Exception ex)
            {
                log.Debug("Problem with sending the xml data message to the service.", ex);
            }

            while (client.Connected && client.Available == 0)
            {
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
            }

            try
            {
                buffer = new byte[client.Available];
                networkStream.Read(buffer, 0, client.Available);
                memoryStream = new MemoryStream(buffer);

                reader = new StreamReader(memoryStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                log.Debug("Problem while receiving the the xml response data message from the service.", ex);
            }
            finally
            {
                networkStream.Close();
                networkStream.Dispose();
                writer.Dispose();
                client.Close();
            }

            return null;
        }

        #endregion

    }
}
