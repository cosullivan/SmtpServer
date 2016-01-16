using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class RcptCommand : SmtpCommand
    {
        readonly IMailbox _address;
        readonly IMailboxFilter _filter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="filter">The mailbox filter to test against the mailbox.</param>
        public RcptCommand(IMailbox address, IMailboxFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _address = address;
            _filter = filter;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            switch (await _filter.CanDeliverToAsync(_address, context.Transaction.From))
            {
                case MailboxFilterResult.Yes:
                    context.Transaction.To.Add(_address);
                    await context.Text.ReplyAsync(SmtpResponse.Ok, cancellationToken);
                    return;

                case MailboxFilterResult.NoTemporarily:
                    await context.Text.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken);
                    return;

                case MailboxFilterResult.NoPermanently:
                    await context.Text.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken);
                    return;
            }

            throw new NotSupportedException("The Acceptance state is not supported.");
        }

        /// <summary>
        /// Gets the address that the mail is to.
        /// </summary>
        public IMailbox Address
        {
            get { return _address; }
        }
    }
}
