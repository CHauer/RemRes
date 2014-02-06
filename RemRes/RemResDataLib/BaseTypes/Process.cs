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

        public string ProcessTitle { get; set; }

        public bool Responding { get; set; }

        public int RAM { get; set;}

        public int PID { get; set; }

        //Removed because not used
        //public string User { get; set; }

    }
}
