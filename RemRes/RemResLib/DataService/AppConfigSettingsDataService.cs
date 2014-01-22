using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RemResLib.DataService.Contracts;
using System.Configuration;

namespace RemResLib.DataService
{
    /// <summary>
    /// The SettingsDataService saves the settings values in the app.config 
    /// file of the current assembly.
    /// </summary>
    public class AppConfigSettingsDataService : ISettingsDataService
    {
        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsDataService"/> class.
        /// </summary>
        public AppConfigSettingsDataService()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        /// <summary>
        /// Loads the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public string LoadSettingValue(string key)
        {
            string loadValue = null;

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            try
            {
                loadValue = ConfigurationManager.AppSettings.Get(key);
            }
            catch(Exception ex)
            {
                log.Debug("Error during load of the Settings value from App.Config.", ex);
            }
            return loadValue;
        }

        /// <summary>
        /// Saves the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public bool SaveSettingValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                try
                {
                    ConfigurationManager.AppSettings.Set(key, value);
                    return true;
                }
                catch (Exception ex)
                {
                    log.Debug("Error during save of the settings value to App.Config.", ex);
                }
            }
            else
            {
                try
                {
                    ConfigurationManager.AppSettings.Add(key, value);
                    return true;
                }
                catch (Exception ex)
                {
                    log.Debug("Error during add a new settings value to App.Config.", ex);
                }
            }

            return false;
        }
    }
}
