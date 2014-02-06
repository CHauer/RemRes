using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using log4net;
using log4net.Appender;
using Mutzl.MvvmLight;
using RemResClientLib.Network.Connector;
using RemResClientLib.Network.Connector.XML;
using RemResClientLib.Network.Notification;
using RemResClientLib.Network.Notification.XML;
using RemResDataLib.Messages;
using RemResTestClient.Logging;
using RemResTestClient.Properties;

namespace RemResTestClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        /// <summary>
        /// The message types
        /// </summary>
        private ObservableCollection<MessageType> messageTypes;

        /// <summary>
        /// The current message type
        /// </summary>
        private MessageType currentMessageType;

        /// <summary>
        /// The command message send
        /// </summary>
        private DependentRelayCommand cmdMessageSend;

        /// <summary>
        /// The client address
        /// </summary>
        private string clientAddress;

        /// <summary>
        /// The user message input
        /// </summary>
        private string userMessageInput;
        
        /// <summary>
        /// The response message
        /// </summary>
        private string responseMessage;

        /// <summary>
        /// The error message
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// The notification enpoint
        /// </summary>
        private int notificationEnpointPort;

        /// <summary>
        /// The service connector
        /// </summary>
        private RemResServiceConnector serviceConnector;

        /// <summary>
        /// The notification endpoint
        /// </summary>
        private NotificationEndpoint notificationEndpoint;

        /// <summary>
        /// The notification received handler
        /// </summary>
        private event NotificationMessage NotificationReceivedHandler;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The service identifier
        /// </summary>
        private Guid serviceID;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            InitializeLog();
            InitializeMessageTypes();
            InitializeCommands();
            InitializeServiceConnect();
            InitializeNotification();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitializeLog()
        {
            log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            var memoryAppender = (MemoryAppenderWithEvents)LogManager.GetRepository().GetAppenders()
                .FirstOrDefault(a => a.Name.Equals("MemoryAppender"));

           if (memoryAppender != null)
           {
               memoryAppender.LogEventOccured += MemoryAppender_LogEventOccured;
           }
        }

        private void MemoryAppender_LogEventOccured(log4net.Core.LoggingEvent obj)
        {
            this.ErrorMessage = obj.RenderedMessage;
        }

        /// <summary>
        /// Initializes the message types.
        /// </summary>
        private void InitializeMessageTypes()
        {
            Type baseType = typeof(RemResMessage);
            Assembly messageAssembly = Assembly.GetAssembly(baseType);

            messageTypes = new ObservableCollection<MessageType>();

            foreach(Type t in messageAssembly.GetTypes())
            {
                if (t.BaseType != null && t.BaseType == baseType)
                {
                    if (!String.IsNullOrEmpty(Resources.ResourceManager.GetString(t.Name)))
                    {
                        messageTypes.Add(new MessageType()
                        {
                            MessageClassType = t,
                            MessageName = t.Name,
                            MessageTemplate = Resources.ResourceManager.GetString(t.Name)
                        });
                    }
                }
            }

            RaisePropertyChanged((() => MessageTypes));
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            cmdMessageSend = new DependentRelayCommand(SendMessage, CanSendMessage,
                this, () => UserMessageInput, () => ClientAddress);
        }

        /// <summary>
        /// Initializes the service connect.
        /// </summary>
        private void InitializeServiceConnect()
        {
            serviceConnector = RemResServiceConnector.GetInstance();
        }

        /// <summary>
        /// Initializes the notification.
        /// </summary>
        private void InitializeNotification()
        {
            NotificationPort = 45520;

            //initilize the notification nedpoint
            notificationEndpoint = new NotificationEndpoint(NotificationPort);

            //add the right connector to the endpoint to enable xml communication
            notificationEndpoint.AddNetworkConnector(new XmlNotificationNetworkConnector());

            //forward the notification event
            notificationEndpoint.NotificationOccured += (message) => NotificationReceivedHandler(message);

            //start the endpoint - listen to the port
            notificationEndpoint.StartNotificationEndpoint();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the client address.
        /// </summary>
        /// <value>
        /// The client address.
        /// </value>
        public string ClientAddress
        {
            get
            {
                return clientAddress;
            }
            set
            {
                clientAddress = value;

                serviceConnector.ClearServiceConnectors();
                RaisePropertyChanged(() => ClientAddress);

                if (!string.IsNullOrEmpty(clientAddress))
                {
                    string[] parts = clientAddress.Split(new[] { ':' });

                    int port = -1;

                    try
                    {
                        port = Convert.ToInt32(parts[1]);
                    }
                    catch
                    {
                        port = 45510;
                    }

                    serviceID = Guid.NewGuid();

                    serviceConnector.AddServiceConnector(serviceID, new XmlServiceConnector(serviceID, parts[0], port));

                }
            }
        }

        /// <summary>
        /// Gets or sets the message types.
        /// </summary>
        /// <value>
        /// The message types.
        /// </value>
        public ObservableCollection<MessageType> MessageTypes
        {
            get
            {
                return messageTypes;
            }
            set
            {
                messageTypes = value;
                RaisePropertyChanged(() => MessageTypes);
            }
        }

        /// <summary>
        /// Gets or sets the type of the selected message.
        /// </summary>
        /// <value>
        /// The type of the selected message.
        /// </value>
        public MessageType SelectedMessageType
        {
            get
            {
                return currentMessageType;
            }
            set
            {
                this.currentMessageType = value;
                this.UserMessageInput = currentMessageType.MessageTemplate;
                RaisePropertyChanged(() => SelectedMessageType);
            }
        }

        /// <summary>
        /// Gets or sets the user message input.
        /// </summary>
        /// <value>
        /// The user message input.
        /// </value>
        public string UserMessageInput
        {
            get
            {
                return userMessageInput;
            }
            set
            {
                this.userMessageInput = value;
                RaisePropertyChanged(() => UserMessageInput);
            }
        }

        /// <summary>
        /// The response message
        /// </summary>
        public string ResponseMessage
        {
            get
            { 
                return responseMessage; 
            }
            set
            {
                responseMessage = value;
                RaisePropertyChanged(() => ResponseMessage);
            }
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }

        /// <summary>
        /// Gets the notification port.
        /// </summary>
        /// <value>
        /// The notification port.
        /// </value>
        public int NotificationPort
        {
            get
            {
                return notificationEnpointPort;
            }
            set
            {
                this.notificationEnpointPort = value;
                RaisePropertyChanged(() => NotificationPort);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the message send.
        /// </summary>
        /// <value>
        /// The message send.
        /// </value>
        public DependentRelayCommand MessageSend
        {
            get
            {
                return cmdMessageSend;
            }
            set
            {
                cmdMessageSend = value;
                RaisePropertyChanged(() => MessageSend);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [notification received].
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

        #region Send Message

        /// <summary>
        /// Sends the message.
        /// </summary>
        private async void SendMessage()
        {
            ResponseMessage = await serviceConnector.SendMessageAsync(serviceID, UserMessageInput);
        }

        /// <summary>
        /// Converts to rem resource message.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        private RemResMessage ConvertToRemResMessage()
        {
            XmlSerializer xmlFormatter = new XmlSerializer(typeof(RemResMessage));
            StringReader reader = new StringReader(UserMessageInput);
            XmlTextReader xmlReader;
            xmlReader = new XmlTextReader(reader);

            try
            {
                return (RemResMessage)xmlFormatter.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Debugger.Break();
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }

            return null;
        }

        /// <summary>
        /// Determines whether this instance [can send message].
        /// </summary>
        /// <returns></returns>
        private bool CanSendMessage()
        {
            if (String.IsNullOrEmpty(clientAddress) || string.IsNullOrEmpty(userMessageInput))
            {
                return false; 
            }
            return true;
        }

        #endregion

        #region MessageType

        /// <summary>
        /// 
        /// </summary>
        public class MessageType
        {
            /// <summary>
            /// Gets or sets the name of the message.
            /// </summary>
            /// <value>
            /// The name of the message.
            /// </value>
            public string MessageName { get; set; }

            /// <summary>
            /// Gets or sets the message template.
            /// </summary>
            /// <value>
            /// The message template.
            /// </value>
            public string MessageTemplate { get; set; }

            /// <summary>
            /// Gets or sets the type of the message.
            /// </summary>
            /// <value>
            /// The type of the message.
            /// </value>
            public Type MessageClassType { get; set; }

        }

        #endregion

        /// <summary>
        /// Stops the notification endpoint.
        /// </summary>
        public void StopNotificationEndpoint()
        {
            notificationEndpoint.StopNotificationEndpoint();
        }
    }
}
