using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class StartTlsCommand : SmtpCommand
    {
        readonly X509Certificate _certificate;
        readonly SslProtocols _sslProtocols;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="certificate">The server side certificate to authenticate with.</param>
        /// <param name="sslProtocols">The allowable SSL protocols.</param>
        public StartTlsCommand(X509Certificate certificate, SslProtocols sslProtocols)
        {
            _certificate = certificate;
            _sslProtocols = sslProtocols;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Text.ReplyAsync(SmtpResponse.ServiceReady, cancellationToken);

            var stream = new SslStream(context.Text.GetInnerStream(), true);

            await stream.AuthenticateAsServerAsync(_certificate, false, _sslProtocols, true);

            context.Text = new NetworkTextStream(stream);
        }
    }
}