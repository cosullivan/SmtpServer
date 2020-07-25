using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class RcptCommand : SmtpCommand
    {
        public const string Command = "RCPT";

        readonly IMailboxFilterFactory _mailboxFilterFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="mailboxFilterFactory">The mailbox filter factory used for creating instances of mailbox filters.</param>
        internal RcptCommand(IMailbox address, IMailboxFilterFactory mailboxFilterFactory) : base(Command)
        {
            Address = address;

            _mailboxFilterFactory = mailboxFilterFactory;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            using (var container = new DisposableContainer<IMailboxFilter>(_mailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanDeliverToAsync(context, Address, context.Transaction.From, cancellationToken).ConfigureAwait(false))
                {
                    case MailboxFilterResult.Yes:
                        context.Transaction.To.Add(Address);
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);
                        return true;

                    case MailboxFilterResult.NoTemporarily:
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
                        return false;

                    case MailboxFilterResult.NoPermanently:
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
                        return false;
                }
            }

            throw new NotSupportedException("The Acceptance state is not supported.");
        }

        /// <summary>
        /// Gets the address that the mail is to.
        /// </summary>
        public IMailbox Address { get; }
    }
}