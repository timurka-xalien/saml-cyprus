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
using ComponentSpace.SAML2.Bindings;
using ComponentSpace.SAML2.Profiles.ArtifactResolution;
using ComponentSpace.SAML2.Profiles.SSOBrowser;

namespace SAML2IdentityProvider.SAML
{
    public partial class SSOService : System.Web.UI.Page
    {
        // The query string parameter identifying the SP to IdP binding in use.
        private const string bindingQueryParameter = "binding";

        // The session key for saving the SSO state during a local login.
        private const string ssoSessionKey = "sso";

        // The SSO state saved during a local login.
        private class SSOState
        {
            public AuthnRequest authnRequest;
            public string relayState;
            public SAMLIdentifiers.Binding idpProtocolBinding;
            public string assertionConsumerServiceURL;
        }

        // Creates an absolute URL from an application relative URL
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Receive the authentication request from the service provider.
        private void ReceiveAuthnRequest(out AuthnRequest authnRequest, out string relayState)
        {
            // Determine the service provider to identity provider binding type.
            // We use a query string parameter rather than having separate endpoints per binding.
            string bindingType = Request.QueryString[bindingQueryParameter];

            Trace.Write("IdP", "Receiving authentication request over binding " + bindingType);

            // Receive the authentication request.
            XmlElement authnRequestXml = null;

            switch (bindingType)
            {
                case SAMLIdentifiers.BindingURIs.HTTPRedirect:
                    bool signed = false;
                    X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                    IdentityProvider.ReceiveAuthnRequestByHTTPRedirect(Request, out authnRequestXml, out relayState, out signed, x509Certificate.PublicKey.Key);
                    break;

                case SAMLIdentifiers.BindingURIs.HTTPPost:
                    IdentityProvider.ReceiveAuthnRequestByHTTPPost(Request, out authnRequestXml, out relayState);
                    break;

                case SAMLIdentifiers.BindingURIs.HTTPArtifact:
                    // Receive the artifact.
                    HTTPArtifact httpArtifact = null;

                    IdentityProvider.ReceiveArtifactByHTTPArtifact(Request, false, out httpArtifact, out relayState);

                    // Create an artifact resolve request.
                    ArtifactResolve artifactResolve = new ArtifactResolve();
                    artifactResolve.Issuer = new Issuer(CreateAbsoluteURL("~/"));
                    artifactResolve.Artifact = new Artifact(httpArtifact.ToString());

                    XmlElement artifactResolveXml = artifactResolve.ToXml();

                    // Send the artifact resolve request and receive the artifact response.
                    string spArtifactResponderURL = WebConfigurationManager.AppSettings["spArtifactResponderURL"];

                    XmlElement artifactResponseXml = ArtifactResolver.SendRequestReceiveResponse(spArtifactResponderURL, artifactResolveXml);

                    ArtifactResponse artifactResponse = new ArtifactResponse(artifactResponseXml);

                    // Extract the authentication request from the artifact response.
                    authnRequestXml = artifactResponse.SAMLMessage;
                    break;

                default:
                    throw new ArgumentException("Invalid service provider to identity provider binding");
            }

            // If using HTTP redirect the message isn't signed as the generated query string is too long for most browsers.
            if (bindingType != SAMLIdentifiers.BindingURIs.HTTPRedirect)
            {
                if (SAMLMessageSignature.IsSigned(authnRequestXml))
                {
                    // Verify the request's signature.
                    X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.SPX509Certificate];

                    if (!SAMLMessageSignature.Verify(authnRequestXml, x509Certificate))
                    {
                        throw new ArgumentException("The authentication request signature failed to verify.");
                    }
                }
            }

            // Deserialize the XML.
            authnRequest = new AuthnRequest(authnRequestXml);

            Trace.Write("IdP", "Received authentication request");
        }

        // Indicate whether a local login is required.
        private bool IsLocalLoginRequired(bool forceAuthn)
        {
            bool requireLocalLogin = false;

            if (forceAuthn)
            {
                requireLocalLogin = true;
            }
            else
            {
                if (!User.Identity.IsAuthenticated)
                {
                    requireLocalLogin = true;
                }
            }

            return requireLocalLogin;
        }

