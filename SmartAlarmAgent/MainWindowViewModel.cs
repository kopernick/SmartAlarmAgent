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

        //public AlarmListCSVRepo mAlarmList
        //{
        //    get { return _mAlarmList; }
        //}

        private static RestorationAlarmDBRepo _mRestorationAlarmList;
        
        #endregion Properties

        #region Constructor
        public MainWindowViewModel()
        {
            _mAlarmList = new AlarmListCSVRepo();
            _mRestorationAlarmList = new RestorationAlarmDBRepo();
            _mAlarmList.ReadAlarmListCSV += OnEventChanged;

            Test_LoadedAsync();
            Test_DBLoadAsync();
            Console.WriteLine("Skip");

        }


        #endregion Constructor

        #region Methode
        private async void Test_LoadedAsync()
        {
            var x = await _mAlarmList.GetAlarmListAsync();
            Console.WriteLine($"Can read Alarm List ? : {x.ToString()}");
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
           var Stations = await _mRestorationAlarmList.GetSationNameAsync();
           Console.WriteLine($"Can read Restoration Alarm List ? : {Stations.Count.ToString()}");

            foreach (var item in Stations)
            {
                Console.WriteLine(item.PkDigitalID + "," + item.StationName.TrimEnd() + ":" + item.PointName + " " + item.MACName);
            }

        }

        private void OnEventChanged(object source, EventChangedArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion Methode
    }
}
