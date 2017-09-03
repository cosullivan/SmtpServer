using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using System.Reflection;
using SmtpServer.IO;
using SmtpServer.Text;

namespace SmtpServer
{
    internal sealed class SmtpSession : IDisposable
    {
        readonly ISmtpServerOptions _options;
        readonly TcpClient _tcpClient;
        readonly SmtpStateMachine _stateMachine;
        readonly SmtpSessionContext _context;
        TaskCompletionSource<bool> _taskCompletionSource;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="tcpClient">The TCP client to operate the session on.</param>
        internal SmtpSession(ISmtpServerOptions options, TcpClient tcpClient)
        {
            _options = options;
            _tcpClient = tcpClient;
            _context = new SmtpSessionContext(options, tcpClient);
            _stateMachine = new SmtpStateMachine(options, _context);
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

            await OutputGreetingAsync(cancellationToken).ReturnOnAnyThread();

            await ExecuteAsync(Context, cancellationToken).ReturnOnAnyThread();
        }

        /// <summary>
        /// Execute the command handler against the specified session context.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var retries = _options.MaxRetryCount;

            while (retries-- > 0 && context.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                var text = await ReadCommandInputAsync(context, cancellationToken);

                if (text == null)
                {
                    return;
                }

                if (TryAccept(context, text, out SmtpCommand command, out SmtpResponse response))
                {
                    try
                    {
                        await ExecuteAsync(command, context, cancellationToken).ReturnOnAnyThread();

                        retries = _options.MaxRetryCount;

                        continue;
                    }
                    catch (SmtpResponseException responseException)
                    {
                        context.IsQuitRequested = responseException.IsQuitRequested;

                        response = responseException.Response;
                    }
                    catch (OperationCanceledException)
                    {
                        await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), cancellationToken);
                        return;
                    }
                }

                await context.Client.ReplyAsync(CreateErrorResponse(response, retries), cancellationToken);
            }
        }

        /// <summary>
        /// Read the command input.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The input that was received from the client.</returns>
        async Task<IReadOnlyList<ArraySegment<byte>>> ReadCommandInputAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            try
            {
                return await context.Client.ReadLineAsync(_options.CommandWaitTimeout, cancellationToken).ReturnOnAnyThread();
            }
            catch (TimeoutException)
            {
                await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "Timeout whilst waiting for input."), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Create an error response.
        /// </summary>
        /// <param name="response">The original response to wrap with the error message information.</param>
        /// <param name="retries">The number of retries remaining before the session is terminated.</param>
        /// <returns>The response that wraps the original response with the additional error information.</returns>
        static SmtpResponse CreateErrorResponse(SmtpResponse response, int retries)
        {
            return new SmtpResponse(response.ReplyCode, $"{response.Message}, {retries} retry(ies) remaining.");
        }

        /// <summary>
        /// Advances the enumerator to the next command in the stream.
        /// </summary>
        /// <param name="context">The session context to use when making session based transitions.</param>
        /// <param name="segments">The list of array segments to read the command from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        bool TryAccept(SmtpSessionContext context, IReadOnlyList<ArraySegment<byte>> segments, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var tokenEnumerator = new TokenEnumerator(new ByteArrayTokenReader(segments));

            return _stateMachine.TryAccept(context, tokenEnumerator, out command, out errorResponse);
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        Task ExecuteAsync(SmtpCommand command, SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.RaiseCommandExecuting(command);

            return command.ExecuteAsync(context, cancellationToken);
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task OutputGreetingAsync(CancellationToken cancellationToken)
        {
            var version = typeof(SmtpSession).GetTypeInfo().Assembly.GetName().Version;

            await Context.Client.WriteLineAsync($"220 {_options.ServerName} v{version} ESMTP ready", cancellationToken).ReturnOnAnyThread();
            await Context.Client.FlushAsync(cancellationToken).ReturnOnAnyThread();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Context.Client.Dispose();

            ((IDisposable)_tcpClient).Dispose();
//#if !NETSTANDARD1_6
//            _taskCompletionSource.Task.Dispose();
//#endif
        }

        /// <summary>
        /// Returns the context for the session.
        /// </summary>
        internal SmtpSessionContext Context => _context;
        
        /// <summary>
        /// Returns the completion task.
        /// </summary>
        internal Task<bool> Task => _taskCompletionSource.Task;
    }
}