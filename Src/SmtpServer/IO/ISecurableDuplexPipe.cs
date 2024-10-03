using System;
using System.IO.Pipelines;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    /// <summary>
    /// Securable Duplex Pipe Interface
    /// </summary>
    public interface ISecurableDuplexPipe : IDuplexPipe, IDisposable
    {
        /// <summary>
        /// Upgrade to a secure pipeline.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a value indicating whether or not the current pipeline is secure.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Returns the used SslProtocol of a secure pipeline
        /// </summary>
        SslProtocols SslProtocol { get; }
    }
}
