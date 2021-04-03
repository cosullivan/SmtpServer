using System;
using System.Collections.Generic;

namespace SmtpServer
{
    public sealed class SmtpServerOptionsBuilder
    {
        readonly List<Action<SmtpServerOptions>> _setters = new List<Action<SmtpServerOptions>>();

        /// <summary>
        /// Builds the options that have been set and returns the built instance.
        /// </summary>
        /// <returns>The server options that have been set.</returns>
        public ISmtpServerOptions Build()
        {
            var serverOptions = new SmtpServerOptions
            {
                Endpoints = new List<IEndpointDefinition>(),
                MaxRetryCount = 5,
                MaxAuthenticationAttempts = 3,
                NetworkBufferSize = 128,
                CommandWaitTimeout = TimeSpan.FromMinutes(5)
            };

            _setters.ForEach(setter => setter(serverOptions));

            return serverOptions;
        }

        /// <summary>
        /// Sets the server name.
        /// </summary>
        /// <param name="value">The name of the server.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder ServerName(string value)
        {
            _setters.Add(options => options.ServerName = value);

            return this;
        }

        /// <summary>
        /// Adds a definition for an endpoint to listen on.
        /// </summary>
        /// <param name="value">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Endpoint(IEndpointDefinition value)
        {
            _setters.Add(options => options.Endpoints.Add(value));

            return this;
        }

        /// <summary>
        /// Adds a definition for an endpoint to listen on.
        /// </summary>
        /// <param name="configure">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Endpoint(Action<EndpointDefinitionBuilder> configure)
        {
            var endpointDefinitionBuilder = new EndpointDefinitionBuilder();
            configure(endpointDefinitionBuilder);

            return Endpoint(endpointDefinitionBuilder.Build());
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="ports">The port to add as the endpoint.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Port(params int[] ports)
        {
            foreach (var port in ports)
            {
                Endpoint(new EndpointDefinitionBuilder().Port(port).Build());
            }

            return this;
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="port">The port to add as the endpoint.</param>
        /// <param name="isSecure">Indicates whether the port is secure by default.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Port(int port, bool isSecure)
        {
            Endpoint(new EndpointDefinitionBuilder().Port(port).IsSecure(isSecure).Build());

            return this;
        }

        /// <summary>
        /// Sets the maximum message size.
        /// </summary>
        /// <param name="value">The maximum message size to allow.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MaxMessageSize(int value)
        {
            _setters.Add(options => options.MaxMessageSize = value);

            return this;
        }

        /// <summary>
        /// Sets the maximum number of retries for a failed command.
        /// </summary>
        /// <param name="value">The maximum number of retries allowed for a failed command.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MaxRetryCount(int value)
        {
            _setters.Add(options => options.MaxRetryCount = value);

            return this;
        }

        /// <summary>
        /// Sets the maximum number of authentication attempts.
        /// </summary>
        /// <param name="value">The maximum number of authentication attempts for a failed authentication.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MaxAuthenticationAttempts(int value)
        {
            _setters.Add(options => options.MaxAuthenticationAttempts = value);

            return this;
        }

        /// <summary>
        /// Sets the size of the buffer for each read operation.
        /// </summary>
        /// <param name="value">The buffer size for each read operation.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder NetworkBufferSize(int value)
        {
            _setters.Add(options => options.NetworkBufferSize = value);

            return this;
        }

        /// <summary>
        /// Sets the timeout to use whilst waiting for a command from the client.
        /// </summary>
        /// <param name="value">The timeout to use whilst waiting for a command from the client.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder CommandWaitTimeout(TimeSpan value)
        {
            _setters.Add(options => options.CommandWaitTimeout = value);
            
            return this;
        }

        #region SmtpServerOptions

        class SmtpServerOptions : ISmtpServerOptions
        {
            /// <summary>
            /// Gets or sets the maximum size of a message.
            /// </summary>
            public int MaxMessageSize { get; set; }

            /// <summary>
            /// The maximum number of retries before quitting the session.
            /// </summary>
            public int MaxRetryCount { get; set; }

            /// <summary>
            /// The maximum number of authentication attempts.
            /// </summary>
            public int MaxAuthenticationAttempts { get; set; }

            /// <summary>
            /// Gets or sets the SMTP server name.
            /// </summary>
            public string ServerName { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            internal List<IEndpointDefinition> Endpoints { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            IReadOnlyList<IEndpointDefinition> ISmtpServerOptions.Endpoints => Endpoints;

            /// <summary>
            /// The timeout to use when waiting for a command from the client.
            /// </summary>
            public TimeSpan CommandWaitTimeout { get; set; }

            /// <summary>
            /// The size of the buffer that is read from each call to the underlying network client.
            /// </summary>
            public int NetworkBufferSize { get; set; }
        }

        #endregion
    }
}