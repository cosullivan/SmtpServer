using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class StartTlsCommand : SmtpCommand
    {
        public const string Command = "STARTTLS";

        readonly X509Certificate _certificate;
        readonly SslProtocols _sslProtocols;

        /// <summary>
        /// Constructor.
        /// </summary>
        internal StartTlsCommand(X509Certificate certificate, SslProtocols sslProtocols) : base(Command)
        {
            _certificate = certificate;
            _sslProtocols = sslProtocols;
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
            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.ServiceReady, cancellationToken).ConfigureAwait(false);

            await context.Pipe.UpgradeAsync(_certificate, _sslProtocols, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}