using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public abstract class MessageStore : IMessageStore
    {
        /// <summary>
        /// Creates an instance of the message store specifically for this session.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <returns>The message store instance specifically for this session.</returns>
        public virtual IMessageStore CreateSessionInstance(ISessionContext context)
        {
            return this;
        }

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public abstract Task<string> SaveAsync(IMimeMessage message, CancellationToken cancellationToken);
    }
}