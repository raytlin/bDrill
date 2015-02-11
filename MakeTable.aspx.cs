using System;
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
using System.Data.SqlClient;
using System.Data.OleDb;




public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (FileUpload1.HasFile)
        {
            emptyupload.Text = "";
            FileUpload1.SaveAs(Server.MapPath("~/" + FileUpload1.FileName));
            updateLabel.Text = "Running....";

        }
        else
        {
            emptyupload.Text = "Choose your file";
            
        }
        
    }

    protected string checkForBU(string CCstring)
    {
        if (CCstring.Contains("_CC"))
        {
            string ccSubString = CCstring.Substring(0,CCstring.Length - 6);

            
            return ccSubString;
        }
        else
        {
            return CCstring;
        }
    }

    protected List<Tuple<string, string>> stopAndHash(XmlNode currentNode)
    {

        List<Tuple<string, string>> listOfPairs = new List<Tuple<string, string>>();

        XmlNode lvlZeroNode = currentNode;
        string accountStr = lvlZeroNode.Attributes["name"].Value;
        string checkedString = checkForBU(accountStr);

        currentNode = lvlZeroNode;

        //conditional has to be 2 parentnodes up because the top parent has the parent of "Document"
        while (currentNode.ParentNode.ParentNode != null)
        {
            Tuple<string, string> pair = new Tuple<string, string>(currentNode.ParentNode.Attributes["name"].Value, checkedString);
            listOfPairs.Add(pair);
            currentNode = currentNode.ParentNode;
        }
        listOfPairs.Add(new Tuple<string, string>(accountStr, checkedString));

        return listOfPairs;
    }

    protected bool areChildrenNonMembers(XmlNode currentNode)
    {
        
        foreach (XmlNode node in currentNode)
        {
            if (node.Name == "Member")
            {
                return false;
            }
        }

        return true;
    }

    protected bool isLevelZero(XmlNode currentNode)
    {

        if (currentNode == null || currentNode.Name != "Member")
        {
            return false;
        }
        else if ((currentNode.HasChildNodes == false && currentNode.Name == "Member") || (areChildrenNonMembers(currentNode) && currentNode.Name == "Member"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

     //start of recursive only stuff
    protected XmlNode rloadTopNode(string filename)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(Server.MapPath(filename));
        XmlNode node = doc.DocumentElement;
        return node;
    }

    protected List<Tuple<string, string>> rNodeTest(XmlNode node, List<Tuple<string, string>> rTableTup)
    {
        
        foreach (XmlNode currentNode in node)
        {
            if (isLevelZero(currentNode))
            {
                //take the currentNode and finds all parents and puts into a list of tuples
                //then add all of the tuples returned to the end of rTableTup
                foreach (Tuple<string, string> pair in stopAndHash(currentNode))
                {
                    rTableTup.Add(pair);
                }
            }
            else
            {
                rTableTup = rNodeTest(currentNode, rTableTup);
            }
        }
        return rTableTup;
    }

    protected void writeToDrillDB(List<Tuple<string, string>> listOfPairs)
    {
        string filelocation = Server.MapPath("fakeCheckbook.accdb");
        string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+filelocation;
        OleDbConnection connection = new OleDbConnection(ConnectionString);
        connection.Open();

        //delete everything in the table before loading 
        OleDbCommand deleteCommand = new OleDbCommand("DELETE * FROM rDrill;", connection);
        deleteCommand.ExecuteNonQuery();
        

        foreach (Tuple<string, string> pair in listOfPairs)
        {
            string query = "INSERT INTO rDrill (parent, levelZero) VALUES (\"" + pair.Item1 + "\", \"" + pair.Item2 + "\");";
            OleDbCommand command = new OleDbCommand(query, connection);
            command.ExecuteNonQuery();
                     
        }
        
        connection.Close();
        updateLabel.Text = "Done<br />";

    }

    

    
}


