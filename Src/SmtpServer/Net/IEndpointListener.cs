using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Net
{
    public interface IEndpointListener : IDisposable
    {
        /// <summary>
        /// Returns a network pipe to the endpoint.
        /// </summary>
        /// <param name="context">The session context that the pipe is being created for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The network pipe from the endpoint.</returns>
        Task<INetworkPipe> GetPipeAsync(ISessionContext context, CancellationToken cancellationToken);
    }
}