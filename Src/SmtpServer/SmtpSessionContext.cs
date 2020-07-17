using System;
using System.Collections.Generic;
using SmtpServer.IO;
using SmtpServer.Protocol;

namespace SmtpServer
{
    internal sealed class SmtpSessionContext : ISessionContext
    {
        /// <summary>
        /// Fired when a command is about to execute.
        /// </summary>
        public event EventHandler<SmtpCommandExecutingEventArgs> CommandExecuting;

        /// <summary>
        /// Fired when the session has been authenticated.
        /// </summary>
        public event EventHandler<EventArgs> SessionAuthenticated;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="endpointDefinition">The endpoint definition.</param>
        internal SmtpSessionContext(ISmtpServerOptions options, IEndpointDefinition endpointDefinition)
        {
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
            CommandExecuting?.Invoke(this, new SmtpCommandExecutingEventArgs(this, command));
        }

        /// <summary>
        /// Raise the session authenticated event.
        /// </summary>
        internal void RaiseSessionAuthenticated()
        {
            SessionAuthenticated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the options that the server was created with.
        /// </summary>
        public ISmtpServerOptions ServerOptions { get; }

        /// <summary>
        /// Gets the endpoint definition.
        /// </summary>
        public IEndpointDefinition EndpointDefinition { get; }

        /// <summary>
        /// Gets the pipeline to read from and write to.
        /// </summary>
        public INetworkPipe NetworkPipe { get; internal set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public SmtpMessageTransaction Transaction { get; }
        
        /// <summary>
        /// Returns the authentication context.
        /// </summary>
        public AuthenticationContext Authentication { get; internal set; } = AuthenticationContext.Unauthenticated;

        /// <summary>
        /// Returns the number of athentication attempts.
        /// </summary>
        public int AuthenticationAttempts { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        public bool IsQuitRequested { get; internal set; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        public IDictionary<string, object> Properties { get; }
    }
}