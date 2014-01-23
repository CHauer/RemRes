using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResLib.DataService.Contracts;
using RemResLib.Network;
using RemResLib.Settings;

namespace RemResLib.DataService
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlNotificationDataService : INotificationDataService
    {

        /// <summary>
        /// The XML formatter
        /// </summary>
        private XmlSerializer xmlFormatter;

        /// <summary>
        /// The settings manager object
        /// </summary>
        private SettingsManager settingsManagerObj;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        /// <summary>
        /// The configuration file name
        /// </summary>
        private string notificationFileName;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNotificationDataService"/> class.
        /// </summary>
        public XmlNotificationDataService()
        {
            InitializeObjects();
            InitializeLog();
            InitializeConfigFile();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the objects.
        /// </summary>
        private void InitializeObjects()
        {
            xmlFormatter = new XmlSerializer(typeof(List<NotificationEndpoint>));
            settingsManagerObj = SettingsManager.GetInstance();
        }

        /// <summary>
        /// Initializes the log.
        /// </summary>
        private void InitializeLog()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// Initializes the configuration file.
        /// </summary>
        private void InitializeConfigFile()
        {
            try
            {
                notificationFileName = settingsManagerObj.GetSettingValue("endpointConfigSave").Value;

                if (string.IsNullOrEmpty(notificationFileName))
                {
                    notificationFileName = "notificationEndpoints.xml";
                }
            }
            catch (Exception ex)
            {
                log.Debug("Error while loading the notificationEndpoints config save file name from settings.", ex);
                return;
            }
        }

        #endregion

        /// <summary>
        /// Loads the notification endpoints.
        /// </summary>
        /// <returns></returns>
        public List<NotificationEndpoint> LoadNotificationEndpoints()
        {
            Stream fileStream = null;
            List<NotificationEndpoint> endpoints = new List<NotificationEndpoint>();

            if (!File.Exists(notificationFileName))
            {
                log.Debug("No config file found.");

                //eturn emtpy list
                return endpoints;
            }

            try
            {
                fileStream = File.OpenRead(notificationFileName);
            }
            catch (Exception ex)
            {
                log.Debug("Error during opening the notification endpoints config file for read.", ex);

                //return empty list
                return endpoints;
            }

            try
            {
                endpoints = (List<NotificationEndpoint>)xmlFormatter.Deserialize(fileStream);
            }
            catch (Exception ex)
            {
                log.Debug("Error during deserializing the notification endpoint file data to endpoint objects.", ex);

                //return empty list
                return endpoints;
            }

            return endpoints;
        }

        /// <summary>
        /// Saves the notification endpoints.
        /// </summary>
        /// <param name="endpoints">The endpoints.</param>
        public void SaveNotificationEndpoints(List<NotificationEndpoint> endpoints)
        {
            Stream fileStream = null;

            try
            {
                fileStream = File.OpenWrite(notificationFileName);
            }
            catch (Exception ex)
            {
                log.Debug("Error during opening the notification endpoint config file for write.", ex);
                return;
            }

            try
            {
                xmlFormatter.Serialize(fileStream, endpoints);
            }
            catch (Exception ex)
            {
                log.Debug("Error during serializing the notification endpoint data to config file.", ex);
            }
            finally
            {
                fileStream.Flush();
                fileStream.Close();
            }

        }
    }
}
