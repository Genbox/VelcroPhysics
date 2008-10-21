<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="System.Web.Silverlight" Namespace="System.Web.UI.SilverlightControls"
    TagPrefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">
<head runat="server">
    <title>FarseerPhysicsWaterDemo</title>
</head>
<body style="height:100%;margin:auto; background-color:Silver">
    <form id="form1" runat="server" style="width:700px; height: 500px; margin:20px auto 0px auto; ">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div  style="height:100%; margin:auto;">
            <asp:Silverlight ID="Xaml1" runat="server" BackColor="Silver" Source="~/ClientBin/FarseerPhysicsWaterDemo.xap" MinimumVersion="2.0.30923.0" Width="700px" Height="500px" />
        </div>
    </form>
</body>
</html>