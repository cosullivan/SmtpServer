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
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.Transaction.To.Count == 0)
            {
                await context.NetworkClient.ReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return false;
            }

            await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.StartMailInput, "end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

            context.Transaction.Message = await ReadMessageAsync(context, cancellationToken).ConfigureAwait(false);

            try
            {
                using (var container = new DisposableContainer<IMessageStore>(Options.MessageStoreFactory.CreateInstance(context)))
                {
                    var response = await container.Instance.SaveAsync(context, context.Transaction, cancellationToken).ConfigureAwait(false);

                    await context.NetworkClient.ReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
            }

            return true;
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

            return serializer.DeserializeAsync(context.NetworkClient, cancellationToken);
        }
    }
}