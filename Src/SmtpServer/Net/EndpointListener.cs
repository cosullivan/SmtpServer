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

        readonly IEndpointDefinition _endpointDefinition;
        readonly TcpListener _tcpListener;
        readonly Action _disposeAction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the listener for.</param>
        /// <param name="tcpListener">The TCP listener for the endpoint.</param>
        /// <param name="disposeAction">The action to execute when the listener has been disposed.</param>
        internal EndpointListener(IEndpointDefinition endpointDefinition, TcpListener tcpListener, Action disposeAction)
        {
            _endpointDefinition = endpointDefinition;
            _tcpListener = tcpListener;
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// During ssl connections allows the client to drop the connection and reconnect while renegotiation for a different TLS version
        /// .NET 7.0 has set this to false to prevent the following vulnerability https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2009-3555
        /// </summary>
        public bool AllowSslClientRenegotiation { get; set; } = false;

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
            stream.ReadTimeout = (int)_endpointDefinition.ReadTimeout.TotalMilliseconds;

            return new SecurableDuplexPipe(stream, () =>
            {
                tcpClient.Close();
                tcpClient.Dispose();
            })
            {
#if NETSTANDARD2_1_OR_GREATER
                AllowRenegotiation = AllowSslClientRenegotiation
#endif
            };
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