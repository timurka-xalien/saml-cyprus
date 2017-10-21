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

namespace SAML2ServiceProvider
{
    public partial class _Default : System.Web.UI.Page
    {
        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void logoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a logout request.
                LogoutRequest logoutRequest = new LogoutRequest();
                logoutRequest.Issuer = new Issuer(CreateAbsoluteURL("~/"));
                logoutRequest.NameID = new NameID(Context.User.Identity.Name);

                // Serialize the logout request to XML for transmission.
                XmlElement logoutRequestXml = logoutRequest.ToXml();

                // Send the logout request to the IdP over HTTP redirect.
                string logoutURL = WebConfigurationManager.AppSettings["idpLogoutURL"];
                X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                SingleLogoutService.SendLogoutRequestByHTTPRedirect(Response, logoutURL, logoutRequestXml, null, x509Certificate.PrivateKey, null);

                // Logout locally.
                FormsAuthentication.SignOut();
                Session.Abandon();

            }
            catch (Exception exception)
            {
                Trace.Write("SP", "Error on logout page", exception);
            }
        }
    }
}
