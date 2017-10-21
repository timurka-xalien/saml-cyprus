<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="GoogleIdP.SAML.Logout" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Identity Provider Logout</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="logoutForm" runat="server">
    <div>
		<h1>Logout at the Identity Provider</h1>
		<p>
		    When you logout from Google Apps you are redirected to this page 
		    where you are automatically logged out at the identity provider.
		</p>
		<p>
		    Please note that Google Apps does not send a SAML logout request.
		</p>
		<p>
		    After logout you would normally redirect the user to the home page.
		</p>
    </div>
    </form>
</body>
</html>
