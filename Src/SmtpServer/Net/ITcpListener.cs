using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Net
{
    public interface ITcpListener : IDisposable
    {
        /// <summary>
        /// Waits for an incoming connection from a client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TCP client that was received.</returns>
        Task<ITcpClient> AcceptAsync(CancellationToken cancellationToken);
    }
}