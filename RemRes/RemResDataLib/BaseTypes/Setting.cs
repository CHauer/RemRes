using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class Setting
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
