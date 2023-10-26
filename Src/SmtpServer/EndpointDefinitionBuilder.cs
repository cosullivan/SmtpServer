using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

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
            var definition = new EndpointDefinition
            {
                ReadTimeout = TimeSpan.FromMinutes(2),
                SupportedSslProtocols = SslProtocols.Tls12,
            };

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
        /// <param name="port">The port to add as the endpoint.</param>
        /// <param name="isSecure">Indicates whether the port is secure by default.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder Port(int port, bool isSecure)
        {
            return Port(port).IsSecure(isSecure);
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

        /// <summary>
        /// Sets a value indicating whether the client must authenticate in order to proceed.
        /// </summary>
        /// <param name="value">true if the client must issue an AUTH command before sending any mail, false if not.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder AuthenticationRequired(bool value = true)
        {
            _setters.Add(options => options.AuthenticationRequired = value);

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        /// <param name="value">true if the AUTH command is available on an unsecure session, false if not.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder AllowUnsecureAuthentication(bool value = true)
        {
            _setters.Add(options => options.AllowUnsecureAuthentication = value);

            return this;
        }

        /// <summary>
        /// Sets the read timeout to apply to stream operations.
        /// </summary>
        /// <param name="value">The timeout value to apply to read operations.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder ReadTimeout(TimeSpan value)
        {
            _setters.Add(options => options.ReadTimeout = value);

            return this;
        }

        /// <summary>
        /// Sets the X509 certificate to use when starting a TLS session.
        /// </summary>
        /// <param name="value">The server's certificate to use when starting a TLS session.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder Certificate(X509Certificate value)
        {
            return Certificate(new StaticCertificateFactory(value));
        }

        /// <summary>
        /// Sets the X509 certificate factory to use when starting a TLS session.
        /// </summary>
        /// <param name="certificateFactory">The certificate factory to use when creating TLS sessions.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder Certificate(ICertificateFactory certificateFactory)
        {
            _setters.Add(options => options.CertificateFactory = certificateFactory);

            return this;
        }

        /// <summary>
        /// Sets the supported SSL protocols.
        /// </summary>
        /// <param name="value">The supported SSL protocols.</param>
        /// <returns>A EndpointDefinitionBuilder to continue building on.</returns>
        public EndpointDefinitionBuilder SupportedSslProtocols(SslProtocols value)
        {
            _setters.Add(options => options.SupportedSslProtocols = value);

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

            /// <summary>
            /// Gets a value indicating whether the client must authenticate in order to proceed.
            /// </summary>
            public bool AuthenticationRequired { get; set; }

            /// <summary>
            /// Gets a value indicating whether authentication should be allowed on an unsecure session.
            /// </summary>
            public bool AllowUnsecureAuthentication { get; set; }

            /// <summary>
            /// The timeout on each individual buffer read.
            /// </summary>
            public TimeSpan ReadTimeout { get; set; }

            /// <summary>
            /// Gets the Server Certificate factory to use when starting a TLS session.
            /// </summary>
            public ICertificateFactory CertificateFactory { get; set; }

            /// <summary>
            /// The supported SSL protocols.
            /// </summary>
            public SslProtocols SupportedSslProtocols { get; set; }
        }

        #endregion

        #region StaticCertificateFactory 

        internal sealed class StaticCertificateFactory : ICertificateFactory
        {
            readonly X509Certificate _serverCertificate;

            public StaticCertificateFactory(X509Certificate serverCertificate)
            {
                _serverCertificate = serverCertificate;
            }            

            public X509Certificate GetServerCertificate(ISessionContext sessionContext)
            {
                return _serverCertificate;
            }
        }


        #endregion
    }
}