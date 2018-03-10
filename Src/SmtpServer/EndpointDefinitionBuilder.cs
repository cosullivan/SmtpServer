using System;
using System.Collections.Generic;
using System.Net;

namespace SmtpServer
{
    public sealed class EndpointDefinitionBuilder
    {
        readonly List<Action<EndpointDefinition>> _setters = new List<Action<EndpointDefinition>>();

        /// <summary>
        /// Build the endpoint definition.
        /// </summary>
        /// <returns>The endpoint definition that was built.</returns>
        public IEndpointDefinition Build()
        {
            var definition = new EndpointDefinition();

            _setters.ForEach(setter => setter(definition));

            return definition;
        }

        /// <summary>
        /// Sets the endpoint to listen on.
        /// </summary>
        /// <param name="endpoint">The endpoint to listen on.</param>
        /// <returns>The endpoint builder to continue building on.</returns>
        public EndpointDefinitionBuilder Endpoint(IPEndPoint endpoint)
        {
            _setters.Add(definition => definition.Endpoint = endpoint);

            return this;
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="port">The port for the endpoint to listen on.</param>
        /// <returns>The endpoint builder to continue building on.</returns>
        public EndpointDefinitionBuilder Port(int port)
        {
            _setters.Add(definition => definition.Endpoint = new IPEndPoint(IPAddress.Any, port));

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the endpoint is secure by default.
        /// </summary>
        /// <param name="value">true to enable SSL by default, false if not.</param>
        /// <returns>The endpoint builder to continue building on.</returns>
        /// <remarks>If this is set to true, then SSL will be enabled during the connection phase.</remarks>
        public EndpointDefinitionBuilder IsSecure(bool value)
        {
            _setters.Add(definition => definition.IsSecure = value);

            return this;
        }

        #region EndpointDefinition

        internal sealed class EndpointDefinition : IEndpointDefinition
        {
            /// <summary>
            /// The IP endpoint to listen on.
            /// </summary>
            public IPEndPoint Endpoint { get; set; }

            /// <summary>
            /// Indicates whether the endpoint is secure by default.
            /// </summary>
            public bool IsSecure { get; set; }
        }

        #endregion
    }
}