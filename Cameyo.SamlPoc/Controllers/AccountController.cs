using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Cameyo.SamlPoc.Models;
using Cameyo.SamlPoc.Services;

namespace Cameyo.SamlPoc.Controllers
{
    [Authorize]
    public partial class AccountController : Controller
    {
        private AuthenticationService _authenticationService;

        public AccountController()
        {
            _authenticationService = new AuthenticationService(AuthenticationManager, UserManager);
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
            : this()
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IdentityProviders = SamlIdentityProvidersRepository.GetRegisteredIdentityProviders();
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if we need to login user locally (in case no SAML Identity Provider is registered for specified email domain)
            var emailDomain = Utils.GetEmailDomain(model.Email);
            var isSamlAuthenticationRequired = 
                SamlIdentityProvidersRepository.IsSamlAuthenticationRequired(emailDomain);

            if (isSamlAuthenticationRequired)
            {
                return RedirectToAction("Login", "SAML", new { domain = emailDomain, returnUrl = returnUrl });
            }

            // Authenticate locally
            var succeeded = _authenticationService.Authenticate(AuthenticationType.Local, model.Email, model.Password);

            if (succeeded)
            { 
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "Invalid login attempt.");

            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var authenticationType = ((ClaimsIdentity)User.Identity).FindFirstValue(nameof(AuthenticationType));

            var userName = User.Identity.Name;
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: User {userName} was logged out locally.");

            if (authenticationType == AuthenticationType.Saml.ToString())
            {
                return RedirectToAction("LogOff", "SAML");
            }

            return RedirectToAction("Index", "Home");
        }

        private SamlIdentityProvidersRepository SamlIdentityProvidersRepository
        {
            get => SamlIdentityProvidersRepository.GetInstance();
        }
    }
}