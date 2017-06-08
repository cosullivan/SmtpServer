using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public abstract class MailboxFilter : IMailboxFilter, IMailboxFilterFactory
    {
        /// <summary>
        /// Creates an instance of the message box filter.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The mailbox filter for the session.</returns>
        public virtual IMailboxFilter CreateInstance(ISessionContext context)
        {
            return this;
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public virtual Task<MailboxFilterResult> CanAcceptFromAsync(
            ISessionContext context, 
            IMailbox @from, 
            int size,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public virtual Task<MailboxFilterResult> CanDeliverToAsync(
            ISessionContext context, 
            IMailbox to, 
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }
    }
}