using System.Web.Mvc;
using System.Web.Security;

using MvcExampleIdentityProvider.Models;

namespace MvcExampleIdentityProvider.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel("idp-user"));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // For simplicity, this example uses forms authentication with credentials stored in web.config.
            // Your application can use any authentication method you choose (eg Active Directory, custom database etc).
            // There are no restrictions on the method of authentication.
            if (ModelState.IsValid && FormsAuthentication.Authenticate(model.UserName, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, false);
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "Invalid credentials. The user name and password should be \"idp-user\" and \"password\".");
            return View(model);
        }
    }
}
