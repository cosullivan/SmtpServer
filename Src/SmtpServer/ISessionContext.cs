using System;
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
        /// Gets the remote endpoint of the client.
        /// </summary>
        EndPoint RemoteEndPoint { get; }
    }
}