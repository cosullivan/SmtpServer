using System;
using System.Collections.Generic;
using System.Net;

namespace SmtpServer
{
    public interface ISessionContext
    {
        /// <summary>
        /// Fired when a command is about to execute.
        /// </summary>
        event EventHandler<SmtpCommandExecutingEventArgs> CommandExecuting;

        /// <summary>
        /// Fired when the session has been authenticated.
        /// </summary>
        event EventHandler<EventArgs> SessionAuthenticated;

        /// <summary>
        /// Gets the remote endpoint of the client.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Returns a value indicating whether or not the current session is secure.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Returns a value indicating whether or nor the current session is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}