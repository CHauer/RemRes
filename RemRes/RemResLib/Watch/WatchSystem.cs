using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.Messages;
using RemResLib.DataService.Contracts;
using RemResLib.Watch.Contract;

namespace RemResLib.Watch
{
    public class WatchSystem : IWatchSystem
    {
        /// <summary>
        /// The WatchSystem singelton instance object
        /// </summary>
        private static WatchSystem wsObj;

        /// <summary>
        /// The configuration data service object
        /// </summary>
        private IConfigDataService configDataServiceObj;

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="WatchSystem"/> class from being created.
        /// </summary>
        private WatchSystem()
        {
        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static WatchSystem GetInstance()
        {
            if (wsObj == null)
            {
                return wsObj;
            }

            return wsObj;
        }

        #endregion

        #region Injection Properties

        /// <summary>
        /// Gets or sets the configuration data service.
        /// </summary>
        /// <value>
        /// The configuration data service.
        /// </value>
        public IConfigDataService ConfigDataService
        {
            get
            {
                return this.configDataServiceObj;
            }
            set
            {
                this.configDataServiceObj = value;
            }
        }

        #endregion

        #region Message Handler Methods

        /// <summary>
        /// Adds the watch rule.
        /// </summary>
        /// <returns></returns>
        [RemResMessageHandler(typeof(AddWatchRule))]
        public RemResMessage AddWatchRule()
        {
            return null;
        }

        #endregion
    }
}
