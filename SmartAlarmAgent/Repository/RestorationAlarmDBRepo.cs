using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmData;
using System.Data.Entity;
using SmartAlarmAgent.Service;
using SmartAlarmAgent.Model;

namespace SmartAlarmAgent.Repository
{
     class RestorationAlarmDBRepo
    {
        #region Properties

        private RestorationAlarmDbContext _RestAlarmContext;
        public RestorationAlarmDbContext RestAlarmContext
        {
            get
            {
                return _RestAlarmContext;
            }
            set
            {
                _RestAlarmContext = value;
            }
        }

        public DateTime dLastLoadDB { get; set; }

        #endregion Properties

        #region Event & Delegate
        public static  event EventHandler<RestEventArgs> RestAlarmDBChanged;
        private void onRestAlarmDBChanged(RestEventArgs arg)
        {
            if (RestAlarmDBChanged != null)
                RestAlarmDBChanged(null, arg);
        }
        #endregion Event & Delegate

        #region Constructor
        public RestorationAlarmDBRepo()
        {
            this._RestAlarmContext = new RestorationAlarmDbContext();
        }

        #endregion Constructor

        #region Methode
        public async Task<List<DigitalPointInfo>> GetAllDigitalPointInfoAsync()
        {

            try
            {

                Console.WriteLine(DateTime.Now.ToString() + " : GetAllDigitalPointInfo Read DB Success");

                return await _RestAlarmContext.DigitalPointInfo
                    .Where(c => !(string.IsNullOrEmpty(c.MACName)))
                    .ToListAsync<DigitalPointInfo>();
            }
            catch
            {

                Console.WriteLine(DateTime.Now.ToString() + " : GetAllDigitalPointInfo Read DB Fail");
                return null;
                
            }

            
        }

        public async Task<List<RestorationAlarmList>> GetRestorationAlarmListTimeDscAsync()
        {
            RestEventArgs args = new RestEventArgs();

            try
            {
                args.message = "Read DB Success";
                args.TimeStamp = DateTime.Now;

                return await _RestAlarmContext.RestorationAlarmList
                    .OrderByDescending(c => c.DateTime)
                    .Take(1)
                    .ToListAsync< RestorationAlarmList>();
            }
            catch
            {
                args.message = "Read DB Fail";
                args.TimeStamp = DateTime.Now;
                return null;

            }
            finally
            {
                //Send Event after Get All DigitalInfo
                onRestAlarmDBChanged(args); //Raise the Event

            }
        }

      
        public bool GetDBStatus()
        {
            RestEventArgs args = new RestEventArgs();

            var isExist = RestAlarmContext.Database.Exists();
            if (isExist)
                args.message = "Connected";
            else
                args.message = "Disconnected";
                
            args.TimeStamp = DateTime.Now;
            onRestAlarmDBChanged(args); //Raise the Event

            return isExist;
        }
    

        public void Complete()
        {
            RestAlarmContext.SaveChanges();
        }


        #endregion Methode

    }
}
