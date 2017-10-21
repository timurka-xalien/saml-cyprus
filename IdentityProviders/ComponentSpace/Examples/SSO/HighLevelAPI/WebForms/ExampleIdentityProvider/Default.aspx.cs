using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Security;

using ComponentSpace.SAML2;

namespace ExampleIdentityProvider
{
    public static class AppSettings
    {
        public const string Attribute = "Attribute";
        public const string PartnerSP = "PartnerSP";
        public const string SubjectName = "SubjectName";
        public const string TargetUrl = "TargetUrl";
    }

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void ssoLinkButton_Click(object sender, EventArgs e)
        {
            // Initiate single sign-on to the service provider (IdP-initiated SSO)
            // by sending a SAML response containing a SAML assertion to the SP.
            // Use the configured or logged in user name as the user name to send to the service provider (SP).
            // Include some user attributes.
            // If a target URL is specified the SP should display this page once SSO completes.
            string partnerSP = WebConfigurationManager.AppSettings[AppSettings.PartnerSP];
            string targetUrl = WebConfigurationManager.AppSettings[AppSettings.TargetUrl];

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

            SAMLIdentityProvider.InitiateSSO(
                Response,
                userName,
                attributes,
                targetUrl,
                partnerSP);
        }

        protected void logoutButton_Click(object sender, EventArgs e)
        {
            // Logout locally.
            FormsAuthentication.SignOut();

            if (SAMLIdentityProvider.CanSLO())
            {
                // Request logout at the service providers.
                SAMLIdentityProvider.InitiateSLO(Response, null, null);
            }
            else
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}
