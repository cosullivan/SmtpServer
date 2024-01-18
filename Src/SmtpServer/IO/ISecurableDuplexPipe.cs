using System;
using System.IO.Pipelines;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public interface ISecurableDuplexPipe : IDuplexPipe, IDisposable
    {
        /// <summary>
        /// Upgrade to a secure pipeline.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="remoteCertificateValidationCallback">A user-provided callback method to validate the client certificate. If is null, no validation procedure will be performed</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback remoteCertificateValidationCallback = null);

        /// <summary>
        /// Returns a value indicating whether or not the current pipeline is secure.
        /// </summary>
        bool IsSecure { get; }
    }
}