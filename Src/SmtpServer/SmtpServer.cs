using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer
{
    public class SmtpServer
    {
        /// <summary>
        /// Raised when a session has been created.
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCreated;

        /// <summary>
        /// Raised when a session has completed.
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCompleted;

        readonly ISmtpServerOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        public SmtpServer(ISmtpServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Raises the SessionCreated Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnSessionCreated(SessionEventArgs args)
        {
            SessionCreated?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the SessionCompleted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnSessionCompleted(SessionEventArgs args)
        {
            SessionCompleted?.Invoke(this, args);
        }

        /// <summary>
        /// Starts the SMTP server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_options.Endpoints.Select(e => ListenAsync(e, cancellationToken))).ReturnOnAnyThread();
        }

        /// <summary>
        /// Listen for SMTP traffic on the given endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The definition of the endpoint to listen on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task ListenAsync(IEndpointDefinition endpointDefinition, CancellationToken cancellationToken)
        {
            var tcpListener = new TcpListener(endpointDefinition.Endpoint);
            tcpListener.Start();

            // keep track of the running tasks for disposal
            var sessions = new ConcurrentDictionary<SmtpSession, SmtpSession>();

            try
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    // wait for a client connection
                    var tcpClient = await tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);

                    var networkClient = new NetworkClient(tcpClient.GetStream(), _options.NetworkBufferSize, _options.NetworkBufferReadTimeout);

                    if (endpointDefinition.IsSecure && _options.ServerCertificate != null)
                    {
                        await networkClient.UpgradeAsync(_options.ServerCertificate, _options.SupportedSslProtocols, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    // create a new session to handle the connection
                    var session = new SmtpSession(_options, tcpClient, networkClient);
                    sessions.TryAdd(session, session);

                    OnSessionCreated(new SessionEventArgs(session.Context));

                    session.Run(cancellationToken);

                    #pragma warning disable 4014
                    session.Task
                        .ContinueWith(t =>
                        {
                            if (sessions.TryRemove(session, out SmtpSession s))
                            {
                                s.Dispose();
                            }

                            OnSessionCompleted(new SessionEventArgs(session.Context));
                        },
                        cancellationToken);
                    #pragma warning restore 4014
                }

                // the server has been cancelled, wait for the tasks to complete
                await Task.WhenAll(sessions.Keys.Select(s => s.Task)).ConfigureAwait(false);
            }
            finally
            {
                tcpListener.Stop();
            }
        }
    }

    public interface ITcpListenerFactory
    {
        /// <summary>
        /// Create an instance of a TCP listener for an endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the TCP listener for.</param>
        /// <returns>The TCP listener that is listening to the given endpoint definition.</returns>
        ITcpListener CreateListener(IEndpointDefinition endpointDefinition);
    }

    internal sealed class DefaultTcpListenerFactory : ITcpListenerFactory
    {
        /// <summary>
        /// Create an instance of a TCP listener for an endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the TCP listener for.</param>
        /// <returns>The TCP listener that is listening to the given endpoint definition.</returns>
        public ITcpListener CreateListener(IEndpointDefinition endpointDefinition)
        {
            var tcpListener = new TcpListener(endpointDefinition.Endpoint);
            tcpListener.Start();

            return new DefaultTcpListener(tcpListener);
        }
    }

    public interface ITcpListener : IDisposable
    {
        /// <summary>
        /// Waits for an incoming connection from a client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TCP client that was received.</returns>
        Task<ITcpClient> AcceptAsync(CancellationToken cancellationToken);
    }

    internal sealed class DefaultTcpListener : ITcpListener
    {
        readonly TcpListener _tcpListener;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpListener">The underlying TCP listener.</param>
        internal DefaultTcpListener(TcpListener tcpListener)
        {
            _tcpListener = tcpListener;
        }

        /// <summary>
        /// Waits for an incoming connection from a client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TCP client that was received.</returns>
        public async Task<ITcpClient> AcceptAsync(CancellationToken cancellationToken)
        {
            var tcpClient = _tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken);

            return new DefaultTcpClient(tcpClient);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _tcpListener.Stop();
        }
    }

    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Returns the network stream from the TCP client.
        /// </summary>
        /// <returns>The network stream from the TCP client.</returns>
        NetworkStream GetStream();
    }

    internal sealed class DefaultTcpClient : ITcpClient
    {
        readonly TcpClient _tcpClient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpClient">The underlying TCP client.</param>
        internal DefaultTcpClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        /// <summary>
        /// Returns the network stream from the TCP client.
        /// </summary>
        /// <returns>The network stream from the TCP client.</returns>
        public NetworkStream GetStream()
        {
            return _tcpClient.GetStream();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_tcpClient is IDisposable as disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// The remote endpoint that the connection is with.
        /// </summary>
        public EndPoint RemoteEndPoint { get; } = _tcpClient.Client.RemoteEndPoint;
    }
}