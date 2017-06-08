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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="address">The address.</param>
        internal RcptCommand(ISmtpServerOptions options, IMailbox address) : base(options)
        {
            Address = address;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            using (var container = new DisposableContainer<IMailboxFilter>(Options.MailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanDeliverToAsync(context, Address, context.Transaction.From, cancellationToken))
                {
                    case MailboxFilterResult.Yes:
                        context.Transaction.To.Add(Address);
                        await context.Client.ReplyAsync(SmtpResponse.Ok, cancellationToken);
                        return;

                    case MailboxFilterResult.NoTemporarily:
                        await context.Client.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken);
                        return;

                    case MailboxFilterResult.NoPermanently:
                        await context.Client.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken);
                        return;
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