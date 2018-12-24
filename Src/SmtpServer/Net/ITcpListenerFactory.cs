namespace SmtpServer.Net
{
    public interface ITcpListenerFactory
    {
        /// <summary>
        /// Create an instance of a TCP listener for an endpoint.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the TCP listener for.</param>
        /// <returns>The TCP listener that is listening to the given endpoint definition.</returns>
        ITcpListener CreateListener(IEndpointDefinition endpointDefinition);
    }
}