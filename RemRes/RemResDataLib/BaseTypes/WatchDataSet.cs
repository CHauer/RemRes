using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    [XmlRoot(ElementName="WatchDataSet")]
    public class WatchDataSet : List<WatchField>
    {
    }
}
