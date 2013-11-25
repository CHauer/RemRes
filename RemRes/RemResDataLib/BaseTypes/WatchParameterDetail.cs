using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class WatchParameterDetail
    {
        public WatchField WatchField { get; set; }

        public SingleValue SingleValue { get; set; }
    }
}
