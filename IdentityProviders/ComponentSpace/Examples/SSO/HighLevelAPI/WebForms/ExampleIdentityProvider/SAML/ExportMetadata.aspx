<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportMetadata.aspx.cs" Inherits="ExampleIdentityProvider.SAML.ExportMetadata" %>

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
                if ($('#errorMessageLabel').length) {
                    $('#errorMessageLabel').hide();
                }
            });
        });
    </script>

    <form id="metadataForm" runat="server">
    <div>
		<h1>Export SAML Metadata</h1>
		<p>
            Download the local SAML metadata as an XML file so it may be supplied to a partner provider.
		</p>
        <div id="partnerSelectionDiv" runat="server" visible="false">
		    <p>
                Local SAML metadata may be different depending on the partner provider.
		    </p>
            <p>
			    Select the partner provider to receive the local metadata.
		    </p>
            <p>
                <asp:DropDownList ID="partnerNameDropDownList" runat="server">
                </asp:DropDownList>
            </p>
        </div>
        <asp:Button ID="downloadButton" runat="server" Text="Download Metadata" OnClick="DownloadButton_Click" />
        <br/>
		<p>
			<asp:Label ID="errorMessageLabel" runat="server" ForeColor="Red"></asp:Label>
		</p>    
    </div>
    </form>
</body>
</html>
