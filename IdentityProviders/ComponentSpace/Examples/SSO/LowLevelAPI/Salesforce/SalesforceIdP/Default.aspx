<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SalesforceIdP._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Identity Provider</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="defaultForm" runat="server">
	<div>
		<h1>Welcome to the Identity Provider Site</h1>
		<p>
			You are logged in as <%= Context.User.Identity.Name %>.
		</p>
		<p>
		    This example demonstrates SSO to Salesforce.
		</p>
		<p>
		    Click the link to SSO to <asp:HyperLink ID="spHyperLink" runat="server">Salesforce</asp:HyperLink>.
		</p>
	</div>
    </form>
</body>
</html>
