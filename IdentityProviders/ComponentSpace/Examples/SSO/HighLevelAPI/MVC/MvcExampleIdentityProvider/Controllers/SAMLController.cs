using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Utility;

namespace MvcExampleIdentityProvider.Controllers
{
    public class SAMLController : Controller
    {
        private const string ssoPendingSessionKey = "ssoPending";

        public ActionResult SSOService()
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
                    return new EmptyResult();
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

            return new EmptyResult();
        }

        public ActionResult SLOService()
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by the service provider.
            // If a response is received then this is in response to single logout having been initiated by the identity provider.
            bool isRequest = false;
            bool hasCompleted = false;
            string logoutReason = null;
            string partnerSP = null;
            string relayState = null;

            SAMLIdentityProvider.ReceiveSLO(Request, Response, out isRequest, out hasCompleted, out logoutReason, out partnerSP, out relayState);

            if (isRequest)
            {
                // Logout locally.
                FormsAuthentication.SignOut();

                // Respond to the SP-initiated SLO request indicating successful logout.
                SAMLIdentityProvider.SendSLO(Response, null);
            }
            else
            {
                if (hasCompleted)
                {
                    // IdP-initiated SLO has completed.
                    Response.Redirect("~/");
                }
            }

            return new EmptyResult();
        }

        public ActionResult ECP()
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

            return new EmptyResult();
        }
    }
}
