using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class SingleValue
    {

        [XmlAttribute]
        public WatchFieldType Type { get; set; }

        public string Value { get; set; }
    }
}
