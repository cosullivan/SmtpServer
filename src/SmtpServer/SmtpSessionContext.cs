using System;
using System.Collections.Generic;
using SmtpServer.IO;
using SmtpServer.Protocol;

namespace SmtpServer
{
    internal sealed class SmtpSessionContext : ISessionContext
    {
        /// <inheritdoc />
        public Guid SessionId { get; private set; }

        /// <inheritdoc />
        public event EventHandler<SmtpCommandEventArgs> CommandExecuting;

        /// <inheritdoc />
        public event EventHandler<SmtpCommandEventArgs> CommandExecuted;

        /// <inheritdoc />
        public event EventHandler<SmtpResponseExceptionEventArgs> ResponseException;

        /// <inheritdoc />
        public event EventHandler<EventArgs> SessionAuthenticated;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">The service provider instance.</param>
        /// <param name="options">The server options.</param>
        /// <param name="endpointDefinition">The endpoint definition.</param>
        internal SmtpSessionContext(IServiceProvider serviceProvider, ISmtpServerOptions options, IEndpointDefinition endpointDefinition)
        {
            SessionId = Guid.NewGuid();
            ServiceProvider = serviceProvider;
            ServerOptions = options;
            EndpointDefinition = endpointDefinition;
            Transaction = new SmtpMessageTransaction();
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Raise the command executing event.
        /// </summary>
        /// <param name="command">The command that is executing.</param>
        internal void RaiseCommandExecuting(SmtpCommand command)
        {
            CommandExecuting?.Invoke(this, new SmtpCommandEventArgs(this, command));
        }

        /// <summary>
        /// Raise the command executed event.
        /// </summary>
        /// <param name="command">The command that was executed.</param>
        internal void RaiseCommandExecuted(SmtpCommand command)
        {
            CommandExecuted?.Invoke(this, new SmtpCommandEventArgs(this, command));
        }

        /// <summary>
        /// Raise the response exception event.
        /// </summary>
        /// <param name="responseException">The response exception that was raised.</param>
        internal void RaiseResponseException(SmtpResponseException responseException)
        {
            ResponseException?.Invoke(this, new SmtpResponseExceptionEventArgs(this, responseException));
        }

        /// <summary>
        /// Raise the session authenticated event.
        /// </summary>
        internal void RaiseSessionAuthenticated()
        {
            SessionAuthenticated?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public ISmtpServerOptions ServerOptions { get; }

        /// <inheritdoc />
        public IEndpointDefinition EndpointDefinition { get; }

        /// <inheritdoc />
        public ISecurableDuplexPipe Pipe { get; internal set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public SmtpMessageTransaction Transaction { get; }

        /// <inheritdoc />
        public AuthenticationContext Authentication { get; internal set; } = AuthenticationContext.Unauthenticated;

        /// <summary>
        /// Returns the number of athentication attempts.
        /// </summary>
        public int AuthenticationAttempts { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        public bool IsQuitRequested { get; internal set; }

        /// <inheritdoc />
        public IDictionary<string, object> Properties { get; }
    }
}
