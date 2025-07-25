using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Mail Command
    /// </summary>
    public sealed class MailCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Mail Command
        /// </summary>
        public const string Command = "MAIL";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
        public MailCommand(IMailbox address, IReadOnlyDictionary<string, string> parameters) : base(Command)
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
            if (context.EndpointDefinition.AuthenticationRequired && context.Authentication.IsAuthenticated == false)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationRequired, cancellationToken).ConfigureAwait(false);
                return false;
            }

            context.Transaction.Reset();
            context.Transaction.Parameters = Parameters;

            // check if a size has been defined
            var size = GetMessageSize();

            // check against the server supplied maximum
            if (context.ServerOptions.MaxMessageSizeOptions.Length > 0 && size > context.ServerOptions.MaxMessageSizeOptions.Length)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var mailboxFilter = context.ServiceProvider.GetService<IMailboxFilterFactory, IMailboxFilter>(context, MailboxFilter.Default);

            using var container = new DisposableContainer<IMailboxFilter>(mailboxFilter);

            switch (await container.Instance.CanAcceptFromAsync(context, Address, size, cancellationToken).ConfigureAwait(false))
            {
                case true:
                    context.Transaction.From = Address;
                    await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);
                    return true;

                case false:
                    await context.Pipe.Output.WriteReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken).ConfigureAwait(false);
                    return false;
            }

            throw new SmtpResponseException(SmtpResponse.TransactionFailed);
        }

        /// <summary>
        /// Gets the estimated message size supplied from the ESMTP command extension.
        /// </summary>
        /// <returns>The estimated message size that was supplied by the client.</returns>
        int GetMessageSize()
        {
            if (Parameters.TryGetValue("SIZE", out var value) == false)
            {
                return 0;
            }

            return int.TryParse(value, out var size) == false ? 0 : size;
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
