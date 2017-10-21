using System;
using System.Collections.Generic;
using System.Web.Configuration;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Utility;

namespace ExampleIdentityProvider.SAML
{
    public partial class ECP : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Receive an authn request from an enhanced client or proxy (ECP).
            string partnerSP = null;

            SAMLIdentityProvider.ReceiveSSO(Request, out partnerSP);

            // In this example, the user's credentials are assumed to be included in the HTTP authorization header.
            // The application should authenticate the user against some user registry.
            // In this example, the credentials are assumed to be valid and no check is made.
            string userName = null;
            string password = null;

            HttpBasicAuthentication.GetAuthorizationHeader(Request, out userName, out password);

            // Respond to the authn request by sending a SAML response containing a SAML assertion to the SP.
            // Use the configured or logged in user name as the user name to send to the service provider (SP).
            // Include some user attributes.
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings[AppSettings.SubjectName]))
            {
                userName = WebConfigurationManager.AppSettings[AppSettings.SubjectName];
            }

            IDictionary<string, string> attributes = new Dictionary<string, string>();

            foreach (string key in WebConfigurationManager.AppSettings.Keys)
            {
                if (key.StartsWith(AppSettings.Attribute))
                {
                    attributes[key.Substring(AppSettings.Attribute.Length + 1)] = WebConfigurationManager.AppSettings[key];
                }
            }

            SAMLIdentityProvider.SendSSO(Response, userName, attributes);
        }
    }
}