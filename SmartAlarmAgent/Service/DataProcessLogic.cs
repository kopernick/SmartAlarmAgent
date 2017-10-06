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
using System.Windows.Media;

namespace SmartAlarmAgent.Service
{
    public class DataProcessLogic : PropertyChangeEventBase
    {

        #region Properties

        private ConnectionConfig _connCfg;
        public ConnectionConfig ConnCfg
        {
            get
            {
                return _connCfg;
            }
            set
            {
                _connCfg = value;
                OnPropertyChanged(nameof(ConnCfg));
            }
        }

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
                OnPropertyChanged(nameof(nLastAlarmRecIndex));
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
                OnPropertyChanged(nameof(CSVFile));
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
                OnPropertyChanged(nameof(CSVStatus));
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
                OnPropertyChanged(nameof(CSVLastModify));
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
                OnPropertyChanged(nameof(DBName));
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
                OnPropertyChanged(nameof(DBLastAccess));
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
                OnPropertyChanged(nameof(CSVLastAlarm));
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
                OnPropertyChanged(nameof(DBSLastRec));
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
                OnPropertyChanged(nameof(DBStatus));
            }
        }

        private Brush _CSVBackgroundColor;
        public Brush CSVBackgroundColor
        {
            get { return _CSVBackgroundColor; }
            set
            {
                _CSVBackgroundColor = value;
                OnPropertyChanged(nameof(CSVBackgroundColor));
            }
        }

        private Brush _DBBackgroundColor;
        public Brush DBBackgroundColor
        {
            get { return _DBBackgroundColor; }
            set
            {
                _DBBackgroundColor = value;
                OnPropertyChanged(nameof(DBBackgroundColor));
            }
        }

        private readonly List<RestorationAlarmList> RestAlarmList = new List<RestorationAlarmList>();

        private static AlarmListCSVRepo _mAlarmList;
        public AlarmListCSVRepo mAlarmList
        {
            get { return _mAlarmList; }
            set {  _mAlarmList = value; }
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
        public RestorationAlarmList LastRestAlarmPoint { get; private set; }

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
        public DataProcessLogic(ConnectionConfig connCfg)
        {
            _connCfg = connCfg;

            _mAlarmList = new AlarmListCSVRepo(_connCfg);
            _mRestorationAlarmList = new RestorationAlarmDBRepo(_connCfg);

            //_mRestAlarmList = new RestAlarmDBRepo();                //Test EF

            //AlarmListCSVRepo.RestAlarmCSVChanged += OnAlarmListChanged;
            _mAlarmList.RestAlarmCSVChanged += OnAlarmListChanged;

            //RestorationAlarmDBRepo.RestAlarmDBChanged += OnDBChanged;
            _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged;

            LastRestAlarmPoint = null;

            // _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged;

            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            this.nLastAlarmRecIndex = -1;

        }
        #endregion Constructor

        #region Methode

        public void RefreshConnection(ConnectionConfig connCfg)
        {
            _connCfg = connCfg;

            //Set Connection for CSV Repo [ Change only file Info] 
            _mAlarmList.ConnCfg = _connCfg; 

            //Re Create Database Repository [Can not set by only Connection string]
            _mRestorationAlarmList = new RestorationAlarmDBRepo(_connCfg);  
            _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged; //Subcribe to RestAlarmDBChanged Event

            LastRestAlarmPoint = null;

            _mAlarmList.nLastAlarmRecIndex = -1; //Start Mode
            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            this.nLastAlarmRecIndex = -1;

            _mAlarmList.CSVLastModify = DateTime.Now.AddYears(-1); //Reset CSV file's Last Mod date

            GetCSVData(); //Restart Get CSV data

        }

        #endregion Methode 

        #region Helper

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
                    onCheckCSVData();
                    UpdateConnectionStatus(args, "CSVStatus", true);//Update CSV File Status

                    break;

                case "Read CSV Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Fail");
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS , "Read CSV Fail");
                    UpdateActivityMonitor(args, "Activity");
                    UpdateConnectionStatus(args, "CSVStatus", false);//Update CSV File Status

                    break;

                case "Has New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : "
                        + (_mAlarmList.ListAlarm.Count - _mAlarmList.nStartIndex - 1).ToString() + " New Alarm(s)");
                    await this.onHasNewAlarm(args);
                    break;

                case "No New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : No New Alarm");
                    UpdateActivityMonitor(args, "Activity");

                    break;

                case "Start Process":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Start Data Processing");
                    await this.onStartProcess(args);
                    //UpdateActivityMonitor(args, "Activity");
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

            if (!isDBConnected())
            {
                Console.WriteLine("DBConnection Error");
                _mAlarmList.nLastAlarmRecIndex = -1; // Reset to Ready State for Start Next time when DB Connected

                UpdateConnectionStatus(args, "DBStatus", false);//Update Database File Status
                return false;
            }


            var RestAlarmPointList = await _mRestorationAlarmList.GetRestorationAlarmListTimeDscAsync(); //Read RestorationAlarmList Table from DB
            if (RestAlarmPointList != null)
            {
                LastRestAlarmPoint = RestAlarmPointList.FirstOrDefault(); //Get Last Record
            }

            var LastCsvItem = _mAlarmList.ListAlarm.FirstOrDefault(); //Get First CSV Item
            this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
               
            if (RestAlarmPointList.Count == 0 ||
                LastRestAlarmPoint.DateTime < LastCsvItem.Time) //Empty Database
            {
                //Refresh Point from Database
                _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

                if (_DigitalPointInfoList == null)
                    return false;                         //Can't Access Database

                return await ProcessPoint(_mAlarmList.nStartIndex, args);
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

           // UpdateConnectionStatus(args, "DBStatus", true);//Update Database File Status
            return await ProcessPoint(StartIndex, args);

        }

        private async Task<bool> onHasNewAlarm(RestEventArgs args)
        {

            if (!isDBConnected())
            {
                Console.WriteLine("DBConnection Error");
                _mAlarmList.nLastAlarmRecIndex = -1; // Reset to Ready State for Start Next time when DB Connected
                UpdateConnectionStatus(args, "DBStatus", false);//Update Database File Status
                return false;
            }

            //Refresh Point from Database
            _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

            if (_DigitalPointInfoList == null)
                return false;                         //Can't Access Database

            //UpdateConnectionStatus(args, "DBStatus", true);//Update Database File Status
            return await ProcessPoint(_mAlarmList.nStartIndex, args);

        }

        private async Task<bool> ProcessPoint(int StartIndex, RestEventArgs args)
        {
            this._nNewRestPoint = 0;

            this.RestAlarmList.Clear();

            if (_flgMatchingInProgress == true)
                return false;

            _flgMatchingInProgress = true;

            for (int iIndex = StartIndex + 1; iIndex < _mAlarmList.ListAlarm.Count; iIndex++)
            {
                AlarmList al = _mAlarmList.ListAlarm[iIndex];
                if (al.pointType != PointType.Digital) continue;

                var groupByStations = _DigitalPointInfoList.Where(c => c.StationName.Trim() == al.StationName.Trim());// Grouping Station before Mapping

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

                var RestAlarmPointList = await Task.Run(() => _mRestorationAlarmList.GetRestorationAlarmListTimeDscAsync()); //Read RestorationAlarmList Table from DB
                if (RestAlarmPointList != null)
                {
                    LastRestAlarmPoint = RestAlarmPointList.FirstOrDefault(); //Get Last Record
                }
                args.message = args.message + " " + (_mAlarmList.ListAlarm.Count - StartIndex - 1).ToString() + $" Alarm(s),:=> { _nNewRestPoint} Restoration Alarm(s)";

                _mAlarmList.nStartIndex = _mAlarmList.ListAlarm.Count - 1; // Index start with 0
                _mAlarmList.nLastAlarmRecIndex = (int)_mAlarmList.ListAlarm[_mAlarmList.ListAlarm.Count - 1].RecIndex; //Update LastAlarm Index
                this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
                Console.WriteLine("Current Index : " + nLastAlarmRecIndex);

                Console.WriteLine($"{DateTime.Now.ToString()} : Finish Matching--> Has {_nNewRestPoint} Restoration Alarm(s)");

                UpdateActivityMonitor(args, "Activity"); //Upadte Activity after Dataprocessing
                UpdateConnectionStatus(args, "DBStatus", true);//Update Database Status

                //_mAlarmList.ListAlarm.Clear(); //Clear Data after using
            }
            catch
            {
                Console.WriteLine("Error while Inser to DB");
                args.message = "Error while inser to DB";
                UpdateActivityMonitor(args, "Activity");
                return true;
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
                    UpdateConnectionStatus(args, "DBStatus", true);//Update CSV File Status
                    break;
                case "Disconnected":
                    //UpdateActivityMonitor(args, "Activity");
                    UpdateConnectionStatus(args, "DBStatus", false);//Update Database File Status
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

        private void UpdateConnectionStatus(RestEventArgs args, string _target, bool state)
        {
            //EventChangedEventArgs LogArg = new EventChangedEventArgs();
            
            var LastCsvItem = _mAlarmList.ListAlarm.LastOrDefault(); //Get First CSV Item

           if (_target == "CSVStatus")
            {
                CSVLastModify = _mAlarmList.CSVLastModify;
                CSVStatus = state ? "Connection OK" : "Connection Fail";
                
                CSVFile = this._connCfg.CsvFile;
                CSVLastAlarm = LastCsvItem != null ? (LastCsvItem.Time.ToString() + " : " + LastCsvItem.PointName) : "Non";
                CSVBackgroundColor = state ? Brushes.Green : Brushes.Red;
            }
            else if (_target == "DBStatus")
            {
                //var db = new RestorationAlarmDbContext();
                DBLastAccess = args.TimeStamp;
                DBStatus = state ? "Database Connected" : "Can't Connect to Database";
                
                DBName = this._connCfg.Server + ": " + this._connCfg.Database;
                DBSLastRec = LastRestAlarmPoint != null ? (LastRestAlarmPoint.DateTime.ToString() + " : " + LastRestAlarmPoint.ShortName) : "Non";
                DBBackgroundColor = state ? Brushes.Green : Brushes.Red;
            }
        }
#endregion Helper

    }
}
