using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResDataLib.BaseTypes;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;
using RemResLib.Settings;

namespace RemResLib.DataService
{
    public class XmlConfigDataService : IConfigDataService
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
        private string configFileName;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlConfigDataService"/> class.
        /// </summary>
        public XmlConfigDataService()
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
            xmlFormatter = new XmlSerializer(typeof(WatchRuleSet));
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
                configFileName = settingsManagerObj.GetSettingValue("serviceConfigSave").Value;

                if (string.IsNullOrEmpty(configFileName))
                {
                    configFileName = "config.xml";
                }
            }
            catch(Exception ex)
            {
                log.Debug("Error while loading the service config save file name from settings.", ex);
                return;
            }
        }

        #endregion

        /// <summary>
        /// Loads the watch rules from the config xml file.
        /// </summary>
        /// <returns></returns>
        public IList<WatchRule> LoadWatchRules()
        {
            Stream fileStream = null;
            List<WatchRule> ruleSet = new WatchRuleSet();

            if (!File.Exists(configFileName))
            {
                log.Debug("No config file found.");

                //eturn emtpy list
                return ruleSet;
            }

            try
            {
                fileStream = File.OpenRead(configFileName);
            }
            catch (Exception ex)
            {
                log.Debug("Error during opening the config file for read.", ex);
                
                //return empty list
                return ruleSet;
            }

            try
            {
                ruleSet = (WatchRuleSet)xmlFormatter.Deserialize(fileStream);
            }
            catch (Exception ex)
            {
                log.Debug("Error during deserializing the config file data to watch rules.", ex);

                //return empty list
                return ruleSet;
            }

            return ruleSet;
        }

        /// <summary>
        /// Saves the watch rules.
        /// </summary>
        /// <param name="list">The list.</param>
        public void SaveWatchRules(WatchRuleSet list)
        {
            Stream fileStream = null;

            try
            {
                fileStream = File.OpenWrite(configFileName);
            }
            catch (Exception ex)
            {
                log.Debug("Error during opening the config file for write.", ex);
                return;
            }

            try
            {
                xmlFormatter.Serialize(fileStream, list);   
            }
            catch (Exception ex)
            {
                log.Debug("Error during serializing the watch rule data to config file.", ex);
            }
            finally
            {
                fileStream.Flush();
                fileStream.Close();
            }

        }
    }
}
