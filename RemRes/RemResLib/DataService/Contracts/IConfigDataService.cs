using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemResDataLib.BaseTypes;

namespace RemResLib.DataService.Contracts
{
    public interface IConfigDataService
    {
        /// <summary>
        /// Loads the watch rules.
        /// </summary>
        /// <returns></returns>
        IList<WatchRule> LoadWatchRules();

        /// <summary>
        /// Saves the watch rules.
        /// </summary>
        /// <param name="list">The list.</param>
        void SaveWatchRules(WatchRuleSet list);

    }
}
