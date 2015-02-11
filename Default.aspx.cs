using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Xml;
using System.Web.UI.WebControls.WebParts;
using System.Data;
using System.Diagnostics;
using System.Data.OleDb;
using System.Xml.Xsl;
using System.IO;

using OfficeOpenXml;




public partial class _Default : System.Web.UI.Page
{
    public void Page_Load(object sender, EventArgs e)
    {


    }

    protected string getBU(string entity)
    {
        if (entity.Contains("_MI") || entity.Contains("TMMI"))
        {
            return "TMMI";
        }
        else if (entity.Contains("_AL") || entity.Contains("TMMAL"))
        {
            return "TMMAL";
        }
        else if (entity.Contains("_TX") || entity.Contains("TMMTX"))
        {
            return "TMMTX";
        }
        else if (entity.Contains("_MS") || entity.Contains("TMMMS"))
        {
            return "TMMMS";
        }
        else if (entity.Contains("_NK") || entity.Contains("TMMNK"))
        {
            return "TMMNK";
        }
        else if (entity.Contains("_NJ") || entity.Contains("_NS") || entity.Contains("_NT") || entity.Contains("BODINE"))
        {
            return "BODINE";
        }
        //need to add the abbrev for TABC
        else if (entity.Contains("TABC"))
        {
            return "TABC";
        }
        else if (entity == "Total_Plant_Rollup")
        {
            return "TMMAL\' OR \'TMMI\' OR \'TMMMS\' OR \'TMMWV\' OR \'TMMTX\' OR \'TMMK\' OR \'TMMNK\' OR \'BODINE\' OR \'TABC";
        }
        else
        {
            return entity;
        }

    }


    protected void exportExcel(Hashtable parsedHash, List<string> databaseTables)
    {

        if (parsedHash == null)
        {
            return;
        }
        else
        {

            string filelocation = Server.MapPath("fakeCheckbook.accdb");
            string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filelocation;

            DataSet ds = new DataSet();
            foreach (string databaseTable in databaseTables)
            {
                string query = "SELECT * FROM " + databaseTable + " WHERE (BUSINESS_UNIT_GL = '" + parsedHash["BUSINESS_UNIT_GL.id"] + "') AND DEPTID IN (SELECT DISTINCT levelZero FROM rDrill WHERE parent = '" + parsedHash["Entity.id"] + "') AND ACCOUNT IN (SELECT DISTINCT levelZero FROM rDrill WHERE parent = '" + parsedHash["Account.id"] + "') AND FISCAL_YEAR='" + parsedHash["Years.id"] + "';";

                OleDbConnection connection = new OleDbConnection(ConnectionString);
                OleDbCommand command = new OleDbCommand(query, connection);
                connection.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                ds.Tables.Add(dt);
            }

            ExcelPackage package = new ExcelPackage();

            foreach (DataTable table in ds.Tables)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(table.TableName);
                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    worksheet.Cells[1, i].Value = table.Columns[i - 1].ColumnName;
                }

                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {
                        worksheet.Cells[j + 2, k + 1].Value = table.Rows[j].ItemArray[k].ToString();
                    }
                }

            }

            //Read the Excel file in a byte array. here pck is the Excelworkbook              
            Byte[] fileBytes = package.GetAsByteArray();

            //Clear the response               
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Cookies.Clear();
            //Add the header & other information      
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.CacheControl = "private";
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AppendHeader("Content-Length", fileBytes.Length.ToString());
            Response.AppendHeader("Pragma", "cache");
            Response.AppendHeader("Expires", "60");
            Response.AppendHeader("Content-Disposition",
            "attachment; " +
            "filename=\"drill-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx\"; " +
            "size=" + fileBytes.Length.ToString() + "; " +
            "creation-date=" + DateTime.Now.ToString("yyyyMMddHHmmss") + "; " +
            "modification-date=" + DateTime.Now.ToString("yyyyMMddHHmmss") + "; " +
            "read-date=" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Write it back to the client    
            Response.BinaryWrite(fileBytes);
            Response.End();
        }
    }



    protected Hashtable parsePost(string postString)
    {
        if (postString == "" || postString == null)
        {
            return null;
        }
        else
        {
            string[] splitString = postString.Split(new Char[] { ',' });

            Hashtable postHash = new Hashtable();
            for (int i = 0; i <= splitString.Length - 1; i++)
            {
                postHash.Add(splitString[i].Substring(0, splitString[i].LastIndexOf(".")), splitString[i].Substring(splitString[i].LastIndexOf(".") + 1));
            }

            postHash.Add("BUSINESS_UNIT_GL.id", getBU(postHash["Entity.id"].ToString()));
            postHash["Years.id"] = postHash["Years.id"].ToString().Replace("FY", "20");

            return postHash;
        }
    }

    protected void excelButton_Click(object sender, EventArgs e)
    {

        string postString = hiddenPost.Value;
        Hashtable parsedHash = parsePost(postString);
        List<string> databaseTables = new List<string>();
        databaseTables.Add("checkbook");
        databaseTables.Add("checkbook2");

        exportExcel(parsedHash, databaseTables);

    }

    protected string generateTableHeaders(string tableName)
    {
        string filelocation = Server.MapPath("fakeCheckbook.accdb");
        string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filelocation;

        //this is just to select column names and is not ideal sql but is the only option for Access
        //this should be changed when using a real DB
        string query = "SELECT * FROM " + tableName + " WHERE 1=2;";

        OleDbConnection connection = new OleDbConnection(ConnectionString);
        OleDbCommand command = new OleDbCommand(query, connection);
        connection.Open();
        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
        DataTable dt = new DataTable();
        adapter.Fill(dt);

        //to remove the underscores in the column names, if desired
        foreach (DataColumn column in dt.Columns)
        {
            column.ColumnName = column.ColumnName.Replace("_", " ");
        }

        //to generate the <th> string
        string tableHeaderString = "";
        foreach (DataColumn column in dt.Columns)
        {
            tableHeaderString = tableHeaderString + " <th> " + column.ColumnName + " </th>";
        }

        return tableHeaderString;
    }

}