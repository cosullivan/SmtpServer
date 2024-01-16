using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class StartTlsCommand : SmtpCommand
    {
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
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occur, false
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.ServiceReady, cancellationToken).ConfigureAwait(false);

            var certificate = context.EndpointDefinition.CertificateFactory.GetServerCertificate(context);
            var protocols = context.EndpointDefinition.SupportedSslProtocols;

            var clientCertificateValidator = context.ServiceProvider.GetService<IClientCertificateValidatorFactory, IClientCertificateValidator>(context, ClientCertificateValidator.Default);

            await context.Pipe.UpgradeAsync(certificate, protocols, cancellationToken, clientCertificateValidator.RemoteClientCertificateValidationCallback).ConfigureAwait(false);

            return true;
        }
    }
}