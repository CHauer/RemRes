using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RemResDataLib.BaseTypes;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "GetWatchRuleResult")]
    public class GetWatchRuleResult : RemResMessage
    {
        public WatchRuleSet WatchRuleSet { get; set; }

    }
}
