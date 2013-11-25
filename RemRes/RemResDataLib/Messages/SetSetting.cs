using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "SetSetting")]
    public class SetSetting : RemResMessage
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }
}
