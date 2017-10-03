﻿/****
 * 
 *  DB First for Programming
 * 
 ****/

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

        private ConnectionConfig _connCfg;
        public ConnectionConfig ConnCfg
        {
            get
            {
                return _connCfg;
            }
            set
            {
                _connCfg = value;
            }
        }

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
        public event EventHandler<RestEventArgs> RestAlarmDBChanged;
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

        //Constructor overload
        public RestorationAlarmDBRepo(ConnectionConfig connCfg)
        {
            this._connCfg = connCfg;

            this._RestAlarmContext = new RestorationAlarmDbContext(GetSQLConnString());
        }

        #endregion Constructor

        #region Methode

        public string GetSQLConnString()
        {
            var sql = @"metadata=res://*/RestorationAlarmModel.csdl|res://*/RestorationAlarmModel.ssdl|res://*/RestorationAlarmModel.msl;" +
           @"provider = System.Data.SqlClient;" +
           @"provider connection string=""" +
           "data source = "+ _connCfg.Server + ";" +
          "initial catalog = " + _connCfg.Database + ";" +
          "user id = " + _connCfg.Login + ";" +
          "password = " + _connCfg.Password + ";" +
           @"multipleactiveresultsets = True;" +
           @"App = EntityFramework ;" +
            //@"providerName = " + @"/"System.Data.EntityClient/"" +
           @"""";

            return sql;

        }

        public async Task<List<DigitalPointInfo>> GetAllDigitalPointInfoAsync()
        {

            try
            {
                //ToDo Reconnect Db
                //this._RestAlarmContext = RestorationAlarmDbContext(cnString);

                Console.WriteLine(DateTime.Now.ToString() + " : GetAllDigitalPointInfo Read DB Success");

                return await this._RestAlarmContext.DigitalPointInfo
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

                return await this._RestAlarmContext.RestorationAlarmList
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

            var isExist = this._RestAlarmContext.Database.Exists();
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
            this._RestAlarmContext.SaveChanges();
        }


        #endregion Methode

    }
}
