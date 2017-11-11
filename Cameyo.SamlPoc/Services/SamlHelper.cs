using System.Collections.Generic;

namespace Cameyo.SamlPoc.Services
{
    public static class SamlHelper
    {
        public const string OpenAmEmailAttributeName = "mail";

        public static string ExtractUserEmailFromSamlAttributes(string userName, IDictionary<string, string> attributes)
        {
            // Put your own logic of extracting user email here. It most likely will depend on Identity Provider

            if (attributes.ContainsKey(OpenAmEmailAttributeName))
            {
                var email = attributes[OpenAmEmailAttributeName];
                // Remove email from attributes as later we will add it to user claims under a standard email claim type
                attributes.Remove(OpenAmEmailAttributeName);
                return email;
            }

            return userName.Contains("@") ? userName : null;
        }
    }
}