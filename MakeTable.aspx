<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MakeTable.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Higher level drill</title>
</head>
<body>
<div style="font-family:Comic Sans MS">
    <form id="form1" runat="server">
    <p>Select XML outline file and press run </p>
    <asp:FileUpload ID="FileUpload1" runat="server" />
    <br />
    <br />
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Run" />
    <br />
    <asp:Label ID="emptyupload" runat="server"></asp:Label>
    <% 
              
        List<Tuple<string, string>> tupList = new List<Tuple<string, string>>();

        if (FileUpload1.FileName != "")
        {
            tupList = rNodeTest(rloadTopNode(FileUpload1.FileName), tupList);
            
            writeToDrillDB(tupList);
            
            //below is to just print to screen
            //foreach (Tuple<string, string> pair in tupList)
            //{
            //    Response.Write("INSERT INTO drillTable (parent, levelZero) VALUES (\"" + pair.Item1 + "\", \"" + pair.Item2 + "\");<br />");
            //    //Response.Write(pair.Item1+", "+pair.Item2+"<br />");
            //}
            
         }
        
    %>
    
    <br />
    <br />
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Label ID="updateLabel" runat="server" Text=""></asp:Label>
        </ContentTemplate>
    </asp:UpdatePanel>
    
    <br />
    
    </form>
    </div>
</body>
</html>
