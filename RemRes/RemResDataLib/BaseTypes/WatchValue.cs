using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class WatchValue
    {
        public string Value { get; set; }

        public DateTime MomentOfMeasure { get; set; }
    }
}