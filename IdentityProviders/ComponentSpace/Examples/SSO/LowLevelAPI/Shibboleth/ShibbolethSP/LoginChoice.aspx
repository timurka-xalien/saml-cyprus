<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginChoice.aspx.cs" Inherits="ShibbolethSP.LoginChoice" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Login Selection</title>
	<link href="~/Site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="loginChoiceForm" runat="server" defaultbutton="continueButton">
    <div>
		<h1>Login Selection</h1>
        <p>This example demonstrates SP-initiated SSO.</p>
		<p>The bindings used for SSO are specified by configuration.</p>
		<p>Select the type of login to perform and click the Continue button.</p>
		<h2>Login Location</h2>
		<p>
			You can either login at the identity provider (IdP) and have the asserted identity
			supplied to the service provider (SP) using SAML 2.0 or login directly at the service provider
			which doesn't involve SAML 2.0.
		</p>
		<asp:RadioButtonList ID="loginLocationRadioButtonList" runat="server" AutoPostBack="True">
			<asp:ListItem Value="IdP" Selected="True">Identity Provider</asp:ListItem>
			<asp:ListItem Value="SP">Service Provider</asp:ListItem>
		</asp:RadioButtonList>
		<br />
		<asp:Button ID="continueButton" runat="server" Text="Continue" OnClick="continueButton_Click" />
		<p>
			<asp:Label ID="errorMessageLabel" runat="server" ForeColor="Red"></asp:Label>
		</p>    
	</div>
    </form>
</body>
</html>
