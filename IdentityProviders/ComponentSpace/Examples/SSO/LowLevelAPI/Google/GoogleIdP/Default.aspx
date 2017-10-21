<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GoogleIdP._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
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
	</div>
    </form>
</body>
</html>
