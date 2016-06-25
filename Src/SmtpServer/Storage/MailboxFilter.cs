using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public abstract class MailboxFilter : IMailboxFilter
    {
        /// <summary>
        /// Creates an instance of the message box filter specifically for this session.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <returns>The mailbox filter instance specifically for this session.</returns>
        public virtual IMailboxFilter CreateSessionInstance(ISessionContext context)
        {
            return this;
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public virtual Task<MailboxFilterResult> CanAcceptFromAsync(IMailbox @from, int size = 0)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public virtual Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox @from)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }
    }
}
