using System;
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
    public sealed class ProxyProtocolCommand : SmtpCommand
    {
        public const string Command = "PROXY";

        public IPEndPoint SourceEndpoint { get; }
        public IPEndPoint DestinationEndpoint { get; }

        public ProxyProtocolCommand(ISmtpServerOptions options, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint) : base(options)
        {
            SourceEndpoint = sourceEndpoint;
            DestinationEndpoint = destinationEndpoint;
        }

        internal override Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.ProxySourceEndpoint = SourceEndpoint;
            context.ProxyDestinationEndpoint = DestinationEndpoint;

            // Do not transition smtp protocol state for these commands.
            return Task.FromResult(false);
        }
    }
}