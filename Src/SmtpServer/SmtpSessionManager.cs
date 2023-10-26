using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    internal sealed class SmtpSessionManager
    {
        readonly SmtpServer _smtpServer;
        readonly HashSet<SmtpSessionHandle> _sessions = new HashSet<SmtpSessionHandle>();
        readonly object _sessionsLock = new object();
        
        internal SmtpSessionManager(SmtpServer smtpServer)
        {
            _smtpServer = smtpServer;
        }

        internal void Run(SmtpSessionContext sessionContext, CancellationToken cancellationToken)
        {
            var handle = new SmtpSessionHandle(new SmtpSession(sessionContext), sessionContext);
            Add(handle);

            handle.CompletionTask = RunAsync(handle, cancellationToken);

            // ReSharper disable once MethodSupportsCancellation
            handle.CompletionTask.ContinueWith(
                task =>
                {
                    Remove(handle);
                });
        }

        async Task RunAsync(SmtpSessionHandle handle, CancellationToken cancellationToken)
        {
            try
            {
                await UpgradeAsync(handle, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _smtpServer.OnSessionCreated(new SessionEventArgs(handle.SessionContext));

                await handle.Session.RunAsync(cancellationToken);

                _smtpServer.OnSessionCompleted(new SessionEventArgs(handle.SessionContext));
            }
            catch (OperationCanceledException)
            {
                _smtpServer.OnSessionCancelled(new SessionEventArgs(handle.SessionContext));
            }
            catch (Exception ex)
            {
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

                await handle.SessionContext.Pipe.UpgradeAsync(serverCertificate, endpoint.SupportedSslProtocols, cancellationToken).ConfigureAwait(false);
            }
        }

        internal Task WaitAsync()
        {
            IReadOnlyList<Task> tasks;
            
            lock (_sessionsLock)
            {
                tasks = _sessions.Select(session => session.CompletionTask).ToList();
            }
            
            return Task.WhenAll(tasks);
        }

        void Add(SmtpSessionHandle handle)
        {
            lock (_sessionsLock)
            {
                _sessions.Add(handle);
            }
        }

        void Remove(SmtpSessionHandle handle)
        {
            lock (_sessionsLock)
            {
                _sessions.Remove(handle);
            }
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