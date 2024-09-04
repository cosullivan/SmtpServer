using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.Net;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Server
    /// </summary>
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

        /// <summary>
        /// Raised when a session has faulted.
        /// </summary>
        public event EventHandler<SessionFaultedEventArgs> SessionFaulted;

        /// <summary>
        /// Raised when a session has been cancelled through the cancellation token.
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCancelled;

        readonly ISmtpServerOptions _options;
        readonly IServiceProvider _serviceProvider;
        readonly IEndpointListenerFactory _endpointListenerFactory;
        readonly SmtpSessionManager _sessions;
        readonly CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();
        readonly TaskCompletionSource<bool> _shutdownTask = new TaskCompletionSource<bool>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="serviceProvider">The service provider to use when resolving services.</param>
        public SmtpServer(ISmtpServerOptions options, IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _sessions = new SmtpSessionManager(this);
            _endpointListenerFactory = serviceProvider.GetServiceOrDefault(EndpointListenerFactory.Default);
        }

        /// <summary>
        /// Raises the SessionCreated Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected internal virtual void OnSessionCreated(SessionEventArgs args)
        {
            SessionCreated?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the SessionCompleted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected internal virtual void OnSessionCompleted(SessionEventArgs args)
        {
            SessionCompleted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the SessionCompleted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected internal virtual void OnSessionFaulted(SessionFaultedEventArgs args)
        {
            SessionFaulted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the SessionCancelled Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected internal virtual void OnSessionCancelled(SessionEventArgs args)
        {
            SessionCancelled?.Invoke(this, args);
        }

        /// <summary>
        /// Starts the SMTP server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = _options.Endpoints.Select(e => ListenAsync(e, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _shutdownTask.TrySetResult(true);

            await _sessions.WaitAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Shutdown the server and allow any active sessions to finish.
        /// </summary>
        public void Shutdown()
        {
            _shutdownTokenSource.Cancel();
        }

        /// <summary>
        /// Listen for SMTP traffic on the given endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The definition of the endpoint to listen on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task ListenAsync(IEndpointDefinition endpointDefinition, CancellationToken cancellationToken)
        {
            // The listener can be stopped either by the caller cancelling the CancellationToken used when starting the server, or when calling
            // the shutdown method. The Shutdown method will stop the listeners and allow any active sessions to finish gracefully.
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownTokenSource.Token, cancellationToken);

            using var endpointListener = _endpointListenerFactory.CreateListener(endpointDefinition);

            while (cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                var sessionContext = new SmtpSessionContext(_serviceProvider, _options, endpointDefinition);
                
                try
                {
                    // wait for a client connection
                    sessionContext.Pipe = await endpointListener.GetPipeAsync(sessionContext, cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    OnSessionFaulted(new SessionFaultedEventArgs(sessionContext, ex));
                    continue;
                }

                if (sessionContext.Pipe != null)
                {
                    _sessions.Run(sessionContext, cancellationTokenSource.Token);
                }
            }
        }

        /// <summary>
        /// The task that completes when the server has shutdown and stopped accepting new sessions.
        /// </summary>
        public Task ShutdownTask => _shutdownTask.Task;
    }
}
