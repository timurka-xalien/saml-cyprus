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

namespace ShibbolethSP
{
    public static class Configuration
    {
        private static class ConfigurationKeys
        {
            public const string SingleSignOnServiceBinding = "SingleSignOnServiceBinding";
            public const string HttpPostSingleSignOnServiceURL = "HttpPostSingleSignOnServiceURL";
            public const string HttpRedirectSingleSignOnServiceURL = "HttpRedirectSingleSignOnServiceURL";
            public const string HttpArtifactSingleSignOnServiceURL = "HttpArtifactSingleSignOnServiceURL";
            public const string ArtifactResolutionServiceURL = "ArtifactResolutionServiceURL";
        }

        public static SAMLIdentifiers.Binding SingleSignOnServiceBinding
        {
            get
            {
                return SAMLIdentifiers.BindingURIs.URIToBinding(WebConfigurationManager.AppSettings[ConfigurationKeys.SingleSignOnServiceBinding]);
            }
        }

        public static string SingleSignOnServiceURL
        {
            get
            {
                switch (SingleSignOnServiceBinding)
                {
                    case SAMLIdentifiers.Binding.HTTPPost:
                        return HttpPostSingleSignOnServiceURL;

                    case SAMLIdentifiers.Binding.HTTPRedirect:
                        return HttpRedirectSingleSignOnServiceURL;

                    case SAMLIdentifiers.Binding.HTTPArtifact:
                        return HttpArtifactSingleSignOnServiceURL;

                    default:
                        throw new ArgumentException("Invalid single signon service binding");
                }
            }
        }

        public static string HttpPostSingleSignOnServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.HttpPostSingleSignOnServiceURL];
            }
        }

        public static string HttpRedirectSingleSignOnServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.HttpRedirectSingleSignOnServiceURL];
            }
        }

        public static string HttpArtifactSingleSignOnServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.HttpArtifactSingleSignOnServiceURL];
            }
        }

        public static string ArtifactResolutionServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.ArtifactResolutionServiceURL];
            }
        }
    }
}
