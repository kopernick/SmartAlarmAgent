using SmartAlarmAgent.Repository;
using SmartAlarmAgent.Service;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

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

        private string _CSVFile;
        public string CSVFile
        {
            get
            {
                return _CSVFile;

            }
            set
            {
                _CSVFile = value;
                OnPropertyChanged("CSVFile");
            }
        }

        private string _CSVStatus;
        public string CSVStatus
        {
            get
            {
                return _CSVStatus;

            }
            set
            {
                _CSVStatus = value;
                OnPropertyChanged("CSVStatus");
            }
        }

        private DateTime _CSVLastModify;
        public DateTime CSVLastModify
        {
            get
            {
                return _CSVLastModify;

            }
            set
            {
                _CSVLastModify = value;
                OnPropertyChanged("CSVLastModify");
            }
        }

        private string _DBName;
        public string DBName
        {
            get
            {
                return _DBName;

            }
            set
            {
                _DBName = value;
                OnPropertyChanged("DBName");
            }
        }

        private DateTime _DBLastAccess;
        public DateTime DBLastAccess
        {
            get
            {
                return _DBLastAccess;

            }
            set
            {
                _DBLastAccess = value;
                OnPropertyChanged("DBLastAccess");
            }
        }

        private string _CSVLastAlarm;
        public string CSVLastAlarm
        {
            get
            {
                return _CSVLastAlarm;

            }
            set
            {
                _CSVLastAlarm = value;
                OnPropertyChanged("CSVLastAlarm");
            }
        }

        private string _DBSLastRec;
        public string DBSLastRec
        {
            get
            {
                return _DBSLastRec;

            }
            set
            {
                _DBSLastRec = value;
                OnPropertyChanged("DBSLastRec");
            }
        }

        private string _DBStatus;
        public string DBStatus
        {
            get
            {
                return _DBStatus;

            }
            set
            {
                _DBStatus = value;
                OnPropertyChanged("DBStatus");
            }
        }

        private Brush _CSVBackgroundColor;
        public Brush CSVBackgroundColor
        {
            get { return _CSVBackgroundColor; }
            set
            {
                _CSVBackgroundColor = value;
                OnPropertyChanged("CSVBackgroundColor");
            }
        }

        private Brush _DBBackgroundColor;
        public Brush DBBackgroundColor
        {
            get { return _DBBackgroundColor; }
            set
            {
                _DBBackgroundColor = value;
                OnPropertyChanged("DBBackgroundColor");
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
            switch(eventArg.Target)
            { 
            case "Activity":
                  try
                  {
                      lock (_lock)
                      {
                        
                        ListEventLog.Insert(0, new Event() { Message = eventArg.Message, TimeStamp = eventArg.TimeStamp });
                     
                      }
                  }
                  catch(Exception e)
                  {
                      Console.WriteLine("What is It" + e.Message);
                  }
                break;

            case "CSVStatus":
                    Console.WriteLine("CSV Last Modified : " + eventArg.ConnStatus.LastModified
                        +" File : "+ eventArg.ConnStatus.Info 
                        +" Status : "+ (eventArg.ConnStatus.Status?"Connected": "Disconnected"));
                    
                    CSVFile = eventArg.ConnStatus.Info;
                    CSVLastModify = eventArg.ConnStatus.LastModified;
                    CSVStatus = eventArg.ConnStatus.Status ? "Connected" : "Disconnected";
                    CSVLastAlarm = eventArg.ConnStatus.LastRec;
                    CSVBackgroundColor = eventArg.ConnStatus.Status ? Brushes.Green : Brushes.Red;
                    break;

                case "DBStatus":
                    Console.WriteLine("DB Last Modified : " + eventArg.ConnStatus.LastModified
                        + " File : " + eventArg.ConnStatus.Info
                        + " Status : " + (eventArg.ConnStatus.Status ? "Connected" : "Disconnected"));

                    DBName = eventArg.ConnStatus.Info;
                    //DBLastAccess = eventArg.ConnStatus.LastModified;
                    if(eventArg.ConnStatus.Status)
                        DBLastAccess = eventArg.ConnStatus.LastModified;

                    DBStatus = eventArg.ConnStatus.Status ? "Connected" : "Connection Fail";
                    DBSLastRec = eventArg.ConnStatus.LastRec;
                    DBBackgroundColor = eventArg.ConnStatus.Status ? Brushes.Green : Brushes.Red;

                    break;
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
