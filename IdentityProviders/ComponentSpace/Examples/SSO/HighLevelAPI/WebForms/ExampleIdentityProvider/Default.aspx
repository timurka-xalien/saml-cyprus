<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ExampleIdentityProvider._Default" %>

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
            <asp:LinkButton ID="ssoLinkButton" runat="server" OnClick="ssoLinkButton_Click" ToolTip="Initiates SAML single sign-on to the service provider.">SSO to the Service Provider</asp:LinkButton>
		</p>
        <asp:Button ID="logoutButton" runat="server" Text="Logout" OnClick="logoutButton_Click" />
    </div>
    </form>
</body>
</html>
