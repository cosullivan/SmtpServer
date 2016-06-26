using System.Net;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SampleApp
{
    public class ConsoleMailboxFilter : MailboxFilter
    {
        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size = 0)
        {
            var endpoint = (IPEndPoint) context.RemoteEndPoint;
            
            // only accept connections from localhost
            if (endpoint.Address.Equals(IPAddress.Parse("127.0.0.1")))
            {
                return Task.FromResult(MailboxFilterResult.Yes);
            }

            return Task.FromResult(MailboxFilterResult.NoPermanently);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox @from)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }
    }
}