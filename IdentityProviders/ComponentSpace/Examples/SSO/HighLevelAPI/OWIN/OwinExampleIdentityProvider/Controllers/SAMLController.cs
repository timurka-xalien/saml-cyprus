using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Utility;

namespace OwinExampleIdentityProvider.Controllers
{
    public class SAMLController : Controller
    {
        public ActionResult SSOService()
        {
            // Receive the authn request from the service provider (SP-initiated SSO).
            string partnerSP = null;

            SAMLIdentityProvider.ReceiveSSO(Request, out partnerSP);

            // If the user isn't logged in at the identity provider, 
            // have the user login before completing SSO.
            return RedirectToAction("SSOServiceCompletition");
        }

        [Authorize]
        public ActionResult SSOServiceCompletition()
        {
            // Get the name of the logged in user.
            var userName = User.Identity.Name;

            // For demonstration purposes only, include all the claims as SAML attributes.
            // To include a specific claim: ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.GivenName).
            var attributes = new Dictionary<string, string>();

            foreach (var claim in ((ClaimsIdentity)User.Identity).Claims)
            {
                attributes[claim.Type] = claim.Value;
            }

            // The user is logged in at the identity provider.
            // Respond to the authn request by sending a SAML response containing a SAML assertion to the SP.
            SAMLIdentityProvider.SendSSO(Response, userName, attributes);

            return new EmptyResult();
        }

        public ActionResult SLOService()
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by a partner service provider.
            // If a response is received then this is in response to single logout having been initiated by the identity provider.
            bool isRequest = false;
            bool hasCompleted = false;
            string logoutReason = null;
            string partnerSP = null;
            string relayState = null;

            SAMLIdentityProvider.ReceiveSLO(Request, Response, out isRequest, out hasCompleted, out logoutReason, out partnerSP, out relayState);

            if (isRequest)
            {
                // Logout locally.
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                // Respond to the SP-initiated SLO request indicating successful logout.
                SAMLIdentityProvider.SendSLO(Response, null);
            }
            else
            {
                if (hasCompleted)
                {
                    // IdP-initiated SLO has completed.
                    return RedirectToAction("Index", "Home");
                }
            }

            return new EmptyResult();
        }
    }
}