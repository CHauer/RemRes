using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlRoot(ElementName = "OperationStatus")]
    public class OperationStatus : RemResMessage
    {
        public string Command { get; set; }

        public StatusType Status { get; set; }

        public string Message { get; set; }
    }
}
