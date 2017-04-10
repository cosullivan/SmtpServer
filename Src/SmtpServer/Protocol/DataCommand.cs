using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Content;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class DataCommand : SmtpCommand
    {
        readonly IMessageStoreFactory _messageStoreFactory;
        readonly MessageReaderFactory _messageReaderFactory = new MessageReaderFactory();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageStoreFactory">The message store factory.</param>
        public DataCommand(IMessageStoreFactory messageStoreFactory)
        {
            if (messageStoreFactory == null)
            {
                throw new ArgumentNullException(nameof(messageStoreFactory));
            }

            _messageStoreFactory = messageStoreFactory;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.Transaction.To.Count == 0)
            {
                await context.Text.ReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return;
            }

            await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.StartMailInput, "end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

            context.Transaction.Message = await ReadMessageAsync(context, cancellationToken).ConfigureAwait(false);

            try
            {
                // store the transaction
                using (var container = new DisposableContainer<IMessageStore>(_messageStoreFactory.CreateInstance(context)))
                {
                    var response = await container.Instance.SaveAsync(context, context.Transaction, cancellationToken).ConfigureAwait(false);

                    await context.Text.ReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Receive the message content.
        /// </summary>
        /// <param name="context">The SMTP session context to receive the message within.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        async Task<IMessage> ReadMessageAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            var reader = await _messageReaderFactory.CreateInstanceAsync(context.Text.GetInnerStream(), cancellationToken).ConfigureAwait(false);

            return await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}