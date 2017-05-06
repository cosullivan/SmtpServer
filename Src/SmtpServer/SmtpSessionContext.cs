using System;
using System.Net;
using System.Net.Sockets;
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
        /// <param name="tcpClient">The TCP client that the session is connected with.</param>
        internal SmtpSessionContext(TcpClient tcpClient)
        {
            Transaction = new SmtpMessageTransaction();
            RemoteEndPoint = tcpClient.Client.RemoteEndPoint;
            Text = new NetworkClient(tcpClient.GetStream());
        }

        /// <summary>
        /// Indicates to the session that it should quit.
        /// </summary>
        public void Quit()
        {
            IsQuitRequested = true;
        }

        /// <summary>
        /// Raise the command executing event.
        /// </summary>
        /// <param name="command">The command that is executing.</param>
        public void RaiseCommandExecuting(SmtpCommand command)
        {
            CommandExecuting?.Invoke(this, new SmtpCommandExecutingEventArgs(command));
        }

        /// <summary>
        /// Raise the session authenticated event.
        /// </summary>
        public void RaiseSessionAuthenticated()
        {
            SessionAuthenticated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the text stream to read from and write to.
        /// </summary>
        public INetworkClient Text { get; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public SmtpMessageTransaction Transaction { get; }

        /// <summary>
        /// Gets the remote endpoint of the client.
        /// </summary>
        public EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Returns a value indicating whether or not the current session is secure.
        /// </summary>
        public bool IsSecure => Text.IsSecure;

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        public bool IsQuitRequested { get; private set; }
    }
}