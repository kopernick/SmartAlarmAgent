using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmData;

namespace SmartAlarmAgent.Repository
{
    class RestorationAlarmDBRepo
    {
        #region Prooerties

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

        #endregion Properties

        #region Constructor
        public RestorationAlarmDBRepo()
        {
            this._RestAlarmContext = new RestorationAlarmDbContext();
        }

        #endregion Constructor

        #region Methode
        public async Task<List<DigitalPointInfo>> GetSationNameAsync()
        {
            return await Task.Run(() => exeGetSationNameAsync());
        }

        public List<DigitalPointInfo> exeGetSationNameAsync()
        {
            IQueryable<DigitalPointInfo> Stations;
            using (this._RestAlarmContext)
            {
               Stations = this._RestAlarmContext.DigitalPointInfo
                    .Where(c => c.StationName.Contains("BK"))
                    .Where(c => !(string.IsNullOrEmpty(c.MACName))) //same as.Where(c=>!(c.MACName == "" || c.MACName == null)) 
                                                                    //.Where(c=>c.MACName != "" && c.MACName != null)
                    .Take(20);

                //Context.RestorationAlarmList.AddRange();
                //Context.SaveChangesAsync();
                return Stations.ToList< DigitalPointInfo>();
            }
            
        }

        #endregion Methode

    }
}
