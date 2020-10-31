using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.Storage
{
    public abstract class MessageStore : IMessageStore
    {
        public static readonly IMessageStore Default = new DefaultMessageStore();

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="buffer">The buffer that contains the message content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response code to return that indicates the result of the message being saved.</returns>
        public abstract Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        sealed class DefaultMessageStore : MessageStore
        {
            /// <summary>
            /// Save the given message to the underlying storage system.
            /// </summary>
            /// <param name="context">The session context.</param>
            /// <param name="buffer">The buffer that contains the message content.</param>
            /// <param name="transaction">The SMTP message transaction to store.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The response code to return that indicates the result of the message being saved.</returns>
            public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                return Task.FromResult(SmtpResponse.Ok);
            }
        }
    }
}