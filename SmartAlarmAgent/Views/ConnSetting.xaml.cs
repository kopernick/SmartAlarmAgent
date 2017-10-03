using System.Windows.Forms;
using SmartAlarmAgent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using SmartAlarmAgent.Service;
using SmartAlarmData;

namespace SmartAlarmAgent.Views
{
    /// <summary>
    /// Interaction logic for ConnSetting.xaml
    /// </summary>
    public partial class ConnSetting : Window
    {
        public new ConnDialogResult? DialogResult { get; private set; }

        private ConnectionConfig _connCfg;
        private string fileDirectory;
        private string database;
        private string file;

        private OpenFileDialog openFolderDialog { get; set; }


        //public ConnectionConfig ConnCfg = new ConnectionConfig();

        public ConnSetting()
        {
            InitializeComponent();

        }
        public ConnSetting(ConnectionConfig connCfg)
            : this()
        {

            this.DialogResult = null;
            this._connCfg = connCfg;

            this.txtServer.Text = _connCfg.Server;
            this.txtLogin.Text = _connCfg.Login;
            this.txtPassword.Password = _connCfg.Password;
            this.txtSelectedDB.Content = _connCfg.Database;
            this.txtCSVPath.Text = _connCfg.CsvDirectory;
            this.lblCsvFile.Content = _connCfg.CsvFile;

            this.database = _connCfg.Database;
            this.fileDirectory = _connCfg.CsvDirectory;

            this.openFolderDialog = new OpenFileDialog();
            this.openFolderDialog.ValidateNames = false;
            this.openFolderDialog.CheckFileExists = false;
            this.openFolderDialog.CheckPathExists = true;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _connCfg.Server = this.txtServer.Text;
            _connCfg.Login = this.txtLogin.Text;
            _connCfg.Password = this.txtPassword.Password;
            _connCfg.Database = this.database;
            _connCfg.CsvDirectory = this.txtCSVPath.Text;
            _connCfg.CsvFile = this.txtCSVPath.Text + "\\AlarmList.csv";

            this.DialogResult = ConnDialogResult.OK;
            Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            _connCfg.Server = this.txtServer.Text;
            _connCfg.Login = this.txtLogin.Text;
            _connCfg.Password = this.txtPassword.Password;
            _connCfg.Database = this.database;
            _connCfg.CsvDirectory = this.txtCSVPath.Text;
            _connCfg.CsvFile = this.txtCSVPath.Text + "\\AlarmList.csv";

            this.DialogResult = ConnDialogResult.APPLY;

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //this.DialogResult = ConnDialogResult.Cencel;
            //e.Cancel = true; //Not Close 
            //this.Close(); //Hide View
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (this.openFolderDialog)
            {
                this.openFolderDialog.FileName = "Select a folder";

                this.openFolderDialog.ShowDialog();

                if (IsFileExits(Path.GetDirectoryName(openFolderDialog.FileName).ToString()))
                {
                    this.file = openFolderDialog.FileName; //Get File Name
                    this.fileDirectory = Path.GetDirectoryName(this.file).ToString(); //Get File Directory
                    this.txtCSVPath.Text = this.fileDirectory;

                    //_connCfg.CsvDirectory = this.fileDirectory;
                    //_connCfg.CsvFile = _connCfg.CsvDirectory + "\\Digital.csv";

                    this.lblCsvFile.Content = _connCfg.CsvFile;
                    //System.Windows.MessageBox.Show("You selected: " + this.fileDirectory);
                }
                else
                {
                    System.Windows.MessageBox.Show("Directory: " + Path.GetDirectoryName(openFolderDialog.FileName).ToString() + "ไม่มี file:AlarmList.csv");
                }

            }
        }

