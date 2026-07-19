using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Optional mailbox filter interface for recipient parameters supplied on the RCPT command.
    /// </summary>
    public interface IParameterizedMailboxFilter : IMailboxFilter
    {
        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="parameters">The recipient parameters supplied on the RCPT command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the mailbox can be delivered to, false if not.</returns>
        Task<bool> CanDeliverToAsync(
            ISessionContext context,
            IMailbox to,
            IMailbox from,
            IReadOnlyDictionary<string, string> parameters,
            CancellationToken cancellationToken);
    }
}
