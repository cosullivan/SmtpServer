using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Net
{
    public sealed class EndpointListener : IEndpointListener
    {
        public const string LocalEndPointKey = "EndpointListener:LocalEndPoint";
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
        /// Returns a stream from the endpoint.
        /// </summary>
        /// <param name="context">The session context that the stream is being received for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream from the endpoint.</returns>
        public async Task<INetworkStream> GetStreamAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            context.Properties.Add(LocalEndPointKey, _tcpListener.LocalEndpoint);
            context.Properties.Add(RemoteEndPointKey, tcpClient.Client.RemoteEndPoint);

            return new EndpointStream(tcpClient);
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