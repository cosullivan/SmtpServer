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
        /// <param name="options">The server options.</param>
        internal QuitCommand(ISmtpServerOptions options) : base(options) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.IsQuitRequested = true;

            return context.Client.ReplyAsync(SmtpResponse.ServiceClosingTransmissionChannel, cancellationToken);
        }
    }
}