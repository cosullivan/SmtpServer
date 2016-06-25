using System.Net;

namespace SmtpServer
{
    public interface ISessionContext
    {
        /// <summary>
        /// Gets the remote endpoint of the client.
        /// </summary>
        EndPoint RemoteEndPoint { get; }
    }
}