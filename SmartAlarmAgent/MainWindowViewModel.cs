using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAlarmData;

namespace SmartAlarmAgent
{
    public class MainWindowViewModel
    {
        public RestorationAlarmDbContext Context;
        public MainWindowViewModel()
        {
            using (Context = new RestorationAlarmDbContext())
            { 
                var Station = Context.DigitalPointInfo
                    .Where(c => c.StationName.Contains("BK"))
                    .Where(c => !(string.IsNullOrEmpty(c.MACName))) //same as.Where(c=>!(c.MACName == "" || c.MACName == null)) 
                                                                    //.Where(c=>c.MACName != "" && c.MACName != null)
                    .Take(20);

            foreach (var item in Station)
            {
                Console.WriteLine(item.PkDigitalID + "," + item.StationName.TrimEnd() + ":" + item.PointName + " " + item.MACName);
            }


                //Context.RestorationAlarmList.AddRange(;)
                // Context.SaveChangesAsync();
            }

        }

    }
}
