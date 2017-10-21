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

namespace SalesforceIdP
{
    public static class Configuration
    {
        private static class ConfigurationKeys
        {
            public const string AssertionConsumerServiceURL = "AssertionConsumerServiceURL";
            public const string SPTargetURL = "SPTargetURL";
            public const string Issuer = "Issuer";
            public const string SalesforceLoginID = "SalesforceLoginID";
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

        public static string Issuer
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.Issuer];
            }
        }

        public static string SalesforceLoginID
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.SalesforceLoginID];
            }
        }
    }
}
