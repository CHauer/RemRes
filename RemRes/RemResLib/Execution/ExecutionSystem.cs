﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.Watch;
using RemResLib.Watch.Contract;

namespace RemResLib.Execution
{
    public class ExecutionSystem
    {
        /// <summary>
        /// The execution system object
        /// </summary>
        private static ExecutionSystem executionSystemObj;

        /// <summary>
        /// The Message queue
        /// </summary>
        private Queue<ExecutionMessage> messageQueue;

        /// <summary>
        /// The execution system running flag
        /// </summary>
        private bool executionSystemRunning;

        /// <summary>
        /// The execution thread
        /// </summary>
        private Thread executionThread;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The watch system object
        /// </summary>
        private IWatchSystem watchSystemObj;

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="ExecutionSystem"/> class from being created.
        /// </summary>
        private ExecutionSystem()
        {
            messageQueue = new Queue<ExecutionMessage>();
            InitLog();
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

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static ExecutionSystem GetInstance()
        {
            if(executionSystemObj == null)
            {
                executionSystemObj = new ExecutionSystem();
            }

            return executionSystemObj;
        }

        #endregion

        #region Enque Messages

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientID">The client identifier.</param>
        public Guid AddMessageForExecution(RemResMessage message, Guid clientID)
        {
            Guid messageId = Guid.NewGuid();

            messageQueue.Enqueue(new ExecutionMessage()
            { 
                MessageID = messageId,
                Message = message,
                ClientID = clientID 
            });

            return messageId;
        }

        #endregion

        #region Start - Stop

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            executionSystemRunning = true;

            try
            {
                executionThread = new Thread(ProcessMessages);
                executionThread.Start();
            }
            catch(Exception ex)
            {
                log.Error("Problem during start of the Execution System.", ex);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            executionSystemRunning = false;
        }

        #endregion

        #region Process Messages
        
        /// <summary>
        /// Processes the messages.
        /// </summary>
        private void ProcessMessages()
        {
            ExecutionMessage message;

            while (executionSystemRunning)
            {
                while (messageQueue.Peek() == null) 
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
                }
                
                message = messageQueue.Dequeue();

                var method = GetMethodHandlerForMessage(message.Message.GetType());

                if (method != null)
                {
                    method.Invoke(watchSystemObj, new object[] { message.Message, message.ClientID });
                }
                else
                {
                    log.InfoFormat("The System can not find a handler for the message {0}.", message.Message.GetType());
                }
            }
        }

        /// <summary>
        /// Gets the method for message.
        /// </summary>
        /// <param name="messageTyp">The message typ.</param>
        /// <returns></returns>
        private MethodInfo GetMethodHandlerForMessage(Type messageTyp)
        {
            //if injection object not set 
            if (watchSystemObj == null)
            {
                return null;
            }

            // ReSharper disable once CheckForReferenceEqualityInstead.1
            // check each public method and extract (with reflection) the custom attributes
            // and check for message type 
            var methodCandidates = watchSystemObj.GetType().GetMethods(BindingFlags.Public)
                .Where(m => m.GetCustomAttributes<RemResMessageHandler>().Any(a => a.MessageType.Equals(messageTyp)))
                .OrderBy(m => m.Name).ToList();

            // check if methods with attributes are available 
            if (methodCandidates.Count > 0)
            {
                // check if a method handler exists that overrides the other one
                if (methodCandidates.Any(m => m.GetCustomAttributes<RemResMessageHandler>().Any(a => a.OverrideExistingHandler)))
                {
                    return methodCandidates.FirstOrDefault(m => m.GetCustomAttributes<RemResMessageHandler>().Any(a => a.OverrideExistingHandler));
                }

                //otherwise return the first devined handler
                return methodCandidates[0];
            }

            return null;
        }

        #endregion

        #region Execution Message Type

        /// <summary>
        /// Simple emmbedded Data Type of use only in execution system for
        /// data encapsulating
        /// </summary>
        private class ExecutionMessage
        {
            /// <summary>
            /// Gets or sets the message identifier.
            /// </summary>
            /// <value>
            /// The message identifier.
            /// </value>
            public Guid MessageID { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public RemResMessage Message { get; set; }

            /// <summary>
            /// Gets or sets the client identifier.
            /// </summary>
            /// <value>
            /// The client identifier.
            /// </value>
            public Guid ClientID { get; set; }
        }

        #endregion

    }
}
