using SmartAlarmAgent.Repository;
using SmartAlarmAgent.Service;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartAlarmAgent.Model
{
    class ActivityMonitorViewModel : PropertyChangeEventBase
    {

        #region Properties
        private ObservableCollection<Event> _ListEventLog;
        public ObservableCollection<Event> ListEventLog
        {
            get
            {
                return _ListEventLog;
            }
            set
            {
                _ListEventLog = value;
                OnPropertyChanged("ListEventLog");

            }
        }
        private static object _lock = new object();

        //private RestEventArgs eventArg;
        EventChangedEventArgs eventArg;

        #endregion Properties

        #region Class Construct.
        public ActivityMonitorViewModel()
        {
            ListEventLog = new ObservableCollection<Event>();
            DataProcessLogic.SendUpdateEvent += OnAlarmListChanged;
            BindingOperations.EnableCollectionSynchronization(ListEventLog, _lock);
        }

        private void OnAlarmListChanged(object sender, EventChangedEventArgs e)
        {
            eventArg = e;
            Console.WriteLine("Recieving Mas from DataProcessLogic");
            //if(e.UpdatePost != null)
            {
                Task.Factory.StartNew(UpdateActivityConsole);
            }
        }

        #endregion Properties


        #region Helper
        //private void InitializerActivityConsole()
        //{
        //    ListEventLog = new ObservableCollection<Event>();

        //    ListEventLog.Insert((int)EventLogPosition.CSV_STATUS, "Read CSV Success/Fail");
        //    ListEventLog.Insert((int)EventLogPosition.CSV_NEW_EVENT, "New Event");
        //    ListEventLog.Insert((int)EventLogPosition.REST_NEW_POINT, "--> Restoration Point");
        //    ListEventLog.Insert((int)EventLogPosition.SEP_1, "===========================================");
        //    ListEventLog.Insert((int)EventLogPosition.DB_STATUS, "Read Database Success/Fail");
        //    ListEventLog.Insert((int)EventLogPosition.DB_TOTAL_POINT, "Total Point ");
        //    ListEventLog.Insert((int)EventLogPosition.DB_TOTAL_POINT, "Last Access Database");
        //    ListEventLog.Insert((int)EventLogPosition.SEP_2, "===========================================");
        //    ListEventLog.Insert((int)EventLogPosition.MSG_TITLE, "[Message]");
        //    ListEventLog.Insert((int)EventLogPosition.ERR_MSG, "Error.....");
        //    ListEventLog.Insert((int)EventLogPosition.ETC_STATUS, "Under Matching Point/Other");

        //}

        public void UpdateActivityConsole()
        {
            try
            {
                lock (_lock)
                {
                  
                  ListEventLog.Add(new Event() { Message = eventArg.message, TimeStamp = eventArg.TimeStamp });
               
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("What is It" + e.Message);
            }
        }

        //public void OnEventChanged(object source, EventChangedEventArgs e)
        //    {
        //        //this.LogConsole = this.LogConsole.Insert(0, Environment.NewLine);
        //        //this.LogConsole = this.LogConsole.Insert(0, mStr);
        //        Console.WriteLine(e.TimeStamp.ToString() + "\t" + e.Msg.ToString());
        //        //mLogConsole.Text += e.TimeStamp.ToString() + "\t" + e.Status.ToString();
        //    }
        }

    public class EventChangedEventArgs : EventArgs
        {
            public DateTime TimeStamp { get; set; }
            public int UpdatePost { get; set; }
            public string message { get; set; }
    }

    #endregion Helper
}
