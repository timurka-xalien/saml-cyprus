using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Metadata;

namespace ExampleIdentityProvider.SAML
{
    public partial class ImportMetadata : System.Web.UI.Page
    {
        bool importSuccessful;
        IList<string> messages = new List<string>();

        private EntityDescriptor LoadMetadata()
        {
            try
            {
                if (!fileUpload.HasFile)
                {
                    return null;
                }

                XmlDocument xmlDocument = new XmlDocument()
                {
                    PreserveWhitespace = true
                };

                xmlDocument.Load(fileUpload.FileContent);

                return new EntityDescriptor(xmlDocument.DocumentElement);
            }

            catch (Exception exception)
            {
                throw new ArgumentException("The SAML metadata couldn't be loaded.", exception);
            }
        }
        private void ShowResults()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (string message in messages)
            {
                stringBuilder.Append(message);
                stringBuilder.Append("<br/>");
            }

            messageLabel.ForeColor = importSuccessful ? Color.Black : Color.Red;
            messageLabel.Text = stringBuilder.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure the SAML configuration is loaded.
            SAMLController.Initialize();
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Upload the SAML metadata.
                EntityDescriptor entityDescriptor = LoadMetadata();

                if (entityDescriptor == null)
                {
                    return;
                }

                // Import the SAML metadata and update the SAML configuration.
                MetadataImporter.Import(entityDescriptor, SAMLController.Configuration, null);

                // Save the SAML configuration.
                string samlConfigFileName = Server.MapPath("~/App_Data/saml.config");
                SAMLConfigurationFile.Save(SAMLController.Configuration, samlConfigFileName);

                // Show the results.
                importSuccessful = true;
                messages.Add(string.Format("The updated SAML configuration has been successfully saved to {0}.", samlConfigFileName));
                ShowResults();
            }

            catch (Exception exception)
            {
                messages.Add("An error occurred importing the SAML metadata.");
                messages.Add(exception.ToString());
                ShowResults();
            }
        }
    }
}