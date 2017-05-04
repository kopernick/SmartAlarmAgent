using SmartAlarmAgent.Repository;
using SmartAlarmAgent.Service;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace SmartAlarmAgent.Model
{
    class ActivityMonitorViewModel : PropertyChangeEventBase
    {

        #region Properties

        private DispatcherTimer m_dispatcherTimerClearActivity = new System.Windows.Threading.DispatcherTimer();

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

        public string Time { get; private set; }

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

            this.m_dispatcherTimerClearActivity.Interval = new TimeSpan(0, 0, 1); //Get Update Database Period
            this.m_dispatcherTimerClearActivity.Start();
            this.m_dispatcherTimerClearActivity.Tick += dispatcherTimerClearAct_Tick;
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
            switch (eventArg.Target)
            {
                case "Activity":
                    try
                    {
                        lock (_lock)
                        {

                            ListEventLog.Insert(0, new Event() { Message = eventArg.Message, TimeStamp = eventArg.TimeStamp });

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("What is It" + e.Message);
                    }
                    break;

                default:
                    Console.WriteLine("Defualt");
                    break;
            }
        }

        private void dispatcherTimerClearAct_Tick(object sender, EventArgs e)
        {
            Time = DateTime.Now.ToLongTimeString();
            if (Time.Equals("12:00:00 AM") || Time.Equals("00:00:00"))
                ListEventLog.Clear();
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
            public string Message { get; set; }
            public string Target { get; set; }
           public ConnectionStatus ConnStatus { get; set; }
    }
    public class ConnectionStatus
    {
        public DateTime LastModified { get; set; }
        public bool Status  { get; set; }
        public string Info { get; set; }
        public string LastRec { get; set; }
    }

    #endregion Helper
}
