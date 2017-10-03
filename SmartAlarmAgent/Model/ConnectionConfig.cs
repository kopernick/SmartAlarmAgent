using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAlarmAgent.Model
{
    public class ConnectionConfig
    {

        public string Server { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string CsvDirectory { get; set; }
        public string CsvFile { get; set; }
        public ConnectionConfig()
        {

            Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SmartAlarmConfig");

        }
        public void LoadConfiguration()
        {
            try
            {
                Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SmartAlarmConfig");
                this.Server = reg.GetValue("Server").ToString();
                this.CsvDirectory = reg.GetValue("CSVPath").ToString();
                this.Login = reg.GetValue("Login").ToString();
                this.Password = reg.GetValue("Password").ToString();
                this.CsvFile = reg.GetValue("CsvFile").ToString();
                this.Database = reg.GetValue("Database").ToString();

                reg.Close();

                //this.m_dtPointConfiguration = new DataTable("PointConfiguration");
                //this.m_dtPointConfiguration.ReadXmlSchema("C:\\A2L\\PointConfiguration.ds");
                //this.m_dtPointConfiguration.ReadXml("C:\\A2L\\PointConfiguration.db");
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
            }
        }

        public void SaveConfiguration()
        {
            try
            {


                Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SmartAlarmConfig", true);
                reg.SetValue("Server", this.Server);
                reg.SetValue("CSVPath", this.CsvDirectory);
                reg.SetValue("Login", this.Login);
                reg.SetValue("Password", this.Password);
                reg.SetValue("CsvFile", this.CsvFile);
                reg.SetValue("Database", this.Database);
                
                reg.Close();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
            }
        }

    }

    public enum ConnDialogResult
    {
        Cencel,
        OK,
        APPLY
    };

    public enum AuthenMethod
    {
        Windows_Authentication,
        Server_Authentication,
    };


}
