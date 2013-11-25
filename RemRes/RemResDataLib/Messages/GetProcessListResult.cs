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
    [XmlRoot(ElementName = "GetProcessListResult")]
    public class GetProcessListResult : RemResMessage
    {
        public ProcessSet ProcessSet { get; set; }
    }
}
