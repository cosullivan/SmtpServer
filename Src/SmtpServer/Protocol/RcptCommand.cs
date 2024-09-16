using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Rcpt Command
    /// </summary>
    public sealed class RcptCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Rcpt Command
        /// </summary>
        public const string Command = "RCPT";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        public RcptCommand(IMailbox address) : base(Command)
        {
            Address = address;
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
            var mailboxFilter = context.ServiceProvider.GetService<IMailboxFilterFactory, IMailboxFilter>(context, MailboxFilter.Default);

            using var container = new DisposableContainer<IMailboxFilter>(mailboxFilter);

            switch (await container.Instance.CanDeliverToAsync(context, Address, context.Transaction.From, cancellationToken).ConfigureAwait(false))
            {
                case true:
                    context.Transaction.To.Add(Address);
                    await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);
                    return true;

                case false:
                    await context.Pipe.Output.WriteReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
                    return false;
            }

            throw new NotSupportedException("The Acceptance state is not supported.");
        }

        /// <summary>
        /// Gets the address that the mail is to.
        /// </summary>
        public IMailbox Address { get; }
    }
}
