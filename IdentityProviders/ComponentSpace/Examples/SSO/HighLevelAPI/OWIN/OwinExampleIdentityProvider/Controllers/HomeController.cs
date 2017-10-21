using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using ComponentSpace.SAML2;

namespace OwinExampleIdentityProvider.Controllers
{
    public static class AppSettings
    {
        public const string PartnerSP = "PartnerSP";
        public const string TargetUrl = "TargetUrl";
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [Authorize]
        public ActionResult SingleSignOn()
        {
            // Get the configured partner service provider name and target URL.
            // For demonstration purposes only, these are specified in web.config.
            // The partner service provider name is required if there's more than one partner service provider.
            // The service provider should redirect to the optional target URL once SSO completes.
            var partnerSP = WebConfigurationManager.AppSettings[AppSettings.PartnerSP];
            var targetUrl = WebConfigurationManager.AppSettings[AppSettings.TargetUrl];

            // Get the name of the logged in user.
            var userName = User.Identity.Name;

            // For demonstration purposes only, include all the claims as SAML attributes.
            // To include a specific claim: ((ClaimsIdentity) User.Identity).FindFirst(ClaimTypes.GivenName).
            var attributes = new Dictionary<string, string>();

            foreach (var claim in ((ClaimsIdentity)User.Identity).Claims)
            {
                attributes[claim.Type] = claim.Value;
            }

            // Initiate single sign-on to the service provider (IdP-initiated SSO)
            // by sending a SAML response containing a SAML assertion to the SP.
            SAMLIdentityProvider.InitiateSSO(
                Response,
                userName,
                attributes,
                targetUrl,
                partnerSP);

            return new EmptyResult();
        }
    }
}