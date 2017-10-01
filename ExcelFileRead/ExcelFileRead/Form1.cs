using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;       //microsoft Excel 14 object in references-> COM tab
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelFileRead
{
    public partial class ExcelFileRead : Form
    {
        public ExcelFileRead()
        {
            InitializeComponent();
        }


        private void chkIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIntegratedSecurity.Checked == true)
            {
                txtUserName.Visible = false;
                txtPassword.Visible = false;
                lblName.Visible = false;
                lblPassword.Visible = false;
            }
            else
            {
                txtUserName.Visible = true;
                txtPassword.Visible = true;
                lblName.Visible = true;
                lblPassword.Visible = true;
            }


        }

        private void ExcelFileRead_Load(object sender, EventArgs e)
        {
            txtUserName.Visible = false;
            txtPassword.Visible = false;
            lblName.Visible = false;
            lblPassword.Visible = false;
        }




        private void btnSetDatabase_Click(object sender, EventArgs e)
        {
            string datasource =txtserverName.Text.Trim();
            string initialCatalog =txtDatabaseName.Text.Trim();
            string userId = txtUserName.Text.Trim();
            string password = txtPassword.Text.Trim();
            CreateConnectionString(datasource, initialCatalog, userId, password);

            DataTable dt = ExcuteQuery("SELECT name FROM sys.sysdatabases where name='" + initialCatalog + "'");
            if (dt.Rows.Count > 0)
            {
                lblConnectionOK.Visible = true;
            }
            else 
            {
              MessageBox.Show("Database Not found");
            }
        }




        public static void CreateConnectionString(string datasource, string initialCatalog, string userId, string password)
        {
            try
            {
                //Integrated security will be off if either UserID or Password is supplied
                var integratedSecurity = string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(password);

                //Create the connection string using the connection builder
                var connectionBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = datasource,
                    InitialCatalog = initialCatalog,
                    UserID = userId,
                    Password = password,
                    IntegratedSecurity = integratedSecurity
                };

                //Open the app.config for modification
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var AppconfileLocation = config.FilePath;




                //get element by tag name
                // var ConnectionStringElement = XmlDoc.GetElementsByTagName("connectionStrings");

                //Retreive connection string setting
                var connectionString = config.ConnectionStrings.ConnectionStrings["ExcelToDatabase"];
                if (connectionString == null)
                {
                    //Create connection string if it doesn't exist
                    config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings
                    {
                        Name = "ExcelToDatabase",
                        ConnectionString = connectionBuilder.ConnectionString,
                        ProviderName = "System.Data.SqlClient" //Depends on the provider, this is for SQL Server
                    });
                }
                else
                {
                    //Only modify the connection string if it does exist
                    connectionString.ConnectionString = connectionBuilder.ConnectionString;
                }

                //Save changes in the app.config
                config.Save(ConfigurationSaveMode.Modified);



                //Read XML File
                StreamReader rd = new StreamReader(AppconfileLocation);
                var AllXmlFile = rd.ReadToEnd();
                rd.Close();



                //Write XML File
                FileStream fs = new FileStream(AppconfileLocation, FileMode.Open, FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(AllXmlFile);
                }
            }
            catch (Exception ex)
            {
                //TODO: Handle exception
            }
        }





     
        private void btnUpload_Click(object sender, EventArgs e)
        {
            string Filelocation = txtFileUpload.Text;
            var griddata = getExcelFile(Filelocation);
            dgdShowExcel.DataSource = griddata;

        }



        public DataTable getExcelFile(string fileLocation)
        {

            DataTable dt = new DataTable();
            DataColumn clID = new DataColumn("serial", typeof(string));
            dt.Columns.Add(clID);
           
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(fileLocation);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
         
           

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            for (int i = 1; i <= rowCount; i++)
            {

                int progree = i * 100 / rowCount;
                List<string> lt=new List<string>();
                for (int j = 1; j <= colCount; j++)
                {
                    //new line
                    if (j == 1)
                    {
                        if (i != 1)
                        {
                            lt.Add((i-1).ToString());
                        }
                        continue;
                    }
                    var data = xlRange.Cells[i, j].Value2==null? "": xlRange.Cells[i, j].Value2.ToString();
                    if (i == 1)
                    {
                        string whiteSapceRemove = Regex.Replace(data, @"[^0-9a-zA-Z]+", "");
                        DataColumn cl = new DataColumn(whiteSapceRemove, typeof(string));
                        dt.Columns.Add(cl);
                    }
                    else
                    {
                       
                      lt.Add(data);
                    }  
                }
                if (lt.Count > 0)
                {
                 
                    dt.Rows.Add(lt.ToArray());
                    //dt.Rows.Add("1", "2", "3", "4");
                }

                toolStripProgressBar.Value = progree;

            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return dt;

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofdFileOpen = new OpenFileDialog();
            if (ofdFileOpen.ShowDialog() == DialogResult.OK)
            {
                txtFileUpload.Text = ofdFileOpen.FileName;
            }


        }

        private void btnSaveDatabase_Click(object sender, EventArgs e)
        {

            string tableName=txttableName.Text;
            if (tableName == "")
            {
                tableName = "EXCELFILEDATA";
            }
            else
            {
                tableName =Regex.Replace(tableName, @"[^0-9a-zA-Z]+", "");
            }

            int Row = dgdShowExcel.Rows.Count;
            int Column = dgdShowExcel.Columns.Count;

            bool isExists = isExist("Select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME='"+ tableName + "'");
            if (isExists)
            {
                ExcuteNonQuery("drop table "+ tableName + "");
                createTable();  
            }
            else
            {
              createTable();      
            }

            var dta=(DataTable)dgdShowExcel.DataSource;


            BulkCopy(dta, tableName);

            MessageBox.Show("DataSave Successful");




        }


      
        public void createTable()
        {

            int Column = dgdShowExcel.Columns.Count;
            string Query = "Create table ExcelFile(";
            for (int c = 0; c < Column; c++)
            {
                if (c == Column - 1)
                {
                    Query += dgdShowExcel.Columns[c].HeaderText +" nvarchar(max)null)";
                }
                else
                {
                    Query += dgdShowExcel.Columns[c].HeaderText +" nvarchar(max)null,";
                }
            }

            ExcuteNonQuery(Query);
        }





        public bool isExist(string Query)
        {
          DataTable dt=  ExcuteQuery( Query);
          if (dt.Rows.Count > 0)
          {
              return true;
          }

            return false;
        }



        //------------------------Database Connection------------------------


        public void ExcuteNonQuery(string Query)
        {
            ConfigurationManager.RefreshSection("connectionStrings");
            string connection = ConfigurationManager.ConnectionStrings["ExcelToDatabase"].ConnectionString;
            if (connection == null)
            {
                MessageBox.Show("Please set connection String");
                return;
            }

            SqlConnection con = new SqlConnection(connection);
            con.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(Query, con);
              var cd=  cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
        }





        public DataTable ExcuteQuery(string Query)
        {
            ConfigurationManager.RefreshSection("connectionStrings");
            string connection = ConfigurationManager.ConnectionStrings["ExcelToDatabase"].ConnectionString;

            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand com = new SqlCommand(Query, con);
            SqlDataAdapter sda = new SqlDataAdapter(com);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            return dt;
        }




        public void BulkCopy(DataTable dataTable, string tableName)
        {
            ConfigurationManager.RefreshSection("connectionStrings");
            string connection = ConfigurationManager.ConnectionStrings["ExcelToDatabase"].ConnectionString;


            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock);
            sqlBulkCopy.DestinationTableName = tableName;
            sqlBulkCopy.BatchSize = dataTable.Rows.Count;
            sqlBulkCopy.WriteToServer(dataTable);
            sqlBulkCopy.Close();
        }







        //------------------------end Database Connection------------------------

    }
}
