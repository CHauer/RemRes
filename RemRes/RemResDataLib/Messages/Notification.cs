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
    [XmlRoot(ElementName = "Notification")]
    public class Notification : RemResMessage
    {

        public String Type { get; set; }

        public String Message { get; set; }

        public String WatchRuleName{get; set;}

        public string LastValue { get; set; }

        public WatchField WatchField { get; set; }

    }
}
