using System.Net;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SampleApp
{
    public class ConsoleMailboxFilter : IMailboxFilter
    {
        readonly IPEndPoint _remoteEndPoint;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConsoleMailboxFilter() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint of the client connection.</param>
        ConsoleMailboxFilter(IPEndPoint remoteEndPoint)
        {
            _remoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Creates an instance of the message box filter specifically for this session.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point of the client making the connection.</param>
        /// <returns>The mailbox filter instance specifically for this session.</returns>
        public IMailboxFilter CreateSessionInstance(EndPoint remoteEndPoint)
        {
            return new ConsoleMailboxFilter((IPEndPoint)remoteEndPoint);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public Task<MailboxFilterResult> CanAcceptFromAsync(IMailbox @from, int size = 0)
        {
            // only accept connections from localhost
            if (_remoteEndPoint.Address.Equals(IPAddress.Parse("127.0.0.1")))
            {
                return Task.FromResult(MailboxFilterResult.Yes);
            }

            return Task.FromResult(MailboxFilterResult.NoPermanently);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox @from)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }
    }
}