using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class StartTlsCommand : SmtpCommand
    {
        public const string Command = "STARTTLS";

        /// <summary>
        /// Constructor.
        /// </summary>
        internal StartTlsCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            //await context.NetworkClient.ReplyAsync(SmtpResponse.ServiceReady, cancellationToken).ConfigureAwait(false);
            //await context.NetworkClient.Stream.UpgradeAsync(Options.ServerCertificate, Options.SupportedSslProtocols, cancellationToken).ConfigureAwait(false);

            //return true;

            throw new NotImplementedException();
        }
    }
}