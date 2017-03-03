using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmData;
using SmartAlarmAgent.Repository;
using SmartAlarmAgent.Model;
using SmartAlarmAgent.Service;

namespace SmartAlarmAgent
{
    public class MainWindowViewModel
    {
        #region Prooerties

        private List<DigitalPointInfo> _DigitalPointInfoList;
        public List<DigitalPointInfo> DigitalPointInfoList
        {
            get { return _DigitalPointInfoList; }
        }

        private static AlarmListCSVRepo _mAlarmList;

        public AlarmListCSVRepo mAlarmList
        {
            get { return _mAlarmList; }
        }

        private static RestorationAlarmDBRepo _mRestorationAlarmList;
        
        #endregion Properties

        #region Constructor
        public MainWindowViewModel()
        {
            _mAlarmList = new AlarmListCSVRepo();
            _mRestorationAlarmList = new RestorationAlarmDBRepo();

            _mAlarmList.RestAlarmCSVChanged += OnAlarmListChanged;
            _mRestorationAlarmList.RestAlarmDBChanged += OnDBChanged;

            Test_LoadedAsync();
            Test_DBLoadAsync();
            Console.WriteLine("Skip");

        }


        #endregion Constructor

        #region Methode
        private async void Test_LoadedAsync()
        {
            var x = await _mAlarmList.GetAlarmListAsync();
            //Test cw
            var Station = _mAlarmList.ListAlarm
                    .Where(c => c.StationName.Contains("BK"))
                    //.Where(c => !(string.IsNullOrEmpty(c.MACName))) //same as.Where(c=>!(c.MACName == "" || c.MACName == null)) 
                    //.Where(c=>c.MACName != "" && c.MACName != null)
                    .Take(20);

            foreach (var item in Station)
            {
                Console.WriteLine(item.Time + "," + item.StationName.TrimEnd() + ":" + item.PointName + " " + item.Message);
            }
        }

        private async void Test_DBLoadAsync() 
        {
           var Stations = await _mRestorationAlarmList.GetAllDigitalPointInfoAsync();
            if (Stations == null) return;
           Console.WriteLine($"Can read Restoration Alarm List ? : {Stations.Count.ToString()}");

            foreach (var item in Stations.Take(20))
            {
                Console.WriteLine(item.PkDigitalID + "," + item.StationName.TrimEnd() + ":" + item.PointName + " " + item.MACName);
            }

        }

        private void OnAlarmListChanged(object source, RestEventArgs args)
        {
            // throw new NotImplementedException();
            switch (args.message)
            {

                case "Read CSV Success":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Success");
                    break;

                case "Read CSV Fail":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Read AlarmList.csv Fail");
                    break;

                case "Has New Alarm":
                    Console.WriteLine(args.TimeStamp.ToString() + " : New Alarm");
                    break;

                case "Reset":
                    Console.WriteLine(args.TimeStamp.ToString() + " : Reset CSV");
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Main Alarm List No Msg. match");

                    break;
            }
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
            
        #endregion Methode
    }
}
