using System;
using System.Net;
using System.Net.Sockets;

namespace SmtpServer.Net
{
    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Returns the network stream from the TCP client.
        /// </summary>
        /// <returns>The network stream from the TCP client.</returns>
        NetworkStream GetStream();

        /// <summary>
        /// The remote endpoint that the connection is with.
        /// </summary>
        EndPoint RemoteEndPoint { get; }
    }
}