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
        public string CurrYearMont { get; set; }

        public ConnectionConfig()
        {

            Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SmartAlarmConfig");

        }
        public void LoadConfiguration()
        {
            try
            {
                Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SmartAlarmConfig");

                if (reg.GetValue("Server") == null)
                    this.Server = "";
                else
                    this.Server = reg.GetValue("Server").ToString();

                if (reg.GetValue("CSVPath") == null)
                    this.CsvDirectory = "";
                else
                    this.CsvDirectory = reg.GetValue("CSVPath").ToString();

                if (reg.GetValue("Login") == null)
                    this.Login = "";
                else
                    this.Login = reg.GetValue("Login").ToString();

                if (reg.GetValue("Password") == null)
                    this.Password = "";
                else
                    this.Password = reg.GetValue("Password").ToString();

                if (reg.GetValue("CsvFile") == null)
                    this.CsvFile = "";
                else
                    this.CsvFile = reg.GetValue("CsvFile").ToString() ?? "";
                

                if (reg.GetValue("Database") == null)
                    this.Database = "";
                else
                    this.Database = reg.GetValue("Database").ToString();


                if (reg.GetValue("CurrentMonth") == null)
                    this.CurrYearMont = "0";
                else
                    this.CurrYearMont = reg.GetValue("CurrentMonth").ToString();
                
                
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
                reg.SetValue("CurrentMonth", this.CurrYearMont);

                reg.Close();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
            }
        }
        public void SaveCurrentMonth(string currMont)
        {
            this.CurrYearMont = currMont;

            try
            {
                Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SmartAlarmConfig", true);
                reg.SetValue("CurrentMonth", this.CurrYearMont);

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
        CANCEL,
        SAVE,
        APPLY
    };

    public enum AuthenMethod
    {
        Windows_Authentication,
        Server_Authentication,
    };


}
