using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Net
{
    public interface IEndpointListener : IDisposable
    {
        /// <summary>
        /// Returns a stream from the endpoint.
        /// </summary>
        /// <param name="context">The session context that the stream is being received for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream from the endpoint.</returns>
        Task<INetworkStream> GetStreamAsync(ISessionContext context, CancellationToken cancellationToken);
    }
}