using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public class Process
    {
        public string ProcessName { get; set; }

        public int RAM { get; set;}

        public string ProcessDescription{ get; set; }

        public int PID { get; set; }

        public string User { get; set; }

    }
}
