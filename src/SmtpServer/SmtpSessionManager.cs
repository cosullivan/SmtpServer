using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpServer.Logging;

namespace SmtpServer
{
    internal sealed class SmtpSessionManager
    {
        readonly SmtpServer _smtpServer;
        readonly ILoggerFactory _loggerFactory;
        readonly ILogger<SmtpSessionManager> _logger;
        readonly ConcurrentDictionary<Guid, SmtpSessionHandle> _sessions = new ConcurrentDictionary<Guid, SmtpSessionHandle>();
        
        internal SmtpSessionManager(SmtpServer smtpServer, ILoggerFactory loggerFactory)
        {
            _smtpServer = smtpServer;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SmtpSessionManager>();
        }

        internal void Run(SmtpSessionContext sessionContext, CancellationToken cancellationToken)
        {
            var handle = new SmtpSessionHandle(new SmtpSession(sessionContext, _loggerFactory.CreateLogger<SmtpSession>()), sessionContext);
            Add(handle);

            handle.CompletionTask = RunAsync(handle, cancellationToken).ContinueWith(task =>
            {
                Remove(handle);
            });
        }

        async Task RunAsync(SmtpSessionHandle handle, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(SmtpLoggerFactory.CreateSessionScope(handle.SessionContext));
            using var sessionTimeoutCancellationTokenSource = new CancellationTokenSource(handle.SessionContext.EndpointDefinition.SessionTimeout);

            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, sessionTimeoutCancellationTokenSource.Token);

            try
            {
                _logger.LogInformation("SMTP session created from {RemoteEndPoint}.", TryGetProperty(handle.SessionContext, Net.EndpointListener.RemoteEndPointKey));
                _smtpServer.OnSessionCreated(new SessionEventArgs(handle.SessionContext));

                await UpgradeAsync(handle, linkedTokenSource.Token);

                linkedTokenSource.Token.ThrowIfCancellationRequested();

                await handle.Session.RunAsync(linkedTokenSource.Token);

                _logger.LogInformation("SMTP session completed.");
                _smtpServer.OnSessionCompleted(new SessionEventArgs(handle.SessionContext));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SMTP session cancelled.");
                _smtpServer.OnSessionCancelled(new SessionEventArgs(handle.SessionContext));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP session faulted.");
                _smtpServer.OnSessionFaulted(new SessionFaultedEventArgs(handle.SessionContext, ex));
            }
            finally
            {
                await handle.SessionContext.Pipe.Input.CompleteAsync();
                
                handle.SessionContext.Pipe.Dispose();
            }
        }

        async Task UpgradeAsync(SmtpSessionHandle handle, CancellationToken cancellationToken)
        {
            var endpoint = handle.SessionContext.EndpointDefinition;

            if (endpoint.IsSecure && endpoint.CertificateFactory != null)
            {
                var serverCertificate = endpoint.CertificateFactory.GetServerCertificate(handle.SessionContext);

                _logger.LogInformation("SMTP session upgrading to TLS using {SslProtocols}.", endpoint.SupportedSslProtocols);
                await handle.SessionContext.Pipe.UpgradeAsync(serverCertificate, endpoint.SupportedSslProtocols, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("SMTP session upgraded to TLS using {SslProtocol}.", handle.SessionContext.Pipe.SslProtocol);
            }
        }

        internal Task WaitAsync()
        {
            var tasks = _sessions.Values.Select(session => session.CompletionTask).ToList().AsReadOnly();
            return Task.WhenAll(tasks);
        }

        void Add(SmtpSessionHandle handle)
        {
            _sessions.TryAdd(handle.SessionContext.SessionId, handle);
        }

        void Remove(SmtpSessionHandle handle)
        {
            _sessions.TryRemove(handle.SessionContext.SessionId, out _);
        }

        static object TryGetProperty(SmtpSessionContext context, string key)
        {
            return context.Properties.TryGetValue(key, out var value) ? value : null;
        }

        class SmtpSessionHandle
        {
            public SmtpSessionHandle(SmtpSession session, SmtpSessionContext sessionContext)
            {
                Session = session;
                SessionContext = sessionContext;
            }

            public SmtpSession Session { get; }
            
            public SmtpSessionContext SessionContext { get; }

            public Task CompletionTask { get; set; }
        }
    }
}
