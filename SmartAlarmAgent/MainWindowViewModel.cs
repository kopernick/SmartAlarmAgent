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
        private DispatcherTimer m_dispatcherTimerClock = new System.Windows.Threading.DispatcherTimer();

        public DataProcessLogic DataProcessor { get; private set; }

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

        public string _Time;
        public string Time
        {
            get { return _Time; }
            set
            {
                _Time = value;
                OnPropertyChanged("Time");
            }
        }

        #endregion Properties

        #region Constructor
        public MainWindowViewModel()
        {
            
            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            DataProcessor = new DataProcessLogic();

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

            this.m_dispatcherTimerClock.Interval = new TimeSpan(0, 0, 1); //Get Update Database Period
            this.m_dispatcherTimerClock.Start();
            this.m_dispatcherTimerClock.Tick += dispatcherTimerClock_Tick;
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

        private void dispatcherTimerClock_Tick(object sender, EventArgs e)
        {
            Time = DateTime.Now.ToLongTimeString();
                
        }

        #endregion Methode
    }
}
