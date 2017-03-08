using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmData;
using SmartAlarmAgent.Repository;
using SmartAlarmAgent.Model;
using SmartAlarmAgent.Service;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace SmartAlarmAgent
{
     class MainWindowViewModel : PropertyChangeEventBase
    {
        #region Properties

        private DispatcherTimer m_dispatcherTimerCSV = new System.Windows.Threading.DispatcherTimer();
        private DispatcherTimer m_dispatcherTimerDB = new System.Windows.Threading.DispatcherTimer();

        private List<DigitalPointInfo> _DigitalPointInfoList;
        public List<DigitalPointInfo> DigitalPointInfoList
        {
            get { return _DigitalPointInfoList; }
        }

        private ObservableCollection<string> _ListEventLog;
        public ObservableCollection<string> ListEventLog {
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

        private List<AlarmList> _CSVAlarmList;
        public List<AlarmList> CSVAlarmList
        {
            get { return _CSVAlarmList; }
        }


        private static AlarmListCSVRepo _mAlarmList;

        public AlarmListCSVRepo mAlarmList
        {
            get { return _mAlarmList; }
        }

        private static RestorationAlarmDBRepo _mRestorationAlarmList;

        //Test New EF Code first from exiting DB
        private static RestAlarmDBRepo _mRestAlarmList;


        private bool _flgMatchingInProgress;
        public bool flgStart
        {
            get { return _flgMatchingInProgress; }
        }

        private int _nNewRestPoint;
        public int nNewRestPoint
        {
            get { return _nNewRestPoint; }
        }
        #endregion Properties

        #region Constructor
        public MainWindowViewModel()
        {
            _mAlarmList = new AlarmListCSVRepo();
            _mRestorationAlarmList = new RestorationAlarmDBRepo();
            _mRestAlarmList = new RestAlarmDBRepo();                //Test EF
            _mAlarmList.RestAlarmCSVChanged += OnAlarmListChanged;
            _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged;
            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            ListEventLog = new ObservableCollection<string>();
            InitializerLogConsole();

            Initializer();

            Console.WriteLine("Skip");
        }
        #endregion Constructor

        #region Methode

        private void InitializerLogConsole()
        {
            
            ListEventLog.Insert((int)EventLogPosition.CSV_STATUS , "Read CSV Success/Fail");
            ListEventLog.Insert((int)EventLogPosition.CSV_NEW_EVENT , "New Event");
            ListEventLog.Insert((int)EventLogPosition.REST_NEW_POINT , "--> Restoration Point");
            ListEventLog.Insert((int)EventLogPosition.SEP_1 , "===========================================");
            ListEventLog.Insert((int)EventLogPosition.DB_STATUS , "Read Database Success/Fail");
            ListEventLog.Insert((int)EventLogPosition.DB_TOTAL_POINT , "Total Point ");
            ListEventLog.Insert((int)EventLogPosition.DB_TOTAL_POINT , "Last Access Database");
            ListEventLog.Insert((int)EventLogPosition.SEP_2 , "===========================================");
            ListEventLog.Insert((int)EventLogPosition.MSG_TITLE , "[Message]");
            ListEventLog.Insert((int)EventLogPosition.ERR_MSG , "Error.....");
            ListEventLog.Insert((int)EventLogPosition.ETC_STATUS , "Under Matching Point/Other");

        }

        private async void Initializer()
        {
            var x = await _mAlarmList.GetNewAlarmListAsync();
            //if (x == false) return;
            if(_mAlarmList.ListAlarm != null)
                Console.WriteLine($"Can read CSV Alarm List ? : {_mAlarmList.ListAlarm.Count.ToString()}");

            this.m_dispatcherTimerCSV.Interval = new TimeSpan(0, 0, 30); //Get Update CSV File Period
            this.m_dispatcherTimerCSV.Start();
            this.m_dispatcherTimerCSV.Tick += dispatcherTimerCSV_Tick;

            this.m_dispatcherTimerDB.Interval = new TimeSpan(1, 0, 0); //Get Update Database Period
            this.m_dispatcherTimerDB.Start();
            this.m_dispatcherTimerDB.Tick += dispatcherTimerDB_Tick;

        }





        private void OnAlarmListChanged(object source, RestEventArgs args)
        {
            // throw new NotImplementedException();
            switch (args.message)
            {

                case "Read CSV Success":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Success");
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS, "Read CSV Success");
                    break;

                case "Read CSV Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Fail");
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS , "Read CSV Fail");

                    break;

                case "Has New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() +" : "
                        + (mAlarmList.ListAlarm.Count - _mAlarmList.nStartIndex - 1).ToString() + " New Alarm(s)");

                    this.onHasNewAlarm();
                    break;

                case "Has No New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : No New Alarm");
                    break;

                case "Start Process":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Start Insert to DB");
                    this.onHasNewAlarm();
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");
                    break;
            }
        }

        private async void onHasNewAlarm()
        {

            this._nNewRestPoint = 0;

            List<RestorationAlarmList> RestAlarmList = new List<RestorationAlarmList>();
            
            //Refresh Point from Database
            _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

            if (_DigitalPointInfoList == null) 
                return;                         //Can't Access Database

            _flgMatchingInProgress = true;

            for (int iIndex = _mAlarmList.nStartIndex + 1; iIndex < _mAlarmList.ListAlarm.Count; iIndex++)
            {
                AlarmList al = _mAlarmList.ListAlarm[iIndex];
                if (al.pointType != PointType.Digital) continue;

                var groupByStations = _DigitalPointInfoList.Where(c => c.StationName.Trim() == al.StationName.Trim());// Groupong Station before Mapping

                RestorationAlarmList point = _mAlarmList.GetRestorationAlarmPoint(al, groupByStations); //Mapping CSV Point --> DigitalPointInfo Table

                if (point != null)
                {
                    try
                    {
                        RestAlarmList.Add(point);
                        _nNewRestPoint++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());

                    }

                }
            }

             _mRestorationAlarmList.Complete();

            _flgMatchingInProgress = false;

            Console.WriteLine($"{DateTime.Now.ToString()} : Finish Matching Point");

            Console.WriteLine($"Has {_nNewRestPoint} Restoration Alarm(s)");
            //updateLogConsole((int)EventLogPosition.REST_NEW_POINT, $"Has {_nNewRestPoint} Restoration Alarm(s)");

            _mAlarmList.nStartIndex = _mAlarmList.ListAlarm.Count-1;// Index start with 0
            _mRestorationAlarmList.RestAlarmContext.RestorationAlarmList.AddRange(RestAlarmList);
           
        }

        private void updateLogConsole(int Position, string Msg)
        {
            ListEventLog.Insert(Position, Msg);
            ListEventLog.RemoveAt(Position);
        }
        private void OnDBChanged(object source, RestEventArgs args)
        {
            switch (args.message)
            {

                case "Read DB Success":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read Database Success");
                        
                    break;

                case "Read DB Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read Database Fail");
                    break;

                case "Reset":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Reset DB");
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");
                    break;
            }

        }
        private async void dispatcherTimerCSV_Tick(object sender, EventArgs e)
        {
            if (_flgMatchingInProgress == true)
            {
                Console.WriteLine("Under Matching Point Process");
                return ;
            }
            await Task.Run(() => _mAlarmList.GetNewAlarmListAsync());

        }

        private void dispatcherTimerDB_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString() + " : Timer Get Update DB");
        }

        #endregion Methode
    }
}
