using System;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Data;

namespace ExampleIdentityProvider
{
    public class Global : System.Web.HttpApplication
    {
        // This method demonstrates loading configuration programmatically.
        // This is useful if you wish to store configuration in a custom database, for example.
        // Alternatively, configuration is loaded automatically from the saml.config file in the application's directory.
        private static void LoadSAMLConfigurationProgrammatically()
        {
            SAMLConfiguration samlConfiguration = new SAMLConfiguration()
            {
                LocalIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                {
                    Name = "http://localhost/ExampleIdentityProvider",
                    LocalCertificateFile = @"certificates\idp.pfx",
                    LocalCertificatePassword = "password"
                }
            };

            samlConfiguration.AddPartnerServiceProvider(
                new PartnerServiceProviderConfiguration()
                {
                    Name = "http://localhost/ExampleServiceProvider",
                    WantAuthnRequestSigned = true,
                    SignSAMLResponse = true,
                    AssertionConsumerServiceUrl = "http://localhost/ExampleServiceProvider/SAML/AssertionConsumerService.aspx",
                    SingleLogoutServiceUrl = "http://localhost/ExampleServiceProvider/SAML/SLOService.aspx",
                    PartnerCertificateFile = @"certificates\sp.cer"
                });

            SAMLController.Configuration = samlConfiguration;
        }

        // This method demonstrates loading multi-tenanted configuration programmatically.
        // This is useful if you wish to store configuration in a custom database, for example.
        // Alternatively, configuration is loaded automatically from the saml.config file in the application's directory.
        private static void LoadMuliTenantedSAMLConfigurationProgrammatically()
        {
            SAMLConfigurations samlConfigurations = new SAMLConfigurations();

            SAMLConfiguration samlConfiguration = new SAMLConfiguration()
            {
                ID = "tenant1",

                LocalIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                {
                    Name = "http://localhost/ExampleIdentityProvider",
                    LocalCertificateFile = @"certificates\idp.pfx",
                    LocalCertificatePassword = "password"
                }
            };

            samlConfiguration.AddPartnerServiceProvider(
                new PartnerServiceProviderConfiguration()
                {
                    Name = "http://localhost/ExampleServiceProvider",
                    WantAuthnRequestSigned = true,
                    SignSAMLResponse = true,
                    AssertionConsumerServiceUrl = "http://localhost/ExampleServiceProvider/SAML/AssertionConsumerService.aspx",
                    SingleLogoutServiceUrl = "http://localhost/ExampleServiceProvider/SAML/SLOService.aspx",
                    PartnerCertificateFile = @"certificates\sp.cer"
                });

            samlConfigurations.AddConfiguration(samlConfiguration);

            samlConfiguration = new SAMLConfiguration()
            {
                ID = "tenant2",

                LocalIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                {
                    Name = "http://localhost/ExampleIdentityProvider2",
                    LocalCertificateFile = @"certificates\idp.pfx",
                    LocalCertificatePassword = "password"
                }
            };

            samlConfiguration.AddPartnerServiceProvider(
                new PartnerServiceProviderConfiguration()
                {
                    Name = "http://localhost/ExampleServiceProvider2",
                    WantAuthnRequestSigned = true,
                    SignSAMLResponse = true,
                    AssertionConsumerServiceUrl = "http://localhost/ExampleServiceProvider2/SAML/AssertionConsumerService.aspx",
                    SingleLogoutServiceUrl = "http://localhost/ExampleServiceProvider2/SAML/SLOService.aspx",
                    PartnerCertificateFile = @"certificates\sp.cer"
                });

            samlConfigurations.AddConfiguration(samlConfiguration);

            SAMLController.Configurations = samlConfigurations;
        }

        // This method demonstrates using a database to store SAML identifiers and session data in a database.
        // This may be required in a web farm deployment when ASP.NET sessions are not stored in a database.
        // ASP.NET session cookies are used to uniquely identify SSO sessions.
        // ASP.NET session cookies must be enabled in web.config.
        private static void ConfigureSAMLDatabase()
        {
            SAMLController.SSOSessionStore = new DatabaseSSOSessionStore();
            SAMLController.IDCache = new DatabaseIDCache();
        }

        // This method demonstrates using a database to store SAML identifiers and session data in a database.
        // This may be required in a web farm deployment when ASP.NET sessions are not stored in a database.
        // ASP.NET anonymous ID cookies are used to uniquely identify SSO sessions.
        // Anonymous identification must be enabled in web.config.
        private static void ConfigureSAMLDatabaseUsingAnonymousIDSessionIDDelegate()
        {
            SAMLController.SSOSessionStore = new DatabaseSSOSessionStore()
            {
                SessionIDDelegate = new SessionIDDelegate(SessionIDDelegates.GetSessionIDFromAnonymousID)
            };

            SAMLController.IDCache = new DatabaseIDCache();
        }

        // This method demonstrates using a database to store SAML identifiers and session data in a database.
        // This may be required in a web farm deployment when ASP.NET sessions are not stored in a database.
        // A custom session ID delegate is used to uniquely identify SSO sessions.
        private static void ConfigureSAMLDatabaseUsingCustomSessionIDDelegate()
        {
            SAMLController.SSOSessionStore = new DatabaseSSOSessionStore()
            {
                SessionIDDelegate = delegate ()
                {
                    // Return an identifier that uniquely identifies the user's SSO session.
                    // The implementation details are not shown.
                    return null;
                }
            };

            SAMLController.IDCache = new DatabaseIDCache();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}