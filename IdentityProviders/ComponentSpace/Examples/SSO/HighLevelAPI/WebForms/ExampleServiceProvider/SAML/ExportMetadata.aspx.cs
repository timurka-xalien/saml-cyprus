using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Metadata;

namespace ExampleServiceProvider.SAML
{
    public partial class ExportMetadata : System.Web.UI.Page
    {
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Ensure the SAML configuration is loaded.
                SAMLController.Initialize();

                // Display the partner identity provider descriptions.
                partnerSelectionDiv.Visible = SAMLController.Configuration.PartnerIdentityProviderConfigurations.Count > 0;

                partnerNameDropDownList.Items.Add(new ListItem("--- Any ---", ""));

                foreach (PartnerIdentityProviderConfiguration partnerIdentityProviderConfiguration in SAMLController.Configuration.PartnerIdentityProviderConfigurations)
                {
                    string description = partnerIdentityProviderConfiguration.Description;

                    if (string.IsNullOrEmpty(description))
                    {
                        description = partnerIdentityProviderConfiguration.Name;
                    }

                    partnerNameDropDownList.Items.Add(new ListItem(description, partnerIdentityProviderConfiguration.Name));
                }
            }

            catch (Exception)
            {
                errorMessageLabel.Text = "The SAML configuration couldn't be loaded.";
            }
        }

        protected void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                string partnerName = partnerNameDropDownList.SelectedValue;

                // Get the X.509 certificate.
                IList<X509Certificate2> x509Certificates = SAMLController.CertificateManager.GetLocalServiceProviderSignatureCertificates(SAMLController.ConfigurationID, partnerName);
                X509Certificate2 x509Certificate = null;

                if (x509Certificates.Count > 0)
                {
                    x509Certificate = x509Certificates[0];
                }

                // Export the configuration as SAML metadata.
                EntityDescriptor entityDescriptor =
                    MetadataExporter.Export(
                        SAMLController.Configuration,
                        x509Certificate, null,
                        CreateAbsoluteURL("~/SAML/AssertionConsumerService.aspx"), CreateAbsoluteURL("~/SAML/SLOService.aspx"),
                        partnerName);

                // Convert the SAML metadata to XML ready for downloading.
                XmlElement metadataElement = entityDescriptor.ToXml();

                // Download the SAML metadata.
                Response.Clear();
                Response.ContentType = "text/xml";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"metadata.xml\"");

                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(Response.OutputStream, Encoding.UTF8))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    metadataElement.OwnerDocument.Save(xmlTextWriter);
                }

                Response.End();
            }

            catch (Exception exception)
            {
                errorMessageLabel.Text = string.Format("An error occurred exporting the SAML configuration.<br/>{0}", exception.ToString());
            }
        }
    }
}