using Cameyo.SamlPoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cameyo.SamlPoc.Services
{
    public class SamlIdentityProvidersRepository : IDisposable
    {
        private const string DefaultConfigPath = "saml-idps.json";

        private static readonly SamlIdentityProvidersRepository _instance = new SamlIdentityProvidersRepository();
        private IEnumerable<SamlIdentityProvider> _registeredProviders;

        private SamlIdentityProvidersRepository()
        {
        }

        public void CreateDefaultConfiguration()
        {
            var ipdCone = new SamlIdentityProvider
            {
                Id = Guid.NewGuid(),
                Name = "http://cone-idp",
                Description = "Cone Identity Provider",
                SingleSignOnUrl = "http://cone-idp/SAML/SSOService",
                SingleLogoutUrl = "http://cone-idp/SAML/SLOService",
                CertificateFile = "Certificates\\idp.cer",

                RegisteredDomains =
                {
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "cone.com"
                    },
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "cone.net"
                    }
                }
            };

            var idpShib = new SamlIdentityProvider
            {
                Id = Guid.NewGuid(),
                Name = "https://shib-idp/",
                Description = "Shibboleth Identity Provider",
                SingleSignOnUrl = "https://shib-idp/SAML/SSOService.aspx?binding=redirect",
                CertificateFile = "Certificates\\idp.cer",
                SingleLogoutSupported = false,

                RegisteredDomains =
                {
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "shib.com"
                    },
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "shibboleth.net"
                    }
                }
            };

            var ipdKentor = new SamlIdentityProvider
            {
                Id = Guid.NewGuid(),
                Name = "http://kentor-idp/Metadata",
                Description = "Kentor Identity Provider",
                SignAuthnRequest = true,
                SingleSignOnUrl = "http://kentor-idp/",
                SingleLogoutUrl = "http://kentor-idp/Logout",
                UseEmbeddedCertificate = true,

                RegisteredDomains =
                {
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "kentor.com"
                    },
                    new EmailDomain
                    {
                        Id = Guid.NewGuid(),
                        Domain = "kentorsome.net"
                    }
                }
            };

            var providers = new List<SamlIdentityProvider>()
            {
                ipdCone,
                idpShib,
                ipdKentor,
            };

            string data = Utils.SerializeToJson(providers);

            Utils.WriteTextToFile(DefaultConfigAbsolutePath, data);
        }

        public IEnumerable<SamlIdentityProvider> GetRegisteredIdentityProviders()
        {
            if (_registeredProviders == null)
            {
                var data = Utils.ReadTextFromFile(DefaultConfigAbsolutePath);

                _registeredProviders = Utils.DeserializeFromJson<List<SamlIdentityProvider>>(data);
            }

            return _registeredProviders;
        }

        private IEnumerable<string> GetRegisteredEmailDomains()
        {
            return GetRegisteredIdentityProviders()
                .SelectMany(idp => idp.RegisteredDomains.Select(d => d.Domain));
        }

        public bool IsSamlAuthenticationRequired(string emailDomain)
        {
            return GetRegisteredEmailDomains().Contains(emailDomain);
        }

        public string GetIdentityProviderName(string domain)
        {
            return GetRegisteredIdentityProviders()
                .Where(idp => idp.RegisteredDomains.Any(d => d.Domain == domain))
                .Select(idp => idp.Name)
                .SingleOrDefault();
        }

        public static SamlIdentityProvidersRepository GetInstance()
        {
            return _instance;
        }

        public void Dispose()
        {
        }

        private string DefaultConfigAbsolutePath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + DefaultConfigPath;
            }
        }
    }
}