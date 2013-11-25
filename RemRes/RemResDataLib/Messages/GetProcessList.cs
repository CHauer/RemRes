using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "GetProcessList")]
    public class GetProcessList : RemResMessage
    {
        [XmlAttribute]
        public int RAMOver { get; set; }
    }
}
