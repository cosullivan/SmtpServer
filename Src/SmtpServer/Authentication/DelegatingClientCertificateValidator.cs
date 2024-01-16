using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SmtpServer.Authentication
{
    public sealed class DelegatingClientCertificateValidator : ClientCertificateValidator
    {
        private readonly Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> _delegate;

        /// <summary>
        /// Represents a client certificate validator that delegates the certificate validation
        /// logic to a specified function.
        /// </summary>
        public DelegatingClientCertificateValidator(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Gets or sets the client certificate validation callback used for remote certificate validation.
        /// </summary>
        /// <value>
        /// The certificate validation callback.
        /// </value>
        /// <exception cref="InvalidOperationException">Thrown when attempting to set this property.</exception>
        public override RemoteCertificateValidationCallback RemoteClientCertificateValidationCallback
        {
            get => (a,b,c,d) => _delegate(a,b,c,d);
            set => throw new InvalidOperationException("Cannot set this property");
        }
    }
}