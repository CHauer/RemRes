using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "DeleteWatchRule")]
    public class DeleteWatchRule : RemResMessage
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}
