using System.Net.Security;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Represents an abstract class for validating client certificates.
    /// </summary>
    /// <remarks>
    /// The validation of the client certificate will only take place if the client initiates the "STARTTLS" command.
    /// </remarks>
    public abstract class ClientCertificateValidator : IClientCertificateValidator
    {
        /// <summary>
        /// The default client certificate validator used by the application.
        /// </summary>
        public static readonly IClientCertificateValidator Default = new DefaultClientCertificateValidator();

        /// <inheritdoc />
        public abstract RemoteCertificateValidationCallback RemoteClientCertificateValidationCallback { get; set; }

        /// <summary>
        /// Default implementation of a client certificate validator.
        /// </summary>
        sealed class DefaultClientCertificateValidator : ClientCertificateValidator
        {
            public override RemoteCertificateValidationCallback RemoteClientCertificateValidationCallback { get; set; } = null;
        }
    }
}