using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;

namespace RemResLib.DataService.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISettingsDataService
    {

        /// <summary>
        /// Loads the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string LoadSettingValue(string key);

        /// <summary>
        /// Saves the setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool SaveSettingValue(string key, string value);
    }
}
