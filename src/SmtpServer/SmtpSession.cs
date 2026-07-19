using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.StateMachine;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmtpServer
{
    internal sealed class SmtpSession
    {
        const string BufferKey = "SmtpSession:Buffer";
        static readonly Version AssemblyVersion = typeof(SmtpSession).GetTypeInfo().Assembly.GetName().Version;

        readonly SmtpStateMachine _stateMachine;
        readonly SmtpSessionContext _context;
        readonly ISmtpCommandFactory _commandFactory;
        readonly ILogger<SmtpSession> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="logger">The logger to write session diagnostics to.</param>
        internal SmtpSession(SmtpSessionContext context, ILogger<SmtpSession> logger)
        {
            _context = context;
            _logger = logger;
            _stateMachine = new SmtpStateMachine(_context);
            _commandFactory = context.ServiceProvider.GetServiceOrDefault<ISmtpCommandFactory>(new SmtpCommandFactory());
        }

        /// <summary>
        /// Handles the SMTP session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        internal async Task RunAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (await IsConnectionAcceptedAsync(cancellationToken).ConfigureAwait(false) == false)
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

                    if (command == null)
                    {
                        return;
                    }

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
                    LogResponseException(responseException, retries);
                    context.RaiseResponseException(responseException);

                    await context.Pipe.Output.WriteReplyAsync(responseException.Response, cancellationToken).ConfigureAwait(false);

                    context.IsQuitRequested = true;
                }
                catch (SmtpResponseException responseException)
                {
                    LogResponseException(responseException, retries);
                    context.RaiseResponseException(responseException);

                    var response = CreateErrorResponse(responseException.Response, retries);

                    await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        async ValueTask<SmtpCommand> ReadCommandAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            var timeout = new CancellationTokenSource(context.ServerOptions.CommandWaitTimeout);

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

            try
            {
                SmtpCommand command = null;

                await context.Pipe.Input.ReadLineAsync(
                    buffer =>
                    {
                        var parser = new SmtpParser(_commandFactory);

                        if (parser.TryMake(ref buffer, out command, out var errorResponse) == false)
                        {
                            var properties = new Dictionary<string, object>()
                            {
                                { BufferKey, buffer.ToArray() }
                            };

                            throw new SmtpResponseException(errorResponse, false, properties);
                        }

                        return Task.CompletedTask;
                    },
                    context.ServerOptions.MaxCommandLineLength,
                    cancellationTokenSource.Token).ConfigureAwait(false);

                return command;
            }
            catch (OperationCanceledException)
            {
                if (timeout.IsCancellationRequested)
                {
                    throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "Timeout while waiting for input."), true);
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
        async Task<bool> ExecuteAsync(SmtpCommand command, SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var safeCommand = SmtpCommandSnapshot.From(command);

            _logger.LogDebug("SMTP command executing: {CommandName} {CommandArgument}.", safeCommand.Name, safeCommand.Argument);
            context.RaiseCommandExecuting(command);

            var result = await command.ExecuteAsync(context, cancellationToken);

            context.RaiseCommandExecuted(command);
            _logger.LogDebug("SMTP command executed: {CommandName} {CommandArgument}.", safeCommand.Name, safeCommand.Argument);

            return result;
        }

        void LogResponseException(SmtpResponseException responseException, int retries)
        {
            _logger.LogWarning(
                responseException,
                "SMTP response exception {ReplyCode}: {ReplyMessage}. QuitRequested={QuitRequested}, RetriesRemaining={RetriesRemaining}.",
                responseException.Response.ReplyCode,
                responseException.Response.Message,
                responseException.IsQuitRequested,
                retries);
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        ValueTask<FlushResult> OutputGreetingAsync(CancellationToken cancellationToken)
        {
            if (_context.ServerOptions.CustomSmtpGreeting is null)
            {
                var serverVersion = AssemblyVersion;
                _context.Pipe.Output.WriteLine($"220 {_context.ServerOptions.ServerName} v{serverVersion} ESMTP ready");
            }
            else
            {
                _context.Pipe.Output.WriteLine(_context.ServerOptions.CustomSmtpGreeting(_context));
            }
            
            return _context.Pipe.Output.FlushAsync(cancellationToken);
        }

        async Task<bool> IsConnectionAcceptedAsync(CancellationToken cancellationToken)
        {
            var policy = _context.ServerOptions.SessionPolicy;
            if (policy.ConnectionAccepted == null)
            {
                return true;
            }

            var response = await policy.ConnectionAccepted(_context, cancellationToken).ConfigureAwait(false);
            if (IsSuccessResponse(response))
            {
                return true;
            }

            await _context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            _context.IsQuitRequested = true;
            _logger.LogWarning("SMTP session rejected by connection policy with {ReplyCode}: {ReplyMessage}.", response.ReplyCode, response.Message);
            return false;
        }

        internal static bool IsSuccessResponse(SmtpResponse response)
        {
            if (response == null)
            {
                return true;
            }

            var replyCode = (int)response.ReplyCode;
            return replyCode >= 200 && replyCode < 400;
        }
    }
}
