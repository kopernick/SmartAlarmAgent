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
        

        private async void OnAlarmListChanged(object source, RestEventArgs args)
        {
            // throw new NotImplementedException();

            EventChangedEventArgs LogArg = new EventChangedEventArgs();
            switch (args.message)
            {

                case "Read CSV Success":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Success");
                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);

                    onCheckCSVData();
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS, "Read CSV Success");
                    break;

                case "Read CSV Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Fail");
                    //updateLogConsole((int)EventLogPosition.CSV_STATUS , "Read CSV Fail");
                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);

                    break;

                case "Has New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : "
                        + (_mAlarmList.ListAlarm.Count - _mAlarmList.nStartIndex - 1).ToString() + " New Alarm(s)");

                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);

                    await this.onHasNewAlarm();
                    break;

                case "Has No New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : No New Alarm");

                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);
                    break;

                case "Start Process":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Start Insert to DB");

                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);
                    //this.onHasNewAlarm();
                    await this.onStartProcess();
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");

                    LogArg.TimeStamp = args.TimeStamp;
                    LogArg.message = args.message;
                    onUpdateActivityMonitor(LogArg);
                    break;
            }
        }


        private async Task<bool> onStartProcess()
        {
            this._nNewRestPoint = 0;

            this.RestAlarmList.Clear();

            //Refresh Point from Database
            var RestAlarmPointList = await _mRestorationAlarmList.GetRestorationAlarmListAsync();

            if (RestAlarmPointList == null) //Can't Access Database
                return false;

            if (RestAlarmPointList.Count == 0) //Empty Database
            {
                //Refresh Point from Database
                _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

                if (_DigitalPointInfoList == null)
                    return false;                         //Can't Access Database

                return await Task.Run(() => ProcessPoint(_mAlarmList.nStartIndex));
            }

            var LastRestAlarmPoint = RestAlarmPointList.FirstOrDefault();

            if (LastRestAlarmPoint == null)
                return false;

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
            return await Task.Run(() => ProcessPoint(StartIndex));

        }

        private async Task<bool> onHasNewAlarm()
        {
            //Refresh Point from Database
            _DigitalPointInfoList = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();

            if (_DigitalPointInfoList == null)
                return false;                         //Can't Access Database

            return await Task.Run(() => ProcessPoint(_mAlarmList.nStartIndex));

        }

        private bool ProcessPoint(int StatartIndex)
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

            _mAlarmList.nStartIndex = _mAlarmList.ListAlarm.Count - 1; // Index start with 0
            this.nLastAlarmRecIndex = _mAlarmList.nLastAlarmRecIndex;  //Update LastAlarmRecIndex Display
            Console.WriteLine("Current Index : " + nLastAlarmRecIndex);

            _mRestorationAlarmList.RestAlarmContext.RestorationAlarmList.AddRange(this.RestAlarmList);
            _mRestorationAlarmList.Complete();

            _flgMatchingInProgress = false;

            Console.WriteLine($"{DateTime.Now.ToString()} : Finish Matching Point");

            Console.WriteLine($"Has {_nNewRestPoint} Restoration Alarm(s)");
            //updateLogConsole((int)EventLogPosition.REST_NEW_POINT, $"Has {_nNewRestPoint} Restoration Alarm(s)");

            return true;
        }
    }
}
