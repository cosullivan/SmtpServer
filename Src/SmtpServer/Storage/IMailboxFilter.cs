using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public interface IMailboxFilter
    {
        /// <summary>
        /// Creates an instance of the message box filter specifically for this session.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <returns>The mailbox filter instance specifically for this session.</returns>
        IMailboxFilter CreateSessionInstance(ISessionContext context);

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        Task<MailboxFilterResult> CanAcceptFromAsync(IMailbox from, int size = 0);

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox from);
    }
}
