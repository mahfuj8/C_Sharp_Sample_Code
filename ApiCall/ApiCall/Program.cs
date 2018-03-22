using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiCall
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = new Task(DownloadPageAsync);
            t.Start();
            Console.WriteLine("Downloading page...");
            Console.ReadLine();



        }


        static async void DownloadPageAsync()
        {
            // ... Target page.
          //  string page = "http://www.dsebd.org/latest_share_price_all_by_ltp.php";  //All Trade Price 

            string page = "http://www.dsebd.org/bshis_new1_old.php?w=ACI&sid=21656";   // Market Price



            // ... Use HttpClient.
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(page))
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                string result = await content.ReadAsStringAsync();

                ConvertHTMLTablesToDataSet(result);


                // ... Display the result.
                if (result != null &&
                    result.Length >= 50)
                {
                    Console.WriteLine(result.Substring(0, 50) + "...");
                }
            }
        }


        private static DataSet ConvertHTMLTablesToDataSet(string HTML)
        {
            // Declarations  
            DataSet ds = new DataSet();
            DataTable dt = null;
            DataRow dr = null;
            DataColumn dc = null;
            string TableExpression = "<TABLE[^>]*>(.*?)</TABLE>";
            string HeaderExpression = "<TH[^>]*>(.*?)</TH>";
            string RowExpression = "<TR[^>]*>(.*?)</TR>";
            string ColumnExpression = "<TD[^>]*>(.*?)</TD>";
            bool HeadersExist = false;
            int iCurrentColumn = 0;
            int iCurrentRow = 0;
            // Get a match for all the tables in the HTML  
            MatchCollection Tables = Regex.Matches(HTML, TableExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            // Loop through each table element  
            foreach (Match Table in Tables)
            {
                // Reset the current row counter and the header flag  
                iCurrentRow = 0;
                HeadersExist = false;
                // Add a new table to the DataSet  
                dt = new DataTable();
                //Create the relevant amount of columns for this table (use the headers if they exist, otherwise use default names)  
                // if (Table.Value.Contains("<th"))  
                if (Table.Value.Contains("<TH"))
                {
                    // Set the HeadersExist flag  
                    HeadersExist = true;
                    // Get a match for all the rows in the table  
                    MatchCollection Headers = Regex.Matches(Table.Value, HeaderExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    // Loop through each header element  
                    foreach (Match Header in Headers)
                    {
                        dt.Columns.Add(Header.Groups[1].ToString());
                    }
                }
                else
                {
                    for (int iColumns = 1; iColumns <= Regex.Matches(Regex.Matches(Regex.Matches(Table.Value, TableExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase)[0].ToString(), RowExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase)[0].ToString(), ColumnExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).Count; iColumns++)
                    {
                        dt.Columns.Add("Column " + iColumns);
                    }
                }
                //Get a match for all the rows in the table  
                MatchCollection Rows = Regex.Matches(Table.Value, RowExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                // Loop through each row element  
                foreach (Match Row in Rows)
                {
                    // Only loop through the row if it isn't a header row  
                    if (!(iCurrentRow == 0 && HeadersExist))
                    {
                        // Create a new row and reset the current column counter  
                        dr = dt.NewRow();
                        iCurrentColumn = 0;
                        // Get a match for all the columns in the row  
                        MatchCollection Columns = Regex.Matches(Row.Value, ColumnExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        // Loop through each column element  
                        foreach (Match Column in Columns)
                        {
                            // Add the value to the DataRow  
                            dr[iCurrentColumn] = Column.Groups[1].ToString();
                            // Increase the current column  
                            iCurrentColumn++;
                        }
                        // Add the DataRow to the DataTable  
                        dt.Rows.Add(dr);
                    }
                    // Increase the current row counter  
                    iCurrentRow++;
                }
                // Add the DataTable to the DataSet  
                ds.Tables.Add(dt);
            }
            return ds;
        }








    }
}
