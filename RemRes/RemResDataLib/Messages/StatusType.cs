using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    public enum StatusType
    {
        [XmlEnum]
        OK,

        [XmlEnum]
        INVALIDINPUT,

        [XmlEnum]
        ERROR
    }
}
