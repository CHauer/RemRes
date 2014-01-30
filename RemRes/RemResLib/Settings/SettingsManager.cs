using System.Diagnostics.Contracts;
using System.Reflection;
using RemResDataLib.BaseTypes;
using RemResLib.DataService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResLib.Settings
{
    public class SettingsManager
    {
        /// <summary>
        /// The settings manager singelton instance
        /// </summary>
        private static SettingsManager settingsManagerInstance;

        /// <summary>
        /// The settings data service object
        /// </summary>
        private ISettingsDataService settingsDataServiceObj;

        /// <summary>
        /// The log
        /// </summary>
        private static log4net.ILog log;

        #region Constructor 

        /// <summary>
        /// Prevents a default instance of the <see cref="SettingsManager"/> class from being created.
        /// </summary>
        private SettingsManager()
        {
            InitLog();
        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static SettingsManager GetInstance()
        {
            if (settingsManagerInstance == null)
            {
                settingsManagerInstance = new SettingsManager();
            }
            return settingsManagerInstance;
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

        #region Injection Property

        /// <summary>
        /// Gets or sets the settings data service.
        /// </summary>
        /// <value>
        /// The settings data service.
        /// </value>
        public ISettingsDataService SettingsDataService
        {
            get
            {
                return settingsDataServiceObj;
            }
            set
            {
                this.settingsDataServiceObj = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The SettingsDataService has to be initialized</exception>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public Setting GetSettingValue(string key)
        {
            string loadValue = string.Empty;

            if (settingsDataServiceObj == null)
            {
                throw new InvalidOperationException("The SettingsDataService has to be initialized.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            try
            {
                loadValue = settingsDataServiceObj.LoadSettingValue(key);

                if (String.IsNullOrEmpty(loadValue))
                {
                    throw new ArgumentOutOfRangeException(key);
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                string msg = "Settings key not found system.";
                log.Debug(msg, aex);
                
                throw new InvalidOperationException(msg, aex);
            }
            catch (Exception ex)
            {
                string msg = "Error during load process of settings value.";
                log.Debug(msg, ex);

                throw new InvalidOperationException(msg, ex);
            }

            return new Setting()
            {
                Value = loadValue,
                Key = key
            };
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The SettingsDataService has to be initialized.</exception>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public bool SetSettingValue(string key, string value)
        {
            if (settingsDataServiceObj == null)
            {
                throw new InvalidOperationException("The SettingsDataService has to be initialized.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            try
            {
                return settingsDataServiceObj.SaveSettingValue(key, value);
            }
            catch (Exception ex)
            {
                log.Debug("Error during save process of settings value.", ex);
            }

            return false;
        }

    }
}
