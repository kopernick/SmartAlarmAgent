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
        private DataProcessLogic _dataProcessor;

        private OpenFileDialog openFolderDialog { get; set; }


        //public ConnectionConfig ConnCfg = new ConnectionConfig();

        public ConnSetting()
        {
            InitializeComponent();

        }
        public ConnSetting(ConnectionConfig connCfg, DataProcessLogic DataProcessor)
            : this()
        {

            this.DialogResult = null;
            this._connCfg = connCfg;
            this._dataProcessor = DataProcessor;
            //this.nLastAlarmRecIndex = DataProcessor.mAlarmList.nLastAlarmRecIndex;

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

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Do you want to save changes ?", 
                "Save Connection Setting", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _connCfg.SaveConfiguration();

                _dataProcessor.ConnCfg = this._connCfg;
                _dataProcessor.RefreshConnection(this._connCfg); //Refresh Connection Setting
                Console.WriteLine("ConnSetting was Saved");
                this.DialogResult = ConnDialogResult.SAVE;
                Close();
            }
            else
            {
                this.DialogResult = ConnDialogResult.CANCEL;
            }

        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            _connCfg.Server = this.txtServer.Text;
            _connCfg.Login = this.txtLogin.Text;
            _connCfg.Password = this.txtPassword.Password;
            _connCfg.Database = this.database;
            _connCfg.CsvDirectory = this.txtCSVPath.Text;
            _connCfg.CsvFile = this.txtCSVPath.Text + "\\AlarmList.csv";

            _dataProcessor.ConnCfg = _connCfg;
            _dataProcessor.RefreshConnection(_connCfg); //Refresh Connection Setting

            Console.WriteLine("ConnSetting was Apply");
            this.DialogResult = ConnDialogResult.APPLY;

        }

        protected override void OnClosing(CancelEventArgs e) //Override Close Methode
        {

            if (this.DialogResult != ConnDialogResult.SAVE)
            {
                if (this.DialogResult == ConnDialogResult.APPLY)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Do you want to keep changes ?",
                        "Connection Setting Confirmation", System.Windows.MessageBoxButton.YesNo);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _connCfg.SaveConfiguration();
                        Console.WriteLine("Keep changes ConnSetting");
                    }
                    else
                    {
                        _connCfg.LoadConfiguration();
                        _dataProcessor.ConnCfg = _connCfg;
                        _dataProcessor.RefreshConnection(_connCfg); //Refresh Connection Setting
                        Console.WriteLine("ConnSetting was Reload");
                    }

                }
                else
                {
                    //ConnDialogResult.CANCEL
                    Console.WriteLine("ConnSetting was Canceled !");
                }
               
            }
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

        public void TestConnected(string connectionString)
        {
            //RestEventArgs args = new RestEventArgs();
            var _RestAlarmContext = new RestorationAlarmDbContext(connectionString);
            //return _RestAlarmContext.Database.Exists();

            try
            {
                _RestAlarmContext.Database.Connection.Open();
                
                _RestAlarmContext.Database.Connection.Close();
                Console.WriteLine("Connect Success");
                System.Windows.Forms.MessageBox.Show("Connect to: " + this.txtServer.Text + "; " + this.txtSelectedDB.Content + " Success", "Check SQL Connection"
                       , MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                System.Windows.Forms.MessageBox.Show(e.Message, "Check SQL Connection"
                       , MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }


        private void btnTestSQL_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                TestConnected(GetCurrentSQLConnString());
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
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);

                            System.Windows.Forms.MessageBox.Show(e.Message, "Get SQL Table"
                            , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                        
                        
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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
