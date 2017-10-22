using Cameyo.SamlPoc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Cameyo.SamlPoc.Services
{
    public class AuthenticationService
    {
        private IAuthenticationManager _authenticationManager;
        private ApplicationUserManager _userManager;

        public AuthenticationService(IAuthenticationManager authenticationManager, ApplicationUserManager userManager)
        {
            _userManager = userManager;
            _authenticationManager = authenticationManager;
        }

        public bool Authenticate(
            AuthenticationType authenticationType, 
            string email, 
            string password,
            IDictionary<string, string> additionalClaims = null)
        {
            SamlPocTraceListener.Log("SAML", $"AuthenticationService.Authenticate: Authenticate user {email}");

            var user = SamlHelper.FindUser(_userManager, email, password);

            if (user != null)
            {
                var identity = _userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                // Save received attributes as claims
                InitializeUserClaims(authenticationType, additionalClaims, identity);

                // Sign in user
                _authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);

                return true;
            }

            return false;
        }

        private static void InitializeUserClaims(AuthenticationType authenticationType, IDictionary<string, string> additionalClaims, ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(nameof(AuthenticationType), authenticationType.ToString()));

            if (additionalClaims != null)
            {
                identity.AddClaims(additionalClaims.Select(attr => new Claim(attr.Key, attr.Value)));
            }
        }
    }
}