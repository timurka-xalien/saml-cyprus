using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
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

namespace GoogleIdP.SAML
{
    public partial class SSOService : System.Web.UI.Page
    {
        // The SSO state saved during a local login.
        private class SSOState
        {
            private AuthnRequest authnRequest;
            private string relayState;

            public AuthnRequest AuthnRequest
            {
                get
                {
                    return authnRequest;
                }

                set
                {
                    authnRequest = value;
                }
            }

            public string RelayState
            {
                get
                {
                    return relayState;
                }

                set
                {
                    relayState = value;
                }
            }
        }

        // The session key for saving the SSO state during a local login.
        private const string ssoSessionKey = "sso";

        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Receive the authentication request and relay state.
        private void ReceiveAuthnRequest(out AuthnRequest authnRequest, out string relayState)
        {
            Trace.Write("IdP", "Receiving authentication request over binding");

            XmlElement authnRequestXml = null;

            bool signed = false;

            IdentityProvider.ReceiveAuthnRequestByHTTPRedirect(Request, out authnRequestXml, out relayState, out signed, null);

            if (SAMLMessageSignature.IsSigned(authnRequestXml))
            {
                Trace.Write("IdP", "Verifying request signature");

                if (!SAMLMessageSignature.Verify(authnRequestXml))
                {
                    throw new ArgumentException("The authentication request signature failed to verify.");
                }
            }

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
        private SAMLResponse CreateSAMLResponse(AuthnRequest authnRequest)
        {
            Trace.Write("IdP", "Creating SAML response");

            SAMLResponse samlResponse = new SAMLResponse();
            samlResponse.InResponseTo = authnRequest.ID;
            samlResponse.Destination = authnRequest.AssertionConsumerServiceURL;
            Issuer issuer = new Issuer(CreateAbsoluteURL("~/"));
            samlResponse.Issuer = issuer;

            if (User.Identity.IsAuthenticated)
            {
                samlResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Success, null);

                SAMLAssertion samlAssertion = new SAMLAssertion();
                samlAssertion.Issuer = issuer;

                samlAssertion.Conditions = new Conditions(new TimeSpan(0, 10, 0));
                AudienceRestriction audienceRestriction = new AudienceRestriction();
                audienceRestriction.Audiences.Add(new Audience(authnRequest.AssertionConsumerServiceURL));
                samlAssertion.Conditions.ConditionsList.Add(audienceRestriction);

                Subject subject = new Subject(new NameID(User.Identity.Name));
                SubjectConfirmation subjectConfirmation = new SubjectConfirmation(SAMLIdentifiers.SubjectConfirmationMethods.Bearer);
                SubjectConfirmationData subjectConfirmationData = new SubjectConfirmationData();
                subjectConfirmationData.InResponseTo = authnRequest.ID;
                subjectConfirmationData.Recipient = authnRequest.AssertionConsumerServiceURL;
                subjectConfirmationData.NotBefore = samlAssertion.Conditions.NotBefore;
                subjectConfirmationData.NotOnOrAfter = samlAssertion.Conditions.NotOnOrAfter;

                subjectConfirmation.SubjectConfirmationData = subjectConfirmationData;
                subject.SubjectConfirmations.Add(subjectConfirmation);
                samlAssertion.Subject = subject;

                AuthnStatement authnStatement = new AuthnStatement();
                authnStatement.AuthnContext = new AuthnContext();
                authnStatement.AuthnContext.AuthnContextClassRef = new AuthnContextClassRef(SAMLIdentifiers.AuthnContextClasses.Password);
                samlAssertion.Statements.Add(authnStatement);

                samlResponse.Assertions.Add(samlAssertion);
            }
            else
            {
                samlResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Responder, SAMLIdentifiers.SecondaryStatusCodes.AuthnFailed, "The user is not authenticated at the identity provider");
            }

            Trace.Write("IdP", "Created SAML response");

            return samlResponse;
        }

        // Send the SAML response to the SP.
        private void SendSAMLResponse(SAMLResponse samlResponse, string relayState, string assertionConsumerServiceURL)
        {
            Trace.Write("IdP", "Sending SAML response");

            // Serialize the SAML response for transmission.
            XmlElement samlResponseXml = samlResponse.ToXml();

            // Sign the SAML response.            
            X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.IdPX509Certificate];
            SAMLMessageSignature.Generate(samlResponseXml, x509Certificate.PrivateKey, x509Certificate);

            IdentityProvider.SendSAMLResponseByHTTPPost(Response, assertionConsumerServiceURL, samlResponseXml, relayState);

            Trace.Write("IdP", "Sent SAML response");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Get the saved SSO state, if any.
                // If there isn't saved state then receive the authentication request.
                // If there is saved state then we've just completed a local login in response to a prior authentication request.
                SSOState ssoState = (SSOState)Session[ssoSessionKey];

                if (ssoState == null)
                {
                    Trace.Write("IdP", "SSO service");

                    // Receive the authentication request and relay state.
                    AuthnRequest authnRequest = null;
                    string relayState = null;

                    ReceiveAuthnRequest(out authnRequest, out relayState);

                    // Process the request.
                    bool forceAuthn = authnRequest.ForceAuthn;
                    ssoState = new SSOState();
                    ssoState.AuthnRequest = authnRequest;
                    ssoState.RelayState = relayState;

                    // Determine whether or not a local login is required.
                    bool requireLocalLogin = IsLocalLoginRequired(forceAuthn);

                    // If a local login is required then save the session state and initiate a local login.
                    if (requireLocalLogin)
                    {
                        Session[ssoSessionKey] = ssoState;
                        FormsAuthentication.RedirectToLoginPage();

                        return;
                    }
                }

                // Create a SAML response with the user's local identity, if any.
                SAMLResponse samlResponse = CreateSAMLResponse(ssoState.AuthnRequest);

                // Send the SAML response to the service provider.
                SendSAMLResponse(samlResponse, ssoState.RelayState, ssoState.AuthnRequest.AssertionConsumerServiceURL);

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
