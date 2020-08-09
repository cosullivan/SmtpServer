using System;
using System.Net.Sockets;

namespace SmtpServer.Net
{
    public class EndpointListenerFactory : IEndpointListenerFactory
    {
        internal static readonly IEndpointListenerFactory Default = new EndpointListenerFactory();

        /// <summary>
        /// Raised when an endpoint has been started.
        /// </summary>
        public event EventHandler<EndpointEventArgs> EndpointStarted;

        /// <summary>
        /// Raised when an endpoint has been stopped.
        /// </summary>
        public event EventHandler<EndpointEventArgs> EndpointStopped;

        /// <summary>
        /// Create an instance of an endpoint listener for the specified endpoint definition.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the listener for.</param>
        /// <returns>The endpoint listener for the specified endpoint definition.</returns>
        public virtual IEndpointListener CreateListener(IEndpointDefinition endpointDefinition)
        {
            var tcpListener = new TcpListener(endpointDefinition.Endpoint);
            tcpListener.Start();

            var endpointEventArgs = new EndpointEventArgs(endpointDefinition, tcpListener.LocalEndpoint);
            OnEndpointStarted(endpointEventArgs);

            return new EndpointListener(endpointDefinition, tcpListener, () => OnEndpointStopped(endpointEventArgs));
        }

        /// <summary>
        /// Raises the EndPointStarted Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnEndpointStarted(EndpointEventArgs args)
        {
            EndpointStarted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the EndPointStopped Event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected virtual void OnEndpointStopped(EndpointEventArgs args)
        {
            EndpointStopped?.Invoke(this, args);
        }
    }
}