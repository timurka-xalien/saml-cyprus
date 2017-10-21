using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace SAML2ServiceProvider
{
    public class Global : System.Web.HttpApplication
    {
        // The service provider's certificate file name - must be in the application directory.
        private const string spCertificateFileName = "sp.pfx";

        // The service provider's certificate/private key file password.
        private const string spPassword = "password";

        // The identity provider's certificate file name - must be in the application directory.
        private const string idpCertificateFileName = "idp.cer";

        // The application key to the service provider's certificate.
        public const string SPX509Certificate = "spX509Certificate";

        // The application key to the identity provider's certificate.
        public const string IdPX509Certificate = "idpX509Certificate";

        //
        // Loads the certificate from file.
        // A password is only required if the file contains a private key.
        // The machine key set is specified so the certificate is accessible to the IIS process.
        //
        private X509Certificate2 LoadCertificate(string fileName, string password)
        {
            X509Certificate2 functionReturnValue = default(X509Certificate2);

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("The certificate file " + fileName + " doesn't exist.");
            }

            try
            {
                functionReturnValue = new X509Certificate2(fileName, password, X509KeyStorageFlags.MachineKeySet);
            }

            catch (Exception exception)
            {
                throw new ArgumentException("The certificate file " + fileName + " couldn't be loaded - " + exception.Message);
            }

            return functionReturnValue;
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Load the SP certificate
            string fileName = Path.Combine(HttpRuntime.AppDomainAppPath, spCertificateFileName);
            Application[SPX509Certificate] = LoadCertificate(fileName, spPassword);

            // Load the IdP certificate
            fileName = Path.Combine(HttpRuntime.AppDomainAppPath, idpCertificateFileName);
            Application[IdPX509Certificate] = LoadCertificate(fileName, null);
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}