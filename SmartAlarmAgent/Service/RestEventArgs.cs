using System;

namespace SmartAlarmAgent.Service
{
    public class RestEventArgs : EventArgs
    {
            public string message { get; set; }
            public DateTime TimeStamp { get; set; }
            public int? UpdatePost { get; set; }

    }
    
}
