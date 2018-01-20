using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class DataCommand : SmtpCommand
    {
        public const string Command = "DATA";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        internal DataCommand(ISmtpServerOptions options) : base(options) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.Transaction.To.Count == 0)
            {
                await context.Client.ReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return;
            }

            await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.StartMailInput, "Receive message content; end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

            context.Transaction.Message = await ReadMessageAsync(context, cancellationToken).ConfigureAwait(false);

            try
            {
                using (var container = new DisposableContainer<IMessageStore>(Options.MessageStoreFactory.CreateInstance(context)))
                {
                    var response = await container.Instance.SaveAsync(context, context.Transaction, cancellationToken).ConfigureAwait(false);

                    await context.Client.ReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Receive the message content.
        /// </summary>
        /// <param name="context">The SMTP session context to receive the message within.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        Task<IMessage> ReadMessageAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var serializer = new MessageSerializerFactory().CreateInstance();

            return serializer.DeserializeAsync(context.Client, cancellationToken);
        }
    }
}
