using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public abstract class MessageStore : IMessageStore, IMessageStoreFactory
    {
        /// <summary>
        /// Creates an instance of the message store for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The message store instance for the session context.</returns>
        public virtual IMessageStore CreateInstance(ISessionContext context)
        {
            return this;
        }

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public abstract Task<string> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken);
    }
}