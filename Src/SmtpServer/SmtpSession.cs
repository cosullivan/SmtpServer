﻿using System;
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
            _context = new SmtpSessionContext(tcpClient);
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
                var text = await context.Client.ReadLineAsync(cancellationToken).ReturnOnAnyThread();

                if (TryAccept(text, out SmtpCommand command, out SmtpResponse response))
                {
                    try
                    {
                        await ExecuteAsync(command, context, cancellationToken).ReturnOnAnyThread();

                        retries = _options.MaxRetryCount;

                        continue;
                    }
                    catch (SmtpResponseException responseException)
                    {
                        response = responseException.Response;
                    }
                }

                await context.Client.ReplyAsync(CreateErrorResponse(response, retries), cancellationToken);
            }
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
        /// <param name="segments">The list of array segments to read the command from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        bool TryAccept(IReadOnlyList<ArraySegment<byte>> segments, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return _stateMachine.TryAccept(new TokenEnumerator(new ByteArrayTokenReader(segments)), out command, out errorResponse);
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