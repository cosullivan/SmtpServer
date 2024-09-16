using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Mailbox Filter
    /// </summary>
    public abstract class MailboxFilter : IMailboxFilter
    {
        /// <summary>
        /// Default Mailbox Filter
        /// </summary>
        public static readonly IMailboxFilter Default = new DefaultMailboxFilter();

        /// <inheritdoc />
        public virtual Task<bool> CanAcceptFromAsync(
            ISessionContext context, 
            IMailbox @from, 
            int size,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task<bool> CanDeliverToAsync(
            ISessionContext context, 
            IMailbox to, 
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        sealed class DefaultMailboxFilter : MailboxFilter { }
    }
}
