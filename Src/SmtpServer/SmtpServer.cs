using System;
using System.Collections.Concurrent;
using System.Linq;
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
}