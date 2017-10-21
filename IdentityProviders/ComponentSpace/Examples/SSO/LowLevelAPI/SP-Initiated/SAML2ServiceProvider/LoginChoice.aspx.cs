using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Bindings;
using ComponentSpace.SAML2.Profiles.SSOBrowser;

namespace SAML2ServiceProvider
{
    public partial class LoginChoice : System.Web.UI.Page
    {
        // The login can either occur at the identity provider (SSO) or the service provider (local login).
        private class LoginLocations
        {
            public const string IdentityProvider = "IdP";
            public const string ServiceProvider = "SP";
        }

        // The query string parameter identifying the IdP to SP binding in use.
        private const string bindingQueryParameter = "binding";

        // The query string parameter indicating an error occurred.
        private const string errorQueryParameter = "error";

        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Create the assertion consumer service URL.
        // Rather than have different endpoints for each binding we use the same endpoint and
        // identify the binding type by a query string parameter.
        private string CreateAssertionConsumerServiceURL()
        {
            return string.Format("{0}?{1}={2}", CreateAbsoluteURL("~/SAML/AssertionConsumerService.aspx"), bindingQueryParameter, HttpUtility.UrlEncode(idpToSPBindingRadioButtonList.SelectedValue));
        }

        // Create the SSO service URL.
        // Rather than have different endpoints for each binding we use the same endpoint and
        // identify the binding type by a query string parameter.
        private string CreateSSOServiceURL()
        {
            return string.Format("{0}?{1}={2}", WebConfigurationManager.AppSettings["idpssoURL"], bindingQueryParameter, HttpUtility.UrlEncode(spToIdPBindingRadioButtonList.SelectedValue));
        }

        // Create an authentication request.
        private XmlElement CreateAuthnRequest()
        {
            // Create some URLs to identify the service provider to the identity provider.
            // As we're using the same endpoint for the different bindings, add a query string parameter
            // to identify the binding.
            string issuerURL = CreateAbsoluteURL("~/");
            string assertionConsumerServiceURL = CreateAssertionConsumerServiceURL();

            // Create the authentication request.
            AuthnRequest authnRequest = new AuthnRequest();
            authnRequest.Destination = WebConfigurationManager.AppSettings["idpssoURL"];
            authnRequest.Issuer = new Issuer(issuerURL);
            authnRequest.ForceAuthn = false;
            authnRequest.NameIDPolicy = new NameIDPolicy(null, null, true);
            authnRequest.ProtocolBinding = idpToSPBindingRadioButtonList.SelectedValue;
            authnRequest.AssertionConsumerServiceURL = assertionConsumerServiceURL;

            // Serialize the authentication request to XML for transmission.
            XmlElement authnRequestXml = authnRequest.ToXml();

            // Don't sign if using HTTP redirect as the generated query string is too long for most browsers.
            if (spToIdPBindingRadioButtonList.SelectedValue != SAMLIdentifiers.BindingURIs.HTTPRedirect)
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

            // Create and cache the relay state so we remember which SP resource the user wishes 
            // to access after SSO.
            string spResourceURL = CreateAbsoluteURL(FormsAuthentication.GetRedirectUrl("", false));
            string relayState = RelayStateCache.Add(new RelayState(spResourceURL, null));

            // Send the authentication request to the identity provider over the selected binding.
            string idpURL = CreateSSOServiceURL();

            switch (spToIdPBindingRadioButtonList.SelectedValue)
            {
                case SAMLIdentifiers.BindingURIs.HTTPRedirect:
                    X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                    ServiceProvider.SendAuthnRequestByHTTPRedirect(Response, idpURL, authnRequestXml, relayState, x509Certificate.PrivateKey);

                    break;
                case SAMLIdentifiers.BindingURIs.HTTPPost:
                    ServiceProvider.SendAuthnRequestByHTTPPost(Response, idpURL, authnRequestXml, relayState);

                    // Don't send this form.
                    Response.End();

                    break;
                case SAMLIdentifiers.BindingURIs.HTTPArtifact:
                    // Create the artifact.
                    string identificationURL = CreateAbsoluteURL("~/");
                    HTTPArtifactType4 httpArtifact = new HTTPArtifactType4(HTTPArtifactType4.CreateSourceId(identificationURL), HTTPArtifactType4.CreateMessageHandle());

                    // Cache the authentication request for subsequent sending using the artifact resolution protocol.
                    HTTPArtifactState httpArtifactState = new HTTPArtifactState(authnRequestXml, null);
                    HTTPArtifactStateCache.Add(httpArtifact, httpArtifactState);

                    // Send the artifact.
                    ServiceProvider.SendArtifactByHTTPArtifact(Response, idpURL, httpArtifact, relayState, false);
                    break;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Display any error message resulting from a failed login.
            if (!string.IsNullOrEmpty(Request.QueryString[errorQueryParameter]))
            {
                errorMessageLabel.Text = Request.QueryString[errorQueryParameter];
            }
            else
            {
                errorMessageLabel.Text = string.Empty;
            }
        }

        protected void continueButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool idpLogin = loginLocationRadioButtonList.SelectedValue == LoginLocations.IdentityProvider;

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
                Trace.Write("SP", "Error on login choice page", exception);
            }
        }

        protected void loginLocationRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // The binding selections only apply if performing a SAML SSO at the identity provider.
            bool idpLogin = loginLocationRadioButtonList.SelectedValue == LoginLocations.IdentityProvider;

            spToIdPBindingRadioButtonList.Enabled = idpLogin;
            idpToSPBindingRadioButtonList.Enabled = idpLogin;
        }
    }
}
