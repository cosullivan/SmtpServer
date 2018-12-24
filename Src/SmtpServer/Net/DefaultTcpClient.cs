using System;
using System.Net;
using System.Net.Sockets;

namespace SmtpServer.Net
{
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
            if (_tcpClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// The remote endpoint that the connection is with.
        /// </summary>
        public EndPoint RemoteEndPoint => _tcpClient.Client.RemoteEndPoint;
    }
}