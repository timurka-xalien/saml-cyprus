using System;
using System.Web.Security;

using ComponentSpace.SAML2;

namespace ExampleServiceProvider.SAML
{
    public partial class SLOService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by the identity provider.
            // If a response is received then this is in response to single logout having been initiated by the service provider.
            bool isRequest = false;
            string logoutReason = null;
            string partnerIdP = null;
            string relayState = null;

            SAMLServiceProvider.ReceiveSLO(Request, out isRequest, out logoutReason, out partnerIdP, out relayState);

            if (isRequest)
            {
                // Logout locally.
                FormsAuthentication.SignOut();

                // Respond to the IdP-initiated SLO request indicating successful logout.
                SAMLServiceProvider.SendSLO(Response, null);
            }
            else
            {
                // SP-initiated SLO has completed.
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}