using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using System.Data.OleDb;
using System.Data;
using System.Collections.Specialized;

public partial class Data : System.Web.UI.Page
{
    public void Page_Load(object sender, EventArgs e)
    {
        // Required parameters sent from DataTables
        var length = int.Parse(Request.Form["length"]);
        var start = int.Parse(Request.Form["start"]);
        var draw = Request.Form["draw"];

        //custom parameters from DataTalbes
        var BUSINESS_UNIT_GL = Request.Form["BUSINESS_UNIT_GL"];
        var Entity = Request.Form["Entity"];
        var Account = Request.Form["Account"];
        var FISCAL_YEAR = Request.Form["FISCAL_YEAR"];
        var tableName = Request.Form["tableName"];

        //ordering info
        int ordCol = int.Parse(Request.Form["order[0][column]"]); //column to be ordered
        string ordDir = Request.Form["order[0][dir]"]; //asc or dsc

        //add one to the ordCol because DataTables is 0 based but Access is not
        ordCol = ordCol + 1;

        // query all data. when doing for real should probabaly only query the releant range. right now cant do in sql  
        // because of limitation with access  
        string filelocation = System.Web.HttpContext.Current.Server.MapPath("fakeCheckbook.accdb");
        string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filelocation;
        string query = "SELECT * FROM " + tableName + " WHERE (BUSINESS_UNIT_GL = '" + BUSINESS_UNIT_GL + "') AND DEPTID IN (SELECT DISTINCT levelZero FROM rDrill WHERE parent = '" + Entity + "') AND ACCOUNT IN (SELECT DISTINCT levelZero FROM rDrill WHERE parent = '" + Account + "') AND FISCAL_YEAR='" + FISCAL_YEAR + "' ORDER BY "+ordCol+" "+ordDir+";";
        OleDbConnection connection = new OleDbConnection(ConnectionString);
        OleDbCommand command = new OleDbCommand(query, connection);
        connection.Open();
        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
        DataTable dt = new DataTable();
        adapter.Fill(dt);
        connection.Close();

        //prep data to be stored in the "result" var
        //the dt data must be but into an array of arrays
        //right now the line below puts data in an array of DataRows which will not be read correctly by the JSON writer
        var tempdata = dt.Select().Skip(start).Take(length).ToArray();
        object[][] drawArray = new object[tempdata.Length][];
        int i = 0;
        foreach (DataRow row in tempdata)
        {
            drawArray[i] = row.ItemArray;
            i++;
        }

        //put the response expected by Datatables into a var "result"
        var result = new
        {
            recordsTotal = dt.Rows.Count,
            recordsFiltered = dt.Rows.Count,
            draw = draw,
            data = drawArray
        };

        //convert "result" array into JSON and write to response
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(result);
        Page.Response.Clear();
        Page.Response.ClearContent();
        Page.Response.ClearHeaders();
        Page.Response.Cookies.Clear();
        Page.Response.ContentType = "application/json";
        Page.Response.Write(json);
    }
}