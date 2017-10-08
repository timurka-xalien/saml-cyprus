using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

using ComponentSpace.SAML2;
using Cameyo.SamlPoc.WebApp.Services;

namespace Cameyo.SamlPoc.WebApp.Controllers
{
    public static class AppSettings
    {
        public const string PartnerIdP = "PartnerIdP";
    }

    [Authorize]
    public class HomeController : Controller
    {
        public HomeController()
        {
           //  SamlConfigurationManager.ConfigureIdentityProviders();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logout()
        {
            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Request for SLO received.");

            // Logout locally.
            FormsAuthentication.SignOut();

            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: User was logged out locally.");

            if (SAMLServiceProvider.CanSLO())
            {
                // Request logout at the identity provider.
                string partnerIdP = Session["IdentityProvider"].ToString();

                SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Initiating SLO with IdP {partnerIdP}.");

                SAMLServiceProvider.InitiateSLO(Response, null, null, partnerIdP);

                return new EmptyResult();
            }

            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Identity Provider doesn't support SLO.");

            return RedirectToAction("Index", "Home");
        }
    }
}