        private void btnCSVCheck_Click(object sender, RoutedEventArgs e)
        {
            if (IsFileExits(this.txtCSVPath.Text))
            {
                System.Windows.Forms.MessageBox.Show("File AlarmList.CSV มีใน " + this.txtCSVPath.Text, "Check AlarmList.csv"
                    , MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.fileDirectory = this.txtCSVPath.Text; //Get File Directory
                //_connCfg.CsvDirectory = this.fileDirectory;
                //_connCfg.CsvFile = this.fileDirectory + "\\Digital.csv";

                this.lblCsvFile.Content = this.fileDirectory + "\\AlarmList.csv";
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ไม่พบ File AlarmList.CSV ใน " + this.txtCSVPath.Text, "Check AlarmList.csv"
                    , MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsFileExits(string path)
        {
            //FileInfo file = new FileInfo(path);
            string fileName = "AlarmList.csv";
            FileInfo file = new FileInfo(path + "\\" + fileName);
            using (new WaitCursor())
            {
                try
                {
                    if (file.Exists)
                    {
                        Console.WriteLine("Founded");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Error File not found");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error " + e.Message);
                    return false;
                }
            }

        }

        public string GetSQLConnString()
        {

#if true
            
          var sql = @"metadata=res://*/RestorationAlarmModel.csdl|res://*/RestorationAlarmModel.ssdl|res://*/RestorationAlarmModel.msl;" +
               @"provider = System.Data.SqlClient;" +
               @"provider connection string=""" +
               "data source = " + _connCfg.Server + ";" +
               "initial catalog = " + _connCfg.Database + ";" +
               "user id = " + _connCfg.Login + ";" +
               "password = " + _connCfg.Password + ";" +
               @"multipleactiveresultsets = True;" +
               @"App = EntityFramework ;" +
               //@"providerName = " + @"/"System.Data.EntityClient/"" +
               @"""";

                return sql;

#else

            string sql = " user id = " + this.txtLogin.Text + ";" +
                         " password = " + this.txtPassword.Password + ";" +
                         " server = " + this.txtServer.Text + ";" +
                         " database = " + this.txtSelectedDB.Content + ";" +
                         @" Integrated Security = true;
                            Trusted_Connection = no;
                            database = AlarmAbnormalOnline;
                            connection timeout = 30;";
            return sql;

            return @"
                       user id = macapp;
                       password = mac;
                       server = 10.20.86.119\MAODSCADA;
                       Integrated Security = true;
                       Trusted_Connection = no;
                       database = AlarmAbnormalOnline;
                       connection timeout=30
                     ";
#endif
        }

        public string GetDefualtSQLConnString()
        {

            var sql = @"metadata=res://*/RestorationAlarmModel.csdl|res://*/RestorationAlarmModel.ssdl|res://*/RestorationAlarmModel.msl;" +
              @"provider = System.Data.SqlClient;" +
              @"provider connection string=""" +
              "data source = " + this.txtServer.Text + ";" +
             //"initial catalog = "  +this.txtSelectedDB.Content + ";" +
             "user id = " + this.txtLogin.Text + ";" +
             "password = " + this.txtPassword.Password + ";" +
              @"multipleactiveresultsets = True;" +
              @"App = EntityFramework ;" +
              //@"providerName = " + @"/"System.Data.EntityClient/"" +
              @"""";
           return sql;

        }

        public string GetCurrentSQLConnString()
        {

            var sql = @"metadata=res://*/RestorationAlarmModel.csdl|res://*/RestorationAlarmModel.ssdl|res://*/RestorationAlarmModel.msl;" +
              @"provider = System.Data.SqlClient;" +
              @"provider connection string=""" +
              "data source = " + this.txtServer.Text + ";" +
              "initial catalog = "  + this.txtSelectedDB.Content + ";" +
             "user id = " + this.txtLogin.Text + ";" +
             "password = " + this.txtPassword.Password + ";" +
              @"multipleactiveresultsets = True;" +
              @"App = EntityFramework ;" +
              //@"providerName = " + @"/"System.Data.EntityClient/"" +
              @"""";
            return sql;

        }

        public bool IsServerConnected(string connectionString)
        {
            //RestEventArgs args = new RestEventArgs();
            var _RestAlarmContext = new RestorationAlarmDbContext(connectionString);
            //return _RestAlarmContext.Database.Exists();

            try
            {
                _RestAlarmContext.Database.Connection.Open();
                
                _RestAlarmContext.Database.Connection.Close();
                Console.WriteLine("Connect Success");
                return true;
            }
            catch
            {
                return false;
            }
        }


        private void btnTestSQL_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                if (IsServerConnected(GetCurrentSQLConnString()))
                {
                    System.Windows.Forms.MessageBox.Show("Connect to: " + this.txtServer.Text + "; " + this.txtSelectedDB.Content + " Success", "Check SQL Connection"
                        , MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid User name or Password", "Check SQL Connection"
                        , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private List<string> GetDatabase()
        {
            using (new WaitCursor())
            {
                try
                {
                    using (var _RestAlarmContext = new RestorationAlarmDbContext(GetDefualtSQLConnString()))
                    {
                        try
                        {
                            _RestAlarmContext.Database.Connection.Open();
                            DataTable databases = _RestAlarmContext.Database.Connection.GetSchema("Databases");
                            List<string> TableNames = new List<string>();

                            foreach (DataRow row in databases.Rows)
                            {
                                string strDatabaseName = row["database_name"].ToString();
                                TableNames.Add(strDatabaseName);
                            }
                            _RestAlarmContext.Database.Connection.Close();
                            Console.WriteLine("Break Test");
                            return TableNames;
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show("Invalid Server name", "Get SQL Table"
                            , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                        
                        
                    }
                    
                }
                catch
                {
                    return null;
                }
            }

        }

        private void btnGetDb_Click(object sender, RoutedEventArgs e)
        {
            this.cmbDatabase.ItemsSource = GetDatabase();

        }

        private void cmbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.database = this.cmbDatabase.SelectedItem.ToString();
            this.txtSelectedDB.Content = this.database;
            Console.WriteLine("Select Database: " + this.database);
        }
    }
}
