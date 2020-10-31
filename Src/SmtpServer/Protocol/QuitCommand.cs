using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class QuitCommand : SmtpCommand
    {
        public const string Command = "QUIT";

        /// <summary>
        /// Constructor.
        /// </summary>
        public QuitCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.IsQuitRequested = true;

            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.ServiceClosingTransmissionChannel, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}