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

namespace SAML2SP.SAML
{
    public partial class AssertionConsumerService : System.Web.UI.Page
    {
        // Receive the SAML response from the identity provider.
        private void ReceiveSAMLResponse(out SAMLResponse samlResponse, out string relayState)
        {
            Trace.Write("SP", "Receiving SAML response");

            // Receive the SAML response.
            XmlElement samlResponseXml = null;

            ServiceProvider.ReceiveSAMLResponseByHTTPPost(Request, out samlResponseXml, out relayState);

            // Verify the response's signature.
            if (SAMLMessageSignature.IsSigned(samlResponseXml))
            {
                Trace.Write("SP", "Verifying response signature");
                X509Certificate2 x509Certificate = (X509Certificate2)Application[Global.IdPX509Certificate];

                if (!SAMLMessageSignature.Verify(samlResponseXml, x509Certificate))
                {
                    throw new ArgumentException("The SAML response signature failed to verify.");
                }
            }

            // Deserialize the XML.
            samlResponse = new SAMLResponse(samlResponseXml);

            Trace.Write("SP", "Received SAML response");
        }

        // Process the SAML response.
        private void ProcessSAMLResponse(SAMLResponse samlResponse, string relayState)
        {
            Trace.Write("SP", "Processing SAML response");

            // Check whether the SAML response indicates success.
            if (!samlResponse.IsSuccess())
            {
                throw new ArgumentException("Received error response");
            }

            // Extract the asserted identity from the SAML response.
            SAMLAssertion samlAssertion = null;

            if (samlResponse.GetUnsignedAssertions().Count > 0)
            {
                samlAssertion = samlResponse.GetUnsignedAssertions()[0];
            }
            else
            {
                throw new ArgumentException("No assertions in response");
            }

            // Enforce single use of the SAML assertion.
            if (!AssertionIDCache.Add(samlAssertion))
            {
                throw new ArgumentException("The SAML assertion has already been used");
            }

            // Get the subject name identifier.
            string userName = null;

            if (samlAssertion.Subject.NameID != null)
            {
                userName = samlAssertion.Subject.NameID.NameIdentifier;
            }
            else
            {
                throw new ArgumentException("No name in subject");
            }

            // Create a login context for the asserted identity.
            FormsAuthentication.SetAuthCookie(userName, false);

            // Redirect to the requested URL.
            Response.Redirect(relayState, false);

            Trace.Write("SP", "Processed successful SAML response");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("SP", "Assertion consumer service");

                // Receive the SAML response.
                SAMLResponse samlResponse = null;
                string relayState = null;

                ReceiveSAMLResponse(out samlResponse, out relayState);

                // Process the SAML response.
                ProcessSAMLResponse(samlResponse, relayState);
            }

            catch (Exception exception)
            {
                Trace.Write("SP", "Error in assertion consumer service", exception);
            }
        }
    }
}
