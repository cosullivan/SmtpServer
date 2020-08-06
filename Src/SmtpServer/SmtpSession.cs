using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using System.Reflection;
using SmtpServer.IO;
using SmtpServer.Text;
using System.IO.Pipelines;
using SmtpServer.StateMachine;

namespace SmtpServer
{
    internal sealed class SmtpSession
    {
        readonly SmtpStateMachine _stateMachine;
        readonly SmtpSessionContext _context;
        TaskCompletionSource<bool> _taskCompletionSource;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        internal SmtpSession(SmtpSessionContext context)
        {
            _context = context;
            _stateMachine = new SmtpStateMachine(_context);
        }

        /// <summary>
        /// Executes the session.
        /// </summary>
        /// <param name="completedAction">The callback to execute when the session has completed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Run(Action<Exception> completedAction, CancellationToken cancellationToken)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();

            // ReSharper disable once MethodSupportsCancellation
            RunAsync(cancellationToken).ContinueWith(
                task =>
                {
                    if (task.Exception != null)
                    {
                        completedAction(task.Exception);

                        _taskCompletionSource.SetException(task.Exception);
                        
                        return;
                    }

                    completedAction(null);

                    _taskCompletionSource.SetResult(task.IsCompleted);
                });
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

            await ExecuteAsync(_context, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute the command handler against the specified session context.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var retries = _context.ServerOptions.MaxRetryCount;

            while (retries-- > 0 && context.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    var command = await ReadCommandAsync(context, cancellationToken).ConfigureAwait(false);

                    if (_stateMachine.TryAccept(command, out var errorResponse) == false)
                    {
                        throw new SmtpResponseException(errorResponse);
                    }

                    if (await ExecuteAsync(command, context, cancellationToken).ConfigureAwait(false))
                    {
                        _stateMachine.Transition(context);
                    }

                    retries = _context.ServerOptions.MaxRetryCount;
                }
                catch (SmtpResponseException responseException) when (responseException.IsQuitRequested)
                {
                    await context.Pipe.Output.WriteReplyAsync(responseException.Response, cancellationToken).ConfigureAwait(false);
                }
                catch (SmtpResponseException responseException)
                {
                    var response = CreateErrorResponse(responseException.Response, retries);

                    await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        static async ValueTask<SmtpCommand> ReadCommandAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            var timeout = new CancellationTokenSource(context.ServerOptions.CommandWaitTimeout);

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

            try
            {
                SmtpCommand command = null;

                await context.Pipe.Input.ReadLineAsync(
                    buffer =>
                    {
#if DEBUG
                        Console.WriteLine(StringUtil.Create(buffer));
#endif
                        var parser = new SmtpParser(context.ServerOptions.SmtpCommandFactory);

                        if (parser.TryMake(ref buffer, out command, out var errorResponse) == false)
                        {
                            throw new SmtpResponseException(errorResponse);
                        }

                        return Task.CompletedTask;
                    },
                    cancellationTokenSource.Token).ConfigureAwait(false);

                return command;
            }
            catch (OperationCanceledException)
            {
                if (timeout.IsCancellationRequested)
                {
                    throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "Timeout whilst waiting for input."), true);
                }

                throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), true);
            }
            finally
            {
                timeout.Dispose();
                cancellationTokenSource.Dispose();
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
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        static Task<bool> ExecuteAsync(SmtpCommand command, SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.RaiseCommandExecuting(command);

            return command.ExecuteAsync(context, cancellationToken);
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        ValueTask<FlushResult> OutputGreetingAsync(CancellationToken cancellationToken)
        {
            var version = typeof(SmtpSession).GetTypeInfo().Assembly.GetName().Version;

            _context.Pipe.Output.WriteLine($"220 {_context.ServerOptions.ServerName} v{version} ESMTP ready");
            
            return _context.Pipe.Output.FlushAsync(cancellationToken);
        }
        
        /// <summary>
        /// Returns the completion task.
        /// </summary>
        internal Task<bool> CompletionTask => _taskCompletionSource.Task;
    }
}
