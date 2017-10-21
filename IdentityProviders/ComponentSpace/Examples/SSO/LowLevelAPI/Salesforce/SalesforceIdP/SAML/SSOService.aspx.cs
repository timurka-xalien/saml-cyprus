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
using ComponentSpace.SAML2.Profiles.SSOBrowser;

namespace SalesforceIdP.SAML
{
    public partial class SSOService : System.Web.UI.Page
    {
        // The query string parameter identifying the target URL of the SP.
        private const string targetQueryParameter = "target";

        // The Salesforce audience URI,
        private const string audienceURI = "https://saml.salesforce.com";

        // Salesforce SAML attribute names
        private class AttributeNames
        {
            public const string SSOStartPage = "ssoStartPage";
            public const string LogoutURL = "logoutURL";
        }

        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Create a SAML response with the user's local identity.
        private SAMLResponse CreateSAMLResponse()
        {
            Trace.Write("IdP", "Creating SAML response");

            SAMLResponse samlResponse = new SAMLResponse();
            samlResponse.Destination = Configuration.AssertionConsumerServiceURL;
            Issuer issuer = new Issuer(Configuration.Issuer);
            samlResponse.Issuer = issuer;
            samlResponse.Status = new Status(SAMLIdentifiers.PrimaryStatusCodes.Success, null);

            SAMLAssertion samlAssertion = new SAMLAssertion();
            samlAssertion.Issuer = issuer;

            // For simplicity, a configured Salesforce user name is used.
            // NB. You must update the web.config to specify a valid Salesforce user name.
            // In a real world application you would perform some sort of local to Salesforce identity mapping.
            Subject subject = new Subject(new NameID(Configuration.SalesforceLoginID, null, null, SAMLIdentifiers.NameIdentifierFormats.Unspecified, null));
            SubjectConfirmation subjectConfirmation = new SubjectConfirmation(SAMLIdentifiers.SubjectConfirmationMethods.Bearer);
            SubjectConfirmationData subjectConfirmationData = new SubjectConfirmationData();
            subjectConfirmationData.Recipient = Configuration.AssertionConsumerServiceURL;
            subjectConfirmation.SubjectConfirmationData = subjectConfirmationData;
            subject.SubjectConfirmations.Add(subjectConfirmation);
            samlAssertion.Subject = subject;

            Conditions conditions = new Conditions(new TimeSpan(1, 0, 0));
            AudienceRestriction audienceRestriction = new AudienceRestriction();
            audienceRestriction.Audiences.Add(new Audience(audienceURI));
            conditions.ConditionsList.Add(audienceRestriction);
            samlAssertion.Conditions = conditions;

            subjectConfirmationData.NotOnOrAfter = conditions.NotOnOrAfter;

            AuthnStatement authnStatement = new AuthnStatement();
            authnStatement.AuthnContext = new AuthnContext();
            authnStatement.AuthnContext.AuthnContextClassRef = new AuthnContextClassRef(SAMLIdentifiers.AuthnContextClasses.Unspecified);
            samlAssertion.Statements.Add(authnStatement);

            AttributeStatement attributeStatement = new AttributeStatement();
            attributeStatement.Attributes.Add(new SAMLAttribute(AttributeNames.SSOStartPage, SAMLIdentifiers.AttributeNameFormats.Unspecified, null, CreateAbsoluteURL("~/Login.aspx")));
            attributeStatement.Attributes.Add(new SAMLAttribute(AttributeNames.LogoutURL, SAMLIdentifiers.AttributeNameFormats.Unspecified, null, CreateAbsoluteURL("~/Logout.aspx")));
            samlAssertion.Statements.Add(attributeStatement);

            samlResponse.Assertions.Add(samlAssertion);

            Trace.Write("IdP", "Created SAML response");

            return samlResponse;
        }

        // Send the SAML response to the SP.
        private void SendSAMLResponse(SAMLResponse samlResponse, string relayState)
        {
            Trace.Write("IdP", "Sending SAML response");

            // Serialize the SAML response for transmission.
            XmlElement samlResponseXml = samlResponse.ToXml();

            // Sign the SAML response.
            X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.IdPX509Certificate];
            SAMLMessageSignature.Generate(samlResponseXml, x509Certificate.PrivateKey, x509Certificate);

            IdentityProvider.SendSAMLResponseByHTTPPost(Response, Configuration.AssertionConsumerServiceURL, samlResponseXml, relayState);

            Trace.Write("IdP", "Sent SAML response");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("IdP", "SSO service");

                string targetURL = Request.QueryString[targetQueryParameter];

                if (string.IsNullOrEmpty(targetURL))
                {
                    return;
                }

                Trace.Write("IdP", "Target URL: " + targetURL);

                // Create a SAML response with the user's local identity.
                SAMLResponse samlResponse = CreateSAMLResponse();

                // Send the SAML response to the service provider.
                SendSAMLResponse(samlResponse, targetURL);
            }

            catch (Exception exception)
            {
                Trace.Write("IdP", "Error in SSO service", exception);
            }
        }
    }
}
