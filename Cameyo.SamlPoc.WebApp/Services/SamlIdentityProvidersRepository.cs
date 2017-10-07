using Cameyo.SamlPoc.WebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cameyo.SamlPoc.WebApp.Services
{
    public class SamlIdentityProvidersRepository
    {
        private const string DefaultConfigPath = "saml-idps.json";

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

            var data = JsonConvert.SerializeObject(providers, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Error = (sender, args) => args.ErrorContext.Handled = true,
            });

            using (var streamWriter = new StreamWriter(DefaultConfigAbsolutePath, false))
            {
                streamWriter.Write(data);
            }
        }

        public IEnumerable<SamlIdentityProvider> GetRegisteredIdentityProviders()
        {
            string data;

            using (var streamReader = new StreamReader(DefaultConfigAbsolutePath, Encoding.UTF8))
            {
                data = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<List<SamlIdentityProvider>>(data);
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