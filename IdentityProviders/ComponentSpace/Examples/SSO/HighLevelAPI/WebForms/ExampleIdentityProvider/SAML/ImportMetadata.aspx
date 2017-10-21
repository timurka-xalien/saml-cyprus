<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportMetadata.aspx.cs" Inherits="ExampleIdentityProvider.SAML.ImportMetadata" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Identity Provider</title>
	<link href="~/Site.css" type="text/css" rel="stylesheet"/>
    <script type="text/javascript" src="/Scripts/jquery-1.11.3.min.js"></script> 
</head>
<body>
    <script language="javascript" type="text/javascript">
        $(document).ready(function () {
            $("form").submit(function () {
                if ($('#messageLabel').length) {
                    $('#messageLabel').hide();
                }
            });
        });
    </script>

    <form id="metadataForm" runat="server">
    <div>
		<h1>Import SAML Metadata</h1>
		<p>
            Upload the partner SAML metadata XML file so it is included in the SAML configuration.
		</p>
        <p>
            <asp:FileUpload ID="fileUpload" runat="server" Width="313px" />
		</p>
        <asp:Button ID="uploadButton" runat="server" Text="Upload Metadata" OnClick="UploadButton_Click" />
        <br/>
        <br/>
		<p>
			<asp:Label ID="messageLabel" runat="server"></asp:Label>
		</p>    
    </div>
    </form>
</body>
</html>
