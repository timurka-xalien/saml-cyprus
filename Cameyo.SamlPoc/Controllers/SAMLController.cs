using Cameyo.SamlPoc.Models;
using Cameyo.SamlPoc.Services;
using ComponentSpace.SAML2;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Cameyo.SamlPoc.Controllers
{
    public class SAMLController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private AuthenticationService _authenticationService;

        public SAMLController()
        {
            _authenticationService = new AuthenticationService(AuthenticationManager, UserManager);
        }

        public SAMLController(
            ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager)
            : this()
        {
            SignInManager = signInManager;
            UserManager = userManager;
        }

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

        public ActionResult AssertionConsumerService()
        {
            bool isInResponseTo = false;
            string partnerIdP = null;
            string authnContext = null;
            string userName = null;
            IDictionary<string, string> attributes = null;
            string targetUrl = null;

            SamlPocTraceListener.Log("SAML", "SamlController.AssertionConsumerService: Request to AssertionConsumerService received");

            // Receive and process the SAML assertion contained in the SAML response.
            // The SAML response is received either as part of IdP-initiated or SP-initiated SSO.
            SAMLServiceProvider.ReceiveSSO(Request, out isInResponseTo, out partnerIdP, out authnContext, out userName, out attributes, out targetUrl);

            // If no target URL is provided, provide a default.
            if (targetUrl == null)
            {
                targetUrl = "~/";
            }

            SamlPocTraceListener.Log("SAML", "SamlController.AssertionConsumerService: Login user automatically using the asserted identity.");

            // Login automatically using the asserted identity.
            // This example uses forms authentication. Your application can use any authentication method you choose.
            // There are no restrictions on the method of authentication.

            var result = SignInUserLocally(userName, attributes);

            if (result != null)
            {
                return result;
            }

            // Redirect to the target URL.
            return RedirectToLocal(targetUrl);
        }

        private ActionResult SignInUserLocally(string userName, IDictionary<string, string> attributes)
        {
            SamlPocTraceListener.Log("SAML", "SamlController.SignInUserLocally: Sign in user locally.");

            var user = SamlHelper.FindUser(UserManager, userName, attributes);

            if (user == null)
            {
                SamlPocTraceListener.Log("SAML", $"SamlController.SignInUserLocally: Register new user: {userName}");

                // Extract user email
                var email = SamlHelper.ExtractUserEmailFromSamlAttributes(userName, attributes);

                // Register new user
                user = new ApplicationUser { UserName = userName, Email = email };
                var result = UserManager.Create(user, userName); // Use fake password

                if (!result.Succeeded)
                {
                    var errors = string.Join("\r\n", result.Errors);

                    SamlPocTraceListener.Log(
                        "SAML",
                        $"SamlController.SignInUserLocally: Error while registering user: {errors}");

                    return View("Error");
                }
            }
            else
            {
                SamlPocTraceListener.Log("SAML", $"SamlController.SignInUserLocally: Found existing user: {userName}");
            }

            // Add user name to attributes
            attributes = attributes ?? new Dictionary<string, string>();
            attributes[ClaimTypes.Name] = userName;

            _authenticationService.Authenticate(AuthenticationType.Saml, userName, userName, attributes);

            return null;
        }

        public ActionResult SLOService()
        {
            SamlPocTraceListener.Log("SAML", "SamlController.SLOService: Request to single logout received from Identity Provider");

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
                SamlPocTraceListener.Log("SAML", "SamlController.SLOService: Processing IdP initiated logout");

                // Logout locally.
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                SamlPocTraceListener.Log("SAML", "SamlController.SLOService: User was logged out. Respond to IdP that logout succeeded.");

                // Respond to the IdP-initiated SLO request indicating successful logout.
                SAMLServiceProvider.SendSLO(Response, null);
            }
            else
            {
                SamlPocTraceListener.Log("SAML", "SamlController.SLOService: SP-initiated SLO has completed. Redirecting to login page.");

                // SP-initiated SLO has completed.
                return RedirectToAction("Index", "Home");
            }

            return new EmptyResult();
        }

        [AllowAnonymous]
        public ActionResult Login(string domain, string returnUrl)
        {
            SamlPocTraceListener.Log("SAML", $"SamlController.SingleSignOn: Request for SSO with IdP of domain {domain} received.");

            // Get appropriate IdP name
            var idpName = SamlIdentityProvidersRepository.GetIdentityProviderName(domain);

            if (idpName == null)
            {
                SamlPocTraceListener.Log("SAML", $"SamlController.SingleSignOn: IdP for domain {domain} not found.");

                return View("Error");
            }

            // To login at the service provider, initiate single sign-on to the identity provider (SP-initiated SSO).
            SAMLServiceProvider.InitiateSSO(Response, returnUrl, idpName);

            SamlPocTraceListener.Log("SAML", $"SamlController.SingleSignOn: SSO with IdP {idpName} initiated.");

            // Save idp name to session. We will reuse it on logout
            Session["IdentityProvider"] = idpName;

            return new EmptyResult();
        }

        public ActionResult LogOff()
        {
            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Request for SLO received.");

            if (SAMLServiceProvider.CanSLO())
            {
                string partnerIdP = Session["IdentityProvider"]?.ToString();

                if (partnerIdP != null)
                {
                    SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Initiating SLO with IdP {partnerIdP}.");

                    // Request logout at the identity provider.
                    SAMLServiceProvider.InitiateSLO(Response, null, null, partnerIdP);

                    return new EmptyResult();
                }
            }

            // Logout locally.
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            SamlPocTraceListener.Log("SAML", $"SamlController.Logout: Identity Provider doesn't support SLO.");

            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return
                    HttpContext?.GetOwinContext()?.Authentication ??
                    System.Web.HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        private ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            set
            {
                _signInManager = value;
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return
                    _userManager
                    ?? HttpContext?.GetOwinContext()?.GetUserManager<ApplicationUserManager>() ??
                    System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private SamlIdentityProvidersRepository SamlIdentityProvidersRepository
        {
            get => SamlIdentityProvidersRepository.GetInstance();
        }
    }
}