using System;
using System.Net;

namespace SmtpServer.Net
{
    /// <summary>
    /// Endpoint EventArgs
    /// </summary>
    public sealed class EndpointEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition.</param>
        /// <param name="localEndPoint">The locally bound endpoint.</param>
        public EndpointEventArgs(IEndpointDefinition endpointDefinition, EndPoint localEndPoint)
        {
            EndpointDefinition = endpointDefinition;
            LocalEndPoint = localEndPoint;
        }
 
        /// <summary>
        /// Returns the endpoint definition.
        /// </summary>
        public IEndpointDefinition EndpointDefinition { get; }
 
        /// <summary>
        /// Returns the locally bound endpoint
        /// </summary>
        public EndPoint LocalEndPoint { get; }
    }
}
