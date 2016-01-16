using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer
{
    public class SmtpServer
    {
        readonly ISmtpServerOptions _options;
        readonly TraceSwitch _logger = new TraceSwitch("SmtpServer", "The SMTP server.");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        public SmtpServer(ISmtpServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Starts the SMTP server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Starting the SMTP Server");

            await Task.WhenAll(_options.Endpoints.Select(e => ListenAsync(e, cancellationToken))).ConfigureAwait(false);
        }

        /// <summary>
        /// Listen for SMTP traffic on the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to listen on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task ListenAsync(IPEndPoint endpoint, CancellationToken cancellationToken)
        {
            _logger.LogVerbose("Listening on port {0}", endpoint.Port);

            var tcpListener = new TcpListener(endpoint);
            tcpListener.Start();

            // keep track of the running tasks for disposal
            var sessions = new ConcurrentDictionary<Task, Task>();

            try
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // wait for a client connection
                    var tcpClient = await tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);

                    _logger.LogVerbose("SMTP client accepted [{0}]", tcpClient.Client.RemoteEndPoint);

                    // create a new session to handle the connection
                    var session = CreateSession(tcpClient);

                    var sessionTask = session.HandleAsync(cancellationToken)
                        .ContinueWith(t =>
                            {
                                Task task;
                                sessions.TryRemove(t, out task);

                                _logger.LogVerbose("SMTP client closed [{0}]", tcpClient.Client.RemoteEndPoint);

                                // closing the client will dispose of the stream
                                tcpClient.Close();
                            }, 
                            cancellationToken);

                    // keep track of the session 
                    sessions.TryAdd(sessionTask, sessionTask);
                }

                // the server has been cancelled, wait for the tasks to complete
                await Task.WhenAll(sessions.Values).ConfigureAwait(false);
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        /// <summary>
        /// Creates a new SMTP session.
        /// </summary>
        /// <param name="tcpClient">The TCP client that the session is reading & writing to.</param>
        /// <returns>The SMTP session.</returns>
        SmtpSession CreateSession(TcpClient tcpClient)
        {
            return new SmtpSession(_options, tcpClient, new SmtpStateMachine(CreateCommandFactory()));
        }

        /// <summary>
        /// Create an instance of the SMTP Command Factory.
        /// </summary>
        /// <returns>An instance of the SMTP server command factory.</returns>
        SmtpCommandFactory CreateCommandFactory()
        {
            return new SmtpCommandFactory(_options, new SmtpParser());
        }
    }
}
