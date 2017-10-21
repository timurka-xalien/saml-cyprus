using System;
using System.Data;
using System.Configuration;
using System.Collections;
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

namespace ShibbolethSP.SAML
{
    public partial class ArtifactResolutionService : System.Web.UI.Page
    {
        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL)
        {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Process the artifact resolve request received from the identity provider in response
        // to the artifact sent by the service provider.
        private void ProcessArtifactResolve()
        {
            Trace.Write("SP", "Processing artifact resolve request");

            // Receive the artifact resolve request.
            XmlElement artifactResolveXml = ArtifactResolver.ReceiveArtifactResolve(Request);
            ArtifactResolve artifactResolve = new ArtifactResolve(artifactResolveXml);

            // Get the artifact.
            HTTPArtifactType4 httpArtifact = new HTTPArtifactType4(artifactResolve.Artifact.ArtifactValue);

            // Remove the artifact state from the cache.
            HTTPArtifactState httpArtifactState = HTTPArtifactStateCache.Remove(httpArtifact);

            if (httpArtifactState == null)
            {
                throw new ArgumentException("Invalid artifact.");
            }

            // Create an artifact response containing the cached SAML message.
            ArtifactResponse artifactResponse = new ArtifactResponse();
            artifactResponse.Issuer = new Issuer(CreateAbsoluteURL("~/"));
            artifactResponse.SAMLMessage = httpArtifactState.SAMLMessage;

            XmlElement artifactResponseXml = artifactResponse.ToXml();

            // Send the artifact response.
            ArtifactResolver.SendArtifactResponse(Response, artifactResponseXml);

            Trace.Write("SP", "Processed artifact resolve request");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("SP", "Artifact resolution service");

                ProcessArtifactResolve();
            }

            catch (Exception exception)
            {
                Trace.Write("SP", "Error in artifact resolution service", exception);
            }
        }
    }
}
