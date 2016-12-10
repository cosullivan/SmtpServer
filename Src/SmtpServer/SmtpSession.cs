using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using SmtpServer.Protocol.Text;

namespace SmtpServer
{
    internal sealed class SmtpSession : IDisposable
    {
        readonly ISmtpServerOptions _options;
        readonly TcpClient _tcpClient;
        readonly SmtpStateMachine _stateMachine;
        TaskCompletionSource<bool> _taskCompletionSource;
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
            _tcpClient = tcpClient;
            _stateMachine = stateMachine;

            Context = new SmtpSessionContext(new SmtpTransaction(), stateMachine, tcpClient.Client.RemoteEndPoint)
            {
                Text = new NetworkTextStream(tcpClient.GetStream())
            };
        }

        /// <summary>
        /// Executes the session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Run(CancellationToken cancellationToken)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();

            RunAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    try
                    {
                        _taskCompletionSource.SetResult(t.IsCompleted);
                    }
                    catch
                    {
                        _taskCompletionSource.SetResult(false);
                    }
                }, 
                cancellationToken);
        }

        /// <summary>
        /// Handles the SMTP session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task RunAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await OutputGreetingAsync(cancellationToken).ConfigureAwait(false);

            while (_retryCount-- > 0 && Context.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                var text = await Context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                SmtpCommand command;
                SmtpResponse errorResponse;
                if (_stateMachine.TryAccept(new TokenEnumerator(new StringTokenReader(text)), out command, out errorResponse) == false)
                {
                    await OuputErrorMessageAsync(errorResponse, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                // the command was a normal command so we can reset the retry count
                _retryCount = 5;

                await command.ExecuteAsync(Context, cancellationToken).ConfigureAwait(false);
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

            await Context.Text.WriteLineAsync($"220 {_options.ServerName} v{version} ESMTP ready", cancellationToken).ConfigureAwait(false);
            await Context.Text.FlushAsync(cancellationToken).ConfigureAwait(false);
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

            return Context.Text.ReplyAsync(response, cancellationToken);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Context.Text.Dispose();

            _tcpClient.Close();
            _taskCompletionSource.Task.Dispose();
        }

        /// <summary>
        /// Returns the context for the session.
        /// </summary>
        internal SmtpSessionContext Context { get; }
        
        /// <summary>
        /// Returns the completion task.
        /// </summary>
        internal Task<bool> Task
        {
            get { return _taskCompletionSource.Task; }
        }
    }
}