using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Start Tls Command
    /// </summary>
    public sealed class StartTlsCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Start Tls Command
        /// </summary>
        public const string Command = "STARTTLS";

        /// <summary>
        /// Constructor.
        /// </summary>
        public StartTlsCommand() : base(Command) { }

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

            var certificate = context.EndpointDefinition.CertificateFactory.GetServerCertificate(context);

            var protocols = context.EndpointDefinition.SupportedSslProtocols;

            await context.Pipe.UpgradeAsync(certificate, protocols, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
