using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{

    [Serializable]
    public class WatchField
    {
        [XmlAttribute]
        public WatchFieldType Type { get; set; }
        public string WatchObject { get; set; }

        public string WatchProperty { get; set; }

        public List<WatchValue> WatchFieldValues { get; set; }
    }
}
