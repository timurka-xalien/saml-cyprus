<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LocalLogin.aspx.cs" Inherits="SAML2ServiceProvider.SAML.LocalLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Service Provider Login</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
</head>
<body>
    <form id="localLoginForm" runat="server" defaultbutton="loginButton" defaultfocus="passwordTextBox">
    <div>
		<h1>Login to the Service Provider</h1>
		<table border="0" cellpadding="2">
			<tr>
				<td>
					<p>User name:</p>
				</td>
				<td>
					<asp:TextBox ID="usernameTextBox" runat="server">sp-user</asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>
					<p>Password:</p>
				</td>
				<td>
					<asp:TextBox ID="passwordTextBox" runat="server" TextMode="Password"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td align="right" colspan="2">
					<asp:Button ID="loginButton" runat="server" Text="Login" OnClick="loginButton_Click" />
				</td>
			</tr>
		</table>
		<p>
			<asp:Label ID="errorMessageLabel" runat="server" ForeColor="Red"></asp:Label>
		</p>    
    </div>
    </form>
</body>
</html>

