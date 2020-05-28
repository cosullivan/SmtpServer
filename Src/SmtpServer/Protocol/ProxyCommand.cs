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
        public const string ProxySourceEndpointKey = "Proxy:ProxySourceEndpoint";
        public const string ProxyDestinationEndpointKey = "Proxy:ProxyDestinationEndpoint";

        public const string Command = "PROXY";

        public ProxyCommand(ISmtpServerOptions options, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint) : base(options)
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

        public IPEndPoint SourceEndpoint { get; }

        public IPEndPoint DestinationEndpoint { get; }
    }
}