using System;
using System.Net;

namespace SmtpServer
{
    public class EndPointEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endPointDefinition">The endpoint definition.</param>
        /// <param name="localEndPoint">The locally bound endpoint.</param>
        public EndPointEventArgs(IEndpointDefinition endPointDefinition, EndPoint localEndPoint)
        {
            EndPointDefinition = endPointDefinition;
            LocalEndPoint = localEndPoint;
        }

        /// <summary>
        /// Returns the endpoint definition.
        /// </summary>
        public IEndpointDefinition EndPointDefinition { get; }

        /// <summary>
        /// Returns the locally bound endpoint
        /// </summary>
        public EndPoint LocalEndPoint { get; }
    }
}