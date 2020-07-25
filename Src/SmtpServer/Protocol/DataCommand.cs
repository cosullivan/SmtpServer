using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Storage;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    public sealed class DataCommand : SmtpCommand
    {
        public const string Command = "DATA";

        readonly IMessageStoreFactory _messageStoreFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageStoreFactory">The message store factory to use when creating the message stores.</param>
        internal DataCommand(IMessageStoreFactory messageStoreFactory) : base(Command)
        {
            _messageStoreFactory = messageStoreFactory;
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
            if (context.Transaction.To.Count == 0)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return false;
            }

            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.StartMailInput, "end with <CRLF>.<CRLF>"), cancellationToken).ConfigureAwait(false);

            try
            {
                using var container = new DisposableContainer<IMessageStore>(_messageStoreFactory.CreateInstance(context));

                SmtpResponse response = null;

                await context.Pipe.Input.ReadDotBlockAsync(
                    async buffer =>
                    {
#if DEBUG
                        Console.WriteLine(StringUtil.Create(buffer));
#endif 

                        // ReSharper disable once AccessToDisposedClosure
                        response = await container.Instance.SaveAsync(context, context.Transaction, buffer, cancellationToken).ConfigureAwait(false);
                    }, 
                    cancellationToken).ConfigureAwait(false);
                    
                await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
    }
}