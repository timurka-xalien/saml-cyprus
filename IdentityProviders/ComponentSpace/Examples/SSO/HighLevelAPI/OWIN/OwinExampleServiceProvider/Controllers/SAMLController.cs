using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

using ComponentSpace.SAML2;

using OwinExampleServiceProvider.Models;
using System.Data.Entity.Validation;

namespace OwinExampleServiceProvider.Controllers
{
    public class SAMLController : Controller
    {
        public ActionResult AssertionConsumerService()
        {
            bool isInResponseTo = false;
            string partnerIdP = null;
            string authnContext = null;
            string userName = null;
            IDictionary<string, string> attributes = null;
            string targetUrl = null;

            // Receive and process the SAML assertion contained in the SAML response.
            // The SAML response is received either as part of IdP-initiated or SP-initiated SSO.
            SAMLServiceProvider.ReceiveSSO(Request, out isInResponseTo, out partnerIdP, out authnContext, out userName, out attributes, out targetUrl);

            // Automatically provision the user.
            // If the user doesn't exist locally then create the user.
            var applicationUserManager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();
            var applicationUser = applicationUserManager.FindByName(userName);

            if (applicationUser == null)
            {
                applicationUser = new ApplicationUser();

                applicationUser.UserName = userName;
                applicationUser.Email = userName;
                applicationUser.EmailConfirmed = true;

                if (attributes != null)
                {
                    if (attributes.ContainsKey(ClaimTypes.GivenName))
                    {
                        applicationUser.Claims.Add(new IdentityUserClaim() { ClaimType = ClaimTypes.GivenName, ClaimValue = attributes[ClaimTypes.GivenName], UserId = applicationUser.Id });
                    }

                    if (attributes.ContainsKey(ClaimTypes.Surname))
                    {
                        applicationUser.Claims.Add(new IdentityUserClaim() { ClaimType = ClaimTypes.Surname, ClaimValue = attributes[ClaimTypes.Surname], UserId = applicationUser.Id });
                    }
                }

                var identityResult = applicationUserManager.Create(applicationUser);

                if (!identityResult.Succeeded)
                {
                    throw new Exception(string.Format("The user {0} couldn't be created.\n{1}", userName, identityResult));
                }
            }

            // Automatically login using the asserted identity.
            var applicationSignInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            applicationSignInManager.SignIn(applicationUser, false, false);

            // Redirect to the target URL if supplied.
            if (!string.IsNullOrEmpty(targetUrl))
            {
                return Redirect(targetUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult SLOService()
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by the identity provider.
            // If a response is received then this is in response to single logout having been initiated by the service provider.
            bool isRequest = false;
            string logoutReason = null;
            string partnerIdP = null;
            string relayState = null;

            SAMLServiceProvider.ReceiveSLO(Request, out isRequest, out logoutReason, out partnerIdP, out relayState);

            if (isRequest)
            {
                // Logout locally.
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                // Respond to the IdP-initiated SLO request indicating successful logout.
                SAMLServiceProvider.SendSLO(Response, null);
            }
            else
            {
                // SP-initiated SLO has completed.
                return RedirectToAction("Index", "Home");
            }

            return new EmptyResult();
        }
    }
}