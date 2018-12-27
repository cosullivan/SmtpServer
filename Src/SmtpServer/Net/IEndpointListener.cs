using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        Task<Stream> GetStreamAsync(ISessionContext context, CancellationToken cancellationToken);
    }
}