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

        private DataProcessLogic DataProcessor = new DataProcessLogic();

        private int _nLastAlarmRecIndex;
        public int nLastAlarmRecIndex
        { get
            {
                return _nLastAlarmRecIndex;
            }
            set
            {
                _nLastAlarmRecIndex = value;
                //OnPropertyChanged("nLastAlarmRecIndex");
            }
        }

       

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
            
            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;

            Initializer();
            Console.WriteLine("Skip");
        }
        #endregion Constructor

       
        
        #region Methode

        private async void Initializer()
        {
            await Task.Run(() => DataProcessor.GetCSVData());

            this.m_dispatcherTimerCSV.Interval = new TimeSpan(0, 0, 30); //Get Update CSV File Period
            this.m_dispatcherTimerCSV.Start();
            this.m_dispatcherTimerCSV.Tick += dispatcherTimerCSV_Tick;

            //this.m_dispatcherTimerDB.Interval = new TimeSpan(1, 0, 0); //Get Update Database Period
            //this.m_dispatcherTimerDB.Start();
            //this.m_dispatcherTimerDB.Tick += dispatcherTimerDB_Tick;
            this.nLastAlarmRecIndex = DataProcessor.nLastAlarmRecIndex;

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
            if (DataProcessor.flgStart == true)
            {
                Console.WriteLine("Under Matching Point Process");
                return;
            }
            await Task.Run(() => DataProcessor.GetCSVData());
        }

        private void dispatcherTimerDB_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString() + " : Timer Get Update DB");
        }

        #endregion Methode
    }
}
