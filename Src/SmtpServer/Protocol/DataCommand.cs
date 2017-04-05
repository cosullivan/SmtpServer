using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class DataCommand : SmtpCommand
    {
        readonly IMessageStoreFactory _messageStoreFactory;

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
        public override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.Transaction.To.Count == 0)
            {
                await context.Text.ReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return;
            }

            await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.StartMailInput, "end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

            await ReceiveContentAsync(context, cancellationToken);

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
        async Task ReceiveContentAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.TransferEncoding == ContentEncoding.EightBit)
            {
                context.Text = new NetworkTextStream(context.Text.GetInnerStream(), Encoding.UTF8);
            }

            await ReceiveShortLineContentAsync(context, cancellationToken);

            if (context.TransferEncoding == ContentEncoding.EightBit)
            {
                context.Text = new NetworkTextStream(context.Text.GetInnerStream(), Encoding.ASCII);
            }
        }

        /// <summary>
        /// Receive the message content in short line format.
        /// </summary>
        /// <param name="context">The SMTP session context to receive the message within.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        async Task ReceiveShortLineContentAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            try
            {
                string text;
                while ((text = await context.Text.ReadLineAsync(TimeSpan.FromSeconds(60), cancellationToken).ConfigureAwait(false)) != ".")
                {
                    // need to trim the '.' at the start of the line if it 
                    // exists as this would have been added for transparency
                    // http://tools.ietf.org/html/rfc5321#section-4.5.2
                    context.Transaction.Mime.AppendLine(text.TrimStart('.'));
                }
            }
            catch (TimeoutException)
            {
                // TODO: not sure what the best thing to do here is
                throw;
            }
        }
    }
}