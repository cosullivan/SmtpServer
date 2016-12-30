using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using SmtpServer.Protocol.Text;

namespace SmtpServer
{
    public class SmtpCommandProcessor : ISmtpCommandProcessor
    {
        readonly int _maxRetries;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SmtpCommandProcessor() : this(5) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxRetries">The maximum number of retries.</param>
        public SmtpCommandProcessor(int maxRetries)
        {
            _maxRetries = maxRetries;
        }

        /// <summary>
        /// Execute the command handler against the specified session context.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public virtual async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            var retryCount = _maxRetries;

            while (retryCount-- > 0 && context.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                var text = await context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                SmtpCommand command;
                SmtpResponse errorResponse;
                if (TryAccept(context, text, out command, out errorResponse) == false)
                {
                    var response = new SmtpResponse(errorResponse.ReplyCode, $"{errorResponse.Message}, {retryCount} retry(ies) remaining.");

                    await context.Text.ReplyAsync(response, cancellationToken);

                    continue;
                }

                // the command was a normal command so we can reset the retry count
                retryCount = _maxRetries;

                await ExecuteAsync(command, context, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advances the enumerator to the next command in the stream.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="text">The text to return the commands from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        protected virtual bool TryAccept(ISmtpSessionContext context, string text, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return context.StateMachine.TryAccept(new TokenEnumerator(new StringTokenReader(text)), out command, out errorResponse);
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        protected virtual Task ExecuteAsync(SmtpCommand command, ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(context, cancellationToken);
        }
    }
}