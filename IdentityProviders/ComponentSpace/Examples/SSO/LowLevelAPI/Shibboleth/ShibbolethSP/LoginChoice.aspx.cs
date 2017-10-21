using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Bindings;
using ComponentSpace.SAML2.Profiles.ArtifactResolution;
using ComponentSpace.SAML2.Profiles.SSOBrowser;

namespace ShibbolethSP
{
    public partial class LoginChoice : System.Web.UI.Page
    {
        // The login can either occur at the identity provider (SSO) or the service provider (local login).
        private class LoginLocations
        {
            public const string IdentityProvider = "IdP";
            public const string ServiceProvider = "SP";
        }

        // The query string parameter indicating an error occurred.
        private const string errorQueryParameter = "error";

        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Create an authentication request.
        private XmlElement CreateAuthnRequest()
        {
            // Create the authentication request.
            AuthnRequest authnRequest = new AuthnRequest();
            authnRequest.Destination = Configuration.SingleSignOnServiceURL;
            authnRequest.Issuer = new Issuer(CreateAbsoluteURL("~/"));
            authnRequest.ForceAuthn = false;
            authnRequest.NameIDPolicy = new NameIDPolicy(null, null, true);

            // Serialize the authentication request to XML for transmission.
            XmlElement authnRequestXml = authnRequest.ToXml();

            // Don't sign if using HTTP redirect as the generated query string is too long for most browsers.        
            if (Configuration.SingleSignOnServiceBinding != SAMLIdentifiers.Binding.HTTPRedirect)
            {
                // Sign the authentication request.
                X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                SAMLMessageSignature.Generate(authnRequestXml, x509Certificate.PrivateKey, x509Certificate);
            }

            return authnRequestXml;
        }

        // Initiate the SSO by sending an authentication request to the identity provider.
        private void RequestLoginAtIdentityProvider()
        {
            // Create the authentication request.
            XmlElement authnRequestXml = CreateAuthnRequest();

            // Create and cache the relay state so we remember which SP resource the user wishes to access after SSO.
            string spResourceURL = CreateAbsoluteURL(FormsAuthentication.GetRedirectUrl("", false));
            string relayState = RelayStateCache.Add(new RelayState(spResourceURL, null));

            // Send the authentication request to the identity provider over the configured binding.
            switch (Configuration.SingleSignOnServiceBinding)
            {
                case SAMLIdentifiers.Binding.HTTPRedirect:
                    X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                    ServiceProvider.SendAuthnRequestByHTTPRedirect(Response, Configuration.SingleSignOnServiceURL, authnRequestXml, relayState, x509Certificate.PrivateKey);
                    break;

                case SAMLIdentifiers.Binding.HTTPPost:
                    ServiceProvider.SendAuthnRequestByHTTPPost(Response, Configuration.SingleSignOnServiceURL, authnRequestXml, relayState);

                    // Don't send this form.
                    Response.End();
                    break;

                case SAMLIdentifiers.Binding.HTTPArtifact:
                    // Create the artifact.
                    string identificationURL = CreateAbsoluteURL("~/");
                    HTTPArtifactType4 httpArtifact = new HTTPArtifactType4(HTTPArtifactType4.CreateSourceId(identificationURL), HTTPArtifactType4.CreateMessageHandle());

                    // Cache the authentication request for subsequent sending using the artifact resolution protocol.
                    HTTPArtifactState httpArtifactState = new HTTPArtifactState(authnRequestXml, null);
                    HTTPArtifactStateCache.Add(httpArtifact, httpArtifactState);

                    // Send the artifact.
                    ServiceProvider.SendArtifactByHTTPArtifact(Response, Configuration.SingleSignOnServiceURL, httpArtifact, relayState, false);
                    break;

                default:
                    throw new ArgumentException("Invalid binding type");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Display any error message resulting from a failed login.
                if (!String.IsNullOrEmpty(Request.QueryString[errorQueryParameter]))
                {
                    errorMessageLabel.Text = Request.QueryString[errorQueryParameter];
                }
                else
                {
                    errorMessageLabel.Text = String.Empty;
                }
            }

            catch (Exception exception)
            {
                Trace.Write("SP", "Error in login choice page load", exception);
            }
        }

        protected void continueButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool idpLogin = (loginLocationRadioButtonList.SelectedValue == LoginLocations.IdentityProvider);

                if (idpLogin)
                {
                    // Initiate SSO using the Web Browser SSO profile.
                    RequestLoginAtIdentityProvider();
                }
                else
                {
                    // Perform a local login at the service provider.
                    Response.Redirect("~/SAML/LocalLogin.aspx", false);
                }
            }

            catch (ThreadAbortException)
            {
                // Ignore this exception as it's generated by the HttpResponse.End call.
            }

            catch (Exception exception)
            {
                Trace.Write("SP", "Error in login choice page continue button click", exception);
            }
        }
    }
}
