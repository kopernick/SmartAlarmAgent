using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmAgent.Model;
using SmartAlarmAgent.Service;
using System.IO;

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
        public delegate void ReadAlarmListDelegate(object source, EventChangedArgs args);
        public event ReadAlarmListDelegate ReadAlarmListCSV;
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
        public async Task<bool> GetAlarmListAsync()
        {
            return await Task.Run(() => exeGetAlarmListCSV());
        }

        private bool exeGetAlarmListCSV()
        {
            EventChangedArgs args = new EventChangedArgs();

            this._listAlarm.Clear();
            try
            {
                string csvFile = @"\\10.20.86.210\ExportDB\AlarmList.csv";


#if true
                this._listAlarm = File.ReadLines(csvFile)
                            .Skip(1)
                            .Select(line => AlarmList.GetLineAlarmListCsv(line))
                            .ToList();

                this.dLastLoadDB = DateTime.Now;
                args.Status = "Read Success";
                args.TimeStamp = this.dLastReadCSV;
                this.dLastReadCSV = args.TimeStamp;
                onReadAlarmListCSV(args); //Raise the Event
              
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
                args.Status = "Read Not Success";
                args.TimeStamp = DateTime.Now;
                    //this.m_dLastReadCSV = args.TimeStamp;
                onReadAlarmListCSV(args); //Raise the Event
                return false;
            }
        }


        private bool IsFileLocked(string path)
        {
            FileInfo file = new FileInfo(path);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read);

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

        protected virtual void onReadAlarmListCSV(EventChangedArgs args)
        {
            ReadAlarmListCSV?.Invoke(this, args);
        }
        #endregion Methode
    }
}
