﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RemResService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            #region Eventlog erzeuge
            //Eventlog erzeugen
            if (!EventLog.SourceExists("RemResService"))
            {
                try
                {
                    EventLog.CreateEventSource("RemResService", "RemResLog");
                }
                catch { ;}
            }
            #endregion
        }
    }
}
