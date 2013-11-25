using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class WatchRule
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Notifiy { get; set; }

        [XmlAttribute]
        public int Period { get; set; }

        public WatchField WatchField { get; set; }

        public WatchParameter WatchParameter { get; set; }
    }
}
