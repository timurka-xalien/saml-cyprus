using System.Web.Configuration;
using System.Web.Mvc;

using ComponentSpace.SAML2;
using Cameyo.SamlPoc.WebApp.Services;

namespace Cameyo.SamlPoc.WebApp.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult SingleSignOn(string idpName)
        {
            SamlPocTraceListener.Log("SAML", $"SamlController.SingleSignOn: Request for SSO with IdP {idpName} received.");

            // To login at the service provider, initiate single sign-on to the identity provider (SP-initiated SSO).
            //string partnerIdP = WebConfigurationManager.AppSettings[idpName];
            SAMLServiceProvider.InitiateSSO(Response, null, idpName);

            SamlPocTraceListener.Log("SAML", $"SamlController.SingleSignOn: SSO with IdP {idpName} initiated.");

            Session["IdentityProvider"] = idpName;

            return new EmptyResult();
        }
    }
}
