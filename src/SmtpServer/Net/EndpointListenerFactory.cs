using System;
using System.Net.Sockets;

namespace SmtpServer.Net
{
    /// <summary>
    /// Endpoint Listener Factory
    /// </summary>
    public class EndpointListenerFactory : IEndpointListenerFactory
    {
        /// <summary>
        /// Default Endpoint Listener Factory
        /// </summary>
        internal static readonly IEndpointListenerFactory Default = new EndpointListenerFactory();

        /// <summary>
        /// Raised when an endpoint has been started.
        /// </summary>
        public event EventHandler<EndpointEventArgs> EndpointStarted;

        /// <summary>
        /// Raised when an endpoint has been stopped.
        /// </summary>
        public event EventHandler<EndpointEventArgs> EndpointStopped;

        /// <inheritdoc />
        public virtual IEndpointListener CreateListener(IEndpointDefinition endpointDefinition)
        {
            var tcpListener = new TcpListener(endpointDefinition.Endpoint);
            tcpListener.Start();

            var endpointEventArgs = new EndpointEventArgs(endpointDefinition, tcpListener.LocalEndpoint);
            OnEndpointStarted(endpointEventArgs);

            return new EndpointListener(tcpListener, () => OnEndpointStopped(endpointEventArgs));
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
