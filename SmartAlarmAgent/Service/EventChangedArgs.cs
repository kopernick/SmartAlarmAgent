﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAlarmAgent.Service
{
        public class EventChangedArgs : EventArgs
        {
            public string Status { get; set; }
            public DateTime TimeStamp { get; set; }
        }
    
}
