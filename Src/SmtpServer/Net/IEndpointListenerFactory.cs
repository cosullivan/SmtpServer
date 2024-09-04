namespace SmtpServer.Net
{
    /// <summary>
    /// Endpoint Listener Factory Interface
    /// </summary>
    public interface IEndpointListenerFactory
    {
        /// <summary>
        /// Create an instance of an endpoint listener for the specified endpoint definition.
        /// </summary>
        /// <param name="endpointDefinition">The endpoint definition to create the listener for.</param>
        /// <returns>The endpoint listener for the specified endpoint definition.</returns>
        IEndpointListener CreateListener(IEndpointDefinition endpointDefinition);
    }
}
