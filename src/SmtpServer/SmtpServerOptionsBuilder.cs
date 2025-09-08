using System;
using System.Collections.Generic;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Server Options Builder
    /// </summary>
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
                MaxMessageSizeOptions = new MaxMessageSizeOptions(),
                Endpoints = new List<IEndpointDefinition>(),
                MaxRetryCount = 5,
                MaxAuthenticationAttempts = 3,
                NetworkBufferSize = 128,
                CommandWaitTimeout = TimeSpan.FromMinutes(5),
                CustomSmtpGreeting = null,
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
        /// <param name="length">The maximum message size to allow in bytes.</param>
        /// <param name="handling">The handling type.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MaxMessageSize(int length, MaxMessageSizeHandling handling = MaxMessageSizeHandling.Ignore)
        {
            _setters.Add(options => options.MaxMessageSizeOptions = new MaxMessageSizeOptions(handling, length));

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
        /// Sets the timeout used when waiting for a command from the client.
        /// </summary>
        /// <param name="value">The timeout used when waiting for a command from the client.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder CommandWaitTimeout(TimeSpan value)
        {
            _setters.Add(options => options.CommandWaitTimeout = value);
            
            return this;
        }

        /// <summary>
        /// Sets the custom SMTP greeting message sent to the client upon connection,
        /// typically returned as the initial "220" response.
        /// </summary>
        /// <param name="smtpGreetingFunc">
        /// A delegate that returns the greeting message to send to the client,
        /// based on the <see cref="ISessionContext"/> (e.g., client IP, TLS state).
        /// Example: <c>sessionContext => $"220 {sessionContext.ServerOptions.ServerName} ESMTP ready"</c>
        /// </param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder CustomSmtpGreeting(Func<ISessionContext, string> smtpGreetingFunc)
        {
            _setters.Add(options => options.CustomSmtpGreeting = smtpGreetingFunc);

            return this;
        }

        #region SmtpServerOptions

        class SmtpServerOptions : ISmtpServerOptions
        {
            /// <summary>
            /// Gets or sets the maximum message size option.
            /// </summary>
            public IMaxMessageSizeOptions MaxMessageSizeOptions { get; set; }

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

            /// <summary>
            /// Gets or sets the custom greeting message sent by the server in response to the initial SMTP connection.
            /// This message is returned after the client connects and before any commands are issued (e.g., "220 mail.example.com v1.0 ESMTP ready").
            /// If not set, a default greeting will be used.
            /// </summary>
            public Func<ISessionContext, string> CustomSmtpGreeting { get; set; }
        }

        #endregion
    }
}
