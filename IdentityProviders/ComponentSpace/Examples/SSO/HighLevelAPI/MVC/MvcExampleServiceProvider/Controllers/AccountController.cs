using System.Web.Configuration;
using System.Web.Mvc;

using ComponentSpace.SAML2;

namespace MvcExampleServiceProvider.Controllers
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
        public ActionResult SingleSignOn()
        {
            // To login at the service provider, initiate single sign-on to the identity provider (SP-initiated SSO).
            string partnerIdP = WebConfigurationManager.AppSettings[AppSettings.PartnerIdP];
            SAMLServiceProvider.InitiateSSO(Response, null, partnerIdP);

            return new EmptyResult();
        }
    }
}
