using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "NotifyMe")]
    public class NotifyMe : RemResMessage
    {
        public string Endpoint { get; set; }
    }
}
