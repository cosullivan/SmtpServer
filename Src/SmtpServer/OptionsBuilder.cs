using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpServer
{
    public sealed class OptionsBuilder
    {
        readonly SmtpServerOptions _options = new SmtpServerOptions
        {
            MaxRetryCount = 5,
            SupportedSslProtocols = SslProtocols.Tls,
            NetworkBufferSize = 128,
            NetworkBufferReadTimeout = TimeSpan.FromMinutes(2),
            CommandWaitTimeout = TimeSpan.FromMinutes(5),
            Logger = new NullLogger()
        };

        /// <summary>
        /// Builds the options that have been set and returns the built instance.
        /// </summary>
        /// <returns>The server options that have been set.</returns>
        public ISmtpServerOptions Build()
        {
            _options.MessageStoreFactory = _options.MessageStoreFactory ?? new DoNothingMessageStore();

            return _options;
        }

        /// <summary>
        /// Sets the server name.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder ServerName(string serverName)
        {
            _options.ServerName = serverName;

            return this;
        }

        /// <summary>
        /// Sets the X509 certificate to use when starting a TLS session.
        /// </summary>
        /// <param name="serverCertificate">The server's certificate to use when starting a TLS session.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Certificate(X509Certificate serverCertificate)
        {
            _options.ServerCertificate = serverCertificate;

            return this;
        }

        /// <summary>
        /// Adds an endpoint to listen on.
        /// </summary>
        /// <param name="endpoint">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Endpoint(IPEndPoint endpoint)
        {
            _options.Endpoints.Add(endpoint);

            return this;
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="ports">The port to add as the endpoint.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Port(params int[] ports)
        {
            foreach (var port in ports)
            {
                _options.Endpoints.Add(new IPEndPoint(IPAddress.Any, port));
            }

            return this;
        }

        /// <summary>
        /// Adds a message store factory.
        /// </summary>
        /// <param name="messageStoreFactory">The message store factory to use.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MessageStore(IMessageStoreFactory messageStoreFactory)
        {
            _options.MessageStoreFactory = messageStoreFactory;

            return this;
        }

        /// <summary>
        /// Adds a mailbox filter factory.
        /// </summary>
        /// <param name="mailboxFilterFactory">The mailbox filter factory to add.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MailboxFilter(IMailboxFilterFactory mailboxFilterFactory)
        {
            _options.MailboxFilterFactories.Add(mailboxFilterFactory);

            return this;
        }

        /// <summary>
        /// Sets the user aAuthenticator factory.
        /// </summary>
        /// <param name="userAuthenticatorFactory">The user authenticator factory.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder UserAuthenticator(IUserAuthenticatorFactory userAuthenticatorFactory)
        {
            _options.UserAuthenticatorFactory = userAuthenticatorFactory;

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        /// <param name="value">true if the AUTH command is available on an unsecure session, false if not.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder AllowUnsecureAuthentication(bool value = true)
        {
            _options.AllowUnsecureAuthentication = value;

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the client must authenticate in order to proceed.
        /// </summary>
        /// <param name="value">true if the client must issue an AUTH command before sending any mail, false if not.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder AuthenticationRequired(bool value = true)
        {
            _options.AuthenticationRequired = value;

            return this;
        }

        /// <summary>
        /// Sets the maximum message size.
        /// </summary>
        /// <param name="maxMessageSize">The maximum message size to allow.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MaxMessageSize(int maxMessageSize)
        {
            _options.MaxMessageSize = maxMessageSize;

            return this;
        }

        /// <summary>
        /// Sets the maximum number of retries for a failed command.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retries allowed for a failed command.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MaxRetryCount(int maxRetryCount)
        {
            _options.MaxRetryCount = maxRetryCount;

            return this;
        }

        /// <summary>
        /// Sets the supported SSL protocols.
        /// </summary>
        /// <param name="sslProtocols">The supported SSL protocols.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder SupportedSslProtocols(SslProtocols sslProtocols)
        {
            _options.SupportedSslProtocols = sslProtocols;

            return this;
        }

        /// <summary>
        /// Sets the size of the buffer for each read operation.
        /// </summary>
        /// <param name="size">The buffer size for each read operation.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public OptionsBuilder NetworkBufferSize(int size)
        {
            _options.NetworkBufferSize = size;

            return this;
        }

        /// <summary>
        /// Sets the timeout for each network buffer read operation.
        /// </summary>
        /// <param name="timeout">The timeout to use whilst waiting for each network buffer read.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public OptionsBuilder NetworkBufferReadTimeout(TimeSpan timeout)
        {
            _options.NetworkBufferReadTimeout = timeout;

            return this;
        }

        /// <summary>
        /// Sets the timeout to use whilst waiting for a command from the client.
        /// </summary>
        /// <param name="timeout">The timeout to use whilst waiting for a command from the client.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public OptionsBuilder CommandWaitTimeout(TimeSpan timeout)
        {
            _options.CommandWaitTimeout = timeout;

            return this;
        }

        /// <summary>
        /// Sets the logger instance.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Logger(ILogger logger)
        {
            _options.Logger = logger;

            return this;
        }
    }
}