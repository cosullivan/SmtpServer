using System.Net.Security;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Represents a client certificate validator for validating client certificates.
    /// </summary>
    public abstract class ClientCertificateValidator : IClientCertificateValidator
    {
        /// <summary>
        /// The default client certificate validator used by the application.
        /// </summary>
        public static readonly IClientCertificateValidator Default = new DefaultClientCertificateValidator();

        /// <inheritroc />
        public abstract RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }

        /// <summary>
        /// Default implementation of a client certificate validator.
        /// </summary>
        sealed class DefaultClientCertificateValidator : ClientCertificateValidator
        {
            public override RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; } = null;
        }
    }
}