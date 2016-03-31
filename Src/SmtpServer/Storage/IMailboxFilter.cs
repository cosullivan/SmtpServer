using System.Net;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    public interface IMailboxFilter
    {
        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point of the client making the connection.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        Task<MailboxFilterResult> CanAcceptFromAsync(EndPoint remoteEndPoint, IMailbox from, int size = 0);

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox from);
    }
}
