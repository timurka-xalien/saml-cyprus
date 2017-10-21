<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="GoogleIdP.ChangePassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Identity Provider Change Password</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="changePasswordForm" runat="server">
    <div>
		<h1>Change Password at the Identity Provider</h1>
		<p>
			Users are redirected to this page from within Google Apps 
			so they may change their password at the identity provider. 
		</p>
		<p>
		    Password changing is not supported in this example application.
		</p>
    </div>
    </form>
</body>
</html>
