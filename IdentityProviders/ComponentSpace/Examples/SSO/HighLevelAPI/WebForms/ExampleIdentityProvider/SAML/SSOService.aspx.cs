using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Security;

using ComponentSpace.SAML2;

namespace ExampleIdentityProvider.SAML
{
    public partial class SSOService : System.Web.UI.Page
    {
        private const string ssoPendingSessionKey = "ssoPending";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Either an authn request has been received or login has just completed in response to a previous authn request.
            // The SSO pending session flag is false if an authn request is expected. Otherwise, it is true if
            // a login has just completed and control is being returned to this page.
            bool ssoPending = Session[ssoPendingSessionKey] != null && (bool)Session[ssoPendingSessionKey] == true;

            if (!(ssoPending && User.Identity.IsAuthenticated))
            {
                string partnerSP = null;

                // Receive the authn request from the service provider (SP-initiated SSO).
                SAMLIdentityProvider.ReceiveSSO(Request, out partnerSP);

                // If the user isn't logged in at the identity provider, force the user to login.
                if (!User.Identity.IsAuthenticated)
                {
                    Session[ssoPendingSessionKey] = true;
                    FormsAuthentication.RedirectToLoginPage();
                    return;
                }
            }

            Session[ssoPendingSessionKey] = null;

            // The user is logged in at the identity provider.
            // Respond to the authn request by sending a SAML response containing a SAML assertion to the SP.
            // Use the configured or logged in user name as the user name to send to the service provider (SP).
            // Include some user attributes.
            string userName = WebConfigurationManager.AppSettings[AppSettings.SubjectName];

            if (string.IsNullOrEmpty(userName))
            {
                userName = User.Identity.Name;
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
