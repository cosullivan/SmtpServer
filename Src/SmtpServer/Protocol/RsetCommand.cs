using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class RsetCommand : SmtpCommand
    {
        public const string Command = "RSET";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        internal RsetCommand(ISmtpServerOptions options) : base(options) { }

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

            await context.NetworkClient.ReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}