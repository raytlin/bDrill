<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Drillthrough</title>
    
    <!-- jQuery UI CSS -->
    <link href="jquery-ui.css" rel="stylesheet" type="text/css" />
    <!-- DataTables CSS -->
    <link href="jquery.dataTables.css" rel="stylesheet" type="text/css" />
    <!-- jQuery -->
    <script src="jquery-1.10.2.js" type="text/javascript"></script>
    <!-- jQuery UI -->
    <script src="jquery-ui.js" type="text/javascript"></script>
    <!-- DataTables -->
    <script src="jquery.dataTables.min.js" type="text/javascript"></script>
    
    <% 
        string postString = Page.Request.Form["attribute"];

        if (postString != null)
        {
            Hashtable parsedHash = parsePost(postString);

            //set the hidden fields to send the parsed values to the web service through DataTables
            BUSINESS_UNIT_GL.Value = parsedHash["BUSINESS_UNIT_GL.id"].ToString();
            Entity.Value = parsedHash["Entity.id"].ToString();
            Account.Value = parsedHash["Account.id"].ToString();
            FISCAL_YEAR.Value = parsedHash["Years.id"].ToString();

            //set the hiddenfield used that is used by the excel export function
            hiddenPost.Value = Page.Request.Form["attribute"];
        }
    %>

    <script type="text/javascript">
        //hide everything to prevent flash of unstyled content
        $('html').hide();

        $(function () {
            $("#tabs").tabs();
        });

        $(document).ready(function () {
            $('#checkbook').DataTable({
                "scrollX": true, //sets horizontal scroll
                "ordering": true, //turn off ordering my clicking on headers. can be implemented with SQL alterations
                "scrollY": "300px", //veritcal scroll set to 300px. vh does not work in IE8
                "processing": true, 
                "serverSide": true, //turn on server side processing
                "filter": false, //turn off search filter. can be implemented with SQL changes
                "ajax": { 
                    "url": "../bDrillv3/Data.aspx", //url to webservice to return server side processed data
                    "type": "POST", //using post instead of default of GET bc asp.net cant handle large query strings
                    "data": function (d) { //send in all dimension values as parameters 
                        d.BUSINESS_UNIT_GL = document.getElementById('BUSINESS_UNIT_GL').value;
                        d.Entity = document.getElementById('Entity').value;
                        d.Account = document.getElementById('Account').value;
                        d.FISCAL_YEAR = document.getElementById('FISCAL_YEAR').value;
                        d.tableName = "checkbook";
                    }
                }
            }); //end datatables
        });      //end ready

        $(document).ready(function () {
            $('#checkbook2').DataTable({
                "scrollX": true,
                "scrollY": "300px",
                "ordering": true,
                "processing": true,
                "serverSide": true,
                "filter": false,
                "ajax": {
                    "url": "../bDrillv3/Data.aspx",
                    "type": "POST",
                    "data": function (d) {
                        d.BUSINESS_UNIT_GL = document.getElementById('BUSINESS_UNIT_GL').value;
                        d.Entity = document.getElementById('Entity').value;
                        d.Account = document.getElementById('Account').value;
                        d.FISCAL_YEAR = document.getElementById('FISCAL_YEAR').value;
                        d.tableName = "checkbook2";
                    }
                }
            }); //end datatables
        });      //end ready

        //show html tag after load
        $(window).load(function () {
            $('html').show();
        }); //end load

    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:HiddenField ID="BUSINESS_UNIT_GL" runat="server" />
    <asp:HiddenField ID="Entity" runat="server" />
    <asp:HiddenField ID="Account" runat="server" />
    <asp:HiddenField ID="FISCAL_YEAR" runat="server" />
    <div id="tabs" style="font-size: small; font-family: Verdana; text-align: center">
        <ul>
            <li><a href="#tabs-1">AP</a></li>
            <li><a href="#tabs-2">AR</a></li>
            <li><a href="#tabs-3">Scrap</a></li>
        </ul>
        <div id="tabs-1">
            <table class="display" id="checkbook" cellspacing="0" width="100%">
                <thead>
                    <tr>
                        <%= generateTableHeaders("checkbook") %>
                    </tr>
                </thead>
            </table>
        </div>
        <div id="tabs-2">
            <table class="display" id="checkbook2" cellspacing="0" width="100%">
                <thead>
                    <tr>
                        <%= generateTableHeaders("checkbook2") %>
                    </tr>
                </thead>
            </table>
        </div>

        <div id="tabs-3">
        </div>

    </div>
    <% 
        if (Page.Request.Form["attribute"] == null)
        {
            Response.Write("<SCRIPT LANGUAGE=\"JavaScript\">alert(\"NO POST DATA RECEIVED FROM ESSBASE\")</SCRIPT>");
        }

        
        
    %>
    <asp:HiddenField ID="hiddenPost" runat="server"></asp:HiddenField>
    <p>
        <asp:Button ID="excelButton" runat="server" Text="Export to Excel" OnClick="excelButton_Click" />
    </p>
    </form>
</body>
</html>
