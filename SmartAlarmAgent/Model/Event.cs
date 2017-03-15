using SmartAlarmAgent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAlarmAgent.Model
{
    public class Event : PropertyChangeEventBase
    {
        private string _massage;
        public string Message
        {
            get { return _massage; }
            set
            {
                if (_massage == value) return;
                _massage = value;
                RaisePropertyChanged("Message");

            }
        }

        private DateTime _TimeStamp;

        public DateTime TimeStamp
        {
            get { return _TimeStamp; }
            set
            {
                if (_TimeStamp == value) return;
                _TimeStamp = value;
                RaisePropertyChanged("TimeStamp");
            }
        }
    }
}
