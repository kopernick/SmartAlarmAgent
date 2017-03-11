using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmAgent.Model;
using SmartAlarmAgent.Service;
using System.IO;
using System.Threading;
using SmartAlarmData;

namespace SmartAlarmAgent.Repository
{
     class AlarmListCSVRepo
    {
        
        #region Properties
        private int _nLastAlarmRecIndex;
        public int nLastAlarmRecIndex
        {
            get { return _nLastAlarmRecIndex; }
            set { _nLastAlarmRecIndex = value; }
        }

        private DateTime _CSVLastModify;
        public DateTime CSVLastModify
        {
            get { return _CSVLastModify; }
        }
        public DateTime dLastLoadDB { get; set; }
        public DateTime dLastReadCSV { get; set; }
        public DateTime dLastInsertRestAlarm { get; set; }

        private bool _bFlgFileIsLocked;
        public bool bFlgFileIsLocked
        {
            get { return _bFlgFileIsLocked; }
        }
        private int _nLastRestAlarmID;
        public int nLastRestAlarmID
        {
            get { return _nLastRestAlarmID; }
        }

        private int _nStartIndex;
        public int nStartIndex
        {
            get { return _nStartIndex; }
            set
            {
                _nStartIndex = value;
            }
        }

        private List<AlarmList> _listAlarm;

        public List<AlarmList> ListAlarm
        {
            get { return _listAlarm; }
        }

        #endregion Properties

        #region Event & Delegate
        public event EventHandler<RestEventArgs> RestAlarmCSVChanged;
        private void onRestAlarmCSVChanged(RestEventArgs arg)
        {
            if (RestAlarmCSVChanged != null)
                RestAlarmCSVChanged(null, arg);
        }
        #endregion Event & Delegate

        #region Constructor
        public AlarmListCSVRepo()
        {

            this.dLastLoadDB = DateTime.Now;
            this._nLastAlarmRecIndex = -1;
            this._nLastRestAlarmID = 0;
            this._nStartIndex = -1;

            this._listAlarm = new List<AlarmList>();

        }

        #endregion Constructor

        #region Methode
        public async Task<List<AlarmList>> GetInitAlarmListAsync()
        {
            return await Task.Run(() => exeGetInitAlarmListCSV());
        }

        private List<AlarmList> exeGetInitAlarmListCSV()
        {
            RestEventArgs args = new RestEventArgs();

            this._listAlarm.Clear();

            try
            {
                string csvFile = @"\\10.20.86.210\ExportDB\AlarmList.csv";
                // string csvFile = @"c:\ExportDB\AlarmList.csv";


#if true
                FileInfo file = new FileInfo(csvFile);
                var DateModification = file.LastWriteTime;

                if (_CSVLastModify == DateModification)
                {
                    Console.WriteLine("CSV not Update");
                    return null;
                }

                _CSVLastModify = DateModification;
                Console.WriteLine("CSV Last Modification : "+ DateModification);
                
                int nRetry_read = 0;

                do //Retry tor Read CSV File 
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (nRetry_read++ >= 10)
                    {
                        args.message = "Read CSV Fail";
                        args.TimeStamp = DateTime.Now;
                        //this.m_dLastReadCSV = args.TimeStamp;
                        onRestAlarmCSVChanged(args); //Raise the Event
                        return null;
                    }
                } while (IsFileLocked(csvFile));

                dLastReadCSV = DateTime.Now;
                args.message = "Read CSV Success";
                args.TimeStamp = this.dLastReadCSV;
                onRestAlarmCSVChanged(args); //Raise the Event

                return this._listAlarm = File.ReadLines(csvFile)
                            .Skip(1)
                            .Select(line => AlarmList.GetLineAlarmListCsv(line))
                            .ToList();
 
#else
                //Oldcode
                DataTable dt = new DataTable("AlarmList");
                using (TextFieldParser parser = new TextFieldParser(csvFile))
                    {
                        
                            parser.Delimiters = new string[] { "," };
                            int iLine = 0;
                            while (true)
                            {
                                string[] parts = parser.ReadFields();
                                if (parts == null) break;
                                if (iLine++ == 0)
                                {
                                    for (int iCol = 0; iCol < parts.Length; iCol++) dt.Columns.Add(parts[iCol]);
                                    continue;
                                }
                                dt.Rows.Add(parts);
                            }

                            //dt.DefaultView.Sort = "DATETIME";
                            dt = dt.DefaultView.ToTable();

                            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                            {
                                DataRow row = dt.Rows[iRow];
                                DigitalAlarm al = new DigitalAlarm(row);
                                this.m_listAlarm.Add(al);
                            }

                            this.m_dLastReadCSV = DateTime.Now;
                            return true;
                    }


#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                args.message = "Read CSV Fail";
                args.TimeStamp = DateTime.Now;
                //this.m_dLastReadCSV = args.TimeStamp;
                onRestAlarmCSVChanged(args); //Raise the Event
                return null;
            }
        }

