using System.Net.Security;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Provides an interface for validating client certificates.
    /// </summary>
    public interface IClientCertificateValidator
    {
        /// <summary>
        /// Gets or sets the callback method used to validate the client certificate provided by the remote party in a secure connection.
        /// </summary>
        RemoteCertificateValidationCallback RemoteClientCertificateValidationCallback { get; set; }
    }
}