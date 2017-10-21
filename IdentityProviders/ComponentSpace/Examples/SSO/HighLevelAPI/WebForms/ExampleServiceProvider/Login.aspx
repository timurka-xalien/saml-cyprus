<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ExampleServiceProvider.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Service Provider Login</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="loginForm" runat="server">
    <div>
		<h1>Login to the Service Provider</h1>
		<p>
            <asp:LinkButton ID="ssoLinkButton" runat="server" OnClick="ssoLinkButton_Click" ToolTip="Initiates SAML single sign-on to the identity provider.">SSO to the Identity Provider</asp:LinkButton>
		</p>
    </div>
    </form>
</body>
</html>
