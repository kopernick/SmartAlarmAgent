﻿/****
 * 
 *  Code First for Edit DB Migration
 * 
 ****/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmCFData;
using SmartAlarmAgent.Service;
using System.Data.Entity;

namespace SmartAlarmAgent.Repository
{
    class RestAlarmDBRepo
    {

        #region Properties

        private RestAlarmDbContext _RestAlarmContext;
        public RestAlarmDbContext RestAlarmContext
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
        public event EventHandler<RestEventArgs> RestAlarmDBChanged;
        private void onRestAlarmDBChanged(RestEventArgs arg)
        {
            if (RestAlarmDBChanged != null)
                RestAlarmDBChanged(null, arg);
        }
        #endregion Event & Delegate

        #region Constructor
        public RestAlarmDBRepo()
        {
            this._RestAlarmContext = new RestAlarmDbContext();
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

                return await _RestAlarmContext.DigitalPointInfoes
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

        public void Complete()
        {
            _RestAlarmContext.SaveChanges();
        }


        #endregion Methode
    }
}
