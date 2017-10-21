using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace SAML2SP
{
    public class Global : System.Web.HttpApplication
    {
        // The identity provider's certificate file name - must be in the application directory.
        private const string idpCertificateFileName = "idp.cer";

        // The application key to the identity provider's certificate.
        public const string IdPX509Certificate = "idpX509Certificate";

        // As part of the HTTP artifact profile, an artifact resolve message is sent to the artifact resolution service,
        // typically using SOAP over HTTPS. In a test environment with self-signed certificates, override certificate validation
        // so all certificates are trusted.
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        // Loads the certificate from file.
        // A password is only required if the file contains a private key.
        // The machine key set is specified so the certificate is accessible to the IIS process.
        private static X509Certificate2 LoadCertificate(string fileName, string password)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("The certificate file " + fileName + " doesn't exist.");
            }

            try
            {
                return new X509Certificate2(fileName, password, X509KeyStorageFlags.MachineKeySet);
            }

            catch (Exception exception)
            {
                throw new ArgumentException("The certificate file " + fileName + " couldn't be loaded - " + exception.Message);
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // In a test environment, trust all certificates.
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            // Load the IdP certificate.
            String fileName = Path.Combine(HttpRuntime.AppDomainAppPath, idpCertificateFileName);
            Application[IdPX509Certificate] = LoadCertificate(fileName, null);
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}