        public async Task<bool> GetNewAlarmListAsync()
        {
            if (exeGetInitAlarmListCSV() == null)
                    return false;                   //Can't Read CSV file

            return await Task.Run(() => exeGetNewAlarmListCSV());
        }


        private bool exeGetNewAlarmListCSV()
        {
            RestEventArgs args = new RestEventArgs();

            int iStartIndex = 0;
            
            if (this._nLastAlarmRecIndex >= 0) //If Start program m_nLastAlarmRecIndex is set to -1 see also Alarm4Restoration()
            {
                for (int iIndex = 0; iIndex < this._listAlarm.Count; iIndex++)
                {
                    var al = this._listAlarm[iIndex];
                    if (al.RecIndex != this._nLastAlarmRecIndex) continue;

                    iStartIndex = (iIndex) % this._listAlarm.Count;  // New Incoming Alarm Star here. ListAlarm.Count = 20,000 
                    if (this._nStartIndex == iStartIndex)
                    {
                        args.message = "Has No New Alarm";
                        args.TimeStamp = DateTime.Now;
                        onRestAlarmCSVChanged(args); 

                        break;//Same Position in CSV Has no New Alarm
                    }

                    this._nStartIndex = iStartIndex;
                    this._nLastAlarmRecIndex = (int)this._listAlarm[this._listAlarm.Count-1].RecIndex; //Update LastAlarm Index

                    args.message = "Has New Alarm";
                    args.TimeStamp = DateTime.Now;
                    onRestAlarmCSVChanged(args); //Raise the Event when has new event
                    break;
                }
            }else
            {
                if(this._listAlarm != null || this._listAlarm.Count != 0)
                    this._nLastAlarmRecIndex = (int)this._listAlarm[this._listAlarm.Count-1].RecIndex; //Update LastAlarm Index

                args.message = "Start Process";
                args.TimeStamp = DateTime.Now;
                onRestAlarmCSVChanged(args); //Raise the Event when has new event
            }
            
            return iStartIndex != 0 ;
        }
        private bool IsFileLocked(string path)
        {
            FileInfo file = new FileInfo(path);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read);

            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Error File not found");
                return this._bFlgFileIsLocked = true; //File is Locked
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e.Message);
                return this._bFlgFileIsLocked = true; //File is Locked
            }
            finally
            {
                if (stream != null) stream.Dispose();

            }

            return this._bFlgFileIsLocked = false; //File isn't Locked
        }

        public RestorationAlarmList GetRestorationAlarmPoint(AlarmList al, IEnumerable<DigitalPointInfo> groupByStations)
        {
            var RestorationAlarm = new RestorationAlarmList();

            var pointInfo = groupByStations.Where(c => c.PointName.Trim() == al.PointName.Trim()).FirstOrDefault();

            if (pointInfo == null) return null;

            try
            {
                

                RestorationAlarm.DateTime = al.Time;
                RestorationAlarm.PointType = (Byte)al.pointType;
                RestorationAlarm.FkIndexID = (int)al.FkIndex;
                RestorationAlarm.StationName = al.StationName;
                RestorationAlarm.PointName = al.PointName;
                RestorationAlarm.ShortName = pointInfo.ShortName;
                RestorationAlarm.AlarmType = (int)al.AlarmType;
                RestorationAlarm.Flashing = al.Flashing;
                RestorationAlarm.ActualValue = al.ActualValue;
                RestorationAlarm.Message = al.Message;
                RestorationAlarm.SourceName = al.SourceName;
                RestorationAlarm.SourceID = al.SourceID;
                RestorationAlarm.SourceType = (Byte)al.SourceType;
                RestorationAlarm.AlarmFlag = (Byte)al.AlarmFlag;
                RestorationAlarm.DeviceID = pointInfo.DeviceID;
                RestorationAlarm.DeviceType = pointInfo.DeviceType;
                RestorationAlarm.MACName = pointInfo.MACName;
                RestorationAlarm.Priority = pointInfo.Priority;

                return RestorationAlarm;
            }
            catch
            {
                Console.WriteLine($"Convertion Error");
                return null;
            }
            
        }

        #endregion Methode
    }
}