        // Create a SAML response with the user's local identity, if any, or indicating an error.
        private SAMLResponse CreateSAMLResponse(SSOState ssoState)
        {
            Trace.Write("IdP", "Creating SAML response");

            SAMLResponse samlResponse = new SAMLResponse();
            string issuerURL = CreateAbsoluteURL("~/");
            Issuer issuer = new Issuer(issuerURL);
            samlResponse.Issuer = issuer;

            if (User.Identity.IsAuthenticated)
            {
                samlResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Success, null);

                SAMLAssertion samlAssertion = new SAMLAssertion();
                samlAssertion.Issuer = issuer;

                Subject subject = new Subject(new NameID(User.Identity.Name));
                SubjectConfirmation subjectConfirmation = new SubjectConfirmation(SAMLIdentifiers.SubjectConfirmationMethods.Bearer);
                SubjectConfirmationData subjectConfirmationData = new SubjectConfirmationData();
                subjectConfirmationData.InResponseTo = ssoState.authnRequest.ID;
                subjectConfirmationData.Recipient = ssoState.assertionConsumerServiceURL;
                subjectConfirmation.SubjectConfirmationData = subjectConfirmationData;
                subject.SubjectConfirmations.Add(subjectConfirmation);
                samlAssertion.Subject = subject;

                AuthnStatement authnStatement = new AuthnStatement();
                authnStatement.AuthnContext = new AuthnContext();
                authnStatement.AuthnContext.AuthnContextClassRef = new AuthnContextClassRef(SAMLIdentifiers.AuthnContextClasses.Password);
                samlAssertion.Statements.Add(authnStatement);

                // Attributes may be included in the SAML assertion.
                AttributeStatement attributeStatement = new AttributeStatement();
                attributeStatement.Attributes.Add(new SAMLAttribute("Membership", SAMLIdentifiers.AttributeNameFormats.Basic, null, "Gold"));
                samlAssertion.Statements.Add(attributeStatement);

                samlResponse.Assertions.Add(samlAssertion);
            }
            else
            {
                samlResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Responder, SAMLIdentifiers.SecondaryStatusCodes.AuthnFailed, "The user is not authenticated at the identity provider");
            }

            Trace.Write("IdP", "Created SAML response");

            return samlResponse;
        }

        // Send the SAML response over the specified binding.
        private void SendSAMLResponse(SAMLResponse samlResponse, SSOState ssoState)
        {
            Trace.Write("IdP", "Sending SAML response");

            // Serialize the SAML response for transmission.
            XmlElement samlResponseXml = samlResponse.ToXml();

            // Sign the SAML response 
            X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.IdPX509Certificate];

            SAMLMessageSignature.Generate(samlResponseXml, x509Certificate.PrivateKey, x509Certificate);

            // Send the SAML response to the service provider.
            switch (ssoState.idpProtocolBinding)
            {
                case SAMLIdentifiers.Binding.HTTPPost:
                    IdentityProvider.SendSAMLResponseByHTTPPost(Response, ssoState.assertionConsumerServiceURL, samlResponseXml, ssoState.relayState);
                    break;

                case SAMLIdentifiers.Binding.HTTPArtifact:
                    // Create the artifact.
                    string identificationURL = CreateAbsoluteURL("~/");
                    HTTPArtifactType4 httpArtifact = new HTTPArtifactType4(HTTPArtifactType4.CreateSourceId(identificationURL), HTTPArtifactType4.CreateMessageHandle());

                    // Cache the authentication request for subsequent sending using the artifact resolution protocol.
                    HTTPArtifactState httpArtifactState = new HTTPArtifactState(samlResponseXml, null);
                    HTTPArtifactStateCache.Add(httpArtifact, httpArtifactState);

                    // Send the artifact.
                    IdentityProvider.SendArtifactByHTTPArtifact(Response, ssoState.assertionConsumerServiceURL, httpArtifact, ssoState.relayState, false);
                    break;

                default:
                    Trace.Write("IdP", "Invalid identity provider binding");
                    break;
            }

            Trace.Write("IdP", "Sent SAML response");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("IdP", "SSO service");

                // Get the saved SSO state, if any.
                // If there isn't saved state then receive the authentication request.
                // If there is saved state then we've just completed a local login in response 
                // to a prior authentication request.
                SSOState ssoState = (SSOState)Session[ssoSessionKey];

                if (ssoState == null)
                {
                    // Receive the authentication request.
                    AuthnRequest authnRequest = null;
                    string relayState = null;

                    ReceiveAuthnRequest(out authnRequest, out relayState);

                    if (authnRequest == null)
                    {
                        Trace.Write("IdP", "No authentication request");
                        return;
                    }

                    // Process the authentication request.
                    bool forceAuthn = authnRequest.ForceAuthn;

                    ssoState = new SSOState();
                    ssoState.authnRequest = authnRequest;
                    ssoState.relayState = relayState;

                    if (!string.IsNullOrEmpty(authnRequest.ProtocolBinding))
                    {
                        ssoState.idpProtocolBinding = SAMLIdentifiers.BindingURIs.URIToBinding(authnRequest.ProtocolBinding);
                    }
                    else
                    {
                        ssoState.idpProtocolBinding = SAMLIdentifiers.Binding.HTTPPost;
                    }

                    if (!string.IsNullOrEmpty(authnRequest.AssertionConsumerServiceURL))
                    {
                        ssoState.assertionConsumerServiceURL = authnRequest.AssertionConsumerServiceURL;
                    }
                    else
                    {
                        ssoState.assertionConsumerServiceURL = WebConfigurationManager.AppSettings["spAssertionConsumerServiceURL"];
                    }

                    // Determine whether or not a local login is required.
                    bool requireLocalLogin = IsLocalLoginRequired(forceAuthn);

                    // If a local login is required then save the authentication request 
                    // and initiate a local login.
                    if (requireLocalLogin)
                    {
                        // Save the SSO state.
                        Session[ssoSessionKey] = ssoState;

                        // Initiate a local login.
                        FormsAuthentication.RedirectToLoginPage();
                        return;
                    }
                }

                // Create a SAML response with the user's local identity, if any.
                SAMLResponse samlResponse = CreateSAMLResponse(ssoState);

                // Send the SAML response to the service provider.
                SendSAMLResponse(samlResponse, ssoState);

                // Clear the SSO state.
                Session[ssoSessionKey] = null;

            }

            catch (Exception exception)
            {
                Trace.Write("IdP", "Error in SSO service", exception);
            }
        }
    }
}
