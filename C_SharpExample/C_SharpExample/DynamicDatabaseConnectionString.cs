using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using System.Configuration;

namespace C_SharpExample
{
    class DynamicDatabaseConnectionString
    {

        public static void CreateConnectionString(string datasource, string initialCatalog, string userId, string password, string ConnectionSringName)
        {
            try
            {
                //Integrated security will be off if either UserID or Password is supplied
                var integratedSecurity = string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password);

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


                //Check Connection string Element available
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(AppconfileLocation);
                var ConnectionStringElement = XmlDoc.GetElementsByTagName("connectionStrings");

                if (ConnectionStringElement == null)
                {
                    XmlDocument xmlDoc1 = new XmlDocument();
                    XmlNode rootNode = xmlDoc1.CreateElement("connectionStrings");
                    xmlDoc1.AppendChild(rootNode);

                }


                //Retreive connection string setting
                var connectionString = config.ConnectionStrings.ConnectionStrings["ConnectionStringName"];
                if (connectionString == null)
                {
                    //Create connection string if it doesn't exist
                    config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings
                    {
                        Name = ConnectionSringName,
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











        // When need existing connection string change 
        public void updateConfigFile(string datasource, string initialCatalog, string userId, string password)
        {

            var integratedSecurity = string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password);
            //Constructing connection string from the inputs
            StringBuilder Con = new StringBuilder("Data Source=");
            Con.Append("" + datasource + ";");
            Con.Append("Initial Catalog=");
            Con.Append("" + initialCatalog + ";");
            Con.Append("UserID=");
            Con.Append("" + userId + ";");
            Con.Append("Password=");
            Con.Append("" + password + ";");
            Con.Append("IntegratedSecurity=");
            Con.Append("" + integratedSecurity + ";");

            string strCon = Con.ToString();


            //updating config file
            XmlDocument XmlDoc = new XmlDocument();
            //Loading the Config file
            XmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            foreach (XmlElement xElement in XmlDoc.DocumentElement)
            {
                //Find connection String
                if (xElement.Name == "connectionStrings")
                {
                    //setting the coonection string
                    xElement.FirstChild.Attributes[2].Value = strCon;
                }
            }

            //writing the connection string in config file
            XmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }





        //Read data from Database
        private DataTable VirtualConnectionStringSet()
        {
            try
            {
                //Create new sql connection
                SqlConnection Db = new SqlConnection();
                ////to refresh connection string each time else it will use             previous connection string
                ConfigurationManager.RefreshSection("connectionStrings");
                Db.ConnectionString = ConfigurationManager.ConnectionStrings["YourConnectionStringName"].ToString();
                //To check new connection string is working or not. Please use the existing table otherwise it will give error
                SqlDataAdapter da = new SqlDataAdapter("select * from Company", Db);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception E)
            {
                //Show error
                // MessageBox.Show(ConfigurationManager.ConnectionStrings["con"].ToString() + ".This is invalid connection", "Incorrect server/Database");
            }
            return null;
        }









    }
}
