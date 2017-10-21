using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using ComponentSpace.SAML2;

namespace SAML2IdP
{
    public static class Configuration
    {
        private static class ConfigurationKeys
        {
            public const string AssertionConsumerServiceURL = "AssertionConsumerServiceURL";
            public const string SPTargetURL = "SPTargetURL";
        }

        public static string AssertionConsumerServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.AssertionConsumerServiceURL];
            }
        }

        public static string SPTargetURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.SPTargetURL];
            }
        }
    }
}
