using SmartAlarmAgent.Model;
using SmartAlarmAgent.Repository;
using SmartAlarmData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartAlarmAgent.Service
{
    class DataProcessLogic : PropertyChangeEventBase
    {

        #region Properties

        private int _nLastAlarmRecIndex;
        public int nLastAlarmRecIndex
        {
            get
            {
                return _nLastAlarmRecIndex;
            }
            set
            {
                _nLastAlarmRecIndex = value;
                OnPropertyChanged("nLastAlarmRecIndex");
            }
        }
        private int _nNewRestPoint;
        public int nNewRestPoint
        {
            get { return _nNewRestPoint; }
        }

        private bool _flgMatchingInProgress;
        public bool flgStart
        {
            get { return _flgMatchingInProgress; }
        }

        private List<AlarmList> _listAlarm;

        public List<AlarmList> ListAlarm
        {
            get { return _listAlarm; }
        }

        private DateTime _CSVLastModify;
        public DateTime CSVLastModify
        {
            get { return _CSVLastModify; }
        }

        private readonly List<RestorationAlarmList> RestAlarmList = new List<RestorationAlarmList>();
        private static AlarmListCSVRepo _mAlarmList;
        public AlarmListCSVRepo mAlarmList
        {
            get { return _mAlarmList; }
        }

        private static RestorationAlarmDBRepo _mRestorationAlarmList;

        private List<DigitalPointInfo> _DigitalPointInfoList;
        public List<DigitalPointInfo> DigitalPointInfoList
        {
            get { return _DigitalPointInfoList; }
        }

        public DateTime dLastLoadDB { get; set; }
        public DateTime dLastReadCSV { get; set; }
        public DateTime dLastInsertRestAlarm { get; set; }
        //public RestorationAlarmList LastRestAlarmPoint { get; private set; }
        //public AlarmList LastCsvItem { get; private set; }

        #endregion Properties


        #region Event & Delegate

        public static event EventHandler<EventChangedEventArgs> SendUpdateEvent;
        private void onUpdateActivityMonitor(EventChangedEventArgs arg)
        {
            if (SendUpdateEvent != null)
                SendUpdateEvent(null, arg);
        }
        #endregion Event & Delegate


        #region Constructor
        public DataProcessLogic()
        {
            _mAlarmList = new AlarmListCSVRepo();
            _mRestorationAlarmList = new RestorationAlarmDBRepo();
            //_mRestAlarmList = new RestAlarmDBRepo();                //Test EF

            AlarmListCSVRepo.RestAlarmCSVChanged += OnAlarmListChanged;

            RestorationAlarmDBRepo.RestAlarmDBChanged += OnDBChanged;

            // _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged;

            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            this.nLastAlarmRecIndex = 0;

            Console.WriteLine("Skip");
        }
        #endregion Constructor

        public async void GetCSVData()
        {
            await _mAlarmList.GetAlarmListCSVAsync();
        }

        public async void onCheckCSVData()
        {
            await _mAlarmList.CheckNewAlarmListAsync();
        }
        
        public bool isDBConnected()
        {
            return  _mRestorationAlarmList.GetDBStatus();
        }

        private async void OnAlarmListChanged(object source, RestEventArgs args)
        {
            // throw new NotImplementedException();

            EventChangedEventArgs LogArg = new EventChangedEventArgs();
            switch (args.message)
            {

                case "Read CSV Success":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Success");
                    //UpdateActivityMonitor(args, "Activity");
                    UpdateActivityStatus(args, "CSVStatus", true);//Update CSV File Status

                    onCheckCSVData();

                    break;

                case "Read CSV Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Fail");
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS , "Read CSV Fail");
                    UpdateActivityMonitor(args, "Activity");
                    UpdateActivityStatus(args, "CSVStatus", false);//Update CSV File Status

                    break;

                case "Has New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : "
                        + (_mAlarmList.ListAlarm.Count - _mAlarmList.nStartIndex - 1).ToString() + " New Alarm(s)");

                    await this.onHasNewAlarm(args);
                    break;

                case "Has No New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : No New Alarm");
                    UpdateActivityMonitor(args, "Activity");

                    break;

                case "Start Process":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Start Data Processing");

                    UpdateActivityMonitor(args, "Activity");
                    await this.onStartProcess(args);
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");
                    //UpdateActivityMonitor(args, "Activity");
                    break;
            }
        }

        private async Task<bool> onStartProcess(RestEventArgs args)
        {
            this._nNewRestPoint = 0;

            this.RestAlarmList.Clear();

            //if (!isDBConnected())
            //{
            //    Console.WriteLine("DBConnection Error");
            //    _mAlarmList.nLastAlarmRecIndex = -1; // Reset to Ready State for Start Next time when DB Connected
            //    return false;
            //}


            var RestAlarmPointList = await _mRestorationAlarmList.GetRestorationAlarmListTimeAscAsync();
            if(RestAlarmPointList == null)
            {
                Console.WriteLine("DBConnection Error");
                _mAlarmList.nLastAlarmRecIndex = -1; // Reset to Ready State for Start Next time when DB Connected
                return false;
            }

            var LastRestAlarmPoint = RestAlarmPointList.LastOrDefault(); //Get Last Record

            var LastCsvItem = _mAlarmList.ListAlarm.FirstOrDefault(); //Get First CSV Item
            this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
               
            if (RestAlarmPointList.Count == 0 ||
                LastRestAlarmPoint.DateTime < LastCsvItem.Time) //Empty Database
            {
                //Refresh Point from Database
                _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

                if (_DigitalPointInfoList == null)
                    return false;                         //Can't Access Database

                return await Task.Run(() => ProcessPoint(_mAlarmList.nStartIndex, args));
            }


            //Step 1 Compare Last DigitalPointInfoList Point
            int StartIndex = 0;
            AlarmList StartPoint = null;

            foreach (var item in _mAlarmList.ListAlarm)
            {
                StartIndex++;

                if (item.StationName.Trim() != LastRestAlarmPoint.StationName.Trim())
                    continue;
                if (item.PointName.Trim() != LastRestAlarmPoint.PointName.Trim())
                    continue;
                if (item.Time == LastRestAlarmPoint.DateTime)
                {
                    StartPoint = item; //Start Position
                    break;
                }

            }

            if (StartPoint == null)     //No Match point
                StartIndex = -1;

            //Refresh Point from Database
            _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

            if (_DigitalPointInfoList == null)
                return false;                         //Can't Access Database
            return await Task.Run(() => ProcessPoint(StartIndex, args));

        }

        private async Task<bool> onHasNewAlarm(RestEventArgs args)
        {

            if (!isDBConnected())
            {
                Console.WriteLine("DBConnection Error");
                _mAlarmList.nLastAlarmRecIndex = -1; // Reset to Ready State for Start Next time when DB Connected
                return false;
            }

            //Refresh Point from Database
            _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

            if (_DigitalPointInfoList == null)
                return false;                         //Can't Access Database

            return await Task.Run(() => ProcessPoint(_mAlarmList.nStartIndex, args));

        }

        private bool ProcessPoint(int StatartIndex, RestEventArgs args)
        {
            this._nNewRestPoint = 0;

            this.RestAlarmList.Clear();

            if (_flgMatchingInProgress == true)
                return false;

            _flgMatchingInProgress = true;

            for (int iIndex = StatartIndex + 1; iIndex < _mAlarmList.ListAlarm.Count; iIndex++)
            {
                AlarmList al = _mAlarmList.ListAlarm[iIndex];
                if (al.pointType != PointType.Digital) continue;

                var groupByStations = _DigitalPointInfoList.Where(c => c.StationName.Trim() == al.StationName.Trim());// Groupong Station before Mapping

                RestorationAlarmList point = _mAlarmList.GetRestorationAlarmPoint(al, groupByStations); //Mapping CSV Point --> DigitalPointInfo Table

                if (point != null)
                {
                    try
                    {
                        this.RestAlarmList.Add(point);
                        _nNewRestPoint++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());

                    }

                }
            }

            //_mAlarmList.nStartIndex = _mAlarmList.ListAlarm.Count - 1; // Index start with 0
            //this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
            

            try
            {
                _mRestorationAlarmList.RestAlarmContext.RestorationAlarmList.AddRange(this.RestAlarmList);
                _mRestorationAlarmList.Complete();

                args.message = args.message + " " + (_mAlarmList.ListAlarm.Count - StatartIndex - 1).ToString() + $" New Alarm(s), Has { _nNewRestPoint} Restoration Alarm(s)";

                UpdateActivityMonitor(args, "Activity");

                _mAlarmList.nStartIndex = _mAlarmList.ListAlarm.Count - 1; // Index start with 0
                _mAlarmList.nLastAlarmRecIndex = (int)_mAlarmList.ListAlarm[_mAlarmList.ListAlarm.Count - 1].RecIndex; //Update LastAlarm Index
                this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
                Console.WriteLine("Current Index : " + nLastAlarmRecIndex);

                Console.WriteLine($"{DateTime.Now.ToString()} : Finish Matching--> Has {_nNewRestPoint} Restoration Alarm(s)");

                

                //_mAlarmList.ListAlarm.Clear(); //Clear Data after using
            }
            catch
            {
                Console.WriteLine("Error on Save to DB");
            }
            finally
            {
                _flgMatchingInProgress = false;
            }

            
            //updateLogConsole((int)EventLogPosition.REST_NEW_POINT, $"Has {_nNewRestPoint} Restoration Alarm(s)");

            return true;
        }

        private async void OnDBChanged(object source, RestEventArgs args)
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

                case "Connected":
                    //UpdateActivityMonitor(args, "Activity");
                    UpdateActivityStatus(args, "DBStatus", true);//Update CSV File Status
                    break;
                case "Disconnected":
                    //UpdateActivityMonitor(args, "Activity");
                    UpdateActivityStatus(args, "DBStatus", false);//Update Database File Status
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");
                    break;
            }

        }

        private void UpdateActivityMonitor(RestEventArgs args, string _target)
        {
            EventChangedEventArgs LogArg = new EventChangedEventArgs();
            LogArg.TimeStamp = args.TimeStamp;
            LogArg.Message = args.message;
            LogArg.Target = _target;

            onUpdateActivityMonitor(LogArg);
        }

        private async void UpdateActivityStatus(RestEventArgs args, string _target, bool state)
        {
            EventChangedEventArgs LogArg = new EventChangedEventArgs();
            LogArg.TimeStamp = args.TimeStamp;
            LogArg.Message = args.message;
            LogArg.Target = _target;

            var RestAlarmPointList = await _mRestorationAlarmList.GetRestorationAlarmListTimeAscAsync();
            RestorationAlarmList LastRestAlarmPoint = null;

            if (RestAlarmPointList != null)
                LastRestAlarmPoint = RestAlarmPointList.LastOrDefault(); //Get Last Record
            
            var LastCsvItem = _mAlarmList.ListAlarm.LastOrDefault(); //Get First CSV Item

            if (_target == "CSVStatus")
            {
                LogArg.ConnStatus = new ConnectionStatus()
                {
                    LastModified = _mAlarmList.CSVLastModify,
                    Status = state,
                    Info = _mAlarmList.CSVFile,
                    LastRec = LastCsvItem != null ? (LastCsvItem.Time.ToString() + " : " + LastCsvItem.PointName) : "Non"

                    //LastCsvItem = null;
                };
            }
            else if (_target == "DBStatus")
            {
                //var db = new RestorationAlarmDbContext();

                LogArg.ConnStatus = new ConnectionStatus()
                {
                    LastModified = args.TimeStamp,
                    Status = state,
                    Info = _mRestorationAlarmList.RestAlarmContext.Database.Connection.DataSource.ToString(),

                    LastRec = LastRestAlarmPoint != null ? (LastRestAlarmPoint.DateTime.ToString() + " : " + LastRestAlarmPoint.ShortName) : "Non"
                };
            }
            onUpdateActivityMonitor(LogArg);
        }

    }
}
