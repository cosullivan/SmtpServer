using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class MailCommand : SmtpCommand
    {
        readonly ISmtpServerOptions _options;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options that the server was created with.</param>
        /// <param name="address">The address.</param>
        /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
        public MailCommand(ISmtpServerOptions options, IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            _options = options;

            Address = address;
            Parameters = parameters;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Transaction.Reset();

            // check if a size has been defined
            var size = GetMessageSize();

            // check against the server supplied maximum
            if (_options.MaxMessageSize > 0 && size > _options.MaxMessageSize)
            {
                await context.Text.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken);
                return;
            }

            using (var container = new DisposableContainer<IMailboxFilter>(_options.MailboxFilterFactory.CreateInstance(context)))
            {
                switch (await container.Instance.CanAcceptFromAsync(context, Address, size))
                {
                    case MailboxFilterResult.Yes:
                        context.Transaction.From = Address;
                        context.TransferEncoding = GetTransferEncoding() ?? context.TransferEncoding;
                        await context.Text.ReplyAsync(SmtpResponse.Ok, cancellationToken);
                        return;

                    case MailboxFilterResult.NoTemporarily:
                        await context.Text.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken);
                        return;

                    case MailboxFilterResult.NoPermanently:
                        await context.Text.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken);
                        return;

                    case MailboxFilterResult.SizeLimitExceeded:
                        await context.Text.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken);
                        return;
                }
            }

            throw new NotSupportedException("The Acceptance state is not supported.");
        }

        /// <summary>
        /// Returns the content transfer encoding if it has been specified through the extended mail parameters.
        /// </summary>
        /// <returns>The required content transfer encoding to use.</returns>
        ContentTransferEncoding? GetTransferEncoding()
        {
            string value;
            if (Parameters.TryGetValue("BODY", out value) == false)
            {
                return ContentTransferEncoding.SevenBit;
            }

            if (String.Equals(value, "8BITMIME", StringComparison.OrdinalIgnoreCase))
            {
                return ContentTransferEncoding.EightBit;
            }

            return ContentTransferEncoding.SevenBit;
        }

        /// <summary>
        /// Gets the estimated message size supplied from the ESMTP command extension.
        /// </summary>
        /// <returns>The estimated message size that was supplied by the client.</returns>
        int GetMessageSize()
        {
            string value;
            if (Parameters.TryGetValue("SIZE", out value) == false)
            {
                return 0;
            }

            int size;
            if (Int32.TryParse(value, out size) == false)
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