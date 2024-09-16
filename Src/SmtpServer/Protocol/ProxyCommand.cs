using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Support for proxy protocol version 1 header for use with HAProxy.
    /// Documented at http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
    /// This should always (and only ever) be the first command seen on a new connection from HAProxy
    /// </summary>
    public sealed class ProxyCommand : SmtpCommand
    {
        /// <summary>
        /// Proxy Source Endpoint Key
        /// </summary>
        public const string ProxySourceEndpointKey = "Proxy:ProxySourceEndpoint";

        /// <summary>
        /// Proxy Destination Endpoint Key
        /// </summary>
        public const string ProxyDestinationEndpointKey = "Proxy:ProxyDestinationEndpoint";

        /// <summary>
        /// Smtp Proxy Command
        /// </summary>
        public const string Command = "PROXY";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceEndpoint">The source endpoint.</param>
        /// <param name="destinationEndpoint">The destination endpoint.</param>
        public ProxyCommand(IPEndPoint sourceEndpoint = null, IPEndPoint destinationEndpoint = null) : base(Command)
        {
            SourceEndpoint = sourceEndpoint;
            DestinationEndpoint = destinationEndpoint;
        }

        /// <inheritdoc />
        internal override Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Properties.Add(ProxySourceEndpointKey, SourceEndpoint);
            context.Properties.Add(ProxyDestinationEndpointKey, DestinationEndpoint);
            
            // Do not transition smtp protocol state for these commands.
            return Task.FromResult(false);
        }

        /// <summary>
        /// The source endpoint.
        /// </summary>
        public IPEndPoint SourceEndpoint { get; }

        /// <summary>
        /// The destination endpoint.
        /// </summary>
        public IPEndPoint DestinationEndpoint { get; }
    }
}
