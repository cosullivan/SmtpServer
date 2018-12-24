using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Net
{
    internal sealed class DefaultTcpListener : ITcpListener
    {
        readonly TcpListener _tcpListener;
        readonly Action _disposableAction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpListener">The underlying TCP listener.</param>
        /// <param name="disposableAction">The callback to execute when the listener has been disposed.</param>
        public DefaultTcpListener(TcpListener tcpListener, Action disposableAction)
        {
            _tcpListener = tcpListener;
            _disposableAction = disposableAction;
        }

        /// <summary>
        /// Waits for an incoming connection from a client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TCP client that was received.</returns>
        public async Task<ITcpClient> AcceptAsync(CancellationToken cancellationToken)
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync().WithCancellation(cancellationToken);

            return new DefaultTcpClient(tcpClient);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _tcpListener.Stop();

            _disposableAction();
        }
    }
}