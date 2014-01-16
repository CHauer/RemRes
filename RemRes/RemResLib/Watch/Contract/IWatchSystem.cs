using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResLib.DataService.Contracts;

namespace RemResLib.Watch.Contract
{
    public interface IWatchSystem
    {
        /// <summary>
        /// Gets or sets the configuration data service.
        /// </summary>
        /// <value>
        /// The configuration data service.
        /// </value>
        IConfigDataService ConfigDataService { get; set; }


    }
}
