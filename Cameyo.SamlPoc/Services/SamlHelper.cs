using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cameyo.SamlPoc.Services
{
    public static class SamlHelper
    {
        public static string ExtractUserEmailFromSamlAttributes(string userName, IDictionary<string, string> attributes)
        {
            // Put your own logic of extracting user email here. It most likely will depend on Identity Provider
            return userName.Contains("@") ? userName : null;
        }
    }
}