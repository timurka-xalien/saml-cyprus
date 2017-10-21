using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

using ComponentSpace.SAML2;

namespace ExampleServiceProvider
{
    public partial class _Default : System.Web.UI.Page
    {
        private class AttributeDataSource
        {
            private string attributeName;
            private string attributeValue;

            public static IList<AttributeDataSource> Get(IDictionary<string, string> attributes)
            {
                IList<AttributeDataSource> attributeDataSources = new List<AttributeDataSource>();

                foreach (string attributeName in attributes.Keys)
                {
                    attributeDataSources.Add(new AttributeDataSource(attributeName, HttpUtility.HtmlEncode(attributes[attributeName])));
                }

                return attributeDataSources;
            }

            private AttributeDataSource(string attributeName, string attributeValue)
            {
                this.attributeName = attributeName;
                this.attributeValue = attributeValue;
            }

            public string AttributeName
            {
                get
                {
                    return attributeName;
                }
            }

            public string AttributeValue
            {
                get
                {
                    return attributeValue;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Display the attributes returned by the identity provider.
            IDictionary<string, string> attributes = (IDictionary<string, string>)Session[SAML.AssertionConsumerService.AttributesSessionKey];

            if (attributes != null && attributes.Count > 0)
            {
                attributesDiv.Visible = true;
                attributesRepeater.DataSource = AttributeDataSource.Get(attributes);
                attributesRepeater.DataBind();
            }
        }

        protected void logoutButton_Click(object sender, EventArgs e)
        {
            // Logout locally.
            FormsAuthentication.SignOut();

            if (SAMLServiceProvider.CanSLO(WebConfigurationManager.AppSettings[AppSettings.PartnerIdP]))
            {
                // Request logout at the identity provider.
                string partnerIdP = WebConfigurationManager.AppSettings[AppSettings.PartnerIdP];
                SAMLServiceProvider.InitiateSLO(Response, null, null, partnerIdP);
            }
            else
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}
