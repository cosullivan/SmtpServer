using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using SmtpServer.Protocol.Text;

namespace SmtpServer
{
    internal sealed class SmtpSession
    {
        readonly ISmtpServerOptions _options;
        readonly SmtpStateMachine _stateMachine;
        readonly SmtpSessionContext _context;
        int _retryCount = 5;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="tcpClient">The TCP client to operate the session on.</param>
        /// <param name="stateMachine">The SMTP state machine.</param>
        internal SmtpSession(ISmtpServerOptions options, TcpClient tcpClient, SmtpStateMachine stateMachine)
        {
            _options = options;
            _stateMachine = stateMachine;

            _context = new SmtpSessionContext(new SmtpTransaction(), stateMachine, tcpClient.Client.RemoteEndPoint)
            {
                Text = new NetworkTextStream(tcpClient)
            };
        }

        /// <summary>
        /// Handles the SMTP session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await OutputGreetingAsync(cancellationToken).ConfigureAwait(false);

            while (_retryCount-- > 0 && _context.IsQuitRequested == false)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = await _context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                SmtpCommand command;
                SmtpResponse errorResponse;
                if (_stateMachine.TryAccept(new TokenEnumerator(new StringTokenReader(text)), out command, out errorResponse) == false)
                {
                    await OuputErrorMessageAsync(errorResponse, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                // the command was a normal command so we can reset the retry count
                _retryCount = 5;

                await command.ExecuteAsync(_context, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task OutputGreetingAsync(CancellationToken cancellationToken)
        {
            var version = typeof(SmtpSession).Assembly.GetName().Version;

            await _context.Text.WriteLineAsync($"220 {_options.ServerName} v{version} ESMTP ready", cancellationToken).ConfigureAwait(false);
            await _context.Text.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Output the error message.
        /// </summary>
        /// <param name="errorResponse">The response that contains the error message and reply code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        Task OuputErrorMessageAsync(SmtpResponse errorResponse, CancellationToken cancellationToken)
        {
            var response = new SmtpResponse(errorResponse.ReplyCode, $"{errorResponse.Message}, {_retryCount} retry(ies) remaining.");

            return _context.Text.ReplyAsync(response, cancellationToken);
        }
    }
}
