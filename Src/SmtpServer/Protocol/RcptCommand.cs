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
        readonly IMailboxFilterFactory _mailboxFilterFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="mailboxFilterFactory">The mailbox filter factory to use.</param>
        public RcptCommand(IMailbox address, IMailboxFilterFactory mailboxFilterFactory)
        {
            if (mailboxFilterFactory == null)
            {
                throw new ArgumentNullException(nameof(mailboxFilterFactory));
            }

            _address = address;
            _mailboxFilterFactory = mailboxFilterFactory;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            using (var container = new DisposableContainer<IMailboxFilter>(_mailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanDeliverToAsync(context, _address, context.Transaction.From))
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