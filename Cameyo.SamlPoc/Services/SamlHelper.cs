using Cameyo.SamlPoc.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace Cameyo.SamlPoc.Services
{
    public static class SamlHelper
    {
        public static string ExtractUserEmailFromSamlAttributes(string userName, IDictionary<string, string> attributes)
        {
            // Put your own logic of extracting user email here. It most likely will depend on Identity Provider
            return userName.Contains("@") ? userName : null;
        }

        public static ApplicationUser FindUser(ApplicationUserManager userManager, string userName, IDictionary<string, string> attributes)
        {
            // Put your own logic of finding user here
            return userManager.FindByName(userName);
        }

        public static ApplicationUser FindUser(ApplicationUserManager userManager, string userName, string password)
        {
            // Put your own logic of finding user here
            return userManager.Find(userName, password);
        }

    }
}