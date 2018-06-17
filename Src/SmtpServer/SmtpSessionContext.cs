using System;
using System.Collections.Generic;
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
        /// <param name="options">The server options.</param>
        /// <param name="tcpClient">The TCP client that the session is connected with.</param>
        /// <param name="networkClient">The network client to use for communications.</param>
        internal SmtpSessionContext(ISmtpServerOptions options, TcpClient tcpClient, INetworkClient networkClient)
        {
            ServerOptions = options;
            Transaction = new SmtpMessageTransaction();
            RemoteEndPoint = tcpClient.Client.RemoteEndPoint;
            NetworkClient = networkClient;
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
        /// Gets the text stream to read from and write to.
        /// </summary>
        public INetworkClient NetworkClient { get; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public SmtpMessageTransaction Transaction { get; }

        /// <summary>
        /// Gets the remote endpoint of the client.
        /// </summary>
        public EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Returns a value indicating whether or nor the current session is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        public bool IsQuitRequested { get; internal set; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// The source address of the client connected to a proxy as reported by proxy-protocol.
        /// </summary>
        public IPEndPoint ProxySourceEndpoint { get; internal set; }
        
        /// <summary>
        /// The destination endpoint on the proxy as reported by proxy-protocol.
        /// </summary>
        public IPEndPoint ProxyDestinationEndpoint { get; internal set; }
    }
}