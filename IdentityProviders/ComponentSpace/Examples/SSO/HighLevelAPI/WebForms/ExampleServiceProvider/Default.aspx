<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ExampleServiceProvider._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Service Provider</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="defaultForm" runat="server">
    <div>
		<h1>Welcome to the Service Provider Site</h1>
		<p>
			You are logged in as <%= Context.User.Identity.Name %>.
		</p>
        <div id="attributesDiv" runat="server" visible="false">
            <h2>User Attributes</h2>
            <asp:repeater id="attributesRepeater" runat="server">
	            <ItemTemplate>
		            <p><%# DataBinder.Eval(Container.DataItem, "AttributeName") %>: <%# DataBinder.Eval(Container.DataItem, "AttributeValue") %></p>
	            </ItemTemplate>
            </asp:repeater>
        </div>
        <asp:Button ID="logoutButton" runat="server" Text="Logout" OnClick="logoutButton_Click" />
    </div>
    </form>
</body>
</html>
