using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Net
{
    /// <summary>
    /// Endpoint Listener
    /// </summary>
    public sealed class EndpointListener : IEndpointListener
    {
        /// <summary>
        /// EndpointListener LocalEndPoint Key
        /// </summary>
        public const string LocalEndPointKey = "EndpointListener:LocalEndPoint";

        /// <summary>
        /// EndpointListener RemoteEndPoint Key
        /// </summary>
        public const string RemoteEndPointKey = "EndpointListener:RemoteEndPoint";

        readonly TcpListener _tcpListener;
        readonly Action _disposeAction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpListener">The TCP listener for the endpoint.</param>
        /// <param name="disposeAction">The action to execute when the listener has been disposed.</param>
        internal EndpointListener(TcpListener tcpListener, Action disposeAction)
        {
            _tcpListener = tcpListener;
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// Returns a securable pipe to the endpoint.
        /// </summary>
        /// <param name="context">The session context that the pipe is being created for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The securable pipe from the endpoint.</returns>
        public async Task<ISecurableDuplexPipe> GetPipeAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            context.Properties.Add(LocalEndPointKey, _tcpListener.LocalEndpoint);
            context.Properties.Add(RemoteEndPointKey, tcpClient.Client.RemoteEndPoint);

            var stream = tcpClient.GetStream();

            return new SecurableDuplexPipe(stream, () =>
            {
                try
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                }
                catch { }
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _tcpListener.Stop();
            _disposeAction();
        }
    }
}
