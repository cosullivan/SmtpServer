using System;
using System.Net.Sockets;

namespace SmtpServer.Net
{
    public sealed class DefaultTcpListenerFactory : ITcpListenerFactory
    {
        /// <summary>
        /// Raised when an endpoint has been started.
        /// </summary
        public event EventHandler<EndPointEventArgs> EndPointStarted;
 
        /// <summary>
        /// Raised when an endpoint has been stopped.
        /// </summary
        public event EventHandler<EndPointEventArgs> EndPointStopped;

        /// <summary>
        /// Create an instance of a TCP listener for an endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the TCP listener for.</param>
        /// <returns>The TCP listener that is listening to the given endpoint definition.</returns>
        public ITcpListener CreateListener(IEndpointDefinition endpointDefinition)
        {
            var tcpListener = new TcpListener(endpointDefinition.Endpoint);
            tcpListener.Start();

            var endPointEventArgs = new EndPointEventArgs(endpointDefinition, tcpListener.LocalEndpoint);
            OnEndPointStarted(endPointEventArgs);

            return new DefaultTcpListener(tcpListener, () => OnEndPointStopped(endPointEventArgs));
        }

        /// <summary>
        /// Raises the EndPointStarted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        void OnEndPointStarted(EndPointEventArgs args)
        {
            EndPointStarted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the EndPointStopped Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        void OnEndPointStopped(EndPointEventArgs args)
        {
            EndPointStopped?.Invoke(this, args);
        }
    }
}