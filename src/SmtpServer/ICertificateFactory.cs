using System.Security.Cryptography.X509Certificates;

namespace SmtpServer
{
    /// <summary>
    /// Certificate Factory Interface
    /// </summary>
    public interface ICertificateFactory
    {
        /// <summary>
        /// Returns the certificate to use for the session.
        /// </summary>
        /// <param name="sessionContext">The session context.</param>
        /// <returns>The certificate to use when starting a TLS session.</returns>
        X509Certificate GetServerCertificate(ISessionContext sessionContext);
    }
}
