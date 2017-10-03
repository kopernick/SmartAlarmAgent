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
using System.Windows.Input;
using SmartAlarmAgent.Views;

namespace SmartAlarmAgent
{
     class MainWindowViewModel : PropertyChangeEventBase
    {
        #region Properties

        private DispatcherTimer m_dispatcherTimerCSV = new System.Windows.Threading.DispatcherTimer();
        private DispatcherTimer m_dispatcherTimerClock = new System.Windows.Threading.DispatcherTimer();

        private ConnectionConfig _connCfg = new ConnectionConfig();
        public DataProcessLogic DataProcessor { get; private set; }

        public ConnSetting ConnSettingView; //Child window:MainWindow
        public ICommand RunConnSetting { get; private set; }

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
            _connCfg.LoadConfiguration();
            _flgMatchingInProgress = false;
            this._nNewRestPoint = 0;
            DataProcessor = new DataProcessLogic(this._connCfg);
            RunConnSetting = new RelayCommand(o => ConnSetting(), o => canConnSetting());
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

        #region Helper

        private bool canConnSetting()
        {
            return true;
        }

        private void ConnSetting()
        {
            //TODO

#if false
            Thread t = new Thread(StartConnSetting);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

#else
            Task t = new Task(StartConnSetting);
            t.RunSynchronously();
#endif



        }

        private void StartConnSetting()
        {

            ConnSettingView = new ConnSetting(this._connCfg); //Init Sort Condition By Create Instance SortFieldView

            ConnSettingView.ShowInTaskbar = true;

            ConnSettingView.ShowDialog();
            if (ConnSettingView.DialogResult == ConnDialogResult.OK ||
                ConnSettingView.DialogResult == ConnDialogResult.APPLY)
            {
                this._connCfg.SaveConfiguration();

                DataProcessor.ConnCfg = this._connCfg;
                _flgMatchingInProgress = false;
               
                DataProcessor.RefreshConnect( this._connCfg); //Refresh Connection Setting
               
                Console.WriteLine("ConnSetting was Saved");
            }
            else
            {
                Console.WriteLine("ConnSetting was Canceled");
            }
        }
        #endregion Helper
    }
}
