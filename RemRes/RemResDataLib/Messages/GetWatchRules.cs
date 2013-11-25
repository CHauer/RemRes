using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "GetWatchRules")]
    public class GetWatchRules : RemResMessage
    {
    }
}
