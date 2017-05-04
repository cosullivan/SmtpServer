using System;
using System.Net;
using System.Net.Sockets;
using SmtpServer.IO;
using SmtpServer.Protocol;

namespace SmtpServer
{
    internal class SmtpSessionContext : ISmtpSessionContext
    {
        /// <summary>
        /// Fired when a command is about to execute.
        /// </summary>
        public event EventHandler<SmtpCommandExecutingEventArgs> CommandExecuting;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpClient">The TCP client that the session is connected with.</param>
        /// <param name="transaction">The SMTP transaction.</param>
        /// <param name="stateMachine">The current state machine for the session.</param>
        internal SmtpSessionContext(TcpClient tcpClient, ISmtpMessageTransaction transaction, ISmtpStateMachine stateMachine)
        {
            Transaction = transaction;
            StateMachine = stateMachine;
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
        /// Raise the SMTP command as executing.
        /// </summary>
        /// <param name="command">The command that is executing.</param>
        internal void RaiseSmtpCommandExecuting(SmtpCommand command)
        {
            CommandExecuting?.Invoke(this, new SmtpCommandExecutingEventArgs(command));
        }

        /// <summary>
        /// Gets the text stream to read from and write to.
        /// </summary>
        public INetworkClient Text { get; }

        /// <summary>
        /// The transfer encoding that is required for the message.
        /// </summary>
        public ContentTransferEncoding TransferEncoding { get; set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public ISmtpMessageTransaction Transaction { get; }

        /// <summary>
        /// Gets the current state machine.
        /// </summary>
        public ISmtpStateMachine StateMachine { get; }

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