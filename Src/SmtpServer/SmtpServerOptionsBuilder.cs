using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Net;
using SmtpServer.Storage;

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
                TcpListenerFactory = new DefaultTcpListenerFactory(),
                MessageStoreFactory = DoNothingMessageStore.Instance,
                MailboxFilterFactory = DoNothingMailboxFilter.Instance,
                UserAuthenticatorFactory = DoNothingUserAuthenticator.Instance,
                MaxRetryCount = 5,
                SupportedSslProtocols = SslProtocols.Tls,
                NetworkBufferSize = 128,
                NetworkBufferReadTimeout = TimeSpan.FromMinutes(2),
                CommandWaitTimeout = TimeSpan.FromMinutes(5),
                Logger = new NullLogger(),
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
        /// Sets the X509 certificate to use when starting a TLS session.
        /// </summary>
        /// <param name="value">The server's certificate to use when starting a TLS session.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Certificate(X509Certificate value)
        {
            _setters.Add(options => options.ServerCertificate = value);

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
        /// Adds a TCP Listener Factory instance.
        /// </summary>
        /// <param name="value">The TCP listener factory instance to use.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder TcpListenerFactory(ITcpListenerFactory value)
        {
            _setters.Add(options => options.TcpListenerFactory = value ?? new DefaultTcpListenerFactory());

            return this;
        }

        /// <summary>
        /// Adds a message store factory.
        /// </summary>
        /// <param name="value">The message store factory to use.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MessageStore(IMessageStoreFactory value)
        {
            _setters.Add(options => options.MessageStoreFactory = value ?? DoNothingMessageStore.Instance);

            return this;
        }

        /// <summary>
        /// Adds a mailbox filter factory.
        /// </summary>
        /// <param name="value">The mailbox filter factory to add.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder MailboxFilter(IMailboxFilterFactory value)
        {
            _setters.Add(options => options.MailboxFilterFactory = value ?? DoNothingMailboxFilter.Instance);

            return this;
        }

        /// <summary>
        /// Sets the user aAuthenticator factory.
        /// </summary>
        /// <param name="value">The user authenticator factory.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder UserAuthenticator(IUserAuthenticatorFactory value)
        {
            _setters.Add(options => options.UserAuthenticatorFactory = value ?? DoNothingUserAuthenticator.Instance);

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        /// <param name="value">true if the AUTH command is available on an unsecure session, false if not.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder AllowUnsecureAuthentication(bool value = true)
        {
            _setters.Add(options => options.AllowUnsecureAuthentication = value);

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the client must authenticate in order to proceed.
        /// </summary>
        /// <param name="value">true if the client must issue an AUTH command before sending any mail, false if not.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder AuthenticationRequired(bool value = true)
        {
            _setters.Add(options => options.AuthenticationRequired = value);

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
        /// Sets the supported SSL protocols.
        /// </summary>
        /// <param name="value">The supported SSL protocols.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder SupportedSslProtocols(SslProtocols value)
        {
            _setters.Add(options => options.SupportedSslProtocols = value);

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
        /// Sets the timeout for each network buffer read operation.
        /// </summary>
        /// <param name="value">The timeout to use whilst waiting for each network buffer read.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder NetworkBufferReadTimeout(TimeSpan value)
        {
            _setters.Add(options => options.NetworkBufferReadTimeout = value);

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

        /// <summary>
        /// Sets the logger instance.
        /// </summary>
        /// <param name="value">The logger instance.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public SmtpServerOptionsBuilder Logger(ILogger value)
        {
            _setters.Add(options => options.Logger = value);

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
            /// Gets or sets the SMTP server name.
            /// </summary>
            public string ServerName { get; set; }

            /// <summary>
            /// Gets the Server Certificate to use when starting a TLS session.
            /// </summary>
            public X509Certificate ServerCertificate { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            internal List<IEndpointDefinition> Endpoints { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            IReadOnlyList<IEndpointDefinition> ISmtpServerOptions.Endpoints => Endpoints;

            /// <summary>
            /// Gets the endpoint listener factory.
            /// </summary>
            public ITcpListenerFactory TcpListenerFactory { get; set; }

            /// <summary>
            /// Gets the message store factory to use.
            /// </summary>
            public IMessageStoreFactory MessageStoreFactory { get; set; }

            /// <summary>
            /// Gets the mailbox filter factory to use.
            /// </summary>
            public IMailboxFilterFactory MailboxFilterFactory { get; set; }

            /// <summary>
            /// Gets the user authenticator factory to use.
            /// </summary>
            public IUserAuthenticatorFactory UserAuthenticatorFactory { get; set; }

            /// <summary>
            /// Gets a value indicating whether authentication should be allowed on an unsecure session.
            /// </summary>
            public bool AllowUnsecureAuthentication { get; set; }

            /// <summary>
            /// Gets a value indicating whether the client must authenticate in order to proceed.
            /// </summary>
            public bool AuthenticationRequired { get; set; }

            /// <summary>
            /// The supported SSL protocols.
            /// </summary>
            public SslProtocols SupportedSslProtocols { get; set; }

            /// <summary>
            /// The timeout to use when waiting for a command from the client.
            /// </summary>
            public TimeSpan CommandWaitTimeout { get; set; }

            /// <summary>
            /// The size of the buffer that is read from each call to the underlying network client.
            /// </summary>
            public int NetworkBufferSize { get; set; }

            /// <summary>
            /// The timeout on each individual buffer read.
            /// </summary>
            public TimeSpan NetworkBufferReadTimeout { get; set; }

            /// <summary>
            /// The logger instance to use.
            /// </summary>
            public ILogger Logger { get; set; }
        }

        #endregion
    }
}