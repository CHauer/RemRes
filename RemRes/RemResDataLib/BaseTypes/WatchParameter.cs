using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class WatchParameter
    {
        [XmlElement(ElementName = "Min")]
        public WatchParameterDetail Min { get; set; }

        [XmlElement(ElementName = "Max")]
        public WatchParameterDetail Max { get; set; }

        [XmlElement(ElementName = "Value")]
        public WatchParameterDetail Value { get; set; }
    }
}
