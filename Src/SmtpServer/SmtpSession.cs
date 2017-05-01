using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using System.Reflection;
using SmtpServer.IO;

namespace SmtpServer
{
    internal sealed class SmtpSession : IDisposable
    {
        readonly ISmtpServerOptions _options;
        readonly TcpClient _tcpClient;
        readonly SmtpCommandProcessor _processor;
        TaskCompletionSource<bool> _taskCompletionSource;
        
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
            _processor = new SmtpCommandProcessor(options.MaxRetryCount);
            
            Context = new SmtpSessionContext(tcpClient, new SmtpMessageTransaction(), stateMachine);
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

            await _processor.ExecuteAsync(Context, cancellationToken);
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task OutputGreetingAsync(CancellationToken cancellationToken)
        {
            var version = typeof(SmtpSession).GetTypeInfo().Assembly.GetName().Version;

            await Context.Text.WriteLineAsync($"220 {_options.ServerName} v{version} ESMTP ready", cancellationToken).ReturnOnAnyThread();
            await Context.Text.FlushAsync(cancellationToken).ReturnOnAnyThread();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Context.Text.Dispose();

            ((IDisposable)_tcpClient).Dispose();
//#if !NETSTANDARD1_6
//            _taskCompletionSource.Task.Dispose();
//#endif
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