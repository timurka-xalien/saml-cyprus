using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Configuration;
using System.Configuration;

namespace Cameyo.SamlPoc.WebApp
{
    public static class SamlConfigurationManager
    {
        const string ServiceProviderName = "saml:ServiceProvider:Name";
        const string ServiceProviderDescription = "saml:ServiceProvider:Description";
        const string ServiceProviderAssertionConsumerServiceUrl = "saml:ServiceProvider:AssertionConsumerServiceUrl";
        const string ServiceProviderLocalCertificateFile = "saml:ServiceProvider:LocalCertificateFile";
        const string ServiceProviderLocalCertificatePassword = "saml:ServiceProvider:LocalCertificatePassword";

        public static void ConfigureIdentityProviders()
        {
            SAMLConfiguration samlConfiguration = new SAMLConfiguration();

            samlConfiguration.LocalServiceProviderConfiguration = new
            LocalServiceProviderConfiguration()
            {
                Name = ConfigurationManager.AppSettings[ServiceProviderName],
                Description = ConfigurationManager.AppSettings[ServiceProviderDescription],
                AssertionConsumerServiceUrl = ConfigurationManager.AppSettings[ServiceProviderAssertionConsumerServiceUrl],
                LocalCertificateFile = ConfigurationManager.AppSettings[ServiceProviderLocalCertificateFile],
                LocalCertificatePassword = ConfigurationManager.AppSettings[ServiceProviderLocalCertificatePassword]
            };

            samlConfiguration.AddPartnerIdentityProvider(
             new PartnerIdentityProviderConfiguration()
             {
                 Name = "http://cone-idp",
                 Description = "Cone Identity Provider",
                 SignAuthnRequest = true,
                 SingleSignOnServiceUrl = "http://cone-idp/SAML/SSOService",
                 SingleLogoutServiceUrl = "http://cone-idp/SAML/SLOService",
                 PartnerCertificateFile = "Certificates\\idp.cer"
             });

            samlConfiguration.AddPartnerIdentityProvider(
                new PartnerIdentityProviderConfiguration()
                {
                    Name = "https://shib-idp/",
                    Description = "Shibboleth Identity Provider",
                    SignAuthnRequest = true,
                    SingleSignOnServiceUrl = "https://shib-idp/SAML/SSOService.aspx?binding=redirect",
                    PartnerCertificateFile = "Certificates\\idp.cer",
                    DisableInboundLogout = true,
                    DisableOutboundLogout = true
                });

            samlConfiguration.AddPartnerIdentityProvider(
                 new PartnerIdentityProviderConfiguration()
                 {
                     Name = "http://kentor-idp/Metadata",
                     Description = "Kentor Identity Provider",
                     SignAuthnRequest = true,
                     SingleSignOnServiceUrl = "http://kentor-idp/",
                     SingleLogoutServiceUrl = "http://kentor-idp/Logout",
                     UseEmbeddedCertificate = true
                 });

            SAMLController.Configuration = samlConfiguration;
        }
    }
}