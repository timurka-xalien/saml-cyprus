using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Profiles.SingleLogout;

namespace SAML2IdentityProvider.SAML
{
    public partial class SLOService : System.Web.UI.Page
    {
        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Create a logout response.
        private LogoutResponse CreateLogoutResponse()
        {
            Trace.Write("IdP", "Creating logout response.");

            LogoutResponse logoutResponse = new LogoutResponse();
            logoutResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Success, null);
            logoutResponse.Issuer = new Issuer(CreateAbsoluteURL("~/"));

            Trace.Write("IdP", "Created logout response.");

            return logoutResponse;
        }

        // Send the logout response.
        private void SendLogoutResponse(LogoutResponse logoutResponse, string relayState)
        {
            Trace.Write("IdP", "Sending logout response.");

            // Serialize the logout response for transmission.
            XmlElement logoutResponseXml = logoutResponse.ToXml();

            // Send the logout response over HTTP redirect.
            X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.IdPX509Certificate];
            SingleLogoutService.SendLogoutResponseByHTTPRedirect(Response, WebConfigurationManager.AppSettings["spLogoutURL"], logoutResponseXml, null, x509Certificate.PrivateKey, null);

            Trace.Write("IdP", "Sent logout response.");
        }

        private void ProcessLogoutRequest(LogoutRequest logoutRequest, string relayState)
        {
            Trace.Write("IdP", "Processing logout request");

            // Logout locally.
            FormsAuthentication.SignOut();
            Session.Abandon();

            // Create a logout response.
            LogoutResponse logoutResponse = CreateLogoutResponse();

            // Send the logout response.
            SendLogoutResponse(logoutResponse, null);
        }

        private void ProcessLogoutResponse(LogoutResponse logoutResponse, string relayState)
        {
            Trace.Write("IdP", "Processing logout response");

            // Redirect to the default page.
            Response.Redirect("~/", false);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("IdP", "Single Logout Service");

                // Receive the logout request or response.
                XmlElement logoutMessage = null;
                string relayState = null;
                bool isRequest = false;
                bool signed = false;
                X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                SingleLogoutService.ReceiveLogoutMessageByHTTPRedirect(Request, out logoutMessage, out relayState, out isRequest, out signed, x509Certificate.PublicKey.Key);

                if (isRequest)
                {
                    ProcessLogoutRequest(new LogoutRequest(logoutMessage), relayState);
                }
                else
                {
                    ProcessLogoutResponse(new LogoutResponse(logoutMessage), relayState);
                }

            }
            catch (Exception exception)
            {
                Trace.Write("IdP", "Error in single logout service.", exception);
            }
        }
    }
}
