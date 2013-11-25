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

        #region Constructor 

        /// <summary>
        /// Prevents a default instance of the <see cref="SettingsManager"/> class from being created.
        /// </summary>
        private SettingsManager()
        {

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

    }
}
