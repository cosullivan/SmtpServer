using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Message Store
    /// </summary>
    public abstract class MessageStore : IMessageStore
    {
        /// <summary>
        /// Default Message Store
        /// </summary>
        public static readonly IMessageStore Default = new DefaultMessageStore();

        /// <inheritdoc />
        public abstract Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        sealed class DefaultMessageStore : MessageStore
        {
            /// <inheritdoc />
            public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                return Task.FromResult(SmtpResponse.Ok);
            }
        }
    }
}
