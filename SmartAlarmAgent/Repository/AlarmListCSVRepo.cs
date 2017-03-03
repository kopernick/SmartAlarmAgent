using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmAgent.Model;
using SmartAlarmAgent.Service;
using System.IO;
using System.Threading;

namespace SmartAlarmAgent.Repository
{
     class AlarmListCSVRepo
    {
        
        #region Properties
        private int _nLastAlarmRecIndex;
        public int nLastAlarmRecIndex
        {
            get { return _nLastAlarmRecIndex; }
        }
        private int _nNewRestPoint;
        public int nNewRestPoint
        {
            get { return _nNewRestPoint; }
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
            this._nNewRestPoint = 0;
            this._nLastRestAlarmID = 0;

            this._listAlarm = new List<AlarmList>();

        }

        #endregion Constructor

        #region Methode
        public async Task<bool> GetInitAlarmListAsync()
        {
            return await Task.Run(() => exeGetInitAlarmListCSV());
        }

        private bool exeGetInitAlarmListCSV()
        {
            RestEventArgs args = new RestEventArgs();

            this._listAlarm.Clear();

            try
            {
                string csvFile = @"\\10.20.86.210\ExportDB\AlarmList2.csv";


#if true
                int nRetry_read = 0;

                do //Retry tor Read CSV File 
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (nRetry_read++ >= 10) break;
                } while (IsFileLocked(csvFile));

                this._listAlarm = File.ReadLines(csvFile)
                            .Skip(1)
                            .Select(line => AlarmList.GetLineAlarmListCsv(line))
                            .ToList();

                this.dLastReadCSV = DateTime.Now;
                args.message = "Read CSV Success";
                args.TimeStamp = this.dLastReadCSV;
                onRestAlarmCSVChanged(args); //Raise the Event
              
                return true;
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
                return false;
            }
        }

        public async Task<bool> GetNewAlarmListAsync()
        {
            return await Task.Run(() => exeGetNewtAlarmListCSV());
        }
        private bool exeGetNewtAlarmListCSV()
        {
            //int iStartIndex = 0;
            //this.m_nNewRestPoint = 0;

            //if (this.m_nLastAlarmRecIndex >= 0) //If Start program m_nLastAlarmRecIndex is set to -1 see also Alarm4Restoration()
            //{
            //    for (int iIndex = 0; iIndex < this.m_listAlarm.Count; iIndex++)
            //    {
            //        DigitalAlarm al = this.m_listAlarm[iIndex];
            //        if (al.RecIndex != this.m_nLastAlarmRecIndex) continue;
            //        iStartIndex = (iIndex) % this.m_listAlarm.Count;  // New Incoming Alarm Star here.
            //        break;
            //    }
            //}

            return true;
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
            catch (IOException)
            {
                return this._bFlgFileIsLocked = true; //File is Locked
            }
            finally
            {
                if (stream != null) stream.Dispose();

            }

            return this._bFlgFileIsLocked = false; //File isn't Locked
        }

        #endregion Methode
    }
}
