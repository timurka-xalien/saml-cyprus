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

namespace ShibbolethIdP
{
    public static class Configuration
    {
        private static class ConfigurationKeys
        {
            public const string AssertionConsumerServiceBinding = "AssertionConsumerServiceBinding";
            public const string HttpPostAssertionConsumerServiceURL = "HttpPostAssertionConsumerServiceURL";
            public const string HttpArtifactAssertionConsumerServiceURL = "HttpArtifactAssertionConsumerServiceURL";
            public const string ArtifactResolutionServiceURL = "ArtifactResolutionServiceURL";
        }

        public static SAMLIdentifiers.Binding AssertionConsumerServiceBinding
        {
            get
            {
                return SAMLIdentifiers.BindingURIs.URIToBinding(WebConfigurationManager.AppSettings[ConfigurationKeys.AssertionConsumerServiceBinding]);
            }
        }

        public static string AssertionConsumerServiceURL
        {
            get
            {
                switch (AssertionConsumerServiceBinding)
                {
                    case SAMLIdentifiers.Binding.HTTPPost:
                        return HttpPostAssertionConsumerServiceURL;

                    case SAMLIdentifiers.Binding.HTTPArtifact:
                        return HttpArtifactAssertionConsumerServiceURL;

                    default:
                        throw new ArgumentException("Invalid assertion consumer service binding");
                }
            }
        }

        public static string HttpPostAssertionConsumerServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.HttpPostAssertionConsumerServiceURL];
            }
        }

        public static string HttpArtifactAssertionConsumerServiceURL
        {
            get
            {
                return WebConfigurationManager.AppSettings[ConfigurationKeys.HttpArtifactAssertionConsumerServiceURL];
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
