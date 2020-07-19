using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Net;

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

        /// <summary>
        /// Raised when a session has faulted.
        /// </summary>
        public event EventHandler<SessionFaultedEventArgs> SessionFaulted;

        /// <summary>
        /// Raised when a session has been cancelled through the cancellation token.
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCancelled;

        readonly ISmtpServerOptions _options;
        readonly SessionManager _sessions;
        readonly CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();
        readonly TaskCompletionSource<bool> _shutdownTask = new TaskCompletionSource<bool>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        public SmtpServer(ISmtpServerOptions options)
        {
            _options = options;
            _sessions = new SessionManager(this);
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
        /// Raises the SessionCompleted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnSessionFaulted(SessionFaultedEventArgs args)
        {
            SessionFaulted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the SessionCancelled Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnSessionCancelled(SessionEventArgs args)
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
            using (var endpointListener = _options.EndpointListenerFactory.CreateListener(endpointDefinition))
            {
                while (_shutdownTokenSource.Token.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    var sessionContext = new SmtpSessionContext(_options, endpointDefinition);

                    try
                    {
                        await ListenAsync(sessionContext, endpointListener, cancellationToken);
                    }
                    catch (OperationCanceledException) when (_shutdownTokenSource.Token.IsCancellationRequested == false)
                    {
                        if (sessionContext.Pipe != null)
                        { 
                            OnSessionCancelled(new SessionEventArgs(sessionContext));
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        OnSessionFaulted(new SessionFaultedEventArgs(sessionContext, ex));
                    }
                }
            }
        }

        async Task ListenAsync(SmtpSessionContext sessionContext, IEndpointListener endpointListener, CancellationToken cancellationToken)
        {
            // The listener can be stopped either by the caller cancelling the CancellationToken used when starting the server, or when calling
            // the shutdown method. The Shutdown method will stop the listeners and allow any active sessions to finish gracefully.
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownTokenSource.Token, cancellationToken);

            // wait for a client connection
            sessionContext.Pipe = await endpointListener.GetPipeAsync(sessionContext, cancellationTokenSource.Token).ConfigureAwait(false);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (sessionContext.EndpointDefinition.IsSecure && _options.ServerCertificate != null)
            {
                //await sessionContext.NetworkClient.Stream.UpgradeAsync(_options.ServerCertificate, _options.SupportedSslProtocols, cancellationToken).ConfigureAwait(false);
                //cancellationToken.ThrowIfCancellationRequested();

                throw new NotImplementedException("TODO");
            }

            _sessions.Run(sessionContext, cancellationToken);
        }

        /// <summary>
        /// The task that completes when the server has shutdown and stopped accepting new sessions.
        /// </summary>
        public Task ShutdownTask => _shutdownTask.Task;
        
        #region SessionManager

        class SessionManager
        {
            readonly SmtpServer _smtpServer;
            readonly HashSet<SmtpSession> _sessions = new HashSet<SmtpSession>();
            readonly object _sessionsLock = new object();
            
            public SessionManager(SmtpServer smtpServer)
            {
                _smtpServer = smtpServer;
            }

            public void Run(SmtpSessionContext sessionContext, CancellationToken cancellationToken)
            {
                var session = new SmtpSession(sessionContext);
                Add(session);

                _smtpServer.OnSessionCreated(new SessionEventArgs(sessionContext));

                session.Run(
                    exception =>
                    {
                        Remove(session);

                        sessionContext.Pipe.Dispose();

                        if (exception != null)
                        {
                            _smtpServer.OnSessionFaulted(new SessionFaultedEventArgs(sessionContext, exception));
                        }
                        
                        _smtpServer.OnSessionCompleted(new SessionEventArgs(sessionContext));
                    }, 
                    cancellationToken);
            }

            public Task WaitAsync()
            {
                IReadOnlyList<Task> tasks;
                
                lock (_sessionsLock)
                {
                    tasks = _sessions.Select(session => session.Task).ToList();
                }
                
                return Task.WhenAll(tasks);
            }

            void Add(SmtpSession session)
            {
                lock (_sessionsLock)
                {
                    _sessions.Add(session);
                }
            }

            void Remove(SmtpSession session)
            {
                lock (_sessionsLock)
                {
                    _sessions.Remove(session);
                }
            }
        }

        #endregion
    }
}