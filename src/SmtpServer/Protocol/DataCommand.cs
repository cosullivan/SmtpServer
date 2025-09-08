using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Data Command
    /// </summary>
    public sealed class DataCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Data Command
        /// </summary>
        public const string Command = "DATA";

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataCommand() : base(Command) { }

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

            var messageStore = context.ServiceProvider.GetService<IMessageStoreFactory, IMessageStore>(context, MessageStore.Default);

            try
            {
                using var container = new DisposableContainer<IMessageStore>(messageStore);

                SmtpResponse response = null;

                await context.Pipe.Input.ReadDotBlockAsync(
                    async buffer =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        response = await container.Instance.SaveAsync(context, context.Transaction, buffer, cancellationToken).ConfigureAwait(false);
                    },
                    context.ServerOptions.MaxMessageSizeOptions,
                    cancellationToken).ConfigureAwait(false);
                    
                await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch (SmtpResponseException)
            {
                return false;
            }
            catch (Exception)
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
        }
    }
}
