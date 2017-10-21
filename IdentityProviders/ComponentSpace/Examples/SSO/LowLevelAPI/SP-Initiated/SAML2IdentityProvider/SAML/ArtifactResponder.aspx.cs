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

namespace SAML2IdentityProvider.SAML
{
    public partial class ArtifactResponder : System.Web.UI.Page
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
            Trace.Write("IdP", "Processing artifact resolve request");

            // Receive the artifact resolve request.
            XmlElement artifactResolveXml = ArtifactResolver.ReceiveArtifactResolve(Request);
            ArtifactResolve artifactResolve = new ArtifactResolve(artifactResolveXml);

            // Get the artifact.
            HTTPArtifactType4 httpArtifact = new HTTPArtifactType4(artifactResolve.Artifact.ArtifactValue);

            // Remove the artifact state from the cache.
            HTTPArtifactState httpArtifactState = HTTPArtifactStateCache.Remove(httpArtifact);

            if (httpArtifactState == null)
            {
                Trace.Write("IdP", string.Format("The artifact {0} is not recognized.", artifactResolve.Artifact.ArtifactValue));
                return;
            }

            // Create an artifact response containing the cached SAML message.
            ArtifactResponse artifactResponse = new ArtifactResponse();
            artifactResponse.Issuer = new Issuer(CreateAbsoluteURL("~/"));
            artifactResponse.SAMLMessage = httpArtifactState.SAMLMessage;

            XmlElement artifactResponseXml = artifactResponse.ToXml();

            // Send the artifact response.
            ArtifactResolver.SendArtifactResponse(Response, artifactResponseXml);

            Trace.Write("IdP", "Processed artifact resolve request");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Trace.Write("IdP", "Artifact responder");

                ProcessArtifactResolve();

            }
            catch (Exception exception)
            {
                Trace.Write("IdP", "Error in artifact responder", exception);
            }
        }
    }
}
