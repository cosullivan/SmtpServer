using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Streaming Message Store Interface
    /// </summary>
    public interface IStreamingMessageStore : IMessageStore
    {
        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="reader">The reader that streams the message content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response code to return that indicates the result of the message being saved.</returns>
        Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, PipeReader reader, CancellationToken cancellationToken);
    }
}
