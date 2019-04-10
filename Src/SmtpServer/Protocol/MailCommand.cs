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
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Transaction.Reset();
            context.Transaction.Parameters = Parameters;

            // check if a size has been defined
            var size = GetMessageSize();

            // check against the server supplied maximum
            if (Options.MaxMessageSize > 0 && size > Options.MaxMessageSize)
            {
                await context.NetworkClient.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
                return false;
            }

            using (var container = new DisposableContainer<IMailboxFilter>(Options.MailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanAcceptFromAsync(context, Address, size, cancellationToken).ConfigureAwait(false))
                {
                    case MailboxFilterResult.Yes:
                        context.Transaction.From = Address;
                        await context.NetworkClient.ReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);
                        return true;

                    case MailboxFilterResult.NoTemporarily:
                        await context.NetworkClient.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
                        return false;

                    case MailboxFilterResult.NoPermanently:
                        await context.NetworkClient.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken).ConfigureAwait(false);
                        return false;

                    case MailboxFilterResult.SizeLimitExceeded:
                        await context.NetworkClient.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
                        return false;
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