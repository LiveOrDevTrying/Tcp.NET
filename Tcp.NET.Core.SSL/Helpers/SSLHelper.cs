using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tcp.NET.Core.SSL.Helpers
{
    public static class SSLHelper
    {
        public static X509Certificate GetServerCert(string subject, StoreLocation storeLocation)
        {
            var store = new X509Store(StoreName.My, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (var currentCertificate
               in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name != null && currentCertificate.Subject.Equals(subject))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }

            return foundCertificate;
        }
    }
}
