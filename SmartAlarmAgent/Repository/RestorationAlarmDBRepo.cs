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
            RestEventArgs args = new RestEventArgs();

            try
            {
                args.message = "Read DB Success";
                args.TimeStamp = DateTime.Now;

                return await _RestAlarmContext.DigitalPointInfo
                    .Where(c => !(string.IsNullOrEmpty(c.MACName)))
                    .ToListAsync<DigitalPointInfo>();
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

        public async Task<List<RestorationAlarmList>> GetRestorationAlarmListTimeAscAsync()
        {
            RestEventArgs args = new RestEventArgs();

            try
            {
                args.message = "Read DB Success";
                args.TimeStamp = DateTime.Now;

                return await _RestAlarmContext.RestorationAlarmList
                    .OrderBy(c => c.DateTime)
                    .ToListAsync();

                //var LastRestAlarmPoint = _RestAlarmContext.RestorationAlarmList.LastOrDefault();
                //return LastRestAlarmPoint;
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

            var iState = RestAlarmContext.Database.Exists();
            if (iState)
                args.message = "Connected";
            else
                args.message = "Disconnected";
                
            args.TimeStamp = DateTime.Now;
            onRestAlarmDBChanged(args); //Raise the Event

            return iState;
        }
    

        public void Complete()
        {
            RestAlarmContext.SaveChanges();
        }


        #endregion Methode

    }
}
