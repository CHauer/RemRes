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
    [XmlRoot(ElementName = "GetWatchData")]
    public class GetWatchData : RemResMessage
    {
        [XmlAttribute]
        public String Name { get; set; }

        public WatchField WatchField { get; set; }
    }
}
