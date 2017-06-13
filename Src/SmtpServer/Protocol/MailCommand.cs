using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class MailCommand : SmtpCommand
    {
        public const string Command = "MAIL";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="address">The address.</param>
        /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
        internal MailCommand(ISmtpServerOptions options, IMailbox address, IReadOnlyDictionary<string, string> parameters) : base(options)
        {
            Address = address;
            Parameters = parameters;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Transaction.Reset();
            context.Transaction.Parameters = Parameters;

            // check if a size has been defined
            var size = GetMessageSize();

            // check against the server supplied maximum
            if (Options.MaxMessageSize > 0 && size > Options.MaxMessageSize)
            {
                await context.Client.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken).ReturnOnAnyThread();
                return;
            }

            using (var container = new DisposableContainer<IMailboxFilter>(Options.MailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanAcceptFromAsync(context, Address, size, cancellationToken))
                {
                    case MailboxFilterResult.Yes:
                        context.Transaction.From = Address;
                        await context.Client.ReplyAsync(SmtpResponse.Ok, cancellationToken).ReturnOnAnyThread();
                        return;

                    case MailboxFilterResult.NoTemporarily:
                        await context.Client.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken).ReturnOnAnyThread();
                        return;

                    case MailboxFilterResult.NoPermanently:
                        await context.Client.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken).ReturnOnAnyThread();
                        return;

                    case MailboxFilterResult.SizeLimitExceeded:
                        await context.Client.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken).ReturnOnAnyThread();
                        return;
                }
            }

            throw new SmtpResponseException(SmtpResponse.TransactionFailed);
        }

        /// <summary>
        /// Gets the estimated message size supplied from the ESMTP command extension.
        /// </summary>
        /// <returns>The estimated message size that was supplied by the client.</returns>
        int GetMessageSize()
        {
            if (Parameters.TryGetValue("SIZE", out string value) == false)
            {
                return 0;
            }

            if (Int32.TryParse(value, out int size) == false)
            {
                return 0;
            }

            return size;
        }

        /// <summary>
        /// Gets the address that the mail is from.
        /// </summary>
        public IMailbox Address { get; }

        /// <summary>
        /// The list of extended mail parameters.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }
    }
}