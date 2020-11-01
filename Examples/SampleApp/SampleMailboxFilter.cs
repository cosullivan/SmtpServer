using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Net;
using SmtpServer.Storage;

namespace SampleApp
{
    public class SampleMailboxFilter : MailboxFilter
    {
        readonly TimeSpan _delay;
        public SampleMailboxFilter() : this(TimeSpan.Zero) { }

        public SampleMailboxFilter(TimeSpan delay)
        {
            _delay = delay;
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override async Task<MailboxFilterResult> CanAcceptFromAsync(
            ISessionContext context, 
            IMailbox @from, 
            int size,
            CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
        
            if (@from == Mailbox.Empty)
            {
                return MailboxFilterResult.NoPermanently;
            }

            var endpoint = (IPEndPoint)context.Properties[EndpointListener.RemoteEndPointKey];
            
            if (endpoint.Address.Equals(IPAddress.Parse("127.0.0.1")))
            {
                return MailboxFilterResult.Yes;
            }

            return MailboxFilterResult.NoPermanently;
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override async Task<MailboxFilterResult> CanDeliverToAsync(
            ISessionContext context, 
            IMailbox to, 
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);

            return MailboxFilterResult.Yes;
        }
    }
}